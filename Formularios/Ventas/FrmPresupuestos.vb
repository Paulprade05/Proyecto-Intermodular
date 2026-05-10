Imports System.Data.SQLite

Public Class FrmPresupuestos

#Region "1. Variables Globales y Propiedades"
    Private _numeroPresupuestoActual As String = ""
    Private _dtLineas As DataTable
    Private _idsParaBorrar As New List(Of Integer)
    ' --- MOTOR DE IMPRESIÓN DEL DOCUMENTO ---
    Private WithEvents btnImprimir As New Button()
    Private WithEvents docImprimir As New Printing.PrintDocument()
    Private _filaActualImpresion As Integer = 0
    Private WithEvents cboFormaPago As New ComboBox()
    'Private WithEvents cboRuta As New ComboBox()
    Private lblFormaPago As New Label() With {.Text = "Forma de Pago", .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.WhiteSmoke}
    'Private lblRuta As New Label() With {.Text = "Ruta Asignada", .AutoSize = True, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = Color.WhiteSmoke}
    Private WithEvents cboEstado As New ComboBox()
#End Region

#Region "2. Eventos de Inicialización (Load)"
    Private Sub FrmPresupuestos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SuspendLayout()

        ConfigurarDiseñoResponsive()
        EstilizarGrid(DataGridView1)

        Me.Controls.Add(lblFormaPago) : Me.Controls.Add(cboFormaPago)
        'Me.Controls.Add(lblRuta) : Me.Controls.Add(cboRuta)
        cboFormaPago.DropDownStyle = ComboBoxStyle.DropDownList
        'cboRuta.DropDownStyle = ComboBoxStyle.DropDownList
        CargarDesplegables()
        Me.Controls.Add(cboEstado)
        cboEstado.DropDownStyle = ComboBoxStyle.DropDownList
        cboEstado.Items.Clear()
        cboEstado.Items.AddRange(New String() {"Pendiente", "Aceptado", "Rechazado", "Convertido"})
        ReorganizarControlesAutomaticamente()
        ConfigurarColumnasGrid()
        ' CONFIGURACIÓN DEL BOTÓN IMPRIMIR
        btnImprimir.Text = "Exportar PDF"

        ' 1. Le damos el tamaño
        btnImprimir.Size = New Size(120, 35)

        ' 2. TRUCO: Lo anclamos debajo del botón Guardar automáticamente
        ' OJO: Si tu botón de guardar no se llama "ButtonGuardar", cambia el nombre en esta línea
        btnImprimir.Location = New Point(ButtonGuardar.Location.X, ButtonGuardar.Location.Y + ButtonGuardar.Height + 10)

        btnImprimir.BackColor = Color.FromArgb(40, 140, 90) ' Verde corporativo
        btnImprimir.ForeColor = Color.White
        btnImprimir.FlatStyle = FlatStyle.Flat
        btnImprimir.FlatAppearance.BorderSize = 0
        btnImprimir.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnImprimir.Cursor = Cursors.Hand
        Me.Controls.Add(btnImprimir)
        btnImprimir.BringToFront()
        Dim ultimoNum As String = ObtenerUltimoNumeroPresupuesto()
        If Not String.IsNullOrEmpty(ultimoNum) Then
            CargarPresupuesto(ultimoNum)
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

            'Dim daRuta As New SQLiteDataAdapter("SELECT ID_Ruta, NombreZona FROM Rutas WHERE Activo=1", c)
            'Dim dtRuta As New DataTable() : daRuta.Fill(dtRuta)
            'cboRuta.DataSource = dtRuta : cboRuta.DisplayMember = "NombreZona" : cboRuta.ValueMember = "ID_Ruta" : cboRuta.SelectedIndex = -1
        Catch ex As Exception
        End Try
    End Sub
#End Region

