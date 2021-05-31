Imports System.IO
Imports System.Security.Cryptography
Module Hash
    Public Class HashCalculator
        Private sha1 As SHA1
        Private md5 As MD5
        Sub New()
            sha1 = SHA1.Create()
            md5 = MD5.Create()
        End Sub
        Public Function GetHashSha1(ByVal filename As String) As String
            Dim streamSha1 As FileStream = File.OpenRead(filename)
            Dim hashSha1 = BytesToString(sha1.ComputeHash(streamSha1)).ToUpper
            streamSha1.Close() : streamSha1.Dispose()
            Return hashSha1
        End Function

        Public Function GetHashmd5(ByVal filename As String) As String
            Dim streamMD5 As FileStream = File.OpenRead(filename)
            Dim hashMD5 = BytesToString(md5.ComputeHash(streamMD5)).ToUpper
            streamMD5.Close() : streamMD5.Dispose()
            Return hashMD5
        End Function

        Public Function BytesToString(ByVal bytes As Byte()) As String
            Dim result As String = ""

            For Each b As Byte In bytes
                result += b.ToString("x2")
            Next

            Return result
        End Function
    End Class
End Module
