Imports System.Drawing.Drawing2D
Imports System.Data.SQLite ' Necesario para leer la BD

Public Class PagAcceso
    ' Creamos el panel central por código (La "Tarjeta" de Login)
    Private PanelLogin As New Panel()
    ' Creamos el CheckBox para recordar credenciales
    Private CheckBoxRecordar As New CheckBox()
    ' Importamos la función nativa para congelar el dibujo de la ventana
    Private Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    Private Const WM_SETREDRAW As Integer = &HB

    Public Sub New()
        InitializeComponent()
        Me.StartPosition = FormStartPosition.Manual
        Me.Location = New Point(-10000, -10000)
        ConstruirInterfazPremium()
    End Sub

    Private Sub PagAcceso_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CentrarPanelLogin()
        Me.Location = New Point((Screen.PrimaryScreen.WorkingArea.Width - Me.Width) \ 2,
                            (Screen.PrimaryScreen.WorkingArea.Height - Me.Height) \ 2)
        ' Cargar las empresas en el ComboBox
        CargarEmpresas()
        ' Solo recordamos el NOMBRE de usuario, nunca la contraseña (por seguridad).
        ' Limpiamos también cualquier SavedPass que pudiera quedar de versiones anteriores.
        If Not String.IsNullOrEmpty(My.Settings.SavedPass) Then
            My.Settings.SavedPass = ""
            My.Settings.Save()
        End If
        If My.Settings.SavedUser <> "" Then
            TextBoxUsuario.Text = My.Settings.SavedUser
            CheckBoxRecordar.Checked = True
            TextBoxPassword.Focus()
        End If

        Me.Refresh()
    End Sub

    ' 1. CONSTRUCTOR DE INTERFAZ (TODO POR CÓDIGO)
    Private Sub ConstruirInterfazPremium()
        ' --- A) CREAR LA TARJETA CENTRAL ---
        PanelLogin.Size = New Size(400, 520)
        PanelLogin.BackColor = Color.FromArgb(220, 25, 35, 45)

        Dim meth = GetType(Control).GetMethod("SetStyle", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        meth.Invoke(PanelLogin, New Object() {ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.DoubleBuffer, True})

        Me.Controls.Add(PanelLogin)

        ' --- B) COGER EL LOGO ---
        Dim picLogo As PictureBox = Me.Controls.OfType(Of PictureBox)().FirstOrDefault()
        If picLogo IsNot Nothing Then
            PanelLogin.Controls.Add(picLogo)
            picLogo.Bounds = New Rectangle(50, 30, 300, 100)
            picLogo.BackColor = Color.Transparent
            picLogo.SizeMode = PictureBoxSizeMode.Zoom
            picLogo.BringToFront()
        End If

        ' --- C) ORDENAR CAJAS DE TEXTO ---
        ConfigurarCajaTexto(TextBoxUsuario, "USUARIO", 50, 150)
        ConfigurarCajaTexto(TextBoxPassword, "CONTRASEÑA", 50, 230)

        ' --- CONFIGURAR CHECKBOX "RECORDAR DATOS SESION" ---
        PanelLogin.Controls.Add(CheckBoxRecordar)
        CheckBoxRecordar.Bounds = New Rectangle(50, 285, 200, 20)
        CheckBoxRecordar.Text = "Recordar mis datos"
        CheckBoxRecordar.ForeColor = Color.FromArgb(200, 200, 200)
        CheckBoxRecordar.Font = New Font("Segoe UI", 9)
        CheckBoxRecordar.BackColor = Color.Transparent
        CheckBoxRecordar.Cursor = Cursors.Hand

        ' --- D) CONFIGURAR EL BOTÓN DEL OJITO ---
        PanelLogin.Controls.Add(ButtonVerPasswd)
        ButtonVerPasswd.Bounds = New Rectangle(315, 257, 33, 26)
        ButtonVerPasswd.FlatStyle = FlatStyle.Flat
        ButtonVerPasswd.FlatAppearance.BorderSize = 0
        ButtonVerPasswd.BackColor = Color.White
        ButtonVerPasswd.Cursor = Cursors.Hand
        ButtonVerPasswd.BringToFront()
        TextBoxPassword.UseSystemPasswordChar = True

        ' --- E) CONFIGURAR EL COMBOBOX EMPRESA ---
        Dim cmbEmpresa As ComboBox = Me.Controls.OfType(Of ComboBox).FirstOrDefault()
        If cmbEmpresa IsNot Nothing Then
            ConfigurarLabel("EMPRESA", 50, 320)
            PanelLogin.Controls.Add(cmbEmpresa)
            cmbEmpresa.Bounds = New Rectangle(50, 345, 300, 28)
            cmbEmpresa.Font = New Font("Segoe UI", 12)
            cmbEmpresa.DropDownStyle = ComboBoxStyle.DropDownList
        End If

        ' --- F) ORDENAR LOS BOTONES DE ACCIÓN ---
        PanelLogin.Controls.Add(ButtonCrearUsuario)
        PanelLogin.Controls.Add(ButtonAcceso)

        EstilizarBoton(ButtonCrearUsuario, 50, 420, 140, 40, Color.FromArgb(80, 90, 100), "Crear")
        EstilizarBoton(ButtonAcceso, 210, 420, 140, 40, Color.FromArgb(0, 120, 215), "Entrar")

        CentrarPanelLogin()
    End Sub
    Private Sub ButtonCrearUsuario_Click(sender As Object, e As EventArgs) Handles ButtonCrearUsuario.Click
        Dim frmRegistro As New FrmCrearUsuario()
        frmRegistro.ShowDialog() ' ShowDialog congela la pantalla de atrás hasta que se cierre esta
    End Sub

    ' 2. FUNCIONES DE BASE DE DATOS Y DISEÑO

    ' NUEVO: MÉTODO PARA CARGAR LAS EMPRESAS
    Private Sub CargarEmpresas()
        Dim cmbEmpresa As ComboBox = PanelLogin.Controls.OfType(Of ComboBox).FirstOrDefault()
        If cmbEmpresa Is Nothing Then Return

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String = "SELECT ID, NombreFiscal FROM Empresa"
            Using cmd As New SQLiteCommand(sql, c)
                Dim da As New SQLiteDataAdapter(cmd)
                Dim dt As New DataTable()
                da.Fill(dt)

                If dt.Rows.Count > 0 Then
                    cmbEmpresa.DataSource = dt
                    cmbEmpresa.DisplayMember = "NombreFiscal"
                    cmbEmpresa.ValueMember = "ID"
                End If
            End Using
        Catch ex As Exception
            ' Si falla la carga, simplemente no rompe el programa
        End Try
    End Sub

    Private Sub ConfigurarCajaTexto(txt As TextBox, titulo As String, x As Integer, y As Integer)
        ConfigurarLabel(titulo, x, y)
        If txt IsNot Nothing Then
            PanelLogin.Controls.Add(txt)
            txt.Bounds = New Rectangle(x, y + 25, 300, 30)
            txt.Font = New Font("Segoe UI", 12)
            txt.BorderStyle = BorderStyle.FixedSingle
        End If
    End Sub

    Private Sub ConfigurarLabel(texto As String, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = texto
        lbl.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lbl.ForeColor = Color.FromArgb(200, 200, 200)
        lbl.AutoSize = True
        lbl.Location = New Point(x, y)
        PanelLogin.Controls.Add(lbl)
    End Sub

    Private Sub EstilizarBoton(btn As Button, x As Integer, y As Integer, w As Integer, h As Integer, bgColor As Color, texto As String)
        If btn IsNot Nothing Then
            btn.Bounds = New Rectangle(x, y, w, h)
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 0
            btn.BackColor = bgColor
            btn.ForeColor = Color.White
            btn.Font = New Font("Segoe UI", 11, FontStyle.Bold)
            btn.Cursor = Cursors.Hand
            btn.Text = texto
        End If
    End Sub

    Private Sub CentrarPanelLogin()
        PanelLogin.Location = New Point((Me.ClientSize.Width - PanelLogin.Width) \ 2, (Me.ClientSize.Height - PanelLogin.Height) \ 2)
    End Sub

    Private Sub PagAcceso_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        CentrarPanelLogin()
    End Sub

    ' 3. LÓGICA DE INTERACCIÓN (ACCESOS Y OJITO)
    Private Sub ButtonVerPass_MouseDown(sender As Object, e As MouseEventArgs) Handles ButtonVerPasswd.MouseDown
        TextBoxPassword.UseSystemPasswordChar = False
    End Sub

    Private Sub ButtonVerPass_MouseUp(sender As Object, e As MouseEventArgs) Handles ButtonVerPasswd.MouseUp
        TextBoxPassword.UseSystemPasswordChar = True
    End Sub

    ' NUEVO: VERIFICACIÓN DE USUARIO Y GUARDADO DE CREDENCIALES
    Private Sub ButtonAcceso_Click(sender As Object, e As EventArgs) Handles ButtonAcceso.Click
        Dim usr As String = TextBoxUsuario.Text.Trim()
        Dim pwd As String = TextBoxPassword.Text.Trim()

        If String.IsNullOrWhiteSpace(usr) OrElse String.IsNullOrWhiteSpace(pwd) Then
            MessageBox.Show("Por favor, introduce usuario y contraseña.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Buscamos al usuario por nombre y leemos sus datos para verificar la contraseña en código
            ' (no en SQL), porque la contraseña está hasheada con salt único y no se puede comparar
            ' directamente con un WHERE.
            Dim sql As String = "SELECT ID_Usuario, Password, Rol, NombreCompleto, Activo FROM Usuarios WHERE NombreUsuario = @u LIMIT 1"
            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@u", usr)
                Using r = cmd.ExecuteReader()
                    If Not r.Read() Then
                        MessageBox.Show("Usuario o contraseña incorrectos. Revisa tus credenciales.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                    End If

                    Dim idUser As Integer = Convert.ToInt32(r("ID_Usuario"))
                    Dim passBD As String = If(IsDBNull(r("Password")), "", r("Password").ToString())
                    Dim rol As String = If(IsDBNull(r("Rol")), "", r("Rol").ToString())
                    Dim nombreCompleto As String = If(IsDBNull(r("NombreCompleto")), "", r("NombreCompleto").ToString())
                    Dim activo As Boolean = (Not IsDBNull(r("Activo"))) AndAlso Convert.ToInt32(r("Activo")) = 1
                    r.Close()

                    If Not activo Then
                        MessageBox.Show("Este usuario está desactivado. Contacta con el administrador.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                    End If

                    If Not PasswordHasher.Verificar(pwd, passBD) Then
                        MessageBox.Show("Usuario o contraseña incorrectos. Revisa tus credenciales.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                    End If

                    ' --- Si la contraseña en BD seguía en texto plano, la migramos a hash ahora ---
                    If PasswordHasher.NecesitaMigracion(passBD) Then
                        Try
                            Dim nuevoHash As String = PasswordHasher.Hashear(pwd)
                            Using cmdMig As New SQLiteCommand("UPDATE Usuarios SET Password=@p WHERE ID_Usuario=@id", c)
                                cmdMig.Parameters.AddWithValue("@p", nuevoHash)
                                cmdMig.Parameters.AddWithValue("@id", idUser)
                                cmdMig.ExecuteNonQuery()
                            End Using
                        Catch
                            ' Si falla la migración, no rompemos el login. Se reintentará en el próximo acceso.
                        End Try
                    End If

                    ' --- Actualizar UltimoAcceso ---
                    Try
                        Using cmdAcc As New SQLiteCommand("UPDATE Usuarios SET UltimoAcceso=@f WHERE ID_Usuario=@id", c)
                            cmdAcc.Parameters.AddWithValue("@f", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            cmdAcc.Parameters.AddWithValue("@id", idUser)
                            cmdAcc.ExecuteNonQuery()
                        End Using
                    Catch
                        ' No bloqueante.
                    End Try

                    ' --- Recordar SOLO el nombre de usuario, NUNCA la contraseña en disco ---
                    If CheckBoxRecordar.Checked Then
                        My.Settings.SavedUser = usr
                    Else
                        My.Settings.SavedUser = ""
                    End If
                    My.Settings.SavedPass = "" ' Por seguridad, nos aseguramos de NO dejar la contraseña guardada
                    My.Settings.Save()

                    ' --- Sesión completa ---
                    ComunSesionActual.Usuario = usr
                    ComunSesionActual.Contrasena = "" ' No guardamos la pass en memoria de la app
                    ComunSesionActual.IdUsuario = idUser
                    ComunSesionActual.Rol = rol
                    ComunSesionActual.NombreCompleto = nombreCompleto

                    Dim PagHome As New PagHome()
                    AddHandler PagHome.FormClosed, Sub(s, args) Me.Close()
                    PagHome.Show()
                    Me.Hide()
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Error al conectar con la base de datos: " & ex.Message, "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TextBoxPassword_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBoxPassword.KeyDown
        If e.KeyCode = Keys.Enter Then
            ButtonAcceso.PerformClick()
            e.SuppressKeyPress = True
        End If
    End Sub




End Class

' (MANTÉN AQUÍ DEBAJO TU CLASE PanelTransparente)
' PANEL SEMITRANSPARENTE CON LOGO AJUSTADO (ZOOM)
Public Class PanelTransparente
    Inherits Panel

    ' Opacidad (0-255)
    Private _opacidad As Integer = 150
    Public Property Opacidad As Integer
        Get
            Return _opacidad
        End Get
        Set(value As Integer)
            _opacidad = value
            Me.Invalidate()
        End Set
    End Property

    ' Imagen del Logo
    Private _logo As Image
    Public Property Logo As Image
        Get
            Return _logo
        End Get
        Set(value As Image)
            _logo = value
            Me.Invalidate() ' Si cambia la foto, repintamos
        End Set
    End Property



    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        ' No pintamos fondo estándar
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        ' 1. Pintar el fondo blanco semitransparente (si la opacidad es > 0)
        If _opacidad > 0 Then
            Using pincel As New SolidBrush(Color.FromArgb(_opacidad, 255, 255, 255))
                e.Graphics.FillRectangle(pincel, Me.ClientRectangle)
            End Using
        End If

        ' 2. Pintar el Logo con modo ZOOM
        If _logo IsNot Nothing Then
            e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            e.Graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality

            Dim ratioX As Double = Me.Width / _logo.Width
            Dim ratioY As Double = Me.Height / _logo.Height
            Dim ratio As Double = Math.Min(ratioX, ratioY)

            Dim nuevoAncho As Integer = CInt(_logo.Width * ratio)
            Dim nuevoAlto As Integer = CInt(_logo.Height * ratio)

            Dim posX As Integer = (Me.Width - nuevoAncho) \ 2
            Dim posY As Integer = (Me.Height - nuevoAlto) \ 2

            e.Graphics.DrawImage(_logo, posX, posY, nuevoAncho, nuevoAlto)
        End If
    End Sub
    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            ' WS_EX_COMPOSITED (0x02000000) - Pinta todos los controles de una vez 
            ' de abajo hacia arriba en una sola pasada de memoria.
            cp.ExStyle = cp.ExStyle Or &H2000000
            Return cp
        End Get
    End Property
End Class
