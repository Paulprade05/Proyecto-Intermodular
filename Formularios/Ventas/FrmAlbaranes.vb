Imports System.Data.SQLite
Imports System.Text

Public Class FrmAlbaranes
    ' flag para acordarme si la agencia que ha elegido no tiene rutas
    Private _agenciaManualSinRutas As Boolean = False
    ' la clave es texto tipo ALB-26-001 (no entero, ojo)
    Private _numeroAlbaranActual As String = ""
    Private _dtLineas As DataTable
    Private _idsParaBorrar As New List(Of Integer)
    ' --- cosas para imprimir el albaran ---
    Private WithEvents btnImprimir As New Button()
    Private WithEvents docImprimir As New Printing.PrintDocument()
    Private _filaActualImpresion As Integer = 0
    ' --- combos nuevos que metí despues ---
    Private WithEvents cboFormaPago As New ComboBox()
    Private WithEvents cboRuta As New ComboBox()
    Private lblFormaPago As New Label() With {.Text = "Forma de Pago", .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.WhiteSmoke}
    Private lblRuta As New Label() With {.Text = "Ruta Asignada", .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.WhiteSmoke}
    Private WithEvents cboEstado As New ComboBox()
    Private Sub FrmAlbaranes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SuspendLayout()

        ' estilos del grid y la fecha
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
            .DisplayMember = "Nombre"
            .ValueMember = "ID_Agencia"
            .SelectedIndex = -1
        End With

        Me.Controls.Add(lblFormaPago) : Me.Controls.Add(cboFormaPago)
        Me.Controls.Add(lblRuta) : Me.Controls.Add(cboRuta)
        cboFormaPago.DropDownStyle = ComboBoxStyle.DropDownList
        cboRuta.DropDownStyle = ComboBoxStyle.DropDownList
        Me.Controls.Add(cboEstado)
        cboEstado.DropDownStyle = ComboBoxStyle.DropDownList
        cboEstado.Items.Clear()
        cboEstado.Items.AddRange(New String() {"Pendiente", "Entregado", "Facturado"})
        CargarDesplegables()

        Dim ultimoNum As String = ObtenerUltimoNumeroAlbaran()
        If Not String.IsNullOrEmpty(ultimoNum) Then
            CargarAlbaran(ultimoNum)
        Else
            LimpiarFormulario()
        End If

        TabControlModerno2.DrawMode = TabDrawMode.Normal
        TabControlModerno2.SizeMode = TabSizeMode.Normal
        TabControlModerno2.ItemSize = New Size(0, 0)

        If TextBox5 IsNot Nothing Then TextBox5.Visible = False
        If Button4 IsNot Nothing Then Button4.Visible = False

        ReorganizarControlesAutomaticamente()

        Me.ResumeLayout(True)
    End Sub

    Private Sub CargarDesplegables()
        _agenciaManualSinRutas = False ' reseteo el flag al cargar
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim daPago As New SQLiteDataAdapter("SELECT ID_FormaPago, Descripcion FROM FormasPago WHERE Activo=1", c)
            Dim dtPago As New DataTable() : daPago.Fill(dtPago)
            cboFormaPago.DataSource = dtPago : cboFormaPago.DisplayMember = "Descripcion" : cboFormaPago.ValueMember = "ID_FormaPago" : cboFormaPago.SelectedIndex = -1

            Dim daRuta As New SQLiteDataAdapter("SELECT ID_Ruta, NombreZona FROM Rutas WHERE Activo=1", c)
            Dim dtRuta As New DataTable() : daRuta.Fill(dtRuta)
            cboRuta.DataSource = dtRuta : cboRuta.DisplayMember = "NombreZona" : cboRuta.ValueMember = "ID_Ruta" : cboRuta.SelectedIndex = -1
        Catch ex As Exception
        End Try

    End Sub

    ' 1. monto la datatable y el grid
    Private Sub ConfigurarEstructuraDatos()
        If _dtLineas Is Nothing Then _dtLineas = New DataTable()

        If Not _dtLineas.Columns.Contains("ID_Linea") Then _dtLineas.Columns.Add("ID_Linea", GetType(Object))
        If Not _dtLineas.Columns.Contains("NumeroAlbaran") Then _dtLineas.Columns.Add("NumeroAlbaran", GetType(String))
        If Not _dtLineas.Columns.Contains("NumeroOrden") Then _dtLineas.Columns.Add("NumeroOrden", GetType(Integer))
        If Not _dtLineas.Columns.Contains("ID_Articulo") Then _dtLineas.Columns.Add("ID_Articulo", GetType(Object))
        If Not _dtLineas.Columns.Contains("Descripcion") Then _dtLineas.Columns.Add("Descripcion", GetType(String))

        If Not _dtLineas.Columns.Contains("CantidadServida") Then _dtLineas.Columns.Add("CantidadServida", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("PrecioUnitario") Then _dtLineas.Columns.Add("PrecioUnitario", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("Descuento") Then _dtLineas.Columns.Add("Descuento", GetType(Decimal))

        ' --- columnas para el iva (las añadi despues) ---
        If Not _dtLineas.Columns.Contains("PorcentajeIVA") Then _dtLineas.Columns.Add("PorcentajeIVA", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("PrecioConIVA") Then _dtLineas.Columns.Add("PrecioConIVA", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("Total") Then _dtLineas.Columns.Add("Total", GetType(Decimal))
        If Not _dtLineas.Columns.Contains("TotalConIVA") Then _dtLineas.Columns.Add("TotalConIVA", GetType(Decimal))

    End Sub

    Private Sub ConfigurarGrid()
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.Columns.Clear()
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Linea", .DataPropertyName = "ID_Linea", .Visible = False})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NumeroOrden", .DataPropertyName = "NumeroOrden", .HeaderText = "Nº", .Width = 40, .ReadOnly = True, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Articulo", .DataPropertyName = "ID_Articulo", .HeaderText = "ID Art", .Width = 70})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CantidadServida", .DataPropertyName = "CantidadServida", .HeaderText = "Entregado", .Width = 75, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2", .BackColor = Color.Ivory}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PrecioUnitario", .DataPropertyName = "PrecioUnitario", .HeaderText = "Precio Base", .Width = 85, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2"}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descuento", .DataPropertyName = "Descuento", .HeaderText = "% Dto", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2"}})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PorcentajeIVA", .DataPropertyName = "PorcentajeIVA", .HeaderText = "% IVA", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N0"}})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Total", .DataPropertyName = "Total", .HeaderText = "Total Base", .ReadOnly = True, .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.WhiteSmoke}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "TotalConIVA", .DataPropertyName = "TotalConIVA", .HeaderText = "Total (+IVA)", .ReadOnly = True, .Width = 100, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.FromArgb(230, 240, 250)}})
    End Sub

    ' 2. importar de un pedido
    Private Sub btnImportarPedido_Click(sender As Object, e As EventArgs)
        Using frm As New FrmBuscador
            frm.TablaABuscar = "Pedidos"
            If frm.ShowDialog = DialogResult.OK Then
                Dim numPedido = frm.Resultado
                If Not String.IsNullOrEmpty(numPedido) Then ImportarDatosPedido(numPedido)
            End If
        End Using
    End Sub



    Private Sub ImportarDatosPedido(numeroPedido As String)
        Dim c = ConexionBD.GetConnection()
        Try
            If c.State <> ConnectionState.Open Then c.Open()

            ' 1. cabecera. el SELECT P.* tira de todo, incluida la ruta y la forma de pago
            Dim sqlCab As String = "SELECT P.*, C.NombreFiscal, C.Direccion, C.Poblacion, C.CodigoPostal, V.Nombre AS NombreVend " &
                               "FROM Pedidos P " &
                               "LEFT JOIN Clientes C ON P.CodigoCliente = C.CodigoCliente " &
                               "LEFT JOIN Vendedores V ON P.ID_Vendedor = V.ID_Vendedor " &
                               "WHERE P.NumeroPedido = @num"

            Using cmd As New SQLiteCommand(sqlCab, c)
                cmd.Parameters.AddWithValue("@num", numeroPedido)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        LimpiarFormulario()

                        ' me guardo el numero de pedido para luego marcarlo como servido
                        ' OJO: hay que poner .Text Y .Tag. el guardar lee el .Tag, no el texto
                        ' me llevo un rato darme cuenta de eso 
                        TextBoxPedidoOrigen.Text = numeroPedido
                        TextBoxPedidoOrigen.Tag = numeroPedido

                        TextBoxIdCliente.Text = r("CodigoCliente").ToString()
                        TextBoxCliente.Text = r("NombreFiscal").ToString()
                        TextBoxIdVendedor.Text = If(IsDBNull(r("ID_Vendedor")), "", r("ID_Vendedor").ToString())
                        TextBoxVendedor.Text = If(IsDBNull(r("NombreVend")), "", r("NombreVend").ToString())

                        TextBoxDireccion.Text = If(IsDBNull(r("Direccion")), "", r("Direccion").ToString())
                        TextBoxPoblacion.Text = If(IsDBNull(r("Poblacion")), "", r("Poblacion").ToString())
                        TextBoxCP.Text = If(IsDBNull(r("CodigoPostal")), "", r("CodigoPostal").ToString())
                        TextBoxObservaciones.Text = "Generado desde Pedido " & numeroPedido

                        ' --- forma de pago y ruta ---
                        Dim idFPago = r("ID_FormaPago")
                        If Not IsDBNull(idFPago) AndAlso idFPago IsNot Nothing AndAlso idFPago.ToString() <> "" Then
                            cboFormaPago.SelectedValue = Convert.ToInt32(idFPago)
                        Else
                            cboFormaPago.SelectedIndex = -1
                        End If

                        Dim idRuta = r("ID_Ruta")
                        If Not IsDBNull(idRuta) AndAlso idRuta IsNot Nothing AndAlso idRuta.ToString() <> "" Then
                            Try
                                cboRuta.SelectedValue = Convert.ToInt32(idRuta)
                                AutoAsignarAgenciaDesdeRuta()
                            Catch
                                cboRuta.SelectedIndex = -1
                            End Try
                        Else
                            cboRuta.SelectedIndex = -1
                        End If
                    End If
                End Using
            End Using

            ' 2. ahora las lineas del pedido
            Dim sqlLin As String = "SELECT * FROM LineasPedido WHERE NumeroPedido = @num ORDER BY NumeroOrden ASC"
            Dim dtOrigen As New DataTable()
            Using cmd As New SQLiteCommand(sqlLin, c)
                cmd.Parameters.AddWithValue("@num", numeroPedido)
                Dim da As New SQLiteDataAdapter(cmd)
                da.Fill(dtOrigen)
            End Using

            _dtLineas.Rows.Clear()
            For Each rowOrig As DataRow In dtOrigen.Rows
                Dim rowNew As DataRow = _dtLineas.NewRow()

                ' aqui irá el numero del albaran nuevo cuando se guarde
                rowNew("NumeroAlbaran") = _numeroAlbaranActual
                rowNew("ID_Linea") = DBNull.Value ' lo dejo null para que sea INSERT y no UPDATE
                rowNew("NumeroOrden") = rowOrig("NumeroOrden")
                rowNew("ID_Articulo") = rowOrig("ID_Articulo")
                rowNew("Descripcion") = rowOrig("Descripcion")

                ' --- MATEMÁTICAS ---
                Dim cant As Decimal = 0 : Decimal.TryParse(rowOrig("Cantidad").ToString(), cant)
                Dim pr As Decimal = 0 : Decimal.TryParse(rowOrig("PrecioUnitario").ToString(), pr)
                Dim dt As Decimal = 0 : Decimal.TryParse(rowOrig("Descuento").ToString(), dt)
                Dim iva As Decimal = 21
                If dtOrigen.Columns.Contains("PorcentajeIVA") AndAlso Not IsDBNull(rowOrig("PorcentajeIVA")) Then
                    Decimal.TryParse(rowOrig("PorcentajeIVA").ToString(), iva)
                End If

                ' En el albarán, la cantidad servida suele ser por defecto toda la que se pidió
                rowNew("CantidadServida") = cant
                rowNew("PrecioUnitario") = pr
                rowNew("Descuento") = dt
                rowNew("PorcentajeIVA") = iva
                rowNew("PrecioConIVA") = pr * (1 + (iva / 100))

                Dim totalSinIva As Decimal = (cant * pr) * (1 - (dt / 100))
                rowNew("Total") = totalSinIva
                rowNew("TotalConIVA") = totalSinIva * (1 + (iva / 100))

                ' Guardamos el rastro: qué línea de pedido es esta


                _dtLineas.Rows.Add(rowNew)
            Next

            CalcularTotalesGenerales()
            MessageBox.Show("Datos del Pedido cargados correctamente.", "Importación", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error al importar el pedido: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub



    ' 3. CALCULO Y GRID
    Private Sub CalcularTotalesGenerales()
        Dim base As Decimal = 0
        Dim sumaIva As Decimal = 0

        If _dtLineas IsNot Nothing Then
            For Each row As DataRow In _dtLineas.Rows
                If row.RowState <> DataRowState.Deleted Then
                    Dim t As Decimal = 0
                    Dim ivaLinea As Decimal = 21

                    If _dtLineas.Columns.Contains("Total") Then Decimal.TryParse(row("Total").ToString(), t)
                    If _dtLineas.Columns.Contains("PorcentajeIVA") AndAlso Not IsDBNull(row("PorcentajeIVA")) Then
                        Decimal.TryParse(row("PorcentajeIVA").ToString(), ivaLinea)
                    End If

                    base += t
                    sumaIva += t * (ivaLinea / 100)
                End If
            Next
        End If

        If TextBoxBase IsNot Nothing Then TextBoxBase.Text = base.ToString("C2")
        If TextBoxIva IsNot Nothing Then TextBoxIva.Text = sumaIva.ToString("C2")
        If TextBoxTotalAlb IsNot Nothing Then TextBoxTotalAlb.Text = (base + sumaIva).ToString("C2")
    End Sub

    Private Sub DataGridView1_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles DataGridView1.CellBeginEdit
        Dim colName = DataGridView1.Columns(e.ColumnIndex).Name
        If colName = "ID_Articulo" Then
            Dim celda = DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex)
            celda.Tag = celda.Value?.ToString()
        End If
    End Sub

    Private Sub DataGridView1_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellEndEdit
        Dim fila = DataGridView1.Rows(e.RowIndex)
        Dim colName = DataGridView1.Columns(e.ColumnIndex).Name

        If colName = "ID_Articulo" Then
            Dim idArt As String = fila.Cells("ID_Articulo").Value?.ToString()
            Dim idAntiguo As String = fila.Cells("ID_Articulo").Tag?.ToString()

            If idArt = idAntiguo Then Return

            If String.IsNullOrWhiteSpace(idArt) Then
                fila.Cells("Descripcion").Value = "" : fila.Cells("PrecioUnitario").Value = 0 : fila.Cells("PorcentajeIVA").Value = 21 : fila.Tag = Nothing : Return
            End If
            Try
                Dim c = ConexionBD.GetConnection()
                Dim cmd As New SQLiteCommand("SELECT Descripcion, PrecioVenta, StockActual, TipoIVA FROM Articulos WHERE ID_Articulo = @id", c)
                cmd.Parameters.AddWithValue("@id", idArt)
                If c.State <> ConnectionState.Open Then c.Open()
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        fila.Cells("Descripcion").Value = r("Descripcion").ToString()
                        fila.Cells("PrecioUnitario").Value = Convert.ToDecimal(r("PrecioVenta"))
                        fila.Cells("PorcentajeIVA").Value = If(IsDBNull(r("TipoIVA")), 21, Convert.ToDecimal(r("TipoIVA")))

                        If Not IsDBNull(r("StockActual")) Then fila.Tag = Convert.ToDecimal(r("StockActual"))
                        If IsDBNull(fila.Cells("CantidadServida").Value) OrElse Val(fila.Cells("CantidadServida").Value) = 0 Then fila.Cells("CantidadServida").Value = 1

                        fila.Cells("ID_Articulo").Tag = idArt
                    Else
                        MessageBox.Show("Artículo no encontrado")
                    End If
                End Using
            Catch
            End Try
        End If

        If colName = "CantidadServida" Or colName = "PrecioUnitario" Or colName = "Descuento" Or colName = "PorcentajeIVA" Then
            Dim cant As Decimal = 0, prec As Decimal = 0, dto As Decimal = 0, iva As Decimal = 21
            Decimal.TryParse(fila.Cells("CantidadServida").Value?.ToString(), cant)
            Decimal.TryParse(fila.Cells("PrecioUnitario").Value?.ToString(), prec)
            Decimal.TryParse(fila.Cells("Descuento").Value?.ToString(), dto)
            Decimal.TryParse(fila.Cells("PorcentajeIVA").Value?.ToString(), iva)

            Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))
            Dim totalConIva As Decimal = totalSinIva * (1 + (iva / 100))

            fila.Cells("Total").Value = totalSinIva
            fila.Cells("TotalConIVA").Value = totalConIva

            CalcularTotalesGenerales()
        End If
    End Sub

    ' 4. GUARDAR Y CARGAR BBDD
    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles ButtonGuardar.Click
        If String.IsNullOrWhiteSpace(TextBoxIdCliente.Text) Then MessageBox.Show("Falta el Cliente") : Return

        Dim esNuevo As Boolean = String.IsNullOrEmpty(_numeroAlbaranActual)
        If esNuevo Then
            TextBoxAlbaran.Text = GenerarProximoNumeroAlbaran()
            _numeroAlbaranActual = TextBoxAlbaran.Text
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
                    Dim tot As Decimal = 0 : Decimal.TryParse(r("Total").ToString(), tot)
                    Dim ivaLinea As Decimal = 21 : Decimal.TryParse(r("PorcentajeIVA").ToString(), ivaLinea)
                    sumaBase += tot
                    sumaIva += tot * (ivaLinea / 100)
                End If
            Next

            ' B) Guardar Cabecera
            Dim idAgencia As Object = If(cboAgencias.SelectedValue IsNot Nothing AndAlso cboAgencias.SelectedIndex <> -1, cboAgencias.SelectedValue, DBNull.Value)
            Dim idFormaPago As Object = If(cboFormaPago.SelectedValue IsNot Nothing AndAlso cboFormaPago.SelectedIndex <> -1, cboFormaPago.SelectedValue, DBNull.Value)
            Dim idVend As Object = If(IsNumeric(TextBoxIdVendedor.Text) AndAlso Val(TextBoxIdVendedor.Text) > 0, Convert.ToInt32(TextBoxIdVendedor.Text), DBNull.Value)

            Dim idRuta As Object = If(cboRuta.SelectedValue IsNot Nothing AndAlso cboRuta.SelectedIndex <> -1, cboRuta.SelectedValue, DBNull.Value)

            Dim sql As String
            If esNuevo Then
                sql = "INSERT INTO Albaranes (NumeroAlbaran, NumeroPedido, Fecha, CodigoCliente, ID_Agencia, ID_FormaPago, ID_Ruta, NumeroBultos, PesoTotal, CodigoSeguimiento, Portes, Estado, Observaciones, DireccionEnvio, Poblacion, CodigoPostal, FechaEntrega, BaseImponible, ImporteIVA, Total, ID_Vendedor) " &
                  "VALUES (@num, @ped, @fecha, @cli, @agencia, @formaPago, @ruta, @bultos, @peso, @track, @portes, @estado, @obs, @dir, @pob, @cp, @FEntrega, @base, @iva, @total, @vend)"
            Else
                sql = "UPDATE Albaranes SET NumeroPedido=@ped, Fecha=@fecha, CodigoCliente=@cli, ID_Agencia=@agencia, ID_FormaPago=@formaPago, ID_Ruta=@ruta, NumeroBultos=@bultos, PesoTotal=@peso, CodigoSeguimiento=@track, Portes=@portes, Estado=@estado, Observaciones=@obs, DireccionEnvio=@dir, FechaEntrega=@FEntrega, Poblacion=@pob, CodigoPostal=@cp, BaseImponible=@base, ImporteIVA=@iva, Total=@total, ID_Vendedor=@vend WHERE NumeroAlbaran=@num"
            End If

            Dim fechaAlbaran As DateTime = DateTime.Now
            If DateTime.TryParse(TextBoxFecha.Text, fechaAlbaran) = False Then fechaAlbaran = DateTime.Now

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Transaction = trans
                cmd.Parameters.AddWithValue("@num", _numeroAlbaranActual)

                Dim ped As Object = If(TextBoxPedidoOrigen.Tag IsNot Nothing AndAlso TextBoxPedidoOrigen.Tag.ToString() <> "", TextBoxPedidoOrigen.Tag, DBNull.Value)
                cmd.Parameters.AddWithValue("@ped", ped)
                cmd.Parameters.AddWithValue("@fecha", fechaAlbaran.ToString("yyyy-MM-dd HH:mm:ss"))
                cmd.Parameters.AddWithValue("@cli", TextBoxIdCliente.Text)

                cmd.Parameters.AddWithValue("@agencia", idAgencia)
                cmd.Parameters.AddWithValue("@formaPago", idFormaPago)
                cmd.Parameters.AddWithValue("@ruta", idRuta)
                cmd.Parameters.AddWithValue("@bultos", If(IsNumeric(TextBoxBultos.Text), TextBoxBultos.Text, 1))
                cmd.Parameters.AddWithValue("@peso", If(IsNumeric(TextBoxPeso.Text), TextBoxPeso.Text, 0))
                cmd.Parameters.AddWithValue("@track", TextBoxTracking.Text)
                cmd.Parameters.AddWithValue("@portes", ComboBoxPortes.Text)
                cmd.Parameters.AddWithValue("@vend", idVend)
                cmd.Parameters.AddWithValue("@estado", cboEstado.Text)
                cmd.Parameters.AddWithValue("@obs", TextBoxObservaciones.Text)
                cmd.Parameters.AddWithValue("@FEntrega", DateTimePickerFecha.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                cmd.Parameters.AddWithValue("@dir", TextBoxDireccion.Text)
                cmd.Parameters.AddWithValue("@pob", TextBoxPoblacion.Text)
                cmd.Parameters.AddWithValue("@cp", TextBoxCP.Text)

                cmd.Parameters.AddWithValue("@base", sumaBase)
                cmd.Parameters.AddWithValue("@iva", sumaIva)
                cmd.Parameters.AddWithValue("@total", sumaBase + sumaIva)
                cmd.ExecuteNonQuery()
            End Using

            ' C) Guardar Líneas y PROCESAR MOVIMIENTOS
            For Each idDel In _idsParaBorrar
                Dim idArtDel As Integer = 0
                Dim cantDel As Decimal = 0
                Using cmdInfo As New SQLiteCommand("SELECT ID_Articulo, CantidadServida FROM LineasAlbaran WHERE ID_Linea = @id", c, trans)
                    cmdInfo.Parameters.AddWithValue("@id", idDel)
                    Using reader = cmdInfo.ExecuteReader()
                        If reader.Read() Then
                            If Not IsDBNull(reader("ID_Articulo")) Then idArtDel = Convert.ToInt32(reader("ID_Articulo"))
                            cantDel = Convert.ToDecimal(reader("CantidadServida"))
                        End If
                    End Using
                End Using

                Using cmdDel As New SQLiteCommand("DELETE FROM LineasAlbaran WHERE ID_Linea = @id", c)
                    cmdDel.Transaction = trans : cmdDel.Parameters.AddWithValue("@id", idDel) : cmdDel.ExecuteNonQuery()
                End Using
                If idArtDel > 0 Then ActualizarStockYMovimiento(c, trans, _numeroAlbaranActual, idArtDel, -cantDel, fechaAlbaran)
            Next
            _idsParaBorrar.Clear()

            If _dtLineas IsNot Nothing Then
                For Each r As DataRow In _dtLineas.Rows
                    If r.RowState = DataRowState.Deleted Then Continue For

                    Dim cant As Decimal = 0 : Decimal.TryParse(r("CantidadServida").ToString(), cant)
                    Dim prec As Decimal = 0 : Decimal.TryParse(r("PrecioUnitario").ToString(), prec)
                    Dim desc As Decimal = 0 : Decimal.TryParse(r("Descuento").ToString(), desc)
                    Dim iva As Decimal = If(IsDBNull(r("PorcentajeIVA")), 21, Convert.ToDecimal(r("PorcentajeIVA")))
                    Dim tot As Decimal = (cant * prec) * (1 - (desc / 100))

                    Dim idLin = r("ID_Linea")
                    Dim idArt As Object = If(IsNumeric(r("ID_Articulo")) AndAlso Val(r("ID_Articulo")) > 0, r("ID_Articulo"), DBNull.Value)

                    If IsDBNull(idLin) OrElse Not IsNumeric(idLin) Then
                        ' NUEVO: Capturamos el ID de la línea del pedido si existe

                        Dim sqlIns As String = "INSERT INTO LineasAlbaran (NumeroAlbaran, NumeroOrden, ID_Articulo, Descripcion, CantidadServida, PrecioUnitario, Descuento, PorcentajeIVA, Total) VALUES (@alb, @ord, @art, @desc, @cant, @prec, @dcto, @iva, @tot)"
                        Using cmdL As New SQLiteCommand(sqlIns, c)
                            cmdL.Transaction = trans
                            cmdL.Parameters.AddWithValue("@alb", _numeroAlbaranActual)
                            cmdL.Parameters.AddWithValue("@ord", r("NumeroOrden"))
                            cmdL.Parameters.AddWithValue("@art", idArt)
                            cmdL.Parameters.AddWithValue("@desc", r("Descripcion"))
                            cmdL.Parameters.AddWithValue("@cant", cant)
                            cmdL.Parameters.AddWithValue("@prec", prec)
                            cmdL.Parameters.AddWithValue("@dcto", desc)
                            cmdL.Parameters.AddWithValue("@iva", iva)
                            cmdL.Parameters.AddWithValue("@tot", tot)
                            cmdL.ExecuteNonQuery()
                        End Using
                        If Not IsDBNull(idArt) Then ActualizarStockYMovimiento(c, trans, _numeroAlbaranActual, Convert.ToInt32(idArt), cant, fechaAlbaran)
                    ElseIf r.RowState = DataRowState.Modified Then
                        Dim cantAnterior As Decimal = 0
                        If r.HasVersion(DataRowVersion.Original) AndAlso Not IsDBNull(r("CantidadServida", DataRowVersion.Original)) Then
                            Decimal.TryParse(r("CantidadServida", DataRowVersion.Original).ToString(), cantAnterior)
                        End If
                        Dim variacion = cant - cantAnterior

                        Dim sqlUpd As String = "UPDATE LineasAlbaran SET ID_Articulo=@art, Descripcion=@desc, CantidadServida=@cant, PrecioUnitario=@prec, Descuento=@dcto, PorcentajeIVA=@iva, Total=@tot WHERE ID_Linea=@id"
                        Using cmdL As New SQLiteCommand(sqlUpd, c)
                            cmdL.Transaction = trans
                            cmdL.Parameters.AddWithValue("@art", idArt)
                            cmdL.Parameters.AddWithValue("@desc", r("Descripcion"))
                            cmdL.Parameters.AddWithValue("@cant", cant)
                            cmdL.Parameters.AddWithValue("@prec", prec)
                            cmdL.Parameters.AddWithValue("@dcto", desc)
                            cmdL.Parameters.AddWithValue("@iva", iva)
                            cmdL.Parameters.AddWithValue("@tot", tot)
                            cmdL.Parameters.AddWithValue("@id", idLin)
                            cmdL.ExecuteNonQuery()
                        End Using
                        If Not IsDBNull(idArt) AndAlso variacion <> 0 Then
                            ActualizarStockYMovimiento(c, trans, _numeroAlbaranActual, Convert.ToInt32(idArt), variacion, fechaAlbaran)
                        End If
                    End If
                Next
            End If
            ' MAGIA DE ESTADOS: Marcar Pedido como Servido
            If TextBoxPedidoOrigen.Tag IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(TextBoxPedidoOrigen.Tag.ToString()) Then
                Dim sqlEstado As String = "UPDATE Pedidos SET Estado = @estado WHERE NumeroPedido = @idPed"
                Using cmdEst As New SQLiteCommand(sqlEstado, c)
                    cmdEst.Transaction = trans
                    cmdEst.Parameters.AddWithValue("@estado", EstadosDocumento.PEDIDO_SERVIDO)
                    cmdEst.Parameters.AddWithValue("@idPed", TextBoxPedidoOrigen.Tag.ToString())
                    cmdEst.ExecuteNonQuery()
                End Using
            End If
            trans.Commit()
            MessageBox.Show("Albarán Guardado y Movimientos generados.")
            CargarAlbaran(_numeroAlbaranActual)

        Catch ex As Exception
            If trans IsNot Nothing Then trans.Rollback()
            LogErrores.Registrar("FrmAlbaranes.Guardar", ex)
            MessageBox.Show("Error al guardar: " & ex.Message)
        End Try
    End Sub

    Private Sub CargarAlbaran(num As String)
        Try
            Dim conexion = ConexionBD.GetConnection()
            If conexion.State <> ConnectionState.Open Then conexion.Open()

            Dim sql As String = "SELECT A.*, C.NombreFiscal AS NombreCliente, V.Nombre AS NombreVendedor, T.Nombre As NombreAgencia, C.Poblacion, C.CodigoPostal " &
                                "FROM Albaranes A " &
                                "LEFT JOIN Clientes C ON A.CodigoCliente = C.CodigoCliente " &
                                "LEFT JOIN Vendedores V ON A.ID_Vendedor = V.ID_Vendedor " &
                                "LEFT JOIN Agencias T ON A.ID_Agencia = T.ID_Agencia " &
                                "WHERE A.NumeroAlbaran= @num"

            Using cmd As New SQLiteCommand(sql, conexion)
                cmd.Parameters.AddWithValue("@num", num)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        _numeroAlbaranActual = num

                        TextBoxAlbaran.Text = reader("NumeroAlbaran").ToString()
                        If Not IsDBNull(reader("Fecha")) Then TextBoxFecha.Text = Convert.ToDateTime(reader("Fecha")).ToShortDateString()
                        TextBoxObservaciones.Text = If(IsDBNull(reader("Observaciones")), "", reader("Observaciones").ToString())
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

                        TextBoxIdCliente.Text = reader("CodigoCliente").ToString()
                        TextBoxCliente.Text = If(IsDBNull(reader("NombreCliente")), "", reader("NombreCliente").ToString())
                        TextBoxCliente.Tag = reader("CodigoCliente")
                        TextBoxDireccion.Text = reader("DireccionEnvio").ToString()
                        TextBoxPoblacion.Text = reader("Poblacion").ToString()
                        TextBoxCP.Text = reader("CodigoPostal").ToString()
                        ' --- FORMA DE PAGO ---
                        If Not IsDBNull(reader("ID_FormaPago")) Then
                            cboFormaPago.SelectedValue = Convert.ToInt32(reader("ID_FormaPago"))
                        Else
                            cboFormaPago.SelectedIndex = -1
                        End If

                        ' --- RUTA ASIGNADA ---
                        If Not IsDBNull(reader("ID_Ruta")) Then
                            cboRuta.SelectedValue = Convert.ToInt32(reader("ID_Ruta"))
                        Else
                            cboRuta.SelectedIndex = -1
                        End If
                        TextBoxIdVendedor.Text = If(IsDBNull(reader("ID_Vendedor")), "", reader("ID_Vendedor").ToString())
                        TextBoxVendedor.Text = If(IsDBNull(reader("NombreVendedor")), "", reader("NombreVendedor").ToString())

                        If Not IsDBNull(reader("ID_Agencia")) Then
                            cboAgencias.SelectedValue = Convert.ToInt32(reader("ID_Agencia"))
                        ElseIf cboRuta.SelectedIndex <> -1 Then
                            ' Albaran sin agencia pero con ruta: derivar agencia desde la ruta
                            AutoAsignarAgenciaDesdeRuta()
                        Else
                            cboAgencias.SelectedIndex = -1
                        End If

                        ' Cuando reabrimos un albarán ya guardado, restablecemos también el .Tag para
                        ' que un guardado posterior pueda actualizar el estado del pedido si hace falta.
                        If Not IsDBNull(reader("NumeroPedido")) Then
                            TextBoxPedidoOrigen.Text = reader("NumeroPedido").ToString()
                            TextBoxPedidoOrigen.Tag = reader("NumeroPedido").ToString()
                        End If
                        If Not IsDBNull(reader("FechaEntrega")) Then DateTimePickerFecha.Value = Convert.ToDateTime(reader("FechaEntrega"))

                        TextBoxBultos.Text = reader("NumeroBultos")
                        TextBoxPeso.Text = reader("PesoTotal")
                        TextBoxTracking.Text = reader("CodigoSeguimiento")
                        ComboBoxPortes.Text = If(IsDBNull(reader("Portes")), "Pagados", reader("Portes").ToString())
                    Else
                        MessageBox.Show("Albaran no encontrado.")
                        Return
                    End If
                End Using
            End Using

            CargarLineas()
        Catch ex As Exception
            MessageBox.Show("Error al cargar: " & ex.Message)
        End Try
    End Sub

    Private Sub CargarLineas()
        Try
            Dim sql As String = "SELECT * FROM LineasAlbaran WHERE NumeroAlbaran = @num ORDER BY NumeroOrden ASC"
            Dim conexion = ConexionBD.GetConnection()
            Using cmd As New SQLiteCommand(sql, conexion)
                cmd.Parameters.AddWithValue("@num", _numeroAlbaranActual)
                Dim da As New SQLiteDataAdapter(cmd)
                _dtLineas = New DataTable()
                da.Fill(_dtLineas)

                ConfigurarEstructuraDatos()

                For Each row As DataRow In _dtLineas.Rows
                    Dim cant As Decimal = If(IsDBNull(row("CantidadServida")), 0, Convert.ToDecimal(row("CantidadServida")))
                    Dim prec As Decimal = If(IsDBNull(row("PrecioUnitario")), 0, Convert.ToDecimal(row("PrecioUnitario")))
                    Dim dto As Decimal = If(IsDBNull(row("Descuento")), 0, Convert.ToDecimal(row("Descuento")))
                    Dim iva As Decimal = If(IsDBNull(row("PorcentajeIVA")), 21, Convert.ToDecimal(row("PorcentajeIVA")))

                    row("PorcentajeIVA") = iva
                    row("PrecioConIVA") = prec * (1 + (iva / 100))

                    Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))
                    row("Total") = totalSinIva
                    row("TotalConIVA") = totalSinIva * (1 + (iva / 100))
                Next

                DataGridView1.DataSource = _dtLineas
            End Using

            CalcularTotalesGenerales()
            If DataGridView1.Columns.Contains("ID_Linea") Then DataGridView1.Columns("ID_Linea").Visible = False

        Catch ex As Exception
            MessageBox.Show("Error al cargar líneas: " & ex.Message)
        End Try
    End Sub

    Private Sub LimpiarFormulario()
        _numeroAlbaranActual = ""
        _idsParaBorrar.Clear()
        TextBoxAlbaran.Text = GenerarProximoNumeroAlbaran()
        TextBoxIdCliente.Text = "" : TextBoxCliente.Text = ""
        TextBoxPedidoOrigen.Text = "" : TextBoxPedidoOrigen.Tag = Nothing
        TextBoxIdVendedor.Text = "" : TextBoxVendedor.Text = ""
        cboAgencias.SelectedIndex = -1

        TextBoxBultos.Text = "1" : TextBoxPeso.Text = "0"
        TextBoxDireccion.Text = "" : TextBoxPoblacion.Text = "" : TextBoxCP.Text = ""
        TextBoxTracking.Text = GenerarCodigoSeguimiento()
        TextBoxFecha.Text = DateTime.Now.ToShortDateString()
        DateTimePickerFecha.Value = DateTime.Now
        TextBoxBase.Text = "0,00" : TextBoxIva.Text = "0,00" : TextBoxTotalAlb.Text = "0,00"
        cboEstado.Text = "Pendiente"
        ' Reseteamos los desplegables para quitar cualquier filtro de agencia aplicado
        CargarDesplegables()
        ConfigurarGrid()
        _dtLineas = New DataTable()
        ConfigurarEstructuraDatos()
        DataGridView1.DataSource = _dtLineas
        If DataGridView1.Columns.Contains("ID_Linea") Then DataGridView1.Columns("ID_Linea").Visible = False
    End Sub

    Private Function GenerarProximoNumeroAlbaran() As String
        ' Delegamos en NumeradorDocumentos para tener orden numérico (no lexicográfico).
        Return NumeradorDocumentos.SiguienteNumero("ALB-", "Albaranes", "NumeroAlbaran")
    End Function

    Private Function ObtenerUltimoNumeroAlbaran() As String
        Dim ultimoNumero As String = ""
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim sql As String = "SELECT MAX(NumeroAlbaran) FROM Albaranes"
            Using cmd As New SQLiteCommand(sql, conexion)
                If conexion.State <> ConnectionState.Open Then conexion.Open()
                Dim resultado = cmd.ExecuteScalar()
                If resultado IsNot Nothing AndAlso Not IsDBNull(resultado) Then
                    ultimoNumero = resultado.ToString()
                End If
            End Using
        Catch
        End Try
        Return ultimoNumero
    End Function

    ' 5. BOTONERA Y NAVEGACIÓN
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Using frm As New FrmBuscador()
            frm.TablaABuscar = "Albaranes"
            If frm.ShowDialog() = DialogResult.OK Then
                CargarAlbaran(frm.Resultado)
            End If
        End Using
    End Sub

    Private Sub ButtonAnterior_Click(sender As Object, e As EventArgs) Handles ButtonAnterior.Click
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim numDestino As String = ""
            Dim sql As String = "SELECT MAX(NumeroAlbaran) FROM Albaranes WHERE NumeroAlbaran < @actual"
            Using cmd As New SQLiteCommand(sql, conexion)
                If String.IsNullOrEmpty(_numeroAlbaranActual) Then
                    cmd.CommandText = "SELECT MAX(NumeroAlbaran) FROM Albaranes"
                Else
                    cmd.Parameters.AddWithValue("@actual", _numeroAlbaranActual)
                End If

                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    numDestino = result.ToString()
                    CargarAlbaran(numDestino)
                Else
                    MessageBox.Show("Primer albaran.")
                End If
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ButtonSiguiente_Click(sender As Object, e As EventArgs) Handles ButtonSiguiente.Click
        If String.IsNullOrEmpty(_numeroAlbaranActual) Then Return
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim sql As String = "SELECT MIN(NumeroAlbaran) FROM Albaranes WHERE NumeroAlbaran > @actual"
            Using cmd As New SQLiteCommand(sql, conexion)
                cmd.Parameters.AddWithValue("@actual", _numeroAlbaranActual)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    CargarAlbaran(result.ToString())
                Else
                    MessageBox.Show("Ultimo albaran")
                End If
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ButtonNuevo_Click(sender As Object, e As EventArgs) Handles ButtonNuevoPed.Click
        LimpiarFormulario()
    End Sub

    Private Sub ButtonBorrar_Click(sender As Object, e As EventArgs) Handles ButtonBorrar.Click
        If _numeroAlbaranActual = "" Then
            MessageBox.Show("No puedes borrar un albaran que aún no existe (es nuevo).")
            Return
        End If

        If MessageBox.Show("¿Estás seguro de ELIMINAR este albaran? Se devolverá el stock al almacén.", "Confirmar Borrado", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return

        Dim conexion = ConexionBD.GetConnection()
        Dim transaccion As SQLiteTransaction = Nothing

        Try
            If conexion.State <> ConnectionState.Open Then conexion.Open()
            transaccion = conexion.BeginTransaction()

            Dim sqlRecuperar As String = "SELECT ID_Articulo, CantidadServida FROM LineasAlbaran WHERE NumeroAlbaran = @id"
            Dim lineasABorrar As New List(Of Tuple(Of Integer, Decimal))
            Using cmdRec As New SQLiteCommand(sqlRecuperar, conexion, transaccion)
                cmdRec.Parameters.AddWithValue("@id", _numeroAlbaranActual)
                Using reader = cmdRec.ExecuteReader()
                    While reader.Read()
                        If Not IsDBNull(reader("ID_Articulo")) Then
                            lineasABorrar.Add(New Tuple(Of Integer, Decimal)(Convert.ToInt32(reader("ID_Articulo")), Convert.ToDecimal(reader("CantidadServida"))))
                        End If
                    End While
                End Using
            End Using

            For Each linea In lineasABorrar
                ActualizarStockYMovimiento(conexion, transaccion, _numeroAlbaranActual & " (Borrado)", linea.Item1, -linea.Item2, DateTime.Now)
            Next

            Dim sqlLineas As String = "DELETE FROM LineasAlbaran WHERE NumeroAlbaran= @id"
            Using cmd As New SQLiteCommand(sqlLineas, conexion)
                cmd.Transaction = transaccion : cmd.Parameters.AddWithValue("@id", _numeroAlbaranActual) : cmd.ExecuteNonQuery()
            End Using

            Dim sqlCabecera As String = "DELETE FROM Albaranes WHERE NumeroAlbaran = @id"
            Using cmd As New SQLiteCommand(sqlCabecera, conexion)
                cmd.Transaction = transaccion : cmd.Parameters.AddWithValue("@id", _numeroAlbaranActual) : cmd.ExecuteNonQuery()
            End Using

            transaccion.Commit()
            MessageBox.Show("Albarán eliminado correctamente. Stock restaurado.", "Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LimpiarFormulario()

        Catch ex As Exception
            If transaccion IsNot Nothing Then transaccion.Rollback()
            MessageBox.Show("Error crítico al borrar: " & ex.Message)
        End Try
    End Sub

    Private Sub ButtonNuevaLinea_Click(sender As Object, e As EventArgs) Handles ButtonNuevaLinea.Click
        If _dtLineas Is Nothing Then _dtLineas = New DataTable()
        ConfigurarEstructuraDatos()

        Dim nuevaFila As DataRow = _dtLineas.NewRow()
        Dim orden As Integer = 0
        For Each row As DataRow In _dtLineas.Rows
            If row.RowState <> DataRowState.Deleted Then orden += 1
        Next

        nuevaFila("NumeroOrden") = orden + 1
        nuevaFila("NumeroAlbaran") = _numeroAlbaranActual
        nuevaFila("ID_Linea") = DBNull.Value
        nuevaFila("ID_Articulo") = 0
        nuevaFila("Descripcion") = ""
        nuevaFila("CantidadServida") = 1
        nuevaFila("PrecioUnitario") = 0
        nuevaFila("Descuento") = 0
        nuevaFila("PorcentajeIVA") = 21
        nuevaFila("Total") = 0

        _dtLineas.Rows.Add(nuevaFila)

        If DataGridView1.Rows.Count > 0 Then
            Dim ultimaFilaIndex As Integer = DataGridView1.Rows.Count - 1
            DataGridView1.CurrentCell = DataGridView1.Rows(ultimaFilaIndex).Cells("ID_Articulo")
            DataGridView1.BeginEdit(True)
        End If
    End Sub

    Private Sub ButtonBorrarLineas_Click(sender As Object, e As EventArgs) Handles ButtonBorrarLineas.Click
        If DataGridView1.SelectedRows.Count = 0 Then Return
        For Each f As DataGridViewRow In DataGridView1.SelectedRows
            If Not f.IsNewRow Then
                Dim idVal = f.Cells("ID_Linea").Value
                If IsNumeric(idVal) AndAlso Val(idVal) > 0 Then _idsParaBorrar.Add(CInt(idVal))
                DataGridView1.Rows.Remove(f)
            End If
        Next
        CalcularTotalesGenerales()
    End Sub

    ' 6. EVENTOS DE CAJAS DE TEXTO
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

            ' Buscamos al cliente en la base de datos
            Dim cmd As New SQLiteCommand("SELECT NombreFiscal, Direccion, Poblacion, CodigoPostal FROM Clientes WHERE CodigoCliente=@id", c)
            cmd.Parameters.AddWithValue("@id", TextBoxIdCliente.Text.Trim())

            Dim r = cmd.ExecuteReader()

            If r.Read() Then
                ' ¡Cliente encontrado! Rellenamos los datos de logística del Albarán
                TextBoxCliente.Text = r("NombreFiscal").ToString()
                TextBoxDireccion.Text = r("Direccion").ToString()
                TextBoxPoblacion.Text = r("Poblacion").ToString()
                TextBoxCP.Text = r("CodigoPostal").ToString()
            Else
                ' El cliente NO existe
                Dim respuesta As DialogResult = MessageBox.Show(
                    "El código de cliente '" & TextBoxIdCliente.Text & "' no existe." & vbCrLf & vbCrLf &
                    "¿Deseas crear esta nueva ficha de cliente ahora?",
                    "Cliente no encontrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)

                If respuesta = DialogResult.Yes Then
                    ' Guardamos el código que escribió para el Pro-Tip
                    Dim codigoFaltante As String = TextBoxIdCliente.Text.Trim()

                    ' Limpiamos el albarán
                    TextBoxIdCliente.Text = ""
                    TextBoxCliente.Text = ""

                    ' Abrimos la ficha y le pasamos el código
                    Dim frm As New FrmClienteDetalle()
                    frm.CodigoNuevoPredefinido = codigoFaltante ' <--- La magia actúa de nuevo
                    frm.ShowDialog()

                    ' Devolvemos el foco para que lo vuelva a escribir ya creado
                    TextBoxIdCliente.Focus()
                Else
                    ' Si no lo quiere crear, borramos el código falso
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

    Private Sub EstilizarFecha(dtp As DateTimePicker)
        dtp.Format = DateTimePickerFormat.Custom : dtp.CustomFormat = "dd/MM/yyyy"
        dtp.Font = New Font("Segoe UI", 10) : dtp.MinimumSize = New Size(0, 25)
    End Sub

    ' 7. REORGANIZACIÓN VISUAL
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
        lblTitulo.Text = "ALBARÁN DE VENTA"
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

        ' Nº de albarán destacado
        If TextBoxAlbaran IsNot Nothing Then
            TextBoxAlbaran.Bounds = New Rectangle(margenIzq + 410, 28, 130, 28)
            TextBoxAlbaran.BackColor = COLOR_PANEL_TOTALES
            TextBoxAlbaran.ForeColor = COLOR_ACENTO
            TextBoxAlbaran.Font = New Font("Segoe UI", 11.5F, FontStyle.Bold)
            TextBoxAlbaran.BorderStyle = BorderStyle.FixedSingle
            TextBoxAlbaran.TextAlign = HorizontalAlignment.Center
            TextBoxAlbaran.BringToFront()
        End If

        ' Botón lupa pegado al TextBoxAlbaran
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

        ' 2. ZONA DE FORMULARIO — IDÉNTICA AL ALBARÁN DE COMPRA
        Dim yInicioFormulario As Integer = alturaBanda + 25

        Dim col1_X As Integer = margenIzq                  ' Cliente / Vendedor
        Dim col2_X As Integer = 600                        ' Fechas / Estado / Observaciones
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
                    Case "fecha entrega" : ctrl.Location = New Point(col2_X + 160, yFila1)
                    Case "vendedor" : ctrl.Location = New Point(col1_X, yFila3)
                    Case "estado" : ctrl.Location = New Point(col2_X, yFila3)
                    Case "agencia" : ctrl.Location = New Point(col3_X, yFila1)
                End Select
            End If
        Next

        ' Eliminar duplicados de Observaciones en el header si existieran de versiones anteriores
        For Each ctlName In {"LblObservacionesAlb", "TextBoxObsAlbPrincipal"}
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

        ' --- Bloque 3: Agencia + Pedido origen (igual que Compras tiene Nº Alb. Proveedor + Pedido) ---
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

        ' 4. PANEL LOGÍSTICA (TabControl) — visible o colapsado según _logisticaVisible
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

    ' Toggle del panel desplegable de logística
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

    ' 8. STOCK EN TIEMPO REAL 
    Private Sub DataGridView1_SelectionChanged(sender As Object, e As EventArgs) Handles DataGridView1.SelectionChanged
        ActualizarLabelStock()
    End Sub

    Private Sub DataGridView1_CellEndEdit_StockLbl(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellEndEdit
        ActualizarLabelStock()
    End Sub

    Private Sub ActualizarLabelStock()
        Dim lblStock As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LabelStock")
        If lblStock Is Nothing Then Return

        If DataGridView1.CurrentRow Is Nothing OrElse DataGridView1.CurrentRow.IsNewRow Then
            lblStock.Text = "Stock disponible: -"
            lblStock.ForeColor = Color.FromArgb(170, 175, 180)
            Return
        End If

        Try
            Dim idCelda As Object = DataGridView1.CurrentRow.Cells("ID_Articulo").Value

            If idCelda Is Nothing OrElse DBNull.Value.Equals(idCelda) OrElse Convert.ToInt32(idCelda) <= 0 Then
                lblStock.Text = "Stock disponible: -"
                lblStock.ForeColor = Color.FromArgb(170, 175, 180)
                Return
            End If

            Dim idArticulo As Integer = Convert.ToInt32(idCelda)
            Dim cantidadEnLinea As Decimal = 0
            Dim cantCelda As Object = DataGridView1.CurrentRow.Cells("CantidadServida").Value
            If cantCelda IsNot Nothing AndAlso Not DBNull.Value.Equals(cantCelda) Then
                cantidadEnLinea = Convert.ToDecimal(cantCelda)
            End If

            ' Determinar si la linea ya esta guardada en BD (su descuento de stock ya se aplico).
            Dim lineaYaGuardada As Boolean = False
            Try
                Dim drv As DataRowView = TryCast(DataGridView1.CurrentRow.DataBoundItem, DataRowView)
                If drv IsNot Nothing Then
                    Dim st = drv.Row.RowState
                    If st = DataRowState.Unchanged OrElse st = DataRowState.Modified Then
                        lineaYaGuardada = True
                    End If
                End If
            Catch
            End Try

            Dim stockActual As Decimal = ConsultarStock(idArticulo)

            If lineaYaGuardada Then
                ' La cantidad de esta linea ya esta descontada en stockActual. No volver a restar.
                lblStock.Text = $"Stock actual: {stockActual} (linea ya guardada)"
                lblStock.ForeColor = Color.FromArgb(40, 180, 90)
            Else
                ' Linea nueva o modificada sin guardar: previsualizar como quedaria el stock al guardar.
                Dim stockRestante As Decimal = stockActual - cantidadEnLinea
                If stockRestante > 0 Then
                    lblStock.Text = $"Stock actual: {stockActual} (Te quedarán: {stockRestante})"
                    lblStock.ForeColor = Color.FromArgb(40, 180, 90)
                ElseIf stockRestante = 0 Then
                    lblStock.Text = $"Stock actual: {stockActual} (¡Atención! Te quedarás a 0)"
                    lblStock.ForeColor = Color.FromArgb(220, 160, 40)
                Else
                    lblStock.Text = $"¡Stock insuficiente! Actual: {stockActual} (Te faltan: {Math.Abs(stockRestante)})"
                    lblStock.ForeColor = Color.FromArgb(255, 80, 80)
                End If
            End If
            lblStock.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        Catch ex As Exception
            lblStock.Text = "Stock disponible: -"
        End Try
    End Sub

    Private Function ConsultarStock(idArticulo As Integer) As Decimal
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim sql As String = "SELECT StockActual FROM Articulos WHERE ID_Articulo = @id"
            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@id", idArticulo)
                Dim res = cmd.ExecuteScalar()
                If res IsNot Nothing AndAlso Not DBNull.Value.Equals(res) Then Return Convert.ToDecimal(res)
            End Using
        Catch
        End Try
        Return 0
    End Function

    Protected Overrides ReadOnly Property CreateParams As System.Windows.Forms.CreateParams
        Get
            Dim cp As System.Windows.Forms.CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or &H2000000
            Return cp
        End Get
    End Property

    Private Sub ActualizarStockYMovimiento(c As SQLiteConnection, trans As SQLiteTransaction, albaran As String, idArticulo As Integer, variacionSalida As Decimal, fecha As DateTime)
        If variacionSalida = 0 Then Return

        Dim stockActual As Decimal = 0
        Using cmdStock As New SQLiteCommand("SELECT StockActual FROM Articulos WHERE ID_Articulo = @id", c, trans)
            cmdStock.Parameters.AddWithValue("@id", idArticulo)
            Dim res = cmdStock.ExecuteScalar()
            If res IsNot Nothing AndAlso Not IsDBNull(res) Then stockActual = Convert.ToDecimal(res)
        End Using

        Dim nuevoStock As Decimal = stockActual - variacionSalida

        Using cmdUpd As New SQLiteCommand("UPDATE Articulos SET StockActual = @nuevo WHERE ID_Articulo = @id", c, trans)
            cmdUpd.Parameters.AddWithValue("@nuevo", nuevoStock)
            cmdUpd.Parameters.AddWithValue("@id", idArticulo)
            cmdUpd.ExecuteNonQuery()
        End Using

        Dim tipoMov As String = If(variacionSalida > 0, EstadosDocumento.MOV_SALIDA, EstadosDocumento.MOV_ENTRADA)
        Dim cantidadMov As Decimal = Math.Abs(variacionSalida)

        ' ID de usuario real desde la sesión (si no hay sesión válida, se guarda NULL).
        ' Antes estaba hardcodeado a 1, lo que destrozaba la auditoría.
        Dim idUsr As Object = If(ComunSesionActual.IdUsuario > 0, CType(ComunSesionActual.IdUsuario, Object), DBNull.Value)

        Dim sqlMov As String = "INSERT INTO MovimientosAlmacen (Fecha, ID_Articulo, TipoMovimiento, Cantidad, StockResultante, DocumentoReferencia, ID_Usuario) " &
                               "VALUES (@fecha, @idArt, @tipo, @cant, @stockRes, @doc, @usr)"
        Using cmdMov As New SQLiteCommand(sqlMov, c, trans)
            cmdMov.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd HH:mm:ss"))
            cmdMov.Parameters.AddWithValue("@idArt", idArticulo)
            cmdMov.Parameters.AddWithValue("@tipo", tipoMov)
            cmdMov.Parameters.AddWithValue("@cant", cantidadMov)
            cmdMov.Parameters.AddWithValue("@stockRes", nuevoStock)
            cmdMov.Parameters.AddWithValue("@doc", albaran)
            cmdMov.Parameters.AddWithValue("@usr", idUsr)
            cmdMov.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub btnImportarPedido_Click_1(sender As Object, e As EventArgs) Handles btnImportarPedido.Click
        Using frm As New FrmBuscador
            frm.TablaABuscar = "Pedidos"
            If frm.ShowDialog = DialogResult.OK Then
                Dim numPedido = frm.Resultado
                If Not String.IsNullOrEmpty(numPedido) Then ImportarDatosPedido(numPedido)
            End If
        End Using
    End Sub
    ' MOTOR DE EXPORTACIÓN A PDF (ALBARANES)
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        ' OJO: Cambia TextBoxAlbaran por el nombre real de tu TextBox del ID del albarán
        If String.IsNullOrWhiteSpace(TextBoxAlbaran.Text) Then
            MessageBox.Show("No hay ningún documento cargado para imprimir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        docImprimir.DefaultPageSettings.Landscape = False
        docImprimir.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docImprimir.DocumentName = "Albaran_" & TextBoxAlbaran.Text

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

            ' B) DATOS DEL DOCUMENTO (ALBARÁN)
            g.DrawString("ALBARÁN", fTituloDoc, bAzulCorporativo, margenDer, yPos, formatoDerecha)
            g.DrawString("Nº Documento: " & TextBoxAlbaran.Text, fCabecera, bNegro, margenDer, yPos + 40, formatoDerecha)
            g.DrawString("Fecha: " & TextBoxFecha.Text, fNormal, bGrisOscuro, margenDer, yPos + 60, formatoDerecha)

            ' Trazabilidad: De qué pedido viene
            If TextBoxPedidoOrigen IsNot Nothing AndAlso TextBoxPedidoOrigen.Text <> "" Then
                g.DrawString("Pedido Origen: " & TextBoxPedidoOrigen.Text, fNormal, bGrisOscuro, margenDer, yPos + 85, formatoDerecha)
            End If

            yPos += 110
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

        ' Ojo: En la cabecera he puesto "Entregado" en vez de "Cant."
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

            ' ¡CUIDADO AQUÍ! He cambiado la lectura a la columna "Entregado". 
            ' Asegúrate de que el Name interno de esa columna es "Entregado"
            Dim cant As String = If(row.Cells("CantidadServida").Value IsNot Nothing, row.Cells("CantidadServida").Value.ToString(), "0")
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

            ' Asegúrate de que las cajas de texto de los totales se llaman así
            Dim totalBase As String = TextBoxBase.Text.Replace("Base imponible :", "").Trim()
            Dim totalIVA As String = TextBoxIva.Text.Replace("I.V.A :", "").Trim()
            Dim totalDoc As String = TextBoxTotalAlb.Text.Replace("TOTAL :", "").Trim()

            Dim anchoCajaTotales As Integer = 300
            Dim xCaja As Integer = margenDer - anchoCajaTotales

            g.DrawString("Base Imponible:", fNormal, bGrisOscuro, xCaja, yPos)
            g.DrawString(totalBase, fNormal, bNegro, margenDer, yPos, formatoDerecha)
            yPos += 25

            g.DrawString("Impuestos (IVA):", fNormal, bGrisOscuro, xCaja, yPos)
            g.DrawString(totalIVA, fNormal, bNegro, margenDer, yPos, formatoDerecha)
            yPos += 30

            g.FillRectangle(bAzulCorporativo, xCaja, yPos, anchoCajaTotales, 45)

            Dim rectTxtTotal As New Rectangle(xCaja + 15, yPos, 150, 45)
            Dim rectValTotal As New Rectangle(xCaja + 150, yPos, anchoCajaTotales - 165, 45)

            g.DrawString("TOTAL A PAGAR", fCabecera, bBlanco, rectTxtTotal, formatoIzquierda)
            g.DrawString(totalDoc, fTotalGordo, bBlanco, rectValTotal, formatoDerecha)

            ' --- LOGÍSTICA Y ENVÍO (Abajo a la izquierda) ---
            Dim yNotas As Integer = yPos ' Usamos la misma altura donde empiezan los totales

            g.DrawString("DATOS DE ENVÍO:", fCabecera, bAzulCorporativo, margenIzq, yNotas)

            ' Montamos un bloque de texto con la info de transporte
            Dim datosEnvio As String = "Agencia: " & cboAgencias.Text & "  |  Portes: " & ComboBoxPortes.Text
            If TextBoxBultos.Text <> "" Or TextBoxPeso.Text <> "" Then
                datosEnvio &= vbCrLf & "Bultos: " & TextBoxBultos.Text & "  |  Peso: " & TextBoxPeso.Text & " kg"
            End If
            If TextBoxTracking.Text <> "" Then
                datosEnvio &= vbCrLf & "Tracking: " & TextBoxTracking.Text
            End If

            g.DrawString(datosEnvio, fNormal, bGrisOscuro, margenIzq, yNotas + 20)
            yNotas += 70

            ' --- OBSERVACIONES ---
            If TextBoxObservaciones.Text.Trim() <> "" Then
                g.DrawString("Observaciones:", fCabecera, bAzulCorporativo, margenIzq, yNotas)
                Dim rectObs As New Rectangle(margenIzq, yNotas + 20, anchoCajaTotales - 20, 50)
                g.DrawString(TextBoxObservaciones.Text, fNormal, bGrisOscuro, rectObs)
            End If

            ' --- CAJA DE FIRMA (Recibí conforme) ---
            ' Bajamos bastante para dejar espacio para que el cliente firme a bolígrafo
            Dim yFirma As Integer = yPos + 140

            ' Línea para firmar encima
            g.DrawLine(lapizFino, margenIzq, yFirma, margenIzq + 250, yFirma)

            ' Textos legales de recepción
            g.DrawString("Firma y Sello del Cliente (Recibí conforme)", fCabecera, bAzulCorporativo, margenIzq, yFirma + 5)
            g.DrawString("Fecha de recepción: ___ / ___ / 20__", fNormal, bGrisOscuro, margenIzq, yFirma + 25)

            e.HasMorePages = False
        End If
    End Sub
    ' LOGÍSTICA: COMBOS DEPENDIENTES (RUTAS Y AGENCIAS)

    ' LOGÍSTICA: COMBOS DEPENDIENTES (RUTAS Y AGENCIAS)

    ' 1. EL USUARIO ELIGE UNA RUTA -> Auto-seleccionamos la Agencia
    Private Sub cboRuta_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles cboRuta.SelectionChangeCommitted
        AutoAsignarAgenciaDesdeRuta()
    End Sub

    ' Si cboRuta tiene una ruta seleccionada, busca la agencia asociada en la tabla Rutas y la setea en cboAgencias. Llamar siempre que se asigne ruta programaticamente (imports, carga de documentos existentes) para mantener la pareja ruta-agencia coherente.
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

    ' 3. MÉTODO PARA FILTRAR EL DESPLEGABLE DE RUTAS
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
            ' Error silencioso
        End Try
    End Sub
End Class

Public Module GeneradorUtilidades
    Public Function GenerarCodigoSeguimiento() As String
        Dim letras As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
        Dim numeros As String = "0123456789"
        Dim rand As New Random()
        Dim resultado As New StringBuilder(11)

        For i As Integer = 1 To 7
            Dim indice As Integer = rand.Next(0, letras.Length)
            resultado.Append(letras(indice))
        Next
        For i As Integer = 1 To 4
            Dim indice As Integer = rand.Next(0, numeros.Length)
            resultado.Append(numeros(indice))
        Next

        Return resultado.ToString()
    End Function

    Public Function ObtenerAgencias() As List(Of Agencia)
        Dim lista As New List(Of Agencia)()
        Dim sql As String = "SELECT ID_Agencia, Nombre FROM Agencias ORDER BY Nombre ASC"
        Dim conn As SQLiteConnection = ConexionBD.GetConnection
        Try
            Using cmd As New SQLiteCommand(sql, conn)
                Using reader As SQLiteDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim ag As New Agencia()
                        ag.ID_Agencia = Convert.ToInt32(reader("ID_Agencia"))
                        ag.Nombre = reader("Nombre").ToString()
                        lista.Add(ag)
                    End While
                End Using
            End Using
        Catch ex As Exception
            MsgBox("Error al cargar agencias: " & ex.Message)
        End Try
        Return lista
    End Function

End Module

Public Class Agencia
    Public Property ID_Agencia As Integer
    Public Property Nombre As String
    Public Overrides Function ToString() As String
        Return Nombre
    End Function
End Class
