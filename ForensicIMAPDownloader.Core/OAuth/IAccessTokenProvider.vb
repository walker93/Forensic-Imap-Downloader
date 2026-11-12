Imports System.Threading

''' <summary>What the provider told us about a completed OAuth sign-in. Recorded in the audit log.</summary>
Public Class OAuthSignInResult
    ''' <summary>Account that actually signed in, as reported by the identity provider (may be empty).</summary>
    Public Property Account As String
    Public Property TenantId As String
    Public Property Scopes As IReadOnlyList(Of String) = Array.Empty(Of String)()
    Public Property ExpiresOn As DateTimeOffset?
End Class

''' <summary>
''' Obtains OAuth 2.0 access tokens for IMAP (SASL XOAUTH2).
'''
''' <see cref="SignInAsync"/> runs the interactive part once; <see cref="GetAccessTokenAsync"/>
''' is then called for every new IMAP connection and must refresh silently, since access
''' tokens (about one hour) are much shorter-lived than a forensic download.
'''
''' Implementations keep tokens in memory only: nothing that grants access to the mailbox
''' may be left on the examiner's disk or written to the log.
''' </summary>
Public Interface IAccessTokenProvider
    ''' <summary>Human-readable provider name for the audit log, e.g. "Microsoft".</summary>
    ReadOnly Property ProviderName As String

    ''' <summary>OAuth client (application) ID in use. Public by design for desktop apps, so safe to log.</summary>
    ReadOnly Property ClientId As String

    ''' <summary>Where the account owner can review and revoke the access granted to this application.</summary>
    ReadOnly Property RevocationUrl As String

    Function SignInAsync(loginHint As String, cancellationToken As CancellationToken) As Task(Of OAuthSignInResult)

    Function GetAccessTokenAsync(cancellationToken As CancellationToken) As Task(Of String)

    ''' <summary>
    ''' Drops the cached tokens and, where the provider supports it, revokes the grant server-side.
    ''' Returns True only when the grant was actually revoked at the provider.
    ''' </summary>
    Function SignOutAsync(cancellationToken As CancellationToken) As Task(Of Boolean)
End Interface
