Imports System.IO
Imports MimeKit
Imports Xunit

Public Class MimeMessageExtensionsTests

    <Fact>
    Public Sub SaveAsEml_WithDateHeader_SetsFileTimestampsToMessageDate()
        Dim msg As New MimeMessage()
        msg.From.Add(New MailboxAddress("Sender", "sender@example.com"))
        msg.To.Add(New MailboxAddress("Recipient", "recipient@example.com"))
        msg.Subject = "Test"
        msg.Date = New DateTimeOffset(2021, 2, 25, 8, 54, 27, TimeSpan.Zero)
        msg.Body = New TextPart("plain") With {.Text = "body"}

        Dim filePath = Path.GetTempFileName()
        Try
            msg.SaveAsEml(filePath)
            Dim expected = New Date(2021, 2, 25, 8, 54, 27, DateTimeKind.Utc)
            Assert.Equal(expected, File.GetLastWriteTimeUtc(filePath))
            Assert.Equal(expected, File.GetCreationTimeUtc(filePath))
        Finally
            File.Delete(filePath)
        End Try
    End Sub

    <Fact>
    Public Sub SaveAsEml_WithoutDateHeader_FallsBackToEpoch()
        Dim msg As New MimeMessage()
        msg.Headers.Remove(HeaderId.Date)
        msg.From.Add(New MailboxAddress("Sender", "sender@example.com"))
        msg.Body = New TextPart("plain") With {.Text = "body"}

        Dim filePath = Path.GetTempFileName()
        Try
            msg.SaveAsEml(filePath)
            Dim expected = New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            Assert.Equal(expected, File.GetLastWriteTimeUtc(filePath))
        Finally
            File.Delete(filePath)
        End Try
    End Sub

    <Fact>
    Public Sub SaveAsEml_WritesReadableMimeMessage()
        Dim msg As New MimeMessage()
        msg.From.Add(New MailboxAddress("Sender", "sender@example.com"))
        msg.To.Add(New MailboxAddress("Recipient", "recipient@example.com"))
        msg.Subject = "Round trip"
        msg.Body = New TextPart("plain") With {.Text = "body text"}

        Dim filePath = Path.GetTempFileName()
        Try
            msg.SaveAsEml(filePath)
            Dim reloaded = MimeMessage.Load(filePath)
            Assert.Equal("Round trip", reloaded.Subject)
            Assert.Equal("sender@example.com", reloaded.From.Mailboxes.First().Address)
        Finally
            File.Delete(filePath)
        End Try
    End Sub

End Class
