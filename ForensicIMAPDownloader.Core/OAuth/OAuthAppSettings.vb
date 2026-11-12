Imports System.IO
Imports System.Text.Json

''' <summary>
''' The examiner's OAuth application registrations, remembered between runs.
'''
''' Only public identifiers are stored: the Microsoft client ID/tenant and the *path* of the
''' Google client file. No token and no secret is ever written here.
''' </summary>
Public Class OAuthAppSettings

    ''' <summary>
    ''' Client ID shipped with the tool for personal Microsoft accounts. Empty until the project's
    ''' own Entra app registration exists; the operator then has to supply one in the UI.
    ''' </summary>
    Public Const DefaultMicrosoftClientId As String = ""

    Public Property MicrosoftClientId As String = ""
    Public Property MicrosoftTenant As String = MicrosoftTokenProvider.ConsumersTenant
    Public Property MicrosoftUseDeviceCode As Boolean
    Public Property GoogleClientSecretsPath As String = ""

    ''' <summary>The operator's client ID when set, otherwise the one shipped with the tool.</summary>
    Public Function EffectiveMicrosoftClientId() As String
        Return If(String.IsNullOrWhiteSpace(MicrosoftClientId), DefaultMicrosoftClientId, MicrosoftClientId.Trim())
    End Function

    Public Shared Function DefaultPath() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ForensicIMAPDownloader", "oauth.json")
    End Function

    ''' <summary>Returns defaults when the file is missing or unreadable: a broken settings file must not block an acquisition.</summary>
    Public Shared Function Load(filePath As String) As OAuthAppSettings
        Try
            If File.Exists(filePath) Then
                Dim loaded = JsonSerializer.Deserialize(Of OAuthAppSettings)(File.ReadAllText(filePath))
                If loaded IsNot Nothing Then Return loaded
            End If
        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return New OAuthAppSettings()
    End Function

    Public Sub Save(filePath As String)
        Directory.CreateDirectory(Path.GetDirectoryName(filePath))
        File.WriteAllText(filePath, JsonSerializer.Serialize(Me, New JsonSerializerOptions With {.WriteIndented = True}))
    End Sub

End Class
