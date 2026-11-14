Imports System.Collections.Concurrent
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Just enough of an IMAP server, on a loopback port without TLS, to observe how the client
''' authenticates and downloads: it records every AUTHENTICATE/LOGIN command, accepts an XOAUTH2
''' login only for the configured bearer token, serves one folder of <see cref="Messages"/>, and
''' can drop the connection or stop answering on chosen FETCH commands.
''' </summary>
Public NotInheritable Class FakeImapServer
    Implements IDisposable

    Private Const Capabilities As String = "IMAP4rev1 SASL-IR AUTH=XOAUTH2 AUTH=PLAIN"

    Private ReadOnly listener As New TcpListener(IPAddress.Loopback, 0)
    Private ReadOnly stopSource As New CancellationTokenSource()

    ''' <summary>Bearer tokens the server accepts; any other XOAUTH2 token is rejected.</summary>
    Public ReadOnly Property AcceptedTokens As New HashSet(Of String)

    ''' <summary>Untagged responses returned for a LIST of every folder ("*").</summary>
    Public Property FolderListLines As String() = {"* LIST () ""/"" INBOX"}

    ''' <summary>Raw authentication commands received, in order, across all connections.</summary>
    Public ReadOnly Property AuthCommands As New ConcurrentQueue(Of String)

    ''' <summary>Messages of the folder opened with EXAMINE/SELECT, by UID.</summary>
    Public ReadOnly Property Messages As New SortedDictionary(Of UInteger, String)

    Public Property UidValidity As UInteger = 42

    ''' <summary>When set, UIDVALIDITY takes this value once a connection has been dropped.</summary>
    Public Property UidValidityAfterDrop As UInteger?

    ''' <summary>FETCH commands (1-based, counted across connections) on which the server closes the connection.</summary>
    Public ReadOnly Property DropOnFetch As New HashSet(Of Integer)

    ''' <summary>FETCH commands (1-based, counted across connections) the server never answers.</summary>
    Public ReadOnly Property HangOnFetch As New HashSet(Of Integer)

    ''' <summary>Connections accepted so far.</summary>
    Public ReadOnly Property Connections As Integer
        Get
            Return Volatile.Read(connectionCount)
        End Get
    End Property

    Private connectionCount As Integer
    Private fetchCount As Integer
    Private dropped As Boolean

    Public Sub New()
        listener.Start()
        Port = DirectCast(listener.LocalEndpoint, IPEndPoint).Port
        Task.Run(AddressOf AcceptLoopAsync)
    End Sub

    Public ReadOnly Property Port As Integer

    ''' <summary>Decodes the SASL XOAUTH2 initial response of the n-th authentication command.</summary>
    Public Function DecodedXOAuth2(index As Integer) As String
        Dim parts = AuthCommands.ElementAt(index).Split(" "c)
        Return Encoding.UTF8.GetString(Convert.FromBase64String(parts(parts.Length - 1)))
    End Function

    Private Async Function AcceptLoopAsync() As Task
        Try
            While Not stopSource.IsCancellationRequested
                Dim client = Await listener.AcceptTcpClientAsync(stopSource.Token)
                Interlocked.Increment(connectionCount)
                Dim ignored = Task.Run(Function() ServeAsync(client))
            End While
        Catch ex As OperationCanceledException
        Catch ex As SocketException
        End Try
    End Function

    Private Async Function ServeAsync(client As TcpClient) As Task
        Using client
            Dim stream = client.GetStream()
            Dim reader As New IO.StreamReader(stream, Encoding.ASCII)
            Dim writer As New IO.StreamWriter(stream, Encoding.ASCII) With {.NewLine = vbCrLf, .AutoFlush = True}
            Try
                Await writer.WriteLineAsync($"* OK [CAPABILITY {Capabilities}] fake ready")
                While True
                    Dim line = Await reader.ReadLineAsync()
                    If line Is Nothing Then Exit While

                    Dim parts = line.Split(" "c)
                    Dim tag = parts(0)
                    Dim command = If(parts.Length > 1, parts(1).ToUpperInvariant(), "")

                    Select Case command
                        Case "CAPABILITY"
                            Await writer.WriteLineAsync($"* CAPABILITY {Capabilities}")
                            Await writer.WriteLineAsync($"{tag} OK done")
                        Case "AUTHENTICATE"
                            AuthCommands.Enqueue(line)
                            If parts(2).ToUpperInvariant() = "XOAUTH2" AndAlso Not IsAcceptedToken(parts(3)) Then
                                ' Real servers answer with a base64 JSON challenge, which the client acknowledges with an empty line.
                                Await writer.WriteLineAsync("+ " & Convert.ToBase64String(Encoding.ASCII.GetBytes("{""status"":""400""}")))
                                Await reader.ReadLineAsync()
                                Await writer.WriteLineAsync($"{tag} NO AUTHENTICATE failed")
                            Else
                                Await writer.WriteLineAsync($"{tag} OK [CAPABILITY IMAP4rev1] authenticated")
                            End If
                        Case "LOGIN"
                            AuthCommands.Enqueue(line)
                            Await writer.WriteLineAsync($"{tag} OK [CAPABILITY IMAP4rev1] authenticated")
                        Case "LIST"
                            If line.Contains("""*""") Then
                                For Each listLine In FolderListLines
                                    Await writer.WriteLineAsync(listLine)
                                Next
                            Else
                                Dim isInbox = line.IndexOf("INBOX", StringComparison.OrdinalIgnoreCase) >= 0
                                Await writer.WriteLineAsync(If(isInbox, "* LIST () ""/"" INBOX", "* LIST (\Noselect) ""/"" """""))
                            End If
                            Await writer.WriteLineAsync($"{tag} OK done")
                        Case "EXAMINE", "SELECT"
                            Dim validity = If(dropped AndAlso UidValidityAfterDrop.HasValue, UidValidityAfterDrop.Value, UidValidity)
                            Await writer.WriteLineAsync($"* {Messages.Count} EXISTS")
                            Await writer.WriteLineAsync("* 0 RECENT")
                            Await writer.WriteLineAsync("* FLAGS (\Seen)")
                            Await writer.WriteLineAsync($"* OK [UIDVALIDITY {validity}] UIDs valid")
                            Await writer.WriteLineAsync($"* OK [UIDNEXT {If(Messages.Count = 0, 1UI, Messages.Keys.Last() + 1UI)}] next")
                            Await writer.WriteLineAsync($"{tag} OK [READ-ONLY] {command} completed")
                        Case "UID"
                            Dim subCommand = parts(2).ToUpperInvariant()
                            If subCommand = "SEARCH" Then
                                Await writer.WriteLineAsync("* SEARCH " & String.Join(" ", Messages.Keys))
                                Await writer.WriteLineAsync($"{tag} OK SEARCH completed")
                            ElseIf subCommand = "FETCH" Then
                                Dim n = Interlocked.Increment(fetchCount)
                                If DropOnFetch.Contains(n) Then
                                    dropped = True
                                    Exit While
                                End If
                                If HangOnFetch.Contains(n) Then
                                    ' Never answer; give up once the client has long gone.
                                    Await Task.Delay(TimeSpan.FromSeconds(10))
                                    Exit While
                                End If
                                Dim uid = UInteger.Parse(parts(3))
                                Dim body = Messages(uid)
                                Dim sequence = Messages.Keys.ToList().IndexOf(uid) + 1
                                Await writer.WriteAsync($"* {sequence} FETCH (UID {uid} BODY[] {{{Encoding.ASCII.GetByteCount(body)}}}{vbCrLf}{body})" & vbCrLf)
                                Await writer.WriteLineAsync($"{tag} OK FETCH completed")
                            Else
                                Await writer.WriteLineAsync($"{tag} OK done")
                            End If
                        Case "LOGOUT"
                            Await writer.WriteLineAsync("* BYE bye")
                            Await writer.WriteLineAsync($"{tag} OK done")
                            Exit While
                        Case Else
                            Await writer.WriteLineAsync($"{tag} OK done")
                    End Select
                End While
            Catch ex As IO.IOException
            End Try
        End Using
    End Function

    Private Function IsAcceptedToken(initialResponse As String) As Boolean
        Dim decoded = Encoding.UTF8.GetString(Convert.FromBase64String(initialResponse))
        Return AcceptedTokens.Any(Function(token) decoded.Contains($"auth=Bearer {token}{ChrW(1)}"))
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        stopSource.Cancel()
        listener.Stop()
        stopSource.Dispose()
    End Sub

End Class
