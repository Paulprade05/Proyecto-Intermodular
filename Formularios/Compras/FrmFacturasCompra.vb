Imports System.Data.SQLite

Public Class FrmFacturasCompra

#Region "1. Variables Globales y Propiedades"
    ' Clave principal TEXTO (Ej: FAC-001)
    Private _numeroFacturaActual As String = ""
    Private _dtLineas As DataTable
    Private _idsParaBorrar As New List(Of Integer)
    ' --- MOTOR DE IMPRESIÓN DEL DOCUMENTO ---
    Private WithEvents btnImprimir As New Button()
    Private WithEvents docImprimir As New Printing.PrintDocument()
    Private _filaActualImpresion As Integer = 0
    ' --- DESPLEGABLES DINÁMICOS ---
    Private WithEvents cboFormaPago As New ComboBox()
    Private lblFormaPago As New Label() With {.Text = "Forma de Pago", .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.WhiteSmoke}
    Private WithEvents cboEstado As New ComboBox()
#End Region

#Region "2. Eventos de Inicialización (Load)"
    Private Sub FrmFacturasCompra_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SuspendLayout()

        ConfigurarDiseñoResponsive()
        FrmPresupuestos.EstilizarGrid(DataGridView1)

        ' Combos
        Me.Controls.Add(lblFormaPago) : Me.Controls.Add(cboFormaPago)
        cboFormaPago.DropDownStyle = ComboBoxStyle.DropDownList
        CargarDesplegables()

        Me.Controls.Add(cboEstado)
        cboEstado.DropDownStyle = ComboBoxStyle.DropDownList
        cboEstado.Items.Clear()
        cboEstado.Items.AddRange(New String() {"Pendiente", "Pagada", "Vencida", "Disputada", "Cancelada"})

        ReorganizarControlesAutomaticamente()
        ConfigurarColumnasGrid()

        ' Vencimiento por defecto: hoy + 30 días
        DateTimePickerVencimiento.Value = DateTime.Now.AddDays(30)

        ' CONFIGURACIÓN DEL BOTÓN IMPRIMIR (Exportar PDF)
        btnImprimir.Text = "Exportar PDF"
        btnImprimir.Size = New Size(120, 35)
        btnImprimir.Location = New Point(ButtonGuardar.Location.X, ButtonGuardar.Location.Y + ButtonGuardar.Height + 10)
        btnImprimir.BackColor = Color.FromArgb(40, 140, 90)
        btnImprimir.ForeColor = Color.White
        btnImprimir.FlatStyle = FlatStyle.Flat
        btnImprimir.FlatAppearance.BorderSize = 0
        btnImprimir.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnImprimir.Cursor = Cursors.Hand
        Me.Controls.Add(btnImprimir)
        btnImprimir.BringToFront()

        Dim ultimoNum As String = ObtenerUltimoNumeroFactura()
        If Not String.IsNullOrEmpty(ultimoNum) Then
            CargarFactura(ultimoNum)
        Else
            LimpiarFormulario()
        End If
        Me.ResumeLayout(True)
    End Sub

    Private Sub CargarDesplegables()
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim daPago As New SQLiteDataAdapter("SELECT ID_FormaPago, Descripcion FROM FormasPago WHERE Activo=1", c)
            Dim dtPago As New DataTable() : daPago.Fill(dtPago)
            cboFormaPago.DataSource = dtPago : cboFormaPago.DisplayMember = "Descripcion" : cboFormaPago.ValueMember = "ID_FormaPago" : cboFormaPago.SelectedIndex = -1
        Catch ex As Exception
        End Try
    End Sub
#End Region

#Region "3. Configuración UI y Diseño"
    Private Sub ConfigurarDiseñoResponsive()
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Dim controlesTotales As Control() = {LabelBase, TextBoxBase, LabelIva, TextBoxIva, Label7, TextBoxTotalFac, LabelStock}
        For Each ctrl In controlesTotales : ctrl.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right : Next
        ButtonAnterior.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        ButtonSiguiente.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        Dim botonesIzq As Control() = {ButtonGuardar, ButtonBorrar, ButtonNuevoFac, ButtonBorrarLineas, ButtonNuevaLinea}
        For Each ctrl In botonesIzq : ctrl.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left : Next
        If LabelStock IsNot Nothing Then LabelStock.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
    End Sub

    Private Sub ConfigurarColumnasGrid()
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.Columns.Clear()

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Linea", .DataPropertyName = "ID_Linea", .Visible = False})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NumeroOrden", .DataPropertyName = "NumeroOrden", .HeaderText = "Nº", .Width = 40, .ReadOnly = True, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Articulo", .DataPropertyName = "ID_Articulo", .HeaderText = "ID Art", .Width = 60})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

        ' En factura de compra es Cantidad (no CantidadRecibida ni CantidadSolicitada)
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Cantidad", .DataPropertyName = "Cantidad", .HeaderText = "Cantidad", .Width = 75, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2", .BackColor = Color.Ivory}})
        ' La columna BD se llama PrecioCosteReal — el valor real facturado, que puede diferir del coste teórico
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PrecioCosteReal", .DataPropertyName = "PrecioCosteReal", .HeaderText = "Precio facturado", .Width = 120, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2"}})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descuento", .DataPropertyName = "Descuento", .HeaderText = "% Dto", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2"}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PorcentajeIVA", .DataPropertyName = "PorcentajeIVA", .HeaderText = "% IVA", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N0"}})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Total", .DataPropertyName = "Total", .HeaderText = "Total Base", .ReadOnly = True, .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.WhiteSmoke}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "TotalConIVA", .DataPropertyName = "TotalConIVA", .HeaderText = "Total (+IVA)", .ReadOnly = True, .Width = 100, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.FromArgb(230, 240, 250)}})
    End Sub
#End Region

#Region "4. Lógica de Negocio y Cálculos"
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
        If TextBoxTotalFac IsNot Nothing Then TextBoxTotalFac.Text = (base + sumaIva).ToString("C2")
    End Sub

    Private Sub LimpiarFormulario()
        _numeroFacturaActual = ""
        _idsParaBorrar.Clear()
        TextBoxFactura.Text = GenerarProximoNumeroFactura()

        TextBoxProveedor.Text = "" : TextBoxIdProveedor.Text = "" : TextBoxProveedor.Tag = Nothing
        TextBoxComprador.Text = "" : TextBoxIdComprador.Text = ""
        TextBoxObservaciones.Text = ""
        TextBoxAlbaranOrigen.Text = ""
        TextBoxFacProveedor.Text = ""
        TextBoxFecha.Text = DateTime.Now.ToShortDateString()
        DateTimePickerVencimiento.Value = DateTime.Now.AddDays(30)
        cboEstado.Text = "Pendiente"
        TextBoxBase.Text = "0,00 €" : TextBoxIva.Text = "0,00 €" : TextBoxTotalFac.Text = "0,00 €"

        _dtLineas = New DataTable()
        ConfigurarEstructuraDataTable()
        DataGridView1.DataSource = _dtLineas
        If cboFormaPago IsNot Nothing Then cboFormaPago.SelectedIndex = -1
        TextBoxIdProveedor.Focus()
    End Sub

    Private Sub ConfigurarEstructuraDataTable()
        With _dtLineas.Columns
            If Not .Contains("ID_Linea") Then .Add("ID_Linea", GetType(Object))
            If Not .Contains("ID_FacturaCompra") Then .Add("ID_FacturaCompra", GetType(Object))
            If Not .Contains("NumeroOrden") Then .Add("NumeroOrden", GetType(Integer))
            If Not .Contains("ID_Articulo") Then .Add("ID_Articulo", GetType(Object))
            If Not .Contains("Descripcion") Then .Add("Descripcion", GetType(String))
            If Not .Contains("Cantidad") Then .Add("Cantidad", GetType(Decimal))
            If Not .Contains("PrecioCosteReal") Then .Add("PrecioCosteReal", GetType(Decimal))
            If Not .Contains("Descuento") Then .Add("Descuento", GetType(Decimal))

            ' --- COLUMNAS IVA Y VIRTUALES ---
            If Not .Contains("PorcentajeIVA") Then .Add("PorcentajeIVA", GetType(Decimal))
            If Not .Contains("PrecioConIVA") Then .Add("PrecioConIVA", GetType(Decimal))
            If Not .Contains("Total") Then .Add("Total", GetType(Decimal))
            If Not .Contains("TotalConIVA") Then .Add("TotalConIVA", GetType(Decimal))
        End With
    End Sub

    Private Function GenerarProximoNumeroFactura() As String
        ' Delegamos en NumeradorDocumentos. CAMBIO IMPORTANTE: antes el prefijo era "FAC-",
        ' el mismo que las facturas de VENTA. Ahora usamos "FCO-" (Factura de COmpra) para
        ' distinguirlos claramente. Esto es coherente con "PEC-" (PEdido Compra) y
        ' "ALC-" (ALbarán Compra) que ya se usaban. Como en FacturasCompra hay 0 registros,
        ' este cambio no afecta a datos existentes.
        Return NumeradorDocumentos.SiguienteNumero("FCO-", "FacturasCompra", "NumeroFacturaCompra")
    End Function
