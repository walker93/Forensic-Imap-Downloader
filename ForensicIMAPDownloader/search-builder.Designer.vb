<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class search_builder
    Inherits System.Windows.Forms.UserControl

    'UserControl esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(search_builder))
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.AND_btn = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.NOT_btn = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.param_cb = New System.Windows.Forms.ToolStripComboBox()
        Me.ToolStripContainer1 = New System.Windows.Forms.ToolStripContainer()
        Me.query_tb = New ScintillaNET.Scintilla()
        Me.AutocompleteMenu1 = New AutocompleteMenuNS.AutocompleteMenu()
        Me.sel_lbl = New System.Windows.Forms.ToolStripLabel()
        Me.ToolStrip1.SuspendLayout()
        Me.ToolStripContainer1.ContentPanel.SuspendLayout()
        Me.ToolStripContainer1.TopToolStripPanel.SuspendLayout()
        Me.ToolStripContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Dock = System.Windows.Forms.DockStyle.None
        Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AND_btn, Me.ToolStripButton1, Me.NOT_btn, Me.ToolStripSeparator1, Me.param_cb, Me.sel_lbl})
        Me.ToolStrip1.Location = New System.Drawing.Point(3, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
        Me.ToolStrip1.Size = New System.Drawing.Size(285, 25)
        Me.ToolStrip1.TabIndex = 1
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'AND_btn
        '
        Me.AND_btn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.AND_btn.Image = CType(resources.GetObject("AND_btn.Image"), System.Drawing.Image)
        Me.AND_btn.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.AND_btn.Name = "AND_btn"
        Me.AND_btn.Size = New System.Drawing.Size(36, 22)
        Me.AND_btn.Text = "AND"
        Me.AND_btn.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), System.Drawing.Image)
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(27, 22)
        Me.ToolStripButton1.Text = "OR"
        '
        'NOT_btn
        '
        Me.NOT_btn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.NOT_btn.Image = CType(resources.GetObject("NOT_btn.Image"), System.Drawing.Image)
        Me.NOT_btn.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.NOT_btn.Name = "NOT_btn"
        Me.NOT_btn.Size = New System.Drawing.Size(34, 22)
        Me.NOT_btn.Text = "NOT"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'param_cb
        '
        Me.param_cb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.param_cb.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.param_cb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.param_cb.Name = "param_cb"
        Me.param_cb.Size = New System.Drawing.Size(121, 25)
        '
        'ToolStripContainer1
        '
        Me.ToolStripContainer1.BottomToolStripPanelVisible = False
        '
        'ToolStripContainer1.ContentPanel
        '
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.query_tb)
        Me.ToolStripContainer1.ContentPanel.Size = New System.Drawing.Size(500, 313)
        Me.ToolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ToolStripContainer1.LeftToolStripPanelVisible = False
        Me.ToolStripContainer1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStripContainer1.Name = "ToolStripContainer1"
        Me.ToolStripContainer1.RightToolStripPanelVisible = False
        Me.ToolStripContainer1.Size = New System.Drawing.Size(500, 338)
        Me.ToolStripContainer1.TabIndex = 2
        Me.ToolStripContainer1.Text = "ToolStripContainer1"
        '
        'ToolStripContainer1.TopToolStripPanel
        '
        Me.ToolStripContainer1.TopToolStripPanel.Controls.Add(Me.ToolStrip1)
        '
        'query_tb
        '
        Me.query_tb.AutoCIgnoreCase = True
        Me.query_tb.Dock = System.Windows.Forms.DockStyle.Fill
        Me.query_tb.Location = New System.Drawing.Point(0, 0)
        Me.query_tb.Name = "query_tb"
        Me.query_tb.Size = New System.Drawing.Size(500, 313)
        Me.query_tb.TabIndex = 1
        Me.query_tb.WrapMode = ScintillaNET.WrapMode.Whitespace
        '
        'AutocompleteMenu1
        '
        Me.AutocompleteMenu1.Colors = CType(resources.GetObject("AutocompleteMenu1.Colors"), AutocompleteMenuNS.Colors)
        Me.AutocompleteMenu1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!)
        Me.AutocompleteMenu1.ImageList = Nothing
        Me.AutocompleteMenu1.Items = New String(-1) {}
        Me.AutocompleteMenu1.TargetControlWrapper = Nothing
        '
        'sel_lbl
        '
        Me.sel_lbl.Name = "sel_lbl"
        Me.sel_lbl.Size = New System.Drawing.Size(25, 22)
        Me.sel_lbl.Text = "Sel:"
        '
        'search_builder
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.ToolStripContainer1)
        Me.Name = "search_builder"
        Me.Size = New System.Drawing.Size(500, 338)
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ToolStripContainer1.ContentPanel.ResumeLayout(False)
        Me.ToolStripContainer1.TopToolStripPanel.ResumeLayout(False)
        Me.ToolStripContainer1.TopToolStripPanel.PerformLayout()
        Me.ToolStripContainer1.ResumeLayout(False)
        Me.ToolStripContainer1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents AND_btn As ToolStripButton
    Friend WithEvents ToolStripButton1 As ToolStripButton
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents param_cb As ToolStripComboBox
    Friend WithEvents ToolStripContainer1 As ToolStripContainer
    Friend WithEvents query_tb As ScintillaNET.Scintilla
    Friend WithEvents AutocompleteMenu1 As AutocompleteMenuNS.AutocompleteMenu
    Friend WithEvents NOT_btn As ToolStripButton
    Friend WithEvents sel_lbl As ToolStripLabel
End Class
