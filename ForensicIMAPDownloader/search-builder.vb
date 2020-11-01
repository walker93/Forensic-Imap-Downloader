Imports System.Text.RegularExpressions
Imports ForensicIMAPDownloader
Imports ScintillaNET

Public Class search_builder
    Dim text_filter As String = "STRING"
    Dim bool_filter As String = "BOOL"
    Dim date_filter As String = "DATE"
    Dim search_dict As New Dictionary(Of String, String)
    Private Sub search_builder_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        populatedict()
        param_cb.Items.AddRange(search_dict.Keys.ToArray)
        AutocompleteMenu1.TargetControlWrapper = New ScintillaWrapper(query_tb)
        Dim autocompletelist As New List(Of AutocompleteMenuNS.AutocompleteItem)
        For Each el In search_dict
            autocompletelist.Add(New AutocompleteMenuNS.AutocompleteItem(el.Key))
        Next
        AutocompleteMenu1.SetAutocompleteItems(autocompletelist)

        query_tb.Styles(Style.BraceLight).BackColor = Color.LightGray
        query_tb.Styles(Style.BraceLight).ForeColor = Color.BlueViolet
        query_tb.Styles(Style.BraceBad).ForeColor = Color.Red
    End Sub

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

    Private Sub AND_btn_Click(sender As Object, e As EventArgs) Handles AND_btn.Click
        If query_tb.SelectedText = "" Or query_tb.SelectedText = "___" Then
            query_tb.ReplaceSelection("AND(___;___)")
            query_tb.SetSelection(query_tb.CurrentPosition - 5, query_tb.CurrentPosition - 8)
        Else
            query_tb.ReplaceSelection($"AND({query_tb.SelectedText};___)")
        End If

    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        If query_tb.SelectedText = "" Or query_tb.SelectedText = "___" Then
            query_tb.ReplaceSelection("OR(___;___)")
            query_tb.SetSelection(query_tb.CurrentPosition - 5, query_tb.CurrentPosition - 8)
        Else
            query_tb.ReplaceSelection($"OR({query_tb.SelectedText};___)")
        End If
    End Sub

    Private Sub NOT_btn_Click(sender As Object, e As EventArgs) Handles NOT_btn.Click
        If query_tb.SelectedText = "" Or query_tb.SelectedText = "___" Then
            query_tb.ReplaceSelection("NOT(___)")
            query_tb.SetSelection(query_tb.CurrentPosition - 1, query_tb.CurrentPosition - 4)
        Else
            query_tb.ReplaceSelection($"NOT({query_tb.SelectedText})")
        End If

    End Sub

    Private Sub param_cb_SelectedIndexChanged(sender As Object, e As EventArgs) Handles param_cb.SelectedIndexChanged
        Dim value As String = search_dict(param_cb.SelectedItem)
        Dim default_text As String = ""
        Select Case value
            Case "DATE"
                default_text = "yyyy-MM-dd"
            Case "BOOL"
                default_text = "TRUE/FALSE"
        End Select
        Dim input = InputBox(param_cb.SelectedItem & "=", "Input filter value", default_text)
        If input = default_text Then Exit Sub
        query_tb.ReplaceSelection(param_cb.SelectedItem & "=" & input)

    End Sub

    Private lastCaretPos As Integer = 0

    Private Sub query_tb_UpdateUI(ByVal sender As Object, ByVal e As UpdateUIEventArgs) Handles query_tb.UpdateUI
        Dim caretPos = query_tb.CurrentPosition

        If lastCaretPos <> caretPos Then
            lastCaretPos = caretPos
            Dim bracePos1 = -1
            Dim bracePos2 = -1

            If caretPos > 0 AndAlso IsBrace(query_tb.GetCharAt(caretPos - 1)) Then
                bracePos1 = (caretPos - 1)
            ElseIf IsBrace(query_tb.GetCharAt(caretPos)) Then
                bracePos1 = caretPos
            End If

            If bracePos1 >= 0 Then
                bracePos2 = query_tb.BraceMatch(bracePos1)

                If bracePos2 = query_tb.InvalidPosition Then
                    query_tb.BraceBadLight(bracePos1)
                Else
                    query_tb.BraceHighlight(bracePos1, bracePos2)
                End If
            Else
                query_tb.BraceHighlight(query_tb.InvalidPosition, query_tb.InvalidPosition)
            End If
        End If
        If e.Change = UpdateChange.Selection Then
            sel_lbl.Text = $"Sel: {query_tb.SelectionStart}; {query_tb.SelectionEnd}"
        End If
    End Sub
    Private Shared Function IsBrace(ByVal c As Integer) As Boolean
        Select Case c
            Case AscW("("c), AscW(")"c), AscW("["c), AscW("]"c), AscW("{"c), AscW("}"c)
                Return True
        End Select

        Return False
    End Function


End Class
