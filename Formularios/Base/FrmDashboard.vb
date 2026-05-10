Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Data.SQLite
Public Class FrmDashboard
    Private chartVentas As New Chart()

    Private Sub FrmDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = Color.FromArgb(70, 75, 80)
        Me.Controls.Clear()

        ' 1. TÍTULO
        Dim lblTitulo As New Label With {
            .Text = "ANÁLISIS DE VENTAS " & DateTime.Now.Year,
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = Color.WhiteSmoke,
            .AutoSize = True,
            .Location = New Point(40, 25)
        }
        Me.Controls.Add(lblTitulo)

        ' 2. CONTENEDOR DE TARJETAS (Arriba)
        Dim panelTarjetas As New FlowLayoutPanel With {
            .Location = New Point(40, 75),
            .Size = New Size(Me.Width - 80, 150),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False
        }
        Me.Controls.Add(panelTarjetas)

        ' 3. CONTENEDOR PARA LA GRÁFICA (Abajo)
        Dim panelGrafica As New Panel With {
            .Location = New Point(40, 240),
            .Size = New Size(Me.Width - 80, Me.Height - 280),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right,
            .BackColor = Color.FromArgb(40, 50, 70)
        }
        Me.Controls.Add(panelGrafica)

        ' --- CARGAR DATOS ---
        CargarTarjetas(panelTarjetas)
        ConfigurarGraficaInteractiva(panelGrafica)
    End Sub

    Private Sub CargarTarjetas(contenedor As FlowLayoutPanel)
        ' Recuperamos los 5 datos clave
        Dim vAnio = GetTotalAño()
        Dim vPendiente = GetTotalEstado("Pendiente")
        Dim vVencido = GetTotalEstado("Vencida")
        Dim nClientes = GetConteoTabla("Clientes")
        Dim nSinStock = GetConteoTabla("Articulos", "WHERE StockActual <= 0")

        ' Añadimos las 5 tarjetas al panel
        contenedor.Controls.Add(CrearTarjeta("Ventas " & DateTime.Now.Year, vAnio.ToString("C2"), Color.FromArgb(0, 150, 255)))
        contenedor.Controls.Add(CrearTarjeta("Pendiente", vPendiente.ToString("C2"), Color.FromArgb(230, 120, 20)))
        contenedor.Controls.Add(CrearTarjeta("Vencido", vVencido.ToString("C2"), Color.FromArgb(209, 52, 56)))
        contenedor.Controls.Add(CrearTarjeta("Clientes", nClientes.ToString(), Color.FromArgb(40, 140, 90)))
        contenedor.Controls.Add(CrearTarjeta("Sin Stock", nSinStock.ToString(), Color.FromArgb(255, 193, 7)))
    End Sub

    Private Sub ConfigurarGraficaInteractiva(contenedor As Panel)
        chartVentas.Dock = DockStyle.Fill
        chartVentas.BackColor = Color.Transparent

        ' Área del gráfico
        Dim area As New ChartArea("Principal")
        area.BackColor = Color.Transparent
        area.AxisX.LabelStyle.ForeColor = Color.WhiteSmoke
        area.AxisX.LineColor = Color.Gray
        area.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 60, 60)
        area.AxisX.Interval = 1
        area.AxisY.LabelStyle.ForeColor = Color.WhiteSmoke
        area.AxisY.LineColor = Color.Gray
        area.AxisY.MajorGrid.LineColor = Color.FromArgb(60, 60, 60)
        area.AxisY.LabelStyle.Format = "{0} €"

        chartVentas.ChartAreas.Clear()
        chartVentas.ChartAreas.Add(area)

        Dim serie As New Series("Ventas")
        serie.ChartType = SeriesChartType.Column
        serie.Color = Color.FromArgb(0, 150, 255)
        serie.ToolTip = "Total: #VALY{C2}"
        chartVentas.Series.Clear()
        chartVentas.Series.Add(serie)

        ' --- LÓGICA PARA PINTAR LOS 12 MESES ---
        Dim ventasPorMes As New Dictionary(Of Integer, Decimal)
        For i As Integer = 1 To 12 : ventasPorMes.Add(i, 0) : Next ' Inicializamos todo el año a 0

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim sql = "SELECT strftime('%m', Fecha) as Mes, SUM(TotalFactura) as Total " &
                      "FROM FacturasVenta WHERE Fecha LIKE @anio AND Estado <> 'Cancelada' GROUP BY Mes"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@anio", DateTime.Now.Year.ToString() & "-%")
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        Dim m As Integer = Convert.ToInt32(r("Mes"))
                        ventasPorMes(m) = Convert.ToDecimal(r("Total"))
                    End While
                End Using
            End Using
        Catch : End Try

        ' Dibujamos los 12 puntos en el gráfico
        Dim nombresMeses() As String = {"", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"}
        For i As Integer = 1 To 12
            serie.Points.AddXY(nombresMeses(i), ventasPorMes(i))
        Next

        contenedor.Controls.Add(chartVentas)
    End Sub

    ' --- HELPERS DE APOYO ---
    Private Function GetTotalAño() As Decimal
        Try
            Dim c = ConexionBD.GetConnection()
            Dim cmd As New SQLiteCommand("SELECT SUM(TotalFactura) FROM FacturasVenta WHERE Fecha LIKE @anio AND Estado <> 'Cancelada'", c)
            cmd.Parameters.AddWithValue("@anio", DateTime.Now.Year.ToString() & "%")
            Return Convert.ToDecimal(If(cmd.ExecuteScalar(), 0))
        Catch : Return 0 : End Try
    End Function

    Private Function GetTotalEstado(estado As String) As Decimal
        Try
            Dim c = ConexionBD.GetConnection()
            Dim cmd As New SQLiteCommand("SELECT SUM(TotalFactura) FROM FacturasVenta WHERE Estado = @est", c)
            cmd.Parameters.AddWithValue("@est", estado)
            Return Convert.ToDecimal(If(cmd.ExecuteScalar(), 0))
        Catch : Return 0 : End Try
    End Function

    Private Function CrearTarjeta(titulo As String, valor As String, colorAcento As Color) As Panel
        Dim pnl As New Panel With {.Size = New Size(220, 100), .BackColor = Color.FromArgb(40, 50, 70), .Margin = New Padding(0, 0, 20, 0)}
        Dim pBorde As New Panel With {.Height = 4, .Dock = DockStyle.Top, .BackColor = colorAcento}
        Dim lTit As New Label With {.Text = titulo, .Location = New Point(10, 20), .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 8, FontStyle.Bold), .AutoSize = True}
        Dim lVal As New Label With {.Text = valor, .Location = New Point(10, 45), .ForeColor = colorAcento, .Font = New Font("Segoe UI", 16, FontStyle.Bold), .AutoSize = True}
        ' --- NUEVO: ASIGNAR EVENTO SI ES LA TARJETA DE STOCK ---
        If titulo.Contains("Sin Stock") Then
            ' Le damos el nombre para identificarla luego
            pnl.Name = "CardStock"
            lTit.Name = "CardStock"
            lVal.Name = "CardStock"

            ' Añadimos el evento a los tres (Panel, Titulo y Valor) para que de igual donde pinches
            AddHandler pnl.Click, AddressOf TarjetaStock_Click
            AddHandler lTit.Click, AddressOf TarjetaStock_Click
            AddHandler lVal.Click, AddressOf TarjetaStock_Click
        End If

        pnl.Controls.AddRange({pBorde, lTit, lVal})
        Return pnl

    End Function
    ' El evento que salta al pinchar
    Private Sub TarjetaStock_Click(sender As Object, e As EventArgs)
        ' Buscamos la instancia de la PagHome (que es el padre de este Dashboard)
        Dim home = DirectCast(Me.ParentForm, PagHome)

        ' Creamos el formulario de artículos pero con un "truco": le pasamos un parámetro
        Dim frm As New frmArticulos(soloSinStock:=True)

        ' Usamos tu método de la PagHome para abrirlo
        home.AbrirFormulario(frm)
    End Sub
    ' --- FUNCIÓN AUXILIAR PARA CONTAR (Añade esta también) ---
    Private Function GetConteoTabla(tabla As String, Optional filtro As String = "") As Integer
        Try
            Dim c = ConexionBD.GetConnection()
            Dim cmd As New SQLiteCommand($"SELECT COUNT(*) FROM {tabla} {filtro}", c)
            Return Convert.ToInt32(cmd.ExecuteScalar())
        Catch : Return 0 : End Try

    End Function
End Class