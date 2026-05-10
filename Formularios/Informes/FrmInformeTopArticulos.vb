Imports System.Data.SQLite
Imports System.Drawing.Printing

Public Class FrmInformeTopArticulos

    ' Controles UI
    Private dtpInicio As New DateTimePicker()
    Private dtpFin As New DateTimePicker()
    Private WithEvents btnGenerar As New Button()
    Private WithEvents btnImprimir As New Button()
    Private dgvTop As New DataGridView()

    ' Motor PDF
    Private WithEvents docInforme As New PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private _dtTop As New DataTable()

    Private Sub FrmInformeTopArticulos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Top Artículos Más Vendidos"
        Me.BackColor = Color.FromArgb(70, 75, 80)

        ConstruirInterfaz()

        ' Por defecto, mostramos lo más vendido del año actual
        dtpInicio.Value = New DateTime(DateTime.Now.Year, 1, 1)
        dtpFin.Value = DateTime.Now

        CargarTopArticulos()
    End Sub

    Private Sub ConstruirInterfaz()
        Me.Controls.Clear()

        ' 1. PANEL SUPERIOR
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 110,
            .BackColor = Color.FromArgb(70, 75, 80)
        }
        Me.Controls.Add(pnlHeader)

        Dim lblTitulo As New Label With {.Text = "RANKING DE ARTÍCULOS MÁS VENDIDOS", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .ForeColor = Color.WhiteSmoke, .AutoSize = True, .Location = New Point(30, 20)}
        pnlHeader.Controls.Add(lblTitulo)

        ' Selectores de fecha
        Dim lblDesde As New Label With {.Text = "Desde:", .ForeColor = Color.WhiteSmoke, .Location = New Point(30, 75), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpInicio.Format = DateTimePickerFormat.Short : dtpInicio.Location = New Point(90, 73) : dtpInicio.Width = 110
        pnlHeader.Controls.Add(lblDesde) : pnlHeader.Controls.Add(dtpInicio)

        Dim lblHasta As New Label With {.Text = "Hasta:", .ForeColor = Color.WhiteSmoke, .Location = New Point(220, 75), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpFin.Format = DateTimePickerFormat.Short : dtpFin.Location = New Point(270, 73) : dtpFin.Width = 110
        pnlHeader.Controls.Add(lblHasta) : pnlHeader.Controls.Add(dtpFin)

        ' Botones
        btnGenerar.Text = "Generar Ranking" : btnGenerar.Bounds = New Rectangle(410, 70, 150, 30)
        btnGenerar.BackColor = Color.FromArgb(0, 120, 215) : btnGenerar.ForeColor = Color.White : btnGenerar.FlatStyle = FlatStyle.Flat : btnGenerar.FlatAppearance.BorderSize = 0
        pnlHeader.Controls.Add(btnGenerar)

        btnImprimir.Text = "Exportar PDF" : btnImprimir.Bounds = New Rectangle(580, 70, 120, 30)
        btnImprimir.BackColor = Color.FromArgb(255, 140, 0) ' Naranja/Dorado para el "Top"
        btnImprimir.ForeColor = Color.White : btnImprimir.FlatStyle = FlatStyle.Flat : btnImprimir.FlatAppearance.BorderSize = 0
        btnImprimir.Visible = False
        pnlHeader.Controls.Add(btnImprimir)

        ' 2. CONTENEDOR DE LA TABLA
        Dim pnlGridContainer As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(30, 20, 30, 20)
        }
        Me.Controls.Add(pnlGridContainer)
        pnlGridContainer.BringToFront()

        ' 3. TABLA DE RESULTADOS
        dgvTop.Dock = DockStyle.Fill
        dgvTop.AllowUserToAddRows = False
        dgvTop.ReadOnly = True
        dgvTop.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvTop.BorderStyle = BorderStyle.None
        dgvTop.RowHeadersVisible = False

        Try : FrmPresupuestos.EstilizarGrid(dgvTop) : Catch : End Try

        dgvTop.BackgroundColor = Me.BackColor
        dgvTop.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        pnlGridContainer.Controls.Add(dgvTop)
    End Sub

    ' LÓGICA DE BASE DE DATOS (Adaptada a tu estructura real)
    Private Sub btnGenerar_Click(sender As Object, e As EventArgs) Handles btnGenerar.Click
        CargarTopArticulos()
    End Sub

    Private Sub CargarTopArticulos()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Hacemos JOIN con LineasFacturaVenta, FacturasVenta y Articulos
            Dim sql As String = "SELECT IFNULL(A.CodigoReferencia, L.ID_Articulo) AS Codigo, L.Descripcion, SUM(L.Cantidad) AS UnidadesVendidas, SUM(L.Total) AS Ingresos " &
                                "FROM LineasFacturaVenta L " &
                                "INNER JOIN FacturasVenta F ON L.NumeroFactura = F.NumeroFactura " &
                                "LEFT JOIN Articulos A ON L.ID_Articulo = A.ID_Articulo " &
                                "WHERE F.Fecha >= @inicio AND F.Fecha <= @fin " &
                                "GROUP BY Codigo, L.Descripcion " &
                                "ORDER BY UnidadesVendidas DESC " &
                                "LIMIT 100"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@inicio", dtpInicio.Value.ToString("yyyy-MM-dd 00:00:00"))
                cmd.Parameters.AddWithValue("@fin", dtpFin.Value.ToString("yyyy-MM-dd 23:59:59"))

                Dim da As New SQLiteDataAdapter(cmd)
                _dtTop.Clear()
                da.Fill(_dtTop)
            End Using

            ' Añadimos una columna virtual para la "Posición" (1, 2, 3...)
            If Not _dtTop.Columns.Contains("Posicion") Then
                _dtTop.Columns.Add("Posicion", GetType(Integer))
            End If
            Dim pos As Integer = 1
            For Each row As DataRow In _dtTop.Rows
                row("Posicion") = pos
                pos += 1
            Next

            ConfigurarGridTop()
            btnImprimir.Visible = _dtTop.Rows.Count > 0

        Catch ex As Exception
            MessageBox.Show("Error al generar el ranking: " & ex.Message, "Aviso SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub ConfigurarGridTop()
        dgvTop.DataSource = _dtTop
        If dgvTop.Columns.Count = 0 Then Return

        ' Ocultamos columnas innecesarias y ordenamos
        dgvTop.Columns("Posicion").DisplayIndex = 0
        dgvTop.Columns("Posicion").HeaderText = "#"
        dgvTop.Columns("Posicion").Width = 50
        dgvTop.Columns("Posicion").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        dgvTop.Columns("Posicion").DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)

        If dgvTop.Columns.Contains("Codigo") Then
            dgvTop.Columns("Codigo").HeaderText = "Cód."
            dgvTop.Columns("Codigo").Width = 100
            dgvTop.Columns("Codigo").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If

        If dgvTop.Columns.Contains("Descripcion") Then
            dgvTop.Columns("Descripcion").HeaderText = "Descripción del Artículo"
            dgvTop.Columns("Descripcion").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        End If

        If dgvTop.Columns.Contains("UnidadesVendidas") Then
            dgvTop.Columns("UnidadesVendidas").HeaderText = "Uds. Vendidas"
            dgvTop.Columns("UnidadesVendidas").Width = 120
            dgvTop.Columns("UnidadesVendidas").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            dgvTop.Columns("UnidadesVendidas").DefaultCellStyle.Format = "N2"
            dgvTop.Columns("UnidadesVendidas").DefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        End If

        If dgvTop.Columns.Contains("Ingresos") Then
            dgvTop.Columns("Ingresos").HeaderText = "Ingresos Generados"
            dgvTop.Columns("Ingresos").Width = 150
            dgvTop.Columns("Ingresos").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            dgvTop.Columns("Ingresos").DefaultCellStyle.Format = "C2"
            dgvTop.Columns("Ingresos").DefaultCellStyle.ForeColor = Color.DarkGreen
        End If
    End Sub

    ' MOTOR DE IMPRESIÓN PDF
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        docInforme.DefaultPageSettings.Landscape = False
        docInforme.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docInforme.DocumentName = "TopArticulos_" & DateTime.Now.ToString("ddMMyy")

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
        Dim bNaranja As New SolidBrush(Color.FromArgb(255, 140, 0))
        Dim bGris As New SolidBrush(Color.FromArgb(220, 220, 220))
        Dim lapizFino As New Pen(Color.FromArgb(220, 220, 220), 1)

        Dim yPos As Integer = 50
        Dim margenIzq As Integer = 50

        If _filaActualImpresion = 0 Then
            g.DrawString("OPTIMA SOFTWARE - ANÁLISIS DE VENTAS", fSub, bNegro, margenIzq, yPos)
            yPos += 25
            g.DrawString("TOP ARTÍCULOS MÁS VENDIDOS", fTitulo, bNaranja, margenIzq, yPos)
            yPos += 30
            g.DrawString($"Periodo: {dtpInicio.Value.ToShortDateString()} al {dtpFin.Value.ToShortDateString()}", fSub, bNegro, margenIzq, yPos)
            yPos += 40
        End If

        ' Cabecera de tabla gris
        g.FillRectangle(bGris, margenIzq, yPos, 727, 30)
        g.DrawString("#", fCabecera, bNegro, margenIzq + 10, yPos + 5)
        g.DrawString("Cód", fCabecera, bNegro, margenIzq + 50, yPos + 5)
        g.DrawString("Descripción", fCabecera, bNegro, margenIzq + 130, yPos + 5)
        g.DrawString("Uds. Vendidas", fCabecera, bNegro, margenIzq + 480, yPos + 5)
        g.DrawString("Ingresos", fCabecera, bNegro, margenIzq + 650, yPos + 5)
        yPos += 35

        Dim formatoCentro As New StringFormat() With {.Alignment = StringAlignment.Center}
        Dim formatoDer As New StringFormat() With {.Alignment = StringAlignment.Far}

        Dim totalUnidades As Decimal = 0
        Dim totalIngresos As Decimal = 0

        While _filaActualImpresion < _dtTop.Rows.Count
            Dim row As DataRow = _dtTop.Rows(_filaActualImpresion)

            ' Posición (Top 1, 2, 3...)
            g.DrawString(row("Posicion").ToString(), fFilaBold, bNaranja, margenIzq + 15, yPos)

            ' Código (Ahora trae el ART-XXX si existe, si no el ID)
            g.DrawString(row("Codigo").ToString(), fFila, bNegro, margenIzq + 50, yPos)

            ' Descripción
            Dim desc As String = row("Descripcion").ToString()
            If desc.Length > 45 Then desc = desc.Substring(0, 42) & "..."
            g.DrawString(desc, fFila, bNegro, margenIzq + 130, yPos)

            ' Valores
            Dim uds As Decimal = Convert.ToDecimal(row("UnidadesVendidas"))
            Dim ingresos As Decimal = Convert.ToDecimal(row("Ingresos"))

            g.DrawString(uds.ToString("N2"), fFilaBold, bNegro, margenIzq + 530, yPos, formatoCentro)
            g.DrawString(ingresos.ToString("C2"), fFila, bNegro, margenIzq + 710, yPos, formatoDer)

            yPos += 25
            g.DrawLine(lapizFino, margenIzq, yPos, margenIzq + 727, yPos)
            yPos += 5

            _filaActualImpresion += 1

            If yPos > 1050 AndAlso _filaActualImpresion < _dtTop.Rows.Count Then
                e.HasMorePages = True
                Return
            End If
        End While

        ' Totales
        If _filaActualImpresion >= _dtTop.Rows.Count Then
            yPos += 15

            For Each r As DataRow In _dtTop.Rows
                totalUnidades += Convert.ToDecimal(r("UnidadesVendidas"))
                totalIngresos += Convert.ToDecimal(r("Ingresos"))
            Next

            g.DrawString("TOTALES DEL RANKING:", fCabecera, bNaranja, margenIzq + 280, yPos)
            g.DrawString(totalUnidades.ToString("N2") & " Uds.", fCabecera, bNegro, margenIzq + 530, yPos, formatoCentro)
            g.DrawString(totalIngresos.ToString("C2"), fTitulo, bNegro, margenIzq + 710, yPos - 5, formatoDer)
            e.HasMorePages = False
        End If
    End Sub

End Class
