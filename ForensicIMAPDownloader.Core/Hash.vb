Imports System.IO
Imports System.Security.Cryptography

Public Class HashCalculator
    Implements IDisposable

    Private ReadOnly sha1 As SHA1
    Private ReadOnly md5 As MD5
    Private disposed As Boolean

    Sub New()
        sha1 = SHA1.Create()
        md5 = MD5.Create()
    End Sub

    Public Function GetHashSha1(ByVal filename As String) As String
        Using stream As FileStream = File.OpenRead(filename)
            Return BytesToString(sha1.ComputeHash(stream)).ToUpperInvariant()
        End Using
    End Function

    Public Function GetHashmd5(ByVal filename As String) As String
        Using stream As FileStream = File.OpenRead(filename)
            Return BytesToString(md5.ComputeHash(stream)).ToUpperInvariant()
        End Using
    End Function

    Public Function BytesToString(ByVal bytes As Byte()) As String
        Dim result As New Text.StringBuilder(bytes.Length * 2)
        For Each b As Byte In bytes
            result.Append(b.ToString("x2"))
        Next
        Return result.ToString()
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If disposed Then Exit Sub
        If disposing Then
            sha1.Dispose()
            md5.Dispose()
        End If
        disposed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
