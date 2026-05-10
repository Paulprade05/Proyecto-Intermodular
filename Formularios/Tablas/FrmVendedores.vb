Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SQLite

Public Class FrmVendedores

    ' 1. DECLARACIÓN DE CONTROLES
    Private WithEvents txtCodigo As New TextBox()
    Private WithEvents txtDNI As New TextBox()
    Private WithEvents dtpFechaAlta As New DateTimePicker()
    Private WithEvents chkActivo As New CheckBox()

    Private WithEvents txtNombre As New TextBox()
    Private WithEvents txtTelefono As New TextBox()
    Private WithEvents txtEmail As New TextBox()

    Private WithEvents txtDireccion As New TextBox()
    Private WithEvents txtPoblacion As New TextBox()
    Private WithEvents txtProvincia As New TextBox()
    Private WithEvents txtCP As New TextBox()

    Private WithEvents txtComision As New TextBox()
    Private WithEvents txtObservaciones As New TextBox()

    Private WithEvents btnGuardar As New Button()
    Private WithEvents btnBorrar As New Button()
    Private WithEvents btnNuevo As New Button()

    Private WithEvents dgvVendedores As New DataGridView()
    Private _idVendedorActual As Integer = 0

    ' 2. INICIALIZACIÓN
    Private Sub FrmVendedores_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Gestión de Vendedores y Agentes"
        Me.BackColor = Color.WhiteSmoke ' Fondo claro y limpio
        Me.Size = New Size(860, 650) ' Un poco más ancha para que quepan los datos
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen

        ConstruirInterfaz()
        ConfigurarGrid()
        CargarVendedores()
    End Sub

    ' 3. CONSTRUCTOR DE LA INTERFAZ
    Private Sub ConstruirInterfaz()
        Dim margenIzq As Integer = 20
        Dim y1 As Integer = 20  ' Fila 1
        Dim y2 As Integer = 80  ' Fila 2
        Dim y3 As Integer = 140 ' Fila 3
        Dim y4 As Integer = 200 ' Fila 4

        ' --- FUNCIÓN PARA CREAR CAMPOS ---
        Dim CrearCampo = Sub(textoLabel As String, ctrl As Control, x As Integer, y As Integer, w As Integer)
                             Dim lbl As New Label() With {.Text = textoLabel, .Location = New Point(x, y), .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.FromArgb(50, 50, 50)}
                             Me.Controls.Add(lbl)

                             ctrl.Bounds = New Rectangle(x, y + 23, w, 27)
                             ctrl.Font = New Font("Segoe UI", 10.5F)

                             If TypeOf ctrl Is TextBox Then
                                 Dim txt = DirectCast(ctrl, TextBox)
                                 txt.BorderStyle = BorderStyle.FixedSingle : txt.BackColor = Color.White : txt.ForeColor = Color.Black
                             End If
                             Me.Controls.Add(ctrl)
                         End Sub

        ' --- FILA 1: Identificación ---
        CrearCampo("Código", txtCodigo, margenIzq, y1, 120)
        CrearCampo("DNI / CIF", txtDNI, 160, y1, 140)

        dtpFechaAlta.Format = DateTimePickerFormat.Short
        CrearCampo("Fecha Alta", dtpFechaAlta, 320, y1, 120)

        ' Checkbox especial
        chkActivo.Text = "Vendedor Activo"
        chkActivo.Checked = True
        chkActivo.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        chkActivo.Bounds = New Rectangle(470, y1 + 25, 150, 25)
        Me.Controls.Add(chkActivo)

        ' --- FILA 2: Contacto ---
        CrearCampo("Nombre Completo", txtNombre, margenIzq, y2, 320)
        CrearCampo("Teléfono", txtTelefono, 360, y2, 140)
        CrearCampo("Email", txtEmail, 520, y2, 280)

        ' --- FILA 3: Dirección ---
        CrearCampo("Dirección", txtDireccion, margenIzq, y3, 320)
        CrearCampo("Población", txtPoblacion, 360, y3, 180)
        CrearCampo("Provincia", txtProvincia, 560, y3, 150)
        CrearCampo("C.P.", txtCP, 730, y3, 70)

        ' --- FILA 4: Condiciones ---
        CrearCampo("Comisión (%)", txtComision, margenIzq, y4, 100)
        CrearCampo("Observaciones / Notas", txtObservaciones, 140, y4, 660)

        ' --- BOTONES ---
        Dim yBotones As Integer = 265
        ConfigurarBoton(btnGuardar, "Guardar", margenIzq, yBotones, Color.FromArgb(0, 120, 215))
        ConfigurarBoton(btnBorrar, "Borrar", margenIzq + 110, yBotones, Color.FromArgb(209, 52, 56))
        ConfigurarBoton(btnNuevo, "Nuevo", margenIzq + 220, yBotones, Color.FromArgb(40, 140, 90))

        ' --- LÍNEA DIVISORIA ---
        Dim linea As New Label() With {.Bounds = New Rectangle(margenIzq, yBotones + 50, Me.ClientSize.Width - (margenIzq * 2), 2), .BackColor = Color.FromArgb(200, 200, 200), .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right}
        Me.Controls.Add(linea)

        ' --- TABLA ---
        dgvVendedores.Bounds = New Rectangle(margenIzq, yBotones + 70, Me.ClientSize.Width - (margenIzq * 2), 220)
        dgvVendedores.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(dgvVendedores)
    End Sub

    Private Sub ConfigurarBoton(btn As Button, texto As String, x As Integer, y As Integer, colorFondo As Color)
        btn.Text = texto : btn.Bounds = New Rectangle(x, y, 100, 32)
        btn.BackColor = colorFondo : btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat : btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI", 10, FontStyle.Bold) : btn.Cursor = Cursors.Hand
        Me.Controls.Add(btn)
    End Sub

    ' 4. ESTILOS Y COMPORTAMIENTO DEL GRID
    Private Sub ConfigurarGrid()
        Try
            FrmPresupuestos.EstilizarGrid(dgvVendedores)
        Catch ex As Exception
        End Try

        dgvVendedores.AutoGenerateColumns = False
        dgvVendedores.AllowUserToAddRows = False
        dgvVendedores.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvVendedores.ReadOnly = True

        dgvVendedores.Columns.Clear()
        dgvVendedores.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Vendedor", .DataPropertyName = "ID_Vendedor", .Visible = False})
        dgvVendedores.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CodigoVendedor", .DataPropertyName = "CodigoVendedor", .HeaderText = "Código", .Width = 100})
        dgvVendedores.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Nombre", .DataPropertyName = "Nombre", .HeaderText = "Nombre", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        dgvVendedores.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Telefono", .DataPropertyName = "Telefono", .HeaderText = "Teléfono", .Width = 120})
        dgvVendedores.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Comision", .DataPropertyName = "Comision", .HeaderText = "Comisión %", .Width = 100, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight}})

        ' Columna visual para saber si está activo
        Dim colActivo As New DataGridViewCheckBoxColumn() With {.Name = "Activo", .DataPropertyName = "Activo", .HeaderText = "Activo", .Width = 70}
        dgvVendedores.Columns.Add(colActivo)

        ' Columnas ocultas para cargarlas al hacer clic
        Dim columnasOcultas() As String = {"DNI", "Email", "Direccion", "Poblacion", "Provincia", "CP", "Observaciones", "FechaAlta"}
        For Each col In columnasOcultas
            dgvVendedores.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = col, .DataPropertyName = col, .Visible = False})
        Next
    End Sub

    Private Sub AjustarAltoTabla()
        If dgvVendedores Is Nothing Then Return
        Dim altoNecesario As Integer = dgvVendedores.ColumnHeadersHeight
        For Each fila As DataGridViewRow In dgvVendedores.Rows
            altoNecesario += fila.Height
        Next
        altoNecesario += 3
        Dim altoMaximo As Integer = Me.ClientSize.Height - dgvVendedores.Top - 20
        dgvVendedores.Height = If(altoNecesario > altoMaximo, altoMaximo, altoNecesario)
        dgvVendedores.BackgroundColor = Me.BackColor
    End Sub

    ' 5. LÓGICA DE BASE DE DATOS (SQLite)
    Private Sub CargarVendedores()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String = "SELECT * FROM Vendedores ORDER BY CodigoVendedor ASC"
            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvVendedores.DataSource = dt
                AjustarAltoTabla()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar vendedores: " & ex.Message)
        End Try
    End Sub

    Private Sub dgvVendedores_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvVendedores.CellClick
        If e.RowIndex >= 0 Then
            Dim fila As DataGridViewRow = dgvVendedores.Rows(e.RowIndex)
            _idVendedorActual = Convert.ToInt32(fila.Cells("ID_Vendedor").Value)

            txtCodigo.Text = If(IsDBNull(fila.Cells("CodigoVendedor").Value), "", fila.Cells("CodigoVendedor").Value?.ToString())
            txtDNI.Text = If(IsDBNull(fila.Cells("DNI").Value), "", fila.Cells("DNI").Value?.ToString())
            txtNombre.Text = If(IsDBNull(fila.Cells("Nombre").Value), "", fila.Cells("Nombre").Value?.ToString())
            txtTelefono.Text = If(IsDBNull(fila.Cells("Telefono").Value), "", fila.Cells("Telefono").Value?.ToString())
            txtEmail.Text = If(IsDBNull(fila.Cells("Email").Value), "", fila.Cells("Email").Value?.ToString())
            txtDireccion.Text = If(IsDBNull(fila.Cells("Direccion").Value), "", fila.Cells("Direccion").Value?.ToString())
            txtPoblacion.Text = If(IsDBNull(fila.Cells("Poblacion").Value), "", fila.Cells("Poblacion").Value?.ToString())
            txtProvincia.Text = If(IsDBNull(fila.Cells("Provincia").Value), "", fila.Cells("Provincia").Value?.ToString())
            txtCP.Text = If(IsDBNull(fila.Cells("CP").Value), "", fila.Cells("CP").Value?.ToString())
            txtComision.Text = If(IsDBNull(fila.Cells("Comision").Value), "0", fila.Cells("Comision").Value?.ToString())
            txtObservaciones.Text = If(IsDBNull(fila.Cells("Observaciones").Value), "", fila.Cells("Observaciones").Value?.ToString())

            chkActivo.Checked = Not IsDBNull(fila.Cells("Activo").Value) AndAlso (fila.Cells("Activo").Value.ToString() = "1" OrElse fila.Cells("Activo").Value.ToString().ToLower() = "true")

            If Not IsDBNull(fila.Cells("FechaAlta").Value) Then
                Dim fechaParsed As Date
                If Date.TryParse(fila.Cells("FechaAlta").Value.ToString(), fechaParsed) Then
                    dtpFechaAlta.Value = fechaParsed
                End If
            End If
        End If
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If String.IsNullOrWhiteSpace(txtCodigo.Text) OrElse String.IsNullOrWhiteSpace(txtNombre.Text) Then
            MessageBox.Show("El Código y el Nombre son obligatorios.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' === VALIDACIONES DE ENTRADA (NO BLOQUEANTES) ===
        ' El "DNI" del vendedor puede ser NIF o NIE (es siempre persona física).
        If Not Validador.ConfirmarSiHayProblemas(
            ("DNI/NIE del vendedor", Validador.ValidarDocumentoIdentidad(txtDNI.Text)),
            ("Email", Validador.ValidarEmail(txtEmail.Text)),
            ("Teléfono", Validador.ValidarTelefono(txtTelefono.Text)),
            ("Código postal", Validador.ValidarCodigoPostal(txtCP.Text))
        ) Then
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String
            If _idVendedorActual = 0 Then
                sql = "INSERT INTO Vendedores (CodigoVendedor, Nombre, DNI, Telefono, Email, Direccion, Poblacion, Provincia, CP, Comision, Observaciones, Activo, FechaAlta) " &
                      "VALUES (@cod, @nom, @dni, @tel, @email, @dir, @pob, @prov, @cp, @com, @obs, @act, @fec)"
            Else
                sql = "UPDATE Vendedores SET CodigoVendedor=@cod, Nombre=@nom, DNI=@dni, Telefono=@tel, Email=@email, Direccion=@dir, Poblacion=@pob, Provincia=@prov, CP=@cp, Comision=@com, Observaciones=@obs, Activo=@act, FechaAlta=@fec " &
                      "WHERE ID_Vendedor=@id"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@cod", txtCodigo.Text.Trim())
                cmd.Parameters.AddWithValue("@nom", txtNombre.Text.Trim())
                cmd.Parameters.AddWithValue("@dni", txtDNI.Text.Trim().ToUpperInvariant())
                cmd.Parameters.AddWithValue("@tel", txtTelefono.Text.Trim())
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim())
                cmd.Parameters.AddWithValue("@dir", txtDireccion.Text.Trim())
                cmd.Parameters.AddWithValue("@pob", txtPoblacion.Text.Trim())
                cmd.Parameters.AddWithValue("@prov", txtProvincia.Text.Trim())
                cmd.Parameters.AddWithValue("@cp", txtCP.Text.Trim())

                Dim comision As Decimal = 0
                Decimal.TryParse(txtComision.Text, comision)
                cmd.Parameters.AddWithValue("@com", comision)

                cmd.Parameters.AddWithValue("@obs", txtObservaciones.Text.Trim())
                cmd.Parameters.AddWithValue("@act", If(chkActivo.Checked, 1, 0))
                cmd.Parameters.AddWithValue("@fec", dtpFechaAlta.Value.ToString("yyyy-MM-dd"))

                If _idVendedorActual > 0 Then cmd.Parameters.AddWithValue("@id", _idVendedorActual)

                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Vendedor guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnNuevo.PerformClick()
            CargarVendedores()
        Catch ex As Exception
            LogErrores.Registrar("FrmVendedores.Guardar", ex)
            MessageBox.Show("Error al guardar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBorrar_Click(sender As Object, e As EventArgs) Handles btnBorrar.Click
        If _idVendedorActual = 0 Then
            MessageBox.Show("Selecciona un vendedor de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If MessageBox.Show("¿Eliminar definitivamente al vendedor " & txtNombre.Text & "?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                Using cmd As New SQLiteCommand("DELETE FROM Vendedores WHERE ID_Vendedor = @id", c)
                    cmd.Parameters.AddWithValue("@id", _idVendedorActual)
                    cmd.ExecuteNonQuery()
                End Using

                btnNuevo.PerformClick()
                CargarVendedores()
            Catch ex As Exception
                MessageBox.Show("No se puede borrar este vendedor porque tiene facturas o histórico asociado.", "Bloqueo de Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        _idVendedorActual = 0
        txtCodigo.Clear() : txtDNI.Clear() : txtNombre.Clear() : txtTelefono.Clear() : txtEmail.Clear()
        txtDireccion.Clear() : txtPoblacion.Clear() : txtProvincia.Clear() : txtCP.Clear()
        txtComision.Text = "0" : txtObservaciones.Clear()
        chkActivo.Checked = True
        dtpFechaAlta.Value = Date.Now
        txtCodigo.Focus()
        dgvVendedores.ClearSelection()
    End Sub

End Class
