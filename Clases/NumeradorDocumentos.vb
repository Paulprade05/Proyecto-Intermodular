Imports System.Data.SQLite

' clase para sacar el siguiente numero de un documento (PRE-001, PED-002, ALB-003 etc)
'
' antes hacia esto en cada formulario y siempre lo mismo. ademas tenia un bug:
' si pasabas de PRE-999 a PRE-1000 se rompia porque "PRE-1000" alfabeticamente es menor
' que "PRE-999" (raro pero asi van los strings). me llevo dos dias pillarlo
'
' aqui ordeno solo por la parte de los numeros y ya. funciona con cualquier cantidad
' de digitos. empieza con 3 (001) y va creciendo si hace falta
Public Class NumeradorDocumentos

    ' calcula el siguiente numero de la tabla y columna que le pases
    ' ejemplo: SiguienteNumero("ALB-", "Albaranes", "NumeroAlbaran") da "ALB-005"
    Public Shared Function SiguienteNumero(prefijo As String, tabla As String, columna As String) As String
        Dim siguiente As Integer = 1

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' cojo solo los digitos despues del guion y los paso a INTEGER
            ' asi 999 < 1000 y no rompe (con strings 999 era mayor que 1000)
            Dim sql As String = $"SELECT MAX(CAST(SUBSTR({columna}, LENGTH(@prefijo) + 1) AS INTEGER)) " &
                                $"FROM {tabla} WHERE {columna} LIKE @pat"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@prefijo", prefijo)
                cmd.Parameters.AddWithValue("@pat", prefijo & "%")
                Dim res = cmd.ExecuteScalar()
                If res IsNot Nothing AndAlso Not IsDBNull(res) Then
                    siguiente = Convert.ToInt32(res) + 1
                End If
            End Using
        Catch
            ' si peta por lo que sea no quiero que se quede colgado el form
            ' asi que devuelvo un numero raro con los ticks. feo pero unico
            Return prefijo & DateTime.Now.Ticks.ToString().Substring(12)
        End Try

        ' D3 le pone ceros delante hasta llegar a 3 digitos. si es mas grande lo deja igual
        Return prefijo & siguiente.ToString("D3")
    End Function

End Class
