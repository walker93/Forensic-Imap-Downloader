Imports System.Globalization
Imports MailKit.Search

''' <summary>Field kind used by the search builder UI to pick a sensible input widget.</summary>
Public Enum SearchFieldKind
    Text
    Bool
    [Date]
End Enum

''' <summary>
''' Single source of truth for the search filter fields (previously duplicated between
''' search.vb and search-builder.vb) and a parser that turns the AND(...)/OR(...)/NOT(...)
''' mini query language built by the UI into a MailKit <see cref="SearchQuery"/>.
''' </summary>
Public NotInheritable Class SearchQueryParser

    Public Const DateFormat As String = "yyyy-MM-dd"

    Public Shared ReadOnly FilterFields As IReadOnlyDictionary(Of String, SearchFieldKind) = New Dictionary(Of String, SearchFieldKind) From {
        {"Sent After", SearchFieldKind.Date},
        {"Sent Before", SearchFieldKind.Date},
        {"Answered", SearchFieldKind.Bool},
        {"Deleted", SearchFieldKind.Bool},
        {"Draft", SearchFieldKind.Bool},
        {"Seen", SearchFieldKind.Bool},
        {"To", SearchFieldKind.Text},
        {"From", SearchFieldKind.Text},
        {"Subject", SearchFieldKind.Text},
        {"Body", SearchFieldKind.Text},
        {"CC", SearchFieldKind.Text},
        {"BCC", SearchFieldKind.Text}
    }

    Private Sub New()
    End Sub

    ''' <summary>Parses a query string into a MailKit search query. Returns SearchQuery.All for an empty query.</summary>
    Public Shared Function Parse(query As String) As SearchQuery
        If String.IsNullOrWhiteSpace(query) Then Return SearchQuery.All

        query = query.Trim()

        If query.StartsWith("AND(", StringComparison.Ordinal) Then
            Dim operands = SplitOperands(query, 4)
            Return Parse(operands(0)).And(Parse(operands(1)))
        ElseIf query.StartsWith("OR(", StringComparison.Ordinal) Then
            Dim operands = SplitOperands(query, 3)
            Return Parse(operands(0)).Or(Parse(operands(1)))
        ElseIf query.StartsWith("NOT(", StringComparison.Ordinal) Then
            Dim inner = query.Substring(4, query.Length - 5)
            Return SearchQuery.Not(Parse(inner))
        End If

        Dim separatorIndex = query.IndexOf("="c)
        If separatorIndex < 0 Then Return SearchQuery.All

        Dim fieldName = query.Substring(0, separatorIndex)
        Dim fieldValue = query.Substring(separatorIndex + 1)

        If Not FilterFields.ContainsKey(fieldName) Then Return SearchQuery.All

        Return BuildFieldQuery(fieldName, fieldValue)
    End Function

    ''' <summary>Splits "op1;op2" respecting nested parentheses, unlike a plain first-semicolon split.</summary>
    Private Shared Function SplitOperands(query As String, prefixLength As Integer) As String()
        Dim inner = query.Substring(prefixLength, query.Length - prefixLength - 1)
        Dim separatorIndex = FindTopLevelSemicolon(inner)
        If separatorIndex < 0 Then
            Throw New FormatException($"Malformed query: missing top-level ';' separating operands in '{query}'.")
        End If
        Return {inner.Substring(0, separatorIndex), inner.Substring(separatorIndex + 1)}
    End Function

    Private Shared Function FindTopLevelSemicolon(text As String) As Integer
        Dim depth As Integer = 0
        For i = 0 To text.Length - 1
            Select Case text(i)
                Case "("c
                    depth += 1
                Case ")"c
                    depth -= 1
                Case ";"c
                    If depth = 0 Then Return i
            End Select
        Next
        Return -1
    End Function

    Private Shared Function BuildFieldQuery(fieldName As String, fieldValue As String) As SearchQuery
        Select Case fieldName
            Case "Sent After"
                Return SearchQuery.SentSince(ParseDate(fieldValue))
            Case "Sent Before"
                Return SearchQuery.SentBefore(ParseDate(fieldValue))
            Case "Answered"
                Return If(ParseBool(fieldValue), SearchQuery.Answered, SearchQuery.NotAnswered)
            Case "Deleted"
                Return If(ParseBool(fieldValue), SearchQuery.Deleted, SearchQuery.NotDeleted)
            Case "Draft"
                Return If(ParseBool(fieldValue), SearchQuery.Draft, SearchQuery.NotDraft)
            Case "Seen"
                Return If(ParseBool(fieldValue), SearchQuery.Seen, SearchQuery.NotSeen)
            Case "To"
                Return SearchQuery.ToContains(fieldValue)
            Case "From"
                Return SearchQuery.FromContains(fieldValue)
            Case "Subject"
                Return SearchQuery.SubjectContains(fieldValue)
            Case "Body"
                Return SearchQuery.BodyContains(fieldValue)
            Case "CC"
                Return SearchQuery.CcContains(fieldValue)
            Case "BCC"
                Return SearchQuery.BccContains(fieldValue)
            Case Else
                Return SearchQuery.All
        End Select
    End Function

    Public Shared Function ParseDate(value As String) As Date
        Return Date.ParseExact(value.Trim(), DateFormat, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function ParseBool(value As String) As Boolean
        Return String.Equals(value.Trim(), "TRUE", StringComparison.OrdinalIgnoreCase)
    End Function

End Class
