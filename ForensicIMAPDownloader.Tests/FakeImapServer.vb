Imports System.Collections.Concurrent
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Just enough of an IMAP server, on a loopback port without TLS, to observe how the client
''' authenticates: it records every AUTHENTICATE/LOGIN command and accepts an XOAUTH2 login
''' only for the configured bearer token.
''' </summary>
Public NotInheritable Class FakeImapServer
    Implements IDisposable

    Private Const Capabilities As String = "IMAP4rev1 SASL-IR AUTH=XOAUTH2 AUTH=PLAIN"

    Private ReadOnly listener As New TcpListener(IPAddress.Loopback, 0)
    Private ReadOnly stopSource As New CancellationTokenSource()

    ''' <summary>Bearer tokens the server accepts; any other XOAUTH2 token is rejected.</summary>
    Public ReadOnly Property AcceptedTokens As New HashSet(Of String)

    ''' <summary>Raw authentication commands received, in order, across all connections.</summary>
    Public ReadOnly Property AuthCommands As New ConcurrentQueue(Of String)

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
                            Dim isInbox = line.IndexOf("INBOX", StringComparison.OrdinalIgnoreCase) >= 0
                            Await writer.WriteLineAsync(If(isInbox, "* LIST () ""/"" INBOX", "* LIST (\Noselect) ""/"" """""))
                            Await writer.WriteLineAsync($"{tag} OK done")
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
