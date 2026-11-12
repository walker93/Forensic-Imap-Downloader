''' <summary>
''' Shows the device-code sign-in instructions while the sign-in is pending.
''' Closing it cancels the sign-in.
''' </summary>
Public Class DeviceCodeForm
    Inherits Form

    Public Sub New(info As DeviceCodeInfo)
        Text = "Sign in with a device code"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        AutoScaleDimensions = New SizeF(6.0!, 13.0!)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(380, 150)

        Controls.Add(New Label With {.Text = "On any device, open this page and enter the code:", .Location = New Point(12, 12), .AutoSize = True})
        Controls.Add(New TextBox With {.Text = info.VerificationUrl, .Location = New Point(15, 34), .Size = New Size(350, 20), .ReadOnly = True})
        Controls.Add(New TextBox With {
            .Text = info.UserCode,
            .Location = New Point(15, 64),
            .Size = New Size(350, 29),
            .ReadOnly = True,
            .TextAlign = HorizontalAlignment.Center,
            .Font = New Font(FontFamily.GenericMonospace, 14.0!, FontStyle.Bold)
        })

        Dim btn_copy As New Button With {.Text = "&Copy code", .Location = New Point(15, 112), .Size = New Size(90, 23)}
        AddHandler btn_copy.Click, Sub() Clipboard.SetText(info.UserCode)
        Dim btn_cancel As New Button With {.Text = "Cancel", .Location = New Point(290, 112), .Size = New Size(75, 23)}
        AddHandler btn_cancel.Click, Sub() Close()
        Controls.Add(btn_copy)
        Controls.Add(btn_cancel)
        Controls.Add(New Label With {.Text = "Waiting for the sign-in to complete...", .Location = New Point(111, 117), .AutoSize = True, .ForeColor = SystemColors.GrayText})
    End Sub

End Class
