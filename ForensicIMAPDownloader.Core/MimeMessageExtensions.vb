Imports System.IO
Imports System.Runtime.CompilerServices
Imports MimeKit

''' <summary>
''' Saving helpers for MimeKit messages. MimeKit writes a spec-correct .eml directly
''' (WriteTo), and already parses the "Date" header for us (Date property), so unlike the
''' previous System.Net.Mail-based implementation there is no need for an SmtpClient
''' pickup-directory trick or manual Date-header string parsing.
''' </summary>
Public Module MimeMessageExtensions

    Private ReadOnly UnknownSentDate As New DateTimeOffset(New Date(1970, 1, 1), TimeSpan.Zero)

    ''' <summary>Writes the message as .eml and stamps the file's timestamps with the message's Sent date.</summary>
    <Extension()>
    Public Sub SaveAsEml(msg As MimeMessage, filePath As String)
        msg.WriteTo(filePath)

        Dim sentOn = If(msg.Headers.Contains(HeaderId.Date), msg.Date, UnknownSentDate)
        Dim sentOnUtc = sentOn.UtcDateTime

        File.SetCreationTimeUtc(filePath, sentOnUtc)
        File.SetLastWriteTimeUtc(filePath, sentOnUtc)
        File.SetLastAccessTimeUtc(filePath, sentOnUtc)
    End Sub

End Module
