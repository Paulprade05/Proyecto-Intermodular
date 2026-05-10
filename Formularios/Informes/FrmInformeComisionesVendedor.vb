Imports System.Data.SQLite
Imports System.Drawing.Printing

Public Class FrmInformeComisionesVendedor

    ' Controles UI
    Private dtpInicio As New DateTimePicker()
    Private dtpFin As New DateTimePicker()
    Private WithEvents btnGenerar As New Button()
    Private WithEvents btnImprimir As New Button()
    Private dgvComisiones As New DataGridView()

    ' Motor PDF
    Private WithEvents docInforme As New PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private _dtComisiones As New DataTable()

    ' Porcentaje de comisión por defecto (Ej: 5%)

    Private Sub FrmInformeComisionesVendedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Comisiones por Vendedor"
        Me.BackColor = Color.FromArgb(70, 75, 80)

        ConstruirInterfaz()

        ' Por defecto, mostramos el mes actual (las comisiones suelen ser mensuales)
        dtpInicio.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
        dtpFin.Value = DateTime.Now
    End Sub

    Private Sub ConstruirInterfaz()
        ' 1. Limpieza
        Me.Controls.Clear()

        ' 2. PANEL SUPERIOR (Filtros y Botones)
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 110,
            .BackColor = Color.FromArgb(70, 75, 80) ' Fondo premium
        }
        Me.Controls.Add(pnlHeader)

        Dim lblTitulo As New Label With {.Text = "COMISIONES POR VENDEDOR", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .ForeColor = Color.WhiteSmoke, .AutoSize = True, .Location = New Point(30, 20)}
        pnlHeader.Controls.Add(lblTitulo)

        Dim lblDesde As New Label With {.Text = "Desde:", .ForeColor = Color.WhiteSmoke, .Location = New Point(30, 75), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpInicio.Format = DateTimePickerFormat.Short : dtpInicio.Location = New Point(90, 73) : dtpInicio.Width = 110
        pnlHeader.Controls.Add(lblDesde) : pnlHeader.Controls.Add(dtpInicio)

        Dim lblHasta As New Label With {.Text = "Hasta:", .ForeColor = Color.WhiteSmoke, .Location = New Point(220, 75), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpFin.Format = DateTimePickerFormat.Short : dtpFin.Location = New Point(270, 73) : dtpFin.Width = 110
        pnlHeader.Controls.Add(lblHasta) : pnlHeader.Controls.Add(dtpFin)

        btnGenerar.Text = "Calcular Comisiones" : btnGenerar.Bounds = New Rectangle(410, 70, 150, 30)
        btnGenerar.BackColor = Color.FromArgb(0, 120, 215) : btnGenerar.ForeColor = Color.White : btnGenerar.FlatStyle = FlatStyle.Flat : btnGenerar.FlatAppearance.BorderSize = 0
        pnlHeader.Controls.Add(btnGenerar)

        btnImprimir.Text = "Exportar PDF" : btnImprimir.Bounds = New Rectangle(580, 70, 120, 30)
        btnImprimir.BackColor = Color.FromArgb(40, 140, 90) : btnImprimir.ForeColor = Color.White : btnImprimir.FlatStyle = FlatStyle.Flat : btnImprimir.FlatAppearance.BorderSize = 0
        btnImprimir.Visible = False
        pnlHeader.Controls.Add(btnImprimir)

        ' 3. TABLA DE RESULTADOS (Panel contenedor)
        Dim pnlGridContainer As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(30, 20, 30, 20) ' Mismo padding que en tu informe de facturas
        }
        Me.Controls.Add(pnlGridContainer)
        pnlGridContainer.BringToFront()

        ' 4. LA MAGIA DE LA TABLA (Misma lógica exacta que InformeFacturas)
        dgvComisiones.Dock = DockStyle.Fill
        dgvComisiones.AllowUserToAddRows = False
        dgvComisiones.ReadOnly = True
        dgvComisiones.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvComisiones.BorderStyle = BorderStyle.None

        ' Quitamos la columna fea de la izquierda
        dgvComisiones.RowHeadersVisible = False

        ' Aplicamos tu estilo personalizado
        Try : FrmPresupuestos.EstilizarGrid(dgvComisiones) : Catch : End Try

        ' Forzamos el fondo oscuro para ocultar el hueco gigante en blanco
        dgvComisiones.BackgroundColor = Me.BackColor

        ' Quitamos el Fill automático para luego ajustar las columnas manualmente
        dgvComisiones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        pnlGridContainer.Controls.Add(dgvComisiones)
    End Sub

    ' LÓGICA DE BASE DE DATOS
    Private Sub btnGenerar_Click(sender As Object, e As EventArgs) Handles btnGenerar.Click
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' 1. Añadimos V.Comision (o el nombre de tu columna) a la consulta
            Dim sql As String = "SELECT V.ID_Vendedor, V.Nombre AS NombreVendedor, V.Comision AS Porcentaje, SUM(F.BaseImponible) AS TotalVentas " &
                                "FROM FacturasVenta F " &
                                "INNER JOIN Vendedores V ON F.ID_Vendedor = V.ID_Vendedor " &
                                "WHERE F.Fecha >= @inicio AND F.Fecha <= @fin AND F.Estado <> 'Cancelada' " &
                                "GROUP BY V.ID_Vendedor, V.Nombre, V.Comision " &
                                "ORDER BY TotalVentas DESC"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@inicio", dtpInicio.Value.ToString("yyyy-MM-dd 00:00:00"))
                cmd.Parameters.AddWithValue("@fin", dtpFin.Value.ToString("yyyy-MM-dd 23:59:59"))

                Dim da As New SQLiteDataAdapter(cmd)
                _dtComisiones.Clear()
                da.Fill(_dtComisiones)
            End Using

            ' Añadimos la columna donde guardaremos el resultado en Euros
            If Not _dtComisiones.Columns.Contains("ComisionCalculada") Then
                _dtComisiones.Columns.Add("ComisionCalculada", GetType(Decimal))
            End If

            ' 2. El nuevo bucle multiplicando por el porcentaje individual
            For Each row As DataRow In _dtComisiones.Rows
                Dim ventas As Decimal = Convert.ToDecimal(row("TotalVentas"))

                ' Leemos el porcentaje del comercial (por si algún vendedor lo tiene vacío, le ponemos 0)
                Dim pct As Decimal = 0
                If Not IsDBNull(row("Porcentaje")) Then
                    Decimal.TryParse(row("Porcentaje").ToString(), pct)
                End If

                ' Si en la BD guardas un 5, dividimos por 100 para hacer el 0.05
                row("ComisionCalculada") = ventas * (pct / 100D)
            Next

            ConfigurarGridComisiones()
            btnImprimir.Visible = _dtComisiones.Rows.Count > 0

        Catch ex As Exception
            MessageBox.Show("Error al calcular las comisiones dinámicas: " & ex.Message)
        End Try
    End Sub

    Private Sub ConfigurarGridComisiones()
        dgvComisiones.DataSource = _dtComisiones
        If dgvComisiones.Columns.Count = 0 Then Return

        ' Ajustes de columnas manuales
        If dgvComisiones.Columns.Contains("ID_Vendedor") Then
            dgvComisiones.Columns("ID_Vendedor").HeaderText = "Cód."
            dgvComisiones.Columns("ID_Vendedor").Width = 80
            dgvComisiones.Columns("ID_Vendedor").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If

        ' El vendedor ocupa el centro
        If dgvComisiones.Columns.Contains("NombreVendedor") Then
            dgvComisiones.Columns("NombreVendedor").HeaderText = "Vendedor"
            dgvComisiones.Columns("NombreVendedor").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        End If

        If dgvComisiones.Columns.Contains("Porcentaje") Then
            dgvComisiones.Columns("Porcentaje").HeaderText = "% Com."
            dgvComisiones.Columns("Porcentaje").Width = 100
            dgvComisiones.Columns("Porcentaje").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            dgvComisiones.Columns("Porcentaje").DefaultCellStyle.Format = "0.## \%"
        End If

        ' Helper para alinear los dineros a la derecha y darles formato
        Dim AlinearMoneda = Sub(nombre As String, ancho As Integer)
                                If dgvComisiones.Columns.Contains(nombre) Then
                                    dgvComisiones.Columns(nombre).Width = ancho
                                    dgvComisiones.Columns(nombre).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                                    dgvComisiones.Columns(nombre).DefaultCellStyle.Format = "C2"
                                End If
                            End Sub

        AlinearMoneda("TotalVentas", 160)
        dgvComisiones.Columns("TotalVentas").HeaderText = "Base Imponible"

        AlinearMoneda("ComisionCalculada", 160)
        dgvComisiones.Columns("ComisionCalculada").HeaderText = "Total a Pagar"
        ' Ponemos en negrita el total a pagar
        dgvComisiones.Columns("ComisionCalculada").DefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
    End Sub

    ' MOTOR DE IMPRESIÓN PDF
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        docInforme.DefaultPageSettings.Landscape = False
        docInforme.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docInforme.DocumentName = "ComisionesVendedores_" & DateTime.Now.ToString("ddMMyy")

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
            g.DrawString("LIQUIDACIÓN DE COMISIONES POR VENDEDOR", fTitulo, bNegro, margenIzq, yPos)
            yPos += 30
            g.DrawString($"Periodo liquidación: {dtpInicio.Value.ToShortDateString()} al {dtpFin.Value.ToShortDateString()}", fSub, bNegro, margenIzq, yPos)
            g.DrawString($"Impreso el: {DateTime.Now.ToShortDateString()}", fSub, bNegro, 600, yPos)
            yPos += 40
        End If

        ' Cabecera de tabla
        g.FillRectangle(bGris, margenIzq, yPos, 727, 30)
        g.DrawString("Cód", fCabecera, bNegro, margenIzq + 10, yPos + 5)
        g.DrawString("Vendedor", fCabecera, bNegro, margenIzq + 60, yPos + 5)
        g.DrawString("Base Imponible", fCabecera, bNegro, margenIzq + 450, yPos + 5)
        g.DrawString("Comisión", fCabecera, bNegro, margenIzq + 630, yPos + 5)
        yPos += 35

        Dim formatoDer As New StringFormat() With {.Alignment = StringAlignment.Far}
        Dim totalVentasGlobal As Decimal = 0
        Dim totalComisionGlobal As Decimal = 0

        While _filaActualImpresion < _dtComisiones.Rows.Count
            Dim row As DataRow = _dtComisiones.Rows(_filaActualImpresion)

            g.DrawString(row("ID_Vendedor").ToString(), fFila, bNegro, margenIzq + 10, yPos)

            Dim nombre As String = row("NombreVendedor").ToString()
            If nombre.Length > 45 Then nombre = nombre.Substring(0, 45) & "..."
            g.DrawString(nombre, fFila, bNegro, margenIzq + 60, yPos)

            Dim ventas As Decimal = Convert.ToDecimal(row("TotalVentas"))
            Dim comision As Decimal = Convert.ToDecimal(row("ComisionCalculada"))

            g.DrawString(ventas.ToString("C2"), fFila, bNegro, margenIzq + 560, yPos, formatoDer)
            g.DrawString(comision.ToString("C2"), fFilaBold, bNegro, margenIzq + 710, yPos, formatoDer)

            yPos += 25
            _filaActualImpresion += 1

            If yPos > 1050 AndAlso _filaActualImpresion < _dtComisiones.Rows.Count Then
                e.HasMorePages = True
                Return
            End If
        End While

        If _filaActualImpresion >= _dtComisiones.Rows.Count Then
            yPos += 10
            g.DrawLine(Pens.Black, margenIzq, yPos, margenIzq + 727, yPos)
            yPos += 10

            For Each r As DataRow In _dtComisiones.Rows
                totalVentasGlobal += Convert.ToDecimal(r("TotalVentas"))
                totalComisionGlobal += Convert.ToDecimal(r("ComisionCalculada"))
            Next

            g.DrawString("TOTALES DEL PERIODO:", fCabecera, bNegro, margenIzq + 250, yPos)
            g.DrawString(totalVentasGlobal.ToString("C2"), fCabecera, bNegro, margenIzq + 560, yPos, formatoDer)
            g.DrawString(totalComisionGlobal.ToString("C2"), fTitulo, bNegro, margenIzq + 710, yPos - 5, formatoDer)
            e.HasMorePages = False
        End If
    End Sub
End Class
