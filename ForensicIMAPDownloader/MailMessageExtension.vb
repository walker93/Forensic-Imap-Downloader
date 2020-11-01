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
        Using fs = New FileStream(filePath, FileMode.Create)
            msg.ToEMLStream(fs)
        End Using
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

            client.UseDefaultCredentials = True
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            client.PickupDirectoryLocation = tempFolder
            client.Send(msg)
            Dim filePath = Directory.GetFiles(tempFolder).Single()

            Using fs = New FileStream(filePath, FileMode.Open)
                fs.CopyTo(str)
            End Using

            If Directory.Exists(tempFolder) Then
                Directory.Delete(tempFolder, True)
            End If
        End Using
    End Sub
End Module
