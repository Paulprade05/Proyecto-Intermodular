Imports System.Data.SQLite
Imports System.Drawing.Printing

Public Class FrmInformeStockMinimo

    ' los controles que pongo en el form (los creo a mano, sin diseñador)
    Private WithEvents btnGenerar As New Button()
    Private WithEvents btnImprimir As New Button()
    Private dgvStock As New DataGridView()

    ' cosas para imprimir el pdf
    ' la fila actual la guardo aparte porque al imprimir paginas largas
    ' tengo que acordarme por donde iba
    Private WithEvents docInforme As New PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private _dtStock As New DataTable()

    Private Sub FrmInformeStockMinimo_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Alerta de Stock Mínimo"
        Me.BackColor = Color.FromArgb(70, 75, 80)

        ConstruirInterfaz()

        ' nada mas abrir ya cargo los datos, asi el usuario no tiene que darle a un boton
        CargarAlertasStock()
    End Sub

    Private Sub ConstruirInterfaz()
        ' por si las moscas limpio los controles antes de empezar
        Me.Controls.Clear()

        ' barra de arriba con el titulo y los botones
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 110,
            .BackColor = Color.FromArgb(70, 75, 80)
        }
        Me.Controls.Add(pnlHeader)

        Dim lblTitulo As New Label With {.Text = "ALERTA DE STOCK MÍNIMO", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .ForeColor = Color.WhiteSmoke, .AutoSize = True, .Location = New Point(30, 20)}
        pnlHeader.Controls.Add(lblTitulo)

        btnGenerar.Text = "Actualizar Datos" : btnGenerar.Bounds = New Rectangle(30, 70, 150, 30)
        btnGenerar.BackColor = Color.FromArgb(0, 120, 215) : btnGenerar.ForeColor = Color.White : btnGenerar.FlatStyle = FlatStyle.Flat : btnGenerar.FlatAppearance.BorderSize = 0
        pnlHeader.Controls.Add(btnGenerar)

        btnImprimir.Text = "Exportar PDF" : btnImprimir.Bounds = New Rectangle(200, 70, 120, 30)
        btnImprimir.BackColor = Color.FromArgb(209, 52, 56) ' rojo porque es una alerta, llama mas la atencion
        btnImprimir.ForeColor = Color.White : btnImprimir.FlatStyle = FlatStyle.Flat : btnImprimir.FlatAppearance.BorderSize = 0
        btnImprimir.Visible = False
        pnlHeader.Controls.Add(btnImprimir)

        ' panel donde mete la tabla, con un poco de margen para que no quede pegado
        Dim pnlGridContainer As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(30, 20, 30, 20)
        }
        Me.Controls.Add(pnlGridContainer)
        pnlGridContainer.BringToFront()

        ' la tabla en si
        dgvStock.Dock = DockStyle.Fill
        dgvStock.AllowUserToAddRows = False
        dgvStock.ReadOnly = True
        dgvStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvStock.BorderStyle = BorderStyle.None
        dgvStock.RowHeadersVisible = False

        ' Estilo corporativo
        Try : FrmPresupuestos.EstilizarGrid(dgvStock) : Catch : End Try

        dgvStock.BackgroundColor = Me.BackColor
        dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        pnlGridContainer.Controls.Add(dgvStock)
    End Sub

    ' LÓGICA DE BASE DE DATOS
    Private Sub btnGenerar_Click(sender As Object, e As EventArgs) Handles btnGenerar.Click
        CargarAlertasStock()
    End Sub

    Private Sub CargarAlertasStock()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Filtramos donde el stock es peligroso y calculamos cuántos faltan para llegar al mínimo
            Dim sql As String = "SELECT CodigoReferencia, Descripcion, StockMinimo, StockActual, (StockMinimo - StockActual) AS Reponer " &
                                "FROM Articulos " &
                                "WHERE StockActual <= StockMinimo AND Activo = 1 " &
                                "ORDER BY Reponer DESC, Descripcion ASC"

            Using cmd As New SQLiteCommand(sql, c)
                Dim da As New SQLiteDataAdapter(cmd)
                _dtStock.Clear()
                da.Fill(_dtStock)
            End Using

            ConfigurarGridStock()
            btnImprimir.Visible = _dtStock.Rows.Count > 0

        Catch ex As Exception
            MessageBox.Show("Error al cargar las alertas: " & ex.Message)
        End Try
    End Sub

    Private Sub ConfigurarGridStock()
        dgvStock.DataSource = _dtStock
        If dgvStock.Columns.Count = 0 Then Return

        If dgvStock.Columns.Contains("CodigoReferencia") Then
            dgvStock.Columns("CodigoReferencia").HeaderText = "Cód."
            dgvStock.Columns("CodigoReferencia").Width = 100
            dgvStock.Columns("CodigoReferencia").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If

        If dgvStock.Columns.Contains("Descripcion") Then
            dgvStock.Columns("Descripcion").HeaderText = "Descripción del Artículo"
            dgvStock.Columns("Descripcion").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        End If

        Dim FormatearNumero = Sub(nombre As String, titulo As String, ancho As Integer, negrita As Boolean)
                                  If dgvStock.Columns.Contains(nombre) Then
                                      dgvStock.Columns(nombre).HeaderText = titulo
                                      dgvStock.Columns(nombre).Width = ancho
                                      dgvStock.Columns(nombre).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                                      dgvStock.Columns(nombre).DefaultCellStyle.Format = "N2"
                                      If negrita Then
                                          dgvStock.Columns(nombre).DefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
                                          dgvStock.Columns(nombre).DefaultCellStyle.ForeColor = Color.DarkRed
                                      End If
                                  End If
                              End Sub

        FormatearNumero("StockMinimo", "Stock Mín.", 110, False)
        FormatearNumero("StockActual", "Stock Actual", 110, True)
        FormatearNumero("Reponer", "A Reponer", 110, False)
    End Sub

    ' MOTOR DE IMPRESIÓN PDF
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        docInforme.DefaultPageSettings.Landscape = False
        docInforme.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docInforme.DocumentName = "AlertaStock_" & DateTime.Now.ToString("ddMMyy")

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
        Dim bRojo As New SolidBrush(Color.FromArgb(209, 52, 56))
        Dim bGris As New SolidBrush(Color.FromArgb(220, 220, 220))
        Dim lapizFino As New Pen(Color.FromArgb(220, 220, 220), 1) ' Para las líneas de separación

        Dim yPos As Integer = 50
        Dim margenIzq As Integer = 50

        If _filaActualImpresion = 0 Then
            g.DrawString("OPTIMA SOFTWARE - DEPARTAMENTO DE COMPRAS", fSub, bNegro, margenIzq, yPos)
            yPos += 25
            g.DrawString("ALERTA DE ROTURA DE STOCK", fTitulo, bRojo, margenIzq, yPos)
            yPos += 30
            g.DrawString($"Evaluación a fecha: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", fSub, bNegro, margenIzq, yPos)
            yPos += 40
        End If

        ' Cabecera de tabla gris
        g.FillRectangle(bGris, margenIzq, yPos, 727, 30)
        g.DrawString("Cód", fCabecera, bNegro, margenIzq + 10, yPos + 5)
        g.DrawString("Descripción", fCabecera, bNegro, margenIzq + 90, yPos + 5)
        g.DrawString("Stock Mín.", fCabecera, bNegro, margenIzq + 480, yPos + 5)
        g.DrawString("Actual", fCabecera, bNegro, margenIzq + 580, yPos + 5)
        g.DrawString("A Reponer", fCabecera, bNegro, margenIzq + 660, yPos + 5)
        yPos += 35

        Dim formatoCentro As New StringFormat() With {.Alignment = StringAlignment.Center}
        Dim numArticulosEnAlerta As Integer = 0

        While _filaActualImpresion < _dtStock.Rows.Count
            Dim row As DataRow = _dtStock.Rows(_filaActualImpresion)

            ' Imprimir código
            g.DrawString(row("CodigoReferencia").ToString(), fFila, bNegro, margenIzq + 10, yPos)

            ' Imprimir descripción recortada
            Dim desc As String = row("Descripcion").ToString()
            If desc.Length > 50 Then desc = desc.Substring(0, 47) & "..."
            g.DrawString(desc, fFila, bNegro, margenIzq + 90, yPos)

            ' Imprimir valores numéricos
            Dim minimo As Decimal = Convert.ToDecimal(row("StockMinimo"))
            Dim actual As Decimal = Convert.ToDecimal(row("StockActual"))
            Dim reponer As Decimal = Convert.ToDecimal(row("Reponer"))

            g.DrawString(minimo.ToString("N2"), fFila, bNegro, margenIzq + 510, yPos, formatoCentro)
            g.DrawString(actual.ToString("N2"), fFilaBold, bRojo, margenIzq + 600, yPos, formatoCentro)
            g.DrawString(reponer.ToString("N2"), fFilaBold, bNegro, margenIzq + 695, yPos, formatoCentro)

            yPos += 25
            ' Añadimos la línea fina separadora
            g.DrawLine(lapizFino, margenIzq, yPos, margenIzq + 727, yPos)
            yPos += 5

            _filaActualImpresion += 1

            If yPos > 1050 AndAlso _filaActualImpresion < _dtStock.Rows.Count Then
                e.HasMorePages = True
                Return
            End If
        End While

        ' Bloque resumen final
        If _filaActualImpresion >= _dtStock.Rows.Count Then
            yPos += 15
            g.DrawString($"Total de artículos en alerta: {_dtStock.Rows.Count}", fCabecera, bNegro, margenIzq, yPos)
            e.HasMorePages = False
        End If
    End Sub

End Class
