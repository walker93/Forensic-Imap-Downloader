Imports System.IO
Imports System.Threading
Imports MailKit
Imports MailKit.Net.Imap
Imports MailKit.Search
Imports MailKit.Security
Imports MimeKit

Public Class Form1

    Private imapService As ImapDownloadService
    Private archiveService As MailArchiveService
    Private checkClient As ImapClient
    Private saving_file As FileInfo
    Private folders As New List(Of String)
    Private filterQuery As SearchQuery = SearchQuery.All
    Private text_filter As String
    Private TokenSource As New CancellationTokenSource()
    Private token As CancellationToken
    Private tasks As New List(Of Task)
    Private ReadOnly logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger()

    Private Const UnknownAddress As String = "UNKNOWN@forensicsimapdownloader.net"
    Private Const MessageDownloadTimeout As Integer = 2 ' minutes

    Private Sub btn_browse_Click(sender As Object, e As EventArgs) Handles btn_browse.Click
        Dim savedialog As New SaveFileDialog()
        savedialog.OverwritePrompt = False
        savedialog.Filter = ".zip|*.zip"
        If savedialog.ShowDialog() <> DialogResult.OK Then Exit Sub
        txt_path.Text = savedialog.FileName
        saving_file = New FileInfo(savedialog.FileName)
        btn_check.Enabled = True
    End Sub

    Private Async Sub btn_check_Click(sender As Object, e As EventArgs) Handles btn_check.Click
        If saving_file Is Nothing Then Exit Sub

        Dim conf = NLog.LogManager.Configuration
        Dim targ = conf.FindTargetByName(Of NLog.Targets.FileTarget)("logfile")
        targ.FileName = Path.Combine(saving_file.DirectoryName, "report.log")
        NLog.LogManager.Configuration = conf

        Dim settings As New ImapConnectionSettings With {
            .Server = txt_server.Text.Trim(),
            .Port = Integer.Parse(txt_port.Text),
            .UseSsl = cb_ssl.Checked,
            .UserName = txt_user.Text.Trim(),
            .Password = txt_pass.Text
        }
        imapService = New ImapDownloadService(settings, logger)
        archiveService = New MailArchiveService(logger)

        logger.Info("---------------------STARTED LOGGING--------------------------")
        logger.Info("Trying to connect to server {0}", settings.Server)

        btn_check.Enabled = False
        Try
            checkClient = Await Task.Run(Function() imapService.Connect(token))
        Catch ex As AuthenticationException
            MsgBox("Check User or Password")
            btn_check.Enabled = True
            Exit Sub
        Catch ex As SslHandshakeException
            MsgBox("The server's TLS certificate could not be validated")
            logger.Error(ex, "TLS handshake failed")
            btn_check.Enabled = True
            Exit Sub
        Catch ex As System.Net.Sockets.SocketException
            MsgBox("Check your internet connection")
            logger.Error(ex, "Check your internet connection")
            btn_check.Enabled = True
            Exit Sub
        Catch ex As Exception
            MsgBox("A problem on the server occurred")
            logger.Error(ex, "A problem occurred while connecting")
            btn_check.Enabled = True
            Exit Sub
        End Try

        Dim discoveredFolders = Await Task.Run(Function() imapService.ListFolders(checkClient, token))
        folders = discoveredFolders.Select(Function(f) f.FullName).ToList()
        lbl_Status.Text = $"Folders: {folders.Count}"
        logger.Info("Found {0} folders: {1}", folders.Count, String.Join(", ", folders))
        btn_start.Enabled = True
        btn_filter.Enabled = True

        For Each folderName In folders
            Dim folder = checkClient.GetFolder(folderName, token)
            imapService.OpenReadOnly(folder, token)
            Dim uids = Await Task.Run(Function() imapService.Search(folder, filterQuery, token))
            data_tb.Rows.Add(True, folderName, $"{uids.Count} / {uids.Count}", "")
            logger.Info("Folder {0} has {1} emails.", folderName, uids.Count)
        Next
    End Sub

    Private Async Sub btn_filter_Click(sender As Object, e As EventArgs) Handles btn_filter.Click

        Dim search_form As New search(text_filter)
        Dim result = search_form.ShowDialog()
        If result <> DialogResult.OK Then Exit Sub
        text_filter = search_form.saved_query
        filterQuery = search_form.Selected_criteria
        logger.Info("Selected filter = {0}", filterQuery)

        For Each folderName In folders
            Dim rowIndex = folders.IndexOf(folderName)
            Dim oldValue = CStr(data_tb.Rows(rowIndex).Cells(2).Value).Split("/"c)(1).Trim()
            Dim folder = checkClient.GetFolder(folderName, token)
            imapService.OpenReadOnly(folder, token)
            Dim uids = Await Task.Run(Function() imapService.Search(folder, filterQuery, token))
            data_tb.Rows(rowIndex).Cells(2).Value = $"{uids.Count} / {oldValue}"
            logger.Info("Folder {0} has {1} filtered emails, total was {2}.", folderName, uids.Count, oldValue)
        Next
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        filterQuery = SearchQuery.All
        token = TokenSource.Token
    End Sub

    Private Async Sub btn_abort_Click(sender As Object, e As EventArgs) Handles btn_abort.Click
        Dim abort_form As New Abort()
        Dim diagResult = abort_form.ShowDialog()
        If diagResult = DialogResult.Cancel Then Exit Sub
        TokenSource.Cancel()
        Await Task.WhenAll(tasks.ToArray())
        Dim tempDir = Path.Combine(saving_file.DirectoryName, "TEMP")
        Select Case abort_form.choice
            Case 1
                If Directory.Exists(tempDir) Then Directory.Delete(tempDir, True)
                logger.Info("User selected choice 1, deleted already downloaded emails.")
            Case 2
                logger.Info("User selected choice 2, keeping emails in TEMP folder for resume")
            Case 3
                logger.Info("User selected choice 3, archiving and hashing already downloaded emails.")
                Await ZipAndHashAsync()
        End Select
        cb_verification.Enabled = Not cb_verification.Enabled
    End Sub

    Private Async Sub btnStart_ButtonClick(sender As Object, e As EventArgs) Handles btn_start.Click
        If imapService Is Nothing Then Exit Sub
        cb_verification.Enabled = Not cb_verification.Enabled
        logger.Info("Settings: Verification={0}, Max Connections={1}", cb_verification.Checked, num_picker.Value)
        Await Download()
    End Sub

    Private Async Function Download() As Task
        btn_start.Capture = False
        lbl_Status.Text = "Downloading..."
        btn_filter.Enabled = False
        num_picker.Enabled = False
        btn_start.Visible = Not btn_start.Visible : btn_abort.Visible = Not btn_abort.Visible

        Dim concurrency As Integer = CInt(num_picker.Value) + 1
        Dim tempRoot = Path.Combine(saving_file.DirectoryName, "TEMP")
        tasks = New List(Of Task)

        Using semaphore As New SemaphoreSlim(concurrency)
            For Each folderName In folders
                Dim rowIndex = folders.IndexOf(folderName)
                If Not CBool(data_tb.Rows(rowIndex).Cells(0).Value) Then
                    logger.Info("Skipping {0}", folderName)
                    Continue For
                End If

                Await semaphore.WaitAsync(token)
                If token.IsCancellationRequested Then
                    logger.Warn("Operation aborted by the user!")
                    Exit Function
                End If

                Dim currentFolder = folderName
                Dim downloadTask = Task.Run(Sub()
                                                 Try
                                                     logger.Info("starting download of {0}", currentFolder)
                                                     DownloadFolder(currentFolder, tempRoot, token)
                                                 Finally
                                                     semaphore.Release()
                                                 End Try
                                             End Sub, token)
                tasks.Add(downloadTask)
            Next
            Await Task.WhenAll(tasks.ToArray())
        End Using

        If token.IsCancellationRequested Then
            lbl_Status.Text = "Aborted"
            logger.Info("Operation Aborted")
            TokenSource.Dispose()
            TokenSource = New CancellationTokenSource()
            token = TokenSource.Token
            btn_start.Visible = Not btn_start.Visible : btn_abort.Visible = Not btn_abort.Visible
            num_picker.Enabled = True
            btn_filter.Enabled = True
            Exit Function
        End If

        Await ZipAndHashAsync()

        btn_start.Visible = Not btn_start.Visible : btn_abort.Visible = Not btn_abort.Visible
    End Function

    Private Async Function ZipAndHashAsync() As Task
        Dim tempRoot = Path.Combine(saving_file.DirectoryName, "TEMP")

        If cb_verification.Checked Then
            lbl_Status.Text = "Checking downloaded e-mails..."
            Await Task.Run(Sub() archiveService.VerifyDownloadedMails(tempRoot))
        End If

        lbl_Status.Text = "Creating Zip file..."
        Await Task.Run(Sub() archiveService.CreateArchive(tempRoot, saving_file.FullName))

        lbl_Status.Text = "Calculating SHA-1 HASH..."
        Await Task.Run(Function() archiveService.ComputeArchiveHash(saving_file.FullName))

        lbl_Status.Text = "Finished"
        Directory.Delete(tempRoot, True)
        logger.Info("---------------- Finished ----------------")
    End Function

    ''' <summary>
    ''' Downloads every message of one folder over its own IMAP connection - concurrent folder
    ''' downloads must not share a connection (MailKit's ImapClient isn't safe for concurrent use
    ''' from multiple threads, and neither was S22.Imap's, which the previous code shared across
    ''' all download tasks).
    ''' </summary>
    Private Sub DownloadFolder(folderName As String, destinationRoot As String, cancellationToken As CancellationToken)
        Dim rowIndex = folders.IndexOf(folderName)
        Dim client As ImapClient = Nothing
        Try
            client = imapService.Connect(cancellationToken)
            Dim folder = client.GetFolder(folderName, cancellationToken)
            imapService.OpenReadOnly(folder, cancellationToken)
            Dim uids = imapService.Search(folder, filterQuery, cancellationToken)

            Dim destFolder = Path.Combine(destinationRoot, folderName)
            Directory.CreateDirectory(destFolder)

            Dim errorCount As Integer = 0
            Dim downloaded As Integer = 0

            Using hashCalculator As New HashCalculator()
                For Each uid In uids
                    If cancellationToken.IsCancellationRequested Then
                        logger.Warn("Operation aborted by the user!")
                        Exit For
                    End If

                    Dim mailPath = Path.Combine(destFolder, uid.ToString() & ".eml")
                    Try
                        If Not File.Exists(mailPath) Then
                            Dim message = imapService.DownloadMessage(folder, uid, TimeSpan.FromMinutes(MessageDownloadTimeout), cancellationToken)
                            EnsureAddressPresent(message.To, uid, folderName, "To")
                            EnsureAddressPresent(message.From, uid, folderName, "From")
                            message.SaveAsEml(mailPath)
                        End If
                    Catch ex As Exception
                        logger.Warn("Folder {2}: email {0} of {1} had a problem.", downloaded + 1, uids.Count, folderName)
                        logger.Warn(ex)
                        errorCount += 1
                        If File.Exists(mailPath) Then File.Delete(mailPath)
                    Finally
                        If cb_verification.Checked AndAlso File.Exists(mailPath) Then
                            archiveService.RecordHash(mailPath, hashCalculator)
                        End If
                        downloaded += 1
                        UpdateRowProgress(rowIndex, downloaded, uids.Count)
                    End Try
                Next
            End Using

            SetRowCompleted(rowIndex, errorCount, downloaded, uids.Count, folderName, cancellationToken)
        Finally
            If client IsNot Nothing Then imapService.Disconnect(client, CancellationToken.None)
        End Try
    End Sub

    Private Sub EnsureAddressPresent(list As InternetAddressList, uid As UniqueId, folderName As String, fieldName As String)
        If list.Count > 0 Then Exit Sub
        list.Add(New MailboxAddress("UNKNOWN", UnknownAddress))
        logger.Warn("Added UNKNOWN in empty '{0}' field for preventing issues with email uid: {1} in {2}", fieldName, uid, folderName)
    End Sub

    Private Sub UpdateGridRow(rowIndex As Integer, action As Action(Of DataGridViewRow))
        If data_tb.InvokeRequired Then
            data_tb.Invoke(Sub() action(data_tb.Rows(rowIndex)))
        Else
            action(data_tb.Rows(rowIndex))
        End If
    End Sub

    Private Sub UpdateRowProgress(rowIndex As Integer, downloaded As Integer, total As Integer)
        UpdateGridRow(rowIndex, Sub(row)
                                     row.Cells(3).Value = $"Downloading {downloaded}/{total} ({downloaded * 100 \ total}%)"
                                     row.Cells(3).Style.BackColor = Color.FromArgb(255, 201, 102)
                                 End Sub)
    End Sub

    Private Sub SetRowCompleted(rowIndex As Integer, errorCount As Integer, downloaded As Integer, total As Integer, folderName As String, cancellationToken As CancellationToken)
        UpdateGridRow(rowIndex, Sub(row)
                                     If cancellationToken.IsCancellationRequested Then
                                         logger.Warn("Downloaded only {0} emails out of {1} from {2}", downloaded, total, folderName)
                                         row.Cells(3).Value = $"Aborted! Downloaded only {downloaded} emails out of {total}"
                                         row.Cells(3).Style.BackColor = Color.FromArgb(255, 102, 102)
                                     ElseIf errorCount > 0 Then
                                         row.Cells(3).Value = $"Completed with {errorCount} errors"
                                         row.Cells(3).Style.BackColor = Color.FromArgb(255, 102, 102)
                                         logger.Info("Download of {0} completed with {1} errors.", folderName, errorCount)
                                     Else
                                         row.Cells(3).Value = "Completed."
                                         row.Cells(3).Style.BackColor = Color.FromArgb(133, 224, 133)
                                         logger.Info("Download of {0} completed with {1} errors.", folderName, errorCount)
                                     End If
                                 End Sub)
    End Sub

End Class
