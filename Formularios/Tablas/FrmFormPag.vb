Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SQLite

Public Class FrmFormPag

    ' los controles de la pantalla, declarados a mano
    Private WithEvents txtCodigo As New TextBox()
    Private WithEvents txtDescripcion As New TextBox()
    Private WithEvents chkActivo As New CheckBox()

    Private WithEvents txtNumVencimientos As New TextBox()
    Private WithEvents txtDiasPrimer As New TextBox()
    Private WithEvents txtDiasEntre As New TextBox()

    Private WithEvents btnGuardar As New Button()
    Private WithEvents btnBorrar As New Button()
    Private WithEvents btnNuevo As New Button()

    Private WithEvents dgvFormasPago As New DataGridView()
    Private _idFormaActual As Integer = 0

    ' al cargar el form preparo todo
    Private Sub FrmFormasPago_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Gestión de Formas de Pago"
        Me.BackColor = Color.WhiteSmoke
        Me.Size = New Size(650, 550) ' tamaño que me ha cuadrado tras probar
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen

        ConstruirInterfaz()
        ConfigurarGrid()
        CargarFormasPago()
    End Sub

    ' aqui voy poniendo los textbox uno a uno
    Private Sub ConstruirInterfaz()
        Dim margenIzq As Integer = 20
        Dim y1 As Integer = 20
        Dim y2 As Integer = 85

        ' lambdita para no escribir 50 veces lo mismo
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

        ' primera fila, lo basico
        CrearCampo("Código", txtCodigo, margenIzq, y1, 100)
        CrearCampo("Descripción (Ej: Transferencia 30/60)", txtDescripcion, 140, y1, 330)

        chkActivo.Text = "Activa"
        chkActivo.Checked = True
        chkActivo.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        chkActivo.Bounds = New Rectangle(500, y1 + 25, 100, 25)
        Me.Controls.Add(chkActivo)

        ' fila 2 con los numeros para calcular vencimientos
        CrearCampo("Nº Vencimientos", txtNumVencimientos, margenIzq, y2, 120)
        CrearCampo("Días 1º Venc.", txtDiasPrimer, 160, y2, 120)
        CrearCampo("Días entre Venc.", txtDiasEntre, 300, y2, 120)

        ' por defecto pongo al contado (1 pago, 0 dias, 0 entre medias)
        txtNumVencimientos.Text = "1"
        txtDiasPrimer.Text = "0"
        txtDiasEntre.Text = "0"

        ' los botones
        Dim yBotones As Integer = 150
        ConfigurarBoton(btnGuardar, "Guardar", margenIzq, yBotones, Color.FromArgb(0, 120, 215))
        ConfigurarBoton(btnBorrar, "Borrar", margenIzq + 110, yBotones, Color.FromArgb(209, 52, 56))
        ConfigurarBoton(btnNuevo, "Nuevo", margenIzq + 220, yBotones, Color.FromArgb(40, 140, 90))

        ' una raya para separar visualmente
        Dim linea As New Label() With {.Bounds = New Rectangle(margenIzq, yBotones + 50, Me.ClientSize.Width - 55, 2), .BackColor = Color.FromArgb(200, 200, 200), .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right}
        Me.Controls.Add(linea)

        ' la tabla con todo
        dgvFormasPago.Bounds = New Rectangle(margenIzq, yBotones + 70, Me.ClientSize.Width - 55, 250)
        dgvFormasPago.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(dgvFormasPago)
    End Sub

    Private Sub ConfigurarBoton(btn As Button, texto As String, x As Integer, y As Integer, colorFondo As Color)
        btn.Text = texto : btn.Bounds = New Rectangle(x, y, 100, 32)
        btn.BackColor = colorFondo : btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat : btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI", 10, FontStyle.Bold) : btn.Cursor = Cursors.Hand
        Me.Controls.Add(btn)
    End Sub

    ' configuro las columnas del grid
    Private Sub ConfigurarGrid()
        Try
            FrmPresupuestos.EstilizarGrid(dgvFormasPago)
        Catch ex As Exception
        End Try

        dgvFormasPago.AutoGenerateColumns = False
        dgvFormasPago.AllowUserToAddRows = False
        dgvFormasPago.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvFormasPago.ReadOnly = True

        dgvFormasPago.Columns.Clear()
        dgvFormasPago.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_FormaPago", .DataPropertyName = "ID_FormaPago", .Visible = False})
        dgvFormasPago.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Codigo", .DataPropertyName = "Codigo", .HeaderText = "Código", .Width = 90})
        dgvFormasPago.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

        dgvFormasPago.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NumeroVencimientos", .DataPropertyName = "NumeroVencimientos", .HeaderText = "Nº Pagos", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
        dgvFormasPago.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "DiasPrimerVencimiento", .DataPropertyName = "DiasPrimerVencimiento", .Visible = False})
        dgvFormasPago.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "DiasEntreVencimientos", .DataPropertyName = "DiasEntreVencimientos", .Visible = False})

        Dim colActivo As New DataGridViewCheckBoxColumn() With {.Name = "Activo", .DataPropertyName = "Activo", .HeaderText = "Activo", .Width = 60}
        dgvFormasPago.Columns.Add(colActivo)
    End Sub

    Private Sub AjustarAltoTabla()
        If dgvFormasPago Is Nothing Then Return
        Dim altoNecesario As Integer = dgvFormasPago.ColumnHeadersHeight
        For Each fila As DataGridViewRow In dgvFormasPago.Rows
            altoNecesario += fila.Height
        Next
        altoNecesario += 3
        Dim altoMaximo As Integer = Me.ClientSize.Height - dgvFormasPago.Top - 20
        dgvFormasPago.Height = If(altoNecesario > altoMaximo, altoMaximo, altoNecesario)
        dgvFormasPago.BackgroundColor = Me.BackColor
    End Sub

    Private Sub CargarFormasPago()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String = "SELECT * FROM FormasPago ORDER BY Codigo ASC"
            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvFormasPago.DataSource = dt
                AjustarAltoTabla()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar formas de pago: " & ex.Message)
        End Try
    End Sub

    Private Sub dgvFormasPago_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvFormasPago.CellClick
        If e.RowIndex >= 0 Then
            Dim fila As DataGridViewRow = dgvFormasPago.Rows(e.RowIndex)
            _idFormaActual = Convert.ToInt32(fila.Cells("ID_FormaPago").Value)

            txtCodigo.Text = If(IsDBNull(fila.Cells("Codigo").Value), "", fila.Cells("Codigo").Value?.ToString())
            txtDescripcion.Text = If(IsDBNull(fila.Cells("Descripcion").Value), "", fila.Cells("Descripcion").Value?.ToString())
            txtNumVencimientos.Text = If(IsDBNull(fila.Cells("NumeroVencimientos").Value), "1", fila.Cells("NumeroVencimientos").Value?.ToString())
            txtDiasPrimer.Text = If(IsDBNull(fila.Cells("DiasPrimerVencimiento").Value), "0", fila.Cells("DiasPrimerVencimiento").Value?.ToString())
            txtDiasEntre.Text = If(IsDBNull(fila.Cells("DiasEntreVencimientos").Value), "0", fila.Cells("DiasEntreVencimientos").Value?.ToString())

            chkActivo.Checked = Not IsDBNull(fila.Cells("Activo").Value) AndAlso (fila.Cells("Activo").Value.ToString() = "1" OrElse fila.Cells("Activo").Value.ToString().ToLower() = "true")
        End If
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If String.IsNullOrWhiteSpace(txtCodigo.Text) OrElse String.IsNullOrWhiteSpace(txtDescripcion.Text) Then
            MessageBox.Show("El Código y la Descripción son obligatorios.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String
            If _idFormaActual = 0 Then
                sql = "INSERT INTO FormasPago (Codigo, Descripcion, NumeroVencimientos, DiasPrimerVencimiento, DiasEntreVencimientos, Activo) " &
                      "VALUES (@cod, @desc, @numV, @dias1, @diasE, @act)"
            Else
                sql = "UPDATE FormasPago SET Codigo=@cod, Descripcion=@desc, NumeroVencimientos=@numV, DiasPrimerVencimiento=@dias1, DiasEntreVencimientos=@diasE, Activo=@act " &
                      "WHERE ID_FormaPago=@id"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@cod", txtCodigo.Text.Trim())
                cmd.Parameters.AddWithValue("@desc", txtDescripcion.Text.Trim())

                ' OJO: si el usuario lo deja vacio el TryParse lo deja en 0 y no peta
                Dim numV As Integer = 1 : Integer.TryParse(txtNumVencimientos.Text, numV)
                Dim dias1 As Integer = 0 : Integer.TryParse(txtDiasPrimer.Text, dias1)
                Dim diasE As Integer = 0 : Integer.TryParse(txtDiasEntre.Text, diasE)

                cmd.Parameters.AddWithValue("@numV", numV)
                cmd.Parameters.AddWithValue("@dias1", dias1)
                cmd.Parameters.AddWithValue("@diasE", diasE)
                cmd.Parameters.AddWithValue("@act", If(chkActivo.Checked, 1, 0))

                If _idFormaActual > 0 Then cmd.Parameters.AddWithValue("@id", _idFormaActual)

                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Forma de pago guardada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnNuevo.PerformClick()
            CargarFormasPago()
        Catch ex As Exception
            MessageBox.Show("Error al guardar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBorrar_Click(sender As Object, e As EventArgs) Handles btnBorrar.Click
        If _idFormaActual = 0 Then Return

        If MessageBox.Show("¿Eliminar la forma de pago " & txtDescripcion.Text & "?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                Using cmd As New SQLiteCommand("DELETE FROM FormasPago WHERE ID_FormaPago = @id", c)
                    cmd.Parameters.AddWithValue("@id", _idFormaActual)
                    cmd.ExecuteNonQuery()
                End Using

                btnNuevo.PerformClick()
                CargarFormasPago()
            Catch ex As Exception
                MessageBox.Show("No se puede borrar porque hay facturas usando esta forma de pago. Desmárcala como 'Activa' en su lugar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Try
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        _idFormaActual = 0
        txtCodigo.Clear() : txtDescripcion.Clear()
        txtNumVencimientos.Text = "1" : txtDiasPrimer.Text = "0" : txtDiasEntre.Text = "0"
        chkActivo.Checked = True
        txtCodigo.Focus()
        dgvFormasPago.ClearSelection()
    End Sub

End Class
