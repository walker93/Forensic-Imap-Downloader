Imports MailKit.Search
Imports Xunit

Public Class SearchQueryParserTests

    <Fact>
    Public Sub Parse_EmptyQuery_ReturnsAll()
        Assert.Equal(SearchTerm.All, SearchQueryParser.Parse("").Term)
    End Sub

    <Fact>
    Public Sub Parse_UnknownField_ReturnsAll()
        Assert.Equal(SearchTerm.All, SearchQueryParser.Parse("Nonsense=1").Term)
    End Sub

    <Theory>
    <InlineData("Subject", SearchTerm.SubjectContains)>
    <InlineData("Body", SearchTerm.BodyContains)>
    <InlineData("To", SearchTerm.ToContains)>
    <InlineData("From", SearchTerm.FromContains)>
    <InlineData("CC", SearchTerm.CcContains)>
    <InlineData("BCC", SearchTerm.BccContains)>
    Public Sub Parse_TextField_ReturnsMatchingTextQuery(field As String, expectedTerm As SearchTerm)
        Dim result = SearchQueryParser.Parse($"{field}=hello world")
        Dim textQuery = Assert.IsType(Of TextSearchQuery)(result)
        Assert.Equal(expectedTerm, textQuery.Term)
        Assert.Equal("hello world", textQuery.Text)
    End Sub

    <Fact>
    Public Sub Parse_SentAfter_ParsesDateAndUsesSentSince()
        Dim result = SearchQueryParser.Parse("Sent After=2024-01-15")
        Dim dateQuery = Assert.IsType(Of DateSearchQuery)(result)
        Assert.Equal(SearchTerm.SentSince, dateQuery.Term)
        Assert.Equal(New Date(2024, 1, 15), dateQuery.Date)
    End Sub

    <Fact>
    Public Sub Parse_SentBefore_ParsesDateAndUsesSentBefore()
        Dim result = SearchQueryParser.Parse("Sent Before=2024-01-15")
        Dim dateQuery = Assert.IsType(Of DateSearchQuery)(result)
        Assert.Equal(SearchTerm.SentBefore, dateQuery.Term)
    End Sub

    <Theory>
    <InlineData("TRUE", SearchTerm.Answered)>
    <InlineData("FALSE", SearchTerm.NotAnswered)>
    <InlineData("true", SearchTerm.Answered)>
    Public Sub Parse_AnsweredBoolField_IsCaseInsensitive(value As String, expectedTerm As SearchTerm)
        Dim result = SearchQueryParser.Parse($"Answered={value}")
        Assert.Equal(expectedTerm, result.Term)
    End Sub

    <Fact>
    Public Sub Parse_And_CombinesTwoOperands()
        Dim result = SearchQueryParser.Parse("AND(Subject=a;Body=b)")
        Dim binary = Assert.IsType(Of BinarySearchQuery)(result)
        Assert.Equal(SearchTerm.And, binary.Term)
        Assert.Equal("a", DirectCast(binary.Left, TextSearchQuery).Text)
        Assert.Equal("b", DirectCast(binary.Right, TextSearchQuery).Text)
    End Sub

    <Fact>
    Public Sub Parse_Or_CombinesTwoOperands()
        Dim result = SearchQueryParser.Parse("OR(Subject=a;Body=b)")
        Assert.Equal(SearchTerm.Or, result.Term)
    End Sub

    <Fact>
    Public Sub Parse_Not_NegatesOperand()
        Dim result = SearchQueryParser.Parse("NOT(Seen=TRUE)")
        Dim unary = Assert.IsType(Of UnarySearchQuery)(result)
        Assert.Equal(SearchTerm.Not, unary.Term)
        Assert.Equal(SearchTerm.Seen, unary.Operand.Term)
    End Sub

    <Fact>
    Public Sub Parse_NestedAnd_RespectsTopLevelSemicolonNotNestedOnes()
        ' Regression test: the original hand-rolled parser split on the FIRST semicolon found
        ' anywhere in the string, which silently broke nested boolean expressions like this one.
        Dim result = SearchQueryParser.Parse("AND(AND(Subject=a;Body=b);From=c)")
        Dim outer = Assert.IsType(Of BinarySearchQuery)(result)
        Dim inner = Assert.IsType(Of BinarySearchQuery)(outer.Left)
        Assert.Equal("a", DirectCast(inner.Left, TextSearchQuery).Text)
        Assert.Equal("b", DirectCast(inner.Right, TextSearchQuery).Text)
        Assert.Equal("c", DirectCast(outer.Right, TextSearchQuery).Text)
    End Sub

    <Fact>
    Public Sub Parse_ValueContainingEqualsSign_KeepsFullValue()
        Dim result = SearchQueryParser.Parse("Subject=a=b")
        Dim textQuery = Assert.IsType(Of TextSearchQuery)(result)
        Assert.Equal("a=b", textQuery.Text)
    End Sub

End Class
