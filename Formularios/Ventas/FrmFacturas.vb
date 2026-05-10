Imports System.Data.SQLite
Imports System.Text
Imports System.Security.Cryptography
Imports System.Runtime.InteropServices
Public Class FrmFacturas
    ' Variable para recordar si la agencia seleccionada no tiene rutas asignadas
    Private _agenciaManualSinRutas As Boolean = False
    Private _numeroFacturaActual As String = ""
    Private _dtLineas As DataTable
    Private _idsParaBorrar As New List(Of Integer)
    Private WithEvents cboFormaPago As New System.Windows.Forms.ComboBox()
    Private WithEvents cboRuta As New System.Windows.Forms.ComboBox()
    Private lblFormaPago As New Label() With {.Text = "Forma de Pago", .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.WhiteSmoke}
    Private lblRuta As New Label() With {.Text = "Ruta Asignada", .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.WhiteSmoke}
    Private WithEvents cboEstado As New ComboBox()
    ' --- MOTOR DE IMPRESIÓN DEL DOCUMENTO ---
    Private WithEvents btnImprimir As New Button()
    Private WithEvents docImprimir As New Printing.PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private Sub FrmFacturas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SuspendLayout()
        FrmPresupuestos.EstilizarGrid(DataGridView1)
        EstilizarFecha(DateTimePickerFecha)
        If ComboBoxPortes.Items.Count = 0 Then
            ComboBoxPortes.Items.Add("Pagados")
            ComboBoxPortes.Items.Add("Debidos")
            ComboBoxPortes.SelectedIndex = 0
        End If
        ConfigurarGrid()
        Dim listaAgencias As List(Of Agencia) = ObtenerAgencias()
        With cboAgencias
            .DataSource = listaAgencias
            .DisplayMember = "Nombre"    ' Lo que ve el usuario
            .ValueMember = "ID_Agencia"  ' El valor que usaremos para guardar en la DB
            .SelectedIndex = -1          ' Empezar sin ninguna seleccionada
        End With
        Me.Controls.Add(lblFormaPago) : Me.Controls.Add(cboFormaPago)
        Me.Controls.Add(lblRuta) : Me.Controls.Add(cboRuta)
        cboFormaPago.DropDownStyle = ComboBoxStyle.DropDownList
        cboRuta.DropDownStyle = ComboBoxStyle.DropDownList
        Me.Controls.Add(cboEstado)
        cboEstado.DropDownStyle = ComboBoxStyle.DropDownList
        cboEstado.Items.Clear()
        cboEstado.Items.AddRange(New String() {"Pendiente", "Cobrada", "Vencida", "Cancelada"})
        CargarDesplegables()
        Dim ultimoNum As String = ObtenerUltimoNumeroFactura()
        If Not String.IsNullOrEmpty(ultimoNum) Then
            CargarFactura(ultimoNum)
        Else
            LimpiarFormulario()
        End If
        TabControlModerno2.DrawMode = TabDrawMode.Normal
        TabControlModerno2.SizeMode = TabSizeMode.Normal
        TabControlModerno2.ItemSize = New Size(0, 0) ' (0,0) hace que Windows use el ancho real del texto
        If TextBox5 IsNot Nothing Then TextBox5.Visible = False
        If Button4 IsNot Nothing Then Button4.Visible = False

        ' --- AÑADE ESTAS DOS LÍNEAS AQUÍ ---
        ReorganizarControlesAutomaticamente()
        Me.ResumeLayout(True)
    End Sub
    ' 1. CONFIGURACIÓN DE DATOS Y GRID
    Private Sub ConfigurarEstructuraDatos()
        If _dtLineas Is Nothing Then _dtLineas = New DataTable()

        ' Columnas de control
        If Not _dtLineas.Columns.Contains("ID_Linea") Then _dtLineas.Columns.Add("ID_Linea", GetType(Object))
        If Not _dtLineas.Columns.Contains("NumeroFactura") Then _dtLineas.Columns.Add("NumeroFactura", GetType(String))

        ' Datos de línea
        If Not _dtLineas.Columns.Contains("NumeroOrden") Then _dtLineas.Columns.Add("NumeroOrden", GetType(Integer))
        If Not _dtLineas.Columns.Contains("ID_Articulo") Then _dtLineas.Columns.Add("ID_Articulo", GetType(Object))
        If Not _dtLineas.Columns.Contains("Descripcion") Then _dtLineas.Columns.Add("Descripcion", GetType(String))

        ' Valores numéricos
        If Not _dtLineas.Columns.Contains("Cantidad") Then _dtLineas.Columns.Add("Cantidad", GetType(Decimal)) ' Ojo: CantidadServida
        If Not _dtLineas.Columns.Contains("PrecioUnitario") Then _dtLineas.Columns.Add("PrecioUnitario", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("Descuento") Then _dtLineas.Columns.Add("Descuento", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("PorcentajeIVA") Then _dtLineas.Columns.Add("PorcentajeIVA", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("Total") Then _dtLineas.Columns.Add("Total", GetType(Decimal))

        ' Trazabilidad con Pedido (Opcional si lo usas)
        If Not _dtLineas.Columns.Contains("ID_LineaPedido") Then _dtLineas.Columns.Add("ID_LineaPedido", GetType(Object))
        ' Nuevas columnas calculadas para el IVA
        If Not _dtLineas.Columns.Contains("PrecioConIVA") Then _dtLineas.Columns.Add("PrecioConIVA", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("TotalConIVA") Then _dtLineas.Columns.Add("TotalConIVA", GetType(Decimal))
    End Sub
    Private Sub ConfigurarGrid()
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.Columns.Clear()

        ' --- Columna técnica oculta ---
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Linea", .DataPropertyName = "ID_Linea", .Visible = False})

        ' --- 1. Identificación ---
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NumeroOrden", .DataPropertyName = "NumeroOrden", .HeaderText = "Nº", .Width = 40, .ReadOnly = True, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Articulo", .DataPropertyName = "ID_Articulo", .HeaderText = "ID Art", .Width = 60})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        ' --- 2. Cantidad y Precio ---
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Cantidad", .DataPropertyName = "Cantidad", .HeaderText = "Cantidad", .Width = 75, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2", .BackColor = Color.Ivory}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PrecioUnitario", .DataPropertyName = "PrecioUnitario", .HeaderText = "Precio Base", .Width = 85, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2"}})

        ' --- 3. Modificadores (% Dto y % IVA) ---
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descuento", .DataPropertyName = "Descuento", .HeaderText = "% Dto", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2"}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PorcentajeIVA", .DataPropertyName = "PorcentajeIVA", .HeaderText = "% IVA", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N0"}})

        ' --- 4. Totales de la línea ---
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Total", .DataPropertyName = "Total", .HeaderText = "Total Base", .ReadOnly = True, .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.WhiteSmoke}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "TotalConIVA", .DataPropertyName = "TotalConIVA", .HeaderText = "Total (+IVA)", .ReadOnly = True, .Width = 100, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.FromArgb(230, 240, 250)}})
    End Sub

    Private Function ObtenerUltimoNumeroFactura() As String
        Dim ultimoNumero As String = ""
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim sql As String = "SELECT MAX(NumeroFactura) FROM FacturasVenta"
            Using cmd As New SQLiteCommand(sql, conexion)
                If conexion.State <> ConnectionState.Open Then conexion.Open()

                Dim resultado = cmd.ExecuteScalar()

                If resultado IsNot Nothing AndAlso Not IsDBNull(resultado) Then
                    ultimoNumero = resultado.ToString()
                End If
            End Using
        Catch ex As Exception

        End Try
        Return ultimoNumero
    End Function
    Private Sub ImportarDatosPedido(numeroAlbaran As String)
        Dim c = ConexionBD.GetConnection()
        Try
            If c.State <> ConnectionState.Open Then c.Open()
            Dim sqlCab As String = "SELECT A.*, C.NombreFiscal, C.Direccion, C.Poblacion, C.CodigoPostal " &
                                   "FROM Albaranes A " &
                                   "LEFT JOIN Clientes C ON A.ID_Cliente = C.ID_Cliente " &
                                   "LEFT JOIN Vendedores V ON A.ID_Vendedor = V.ID_Vendedor " &
                                   "WHERE A.NumeroAlbaran = @num"
            Using cmd As New SQLiteCommand(sqlCab, c)
                cmd.Parameters.AddWithValue("@num", numeroAlbaran)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        LimpiarFormulario()

                        ' Referencia
                        TextBoxAlbaranOrigen.Text = numeroAlbaran
                        TextBoxAlbaranOrigen.Tag = numeroAlbaran ' Guardamos para BD

                        ' Cliente
                        TextBoxIdCliente.Text = r("ID_Cliente").ToString()
                        TextBoxCliente.Text = r("NombreFiscal").ToString()
                        TextBoxIdVendedor.Text = If(IsDBNull(r("ID_Vendedor")), "", r("ID_Vendedor").ToString())
                        TextBoxVendedor.Text = If(IsDBNull(r("NombreVend")), "", r("NombreVend").ToString())

                        ' Dirección de Envío (Se congela aquí)
                        TextBoxDireccion.Text = If(IsDBNull(r("Direccion")), "", r("Direccion").ToString())
                        TextBoxPoblacion.Text = If(IsDBNull(r("Poblacion")), "", r("Poblacion").ToString())
                        TextBoxCP.Text = If(IsDBNull(r("CodigoPostal")), "", r("CodigoPostal").ToString())

                        TextBoxObservaciones.Text = "Generado desde Pedido " & numeroAlbaran
                    End If
                End Using
            End Using
            Dim sqlLin As String = "SELECT * FROM LineasAlbaran Where NumeroAlbaran = @num order by NumeroOrden ASC"
            Dim dtOrigen As New DataTable()
            Using cmd As New SQLiteCommand(sqlLin, c)
                cmd.Parameters.AddWithValue("@num", numeroAlbaran)
                Dim da As New SQLiteDataAdapter(cmd)
                da.Fill(dtOrigen)
            End Using
            _dtLineas.Rows.Clear()
            For Each rowOrig As DataRow In dtOrigen.Rows
                Dim rowNew As DataRow = _dtLineas.NewRow()

                rowNew("NumeroFactura") = _numeroFacturaActual
                rowNew("ID_Linea") = DBNull.Value

                rowNew("NumeroOrden") = rowOrig("NumeroOrden")
                rowNew("ID_Articulo") = rowOrig("ID_Articulo")
                rowNew("Descripcion") = rowOrig("Descripcion")

                ' Mapeo de Cantidad
                Dim cant As Decimal = 0 : Decimal.TryParse(rowOrig("Cantidad").ToString(), cant)
                rowNew("CantidadServida") = cant ' Por defecto servimos todo lo pedido

                ' Precios
                rowNew("PrecioUnitario") = rowOrig("PrecioUnitario")
                rowNew("Descuento") = rowOrig("Descuento")
                rowNew("Total") = rowOrig("Total")

                ' Trazabilidad (Guardamos el ID de la línea original si lo tienes)
                rowNew("ID_LineaPedido") = rowOrig("ID_Linea")

                _dtLineas.Rows.Add(rowNew)
            Next

            CalcularTotales()
            MessageBox.Show("Datos del Pedido cargados.")
        Catch ex As Exception
            MessageBox.Show("Error al importar: " & ex.Message)
        End Try
    End Sub
    Private Sub CargarDesplegables()
        _agenciaManualSinRutas = False
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            ' Formas de pago
            Dim daPago As New SQLiteDataAdapter("SELECT ID_FormaPago, Descripcion FROM FormasPago WHERE Activo=1", c)
            Dim dtPago As New DataTable() : daPago.Fill(dtPago)
            cboFormaPago.DataSource = dtPago : cboFormaPago.DisplayMember = "Descripcion" : cboFormaPago.ValueMember = "ID_FormaPago" : cboFormaPago.SelectedIndex = -1
            ' Rutas
            Dim daRuta As New SQLiteDataAdapter("SELECT ID_Ruta, NombreZona FROM Rutas WHERE Activo=1", c)
            Dim dtRuta As New DataTable() : daRuta.Fill(dtRuta)
            cboRuta.DataSource = dtRuta : cboRuta.DisplayMember = "NombreZona" : cboRuta.ValueMember = "ID_Ruta" : cboRuta.SelectedIndex = -1
        Catch ex As Exception
        End Try
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles ButtonGuardar.Click
        If String.IsNullOrWhiteSpace(TextBoxIdCliente.Text) Then MessageBox.Show("Falta el Cliente") : Return

        Dim esNuevo As Boolean = String.IsNullOrEmpty(_numeroFacturaActual)
        If esNuevo Then
            TextBoxFactura.Text = GenerarProximoNumeroFactura()
            _numeroFacturaActual = TextBoxFactura.Text
        End If

        Dim c = ConexionBD.GetConnection()
        Dim trans As SQLiteTransaction = Nothing

        Try
            If c.State <> ConnectionState.Open Then c.Open()
            trans = c.BeginTransaction()

            ' A) Calcular Totales
            Dim sumaBase As Decimal = 0
            Dim sumaIva As Decimal = 0

            For Each r As DataRow In _dtLineas.Rows
                If r.RowState <> DataRowState.Deleted Then
                    ' IMPORTANTE: Leemos de la columna "Cantidad" (no CantidadServida)
                    Dim cant As Decimal = 0 : Decimal.TryParse(r("Cantidad").ToString(), cant)
                    Dim prec As Decimal = 0 : Decimal.TryParse(r("PrecioUnitario").ToString(), prec)
                    Dim desc As Decimal = 0 : Decimal.TryParse(r("Descuento").ToString(), desc)
                    Dim ivaLinea As Decimal = 21 : Decimal.TryParse(r("PorcentajeIVA").ToString(), ivaLinea)

                    Dim totLinea As Decimal = (cant * prec) * (1 - (desc / 100))

                    If _dtLineas.Columns.Contains("Total") Then r("Total") = totLinea

                    sumaBase += totLinea
                    sumaIva += totLinea * (ivaLinea / 100)
                End If
            Next

            Dim sumaTotal As Decimal = sumaBase + sumaIva

            ' B') Datos fiscales del cliente
            ' La factura emitida DEBE incluir el CIF/NIF del cliente (obligación legal en España).
            ' Lo leemos directamente de la tabla Clientes para no depender de lo que haya en pantalla
            ' (los TextBox son editables y podrían estar manipulados).
            Dim cifCliente As Object = DBNull.Value
            Dim nombreFiscalCliente As Object = TextBoxCliente.Text.Trim()
            Dim direccionCliente As Object = If(String.IsNullOrWhiteSpace(TextBoxDireccion.Text), DBNull.Value, CType(TextBoxDireccion.Text.Trim(), Object))
            Dim poblacionCliente As Object = If(String.IsNullOrWhiteSpace(TextBoxPoblacion.Text), DBNull.Value, CType(TextBoxPoblacion.Text.Trim(), Object))
            Dim cpCliente As Object = If(String.IsNullOrWhiteSpace(TextBoxCP.Text), DBNull.Value, CType(TextBoxCP.Text.Trim(), Object))

            If Not String.IsNullOrWhiteSpace(TextBoxIdCliente.Text) Then
                Try
                    Using cmdCli As New SQLiteCommand(
                        "SELECT NombreFiscal, CIF, Direccion, Poblacion, CodigoPostal FROM Clientes WHERE CodigoCliente = @cod",
                        c, trans)
                        cmdCli.Parameters.AddWithValue("@cod", TextBoxIdCliente.Text.Trim())
                        Using rCli = cmdCli.ExecuteReader()
                            If rCli.Read() Then
                                ' Sobrescribimos con los datos OFICIALES del cliente
                                If Not IsDBNull(rCli("NombreFiscal")) AndAlso rCli("NombreFiscal").ToString() <> "" Then
                                    nombreFiscalCliente = rCli("NombreFiscal").ToString()
                                End If
                                If Not IsDBNull(rCli("CIF")) AndAlso rCli("CIF").ToString() <> "" Then
                                    cifCliente = rCli("CIF").ToString()
                                End If
                                ' Si los TextBox de dirección están vacíos, usamos los del cliente
                                If IsDBNull(direccionCliente) AndAlso Not IsDBNull(rCli("Direccion")) Then direccionCliente = rCli("Direccion")
                                If IsDBNull(poblacionCliente) AndAlso Not IsDBNull(rCli("Poblacion")) Then poblacionCliente = rCli("Poblacion")
                                If IsDBNull(cpCliente) AndAlso Not IsDBNull(rCli("CodigoPostal")) Then cpCliente = rCli("CodigoPostal")
                            End If
                        End Using
                    End Using
                Catch
                    ' Si falla la consulta del cliente, seguimos con lo que haya en pantalla
                End Try
            End If

            ' Aviso (no bloqueante) si el cliente no tiene CIF: una factura sin CIF del receptor
            ' no es válida legalmente, así que el usuario debe ser consciente.
            If IsDBNull(cifCliente) Then
                If MessageBox.Show(
                    "El cliente no tiene CIF/NIF registrado en su ficha." & vbCrLf & vbCrLf &
                    "Una factura sin CIF del receptor no es válida legalmente." & vbCrLf &
                    "¿Quieres guardar igualmente?",
                    "Aviso fiscal",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then
                    trans.Rollback()
                    Return
                End If
            End If

            ' B) Guardar Cabecera (FacturasVenta)
            Dim sql As String
            If esNuevo Then
                sql = "INSERT INTO FacturasVenta (NumeroFactura, Fecha, CodigoCliente, NumeroAlbaran, BaseImponible, ImporteIVA, TotalFactura, Cobrada, ID_FormaPago, ID_Vendedor, FechaVencimiento, NombreFiscal, CIF, Direccion, Observaciones, ID_Ruta, ID_Agencia, NumeroBultos, PesoTotal, CodigoSeguimiento, Portes, Poblacion, CodigoPostal, Estado) " &
                      "VALUES (@num, @fecha, @cli, @alb, @base, @iva, @total, @cobrada, @fpago, @vend, @fvenc, @fiscal, @cif, @dir, @obs, @ruta, @agencia, @bultos, @peso, @track, @portes, @pob, @cp, @estado)"
            Else
                sql = "UPDATE FacturasVenta SET Fecha=@fecha, CodigoCliente=@cli, NumeroAlbaran=@alb, BaseImponible=@base, ImporteIVA=@iva, TotalFactura=@total, Cobrada=@cobrada, ID_FormaPago=@fpago, ID_Vendedor=@vend, FechaVencimiento=@fvenc, NombreFiscal=@fiscal, CIF=@cif, Direccion=@dir, Observaciones=@obs, ID_Ruta=@ruta, ID_Agencia=@agencia, NumeroBultos=@bultos, PesoTotal=@peso, CodigoSeguimiento=@track, Portes=@portes, Poblacion=@pob, CodigoPostal=@cp, Estado=@estado " &
                      "WHERE NumeroFactura=@num"
            End If

            Dim fechaFactura As DateTime = DateTime.Now
            If DateTime.TryParse(TextBoxFecha.Text, fechaFactura) = False Then fechaFactura = DateTime.Now

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Transaction = trans

                cmd.Parameters.AddWithValue("@num", _numeroFacturaActual)
                cmd.Parameters.AddWithValue("@fecha", fechaFactura)
                cmd.Parameters.AddWithValue("@cli", TextBoxIdCliente.Text)

                Dim alb As Object = If(TextBoxAlbaranOrigen.Tag IsNot Nothing, TextBoxAlbaranOrigen.Tag, DBNull.Value)
                cmd.Parameters.AddWithValue("@alb", alb)

                cmd.Parameters.AddWithValue("@base", sumaBase)
                cmd.Parameters.AddWithValue("@iva", sumaIva)
                cmd.Parameters.AddWithValue("@total", sumaTotal)

                ' Sincronizamos el booleano Cobrada con el Estado del combo:
                ' Si el usuario marca la factura como "Cobrada", el flag pasa a 1.
                ' Cualquier otro estado (Pendiente, Vencida, Cancelada) -> 0.
                Dim cobradaFlag As Integer = If(cboEstado.Text = EstadosDocumento.FACTURA_COBRADA, 1, 0)
                cmd.Parameters.AddWithValue("@cobrada", cobradaFlag)
                cmd.Parameters.AddWithValue("@fpago", If(cboFormaPago.SelectedIndex <> -1, cboFormaPago.SelectedValue, DBNull.Value))
                cmd.Parameters.AddWithValue("@fvenc", DateTimePickerFecha.Value)

                cmd.Parameters.AddWithValue("@fiscal", nombreFiscalCliente)
                cmd.Parameters.AddWithValue("@cif", cifCliente) ' Cargado desde Clientes.CIF (antes era siempre NULL)
                cmd.Parameters.AddWithValue("@dir", direccionCliente)
                cmd.Parameters.AddWithValue("@pob", poblacionCliente)
                cmd.Parameters.AddWithValue("@cp", cpCliente)

                cmd.Parameters.AddWithValue("@vend", If(String.IsNullOrWhiteSpace(TextBoxIdVendedor.Text), DBNull.Value, TextBoxIdVendedor.Text))
                cmd.Parameters.AddWithValue("@ruta", If(cboRuta.SelectedIndex <> -1, cboRuta.SelectedValue, DBNull.Value))
                cmd.Parameters.AddWithValue("@agencia", If(cboAgencias.SelectedIndex <> -1, cboAgencias.SelectedValue, DBNull.Value))

                cmd.Parameters.AddWithValue("@bultos", If(IsNumeric(TextBoxBultos.Text), CInt(TextBoxBultos.Text), 1))
                cmd.Parameters.AddWithValue("@peso", If(IsNumeric(TextBoxPeso.Text), CDbl(TextBoxPeso.Text), 0))
                cmd.Parameters.AddWithValue("@track", TextBoxTracking.Text)
                cmd.Parameters.AddWithValue("@portes", ComboBoxPortes.Text)
                cmd.Parameters.AddWithValue("@estado", cboEstado.Text)
                cmd.Parameters.AddWithValue("@obs", TextBoxObservaciones.Text)

                cmd.ExecuteNonQuery()
            End Using

            ' C) Bajas de Líneas (Devolver Stock)
            For Each idDel In _idsParaBorrar
                Dim idArtDel As Integer = 0
                Dim cantDel As Decimal = 0

                Using cmdInfo As New SQLiteCommand("SELECT ID_Articulo, Cantidad FROM LineasFacturaVenta WHERE ID_Linea = @id", c, trans)
                    cmdInfo.Parameters.AddWithValue("@id", idDel)
                    Using reader = cmdInfo.ExecuteReader()
                        If reader.Read() Then
                            If Not IsDBNull(reader("ID_Articulo")) Then idArtDel = Convert.ToInt32(reader("ID_Articulo"))
                            cantDel = Convert.ToDecimal(reader("Cantidad"))
                        End If
                    End Using
                End Using

                Using cmdDel As New SQLiteCommand("DELETE FROM LineasFacturaVenta WHERE ID_Linea = @id", c, trans)
                    cmdDel.Parameters.AddWithValue("@id", idDel)
                    cmdDel.ExecuteNonQuery()
                End Using

            Next
            _idsParaBorrar.Clear()

            ' D) Inserciones y Actualizaciones de Líneas
            If _dtLineas IsNot Nothing Then
                For Each r As DataRow In _dtLineas.Rows
                    If r.RowState = DataRowState.Deleted Then Continue For

                    Dim cant As Decimal = 0 : Decimal.TryParse(r("Cantidad").ToString(), cant)
                    Dim prec As Decimal = 0 : Decimal.TryParse(r("PrecioUnitario").ToString(), prec)
                    Dim desc As Decimal = 0 : Decimal.TryParse(r("Descuento").ToString(), desc)
                    Dim ivaPorc As Decimal = 21 : Decimal.TryParse(r("PorcentajeIVA").ToString(), ivaPorc)
                    Dim tot As Decimal = (cant * prec) * (1 - (desc / 100))

                    ' IMPORTANTE: Se llama ID_Linea en BD, pero asegurate de que en tu DataTable local (ConfigurarEstructuraDatos) también se llame así.
                    Dim idLin = r("ID_Linea")
                    Dim idArt As Object = If(IsNumeric(r("ID_Articulo")) AndAlso Val(r("ID_Articulo")) > 0, r("ID_Articulo"), DBNull.Value)

                    If IsDBNull(idLin) OrElse Not IsNumeric(idLin) Then
                        ' INSERT
                        Dim sqlIns As String = "INSERT INTO LineasFacturaVenta (NumeroFactura, NumeroOrden, ID_Articulo, Descripcion, Cantidad, PrecioUnitario, Descuento, PorcentajeIVA, Total) " &
                                               "VALUES (@idFac, @ord, @art, @desc, @cant, @prec, @dcto, @iva, @tot)"

                        Using cmdL As New SQLiteCommand(sqlIns, c, trans)
                            cmdL.Parameters.AddWithValue("@idFac", _numeroFacturaActual)
                            cmdL.Parameters.AddWithValue("@ord", r("NumeroOrden"))
                            cmdL.Parameters.AddWithValue("@art", idArt)
                            cmdL.Parameters.AddWithValue("@desc", r("Descripcion"))
                            cmdL.Parameters.AddWithValue("@cant", CDbl(cant))
                            cmdL.Parameters.AddWithValue("@prec", CDbl(prec))
                            cmdL.Parameters.AddWithValue("@dcto", CDbl(desc))
                            cmdL.Parameters.AddWithValue("@iva", CDbl(ivaPorc))
                            cmdL.Parameters.AddWithValue("@tot", CDbl(tot))
                            cmdL.ExecuteNonQuery()
                        End Using


                    ElseIf r.RowState = DataRowState.Modified Then
                        ' UPDATE
                        Dim cantAnterior As Decimal = 0
                        If r.HasVersion(DataRowVersion.Original) AndAlso Not IsDBNull(r("Cantidad", DataRowVersion.Original)) Then
                            Decimal.TryParse(r("Cantidad", DataRowVersion.Original).ToString(), cantAnterior)
                        End If
                        Dim variacion = cant - cantAnterior

                        Dim sqlUpd As String = "UPDATE LineasFacturaVenta SET " &
                                               "ID_Articulo=@art, Descripcion=@desc, Cantidad=@cant, PrecioUnitario=@prec, Descuento=@dcto, PorcentajeIVA=@iva, Total=@tot " &
                                               "WHERE ID_Linea=@id"

                        Using cmdL As New SQLiteCommand(sqlUpd, c, trans)
                            cmdL.Parameters.AddWithValue("@art", idArt)
                            cmdL.Parameters.AddWithValue("@desc", r("Descripcion"))
                            cmdL.Parameters.AddWithValue("@cant", CDbl(cant))
                            cmdL.Parameters.AddWithValue("@prec", CDbl(prec))
                            cmdL.Parameters.AddWithValue("@dcto", CDbl(desc))
                            cmdL.Parameters.AddWithValue("@iva", CDbl(ivaPorc))
                            cmdL.Parameters.AddWithValue("@tot", CDbl(tot))
                            cmdL.Parameters.AddWithValue("@id", idLin)
                            cmdL.ExecuteNonQuery()
                        End Using

                        ' Ajuste de almacén

                    End If
                Next
            End If
            ' MAGIA DE ESTADOS: Marcar Albarán/Pedido como Facturado
            If TextBoxAlbaranOrigen.Tag IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(TextBoxAlbaranOrigen.Tag.ToString()) Then
                ' Suponiendo que facturas desde Albaranes:
                Dim sqlEstado As String = "UPDATE Albaranes SET Estado = @estado WHERE NumeroAlbaran = @idOrigen"
                Using cmdEst As New SQLiteCommand(sqlEstado, c)
                    cmdEst.Transaction = trans
                    cmdEst.Parameters.AddWithValue("@estado", EstadosDocumento.ALBARAN_FACTURADO)
                    cmdEst.Parameters.AddWithValue("@idOrigen", TextBoxAlbaranOrigen.Tag.ToString())
                    cmdEst.ExecuteNonQuery()
                End Using
            End If
            trans.Commit()
            MessageBox.Show("Factura Guardada y Registrada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CargarFactura(_numeroFacturaActual)

        Catch ex As Exception
            If trans IsNot Nothing Then trans.Rollback()
            LogErrores.Registrar("FrmFacturas.Guardar", ex)
            MessageBox.Show("Error crítico al guardar la factura: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub CargarFactura(num As String)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim sql As String = "SELECT F.*, C.NombreFiscal AS NombreCliBD, V.Nombre AS NombreVendedor " &
                                "FROM FacturasVenta F " &
                                "LEFT JOIN Clientes C ON F.CodigoCliente = C.CodigoCliente " &
                                "LEFT JOIN Vendedores V ON F.ID_Vendedor = V.ID_Vendedor " &
                                "WHERE F.NumeroFactura= @num"

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@num", num)

                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        _numeroFacturaActual = num
                        TextBoxFactura.Text = reader("NumeroFactura").ToString()

                        ' --- BLINDAJE TOTAL DE FECHAS (Sin Convert.ToDateTime) ---
                        If Not IsDBNull(reader("Fecha")) AndAlso reader("Fecha").ToString().Trim() <> "" Then
                            Dim dF As DateTime
                            If DateTime.TryParse(reader("Fecha").ToString(), dF) Then
                                TextBoxFecha.Text = dF.ToShortDateString()
                            End If
                        End If

                        If Not IsDBNull(reader("FechaVencimiento")) AndAlso reader("FechaVencimiento").ToString().Trim() <> "" Then
                            Dim dV As DateTime
                            If DateTime.TryParse(reader("FechaVencimiento").ToString(), dV) Then
                                DateTimePickerFecha.Value = dV
                            End If
                        End If

                        ' Textos Simples
                        ' --- ESTADO (A prueba de mayúsculas/minúsculas) ---
                        Dim estadoDB As String = If(IsDBNull(reader("Estado")), "Pendiente", reader("Estado").ToString().Trim())

                        ' Buscamos en la lista ignorando mayúsculas y minúsculas
                        Dim encontrado As Boolean = False
                        For Each item As String In cboEstado.Items
                            If item.Equals(estadoDB, StringComparison.OrdinalIgnoreCase) Then
                                cboEstado.SelectedItem = item
                                encontrado = True
                                Exit For
                            End If
                        Next

                        ' Si después de buscar no encontró nada que se parezca, ponemos Pendiente
                        If Not encontrado Then
                            If cboEstado.Items.Count > 0 Then cboEstado.SelectedIndex = 0
                        End If
                        TextBoxObservaciones.Text = If(IsDBNull(reader("Observaciones")), "", reader("Observaciones").ToString())

                        ' Cliente y Dirección
                        TextBoxIdCliente.Text = reader("CodigoCliente").ToString()
                        Dim nombreCongelado As String = If(IsDBNull(reader("NombreFiscal")), "", reader("NombreFiscal").ToString())
                        Dim nombreActualBD As String = If(IsDBNull(reader("NombreCliBD")), "", reader("NombreCliBD").ToString())

                        TextBoxCliente.Text = If(nombreCongelado <> "", nombreCongelado, nombreActualBD)
                        TextBoxDireccion.Text = reader("Direccion").ToString()
                        TextBoxPoblacion.Text = reader("Poblacion").ToString()
                        TextBoxCP.Text = reader("CodigoPostal").ToString()

                        ' Vendedor
                        TextBoxIdVendedor.Text = If(IsDBNull(reader("ID_Vendedor")), "", reader("ID_Vendedor").ToString())
                        TextBoxVendedor.Text = If(IsDBNull(reader("NombreVendedor")), "", reader("NombreVendedor").ToString())

                        ' Combos (Desplegables)
                        If Not IsDBNull(reader("ID_Agencia")) Then cboAgencias.SelectedValue = Convert.ToInt32(reader("ID_Agencia")) Else cboAgencias.SelectedIndex = -1
                        If Not IsDBNull(reader("ID_FormaPago")) Then cboFormaPago.SelectedValue = Convert.ToInt32(reader("ID_FormaPago")) Else cboFormaPago.SelectedIndex = -1
                        If Not IsDBNull(reader("ID_Ruta")) Then cboRuta.SelectedValue = Convert.ToInt32(reader("ID_Ruta")) Else cboRuta.SelectedIndex = -1
                        ' Si la factura tiene ruta pero no agencia, derivar agencia desde la ruta
                        If cboRuta.SelectedIndex <> -1 AndAlso cboAgencias.SelectedIndex = -1 Then AutoAsignarAgenciaDesdeRuta()

                        If Not IsDBNull(reader("NumeroAlbaran")) AndAlso reader("NumeroAlbaran").ToString() <> "" Then
                            TextBoxAlbaranOrigen.Text = reader("NumeroAlbaran").ToString()
                            TextBoxAlbaranOrigen.Tag = reader("NumeroAlbaran").ToString()
                        Else
                            TextBoxAlbaranOrigen.Text = ""
                            TextBoxAlbaranOrigen.Tag = Nothing
                        End If

                        TextBoxBultos.Text = reader("NumeroBultos").ToString()
                        TextBoxPeso.Text = reader("PesoTotal").ToString()
                        TextBoxTracking.Text = reader("CodigoSeguimiento").ToString()
                        ComboBoxPortes.Text = reader("Portes").ToString()
                    Else
                        MessageBox.Show("Factura no encontrada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                End Using
            End Using

            ' Cargamos las líneas de la factura
            CargarLineas()

        Catch ex As Exception
            MessageBox.Show("Error al cargar la factura: " & ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub CargarLineas()
        Try
            Dim c = ConexionBD.GetConnection()

            Using cmd As New SQLiteCommand("SELECT * FROM LineasFacturaVenta WHERE NumeroFactura = @num ORDER BY NumeroOrden ASC", c)
                cmd.Parameters.AddWithValue("@num", _numeroFacturaActual)

                Dim da As New SQLiteDataAdapter(cmd)
                _dtLineas = New DataTable()
                da.Fill(_dtLineas)

                ' Esto añade las columnas virtuales (PrecioConIVA y TotalConIVA) vacías
                ConfigurarEstructuraDatos()

                ' MAGIA: Calculamos el IVA de las facturas viejas al cargarlas
                For Each row As DataRow In _dtLineas.Rows
                    ' Evitamos errores si alguna celda antigua está nula en la BD
                    Dim cant As Decimal = If(IsDBNull(row("Cantidad")), 0, Convert.ToDecimal(row("Cantidad")))
                    Dim prec As Decimal = If(IsDBNull(row("PrecioUnitario")), 0, Convert.ToDecimal(row("PrecioUnitario")))
                    Dim dto As Decimal = If(IsDBNull(row("Descuento")), 0, Convert.ToDecimal(row("Descuento")))
                    Dim iva As Decimal = If(IsDBNull(row("PorcentajeIVA")), 21, Convert.ToDecimal(row("PorcentajeIVA")))

                    ' Aseguramos que el % de IVA se muestre (por si en la BD era nulo)
                    row("PorcentajeIVA") = iva

                    ' Calculamos los precios virtuales
                    row("PrecioConIVA") = prec * (1 + (iva / 100))

                    Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))
                    row("Total") = totalSinIva ' Refrescamos por si acaso
                    row("TotalConIVA") = totalSinIva * (1 + (iva / 100))
                Next

                DataGridView1.DataSource = _dtLineas
            End Using

            CalcularTotalesGenerales()

            ' Ocultamos las columnas técnicas (oculto las dos por si tienes un nombre u otro)
            If DataGridView1.Columns.Contains("ID_LineaFactura") Then DataGridView1.Columns("ID_LineaFactura").Visible = False
            If DataGridView1.Columns.Contains("ID_Linea") Then DataGridView1.Columns("ID_Linea").Visible = False

        Catch ex As Exception
            MessageBox.Show("Error al cargar líneas: " & ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Private Sub CalcularTotalesGenerales()
        Dim base As Decimal = 0
        Dim sumaIva As Decimal = 0

        If _dtLineas IsNot Nothing Then
            For Each row As DataRow In _dtLineas.Rows
                If row.RowState <> DataRowState.Deleted Then
                    Dim t As Decimal = 0
                    Dim ivaLinea As Decimal = 21 ' Por defecto 21 por si acaso

                    ' Leemos el total de la línea y su IVA
                    If _dtLineas.Columns.Contains("Total") Then Decimal.TryParse(row("Total").ToString(), t)
                    If _dtLineas.Columns.Contains("PorcentajeIVA") AndAlso Not IsDBNull(row("PorcentajeIVA")) Then
                        Decimal.TryParse(row("PorcentajeIVA").ToString(), ivaLinea)
                    End If

                    ' Sumamos a los montones generales
                    base += t
                    sumaIva += t * (ivaLinea / 100)
                End If
            Next
        End If

        ' Actualizamos las cajas (Asegúrate de que tus cajas se llamen así)
        If TextBoxBase IsNot Nothing Then TextBoxBase.Text = base.ToString("C2")
        If TextBoxIva IsNot Nothing Then TextBoxIva.Text = sumaIva.ToString("C2")
        If TextBoxTotalAlb IsNot Nothing Then TextBoxTotalAlb.Text = (base + sumaIva).ToString("C2")
    End Sub

    Private Sub LimpiarFormulario()
        _numeroFacturaActual = ""
        _idsParaBorrar.Clear()
        TextBoxFactura.Text = GenerarProximoNumeroFactura()
        TextBoxIdCliente.Text = "" : TextBoxCliente.Text = ""
        TextBoxAlbaranOrigen.Text = "" : TextBoxAlbaranOrigen.Tag = Nothing
        TextBoxIdVendedor.Text = "" : TextBoxVendedor.Text = ""
        cboAgencias.SelectedValue = ""
        ' Limpiar Logística
        TextBoxBultos.Text = "1" : TextBoxPeso.Text = "0"
        TextBoxDireccion.Text = "" : TextBoxPoblacion.Text = "" : TextBoxCP.Text = ""
        TextBoxTracking.Text = GenerarCodigoSeguimiento()
        TextBoxFecha.Text = DateTime.Now.ToShortDateString()
        DateTimePickerFecha.Value = DateTime.Now.ToShortDateString()
        TextBoxBase.Text = "0,00" : TextBoxTotalAlb.Text = "0,00"
        cboEstado.Text = "Pendiente"
        CargarDesplegables()
        ConfigurarGrid()
        _dtLineas = New DataTable()
        ConfigurarEstructuraDatos()
        DataGridView1.DataSource = _dtLineas
        If DataGridView1.Columns.Contains("ID_Linea") Then DataGridView1.Columns("ID_Linea").Visible = False
    End Sub
    Private Function GenerarProximoNumeroFactura() As String
        ' Delegamos en NumeradorDocumentos para tener orden numérico (no lexicográfico).
        Return NumeradorDocumentos.SiguienteNumero("FAC-", "FacturasVenta", "NumeroFactura")
    End Function
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Using frm As New FrmBuscador()
            frm.TablaABuscar = "Facturas"
            If frm.ShowDialog() = DialogResult.OK Then
                CargarFactura(frm.Resultado)
            End If
        End Using
    End Sub
    Private Sub CalcularTotales()
        Dim base As Decimal = 0
        Dim ivaTot As Decimal = 0
        If _dtLineas IsNot Nothing Then
            For Each r As DataRow In _dtLineas.Rows
                If r.RowState <> DataRowState.Deleted Then
                    Dim c As Decimal = 0 : Decimal.TryParse(r("Cantidad").ToString(), c)
                    Dim p As Decimal = 0 : Decimal.TryParse(r("PrecioUnitario").ToString(), p)
                    Dim d As Decimal = 0 : Decimal.TryParse(r("Descuento").ToString(), d)
                    Dim iva As Decimal = 21
                    If _dtLineas.Columns.Contains("PorcentajeIVA") AndAlso Not IsDBNull(r("PorcentajeIVA")) Then
                        Decimal.TryParse(r("PorcentajeIVA").ToString(), iva)
                    End If
                    Dim baseLinea As Decimal = (c * p) * (1 - (d / 100))
                    base += baseLinea
                    ivaTot += baseLinea * (iva / 100) ' IVA real por línea, no 21% fijo
                End If
            Next
        End If
        TextBoxBase.Text = base.ToString("C2")
        TextBoxTotalAlb.Text = (base + ivaTot).ToString("C2")
    End Sub
    Private Sub DataGridView1_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellEndEdit
        Dim fila = DataGridView1.Rows(e.RowIndex)
        Dim colName = DataGridView1.Columns(e.ColumnIndex).Name

        ' 1. CUANDO EL USUARIO METE EL CÓDIGO DEL ARTÍCULO
        If colName = "ID_Articulo" Then
            Dim idArt As String = fila.Cells("ID_Articulo").Value?.ToString()
            Dim idAntiguo As String = fila.Cells("ID_Articulo").Tag?.ToString()

            ' ---> EL FRENO MÁGICO <---
            ' Si sales de la celda pero el código es EXACTAMENTE el mismo que había antes,
            ' abortamos la operación. Así no destruimos los datos congelados de la factura.
            If idArt = idAntiguo Then Return

            If String.IsNullOrWhiteSpace(idArt) Then
                fila.Cells("Descripcion").Value = ""
                fila.Cells("PrecioUnitario").Value = 0
                fila.Cells("PorcentajeIVA").Value = 21
                fila.Tag = Nothing
                Return
            End If

            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                Dim sql As String = "SELECT Descripcion, PrecioVenta, StockActual, TipoIVA FROM Articulos WHERE ID_Articulo = @id"

                Using cmd As New SQLiteCommand(sql, c)
                    cmd.Parameters.AddWithValue("@id", idArt)
                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            fila.Cells("Descripcion").Value = r("Descripcion").ToString()
                            fila.Cells("PrecioUnitario").Value = Convert.ToDecimal(r("PrecioVenta"))
                            fila.Cells("PorcentajeIVA").Value = If(IsDBNull(r("TipoIVA")), 21, Convert.ToDecimal(r("TipoIVA")))

                            If Not IsDBNull(r("StockActual")) Then fila.Tag = Convert.ToDecimal(r("StockActual"))
                            If IsDBNull(fila.Cells("Cantidad").Value) OrElse Val(fila.Cells("Cantidad").Value) = 0 Then fila.Cells("Cantidad").Value = 1

                            ' Actualizamos el bolsillo secreto con el nuevo ID válido
                            fila.Cells("ID_Articulo").Tag = idArt
                        Else
                            MessageBox.Show("Artículo no encontrado", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error al buscar artículo: " & ex.Message)
            End Try
        End If
        ' 2. RECALCULAR TOTALES AL CAMBIAR CANTIDAD, PRECIO, DTO O IVA
        If colName = "Cantidad" Or colName = "PrecioUnitario" Or colName = "Descuento" Or colName = "PorcentajeIVA" Then
            Dim cant As Decimal = 0, prec As Decimal = 0, dto As Decimal = 0, iva As Decimal = 21

            Decimal.TryParse(fila.Cells("Cantidad").Value?.ToString(), cant)
            Decimal.TryParse(fila.Cells("PrecioUnitario").Value?.ToString(), prec)
            Decimal.TryParse(fila.Cells("Descuento").Value?.ToString(), dto)
            Decimal.TryParse(fila.Cells("PorcentajeIVA").Value?.ToString(), iva)

            ' Matemáticas puras
            Dim precioConIva As Decimal = prec * (1 + (iva / 100))
            Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))
            Dim totalConIva As Decimal = totalSinIva * (1 + (iva / 100))

            ' Metemos los datos a la pantalla

            fila.Cells("Total").Value = totalSinIva
            fila.Cells("TotalConIVA").Value = totalConIva

            ' Avisamos al resumen de abajo para que también se actualice
            CalcularTotalesGenerales()
        End If
    End Sub

    Private Sub ButtonAnterior_Click(sender As Object, e As EventArgs) Handles ButtonAnterior.Click
        ' Lógica de navegación String (Ver Presupuestos)
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim numDestino As String = ""

            ' CAMBIO 7: Navegación alfanumérica (Funciona bien con formatos fijos tipo PRE-001)
            Dim sql As String = "SELECT MAX(NumeroFactura) FROM FacturasVenta WHERE NumeroFactura < @actual"
            Using cmd As New SQLiteCommand(sql, conexion)
                ' Si es nuevo, buscamos el último absoluto
                If String.IsNullOrEmpty(_numeroFacturaActual) Then
                    cmd.CommandText = "SELECT MAX(NumeroFactura) FROM FacturasVenta"
                Else
                    cmd.Parameters.AddWithValue("@actual", _numeroFacturaActual)
                End If

                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    numDestino = result.ToString()
                    CargarFactura(numDestino)
                Else
                    MessageBox.Show("Primer albaran.")
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub ButtonSiguiente_Click(sender As Object, e As EventArgs) Handles ButtonSiguiente.Click
        ' Lógica de navegación String (Ver Presupuesto)
        If String.IsNullOrEmpty(_numeroFacturaActual) Then Return
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim sql As String = "SELECT MIN(NumeroFactura) FROM FacturasVenta WHERE NumeroFactura > @actual"
            Using cmd As New SQLiteCommand(sql, conexion)
                cmd.Parameters.AddWithValue("@actual", _numeroFacturaActual)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    CargarFactura(result.ToString())
                Else
                    MessageBox.Show("Ultimo albaran")
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub ButtonNuevo_Click(sender As Object, e As EventArgs) Handles ButtonNuevoPed.Click
        'Simplemente llamamos al metodo de limpieza que ya se encarga de todo
        LimpiarFormulario()
    End Sub
    Private Sub ButtonBorrar_Click(sender As Object, e As EventArgs) Handles ButtonBorrar.Click
        ' Solo administradores pueden borrar facturas: una vez emitidas, una factura tiene
        ' implicaciones legales/fiscales y no debería poder eliminarla cualquier usuario.
        If Not ComunSesionActual.EsAdministrador() Then
            MessageBox.Show("Solo un Administrador puede eliminar facturas.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' 1. Seguridad: Si no hay factura cargada (es nueva), no hacemos nada
        If String.IsNullOrEmpty(_numeroFacturaActual) Then
            MessageBox.Show("No puedes borrar una factura que aún no se ha guardado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 2. Confirmación del usuario (es una acción destructiva, mejor preguntar)
        If MessageBox.Show($"¿Estás seguro de ELIMINAR la factura {_numeroFacturaActual} y todas sus líneas de forma permanente?", "Confirmar Borrado", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then
            Return
        End If

        Dim conexion = ConexionBD.GetConnection()
        Dim transaccion As SQLiteTransaction = Nothing

        Try
            If conexion.State <> ConnectionState.Open Then conexion.Open()

            ' Abrimos la transacción por si algo falla a medias
            transaccion = conexion.BeginTransaction()

            ' PASO A: Borrar las LÍNEAS (Tabla Hija)
            Dim sqlLineas As String = "DELETE FROM LineasFacturaVenta WHERE NumeroFactura = @num"
            Using cmd As New SQLiteCommand(sqlLineas, conexion)
                cmd.Transaction = transaccion
                cmd.Parameters.AddWithValue("@num", _numeroFacturaActual)
                cmd.ExecuteNonQuery()
            End Using

            ' PASO B: Borrar la CABECERA (Tabla Padre)
            Dim sqlCabecera As String = "DELETE FROM FacturasVenta WHERE NumeroFactura = @num"
            Using cmd As New SQLiteCommand(sqlCabecera, conexion)
                cmd.Transaction = transaccion
                cmd.Parameters.AddWithValue("@num", _numeroFacturaActual)
                cmd.ExecuteNonQuery()
            End Using

            ' Confirmamos que todo ha ido bien
            transaccion.Commit()

            MessageBox.Show("Factura eliminada correctamente.", "Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' 3. Reseteamos el formulario para dejarlo listo para una nueva factura
            LimpiarFormulario()

        Catch ex As Exception
            ' Si la base de datos se queja, deshacemos el borrado para no romper nada
            If transaccion IsNot Nothing Then transaccion.Rollback()
            MessageBox.Show("Error crítico al borrar: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub ButtonNuevaLinea_Click(sender As Object, e As EventArgs) Handles ButtonNuevaLinea.Click
        ' 1. Seguridad: Si la tabla de memoria no existe, la inicializamos
        If _dtLineas Is Nothing Then
            _dtLineas = New DataTable()
        End If

        ' Aseguramos que tenga las columnas creadas (evita el error "Column does not belong to table")
        ConfigurarEstructuraDatos()

        ' 2. Creamos la nueva fila
        Dim nuevaFila As DataRow = _dtLineas.NewRow()

        ' 3. Calculamos el número de orden visual (para que salga 1, 2, 3...)
        Dim orden As Integer = 0
        For Each row As DataRow In _dtLineas.Rows
            If row.RowState <> DataRowState.Deleted Then orden += 1
        Next
        nuevaFila("NumeroOrden") = orden + 1

        ' 4. Valores por defecto (Vitales para que no falle la matemática luego)
        nuevaFila("NumeroFactura") = _numeroFacturaActual ' Si es 0 no pasa nada, se arregla al guardar
        nuevaFila("ID_Linea") = DBNull.Value     ' ESTO ES LO MÁS IMPORTANTE (Indica que es NUEVA)
        nuevaFila("ID_Articulo") = 0
        nuevaFila("Descripcion") = ""
        nuevaFila("Cantidad") = 1
        nuevaFila("PrecioUnitario") = 0
        nuevaFila("Descuento") = 0
        nuevaFila("PorcentajeIVA") = 21
        nuevaFila("Total") = 0

        ' 5. Añadimos a la tabla (se muestra en el Grid automáticamente)
        _dtLineas.Rows.Add(nuevaFila)

        ' 6. Truco de UX: Ponemos el foco en la celda del Artículo para escribir rápido
        If DataGridView1.Rows.Count > 0 Then
            Dim ultimaFilaIndex As Integer = DataGridView1.Rows.Count - 1
            ' Asegúrate de que la columna se llame "ID_Articulo" en tu grid
            DataGridView1.CurrentCell = DataGridView1.Rows(ultimaFilaIndex).Cells("ID_Articulo")
            DataGridView1.BeginEdit(True)
        End If
    End Sub
    ' RECORDAR EL VALOR ANTES DE EDITAR (Para no sobreescribir facturas viejas)
    Private Sub DataGridView1_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles DataGridView1.CellBeginEdit
        Dim colName = DataGridView1.Columns(e.ColumnIndex).Name
        If colName = "ID_Articulo" Then
            Dim celda = DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex)
            ' Guardamos el ID que hay ahora mismo en el "bolsillo" (Tag) de la celda
            celda.Tag = celda.Value?.ToString()
        End If
    End Sub
    Private Sub TextBoxIdCliente_Leave(sender As Object, e As EventArgs) Handles TextBoxIdCliente.Leave
        ' Si el campo está vacío, limpiamos todo y salimos
        If String.IsNullOrWhiteSpace(TextBoxIdCliente.Text) Then
            TextBoxCliente.Text = ""
            TextBoxDireccion.Text = ""
            TextBoxPoblacion.Text = ""
            TextBoxCP.Text = ""
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Buscamos al cliente
            Dim cmd As New SQLiteCommand("SELECT NombreFiscal, Direccion, Poblacion, CodigoPostal FROM Clientes WHERE CodigoCliente=@id", c)
            cmd.Parameters.AddWithValue("@id", TextBoxIdCliente.Text.Trim())

            Dim r = cmd.ExecuteReader()

            If r.Read() Then
                ' ¡Cliente encontrado! Rellenamos los datos
                TextBoxCliente.Text = r("NombreFiscal").ToString()
                TextBoxDireccion.Text = r("Direccion").ToString()
                TextBoxPoblacion.Text = r("Poblacion").ToString()
                TextBoxCP.Text = r("CodigoPostal").ToString()
            Else
                ' El cliente NO existe
                Dim respuesta As DialogResult = MessageBox.Show(
                    "El código de cliente '" & TextBoxIdCliente.Text & "' no existe en la base de datos." & vbCrLf & vbCrLf &
                    "¿Deseas crear un nuevo cliente ahora?",
                    "Cliente no encontrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)

                If respuesta = DialogResult.Yes Then
                    Dim codigoFaltante As String = TextBoxIdCliente.Text.Trim()

                    TextBoxIdCliente.Text = ""
                    TextBoxCliente.Text = ""

                    ' Abrimos la ficha y le pasamos el código por la nueva puerta
                    Dim frm As New FrmClienteDetalle()
                    frm.CodigoNuevoPredefinido = codigoFaltante ' <--- AQUÍ LE PASAMOS EL DATO
                    frm.ShowDialog()

                    TextBoxIdCliente.Focus()
                Else
                    ' Si dice que no, le limpiamos la caja porque el código era inválido
                    TextBoxIdCliente.Text = ""
                    TextBoxCliente.Text = ""
                    TextBoxIdCliente.Focus()
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error al buscar cliente: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub TextBoxIdVendedor_Leave(sender As Object, e As EventArgs) Handles TextBoxIdVendedor.Leave
        ' Tu lógica de buscar vendedor
        If String.IsNullOrWhiteSpace(TextBoxIdVendedor.Text) Then TextBoxVendedor.Text = "" : Return
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim cmd As New SQLiteCommand("SELECT Nombre FROM Vendedores WHERE ID_Vendedor=@id", c)
            cmd.Parameters.AddWithValue("@id", Val(TextBoxIdVendedor.Text))
            Dim r = cmd.ExecuteScalar()
            TextBoxVendedor.Text = If(r IsNot Nothing, r.ToString(), "NO EXISTE")
        Catch
        End Try
    End Sub
    Private Sub btnImportarAlbaran_Click_1(sender As Object, e As EventArgs) Handles btnImportarAlbaran.Click
        Using frm As New FrmBuscador
            frm.TablaABuscar = "Albaranes"
            If frm.ShowDialog = DialogResult.OK Then
                Dim codAlba = frm.Resultado ' Ahora recibimos String
                If Not String.IsNullOrEmpty(codAlba) Then ImportarDatosAlbaran(codAlba)
            End If
        End Using

    End Sub
    Private Sub ImportarDatosAlbaran(CodigoAlbaran As String)
        Dim c = ConexionBD.GetConnection()
        Try
            If c.State <> ConnectionState.Open Then c.Open()

            ' 1. Cargar la cabecera del Albarán (NUEVO: Añadido C.CIF para que la factura sea legal)
            Dim sqlCab As String = "SELECT A.*, C.NombreFiscal, C.CIF, V.Nombre AS NombreVend FROM Albaranes A " &
                               "LEFT JOIN Clientes C ON A.CodigoCliente=C.CodigoCliente " &
                               "LEFT JOIN Vendedores V ON A.ID_Vendedor=V.ID_Vendedor " &
                               "WHERE A.NumeroAlbaran= @num"

            Using cmd As New SQLiteCommand(sqlCab, c)
                cmd.Parameters.AddWithValue("@num", CodigoAlbaran)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        LimpiarFormulario()

                        ' --- DATOS BÁSICOS Y CLIENTE ---
                        TextBoxIdCliente.Text = r("CodigoCliente").ToString()
                        TextBoxCliente.Text = r("NombreFiscal").ToString()
                        TextBoxIdVendedor.Text = If(IsDBNull(r("ID_Vendedor")), "", r("ID_Vendedor").ToString())
                        TextBoxVendedor.Text = If(IsDBNull(r("NombreVend")), "", r("NombreVend").ToString())

                        ' NUEVO: El CIF es obligatorio en las facturas

                        If Not IsDBNull(r("FechaEntrega")) AndAlso DateTimePickerFecha IsNot Nothing Then
                            DateTimePickerFecha.Value = Convert.ToDateTime(r("FechaEntrega"))
                        End If

                        TextBoxObservaciones.Text = $"Generado desde Albarán {CodigoAlbaran}."
                        If TextBoxAlbaranOrigen IsNot Nothing Then
                            TextBoxAlbaranOrigen.Text = CodigoAlbaran
                            TextBoxAlbaranOrigen.Tag = CodigoAlbaran
                        End If

                        ' --- NUEVO: DIRECCIÓN Y ENVÍO ---
                        If TextBoxDireccion IsNot Nothing Then TextBoxDireccion.Text = If(IsDBNull(r("DireccionEnvio")), "", r("DireccionEnvio").ToString())
                        If TextBoxPoblacion IsNot Nothing Then TextBoxPoblacion.Text = If(IsDBNull(r("Poblacion")), "", r("Poblacion").ToString())
                        If TextBoxCP IsNot Nothing Then TextBoxCP.Text = If(IsDBNull(r("CodigoPostal")), "", r("CodigoPostal").ToString())

                        ' --- FINANCIERO: FORMA DE PAGO ---
                        Dim idFPago = r("ID_FormaPago")
                        If Not IsDBNull(idFPago) AndAlso idFPago IsNot Nothing AndAlso idFPago.ToString() <> "" Then
                            cboFormaPago.SelectedValue = Convert.ToInt32(idFPago)
                        Else
                            cboFormaPago.SelectedIndex = -1
                        End If

                        ' --- NUEVO: LOGÍSTICA (Bultos, Peso, Agencia, Tracking...) ---
                        Dim idAgencia = r("ID_Agencia")
                        If cboAgencias IsNot Nothing Then
                            If Not IsDBNull(idAgencia) AndAlso idAgencia IsNot Nothing AndAlso idAgencia.ToString() <> "" Then
                                cboAgencias.SelectedValue = Convert.ToInt32(idAgencia)
                            Else
                                cboAgencias.SelectedIndex = -1
                            End If
                        End If

                        ' --- RUTA ASIGNADA (heredada del albaran) ---
                        Dim idRuta = r("ID_Ruta")
                        If cboRuta IsNot Nothing Then
                            If Not IsDBNull(idRuta) AndAlso idRuta IsNot Nothing AndAlso idRuta.ToString() <> "" Then
                                Try
                                    cboRuta.SelectedValue = Convert.ToInt32(idRuta)
                                    ' Si el albaran no tenia agencia, derivar de la ruta
                                    If cboAgencias.SelectedIndex = -1 Then AutoAsignarAgenciaDesdeRuta()
                                Catch
                                    cboRuta.SelectedIndex = -1
                                End Try
                            ElseIf cboAgencias.SelectedIndex = -1 Then
                                cboRuta.SelectedIndex = -1
                            End If
                        End If

                        If TextBoxBultos IsNot Nothing Then TextBoxBultos.Text = If(IsDBNull(r("NumeroBultos")), "1", r("NumeroBultos").ToString())
                        If TextBoxPeso IsNot Nothing Then TextBoxPeso.Text = If(IsDBNull(r("PesoTotal")), "0", r("PesoTotal").ToString())
                        If TextBoxTracking IsNot Nothing Then TextBoxTracking.Text = If(IsDBNull(r("CodigoSeguimiento")), "", r("CodigoSeguimiento").ToString())
                        If ComboBoxPortes IsNot Nothing Then ComboBoxPortes.Text = If(IsDBNull(r("Portes")), "Pagados", r("Portes").ToString())

                    Else
                        MessageBox.Show("Albarán no encontrado en la base de datos.")
                        Return
                    End If
                End Using
            End Using

            ' 2. Descargar las líneas del Albarán
            Dim sqlLin As String = "SELECT * FROM LineasAlbaran WHERE NumeroAlbaran = @num ORDER BY NumeroOrden ASC"
            Dim dtOrigen As New DataTable()
            Using cmd As New SQLiteCommand(sqlLin, c)
                cmd.Parameters.AddWithValue("@num", CodigoAlbaran)
                Dim da As New SQLiteDataAdapter(cmd)
                da.Fill(dtOrigen)
            End Using

            ' Limpiamos la tabla de la pantalla
            _dtLineas.Rows.Clear()

            ' 3. Bucle traductor (De Albarán a Factura)
            For Each rowOrig As DataRow In dtOrigen.Rows
                Dim rowNew As DataRow = _dtLineas.NewRow()

                rowNew("NumeroFactura") = _numeroFacturaActual
                rowNew("ID_Linea") = DBNull.Value
                rowNew("NumeroOrden") = rowOrig("NumeroOrden")
                rowNew("ID_Articulo") = rowOrig("ID_Articulo")
                rowNew("Descripcion") = rowOrig("Descripcion")

                ' Leer unidades (CantidadServida o Cantidad)
                Dim ca As Decimal = 0
                If dtOrigen.Columns.Contains("CantidadServida") Then
                    Decimal.TryParse(rowOrig("CantidadServida").ToString(), ca)
                ElseIf dtOrigen.Columns.Contains("Cantidad") Then
                    Decimal.TryParse(rowOrig("Cantidad").ToString(), ca)
                End If

                Dim pr As Decimal = 0 : Decimal.TryParse(rowOrig("PrecioUnitario").ToString(), pr)
                Dim dt As Decimal = 0 : Decimal.TryParse(rowOrig("Descuento").ToString(), dt)

                Dim iva As Decimal = 21
                If dtOrigen.Columns.Contains("PorcentajeIVA") AndAlso Not IsDBNull(rowOrig("PorcentajeIVA")) Then
                    Decimal.TryParse(rowOrig("PorcentajeIVA").ToString(), iva)
                End If

                ' Guardamos en las columnas de la Factura
                rowNew("Cantidad") = ca
                rowNew("PrecioUnitario") = pr
                rowNew("Descuento") = dt
                rowNew("PorcentajeIVA") = iva

                ' Columnas visuales
                rowNew("PrecioConIVA") = pr * (1 + (iva / 100))
                Dim totalSinIva As Decimal = (ca * pr) * (1 - (dt / 100))
                rowNew("Total") = totalSinIva
                rowNew("TotalConIVA") = totalSinIva * (1 + (iva / 100))

                _dtLineas.Rows.Add(rowNew)
            Next

            ' Calculamos los totales finales de la pantalla
            CalcularTotalesGenerales()
            MessageBox.Show("Albarán y datos logísticos importados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error al importar: " & ex.Message)
        End Try
    End Sub
    Private Sub EstilizarFecha(dtp As DateTimePicker)
        dtp.Format = DateTimePickerFormat.Custom : dtp.CustomFormat = "dd/MM/yyyy"
        dtp.Font = New Font("Segoe UI", 10) : dtp.MinimumSize = New Size(0, 25)
    End Sub
    ' --- PALETA DE COLORES CENTRALIZADA (rediseño modernizado) ---
    Private Shared ReadOnly COLOR_FONDO As Color = Color.FromArgb(70, 75, 80)
    Private Shared ReadOnly COLOR_BANDA As Color = Color.FromArgb(40, 50, 70)
    Private Shared ReadOnly COLOR_PANEL_TOTALES As Color = Color.FromArgb(25, 30, 40)
    Private Shared ReadOnly COLOR_ACENTO As Color = Color.FromArgb(0, 150, 255)
    Private Shared ReadOnly COLOR_TEXTO_SECUNDARIO As Color = Color.FromArgb(170, 180, 195)
    Private Shared ReadOnly COLOR_LINEA_DIVISORIA As Color = Color.FromArgb(120, 130, 140)
    Private Shared ReadOnly COLOR_SEPARADOR_GRUPO As Color = Color.FromArgb(95, 105, 120)

    Private Shared ReadOnly BTN_AZUL_PRIMARIO As Color = Color.FromArgb(0, 120, 215)
    Private Shared ReadOnly BTN_ROJO_PELIGRO As Color = Color.FromArgb(209, 52, 56)
    Private Shared ReadOnly BTN_VERDE_AÑADIR As Color = Color.FromArgb(40, 140, 90)
    Private Shared ReadOnly BTN_GRIS_NEUTRO As Color = Color.FromArgb(85, 85, 85)

    ' --- ESTADO DEL PANEL DESPLEGABLE DE LOGÍSTICA ---
    Private _logisticaVisible As Boolean = False
    Private WithEvents btnToggleLogistica As New Button()

    Private Sub ReorganizarControlesAutomaticamente()
        If Me.ClientSize.Width < 100 Then Return

        For Each ctrl As Control In Me.Controls
            ctrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        Next

        Dim margenIzq As Integer = 30
        Dim anchoForm As Integer = Me.ClientSize.Width
        Dim altoForm As Integer = Me.ClientSize.Height
        Dim anchoUtil As Integer = anchoForm - (margenIzq * 2)

        ' 1. BANDA SUPERIOR CON TÍTULO Y NÚMERO DE DOCUMENTO
        Dim alturaBanda As Integer = 60

        Dim bandaSuperior As Panel = Me.Controls.OfType(Of Panel)().FirstOrDefault(Function(p) p.Name = "BandaSuperior")
        If bandaSuperior Is Nothing Then
            bandaSuperior = New Panel() With {.Name = "BandaSuperior", .BackColor = COLOR_BANDA}
            Me.Controls.Add(bandaSuperior)
            bandaSuperior.SendToBack()
        End If
        bandaSuperior.Bounds = New Rectangle(0, 0, anchoForm, alturaBanda)
        bandaSuperior.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        Dim lblTitulo As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LblTituloDoc")
        If lblTitulo Is Nothing Then
            lblTitulo = New Label() With {.Name = "LblTituloDoc", .AutoSize = False, .BackColor = COLOR_BANDA,
                                          .ForeColor = Color.White, .TextAlign = ContentAlignment.MiddleLeft,
                                          .Font = New Font("Segoe UI", 18, FontStyle.Bold)}
            Me.Controls.Add(lblTitulo)
        End If
        lblTitulo.Text = "FACTURA DE VENTA"
        lblTitulo.Bounds = New Rectangle(margenIzq, 0, 400, alturaBanda)
        lblTitulo.BringToFront()

        Dim lblNumDocEtiqueta As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LblNumDocEtiqueta")
        If lblNumDocEtiqueta Is Nothing Then
            lblNumDocEtiqueta = New Label() With {.Name = "LblNumDocEtiqueta", .AutoSize = False, .BackColor = COLOR_BANDA,
                                                  .ForeColor = COLOR_TEXTO_SECUNDARIO, .TextAlign = ContentAlignment.MiddleRight,
                                                  .Font = New Font("Segoe UI", 9.5F, FontStyle.Regular), .Text = "Nº Documento"}
            Me.Controls.Add(lblNumDocEtiqueta)
        End If
        lblNumDocEtiqueta.Bounds = New Rectangle(margenIzq + 410, 8, 110, 20)
        lblNumDocEtiqueta.BringToFront()

        ' Nº de factura destacado
        If TextBoxFactura IsNot Nothing Then
            TextBoxFactura.Bounds = New Rectangle(margenIzq + 410, 28, 130, 28)
            TextBoxFactura.BackColor = COLOR_PANEL_TOTALES
            TextBoxFactura.ForeColor = COLOR_ACENTO
            TextBoxFactura.Font = New Font("Segoe UI", 11.5F, FontStyle.Bold)
            TextBoxFactura.BorderStyle = BorderStyle.FixedSingle
            TextBoxFactura.TextAlign = HorizontalAlignment.Center
            TextBoxFactura.BringToFront()
        End If

        If Button1 IsNot Nothing Then
            Button1.Bounds = New Rectangle(margenIzq + 545, 28, 30, 28)
            Button1.BackColor = BTN_AZUL_PRIMARIO
            Button1.ForeColor = Color.White
            Button1.FlatStyle = FlatStyle.Flat
            Button1.FlatAppearance.BorderSize = 0
            Button1.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
            Button1.Cursor = Cursors.Hand
            Button1.Text = "🔍"
            Button1.BringToFront()
        End If

        ' Ocultar etiquetas sueltas (las reemplaza el título)
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Label AndAlso ctrl.Parent Is Me _
               AndAlso ctrl.Name <> "LblTituloDoc" AndAlso ctrl.Name <> "LblNumDocEtiqueta" _
               AndAlso ctrl.Name <> "LineaTotales" AndAlso ctrl.Name <> "PanelTotalesResumen" _
               AndAlso ctrl.Name <> "LineaDivisoria" Then
                Dim texto As String = ctrl.Text.Trim().ToLower()
                If texto = "factura" Or texto = "documento" Or texto = "pedido" Or texto = "albaran" Or texto = "albarán" Then
                    ctrl.Visible = False
                End If
            End If
        Next

        ' 2. ZONA DE FORMULARIO — IDÉNTICA AL ALBARÁN/PEDIDO/FACTURA DE COMPRA
        Dim yInicioFormulario As Integer = alturaBanda + 25

        Dim col1_X As Integer = margenIzq                  ' Cliente / Vendedor
        Dim col2_X As Integer = 600                        ' Fecha / Vencimiento / Estado / Observaciones
        Dim col3_X As Integer = col2_X + 460               ' Agencia (justo después de Observaciones)

        Dim yFila1 As Integer = yInicioFormulario
        Dim yFila2 As Integer = yInicioFormulario + 22
        Dim yFila3 As Integer = yInicioFormulario + 60
        Dim yFila4 As Integer = yInicioFormulario + 82
        Dim yFila5 As Integer = yInicioFormulario + 120
        Dim yFila6 As Integer = yInicioFormulario + 142

        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Label AndAlso ctrl.Parent Is Me _
               AndAlso ctrl.Name <> "LblTituloDoc" AndAlso ctrl.Name <> "LblNumDocEtiqueta" _
               AndAlso ctrl.Name <> "LineaTotales" AndAlso ctrl.Name <> "PanelTotalesResumen" _
               AndAlso ctrl.Name <> "LineaDivisoria" Then
                ctrl.BackColor = Color.Transparent
                ctrl.ForeColor = Color.WhiteSmoke
                ctrl.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
                ctrl.BringToFront()
                Select Case ctrl.Text.Trim().ToLower()
                    Case "cliente" : ctrl.Location = New Point(col1_X, yFila1)
                    Case "fecha" : ctrl.Location = New Point(col2_X, yFila1)
                    Case "fecha entrega", "vencimiento"
                        ctrl.Text = "Vencimiento"
                        ctrl.Location = New Point(col2_X + 160, yFila1)
                    Case "vendedor" : ctrl.Location = New Point(col1_X, yFila3)
                    Case "estado" : ctrl.Location = New Point(col2_X, yFila3)
                    Case "agencia" : ctrl.Location = New Point(col3_X, yFila1)
                End Select
            End If
        Next

        ' Eliminar duplicados de Observaciones en el header si existieran de versiones anteriores
        For Each ctlName In {"LblObservacionesFac", "TextBoxObsFacPrincipal"}
            Dim found = Me.Controls.Find(ctlName, False)
            If found IsNot Nothing AndAlso found.Length > 0 Then
                For Each c In found
                    Me.Controls.Remove(c)
                    c.Dispose()
                Next
            End If
        Next

        ' --- Bloque 1: Cliente y Vendedor ---
        If TextBoxIdCliente IsNot Nothing Then TextBoxIdCliente.Bounds = New Rectangle(col1_X, yFila2, 60, 25)
        If TextBoxCliente IsNot Nothing Then TextBoxCliente.Bounds = New Rectangle(col1_X + 65, yFila2, 460, 25)
        If TextBoxIdVendedor IsNot Nothing Then TextBoxIdVendedor.Bounds = New Rectangle(col1_X, yFila4, 50, 25)
        If TextBoxVendedor IsNot Nothing Then TextBoxVendedor.Bounds = New Rectangle(col1_X + 55, yFila4, 470, 25)

        ' --- Bloque 2: Fechas y Estado ---
        If TextBoxFecha IsNot Nothing Then TextBoxFecha.Bounds = New Rectangle(col2_X, yFila2, 140, 25)
        If DateTimePickerFecha IsNot Nothing Then
            DateTimePickerFecha.Bounds = New Rectangle(col2_X + 160, yFila2, 140, 25)
            DateTimePickerFecha.BringToFront()
        End If
        If cboEstado IsNot Nothing Then cboEstado.Bounds = New Rectangle(col2_X, yFila4, 140, 25)

        ' --- Bloque 3: Agencia ---
        If cboAgencias IsNot Nothing Then cboAgencias.Bounds = New Rectangle(col3_X, yFila2, 280, 25)

        ' --- Tercera fila: Forma de pago + Ruta ---
        If lblFormaPago IsNot Nothing Then
            lblFormaPago.Location = New Point(col1_X, yFila5)
            lblFormaPago.BackColor = Color.Transparent
            lblFormaPago.ForeColor = Color.WhiteSmoke
            lblFormaPago.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
        End If
        If cboFormaPago IsNot Nothing Then
            cboFormaPago.Bounds = New Rectangle(col1_X, yFila6, 220, 25)
            cboFormaPago.Font = New Font("Segoe UI", 10.0F)
        End If

        If lblRuta IsNot Nothing Then
            lblRuta.Location = New Point(col1_X + 240, yFila5)
            lblRuta.BackColor = Color.Transparent
            lblRuta.ForeColor = Color.WhiteSmoke
            lblRuta.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
        End If
        If cboRuta IsNot Nothing Then
            cboRuta.Bounds = New Rectangle(col1_X + 240, yFila6, 400, 25)
            cboRuta.Font = New Font("Segoe UI", 10.0F)
        End If

        ' 3. BOTÓN TOGGLE PARA MOSTRAR/OCULTAR LOGÍSTICA
        Dim yToggle As Integer = yFila6 + 35

        If btnToggleLogistica.Parent Is Nothing Then
            btnToggleLogistica.FlatStyle = FlatStyle.Flat
            btnToggleLogistica.FlatAppearance.BorderSize = 0
            btnToggleLogistica.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            btnToggleLogistica.Cursor = Cursors.Hand
            btnToggleLogistica.BackColor = COLOR_BANDA
            btnToggleLogistica.ForeColor = COLOR_ACENTO
            btnToggleLogistica.TextAlign = ContentAlignment.MiddleLeft
            btnToggleLogistica.Padding = New Padding(15, 0, 0, 0)
            Me.Controls.Add(btnToggleLogistica)
        End If
        btnToggleLogistica.Bounds = New Rectangle(margenIzq, yToggle, anchoUtil, 32)
        btnToggleLogistica.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnToggleLogistica.Text = If(_logisticaVisible, "▼  Ocultar logística y envío", "▶  Mostrar logística y envío")
        btnToggleLogistica.BringToFront()

        ' 4. PANEL LOGÍSTICA (TabControl) — desplegable
        Dim yTabLogistica As Integer = yToggle + 38
        Dim altoTabLogistica As Integer = If(_logisticaVisible, 175, 0)

        If TabControlModerno2 IsNot Nothing Then
            TabControlModerno2.Visible = _logisticaVisible
            If _logisticaVisible Then
                TabControlModerno2.ItemSize = New Size(180, 30)
                TabControlModerno2.Bounds = New Rectangle(margenIzq, yTabLogistica, anchoUtil, altoTabLogistica)
                TabControlModerno2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
                TabControlModerno2.BringToFront()

                For Each tab As TabPage In TabControlModerno2.TabPages
                    tab.BackColor = Me.BackColor
                Next

                Dim MoverInterno = Sub(nombre As String, x As Integer, y As Integer, w As Integer, h As Integer)
                                       Dim encontrados = TabControlModerno2.Controls.Find(nombre, True)
                                       If encontrados.Length > 0 Then
                                           encontrados(0).Bounds = New Rectangle(x, y, w, h)
                                           If TypeOf encontrados(0) Is Label Then
                                               encontrados(0).ForeColor = Color.WhiteSmoke
                                               encontrados(0).BackColor = Color.Transparent
                                               encontrados(0).Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
                                           End If
                                       End If
                                   End Sub

                ' Pestaña "Logística y Envío"
                MoverInterno("Label25", 15, 12, 80, 18) : MoverInterno("TextBoxDireccion", 15, 33, 360, 25)
                MoverInterno("Label27", 390, 12, 80, 18) : MoverInterno("TextBoxPoblacion", 390, 33, 240, 25)
                MoverInterno("Label26", 645, 12, 100, 18) : MoverInterno("TextBoxCP", 645, 33, 110, 25)

                MoverInterno("Label31", 15, 75, 200, 18) : MoverInterno("TextBoxTracking", 15, 96, 360, 25)
                MoverInterno("Label28", 390, 75, 80, 18) : MoverInterno("ComboBoxPortes", 390, 96, 130, 25)
                MoverInterno("Label30", 535, 75, 60, 18) : MoverInterno("TextBoxBultos", 535, 96, 90, 25)
                MoverInterno("Label29", 645, 75, 60, 18) : MoverInterno("TextBoxPeso", 645, 96, 110, 25)

                ' Pestaña "Detalles y Origen"
                MoverInterno("Label20", 15, 12, 100, 18) : MoverInterno("TextBoxAlbaranOrigen", 15, 33, 130, 25)
                MoverInterno("btnImportarAlbaran", 150, 33, 30, 25)
                MoverInterno("Label21", 200, 12, 150, 18) : MoverInterno("TextBoxObservaciones", 200, 33, 555, 100)

                ' Estilizar el botón importar como lupa azul
                Dim btnImp = TabControlModerno2.Controls.Find("btnImportarAlbaran", True)
                If btnImp.Length > 0 AndAlso TypeOf btnImp(0) Is Button Then
                    Dim b As Button = CType(btnImp(0), Button)
                    b.BackColor = BTN_AZUL_PRIMARIO
                    b.ForeColor = Color.White
                    b.FlatStyle = FlatStyle.Flat
                    b.FlatAppearance.BorderSize = 0
                    b.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
                    b.Cursor = Cursors.Hand
                    b.Text = "🔍"
                End If

                Dim ocultarDuplicados = Sub(nombre As String)
                                            Dim encontrados = TabControlModerno2.Controls.Find(nombre, True)
                                            If encontrados.Length > 0 Then encontrados(0).Visible = False
                                        End Sub
                ocultarDuplicados("TextBox5") : ocultarDuplicados("Button4")
            End If
        End If

        ' 5. LÍNEA DIVISORIA + GRID
        Dim lineaDivisoria As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LineaDivisoria")
        If lineaDivisoria Is Nothing Then
            lineaDivisoria = New Label() With {.Name = "LineaDivisoria", .BackColor = COLOR_LINEA_DIVISORIA, .Height = 2}
            Me.Controls.Add(lineaDivisoria)
        End If

        Dim yTabla As Integer = yToggle + 38 + altoTabLogistica + If(_logisticaVisible, 20, 5)
        lineaDivisoria.Bounds = New Rectangle(margenIzq, yTabla - 12, anchoUtil, 2)
        lineaDivisoria.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lineaDivisoria.BringToFront()

        DataGridView1.Parent = Me
        DataGridView1.MaximumSize = New Size(0, 0)
        DataGridView1.Dock = DockStyle.None
        DataGridView1.Bounds = New Rectangle(margenIzq, yTabla, anchoUtil, altoForm - yTabla - 150)
        DataGridView1.BackgroundColor = Me.BackColor
        DataGridView1.BorderStyle = BorderStyle.None
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        If DataGridView1.Columns.Contains("Descripcion") Then DataGridView1.Columns("Descripcion").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

        ' 6. PANEL LATERAL DERECHO DE TOTALES
        Dim xDerecha As Integer = anchoForm - margenIzq
        Dim yTotales As Integer = DataGridView1.Bottom + 12

        Dim panelTotales As Panel = Me.Controls.OfType(Of Panel)().FirstOrDefault(Function(p) p.Name = "PanelTotales")
        If panelTotales Is Nothing Then
            panelTotales = New Panel() With {.Name = "PanelTotales", .BackColor = COLOR_PANEL_TOTALES,
                                              .BorderStyle = BorderStyle.FixedSingle}
            Me.Controls.Add(panelTotales)
            panelTotales.SendToBack()
        End If
        Dim panelAncho As Integer = 340
        Dim panelAlto As Integer = 120
        panelTotales.Bounds = New Rectangle(xDerecha - panelAncho, yTotales, panelAncho, panelAlto)
        panelTotales.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        Dim viejoPanel As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "PanelTotalesResumen")
        If viejoPanel IsNot Nothing Then Me.Controls.Remove(viejoPanel)

        Dim xPanelInt As Integer = xDerecha - panelAncho + 15
        Dim wPanelInt As Integer = panelAncho - 30

        ' Base
        If LabelBase IsNot Nothing Then
            LabelBase.BackColor = COLOR_PANEL_TOTALES
            LabelBase.ForeColor = COLOR_TEXTO_SECUNDARIO
            LabelBase.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
            LabelBase.TextAlign = ContentAlignment.MiddleLeft
            LabelBase.Bounds = New Rectangle(xPanelInt, yTotales + 12, 150, 22)
            LabelBase.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If
        If TextBoxBase IsNot Nothing Then
            TextBoxBase.BackColor = COLOR_PANEL_TOTALES
            TextBoxBase.ForeColor = Color.White
            TextBoxBase.Font = New Font("Segoe UI Semibold", 10.5F, FontStyle.Bold)
            TextBoxBase.BorderStyle = BorderStyle.None
            TextBoxBase.TextAlign = HorizontalAlignment.Right
            TextBoxBase.Bounds = New Rectangle(xPanelInt + wPanelInt - 140, yTotales + 12, 140, 22)
            TextBoxBase.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If

        ' IVA
        If LabelIva IsNot Nothing Then
            LabelIva.BackColor = COLOR_PANEL_TOTALES
            LabelIva.ForeColor = COLOR_TEXTO_SECUNDARIO
            LabelIva.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
            LabelIva.TextAlign = ContentAlignment.MiddleLeft
            LabelIva.Bounds = New Rectangle(xPanelInt, yTotales + 38, 150, 22)
            LabelIva.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If
        If TextBoxIva IsNot Nothing Then
            TextBoxIva.BackColor = COLOR_PANEL_TOTALES
            TextBoxIva.ForeColor = Color.White
            TextBoxIva.Font = New Font("Segoe UI Semibold", 10.5F, FontStyle.Bold)
            TextBoxIva.BorderStyle = BorderStyle.None
            TextBoxIva.TextAlign = HorizontalAlignment.Right
            TextBoxIva.Bounds = New Rectangle(xPanelInt + wPanelInt - 140, yTotales + 38, 140, 22)
            TextBoxIva.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If

        Dim lineaTotal As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LineaTotales")
        If lineaTotal Is Nothing Then
            lineaTotal = New Label() With {.Name = "LineaTotales", .BackColor = Color.FromArgb(80, 90, 105), .Height = 1}
            Me.Controls.Add(lineaTotal)
        End If
        lineaTotal.Bounds = New Rectangle(xPanelInt, yTotales + 68, wPanelInt, 1)
        lineaTotal.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        lineaTotal.BringToFront()

        ' TOTAL destacado
        If Label7 IsNot Nothing Then
            Label7.Text = "TOTAL"
            Label7.BackColor = COLOR_PANEL_TOTALES
            Label7.ForeColor = COLOR_ACENTO
            Label7.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
            Label7.TextAlign = ContentAlignment.MiddleLeft
            Label7.Bounds = New Rectangle(xPanelInt, yTotales + 78, 150, 32)
            Label7.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If
        If TextBoxTotalAlb IsNot Nothing Then
            TextBoxTotalAlb.BackColor = COLOR_PANEL_TOTALES
            TextBoxTotalAlb.ForeColor = COLOR_ACENTO
            TextBoxTotalAlb.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold)
            TextBoxTotalAlb.BorderStyle = BorderStyle.None
            TextBoxTotalAlb.TextAlign = HorizontalAlignment.Right
            TextBoxTotalAlb.Bounds = New Rectangle(xPanelInt + wPanelInt - 180, yTotales + 78, 180, 32)
            TextBoxTotalAlb.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If

        ' 7. BARRA INFERIOR DE BOTONES AGRUPADOS
        Dim yBotones As Integer = DataGridView1.Bottom + 50

        EstilizarBoton(ButtonGuardar, margenIzq, yBotones, BTN_AZUL_PRIMARIO, Color.White)
        EstilizarBoton(ButtonBorrar, margenIzq + 110, yBotones, BTN_ROJO_PELIGRO, Color.White)
        If ButtonNuevoPed IsNot Nothing Then EstilizarBoton(ButtonNuevoPed, margenIzq + 220, yBotones, BTN_AZUL_PRIMARIO, Color.White)

        Dim sep1 As Label = ObtenerOCrearSeparador("SepGrupo1")
        sep1.Bounds = New Rectangle(margenIzq + 332, yBotones + 4, 1, 27)
        sep1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left

        If ButtonNuevaLinea IsNot Nothing Then
            EstilizarBoton(ButtonNuevaLinea, margenIzq + 348, yBotones, BTN_VERDE_AÑADIR, Color.White)
            ButtonNuevaLinea.Text = "+ Añadir Línea" : ButtonNuevaLinea.Width = 120
        End If
        If ButtonBorrarLineas IsNot Nothing Then
            EstilizarBoton(ButtonBorrarLineas, margenIzq + 478, yBotones, BTN_GRIS_NEUTRO, Color.White)
            ButtonBorrarLineas.Text = "− Quitar Línea" : ButtonBorrarLineas.Width = 120
        End If

        Dim sep2 As Label = ObtenerOCrearSeparador("SepGrupo2")
        sep2.Bounds = New Rectangle(margenIzq + 610, yBotones + 4, 1, 27)
        sep2.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left

        EstilizarBoton(ButtonAnterior, margenIzq + 626, yBotones, COLOR_FONDO, Color.White)
        ButtonAnterior.FlatAppearance.BorderColor = COLOR_TEXTO_SECUNDARIO
        ButtonAnterior.FlatAppearance.BorderSize = 1
        ButtonAnterior.Text = "◀ Anterior"

        EstilizarBoton(ButtonSiguiente, margenIzq + 736, yBotones, COLOR_FONDO, Color.White)
        ButtonSiguiente.FlatAppearance.BorderColor = COLOR_TEXTO_SECUNDARIO
        ButtonSiguiente.FlatAppearance.BorderSize = 1
        ButtonSiguiente.Text = "Siguiente ▶"

        ' Botón Exportar PDF
        If btnImprimir.Parent Is Nothing Then
            btnImprimir.Text = "Exportar PDF"
            btnImprimir.Size = New Size(120, 35)
            btnImprimir.BackColor = BTN_VERDE_AÑADIR
            btnImprimir.ForeColor = Color.White
            btnImprimir.FlatStyle = FlatStyle.Flat
            btnImprimir.FlatAppearance.BorderSize = 0
            btnImprimir.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            btnImprimir.Cursor = Cursors.Hand
            Me.Controls.Add(btnImprimir)
        End If
        btnImprimir.Location = New Point(margenIzq, yBotones + 45)
        btnImprimir.BringToFront()

        Dim lblStock As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LabelStock")
        If lblStock IsNot Nothing Then
            lblStock.Location = New Point(margenIzq + 160, DataGridView1.Bottom + 14)
            lblStock.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        End If

        Dim anclajeAbajoIzq As Control() = {ButtonGuardar, ButtonBorrar, ButtonNuevoPed, ButtonNuevaLinea, ButtonBorrarLineas, ButtonAnterior, ButtonSiguiente, btnImprimir}
        For Each c In anclajeAbajoIzq
            If c IsNot Nothing Then c.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        Next
    End Sub

    Private Sub btnToggleLogistica_Click(sender As Object, e As EventArgs) Handles btnToggleLogistica.Click
        _logisticaVisible = Not _logisticaVisible
        ReorganizarControlesAutomaticamente()
    End Sub

    Private Function ObtenerOCrearSeparador(nombre As String) As Label
        Dim sep As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = nombre)
        If sep Is Nothing Then
            sep = New Label() With {.Name = nombre, .BackColor = COLOR_SEPARADOR_GRUPO, .AutoSize = False}
            Me.Controls.Add(sep)
        End If
        sep.BringToFront()
        Return sep
    End Function

    Private Sub EstilizarBoton(btn As System.Windows.Forms.Button, x As Integer, y As Integer, bg As Color, fg As Color)
        If btn IsNot Nothing Then
            btn.Location = New Point(x, y)
            btn.BackColor = bg : btn.ForeColor = fg
            btn.FlatStyle = FlatStyle.Flat : btn.FlatAppearance.BorderSize = 0
            btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            btn.Cursor = Cursors.Hand : btn.Height = 35 : btn.Width = 100
        End If
    End Sub

    ' MOTOR DE EXPORTACIÓN A PDF (FACTURAS)
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        ' OJO: Cambia TextBoxFactura por el nombre real de tu TextBox del ID de la factura
        If String.IsNullOrWhiteSpace(TextBoxFactura.Text) Then
            MessageBox.Show("No hay ningún documento cargado para imprimir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        docImprimir.DefaultPageSettings.Landscape = False
        docImprimir.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docImprimir.DocumentName = "Factura_" & TextBoxFactura.Text

        Dim ppd As New PrintPreviewDialog()
        ppd.Document = docImprimir
        ppd.Width = 900 : ppd.Height = 700
        CType(ppd, Form).WindowState = FormWindowState.Maximized
        ppd.ShowDialog()
    End Sub

    Private Sub docImprimir_BeginPrint(sender As Object, e As Printing.PrintEventArgs) Handles docImprimir.BeginPrint
        _filaActualImpresion = 0
    End Sub

    Private Sub docImprimir_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles docImprimir.PrintPage
        Dim g As Graphics = e.Graphics

        ' --- FUENTES PROFESIONALES ---
        Dim fTituloDoc As New Font("Segoe UI", 24, FontStyle.Bold)
        Dim fEmpresa As New Font("Segoe UI", 14, FontStyle.Bold)
        Dim fCabecera As New Font("Segoe UI", 10, FontStyle.Bold)
        Dim fClienteNombre As New Font("Segoe UI", 12, FontStyle.Bold)
        Dim fNormal As New Font("Segoe UI", 10, FontStyle.Regular)
        Dim fFila As New Font("Segoe UI", 9, FontStyle.Regular)
        Dim fTotalGordo As New Font("Segoe UI", 14, FontStyle.Bold)

        ' --- COLORES Y PINCELES ---
        Dim bNegro As New SolidBrush(Color.Black)
        Dim bGrisOscuro As New SolidBrush(Color.FromArgb(70, 75, 80))
        Dim bAzulCorporativo As New SolidBrush(Color.FromArgb(40, 50, 70))
        Dim bBlanco As New SolidBrush(Color.White)
        Dim bGrisClaro As New SolidBrush(Color.FromArgb(245, 245, 245))

        Dim lapizFino As New Pen(Color.FromArgb(220, 220, 220), 1)
        Dim lapizGrueso As New Pen(Color.FromArgb(40, 50, 70), 2)

        Dim margenIzq As Integer = 50
        Dim margenDer As Integer = 777
        Dim anchoPagina As Integer = 727
        Dim yPos As Integer = 50

        Dim formatoIzquierda As New StringFormat() With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Center}
        Dim formatoDerecha As New StringFormat() With {.Alignment = StringAlignment.Far, .LineAlignment = StringAlignment.Center}
        Dim formatoCentro As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}

        ' 1. CABECERA DEL DOCUMENTO
        If _filaActualImpresion = 0 Then
            ' A) EMPRESA
            Dim empNombre As String = "EMPRESA NO CONFIGURADA", empCIF As String = "", empDireccion As String = "", empCP_Pob As String = "", empTelefono As String = ""
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()
                Dim sql As String = "SELECT NombreFiscal, CIF, Direccion, Poblacion, CodigoPostal, Telefono FROM Empresa LIMIT 1"
                Using cmd As New SQLiteCommand(sql, c)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            empNombre = If(IsDBNull(reader("NombreFiscal")), "", reader("NombreFiscal").ToString())
                            empCIF = If(IsDBNull(reader("CIF")), "", reader("CIF").ToString())
                            empDireccion = If(IsDBNull(reader("Direccion")), "", reader("Direccion").ToString())
                            Dim cp As String = If(IsDBNull(reader("CodigoPostal")), "", reader("CodigoPostal").ToString())
                            Dim pob As String = If(IsDBNull(reader("Poblacion")), "", reader("Poblacion").ToString())
                            empCP_Pob = (cp & " " & pob).Trim()
                            empTelefono = If(IsDBNull(reader("Telefono")), "", reader("Telefono").ToString())
                        End If
                    End Using
                End Using
            Catch ex As Exception
            End Try

            g.DrawString(empNombre, fEmpresa, bAzulCorporativo, margenIzq, yPos)
            g.DrawString("CIF: " & empCIF, fNormal, bGrisOscuro, margenIzq, yPos + 25)
            Dim bloqueContacto As String = empDireccion
            If empCP_Pob <> "" Then bloqueContacto &= vbCrLf & empCP_Pob
            If empTelefono <> "" Then bloqueContacto &= vbCrLf & "Tlf: " & empTelefono
            g.DrawString(bloqueContacto, fNormal, bGrisOscuro, margenIzq, yPos + 45)
            ' B) DATOS DEL DOCUMENTO (FACTURA COMPLETA)
            g.DrawString("FACTURA", fTituloDoc, bAzulCorporativo, margenDer, yPos, formatoDerecha)

            ' Datos principales
            g.DrawString("Nº Documento: " & TextBoxFactura.Text, fCabecera, bNegro, margenDer, yPos + 40, formatoDerecha)
            g.DrawString("Fecha Emisión: " & TextBoxFecha.Text, fNormal, bGrisOscuro, margenDer, yPos + 60, formatoDerecha)
            g.DrawString("Vencimiento: " & DateTimePickerFecha.Value.ToShortDateString(), fCabecera, bAzulCorporativo, margenDer, yPos + 80, formatoDerecha)

            ' Datos de trazabilidad y comercial
            ' (Asumo que tienes un TextBox llamado TextBoxAlbaranOrigen como vimos en códigos anteriores)
            If TextBoxAlbaranOrigen IsNot Nothing AndAlso TextBoxAlbaranOrigen.Text <> "" Then
                g.DrawString("Albarán Origen: " & TextBoxAlbaranOrigen.Text, fNormal, bGrisOscuro, margenDer, yPos + 105, formatoDerecha)
            End If
            g.DrawString("Comercial: " & TextBoxVendedor.Text, fNormal, bGrisOscuro, margenDer, yPos + 125, formatoDerecha)

            yPos += 145 ' Bajamos más la línea separadora para que quepa todo
            g.DrawLine(lapizGrueso, margenIzq, yPos, margenDer, yPos)
            yPos += 20

            ' C) CLIENTE
            Dim cliNombre As String = TextBoxCliente.Text
            Dim cliCIF As String = "", cliDireccion As String = "", cliPoblacion As String = "", cliContacto As String = ""

            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()
                Dim sqlCli As String = "SELECT NombreFiscal, CIF, Direccion, Poblacion, Provincia, Telefono, Email FROM Clientes WHERE NombreFiscal = @filtro OR CodigoCliente = @filtro LIMIT 1"
                Using cmd As New SQLiteCommand(sqlCli, c)
                    cmd.Parameters.AddWithValue("@filtro", TextBoxCliente.Text.Trim())
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            cliNombre = If(IsDBNull(reader("NombreFiscal")), TextBoxCliente.Text, reader("NombreFiscal").ToString())
                            cliCIF = If(IsDBNull(reader("CIF")), "", "CIF: " & reader("CIF").ToString())
                            cliDireccion = If(IsDBNull(reader("Direccion")), "", reader("Direccion").ToString())
                            Dim pob As String = If(IsDBNull(reader("Poblacion")), "", reader("Poblacion").ToString())
                            Dim prov As String = If(IsDBNull(reader("Provincia")), "", reader("Provincia").ToString())
                            cliPoblacion = If(pob <> "" And prov <> "", pob & " (" & prov & ")", pob & prov)
                            Dim tel As String = If(IsDBNull(reader("Telefono")), "", "Tlf: " & reader("Telefono").ToString())
                            Dim mail As String = If(IsDBNull(reader("Email")), "", reader("Email").ToString())
                            cliContacto = If(tel <> "" And mail <> "", tel & " | " & mail, tel & mail)
                        End If
                    End Using
                End Using
            Catch ex As Exception
            End Try

            g.FillRectangle(bGrisClaro, margenIzq, yPos, anchoPagina, 145)
            g.DrawRectangle(lapizFino, margenIzq, yPos, anchoPagina, 145)
            g.DrawString("DATOS DEL CLIENTE:", fCabecera, bAzulCorporativo, margenIzq + 15, yPos + 10)
            g.DrawString(cliNombre, fClienteNombre, bNegro, margenIzq + 15, yPos + 35)

            Dim xDatos As Integer = margenIzq + 15
            Dim yDatos As Integer = yPos + 62
            Dim interlineado As Integer = 18

            If cliCIF <> "" Then g.DrawString(cliCIF, fNormal, bGrisOscuro, xDatos, yDatos) : yDatos += interlineado
            If cliDireccion <> "" Then g.DrawString(cliDireccion, fNormal, bGrisOscuro, xDatos, yDatos) : yDatos += interlineado
            If cliPoblacion <> "" Then g.DrawString(cliPoblacion, fNormal, bGrisOscuro, xDatos, yDatos) : yDatos += interlineado
            If cliContacto <> "" Then g.DrawString(cliContacto, fNormal, bGrisOscuro, xDatos, yDatos)

            yPos += 165
        Else
            yPos += 30
        End If

        ' 2. CABECERA DE LA TABLA
        Dim rectCant As New Rectangle(margenIzq, yPos, 60, 30)
        Dim rectDesc As New Rectangle(margenIzq + 60, yPos, 340, 30)
        Dim rectPrecio As New Rectangle(margenIzq + 400, yPos, 110, 30)
        Dim rectDto As New Rectangle(margenIzq + 510, yPos, 70, 30)
        Dim rectTotal As New Rectangle(margenIzq + 580, yPos, 147, 30)

        g.FillRectangle(bAzulCorporativo, margenIzq, yPos, anchoPagina, 30)

        ' Ojo: En Facturas vuelve a llamarse Cantidad
        g.DrawString("Cant.", fCabecera, bBlanco, rectCant, formatoCentro)
        g.DrawString("Descripción", fCabecera, bBlanco, rectDesc, formatoIzquierda)
        g.DrawString("Precio", fCabecera, bBlanco, rectPrecio, formatoDerecha)
        g.DrawString("% Dto", fCabecera, bBlanco, rectDto, formatoDerecha)
        g.DrawString("Total", fCabecera, bBlanco, rectTotal, formatoDerecha)

        yPos += 35

        ' 3. RECORRER LÍNEAS DEL DATAGRIDVIEW
        While _filaActualImpresion < DataGridView1.Rows.Count
            Dim row As DataGridViewRow = DataGridView1.Rows(_filaActualImpresion)
            If row.IsNewRow Then Exit While

            rectCant.Y = yPos : rectDesc.Y = yPos : rectPrecio.Y = yPos : rectDto.Y = yPos : rectTotal.Y = yPos

            ' Volvemos a leer la columna "Cantidad"
            Dim cant As String = If(row.Cells("Cantidad").Value IsNot Nothing, row.Cells("Cantidad").Value.ToString(), "0")
            Dim desc As String = If(row.Cells("Descripcion").Value IsNot Nothing, row.Cells("Descripcion").Value.ToString(), "")
            Dim precio As String = If(row.Cells("PrecioUnitario").Value IsNot Nothing, Convert.ToDecimal(row.Cells("PrecioUnitario").Value).ToString("N2") & " €", "0,00 €")
            Dim dto As String = If(row.Cells("Descuento").Value IsNot Nothing, row.Cells("Descuento").Value.ToString() & " %", "0 %")
            Dim totalLinea As String = If(row.Cells("Total").Value IsNot Nothing, Convert.ToDecimal(row.Cells("Total").Value).ToString("N2") & " €", "0,00 €")

            If desc.Length > 55 Then desc = desc.Substring(0, 52) & "..."

            g.DrawString(cant, fFila, bNegro, rectCant, formatoCentro)
            g.DrawString(desc, fFila, bNegro, rectDesc, formatoIzquierda)
            g.DrawString(precio, fFila, bNegro, rectPrecio, formatoDerecha)
            g.DrawString(dto, fFila, bNegro, rectDto, formatoDerecha)
            g.DrawString(totalLinea, fFila, bNegro, rectTotal, formatoDerecha)

            yPos += 25
            g.DrawLine(lapizFino, margenIzq, yPos, margenDer, yPos)
            yPos += 5

            _filaActualImpresion += 1

            If yPos > 1000 AndAlso _filaActualImpresion < DataGridView1.Rows.Count Then
                e.HasMorePages = True
                Return
            End If
        End While

        ' 4. TOTALES
        If _filaActualImpresion >= DataGridView1.Rows.Count Then
            yPos += 20

            ' OJO: Cambia TextBoxTotalFac si el tuyo se llama TextBoxTotal u otro
            Dim totalBase As String = TextBoxBase.Text.Replace("Base imponible :", "").Trim()
            Dim totalIVA As String = TextBoxIva.Text.Replace("I.V.A :", "").Trim()
            Dim totalDoc As String = TextBoxTotalAlb.Text.Replace("TOTAL :", "").Trim()

            Dim anchoCajaTotales As Integer = 300
            Dim xCaja As Integer = margenDer - anchoCajaTotales
            ' --- CONDICIONES DE PAGO Y OBSERVACIONES (Abajo a la izquierda) ---
            Dim yNotas As Integer = yPos

            ' Título Forma de Pago
            g.DrawString("Forma de pago:", fCabecera, bAzulCorporativo, margenIzq, yNotas)
            g.DrawString(cboFormaPago.Text, fNormal, bNegro, margenIzq + 110, yNotas)

            ' Si la forma de pago es transferencia, puedes añadir aquí el IBAN en el futuro
            ' If cboFormaPago.Text = "Transferencia" Then
            '    g.DrawString("ES91 1234 5678 90 1234567890", fNormal, bGrisOscuro, margenIzq + 110, yNotas + 18)
            ' End If

            yNotas += 35

            ' Si hay observaciones, las dibujamos con ajuste de línea automático
            If TextBoxObservaciones.Text.Trim() <> "" Then
                g.DrawString("Observaciones:", fCabecera, bAzulCorporativo, margenIzq, yNotas)

                ' Creamos un rectángulo invisible para que el texto haga saltos de línea sin pisar los totales
                Dim rectObs As New Rectangle(margenIzq, yNotas + 20, anchoCajaTotales - 20, 80)
                g.DrawString(TextBoxObservaciones.Text, fNormal, bGrisOscuro, rectObs)
            End If
            g.DrawString("Base Imponible:", fNormal, bGrisOscuro, xCaja, yPos)
            g.DrawString(totalBase, fNormal, bNegro, margenDer, yPos, formatoDerecha)
            yPos += 25

            g.DrawString("Impuestos (IVA):", fNormal, bGrisOscuro, xCaja, yPos)
            g.DrawString(totalIVA, fNormal, bNegro, margenDer, yPos, formatoDerecha)
            yPos += 30

            g.FillRectangle(bAzulCorporativo, xCaja, yPos, anchoCajaTotales, 45)

            Dim rectTxtTotal As New Rectangle(xCaja + 15, yPos, 150, 45)
            Dim rectValTotal As New Rectangle(xCaja + 150, yPos, anchoCajaTotales - 165, 45)

            g.DrawString("TOTAL FACTURA", fCabecera, bBlanco, rectTxtTotal, formatoIzquierda)
            g.DrawString(totalDoc, fTotalGordo, bBlanco, rectValTotal, formatoDerecha)

            yPos += 80
            g.DrawString("Gracias por confiar en nuestros servicios.", fFila, bGrisOscuro, margenIzq, yPos)

            e.HasMorePages = False
        End If
    End Sub
    Protected Overrides ReadOnly Property CreateParams As System.Windows.Forms.CreateParams
        Get
            ' Forzamos el uso del tipo especifico de windows forms
            Dim cp As System.Windows.Forms.CreateParams = MyBase.CreateParams
            'Aplicamos el estilo extendido para evitar parpadeos (WS_EX_COMPOSITED)
            cp.ExStyle = cp.ExStyle Or &H2000000
            Return cp
        End Get
    End Property
    ' LOGÍSTICA: COMBOS DEPENDIENTES (RUTAS Y AGENCIAS) - FACTURAS

    ' 1. EL USUARIO ELIGE UNA RUTA -> Auto-seleccionamos la Agencia
    Private Sub cboRuta_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles cboRuta.SelectionChangeCommitted
        AutoAsignarAgenciaDesdeRuta()
    End Sub

    ' Si cboRuta tiene ruta seleccionada, busca la agencia asociada en Rutas y la asigna a cboAgencias. Llamar tras asignacion programatica de ruta.
    Private Sub AutoAsignarAgenciaDesdeRuta()
        If _agenciaManualSinRutas Then Return
        If cboRuta Is Nothing OrElse cboRuta.SelectedValue Is Nothing Then Return
        If cboRuta.SelectedIndex = -1 Then Return

        Try
            Dim idR As Integer
            If Not Integer.TryParse(cboRuta.SelectedValue.ToString(), idR) Then Return

            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Using cmd As New SQLiteCommand("SELECT ID_Agencia FROM Rutas WHERE ID_Ruta = @idRuta LIMIT 1", c)
                cmd.Parameters.AddWithValue("@idRuta", idR)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    cboAgencias.SelectedValue = Convert.ToInt32(result)
                End If
            End Using
        Catch
        End Try
    End Sub

    ' 2. EL USUARIO ELIGE UNA AGENCIA -> Filtramos las Rutas disponibles
    Private Sub cboAgencias_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles cboAgencias.SelectionChangeCommitted
        If cboAgencias.SelectedValue IsNot Nothing Then
            FiltrarRutasPorAgencia(Convert.ToInt32(cboAgencias.SelectedValue))
        End If
    End Sub

    ' 3. MÉTODO PARA FILTRAR EL DESPLEGABLE DE RUTAS (CON PLAN B)
    Private Sub FiltrarRutasPorAgencia(idAgencia As Integer)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' 1º INTENTO: Buscamos las rutas asignadas a esta agencia
            Dim sql As String = "SELECT ID_Ruta, NombreZona FROM Rutas WHERE ID_Agencia = @idAgencia AND Activo = 1"
            Dim daRutaFiltro As New SQLiteDataAdapter(sql, c)
            daRutaFiltro.SelectCommand.Parameters.AddWithValue("@idAgencia", idAgencia)

            Dim dtRutaFiltro As New DataTable()
            daRutaFiltro.Fill(dtRutaFiltro)

            ' 2º PLAN B (LA MAGIA): Si la agencia no tiene rutas (0 filas), traemos TODAS
            If dtRutaFiltro.Rows.Count = 0 Then
                _agenciaManualSinRutas = True '<--- ACTIVAMOS EL BLOQUEO

                sql = "SELECT ID_Ruta, NombreZona FROM Rutas WHERE Activo = 1"
                daRutaFiltro = New SQLiteDataAdapter(sql, c)
                dtRutaFiltro = New DataTable()
                daRutaFiltro.Fill(dtRutaFiltro)
            Else
                _agenciaManualSinRutas = False '<--- AGENCIA NORMAL, DESACTIVAMOS BLOQUEO
            End If

            ' Actualizamos el origen de datos del combo
            cboRuta.DataSource = dtRutaFiltro
            cboRuta.DisplayMember = "NombreZona"
            cboRuta.ValueMember = "ID_Ruta"
            cboRuta.SelectedIndex = -1 ' Lo dejamos en blanco para que el usuario elija
        Catch ex As Exception
        End Try
    End Sub
End Class
