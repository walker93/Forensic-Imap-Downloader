Imports System.Threading
Imports Microsoft.Identity.Client

''' <summary>Verification details MSAL hands back for the device-code flow, shown to the operator.</summary>
Public Class DeviceCodeInfo
    Public Property VerificationUrl As String
    Public Property UserCode As String
    Public Property Message As String
End Class

''' <summary>
''' OAuth sign-in for Outlook.com / Microsoft 365 mailboxes through MSAL, as a public (desktop)
''' client: no client secret, tokens cached in memory only.
'''
''' Two flows: the system browser (loopback redirect on http://localhost), or the device-code
''' flow, where the account owner signs in on another device - useful when the owner is
''' present but should not type credentials on the examiner's workstation.
''' </summary>
Public Class MicrosoftTokenProvider
    Implements IAccessTokenProvider

    ''' <summary>Delegated IMAP permission, valid for both personal and work/school accounts.</summary>
    Public Const ImapScope As String = "https://outlook.office.com/IMAP.AccessAsUser.All"

    ''' <summary>Tenant segment that restricts sign-in to personal Microsoft accounts.</summary>
    Public Const ConsumersTenant As String = "consumers"

    Private Shared ReadOnly Scopes As String() = {ImapScope}

    Private ReadOnly app As IPublicClientApplication
    Private ReadOnly useDeviceCode As Boolean
    Private ReadOnly deviceCodeCallback As Func(Of DeviceCodeInfo, Task)
    Private account As IAccount

    ''' <param name="clientId">Application (client) ID of the Entra app registration.</param>
    ''' <param name="tenant">"consumers", "common", "organizations" or a tenant ID/domain.</param>
    ''' <param name="deviceCodeCallback">Required when <paramref name="useDeviceCode"/> is True: shows the code to the operator.</param>
    Public Sub New(clientId As String, tenant As String, useDeviceCode As Boolean, Optional deviceCodeCallback As Func(Of DeviceCodeInfo, Task) = Nothing)
        If String.IsNullOrWhiteSpace(clientId) Then Throw New ArgumentException("A Microsoft client ID is required.", NameOf(clientId))
        If useDeviceCode AndAlso deviceCodeCallback Is Nothing Then Throw New ArgumentNullException(NameOf(deviceCodeCallback))

        Me.ClientId = clientId.Trim()
        Me.Tenant = If(String.IsNullOrWhiteSpace(tenant), ConsumersTenant, tenant.Trim())
        Me.useDeviceCode = useDeviceCode
        Me.deviceCodeCallback = deviceCodeCallback

        app = PublicClientApplicationBuilder.Create(Me.ClientId).
            WithAuthority($"https://login.microsoftonline.com/{Me.Tenant}").
            WithRedirectUri("http://localhost").
            Build()
    End Sub

    Public ReadOnly Property Tenant As String

    Public ReadOnly Property ProviderName As String Implements IAccessTokenProvider.ProviderName
        Get
            Return If(useDeviceCode, "Microsoft (device code)", "Microsoft (browser)")
        End Get
    End Property

    Public ReadOnly Property ClientId As String Implements IAccessTokenProvider.ClientId

    Public ReadOnly Property RevocationUrl As String Implements IAccessTokenProvider.RevocationUrl
        Get
            Return If(String.Equals(Tenant, ConsumersTenant, StringComparison.OrdinalIgnoreCase),
                      "https://account.live.com/consent/Manage",
                      "https://myapps.microsoft.com")
        End Get
    End Property

    Public Async Function SignInAsync(loginHint As String, cancellationToken As CancellationToken) As Task(Of OAuthSignInResult) Implements IAccessTokenProvider.SignInAsync
        Dim result As AuthenticationResult
        If useDeviceCode Then
            result = Await app.AcquireTokenWithDeviceCode(Scopes,
                Function(code) deviceCodeCallback(New DeviceCodeInfo With {
                    .VerificationUrl = code.VerificationUrl,
                    .UserCode = code.UserCode,
                    .Message = code.Message
                })).ExecuteAsync(cancellationToken)
        Else
            Dim request = app.AcquireTokenInteractive(Scopes).
                WithUseEmbeddedWebView(False).
                WithPrompt(Prompt.SelectAccount)
            If Not String.IsNullOrWhiteSpace(loginHint) Then request = request.WithLoginHint(loginHint)
            result = Await request.ExecuteAsync(cancellationToken)
        End If

        account = result.Account
        Return New OAuthSignInResult With {
            .Account = result.Account?.Username,
            .TenantId = result.TenantId,
            .Scopes = result.Scopes.ToList(),
            .ExpiresOn = result.ExpiresOn
        }
    End Function

    Public Async Function GetAccessTokenAsync(cancellationToken As CancellationToken) As Task(Of String) Implements IAccessTokenProvider.GetAccessTokenAsync
        If account Is Nothing Then Throw New InvalidOperationException("Sign in before requesting an access token.")
        ' Served from the in-memory cache, transparently refreshed when close to expiry.
        Dim result = Await app.AcquireTokenSilent(Scopes, account).ExecuteAsync(cancellationToken)
        Return result.AccessToken
    End Function

    ''' <summary>
    ''' Microsoft has no token revocation endpoint for public clients: this only forgets the
    ''' cached tokens. The grant itself must be removed by the account owner at <see cref="RevocationUrl"/>.
    ''' </summary>
    Public Async Function SignOutAsync(cancellationToken As CancellationToken) As Task(Of Boolean) Implements IAccessTokenProvider.SignOutAsync
        If account IsNot Nothing Then
            Await app.RemoveAsync(account)
            account = Nothing
        End If
        Return False
    End Function

End Class
