Imports System.Data.SQLite ' hay que instalar el nuget System.Data.SQLite
Imports System.IO

' clase que abre la conexion a la BD una sola vez y la reutiliza siempre
' es un singleton (eso me lo enseño el profe en clase)
Public Class ConexionBD
    ' la unica conexion que va a haber en toda la app
    Private Shared _conexion As SQLiteConnection

    ' la ruta del fichero .db, junto al exe
    Private Shared ReadOnly _dbPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Optima.db")
    Private Shared ReadOnly _connectionString As String = $"Data Source={_dbPath}"

    ' constructor privado para que nadie haga "New ConexionBD" por ahi
    Private Sub New()
    End Sub

    ' me devuelve la conexion. si no la habia creada la crea ahora
    Public Shared Function GetConnection() As SQLiteConnection
        If _conexion Is Nothing Then
            CrearBaseDatosSiNoExiste()
            Try
                _conexion = New SQLiteConnection(_connectionString)
                _conexion.Open()
                ' OJO: en sqlite las foreign keys NO funcionan si no las activas tu
                ' si no haces esto los ON DELETE CASCADE no hacen nada y puedes meter
                ' filas con referencias a cosas que no existen. me llevo un rato pillarlo
                ActivarForeignKeys()
            Catch ex As Exception
                MessageBox.Show("Error crítico de conexión: " & ex.Message)
                Return Nothing ' me salgo o casca el resto
            End Try
        End If

        ' por si acaso la conexion estaba cerrada la reabrimos
        If _conexion IsNot Nothing AndAlso _conexion.State = ConnectionState.Closed Then
            _conexion.Open()
            ActivarForeignKeys() ' al reabrir hay que volver a activarlas
        End If

        Return _conexion
    End Function

    ' enciende las foreign keys de sqlite
    Private Shared Sub ActivarForeignKeys()
        Try
            Using cmd As New SQLiteCommand("PRAGMA foreign_keys = ON;", _conexion)
                cmd.ExecuteNonQuery()
            End Using
        Catch
            ' si peta tampoco pasa nada grave, la app va igual pero sin la proteccion
            ' (que era como iba antes igualmente)
        End Try
    End Sub

    ' crea el fichero de la bd si no existe ya (la primera vez)
    Private Shared Sub CrearBaseDatosSiNoExiste()
        If Not File.Exists(_dbPath) Then
            SQLiteConnection.CreateFile(_dbPath)
        End If
    End Sub
End Class
