Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SQLite

Public Class FrmArticulos
    Private WithEvents txtCodigo As New TextBox()
    Private WithEvents txtCodigoBarras As New TextBox()
    Private WithEvents txtDescripcion As New TextBox()
    Private WithEvents cboFamilia As New ComboBox()
    Private WithEvents cboProveedor As New ComboBox()
    Private WithEvents txtPrecioCompra As New TextBox()
    Private WithEvents txtPrecioVenta As New TextBox()
    Private WithEvents cboIVA As New ComboBox()
    Private WithEvents txtStockActual As New TextBox()
    Private WithEvents txtStockMinimo As New TextBox()
    Private WithEvents txtObservaciones As New TextBox()
    Private WithEvents btnGuardar As New Button()
    Private WithEvents btnBorrar As New Button()
    Private WithEvents btnNuevo As New Button()
    Private WithEvents dgvArticulos As New DataGridView()
    Private _idArticuloActual As Integer = 0
    Private _filtrarSinStockAlAbrir As Boolean = False

    ' este constructor lo uso cuando quiero abrir la ventana solo viendo los que no tienen stock
    Public Sub New(Optional soloSinStock As Boolean = False)
        InitializeComponent()
        _filtrarSinStockAlAbrir = soloSinStock
    End Sub

    Private Sub FrmArticulos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Gestión de Artículos"
        Me.BackColor = Color.FromArgb(70, 75, 80)
        Me.MinimumSize = New Size(1000, 720)

        ConstruirInterfaz()
        ConfigurarGrid()
        CargarDesplegables()

        ' segun el flag, o cargo todos o solo los que estan a 0
        If _filtrarSinStockAlAbrir Then
            CargarArticulos("WHERE StockActual <= 0")
        Else
            CargarArticulos()
        End If
    End Sub

    ' los mismos colores que en las otras pantallas para que no quede raro
    Private Shared ReadOnly COLOR_FONDO As Color = Color.FromArgb(70, 75, 80)
    Private Shared ReadOnly COLOR_BANDA As Color = Color.FromArgb(40, 50, 70)
    Private Shared ReadOnly COLOR_PANEL_TOTALES As Color = Color.FromArgb(25, 30, 40)
    Private Shared ReadOnly COLOR_ACENTO As Color = Color.FromArgb(0, 150, 255)
    Private Shared ReadOnly COLOR_TEXTO_SECUNDARIO As Color = Color.FromArgb(170, 180, 195)
    Private Shared ReadOnly COLOR_LINEA_DIVISORIA As Color = Color.FromArgb(120, 130, 140)
    Private Shared ReadOnly COLOR_SEPARADOR_GRUPO As Color = Color.FromArgb(95, 105, 120)

    ' colores de los botones segun lo que hagan (azul guardar, rojo borrar, etc)
    Private Shared ReadOnly BTN_AZUL_PRIMARIO As Color = Color.FromArgb(0, 120, 215)
    Private Shared ReadOnly BTN_ROJO_PELIGRO As Color = Color.FromArgb(209, 52, 56)
    Private Shared ReadOnly BTN_VERDE_AÑADIR As Color = Color.FromArgb(40, 140, 90)
    Private Shared ReadOnly BTN_GRIS_NEUTRO As Color = Color.FromArgb(85, 85, 85)

    Private Sub ConstruirInterfaz()
        Dim margenIzq As Integer = 30
        Dim anchoForm As Integer = Me.ClientSize.Width

        ' la banda azul de arriba con el titulo
        Dim alturaBanda As Integer = 60

        Dim bandaSuperior As New Panel() With {
            .Name = "BandaSuperior",
            .BackColor = COLOR_BANDA,
            .Bounds = New Rectangle(0, 0, anchoForm, alturaBanda),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }
        Me.Controls.Add(bandaSuperior)
        bandaSuperior.SendToBack()

        Dim lblTitulo As New Label() With {
            .Name = "LblTituloDoc",
            .Text = "GESTIÓN DE ARTÍCULOS",
            .AutoSize = False,
            .BackColor = COLOR_BANDA,
            .ForeColor = Color.White,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .Bounds = New Rectangle(margenIzq, 0, 600, alturaBanda)
        }
        Me.Controls.Add(lblTitulo)
        lblTitulo.BringToFront()

        ' aqui va el formulario, lo divido en 3 filas para que se vea ordenado
        Dim yInicio As Integer = alturaBanda + 20

        Dim yFila1 As Integer = yInicio
        Dim yFila2 As Integer = yInicio + 60
        Dim yFila3 As Integer = yInicio + 120

        ' fila 1: codigos y descripcion
        CrearCampo("Cód. Referencia", txtCodigo, margenIzq, yFila1, 130)
        CrearCampo("Cód. Barras", txtCodigoBarras, 180, yFila1, 150)
        CrearCampo("Descripción del Artículo", txtDescripcion, 350, yFila1, 460)

        ' fila 2: familia, proveedor y los precios
        CrearCampo("Familia", cboFamilia, margenIzq, yFila2, 180)
        cboFamilia.DropDownStyle = ComboBoxStyle.DropDownList

        CrearCampo("Proveedor", cboProveedor, 230, yFila2, 200)
        cboProveedor.DropDownStyle = ComboBoxStyle.DropDownList

        CrearCampo("Coste (€)", txtPrecioCompra, 450, yFila2, 100)
        CrearCampo("P.V.P. (€)", txtPrecioVenta, 570, yFila2, 100)

        CrearCampo("% IVA", cboIVA, 690, yFila2, 120)
        cboIVA.Items.AddRange({"21", "10", "4", "0"})
        cboIVA.DropDownStyle = ComboBoxStyle.DropDownList

        ' fila 3: stock y un cuadro de notas
        CrearCampo("Stock Actual", txtStockActual, margenIzq, yFila3, 100)
        CrearCampo("Stock Mín.", txtStockMinimo, 150, yFila3, 100)

        CrearCampo("Observaciones / Detalles Técnicos", txtObservaciones, 270, yFila3, 540)
        txtObservaciones.Height = 45 : txtObservaciones.Multiline = True

        ' una rayita gris para separar el formulario de la tabla
        Dim yLinea As Integer = yFila3 + 80
        Dim linea As New Label() With {
            .Name = "LineaDivisoria",
            .Bounds = New Rectangle(margenIzq, yLinea, anchoForm - (margenIzq * 2), 2),
            .BackColor = COLOR_LINEA_DIVISORIA,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }
        Me.Controls.Add(linea)

        ' la tabla con todos los articulos
        Dim yTabla As Integer = yLinea + 18
        Dim altoTabla As Integer = Me.ClientSize.Height - yTabla - 80
        dgvArticulos.Bounds = New Rectangle(margenIzq, yTabla, anchoForm - (margenIzq * 2), altoTabla)
        dgvArticulos.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        dgvArticulos.BackgroundColor = Me.BackColor
        dgvArticulos.BorderStyle = BorderStyle.None
        Me.Controls.Add(dgvArticulos)

        ' los 3 botones de abajo
        Dim yBotones As Integer = dgvArticulos.Bottom + 20

        EstilizarBoton(btnGuardar, "Guardar", margenIzq, yBotones, BTN_AZUL_PRIMARIO, Color.White)
        EstilizarBoton(btnBorrar, "Borrar", margenIzq + 110, yBotones, BTN_ROJO_PELIGRO, Color.White)
        EstilizarBoton(btnNuevo, "Nuevo", margenIzq + 220, yBotones, BTN_AZUL_PRIMARIO, Color.White)

        Dim botonesAbajo As Control() = {btnGuardar, btnBorrar, btnNuevo}
        For Each b In botonesAbajo : b.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left : Next
    End Sub

    ' me hago una funcion para no repetir el rollo de label+caja una y otra vez
    Private Sub CrearCampo(textoLabel As String, ctrl As Control, x As Integer, y As Integer, w As Integer)
        Dim lbl As New Label() With {
            .Text = textoLabel,
            .Location = New Point(x, y),
            .AutoSize = True,
            .Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            .ForeColor = Color.WhiteSmoke,
            .BackColor = Color.Transparent
        }
        Me.Controls.Add(lbl)

        ctrl.Bounds = New Rectangle(x, y + 23, w, 27)
        ctrl.Font = New Font("Segoe UI", 10.5F)

        If TypeOf ctrl Is TextBox Then
            DirectCast(ctrl, TextBox).BorderStyle = BorderStyle.FixedSingle
        End If

        ' las pego a la izquierda para que no bailen al cambiar tamaño
        ctrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left

        Me.Controls.Add(ctrl)
    End Sub

    ' otra funcion para los botones, asi todos quedan iguales sin copiar codigo
    Private Sub EstilizarBoton(btn As Button, texto As String, x As Integer, y As Integer, bg As Color, fg As Color)
        btn.Text = texto
        btn.Location = New Point(x, y)
        btn.Size = New Size(100, 35)
        btn.FlatStyle = FlatStyle.Flat
        btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        btn.Cursor = Cursors.Hand
        btn.BackColor = bg
        btn.ForeColor = fg
        btn.FlatAppearance.BorderSize = 0
        Me.Controls.Add(btn)
    End Sub

    Private Sub ConfigurarGrid()
        Try
            FrmPresupuestos.EstilizarGrid(dgvArticulos)
        Catch ex As Exception
        End Try

        ' cosas basicas de la tabla
        dgvArticulos.AutoGenerateColumns = False
        dgvArticulos.AllowUserToAddRows = False
        dgvArticulos.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvArticulos.ReadOnly = True

        ' aqui digo que columnas quiero y que datos van en cada una
        dgvArticulos.Columns.Clear()

        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Articulo", .DataPropertyName = "ID_Articulo", .Visible = False})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CodigoReferencia", .DataPropertyName = "CodigoReferencia", .HeaderText = "Ref.", .Width = 100})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CodigoBarras", .DataPropertyName = "CodigoBarras", .HeaderText = "EAN / Barras", .Width = 120})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PrecioCoste", .DataPropertyName = "PrecioCoste", .HeaderText = "Coste", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "C2", .Alignment = DataGridViewContentAlignment.MiddleRight}})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PrecioVenta", .DataPropertyName = "PrecioVenta", .HeaderText = "P.V.P.", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "C2", .Alignment = DataGridViewContentAlignment.MiddleRight}})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "StockActual", .DataPropertyName = "StockActual", .HeaderText = "Stock", .Width = 80, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})

        ' estas no se ven pero las necesito para guardar luego
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Familia", .DataPropertyName = "ID_Familia", .Visible = False})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_ProveedorHabitual", .DataPropertyName = "ID_ProveedorHabitual", .Visible = False})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "TipoIVA", .DataPropertyName = "TipoIVA", .Visible = False})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "StockMinimo", .DataPropertyName = "StockMinimo", .Visible = False})
        dgvArticulos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Observaciones", .DataPropertyName = "Observaciones", .Visible = False})
    End Sub
    Private Sub AjustarAltoTabla()
        If dgvArticulos Is Nothing Then Return

        ' sumo lo que mide la cabecera mas todas las filas
        Dim altoNecesario As Integer = dgvArticulos.ColumnHeadersHeight
        For Each fila As DataGridViewRow In dgvArticulos.Rows
            altoNecesario += fila.Height
        Next
        ' un pelin mas por si acaso
        altoNecesario += 3

        ' tope para que no se salga de la ventana
        Dim altoMaximo As Integer = Me.ClientSize.Height - dgvArticulos.Top - 80

        ' si pasa del tope lo capo, si no lo dejo
        If altoNecesario > altoMaximo Then
            dgvArticulos.Height = altoMaximo
        Else
            dgvArticulos.Height = altoNecesario
        End If

        dgvArticulos.BackgroundColor = Me.BackColor
    End Sub

    ' si maximizo la ventana hay que recalcular el alto, si no queda feo
    Private Sub FrmArticulos_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If dgvArticulos IsNot Nothing AndAlso dgvArticulos.RowCount > 0 Then
            AjustarAltoTabla()
        End If
    End Sub

    Private Sub CargarArticulos(Optional filtroSQL As String = "")
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' si me pasan filtro lo pego al where, si no nada
            Dim sql As String = "SELECT * FROM Articulos " & filtroSQL & " ORDER BY ID_Articulo ASC"

            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvArticulos.DataSource = dt
                AjustarAltoTabla()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar artículos: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub CargarDesplegables()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' las familias para el combo
            Dim daFam As New SQLiteDataAdapter("SELECT ID_Familia, Nombre FROM Familias", c)
            Dim dtFam As New DataTable()
            daFam.Fill(dtFam)
            cboFamilia.DataSource = dtFam
            cboFamilia.DisplayMember = "Nombre"
            cboFamilia.ValueMember = "ID_Familia"

            ' los proveedores, muestro el nombre pero por dentro guardo el codigo
            Dim sqlProv As String = "SELECT CodigoProveedor, NombreFiscal FROM Proveedores ORDER BY NombreFiscal"
            Using daProv As New SQLiteDataAdapter(sqlProv, c)
                Dim dtProv As New DataTable()
                daProv.Fill(dtProv)

                cboProveedor.DataSource = dtProv
                cboProveedor.DisplayMember = "NombreFiscal"
                cboProveedor.ValueMember = "CodigoProveedor"
                cboProveedor.SelectedIndex = -1
            End Using

        Catch ex As Exception
            MessageBox.Show("Error al cargar combos: " & ex.Message)
        End Try
    End Sub
    Private Sub dgvArticulos_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvArticulos.CellClick
        If e.RowIndex >= 0 Then
            Try
                Dim fila As DataGridViewRow = dgvArticulos.Rows(e.RowIndex)

                _idArticuloActual = Convert.ToInt32(fila.Cells("ID_Articulo").Value)

                txtCodigo.Text = If(IsDBNull(fila.Cells("CodigoReferencia").Value), "", fila.Cells("CodigoReferencia").Value?.ToString())
                txtCodigoBarras.Text = If(IsDBNull(fila.Cells("CodigoBarras").Value), "", fila.Cells("CodigoBarras").Value?.ToString())
                txtDescripcion.Text = If(IsDBNull(fila.Cells("Descripcion").Value), "", fila.Cells("Descripcion").Value?.ToString())
                txtPrecioCompra.Text = If(IsDBNull(fila.Cells("PrecioCoste").Value), "", fila.Cells("PrecioCoste").Value?.ToString())
                txtPrecioVenta.Text = If(IsDBNull(fila.Cells("PrecioVenta").Value), "", fila.Cells("PrecioVenta").Value?.ToString())
                txtStockActual.Text = If(IsDBNull(fila.Cells("StockActual").Value), "", fila.Cells("StockActual").Value?.ToString())
                txtStockMinimo.Text = If(IsDBNull(fila.Cells("StockMinimo").Value), "0", fila.Cells("StockMinimo").Value?.ToString())
                txtObservaciones.Text = If(IsDBNull(fila.Cells("Observaciones").Value), "", fila.Cells("Observaciones").Value?.ToString())

                Dim iva = If(IsDBNull(fila.Cells("TipoIVA").Value), "21", fila.Cells("TipoIVA").Value?.ToString())
                cboIVA.Text = iva

                Dim idFam = fila.Cells("ID_Familia").Value
                If Not IsDBNull(idFam) AndAlso idFam IsNot Nothing Then
                    cboFamilia.SelectedValue = Convert.ToInt32(idFam)
                Else
                    cboFamilia.SelectedIndex = -1
                End If

                Dim idProv = fila.Cells("ID_ProveedorHabitual").Value

                If Not IsDBNull(idProv) AndAlso idProv IsNot Nothing AndAlso idProv.ToString().Trim() <> "" Then
                    Dim valorProv As String = idProv.ToString().Trim()


                    cboProveedor.SelectedValue = valorProv

                    ' a veces el SelectedValue no pilla el item, asi que lo busco a mano
                    ' raro pero funciona
                    If cboProveedor.SelectedIndex = -1 Then
                        For i As Integer = 0 To cboProveedor.Items.Count - 1
                            Dim filaVista As DataRowView = TryCast(cboProveedor.Items(i), DataRowView)
                            If filaVista IsNot Nothing AndAlso filaVista("CodigoProveedor").ToString() = valorProv Then
                                cboProveedor.SelectedIndex = i
                                Exit For
                            End If
                        Next
                    End If
                Else
                    cboProveedor.SelectedIndex = -1
                End If

            Catch ex As Exception
                MessageBox.Show("Error al seleccionar la celda: " & ex.Message)
            End Try
        End If
    End Sub
    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If String.IsNullOrWhiteSpace(txtCodigo.Text) OrElse String.IsNullOrWhiteSpace(txtDescripcion.Text) Then
            MessageBox.Show("La Referencia y la Descripción son obligatorias.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim c = ConexionBD.GetConnection()
        Dim trans As SQLiteTransaction = Nothing
        Try
            If c.State <> ConnectionState.Open Then c.Open()
            trans = c.BeginTransaction()

            ' antes aqui ponia un PRAGMA foreign_keys = OFF y el profe me dijo que NO
            ' porque la conexion es la misma para todo el programa y se quedaba sin FKs
            ' lo he quitado. si peta una FK al actualizar es que hay un dato chungo, mejor verlo

            Dim pCoste As Decimal = 0 : Decimal.TryParse(txtPrecioCompra.Text, pCoste)
            Dim pVenta As Decimal = 0 : Decimal.TryParse(txtPrecioVenta.Text, pVenta)
            Dim stockNuevo As Decimal = 0 : Decimal.TryParse(txtStockActual.Text, stockNuevo)
            Dim sMinimo As Decimal = 0 : Decimal.TryParse(txtStockMinimo.Text, sMinimo)
            Dim iva As Integer = 21 : Integer.TryParse(cboIVA.Text, iva)

            Dim idFamilia As Object = If(cboFamilia.SelectedValue IsNot Nothing, cboFamilia.SelectedValue, DBNull.Value)
            Dim idProveedor As Object = If(cboProveedor.SelectedValue IsNot Nothing, cboProveedor.SelectedValue, DBNull.Value)

            ' si estoy editando me guardo el stock viejo para luego ver si ha cambiado
            Dim stockAnterior As Decimal = 0
            Dim esEdicion As Boolean = (_idArticuloActual > 0)
            If esEdicion Then
                Using cmdAnt As New SQLiteCommand("SELECT StockActual FROM Articulos WHERE ID_Articulo = @id", c, trans)
                    cmdAnt.Parameters.AddWithValue("@id", _idArticuloActual)
                    Dim res = cmdAnt.ExecuteScalar()
                    If res IsNot Nothing AndAlso Not IsDBNull(res) Then stockAnterior = Convert.ToDecimal(res)
                End Using
            End If

            Dim sql As String
            If Not esEdicion Then
                sql = "INSERT INTO Articulos (CodigoReferencia, CodigoBarras, Descripcion, ID_Familia, ID_ProveedorHabitual, PrecioCoste, PrecioVenta, TipoIVA, StockActual, StockMinimo, Observaciones) " &
                      "VALUES (@cod, @barras, @desc, @fam, @prov, @pcoste, @pventa, @iva, @stock, @smin, @obs); SELECT last_insert_rowid();"
            Else
                sql = "UPDATE Articulos SET CodigoReferencia=@cod, CodigoBarras=@barras, Descripcion=@desc, ID_Familia=@fam, ID_ProveedorHabitual=@prov, PrecioCoste=@pcoste, PrecioVenta=@pventa, TipoIVA=@iva, StockActual=@stock, StockMinimo=@smin, Observaciones=@obs " &
                      "WHERE ID_Articulo=@id"
            End If

            Dim idArticuloFinal As Integer = _idArticuloActual

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Transaction = trans
                cmd.Parameters.AddWithValue("@cod", txtCodigo.Text.Trim())
                cmd.Parameters.AddWithValue("@barras", txtCodigoBarras.Text.Trim())
                cmd.Parameters.AddWithValue("@desc", txtDescripcion.Text.Trim())
                cmd.Parameters.AddWithValue("@fam", idFamilia)
                cmd.Parameters.AddWithValue("@prov", idProveedor)
                cmd.Parameters.AddWithValue("@pcoste", pCoste)
                cmd.Parameters.AddWithValue("@pventa", pVenta)
                cmd.Parameters.AddWithValue("@iva", iva)
                cmd.Parameters.AddWithValue("@stock", stockNuevo)
                cmd.Parameters.AddWithValue("@smin", sMinimo)
                cmd.Parameters.AddWithValue("@obs", txtObservaciones.Text)

                If esEdicion Then
                    cmd.Parameters.AddWithValue("@id", _idArticuloActual)
                    cmd.ExecuteNonQuery()
                Else
                    idArticuloFinal = Convert.ToInt32(cmd.ExecuteScalar())
                End If
            End Using

            ' si el stock ha cambiado apunto el ajuste en movimientos
            ' para articulos nuevos no, porque el stock inicial no es entrada ni salida, es lo que hay
            If esEdicion AndAlso stockNuevo <> stockAnterior Then
                Dim variacion As Decimal = stockNuevo - stockAnterior
                Dim tipoMov As String = If(variacion > 0, EstadosDocumento.MOV_AJUSTE_MAS, EstadosDocumento.MOV_AJUSTE_MENOS)
                Dim cantidad As Decimal = Math.Abs(variacion)

                Dim idUsr As Object = If(ComunSesionActual.IdUsuario > 0, CType(ComunSesionActual.IdUsuario, Object), DBNull.Value)

                Dim sqlMov As String = "INSERT INTO MovimientosAlmacen (Fecha, ID_Articulo, TipoMovimiento, Cantidad, StockResultante, DocumentoReferencia, ID_Usuario) " &
                                       "VALUES (@fecha, @idArt, @tipo, @cant, @stockRes, @doc, @usr)"
                Using cmdMov As New SQLiteCommand(sqlMov, c, trans)
                    cmdMov.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmdMov.Parameters.AddWithValue("@idArt", idArticuloFinal)
                    cmdMov.Parameters.AddWithValue("@tipo", tipoMov)
                    cmdMov.Parameters.AddWithValue("@cant", cantidad)
                    cmdMov.Parameters.AddWithValue("@stockRes", stockNuevo)
                    cmdMov.Parameters.AddWithValue("@doc", "Ajuste manual desde ficha de artículo")
                    cmdMov.Parameters.AddWithValue("@usr", idUsr)
                    cmdMov.ExecuteNonQuery()
                End Using
            End If

            trans.Commit()
            MessageBox.Show("Artículo guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnNuevo.PerformClick()
            CargarArticulos()

        Catch ex As Exception
            If trans IsNot Nothing Then trans.Rollback()
            LogErrores.Registrar("frmArticulos.Guardar", ex)
            MessageBox.Show("Error al guardar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBorrar_Click(sender As Object, e As EventArgs) Handles btnBorrar.Click
        If _idArticuloActual = 0 Then
            MessageBox.Show("Selecciona un artículo de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If MessageBox.Show("¿Eliminar definitivamente el artículo seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                Using cmd As New SQLiteCommand("DELETE FROM Articulos WHERE ID_Articulo = @id", c)
                    cmd.Parameters.AddWithValue("@id", _idArticuloActual)
                    cmd.ExecuteNonQuery()
                End Using

                btnNuevo.PerformClick()
                CargarArticulos()
            Catch ex As Exception
                MessageBox.Show("Error al borrar: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        _idArticuloActual = 0
        txtCodigo.Clear() : txtCodigoBarras.Clear() : txtDescripcion.Clear()
        txtPrecioCompra.Clear() : txtPrecioVenta.Clear() : txtStockActual.Clear()
        txtStockMinimo.Clear() : txtObservaciones.Clear()
        cboIVA.SelectedIndex = 0
        cboFamilia.SelectedIndex = -1
        cboProveedor.SelectedIndex = -1
        txtCodigo.Focus()
        dgvArticulos.ClearSelection()
    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or &H2000000
            Return cp
        End Get
    End Property
End Class