#End Region

#Region "5. Persistencia (Base de Datos - CRUD)"
    Private Sub CargarFactura(numeroFactura As String)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' A. CARGAR CABECERA
            ' Tomamos el nombre del proveedor SIEMPRE de la tabla de proveedores (más actual),
            ' pero si quisiéramos histórico podríamos usar los campos congelados de la propia factura.
            Dim sqlCab As String = "SELECT F.*, Pr.NombreFiscal AS NombreProveedorActual, V.Nombre AS NombreComprador, " &
                                   "AC.NumeroAlbaranCompra AS NumAlbaranOrigen " &
                                   "FROM FacturasCompra F " &
                                   "LEFT JOIN Proveedores Pr ON F.ID_Proveedor = Pr.CodigoProveedor " &
                                   "LEFT JOIN Vendedores V ON F.ID_Comprador = V.ID_Vendedor " &
                                   "LEFT JOIN AlbaranesCompra AC ON F.ID_AlbaranCompra = AC.ID_AlbaranCompra " &
                                   "WHERE F.NumeroFacturaCompra = @num"

            Dim idFacturaInterno As Integer = 0

            Using cmd As New SQLiteCommand(sqlCab, c)
                cmd.Parameters.AddWithValue("@num", numeroFactura)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        _numeroFacturaActual = numeroFactura
                        idFacturaInterno = Convert.ToInt32(r("ID_FacturaCompra"))

                        TextBoxFactura.Text = r("NumeroFacturaCompra").ToString()
                        TextBoxFacProveedor.Text = If(IsDBNull(r("NumeroFacturaProveedor")), "", r("NumeroFacturaProveedor").ToString())
                        TextBoxFecha.Text = If(IsDBNull(r("FechaEmision")), "", Convert.ToDateTime(r("FechaEmision")).ToShortDateString())
                        If Not IsDBNull(r("FechaVencimiento")) Then DateTimePickerVencimiento.Value = Convert.ToDateTime(r("FechaVencimiento"))

                        TextBoxObservaciones.Text = If(IsDBNull(r("Observaciones")), "", r("Observaciones").ToString())
                        cboEstado.Text = If(IsDBNull(r("Estado")), "Pendiente", r("Estado").ToString())
                        TextBoxIdProveedor.Text = r("ID_Proveedor").ToString()

                        ' Preferimos el nombre fiscal CONGELADO en la factura. Si está vacío, caemos al actual del proveedor.
                        Dim nombreCongelado As String = If(IsDBNull(r("NombreFiscal")), "", r("NombreFiscal").ToString())
                        If String.IsNullOrWhiteSpace(nombreCongelado) Then
                            nombreCongelado = If(IsDBNull(r("NombreProveedorActual")), "", r("NombreProveedorActual").ToString())
                        End If
                        TextBoxProveedor.Text = nombreCongelado

                        TextBoxIdComprador.Text = If(IsDBNull(r("ID_Comprador")), "", r("ID_Comprador").ToString())
                        TextBoxComprador.Text = If(IsDBNull(r("NombreComprador")), "", r("NombreComprador").ToString())
                        TextBoxAlbaranOrigen.Text = If(IsDBNull(r("NumAlbaranOrigen")), "", r("NumAlbaranOrigen").ToString())

                        Dim idPago = r("ID_FormaPago")
                        If Not IsDBNull(idPago) Then cboFormaPago.SelectedValue = Convert.ToInt32(idPago) Else cboFormaPago.SelectedIndex = -1
                    Else
                        MessageBox.Show("Factura de compra no encontrada.") : Return
                    End If
                End Using
            End Using

            ' B. CARGAR LÍNEAS
            Dim sqlLin As String = "SELECT * FROM LineasFacturaCompra WHERE ID_FacturaCompra = @id ORDER BY NumeroOrden ASC"
            Using cmd As New SQLiteCommand(sqlLin, c)
                cmd.Parameters.AddWithValue("@id", idFacturaInterno)
                Dim da As New SQLiteDataAdapter(cmd)
                _dtLineas = New DataTable()
                da.Fill(_dtLineas)
                ConfigurarEstructuraDataTable()

                ' Recalcular IVA/totales al vuelo
                For Each row As DataRow In _dtLineas.Rows
                    Dim cant As Decimal = If(IsDBNull(row("Cantidad")), 0, Convert.ToDecimal(row("Cantidad")))
                    Dim prec As Decimal = If(IsDBNull(row("PrecioCosteReal")), 0, Convert.ToDecimal(row("PrecioCosteReal")))
                    Dim dto As Decimal = If(IsDBNull(row("Descuento")), 0, Convert.ToDecimal(row("Descuento")))
                    Dim iva As Decimal = If(IsDBNull(row("PorcentajeIVA")), 21, Convert.ToDecimal(row("PorcentajeIVA")))

                    row("PorcentajeIVA") = iva
                    row("PrecioConIVA") = prec * (1 + (iva / 100))

                    Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))
                    row("Total") = totalSinIva
                    row("TotalConIVA") = totalSinIva * (1 + (iva / 100))
                Next

                DataGridView1.DataSource = _dtLineas
                CalcularTotalesGenerales()
            End Using

        Catch ex As Exception
            MessageBox.Show("Error al cargar: " & ex.Message)
        End Try
    End Sub

    Private Sub GuardarFactura()
        If String.IsNullOrWhiteSpace(TextBoxIdProveedor.Text) Then MessageBox.Show("Falta el Proveedor") : Return

        Dim esNuevo As Boolean = String.IsNullOrEmpty(_numeroFacturaActual)
        If esNuevo Then
            TextBoxFactura.Text = GenerarProximoNumeroFactura()
            _numeroFacturaActual = TextBoxFactura.Text
        End If

        ' NumeroFacturaProveedor es NOT NULL en BD: si el usuario no lo rellenó, usamos el nuestro como fallback
        Dim numFacProv As String = If(String.IsNullOrWhiteSpace(TextBoxFacProveedor.Text), _numeroFacturaActual, TextBoxFacProveedor.Text.Trim())

        ' ID del albarán origen (opcional)
        Dim idAlbOrigen As Object = DBNull.Value
        If Not String.IsNullOrWhiteSpace(TextBoxAlbaranOrigen.Text) Then
            Dim resolved = ResolverIdAlbaranCompra(TextBoxAlbaranOrigen.Text.Trim())
            If resolved IsNot Nothing Then idAlbOrigen = resolved
        End If

        ' Datos congelados del proveedor (para conservar histórico fiscal)
        Dim provNombreFiscal As String = ""
        Dim provCIF As String = ""
        Dim provDireccion As String = ""
        Dim provPoblacion As String = ""
        Dim provCP As String = ""
        Try
            Dim cTmp = ConexionBD.GetConnection()
            If cTmp.State <> ConnectionState.Open Then cTmp.Open()
            Using cmdP As New SQLiteCommand("SELECT NombreFiscal, CIF, Direccion, Poblacion, CodigoPostal FROM Proveedores WHERE CodigoProveedor=@id", cTmp)
                cmdP.Parameters.AddWithValue("@id", TextBoxIdProveedor.Text.Trim())
                Using rP = cmdP.ExecuteReader()
                    If rP.Read() Then
                        provNombreFiscal = If(IsDBNull(rP("NombreFiscal")), "", rP("NombreFiscal").ToString())
                        provCIF = If(IsDBNull(rP("CIF")), "", rP("CIF").ToString())
                        provDireccion = If(IsDBNull(rP("Direccion")), "", rP("Direccion").ToString())
                        provPoblacion = If(IsDBNull(rP("Poblacion")), "", rP("Poblacion").ToString())
                        provCP = If(IsDBNull(rP("CodigoPostal")), "", rP("CodigoPostal").ToString())
                    End If
                End Using
            End Using
        Catch
        End Try

        Dim c = ConexionBD.GetConnection()
        Dim trans As SQLiteTransaction = Nothing
        Try
            If c.State <> ConnectionState.Open Then c.Open()
            trans = c.BeginTransaction()

            ' 0. Calcular sumas para la cabecera
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

            ' 1. Guardar Cabecera 
            Dim idFormaPago As Object = If(cboFormaPago.SelectedValue IsNot Nothing AndAlso cboFormaPago.SelectedIndex <> -1, cboFormaPago.SelectedValue, DBNull.Value)
            Dim idComp As Object = If(IsNumeric(TextBoxIdComprador.Text) AndAlso Val(TextBoxIdComprador.Text) > 0, Convert.ToInt32(TextBoxIdComprador.Text), DBNull.Value)

            ' Pagada se deriva del estado
            Dim pagada As Integer = If(cboEstado.Text.Trim().Equals("Pagada", StringComparison.OrdinalIgnoreCase), 1, 0)

            Dim idFacturaInterno As Long = 0
            Dim sql As String = ""

            If esNuevo Then
                sql = "INSERT INTO FacturasCompra (NumeroFacturaCompra, NumeroFacturaProveedor, ID_Proveedor, FechaEmision, FechaVencimiento, Observaciones, Estado, ID_FormaPago, ID_Comprador, ID_AlbaranCompra, BaseImponible, ImporteIVA, TotalPagar, Pagada, NombreFiscal, CIF, Direccion, Poblacion, CodigoPostal) " &
                      "VALUES (@num, @numProv, @prov, @fecha, @fechaVenc, @obs, @est, @formaPago, @comp, @idAlb, @base, @iva, @total, @pagada, @nf, @cif, @dir, @pob, @cp); SELECT last_insert_rowid();"
            Else
                sql = "UPDATE FacturasCompra SET NumeroFacturaProveedor=@numProv, ID_Proveedor=@prov, FechaEmision=@fecha, FechaVencimiento=@fechaVenc, Observaciones=@obs, Estado=@est, ID_FormaPago=@formaPago, ID_Comprador=@comp, ID_AlbaranCompra=@idAlb, BaseImponible=@base, ImporteIVA=@iva, TotalPagar=@total, Pagada=@pagada, NombreFiscal=@nf, CIF=@cif, Direccion=@dir, Poblacion=@pob, CodigoPostal=@cp " &
                      "WHERE NumeroFacturaCompra = @num"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Transaction = trans
                cmd.Parameters.AddWithValue("@num", _numeroFacturaActual)
                cmd.Parameters.AddWithValue("@numProv", numFacProv)
                cmd.Parameters.AddWithValue("@prov", TextBoxIdProveedor.Text.Trim())
                cmd.Parameters.AddWithValue("@comp", idComp)
                cmd.Parameters.AddWithValue("@idAlb", idAlbOrigen)

                ' --- FECHAS ---
                Dim fecha As DateTime
                If DateTime.TryParse(TextBoxFecha.Text, fecha) Then
                    cmd.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd HH:mm:ss"))
                Else
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                End If

                cmd.Parameters.AddWithValue("@fechaVenc", DateTimePickerVencimiento.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                cmd.Parameters.AddWithValue("@obs", TextBoxObservaciones.Text.Trim())
                cmd.Parameters.AddWithValue("@est", cboEstado.Text.Trim())
                cmd.Parameters.AddWithValue("@formaPago", idFormaPago)

                cmd.Parameters.AddWithValue("@base", sumaBase)
                cmd.Parameters.AddWithValue("@iva", sumaIva)
                cmd.Parameters.AddWithValue("@total", sumaBase + sumaIva)
                cmd.Parameters.AddWithValue("@pagada", pagada)

                ' Datos congelados del proveedor
                cmd.Parameters.AddWithValue("@nf", provNombreFiscal)
                cmd.Parameters.AddWithValue("@cif", provCIF)
                cmd.Parameters.AddWithValue("@dir", provDireccion)
                cmd.Parameters.AddWithValue("@pob", provPoblacion)
                cmd.Parameters.AddWithValue("@cp", provCP)

                If esNuevo Then
                    idFacturaInterno = Convert.ToInt64(cmd.ExecuteScalar())
                Else
                    cmd.ExecuteNonQuery()
                End If
            End Using

            ' Si es edición, recuperamos el ID interno
            If Not esNuevo Then
                Using cmdId As New SQLiteCommand("SELECT ID_FacturaCompra FROM FacturasCompra WHERE NumeroFacturaCompra = @num", c)
                    cmdId.Transaction = trans
                    cmdId.Parameters.AddWithValue("@num", _numeroFacturaActual)
                    idFacturaInterno = Convert.ToInt64(cmdId.ExecuteScalar())
                End Using
            End If

            ' 2. Borrar líneas eliminadas
            For Each idDel In _idsParaBorrar
                Using cmdDel As New SQLiteCommand("DELETE FROM LineasFacturaCompra WHERE ID_LineaFactura = @id", c)
                    cmdDel.Transaction = trans : cmdDel.Parameters.AddWithValue("@id", idDel) : cmdDel.ExecuteNonQuery()
                End Using
            Next
            _idsParaBorrar.Clear()

            ' 3. Guardar Líneas
            Dim orden As Integer = 1
            For Each row As DataRow In _dtLineas.Rows
                If row.RowState = DataRowState.Deleted Then Continue For

                Dim idLin = row("ID_Linea")
                Dim idArt As Object = If(IsNumeric(row("ID_Articulo")) AndAlso Val(row("ID_Articulo")) > 0, Convert.ToInt32(row("ID_Articulo")), DBNull.Value)
                Dim sqlLine As String = ""

                If IsDBNull(idLin) OrElse Not IsNumeric(idLin) OrElse Val(idLin) = 0 Then
                    sqlLine = "INSERT INTO LineasFacturaCompra (ID_FacturaCompra, NumeroOrden, ID_Articulo, Descripcion, Cantidad, PrecioCosteReal, Descuento, PorcentajeIVA, Total) " &
                              "VALUES (@idFac, @ord, @art, @desc, @cant, @prec, @dcto, @iva, @tot)"
                Else
                    sqlLine = "UPDATE LineasFacturaCompra SET NumeroOrden=@ord, ID_Articulo=@art, Descripcion=@desc, Cantidad=@cant, PrecioCosteReal=@prec, Descuento=@dcto, PorcentajeIVA=@iva, Total=@tot WHERE ID_LineaFactura=@id"
                End If

                Using cmdL As New SQLiteCommand(sqlLine, c)
                    cmdL.Transaction = trans
                    cmdL.Parameters.AddWithValue("@idFac", idFacturaInterno)
                    cmdL.Parameters.AddWithValue("@ord", orden)
                    cmdL.Parameters.AddWithValue("@art", idArt)
                    cmdL.Parameters.AddWithValue("@desc", If(IsDBNull(row("Descripcion")), "", row("Descripcion")))
                    cmdL.Parameters.AddWithValue("@cant", row("Cantidad"))
                    cmdL.Parameters.AddWithValue("@prec", row("PrecioCosteReal"))
                    cmdL.Parameters.AddWithValue("@dcto", row("Descuento"))
                    cmdL.Parameters.AddWithValue("@iva", If(IsDBNull(row("PorcentajeIVA")), 21, row("PorcentajeIVA")))
                    cmdL.Parameters.AddWithValue("@tot", row("Total"))
                    If sqlLine.Contains("WHERE") Then cmdL.Parameters.AddWithValue("@id", idLin)
                    cmdL.ExecuteNonQuery()
                End Using
                orden += 1
            Next

            ' MAGIA DE ESTADOS (COMPRAS): Marcar AlbaranCompra como Facturado
            ' Igual que en ventas, si esta factura está vinculada a un albarán de compra
            ' lo marcamos automáticamente como "Facturado".
            If Not IsDBNull(idAlbOrigen) Then
                Using cmdEst As New SQLiteCommand("UPDATE AlbaranesCompra SET Estado = @estado WHERE ID_AlbaranCompra = @idAlb", c)
                    cmdEst.Transaction = trans
                    cmdEst.Parameters.AddWithValue("@estado", EstadosDocumento.ALBARAN_COMPRA_FACTURADO)
                    cmdEst.Parameters.AddWithValue("@idAlb", idAlbOrigen)
                    cmdEst.ExecuteNonQuery()
                End Using
            End If

            trans.Commit()
            MessageBox.Show("Guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CargarFactura(_numeroFacturaActual)

        Catch ex As Exception
            If trans IsNot Nothing Then trans.Rollback()
            LogErrores.Registrar("FrmFacturasCompra.Guardar", ex)
            MessageBox.Show("Error al guardar: " & ex.Message)
        End Try
    End Sub

    Private Function ObtenerUltimoNumeroFactura() As String
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim cmd As New SQLiteCommand("SELECT MAX(NumeroFacturaCompra) FROM FacturasCompra", c)
            Dim r = cmd.ExecuteScalar()
            Return If(r Is Nothing OrElse IsDBNull(r), "", r.ToString())
        Catch
            Return ""
        End Try
    End Function

    ' Convierte un Nº de albarán (ALC-XXX) a su ID interno
    Private Function ResolverIdAlbaranCompra(numeroAlbaran As String) As Object
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Using cmd As New SQLiteCommand("SELECT ID_AlbaranCompra FROM AlbaranesCompra WHERE NumeroAlbaranCompra = @num", c)
                cmd.Parameters.AddWithValue("@num", numeroAlbaran)
                Dim r = cmd.ExecuteScalar()
                If r IsNot Nothing AndAlso Not IsDBNull(r) Then Return Convert.ToInt64(r)
            End Using
        Catch
        End Try
        Return Nothing
    End Function
#End Region

#Region "6. Importación desde Albarán de Compra"
    Private Sub ImportarDatosAlbaran(numeroAlbaran As String)
        Dim c = ConexionBD.GetConnection()
        Try
            If c.State <> ConnectionState.Open Then c.Open()

            ' Cabecera del albarán
            Dim sqlCab As String = "SELECT A.*, Pr.NombreFiscal, V.Nombre AS NombreComp " &
                                   "FROM AlbaranesCompra A " &
                                   "LEFT JOIN Proveedores Pr ON A.ID_Proveedor = Pr.CodigoProveedor " &
                                   "LEFT JOIN Vendedores V ON A.ID_Comprador = V.ID_Vendedor " &
                                   "WHERE A.NumeroAlbaranCompra = @num"

            Dim idAlbaran As Long = 0

            Using cmd As New SQLiteCommand(sqlCab, c)
                cmd.Parameters.AddWithValue("@num", numeroAlbaran)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        LimpiarFormulario()

                        ' Referencia al albarán
                        TextBoxAlbaranOrigen.Text = numeroAlbaran
                        idAlbaran = Convert.ToInt64(r("ID_AlbaranCompra"))

                        ' Proveedor
                        TextBoxIdProveedor.Text = r("ID_Proveedor").ToString()
                        TextBoxProveedor.Text = If(IsDBNull(r("NombreFiscal")), "", r("NombreFiscal").ToString())

                        ' Comprador (heredado del albarán si existe)
                        TextBoxIdComprador.Text = If(IsDBNull(r("ID_Comprador")), "", r("ID_Comprador").ToString())
                        TextBoxComprador.Text = If(IsDBNull(r("NombreComp")), "", r("NombreComp").ToString())

                        ' Forma de pago heredada
                        Dim idPago = r("ID_FormaPago")
                        If Not IsDBNull(idPago) Then cboFormaPago.SelectedValue = Convert.ToInt32(idPago)

                        TextBoxObservaciones.Text = "Factura del albarán " & numeroAlbaran
                    Else
                        MessageBox.Show("Albarán no encontrado.")
                        Return
                    End If
                End Using
            End Using

            ' Líneas del albarán
            Dim sqlLin As String = "SELECT * FROM LineasAlbaranCompra WHERE ID_AlbaranCompra = @id ORDER BY NumeroOrden ASC"
            Dim dtOrigen As New DataTable()
            Using cmd As New SQLiteCommand(sqlLin, c)
                cmd.Parameters.AddWithValue("@id", idAlbaran)
                Dim da As New SQLiteDataAdapter(cmd)
                da.Fill(dtOrigen)
            End Using

            _dtLineas.Rows.Clear()
            For Each rowOrig As DataRow In dtOrigen.Rows
                Dim rowNew As DataRow = _dtLineas.NewRow()

                rowNew("ID_Linea") = DBNull.Value
                rowNew("NumeroOrden") = rowOrig("NumeroOrden")
                rowNew("ID_Articulo") = rowOrig("ID_Articulo")
                rowNew("Descripcion") = rowOrig("Descripcion")

                ' Cantidad facturada = cantidad recibida en albarán
                Dim cant As Decimal = 0 : Decimal.TryParse(rowOrig("CantidadRecibida").ToString(), cant)
                rowNew("Cantidad") = cant

                ' Precio = el del albarán (puede ajustarse manualmente si el proveedor factura distinto)
                Dim prec As Decimal = 0 : Decimal.TryParse(rowOrig("PrecioUnitario").ToString(), prec)
                rowNew("PrecioCosteReal") = prec
                rowNew("Descuento") = rowOrig("Descuento")

                Dim iva As Decimal = 21
                If dtOrigen.Columns.Contains("PorcentajeIVA") AndAlso Not IsDBNull(rowOrig("PorcentajeIVA")) Then
                    Decimal.TryParse(rowOrig("PorcentajeIVA").ToString(), iva)
                End If
                rowNew("PorcentajeIVA") = iva

                Dim dto As Decimal = 0 : Decimal.TryParse(rowOrig("Descuento").ToString(), dto)
                Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))

                rowNew("Total") = totalSinIva
                rowNew("TotalConIVA") = totalSinIva * (1 + (iva / 100))
                rowNew("PrecioConIVA") = prec * (1 + (iva / 100))

                _dtLineas.Rows.Add(rowNew)
            Next

            DataGridView1.DataSource = _dtLineas
            CalcularTotalesGenerales()
            MessageBox.Show("Albarán importado. Revisa los precios facturados antes de guardar.", "Importación correcta", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error al importar el albarán: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "7. Manejadores de Eventos (Botones y Grid)"

    Private Sub ButtonGuardar_Click(sender As Object, e As EventArgs) Handles ButtonGuardar.Click
        GuardarFactura()
    End Sub

    Private Sub ButtonNuevoFac_Click(sender As Object, e As EventArgs) Handles ButtonNuevoFac.Click
        LimpiarFormulario()
    End Sub

    Private Sub ButtonBorrar_Click(sender As Object, e As EventArgs) Handles ButtonBorrar.Click
        ' Solo administradores pueden borrar facturas (impacto fiscal).
        If Not ComunSesionActual.EsAdministrador() Then
            MessageBox.Show("Solo un Administrador puede eliminar facturas.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If String.IsNullOrEmpty(_numeroFacturaActual) Then Return
        If MessageBox.Show("¿Seguro que deseas eliminar esta factura de compra?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                ' Obtenemos el id interno
                Dim idInterno As Integer = 0
                Using cmdId As New SQLiteCommand("SELECT ID_FacturaCompra FROM FacturasCompra WHERE NumeroFacturaCompra=@num", c)
                    cmdId.Parameters.AddWithValue("@num", _numeroFacturaActual)
                    Dim rid = cmdId.ExecuteScalar()
                    If rid IsNot Nothing AndAlso Not IsDBNull(rid) Then idInterno = Convert.ToInt32(rid)
                End Using

                If idInterno > 0 Then
                    Using cmd As New SQLiteCommand("DELETE FROM LineasFacturaCompra WHERE ID_FacturaCompra=@id; DELETE FROM FacturasCompra WHERE ID_FacturaCompra=@id;", c)
                        cmd.Parameters.AddWithValue("@id", idInterno)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
                LimpiarFormulario()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub

    Private Sub btnBuscarFactura_Click(sender As Object, e As EventArgs) Handles btnBuscarFactura.Click
        Dim frmBuscar As New FrmBuscador()
        frmBuscar.TablaABuscar = "FacturasCompra"
        If frmBuscar.ShowDialog() = DialogResult.OK Then
            CargarFactura(frmBuscar.Resultado)
        End If
    End Sub

    Private Sub btnImportarAlbaran_Click(sender As Object, e As EventArgs) Handles btnImportarAlbaran.Click
        ' Si hay un nº de albarán escrito directamente, lo usamos
        If Not String.IsNullOrWhiteSpace(TextBoxAlbaranOrigen.Text) Then
            Dim resp = MessageBox.Show($"¿Importar las líneas del albarán '{TextBoxAlbaranOrigen.Text}'?{vbCrLf}Esto reemplazará los datos actuales del formulario.", "Confirmar importación", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If resp = DialogResult.Yes Then ImportarDatosAlbaran(TextBoxAlbaranOrigen.Text.Trim())
            Return
        End If

        ' Si no, abrimos el buscador de albaranes
        Dim frmBuscar As New FrmBuscador()
        frmBuscar.TablaABuscar = "AlbaranesCompra"
        If frmBuscar.ShowDialog() = DialogResult.OK Then
            ImportarDatosAlbaran(frmBuscar.Resultado)
        End If
    End Sub

    Private Sub Navegar(direccion As String)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim sql As String = If(direccion = "ANT",
                                   "SELECT MAX(NumeroFacturaCompra) FROM FacturasCompra WHERE NumeroFacturaCompra < @act",
                                   "SELECT MIN(NumeroFacturaCompra) FROM FacturasCompra WHERE NumeroFacturaCompra > @act")

            If String.IsNullOrEmpty(_numeroFacturaActual) And direccion = "ANT" Then
                sql = "SELECT MAX(NumeroFacturaCompra) FROM FacturasCompra"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@act", _numeroFacturaActual)
                Dim res = cmd.ExecuteScalar()
                If res IsNot Nothing AndAlso Not IsDBNull(res) Then CargarFactura(res.ToString())
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub ButtonAnterior_Click(sender As Object, e As EventArgs) Handles ButtonAnterior.Click
        Navegar("ANT")
    End Sub

    Private Sub ButtonSiguiente_Click(sender As Object, e As EventArgs) Handles ButtonSiguiente.Click
        Navegar("SIG")
    End Sub

    Private Sub ButtonNuevaLinea_Click(sender As Object, e As EventArgs) Handles ButtonNuevaLinea.Click
        If _dtLineas Is Nothing Then
            _dtLineas = New DataTable()
            ConfigurarEstructuraDataTable()
        End If

        Dim fila = _dtLineas.NewRow()
        fila("NumeroOrden") = _dtLineas.Rows.Count + 1
        fila("Cantidad") = 1
        fila("PrecioCosteReal") = 0
        fila("Descuento") = 0
        fila("PorcentajeIVA") = 21
        fila("Total") = 0
        _dtLineas.Rows.Add(fila)

        If DataGridView1.Rows.Count > 0 Then
            DataGridView1.CurrentCell = DataGridView1.Rows(DataGridView1.Rows.Count - 1).Cells("ID_Articulo")
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

    ' EL FRENO MÁGICO (evitar relookup si el ID no cambió)
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

        ' A. Artículo
        If colName = "ID_Articulo" Then
            Dim idArt As String = fila.Cells("ID_Articulo").Value?.ToString()
            Dim idAntiguo As String = fila.Cells("ID_Articulo").Tag?.ToString()

            If idArt = idAntiguo Then Return

            If String.IsNullOrWhiteSpace(idArt) Then
                fila.Cells("Descripcion").Value = ""
                fila.Cells("PrecioCosteReal").Value = 0
                fila.Cells("PorcentajeIVA").Value = 21
                fila.Tag = Nothing
                Return
            End If

            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()

                ' En compras usamos PrecioCoste como precio inicial sugerido (el usuario podrá ajustarlo)
                Dim sql As String = "SELECT Descripcion, PrecioCoste, StockActual, TipoIVA FROM Articulos WHERE ID_Articulo = @id"
                Using cmd As New SQLiteCommand(sql, c)
                    cmd.Parameters.AddWithValue("@id", idArt)
                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            fila.Cells("Descripcion").Value = r("Descripcion").ToString()
                            fila.Cells("PrecioCosteReal").Value = If(IsDBNull(r("PrecioCoste")), 0, Convert.ToDecimal(r("PrecioCoste")))
                            fila.Cells("PorcentajeIVA").Value = If(IsDBNull(r("TipoIVA")), 21, Convert.ToDecimal(r("TipoIVA")))
                            If Not IsDBNull(r("StockActual")) Then fila.Tag = Convert.ToDecimal(r("StockActual"))
                            If IsDBNull(fila.Cells("Cantidad").Value) OrElse Val(fila.Cells("Cantidad").Value) = 0 Then fila.Cells("Cantidad").Value = 1
                            fila.Cells("ID_Articulo").Tag = idArt
                        Else
                            MessageBox.Show("Artículo no encontrado", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If
                    End Using
                End Using
            Catch ex As Exception
            End Try
        End If

        ' B. Cálculos
        If colName = "Cantidad" Or colName = "PrecioCosteReal" Or colName = "Descuento" Or colName = "PorcentajeIVA" Then
            Dim cant As Decimal = 0, prec As Decimal = 0, dto As Decimal = 0, iva As Decimal = 21
            Decimal.TryParse(fila.Cells("Cantidad").Value?.ToString(), cant)
            Decimal.TryParse(fila.Cells("PrecioCosteReal").Value?.ToString(), prec)
            Decimal.TryParse(fila.Cells("Descuento").Value?.ToString(), dto)
            Decimal.TryParse(fila.Cells("PorcentajeIVA").Value?.ToString(), iva)

            Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))
            Dim totalConIva As Decimal = totalSinIva * (1 + (iva / 100))

            fila.Cells("Total").Value = totalSinIva
            fila.Cells("TotalConIVA").Value = totalConIva

            CalcularTotalesGenerales()
        End If
    End Sub

    ' INFO DE STOCK EN TIEMPO REAL
    Private Sub DataGridView1_SelectionChanged(sender As Object, e As EventArgs) Handles DataGridView1.SelectionChanged
        If LabelStock Is Nothing Then Return
        If DataGridView1.CurrentRow Is Nothing OrElse DataGridView1.CurrentRow.IsNewRow Then
            LabelStock.Text = "Stock disponible: -"
            LabelStock.ForeColor = Color.FromArgb(170, 175, 180)
            Return
        End If

        Try
            Dim idCelda As Object = DataGridView1.CurrentRow.Cells("ID_Articulo").Value
            If idCelda IsNot Nothing AndAlso Not DBNull.Value.Equals(idCelda) Then
                Dim idArticulo As Integer = Convert.ToInt32(idCelda)
                Dim stockActual As Decimal = ConsultarStock(idArticulo)
                LabelStock.Text = $"Stock actual: {stockActual}"
                LabelStock.ForeColor = Color.FromArgb(40, 180, 90)
                LabelStock.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
            End If
        Catch ex As Exception
            LabelStock.Text = "Stock disponible: -"
        End Try
    End Sub

    Private Function ConsultarStock(idArticulo As Integer) As Decimal
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim cmd As New SQLiteCommand("SELECT StockActual FROM Articulos WHERE ID_Articulo = @id", c)
            cmd.Parameters.AddWithValue("@id", idArticulo)
            Dim resultado As Object = cmd.ExecuteScalar()
            If resultado IsNot Nothing AndAlso Not DBNull.Value.Equals(resultado) Then Return Convert.ToDecimal(resultado)
        Catch
        End Try
        Return 0
    End Function

    Private Sub DataGridView1_RowsRemoved(sender As Object, e As DataGridViewRowsRemovedEventArgs) Handles DataGridView1.RowsRemoved
        CalcularTotalesGenerales()
    End Sub

    Private Sub TextBoxIdProveedor_Leave(sender As Object, e As EventArgs) Handles TextBoxIdProveedor.Leave
        If String.IsNullOrWhiteSpace(TextBoxIdProveedor.Text) Then
            TextBoxProveedor.Text = ""
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim cmd As New SQLiteCommand("SELECT NombreFiscal FROM Proveedores WHERE CodigoProveedor=@id", c)
            cmd.Parameters.AddWithValue("@id", TextBoxIdProveedor.Text.Trim())

            Using r = cmd.ExecuteReader()
                If r.Read() Then
                    TextBoxProveedor.Text = r("NombreFiscal").ToString()
                Else
                    Dim respuesta As DialogResult = MessageBox.Show(
                        "El código de proveedor '" & TextBoxIdProveedor.Text & "' no existe." & vbCrLf & vbCrLf &
                        "¿Deseas crear esta nueva ficha de proveedor ahora?",
                        "Proveedor no encontrado",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question)

                    If respuesta = DialogResult.Yes Then
                        Dim codigoFaltante As String = TextBoxIdProveedor.Text.Trim()
                        TextBoxIdProveedor.Text = ""
                        TextBoxProveedor.Text = ""

                        Dim frm As New FrmProveedorDetalle()
                        frm.CodigoProveedorSeleccionado = codigoFaltante
                        frm.ShowDialog()
                        TextBoxIdProveedor.Focus()
                    Else
                        TextBoxIdProveedor.Text = ""
                        TextBoxProveedor.Text = ""
                        TextBoxIdProveedor.Focus()
                    End If
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al buscar proveedor: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TextBoxIdComprador_Leave(sender As Object, e As EventArgs) Handles TextBoxIdComprador.Leave
        If String.IsNullOrWhiteSpace(TextBoxIdComprador.Text) Then TextBoxComprador.Text = "" : Return
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim cmd As New SQLiteCommand("SELECT Nombre FROM Vendedores WHERE ID_Vendedor=@id", c)
            cmd.Parameters.AddWithValue("@id", Val(TextBoxIdComprador.Text))
            Dim r = cmd.ExecuteScalar()
            TextBoxComprador.Text = If(r IsNot Nothing, r.ToString(), "NO EXISTE")
        Catch
        End Try
    End Sub
#End Region

#Region "Auto-Organización Visual (Diseño Modernizado)"
    ' --- PALETA DE COLORES CENTRALIZADA ---
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

    Private Sub ReorganizarControlesAutomaticamente()
        For Each ctrl As Control In Me.Controls : ctrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left : Next

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
        lblTitulo.Text = "FACTURA DE COMPRA"
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
        TextBoxFactura.Bounds = New Rectangle(margenIzq + 410, 28, 130, 28)
        TextBoxFactura.BackColor = COLOR_PANEL_TOTALES
        TextBoxFactura.ForeColor = COLOR_ACENTO
        TextBoxFactura.Font = New Font("Segoe UI", 11.5F, FontStyle.Bold)
        TextBoxFactura.BorderStyle = BorderStyle.FixedSingle
        TextBoxFactura.TextAlign = HorizontalAlignment.Center
        TextBoxFactura.BringToFront()

        ' Lupa pegada al TextBoxFactura
        btnBuscarFactura.Bounds = New Rectangle(margenIzq + 545, 28, 30, 28)
        btnBuscarFactura.BackColor = BTN_AZUL_PRIMARIO
        btnBuscarFactura.ForeColor = Color.White
        btnBuscarFactura.FlatStyle = FlatStyle.Flat
        btnBuscarFactura.FlatAppearance.BorderSize = 0
        btnBuscarFactura.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        btnBuscarFactura.Cursor = Cursors.Hand
        btnBuscarFactura.Text = "🔍"
        btnBuscarFactura.BringToFront()

        ' Ocultar etiqueta "Factura" suelta — la sustituye el título
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Label AndAlso ctrl.Name <> "LblTituloDoc" AndAlso ctrl.Name <> "LblNumDocEtiqueta" _
               AndAlso ctrl.Name <> "LineaTotales" AndAlso ctrl.Name <> "PanelTotalesResumen" _
               AndAlso ctrl.Name <> "LineaDivisoria" Then
                Dim texto As String = ctrl.Text.Trim().ToLower()
                If texto = "factura" Then ctrl.Visible = False
            End If
        Next

        ' 2. ZONA DE FORMULARIO (idéntica a Albaranes Compra)
        Dim yInicioFormulario As Integer = alturaBanda + 25

        Dim col1_X As Integer = margenIzq                  ' Bloque 1 - Proveedor / Comprador
        Dim col2_X As Integer = 600                        ' Bloque 2 - Fechas / Estado / Observaciones
        Dim col3_X As Integer = col2_X + 460               ' Bloque 3 - Trazabilidad

        Dim yFila1 As Integer = yInicioFormulario
        Dim yFila2 As Integer = yInicioFormulario + 22
        Dim yFila3 As Integer = yInicioFormulario + 60
        Dim yFila4 As Integer = yInicioFormulario + 82
        Dim yFila5 As Integer = yInicioFormulario + 120
        Dim yFila6 As Integer = yInicioFormulario + 142

        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Label AndAlso ctrl.Name <> "LineaTotales" AndAlso ctrl.Name <> "PanelTotalesResumen" _
               AndAlso ctrl.Name <> "LblTituloDoc" AndAlso ctrl.Name <> "LblNumDocEtiqueta" _
               AndAlso ctrl.Name <> "LineaDivisoria" Then
                ctrl.BackColor = Color.Transparent
                ctrl.ForeColor = Color.WhiteSmoke
                ctrl.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
                ctrl.BringToFront()
                Dim texto As String = ctrl.Text.Trim().ToLower()
                Select Case texto
                    Case "proveedor" : ctrl.Location = New Point(col1_X, yFila1)
                    Case "fecha emisión" : ctrl.Location = New Point(col2_X, yFila1)
                    Case "fecha vencimiento" : ctrl.Location = New Point(col2_X + 160, yFila1)
                    Case "comprador" : ctrl.Location = New Point(col1_X, yFila3)
                    Case "estado" : ctrl.Location = New Point(col2_X, yFila3)
                    Case "observaciones" : ctrl.Location = New Point(col2_X + 160, yFila3)
                    Case "nº fac. proveedor" : ctrl.Location = New Point(col3_X, yFila1)
                    Case "albarán" : ctrl.Location = New Point(col3_X, yFila3)
                End Select
            End If
        Next

        ' --- Bloque 1: Proveedor y Comprador ---
        TextBoxIdProveedor.Bounds = New Rectangle(col1_X, yFila2, 60, 25)
        TextBoxProveedor.Bounds = New Rectangle(col1_X + 65, yFila2, 460, 25)
        TextBoxIdComprador.Bounds = New Rectangle(col1_X, yFila4, 50, 25)
        TextBoxComprador.Bounds = New Rectangle(col1_X + 55, yFila4, 470, 25)

        ' --- Bloque 2: Fechas y Estado ---
        TextBoxFecha.Bounds = New Rectangle(col2_X, yFila2, 140, 25)
        If DateTimePickerVencimiento IsNot Nothing Then DateTimePickerVencimiento.Bounds = New Rectangle(col2_X + 160, yFila2, 140, 25)
        cboEstado.Bounds = New Rectangle(col2_X, yFila4, 140, 25)
        TextBoxObservaciones.Bounds = New Rectangle(col2_X + 160, yFila4, 280, 25)

        ' --- Bloque 3: Trazabilidad (Nº Fac. Proveedor + Albarán origen) ---
        TextBoxFacProveedor.Bounds = New Rectangle(col3_X, yFila2, 165, 25)
        If TextBoxAlbaranOrigen IsNot Nothing Then TextBoxAlbaranOrigen.Bounds = New Rectangle(col3_X, yFila4, 130, 25)
        If btnImportarAlbaran IsNot Nothing Then
            btnImportarAlbaran.Bounds = New Rectangle(col3_X + 135, yFila4, 30, 25)
            btnImportarAlbaran.BackColor = BTN_AZUL_PRIMARIO
            btnImportarAlbaran.ForeColor = Color.White
            btnImportarAlbaran.FlatStyle = FlatStyle.Flat
            btnImportarAlbaran.FlatAppearance.BorderSize = 0
            btnImportarAlbaran.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
            btnImportarAlbaran.Cursor = Cursors.Hand
            btnImportarAlbaran.Text = "🔍"
        End If

        ' --- Tercera fila: Forma de pago ---
        lblFormaPago.Location = New Point(col1_X, yFila5)
        lblFormaPago.BackColor = Color.Transparent
        lblFormaPago.ForeColor = Color.WhiteSmoke
        lblFormaPago.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
        cboFormaPago.Bounds = New Rectangle(col1_X, yFila6, 300, 25)
        cboFormaPago.Font = New Font("Segoe UI", 10.0F)

        ' 3. LÍNEA DIVISORIA + GRID
        Dim lineaDivisoria As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LineaDivisoria")
        If lineaDivisoria Is Nothing Then
            lineaDivisoria = New Label() With {.Name = "LineaDivisoria", .BackColor = COLOR_LINEA_DIVISORIA, .Height = 2}
            Me.Controls.Add(lineaDivisoria)
        End If

        Dim yTabla As Integer = yFila6 + 50
        lineaDivisoria.Bounds = New Rectangle(margenIzq, yTabla - 18, anchoUtil, 2)
        lineaDivisoria.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lineaDivisoria.BringToFront()

        Dim altoTabla As Integer = altoForm - yTabla - 145
        DataGridView1.Bounds = New Rectangle(margenIzq, yTabla, anchoUtil, altoTabla)
        DataGridView1.BackgroundColor = Me.BackColor
        DataGridView1.BorderStyle = BorderStyle.None
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        ' 4. PANEL LATERAL DE TOTALES
        Dim xDerecha As Integer = DataGridView1.Right
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
        LabelBase.BackColor = COLOR_PANEL_TOTALES
        LabelBase.ForeColor = COLOR_TEXTO_SECUNDARIO
        LabelBase.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
        LabelBase.TextAlign = ContentAlignment.MiddleLeft
        LabelBase.Bounds = New Rectangle(xPanelInt, yTotales + 12, 150, 22)
        LabelBase.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        TextBoxBase.BackColor = COLOR_PANEL_TOTALES
        TextBoxBase.ForeColor = Color.White
        TextBoxBase.Font = New Font("Segoe UI Semibold", 10.5F, FontStyle.Bold)
        TextBoxBase.BorderStyle = BorderStyle.None
        TextBoxBase.TextAlign = HorizontalAlignment.Right
        TextBoxBase.Bounds = New Rectangle(xPanelInt + wPanelInt - 140, yTotales + 12, 140, 22)
        TextBoxBase.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        ' IVA
        LabelIva.BackColor = COLOR_PANEL_TOTALES
        LabelIva.ForeColor = COLOR_TEXTO_SECUNDARIO
        LabelIva.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
        LabelIva.TextAlign = ContentAlignment.MiddleLeft
        LabelIva.Bounds = New Rectangle(xPanelInt, yTotales + 38, 150, 22)
        LabelIva.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        TextBoxIva.BackColor = COLOR_PANEL_TOTALES
        TextBoxIva.ForeColor = Color.White
        TextBoxIva.Font = New Font("Segoe UI Semibold", 10.5F, FontStyle.Bold)
        TextBoxIva.BorderStyle = BorderStyle.None
        TextBoxIva.TextAlign = HorizontalAlignment.Right
        TextBoxIva.Bounds = New Rectangle(xPanelInt + wPanelInt - 140, yTotales + 38, 140, 22)
        TextBoxIva.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        ' Línea separadora
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
        If TextBoxTotalFac IsNot Nothing Then
            TextBoxTotalFac.BackColor = COLOR_PANEL_TOTALES
            TextBoxTotalFac.ForeColor = COLOR_ACENTO
            TextBoxTotalFac.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold)
            TextBoxTotalFac.BorderStyle = BorderStyle.None
            TextBoxTotalFac.TextAlign = HorizontalAlignment.Right
            TextBoxTotalFac.Bounds = New Rectangle(xPanelInt + wPanelInt - 180, yTotales + 78, 180, 32)
            TextBoxTotalFac.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If

        ' 5. BARRA INFERIOR DE BOTONES AGRUPADOS
        Dim yBotones As Integer = DataGridView1.Bottom + 50

        EstilizarBoton(ButtonGuardar, margenIzq, yBotones, BTN_AZUL_PRIMARIO, Color.White)
        EstilizarBoton(ButtonBorrar, margenIzq + 110, yBotones, BTN_ROJO_PELIGRO, Color.White)
        EstilizarBoton(ButtonNuevoFac, margenIzq + 220, yBotones, BTN_AZUL_PRIMARIO, Color.White)

        Dim sep1 As Label = ObtenerOCrearSeparador("SepGrupo1")
        sep1.Bounds = New Rectangle(margenIzq + 332, yBotones + 4, 1, 27)
        sep1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left

        EstilizarBoton(ButtonNuevaLinea, margenIzq + 348, yBotones, BTN_VERDE_AÑADIR, Color.White)
        ButtonNuevaLinea.Text = "+ Añadir Línea" : ButtonNuevaLinea.Width = 120

        EstilizarBoton(ButtonBorrarLineas, margenIzq + 478, yBotones, BTN_GRIS_NEUTRO, Color.White)
        ButtonBorrarLineas.Text = "− Quitar Línea" : ButtonBorrarLineas.Width = 120

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

        Dim botonesAbajo As Control() = {ButtonGuardar, ButtonBorrar, ButtonNuevoFac, ButtonNuevaLinea, ButtonBorrarLineas, ButtonAnterior, ButtonSiguiente}
        For Each b In botonesAbajo
            If b IsNot Nothing Then b.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        Next

        If LabelStock IsNot Nothing Then
            LabelStock.Location = New Point(margenIzq, DataGridView1.Bottom + 14)
            LabelStock.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        End If
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

    Private Sub EstilizarBoton(btn As Button, x As Integer, y As Integer, bg As Color, fg As Color)
        btn.Location = New Point(x, y)
        btn.Size = New Size(100, 35)
        btn.FlatStyle = FlatStyle.Flat
        btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        btn.Cursor = Cursors.Hand
        btn.BackColor = bg
        btn.ForeColor = fg
        btn.FlatAppearance.BorderSize = 0
    End Sub
#End Region

    ' MOTOR DE EXPORTACIÓN A PDF (FACTURAS DE COMPRA)
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        If String.IsNullOrWhiteSpace(TextBoxFactura.Text) Then
            MessageBox.Show("No hay ningún documento cargado para imprimir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        docImprimir.DefaultPageSettings.Landscape = False
        docImprimir.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docImprimir.DocumentName = "FacturaCompra_" & TextBoxFactura.Text

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

        Dim fTituloDoc As New Font("Segoe UI", 24, FontStyle.Bold)
        Dim fEmpresa As New Font("Segoe UI", 14, FontStyle.Bold)
        Dim fCabecera As New Font("Segoe UI", 10, FontStyle.Bold)
        Dim fProvNombre As New Font("Segoe UI", 12, FontStyle.Bold)
        Dim fNormal As New Font("Segoe UI", 10, FontStyle.Regular)
        Dim fFila As New Font("Segoe UI", 9, FontStyle.Regular)
        Dim fTotalGordo As New Font("Segoe UI", 14, FontStyle.Bold)

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
            ' A) EMPRESA (la nuestra, la que recibe la factura)
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

            ' B) DATOS DEL DOCUMENTO (FACTURA DE COMPRA)
            g.DrawString("FACTURA RECIBIDA", fTituloDoc, bAzulCorporativo, margenDer, yPos, formatoDerecha)

            g.DrawString("Nº Documento: " & TextBoxFactura.Text, fCabecera, bNegro, margenDer, yPos + 40, formatoDerecha)
            g.DrawString("Fecha emisión: " & TextBoxFecha.Text, fNormal, bGrisOscuro, margenDer, yPos + 60, formatoDerecha)
            g.DrawString("Vencimiento: " & DateTimePickerVencimiento.Value.ToShortDateString(), fCabecera, bAzulCorporativo, margenDer, yPos + 80, formatoDerecha)

            If Not String.IsNullOrWhiteSpace(TextBoxFacProveedor.Text) Then
                g.DrawString("Nº fac. proveedor: " & TextBoxFacProveedor.Text, fNormal, bGrisOscuro, margenDer, yPos + 105, formatoDerecha)
            End If
            If Not String.IsNullOrWhiteSpace(TextBoxAlbaranOrigen.Text) Then
                g.DrawString("Origen: Albarán " & TextBoxAlbaranOrigen.Text, fNormal, bGrisOscuro, margenDer, yPos + 125, formatoDerecha)
            End If

            yPos += 145
            g.DrawLine(lapizGrueso, margenIzq, yPos, margenDer, yPos)
            yPos += 20

            ' C) PROVEEDOR (datos congelados de la propia factura)
            Dim provNombre As String = TextBoxProveedor.Text
            Dim provCIF As String = "", provDireccion As String = "", provPoblacion As String = "", provContacto As String = ""

            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()
                ' Tomamos prioritariamente los datos congelados; si están vacíos, los del proveedor actual
                Dim sqlProv As String = "SELECT F.NombreFiscal AS NF_Frozen, F.CIF AS CIF_Frozen, F.Direccion AS Dir_Frozen, F.Poblacion AS Pob_Frozen, F.CodigoPostal AS CP_Frozen, " &
                                        "Pr.NombreFiscal AS NF_Live, Pr.CIF AS CIF_Live, Pr.Direccion AS Dir_Live, Pr.Poblacion AS Pob_Live, Pr.Provincia AS Prov_Live, Pr.Telefono AS Tel_Live, Pr.Email AS Mail_Live " &
                                        "FROM FacturasCompra F " &
                                        "LEFT JOIN Proveedores Pr ON F.ID_Proveedor = Pr.CodigoProveedor " &
                                        "WHERE F.NumeroFacturaCompra = @num"
                Using cmd As New SQLiteCommand(sqlProv, c)
                    cmd.Parameters.AddWithValue("@num", _numeroFacturaActual)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim nfFrozen = If(IsDBNull(reader("NF_Frozen")), "", reader("NF_Frozen").ToString())
                            provNombre = If(String.IsNullOrWhiteSpace(nfFrozen), If(IsDBNull(reader("NF_Live")), TextBoxProveedor.Text, reader("NF_Live").ToString()), nfFrozen)

                            Dim cifFrozen = If(IsDBNull(reader("CIF_Frozen")), "", reader("CIF_Frozen").ToString())
                            Dim cifFinal = If(String.IsNullOrWhiteSpace(cifFrozen), If(IsDBNull(reader("CIF_Live")), "", reader("CIF_Live").ToString()), cifFrozen)
                            If cifFinal <> "" Then provCIF = "CIF: " & cifFinal

                            Dim dirFrozen = If(IsDBNull(reader("Dir_Frozen")), "", reader("Dir_Frozen").ToString())
                            provDireccion = If(String.IsNullOrWhiteSpace(dirFrozen), If(IsDBNull(reader("Dir_Live")), "", reader("Dir_Live").ToString()), dirFrozen)

                            Dim pobFrozen = If(IsDBNull(reader("Pob_Frozen")), "", reader("Pob_Frozen").ToString())
                            Dim pobFinal = If(String.IsNullOrWhiteSpace(pobFrozen), If(IsDBNull(reader("Pob_Live")), "", reader("Pob_Live").ToString()), pobFrozen)
                            Dim provLive As String = If(IsDBNull(reader("Prov_Live")), "", reader("Prov_Live").ToString())
                            provPoblacion = If(pobFinal <> "" And provLive <> "", pobFinal & " (" & provLive & ")", pobFinal & provLive)

                            Dim tel As String = If(IsDBNull(reader("Tel_Live")), "", "Tlf: " & reader("Tel_Live").ToString())
                            Dim mail As String = If(IsDBNull(reader("Mail_Live")), "", reader("Mail_Live").ToString())
                            provContacto = If(tel <> "" And mail <> "", tel & " | " & mail, tel & mail)
                        End If
                    End Using
                End Using
            Catch ex As Exception
            End Try

            g.FillRectangle(bGrisClaro, margenIzq, yPos, anchoPagina, 145)
            g.DrawRectangle(lapizFino, margenIzq, yPos, anchoPagina, 145)
            g.DrawString("DATOS DEL PROVEEDOR:", fCabecera, bAzulCorporativo, margenIzq + 15, yPos + 10)
            g.DrawString(provNombre, fProvNombre, bNegro, margenIzq + 15, yPos + 35)

            Dim xDatos As Integer = margenIzq + 15
            Dim yDatos As Integer = yPos + 62
            Dim interlineado As Integer = 18

            If provCIF <> "" Then g.DrawString(provCIF, fNormal, bGrisOscuro, xDatos, yDatos) : yDatos += interlineado
            If provDireccion <> "" Then g.DrawString(provDireccion, fNormal, bGrisOscuro, xDatos, yDatos) : yDatos += interlineado
            If provPoblacion <> "" Then g.DrawString(provPoblacion, fNormal, bGrisOscuro, xDatos, yDatos) : yDatos += interlineado
            If provContacto <> "" Then g.DrawString(provContacto, fNormal, bGrisOscuro, xDatos, yDatos)

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

            Dim cant As String = If(row.Cells("Cantidad").Value IsNot Nothing, row.Cells("Cantidad").Value.ToString(), "0")
            Dim desc As String = If(row.Cells("Descripcion").Value IsNot Nothing, row.Cells("Descripcion").Value.ToString(), "")
            Dim precio As String = If(row.Cells("PrecioCosteReal").Value IsNot Nothing, Convert.ToDecimal(row.Cells("PrecioCosteReal").Value).ToString("N2") & " €", "0,00 €")
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

            Dim totalBase As String = TextBoxBase.Text.Replace("Base imponible :", "").Trim()
            Dim totalIVA As String = TextBoxIva.Text.Replace("I.V.A :", "").Trim()
            Dim totalDoc As String = TextBoxTotalFac.Text.Replace("TOTAL :", "").Trim()

            Dim anchoCajaTotales As Integer = 300
            Dim xCaja As Integer = margenDer - anchoCajaTotales
            Dim yNotas As Integer = yPos

            If cboFormaPago.Text <> "" Then
                g.DrawString("Forma de pago:", fCabecera, bAzulCorporativo, margenIzq, yNotas)
                g.DrawString(cboFormaPago.Text, fNormal, bNegro, margenIzq + 110, yNotas)
                yNotas += 35
            End If

            If TextBoxObservaciones.Text.Trim() <> "" Then
                g.DrawString("Observaciones:", fCabecera, bAzulCorporativo, margenIzq, yNotas)
                Dim rectObs As New Rectangle(margenIzq, yNotas + 20, anchoCajaTotales - 20, 60)
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

            g.DrawString("TOTAL A PAGAR", fCabecera, bBlanco, rectTxtTotal, formatoIzquierda)
            g.DrawString(totalDoc, fTotalGordo, bBlanco, rectValTotal, formatoDerecha)

            yPos += 80

            ' Estado de la factura abajo
            Dim mensajeEstado As String = "Estado: " & cboEstado.Text
            If cboEstado.Text.Equals("Pagada", StringComparison.OrdinalIgnoreCase) Then
                g.DrawString(mensajeEstado, fCabecera, New SolidBrush(Color.FromArgb(40, 140, 90)), margenIzq, yPos)
            ElseIf cboEstado.Text.Equals("Vencida", StringComparison.OrdinalIgnoreCase) Then
                g.DrawString(mensajeEstado, fCabecera, New SolidBrush(Color.FromArgb(209, 52, 56)), margenIzq, yPos)
            Else
                g.DrawString(mensajeEstado, fCabecera, bGrisOscuro, margenIzq, yPos)
            End If

            e.HasMorePages = False
        End If
    End Sub
End Class
