''' <summary>
''' Lets the examiner enter their own OAuth application registrations (see docs/oauth-setup.md).
''' Built in code rather than with the designer: it is a handful of fixed controls.
''' </summary>
Public Class OAuthSettingsForm
    Inherits Form

    Private ReadOnly settings As OAuthAppSettings
    Private ReadOnly txt_ms_client As New TextBox()
    Private ReadOnly txt_ms_tenant As New TextBox()
    Private ReadOnly cb_device_code As New CheckBox()
    Private ReadOnly txt_google_path As New TextBox()

    Public Sub New(settings As OAuthAppSettings)
        Me.settings = settings

        Text = "OAuth settings"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        AutoScaleDimensions = New SizeF(6.0!, 13.0!)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(440, 262)

        Dim microsoftBox As New GroupBox With {.Text = "Microsoft (Outlook.com / Microsoft 365)", .Location = New Point(12, 12), .Size = New Size(416, 120)}
        microsoftBox.Controls.Add(New Label With {.Text = "Client ID:", .Location = New Point(10, 25), .AutoSize = True})
        txt_ms_client.Location = New Point(80, 22)
        txt_ms_client.Size = New Size(324, 20)
        txt_ms_client.PlaceholderText = If(OAuthAppSettings.DefaultMicrosoftClientId = "", "Application (client) ID of your Entra app registration", "Leave empty to use the built-in application")
        microsoftBox.Controls.Add(txt_ms_client)
        microsoftBox.Controls.Add(New Label With {.Text = "Tenant:", .Location = New Point(10, 51), .AutoSize = True})
        txt_ms_tenant.Location = New Point(80, 48)
        txt_ms_tenant.Size = New Size(140, 20)
        microsoftBox.Controls.Add(txt_ms_tenant)
        microsoftBox.Controls.Add(New Label With {.Text = "consumers = personal accounts only", .Location = New Point(226, 51), .AutoSize = True, .ForeColor = SystemColors.GrayText})
        cb_device_code.Text = "Sign in with a device code (the account owner signs in on another device)"
        cb_device_code.Location = New Point(13, 82)
        cb_device_code.AutoSize = True
        microsoftBox.Controls.Add(cb_device_code)

        Dim googleBox As New GroupBox With {.Text = "Google (Gmail)", .Location = New Point(12, 140), .Size = New Size(416, 76)}
        googleBox.Controls.Add(New Label With {.Text = "Client file:", .Location = New Point(10, 25), .AutoSize = True})
        txt_google_path.Location = New Point(80, 22)
        txt_google_path.Size = New Size(258, 20)
        txt_google_path.ReadOnly = True
        googleBox.Controls.Add(txt_google_path)
        Dim btn_google_browse As New Button With {.Text = "&Browse", .Location = New Point(344, 20), .Size = New Size(60, 23)}
        AddHandler btn_google_browse.Click, AddressOf BrowseGoogleClientFile
        googleBox.Controls.Add(btn_google_browse)
        googleBox.Controls.Add(New Label With {.Text = "client_secret_*.json of a ""Desktop app"" OAuth client", .Location = New Point(80, 48), .AutoSize = True, .ForeColor = SystemColors.GrayText})

        Dim btn_ok As New Button With {.Text = "OK", .Location = New Point(272, 227), .Size = New Size(75, 23)}
        AddHandler btn_ok.Click, AddressOf SaveAndClose
        Dim btn_cancel As New Button With {.Text = "Cancel", .Location = New Point(353, 227), .Size = New Size(75, 23), .DialogResult = DialogResult.Cancel}

        Controls.AddRange({microsoftBox, googleBox, btn_ok, btn_cancel})
        AcceptButton = btn_ok
        CancelButton = btn_cancel

        txt_ms_client.Text = settings.MicrosoftClientId
        txt_ms_tenant.Text = settings.MicrosoftTenant
        cb_device_code.Checked = settings.MicrosoftUseDeviceCode
        txt_google_path.Text = settings.GoogleClientSecretsPath
    End Sub

    Private Sub BrowseGoogleClientFile(sender As Object, e As EventArgs)
        Using dialog As New OpenFileDialog With {.Filter = "Google OAuth client (*.json)|*.json", .CheckFileExists = True}
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
            Try
                GoogleTokenProvider.FromClientSecretsFile(dialog.FileName)
            Catch ex As Exception
                MsgBox("This is not a Google ""Desktop app"" OAuth client file." & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
                Exit Sub
            End Try
            txt_google_path.Text = dialog.FileName
        End Using
    End Sub

    Private Sub SaveAndClose(sender As Object, e As EventArgs)
        settings.MicrosoftClientId = txt_ms_client.Text.Trim()
        settings.MicrosoftTenant = If(txt_ms_tenant.Text.Trim() = "", MicrosoftTokenProvider.ConsumersTenant, txt_ms_tenant.Text.Trim())
        settings.MicrosoftUseDeviceCode = cb_device_code.Checked
        settings.GoogleClientSecretsPath = txt_google_path.Text
        DialogResult = DialogResult.OK
        Close()
    End Sub

End Class
