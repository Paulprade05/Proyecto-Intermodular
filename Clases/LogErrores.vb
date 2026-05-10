Imports System.IO

' clase para guardar los errores en un fichero de texto
' antes los errores se enseñaban en un MessageBox y luego ya nadie los volvia a ver
' asi al menos quedan apuntados por si despues quiero mirarlos
'
' guarda en optima_log.txt al lado del exe
' cada linea queda asi: [fecha] [USR=loquesea] [donde] mensaje
' cuando el fichero pasa de 1 MB lo renombra a optima_log.old.txt y empieza otro
'
' como se usa:
'   Catch ex As Exception
'       LogErrores.Registrar("FrmFacturas.Guardar", ex)
'       MessageBox.Show("Ha pasado algo...")
'   End Try
Public Class LogErrores

    Private Shared ReadOnly _ruta As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "optima_log.txt")
    Private Shared ReadOnly _rutaOld As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "optima_log.old.txt")
    Private Shared ReadOnly _lock As New Object()
    Private Const MAX_BYTES As Long = 1024 * 1024 ' 1 MB me parece suficiente

    ' apunta una excepcion con todo el rollo del stacktrace
    Public Shared Sub Registrar(modulo As String, ex As Exception)
        Try
            EscribirLinea($"[ERROR] [{modulo}] {ex.GetType().Name}: {ex.Message}")
            If ex.StackTrace IsNot Nothing Then
                EscribirLinea($"        StackTrace: {ex.StackTrace.Replace(vbCrLf, " | ")}")
            End If
        Catch
            ' si peta el propio log lo trago. mejor quedarme sin log que romperle la app al user
        End Try
    End Sub

    ' apunta info normal (no es error, solo para saber que ha pasado algo)
    Public Shared Sub Info(modulo As String, mensaje As String)
        Try
            EscribirLinea($"[INFO]  [{modulo}] {mensaje}")
        Catch
        End Try
    End Sub

    ' apunta un aviso. algo raro pero no es un error de cascar la app
    Public Shared Sub Aviso(modulo As String, mensaje As String)
        Try
            EscribirLinea($"[AVISO] [{modulo}] {mensaje}")
        Catch
        End Try
    End Sub

    Private Shared Sub EscribirLinea(texto As String)
        SyncLock _lock
            ' si el fichero pesa mucho lo muevo a .old y empiezo otro nuevo
            Try
                If File.Exists(_ruta) AndAlso New FileInfo(_ruta).Length > MAX_BYTES Then
                    If File.Exists(_rutaOld) Then File.Delete(_rutaOld)
                    File.Move(_ruta, _rutaOld)
                End If
            Catch
                ' si peta lo dejo, no quiero que el log bloquee nada
            End Try

            Dim usr As String = If(ComunSesionActual.Usuario, "?")
            Dim linea As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [USR={usr}] {texto}"
            File.AppendAllText(_ruta, linea & Environment.NewLine)
        End SyncLock
    End Sub

End Class
