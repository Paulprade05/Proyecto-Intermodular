Imports System.Data.SQLite
Imports System.IO

' clase que se encarga de hacer copias de la base de datos
'
' antes el codigo de backup estaba en dos sitios (en PagHome para el auto y en
' FrmConfiguracion para el manual) y ademas con bugs:
'   - PagHome tenia mal la ruta ("BD\Optima.db") y nunca hacia la copia
'   - los dos usaban File.Copy directamente, que es peligroso si la bd esta escribiendo
'
' ahora todo pasa por aqui y uso "VACUUM INTO" de sqlite, que es la forma buena de copiar
' una bd que esta abierta sin que se rompa
Public Class GestorBackups

    ' donde esta la bd actual
    Public Shared ReadOnly Property RutaBDActual() As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Optima.db")
        End Get
    End Property

    ' carpeta donde guardo las bd que se sustituyen al restaurar (papelera por si acaso)
    Public Shared ReadOnly Property RutaPapelera() As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PapeleraDB")
        End Get
    End Property

    ' copia automatica. devuelve la ruta del fichero o Nothing si falla
    ' no enseña ningun mensaje al usuario, va en silencio
    Public Shared Function HacerCopiaAutomatica(carpetaDestino As String) As String
        Try
            Dim carpeta As String = carpetaDestino
            If String.IsNullOrWhiteSpace(carpeta) Then
                carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CopiasSeguridad")
            End If
            If Not Directory.Exists(carpeta) Then Directory.CreateDirectory(carpeta)

            Dim nombre As String = "OptimaDB_AUTO_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".sqlite"
            Dim destino As String = Path.Combine(carpeta, nombre)

            HacerVacuumInto(destino)
            LimpiarCopiasAntiguas(carpeta, "OptimaDB_AUTO_*.sqlite", 5)

            LogErrores.Info("GestorBackups.Auto", "Copia automática creada: " & destino)
            Return destino
        Catch ex As Exception
            ' si va en automatico no quiero molestar al user, pero apunto en el log
            LogErrores.Registrar("GestorBackups.Auto", ex)
            Return Nothing
        End Try
    End Function

    ' copia manual (cuando el user pulsa el boton). aqui SI lanzo excepciones para que
    ' el formulario las pille y le diga al usuario que paso
    Public Shared Function HacerCopiaManual(carpetaDestino As String) As String
        If String.IsNullOrWhiteSpace(carpetaDestino) OrElse Not Directory.Exists(carpetaDestino) Then
            Throw New ArgumentException("La carpeta de destino no es válida.")
        End If

        Dim nombre As String = "OptimaDB_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".sqlite"
        Dim destino As String = Path.Combine(carpetaDestino, nombre)

        HacerVacuumInto(destino)

        LogErrores.Info("GestorBackups.Manual", "Copia manual creada: " & destino)
        Return destino
    End Function

    ' restaura una bd desde un fichero. la bd actual se mueve a la papelera por si me equivoco
    ' y quiero recuperarla luego
    Public Shared Sub Restaurar(rutaFicheroOrigen As String)
        If Not File.Exists(rutaFicheroOrigen) Then
            Throw New FileNotFoundException("No se encuentra el archivo de copia.", rutaFicheroOrigen)
        End If

        ' creo la papelera si no existe
        If Not Directory.Exists(RutaPapelera) Then Directory.CreateDirectory(RutaPapelera)

        ' cierro la conexion actual para que Windows libere el fichero
        Try
            Dim c = ConexionBD.GetConnection()
            If c IsNot Nothing AndAlso c.State = ConnectionState.Open Then c.Close()
        Catch
        End Try
        SQLiteConnection.ClearAllPools()
        GC.Collect()
        GC.WaitForPendingFinalizers()
        System.Threading.Thread.Sleep(500)

        ' muevo la bd vieja a papelera
        If File.Exists(RutaBDActual) Then
            Dim destinoPapelera As String = Path.Combine(RutaPapelera, "BD_Eliminada_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".sqlite")
            File.Move(RutaBDActual, destinoPapelera)
        End If

        ' copio la nueva en su sitio
        File.Copy(rutaFicheroOrigen, RutaBDActual)

        LogErrores.Info("GestorBackups.Restaurar", $"BD restaurada desde {rutaFicheroOrigen}")
    End Sub

    ' cuanto pesa la papelera en MB
    Public Shared Function TamanoPapeleraMB() As Decimal
        If Not Directory.Exists(RutaPapelera) Then Return 0
        Dim bytes As Long = 0
        For Each f As FileInfo In New DirectoryInfo(RutaPapelera).GetFiles()
            bytes += f.Length
        Next
        Return Math.Round(CDec(bytes) / (1024D * 1024D), 2)
    End Function

    ' vacia la papelera. devuelve cuantos archivos borro
    Public Shared Function VaciarPapelera() As Integer
        If Not Directory.Exists(RutaPapelera) Then Return 0
        Dim n As Integer = 0
        For Each f As FileInfo In New DirectoryInfo(RutaPapelera).GetFiles()
            Try
                f.Delete()
                n += 1
            Catch
                ' si esta bloqueado lo dejo y ya
            End Try
        Next
        Return n
    End Function

    ' aqui es donde hago la magia: VACUUM INTO te crea una copia limpia y consistente
    ' de la bd aunque este abierta. SQLite se encarga de los locks por dentro
    Private Shared Sub HacerVacuumInto(destino As String)
        ' si ya existe (raro, en el mismo segundo) lo borro para que no de error
        If File.Exists(destino) Then File.Delete(destino)

        Dim c = ConexionBD.GetConnection()
        If c.State <> ConnectionState.Open Then c.Open()

        ' parece que VACUUM INTO no traga parametros normales, asi que tengo que escapar
        ' las comillas a mano (por si la ruta tiene una)
        Dim destinoEscapado As String = destino.Replace("'", "''")
        Using cmd As New SQLiteCommand($"VACUUM INTO '{destinoEscapado}'", c)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ' deja como mucho N copias automaticas. Las antiguas las borra
    ' solo toca las que cumplen el patron, las copias manuales no las tocamos
    Private Shared Sub LimpiarCopiasAntiguas(carpeta As String, patron As String, maximo As Integer)
        Try
            Dim dir As New DirectoryInfo(carpeta)
            Dim archivos = dir.GetFiles(patron) _
                              .OrderBy(Function(f) f.CreationTime) _
                              .ToList()
            If archivos.Count > maximo Then
                For i As Integer = 0 To archivos.Count - maximo - 1
                    Try : archivos(i).Delete() : Catch : End Try
                Next
            End If
        Catch ex As Exception
            LogErrores.Registrar("GestorBackups.LimpiarCopiasAntiguas", ex)
        End Try
    End Sub

End Class
