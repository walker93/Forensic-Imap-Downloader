Imports System
Imports System.Net.Mail
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices

Module MailMessageExtension
    <Extension()>
    Public Sub SaveMailMessage(ByVal msg As MailMessage, ByVal filePath As String)
        Dim SentOn = msg.Headers.Item("Date")
        If SentOn = "" Then SentOn = New Date(1970, 1, 1).ToString("r", Globalization.CultureInfo.CreateSpecificCulture("en-US"))

        Using fs = New FileStream(filePath, FileMode.Create)
            msg.ToEMLStream(fs)
        End Using
        Dim SentOnDate As Date
        Try
            SentOnDate = Date.Parse(SentOn, Globalization.CultureInfo.CreateSpecificCulture("en-US")) '25 Feb 2021 08:54:27 +0000
        Catch ex As Exception
            SentOnDate = Date.Parse(SentOn.Substring(5), Globalization.CultureInfo.CreateSpecificCulture("en-US")) 'Thu, 25 Feb 2021 08:54:27 +0000
        End Try

        If IsNothing(SentOnDate) Then Exit Sub
        SentOnDate = SentOnDate.ToUniversalTime
        IO.File.SetCreationTimeUtc(filePath, SentOnDate)
        IO.File.SetLastWriteTimeUtc(filePath, SentOnDate)
        IO.File.SetLastAccessTimeUtc(filePath, SentOnDate)

    End Sub

    <Extension()>
    Public Sub ToEMLStream(ByVal msg As MailMessage, ByVal str As Stream)
        Using client = New SmtpClient()
            Dim id = Guid.NewGuid()
            Dim tempFolder = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name)
            tempFolder = Path.Combine(tempFolder, "MailMessageToEMLTemp")
            tempFolder = Path.Combine(tempFolder, id.ToString())

            If Not Directory.Exists(tempFolder) Then
                Directory.CreateDirectory(tempFolder)
            End If
            Dim SentOn = msg.Headers.Item("Date")
            client.UseDefaultCredentials = True
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            client.PickupDirectoryLocation = tempFolder
            client.Send(msg)
            Dim filePath = Directory.GetFiles(tempFolder).Single()

            Dim mail As List(Of String) = IO.File.ReadAllLines(filePath, Encoding.UTF8).ToList
            Dim date_index = mail.IndexOf(mail.Where(Function(x) x.StartsWith("Date")).First)
            mail.Item(date_index) = "Date: " & SentOn
            Dim textMail = String.Join(vbNewLine, mail)
            str.Write(Encoding.UTF8.GetBytes(textMail), 0, Encoding.UTF8.GetByteCount(textMail))
            If Directory.Exists(tempFolder) Then
                Directory.Delete(tempFolder, True)
            End If
        End Using
    End Sub

End Module