#Region "3. Configuración UI y Diseño"
    Private Sub ConfigurarDiseñoResponsive()
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Dim controlesTotales As Control() = {LabelBase, TextBoxBase, LabelIva, TextBoxIva, Label7, TextBoxTotalPresup, LabelStock}
        For Each ctrl In controlesTotales : ctrl.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right : Next
        ButtonAnterior.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        ButtonSiguiente.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        Dim botonesIzq As Control() = {ButtonGuardar, ButtonBorrar, ButtonNuevoPresup, ButtonBorrarLineas, ButtonNuevaLinea}
        For Each ctrl In botonesIzq : ctrl.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left : Next
    End Sub

    Private Sub ConfigurarColumnasGrid()
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.Columns.Clear()

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Linea", .DataPropertyName = "ID_Linea", .Visible = False})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NumeroOrden", .DataPropertyName = "NumeroOrden", .HeaderText = "Nº", .Width = 40, .ReadOnly = True, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "ID_Articulo", .DataPropertyName = "ID_Articulo", .HeaderText = "ID Art", .Width = 60})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descripcion", .DataPropertyName = "Descripcion", .HeaderText = "Descripción", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Cantidad", .DataPropertyName = "Cantidad", .HeaderText = "Cantidad", .Width = 75, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2", .BackColor = Color.Ivory}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PrecioUnitario", .DataPropertyName = "PrecioUnitario", .HeaderText = "Precio Base", .Width = 85, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2"}})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Descuento", .DataPropertyName = "Descuento", .HeaderText = "% Dto", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N2"}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "PorcentajeIVA", .DataPropertyName = "PorcentajeIVA", .HeaderText = "% IVA", .Width = 60, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "N0"}})

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Total", .DataPropertyName = "Total", .HeaderText = "Total Base", .ReadOnly = True, .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.WhiteSmoke}})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "TotalConIVA", .DataPropertyName = "TotalConIVA", .HeaderText = "Total (+IVA)", .ReadOnly = True, .Width = 100, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "C2", .BackColor = Color.FromArgb(230, 240, 250)}})
    End Sub

    Public Shared Sub EstilizarGrid(dgv As DataGridView)
        dgv.BackgroundColor = Color.White : dgv.BorderStyle = BorderStyle.None : dgv.CellBorderStyle = DataGridViewCellBorderStyle.None
        dgv.EnableHeadersVisualStyles = False : dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 55, 65)
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgv.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        dgv.ColumnHeadersHeight = 40
        dgv.DefaultCellStyle.BackColor = Color.White : dgv.DefaultCellStyle.ForeColor = Color.Black
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 255) : dgv.DefaultCellStyle.SelectionForeColor = Color.Black
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245)
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 255)
        dgv.RowHeadersVisible = False : dgv.RowTemplate.Height = 35 : dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgv.MultiSelect = False
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
        If TextBoxTotalPresup IsNot Nothing Then TextBoxTotalPresup.Text = (base + sumaIva).ToString("C2")
    End Sub

    Private Sub LimpiarFormulario()
        _numeroPresupuestoActual = ""
        _idsParaBorrar.Clear()
        TextBoxPresupuesto.Text = GenerarProximoNumero()

        TextBoxCliente.Text = "" : TextBoxIdCliente.Text = "" : TextBoxCliente.Tag = Nothing
        TextBoxVendedor.Text = "" : TextBoxIdVendedor.Text = ""
        TextBoxObservaciones.Text = ""
        TextBoxFecha.Text = DateTime.Now.ToShortDateString()
        cboEstado.Text = "Pendiente"
        TextBoxBase.Text = "0,00 €" : TextBoxIva.Text = "0,00 €" : TextBoxTotalPresup.Text = "0,00 €"

        _dtLineas = New DataTable()
        ConfigurarEstructuraDataTable()
        DataGridView1.DataSource = _dtLineas
        If cboFormaPago IsNot Nothing Then cboFormaPago.SelectedIndex = -1
        'If cboRuta IsNot Nothing Then cboRuta.SelectedIndex = -1
        TextBoxIdCliente.Focus()
    End Sub


    Private Sub ConfigurarEstructuraDataTable()
        With _dtLineas.Columns
            If Not .Contains("ID_Linea") Then .Add("ID_Linea", GetType(Object))
            If Not .Contains("NumeroPresupuesto") Then .Add("NumeroPresupuesto", GetType(String))
            If Not .Contains("NumeroOrden") Then .Add("NumeroOrden", GetType(Integer))
            If Not .Contains("ID_Articulo") Then .Add("ID_Articulo", GetType(Object))
            If Not .Contains("Descripcion") Then .Add("Descripcion", GetType(String))
            If Not .Contains("Cantidad") Then .Add("Cantidad", GetType(Decimal))
            If Not .Contains("PrecioUnitario") Then .Add("PrecioUnitario", GetType(Decimal))
            If Not .Contains("Descuento") Then .Add("Descuento", GetType(Decimal))

            ' --- NUEVAS COLUMNAS IVA Y VIRTUALES ---
            If Not .Contains("PorcentajeIVA") Then .Add("PorcentajeIVA", GetType(Decimal))
            If Not .Contains("PrecioConIVA") Then .Add("PrecioConIVA", GetType(Decimal))
            If Not .Contains("Total") Then .Add("Total", GetType(Decimal))
            If Not .Contains("TotalConIVA") Then .Add("TotalConIVA", GetType(Decimal))
        End With
    End Sub

    Private Function GenerarProximoNumero() As String
        ' Antes esta función ordenaba por string (ORDER BY NumeroPresupuesto DESC), lo que rompía
        ' la numeración al pasar de PRE-999 a PRE-1000. Ahora delegamos en NumeradorDocumentos
        ' que ordena por la parte numérica.
        Return NumeradorDocumentos.SiguienteNumero("PRE-", "Presupuestos", "NumeroPresupuesto")
    End Function
#End Region

