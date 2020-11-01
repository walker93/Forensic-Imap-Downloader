Imports System.IO
Imports System.Security.Cryptography
Module Hash

    Private hash As SHA1 = SHA1.Create()

    Public Function GetHashSha1(ByVal filename As String) As String
        Using stream As FileStream = File.OpenRead(filename)
            Return BytesToString(hash.ComputeHash(stream)).ToUpper
        End Using
    End Function


    Public Function BytesToString(ByVal bytes As Byte()) As String
        Dim result As String = ""

        For Each b As Byte In bytes
            result += b.ToString("x2")
        Next

        Return result
    End Function
End Module
