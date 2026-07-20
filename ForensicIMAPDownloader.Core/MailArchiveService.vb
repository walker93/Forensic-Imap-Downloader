Imports System.Collections.Concurrent
Imports System.IO
Imports System.IO.Compression

Public Class MailVerificationResult
    Public Property DeletedCount As Integer
    Public Property EditedCount As Integer
    Public Property UnexpectedFiles As New List(Of String)
End Class

''' <summary>
''' Archiving and hash-verification for the downloaded mailbox. <see cref="MailHashes"/> is a
''' <see cref="ConcurrentDictionary(Of String, String)"/> because entries are written from
''' multiple concurrent download tasks (one per folder) - a plain Dictionary here was the
''' previous code's data-race: concurrent unsynchronized writes to Dictionary.Add can throw or
''' corrupt the dictionary's internal state.
''' </summary>
Public Class MailArchiveService

    Private ReadOnly logger As NLog.Logger
    Public ReadOnly MailHashes As New ConcurrentDictionary(Of String, String)

    Public Sub New(logger As NLog.Logger)
        Me.logger = logger
    End Sub

    Public Sub RecordHash(filePath As String, hashCalculator As HashCalculator)
        MailHashes(filePath) = hashCalculator.GetHashmd5(filePath)
    End Sub

    Public Sub CreateArchive(sourceDirectory As String, destinationZipPath As String)
        If File.Exists(destinationZipPath) Then File.Delete(destinationZipPath)
        logger.Info("Creating Zip file...")
        ZipFile.CreateFromDirectory(sourceDirectory, destinationZipPath)
    End Sub

    Public Function ComputeArchiveHash(zipPath As String) As String
        logger.Info("Calculating SHA-1 HASH...")
        Using hashCalculator As New HashCalculator()
            Dim hash = hashCalculator.GetHashSha1(zipPath)
            logger.Info("Calculated SHA-1 HASH: {0}", hash)
            Return hash
        End Using
    End Function

    ''' <summary>Cross-checks every recorded per-file hash against disk and flags any extra, un-hashed files.</summary>
    Public Function VerifyDownloadedMails(tempDirectory As String) As MailVerificationResult
        logger.Info("Checking downloaded E-mail Hashes...")
        Dim result As New MailVerificationResult()

        Using hashCalculator As New HashCalculator()
            Dim filesOnDisk As New HashSet(Of String)(Directory.EnumerateFiles(tempDirectory, "*", SearchOption.AllDirectories))

            For Each entry In MailHashes
                If File.Exists(entry.Key) Then
                    If hashCalculator.GetHashmd5(entry.Key) <> entry.Value Then
                        logger.Error("{0} has been edited after download!", entry.Key)
                        result.EditedCount += 1
                    End If
                    filesOnDisk.Remove(entry.Key)
                Else
                    logger.Error("{0} has been deleted after download!", entry.Key)
                    result.DeletedCount += 1
                End If
            Next

            result.UnexpectedFiles = filesOnDisk.ToList()
        End Using

        If result.UnexpectedFiles.Count > 0 Then
            logger.Error("Found {0} e-mails that weren't downloaded by the program!", result.UnexpectedFiles.Count)
            For Each extraFile In result.UnexpectedFiles
                logger.Error(" - {0}", extraFile)
            Next
        End If

        If result.DeletedCount + result.EditedCount > 0 Then
            logger.Error("Found {0} deleted e-mails and {1} edited e-mails!", result.DeletedCount, result.EditedCount)
        End If

        logger.Info("Check completed")
        Return result
    End Function

End Class
