Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data.SQLite

Public Class FrmMovimientos

    ' 1. DECLARACIÓN DE CONTROLES (Filtros y Tabla)
    Private WithEvents cboFiltroArticulo As New ComboBox()
    Private WithEvents dtpDesde As New DateTimePicker()
    Private WithEvents dtpHasta As New DateTimePicker()

    Private WithEvents btnFiltrar As New Button()
    Private WithEvents btnLimpiar As New Button()

    Private WithEvents dgvMovimientos As New DataGridView()

    ' 2. INICIALIZACIÓN
    Private Sub FrmMovimientosAlmacen_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Historial de Movimientos de Almacén"
        Me.BackColor = Color.FromArgb(70, 75, 80) ' Tema oscuro premium
        Me.MinimumSize = New Size(1050, 670)

        ' Ajustamos las fechas por defecto (Últimos 30 días)
        dtpDesde.Format = DateTimePickerFormat.Short
        dtpHasta.Format = DateTimePickerFormat.Short
        dtpDesde.Value = DateTime.Today.AddDays(-30)
        dtpHasta.Value = DateTime.Today

        ConstruirInterfaz()
        ConfigurarGrid()

        CargarComboArticulos()
        CargarMovimientos() ' Carga inicial
    End Sub

    ' 3. CONSTRUCTOR DE LA INTERFAZ (Diseño Modernizado)

    ' --- PALETA DE COLORES CENTRALIZADA (idéntica al resto de módulos) ---
    Private Shared ReadOnly COLOR_FONDO As Color = Color.FromArgb(70, 75, 80)
    Private Shared ReadOnly COLOR_BANDA As Color = Color.FromArgb(40, 50, 70)
    Private Shared ReadOnly COLOR_PANEL_TOTALES As Color = Color.FromArgb(25, 30, 40)
    Private Shared ReadOnly COLOR_ACENTO As Color = Color.FromArgb(0, 150, 255)
    Private Shared ReadOnly COLOR_TEXTO_SECUNDARIO As Color = Color.FromArgb(170, 180, 195)
    Private Shared ReadOnly COLOR_LINEA_DIVISORIA As Color = Color.FromArgb(120, 130, 140)
    Private Shared ReadOnly COLOR_SEPARADOR_GRUPO As Color = Color.FromArgb(95, 105, 120)

    ' --- COLORES SEMÁNTICOS DE BOTONES ---
    Private Shared ReadOnly BTN_AZUL_PRIMARIO As Color = Color.FromArgb(0, 120, 215)
    Private Shared ReadOnly BTN_ROJO_PELIGRO As Color = Color.FromArgb(209, 52, 56)
    Private Shared ReadOnly BTN_VERDE_AÑADIR As Color = Color.FromArgb(40, 140, 90)
    Private Shared ReadOnly BTN_GRIS_NEUTRO As Color = Color.FromArgb(85, 85, 85)

    Private Sub ConstruirInterfaz()
        Dim margenIzq As Integer = 30
        Dim anchoForm As Integer = Me.ClientSize.Width

        ' 1. BANDA SUPERIOR CON TÍTULO
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
            .Text = "HISTORIAL DE MOVIMIENTOS",
            .AutoSize = False,
            .BackColor = COLOR_BANDA,
            .ForeColor = Color.White,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .Bounds = New Rectangle(margenIzq, 0, 600, alturaBanda)
        }
        Me.Controls.Add(lblTitulo)
        lblTitulo.BringToFront()

        ' 2. ZONA DE FILTROS (debajo de la banda)
        Dim yFila1 As Integer = alturaBanda + 25

        ' --- Filtros ---
        CrearFiltro("Filtrar por Artículo", cboFiltroArticulo, margenIzq, yFila1, 350)
        CrearFiltro("Desde Fecha", dtpDesde, margenIzq + 380, yFila1, 130)
        CrearFiltro("Hasta Fecha", dtpHasta, margenIzq + 540, yFila1, 130)

        ' --- Botones de filtro alineados con los textboxes (a la altura del valor) ---
        EstilizarBoton(btnFiltrar, "🔍 Filtrar", margenIzq + 700, yFila1 + 23, BTN_AZUL_PRIMARIO, Color.White)
        EstilizarBoton(btnLimpiar, "Limpiar", margenIzq + 810, yFila1 + 23, BTN_GRIS_NEUTRO, Color.White)

        ' 3. LÍNEA DIVISORIA ENTRE FILTROS Y GRID
        Dim yLinea As Integer = yFila1 + 75
        Dim linea As New Label() With {
            .Name = "LineaDivisoria",
            .Bounds = New Rectangle(margenIzq, yLinea, anchoForm - (margenIzq * 2), 2),
            .BackColor = COLOR_LINEA_DIVISORIA,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }
        Me.Controls.Add(linea)

        ' 4. GRID DE MOVIMIENTOS (ocupa el resto)
        Dim yTabla As Integer = yLinea + 18
        dgvMovimientos.Bounds = New Rectangle(margenIzq, yTabla, anchoForm - (margenIzq * 2), Me.ClientSize.Height - yTabla - 40)
        dgvMovimientos.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        dgvMovimientos.BackgroundColor = Me.BackColor
        dgvMovimientos.BorderStyle = BorderStyle.None
        Me.Controls.Add(dgvMovimientos)
    End Sub

    ' Método para dibujar etiqueta + control de filtro con estilo moderno
    Private Sub CrearFiltro(textoLabel As String, ctrl As Control, x As Integer, y As Integer, w As Integer)
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
        If TypeOf ctrl Is ComboBox Then DirectCast(ctrl, ComboBox).DropDownStyle = ComboBoxStyle.DropDownList
        Me.Controls.Add(ctrl)
    End Sub

    ' Estilo moderno de botón unificado (idéntico al resto de módulos)
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


    ' 4. ESTILOS DEL GRID (Usando tu diseño oficial)
    Private Sub ConfigurarGrid()
        ' 1. Magia de diseño: Llamamos a tu función global de estilos
        Try
            FrmPresupuestos.EstilizarGrid(dgvMovimientos)
        Catch ex As Exception
            ' Por si acaso la función no es accesible desde aquí
        End Try

        ' 2. Ajustes básicos
        dgvMovimientos.AutoGenerateColumns = False
        dgvMovimientos.AllowUserToAddRows = False
        dgvMovimientos.ReadOnly = True ' Es un historial, no se edita a mano
        dgvMovimientos.SelectionMode = DataGridViewSelectionMode.FullRowSelect

        ' 3. Mapeo de columnas
        dgvMovimientos.Columns.Clear()

        dgvMovimientos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Fecha", .DataPropertyName = "Fecha", .HeaderText = "Fecha", .Width = 100})
        dgvMovimientos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "DocumentoReferencia", .DataPropertyName = "DocumentoReferencia", .HeaderText = "Documento", .Width = 140})
        dgvMovimientos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CodigoReferencia", .DataPropertyName = "CodigoReferencia", .HeaderText = "Ref. Artículo", .Width = 120})
        dgvMovimientos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción del Artículo", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        dgvMovimientos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "TipoMovimiento", .DataPropertyName = "TipoMovimiento", .HeaderText = "Tipo", .Width = 100, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
        dgvMovimientos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Cantidad", .DataPropertyName = "Cantidad", .HeaderText = "Cantidad", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)}})
        dgvMovimientos.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "StockResultante", .DataPropertyName = "StockResultante", .HeaderText = "Stock Final", .Width = 100, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight}})
    End Sub

    ' 5. MAGIA VISUAL: Colorear Entradas y Salidas
    Private Sub dgvMovimientos_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvMovimientos.CellFormatting
        If e.RowIndex >= 0 AndAlso (dgvMovimientos.Columns(e.ColumnIndex).Name = "TipoMovimiento" Or dgvMovimientos.Columns(e.ColumnIndex).Name = "Cantidad") Then

            Dim valCelda = dgvMovimientos.Rows(e.RowIndex).Cells("TipoMovimiento").Value
            If valCelda IsNot Nothing AndAlso Not IsDBNull(valCelda) Then
                Dim tipoStr As String = valCelda.ToString().Trim().ToUpper()

                If tipoStr = "ENTRADA" OrElse tipoStr = "COMPRA" Then
                    e.CellStyle.ForeColor = Color.FromArgb(20, 160, 40) ' Verde
                    e.CellStyle.SelectionForeColor = Color.FromArgb(20, 160, 40)
                    e.CellStyle.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
                ElseIf tipoStr = "SALIDA" OrElse tipoStr = "VENTA" Then
                    e.CellStyle.ForeColor = Color.Red ' Rojo intenso
                    e.CellStyle.SelectionForeColor = Color.Red
                    e.CellStyle.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
                End If
            End If

        End If
    End Sub
    Private Sub AjustarAltoTabla()
        ' 1. Calculamos lo que ocupan la cabecera y todas las filas
        Dim altoNecesario As Integer = dgvMovimientos.ColumnHeadersHeight
        For Each fila As DataGridViewRow In dgvMovimientos.Rows
            altoNecesario += fila.Height
        Next

        ' Le sumamos un pelín para el borde inferior
        altoNecesario += 3

        ' 2. Calculamos el tope máximo (para que no se salga de la pantalla por abajo)
        Dim altoMaximo As Integer = Me.ClientSize.Height - dgvMovimientos.Top - 40

        ' 3. Ajustamos el alto real del control
        If altoNecesario > altoMaximo Then
            dgvMovimientos.Height = altoMaximo ' Si hay muchos datos, llega hasta abajo y pone scroll
        Else
            dgvMovimientos.Height = altoNecesario ' Si hay pocos, se encoge mágicamente
        End If

        ' Truco extra: Pintar el fondo que sobra del mismo color oscuro que el programa
        dgvMovimientos.BackgroundColor = Me.BackColor
    End Sub
    ' 6. LÓGICA DE BASE DE DATOS
    Private Sub CargarComboArticulos()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Cargamos los artículos para poder filtrar (Mostramos Código + Descripción)
            Dim sql As String = "SELECT ID_Articulo, CodigoReferencia || ' - ' || Descripcion AS NombreCompleto FROM Articulos ORDER BY CodigoReferencia"
            Using da As New SQLiteDataAdapter(sql, c)
                Dim dt As New DataTable()
                da.Fill(dt)

                ' Añadimos una fila "Todos" al principio
                Dim dr As DataRow = dt.NewRow()
                dr("ID_Articulo") = 0
                dr("NombreCompleto") = "-- TODOS LOS ARTÍCULOS --"
                dt.Rows.InsertAt(dr, 0)

                cboFiltroArticulo.DisplayMember = "NombreCompleto"
                cboFiltroArticulo.ValueMember = "ID_Articulo"
                cboFiltroArticulo.DataSource = dt
                cboFiltroArticulo.SelectedIndex = 0
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Private Sub CargarMovimientos()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' CONSULTA MAESTRA: Unimos Movimientos con Artículos para ver el nombre real
            Dim sql As String = "SELECT m.Fecha, m.DocumentoReferencia, a.CodigoReferencia, a.Descripcion, m.TipoMovimiento, m.Cantidad, m.StockResultante " &
                                "FROM MovimientosAlmacen m " &
                                "LEFT JOIN Articulos a ON m.ID_Articulo = a.ID_Articulo " &
                                "WHERE date(m.Fecha) >= date(@desde) AND date(m.Fecha) <= date(@hasta) "

            ' Si el usuario seleccionó un artículo concreto, lo añadimos al filtro
            Dim idFiltro As Integer = 0
            If cboFiltroArticulo.SelectedValue IsNot Nothing Then
                Integer.TryParse(cboFiltroArticulo.SelectedValue.ToString(), idFiltro)
            End If
            If idFiltro > 0 Then
                sql &= "AND m.ID_Articulo = @idArticulo "
            End If

            sql &= "ORDER BY m.Fecha DESC, m.ID_Movimiento DESC"

            Using cmd As New SQLiteCommand(sql, c)
                ' Pasamos las fechas en formato ISO seguro para SQLite (YYYY-MM-DD)
                cmd.Parameters.AddWithValue("@desde", dtpDesde.Value.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@hasta", dtpHasta.Value.ToString("yyyy-MM-dd"))
                If idFiltro > 0 Then cmd.Parameters.AddWithValue("@idArticulo", idFiltro)

                Dim da As New SQLiteDataAdapter(cmd)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvMovimientos.DataSource = dt
                AjustarAltoTabla()
            End Using

        Catch ex As Exception
            MessageBox.Show("Error al cargar movimientos: " & ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnFiltrar_Click(sender As Object, e As EventArgs) Handles btnFiltrar.Click
        CargarMovimientos()
    End Sub

    Private Sub btnLimpiar_Click(sender As Object, e As EventArgs) Handles btnLimpiar.Click
        cboFiltroArticulo.SelectedIndex = 0
        dtpDesde.Value = DateTime.Today.AddDays(-30)
        dtpHasta.Value = DateTime.Today
        CargarMovimientos()
    End Sub

    ' Optimización Visual
    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or &H2000000
            Return cp
        End Get
    End Property
End Class