#Region "5. Persistencia (Base de Datos - CRUD)"
    Private Sub CargarPresupuesto(numeroPresupuesto As String)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' A. CARGAR CABECERA
            Dim sqlCab As String = "SELECT P.*, C.NombreFiscal AS NombreCliente, V.Nombre AS NombreVendedor " &
                                   "FROM Presupuestos P " &
                                   "LEFT JOIN Clientes C ON P.CodigoCliente = C.CodigoCliente " &
                                   "LEFT JOIN Vendedores V ON P.ID_Vendedor = V.ID_Vendedor " &
                                   "WHERE P.NumeroPresupuesto = @num"

            Using cmd As New SQLiteCommand(sqlCab, c)
                cmd.Parameters.AddWithValue("@num", numeroPresupuesto)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        _numeroPresupuestoActual = numeroPresupuesto
                        TextBoxPresupuesto.Text = r("NumeroPresupuesto").ToString()
                        TextBoxFecha.Text = If(IsDBNull(r("Fecha")), "", Convert.ToDateTime(r("Fecha")).ToShortDateString())
                        TextBoxObservaciones.Text = r("Observaciones").ToString()
                        cboEstado.Text = r("Estado").ToString()
                        TextBoxIdCliente.Text = r("CodigoCliente").ToString()
                        TextBoxCliente.Text = r("NombreCliente").ToString()
                        TextBoxIdVendedor.Text = r("ID_Vendedor").ToString()
                        TextBoxVendedor.Text = r("NombreVendedor").ToString()

                        Dim idPago = r("ID_FormaPago")
                        If Not IsDBNull(idPago) Then cboFormaPago.SelectedValue = Convert.ToInt32(idPago) Else cboFormaPago.SelectedIndex = -1

                        'Dim idRut = r("ID_Ruta")
                        'If Not IsDBNull(idRut) Then cboRuta.SelectedValue = Convert.ToInt32(idRut) Else cboRuta.SelectedIndex = -1
                    Else
                        MessageBox.Show("Presupuesto no encontrado.") : Return
                    End If
                End Using
            End Using

            ' B. CARGAR LÍNEAS
            Dim sqlLin As String = "SELECT * FROM LineasPresupuesto WHERE NumeroPresupuesto = @num ORDER BY NumeroOrden ASC"
            Using cmd As New SQLiteCommand(sqlLin, c)
                cmd.Parameters.AddWithValue("@num", numeroPresupuesto)
                Dim da As New SQLiteDataAdapter(cmd)
                _dtLineas = New DataTable()
                da.Fill(_dtLineas)
                ConfigurarEstructuraDataTable()

                ' Magia para calcular IVA al vuelo en presupuestos viejos
                For Each row As DataRow In _dtLineas.Rows
                    Dim cant As Decimal = If(IsDBNull(row("Cantidad")), 0, Convert.ToDecimal(row("Cantidad")))
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
                CalcularTotalesGenerales()
            End Using

        Catch ex As Exception
            MessageBox.Show("Error al cargar: " & ex.Message)
        End Try
    End Sub

    Private Sub GuardarPresupuesto()
        If String.IsNullOrWhiteSpace(TextBoxIdCliente.Text) Then MessageBox.Show("Falta el Cliente") : Return

        Dim esNuevo As Boolean = String.IsNullOrEmpty(_numeroPresupuestoActual)
        If esNuevo Then
            TextBoxPresupuesto.Text = GenerarProximoNumero()
            _numeroPresupuestoActual = TextBoxPresupuesto.Text
        End If

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
            'Dim idRuta As Object = If(cboRuta.SelectedValue IsNot Nothing AndAlso cboRuta.SelectedIndex <> -1, cboRuta.SelectedValue, DBNull.Value)
            Dim idVend As Object = If(IsNumeric(TextBoxIdVendedor.Text) AndAlso Val(TextBoxIdVendedor.Text) > 0, Convert.ToInt32(TextBoxIdVendedor.Text), DBNull.Value)

            Dim sql As String = ""
            If esNuevo Then
                sql = "INSERT INTO Presupuestos (NumeroPresupuesto, CodigoCliente, ID_Vendedor, Fecha, Observaciones, Estado, ID_FormaPago, BaseImponible, ImporteIVA, TotalPresupuesto) " &
                      "VALUES (@num, @cli, @vend, @fecha, @obs, @est, @formaPago, @base, @iva, @total)"
            Else
                sql = "UPDATE Presupuestos SET CodigoCliente=@cli, ID_Vendedor=@vend, Fecha=@fecha, Observaciones=@obs, Estado=@est, ID_FormaPago=@formaPago, BaseImponible=@base, ImporteIVA=@iva, TotalPresupuesto=@total " &
                      "WHERE NumeroPresupuesto = @num"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Transaction = trans
                cmd.Parameters.AddWithValue("@num", _numeroPresupuestoActual)
                cmd.Parameters.AddWithValue("@cli", TextBoxIdCliente.Text.Trim())
                cmd.Parameters.AddWithValue("@vend", idVend)

                Dim fecha As DateTime
                If DateTime.TryParse(TextBoxFecha.Text, fecha) Then cmd.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd HH:mm:ss")) Else cmd.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                cmd.Parameters.AddWithValue("@obs", TextBoxObservaciones.Text.Trim())
                cmd.Parameters.AddWithValue("@est", cboEstado.Text.Trim())
                cmd.Parameters.AddWithValue("@formaPago", idFormaPago)
                'cmd.Parameters.AddWithValue("@ruta", idRuta)

                cmd.Parameters.AddWithValue("@base", sumaBase)
                cmd.Parameters.AddWithValue("@iva", sumaIva)
                cmd.Parameters.AddWithValue("@total", sumaBase + sumaIva)
                cmd.ExecuteNonQuery()
            End Using

            ' 2. Borrar líneas eliminadas
            For Each idDel In _idsParaBorrar
                Using cmdDel As New SQLiteCommand("DELETE FROM LineasPresupuesto WHERE ID_Linea = @id", c)
                    cmdDel.Transaction = trans : cmdDel.Parameters.AddWithValue("@id", idDel) : cmdDel.ExecuteNonQuery()
                End Using
            Next
            _idsParaBorrar.Clear()

            ' 3. Guardar Líneas
            Dim orden As Integer = 1
            For Each row As DataRow In _dtLineas.Rows
                If row.RowState = DataRowState.Deleted Then Continue For

                Dim idLin = row("ID_Linea")
                Dim idArt = If(Val(row("ID_Articulo")) > 0, row("ID_Articulo"), DBNull.Value)
                Dim sqlLine As String = ""

                If IsDBNull(idLin) OrElse Val(idLin) = 0 Then
                    sqlLine = "INSERT INTO LineasPresupuesto (NumeroPresupuesto, NumeroOrden, ID_Articulo, Descripcion, Cantidad, PrecioUnitario, Descuento, PorcentajeIVA, Total) " &
                              "VALUES (@num, @ord, @art, @desc, @cant, @prec, @dcto, @iva, @tot)"
                Else
                    sqlLine = "UPDATE LineasPresupuesto SET NumeroOrden=@ord, ID_Articulo=@art, Descripcion=@desc, Cantidad=@cant, PrecioUnitario=@prec, Descuento=@dcto, PorcentajeIVA=@iva, Total=@tot WHERE ID_Linea=@id"
                End If

                Using cmdL As New SQLiteCommand(sqlLine, c)
                    cmdL.Transaction = trans
                    cmdL.Parameters.AddWithValue("@num", _numeroPresupuestoActual)
                    cmdL.Parameters.AddWithValue("@ord", orden)
                    cmdL.Parameters.AddWithValue("@art", idArt)
                    cmdL.Parameters.AddWithValue("@desc", row("Descripcion"))
                    cmdL.Parameters.AddWithValue("@cant", row("Cantidad"))
                    cmdL.Parameters.AddWithValue("@prec", row("PrecioUnitario"))
                    cmdL.Parameters.AddWithValue("@dcto", row("Descuento"))
                    cmdL.Parameters.AddWithValue("@iva", If(IsDBNull(row("PorcentajeIVA")), 21, row("PorcentajeIVA")))
                    cmdL.Parameters.AddWithValue("@tot", row("Total"))
                    If Not String.IsNullOrEmpty(sqlLine) AndAlso sqlLine.Contains("WHERE") Then cmdL.Parameters.AddWithValue("@id", idLin)
                    cmdL.ExecuteNonQuery()
                End Using
                orden += 1
            Next

            trans.Commit()
            MessageBox.Show("Guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CargarPresupuesto(_numeroPresupuestoActual)

        Catch ex As Exception
            If trans IsNot Nothing Then trans.Rollback()
            LogErrores.Registrar("FrmPresupuestos.Guardar", ex)
            MessageBox.Show("Error al guardar: " & ex.Message)
        End Try
    End Sub

    Private Function ObtenerUltimoNumeroPresupuesto() As String
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim cmd As New SQLiteCommand("SELECT MAX(NumeroPresupuesto) FROM Presupuestos", c)
            Dim r = cmd.ExecuteScalar()
            Return If(IsDBNull(r), "", r.ToString())
        Catch
            Return ""
        End Try
    End Function
#End Region

#Region "6. Manejadores de Eventos (Botones y Grid)"

    Private Sub ButtonGuardar_Click(sender As Object, e As EventArgs) Handles ButtonGuardar.Click
        GuardarPresupuesto()
    End Sub

    Private Sub ButtonNuevoPresup_Click(sender As Object, e As EventArgs) Handles ButtonNuevoPresup.Click
        LimpiarFormulario()
    End Sub

    Private Sub ButtonBorrar_Click(sender As Object, e As EventArgs) Handles ButtonBorrar.Click
        If String.IsNullOrEmpty(_numeroPresupuestoActual) Then Return
        If MessageBox.Show("¿Seguro que deseas eliminar este presupuesto?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Dim c = ConexionBD.GetConnection()
                If c.State <> ConnectionState.Open Then c.Open()
                Dim cmd As New SQLiteCommand("DELETE FROM LineasPresupuesto WHERE NumeroPresupuesto=@num; DELETE FROM Presupuestos WHERE NumeroPresupuesto=@num;", c)
                cmd.Parameters.AddWithValue("@num", _numeroPresupuestoActual)
                cmd.ExecuteNonQuery()
                LimpiarFormulario()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub

    Private Sub btnBuscarPresupuesto_Click(sender As Object, e As EventArgs) Handles btnBuscarPresupuesto.Click
        Dim frmBuscar As New FrmBuscador()
        frmBuscar.TablaABuscar = "Presupuestos"

        If frmBuscar.ShowDialog() = DialogResult.OK Then
            CargarPresupuesto(frmBuscar.Resultado)
        End If
    End Sub


    Private Sub Navegar(direccion As String)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Dim sql As String = If(direccion = "ANT",
                                   "SELECT MAX(NumeroPresupuesto) FROM Presupuestos WHERE NumeroPresupuesto < @act",
                                   "SELECT MIN(NumeroPresupuesto) FROM Presupuestos WHERE NumeroPresupuesto > @act")

            If String.IsNullOrEmpty(_numeroPresupuestoActual) And direccion = "ANT" Then
                sql = "SELECT MAX(NumeroPresupuesto) FROM Presupuestos"
            End If

            Using cmd As New SQLiteCommand(sql, c)
                cmd.Parameters.AddWithValue("@act", _numeroPresupuestoActual)
                Dim res = cmd.ExecuteScalar()
                If res IsNot Nothing AndAlso Not IsDBNull(res) Then CargarPresupuesto(res.ToString())
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
        fila("NumeroPresupuesto") = _numeroPresupuestoActual
        fila("Cantidad") = 1
        fila("PrecioUnitario") = 0
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


    ' EL FRENO MÁGICO
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
        If colName = "Cantidad" Or colName = "PrecioUnitario" Or colName = "Descuento" Or colName = "PorcentajeIVA" Then
            Dim cant As Decimal = 0, prec As Decimal = 0, dto As Decimal = 0, iva As Decimal = 21
            Decimal.TryParse(fila.Cells("Cantidad").Value?.ToString(), cant)
            Decimal.TryParse(fila.Cells("PrecioUnitario").Value?.ToString(), prec)
            Decimal.TryParse(fila.Cells("Descuento").Value?.ToString(), dto)
            Decimal.TryParse(fila.Cells("PorcentajeIVA").Value?.ToString(), iva)

            Dim precioConIva As Decimal = prec * (1 + (iva / 100))
            Dim totalSinIva As Decimal = (cant * prec) * (1 - (dto / 100))
            Dim totalConIva As Decimal = totalSinIva * (1 + (iva / 100))


            fila.Cells("Total").Value = totalSinIva
            fila.Cells("TotalConIVA").Value = totalConIva

            CalcularTotalesGenerales()
        End If
    End Sub

    Private Sub DataGridView1_RowEnter(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.RowEnter
        If DataGridView1.Rows(e.RowIndex).Tag IsNot Nothing Then
            Dim stock As Decimal = Val(DataGridView1.Rows(e.RowIndex).Tag)
            LabelStock.Text = If(stock <= 0, "⚠️ ¡SIN STOCK!", $"Stock Disponible: {stock}")
            LabelStock.ForeColor = If(stock <= 0, Color.Red, Color.DarkGreen)
        Else
            LabelStock.Text = ""
        End If
    End Sub



    Private Sub DataGridView1_RowsRemoved(sender As Object, e As DataGridViewRowsRemovedEventArgs) Handles DataGridView1.RowsRemoved
        CalcularTotalesGenerales()
    End Sub

    Private Sub TextBoxIdCliente_Leave(sender As Object, e As EventArgs) Handles TextBoxIdCliente.Leave
        ' Si el campo está vacío, limpiamos y salimos
        If String.IsNullOrWhiteSpace(TextBoxIdCliente.Text) Then
            TextBoxCliente.Text = ""
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            Dim cmd As New SQLiteCommand("SELECT NombreFiscal, ID_Ruta FROM Clientes WHERE CodigoCliente=@id", c)
            cmd.Parameters.AddWithValue("@id", TextBoxIdCliente.Text.Trim())

            Using r = cmd.ExecuteReader()
                If r.Read() Then
                    ' ¡Cliente encontrado! Rellenamos el nombre
                    TextBoxCliente.Text = r("NombreFiscal").ToString()

                    ' (Mantengo tu código original comentado por si decides usarlo)
                    ' Dim idRut = r("ID_Ruta")
                    ' If Not IsDBNull(idRut) Then cboRuta.SelectedValue = Convert.ToInt32(idRut)
                Else
                    ' El cliente NO existe, lanzamos la pregunta interactiva
                    Dim respuesta As DialogResult = MessageBox.Show(
                        "El código de cliente '" & TextBoxIdCliente.Text & "' no existe." & vbCrLf & vbCrLf &
                        "¿Deseas crear esta nueva ficha de cliente ahora?",
                        "Cliente no encontrado",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question)

                    If respuesta = DialogResult.Yes Then
                        ' Guardamos el código que escribió para pasárselo a la ficha
                        Dim codigoFaltante As String = TextBoxIdCliente.Text.Trim()

                        ' Limpiamos las cajas del presupuesto
                        TextBoxIdCliente.Text = ""
                        TextBoxCliente.Text = ""

                        ' Abrimos la ficha y le pasamos el código para que se autorellene
                        Dim frm As New FrmClienteDetalle()
                        frm.CodigoNuevoPredefinido = codigoFaltante
                        frm.ShowDialog()

                        ' Devolvemos el foco para que lo escriba ya sabiendo que existe
                        TextBoxIdCliente.Focus()
                    Else
                        ' Si dice que no, borramos el código falso
                        TextBoxIdCliente.Text = ""
                        TextBoxCliente.Text = ""
                        TextBoxIdCliente.Focus()
                    End If
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al buscar cliente: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TextBoxIdVendedor_Leave(sender As Object, e As EventArgs) Handles TextBoxIdVendedor.Leave
        If String.IsNullOrWhiteSpace(TextBoxIdVendedor.Text) Then TextBoxVendedor.Text = "" : Return

        ' Validamos primero que el ID sea numérico antes de tocar la BD.
        ' ID_Vendedor es INTEGER, por lo que cualquier texto no numérico no encontrará vendedor.
        Dim idVend As Integer
        If Not Integer.TryParse(TextBoxIdVendedor.Text.Trim(), idVend) OrElse idVend <= 0 Then
            TextBoxVendedor.Text = "NO EXISTE"
            Return
        End If

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            ' Usamos parámetros (NUNCA concatenar texto del usuario en SQL — riesgo de inyección)
            Using cmd As New SQLiteCommand("SELECT Nombre FROM Vendedores WHERE ID_Vendedor = @id", c)
                cmd.Parameters.AddWithValue("@id", idVend)
                Dim r = cmd.ExecuteScalar()
                TextBoxVendedor.Text = If(r IsNot Nothing AndAlso Not IsDBNull(r), r.ToString(), "NO EXISTE")
            End Using
        Catch
        End Try
    End Sub
#End Region

#Region "Auto-Organización Visual (Diseño Modernizado)"
    ' --- PALETA DE COLORES CENTRALIZADA (idéntica a Pedidos/Albaranes/Facturas) ---
    Private Shared ReadOnly COLOR_FONDO As Color = Color.FromArgb(70, 75, 80)
    Private Shared ReadOnly COLOR_BANDA As Color = Color.FromArgb(40, 50, 70)
    Private Shared ReadOnly COLOR_PANEL_TOTALES As Color = Color.FromArgb(25, 30, 40)
    Private Shared ReadOnly COLOR_ACENTO As Color = Color.FromArgb(0, 150, 255)
    Private Shared ReadOnly COLOR_TEXTO_SECUNDARIO As Color = Color.FromArgb(170, 180, 195)
    Private Shared ReadOnly COLOR_LINEA_DIVISORIA As Color = Color.FromArgb(120, 130, 140)
    Private Shared ReadOnly COLOR_SEPARADOR_GRUPO As Color = Color.FromArgb(95, 105, 120)

    ' --- COLORES SEMÁNTICOS DE BOTONES ---
    Private Shared ReadOnly BTN_AZUL_PRIMARIO As Color = Color.FromArgb(0, 120, 215)
    Private Shared ReadOnly BTN_ROJO_PELIGRO As Color = Color.FromArgb(209, 52, 56)
    Private Shared ReadOnly BTN_VERDE_AÑADIR As Color = Color.FromArgb(40, 140, 90)
    Private Shared ReadOnly BTN_GRIS_NEUTRO As Color = Color.FromArgb(85, 85, 85)

    Private Sub ReorganizarControlesAutomaticamente()
        For Each ctrl As Control In Me.Controls : ctrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left : Next

        Dim margenIzq As Integer = 30
        Dim anchoForm As Integer = Me.ClientSize.Width
        Dim altoForm As Integer = Me.ClientSize.Height

        ' 1. BANDA SUPERIOR CON TÍTULO Y NÚMERO DE DOCUMENTO
        Dim alturaBanda As Integer = 60

        ' Panel de fondo de la banda
        Dim bandaSuperior As Panel = Me.Controls.OfType(Of Panel)().FirstOrDefault(Function(p) p.Name = "BandaSuperior")
        If bandaSuperior Is Nothing Then
            bandaSuperior = New Panel() With {.Name = "BandaSuperior", .BackColor = COLOR_BANDA}
            Me.Controls.Add(bandaSuperior)
            bandaSuperior.SendToBack()
        End If
        bandaSuperior.Bounds = New Rectangle(0, 0, anchoForm, alturaBanda)
        bandaSuperior.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        ' Título grande "PRESUPUESTO"
        Dim lblTitulo As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LblTituloDoc")
        If lblTitulo Is Nothing Then
            lblTitulo = New Label() With {.Name = "LblTituloDoc", .AutoSize = False, .BackColor = COLOR_BANDA,
                                          .ForeColor = Color.White, .TextAlign = ContentAlignment.MiddleLeft,
                                          .Font = New Font("Segoe UI", 18, FontStyle.Bold)}
            Me.Controls.Add(lblTitulo)
        End If
        lblTitulo.Text = "PRESUPUESTO"
        lblTitulo.Bounds = New Rectangle(margenIzq, 0, 400, alturaBanda)
        lblTitulo.BringToFront()

        ' Etiqueta "Nº Documento" pequeña a la derecha del título
        Dim lblNumDocEtiqueta As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LblNumDocEtiqueta")
        If lblNumDocEtiqueta Is Nothing Then
            lblNumDocEtiqueta = New Label() With {.Name = "LblNumDocEtiqueta", .AutoSize = False, .BackColor = COLOR_BANDA,
                                                  .ForeColor = COLOR_TEXTO_SECUNDARIO, .TextAlign = ContentAlignment.MiddleRight,
                                                  .Font = New Font("Segoe UI", 9.5F, FontStyle.Regular), .Text = "Nº Documento"}
            Me.Controls.Add(lblNumDocEtiqueta)
        End If
        lblNumDocEtiqueta.Bounds = New Rectangle(margenIzq + 410, 8, 110, 20)
        lblNumDocEtiqueta.BringToFront()

        ' TextBoxPresupuesto reposicionado dentro de la banda, destacado
        TextBoxPresupuesto.Bounds = New Rectangle(margenIzq + 410, 28, 130, 28)
        TextBoxPresupuesto.BackColor = COLOR_PANEL_TOTALES
        TextBoxPresupuesto.ForeColor = COLOR_ACENTO
        TextBoxPresupuesto.Font = New Font("Segoe UI", 11.5F, FontStyle.Bold)
        TextBoxPresupuesto.BorderStyle = BorderStyle.FixedSingle
        TextBoxPresupuesto.TextAlign = HorizontalAlignment.Center
        TextBoxPresupuesto.BringToFront()

        ' Botón lupa pegado al TextBoxPresupuesto
        btnBuscarPresupuesto.Bounds = New Rectangle(margenIzq + 545, 28, 30, 28)
        btnBuscarPresupuesto.BackColor = BTN_AZUL_PRIMARIO
        btnBuscarPresupuesto.ForeColor = Color.White
        btnBuscarPresupuesto.FlatStyle = FlatStyle.Flat
        btnBuscarPresupuesto.FlatAppearance.BorderSize = 0
        btnBuscarPresupuesto.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        btnBuscarPresupuesto.Cursor = Cursors.Hand
        btnBuscarPresupuesto.Text = "🔍"
        btnBuscarPresupuesto.BackgroundImage = Nothing
        btnBuscarPresupuesto.BringToFront()

        ' Ocultar la antigua etiqueta "Presupuesto" suelta — el título ya cumple su función
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Label AndAlso ctrl.Name <> "LblTituloDoc" AndAlso ctrl.Name <> "LblNumDocEtiqueta" _
               AndAlso ctrl.Name <> "LineaTotales" AndAlso ctrl.Name <> "PanelTotalesResumen" _
               AndAlso ctrl.Name <> "LineaDivisoria" Then
                Dim texto As String = ctrl.Text.Trim().ToLower()
                If texto = "presupuesto" Then ctrl.Visible = False
            End If
        Next

        ' 2. ZONA DE FORMULARIO (Cliente, Fechas, Estado, etc.)
        Dim yInicioFormulario As Integer = alturaBanda + 25

        ' Tres bloques de cabecera con coordenadas
        Dim col1_X As Integer = margenIzq                  ' Bloque 1 - Cliente / Vendedor
        Dim col2_X As Integer = 600                        ' Bloque 2 - Fecha / Estado
        Dim col3_X As Integer = anchoForm - 470            ' Bloque 3 - Forma de pago

        Dim yFila1 As Integer = yInicioFormulario          ' Fila etiqueta superior
        Dim yFila2 As Integer = yInicioFormulario + 22     ' Fila valor (textbox)
        Dim yFila3 As Integer = yInicioFormulario + 60     ' Fila etiqueta segunda fila
        Dim yFila4 As Integer = yInicioFormulario + 82     ' Fila valor segunda fila
        Dim yFila5 As Integer = yInicioFormulario + 120    ' Tercera línea (forma pago)
        Dim yFila6 As Integer = yInicioFormulario + 142

        ' --- Reposicionamiento de etiquetas (no incluye "Presupuesto", ya oculto) ---
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
                    Case "cliente" : ctrl.Location = New Point(col1_X, yFila1)
                    Case "fecha" : ctrl.Location = New Point(col2_X, yFila1)
                    Case "vendedor" : ctrl.Location = New Point(col1_X, yFila3)
                    Case "estado" : ctrl.Location = New Point(col2_X, yFila3)
                    Case "observaciones" : ctrl.Location = New Point(col2_X + 160, yFila3)
                End Select
            End If
        Next

        ' --- Bloque 1: Cliente y Vendedor (izquierda) ---
        TextBoxIdCliente.Bounds = New Rectangle(col1_X, yFila2, 60, 25)
        TextBoxCliente.Bounds = New Rectangle(col1_X + 65, yFila2, 460, 25)
        TextBoxIdVendedor.Bounds = New Rectangle(col1_X, yFila4, 50, 25)
        TextBoxVendedor.Bounds = New Rectangle(col1_X + 55, yFila4, 470, 25)

        ' --- Bloque 2: Fecha, Estado y Observaciones (centro) ---
        TextBoxFecha.Bounds = New Rectangle(col2_X, yFila2, 140, 25)
        cboEstado.Bounds = New Rectangle(col2_X, yFila4, 140, 25)
        TextBoxObservaciones.Bounds = New Rectangle(col2_X + 160, yFila4, 280, 25)

        ' --- Tercera fila: Forma de pago (debajo, columna izquierda) ---
        lblFormaPago.Location = New Point(col1_X, yFila5)
        lblFormaPago.BackColor = Color.Transparent
        lblFormaPago.ForeColor = Color.WhiteSmoke
        lblFormaPago.Font = New Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
        cboFormaPago.Bounds = New Rectangle(col1_X, yFila6, 220, 25)
        cboFormaPago.Font = New Font("Segoe UI", 10.0F)

        ' 3. LÍNEA DIVISORIA ENTRE CABECERA Y GRID
        Dim lineaDivisoria As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LineaDivisoria")
        If lineaDivisoria Is Nothing Then
            lineaDivisoria = New Label() With {.Name = "LineaDivisoria", .BackColor = COLOR_LINEA_DIVISORIA, .Height = 2}
            Me.Controls.Add(lineaDivisoria)
        End If

        Dim yTabla As Integer = yFila6 + 50
        lineaDivisoria.Bounds = New Rectangle(margenIzq, yTabla - 18, anchoForm - (margenIzq * 2), 2)
        lineaDivisoria.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lineaDivisoria.BringToFront()

        ' 4. GRID DE LÍNEAS (ocupa todo el espacio central)
        Dim altoTabla As Integer = altoForm - yTabla - 145
        DataGridView1.Bounds = New Rectangle(margenIzq, yTabla, anchoForm - (margenIzq * 2), altoTabla)
        DataGridView1.BackgroundColor = Me.BackColor
        DataGridView1.BorderStyle = BorderStyle.None
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        ' 5. PANEL LATERAL DERECHO DE TOTALES (DESTACADO)
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

        ' Eliminar el viejo panel-Label si existía (limpieza de la versión anterior)
        Dim viejoPanel As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "PanelTotalesResumen")
        If viejoPanel IsNot Nothing Then Me.Controls.Remove(viejoPanel)

        ' Posiciones internas del panel (coordenadas absolutas en el formulario)
        Dim xPanelInt As Integer = xDerecha - panelAncho + 15
        Dim wPanelInt As Integer = panelAncho - 30

        ' Base imponible
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

        ' Línea separadora interna del panel
        Dim lineaTotal As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "LineaTotales")
        If lineaTotal Is Nothing Then
            lineaTotal = New Label() With {.Name = "LineaTotales", .BackColor = Color.FromArgb(80, 90, 105), .Height = 1}
            Me.Controls.Add(lineaTotal)
        End If
        lineaTotal.Bounds = New Rectangle(xPanelInt, yTotales + 68, wPanelInt, 1)
        lineaTotal.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        lineaTotal.BringToFront()

        ' TOTAL - destacado
        If Label7 IsNot Nothing Then
            Label7.Text = "TOTAL"
            Label7.BackColor = COLOR_PANEL_TOTALES
            Label7.ForeColor = COLOR_ACENTO
            Label7.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
            Label7.TextAlign = ContentAlignment.MiddleLeft
            Label7.Bounds = New Rectangle(xPanelInt, yTotales + 78, 150, 32)
            Label7.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If

        If TextBoxTotalPresup IsNot Nothing Then
            TextBoxTotalPresup.BackColor = COLOR_PANEL_TOTALES
            TextBoxTotalPresup.ForeColor = COLOR_ACENTO
            TextBoxTotalPresup.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold)
            TextBoxTotalPresup.BorderStyle = BorderStyle.None
            TextBoxTotalPresup.TextAlign = HorizontalAlignment.Right
            TextBoxTotalPresup.Bounds = New Rectangle(xPanelInt + wPanelInt - 180, yTotales + 78, 180, 32)
            TextBoxTotalPresup.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        End If

        ' 6. BARRA INFERIOR DE BOTONES AGRUPADOS
        Dim yBotones As Integer = DataGridView1.Bottom + 50

        ' Grupo A: acciones del documento (Guardar, Borrar, Nuevo)
        EstilizarBoton(ButtonGuardar, margenIzq, yBotones, BTN_AZUL_PRIMARIO, Color.White)
        EstilizarBoton(ButtonBorrar, margenIzq + 110, yBotones, BTN_ROJO_PELIGRO, Color.White)
        EstilizarBoton(ButtonNuevoPresup, margenIzq + 220, yBotones, BTN_AZUL_PRIMARIO, Color.White)

        ' Separador 1 (vertical)
        Dim sep1 As Label = ObtenerOCrearSeparador("SepGrupo1")
        sep1.Bounds = New Rectangle(margenIzq + 332, yBotones + 4, 1, 27)
        sep1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left

        ' Grupo B: gestión de líneas
        EstilizarBoton(ButtonNuevaLinea, margenIzq + 348, yBotones, BTN_VERDE_AÑADIR, Color.White)
        ButtonNuevaLinea.Text = "+ Añadir Línea" : ButtonNuevaLinea.Width = 120

        EstilizarBoton(ButtonBorrarLineas, margenIzq + 478, yBotones, BTN_GRIS_NEUTRO, Color.White)
        ButtonBorrarLineas.Text = "− Quitar Línea" : ButtonBorrarLineas.Width = 120

        ' Separador 2
        Dim sep2 As Label = ObtenerOCrearSeparador("SepGrupo2")
        sep2.Bounds = New Rectangle(margenIzq + 610, yBotones + 4, 1, 27)
        sep2.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left

        ' Grupo C: navegación entre documentos (anclados al lado izquierdo de la barra)
        EstilizarBoton(ButtonAnterior, margenIzq + 626, yBotones, COLOR_FONDO, Color.White)
        ButtonAnterior.FlatAppearance.BorderColor = COLOR_TEXTO_SECUNDARIO
        ButtonAnterior.FlatAppearance.BorderSize = 1
        ButtonAnterior.Text = "◀ Anterior"

        EstilizarBoton(ButtonSiguiente, margenIzq + 736, yBotones, COLOR_FONDO, Color.White)
        ButtonSiguiente.FlatAppearance.BorderColor = COLOR_TEXTO_SECUNDARIO
        ButtonSiguiente.FlatAppearance.BorderSize = 1
        ButtonSiguiente.Text = "Siguiente ▶"

        ' Anclamos los botones del lado izquierdo
        Dim botonesAbajo As Control() = {ButtonGuardar, ButtonBorrar, ButtonNuevoPresup, ButtonNuevaLinea, ButtonBorrarLineas, ButtonAnterior, ButtonSiguiente}
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

    ' MOTOR DE EXPORTACIÓN A PDF (DOCUMENTO INDIVIDUAL)
    Private Sub btnImprimir_Click(sender As Object, e As EventArgs) Handles btnImprimir.Click
        ' Validación: Que haya un documento cargado en pantalla
        ' (Cambia "txtPresupuesto" por el nombre real de tu caja de texto del Nº de Presupuesto)
        If String.IsNullOrWhiteSpace(TextBoxPresupuesto.Text) Then
            MessageBox.Show("No hay ningún documento cargado para imprimir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        docImprimir.DefaultPageSettings.Landscape = False ' A4 Vertical
        docImprimir.DefaultPageSettings.PaperSize = New Printing.PaperSize("A4", 827, 1169)
        docImprimir.DocumentName = "Presupuesto_" & TextBoxPresupuesto.Text

        Dim ppd As New PrintPreviewDialog()
        ppd.Document = docImprimir
        ppd.Width = 900 : ppd.Height = 700
        CType(ppd, Form).WindowState = FormWindowState.Maximized
        ppd.ShowDialog()
    End Sub

    Private Sub docImprimir_BeginPrint(sender As Object, e As Printing.PrintEventArgs) Handles docImprimir.BeginPrint
        _filaActualImpresion = 0 ' Reseteamos siempre que se va a generar el PDF
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

        ' --- FORMATOS DE ALINEACIÓN ---
        Dim formatoIzquierda As New StringFormat() With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Center}
        Dim formatoDerecha As New StringFormat() With {.Alignment = StringAlignment.Far, .LineAlignment = StringAlignment.Center}
        Dim formatoCentro As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}

        ' 1. CABECERA DEL DOCUMENTO (Solo página 1)
        If _filaActualImpresion = 0 Then
            ' A) LEER DATOS DE LA EMPRESA (BD)
            Dim empNombre As String = "EMPRESA NO CONFIGURADA"
            Dim empCIF As String = "", empDireccion As String = "", empCP_Pob As String = "", empTelefono As String = ""

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

            ' DIBUJAR DATOS DE EMPRESA 
            g.DrawString(empNombre, fEmpresa, bAzulCorporativo, margenIzq, yPos)
            g.DrawString("CIF: " & empCIF, fNormal, bGrisOscuro, margenIzq, yPos + 25)

            Dim bloqueContacto As String = empDireccion
            If empCP_Pob <> "" Then bloqueContacto &= vbCrLf & empCP_Pob
            If empTelefono <> "" Then bloqueContacto &= vbCrLf & "Tlf: " & empTelefono
            g.DrawString(bloqueContacto, fNormal, bGrisOscuro, margenIzq, yPos + 45)

            ' NUEVO: B) DATOS DEL DOCUMENTO (PRESUPUESTO)
            g.DrawString("PRESUPUESTO", fTituloDoc, bAzulCorporativo, margenDer, yPos, formatoDerecha)
            g.DrawString("Nº Documento: " & TextBoxPresupuesto.Text, fCabecera, bNegro, margenDer, yPos + 40, formatoDerecha)
            g.DrawString("Fecha: " & TextBoxFecha.Text, fNormal, bGrisOscuro, margenDer, yPos + 60, formatoDerecha)

            ' Trazabilidad y Comercial
            g.DrawString("Comercial: " & TextBoxVendedor.Text, fNormal, bGrisOscuro, margenDer, yPos + 85, formatoDerecha)
            g.DrawString("Validez de la oferta: 30 días", fNormal, bGrisOscuro, margenDer, yPos + 105, formatoDerecha)

            yPos += 130
            g.DrawLine(lapizGrueso, margenIzq, yPos, margenDer, yPos)
            yPos += 20

            ' C) LEER DATOS DEL CLIENTE (BD)
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

            ' DIBUJAR CAJA DEL CLIENTE 
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

        ' 4. TOTALES Y CONDICIONES (PIE DE PÁGINA)
        If _filaActualImpresion >= DataGridView1.Rows.Count Then
            yPos += 20

            Dim totalBase As String = TextBoxBase.Text.Replace("Base imponible :", "").Trim()
            Dim totalIVA As String = TextBoxIva.Text.Replace("I.V.A :", "").Trim()
            Dim totalDoc As String = TextBoxTotalPresup.Text.Replace("TOTAL :", "").Trim()

            Dim anchoCajaTotales As Integer = 300
            Dim xCaja As Integer = margenDer - anchoCajaTotales

            ' NUEVO: CONDICIONES Y OBSERVACIONES (Abajo a la izquierda)
            Dim yNotas As Integer = yPos

            ' Forma de Pago propuesta
            If cboFormaPago.Text <> "" Then
                g.DrawString("Forma de pago propuesta:", fCabecera, bAzulCorporativo, margenIzq, yNotas)
                g.DrawString(cboFormaPago.Text, fNormal, bNegro, margenIzq + 185, yNotas)
                yNotas += 35
            End If

            ' Observaciones del presupuesto
            If TextBoxObservaciones.Text.Trim() <> "" Then
                g.DrawString("Condiciones y Observaciones:", fCabecera, bAzulCorporativo, margenIzq, yNotas)
                Dim rectObs As New Rectangle(margenIzq, yNotas + 20, anchoCajaTotales - 20, 80)
                g.DrawString(TextBoxObservaciones.Text, fNormal, bGrisOscuro, rectObs)
            End If

            ' DIBUJAR TOTALES (Abajo a la derecha)
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
            g.DrawString("Gracias por confiar en nuestros servicios.", fFila, bGrisOscuro, margenIzq, yPos)

            e.HasMorePages = False
        End If
    End Sub
End Class
