Imports System.Data.SQLite

Public Class FrmBuscador
    ' lo dejo como string porque a veces es "PRE-001" y otras un numero a secas
    ' si lo pongo Integer me peta cuando es codigo con letras
    Public Property Resultado As String = ""

    ' aqui le digo desde fuera que tabla quiero buscar (presupuestos, clientes, etc)
    Public Property TablaABuscar As String = "Presupuestos"

    Private _dtDatos As DataTable

    Private Sub FrmBuscador_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ConfigurarGrid()
        CargarDatos()
        TextBoxBuscar.Focus()
    End Sub

    Private Sub ConfigurarGrid()
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.AllowUserToDeleteRows = False
        DataGridView1.ReadOnly = True
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.MultiSelect = False
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        DataGridView1.RowHeadersVisible = False
        DataGridView1.BackgroundColor = Color.White
        ' cuatro cosas para que no se vea horrible
        DataGridView1.DefaultCellStyle.Font = New Font("Segoe UI", 9)
        DataGridView1.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
    End Sub

    Private Sub CargarDatos()
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim sql As String = ""

            Select Case TablaABuscar

                ' --- VENTAS ---
                Case "Presupuestos"
                    Me.Text = "Buscar Presupuesto"
                    sql = "SELECT NumeroPresupuesto AS [Nº Doc], " &
                          "C.NombreFiscal AS Tercero, P.Fecha, P.Estado " &
                          "FROM Presupuestos P " &
                          "LEFT JOIN Clientes C ON P.CodigoCliente = C.CodigoCliente " &
                          "ORDER BY P.NumeroPresupuesto DESC"

                Case "Pedidos"
                    Me.Text = "Buscar Pedido"
                    sql = "SELECT NumeroPedido AS [Nº Doc], " &
                          "C.NombreFiscal AS Tercero, P.Fecha, P.Estado " &
                          "FROM Pedidos P " &
                          "LEFT JOIN Clientes C ON P.CodigoCliente = C.CodigoCliente " &
                          "ORDER BY P.NumeroPedido DESC"

                Case "Albaranes"
                    Me.Text = "Buscar Albaran"
                    sql = "SELECT NumeroAlbaran AS [Nº Doc], " &
                          "C.NombreFiscal AS Tercero, A.Fecha, A.Estado " &
                          "FROM Albaranes A " &
                          "LEFT JOIN Clientes C ON A.CodigoCliente = C.CodigoCliente " &
                          "ORDER BY A.NumeroAlbaran DESC"

                ' --- COMPRAS ---
                Case "PedidosCompra"
                    Me.Text = "Buscar Pedido de Compra"
                    sql = "SELECT NumeroPedidoCompra AS [Nº Doc], " &
                          "Pr.NombreFiscal AS Tercero, P.Fecha, P.Estado " &
                          "FROM PedidosCompra P " &
                          "LEFT JOIN Proveedores Pr ON P.ID_Proveedor = Pr.CodigoProveedor " &
                          "ORDER BY P.NumeroPedidoCompra DESC"

                Case "AlbaranesCompra"
                    Me.Text = "Buscar Albarán de Compra"
                    sql = "SELECT NumeroAlbaranCompra AS [Nº Doc], " &
                          "Pr.NombreFiscal AS Tercero, A.Fecha, A.Estado " &
                          "FROM AlbaranesCompra A " &
                          "LEFT JOIN Proveedores Pr ON A.ID_Proveedor = Pr.CodigoProveedor " &
                          "ORDER BY A.NumeroAlbaranCompra DESC"

                Case "FacturasCompra"
                    Me.Text = "Buscar Factura de Compra"
                    sql = "SELECT NumeroFacturaCompra AS [Nº Doc], " &
                          "Pr.NombreFiscal AS Tercero, F.FechaEmision AS Fecha, F.Estado " &
                          "FROM FacturasCompra F " &
                          "LEFT JOIN Proveedores Pr ON F.ID_Proveedor = Pr.CodigoProveedor " &
                          "ORDER BY F.NumeroFacturaCompra DESC"

                ' --- MAESTROS (clientes, articulos, proveedores) ---
                Case "Clientes"
                    Me.Text = "Buscar Cliente"
                    ' OJO: la clave de Clientes es CodigoCliente (texto), no ID_Cliente.
                    ' me tire un rato porque petaba con "no such column: ID_Cliente" hasta que me di cuenta
                    sql = "SELECT CodigoCliente AS ID, NombreFiscal AS Nombre, CIF, Poblacion FROM Clientes WHERE Activo = 1 ORDER BY NombreFiscal"

                Case "Articulos"
                    Me.Text = "Buscar Artículo"
                    sql = "SELECT ID_Articulo AS ID, CodigoReferencia AS Codigo, Descripcion, PrecioVenta, StockActual AS Stock FROM Articulos WHERE Activo = 1 ORDER BY Descripcion"

                Case "Proveedores"
                    Me.Text = "Buscar Proveedor"
                    sql = "SELECT CodigoProveedor AS ID, NombreFiscal AS Nombre, CIF, Poblacion FROM Proveedores WHERE Activo = 1 ORDER BY NombreFiscal"

            End Select

            If String.IsNullOrEmpty(sql) Then Return

            If conexion.State <> ConnectionState.Open Then conexion.Open()
            Dim da As New SQLiteDataAdapter(sql, conexion)
            _dtDatos = New DataTable()
            da.Fill(_dtDatos)

            DataGridView1.DataSource = _dtDatos

            ' la columna ID no la quiero ver en pantalla para los maestros, queda mas limpio
            ' que solo se vea el nombre y el CIF
            If DataGridView1.Columns.Contains("ID") AndAlso (TablaABuscar = "Clientes" Or TablaABuscar = "Articulos" Or TablaABuscar = "Proveedores") Then
                DataGridView1.Columns("ID").Visible = False
            End If

        Catch ex As Exception
            MessageBox.Show("Error al cargar datos: " & ex.Message)
        End Try
    End Sub

    Private Sub TextBoxBuscar_TextChanged(sender As Object, e As EventArgs) Handles TextBoxBuscar.TextChanged
        If _dtDatos Is Nothing Then Return
        Try
            Dim filtro As String = TextBoxBuscar.Text.Replace("'", "''")
            Select Case TablaABuscar
                Case "Presupuestos", "Pedidos", "Albaranes", "PedidosCompra", "AlbaranesCompra", "FacturasCompra"
                    _dtDatos.DefaultView.RowFilter = $"[Nº Doc] LIKE '%{filtro}%' OR Tercero LIKE '%{filtro}%'"
                Case "Clientes", "Proveedores"
                    _dtDatos.DefaultView.RowFilter = $"Nombre LIKE '%{filtro}%' OR CIF LIKE '%{filtro}%' OR ID LIKE '%{filtro}%'"
                Case "Articulos"
                    _dtDatos.DefaultView.RowFilter = $"Descripcion LIKE '%{filtro}%' OR Codigo LIKE '%{filtro}%'"
            End Select
        Catch
        End Try
    End Sub

    Private Sub DataGridView1_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellDoubleClick
        SeleccionarYSalir()
    End Sub

    Private Sub ButtonAceptar_Click(sender As Object, e As EventArgs) Handles ButtonAceptar.Click
        SeleccionarYSalir()
    End Sub

    ' Lógica de selección agnóstica (número o texto)
    Private Sub SeleccionarYSalir()
        If DataGridView1.SelectedRows.Count > 0 Then
            Dim fila = DataGridView1.SelectedRows(0)
            Dim valorRecuperado As String = ""

            ' Las tablas de documentos (ventas y compras) usan "Nº Doc" como clave de búsqueda;
            ' las tablas de maestros (clientes, artículos) usan el ID numérico interno.
            Select Case TablaABuscar
                Case "Presupuestos", "Pedidos", "Albaranes", "PedidosCompra", "AlbaranesCompra", "FacturasCompra"
                    valorRecuperado = fila.Cells("Nº Doc").Value.ToString()
                Case Else
                    ' Maestros: Clientes, Proveedores y Articulos. Todos exponen su PK como "ID".
                    valorRecuperado = fila.Cells("ID").Value.ToString()
            End Select

            If Not String.IsNullOrEmpty(valorRecuperado) Then
                Resultado = valorRecuperado
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If
        Else
            MessageBox.Show("Por favor, selecciona un registro.")
        End If
    End Sub

    Private Sub ButtonCancelar_Click(sender As Object, e As EventArgs) Handles ButtonCancelar.Click
        Resultado = ""
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
End Class
