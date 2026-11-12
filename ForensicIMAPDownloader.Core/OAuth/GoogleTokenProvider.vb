Imports System.IO
Imports System.Threading
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Auth.OAuth2.Flows
Imports Google.Apis.Util.Store

''' <summary>
''' OAuth sign-in for Gmail through the installed-application flow (system browser, loopback
''' redirect), using the examiner's own Google Cloud OAuth client ("Desktop app" type).
'''
''' Tokens live in memory only (<see cref="NullDataStore"/>), and <see cref="SignOutAsync"/>
''' revokes the grant at Google, so a finished acquisition leaves no standing access behind.
''' </summary>
Public Class GoogleTokenProvider
    Implements IAccessTokenProvider

    ''' <summary>The only scope Gmail accepts for IMAP access.</summary>
    Public Const ImapScope As String = "https://mail.google.com/"

    Private Const UserKey As String = "forensic-imap-downloader"

    Private ReadOnly secrets As ClientSecrets
    Private credential As UserCredential

    Public Sub New(secrets As ClientSecrets)
        If secrets Is Nothing OrElse String.IsNullOrWhiteSpace(secrets.ClientId) Then
            Throw New ArgumentException("The Google client secrets do not contain a client ID.", NameOf(secrets))
        End If
        Me.secrets = secrets
    End Sub

    ''' <summary>Loads the client_secret_*.json downloaded from the Google Cloud console.</summary>
    Public Shared Function FromClientSecretsFile(path As String) As GoogleTokenProvider
        Using stream = File.OpenRead(path)
            Return FromClientSecretsStream(stream)
        End Using
    End Function

    Public Shared Function FromClientSecretsStream(stream As Stream) As GoogleTokenProvider
        Dim loaded = GoogleClientSecrets.FromStream(stream)
        If loaded?.Secrets Is Nothing Then
            Throw New InvalidDataException("Not a Google OAuth client file: expected an ""installed"" (Desktop app) client.")
        End If
        Return New GoogleTokenProvider(loaded.Secrets)
    End Function

    Public ReadOnly Property ProviderName As String Implements IAccessTokenProvider.ProviderName
        Get
            Return "Google (browser)"
        End Get
    End Property

    Public ReadOnly Property ClientId As String Implements IAccessTokenProvider.ClientId
        Get
            Return secrets.ClientId
        End Get
    End Property

    Public ReadOnly Property RevocationUrl As String Implements IAccessTokenProvider.RevocationUrl
        Get
            Return "https://myaccount.google.com/connections"
        End Get
    End Property

    Public Async Function SignInAsync(loginHint As String, cancellationToken As CancellationToken) As Task(Of OAuthSignInResult) Implements IAccessTokenProvider.SignInAsync
        Dim initializer As New GoogleAuthorizationCodeFlow.Initializer With {
            .ClientSecrets = secrets,
            .Scopes = {ImapScope},
            .DataStore = New NullDataStore()
        }
        If Not String.IsNullOrWhiteSpace(loginHint) Then initializer.LoginHint = loginHint

        Dim installedApp As New AuthorizationCodeInstalledApp(New GoogleAuthorizationCodeFlow(initializer), New LocalServerCodeReceiver())
        credential = Await installedApp.AuthorizeAsync(UserKey, cancellationToken)

        Dim token = credential.Token
        Dim expiresOn As DateTimeOffset? = Nothing
        If token.ExpiresInSeconds.HasValue Then
            expiresOn = New DateTimeOffset(token.IssuedUtc, TimeSpan.Zero).AddSeconds(token.ExpiresInSeconds.Value)
        End If

        ' Google does not return the signed-in address without an extra identity scope; requesting
        ' one would widen the consent beyond what the acquisition needs, so Account stays empty.
        Return New OAuthSignInResult With {
            .Scopes = If(token.Scope, "").Split(" "c, StringSplitOptions.RemoveEmptyEntries),
            .ExpiresOn = expiresOn
        }
    End Function

    Public Async Function GetAccessTokenAsync(cancellationToken As CancellationToken) As Task(Of String) Implements IAccessTokenProvider.GetAccessTokenAsync
        If credential Is Nothing Then Throw New InvalidOperationException("Sign in before requesting an access token.")
        ' Refreshes with the in-memory refresh token when the access token is close to expiry.
        Return Await credential.GetAccessTokenForRequestAsync(cancellationToken:=cancellationToken)
    End Function

    Public Async Function SignOutAsync(cancellationToken As CancellationToken) As Task(Of Boolean) Implements IAccessTokenProvider.SignOutAsync
        If credential Is Nothing Then Return False
        Dim revoked = Await credential.RevokeTokenAsync(cancellationToken)
        credential = Nothing
        Return revoked
    End Function

End Class
