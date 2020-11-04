Imports System.IO.Compression
Imports System.Threading
Imports S22.Imap

Public Class Form1

    Dim imap_client As ImapClient
    Dim server, user, pass As String
    Dim port As Integer
    Dim folders As New List(Of String)
    Dim saving_file As IO.FileInfo
    Dim filter As New SearchCondition()
    Dim text_filter As String
    Dim TokenSource As New CancellationTokenSource()
    Dim token As CancellationToken

    Dim logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

    Private Sub btn_browse_Click(sender As Object, e As EventArgs) Handles btn_browse.Click
        Dim savedialog As New SaveFileDialog()
        savedialog.OverwritePrompt = False
        savedialog.Filter = ".zip|*.zip"
        If Not savedialog.ShowDialog = DialogResult.OK Then Exit Sub
        txt_path.Text = savedialog.FileName
        saving_file = New IO.FileInfo(savedialog.FileName)
        If saving_file.Exists Then saving_file.Delete()
        btn_check.Enabled = True
    End Sub

    Private Sub btn_check_Click(sender As Object, e As EventArgs) Handles btn_check.Click
        If IsNothing(saving_file) Then Exit Sub

        Dim conf = NLog.LogManager.Configuration
        Dim targ = conf.FindTargetByName(Of NLog.Targets.FileTarget)("logfile")
        targ.FileName = IO.Path.Combine(saving_file.DirectoryName, "report.log")
        NLog.LogManager.Configuration = conf

        server = txt_server.Text.Trim
        user = txt_user.Text.Trim
        pass = txt_pass.Text
        port = Integer.Parse(txt_port.Text)
        logger.Info("---------------------STARTED LOGGING--------------------------")
        logger.Info("Trying to connect to server {0}", server)
        Try
            imap_client = New ImapClient(server, port, user, pass, AuthMethod.Auto, cb_ssl.CheckState)
        Catch auth As InvalidCredentialsException
            MsgBox("Check User or Password") : Exit Sub
        Catch sock As Net.Sockets.SocketException
            MsgBox("Check your internet connection")
            logger.Error("Check your internet connection") : Exit Sub
        Catch bad_res As BadServerResponseException
            MsgBox("A problem on the server occurred")
            logger.Error("A problem on the server occurred") : Exit Sub
        End Try

        Dim mailboxInfo As MailboxInfo = imap_client.GetMailboxInfo

        logger.Info("Connected - Space Used: {0} bytes", mailboxInfo.UsedStorage)
        folders = imap_client.ListMailboxes.ToList
        lbl_Status.Text = String.Format("Folders: {0}", folders.Count)
        logger.Info("Found {0} folders: {1}", folders.Count, String.Join(", ", folders))
        btn_check.Enabled = False
        btn_start.Enabled = True
        btn_filter.Enabled = True

        For Each folder In folders
            Dim uids = imap_client.Search(filter, folder)
            data_tb.Rows.Add(True, folder, $"{uids.Count} / {uids.Count}", "")
            Application.DoEvents()
            logger.Info("Folder {0} has {1} emails.", folder, uids.Count)
        Next

    End Sub

    Private Sub btn_filter_Click(sender As Object, e As EventArgs) Handles btn_filter.Click

        Dim search_form As New search(text_filter)
        Dim result = search_form.ShowDialog()
        If Not result = DialogResult.OK Then Exit Sub
        text_filter = search_form.saved_query
        filter = search_form.Selected_criteria
        logger.Info($"Selected filter = {filter.ToString}")

        For Each folder In folders
            Dim old_value = data_tb.Rows.Item(folders.IndexOf(folder)).Cells.Item(2).Value.ToString.Split("/")(1).Trim
            Dim uids = imap_client.Search(filter, folder)
            data_tb.Rows.Item(folders.IndexOf(folder)).Cells.Item(2).Value = uids.Count.ToString & " / " & old_value
            'data_tb.Rows.Add(True, folder, uids.Count, "")
            Application.DoEvents()
            logger.Info("Folder {0} has {1} filtered emails, total was {2}.", folder, uids.Count, old_value)
        Next
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        filter = SearchCondition.All
        token = TokenSource.Token
    End Sub

    Private Sub btn_abort_Click(sender As Object, e As EventArgs) Handles btn_abort.Click

        TokenSource.Cancel()
    End Sub

    Private Async Sub btnStart_ButtonClick(sender As Object, e As EventArgs) Handles btn_start.Click
        If IsNothing(imap_client) Then Exit Sub
        Await Download()
    End Sub

    Async Function Download() As Task
        btn_start.Capture = False
        lbl_Status.Text = String.Format("Downloading...")
        btn_filter.Enabled = False
        num_picker.Enabled = False
        btn_start.Visible = Not btn_start.Visible : btn_abort.Visible = Not btn_abort.Visible
        Dim concurrency As Integer = num_picker.Value

        Using semaphore As New SemaphoreSlim(concurrency)
            Dim tasks As New List(Of Task)
            For Each folder In folders
                If Not data_tb.Rows.Item(folders.IndexOf(folder)).Cells.Item(0).Value Then
                    logger.Info("Skipping {0}", folder)
                    Continue For
                End If
                semaphore.Wait()
                Dim t = Task.Factory.StartNew(Sub()
                                                  Try
                                                      logger.Info("starting download Of {0}", folder)
                                                      download_folder(folder, token)
                                                  Finally
                                                      semaphore.Release()
                                                  End Try
                                              End Sub, token)
                tasks.Add(t)
                Application.DoEvents()
                If token.IsCancellationRequested Then
                    logger.Warn("Operation aborted by the user!")
                    Exit Function
                End If

            Next
            Await Task.WhenAll(tasks.ToArray)
        End Using
        If token.IsCancellationRequested Then
            lbl_Status.Text = String.Format("Aborted")
            logger.Info("Operation Aborted")
            TokenSource.Dispose()
            TokenSource = New CancellationTokenSource
            token = TokenSource.Token
            btn_start.Visible = Not btn_start.Visible : btn_abort.Visible = Not btn_abort.Visible
            num_picker.Enabled = True
            Exit Function
        End If
        lbl_Status.Text = String.Format("Creating Zip file...")
        logger.Info("Creating Zip file...")
        Task.Run(Sub()
                     ZipFile.CreateFromDirectory(saving_file.DirectoryName & "\TEMP\", saving_file.FullName)
                 End Sub).Wait()
        lbl_Status.Text = String.Format("Calculating HASH...")
        logger.Info("Calculating HASH...")
        Dim hash As String = ""
        Task.Run(Sub()
                     hash = GetHashSha1(saving_file.FullName)
                 End Sub).Wait()
        logger.Info("Calculated HASH: {0}", hash)
        lbl_Status.Text = String.Format("Finished")
        IO.Directory.Delete(saving_file.DirectoryName & "\TEMP\", True)

        btn_start.Visible = Not btn_start.Visible : btn_abort.Visible = Not btn_abort.Visible
    End Function

    Sub download_folder(folder As String, CT As CancellationToken)
        Dim uids = imap_client.Search(filter, folder)
        Dim count = 0
        Dim dest_folder = IO.Path.Combine(saving_file.DirectoryName, "TEMP", folder & "\")
        Dim mail_path
        Dim message As Net.Mail.MailMessage
        Dim err_count As Integer
        IO.Directory.CreateDirectory(dest_folder)
        For Each uid In uids
            If CT.IsCancellationRequested Then
                logger.Warn($"Operation aborted by the user!")
                Exit For
            End If
            Try
                mail_path = dest_folder & uid & ".eml"
                If Not IO.File.Exists(mail_path) Then
                    message = imap_client.GetMessage(uid, FetchOptions.Normal, False, folder)
                    If message.To.Count = 0 Then
                        message.To.Add("UNKNOWN@forensicsimapdownloader.net")
                        logger.Warn($"Added UKNOWN in empty 'To' field for preventing Exception in email with uid: {uid} in {folder}")
                    End If
                    If IsNothing(message.From) Then
                        message.From = New Net.Mail.MailAddress("UKNOWN@forensicsimapdownloader.net")
                        logger.Warn($"Added UKNOWN in empty 'From' field for preventing Exception in email with uid: {uid} in {folder}")
                    End If
                    message.SaveMailMessage(mail_path)
                End If
            Catch ex As Exception
                logger.Warn("Folder {2}: email {0} of {1} had a problem.", count + 1, uids.Count, folder)
                logger.Warn(ex)
                err_count += 1
                If IO.File.Exists(mail_path) Then IO.File.Delete(mail_path)
            Finally
                Dim status = String.Format("Downloading {0}/{1} ({2}%)", count + 1, uids.Count, (count + 1) * 100 \ uids.Count)
                data_tb.Rows.Item(folders.IndexOf(folder)).Cells.Item(3).Value = status
                data_tb.Rows.Item(folders.IndexOf(folder)).Cells.Item(3).Style.BackColor = Color.FromArgb(255, 201, 102)
                count += 1
                Application.DoEvents()
            End Try
        Next
        With data_tb.Rows.Item(folders.IndexOf(folder)).Cells.Item(3)
            If err_count > 0 Then
                .Value = $"Completed with {err_count} errors"
                .Style.BackColor = Color.FromArgb(255, 102, 102)
            Else
                .Value = "Completed."
                .Style.BackColor = Color.FromArgb(133, 224, 133)
            End If
            If CT.IsCancellationRequested Then
                logger.Warn($"Downloaded only {count} emails out of {uids.Count} from {folder}")
                .Value = $"Aborted! Downloaded only {count} emails out of {uids.Count}"
                .Style.BackColor = Color.FromArgb(255, 102, 102)
            Else
                logger.Info("Download of {0} completed with {1} errors.", folder, err_count)
            End If
        End With

    End Sub
End Class
