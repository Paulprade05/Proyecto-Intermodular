Imports System.Security.Cryptography
Imports System.Text

' clase para guardar contraseñas hasheadas
' antes las guardaba en texto plano en la tabla Usuarios pero el profe me dijo
' que eso es un error gordisimo asi que ahora uso PBKDF2 con sha256
'
' lo que se guarda en la bd queda asi: "PBKDF2$iteraciones$salt$hash"
' (todo en base64 menos la palabra PBKDF2 y los numeros)
'
' si me encuentro una pass vieja sin el prefijo PBKDF2$ la comparo en plano
' y al siguiente login la cambio a la nueva forma sin que se entere nadie
Public Class PasswordHasher

    Private Const PREFIJO As String = "PBKDF2"
    Private Const ITERACIONES As Integer = 50000   ' he probado y con menos va rapido pero dicen que asi es mas seguro
    Private Const SALT_BYTES As Integer = 16
    Private Const HASH_BYTES As Integer = 32

    ' devuelve el string final que toca guardar en la bd
    Public Shared Function Hashear(password As String) As String
        If password Is Nothing Then password = ""
        Dim salt(SALT_BYTES - 1) As Byte
        Using rng As New RNGCryptoServiceProvider()
            rng.GetBytes(salt)
        End Using
        Dim hash As Byte() = DerivarHash(password, salt, ITERACIONES, HASH_BYTES)
        Return $"{PREFIJO}${ITERACIONES}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}"
    End Function

    ' compara lo que escribe el usuario con lo que tengo guardado
    ' funciona tanto si es el formato nuevo como si es el viejo en plano
    Public Shared Function Verificar(passwordIntroducida As String, passwordEnBD As String) As Boolean
        If passwordEnBD Is Nothing Then Return False
        If passwordIntroducida Is Nothing Then passwordIntroducida = ""

        ' contraseñas viejas en texto plano (las de antes del refactor)
        If Not passwordEnBD.StartsWith(PREFIJO & "$") Then
            Return passwordIntroducida = passwordEnBD
        End If

        ' nuevo formato, lo parto por $
        Dim partes = passwordEnBD.Split("$"c)
        If partes.Length <> 4 Then Return False

        Try
            Dim iter As Integer = Integer.Parse(partes(1))
            Dim salt As Byte() = Convert.FromBase64String(partes(2))
            Dim hashEsperado As Byte() = Convert.FromBase64String(partes(3))
            Dim hashCalculado As Byte() = DerivarHash(passwordIntroducida, salt, iter, hashEsperado.Length)

            ' lei en stack overflow que no se pueden comparar bytes con = porque
            ' un atacante puede medir el tiempo y deducir cosas. asi que voy uno por uno
            Return CompararConstante(hashEsperado, hashCalculado)
        Catch
            Return False
        End Try
    End Function

    ' me dice si la pass guardada todavia es de las viejas (sin hash) y hay que actualizarla
    Public Shared Function NecesitaMigracion(passwordEnBD As String) As Boolean
        If String.IsNullOrEmpty(passwordEnBD) Then Return False
        Return Not passwordEnBD.StartsWith(PREFIJO & "$")
    End Function

    ' calcula el hash de la pass con la sal. usa la clase rara esa de .net
    Private Shared Function DerivarHash(password As String, salt As Byte(), iter As Integer, longitudBytes As Integer) As Byte()
        Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256)
            Return pbkdf2.GetBytes(longitudBytes)
        End Using
    End Function

    ' compara dos arrays de bytes pero sin cortar antes (para lo del timing)
    Private Shared Function CompararConstante(a As Byte(), b As Byte()) As Boolean
        If a.Length <> b.Length Then Return False
        Dim diff As Integer = 0
        For i As Integer = 0 To a.Length - 1
            diff = diff Or (a(i) Xor b(i))
        Next
        Return diff = 0
    End Function

End Class
