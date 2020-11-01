Imports S22.Imap

Public Class search
    Public Selected_criteria As SearchCondition
    Dim search_dict As New Dictionary(Of String, String)
    Public saved_query As String
    Private Sub btn_ok_Click(sender As Object, e As EventArgs) Handles btn_ok.Click
        Dim query = search_builder1.query_tb.Text
        saved_query = query
        Selected_criteria = createSearchCondition(query)
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Dim text_filter As String = "STRING"
    Dim bool_filter As String = "BOOL"
    Dim date_filter As String = "DATE"

    Function createSearchCondition(query As String) As SearchCondition

        If query.StartsWith("AND(") Then
            query = query.Remove(0, 4)
            query = query.Remove(query.LastIndexOf(")"), 1)
            Dim operands = query.Split({(";"c)}, 2)
            Return createSearchCondition(operands(0)).And(createSearchCondition(operands(1)))
        ElseIf query.StartsWith("OR(") Then
            query = query.Remove(0, 3)
            query = query.Remove(query.LastIndexOf(")"), 1)
            Dim operands = query.Split({(";"c)}, 2)
            Return createSearchCondition(operands(0)).Or(createSearchCondition(operands(1)))
        ElseIf query.StartsWith("NOT(") Then
            query = query.Remove(0, 4)
            query = query.Remove(query.LastIndexOf(")"), 1)
            Return (createSearchCondition(query)).Not(SearchCondition.All)
        Else
            For Each param In search_dict
                If query.StartsWith(param.Key) Then
                    Dim key_value = query.Split("=")
                    Select Case param.Key
                        Case "Sent After"
                            Return SearchCondition.SentSince(Date.Parse(key_value(1))) ', "yyyy-MM-dd HH:mm:ss", Nothing))
                        Case "Sent Before"
                            Return SearchCondition.SentBefore(Date.Parse(key_value(1))) ', "yyyy-MM-dd HH:mm:ss", Nothing))
                        Case "Answered"
                            Return If(key_value(1).ToUpper = "TRUE", SearchCondition.Answered, SearchCondition.Unanswered)
                        Case "Deleted"
                            Return If(key_value(1).ToUpper = "TRUE", SearchCondition.Deleted, SearchCondition.Undeleted)
                        Case "Draft"
                            Return If(key_value(1).ToUpper = "TRUE", SearchCondition.Draft, SearchCondition.Undraft)
                        Case "Seen"
                            Return If(key_value(1).ToUpper = "TRUE", SearchCondition.Seen, SearchCondition.Unseen)
                        Case "To"
                            Return SearchCondition.To(key_value(1))
                        Case "From"
                            Return SearchCondition.From(key_value(1))
                        Case "Subject"
                            Return SearchCondition.Subject(key_value(1))
                        Case "Body"
                            Return SearchCondition.Body(key_value(1))
                        Case "CC"
                            Return SearchCondition.Cc(key_value(1))
                        Case "BCC"
                            Return SearchCondition.BCC(key_value(1))
                    End Select
                End If
            Next
        End If
        Return SearchCondition.All
    End Function


    Sub populatedict()
        search_dict.Add("Sent After", date_filter)
        search_dict.Add("Sent Before", date_filter)
        search_dict.Add("Answered", bool_filter)
        search_dict.Add("Deleted", bool_filter)
        search_dict.Add("Draft", bool_filter)
        search_dict.Add("Seen", bool_filter)
        search_dict.Add("To", text_filter)
        search_dict.Add("From", text_filter)
        search_dict.Add("Subject", text_filter)
        search_dict.Add("Body", text_filter)
        search_dict.Add("CC", text_filter)
        search_dict.Add("BCC", text_filter)
    End Sub
    Dim counter = 0

    Private Sub search_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        populatedict()

    End Sub

    Public Sub New(q As String)

        ' La chiamata è richiesta dalla finestra di progettazione.
        InitializeComponent()

        ' Aggiungere le eventuali istruzioni di inizializzazione dopo la chiamata a InitializeComponent().
        search_builder1.query_tb.Text = q
    End Sub
End Class