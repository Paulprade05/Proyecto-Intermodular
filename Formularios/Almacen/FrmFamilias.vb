Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SQLite

Public Class FrmFamilias
    Private WithEvents txtCodigo As New TextBox()
    Private WithEvents txtNombre As New TextBox()
    Private WithEvents txtDescripcion As New TextBox()

    Private WithEvents btnGuardar As New Button()
    Private WithEvents btnBorrar As New Button()
    Private WithEvents btnNuevo As New Button()

    Private WithEvents dgvFamilias As New DataGridView()

    Private _idFamiliaActual As Integer = 0

    Private Sub FrmFamilias_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Gestión de Familias"
        Me.BackColor = Color.WhiteSmoke ' Fondo claro y limpio
        Me.WindowState = FormWindowState.Normal
        Me.Size = New Size(560, 520)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen

        ConstruirInterfaz()
        ConfigurarGrid()
        CargarFamilias()
    End Sub

    Private Sub ConstruirInterfaz()
        Dim margenIzq As Integer = 20
        Dim yFila1 As Integer = 20
        Dim yFila2 As Integer = 80

        ' FUNCIÓN PARA CREAR CAMPOS 
        Dim CrearCampo = Sub(textoLabel As String, ctrl As Control, x As Integer, y As Integer, w As Integer)
                             Dim lbl As New Label() With {
                                 .Text = textoLabel,
                                 .Location = New Point(x, y),
                                 .AutoSize = True,
                                 .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                                 .ForeColor = Color.FromArgb(50, 50, 50)
                             }
                             Me.Controls.Add(lbl)

                             ctrl.Bounds = New Rectangle(x, y + 23, w, 27)
                             ctrl.Font = New Font("Segoe UI", 10.5F)

                             If TypeOf ctrl Is TextBox Then
                                 Dim txt = DirectCast(ctrl, TextBox)
                                 txt.BorderStyle = BorderStyle.FixedSingle
                                 txt.BackColor = Color.White
                                 txt.ForeColor = Color.Black
                             End If

                             Me.Controls.Add(ctrl)
                         End Sub


        ' Fila 1: Codigo y Nombre alineados
        CrearCampo("Código", txtCodigo, margenIzq, yFila1, 100)
        CrearCampo("Nombre de la Familia", txtNombre, margenIzq + 120, yFila1, 380)

        ' Fila 2: Descripcion
        CrearCampo("Descripción (Opcional)", txtDescripcion, margenIzq, yFila2, 500)

        ' BOTONES
        Dim yBotones As Integer = 145
        ConfigurarBoton(btnGuardar, "Guardar", margenIzq, yBotones, Color.FromArgb(0, 120, 215))
        ConfigurarBoton(btnBorrar, "Borrar", margenIzq + 110, yBotones, Color.FromArgb(209, 52, 56))
        ConfigurarBoton(btnNuevo, "Nuevo", margenIzq + 220, yBotones, Color.FromArgb(40, 140, 90))

        'LINEA DIVISORIA
        Dim linea As New Label() With {
            .Bounds = New Rectangle(margenIzq, yBotones + 50, 500, 2),
            .BackColor = Color.FromArgb(200, 200, 200)
        }
        Me.Controls.Add(linea)

        dgvFamilias.Bounds = New Rectangle(margenIzq, yBotones + 70, 500, 215)
        dgvFamilias.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(dgvFamilias)
    End Sub

    Private Sub ConfigurarBoton(btn As Button, texto As String, x As Integer, y As Integer, colorFondo As Color)
        btn.Text = texto : btn.Bounds = New Rectangle(x, y, 100, 32)
        btn.BackColor = colorFondo : btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat : btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI", 10, FontStyle.Bold) : btn.Cursor = Cursors.Hand
        Me.Controls.Add(btn)
    End Sub



    Private Sub ConfigurarGrid()
        ' 1. Aplicamos diseño 
        Try
            FrmPresupuestos.EstilizarGrid(dgvFamilias)
        Catch ex As Exception
        End Try

        ' 2. Ajustes básicos
        dgvFamilias.AutoGenerateColumns = False
        dgvFamilias.AllowUserToAddRows = False
        dgvFamilias.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvFamilias.ReadOnly = True

        ' 3. Mapeo de columnas
        dgvFamilias.Columns.Clear()

        dgvFamilias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Familia", .DataPropertyName = "ID_Familia", .Visible = False})
        dgvFamilias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Codigo", .DataPropertyName = "Codigo", .HeaderText = "Código", .Width = 100})
        dgvFamilias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Nombre", .DataPropertyName = "Nombre", .HeaderText = "Nombre de Familia", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        dgvFamilias.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción", .Width = 180})
    End Sub

    Private Sub AjustarAltoTabla()
        If dgvFamilias Is Nothing Then Return

        ' 1. Calculamos lo que ocupan la cabecera y todas las filas
        Dim altoNecesario As Integer = dgvFamilias.ColumnHeadersHeight
        For Each fila As DataGridViewRow In dgvFamilias.Rows
            altoNecesario += fila.Height
        Next

        altoNecesario += 3

        ' 2. Calculamos el tope máximo 
        Dim altoMaximo As Integer = Me.ClientSize.Height - dgvFamilias.Top - 20

        ' 3. Ajustamos el alto real del control
        If altoNecesario > altoMaximo Then
            dgvFamilias.Height = altoMaximo
        Else
            dgvFamilias.Height = altoNecesario
        End If

        dgvFamilias.BackgroundColor = Me.BackColor
    End Sub

    ' LÓGICA DE BASE DE DATOS (SQLite)
    Private Sub CargarFamilias()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String = "SELECT * FROM Familias ORDER BY Codigo ASC"
            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvFamilias.DataSource = dt

                ' Ajustamos la altura de la tabla una vez cargados los datos
                AjustarAltoTabla()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar familias: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub



    Private Sub dgvFamilias_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvFamilias.CellClick
        If e.RowIndex >= 0 Then
            Dim fila As DataGridViewRow = dgvFamilias.Rows(e.RowIndex)
            _idFamiliaActual = Convert.ToInt32(fila.Cells("ID_Familia").Value)

            txtCodigo.Text = If(IsDBNull(fila.Cells("Codigo").Value), "", fila.Cells("Codigo").Value?.ToString())
            txtNombre.Text = If(IsDBNull(fila.Cells("Nombre").Value), "", fila.Cells("Nombre").Value?.ToString())
            txtDescripcion.Text = If(IsDBNull(fila.Cells("Descripcion").Value), "", fila.Cells("Descripcion").Value?.ToString())
        End If
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If String.IsNullOrWhiteSpace(txtCodigo.Text) OrElse String.IsNullOrWhiteSpace(txtNombre.Text) Then
            MessageBox.Show("El Código y el Nombre son obligatorios.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String
            If _idFamiliaActual = 0 Then
                sql = "INSERT INTO Familias (Codigo, Nombre, Descripcion) VALUES (@cod, @nombre, @desc)"
            Else
                sql = "UPDATE Familias SET Codigo=@cod, Nombre=@nombre, Descripcion=@desc WHERE ID_Familia=@id"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@cod", txtCodigo.Text.Trim())
                cmd.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim())
                cmd.Parameters.AddWithValue("@desc", txtDescripcion.Text.Trim())
                If _idFamiliaActual > 0 Then cmd.Parameters.AddWithValue("@id", _idFamiliaActual)
                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Familia guardada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnNuevo.PerformClick()
            CargarFamilias()
        Catch ex As Exception
            MessageBox.Show("Error al guardar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBorrar_Click(sender As Object, e As EventArgs) Handles btnBorrar.Click
        If _idFamiliaActual = 0 Then
            MessageBox.Show("Selecciona una familia de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If MessageBox.Show("¿Eliminar definitivamente la familia " & txtNombre.Text & "?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                Using cmd As New SQLiteCommand("DELETE FROM Familias WHERE ID_Familia = @id", c)
                    cmd.Parameters.AddWithValue("@id", _idFamiliaActual)
                    cmd.ExecuteNonQuery()
                End Using

                btnNuevo.PerformClick()
                CargarFamilias()
            Catch ex As Exception
                MessageBox.Show("Error al borrar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        _idFamiliaActual = 0
        txtCodigo.Clear()
        txtNombre.Clear()
        txtDescripcion.Clear()
        txtCodigo.Focus()
        dgvFamilias.ClearSelection()
    End Sub

End Class
