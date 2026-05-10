Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SQLite

Public Class FrmAgencias

    ' los controles de la pantalla, todos a pelo (sin diseñador)
    Private WithEvents txtCodigo As New TextBox()
    Private WithEvents txtNombre As New TextBox()
    Private WithEvents chkActivo As New CheckBox()

    Private WithEvents txtTelefono As New TextBox()
    Private WithEvents txtEmail As New TextBox()
    Private WithEvents txtWebSeguimiento As New TextBox()
    Private WithEvents txtObservaciones As New TextBox()

    Private WithEvents btnGuardar As New Button()
    Private WithEvents btnBorrar As New Button()
    Private WithEvents btnNuevo As New Button()

    Private WithEvents dgvAgencias As New DataGridView()
    Private _idAgenciaActual As Integer = 0

    ' cuando se abre el form preparo todo
    Private Sub FrmAgencias_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Gestión de Agencias de Transporte"
        Me.BackColor = Color.WhiteSmoke
        Me.Size = New Size(750, 600) ' lo hice mas grande pq sino se amontonaba todo
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen

        ConstruirInterfaz()
        ConfigurarGrid()
        CargarAgencias()
    End Sub

    ' aqui voy montando los textbox y las labels uno por uno
    Private Sub ConstruirInterfaz()
        Dim margenIzq As Integer = 20
        Dim y1 As Integer = 20
        Dim y2 As Integer = 85
        Dim y3 As Integer = 150

        ' un lambda para no repetir el mismo codigo 20 veces
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

        ' fila de arriba con lo basico
        CrearCampo("Código", txtCodigo, margenIzq, y1, 100)
        CrearCampo("Nombre de la Agencia (Ej: SEUR)", txtNombre, 140, y1, 350)

        chkActivo.Text = "Agencia Activa"
        chkActivo.Checked = True
        chkActivo.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        chkActivo.Bounds = New Rectangle(520, y1 + 25, 150, 25)
        Me.Controls.Add(chkActivo)

        ' segunda fila, telefono email y la web del seguimiento
        CrearCampo("Teléfono Contacto", txtTelefono, margenIzq, y2, 140)
        CrearCampo("Correo Electrónico", txtEmail, 180, y2, 220)
        CrearCampo("Página Web (Seguimiento de envíos)", txtWebSeguimiento, 420, y2, 280)

        ' tercera fila para apuntar cosas sueltas
        CrearCampo("Observaciones / Notas internas", txtObservaciones, margenIzq, y3, 680)

        ' los 3 botones de siempre
        Dim yBotones As Integer = 215
        ConfigurarBoton(btnGuardar, "Guardar", margenIzq, yBotones, Color.FromArgb(0, 120, 215))
        ConfigurarBoton(btnBorrar, "Borrar", margenIzq + 110, yBotones, Color.FromArgb(209, 52, 56))
        ConfigurarBoton(btnNuevo, "Nuevo", margenIzq + 220, yBotones, Color.FromArgb(40, 140, 90))

        ' una rayita gris para separar, feo pero funciona
        Dim linea As New Label() With {.Bounds = New Rectangle(margenIzq, yBotones + 50, Me.ClientSize.Width - 55, 2), .BackColor = Color.FromArgb(200, 200, 200), .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right}
        Me.Controls.Add(linea)

        ' la tabla de abajo
        dgvAgencias.Bounds = New Rectangle(margenIzq, yBotones + 70, Me.ClientSize.Width - 55, 250)
        dgvAgencias.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(dgvAgencias)
    End Sub

    Private Sub ConfigurarBoton(btn As Button, texto As String, x As Integer, y As Integer, colorFondo As Color)
        btn.Text = texto : btn.Bounds = New Rectangle(x, y, 100, 32)
        btn.BackColor = colorFondo : btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat : btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI", 10, FontStyle.Bold) : btn.Cursor = Cursors.Hand
        Me.Controls.Add(btn)
    End Sub

    ' configuro las columnas del grid a mano
    Private Sub ConfigurarGrid()
        Try
            FrmPresupuestos.EstilizarGrid(dgvAgencias)
        Catch ex As Exception
        End Try

        dgvAgencias.AutoGenerateColumns = False
        dgvAgencias.AllowUserToAddRows = False
        dgvAgencias.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvAgencias.ReadOnly = True

        dgvAgencias.Columns.Clear()
        dgvAgencias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Agencia", .DataPropertyName = "ID_Agencia", .Visible = False})
        dgvAgencias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Codigo", .DataPropertyName = "Codigo", .HeaderText = "Código", .Width = 90})
        dgvAgencias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Nombre", .DataPropertyName = "Nombre", .HeaderText = "Agencia", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        dgvAgencias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Telefono", .DataPropertyName = "Telefono", .HeaderText = "Teléfono", .Width = 110})
        dgvAgencias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "WebSeguimiento", .DataPropertyName = "WebSeguimiento", .HeaderText = "Página Web", .Width = 180})

        Dim colActivo As New DataGridViewCheckBoxColumn() With {.Name = "Activo", .DataPropertyName = "Activo", .HeaderText = "Activo", .Width = 60}
        dgvAgencias.Columns.Add(colActivo)

        ' estas no se ven pero las necesito para rellenar los campos al hacer click
        dgvAgencias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Email", .DataPropertyName = "Email", .Visible = False})
        dgvAgencias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Observaciones", .DataPropertyName = "Observaciones", .Visible = False})
    End Sub

    Private Sub AjustarAltoTabla()
        If dgvAgencias Is Nothing Then Return
        Dim altoNecesario As Integer = dgvAgencias.ColumnHeadersHeight
        For Each fila As DataGridViewRow In dgvAgencias.Rows
            altoNecesario += fila.Height
        Next
        altoNecesario += 3
        Dim altoMaximo As Integer = Me.ClientSize.Height - dgvAgencias.Top - 20
        dgvAgencias.Height = If(altoNecesario > altoMaximo, altoMaximo, altoNecesario)
        dgvAgencias.BackgroundColor = Me.BackColor
    End Sub

    Private Sub CargarAgencias()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String = "SELECT * FROM Agencias ORDER BY Codigo ASC"
            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvAgencias.DataSource = dt
                AjustarAltoTabla()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar agencias: " & ex.Message)
        End Try
    End Sub

    Private Sub dgvAgencias_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvAgencias.CellClick
        If e.RowIndex >= 0 Then
            Dim fila As DataGridViewRow = dgvAgencias.Rows(e.RowIndex)
            _idAgenciaActual = Convert.ToInt32(fila.Cells("ID_Agencia").Value)

            txtCodigo.Text = If(IsDBNull(fila.Cells("Codigo").Value), "", fila.Cells("Codigo").Value?.ToString())
            txtNombre.Text = If(IsDBNull(fila.Cells("Nombre").Value), "", fila.Cells("Nombre").Value?.ToString())
            txtTelefono.Text = If(IsDBNull(fila.Cells("Telefono").Value), "", fila.Cells("Telefono").Value?.ToString())
            txtEmail.Text = If(IsDBNull(fila.Cells("Email").Value), "", fila.Cells("Email").Value?.ToString())
            txtWebSeguimiento.Text = If(IsDBNull(fila.Cells("WebSeguimiento").Value), "", fila.Cells("WebSeguimiento").Value?.ToString())
            txtObservaciones.Text = If(IsDBNull(fila.Cells("Observaciones").Value), "", fila.Cells("Observaciones").Value?.ToString())

            chkActivo.Checked = Not IsDBNull(fila.Cells("Activo").Value) AndAlso (fila.Cells("Activo").Value.ToString() = "1" OrElse fila.Cells("Activo").Value.ToString().ToLower() = "true")
        End If
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If String.IsNullOrWhiteSpace(txtCodigo.Text) OrElse String.IsNullOrWhiteSpace(txtNombre.Text) Then
            MessageBox.Show("El Código y el Nombre de la agencia son obligatorios.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String
            If _idAgenciaActual = 0 Then
                sql = "INSERT INTO Agencias (Codigo, Nombre, Telefono, Email, WebSeguimiento, Observaciones, Activo) " &
                      "VALUES (@cod, @nom, @tel, @email, @web, @obs, @act)"
            Else
                sql = "UPDATE Agencias SET Codigo=@cod, Nombre=@nom, Telefono=@tel, Email=@email, WebSeguimiento=@web, Observaciones=@obs, Activo=@act " &
                      "WHERE ID_Agencia=@id"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@cod", txtCodigo.Text.Trim())
                cmd.Parameters.AddWithValue("@nom", txtNombre.Text.Trim())
                cmd.Parameters.AddWithValue("@tel", txtTelefono.Text.Trim())
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim())
                cmd.Parameters.AddWithValue("@web", txtWebSeguimiento.Text.Trim())
                cmd.Parameters.AddWithValue("@obs", txtObservaciones.Text.Trim())
                cmd.Parameters.AddWithValue("@act", If(chkActivo.Checked, 1, 0))

                If _idAgenciaActual > 0 Then cmd.Parameters.AddWithValue("@id", _idAgenciaActual)

                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Agencia guardada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnNuevo.PerformClick()
            CargarAgencias()
        Catch ex As Exception
            MessageBox.Show("Error al guardar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBorrar_Click(sender As Object, e As EventArgs) Handles btnBorrar.Click
        If _idAgenciaActual = 0 Then Return

        If MessageBox.Show("¿Eliminar definitivamente la agencia " & txtNombre.Text & "?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                Using cmd As New SQLiteCommand("DELETE FROM Agencias WHERE ID_Agencia = @id", c)
                    cmd.Parameters.AddWithValue("@id", _idAgenciaActual)
                    cmd.ExecuteNonQuery()
                End Using

                btnNuevo.PerformClick()
                CargarAgencias()
            Catch ex As Exception
                MessageBox.Show("No se puede borrar esta agencia porque está asignada a una o más Rutas. Desmárcala como 'Activa' en su lugar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Try
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        _idAgenciaActual = 0
        txtCodigo.Clear() : txtNombre.Clear() : txtTelefono.Clear() : txtEmail.Clear()
        txtWebSeguimiento.Clear() : txtObservaciones.Clear()
        chkActivo.Checked = True
        txtCodigo.Focus()
        dgvAgencias.ClearSelection()
    End Sub

End Class
