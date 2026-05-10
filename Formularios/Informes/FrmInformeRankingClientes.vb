Imports System.Data.SQLite
Imports System.Drawing.Printing

Public Class FrmInformeRankingClientes

    ' Controles UI
    Private dtpInicio As New DateTimePicker()
    Private dtpFin As New DateTimePicker()
    Private WithEvents btnGenerar As New Button()
    Private WithEvents btnImprimir As New Button()
    Private dgvRanking As New DataGridView()

    ' Motor PDF
    Private WithEvents docInforme As New PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private _dtRanking As New DataTable()

    Private Sub FrmInformeRankingClientes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Ranking de Ventas por Cliente"
        Me.BackColor = Color.FromArgb(70, 75, 80)

        ConstruirInterfaz()

        ' Por defecto, mostramos las ventas de todo el año actual
        dtpInicio.Value = New DateTime(DateTime.Now.Year, 1, 1)
        dtpFin.Value = DateTime.Now
    End Sub

    Private Sub ConstruirInterfaz()
        ' 1. Limpieza
        Me.Controls.Clear()

        ' 2. PANEL SUPERIOR (Filtros y Botones)
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 110,
            .BackColor = Color.FromArgb(70, 75, 80)
        }
        Me.Controls.Add(pnlHeader)

        Dim lblTitulo As New Label With {.Text = "RANKING DE VENTAS", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .ForeColor = Color.WhiteSmoke, .AutoSize = True, .Location = New Point(30, 20)}
        pnlHeader.Controls.Add(lblTitulo)

        Dim lblDesde As New Label With {.Text = "Desde fecha:", .ForeColor = Color.WhiteSmoke, .Location = New Point(30, 75), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpInicio.Format = DateTimePickerFormat.Short : dtpInicio.Location = New Point(120, 73) : dtpInicio.Width = 120
        pnlHeader.Controls.Add(lblDesde) : pnlHeader.Controls.Add(dtpInicio)

        Dim lblHasta As New Label With {.Text = "Hasta fecha:", .ForeColor = Color.WhiteSmoke, .Location = New Point(260, 75), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpFin.Format = DateTimePickerFormat.Short : dtpFin.Location = New Point(350, 73) : dtpFin.Width = 120
        pnlHeader.Controls.Add(lblHasta) : pnlHeader.Controls.Add(dtpFin)

        btnGenerar.Text = "Generar Ranking" : btnGenerar.Bounds = New Rectangle(500, 70, 140, 30)
        btnGenerar.BackColor = Color.FromArgb(0, 120, 215) : btnGenerar.ForeColor = Color.White : btnGenerar.FlatStyle = FlatStyle.Flat : btnGenerar.FlatAppearance.BorderSize = 0
        pnlHeader.Controls.Add(btnGenerar)

        btnImprimir.Text = "Exportar PDF" : btnImprimir.Bounds = New Rectangle(650, 70, 120, 30)
        btnImprimir.BackColor = Color.FromArgb(40, 140, 90) : btnImprimir.ForeColor = Color.White : btnImprimir.FlatStyle = FlatStyle.Flat : btnImprimir.FlatAppearance.BorderSize = 0
        btnImprimir.Visible = False
        pnlHeader.Controls.Add(btnImprimir)

        ' 3. TABLA DE RESULTADOS (Panel contenedor)
        Dim pnlGridContainer As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(30, 20, 30, 20) ' Mismo padding que en InformeFacturas
        }
        Me.Controls.Add(pnlGridContainer)
        pnlGridContainer.BringToFront()

        ' 4. LA MAGIA DE LA TABLA (IDÉNTICO A FACTURAS)
        dgvRanking.Dock = DockStyle.Fill
        dgvRanking.AllowUserToAddRows = False
        dgvRanking.ReadOnly = True
        dgvRanking.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvRanking.BorderStyle = BorderStyle.None

        ' Quitamos la columna fea de la izquierda
        dgvRanking.RowHeadersVisible = False

        ' Aplicamos tu estilo personalizado
        Try : FrmPresupuestos.EstilizarGrid(dgvRanking) : Catch : End Try

        ' Forzamos el fondo oscuro para ocultar el hueco gigante en blanco
        dgvRanking.BackgroundColor = Me.BackColor

        ' Quitamos el Fill automático para luego ajustar las columnas manualmente
        dgvRanking.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        pnlGridContainer.Controls.Add(dgvRanking)
    End Sub

    ' LÓGICA DE BASE DE DATOS
    Private Sub btnGenerar_Click(sender As Object, e As EventArgs) Handles btnGenerar.Click
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' AGRUPAMOS POR CLIENTE Y SUMAMOS EL TOTAL FACTURADO
            Dim sql As String = "SELECT CodigoCliente, NombreFiscal, SUM(TotalFactura) AS TotalVendido " &
                                "FROM FacturasVenta " &
                                "WHERE Fecha >= @inicio AND Fecha <= @fin AND Estado <> 'Cancelada' " &
                                "GROUP BY CodigoCliente, NombreFiscal " &
                                "ORDER BY TotalVendido DESC"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@inicio", dtpInicio.Value.ToString("yyyy-MM-dd 00:00:00"))
                cmd.Parameters.AddWithValue("@fin", dtpFin.Value.ToString("yyyy-MM-dd 23:59:59"))

                Dim da As New SQLiteDataAdapter(cmd)
                _dtRanking.Clear()
                da.Fill(_dtRanking)
            End Using

            ' Añadimos una columna virtual para la "Posición" (1º, 2º, 3º...)
            If Not _dtRanking.Columns.Contains("Posicion") Then
                _dtRanking.Columns.Add("Posicion", GetType(Integer))
                _dtRanking.Columns("Posicion").SetOrdinal(0)
            End If

            Dim pos As Integer = 1
            For Each row As DataRow In _dtRanking.Rows
                row("Posicion") = pos
                pos += 1
            Next

            ConfigurarGridRanking()
            btnImprimir.Visible = _dtRanking.Rows.Count > 0

        Catch ex As Exception
            MessageBox.Show("Error al generar el ranking: " & ex.Message)
        End Try
    End Sub

    Private Sub ConfigurarGridRanking()
        dgvRanking.DataSource = _dtRanking
        If dgvRanking.Columns.Count = 0 Then Return

        If dgvRanking.Columns.Contains("Posicion") Then
            dgvRanking.Columns("Posicion").HeaderText = "#"
            dgvRanking.Columns("Posicion").Width = 60
            dgvRanking.Columns("Posicion").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            dgvRanking.Columns("Posicion").DefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        End If

        If dgvRanking.Columns.Contains("CodigoCliente") Then
            dgvRanking.Columns("CodigoCliente").HeaderText = "Cód. Cliente"
            dgvRanking.Columns("CodigoCliente").Width = 120
        End If

        ' El cliente ocupa el centro restante
        If dgvRanking.Columns.Contains("NombreFiscal") Then
            dgvRanking.Columns("NombreFiscal").HeaderText = "Nombre del Cliente"
            dgvRanking.Columns("NombreFiscal").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        End If

        If dgvRanking.Columns.Contains("TotalVendido") Then
            dgvRanking.Columns("TotalVendido").HeaderText = "Total Facturado"
            dgvRanking.Columns("TotalVendido").Width = 160
            dgvRanking.Columns("TotalVendido").DefaultCellStyle.Format = "C2"
            dgvRanking.Columns("TotalVendido").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            dgvRanking.Columns("TotalVendido").DefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        End If
    End Sub

    ' MOTOR DE IMPRESIÓN PDF (DISEÑO VERTICAL)
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        docInforme.DefaultPageSettings.Landscape = False ' FORMATO VERTICAL A4
        docInforme.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docInforme.DocumentName = "RankingClientes_" & DateTime.Now.ToString("ddMMyy")

        Dim ppd As New PrintPreviewDialog()
        ppd.Document = docInforme
        ppd.Width = 900 : ppd.Height = 700
        CType(ppd, Form).WindowState = FormWindowState.Maximized
        ppd.ShowDialog()
    End Sub

    Private Sub docInforme_BeginPrint(sender As Object, e As PrintEventArgs) Handles docInforme.BeginPrint
        _filaActualImpresion = 0 ' Reseteo obligatorio
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

        ' --- CABECERA DEL DOCUMENTO (Solo en la primera página) ---
        If _filaActualImpresion = 0 Then
            g.DrawString("OPTIMA SOFTWARE - INFORME CORPORATIVO", fSub, bNegro, margenIzq, yPos)
            yPos += 25
            g.DrawString("RANKING DE VENTAS POR CLIENTE", fTitulo, bNegro, margenIzq, yPos)
            yPos += 30
            g.DrawString($"Periodo analizado: {dtpInicio.Value.ToShortDateString()} al {dtpFin.Value.ToShortDateString()}", fSub, bNegro, margenIzq, yPos)
            g.DrawString($"Impreso el: {DateTime.Now.ToShortDateString()}", fSub, bNegro, 600, yPos)
            yPos += 40
        End If

        ' --- CABECERA DE LA TABLA ---
        g.FillRectangle(bGris, margenIzq, yPos, 727, 30) ' Fondo gris
        g.DrawString("#", fCabecera, bNegro, margenIzq + 10, yPos + 5)
        g.DrawString("Cód. Cliente", fCabecera, bNegro, margenIzq + 60, yPos + 5)
        g.DrawString("Nombre del Cliente", fCabecera, bNegro, margenIzq + 180, yPos + 5)
        g.DrawString("Total Facturado", fCabecera, bNegro, margenIzq + 580, yPos + 5)
        yPos += 35

        ' --- DIBUJAR FILAS ---
        Dim formatoDer As New StringFormat() With {.Alignment = StringAlignment.Far}
        Dim granTotal As Decimal = 0

        While _filaActualImpresion < _dtRanking.Rows.Count
            Dim row As DataRow = _dtRanking.Rows(_filaActualImpresion)

            g.DrawString(row("Posicion").ToString(), fFila, bNegro, margenIzq + 10, yPos)
            g.DrawString(row("CodigoCliente").ToString(), fFila, bNegro, margenIzq + 60, yPos)

            ' Cortamos el nombre del cliente si es muy largo
            Dim nombre As String = row("NombreFiscal").ToString()
            If nombre.Length > 45 Then nombre = nombre.Substring(0, 45) & "..."
            g.DrawString(nombre, fFila, bNegro, margenIzq + 180, yPos)

            Dim total As Decimal = Convert.ToDecimal(row("TotalVendido"))
            g.DrawString(total.ToString("C2"), fFilaBold, bNegro, margenIzq + 710, yPos, formatoDer)

            yPos += 25
            _filaActualImpresion += 1

            ' Control de Salto de Página
            If yPos > 1050 AndAlso _filaActualImpresion < _dtRanking.Rows.Count Then
                e.HasMorePages = True
                Return
            End If
        End While

        ' --- TOTAL GLOBAL AL FINAL ---
        If _filaActualImpresion >= _dtRanking.Rows.Count Then
            yPos += 10
            g.DrawLine(Pens.Black, margenIzq, yPos, margenIzq + 727, yPos)
            yPos += 10

            ' Calculamos el total de todo el grid para ponerlo abajo
            For Each r As DataRow In _dtRanking.Rows
                granTotal += Convert.ToDecimal(r("TotalVendido"))
            Next

            g.DrawString("TOTAL ACUMULADO DEL RANKING:", fCabecera, bNegro, margenIzq + 350, yPos)
            g.DrawString(granTotal.ToString("C2"), fTitulo, bNegro, margenIzq + 710, yPos - 5, formatoDer)
            e.HasMorePages = False
        End If
    End Sub
End Class
