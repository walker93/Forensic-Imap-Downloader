<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Abort
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata in Progettazione Windows Form.  
    'Non modificarla mediante l'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Label1 = New Label()
        choice1_discard = New RadioButton()
        choice2_keep = New RadioButton()
        choice3_finish = New RadioButton()
        btn_ok = New Button()
        btn_cancel = New Button()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(14, 10)
        Label1.Margin = New Padding(4, 0, 4, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(245, 30)
        Label1.TabIndex = 0
        Label1.Text = "You are about to abort the current download." & vbCrLf & "Would you like to:"
        ' 
        ' choice1_discard
        ' 
        choice1_discard.AutoSize = True
        choice1_discard.Checked = True
        choice1_discard.Location = New Point(18, 44)
        choice1_discard.Margin = New Padding(4, 3, 4, 3)
        choice1_discard.Name = "choice1_discard"
        choice1_discard.Size = New Size(170, 19)
        choice1_discard.TabIndex = 1
        choice1_discard.TabStop = True
        choice1_discard.Text = "Discard downloaded emails"
        choice1_discard.UseVisualStyleBackColor = True
        ' 
        ' choice2_keep
        ' 
        choice2_keep.AutoSize = True
        choice2_keep.Location = New Point(18, 70)
        choice2_keep.Margin = New Padding(4, 3, 4, 3)
        choice2_keep.Name = "choice2_keep"
        choice2_keep.Size = New Size(404, 19)
        choice2_keep.TabIndex = 1
        choice2_keep.Text = "Keep downloaded emails in TEMP folder to continue the download later"
        choice2_keep.UseVisualStyleBackColor = True
        ' 
        ' choice3_finish
        ' 
        choice3_finish.AutoSize = True
        choice3_finish.Location = New Point(18, 97)
        choice3_finish.Margin = New Padding(4, 3, 4, 3)
        choice3_finish.Name = "choice3_finish"
        choice3_finish.Size = New Size(413, 19)
        choice3_finish.TabIndex = 1
        choice3_finish.Text = "Archive and calculate hash of downloaded emails to finish the acquisition"
        choice3_finish.UseVisualStyleBackColor = True
        ' 
        ' btn_ok
        ' 
        btn_ok.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btn_ok.Location = New Point(374, 134)
        btn_ok.Margin = New Padding(4, 3, 4, 3)
        btn_ok.Name = "btn_ok"
        btn_ok.Size = New Size(88, 27)
        btn_ok.TabIndex = 2
        btn_ok.Text = "&Ok"
        btn_ok.UseVisualStyleBackColor = True
        ' 
        ' btn_cancel
        ' 
        btn_cancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btn_cancel.DialogResult = DialogResult.Cancel
        btn_cancel.Location = New Point(279, 134)
        btn_cancel.Margin = New Padding(4, 3, 4, 3)
        btn_cancel.Name = "btn_cancel"
        btn_cancel.Size = New Size(88, 27)
        btn_cancel.TabIndex = 2
        btn_cancel.Text = "&Cancel"
        btn_cancel.UseVisualStyleBackColor = True
        ' 
        ' Abort
        ' 
        AcceptButton = btn_ok
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btn_cancel
        ClientSize = New Size(475, 175)
        ControlBox = False
        Controls.Add(btn_cancel)
        Controls.Add(btn_ok)
        Controls.Add(choice3_finish)
        Controls.Add(choice2_keep)
        Controls.Add(choice1_discard)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.FixedDialog
        Margin = New Padding(4, 3, 4, 3)
        MaximizeBox = False
        MinimizeBox = False
        Name = "Abort"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Abort"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents choice1_discard As RadioButton
    Friend WithEvents choice2_keep As RadioButton
    Friend WithEvents choice3_finish As RadioButton
    Friend WithEvents btn_ok As Button
    Friend WithEvents btn_cancel As Button
End Class
