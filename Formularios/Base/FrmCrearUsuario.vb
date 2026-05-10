Imports System.Data.SQLite

Public Class FrmCrearUsuario

    ' Nuevos controles generados por código
    Private txtNombreCompleto As New TextBox()
    Private txtEmail As New TextBox()
    Private txtUsuario As New TextBox()
    Private txtPass As New TextBox()
    Private txtPass2 As New TextBox()
    Private cmbRol As New ComboBox()
    Private txtCIF As New TextBox()

    Private WithEvents btnCrear As New Button()
    Private WithEvents btnCancelar As New Button()

    Public Sub New()
        InitializeComponent()

        ' Hacemos la ventana más alta para que quepa todo (Alto: 700)
        Me.Size = New Size(400, 720)
        Me.BackColor = Color.FromArgb(40, 50, 70) ' Fondo oscuro premium
        Me.ForeColor = Color.WhiteSmoke
        Me.Text = "Registro de Nuevo Usuario"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen

        ConstruirInterfaz()
    End Sub

    Private Sub ConstruirInterfaz()
        Dim lblTitulo As New Label With {.Text = "CREAR USUARIO", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .Location = New Point(40, 20), .AutoSize = True}
        Me.Controls.Add(lblTitulo)

        ' 1. Datos Personales
        ConfigurarCampo("Nombre Completo:", txtNombreCompleto, 40, 70)
        ConfigurarCampo("Correo Electrónico:", txtEmail, 40, 140)

        ' 2. Datos de Acceso
        ConfigurarCampo("Nombre de Usuario (Login):", txtUsuario, 40, 210)
        ConfigurarCampo("Contraseña:", txtPass, 40, 280, True)
        ConfigurarCampo("Repetir Contraseña:", txtPass2, 40, 350, True)

        ' 3. Rol en la empresa (ComboBox)
        Dim lblRol As New Label With {.Text = "Rol en el ERP:", .Location = New Point(40, 420), .AutoSize = True, .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = Color.FromArgb(200, 200, 200)}
        Me.Controls.Add(lblRol)

        cmbRol.Bounds = New Rectangle(40, 445, 300, 30)
        cmbRol.Font = New Font("Segoe UI", 12)
        cmbRol.DropDownStyle = ComboBoxStyle.DropDownList
        ' Añadimos los roles básicos de un ERP
        cmbRol.Items.AddRange({"Usuario"})
        cmbRol.SelectedIndex = 0 ' Por defecto seleccionamos "Usuario"
        Me.Controls.Add(cmbRol)

        ' 4. Seguridad
        ConfigurarCampo("CIF de la Empresa (Seguridad):", txtCIF, 40, 500)

        ' 5. Botones
        btnCancelar.Text = "Cancelar"
        btnCancelar.Bounds = New Rectangle(40, 600, 140, 40)
        btnCancelar.BackColor = Color.FromArgb(80, 90, 100)
        btnCancelar.FlatStyle = FlatStyle.Flat
        btnCancelar.FlatAppearance.BorderSize = 0
        btnCancelar.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        btnCancelar.Cursor = Cursors.Hand
        Me.Controls.Add(btnCancelar)

        btnCrear.Text = "Confirmar"
        btnCrear.Bounds = New Rectangle(200, 600, 140, 40)
        btnCrear.BackColor = Color.FromArgb(0, 120, 215)
        btnCrear.FlatStyle = FlatStyle.Flat
        btnCrear.FlatAppearance.BorderSize = 0
        btnCrear.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        btnCrear.Cursor = Cursors.Hand
        Me.Controls.Add(btnCrear)
    End Sub

    Private Sub ConfigurarCampo(titulo As String, txt As TextBox, x As Integer, y As Integer, Optional esPass As Boolean = False)
        Dim lbl As New Label With {.Text = titulo, .Location = New Point(x, y), .AutoSize = True, .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = Color.FromArgb(200, 200, 200)}
        Me.Controls.Add(lbl)

        txt.Bounds = New Rectangle(x, y + 25, 300, 30)
        txt.Font = New Font("Segoe UI", 12)
        txt.BorderStyle = BorderStyle.FixedSingle
        If esPass Then txt.UseSystemPasswordChar = True
        Me.Controls.Add(txt)
    End Sub

    ' LÓGICA DE BASE DE DATOS Y VALIDACIÓN
    Private Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        Me.Close()
    End Sub

    Private Sub btnCrear_Click(sender As Object, e As EventArgs) Handles btnCrear.Click
        Dim nom As String = txtNombreCompleto.Text.Trim()
        Dim mail As String = txtEmail.Text.Trim()
        Dim usr As String = txtUsuario.Text.Trim()
        Dim pwd As String = txtPass.Text.Trim()
        Dim pwd2 As String = txtPass2.Text.Trim()
        Dim rol As String = cmbRol.SelectedItem.ToString()
        Dim cif As String = txtCIF.Text.Trim()

        ' 1. Validaciones de campos vacíos
        If nom = "" Or mail = "" Or usr = "" Or pwd = "" Or cif = "" Then
            MessageBox.Show("Por favor, rellena todos los campos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        ' 2. Validación de contraseñas
        If pwd <> pwd2 Then
            MessageBox.Show("Las contraseñas no coinciden. Vuelve a escribirlas.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' 3. Medida de Seguridad: Verificar CIF de la Empresa
            Dim sqlCIF As String = "SELECT COUNT(*) FROM Empresa WHERE CIF = @cif"
            Using cmd As New SQLiteCommand(sqlCIF, c)
                cmd.Parameters.AddWithValue("@cif", cif)
                If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then
                    MessageBox.Show("El CIF de empresa introducido no es correcto." & vbCrLf & "Contacta con el administrador.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If
            End Using

            ' 4. Verificar que el usuario no exista ya (Alias)
            Dim sqlUser As String = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @usr"
            Using cmd As New SQLiteCommand(sqlUser, c)
                cmd.Parameters.AddWithValue("@usr", usr)
                If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                    MessageBox.Show("Ese nombre de usuario ya está en uso. Elige otro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    Return
                End If
            End Using

            ' 5. Guardar el nuevo usuario con TODOS LOS DATOS (con la contraseña hasheada)
            Dim passwordHasheada As String = PasswordHasher.Hashear(pwd)

            Dim sqlInsert As String = "INSERT INTO Usuarios (NombreUsuario, Password, Rol, NombreCompleto, Email, Activo, FechaRegistro) " &
                                      "VALUES (@usr, @pwd, @rol, @nom, @mail, 1, @fecha)"
            Using cmd As New SQLiteCommand(sqlInsert, c)
                cmd.Parameters.AddWithValue("@usr", usr)
                cmd.Parameters.AddWithValue("@pwd", passwordHasheada)
                cmd.Parameters.AddWithValue("@rol", rol)
                cmd.Parameters.AddWithValue("@nom", nom)
                cmd.Parameters.AddWithValue("@mail", mail)
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("¡Usuario " & nom & " creado con éxito!" & vbCrLf & "Ya puedes iniciar sesión en OPTIMA.", "Registro completado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close() ' Cerramos la ventana y volvemos al login

        Catch ex As Exception
            MessageBox.Show("Error al guardar en la base de datos: " & ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
