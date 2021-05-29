<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
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
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.btn_abort = New System.Windows.Forms.Button()
        Me.num_picker = New System.Windows.Forms.NumericUpDown()
        Me.cb_ssl = New System.Windows.Forms.CheckBox()
        Me.btn_browse = New System.Windows.Forms.Button()
        Me.btn_start = New System.Windows.Forms.Button()
        Me.btn_filter = New System.Windows.Forms.Button()
        Me.btn_check = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txt_pass = New System.Windows.Forms.TextBox()
        Me.txt_port = New System.Windows.Forms.TextBox()
        Me.txt_path = New System.Windows.Forms.TextBox()
        Me.txt_user = New System.Windows.Forms.TextBox()
        Me.txt_server = New System.Windows.Forms.TextBox()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.lbl_Status = New System.Windows.Forms.ToolStripStatusLabel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.data_tb = New System.Windows.Forms.DataGridView()
        Me.cb_col = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.folders_col = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.mess_num_col = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.prog_col = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.GroupBox1.SuspendLayout()
        CType(Me.num_picker, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.StatusStrip1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.data_tb, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.btn_abort)
        Me.GroupBox1.Controls.Add(Me.num_picker)
        Me.GroupBox1.Controls.Add(Me.cb_ssl)
        Me.GroupBox1.Controls.Add(Me.btn_browse)
        Me.GroupBox1.Controls.Add(Me.btn_start)
        Me.GroupBox1.Controls.Add(Me.btn_filter)
        Me.GroupBox1.Controls.Add(Me.btn_check)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label6)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.txt_pass)
        Me.GroupBox1.Controls.Add(Me.txt_port)
        Me.GroupBox1.Controls.Add(Me.txt_path)
        Me.GroupBox1.Controls.Add(Me.txt_user)
        Me.GroupBox1.Controls.Add(Me.txt_server)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(590, 106)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Settings"
        '
        'btn_abort
        '
        Me.btn_abort.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_abort.Location = New System.Drawing.Point(504, 67)
        Me.btn_abort.Name = "btn_abort"
        Me.btn_abort.Size = New System.Drawing.Size(75, 23)
        Me.btn_abort.TabIndex = 10
        Me.btn_abort.Text = "&Abort"
        Me.btn_abort.UseVisualStyleBackColor = True
        Me.btn_abort.Visible = False
        '
        'num_picker
        '
        Me.num_picker.Location = New System.Drawing.Point(458, 18)
        Me.num_picker.Maximum = New Decimal(New Integer() {15, 0, 0, 0})
        Me.num_picker.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.num_picker.Name = "num_picker"
        Me.num_picker.Size = New System.Drawing.Size(33, 20)
        Me.num_picker.TabIndex = 1001
        Me.num_picker.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'cb_ssl
        '
        Me.cb_ssl.AutoSize = True
        Me.cb_ssl.Checked = True
        Me.cb_ssl.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cb_ssl.Location = New System.Drawing.Point(308, 19)
        Me.cb_ssl.Name = "cb_ssl"
        Me.cb_ssl.Size = New System.Drawing.Size(46, 17)
        Me.cb_ssl.TabIndex = 3
        Me.cb_ssl.Text = "SSL"
        Me.cb_ssl.UseVisualStyleBackColor = True
        '
        'btn_browse
        '
        Me.btn_browse.Location = New System.Drawing.Point(431, 67)
        Me.btn_browse.Name = "btn_browse"
        Me.btn_browse.Size = New System.Drawing.Size(60, 23)
        Me.btn_browse.TabIndex = 6
        Me.btn_browse.Text = "&Browse"
        Me.btn_browse.UseVisualStyleBackColor = True
        '
        'btn_start
        '
        Me.btn_start.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_start.Enabled = False
        Me.btn_start.Location = New System.Drawing.Point(504, 67)
        Me.btn_start.Name = "btn_start"
        Me.btn_start.Size = New System.Drawing.Size(75, 23)
        Me.btn_start.TabIndex = 9
        Me.btn_start.Text = "&Download"
        Me.btn_start.UseVisualStyleBackColor = True
        '
        'btn_filter
        '
        Me.btn_filter.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_filter.Enabled = False
        Me.btn_filter.Location = New System.Drawing.Point(504, 41)
        Me.btn_filter.Name = "btn_filter"
        Me.btn_filter.Size = New System.Drawing.Size(75, 23)
        Me.btn_filter.TabIndex = 8
        Me.btn_filter.Text = "&Filter"
        Me.btn_filter.UseVisualStyleBackColor = True
        '
        'btn_check
        '
        Me.btn_check.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_check.Enabled = False
        Me.btn_check.Location = New System.Drawing.Point(504, 15)
        Me.btn_check.Name = "btn_check"
        Me.btn_check.Size = New System.Drawing.Size(75, 23)
        Me.btn_check.TabIndex = 7
        Me.btn_check.Text = "&Connect"
        Me.btn_check.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(193, 20)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(29, 13)
        Me.Label4.TabIndex = 2
        Me.Label4.Text = "Port:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(6, 20)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(70, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "IMAP Server:"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(360, 20)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(92, 13)
        Me.Label6.TabIndex = 2
        Me.Label6.Text = "Max Connections:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(289, 46)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(56, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Password:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(18, 72)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(60, 13)
        Me.Label5.TabIndex = 2
        Me.Label5.Text = "Save Path:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 46)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(58, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Username:"
        '
        'txt_pass
        '
        Me.txt_pass.Location = New System.Drawing.Point(351, 43)
        Me.txt_pass.Name = "txt_pass"
        Me.txt_pass.Size = New System.Drawing.Size(140, 20)
        Me.txt_pass.TabIndex = 5
        '
        'txt_port
        '
        Me.txt_port.Location = New System.Drawing.Point(228, 17)
        Me.txt_port.Name = "txt_port"
        Me.txt_port.Size = New System.Drawing.Size(74, 20)
        Me.txt_port.TabIndex = 2
        Me.txt_port.Text = "993"
        '
        'txt_path
        '
        Me.txt_path.Location = New System.Drawing.Point(82, 69)
        Me.txt_path.Name = "txt_path"
        Me.txt_path.ReadOnly = True
        Me.txt_path.Size = New System.Drawing.Size(343, 20)
        Me.txt_path.TabIndex = 1000
        Me.txt_path.TabStop = False
        '
        'txt_user
        '
        Me.txt_user.Location = New System.Drawing.Point(82, 43)
        Me.txt_user.Name = "txt_user"
        Me.txt_user.Size = New System.Drawing.Size(201, 20)
        Me.txt_user.TabIndex = 4
        '
        'txt_server
        '
        Me.txt_server.AutoCompleteCustomSource.AddRange(New String() {"imaps.aruba.it", "imap.gmail.com", "imap.mail.me.com", "outlook.office365.com", "imap-mail.outlook.com", "imap.mail.com", "in.virgilio.it", "imapmail.libero.it", "imap.tiscali.it", "imap.hostinger.com"})
        Me.txt_server.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.txt_server.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource
        Me.txt_server.Location = New System.Drawing.Point(82, 17)
        Me.txt_server.Name = "txt_server"
        Me.txt_server.Size = New System.Drawing.Size(105, 20)
        Me.txt_server.TabIndex = 1
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lbl_Status})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 315)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(614, 22)
        Me.StatusStrip1.TabIndex = 1
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'lbl_Status
        '
        Me.lbl_Status.Name = "lbl_Status"
        Me.lbl_Status.Size = New System.Drawing.Size(23, 17)
        Me.lbl_Status.Text = "0%"
        '
        'Panel1
        '
        Me.Panel1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel1.Controls.Add(Me.data_tb)
        Me.Panel1.Location = New System.Drawing.Point(0, 124)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(614, 188)
        Me.Panel1.TabIndex = 2
        '
        'data_tb
        '
        Me.data_tb.AllowUserToAddRows = False
        Me.data_tb.AllowUserToDeleteRows = False
        Me.data_tb.AllowUserToResizeColumns = False
        Me.data_tb.AllowUserToResizeRows = False
        Me.data_tb.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.data_tb.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.cb_col, Me.folders_col, Me.mess_num_col, Me.prog_col})
        Me.data_tb.Dock = System.Windows.Forms.DockStyle.Fill
        Me.data_tb.GridColor = System.Drawing.SystemColors.Control
        Me.data_tb.Location = New System.Drawing.Point(0, 0)
        Me.data_tb.Name = "data_tb"
        Me.data_tb.RowHeadersVisible = False
        Me.data_tb.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.data_tb.Size = New System.Drawing.Size(614, 188)
        Me.data_tb.TabIndex = 0
        Me.data_tb.TabStop = False
        '
        'cb_col
        '
        Me.cb_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.cb_col.HeaderText = "Selected"
        Me.cb_col.Name = "cb_col"
        Me.cb_col.Width = 55
        '
        'folders_col
        '
        Me.folders_col.HeaderText = "Folders"
        Me.folders_col.Name = "folders_col"
        Me.folders_col.ReadOnly = True
        '
        'mess_num_col
        '
        Me.mess_num_col.HeaderText = "Messages"
        Me.mess_num_col.Name = "mess_num_col"
        Me.mess_num_col.ReadOnly = True
        '
        'prog_col
        '
        Me.prog_col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.prog_col.HeaderText = "Progress"
        Me.prog_col.Name = "prog_col"
        Me.prog_col.ReadOnly = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(614, 337)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.GroupBox1)
        Me.MinimumSize = New System.Drawing.Size(570, 300)
        Me.Name = "Form1"
        Me.ShowIcon = False
        Me.Text = "Forensic IMAP Downloader"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.num_picker, System.ComponentModel.ISupportInitialize).EndInit()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        CType(Me.data_tb, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Label1 As Label
    Friend WithEvents txt_user As TextBox
    Friend WithEvents txt_server As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents txt_pass As TextBox
    Friend WithEvents txt_port As TextBox
    Friend WithEvents btn_check As Button
    Friend WithEvents cb_ssl As CheckBox
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents lbl_Status As ToolStripStatusLabel
    Friend WithEvents Panel1 As Panel
    Friend WithEvents data_tb As DataGridView
    Friend WithEvents Label5 As Label
    Friend WithEvents txt_path As TextBox
    Friend WithEvents btn_browse As Button
    Friend WithEvents cb_col As DataGridViewCheckBoxColumn
    Friend WithEvents folders_col As DataGridViewTextBoxColumn
    Friend WithEvents mess_num_col As DataGridViewTextBoxColumn
    Friend WithEvents prog_col As DataGridViewTextBoxColumn
    Friend WithEvents btn_start As Button
    Friend WithEvents btn_filter As Button
    Friend WithEvents num_picker As NumericUpDown
    Friend WithEvents Label6 As Label
    Friend WithEvents btn_abort As Button
End Class
