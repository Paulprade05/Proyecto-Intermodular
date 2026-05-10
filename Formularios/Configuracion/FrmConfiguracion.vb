Imports System.Data.SQLite
Imports System.IO
Imports System.Drawing.Imaging

Public Class FrmConfiguracion

    ' Controles UI principales
    Private tcConfig As New TabControl()
    Private tabEmpresa As New TabPage("1. Datos de Empresa")
    Private tabUsuarios As New TabPage("2. Usuarios y Permisos")
    Private tabBackups As New TabPage("3. Copias de Seguridad")
    Private WithEvents btnGuardar As New Button()

    ' --- NUEVO LAYOUT: SIDEBAR + PANELES DE CONTENIDO ---
    Private pnlSidebar As New Panel()
    Private pnlContenido As New Panel()
    Private pnlEmpresa As New Panel()
    Private pnlUsuarios As New Panel()
    Private pnlBackups As New Panel()
    Private WithEvents lblNavEmpresa As New Label()
    Private WithEvents lblNavUsuarios As New Label()
    Private WithEvents lblNavBackups As New Label()
    Private indicadorEmpresa As New Panel()
    Private indicadorUsuarios As New Panel()
    Private indicadorBackups As New Panel()

    ' Sección que se debe mostrar al terminar de cargar el formulario.
    ' Si IrAPestana() se llama ANTES de ShowDialog(), guardamos aquí el índice
    ' y lo aplicamos al final del Load. Si no se ha pedido nada, vale 0 (Empresa).
    Private _pestanaInicial As Integer = 0

    ' --- CONSTANTE DE TU BASE DE DATOS (¡CÁMBIALO POR EL TUYO!) ---
    ' --- CONSTANTES DE TU BASE DE DATOS ---
    Private Const NOMBRE_ARCHIVO_BD As String = "BD\Optima.db"
    Private ReadOnly RUTA_ACTUAL_BD As String = Path.Combine(Application.StartupPath, NOMBRE_ARCHIVO_BD)
    Private ReadOnly RUTA_PAPELERA As String = Path.Combine(Application.StartupPath, "PapeleraDB")

    ' NUEVO: La ruta estándar donde irán las copias si el usuario no elige otra
    Private ReadOnly RUTA_BACKUP_DEFECTO As String = Path.Combine(Application.StartupPath, "BackUp")

    ' --- Controles Pestaña 1 (Empresa) ---
    Private txtNombreFiscal, txtNombreComercial, txtCIF, txtDireccion As New TextBox()
    Private txtPoblacion, txtCP, txtProvincia, txtTelefono, txtEmail, txtWeb, txtRegistro As New TextBox()
    Private picLogo As New PictureBox()
    Private WithEvents btnCargarLogo As New Button()

    ' --- Controles Pestaña 2 (Usuarios) ---
    Private WithEvents dgvUsuarios As New DataGridView()
    Private txtIdUsuario, txtUsername, txtPassword, txtNombreCompleto, txtEmailUsuario As New TextBox()
    Private cboRol As New ComboBox()
    Private chkActivo As New CheckBox()
    Private WithEvents btnNuevoUsuario, btnGuardarUsuario, btnEliminarUsuario As New Button()
    Private _dtUsuarios As New DataTable()

    ' --- Controles Pestaña 3 (Backups) ---
    Private txtRutaBackup As New TextBox()
    Private WithEvents btnExaminarRuta As New Button()
    Private chkAutoBackup As New CheckBox()
    Private WithEvents cboFrecuencia As New ComboBox()
    Private dtpHoraBackup As New DateTimePicker()
    Private chkDias As New CheckedListBox()
    Private WithEvents btnHacerCopia As New Button()
    Private WithEvents btnRestaurarCopia As New Button()
    Private WithEvents btnVaciarPapelera As New Button()

    Private Sub FrmConfiguracion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Configuración del Sistema"
        Me.Size = New Size(1000, 760)
        Me.MinimumSize = New Size(950, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = COLOR_FONDO

        ' Creamos las carpetas base si no existen
        If Not Directory.Exists(RUTA_PAPELERA) Then Directory.CreateDirectory(RUTA_PAPELERA)
        If Not Directory.Exists(RUTA_BACKUP_DEFECTO) Then Directory.CreateDirectory(RUTA_BACKUP_DEFECTO)

        ConstruirInterfaz()
        CargarDatosGenerales()
        CargarListaUsuarios()
    End Sub

    ' Cambia a la sección indicada (0=Empresa, 1=Usuarios, 2=Copias de Seguridad). Es seguro llamarla ANTES de ShowDialog: en ese caso guarda el índice y se aplicará al terminar de cargar la interfaz. Si se llama DESPUÉS (formulario ya visible), cambia de sección al instante.
    Public Sub IrAPestana(indice As Integer)
        ' Guardamos siempre el índice — así, aunque el formulario aún no esté cargado,
        ' al final del Load se aplicará el panel correcto.
        _pestanaInicial = indice

        ' Si el formulario ya está construido (Handle creado y panel visible existe),
        ' cambiamos de sección al instante. Si todavía no, el Load se encargará.
        If Me.IsHandleCreated AndAlso pnlEmpresa.Parent IsNot Nothing Then
            AplicarPestanaInicial()
        End If
    End Sub

    ' Aplica la sección guardada en _pestanaInicial. Se llama desde el final de
    ' ConstruirInterfaz y desde IrAPestana cuando el formulario ya está visible.
    Private Sub AplicarPestanaInicial()
        Select Case _pestanaInicial
            Case 1 : MostrarSeccion(pnlUsuarios, lblNavUsuarios, indicadorUsuarios)
            Case 2 : MostrarSeccion(pnlBackups, lblNavBackups, indicadorBackups)
            Case Else : MostrarSeccion(pnlEmpresa, lblNavEmpresa, indicadorEmpresa)
        End Select
    End Sub

    ' 1. DISEÑO DE LA INTERFAZ - Rediseño con Sidebar + Tarjetas

    ' --- PALETA DE COLORES (alineada con el resto de módulos) ---
    Private Shared ReadOnly COLOR_FONDO As Color = Color.FromArgb(70, 75, 80)
    Private Shared ReadOnly COLOR_SIDEBAR As Color = Color.FromArgb(40, 50, 63)
    Private Shared ReadOnly COLOR_SIDEBAR_HOVER As Color = Color.FromArgb(50, 62, 78)
    Private Shared ReadOnly COLOR_TARJETA As Color = Color.FromArgb(56, 62, 70)
    Private Shared ReadOnly COLOR_TARJETA_BORDE As Color = Color.FromArgb(80, 88, 98)
    Private Shared ReadOnly COLOR_INPUT_FONDO As Color = Color.FromArgb(60, 66, 74)
    Private Shared ReadOnly COLOR_INPUT_BORDE As Color = Color.FromArgb(95, 105, 118)
    Private Shared ReadOnly COLOR_ACENTO As Color = Color.FromArgb(0, 150, 255)
    Private Shared ReadOnly COLOR_TEXTO_SECUNDARIO As Color = Color.FromArgb(170, 180, 195)
    Private Shared ReadOnly COLOR_LINEA As Color = Color.FromArgb(70, 80, 92)

    ' --- COLORES SEMÁNTICOS DE BOTONES ---
    Private Shared ReadOnly BTN_AZUL_PRIMARIO As Color = Color.FromArgb(0, 120, 215)
    Private Shared ReadOnly BTN_ROJO_PELIGRO As Color = Color.FromArgb(209, 52, 56)
    Private Shared ReadOnly BTN_VERDE_AÑADIR As Color = Color.FromArgb(40, 140, 90)
    Private Shared ReadOnly BTN_GRIS_NEUTRO As Color = Color.FromArgb(85, 85, 85)

    Private Sub ConstruirInterfaz()
        Me.Controls.Clear()
        Me.BackColor = COLOR_FONDO

        Dim anchoSidebar As Integer = 220

        ' 1. SIDEBAR IZQUIERDA
        pnlSidebar.BackColor = COLOR_SIDEBAR
        pnlSidebar.Bounds = New Rectangle(0, 0, anchoSidebar, Me.ClientSize.Height)
        pnlSidebar.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        Me.Controls.Add(pnlSidebar)

        ' --- Cabecera de la sidebar ---
        Dim lblTituloSidebar As New Label With {
            .Text = "Configuración",
            .Font = New Font("Segoe UI", 15, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = COLOR_SIDEBAR,
            .AutoSize = False,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Bounds = New Rectangle(20, 22, anchoSidebar - 40, 28)
        }
        pnlSidebar.Controls.Add(lblTituloSidebar)

        Dim lblSubtituloSidebar As New Label With {
            .Text = "Ajustes del sistema",
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Regular),
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_SIDEBAR,
            .AutoSize = False,
            .Bounds = New Rectangle(20, 50, anchoSidebar - 40, 18)
        }
        pnlSidebar.Controls.Add(lblSubtituloSidebar)

        ' --- Línea separadora bajo cabecera ---
        Dim sepCabecera As New Panel With {
            .BackColor = Color.FromArgb(60, 70, 85),
            .Bounds = New Rectangle(20, 82, anchoSidebar - 40, 1)
        }
        pnlSidebar.Controls.Add(sepCabecera)

        ' --- Items de menú de la sidebar ---
        Dim yMenu As Integer = 100
        ConfigurarItemSidebar(lblNavEmpresa, indicadorEmpresa, "Empresa", yMenu, 0)
        ConfigurarItemSidebar(lblNavUsuarios, indicadorUsuarios, "Usuarios", yMenu + 46, 1)
        ConfigurarItemSidebar(lblNavBackups, indicadorBackups, "Copias de seguridad", yMenu + 92, 2)

        ' --- Botón Guardar fijado al pie de la sidebar ---
        btnGuardar.Text = "Guardar cambios"
        btnGuardar.Size = New Size(anchoSidebar - 40, 38)
        btnGuardar.Location = New Point(20, Me.ClientSize.Height - 60)
        btnGuardar.BackColor = BTN_VERDE_AÑADIR
        btnGuardar.ForeColor = Color.White
        btnGuardar.FlatStyle = FlatStyle.Flat
        btnGuardar.FlatAppearance.BorderSize = 0
        btnGuardar.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        btnGuardar.Cursor = Cursors.Hand
        btnGuardar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlSidebar.Controls.Add(btnGuardar)

        ' Línea sobre el botón Guardar
        Dim sepGuardar As New Panel With {
            .BackColor = Color.FromArgb(60, 70, 85),
            .Bounds = New Rectangle(20, Me.ClientSize.Height - 75, anchoSidebar - 40, 1),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }
        pnlSidebar.Controls.Add(sepGuardar)

        ' 2. ZONA DE CONTENIDO (a la derecha de la sidebar)
        pnlContenido.BackColor = COLOR_FONDO
        pnlContenido.Bounds = New Rectangle(anchoSidebar, 0, Me.ClientSize.Width - anchoSidebar, Me.ClientSize.Height)
        pnlContenido.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlContenido.AutoScroll = True
        Me.Controls.Add(pnlContenido)

        ' Construimos cada sección dentro de su panel
        ConstruirSeccionEmpresa()
        ConstruirSeccionUsuarios()
        ConstruirSeccionBackups()

        ' Mostramos la sección que se haya pedido (por defecto, Empresa).
        ' Si alguien llamó a IrAPestana() antes de ShowDialog(), aquí se aplica el índice
        ' que se guardó en _pestanaInicial.
        AplicarPestanaInicial()
    End Sub

    ' --- Helper: configurar un item del menú lateral ---
    Private Sub ConfigurarItemSidebar(lbl As Label, indicador As Panel, texto As String, y As Integer, indice As Integer)
        ' Indicador vertical azul (visible solo cuando está activo)
        indicador.BackColor = COLOR_ACENTO
        indicador.Bounds = New Rectangle(0, y, 3, 36)
        indicador.Visible = False
        pnlSidebar.Controls.Add(indicador)

        ' Etiqueta del item
        lbl.Text = texto
        lbl.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
        lbl.ForeColor = COLOR_TEXTO_SECUNDARIO
        lbl.BackColor = COLOR_SIDEBAR
        lbl.AutoSize = False
        lbl.TextAlign = ContentAlignment.MiddleLeft
        lbl.Padding = New Padding(20, 0, 10, 0)
        lbl.Bounds = New Rectangle(0, y, 220, 36)
        lbl.Cursor = Cursors.Hand
        lbl.Tag = indice
        pnlSidebar.Controls.Add(lbl)
    End Sub

    ' --- Cambia de sección (oculta los demás paneles, marca el item activo) ---
    Private Sub MostrarSeccion(panelActivo As Panel, lblActivo As Label, indicadorActivo As Panel)
        If pnlEmpresa IsNot Nothing Then pnlEmpresa.Visible = (panelActivo Is pnlEmpresa)
        If pnlUsuarios IsNot Nothing Then pnlUsuarios.Visible = (panelActivo Is pnlUsuarios)
        If pnlBackups IsNot Nothing Then pnlBackups.Visible = (panelActivo Is pnlBackups)

        ' Reset look de los items
        ResetItemSidebar(lblNavEmpresa, indicadorEmpresa)
        ResetItemSidebar(lblNavUsuarios, indicadorUsuarios)
        ResetItemSidebar(lblNavBackups, indicadorBackups)

        ' Marca el activo
        lblActivo.BackColor = COLOR_SIDEBAR_HOVER
        lblActivo.ForeColor = Color.White
        lblActivo.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        indicadorActivo.Visible = True
    End Sub

    Private Sub ResetItemSidebar(lbl As Label, indicador As Panel)
        lbl.BackColor = COLOR_SIDEBAR
        lbl.ForeColor = COLOR_TEXTO_SECUNDARIO
        lbl.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
        indicador.Visible = False
    End Sub

    ' --- Handlers de click en los items de la sidebar ---
    Private Sub lblNavEmpresa_Click(sender As Object, e As EventArgs) Handles lblNavEmpresa.Click
        MostrarSeccion(pnlEmpresa, lblNavEmpresa, indicadorEmpresa)
    End Sub

    Private Sub lblNavUsuarios_Click(sender As Object, e As EventArgs) Handles lblNavUsuarios.Click
        MostrarSeccion(pnlUsuarios, lblNavUsuarios, indicadorUsuarios)
    End Sub

    Private Sub lblNavBackups_Click(sender As Object, e As EventArgs) Handles lblNavBackups.Click
        MostrarSeccion(pnlBackups, lblNavBackups, indicadorBackups)
    End Sub

    ' HELPERS DE CABECERA Y TARJETAS

    ' Cabecera grande de cada sección (título + subtítulo)
    Private Sub AnyadirCabeceraSeccion(panelPadre As Panel, titulo As String, subtitulo As String)
        Dim lblTit As New Label With {
            .Text = titulo,
            .Font = New Font("Segoe UI", 18.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = COLOR_FONDO,
            .AutoSize = False,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Bounds = New Rectangle(30, 25, 700, 32)
        }
        panelPadre.Controls.Add(lblTit)

        Dim lblSub As New Label With {
            .Text = subtitulo,
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Regular),
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_FONDO,
            .AutoSize = False,
            .Bounds = New Rectangle(30, 58, 700, 18)
        }
        panelPadre.Controls.Add(lblSub)
    End Sub

    ' Crea una tarjeta con título y la añade al panel padre. Devuelve la tarjeta para añadirle controles dentro
    Private Function CrearTarjeta(panelPadre As Panel, titulo As String, subtitulo As String, x As Integer, y As Integer, w As Integer, h As Integer) As Panel
        Dim tarjeta As New Panel With {
            .BackColor = COLOR_TARJETA,
            .BorderStyle = BorderStyle.FixedSingle,
            .Bounds = New Rectangle(x, y, w, h)
        }
        panelPadre.Controls.Add(tarjeta)

        ' Título de la tarjeta
        Dim lblTit As New Label With {
            .Text = titulo,
            .Font = New Font("Segoe UI Semibold", 10.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = COLOR_TARJETA,
            .AutoSize = False,
            .Bounds = New Rectangle(16, 14, w - 32, 20)
        }
        tarjeta.Controls.Add(lblTit)

        ' Subtítulo de la tarjeta (descripción breve)
        If Not String.IsNullOrEmpty(subtitulo) Then
            Dim lblSub As New Label With {
                .Text = subtitulo,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Regular),
                .ForeColor = COLOR_TEXTO_SECUNDARIO,
                .BackColor = COLOR_TARJETA,
                .AutoSize = False,
                .Bounds = New Rectangle(16, 34, w - 32, 16)
            }
            tarjeta.Controls.Add(lblSub)
        End If

        Return tarjeta
    End Function

    ' Añade un campo (etiqueta + textbox) dentro de una tarjeta con estilo input oscuro
    Private Sub AnyadirCampoEnTarjeta(tarjeta As Panel, textoLabel As String, txt As TextBox, x As Integer, y As Integer, w As Integer)
        Dim lbl As New Label With {
            .Text = textoLabel,
            .Location = New Point(x, y),
            .AutoSize = True,
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_TARJETA,
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Regular)
        }
        tarjeta.Controls.Add(lbl)

        txt.Bounds = New Rectangle(x, y + 18, w, 28)
        txt.Font = New Font("Segoe UI", 10.0F)
        txt.BackColor = COLOR_INPUT_FONDO
        txt.ForeColor = Color.White
        txt.BorderStyle = BorderStyle.FixedSingle
        tarjeta.Controls.Add(txt)
    End Sub

    ' Estiliza un combo con look oscuro coherente
    Private Sub EstilizarCombo(cbo As ComboBox)
        cbo.BackColor = COLOR_INPUT_FONDO
        cbo.ForeColor = Color.White
        cbo.FlatStyle = FlatStyle.Flat
        cbo.Font = New Font("Segoe UI", 10.0F)
    End Sub

    ' Estilo moderno de botón unificado
    Private Sub EstilizarBoton(btn As Button, texto As String, x As Integer, y As Integer, bg As Color, fg As Color, Optional ancho As Integer = 110, Optional alto As Integer = 34)
        btn.Text = texto
        btn.Location = New Point(x, y)
        btn.Size = New Size(ancho, alto)
        btn.FlatStyle = FlatStyle.Flat
        btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        btn.Cursor = Cursors.Hand
        btn.BackColor = bg
        btn.ForeColor = fg
        btn.FlatAppearance.BorderSize = 0
    End Sub

    ' SECCIÓN 1: EMPRESA
    Private Sub ConstruirSeccionEmpresa()
        pnlEmpresa.BackColor = COLOR_FONDO
        pnlEmpresa.Bounds = New Rectangle(0, 0, pnlContenido.ClientSize.Width, pnlContenido.ClientSize.Height)
        pnlEmpresa.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlContenido.Controls.Add(pnlEmpresa)

        AnyadirCabeceraSeccion(pnlEmpresa, "Datos de empresa", "Información fiscal, comercial y de contacto utilizada en facturas y documentos")

        ' --- TARJETA 1: Datos fiscales y comerciales ---
        Dim tFiscal As Panel = CrearTarjeta(pnlEmpresa, "Datos fiscales y comerciales", "Razón social, identificación fiscal y nombre comercial", 30, 100, 720, 250)
        AnyadirCampoEnTarjeta(tFiscal, "NOMBRE FISCAL", txtNombreFiscal, 16, 60, 320)
        AnyadirCampoEnTarjeta(tFiscal, "NOMBRE COMERCIAL", txtNombreComercial, 350, 60, 220)
        AnyadirCampoEnTarjeta(tFiscal, "CIF / NIF", txtCIF, 580, 60, 120)
        AnyadirCampoEnTarjeta(tFiscal, "DIRECCIÓN COMPLETA", txtDireccion, 16, 130, 380)
        AnyadirCampoEnTarjeta(tFiscal, "POBLACIÓN", txtPoblacion, 410, 130, 200)
        AnyadirCampoEnTarjeta(tFiscal, "C.P.", txtCP, 625, 130, 75)
        AnyadirCampoEnTarjeta(tFiscal, "PROVINCIA", txtProvincia, 16, 200, 180)
        AnyadirCampoEnTarjeta(tFiscal, "TELÉFONO", txtTelefono, 210, 200, 180)
        AnyadirCampoEnTarjeta(tFiscal, "EMAIL", txtEmail, 405, 200, 295)

        ' --- TARJETA 2: Web y registro mercantil ---
        Dim tWeb As Panel = CrearTarjeta(pnlEmpresa, "Web y registro mercantil", "Datos adicionales que aparecen en el pie de las facturas", 30, 365, 720, 110)
        AnyadirCampoEnTarjeta(tWeb, "SITIO WEB", txtWeb, 16, 60, 280)
        AnyadirCampoEnTarjeta(tWeb, "DATOS REGISTRALES (Para pie de facturas)", txtRegistro, 310, 60, 390)

        ' --- TARJETA 3: Identidad corporativa (logo) ---
        Dim tLogo As Panel = CrearTarjeta(pnlEmpresa, "Identidad corporativa", "Logo que aparecerá impreso en presupuestos y facturas", 30, 490, 720, 170)

        picLogo.Bounds = New Rectangle(16, 60, 220, 90)
        picLogo.BorderStyle = BorderStyle.FixedSingle
        picLogo.SizeMode = PictureBoxSizeMode.Zoom
        picLogo.BackColor = COLOR_INPUT_FONDO
        tLogo.Controls.Add(picLogo)

        Dim lblLogoHint As New Label With {
            .Text = "Sube una imagen PNG o JPG. Se redimensionará automáticamente al imprimir.",
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_TARJETA,
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Regular),
            .AutoSize = False,
            .Bounds = New Rectangle(255, 60, 440, 32)
        }
        tLogo.Controls.Add(lblLogoHint)

        EstilizarBoton(btnCargarLogo, "Examinar imagen…", 255, 100, BTN_AZUL_PRIMARIO, Color.White, 170, 34)
        tLogo.Controls.Add(btnCargarLogo)
    End Sub

    ' SECCIÓN 2: USUARIOS
    Private Sub ConstruirSeccionUsuarios()
        pnlUsuarios.BackColor = COLOR_FONDO
        pnlUsuarios.Bounds = New Rectangle(0, 0, pnlContenido.ClientSize.Width, pnlContenido.ClientSize.Height)
        pnlUsuarios.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlContenido.Controls.Add(pnlUsuarios)

        AnyadirCabeceraSeccion(pnlUsuarios, "Usuarios y permisos", "Gestiona quién puede acceder al sistema y con qué rol")

        ' --- TARJETA 1: Listado de usuarios ---
        Dim tLista As Panel = CrearTarjeta(pnlUsuarios, "Usuarios del sistema", "Selecciona un usuario para editar sus datos", 30, 100, 720, 240)

        dgvUsuarios.Bounds = New Rectangle(16, 60, 688, 165)
        dgvUsuarios.AllowUserToAddRows = False
        dgvUsuarios.ReadOnly = True
        dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvUsuarios.RowHeadersVisible = False
        dgvUsuarios.BackgroundColor = COLOR_TARJETA
        dgvUsuarios.BorderStyle = BorderStyle.None
        Try : FrmPresupuestos.EstilizarGrid(dgvUsuarios) : Catch : End Try
        tLista.Controls.Add(dgvUsuarios)

        ' --- TARJETA 2: Ficha del usuario ---
        Dim tFicha As Panel = CrearTarjeta(pnlUsuarios, "Ficha del usuario", "Datos del usuario seleccionado o nuevo registro", 30, 355, 720, 260)

        txtIdUsuario.Visible = False
        tFicha.Controls.Add(txtIdUsuario)

        AnyadirCampoEnTarjeta(tFicha, "USUARIO (LOGIN)", txtUsername, 16, 60, 200)
        AnyadirCampoEnTarjeta(tFicha, "CONTRASEÑA", txtPassword, 230, 60, 200)
        txtPassword.UseSystemPasswordChar = True

        ' Rol (combo) — lo monto manualmente porque AnyadirCampoEnTarjeta es para TextBox
        Dim lblRol As New Label With {
            .Text = "ROL DEL SISTEMA",
            .Location = New Point(444, 60),
            .AutoSize = True,
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_TARJETA,
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Regular)
        }
        tFicha.Controls.Add(lblRol)

        cboRol.Bounds = New Rectangle(444, 78, 200, 28)
        cboRol.DropDownStyle = ComboBoxStyle.DropDownList
        cboRol.Items.Clear()
        cboRol.Items.AddRange({"Administrador", "Usuario"})
        EstilizarCombo(cboRol)
        tFicha.Controls.Add(cboRol)

        ' Cuenta activa
        chkActivo.Text = "Cuenta activa"
        chkActivo.Bounds = New Rectangle(16, 130, 200, 24)
        chkActivo.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        chkActivo.ForeColor = Color.White
        chkActivo.BackColor = COLOR_TARJETA
        chkActivo.Checked = True
        tFicha.Controls.Add(chkActivo)

        AnyadirCampoEnTarjeta(tFicha, "NOMBRE COMPLETO", txtNombreCompleto, 16, 165, 320)
        AnyadirCampoEnTarjeta(tFicha, "CORREO ELECTRÓNICO", txtEmailUsuario, 350, 165, 340)

        ' Botones de acción
        EstilizarBoton(btnNuevoUsuario, "+ Nuevo", 16, 215, BTN_AZUL_PRIMARIO, Color.White, 100, 32)
        tFicha.Controls.Add(btnNuevoUsuario)
        EstilizarBoton(btnGuardarUsuario, "Guardar usuario", 124, 215, BTN_VERDE_AÑADIR, Color.White, 140, 32)
        tFicha.Controls.Add(btnGuardarUsuario)
        EstilizarBoton(btnEliminarUsuario, "Eliminar", 272, 215, BTN_ROJO_PELIGRO, Color.White, 100, 32)
        tFicha.Controls.Add(btnEliminarUsuario)
    End Sub

    ' SECCIÓN 3: BACKUPS
    Private Sub ConstruirSeccionBackups()
        pnlBackups.BackColor = COLOR_FONDO
        pnlBackups.Bounds = New Rectangle(0, 0, pnlContenido.ClientSize.Width, pnlContenido.ClientSize.Height)
        pnlBackups.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlContenido.Controls.Add(pnlBackups)

        AnyadirCabeceraSeccion(pnlBackups, "Copias de seguridad", "Realiza copias manuales, programa copias automáticas o restaura una BD")

        ' --- TARJETA 1: Copia manual y restauración ---
        Dim tManual As Panel = CrearTarjeta(pnlBackups, "Copia manual y restauración", "Realiza una copia inmediata o restaura desde un archivo existente", 30, 100, 720, 200)

        AnyadirCampoEnTarjeta(tManual, "CARPETA DESTINO DE LAS COPIAS", txtRutaBackup, 16, 60, 480)

        EstilizarBoton(btnExaminarRuta, "Examinar…", 510, 78, BTN_GRIS_NEUTRO, Color.White, 100, 28)
        tManual.Controls.Add(btnExaminarRuta)

        EstilizarBoton(btnHacerCopia, "💾  HACER COPIA AHORA", 16, 135, BTN_AZUL_PRIMARIO, Color.White, 220, 42)
        btnHacerCopia.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        tManual.Controls.Add(btnHacerCopia)

        EstilizarBoton(btnRestaurarCopia, "🔄  RESTAURAR COPIA (PELIGRO)", 250, 135, BTN_ROJO_PELIGRO, Color.White, 270, 42)
        btnRestaurarCopia.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        tManual.Controls.Add(btnRestaurarCopia)

        ' --- TARJETA 2: Copias automáticas ---
        Dim tAuto As Panel = CrearTarjeta(pnlBackups, "Copias automáticas", "Programa copias periódicas mientras el programa esté abierto", 30, 315, 720, 240)

        ' Toggle (CheckBox tipo botón rectangular) en la esquina superior derecha de la tarjeta
        chkAutoBackup.Text = ""
        chkAutoBackup.Appearance = Appearance.Button
        chkAutoBackup.FlatStyle = FlatStyle.Flat
        chkAutoBackup.FlatAppearance.BorderSize = 0
        chkAutoBackup.Bounds = New Rectangle(640, 18, 60, 24)
        chkAutoBackup.BackColor = COLOR_INPUT_FONDO
        chkAutoBackup.TextAlign = ContentAlignment.MiddleCenter
        chkAutoBackup.UseVisualStyleBackColor = False
        AddHandler chkAutoBackup.CheckedChanged, AddressOf chkAutoBackup_CheckedChanged
        tAuto.Controls.Add(chkAutoBackup)
        ' Aplicamos el estado visual inicial
        ActualizarToggleAuto()

        ' Etiqueta "Frecuencia"
        Dim lblFrec As New Label With {
            .Text = "FRECUENCIA",
            .Location = New Point(16, 70),
            .AutoSize = True,
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_TARJETA,
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Regular)
        }
        tAuto.Controls.Add(lblFrec)

        cboFrecuencia.Bounds = New Rectangle(16, 88, 160, 28)
        cboFrecuencia.DropDownStyle = ComboBoxStyle.DropDownList
        cboFrecuencia.Items.Clear()
        cboFrecuencia.Items.AddRange({"Diaria", "Semanal"})
        cboFrecuencia.SelectedIndex = 0
        EstilizarCombo(cboFrecuencia)
        tAuto.Controls.Add(cboFrecuencia)

        ' Etiqueta "Hora"
        Dim lblHora As New Label With {
            .Text = "HORA DE EJECUCIÓN",
            .Location = New Point(190, 70),
            .AutoSize = True,
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_TARJETA,
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Regular)
        }
        tAuto.Controls.Add(lblHora)

        dtpHoraBackup.Bounds = New Rectangle(190, 88, 110, 28)
        dtpHoraBackup.Format = DateTimePickerFormat.Time
        dtpHoraBackup.ShowUpDown = True
        tAuto.Controls.Add(dtpHoraBackup)

        ' Etiqueta "Días"
        Dim lblDias As New Label With {
            .Text = "DÍAS DE LA SEMANA (si es semanal)",
            .Location = New Point(330, 70),
            .AutoSize = True,
            .ForeColor = COLOR_TEXTO_SECUNDARIO,
            .BackColor = COLOR_TARJETA,
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Regular)
        }
        tAuto.Controls.Add(lblDias)

        chkDias.Bounds = New Rectangle(330, 88, 370, 130)
        chkDias.Items.Clear()
        chkDias.Items.AddRange({"Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"})
        chkDias.CheckOnClick = True
        chkDias.Enabled = False
        chkDias.MultiColumn = True
        chkDias.ColumnWidth = 120
        chkDias.BackColor = COLOR_INPUT_FONDO
        chkDias.ForeColor = Color.White
        chkDias.BorderStyle = BorderStyle.FixedSingle
        tAuto.Controls.Add(chkDias)

        ' --- TARJETA 3: Mantenimiento ---
        Dim tMant As Panel = CrearTarjeta(pnlBackups, "Mantenimiento", "Al restaurar, la BD antigua se conserva en una papelera por seguridad", 30, 570, 720, 110)

        EstilizarBoton(btnVaciarPapelera, "🗑️  Vaciar papelera de bases de datos (" & ObtenerTamanoPapelera() & ")", 16, 60, BTN_GRIS_NEUTRO, Color.White, 400, 36)
        tMant.Controls.Add(btnVaciarPapelera)
    End Sub

    ' Repinta el toggle "Activar copias automáticas" según su estado
    Private Sub ActualizarToggleAuto()
        If chkAutoBackup.Checked Then
            chkAutoBackup.Text = "ACTIVO"
            chkAutoBackup.BackColor = COLOR_ACENTO
            chkAutoBackup.ForeColor = Color.White
        Else
            chkAutoBackup.Text = "OFF"
            chkAutoBackup.BackColor = COLOR_INPUT_FONDO
            chkAutoBackup.ForeColor = COLOR_TEXTO_SECUNDARIO
        End If
        chkAutoBackup.Font = New Font("Segoe UI", 8.0F, FontStyle.Bold)
    End Sub

    Private Sub chkAutoBackup_CheckedChanged(sender As Object, e As EventArgs)
        ActualizarToggleAuto()
    End Sub

    ' EVENTOS UI SECUNDARIOS
    Private Sub cboFrecuencia_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFrecuencia.SelectedIndexChanged
        chkDias.Enabled = (cboFrecuencia.Text = "Semanal")
    End Sub

    Private Sub btnCargarLogo_Click(sender As Object, e As EventArgs) Handles btnCargarLogo.Click
        Dim opf As New OpenFileDialog() With {.Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"}
        If opf.ShowDialog() = DialogResult.OK Then
            Try : picLogo.Image = Image.FromFile(opf.FileName) : Catch : MessageBox.Show("Error al cargar.") : End Try
        End If
    End Sub

    Private Function ImageToBytes(img As Image) As Byte()
        If img Is Nothing Then Return Nothing
        Using ms As New MemoryStream()
            img.Save(ms, ImageFormat.Png)
            Return ms.ToArray()
        End Using
    End Function

    Private Function BytesToImage(b As Byte()) As Image
        If b Is Nothing OrElse b.Length = 0 Then Return Nothing
        Using ms As New MemoryStream(b)
            Return Image.FromStream(ms)
        End Using
    End Function

    ' LÓGICA DE USUARIOS (CRUD)
    Private Sub CargarListaUsuarios()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String = "SELECT ID_Usuario, NombreUsuario, Password, Rol, NombreCompleto, Email, Activo, FechaRegistro FROM Usuarios"
            Using cmd As New SQLiteCommand(sql, c)
                Dim da As New SQLiteDataAdapter(cmd)
                _dtUsuarios.Clear()
                da.Fill(_dtUsuarios)
            End Using

            dgvUsuarios.DataSource = _dtUsuarios

            If dgvUsuarios.Columns.Count > 0 Then
                dgvUsuarios.Columns("ID_Usuario").Visible = False
                dgvUsuarios.Columns("Password").Visible = False
                dgvUsuarios.Columns("NombreUsuario").HeaderText = "Login"
                dgvUsuarios.Columns("NombreUsuario").Width = 100
                dgvUsuarios.Columns("NombreCompleto").HeaderText = "Nombre Completo"
                dgvUsuarios.Columns("NombreCompleto").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                dgvUsuarios.Columns("Rol").Width = 120
                dgvUsuarios.Columns("Email").Width = 200
                dgvUsuarios.Columns("Activo").Width = 60
                dgvUsuarios.Columns("FechaRegistro").HeaderText = "F. Registro"
                dgvUsuarios.Columns("FechaRegistro").Width = 120
            End If
        Catch ex As Exception
            MessageBox.Show("Error al cargar usuarios: " & ex.Message)
        End Try
    End Sub

    Private Sub dgvUsuarios_SelectionChanged(sender As Object, e As EventArgs) Handles dgvUsuarios.SelectionChanged
        If dgvUsuarios.CurrentRow IsNot Nothing Then
            Dim row As DataGridViewRow = dgvUsuarios.CurrentRow
            txtIdUsuario.Text = row.Cells("ID_Usuario").Value.ToString()
            txtUsername.Text = If(IsDBNull(row.Cells("NombreUsuario").Value), "", row.Cells("NombreUsuario").Value.ToString())
            ' La contraseña en BD está hasheada y no es recuperable. Dejamos el campo vacío:
            ' si el admin lo deja en blanco, NO se cambia. Si escribe algo, se sustituye por el hash de eso.
            txtPassword.Text = ""
            txtPassword.PlaceholderText = "(dejar en blanco para no cambiar)"
            cboRol.Text = If(IsDBNull(row.Cells("Rol").Value), "Usuario", row.Cells("Rol").Value.ToString())
            txtNombreCompleto.Text = If(IsDBNull(row.Cells("NombreCompleto").Value), "", row.Cells("NombreCompleto").Value.ToString())
            txtEmailUsuario.Text = If(IsDBNull(row.Cells("Email").Value), "", row.Cells("Email").Value.ToString())
            chkActivo.Checked = If(IsDBNull(row.Cells("Activo").Value), False, Convert.ToBoolean(row.Cells("Activo").Value))
        End If
    End Sub

    Private Sub btnNuevoUsuario_Click(sender As Object, e As EventArgs) Handles btnNuevoUsuario.Click
        ' Solo administradores pueden crear/editar usuarios.
        If Not ComunSesionActual.EsAdministrador() Then
            MessageBox.Show("Esta acción está restringida a usuarios con rol Administrador.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        dgvUsuarios.ClearSelection()
        txtIdUsuario.Text = "" : txtUsername.Text = "" : txtPassword.Text = "" : txtNombreCompleto.Text = "" : txtEmailUsuario.Text = ""
        cboRol.SelectedIndex = 1 : chkActivo.Checked = True : txtUsername.Focus()
    End Sub

    Private Sub btnGuardarUsuario_Click(sender As Object, e As EventArgs) Handles btnGuardarUsuario.Click
        ' Solo administradores pueden guardar usuarios.
        If Not ComunSesionActual.EsAdministrador() Then
            MessageBox.Show("Esta acción está restringida a usuarios con rol Administrador.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim esEdicion As Boolean = Not String.IsNullOrEmpty(txtIdUsuario.Text)

        ' En alta, login y pass son obligatorios. En edición, solo el login (la pass se mantiene si está vacía).
        If String.IsNullOrWhiteSpace(txtUsername.Text) Then
            MessageBox.Show("El Login es obligatorio.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If
        If Not esEdicion AndAlso String.IsNullOrWhiteSpace(txtPassword.Text) Then
            MessageBox.Show("La contraseña es obligatoria al crear un usuario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If

        ' Validamos el email del usuario (si lo ha rellenado). No bloquea: solo avisa.
        If Not Validador.ConfirmarSiHayProblemas(
            ("Email del usuario", Validador.ValidarEmail(txtEmailUsuario.Text))
        ) Then
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim cmd As New SQLiteCommand(c)

            Dim cambiaPassword As Boolean = Not String.IsNullOrWhiteSpace(txtPassword.Text)
            Dim passwordHasheada As String = ""
            If cambiaPassword Then passwordHasheada = PasswordHasher.Hashear(txtPassword.Text.Trim())

            If Not esEdicion Then
                cmd.CommandText = "INSERT INTO Usuarios (NombreUsuario, Password, Rol, NombreCompleto, Email, Activo, FechaRegistro) VALUES (@user, @pass, @rol, @nom, @email, @act, @fecha)"
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                cmd.Parameters.AddWithValue("@pass", passwordHasheada)
            Else
                If cambiaPassword Then
                    cmd.CommandText = "UPDATE Usuarios SET NombreUsuario=@user, Password=@pass, Rol=@rol, NombreCompleto=@nom, Email=@email, Activo=@act WHERE ID_Usuario=@id"
                    cmd.Parameters.AddWithValue("@pass", passwordHasheada)
                Else
                    ' Pass en blanco -> NO la cambiamos
                    cmd.CommandText = "UPDATE Usuarios SET NombreUsuario=@user, Rol=@rol, NombreCompleto=@nom, Email=@email, Activo=@act WHERE ID_Usuario=@id"
                End If
                cmd.Parameters.AddWithValue("@id", txtIdUsuario.Text)
            End If

            cmd.Parameters.AddWithValue("@user", txtUsername.Text.Trim())
            cmd.Parameters.AddWithValue("@rol", cboRol.Text)
            cmd.Parameters.AddWithValue("@nom", txtNombreCompleto.Text.Trim())
            cmd.Parameters.AddWithValue("@email", txtEmailUsuario.Text.Trim())
            cmd.Parameters.AddWithValue("@act", If(chkActivo.Checked, 1, 0))

            cmd.ExecuteNonQuery()
            CargarListaUsuarios()
            MessageBox.Show("Usuario guardado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            LogErrores.Registrar("FrmConfiguracion.GuardarUsuario", ex)
            MessageBox.Show("Error al guardar el usuario: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnEliminarUsuario_Click(sender As Object, e As EventArgs) Handles btnEliminarUsuario.Click
        ' Solo administradores pueden borrar usuarios.
        If Not ComunSesionActual.EsAdministrador() Then
            MessageBox.Show("Esta acción está restringida a usuarios con rol Administrador.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If String.IsNullOrEmpty(txtIdUsuario.Text) Then Return

        ' Salvaguarda: que un admin no se borre a sí mismo (se quedaría sin acceso).
        Dim idAEliminar As Integer = 0
        Integer.TryParse(txtIdUsuario.Text, idAEliminar)
        If idAEliminar > 0 AndAlso idAEliminar = ComunSesionActual.IdUsuario Then
            MessageBox.Show("No puedes eliminar el usuario con el que has iniciado sesión.", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If MessageBox.Show("¿Eliminar usuario '" & txtUsername.Text & "'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()
                Dim cmd As New SQLiteCommand("DELETE FROM Usuarios WHERE ID_Usuario = @id", c)
                cmd.Parameters.AddWithValue("@id", txtIdUsuario.Text)
                cmd.ExecuteNonQuery()
                LogErrores.Info("FrmConfiguracion.EliminarUsuario", $"Usuario ID {txtIdUsuario.Text} eliminado por {ComunSesionActual.Usuario}")
                btnNuevoUsuario.PerformClick()
                CargarListaUsuarios()
            Catch ex As Exception
                LogErrores.Registrar("FrmConfiguracion.EliminarUsuario", ex)
                MessageBox.Show("Error al eliminar: " & ex.Message)
            End Try
        End If
    End Sub

    ' LÓGICA DE COPIAS DE SEGURIDAD
    Private Sub btnExaminarRuta_Click(sender As Object, e As EventArgs) Handles btnExaminarRuta.Click
        Dim fbd As New FolderBrowserDialog() With {.Description = "Selecciona la carpeta para copias de seguridad"}
        If fbd.ShowDialog() = DialogResult.OK Then txtRutaBackup.Text = fbd.SelectedPath
    End Sub

    Private Sub btnHacerCopia_Click(sender As Object, e As EventArgs) Handles btnHacerCopia.Click
        If String.IsNullOrWhiteSpace(txtRutaBackup.Text) OrElse Not Directory.Exists(txtRutaBackup.Text) Then
            MessageBox.Show("Selecciona una carpeta válida.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If
        Try
            ' Delegamos en el gestor centralizado: usa VACUUM INTO (correcto para BD viva).
            Dim rutaCreada As String = GestorBackups.HacerCopiaManual(txtRutaBackup.Text)
            MessageBox.Show("Copia realizada con éxito en:" & vbCrLf & rutaCreada, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            LogErrores.Registrar("FrmConfiguracion.btnHacerCopia", ex)
            MessageBox.Show("Error al hacer copia: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnRestaurarCopia_Click(sender As Object, e As EventArgs) Handles btnRestaurarCopia.Click
        Dim opf As New OpenFileDialog() With {.Filter = "BD SQLite (*.db;*.sqlite)|*.db;*.sqlite|Todos (*.*)|*.*"}
        If opf.ShowDialog() = DialogResult.OK Then
            If MessageBox.Show("¡ATENCIÓN! Vas a reemplazar la BD actual." & vbCrLf & "Se creará una copia en papelera." & vbCrLf & "¿Seguro?", "Peligro", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                Try
                    GestorBackups.Restaurar(opf.FileName)
                    MessageBox.Show("Restauración completada." & vbCrLf & "El programa se cerrará para aplicar los cambios. Vuelve a abrirlo.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Application.Exit()
                Catch ex As Exception
                    LogErrores.Registrar("FrmConfiguracion.btnRestaurar", ex)
                    MessageBox.Show("Error Crítico al restaurar: " & ex.Message & vbCrLf & vbCrLf &
                                    "CONSEJO: Comprueba que NO tienes la base de datos abierta en el programa 'DB Browser for SQLite'.", "Error de Bloqueo", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End If
    End Sub

    Private Function ObtenerTamanoPapelera() As String
        Return GestorBackups.TamanoPapeleraMB().ToString("0.00") & " MB"
    End Function

    Private Sub btnVaciarPapelera_Click(sender As Object, e As EventArgs) Handles btnVaciarPapelera.Click
        If MessageBox.Show("¿Borrar permanentemente la papelera?", "Confirmar", MessageBoxButtons.YesNo) = DialogResult.Yes Then
            Try
                Dim n As Integer = GestorBackups.VaciarPapelera()
                btnVaciarPapelera.Text = "🗑️ Vaciar Papelera de Bases de Datos (0.00 MB)"
                MessageBox.Show($"Papelera vaciada. {n} archivo(s) eliminado(s).", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                LogErrores.Registrar("FrmConfiguracion.VaciarPapelera", ex)
                MessageBox.Show("Error: " & ex.Message)
            End Try
        End If
    End Sub

    ' CARGAR Y GUARDAR TODA LA CONFIGURACIÓN (EMPRESA + BACKUP)
    Private Sub CargarDatosGenerales()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim sql As String = "SELECT * FROM Empresa WHERE ID = 1"
            Using cmd As New SQLiteCommand(sql, c)
                Using r As SQLiteDataReader = cmd.ExecuteReader()
                    If r.Read() Then
                        txtNombreFiscal.Text = If(IsDBNull(r("NombreFiscal")), "", r("NombreFiscal").ToString())
                        txtNombreComercial.Text = If(IsDBNull(r("NombreComercial")), "", r("NombreComercial").ToString())
                        txtCIF.Text = If(IsDBNull(r("CIF")), "", r("CIF").ToString())
                        txtDireccion.Text = If(IsDBNull(r("Direccion")), "", r("Direccion").ToString())
                        txtPoblacion.Text = If(IsDBNull(r("Poblacion")), "", r("Poblacion").ToString())
                        txtCP.Text = If(IsDBNull(r("CodigoPostal")), "", r("CodigoPostal").ToString())
                        txtProvincia.Text = If(IsDBNull(r("Provincia")), "", r("Provincia").ToString())
                        txtTelefono.Text = If(IsDBNull(r("Telefono")), "", r("Telefono").ToString())
                        txtEmail.Text = If(IsDBNull(r("Email")), "", r("Email").ToString())
                        txtWeb.Text = If(IsDBNull(r("SitioWeb")), "", r("SitioWeb").ToString())
                        txtRegistro.Text = If(IsDBNull(r("RegistroMercantil")), "", r("RegistroMercantil").ToString())
                        If Not IsDBNull(r("Logo")) Then picLogo.Image = BytesToImage(CType(r("Logo"), Byte()))

                        ' --- Datos Backup ---

                        ' Leemos la ruta que hay guardada en la base de datos
                        Dim rutaGuardada As String = If(IsDBNull(r("RutaBackup")), "", r("RutaBackup").ToString().Trim())

                        ' Si está vacía (es la primera vez que abre el programa), le ponemos la de defecto
                        If String.IsNullOrEmpty(rutaGuardada) Then
                            txtRutaBackup.Text = RUTA_BACKUP_DEFECTO
                        Else
                            txtRutaBackup.Text = rutaGuardada
                        End If
                        chkAutoBackup.Checked = If(IsDBNull(r("AutoBackup")), False, Convert.ToBoolean(r("AutoBackup")))
                        cboFrecuencia.Text = If(IsDBNull(r("FrecuenciaBackup")), "Diaria", r("FrecuenciaBackup").ToString())
                        If Not IsDBNull(r("HoraBackup")) Then
                            Dim horaG As DateTime
                            If DateTime.TryParse(r("HoraBackup").ToString(), horaG) Then dtpHoraBackup.Value = horaG
                        End If
                        If Not IsDBNull(r("DiasBackup")) Then
                            Dim diasG As String() = r("DiasBackup").ToString().Split(","c)
                            For i As Integer = 0 To chkDias.Items.Count - 1
                                chkDias.SetItemChecked(i, diasG.Contains(i.ToString()))
                            Next
                        End If
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar configuración: " & ex.Message)
        End Try
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        ' === VALIDACIONES DE ENTRADA (NO BLOQUEANTES) ===
        ' Avisamos al usuario si algo no cuadra pero le dejamos guardar (puede haber datos
        ' importados con CIF "antiguos" que no superan la validación AEAT).
        If Not Validador.ConfirmarSiHayProblemas(
            ("CIF de la empresa", Validador.ValidarCIF(txtCIF.Text)),
            ("Email", Validador.ValidarEmail(txtEmail.Text)),
            ("Código postal", Validador.ValidarCodigoPostal(txtCP.Text)),
            ("Teléfono", Validador.ValidarTelefono(txtTelefono.Text))
        ) Then
            Return
        End If

        ' --- Validación específica del backup automático: si está activo, hay que tener carpeta válida ---
        ' Esta SÍ es bloqueante: no tiene sentido activar copias automáticas con una ruta vacía.
        If chkAutoBackup.Checked Then
            If String.IsNullOrWhiteSpace(txtRutaBackup.Text) OrElse Not Directory.Exists(txtRutaBackup.Text) Then
                MessageBox.Show("Si activas las copias automáticas debes seleccionar una carpeta de destino válida.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtRutaBackup.Focus() : Return
            End If
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim diasSeleccionados As New List(Of String)
            For i As Integer = 0 To chkDias.Items.Count - 1
                If chkDias.GetItemChecked(i) Then diasSeleccionados.Add(i.ToString())
            Next
            Dim diasString As String = String.Join(",", diasSeleccionados)

            Dim sql As String = "UPDATE Empresa SET NombreFiscal=@nomF, NombreComercial=@nomC, CIF=@cif, Direccion=@dir, Poblacion=@pob, CodigoPostal=@cp, Provincia=@prov, Telefono=@tel, Email=@ema, SitioWeb=@web, RegistroMercantil=@reg, Logo=@logo, RutaBackup=@rutaB, AutoBackup=@autoB, FrecuenciaBackup=@frecB, HoraBackup=@horaB, DiasBackup=@diasB WHERE ID = 1"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@nomF", txtNombreFiscal.Text.Trim())
                cmd.Parameters.AddWithValue("@nomC", txtNombreComercial.Text.Trim())
                cmd.Parameters.AddWithValue("@cif", txtCIF.Text.Trim().ToUpperInvariant())
                cmd.Parameters.AddWithValue("@dir", txtDireccion.Text.Trim())
                cmd.Parameters.AddWithValue("@pob", txtPoblacion.Text.Trim())
                cmd.Parameters.AddWithValue("@cp", txtCP.Text.Trim())
                cmd.Parameters.AddWithValue("@prov", txtProvincia.Text.Trim())
                cmd.Parameters.AddWithValue("@tel", txtTelefono.Text.Trim())
                cmd.Parameters.AddWithValue("@ema", txtEmail.Text.Trim())
                cmd.Parameters.AddWithValue("@web", txtWeb.Text.Trim())
                cmd.Parameters.AddWithValue("@reg", txtRegistro.Text.Trim())
                If picLogo.Image IsNot Nothing Then cmd.Parameters.AddWithValue("@logo", ImageToBytes(picLogo.Image)) Else cmd.Parameters.AddWithValue("@logo", DBNull.Value)

                cmd.Parameters.AddWithValue("@rutaB", txtRutaBackup.Text.Trim())
                cmd.Parameters.AddWithValue("@autoB", If(chkAutoBackup.Checked, 1, 0))
                cmd.Parameters.AddWithValue("@frecB", cboFrecuencia.Text)
                cmd.Parameters.AddWithValue("@horaB", dtpHoraBackup.Value.ToString("HH:mm"))
                cmd.Parameters.AddWithValue("@diasB", diasString)

                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Ajustes globales guardados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            LogErrores.Registrar("FrmConfiguracion.Guardar", ex)
            MessageBox.Show("Error al guardar: " & ex.Message)
        End Try
    End Sub
End Class
