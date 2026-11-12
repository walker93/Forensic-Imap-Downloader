Imports System.Threading
Imports Xunit

Public Class ImapFolderListingTests

    <Fact>
    Public Sub ListFolders_GmailLayout_SkipsTheNoselectContainerButKeepsItsChildren()
        Using server As New FakeImapServer()
            server.FolderListLines = {
                "* LIST (\HasNoChildren) ""/"" INBOX",
                "* LIST (\HasChildren \Noselect) ""/"" ""[Gmail]""",
                "* LIST (\HasNoChildren \Sent) ""/"" ""[Gmail]/Sent Mail""",
                "* LIST (\HasNoChildren \All) ""/"" ""[Gmail]/All Mail"""
            }
            Dim settings As New ImapConnectionSettings With {
                .Server = "127.0.0.1", .Port = server.Port, .UseSsl = False,
                .UserName = "owner@example.com", .Password = "app-password"
            }
            Dim service As New ImapDownloadService(settings, NLog.LogManager.CreateNullLogger())

            Dim client = service.Connect(CancellationToken.None)
            Try
                Dim names = service.ListFolders(client, CancellationToken.None).Select(Function(f) f.FullName).ToList()

                Assert.Equal({"INBOX", "[Gmail]/Sent Mail", "[Gmail]/All Mail"}, names)
            Finally
                service.Disconnect(client, CancellationToken.None)
            End Try
        End Using
    End Sub

End Class
