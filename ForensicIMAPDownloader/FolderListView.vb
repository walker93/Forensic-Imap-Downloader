''' <summary>
''' The folder list of the main window: which folders to download, how many messages each has,
''' and the download progress.
'''
''' Rows are always found by folder name, never by position, so sorting the grid can never
''' make an action land on a different folder. Progress can be reported from any thread.
''' </summary>
Public Class FolderListView
    Inherits UserControl

    Public Enum FolderState
        Pending
        Downloading
        Completed
        CompletedWithErrors
        Aborted
        Failed
    End Enum

    Private Const ColSelected As String = "Selected"
    Private Const ColName As String = "Folder"
    Private Const ColMatching As String = "Matching"
    Private Const ColTotal As String = "Total"
    Private Const ColProgress As String = "Progress"

    Private ReadOnly grid As New DataGridView()
    Private ReadOnly btnSelectAll As New ToolStripButton("Select all")
    Private ReadOnly btnSelectNone As New ToolStripButton("Select none")
    Private ReadOnly btnInvert As New ToolStripButton("Invert selection")
    Private ReadOnly lblSummary As New ToolStripLabel()
    Private locked As Boolean

    Public Sub New()
        Dim toolbar As New ToolStrip With {.GripStyle = ToolStripGripStyle.Hidden, .Dock = DockStyle.Top}
        lblSummary.Alignment = ToolStripItemAlignment.Right
        toolbar.Items.AddRange({btnSelectAll, btnSelectNone, btnInvert, lblSummary})
        AddHandler btnSelectAll.Click, Sub() SetAllSelected(Function(current) True)
        AddHandler btnSelectNone.Click, Sub() SetAllSelected(Function(current) False)
        AddHandler btnInvert.Click, Sub() SetAllSelected(Function(current) Not current)

        grid.Dock = DockStyle.Fill
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.AllowUserToResizeColumns = True
        grid.AllowUserToOrderColumns = True
        grid.RowHeadersVisible = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect = True
        grid.GridColor = SystemColors.Control
        grid.BackgroundColor = SystemColors.Window
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        grid.Columns.Add(New DataGridViewCheckBoxColumn With {
            .Name = ColSelected, .HeaderText = "✓", .ToolTipText = "Download this folder",
            .SortMode = DataGridViewColumnSortMode.Automatic, .ValueType = GetType(Boolean),
            .FillWeight = 8, .MinimumWidth = 32})
        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = ColName, .HeaderText = "Folder", .ReadOnly = True, .FillWeight = 45, .MinimumWidth = 80})
        grid.Columns.Add(NumberColumn(ColMatching, "Messages", "Messages matching the current filter"))
        grid.Columns.Add(NumberColumn(ColTotal, "Total", "All messages in the folder"))
        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = ColProgress, .HeaderText = "Progress", .ReadOnly = True, .FillWeight = 37, .MinimumWidth = 80})

        ' A checkbox click is only "dirty" until the cell loses focus: commit it at once, so the
        ' state the operator sees is the state Download reads.
        AddHandler grid.CurrentCellDirtyStateChanged,
            Sub()
                If grid.IsCurrentCellDirty Then grid.CommitEdit(DataGridViewDataErrorContexts.Commit)
            End Sub
        AddHandler grid.CellValueChanged,
            Sub(sender, e)
                If e.RowIndex >= 0 AndAlso grid.Columns(e.ColumnIndex).Name = ColSelected Then UpdateSummary()
            End Sub
        ' Space toggles every highlighted row, so a block of folders can be (de)selected at once.
        AddHandler grid.KeyDown, AddressOf Grid_KeyDown

        Controls.Add(grid)
        Controls.Add(toolbar)
        UpdateSummary()
    End Sub

    Private Shared Function NumberColumn(name As String, header As String, tooltip As String) As DataGridViewTextBoxColumn
        Dim column As New DataGridViewTextBoxColumn With {
            .Name = name, .HeaderText = header, .ToolTipText = tooltip, .ReadOnly = True,
            .ValueType = GetType(Integer), .FillWeight = 10, .MinimumWidth = 55}
        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        Return column
    End Function

    ''' <summary>Replaces the list. Every folder starts selected.</summary>
    Public Sub SetFolders(folders As IEnumerable(Of KeyValuePair(Of String, Integer)))
        grid.Rows.Clear()
        For Each folder In folders
            grid.Rows.Add(True, folder.Key, folder.Value, folder.Value, "")
        Next
        UpdateSummary()
    End Sub

    Public Sub Clear()
        grid.Rows.Clear()
        UpdateSummary()
    End Sub

    ''' <summary>Messages in the folder matching the current filter.</summary>
    Public Sub SetMatchingCount(folderName As String, count As Integer)
        Dim row = FindRow(folderName)
        If row Is Nothing Then Exit Sub
        row.Cells(ColMatching).Value = count
        UpdateSummary()
    End Sub

    Public Function GetTotalCount(folderName As String) As Integer
        Dim row = FindRow(folderName)
        Return If(row Is Nothing, 0, CInt(row.Cells(ColTotal).Value))
    End Function

    ''' <summary>The folders ticked for download, in the order currently shown.</summary>
    Public Function SelectedFolders() As List(Of String)
        Return (From row In grid.Rows.Cast(Of DataGridViewRow)()
                Where IsSelected(row)
                Select CStr(row.Cells(ColName).Value)).ToList()
    End Function

    ''' <summary>While locked (during a download) the selection cannot be changed.</summary>
    <System.ComponentModel.Browsable(False), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property SelectionLocked As Boolean
        Get
            Return locked
        End Get
        Set(value As Boolean)
            locked = value
            grid.Columns(ColSelected).ReadOnly = value
            btnSelectAll.Enabled = Not value
            btnSelectNone.Enabled = Not value
            btnInvert.Enabled = Not value
        End Set
    End Property

    ''' <summary>Thread-safe; does not wait for the UI.</summary>
    Public Sub ReportProgress(folderName As String, downloaded As Integer, total As Integer)
        Dim percent = If(total = 0, 100, downloaded * 100 \ total)
        RunOnUi(Sub() SetStatus(folderName, FolderState.Downloading, $"Downloading {downloaded}/{total} ({percent}%)"), wait:=False)
    End Sub

    ''' <summary>Thread-safe; waits until the UI shows the result.</summary>
    Public Sub ReportState(folderName As String, state As FolderState, text As String)
        RunOnUi(Sub() SetStatus(folderName, state, text), wait:=True)
    End Sub

    Private Sub SetStatus(folderName As String, state As FolderState, text As String)
        Dim row = FindRow(folderName)
        If row Is Nothing Then Exit Sub
        Dim cell = row.Cells(ColProgress)
        cell.Value = text
        cell.Style.BackColor = StateColor(state)
    End Sub

    Private Shared Function StateColor(state As FolderState) As Color
        Select Case state
            Case FolderState.Downloading : Return Color.FromArgb(255, 201, 102)
            Case FolderState.Completed : Return Color.FromArgb(133, 224, 133)
            Case FolderState.CompletedWithErrors, FolderState.Aborted, FolderState.Failed : Return Color.FromArgb(255, 102, 102)
            Case Else : Return Color.Empty
        End Select
    End Function

    Private Sub RunOnUi(action As Action, wait As Boolean)
        If IsDisposed OrElse Not IsHandleCreated Then Exit Sub
        If Not InvokeRequired Then
            action()
        ElseIf wait Then
            Invoke(action)
        Else
            BeginInvoke(action)
        End If
    End Sub

    Private Function FindRow(folderName As String) As DataGridViewRow
        For Each row As DataGridViewRow In grid.Rows
            If String.Equals(CStr(row.Cells(ColName).Value), folderName, StringComparison.Ordinal) Then Return row
        Next
        Return Nothing
    End Function

    Private Shared Function IsSelected(row As DataGridViewRow) As Boolean
        Dim value = row.Cells(ColSelected).Value
        Return value IsNot Nothing AndAlso CBool(value)
    End Function

    Private Sub SetAllSelected(newValue As Func(Of Boolean, Boolean))
        If locked Then Exit Sub
        grid.EndEdit()
        For Each row As DataGridViewRow In grid.Rows
            row.Cells(ColSelected).Value = newValue(IsSelected(row))
        Next
        UpdateSummary()
    End Sub

    Private Sub Grid_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Space OrElse locked OrElse grid.SelectedRows.Count < 2 Then Exit Sub
        Dim rows = grid.SelectedRows.Cast(Of DataGridViewRow)().ToList()
        Dim target = Not rows.All(AddressOf IsSelected)
        grid.EndEdit()
        For Each row In rows
            row.Cells(ColSelected).Value = target
        Next
        e.Handled = True
        e.SuppressKeyPress = True
        UpdateSummary()
    End Sub

    Private Sub UpdateSummary()
        Dim rows = grid.Rows.Cast(Of DataGridViewRow)().ToList()
        Dim selected = rows.Where(AddressOf IsSelected).ToList()
        Dim messages = selected.Sum(Function(row) CInt(row.Cells(ColMatching).Value))
        lblSummary.Text = $"{selected.Count} of {rows.Count} folders selected, {messages} messages"
    End Sub

End Class
