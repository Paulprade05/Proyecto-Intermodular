Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SQLite

Public Class FrmRutas

    ' los controles del form, todos a mano
    Private WithEvents txtCodigo As New TextBox()
    Private WithEvents txtNombreZona As New TextBox()
    Private WithEvents chkActivo As New CheckBox()

    Private WithEvents txtDiasReparto As New TextBox()
    Private WithEvents cboAgencia As New ComboBox()
    Private WithEvents txtObservaciones As New TextBox()

    Private WithEvents btnGuardar As New Button()
    Private WithEvents btnBorrar As New Button()
    Private WithEvents btnNuevo As New Button()

    Private WithEvents dgvRutas As New DataGridView()
    Private _idRutaActual As Integer = 0

    ' al cargar preparo todo
    Private Sub FrmRutas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Gestión de Rutas de Reparto"
        Me.BackColor = Color.WhiteSmoke
        Me.Size = New Size(700, 550)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen

        ConstruirInterfaz()
        ConfigurarGrid()

        ' OJO: primero las agencias pq sino el combo sale vacio cuando cargo las rutas
        CargarComboAgencias()
        CargarRutas()
    End Sub

    ' aqui voy montando los controles a pelo
    Private Sub ConstruirInterfaz()
        Dim margenIzq As Integer = 20
        Dim y1 As Integer = 20
        Dim y2 As Integer = 85
        Dim y3 As Integer = 150

        Dim CrearCampo = Sub(textoLabel As String, ctrl As Control, x As Integer, y As Integer, w As Integer)
                             Dim lbl As New Label() With {.Text = textoLabel, .Location = New Point(x, y), .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.FromArgb(50, 50, 50)}
                             Me.Controls.Add(lbl)

                             ctrl.Bounds = New Rectangle(x, y + 23, w, 27)
                             ctrl.Font = New Font("Segoe UI", 10.5F)

                             If TypeOf ctrl Is TextBox Then
                                 Dim txt = DirectCast(ctrl, TextBox)
                                 txt.BorderStyle = BorderStyle.FixedSingle : txt.BackColor = Color.White : txt.ForeColor = Color.Black
                             ElseIf TypeOf ctrl Is ComboBox Then
                                 Dim cbo = DirectCast(ctrl, ComboBox)
                                 cbo.DropDownStyle = ComboBoxStyle.DropDownList
                             End If
                             Me.Controls.Add(ctrl)
                         End Sub

        ' fila 1
        CrearCampo("Código Ruta", txtCodigo, margenIzq, y1, 100)
        CrearCampo("Nombre de la Zona", txtNombreZona, 140, y1, 380)

        chkActivo.Text = "Ruta Activa"
        chkActivo.Checked = True
        chkActivo.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        chkActivo.Bounds = New Rectangle(540, y1 + 25, 120, 25)
        Me.Controls.Add(chkActivo)

        ' fila 2
        CrearCampo("Días de Reparto (Ej: L-X-V)", txtDiasReparto, margenIzq, y2, 250)
        CrearCampo("Agencia Asignada", cboAgencia, 290, y2, 230)

        ' fila 3 para notas
        CrearCampo("Observaciones / Notas", txtObservaciones, margenIzq, y3, 500)

        ' botones
        Dim yBotones As Integer = 215
        ConfigurarBoton(btnGuardar, "Guardar", margenIzq, yBotones, Color.FromArgb(0, 120, 215))
        ConfigurarBoton(btnBorrar, "Borrar", margenIzq + 110, yBotones, Color.FromArgb(209, 52, 56))
        ConfigurarBoton(btnNuevo, "Nuevo", margenIzq + 220, yBotones, Color.FromArgb(40, 140, 90))

        ' rayita gris para separar
        Dim linea As New Label() With {.Bounds = New Rectangle(margenIzq, yBotones + 50, Me.ClientSize.Width - 55, 2), .BackColor = Color.FromArgb(200, 200, 200), .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right}
        Me.Controls.Add(linea)

        ' tabla de abajo
        dgvRutas.Bounds = New Rectangle(margenIzq, yBotones + 70, Me.ClientSize.Width - 55, 190)
        dgvRutas.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(dgvRutas)
    End Sub

    Private Sub ConfigurarBoton(btn As Button, texto As String, x As Integer, y As Integer, colorFondo As Color)
        btn.Text = texto : btn.Bounds = New Rectangle(x, y, 100, 32)
        btn.BackColor = colorFondo : btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat : btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI", 10, FontStyle.Bold) : btn.Cursor = Cursors.Hand
        Me.Controls.Add(btn)
    End Sub

    ' configuro el grid
    Private Sub ConfigurarGrid()
        Try
            FrmPresupuestos.EstilizarGrid(dgvRutas)
        Catch ex As Exception
        End Try

        dgvRutas.AutoGenerateColumns = False
        dgvRutas.AllowUserToAddRows = False
        dgvRutas.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvRutas.ReadOnly = True

        dgvRutas.Columns.Clear()
        dgvRutas.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Ruta", .DataPropertyName = "ID_Ruta", .Visible = False})
        dgvRutas.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Codigo", .DataPropertyName = "Codigo", .HeaderText = "Código", .Width = 90})
        dgvRutas.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NombreZona", .DataPropertyName = "NombreZona", .HeaderText = "Zona", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        dgvRutas.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "DiasReparto", .DataPropertyName = "DiasReparto", .HeaderText = "Días", .Width = 140})

        ' el nombre de la agencia me lo trae el JOIN del SELECT, asi no tengo que ir a buscarlo
        dgvRutas.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NombreAgencia", .DataPropertyName = "NombreAgencia", .HeaderText = "Agencia", .Width = 120})

        Dim colActivo As New DataGridViewCheckBoxColumn() With {.Name = "Activo", .DataPropertyName = "Activo", .HeaderText = "Activo", .Width = 60}
        dgvRutas.Columns.Add(colActivo)

        ' columnas escondidas que uso al hacer click en una fila
        dgvRutas.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Agencia", .DataPropertyName = "ID_Agencia", .Visible = False})
        dgvRutas.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Observaciones", .DataPropertyName = "Observaciones", .Visible = False})
    End Sub

    Private Sub AjustarAltoTabla()
        If dgvRutas Is Nothing Then Return
        Dim altoNecesario As Integer = dgvRutas.ColumnHeadersHeight
        For Each fila As DataGridViewRow In dgvRutas.Rows
            altoNecesario += fila.Height
        Next
        altoNecesario += 3
        Dim altoMaximo As Integer = Me.ClientSize.Height - dgvRutas.Top - 20
        dgvRutas.Height = If(altoNecesario > altoMaximo, altoMaximo, altoNecesario)
        dgvRutas.BackgroundColor = Me.BackColor
    End Sub

    Private Sub CargarComboAgencias()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' al usuario le enseño el nombre pero por dentro guardo el id
            Dim sql As String = "SELECT ID_Agencia, Nombre FROM Agencias ORDER BY Nombre"
            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)
                cboAgencia.DataSource = dt
                cboAgencia.DisplayMember = "Nombre"
                cboAgencia.ValueMember = "ID_Agencia"
                cboAgencia.SelectedIndex = -1
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Private Sub CargarRutas()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' un LEFT JOIN para sacar el nombre de la agencia en la misma consulta
            Dim sql As String = "SELECT r.*, a.Nombre AS NombreAgencia FROM Rutas r LEFT JOIN Agencias a ON r.ID_Agencia = a.ID_Agencia ORDER BY r.Codigo ASC"
            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvRutas.DataSource = dt
                AjustarAltoTabla()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar rutas: " & ex.Message)
        End Try
    End Sub

    Private Sub dgvRutas_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvRutas.CellClick
        If e.RowIndex >= 0 Then
            Dim fila As DataGridViewRow = dgvRutas.Rows(e.RowIndex)
            _idRutaActual = Convert.ToInt32(fila.Cells("ID_Ruta").Value)

            txtCodigo.Text = If(IsDBNull(fila.Cells("Codigo").Value), "", fila.Cells("Codigo").Value?.ToString())
            txtNombreZona.Text = If(IsDBNull(fila.Cells("NombreZona").Value), "", fila.Cells("NombreZona").Value?.ToString())
            txtDiasReparto.Text = If(IsDBNull(fila.Cells("DiasReparto").Value), "", fila.Cells("DiasReparto").Value?.ToString())
            txtObservaciones.Text = If(IsDBNull(fila.Cells("Observaciones").Value), "", fila.Cells("Observaciones").Value?.ToString())

            chkActivo.Checked = Not IsDBNull(fila.Cells("Activo").Value) AndAlso (fila.Cells("Activo").Value.ToString() = "1" OrElse fila.Cells("Activo").Value.ToString().ToLower() = "true")

            ' meto la agencia en el combo, con cuidado de que no sea null
            Dim idAgencia = fila.Cells("ID_Agencia").Value
            If Not IsDBNull(idAgencia) AndAlso idAgencia IsNot Nothing AndAlso idAgencia.ToString().Trim() <> "" Then
                cboAgencia.SelectedValue = Convert.ToInt32(idAgencia)
            Else
                cboAgencia.SelectedIndex = -1
            End If
        End If
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If String.IsNullOrWhiteSpace(txtCodigo.Text) OrElse String.IsNullOrWhiteSpace(txtNombreZona.Text) Then
            MessageBox.Show("El Código y la Zona son obligatorios.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String
            If _idRutaActual = 0 Then
                sql = "INSERT INTO Rutas (Codigo, NombreZona, DiasReparto, ID_Agencia, Observaciones, Activo) " &
                      "VALUES (@cod, @zona, @dias, @agencia, @obs, @act)"
            Else
                sql = "UPDATE Rutas SET Codigo=@cod, NombreZona=@zona, DiasReparto=@dias, ID_Agencia=@agencia, Observaciones=@obs, Activo=@act " &
                      "WHERE ID_Ruta=@id"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@cod", txtCodigo.Text.Trim())
                cmd.Parameters.AddWithValue("@zona", txtNombreZona.Text.Trim())
                cmd.Parameters.AddWithValue("@dias", txtDiasReparto.Text.Trim())

                ' si no han elegido nada en el combo guardo NULL
                If cboAgencia.SelectedValue IsNot Nothing Then
                    cmd.Parameters.AddWithValue("@agencia", Convert.ToInt32(cboAgencia.SelectedValue))
                Else
                    cmd.Parameters.AddWithValue("@agencia", DBNull.Value)
                End If

                cmd.Parameters.AddWithValue("@obs", txtObservaciones.Text.Trim())
                cmd.Parameters.AddWithValue("@act", If(chkActivo.Checked, 1, 0))

                If _idRutaActual > 0 Then cmd.Parameters.AddWithValue("@id", _idRutaActual)

                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Ruta guardada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnNuevo.PerformClick()
            CargarRutas()
        Catch ex As Exception
            MessageBox.Show("Error al guardar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBorrar_Click(sender As Object, e As EventArgs) Handles btnBorrar.Click
        If _idRutaActual = 0 Then Return

        If MessageBox.Show("¿Eliminar definitivamente la ruta " & txtNombreZona.Text & "?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                Using cmd As New SQLiteCommand("DELETE FROM Rutas WHERE ID_Ruta = @id", c)
                    cmd.Parameters.AddWithValue("@id", _idRutaActual)
                    cmd.ExecuteNonQuery()
                End Using

                btnNuevo.PerformClick()
                CargarRutas()
            Catch ex As Exception
                MessageBox.Show("No se puede borrar porque se ha usado en algún pedido o cliente. Desactívala en su lugar desmarcando la casilla 'Ruta Activa'.", "Bloqueo de Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Try
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        _idRutaActual = 0
        txtCodigo.Clear() : txtNombreZona.Clear() : txtDiasReparto.Clear() : txtObservaciones.Clear()
        cboAgencia.SelectedIndex = -1
        chkActivo.Checked = True
        txtCodigo.Focus()
        dgvRutas.ClearSelection()
    End Sub

End Class
