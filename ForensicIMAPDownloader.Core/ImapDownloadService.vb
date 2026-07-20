Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports MailKit
Imports MailKit.Net.Imap
Imports MailKit.Search
Imports MailKit.Security
Imports MimeKit

Public Class ImapConnectionSettings
    Public Property Server As String
    Public Property Port As Integer
    Public Property UseSsl As Boolean
    Public Property UserName As String
    Public Property Password As String
End Class

''' <summary>
''' Wraps MailKit's <see cref="ImapClient"/> for the forensic download workflow.
'''
''' Each call to <see cref="Connect"/> opens a brand-new, independently authenticated
''' connection. MailKit's ImapClient is not safe to share across threads, so concurrent
''' folder downloads each get their own connection instead of taking turns on one shared
''' client - the previous S22.Imap-based code funneled every concurrent download task through
''' a single shared client instance, which was a latent thread-safety bug.
''' </summary>
Public Class ImapDownloadService

    Private ReadOnly settings As ImapConnectionSettings
    Private ReadOnly logger As NLog.Logger

    Public Sub New(settings As ImapConnectionSettings, logger As NLog.Logger)
        Me.settings = settings
        Me.logger = logger
    End Sub

    ''' <summary>Opens and authenticates a new IMAP connection using the configured credentials.</summary>
    Public Function Connect(cancellationToken As CancellationToken) As ImapClient
        Dim client As New ImapClient()
        client.ServerCertificateValidationCallback = AddressOf ValidateServerCertificate
        Try
            Dim socketOptions = If(settings.UseSsl, SecureSocketOptions.SslOnConnect, SecureSocketOptions.None)
            client.Connect(settings.Server, settings.Port, socketOptions, cancellationToken)
            client.Authenticate(settings.UserName, settings.Password, cancellationToken)
            Return client
        Catch
            client.Dispose()
            Throw
        End Try
    End Function

    ''' <summary>Sends IMAP LOGOUT and releases the connection.</summary>
    Public Sub Disconnect(client As ImapClient, cancellationToken As CancellationToken)
        Try
            If client.IsConnected Then client.Disconnect(True, cancellationToken)
        Finally
            client.Dispose()
        End Try
    End Sub

    ''' <summary>Lists every folder in the account's personal namespace(s).</summary>
    Public Function ListFolders(client As ImapClient, cancellationToken As CancellationToken) As IList(Of IMailFolder)
        Dim allFolders As New List(Of IMailFolder)
        For Each ns As FolderNamespace In client.PersonalNamespaces
            allFolders.AddRange(client.GetFolders(ns, StatusItems.None, False, cancellationToken))
        Next
        Return allFolders
    End Function

    ''' <summary>Opens a folder read-only: a forensic collector must never mutate mailbox state (e.g. \Seen flags).</summary>
    Public Sub OpenReadOnly(folder As IMailFolder, cancellationToken As CancellationToken)
        If Not folder.IsOpen OrElse folder.Access <> FolderAccess.ReadOnly Then
            folder.Open(FolderAccess.ReadOnly, cancellationToken)
        End If
    End Sub

    Public Function Search(folder As IMailFolder, query As SearchQuery, cancellationToken As CancellationToken) As IList(Of UniqueId)
        Return folder.Search(query, cancellationToken)
    End Function

    ''' <summary>
    ''' Downloads a single message, raising a <see cref="TimeoutException"/> if it takes longer
    ''' than <paramref name="timeout"/>. Replaces the previous Thread.Abort()-based timeout
    ''' helper (timeout.vb), which could corrupt shared state and no longer exists on modern .NET.
    ''' </summary>
    Public Function DownloadMessage(folder As IMailFolder, uid As UniqueId, timeout As TimeSpan, cancellationToken As CancellationToken) As MimeMessage
        Using timeoutSource As New CancellationTokenSource(timeout),
              linkedSource As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token)
            Try
                Return folder.GetMessage(uid, linkedSource.Token)
            Catch ex As OperationCanceledException When timeoutSource.IsCancellationRequested AndAlso Not cancellationToken.IsCancellationRequested
                Throw New TimeoutException($"Downloading message {uid} from '{folder.FullName}' timed out after {timeout}.", ex)
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Explicit, logged TLS certificate validation. Previously the SSL checkbox was passed
    ''' straight to the IMAP client with no visible validation logic; this keeps the default
    ''' secure behavior (reject anything but a fully valid chain) and records the certificate
    ''' details for the forensic audit log whenever validation fails.
    ''' </summary>
    Private Function ValidateServerCertificate(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) As Boolean
        If sslPolicyErrors = SslPolicyErrors.None Then Return True

        Dim thumbprint = TryCast(certificate, X509Certificate2)?.Thumbprint
        logger.Error("TLS certificate validation failed for {0}: {1} (Subject: {2}, Thumbprint: {3})",
                     settings.Server, sslPolicyErrors, certificate?.Subject, thumbprint)
        Return False
    End Function

End Class
