<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PagHome
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim TreeNode1 As TreeNode = New TreeNode("Presupuestos")
        Dim TreeNode2 As TreeNode = New TreeNode("Pedidos")
        Dim TreeNode3 As TreeNode = New TreeNode("Albaranes")
        Dim TreeNode4 As TreeNode = New TreeNode("Facturas")
        Dim TreeNode5 As TreeNode = New TreeNode("Ventas", 4, 4, New TreeNode() {TreeNode1, TreeNode2, TreeNode3, TreeNode4})
        Dim TreeNode6 As TreeNode = New TreeNode("Pedidos a proveedor")
        Dim TreeNode7 As TreeNode = New TreeNode("Albaranes entrada")
        Dim TreeNode8 As TreeNode = New TreeNode("Facturas compra")
        Dim TreeNode9 As TreeNode = New TreeNode("Compras", 3, 3, New TreeNode() {TreeNode6, TreeNode7, TreeNode8})
        Dim TreeNode10 As TreeNode = New TreeNode("Clientes")
        Dim TreeNode11 As TreeNode = New TreeNode("Proveedores")
        Dim TreeNode12 As TreeNode = New TreeNode("Terceros", 2, 2, New TreeNode() {TreeNode10, TreeNode11})
        Dim TreeNode13 As TreeNode = New TreeNode("Articulos")
        Dim TreeNode14 As TreeNode = New TreeNode("Familias")
        Dim TreeNode15 As TreeNode = New TreeNode("Movimientos de almacen")
        Dim TreeNode16 As TreeNode = New TreeNode("Almacen", 1, 1, New TreeNode() {TreeNode13, TreeNode14, TreeNode15})
        Dim TreeNode17 As TreeNode = New TreeNode("Vendedores")
        Dim TreeNode18 As TreeNode = New TreeNode("Formas de pago")
        Dim TreeNode19 As TreeNode = New TreeNode("Rutas")
        Dim TreeNode20 As TreeNode = New TreeNode("Agencias")
        Dim TreeNode21 As TreeNode = New TreeNode("Tablas", 0, 0, New TreeNode() {TreeNode17, TreeNode18, TreeNode19, TreeNode20})
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PagHome))
        TvNavegacion = New TreeView()
        ImageList1 = New ImageList(components)
        PictureBoxLogoHome = New PictureBox()
        MenuStripVacio = New MenuStrip()
        Informes = New ToolStripMenuItem()
        Configuracion = New ToolStripMenuItem()
        ToolStripTextBox1 = New ToolStripTextBox()
        NombreEmpresa = New ToolStripTextBox()
        Panel = New Panel()
        CType(PictureBoxLogoHome, ComponentModel.ISupportInitialize).BeginInit()
        MenuStripVacio.SuspendLayout()
        SuspendLayout()
        ' 
        ' TvNavegacion
        ' 
        TvNavegacion.BackColor = Color.White
        TvNavegacion.BorderStyle = BorderStyle.None
        TvNavegacion.Dock = DockStyle.Left
        TvNavegacion.DrawMode = TreeViewDrawMode.OwnerDrawText
        TvNavegacion.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TvNavegacion.FullRowSelect = True
        TvNavegacion.ImageIndex = 0
        TvNavegacion.ImageList = ImageList1
        TvNavegacion.Indent = 19
        TvNavegacion.ItemHeight = 45
        TvNavegacion.Location = New Point(0, 31)
        TvNavegacion.Margin = New Padding(0)
        TvNavegacion.Name = "TvNavegacion"
        TreeNode1.ImageKey = "(valor predeterminado)"
        TreeNode1.Name = "NodoPresupuesto"
        TreeNode1.SelectedImageKey = "(valor predeterminado)"
        TreeNode1.Text = "Presupuestos"
        TreeNode2.Name = "NodoPedidos"
        TreeNode2.Text = "Pedidos"
        TreeNode3.Name = "NodoAlbaranes"
        TreeNode3.Text = "Albaranes"
        TreeNode4.Name = "NodoFacturas"
        TreeNode4.Text = "Facturas"
        TreeNode5.ImageIndex = 4
        TreeNode5.Name = "NodoVentas"
        TreeNode5.SelectedImageIndex = 4
        TreeNode5.Text = "Ventas"
        TreeNode6.Name = "NodoPedProvee"
        TreeNode6.Text = "Pedidos a proveedor"
        TreeNode7.Name = "NodoAlbaEntrada"
        TreeNode7.Text = "Albaranes entrada"
        TreeNode8.Name = "NodoFacturaComp"
        TreeNode8.Text = "Facturas compra"
        TreeNode9.ImageIndex = 3
        TreeNode9.Name = "NodoCompras"
        TreeNode9.SelectedImageIndex = 3
        TreeNode9.Text = "Compras"
        TreeNode10.Name = "NodoCliente"
        TreeNode10.Text = "Clientes"
        TreeNode11.Name = "NodoProveedor"
        TreeNode11.Text = "Proveedores"
        TreeNode12.ImageIndex = 2
        TreeNode12.Name = "NodoTerceros"
        TreeNode12.SelectedImageIndex = 2
        TreeNode12.Text = "Terceros"
        TreeNode13.Name = "NodoArticulos"
        TreeNode13.Text = "Articulos"
        TreeNode14.Name = "NodoFamilias"
        TreeNode14.Text = "Familias"
        TreeNode15.Name = "NodoMovAlmacen"
        TreeNode15.Text = "Movimientos de almacen"
        TreeNode16.ImageIndex = 1
        TreeNode16.Name = "NodoAlmacen"
        TreeNode16.SelectedImageIndex = 1
        TreeNode16.Text = "Almacen"
        TreeNode17.Name = "NodoVendedor"
        TreeNode17.Text = "Vendedores"
        TreeNode18.Name = "NodoFPago"
        TreeNode18.Text = "Formas de pago"
        TreeNode19.Name = "NodoRutas"
        TreeNode19.Text = "Rutas"
        TreeNode20.Name = "NodoAgencias"
        TreeNode20.Text = "Agencias"
        TreeNode21.ImageIndex = 0
        TreeNode21.Name = "NodoTablas"
        TreeNode21.SelectedImageIndex = 0
        TreeNode21.Text = "Tablas"
        TvNavegacion.Nodes.AddRange(New TreeNode() {TreeNode5, TreeNode9, TreeNode12, TreeNode16, TreeNode21})
        TvNavegacion.SelectedImageIndex = 0
        TvNavegacion.ShowLines = False
        TvNavegacion.Size = New Size(249, 757)
        TvNavegacion.TabIndex = 0
        ' 
        ' ImageList1
        ' 
        ImageList1.ColorDepth = ColorDepth.Depth32Bit
        ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), ImageListStreamer)
        ImageList1.TransparentColor = Color.Transparent
        ImageList1.Images.SetKeyName(0, "red.png")
        ImageList1.Images.SetKeyName(1, "almacen.png")
        ImageList1.Images.SetKeyName(2, "usuario.png")
        ImageList1.Images.SetKeyName(3, "carrito-de-compras.png")
        ImageList1.Images.SetKeyName(4, "ventas.png")
        ' 
        ' PictureBoxLogoHome
        ' 
        PictureBoxLogoHome.Location = New Point(252, 833)
        PictureBoxLogoHome.Name = "PictureBoxLogoHome"
        PictureBoxLogoHome.Size = New Size(415, 164)
        PictureBoxLogoHome.TabIndex = 1
        PictureBoxLogoHome.TabStop = False
        ' 
        ' MenuStripVacio
        ' 
        MenuStripVacio.BackColor = Color.Silver
        MenuStripVacio.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        MenuStripVacio.ImageScalingSize = New Size(20, 20)
        MenuStripVacio.Items.AddRange(New ToolStripItem() {Informes, Configuracion, ToolStripTextBox1, NombreEmpresa})
        MenuStripVacio.Location = New Point(0, 0)
        MenuStripVacio.Name = "MenuStripVacio"
        MenuStripVacio.Padding = New Padding(10, 5, 10, 5)
        MenuStripVacio.Size = New Size(1536, 31)
        MenuStripVacio.TabIndex = 2
        MenuStripVacio.Text = "                       "
        ' 
        ' Informes
        ' 
        Informes.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Informes.Name = "Informes"
        Informes.Padding = New Padding(10, 0, 10, 0)
        Informes.Size = New Size(87, 21)
        Informes.Text = "Informes"
        ' 
        ' Configuracion
        ' 
        Configuracion.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Configuracion.Name = "Configuracion"
        Configuracion.Padding = New Padding(10, 0, 10, 0)
        Configuracion.Size = New Size(119, 21)
        Configuracion.Text = "Configuracion"
        ' 
        ' ToolStripTextBox1
        ' 
        ToolStripTextBox1.BackColor = Color.Silver
        ToolStripTextBox1.BorderStyle = BorderStyle.None
        ToolStripTextBox1.Name = "ToolStripTextBox1"
        ToolStripTextBox1.Size = New Size(100, 21)
        ' 
        ' NombreEmpresa
        ' 
        NombreEmpresa.BackColor = Color.Silver
        NombreEmpresa.BorderStyle = BorderStyle.None
        NombreEmpresa.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        NombreEmpresa.Name = "NombreEmpresa"
        NombreEmpresa.Size = New Size(200, 21)
        NombreEmpresa.Text = "NOMBREEMPRESA"
        NombreEmpresa.TextBoxTextAlign = HorizontalAlignment.Right
        ' 
        ' Panel
        ' 
        Panel.BackColor = Color.Transparent
        Panel.Dock = DockStyle.Fill
        Panel.ForeColor = Color.WhiteSmoke
        Panel.Location = New Point(249, 31)
        Panel.Name = "Panel"
        Panel.Size = New Size(1287, 757)
        Panel.TabIndex = 3
        ' 
        ' PagHome
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        ClientSize = New Size(1536, 788)
        Controls.Add(Panel)
        Controls.Add(PictureBoxLogoHome)
        Controls.Add(TvNavegacion)
        Controls.Add(MenuStripVacio)
        FormBorderStyle = FormBorderStyle.Fixed3D
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MainMenuStrip = MenuStripVacio
        Name = "PagHome"
        StartPosition = FormStartPosition.CenterScreen
        Text = "OPTIMA"
        WindowState = FormWindowState.Maximized
        CType(PictureBoxLogoHome, ComponentModel.ISupportInitialize).EndInit()
        MenuStripVacio.ResumeLayout(False)
        MenuStripVacio.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents TvNavegacion As TreeView
    Friend WithEvents PictureBoxLogoHome As PictureBox
    Friend WithEvents MenuStripVacio As MenuStrip
    Friend WithEvents Informes As ToolStripMenuItem
    Friend WithEvents Configuracion As ToolStripMenuItem
    Friend WithEvents NombreEmpresa As ToolStripTextBox
    Friend WithEvents ToolStripTextBox1 As ToolStripTextBox
    Friend WithEvents Panel As Panel
    Friend WithEvents ImageList1 As ImageList
End Class