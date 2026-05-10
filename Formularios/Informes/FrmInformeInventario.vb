Imports System.Data.SQLite
Imports System.Drawing.Printing

Public Class FrmInformeInventario

    ' Controles UI
    Private WithEvents btnGenerar As New Button()
    Private WithEvents btnImprimir As New Button()
    Private dgvInventario As New DataGridView()

    ' Motor PDF
    Private WithEvents docInforme As New PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private _dtInventario As New DataTable()
    Dim lapizFino As New Pen(Color.FromArgb(220, 220, 220), 1)
    Private Sub FrmInformeInventario_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Inventario Valorado"
        Me.BackColor = Color.FromArgb(70, 75, 80)

        ConstruirInterfaz()

        ' Autocargamos el inventario al abrir
        CargarInventario()
    End Sub

    Private Sub ConstruirInterfaz()
        ' 1. Limpieza
        Me.Controls.Clear()

        ' 2. PANEL SUPERIOR (Botones y Títulos)
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 110,
            .BackColor = Color.FromArgb(70, 75, 80)
        }
        Me.Controls.Add(pnlHeader)

        Dim lblTitulo As New Label With {.Text = "INVENTARIO VALORADO ACTUAL", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .ForeColor = Color.WhiteSmoke, .AutoSize = True, .Location = New Point(30, 20)}
        pnlHeader.Controls.Add(lblTitulo)

        btnGenerar.Text = "Actualizar Datos" : btnGenerar.Bounds = New Rectangle(30, 70, 150, 30)
        btnGenerar.BackColor = Color.FromArgb(0, 120, 215) : btnGenerar.ForeColor = Color.White : btnGenerar.FlatStyle = FlatStyle.Flat : btnGenerar.FlatAppearance.BorderSize = 0
        pnlHeader.Controls.Add(btnGenerar)

        btnImprimir.Text = "Exportar PDF" : btnImprimir.Bounds = New Rectangle(200, 70, 120, 30)
        btnImprimir.BackColor = Color.FromArgb(40, 140, 90) : btnImprimir.ForeColor = Color.White : btnImprimir.FlatStyle = FlatStyle.Flat : btnImprimir.FlatAppearance.BorderSize = 0
        btnImprimir.Visible = False
        pnlHeader.Controls.Add(btnImprimir)

        ' 3. TABLA DE RESULTADOS
        Dim pnlGridContainer As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(30, 20, 30, 20)
        }
        Me.Controls.Add(pnlGridContainer)
        pnlGridContainer.BringToFront()

        ' 4. LA MAGIA DE LA TABLA
        dgvInventario.Dock = DockStyle.Fill
        dgvInventario.AllowUserToAddRows = False
        dgvInventario.ReadOnly = True
        dgvInventario.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvInventario.BorderStyle = BorderStyle.None
        dgvInventario.RowHeadersVisible = False

        ' Intenta aplicar tu estilo personalizado si existe
        Try : FrmPresupuestos.EstilizarGrid(dgvInventario) : Catch : End Try

        dgvInventario.BackgroundColor = Me.BackColor
        dgvInventario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        pnlGridContainer.Controls.Add(dgvInventario)
    End Sub

    ' LÓGICA DE BASE DE DATOS (NOMBRES CORREGIDOS)
    Private Sub btnGenerar_Click(sender As Object, e As EventArgs) Handles btnGenerar.Click
        CargarInventario()
    End Sub

    Private Sub CargarInventario()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Consulta SQL adaptada EXACTAMENTE a las columnas de tu imagen
            Dim sql As String = "SELECT CodigoReferencia, Descripcion, StockActual, PrecioCoste, (StockActual * PrecioCoste) AS ValorTotal " &
                                "FROM Articulos " &
                                "WHERE StockActual > 0 AND Activo = 1 " &
                                "ORDER BY Descripcion ASC"

            Using cmd As New SQLiteCommand(sql, c)
                Dim da As New SQLiteDataAdapter(cmd)
                _dtInventario.Clear()
                da.Fill(_dtInventario)
            End Using

            ConfigurarGridInventario()
            btnImprimir.Visible = _dtInventario.Rows.Count > 0

        Catch ex As Exception
            MessageBox.Show("Error al cargar el inventario: " & ex.Message)
        End Try
    End Sub

    Private Sub ConfigurarGridInventario()
        dgvInventario.DataSource = _dtInventario
        If dgvInventario.Columns.Count = 0 Then Return

        ' Ajustes de columnas manuales (NOMBRES CORREGIDOS)
        If dgvInventario.Columns.Contains("CodigoReferencia") Then
            dgvInventario.Columns("CodigoReferencia").HeaderText = "Cód."
            dgvInventario.Columns("CodigoReferencia").Width = 100
            dgvInventario.Columns("CodigoReferencia").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If

        If dgvInventario.Columns.Contains("Descripcion") Then
            dgvInventario.Columns("Descripcion").HeaderText = "Descripción del Artículo"
            dgvInventario.Columns("Descripcion").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        End If

        If dgvInventario.Columns.Contains("StockActual") Then
            dgvInventario.Columns("StockActual").HeaderText = "Stock"
            dgvInventario.Columns("StockActual").Width = 80
            dgvInventario.Columns("StockActual").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            dgvInventario.Columns("StockActual").DefaultCellStyle.Format = "N2"
        End If

        Dim AlinearMoneda = Sub(nombre As String, titulo As String, ancho As Integer)
                                If dgvInventario.Columns.Contains(nombre) Then
                                    dgvInventario.Columns(nombre).HeaderText = titulo
                                    dgvInventario.Columns(nombre).Width = ancho
                                    dgvInventario.Columns(nombre).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                                    dgvInventario.Columns(nombre).DefaultCellStyle.Format = "C2"
                                End If
                            End Sub

        AlinearMoneda("PrecioCoste", "Coste Unit.", 120)
        AlinearMoneda("ValorTotal", "Valor Total", 140)

        If dgvInventario.Columns.Contains("ValorTotal") Then
            dgvInventario.Columns("ValorTotal").DefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        End If
    End Sub

    ' MOTOR DE IMPRESIÓN PDF
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        docInforme.DefaultPageSettings.Landscape = False
        docInforme.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docInforme.DocumentName = "InventarioValorado_" & DateTime.Now.ToString("ddMMyy")

        Dim ppd As New PrintPreviewDialog()
        ppd.Document = docInforme
        ppd.Width = 900 : ppd.Height = 700
        CType(ppd, Form).WindowState = FormWindowState.Maximized
        ppd.ShowDialog()
    End Sub

    Private Sub docInforme_BeginPrint(sender As Object, e As PrintEventArgs) Handles docInforme.BeginPrint
        _filaActualImpresion = 0
    End Sub

    Private Sub docInforme_PrintPage(sender As Object, e As PrintPageEventArgs) Handles docInforme.PrintPage
        Dim g As Graphics = e.Graphics
        Dim fTitulo As New Font("Segoe UI", 16, FontStyle.Bold)
        Dim fSub As New Font("Segoe UI", 10, FontStyle.Regular)
        Dim fCabecera As New Font("Segoe UI", 10, FontStyle.Bold)
        Dim fFila As New Font("Segoe UI", 9, FontStyle.Regular)
        Dim fFilaBold As New Font("Segoe UI", 9, FontStyle.Bold)

        Dim bNegro As New SolidBrush(Color.Black)
        Dim bGris As New SolidBrush(Color.FromArgb(220, 220, 220))

        Dim yPos As Integer = 50
        Dim margenIzq As Integer = 50

        If _filaActualImpresion = 0 Then
            g.DrawString("OPTIMA SOFTWARE - INFORME CORPORATIVO", fSub, bNegro, margenIzq, yPos)
            yPos += 25
            g.DrawString("INVENTARIO VALORADO DE ALMACÉN", fTitulo, bNegro, margenIzq, yPos)
            yPos += 30
            g.DrawString($"Stock a fecha: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", fSub, bNegro, margenIzq, yPos)
            yPos += 40
        End If

        ' Cabecera de tabla gris
        g.FillRectangle(bGris, margenIzq, yPos, 727, 30)
        g.DrawString("Cód", fCabecera, bNegro, margenIzq + 10, yPos + 5)
        g.DrawString("Descripción", fCabecera, bNegro, margenIzq + 100, yPos + 5)
        g.DrawString("Stock", fCabecera, bNegro, margenIzq + 450, yPos + 5)
        g.DrawString("Coste Unit.", fCabecera, bNegro, margenIzq + 540, yPos + 5)
        g.DrawString("Valor Total", fCabecera, bNegro, margenIzq + 640, yPos + 5)
        yPos += 35

        Dim formatoDer As New StringFormat() With {.Alignment = StringAlignment.Far}
        Dim formatoCentro As New StringFormat() With {.Alignment = StringAlignment.Center}
        Dim valorTotalAlmacen As Decimal = 0

        While _filaActualImpresion < _dtInventario.Rows.Count
            Dim row As DataRow = _dtInventario.Rows(_filaActualImpresion)

            ' Imprimir código (CORREGIDO)
            g.DrawString(row("CodigoReferencia").ToString(), fFila, bNegro, margenIzq + 10, yPos)

            ' Imprimir descripción recortada
            Dim desc As String = row("Descripcion").ToString()
            If desc.Length > 50 Then desc = desc.Substring(0, 47) & "..."
            g.DrawString(desc, fFila, bNegro, margenIzq + 100, yPos)

            ' Imprimir valores numéricos (CORREGIDO)
            Dim stock As Decimal = Convert.ToDecimal(row("StockActual"))
            Dim precio As Decimal = Convert.ToDecimal(row("PrecioCoste"))
            Dim valorFila As Decimal = Convert.ToDecimal(row("ValorTotal"))

            g.DrawString(stock.ToString("N2"), fFila, bNegro, margenIzq + 470, yPos, formatoCentro)
            g.DrawString(precio.ToString("C2"), fFila, bNegro, margenIzq + 605, yPos, formatoDer)
            g.DrawString(valorFila.ToString("C2"), fFilaBold, bNegro, margenIzq + 710, yPos, formatoDer)

            yPos += 25
            g.DrawLine(lapizFino, margenIzq, yPos, margenIzq + 727, yPos)
            _filaActualImpresion += 1

            If yPos > 1050 AndAlso _filaActualImpresion < _dtInventario.Rows.Count Then
                e.HasMorePages = True
                Return
            End If
        End While

        ' Bloque de totales
        If _filaActualImpresion >= _dtInventario.Rows.Count Then
            yPos += 10
            g.DrawLine(Pens.Black, margenIzq, yPos, margenIzq + 727, yPos)
            yPos += 10

            For Each r As DataRow In _dtInventario.Rows
                valorTotalAlmacen += Convert.ToDecimal(r("ValorTotal"))
            Next

            g.DrawString("VALOR TOTAL INMOVILIZADO:", fCabecera, bNegro, margenIzq + 380, yPos)
            g.DrawString(valorTotalAlmacen.ToString("C2"), fTitulo, bNegro, margenIzq + 710, yPos - 5, formatoDer)
            e.HasMorePages = False
        End If
    End Sub

End Class
