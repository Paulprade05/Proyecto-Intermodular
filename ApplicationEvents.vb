Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
    ' eventos del ciclo de vida de la app
    ' Startup: cuando arranca, antes de crear la ventana
    ' Shutdown: cuando se cierran todos los forms
    ' UnhandledException: cuando peta algo y nadie lo pillaba
    ' StartupNextInstance: si abro la app dos veces este se dispara
    ' NetworkAvailabilityChanged: si se cae o vuelve la red

    Partial Friend Class MyApplication

        ' aqui pillo cualquier excepcion que se haya escapado de los try-catch
        ' la apunto en optima_log.txt y le digo al user que paso, en vez de que la app se cierre sola
        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Try
                LogErrores.Registrar("UnhandledException", e.Exception)
            Catch
                ' si el log mismo falla pues ya no puedo hacer nada
            End Try

            Try
                MessageBox.Show(
                    "Se ha producido un error inesperado y se ha registrado en el archivo de log." & vbCrLf & vbCrLf &
                    "Detalle: " & e.Exception.Message,
                    "Error inesperado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch
            End Try

            ' le digo a la app que NO se cierre por este error
            e.ExitApplication = False
        End Sub

        ' apunto en el log que la app ha arrancado (asi se cuando se abrio)
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            Try
                LogErrores.Info("Application", "Inicio de aplicación OPTIMA")
            Catch
            End Try
        End Sub

    End Class
End Namespace
