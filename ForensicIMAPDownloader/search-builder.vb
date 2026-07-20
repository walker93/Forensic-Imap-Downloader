Imports ScintillaNET

Public Class search_builder

    Private Sub search_builder_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        param_cb.Items.AddRange(SearchQueryParser.FilterFields.Keys.ToArray())

        ' Native Scintilla autocomplete list (no external AutoCompleteMenu-ScintillaNET dependency,
        ' which pulls in Microsoft.Windows.Compatibility and its known-vulnerable transitive packages).
        query_tb.AutoCSeparator = ControlChars.Lf
        query_tb.AutoCIgnoreCase = True

        query_tb.Styles(Style.BraceLight).BackColor = Color.LightGray
        query_tb.Styles(Style.BraceLight).ForeColor = Color.BlueViolet
        query_tb.Styles(Style.BraceBad).ForeColor = Color.Red
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
        Dim fieldName = CStr(param_cb.SelectedItem)
        Dim default_text As String = ""
        Select Case SearchQueryParser.FilterFields(fieldName)
            Case SearchFieldKind.Date
                default_text = SearchQueryParser.DateFormat
            Case SearchFieldKind.Bool
                default_text = "TRUE/FALSE"
        End Select
        Dim input = InputBox(fieldName & "=", "Input filter value", default_text)
        If input = default_text Then Exit Sub
        query_tb.ReplaceSelection(fieldName & "=" & input)

    End Sub

    ''' <summary>Shows a native Scintilla autocomplete list of matching field names as the user types.</summary>
    Private Sub query_tb_CharAdded(sender As Object, e As CharAddedEventArgs) Handles query_tb.CharAdded
        Dim currentPos = query_tb.CurrentPosition
        Dim lineStart = query_tb.Lines(query_tb.LineFromPosition(currentPos)).Position
        Dim textSoFar = query_tb.GetTextRange(lineStart, currentPos - lineStart)
        Dim tokenStart = textSoFar.LastIndexOfAny({"("c, ";"c, ")"c}) + 1
        Dim currentWord = textSoFar.Substring(tokenStart).TrimStart()

        If currentWord.Length = 0 Then
            query_tb.AutoCCancel()
            Exit Sub
        End If

        Dim matches = SearchQueryParser.FilterFields.Keys.
            Where(Function(fieldName) fieldName.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase)).
            OrderBy(Function(fieldName) fieldName, StringComparer.OrdinalIgnoreCase).
            ToArray()

        If matches.Length = 0 Then
            query_tb.AutoCCancel()
            Exit Sub
        End If

        query_tb.AutoCShow(currentWord.Length, String.Join(vbLf, matches))
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

                If bracePos2 = Scintilla.InvalidPosition Then
                    query_tb.BraceBadLight(bracePos1)
                Else
                    query_tb.BraceHighlight(bracePos1, bracePos2)
                End If
            Else
                query_tb.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition)
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
