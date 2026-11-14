Imports System.Threading
Imports MailKit
Imports MailKit.Search
Imports Xunit

Public Class FolderSessionTests

    Private Shared ReadOnly NoTimeout As TimeSpan = TimeSpan.FromSeconds(30)

    Private Shared Function CreateServer(messageCount As Integer) As FakeImapServer
        Dim server As New FakeImapServer()
        For uid = 1UI To CUInt(messageCount)
            server.Messages(uid) = $"From: a@example.com{vbCrLf}To: b@example.com{vbCrLf}Subject: message {uid}{vbCrLf}{vbCrLf}body {uid}{vbCrLf}"
        Next
        Return server
    End Function

    Private Shared Function CreateSession(server As FakeImapServer, Optional maxReconnects As Integer = FolderSession.DefaultMaxReconnects) As FolderSession
        Dim settings As New ImapConnectionSettings With {
            .Server = "127.0.0.1", .Port = server.Port, .UseSsl = False,
            .UserName = "owner@example.com", .Password = "app-password"
        }
        Dim service As New ImapDownloadService(settings, NLog.LogManager.CreateNullLogger())
        Return New FolderSession(service, "INBOX", NLog.LogManager.CreateNullLogger(), maxReconnects, TimeSpan.Zero)
    End Function

    Private Shared Function Subjects(session As FolderSession, uids As IEnumerable(Of UniqueId)) As List(Of String)
        Return uids.Select(Function(uid) session.DownloadMessage(uid, NoTimeout, CancellationToken.None).Subject).ToList()
    End Function

    <Fact>
    Public Sub Download_WithoutConnectionProblems_UsesOneConnection()
        Using server = CreateServer(3), session = CreateSession(server)
            session.Open(CancellationToken.None)
            Dim uids = session.Search(SearchQuery.All, CancellationToken.None)

            Assert.Equal({"message 1", "message 2", "message 3"}, Subjects(session, uids))
            Assert.Equal(0, session.Reconnections)
            Assert.Equal(1, server.Connections)
        End Using
    End Sub

    <Fact>
    Public Sub Download_ConnectionDroppedMidFolder_ReconnectsAndRetriesTheMessage()
        Using server = CreateServer(3), session = CreateSession(server)
            server.DropOnFetch.Add(2)
            session.Open(CancellationToken.None)
            Dim uids = session.Search(SearchQuery.All, CancellationToken.None)

            Assert.Equal({"message 1", "message 2", "message 3"}, Subjects(session, uids))
            Assert.Equal(1, session.Reconnections)
            Assert.Equal(2, server.Connections)
        End Using
    End Sub

    <Fact>
    Public Sub Download_MessageTimesOut_ReportsTheTimeoutThenReconnectsForTheNextMessage()
        Using server = CreateServer(3), session = CreateSession(server)
            server.HangOnFetch.Add(2)
            session.Open(CancellationToken.None)
            Dim uids = session.Search(SearchQuery.All, CancellationToken.None)

            Assert.Equal("message 1", session.DownloadMessage(uids(0), NoTimeout, CancellationToken.None).Subject)
            Assert.Throws(Of TimeoutException)(Function() session.DownloadMessage(uids(1), TimeSpan.FromMilliseconds(500), CancellationToken.None))
            Assert.Equal("message 3", session.DownloadMessage(uids(2), NoTimeout, CancellationToken.None).Subject)
            Assert.Equal(1, session.Reconnections)
        End Using
    End Sub

    <Fact>
    Public Sub Download_UidValidityChangesAcrossTheReconnection_StopsTheFolder()
        Using server = CreateServer(3), session = CreateSession(server)
            server.DropOnFetch.Add(2)
            server.UidValidityAfterDrop = 99
            session.Open(CancellationToken.None)
            Dim uids = session.Search(SearchQuery.All, CancellationToken.None)
            session.DownloadMessage(uids(0), NoTimeout, CancellationToken.None)

            Dim ex = Assert.Throws(Of UidValidityChangedException)(Function() session.DownloadMessage(uids(1), NoTimeout, CancellationToken.None))
            Assert.Contains("42", ex.Message)
            Assert.Contains("99", ex.Message)
        End Using
    End Sub

    <Fact>
    Public Sub Download_ConnectionKeepsDropping_GivesUpAfterTheAllowedAttempts()
        Using server = CreateServer(1), session = CreateSession(server, maxReconnects:=2)
            For fetch = 1 To 10
                server.DropOnFetch.Add(fetch)
            Next
            session.Open(CancellationToken.None)
            Dim uids = session.Search(SearchQuery.All, CancellationToken.None)

            Assert.Throws(Of FolderConnectionLostException)(Function() session.DownloadMessage(uids(0), NoTimeout, CancellationToken.None))
            ' The first connection plus the two allowed reconnections.
            Assert.Equal(3, server.Connections)
        End Using
    End Sub

    <Fact>
    Public Sub Download_ServerRejectsTheLoginOnReconnection_StopsTheFolderInsteadOfRetrying()
        Using server = CreateServer(2)
            server.DropOnFetch.Add(1)
            Dim provider As New RejectAfterFirstTokenProvider()
            server.AcceptedTokens.Add("first")
            Dim settings As New ImapConnectionSettings With {
                .Server = "127.0.0.1", .Port = server.Port, .UseSsl = False,
                .UserName = "owner@example.com", .TokenProvider = provider
            }
            Dim service As New ImapDownloadService(settings, NLog.LogManager.CreateNullLogger())
            Using session As New FolderSession(service, "INBOX", NLog.LogManager.CreateNullLogger(), 3, TimeSpan.Zero)
                session.Open(CancellationToken.None)
                Dim uids = session.Search(SearchQuery.All, CancellationToken.None)

                Assert.Throws(Of FolderConnectionLostException)(Function() session.DownloadMessage(uids(0), NoTimeout, CancellationToken.None))
                Assert.Equal(2, server.Connections)
            End Using
        End Using
    End Sub

    ''' <summary>Hands out "first", then tokens the server rejects - as if the grant had been revoked.</summary>
    Private Class RejectAfterFirstTokenProvider
        Implements IAccessTokenProvider

        Private requests As Integer

        Public ReadOnly Property ProviderName As String = "Fake" Implements IAccessTokenProvider.ProviderName
        Public ReadOnly Property ClientId As String = "fake-client" Implements IAccessTokenProvider.ClientId
        Public ReadOnly Property RevocationUrl As String = "" Implements IAccessTokenProvider.RevocationUrl

        Public Function SignInAsync(loginHint As String, cancellationToken As CancellationToken) As Tasks.Task(Of OAuthSignInResult) Implements IAccessTokenProvider.SignInAsync
            Return Tasks.Task.FromResult(New OAuthSignInResult())
        End Function

        Public Function GetAccessTokenAsync(cancellationToken As CancellationToken) As Tasks.Task(Of String) Implements IAccessTokenProvider.GetAccessTokenAsync
            requests += 1
            Return Tasks.Task.FromResult(If(requests = 1, "first", "revoked"))
        End Function

        Public Function SignOutAsync(cancellationToken As CancellationToken) As Tasks.Task(Of Boolean) Implements IAccessTokenProvider.SignOutAsync
            Return Tasks.Task.FromResult(False)
        End Function
    End Class

End Class
