Imports System.IO
Imports System.Net.Sockets
Imports System.Threading
Imports MailKit
Imports MailKit.Net.Imap
Imports MailKit.Search
Imports MimeKit

''' <summary>A folder download cannot go on: the remaining messages were not attempted.</summary>
Public MustInherit Class FolderSessionException
    Inherits Exception

    Protected Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub
End Class

''' <summary>The connection was lost and could not be re-established within the allowed attempts.</summary>
Public Class FolderConnectionLostException
    Inherits FolderSessionException

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub
End Class

''' <summary>
''' After a reconnection the folder reported a different UIDVALIDITY: the UIDs found by the
''' search may now designate other messages, so going on could collect the wrong evidence.
''' </summary>
Public Class UidValidityChangedException
    Inherits FolderSessionException

    Public Sub New(message As String)
        MyBase.New(message, Nothing)
    End Sub
End Class

''' <summary>
''' One folder opened read-only over its own IMAP connection, re-established transparently when
''' it drops in the middle of a download.
'''
''' A connection can be lost to a network glitch, to the server closing an idle session (e.g.
''' while the download is paused), or to a message download timing out: MailKit disconnects
''' when a command in progress is cancelled. Before this class, every remaining message of the
''' folder then failed. Now the session reconnects - with a fresh OAuth token if OAuth is in
''' use - reopens the folder read-only and retries the message, up to <c>maxReconnects</c>
''' times in a row. Every reconnection is logged, and the folder's UIDVALIDITY is checked so
''' that UIDs found before the reconnection are only used if they still mean the same messages.
''' </summary>
Public NotInheritable Class FolderSession
    Implements IDisposable

    Public Const DefaultMaxReconnects As Integer = 3

    Private ReadOnly service As ImapDownloadService
    Private ReadOnly logger As NLog.Logger
    Private ReadOnly maxReconnects As Integer
    Private ReadOnly retryDelay As TimeSpan

    Private client As ImapClient
    Private currentFolder As IMailFolder
    Private uidValidity As UInteger

    ''' <param name="retryDelay">Base wait before a reconnection; attempt n waits n times as long.</param>
    Public Sub New(service As ImapDownloadService, folderName As String, logger As NLog.Logger,
                   Optional maxReconnects As Integer = DefaultMaxReconnects, Optional retryDelay As TimeSpan? = Nothing)
        Me.service = service
        Me.FolderName = folderName
        Me.logger = logger
        Me.maxReconnects = maxReconnects
        Me.retryDelay = If(retryDelay, TimeSpan.FromSeconds(2))
    End Sub

    Public ReadOnly Property FolderName As String

    ''' <summary>How many times the connection has been re-established.</summary>
    Public ReadOnly Property Reconnections As Integer

    ''' <summary>Connects, opens the folder read-only and records its UIDVALIDITY.</summary>
    Public Sub Open(cancellationToken As CancellationToken)
        OpenConnection(cancellationToken)
        uidValidity = currentFolder.UidValidity
    End Sub

    Public Function Search(query As SearchQuery, cancellationToken As CancellationToken) As IList(Of UniqueId)
        Return WithReconnect(Function() service.Search(currentFolder, query, cancellationToken), "search", cancellationToken)
    End Function

    ''' <summary>
    ''' Downloads one message. A lost connection is re-established and the message retried; a
    ''' timeout is still reported as a <see cref="TimeoutException"/> for that message, and the
    ''' next call reconnects.
    ''' </summary>
    Public Function DownloadMessage(uid As UniqueId, timeout As TimeSpan, cancellationToken As CancellationToken) As MimeMessage
        Return WithReconnect(Function() service.DownloadMessage(currentFolder, uid, timeout, cancellationToken), $"message {uid}", cancellationToken)
    End Function

    Private Function WithReconnect(Of T)(operation As Func(Of T), what As String, cancellationToken As CancellationToken) As T
        Dim failures = 0
        Do
            Try
                EnsureConnected(cancellationToken)
                Return operation()
            Catch ex As Exception When IsConnectionLoss(ex) AndAlso Not cancellationToken.IsCancellationRequested
                failures += 1
                If failures > maxReconnects Then
                    Throw New FolderConnectionLostException(
                        $"The connection for '{FolderName}' was lost and could not be re-established after {maxReconnects} attempts.", ex)
                End If
                logger.Warn("Connection for folder {0} lost during {1} ({2}: {3}). Reconnecting, attempt {4} of {5}.",
                            FolderName, what, ex.GetType().Name, ex.Message, failures, maxReconnects)
                CloseQuietly()
                If cancellationToken.WaitHandle.WaitOne(TimeSpan.FromTicks(retryDelay.Ticks * failures)) Then
                    cancellationToken.ThrowIfCancellationRequested()
                End If
            End Try
        Loop
    End Function

    ''' <summary>Reconnects if the previous operation left the session without a usable connection.</summary>
    Private Sub EnsureConnected(cancellationToken As CancellationToken)
        If client IsNot Nothing AndAlso client.IsConnected AndAlso currentFolder IsNot Nothing AndAlso currentFolder.IsOpen Then Exit Sub
        CloseQuietly()
        Try
            OpenConnection(cancellationToken)
        Catch ex As Exception When Not IsConnectionLoss(ex) AndAlso Not TypeOf ex Is OperationCanceledException
            ' E.g. the OAuth grant was revoked meanwhile: retrying, or failing every remaining
            ' message one by one, cannot help.
            CloseQuietly()
            Throw New FolderConnectionLostException($"Could not reconnect to '{FolderName}': {ex.Message}", ex)
        End Try

        Dim newUidValidity = currentFolder.UidValidity
        If newUidValidity <> uidValidity Then
            CloseQuietly()
            Throw New UidValidityChangedException(
                $"UIDVALIDITY of '{FolderName}' changed from {uidValidity} to {newUidValidity} after reconnecting: " &
                "the message UIDs found before are no longer valid.")
        End If

        _Reconnections += 1
        logger.Info("Reconnected to folder {0}; UIDVALIDITY unchanged ({1}).", FolderName, uidValidity)
    End Sub

    Private Sub OpenConnection(cancellationToken As CancellationToken)
        client = service.Connect(cancellationToken)
        currentFolder = client.GetFolder(FolderName, cancellationToken)
        service.OpenReadOnly(currentFolder, cancellationToken)
    End Sub

    ''' <summary>
    ''' Failures that mean the connection, not the message, is the problem. A server refusing a
    ''' command (<see cref="ImapCommandException"/>) is about that message and is not retried.
    ''' </summary>
    Private Shared Function IsConnectionLoss(ex As Exception) As Boolean
        Return TypeOf ex Is ServiceNotConnectedException OrElse
               TypeOf ex Is ImapProtocolException OrElse
               TypeOf ex Is IOException OrElse
               TypeOf ex Is SocketException
    End Function

    ''' <summary>Drops a connection that is already broken: no LOGOUT, which would only fail.</summary>
    Private Sub CloseQuietly()
        If client Is Nothing Then Exit Sub
        Try
            If client.IsConnected Then client.Disconnect(False)
        Catch
        Finally
            client.Dispose()
            client = Nothing
            currentFolder = Nothing
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If client Is Nothing Then Exit Sub
        Try
            service.Disconnect(client, CancellationToken.None)
        Catch
            ' Logging out of a dying connection must not hide the outcome of the download.
        Finally
            client = Nothing
            currentFolder = Nothing
        End Try
    End Sub

End Class
