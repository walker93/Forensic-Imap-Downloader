Imports System.IO
Imports System.Threading

Friend Module Program

    <STAThread()>
    Friend Sub Main(args As String())
        ' Unhandled exceptions used to end the process with no trace when they happened before
        ' "Connect" points the log at the evidence folder: record them in a crash log instead.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException

        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New Form1())
    End Sub

    ''' <summary>Kept out of the evidence folder on purpose: the program's own failures are not evidence.</summary>
    Private ReadOnly Property CrashLogPath As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ForensicIMAPDownloader", "crash.log")
        End Get
    End Property

    Private Sub OnThreadException(sender As Object, e As ThreadExceptionEventArgs)
        WriteCrashLog(e.Exception)
        MsgBox("An unexpected error occurred:" & vbCrLf & e.Exception.Message & vbCrLf & vbCrLf &
               "Details were saved to " & CrashLogPath, MsgBoxStyle.Critical)
    End Sub

    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        WriteCrashLog(TryCast(e.ExceptionObject, Exception))
    End Sub

    Private Sub WriteCrashLog(ex As Exception)
        Try
            NLog.LogManager.GetCurrentClassLogger().Fatal(ex, "Unhandled exception")
            Directory.CreateDirectory(Path.GetDirectoryName(CrashLogPath))
            File.AppendAllText(CrashLogPath, $"{DateTimeOffset.Now:u} {ex}{Environment.NewLine}{Environment.NewLine}")
        Catch
            ' Never let crash logging itself throw.
        End Try
    End Sub

End Module
