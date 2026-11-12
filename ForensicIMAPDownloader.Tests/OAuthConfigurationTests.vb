Imports System.IO
Imports System.Text
Imports Xunit

Public Class OAuthConfigurationTests

    Private Shared Function TempFilePath() As String
        Return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "oauth.json")
    End Function

    <Fact>
    Public Sub Settings_SaveThenLoad_RoundTripsEveryField()
        Dim filePath = TempFilePath()
        Try
            Dim saved As New OAuthAppSettings With {
                .MicrosoftClientId = "11111111-2222-3333-4444-555555555555",
                .MicrosoftTenant = "common",
                .MicrosoftUseDeviceCode = True,
                .GoogleClientSecretsPath = "C:\lab\client_secret.json"
            }
            saved.Save(filePath)

            Dim loaded = OAuthAppSettings.Load(filePath)
            Assert.Equal(saved.MicrosoftClientId, loaded.MicrosoftClientId)
            Assert.Equal("common", loaded.MicrosoftTenant)
            Assert.True(loaded.MicrosoftUseDeviceCode)
            Assert.Equal(saved.GoogleClientSecretsPath, loaded.GoogleClientSecretsPath)
        Finally
            Directory.Delete(Path.GetDirectoryName(filePath), True)
        End Try
    End Sub

    <Fact>
    Public Sub Settings_LoadMissingFile_ReturnsDefaultsRestrictedToPersonalAccounts()
        Dim loaded = OAuthAppSettings.Load(TempFilePath())

        Assert.Equal("", loaded.MicrosoftClientId)
        Assert.Equal("consumers", loaded.MicrosoftTenant)
        Assert.False(loaded.MicrosoftUseDeviceCode)
    End Sub

    <Fact>
    Public Sub Settings_LoadCorruptFile_ReturnsDefaultsInsteadOfThrowing()
        Dim filePath = TempFilePath()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllText(filePath, "{ not json")

            Assert.Equal("consumers", OAuthAppSettings.Load(filePath).MicrosoftTenant)
        Finally
            Directory.Delete(Path.GetDirectoryName(filePath), True)
        End Try
    End Sub

    <Fact>
    Public Sub Settings_EffectiveMicrosoftClientId_PrefersTheOperatorsOwnId()
        Dim settings As New OAuthAppSettings With {.MicrosoftClientId = "  my-own-id  "}
        Assert.Equal("my-own-id", settings.EffectiveMicrosoftClientId())

        settings.MicrosoftClientId = ""
        Assert.Equal(OAuthAppSettings.DefaultMicrosoftClientId, settings.EffectiveMicrosoftClientId())
    End Sub

    <Fact>
    Public Sub MicrosoftProvider_WithoutClientId_IsRejected()
        Assert.Throws(Of ArgumentException)(Function() New MicrosoftTokenProvider("  ", "consumers", False))
    End Sub

    <Fact>
    Public Sub MicrosoftProvider_DeviceCodeWithoutCallback_IsRejected()
        Assert.Throws(Of ArgumentNullException)(Function() New MicrosoftTokenProvider("client-id", "consumers", True))
    End Sub

    <Fact>
    Public Sub MicrosoftProvider_BlankTenant_DefaultsToPersonalAccounts()
        Dim provider As New MicrosoftTokenProvider("client-id", "", False)

        Assert.Equal("consumers", provider.Tenant)
        Assert.Equal("https://account.live.com/consent/Manage", provider.RevocationUrl)
    End Sub

    <Fact>
    Public Async Function MicrosoftProvider_TokenBeforeSignIn_Throws() As Threading.Tasks.Task
        Dim provider As New MicrosoftTokenProvider("client-id", "consumers", False)

        Await Assert.ThrowsAsync(Of InvalidOperationException)(Function() provider.GetAccessTokenAsync(Threading.CancellationToken.None))
    End Function

    <Fact>
    Public Sub GoogleProvider_FromDesktopClientFile_ReadsTheClientId()
        Dim json = "{""installed"":{""client_id"":""123-abc.apps.googleusercontent.com"",""client_secret"":""s3cret"",""redirect_uris"":[""http://localhost""]}}"
        Using stream As New MemoryStream(Encoding.UTF8.GetBytes(json))
            Dim provider = GoogleTokenProvider.FromClientSecretsStream(stream)

            Assert.Equal("123-abc.apps.googleusercontent.com", provider.ClientId)
        End Using
    End Sub

    <Fact>
    Public Sub GoogleProvider_FromUnrelatedJson_IsRejected()
        Using stream As New MemoryStream(Encoding.UTF8.GetBytes("{""foo"":1}"))
            Assert.ThrowsAny(Of Exception)(Function() GoogleTokenProvider.FromClientSecretsStream(stream))
        End Using
    End Sub

End Class
