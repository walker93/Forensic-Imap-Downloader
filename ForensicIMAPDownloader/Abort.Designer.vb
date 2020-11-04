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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.choice1_discard = New System.Windows.Forms.RadioButton()
        Me.choice2_keep = New System.Windows.Forms.RadioButton()
        Me.choice3_finish = New System.Windows.Forms.RadioButton()
        Me.btn_ok = New System.Windows.Forms.Button()
        Me.btn_cancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(219, 26)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "You are about to abort the current download." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Would you like to:"
        '
        'choice1_discard
        '
        Me.choice1_discard.AutoSize = True
        Me.choice1_discard.Checked = True
        Me.choice1_discard.Location = New System.Drawing.Point(15, 38)
        Me.choice1_discard.Name = "choice1_discard"
        Me.choice1_discard.Size = New System.Drawing.Size(154, 17)
        Me.choice1_discard.TabIndex = 1
        Me.choice1_discard.TabStop = True
        Me.choice1_discard.Text = "Discard downloaded emails"
        Me.choice1_discard.UseVisualStyleBackColor = True
        '
        'choice2_keep
        '
        Me.choice2_keep.AutoSize = True
        Me.choice2_keep.Location = New System.Drawing.Point(15, 61)
        Me.choice2_keep.Name = "choice2_keep"
        Me.choice2_keep.Size = New System.Drawing.Size(362, 17)
        Me.choice2_keep.TabIndex = 1
        Me.choice2_keep.Text = "Keep downloaded emails in TEMP folder to continue the download later"
        Me.choice2_keep.UseVisualStyleBackColor = True
        '
        'choice3_finish
        '
        Me.choice3_finish.AutoSize = True
        Me.choice3_finish.Location = New System.Drawing.Point(15, 84)
        Me.choice3_finish.Name = "choice3_finish"
        Me.choice3_finish.Size = New System.Drawing.Size(369, 17)
        Me.choice3_finish.TabIndex = 1
        Me.choice3_finish.Text = "Archive and calculate hash of downloaded emails to finish the acquisition"
        Me.choice3_finish.UseVisualStyleBackColor = True
        '
        'btn_ok
        '
        Me.btn_ok.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_ok.Location = New System.Drawing.Point(304, 107)
        Me.btn_ok.Name = "btn_ok"
        Me.btn_ok.Size = New System.Drawing.Size(75, 23)
        Me.btn_ok.TabIndex = 2
        Me.btn_ok.Text = "&Ok"
        Me.btn_ok.UseVisualStyleBackColor = True
        '
        'btn_cancel
        '
        Me.btn_cancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btn_cancel.Location = New System.Drawing.Point(223, 107)
        Me.btn_cancel.Name = "btn_cancel"
        Me.btn_cancel.Size = New System.Drawing.Size(75, 23)
        Me.btn_cancel.TabIndex = 2
        Me.btn_cancel.Text = "&Cancel"
        Me.btn_cancel.UseVisualStyleBackColor = True
        '
        'Abort
        '
        Me.AcceptButton = Me.btn_ok
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btn_cancel
        Me.ClientSize = New System.Drawing.Size(391, 142)
        Me.ControlBox = False
        Me.Controls.Add(Me.btn_cancel)
        Me.Controls.Add(Me.btn_ok)
        Me.Controls.Add(Me.choice3_finish)
        Me.Controls.Add(Me.choice2_keep)
        Me.Controls.Add(Me.choice1_discard)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Abort"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Abort"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents choice1_discard As RadioButton
    Friend WithEvents choice2_keep As RadioButton
    Friend WithEvents choice3_finish As RadioButton
    Friend WithEvents btn_ok As Button
    Friend WithEvents btn_cancel As Button
End Class
