Imports System.Text.RegularExpressions

' clase con todas las validaciones de los formularios
' cada funcion devuelve dos cosas: un boolean (si esta bien o no) y un mensaje
' si lo dejas vacio lo doy por bueno (la obligatoriedad la miro en otro sitio)
'
' valido NIF, NIE, CIF, email, IBAN, codigo postal y telefono (los españoles)
Public Class Validador

    ' las letras del DNI en orden, para sacar la letra a partir del numero
    Private Const LETRAS_DNI As String = "TRWAGMYFPDXBNJZSQVHLCKE"

    ' valida un NIF/DNI: 8 numeros + 1 letra
    Public Shared Function ValidarNIF(nif As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(nif) Then Return (True, "")
        Dim s As String = nif.Trim().ToUpperInvariant()

        If Not Regex.IsMatch(s, "^[0-9]{8}[A-Z]$") Then
            Return (False, "El NIF debe tener 8 dígitos seguidos de una letra (ej. 12345678Z).")
        End If

        Dim numero As Integer = Integer.Parse(s.Substring(0, 8))
        Dim letraEsperada As Char = LETRAS_DNI(numero Mod 23)
        If s(8) <> letraEsperada Then
            Return (False, $"La letra del NIF no es correcta. Debería ser '{letraEsperada}'.")
        End If
        Return (True, "")
    End Function

    ' valida NIE: empieza por X, Y o Z, luego 7 numeros y una letra
    ' truco: la X la cuento como 0, la Y como 1 y la Z como 2 (asi sale el numero)
    Public Shared Function ValidarNIE(nie As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(nie) Then Return (True, "")
        Dim s As String = nie.Trim().ToUpperInvariant()

        If Not Regex.IsMatch(s, "^[XYZ][0-9]{7}[A-Z]$") Then
            Return (False, "El NIE debe empezar por X, Y o Z, seguido de 7 dígitos y una letra (ej. X1234567L).")
        End If

        ' cambio la letra inicial por su numero correspondiente
        Dim sustitucion As String = ""
        Select Case s(0)
            Case "X"c : sustitucion = "0"
            Case "Y"c : sustitucion = "1"
            Case "Z"c : sustitucion = "2"
        End Select

        Dim numero As Integer = Integer.Parse(sustitucion & s.Substring(1, 7))
        Dim letraEsperada As Char = LETRAS_DNI(numero Mod 23)
        If s(8) <> letraEsperada Then
            Return (False, $"La letra del NIE no es correcta. Debería ser '{letraEsperada}'.")
        End If
        Return (True, "")
    End Function

    ' valida CIF de empresas. la formula esta es la oficial de hacienda y es bastante palo
    ' me la tuve que copiar de la web de la AEAT
    Public Shared Function ValidarCIF(cif As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(cif) Then Return (True, "")
        Dim s As String = cif.Trim().ToUpperInvariant()

        If Not Regex.IsMatch(s, "^[A-HJNPQRSUVW][0-9]{7}[0-9A-J]$") Then
            Return (False, "El CIF debe empezar por una letra de organización válida (A-H, J, N, P-S, U-W), 7 dígitos y un carácter de control.")
        End If

        ' lo del digito de control es esto:
        '   - sumas las posiciones pares tal cual
        '   - las impares las multiplicas por 2 y sumas los digitos del resultado
        '   - sumas todo, sacas el modulo 10, y haces (10 - eso) mod 10
        '   - dependiendo de la letra inicial el ultimo char es numero o letra (o vale cualquiera)
        Dim sumaPares As Integer = 0
        Dim sumaImpares As Integer = 0
        For i As Integer = 1 To 7
            Dim d As Integer = Integer.Parse(s(i).ToString())
            If (i Mod 2) = 0 Then
                sumaPares += d
            Else
                Dim x As Integer = d * 2
                sumaImpares += (x \ 10) + (x Mod 10)
            End If
        Next
        Dim total As Integer = sumaPares + sumaImpares
        Dim digitoControl As Integer = (10 - (total Mod 10)) Mod 10

        Dim letraInicial As Char = s(0)
        Dim ultimo As Char = s(8)

        ' algunas letras iniciales obligan a que el control sea letra (P, Q, R, S, W, N)
        ' otras a que sea numero (A, B, E, H). las demas tragan cualquiera de los dos
        Dim letrasControl As String = "JABCDEFGHI"
        Dim letraEsperada As Char = letrasControl(digitoControl)

        Dim soloLetra As String = "PQRSWN"
        Dim soloNumero As String = "ABEH"

        If soloLetra.Contains(letraInicial) Then
            If ultimo <> letraEsperada Then
                Return (False, $"El carácter de control del CIF no es correcto. Debería ser '{letraEsperada}'.")
            End If
        ElseIf soloNumero.Contains(letraInicial) Then
            If ultimo <> Char.Parse(digitoControl.ToString()) Then
                Return (False, $"El dígito de control del CIF no es correcto. Debería ser '{digitoControl}'.")
            End If
        Else
            ' los dos valen
            If ultimo <> letraEsperada AndAlso ultimo <> Char.Parse(digitoControl.ToString()) Then
                Return (False, $"El carácter de control del CIF no es correcto. Debería ser '{letraEsperada}' o '{digitoControl}'.")
            End If
        End If

        Return (True, "")
    End Function

    ' si el campo permite cualquier tipo de doc (cliente puede ser persona, autonomo o empresa)
    ' miro el primer caracter y voy probando: numero -> NIF, X/Y/Z -> NIE, letra -> CIF
    Public Shared Function ValidarDocumentoIdentidad(doc As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(doc) Then Return (True, "")
        Dim s As String = doc.Trim().ToUpperInvariant()

        Dim primerChar As Char = s(0)
        If Char.IsDigit(primerChar) Then
            Return ValidarNIF(s)
        ElseIf "XYZ".Contains(primerChar) Then
            Return ValidarNIE(s)
        Else
            Return ValidarCIF(s)
        End If
    End Function

    ' email. la regex no es 100% del estandar pero pilla casi todo
    Public Shared Function ValidarEmail(email As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(email) Then Return (True, "")
        Dim s As String = email.Trim()

        Dim patron As String = "^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$"
        If Not Regex.IsMatch(s, patron) Then
            Return (False, "El email no tiene un formato válido (ejemplo: nombre@dominio.com).")
        End If
        Return (True, "")
    End Function

    ' IBAN: vale para cualquier pais. lleva un MOD-97 que es la formula que usan los bancos
    Public Shared Function ValidarIBAN(iban As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(iban) Then Return (True, "")

        ' quito espacios y mayusculas
        Dim s As String = Regex.Replace(iban.ToUpperInvariant(), "\s", "")

        If s.Length < 15 OrElse s.Length > 34 Then
            Return (False, "La longitud del IBAN no es válida (entre 15 y 34 caracteres).")
        End If
        If Not Regex.IsMatch(s, "^[A-Z]{2}[0-9]{2}[A-Z0-9]+$") Then
            Return (False, "El IBAN debe empezar por dos letras (país), dos dígitos (control) y luego cuenta.")
        End If

        ' muevo los 4 primeros caracteres al final (asi funciona el algoritmo)
        Dim reordenado As String = s.Substring(4) & s.Substring(0, 4)

        ' las letras se cambian por numeros: A=10, B=11... Z=35
        Dim numerico As New System.Text.StringBuilder()
        For Each ch As Char In reordenado
            If Char.IsDigit(ch) Then
                numerico.Append(ch)
            ElseIf Char.IsLetter(ch) Then
                numerico.Append((Asc(ch) - Asc("A"c) + 10).ToString())
            Else
                Return (False, "El IBAN contiene caracteres no válidos.")
            End If
        Next

        ' calculo el modulo 97 a trozos porque si lo hago de una el numero se me sale del Long
        Dim resto As Integer = 0
        For Each ch As Char In numerico.ToString()
            resto = (resto * 10 + Integer.Parse(ch.ToString())) Mod 97
        Next

        If resto <> 1 Then
            Return (False, "El IBAN no es válido (los dígitos de control no coinciden).")
        End If
        Return (True, "")
    End Function

    ' codigo postal español: 5 numeros y los 2 primeros entre 01 y 52 (las provincias)
    Public Shared Function ValidarCodigoPostal(cp As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(cp) Then Return (True, "")
        Dim s As String = cp.Trim()

        If Not Regex.IsMatch(s, "^[0-9]{5}$") Then
            Return (False, "El código postal debe tener exactamente 5 dígitos.")
        End If
        Dim provincia As Integer = Integer.Parse(s.Substring(0, 2))
        If provincia < 1 OrElse provincia > 52 Then
            Return (False, "El código postal no corresponde a ninguna provincia española (01-52).")
        End If
        Return (True, "")
    End Function

    ' telefono español: 9 numeros empezando por 6, 7, 8 o 9
    ' tambien acepto el +34 delante por si acaso, y guiones/espacios los quito
    Public Shared Function ValidarTelefono(tel As String) As (Ok As Boolean, Mensaje As String)
        If String.IsNullOrWhiteSpace(tel) Then Return (True, "")

        Dim s As String = Regex.Replace(tel.Trim(), "[\s\-()]", "")
        ' fuera el prefijo internacional si lo trae
        If s.StartsWith("+34") Then s = s.Substring(3)
        If s.StartsWith("0034") Then s = s.Substring(4)

        If Not Regex.IsMatch(s, "^[6789][0-9]{8}$") Then
            Return (False, "El teléfono debe tener 9 dígitos y empezar por 6, 7, 8 o 9 (móvil o fijo en España).")
        End If
        Return (True, "")
    End Function

    ' helper para los formularios. le mandas una lista de validaciones, si alguna falla
    ' te enseña un solo MessageBox con todas y te pregunta si quieres guardar igual
    '
    ' lo hago asi porque algunos datos viejos en la bd ya estaban mal y si bloqueo el guardado
    ' no puedes ni editarlos. asi avisa pero deja seguir
    Public Shared Function ConfirmarSiHayProblemas(ParamArray validaciones As (Etiqueta As String, Resultado As (Ok As Boolean, Mensaje As String))()) As Boolean
        Dim problemas As New List(Of String)
        For Each v In validaciones
            If Not v.Resultado.Ok Then
                problemas.Add($"• {v.Etiqueta}: {v.Resultado.Mensaje}")
            End If
        Next
        If problemas.Count = 0 Then Return True

        Dim msg As String = "Se han detectado los siguientes problemas en los datos introducidos:" & vbCrLf & vbCrLf &
                            String.Join(vbCrLf, problemas) & vbCrLf & vbCrLf &
                            "¿Quieres guardar de todos modos?"

        Dim resp = MessageBox.Show(msg, "Datos con avisos", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
        Return (resp = DialogResult.Yes)
    End Function

End Class
