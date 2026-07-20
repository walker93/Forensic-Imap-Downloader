Imports MailKit.Search

Public Class search
    Public Selected_criteria As SearchQuery
    Public saved_query As String

    Private Sub btn_ok_Click(sender As Object, e As EventArgs) Handles btn_ok.Click
        Dim query = search_builder1.query_tb.Text
        Try
            Selected_criteria = SearchQueryParser.Parse(query)
        Catch ex As Exception
            MsgBox($"The filter query is not valid: {ex.Message}")
            Exit Sub
        End Try
        saved_query = query
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Public Sub New(q As String)

        ' La chiamata è richiesta dalla finestra di progettazione.
        InitializeComponent()

        ' Aggiungere le eventuali istruzioni di inizializzazione dopo la chiamata a InitializeComponent().
        search_builder1.query_tb.Text = q
    End Sub
End Class
