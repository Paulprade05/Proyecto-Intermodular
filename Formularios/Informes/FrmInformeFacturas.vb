Imports System.Data.SQLite
Imports System.IO
Imports System.Drawing.Printing

Public Class FrmInformeFacturas
    ' Controles generados por código para no tener que arrastrarlos a mano
    Private dtpDesde As New DateTimePicker()
    Private dtpHasta As New DateTimePicker()
    Private btnGenerar As New Button()
    Private btnExportar As New Button()
    Private dgvResultados As New DataGridView()
    Private lblTotalBase As New Label()
    Private lblTotalIva As New Label()
    Private lblTotalFacturado As New Label()
    Private WithEvents docInforme As New PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private _paginaActual As Integer = 1
    Private Sub FrmInformeFacturas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 1. CONFIGURACIÓN DEL FONDO (Modo Oscuro)
        Me.BackColor = Color.FromArgb(70, 75, 80)
        Me.ForeColor = Color.WhiteSmoke
        Me.Text = "Informe de Facturas Emitidas"

        ' 2. PANEL SUPERIOR (Filtros y Botones)
        Dim panelFiltros As New Panel With {.Dock = DockStyle.Top, .Height = 100}
        Me.Controls.Add(panelFiltros)

        Dim lblTitulo As New Label With {.Text = "INFORME DE FACTURAS EMITIDAS", .Location = New Point(30, 15), .AutoSize = True, .Font = New Font("Segoe UI", 14, FontStyle.Bold), .ForeColor = Color.FromArgb(0, 150, 255)}

        Dim lblDesde As New Label With {.Text = "Desde:", .Location = New Point(30, 60), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpDesde.Bounds = New Rectangle(85, 57, 120, 27)
        dtpDesde.Format = DateTimePickerFormat.Short
        dtpDesde.Font = New Font("Segoe UI", 10)
        dtpDesde.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)

        Dim lblHasta As New Label With {.Text = "Hasta:", .Location = New Point(220, 60), .AutoSize = True, .Font = New Font("Segoe UI", 10)}
        dtpHasta.Bounds = New Rectangle(275, 57, 120, 27)
        dtpHasta.Format = DateTimePickerFormat.Short
        dtpHasta.Font = New Font("Segoe UI", 10)

        ' Botones
        btnGenerar.Text = "Buscar Facturas"
        btnGenerar.Bounds = New Rectangle(415, 55, 140, 32)
        EstilizarBoton(btnGenerar, Color.FromArgb(0, 120, 215))
        AddHandler btnGenerar.Click, AddressOf GenerarInforme

        btnExportar.Text = "Ver / Guardar PDF"
        btnExportar.Bounds = New Rectangle(565, 55, 160, 32)
        EstilizarBoton(btnExportar, Color.FromArgb(209, 52, 56))
        AddHandler btnExportar.Click, AddressOf PrevisualizarPDF

        Dim linea As New Label With {.BackColor = Color.FromArgb(100, 100, 100), .Height = 1, .Dock = DockStyle.Bottom}
        panelFiltros.Controls.AddRange({lblTitulo, lblDesde, dtpDesde, lblHasta, dtpHasta, btnGenerar, btnExportar, linea})

        ' 3. PANEL INFERIOR (Totales 100% blindados a la derecha)
        Dim panelTotales As New Panel With {.Dock = DockStyle.Bottom, .Height = 65, .BackColor = Color.FromArgb(25, 30, 40)}

        ' Usamos los anchos exactos del panel para colocarlos, así nunca se salen de la pantalla
        lblTotalFacturado.AutoSize = False
        lblTotalFacturado.Size = New Size(280, 35)
        lblTotalFacturado.Location = New Point(panelTotales.Width - 300, 15)
        lblTotalFacturado.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblTotalFacturado.TextAlign = ContentAlignment.MiddleRight
        lblTotalFacturado.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        lblTotalFacturado.ForeColor = Color.FromArgb(0, 150, 255)

        lblTotalIva.AutoSize = False
        lblTotalIva.Size = New Size(200, 30)
        lblTotalIva.Location = New Point(panelTotales.Width - 510, 18)
        lblTotalIva.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblTotalIva.TextAlign = ContentAlignment.MiddleRight
        lblTotalIva.Font = New Font("Segoe UI", 11, FontStyle.Bold)

        lblTotalBase.AutoSize = False
        lblTotalBase.Size = New Size(200, 30)
        lblTotalBase.Location = New Point(panelTotales.Width - 720, 18)
        lblTotalBase.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblTotalBase.TextAlign = ContentAlignment.MiddleRight
        lblTotalBase.Font = New Font("Segoe UI", 11, FontStyle.Bold)

        panelTotales.Controls.AddRange({lblTotalBase, lblTotalIva, lblTotalFacturado})
        Me.Controls.Add(panelTotales)

        ' 4. TABLA DE RESULTADOS
        Dim panelGrid As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(30, 20, 30, 20)}
        Me.Controls.Add(panelGrid)

        dgvResultados.Dock = DockStyle.Fill
        dgvResultados.AllowUserToAddRows = False
        dgvResultados.ReadOnly = True
        dgvResultados.BorderStyle = BorderStyle.None

        panelGrid.Controls.Add(dgvResultados)

        panelFiltros.BringToFront()
        panelTotales.BringToFront()
        panelGrid.BringToFront()

        GenerarInforme(Nothing, Nothing)
    End Sub

    Private Sub EstilizarBoton(btn As Button, colorFondo As Color)
        btn.BackColor = colorFondo
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        btn.Cursor = Cursors.Hand
    End Sub

    Private Sub GenerarInforme(sender As Object, e As EventArgs)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Hacemos un LEFT JOIN para traer el NombreFiscal del cliente y lo concatenamos con su código usando ||
            Dim sql As String = "SELECT F.NumeroFactura AS [Nº Factura], F.Fecha, " &
                                "(F.CodigoCliente || ' - ' || IFNULL(C.NombreFiscal, 'Desconocido')) AS [Cliente], " &
                                "F.BaseImponible AS [Base], (F.TotalFactura - F.BaseImponible) AS [Cuota IVA], F.TotalFactura AS [Total Factura], F.Estado " &
                                "FROM FacturasVenta F " &
                                "LEFT JOIN Clientes C ON F.CodigoCliente = C.CodigoCliente " &
                                "WHERE F.Fecha >= @desde AND F.Fecha <= @hasta ORDER BY F.Fecha ASC"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@desde", dtpDesde.Value.ToString("yyyy-MM-dd") & " 00:00:00")
                cmd.Parameters.AddWithValue("@hasta", dtpHasta.Value.ToString("yyyy-MM-dd") & " 23:59:59")

                Dim da As New SQLiteDataAdapter(cmd)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvResultados.DataSource = dt

                ' LA MAGIA DE LA TABLA (DISEÑO, ALINEACIONES Y TAMAÑOS)

                ' 1. Quitamos columnas feas
                dgvResultados.RowHeadersVisible = False

                ' 2. Aplicamos tu estilo personalizado, pero FORZAMOS el fondo oscuro para el hueco en blanco
                Try : FrmPresupuestos.EstilizarGrid(dgvResultados) : Catch : End Try
                dgvResultados.BackgroundColor = Me.BackColor ' Adiós hueco blanco gigante

                ' 3. Ajustamos columnas (Anchos manuales en vez de Fill a lo bruto)
                dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

                If dgvResultados.Columns.Contains("Nº Factura") Then dgvResultados.Columns("Nº Factura").Width = 100
                If dgvResultados.Columns.Contains("Fecha") Then dgvResultados.Columns("Fecha").Width = 100
                If dgvResultados.Columns.Contains("Estado") Then dgvResultados.Columns("Estado").Width = 120

                ' El cliente ocupa todo el espacio central sobrante
                If dgvResultados.Columns.Contains("Cliente") Then dgvResultados.Columns("Cliente").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

                ' Helper para alinear los dineros a la derecha y darles formato
                Dim AlinearMoneda = Sub(nombre As String)
                                        If dgvResultados.Columns.Contains(nombre) Then
                                            dgvResultados.Columns(nombre).Width = 120
                                            dgvResultados.Columns(nombre).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                                            dgvResultados.Columns(nombre).DefaultCellStyle.Format = "C2"
                                        End If
                                    End Sub

                AlinearMoneda("Base")
                AlinearMoneda("Cuota IVA")
                AlinearMoneda("Total Factura")

                ' CÁLCULO DE TOTALES
                Dim sumBase As Decimal = 0
                Dim sumIva As Decimal = 0
                Dim sumTotal As Decimal = 0

                For Each row As DataRow In dt.Rows
                    sumBase += If(IsDBNull(row("Base")), 0, Convert.ToDecimal(row("Base")))
                    sumIva += If(IsDBNull(row("Cuota IVA")), 0, Convert.ToDecimal(row("Cuota IVA")))
                    sumTotal += If(IsDBNull(row("Total Factura")), 0, Convert.ToDecimal(row("Total Factura")))
                Next

                lblTotalBase.Text = "Base Total: " & sumBase.ToString("C2")
                lblTotalIva.Text = "IVA Total: " & sumIva.ToString("C2")
                lblTotalFacturado.Text = "TOTAL FACTURADO: " & sumTotal.ToString("C2")
            End Using
        Catch ex As Exception
            MsgBox("Error al generar informe: " & ex.Message)
        End Try
    End Sub


    ' VISTA PREVIA CON EXPORTACIÓN DIRECTA A PDF INTEGRADA
    Private Sub PrevisualizarPDF(sender As Object, e As EventArgs)
        If dgvResultados.Rows.Count = 0 Then
            MessageBox.Show("No hay facturas para generar el informe.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        _filaActualImpresion = 0
        _paginaActual = 1

        docInforme.DefaultPageSettings.Landscape = True
        ' Forzamos el tamaño del folio a A4 internacional (8.27 x 11.69 pulgadas)
        docInforme.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        ' --- AQUÍ ESTÁ EL TRUCO DEL NOMBRE DEL ARCHIVO ---
        docInforme.DocumentName = "InformeFacturacion_" & DateTime.Now.ToString("dd-MM-yy")

        Try
            docInforme.PrinterSettings.PrinterName = "Microsoft Print to PDF"
        Catch ex As Exception
        End Try

        Dim ppd As New PrintPreviewDialog()
        ppd.Document = docInforme
        ppd.Width = 1169
        ppd.Height = 827
        ppd.Text = "Vista Previa del Informe de Facturas"
        CType(ppd, Form).WindowState = FormWindowState.Maximized

        ppd.ShowDialog()
    End Sub
    ' ESTO RESETEA EL CONTADOR SIEMPRE QUE SE VA A IMPRIMIR O GUARDAR
    Private Sub docInforme_BeginPrint(sender As Object, e As Printing.PrintEventArgs) Handles docInforme.BeginPrint
        _filaActualImpresion = 0
        _paginaActual = 1
    End Sub
    Private Sub docInforme_PrintPage(sender As Object, e As PrintPageEventArgs) Handles docInforme.PrintPage
        ' 1. FUENTES Y COLORES
        Dim fontTitulo As New Font("Segoe UI", 16, FontStyle.Bold)
        Dim fontEmpresa As New Font("Segoe UI", 12, FontStyle.Bold)
        Dim fontDatosEmpresa As New Font("Segoe UI", 9, FontStyle.Regular)
        Dim fontSubtitulo As New Font("Segoe UI", 10, FontStyle.Regular)
        Dim fontCabeceraTabla As New Font("Segoe UI", 10, FontStyle.Bold)
        Dim fontDatos As New Font("Segoe UI", 9, FontStyle.Regular)
        Dim fontTotales As New Font("Segoe UI", 11, FontStyle.Bold)
        Dim fontPie As New Font("Segoe UI", 8, FontStyle.Italic)

        Dim brochaTexto As New SolidBrush(Color.Black)
        Dim brochaGris As New SolidBrush(Color.FromArgb(235, 235, 235))
        Dim brochaRojo As New SolidBrush(Color.FromArgb(209, 52, 56))  ' Pendiente
        Dim brochaVerde As New SolidBrush(Color.FromArgb(40, 140, 90)) ' Cobrada
        Dim lapizLineaGruesa As New Pen(Color.FromArgb(80, 80, 80), 2)
        Dim lapizLineaFina As New Pen(Color.FromArgb(200, 200, 200), 1)

        Dim x As Integer = 50
        Dim y As Integer = 50
        Dim anchoPagina As Integer = e.PageBounds.Width - 100

        ' 2. RECUPERAR DATOS CORPORATIVOS DE LA BBDD
        Dim nombreEmpresa As String = "MI EMPRESA"
        Dim cifEmpresa As String = ""
        Dim dirEmpresa As String = ""
        Dim logoEmpresa As Image = Nothing

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Using cmd As New SQLiteCommand("SELECT NombreFiscal, CIF, Direccion, Poblacion, Logo FROM Empresa LIMIT 1", c)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        nombreEmpresa = If(IsDBNull(r("NombreFiscal")), "MI EMPRESA", r("NombreFiscal").ToString())
                        cifEmpresa = "CIF: " & If(IsDBNull(r("CIF")), "", r("CIF").ToString())
                        dirEmpresa = If(IsDBNull(r("Direccion")), "", r("Direccion").ToString()) & " " & If(IsDBNull(r("Poblacion")), "", r("Poblacion").ToString())

                        ' Magia para convertir el BLOB en Imagen
                        If Not IsDBNull(r("Logo")) Then
                            Dim imgData As Byte() = CType(r("Logo"), Byte())
                            Using ms As New System.IO.MemoryStream(imgData)
                                logoEmpresa = Image.FromStream(ms)
                            End Using
                        End If
                    End If
                End Using
            End Using
        Catch
        End Try

        ' 3. DIBUJAR CABECERA CORPORATIVA
        ' A. Logo y Datos Empresa (Izquierda)
        Dim cursorX As Integer = x
        If logoEmpresa IsNot Nothing Then
            Dim altoLogo As Integer = 60
            Dim anchoLogo As Integer = CInt(altoLogo * (logoEmpresa.Width / logoEmpresa.Height))
            e.Graphics.DrawImage(logoEmpresa, cursorX, y, anchoLogo, altoLogo)
            cursorX += anchoLogo + 15
        End If

        e.Graphics.DrawString(nombreEmpresa.ToUpper(), fontEmpresa, brochaTexto, cursorX, y)
        e.Graphics.DrawString(cifEmpresa, fontDatosEmpresa, brochaTexto, cursorX, y + 20)
        e.Graphics.DrawString(dirEmpresa, fontDatosEmpresa, brochaTexto, cursorX, y + 35)

        ' B. Título del Informe (Derecha)
        Dim formatoDerecha As New StringFormat() With {.Alignment = StringAlignment.Far}
        e.Graphics.DrawString("INFORME DE FACTURACIÓN", fontTitulo, brochaTexto, x + anchoPagina, y, formatoDerecha)
        e.Graphics.DrawString($"Periodo: {dtpDesde.Value.ToShortDateString()} - {dtpHasta.Value.ToShortDateString()}", fontSubtitulo, brochaTexto, x + anchoPagina, y + 25, formatoDerecha)
        e.Graphics.DrawString($"Impreso el: {DateTime.Now.ToShortDateString()}", fontPie, brochaTexto, x + anchoPagina, y + 45, formatoDerecha)

        y += 80
        e.Graphics.DrawLine(lapizLineaGruesa, x, y, x + anchoPagina, y)
        y += 20

        ' 4. DIBUJAR LA TABLA
        ' Cálculo exacto de anchos para apaisado (Total ~ 1069px)
        Dim anchos As Integer() = {110, 100, 420, 110, 100, 120, 109}
        Dim posXCol As New List(Of Integer)()
        Dim acumulado As Integer = x
        For Each w In anchos
            posXCol.Add(acumulado)
            acumulado += w
        Next

        ' Fondo gris y cabeceras
        e.Graphics.FillRectangle(brochaGris, x, y, anchoPagina, 30)
        e.Graphics.DrawString("Nº Factura", fontCabeceraTabla, brochaTexto, posXCol(0) + 5, y + 5)
        e.Graphics.DrawString("Fecha", fontCabeceraTabla, brochaTexto, posXCol(1) + 5, y + 5)
        e.Graphics.DrawString("Cliente", fontCabeceraTabla, brochaTexto, posXCol(2) + 5, y + 5)

        ' El dinero se alinea por su margen derecho (el inicio de la siguiente columna)
        e.Graphics.DrawString("Base", fontCabeceraTabla, brochaTexto, posXCol(4) - 10, y + 5, formatoDerecha)
        e.Graphics.DrawString("IVA", fontCabeceraTabla, brochaTexto, posXCol(5) - 10, y + 5, formatoDerecha)
        e.Graphics.DrawString("Total", fontCabeceraTabla, brochaTexto, posXCol(6) - 10, y + 5, formatoDerecha)
        e.Graphics.DrawString("Estado", fontCabeceraTabla, brochaTexto, posXCol(6) + 5, y + 5)
        y += 35

        ' Filas de datos
        While _filaActualImpresion < dgvResultados.Rows.Count
            Dim row As DataGridViewRow = dgvResultados.Rows(_filaActualImpresion)

            ' Control de salto de página
            If y > e.PageBounds.Height - 150 Then
                e.Graphics.DrawString($"Página {_paginaActual}", fontPie, brochaTexto, x + anchoPagina, e.PageBounds.Height - 40, formatoDerecha)
                _paginaActual += 1
                e.HasMorePages = True
                Return
            End If

            Dim numFac = If(row.Cells("Nº Factura").Value IsNot Nothing, row.Cells("Nº Factura").Value.ToString(), "")
            Dim fec = If(row.Cells("Fecha").Value IsNot Nothing, Convert.ToDateTime(row.Cells("Fecha").Value).ToShortDateString(), "")
            Dim cli = If(row.Cells("Cliente").Value IsNot Nothing, row.Cells("Cliente").Value.ToString(), "")
            If cli.Length > 55 Then cli = cli.Substring(0, 52) & "..." ' Acortamos si es muy largo

            Dim baseStr = If(row.Cells("Base").Value IsNot Nothing, Convert.ToDecimal(row.Cells("Base").Value).ToString("N2"), "0,00")
            Dim ivaStr = If(row.Cells("Cuota IVA").Value IsNot Nothing, Convert.ToDecimal(row.Cells("Cuota IVA").Value).ToString("N2"), "0,00")
            Dim totStr = If(row.Cells("Total Factura").Value IsNot Nothing, Convert.ToDecimal(row.Cells("Total Factura").Value).ToString("N2"), "0,00")
            Dim est = If(row.Cells("Estado").Value IsNot Nothing, row.Cells("Estado").Value.ToString(), "")

            e.Graphics.DrawString(numFac, fontDatos, brochaTexto, posXCol(0) + 5, y)
            e.Graphics.DrawString(fec, fontDatos, brochaTexto, posXCol(1) + 5, y)
            e.Graphics.DrawString(cli, fontDatos, brochaTexto, posXCol(2) + 5, y)

            e.Graphics.DrawString(baseStr & " €", fontDatos, brochaTexto, posXCol(4) - 10, y, formatoDerecha)
            e.Graphics.DrawString(ivaStr & " €", fontDatos, brochaTexto, posXCol(5) - 10, y, formatoDerecha)
            e.Graphics.DrawString(totStr & " €", fontDatos, brochaTexto, posXCol(6) - 10, y, formatoDerecha)

            e.Graphics.DrawString(est, fontDatos, brochaTexto, posXCol(6) + 5, y)

            y += 20
            e.Graphics.DrawLine(lapizLineaFina, x, y, x + anchoPagina, y) ' Línea separadora
            y += 8

            _filaActualImpresion += 1
        End While

        ' 5. TOTALES FINALES Y DESGLOSE FINANCIERO (4 ESTADOS)
        e.HasMorePages = False

        y += 15
        e.Graphics.DrawLine(lapizLineaGruesa, x, y, x + anchoPagina, y)
        y += 15

        ' Variables para nuestro análisis de cobros
        Dim totalCobrado As Decimal = 0
        Dim totalPendiente As Decimal = 0
        Dim totalVencido As Decimal = 0
        Dim totalCancelado As Decimal = 0

        ' Leemos la tabla y repartimos el dinero en sus 4 sacos
        For Each r As DataGridViewRow In dgvResultados.Rows
            Dim est = If(r.Cells("Estado").Value IsNot Nothing, r.Cells("Estado").Value.ToString().ToUpper(), "")
            Dim tot = If(r.Cells("Total Factura").Value IsNot Nothing, Convert.ToDecimal(r.Cells("Total Factura").Value), 0D)

            Select Case est
                Case "COBRADA", "PAGADA"
                    totalCobrado += tot
                Case "VENCIDA"
                    totalVencido += tot
                Case "CANCELADA", "ANULADA"
                    totalCancelado += tot
                Case Else ' "PENDIENTE" y cualquier cosa rara que se escriba
                    totalPendiente += tot
            End Select
        Next

        ' Caja principal de totales (Lo que se declara a Hacienda)
        e.Graphics.FillRectangle(brochaGris, posXCol(2), y, anchoPagina - posXCol(2), 40)
        e.Graphics.DrawString("RESUMEN TOTAL:", fontTotales, brochaTexto, posXCol(2) + 10, y + 10)
        e.Graphics.DrawString(lblTotalBase.Text.Replace("Base Total: ", ""), fontTotales, brochaTexto, posXCol(4) - 10, y + 10, formatoDerecha)
        e.Graphics.DrawString(lblTotalIva.Text.Replace("IVA Total: ", ""), fontTotales, brochaTexto, posXCol(5) - 10, y + 10, formatoDerecha)
        e.Graphics.DrawString(lblTotalFacturado.Text.Replace("TOTAL FACTURADO: ", ""), fontTotales, brochaTexto, posXCol(6) - 10, y + 10, formatoDerecha)

        y += 55

        ' --- DESGLOSE FINANCIERO PRO ---
        ' Pinceles extra que necesitamos aquí
        Dim brochaNaranja As New SolidBrush(Color.FromArgb(230, 120, 20)) ' Para lo Pendiente en plazo
        Dim brochaGrisOscuro As New SolidBrush(Color.FromArgb(150, 150, 150)) ' Para las canceladas

        e.Graphics.DrawString("Análisis de Cobros:", fontCabeceraTabla, brochaTexto, posXCol(4), y)
        y += 25

        ' 1. Cobrado (Dinero en el banco - Verde)
        e.Graphics.DrawString("Cobrado:", fontDatos, brochaTexto, posXCol(4), y)
        e.Graphics.DrawString(totalCobrado.ToString("C2"), fontTotales, brochaVerde, x + anchoPagina, y, formatoDerecha)
        y += 20

        ' 2. Pendiente (Dinero en la calle pero en plazo - Naranja)
        e.Graphics.DrawString("Pendiente:", fontDatos, brochaTexto, posXCol(4), y)
        e.Graphics.DrawString(totalPendiente.ToString("C2"), fontTotales, brochaNaranja, x + anchoPagina, y, formatoDerecha)
        y += 20

        ' 3. Vencido (Morosos - Rojo). Solo lo pintamos si hay morosos.
        If totalVencido > 0 Then
            e.Graphics.DrawString("Vencido:", fontDatos, brochaTexto, posXCol(4), y)
            e.Graphics.DrawString(totalVencido.ToString("C2"), fontTotales, brochaRojo, x + anchoPagina, y, formatoDerecha)
            y += 20
        End If

        ' 4. Cancelado (Dinero que no existe - Gris). Solo lo pintamos si hemos anulado algo.
        If totalCancelado > 0 Then
            e.Graphics.DrawString("Facturas Anuladas:", fontDatos, brochaTexto, posXCol(4), y)
            e.Graphics.DrawString(totalCancelado.ToString("C2"), fontTotales, brochaGrisOscuro, x + anchoPagina, y, formatoDerecha)
            y += 20
        End If

        ' Paginación de la última hoja
        e.Graphics.DrawString($"Página {_paginaActual}", fontPie, brochaTexto, x + anchoPagina, e.PageBounds.Height - 40, formatoDerecha)
    End Sub
End Class
