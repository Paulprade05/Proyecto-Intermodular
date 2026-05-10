' modulo con los datos del usuario que esta logueado ahora mismo
' uso un Module para poder acceder desde cualquier formulario sin tener que pasarlo
Public Module ComunSesionActual
    Public Property Usuario As String
    Public Property Contrasena As String
    Public Property Empresa As String

    ' antes solo guardaba el nombre del usuario y ya. resultado: en los movimientos del almacen
    ' me salia siempre user=1 (lo tenia a fuego en el codigo, jeje). ahora guardo tambien el id
    ' y el rol para que la auditoria diga quien hizo cada cosa
    Public Property IdUsuario As Integer = 0
    Public Property Rol As String = ""
    Public Property NombreCompleto As String = ""

    ' funcioncita para no tener que escribir If Rol = "Admin" en mil sitios
    Public Function EsAdministrador() As Boolean
        Return Rol IsNot Nothing AndAlso Rol.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase)
    End Function

    ' al cerrar sesion limpio todo
    Public Sub CerrarSesion()
        Usuario = ""
        Contrasena = ""
        Empresa = ""
        IdUsuario = 0
        Rol = ""
        NombreCompleto = ""
    End Sub
End Module
