Imports System.IO
Imports Xunit

Public Class HashCalculatorTests

    <Fact>
    Public Sub GetHashSha1_KnownContent_MatchesExpectedHash()
        Dim filePath = Path.GetTempFileName()
        Try
            File.WriteAllText(filePath, "hello world", Text.Encoding.ASCII)
            Using calculator As New HashCalculator()
                Assert.Equal("2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED", calculator.GetHashSha1(filePath))
            End Using
        Finally
            File.Delete(filePath)
        End Try
    End Sub

    <Fact>
    Public Sub GetHashmd5_KnownContent_MatchesExpectedHash()
        Dim filePath = Path.GetTempFileName()
        Try
            File.WriteAllText(filePath, "hello world", Text.Encoding.ASCII)
            Using calculator As New HashCalculator()
                Assert.Equal("5EB63BBBE01EEED093CB22BB8F5ACDC3", calculator.GetHashmd5(filePath))
            End Using
        Finally
            File.Delete(filePath)
        End Try
    End Sub

    <Fact>
    Public Sub GetHashmd5_SameContentTwice_IsDeterministic()
        Dim filePath = Path.GetTempFileName()
        Try
            File.WriteAllText(filePath, "forensic evidence", Text.Encoding.UTF8)
            Using calculator As New HashCalculator()
                Assert.Equal(calculator.GetHashmd5(filePath), calculator.GetHashmd5(filePath))
            End Using
        Finally
            File.Delete(filePath)
        End Try
    End Sub

End Class
