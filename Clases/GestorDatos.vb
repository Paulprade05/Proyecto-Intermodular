Imports System.Data.SQLite

' clase con dos funciones para leer datos de la bd sin tener que repetir codigo
Public Class GestorDatos

    ' me lee una tabla entera y la devuelve como DataTable
    ' uso: Dim dt = GestorDatos.LeerTabla("Clientes")
    Public Shared Function LeerTabla(nombreTabla As String) As DataTable
        Dim dt As New DataTable()

        Try
            Dim conexion = ConexionBD.GetConnection()

            If conexion.State <> ConnectionState.Open Then conexion.Open()

            ' OJO: el nombre de la tabla no se puede pasar como parametro (@tabla),
            ' asi que lo concateno directo. Como es codigo mio y no lo escribe el user no pasa nada
            Dim sql As String = $"SELECT * FROM {nombreTabla}"

            Using cmd As New SQLiteCommand(sql, conexion)
                Using da As New SQLiteDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error al leer la tabla '{nombreTabla}': {ex.Message}", "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Return dt
    End Function

    ' para queries que ya escribo yo, con WHERE, ORDER BY, JOINs etc
    ' uso: Dim dt = GestorDatos.EjecutarSelect("SELECT * FROM Pedidos WHERE Estado='Pendiente'")
    Public Shared Function EjecutarSelect(sql As String) As DataTable
        Dim dt As New DataTable()

        Try
            Dim conexion = ConexionBD.GetConnection()
            If conexion.State <> ConnectionState.Open Then conexion.Open()

            Using cmd As New SQLiteCommand(sql, conexion)
                Using da As New SQLiteDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error en la consulta: {ex.Message}", "Error SQL", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Return dt
    End Function

End Class
