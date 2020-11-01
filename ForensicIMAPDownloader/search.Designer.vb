<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class search
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.btn_cancel = New System.Windows.Forms.Button()
        Me.btn_ok = New System.Windows.Forms.Button()
        Me.search_builder1 = New ForensicIMAPDownloader.search_builder()
        Me.SuspendLayout()
        '
        'btn_cancel
        '
        Me.btn_cancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btn_cancel.Location = New System.Drawing.Point(191, 145)
        Me.btn_cancel.Name = "btn_cancel"
        Me.btn_cancel.Size = New System.Drawing.Size(94, 23)
        Me.btn_cancel.TabIndex = 0
        Me.btn_cancel.Text = "C&ancel"
        Me.btn_cancel.UseVisualStyleBackColor = True
        '
        'btn_ok
        '
        Me.btn_ok.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_ok.Location = New System.Drawing.Point(291, 145)
        Me.btn_ok.Name = "btn_ok"
        Me.btn_ok.Size = New System.Drawing.Size(93, 23)
        Me.btn_ok.TabIndex = 1
        Me.btn_ok.Text = "&Confirm"
        Me.btn_ok.UseVisualStyleBackColor = True
        '
        'search_builder1
        '
        Me.search_builder1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.search_builder1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.search_builder1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.search_builder1.Location = New System.Drawing.Point(12, 12)
        Me.search_builder1.Name = "search_builder1"
        Me.search_builder1.Size = New System.Drawing.Size(372, 127)
        Me.search_builder1.TabIndex = 4
        '
        'search
        '
        Me.AcceptButton = Me.btn_ok
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btn_cancel
        Me.ClientSize = New System.Drawing.Size(396, 176)
        Me.Controls.Add(Me.search_builder1)
        Me.Controls.Add(Me.btn_ok)
        Me.Controls.Add(Me.btn_cancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.MinimumSize = New System.Drawing.Size(267, 171)
        Me.Name = "search"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Search Criteria"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btn_cancel As Button
    Friend WithEvents btn_ok As Button
    Friend WithEvents search_builder1 As search_builder
End Class
