Imports System.Threading
Imports System.Threading.Tasks
Imports MailKit.Security
Imports Xunit

Public Class ImapOAuthAuthenticationTests

    ''' <summary>Hands out "tok-1", "tok-2", ... so a test can tell which token reached the server.</summary>
    Private Class SequentialTokenProvider
        Implements IAccessTokenProvider

        Public Property Requests As Integer

        Public ReadOnly Property ProviderName As String = "Fake" Implements IAccessTokenProvider.ProviderName
        Public ReadOnly Property ClientId As String = "fake-client" Implements IAccessTokenProvider.ClientId
        Public ReadOnly Property RevocationUrl As String = "" Implements IAccessTokenProvider.RevocationUrl

        Public Function SignInAsync(loginHint As String, cancellationToken As CancellationToken) As Task(Of OAuthSignInResult) Implements IAccessTokenProvider.SignInAsync
            Return Task.FromResult(New OAuthSignInResult())
        End Function

        Public Function GetAccessTokenAsync(cancellationToken As CancellationToken) As Task(Of String) Implements IAccessTokenProvider.GetAccessTokenAsync
            Requests += 1
            Return Task.FromResult($"tok-{Requests}")
        End Function

        Public Function SignOutAsync(cancellationToken As CancellationToken) As Task(Of Boolean) Implements IAccessTokenProvider.SignOutAsync
            Return Task.FromResult(False)
        End Function
    End Class

    Private Shared Function CreateService(server As FakeImapServer, tokenProvider As IAccessTokenProvider) As ImapDownloadService
        Dim settings As New ImapConnectionSettings With {
            .Server = "127.0.0.1",
            .Port = server.Port,
            .UseSsl = False,
            .UserName = "owner@example.com",
            .Password = "app-password",
            .TokenProvider = tokenProvider
        }
        Return New ImapDownloadService(settings, NLog.LogManager.CreateNullLogger())
    End Function

    <Fact>
    Public Sub Connect_WithTokenProvider_SendsXOAuth2WithUserAndBearerToken()
        Using server As New FakeImapServer()
            server.AcceptedTokens.Add("tok-1")
            Dim service = CreateService(server, New SequentialTokenProvider())

            Dim client = service.Connect(CancellationToken.None)
            Assert.True(client.IsAuthenticated)
            service.Disconnect(client, CancellationToken.None)

            Assert.Single(server.AuthCommands)
            Assert.Contains("AUTHENTICATE XOAUTH2", server.AuthCommands.First())
            Assert.Equal($"user=owner@example.com{ChrW(1)}auth=Bearer tok-1{ChrW(1)}{ChrW(1)}", server.DecodedXOAuth2(0))
        End Using
    End Sub

    <Fact>
    Public Sub Connect_WithTokenProvider_NeverSendsThePassword()
        Using server As New FakeImapServer()
            server.AcceptedTokens.Add("tok-1")
            Dim service = CreateService(server, New SequentialTokenProvider())

            service.Disconnect(service.Connect(CancellationToken.None), CancellationToken.None)

            Assert.DoesNotContain("app-password", server.DecodedXOAuth2(0))
            Assert.DoesNotContain(server.AuthCommands, Function(command) command.Contains("LOGIN") OrElse command.Contains("PLAIN"))
        End Using
    End Sub

    <Fact>
    Public Sub Connect_CalledTwice_RequestsAFreshTokenForEachConnection()
        Using server As New FakeImapServer()
            server.AcceptedTokens.Add("tok-1")
            server.AcceptedTokens.Add("tok-2")
            Dim provider As New SequentialTokenProvider()
            Dim service = CreateService(server, provider)

            service.Disconnect(service.Connect(CancellationToken.None), CancellationToken.None)
            service.Disconnect(service.Connect(CancellationToken.None), CancellationToken.None)

            Assert.Equal(2, provider.Requests)
            Assert.Contains("auth=Bearer tok-2", server.DecodedXOAuth2(1))
        End Using
    End Sub

    <Fact>
    Public Sub Connect_WhenServerRejectsTheToken_ThrowsAuthenticationException()
        Using server As New FakeImapServer()
            server.AcceptedTokens.Add("some-other-token")
            Dim service = CreateService(server, New SequentialTokenProvider())

            Assert.Throws(Of AuthenticationException)(Function() service.Connect(CancellationToken.None))
        End Using
    End Sub

    <Fact>
    Public Sub Connect_WithoutTokenProvider_UsesThePassword()
        Using server As New FakeImapServer()
            Dim service = CreateService(server, Nothing)

            Dim client = service.Connect(CancellationToken.None)
            Assert.True(client.IsAuthenticated)
            service.Disconnect(client, CancellationToken.None)

            Assert.Single(server.AuthCommands)
            Assert.DoesNotContain("XOAUTH2", server.AuthCommands.First())
        End Using
    End Sub

End Class
