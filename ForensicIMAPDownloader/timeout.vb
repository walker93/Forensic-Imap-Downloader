Imports System.Threading

Module timeout

    Public NotInheritable Class WaitFor(Of TResult)
        ReadOnly _timeout As TimeSpan

        Public Sub New(ByVal timeout As TimeSpan)
            _timeout = timeout
        End Sub

        Public Function Run(ByVal [function] As Func(Of TResult)) As TResult
            If [function] Is Nothing Then Throw New ArgumentNullException("function")
            Dim sync = New Object()
            Dim isCompleted = False
            Dim watcher As WaitCallback = Function(obj)
                                              Dim watchedThread = TryCast(obj, Thread)

                                              SyncLock sync

                                                  If Not isCompleted Then
                                                      Monitor.Wait(sync, _timeout)
                                                  End If
                                              End SyncLock

                                              If Not isCompleted Then
                                                  watchedThread.Abort()
                                              End If
                                          End Function

            Try
                ThreadPool.QueueUserWorkItem(watcher, Thread.CurrentThread)
                Return [function]()
            Catch __unusedThreadAbortException1__ As ThreadAbortException
                Thread.ResetAbort()
                Throw New TimeoutException(String.Format("The operation has timed out after {0}.", _timeout))
            Finally

                SyncLock sync
                    isCompleted = True
                    Monitor.Pulse(sync)
                End SyncLock
            End Try
        End Function

        Public Shared Function Run(ByVal timeout As TimeSpan, ByVal [function] As Func(Of TResult)) As TResult
            Return New WaitFor(Of TResult)(timeout).Run([function])
        End Function
    End Class

End Module
