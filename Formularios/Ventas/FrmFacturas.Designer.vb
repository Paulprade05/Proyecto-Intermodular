<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmFacturas
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmFacturas))
        TabControlModerno2 = New TabControlModerno()
        TabPage5 = New TabPage()
        Label25 = New Label()
        Label26 = New Label()
        TextBoxCP = New TextBox()
        Label27 = New Label()
        TextBoxPeso = New TextBox()
        Label28 = New Label()
        Label29 = New Label()
        TextBoxDireccion = New TextBox()
        TextBoxBultos = New TextBox()
        Label30 = New Label()
        ComboBoxPortes = New ComboBox()
        TextBoxPoblacion = New TextBox()
        TextBoxTracking = New TextBox()
        Label31 = New Label()
        TabPage6 = New TabPage()
        TextBoxAlbaranOrigen = New TextBox()
        Label20 = New Label()
        btnImportarAlbaran = New Button()
        TextBoxObservaciones = New TextBox()
        Label21 = New Label()
        TextBox5 = New TextBox()
        Button4 = New Button()
        Label17 = New Label()
        cboAgencias = New ComboBox()
        Label9 = New Label()
        DateTimePickerFecha = New DateTimePicker()
        LabelStock = New Label()
        Label7 = New Label()
        LabelIva = New Label()
        LabelBase = New Label()
        TextBoxTotalAlb = New TextBox()
        TextBoxIva = New TextBox()
        TextBoxBase = New TextBox()
        ButtonNuevaLinea = New Button()
        ButtonBorrarLineas = New Button()
        ButtonSiguiente = New Button()
        ButtonAnterior = New Button()
        ButtonNuevoPed = New Button()
        ButtonBorrar = New Button()
        DataGridView1 = New DataGridView()
        ConexionBDBindingSource1 = New BindingSource(components)
        TextBoxVendedor = New TextBox()
        TextBoxIdVendedor = New TextBox()
        Label4 = New Label()
        Label6 = New Label()
        TextBoxFecha = New TextBox()
        TextBoxIdCliente = New TextBox()
        ButtonGuardar = New Button()
        Button1 = New Button()
        TextBoxCliente = New TextBox()
        TextBoxFactura = New TextBox()
        Label2 = New Label()
        Label1 = New Label()
        ConexionBDBindingSource = New BindingSource(components)
        Label3 = New Label()
        TabControlModerno2.SuspendLayout()
        TabPage5.SuspendLayout()
        TabPage6.SuspendLayout()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' TabControlModerno2
        ' 
        TabControlModerno2.Controls.Add(TabPage5)
        TabControlModerno2.Controls.Add(TabPage6)
        TabControlModerno2.DrawMode = TabDrawMode.OwnerDrawFixed
        TabControlModerno2.ItemSize = New Size(120, 30)
        TabControlModerno2.Location = New Point(1275, 193)
        TabControlModerno2.Name = "TabControlModerno2"
        TabControlModerno2.SelectedIndex = 0
        TabControlModerno2.Size = New Size(335, 213)
        TabControlModerno2.SizeMode = TabSizeMode.Fixed
        TabControlModerno2.TabIndex = 163
        ' 
        ' TabPage5
        ' 
        TabPage5.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TabPage5.Controls.Add(Label25)
        TabPage5.Controls.Add(Label26)
        TabPage5.Controls.Add(TextBoxCP)
        TabPage5.Controls.Add(Label27)
        TabPage5.Controls.Add(TextBoxPeso)
        TabPage5.Controls.Add(Label28)
        TabPage5.Controls.Add(Label29)
        TabPage5.Controls.Add(TextBoxDireccion)
        TabPage5.Controls.Add(TextBoxBultos)
        TabPage5.Controls.Add(Label30)
        TabPage5.Controls.Add(ComboBoxPortes)
        TabPage5.Controls.Add(TextBoxPoblacion)
        TabPage5.Controls.Add(TextBoxTracking)
        TabPage5.Controls.Add(Label31)
        TabPage5.ForeColor = Color.WhiteSmoke
        TabPage5.Location = New Point(4, 34)
        TabPage5.Name = "TabPage5"
        TabPage5.Padding = New Padding(3)
        TabPage5.Size = New Size(327, 175)
        TabPage5.TabIndex = 0
        TabPage5.Text = "Logistica y Envío"
        ' 
        ' Label25
        ' 
        Label25.AutoSize = True
        Label25.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label25.Location = New Point(9, 8)
        Label25.Name = "Label25"
        Label25.Size = New Size(74, 20)
        Label25.TabIndex = 138
        Label25.Text = "Direccion"
        ' 
        ' Label26
        ' 
        Label26.AutoSize = True
        Label26.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label26.Location = New Point(9, 121)
        Label26.Name = "Label26"
        Label26.Size = New Size(105, 20)
        Label26.TabIndex = 134
        Label26.Text = "Codigo postal"
        ' 
        ' TextBoxCP
        ' 
        TextBoxCP.BackColor = Color.White
        TextBoxCP.Location = New Point(6, 141)
        TextBoxCP.Name = "TextBoxCP"
        TextBoxCP.Size = New Size(114, 25)
        TextBoxCP.TabIndex = 135
        ' 
        ' Label27
        ' 
        Label27.AutoSize = True
        Label27.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label27.Location = New Point(9, 60)
        Label27.Name = "Label27"
        Label27.Size = New Size(77, 20)
        Label27.TabIndex = 136
        Label27.Text = "Poblacion"
        ' 
        ' TextBoxPeso
        ' 
        TextBoxPeso.Location = New Point(198, 141)
        TextBoxPeso.Name = "TextBoxPeso"
        TextBoxPeso.Size = New Size(54, 25)
        TextBoxPeso.TabIndex = 129
        ' 
        ' Label28
        ' 
        Label28.AutoSize = True
        Label28.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label28.Location = New Point(141, 60)
        Label28.Name = "Label28"
        Label28.Size = New Size(54, 20)
        Label28.TabIndex = 133
        Label28.Text = "Portes"
        ' 
        ' Label29
        ' 
        Label29.AutoSize = True
        Label29.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label29.Location = New Point(201, 121)
        Label29.Name = "Label29"
        Label29.Size = New Size(42, 20)
        Label29.TabIndex = 128
        Label29.Text = "Peso"
        ' 
        ' TextBoxDireccion
        ' 
        TextBoxDireccion.BackColor = Color.White
        TextBoxDireccion.Location = New Point(6, 28)
        TextBoxDireccion.Name = "TextBoxDireccion"
        TextBoxDireccion.Size = New Size(114, 25)
        TextBoxDireccion.TabIndex = 139
        ' 
        ' TextBoxBultos
        ' 
        TextBoxBultos.Location = New Point(138, 141)
        TextBoxBultos.Name = "TextBoxBultos"
        TextBoxBultos.Size = New Size(54, 25)
        TextBoxBultos.TabIndex = 127
        ' 
        ' Label30
        ' 
        Label30.AutoSize = True
        Label30.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label30.Location = New Point(141, 121)
        Label30.Name = "Label30"
        Label30.Size = New Size(54, 20)
        Label30.TabIndex = 126
        Label30.Text = "Bultos"
        ' 
        ' ComboBoxPortes
        ' 
        ComboBoxPortes.FormattingEnabled = True
        ComboBoxPortes.Location = New Point(138, 80)
        ComboBoxPortes.Name = "ComboBoxPortes"
        ComboBoxPortes.Size = New Size(121, 25)
        ComboBoxPortes.TabIndex = 132
        ' 
        ' TextBoxPoblacion
        ' 
        TextBoxPoblacion.BackColor = Color.White
        TextBoxPoblacion.Location = New Point(6, 80)
        TextBoxPoblacion.Name = "TextBoxPoblacion"
        TextBoxPoblacion.Size = New Size(114, 25)
        TextBoxPoblacion.TabIndex = 137
        ' 
        ' TextBoxTracking
        ' 
        TextBoxTracking.Location = New Point(138, 28)
        TextBoxTracking.Name = "TextBoxTracking"
        TextBoxTracking.Size = New Size(170, 25)
        TextBoxTracking.TabIndex = 131
        ' 
        ' Label31
        ' 
        Label31.AutoSize = True
        Label31.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label31.Location = New Point(141, 8)
        Label31.Name = "Label31"
        Label31.Size = New Size(170, 20)
        Label31.TabIndex = 130
        Label31.Text = "Codigo de seguimiento"
        ' 
        ' TabPage6
        ' 
        TabPage6.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TabPage6.Controls.Add(TextBoxAlbaranOrigen)
        TabPage6.Controls.Add(Label20)
        TabPage6.Controls.Add(btnImportarAlbaran)
        TabPage6.Controls.Add(TextBoxObservaciones)
        TabPage6.Controls.Add(Label21)
        TabPage6.Controls.Add(TextBox5)
        TabPage6.Controls.Add(Button4)
        TabPage6.Location = New Point(4, 34)
        TabPage6.Name = "TabPage6"
        TabPage6.Padding = New Padding(3)
        TabPage6.Size = New Size(327, 175)
        TabPage6.TabIndex = 1
        TabPage6.Text = "Detalles y Origen"
        ' 
        ' TextBoxAlbaranOrigen
        ' 
        TextBoxAlbaranOrigen.Location = New Point(6, 26)
        TextBoxAlbaranOrigen.Name = "TextBoxAlbaranOrigen"
        TextBoxAlbaranOrigen.Size = New Size(114, 25)
        TextBoxAlbaranOrigen.TabIndex = 124
        ' 
        ' Label20
        ' 
        Label20.AutoSize = True
        Label20.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label20.Location = New Point(9, 6)
        Label20.Name = "Label20"
        Label20.Size = New Size(57, 20)
        Label20.TabIndex = 125
        Label20.Text = "Pedido"
        ' 
        ' btnImportarAlbaran
        ' 
        btnImportarAlbaran.BackgroundImage = CType(resources.GetObject("btnImportarAlbaran.BackgroundImage"), Image)
        btnImportarAlbaran.BackgroundImageLayout = ImageLayout.Zoom
        btnImportarAlbaran.Location = New Point(126, 26)
        btnImportarAlbaran.Name = "btnImportarAlbaran"
        btnImportarAlbaran.Size = New Size(29, 25)
        btnImportarAlbaran.TabIndex = 126
        btnImportarAlbaran.UseVisualStyleBackColor = True
        ' 
        ' TextBoxObservaciones
        ' 
        TextBoxObservaciones.Location = New Point(6, 91)
        TextBoxObservaciones.Multiline = True
        TextBoxObservaciones.Name = "TextBoxObservaciones"
        TextBoxObservaciones.Size = New Size(315, 78)
        TextBoxObservaciones.TabIndex = 123
        ' 
        ' Label21
        ' 
        Label21.AutoSize = True
        Label21.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label21.Location = New Point(9, 70)
        Label21.Name = "Label21"
        Label21.Size = New Size(111, 20)
        Label21.TabIndex = 122
        Label21.Text = "Observaciones"
        ' 
        ' TextBox5
        ' 
        TextBox5.Location = New Point(6, 27)
        TextBox5.Name = "TextBox5"
        TextBox5.Size = New Size(114, 25)
        TextBox5.TabIndex = 119
        ' 
        ' Button4
        ' 
        Button4.BackgroundImage = CType(resources.GetObject("Button4.BackgroundImage"), Image)
        Button4.BackgroundImageLayout = ImageLayout.Zoom
        Button4.Location = New Point(126, 27)
        Button4.Name = "Button4"
        Button4.Size = New Size(29, 25)
        Button4.TabIndex = 121
        Button4.UseVisualStyleBackColor = True
        ' 
        ' Label17
        ' 
        Label17.AutoSize = True
        Label17.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label17.Location = New Point(603, 125)
        Label17.Name = "Label17"
        Label17.Size = New Size(65, 20)
        Label17.TabIndex = 162
        Label17.Text = "Agencia"
        ' 
        ' cboAgencias
        ' 
        cboAgencias.FormattingEnabled = True
        cboAgencias.Location = New Point(604, 148)
        cboAgencias.Name = "cboAgencias"
        cboAgencias.Size = New Size(121, 25)
        cboAgencias.TabIndex = 164
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label9.Location = New Point(725, 74)
        Label9.Name = "Label9"
        Label9.Size = New Size(107, 20)
        Label9.TabIndex = 161
        Label9.Text = "Fecha entrega"
        ' 
        ' DateTimePickerFecha
        ' 
        DateTimePickerFecha.Format = DateTimePickerFormat.Short
        DateTimePickerFecha.Location = New Point(728, 97)
        DateTimePickerFecha.Name = "DateTimePickerFecha"
        DateTimePickerFecha.Size = New Size(125, 25)
        DateTimePickerFecha.TabIndex = 160
        DateTimePickerFecha.Value = New Date(2026, 1, 30, 0, 0, 0, 0)
        ' 
        ' LabelStock
        ' 
        LabelStock.AutoSize = True
        LabelStock.Font = New Font("Segoe UI Black", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelStock.Location = New Point(1270, 435)
        LabelStock.Name = "LabelStock"
        LabelStock.Size = New Size(120, 17)
        LabelStock.TabIndex = 159
        LabelStock.Text = "Stock disponible:-"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label7.Location = New Point(1270, 562)
        Label7.Name = "Label7"
        Label7.Size = New Size(76, 21)
        Label7.TabIndex = 157
        Label7.Text = "TOTAL : "
        ' 
        ' LabelIva
        ' 
        LabelIva.AutoSize = True
        LabelIva.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelIva.Location = New Point(1270, 516)
        LabelIva.Name = "LabelIva"
        LabelIva.Size = New Size(49, 20)
        LabelIva.TabIndex = 156
        LabelIva.Text = "I.V.A :"
        ' 
        ' LabelBase
        ' 
        LabelBase.AutoSize = True
        LabelBase.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelBase.Location = New Point(1270, 473)
        LabelBase.Name = "LabelBase"
        LabelBase.Size = New Size(124, 20)
        LabelBase.TabIndex = 155
        LabelBase.Text = "Base imponible :"
        ' 
        ' TextBoxTotalAlb
        ' 
        TextBoxTotalAlb.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxTotalAlb.BorderStyle = BorderStyle.None
        TextBoxTotalAlb.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxTotalAlb.ForeColor = Color.WhiteSmoke
        TextBoxTotalAlb.Location = New Point(1300, 561)
        TextBoxTotalAlb.Name = "TextBoxTotalAlb"
        TextBoxTotalAlb.ReadOnly = True
        TextBoxTotalAlb.Size = New Size(256, 22)
        TextBoxTotalAlb.TabIndex = 154
        TextBoxTotalAlb.TextAlign = HorizontalAlignment.Right
        ' 
        ' TextBoxIva
        ' 
        TextBoxIva.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxIva.BorderStyle = BorderStyle.None
        TextBoxIva.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxIva.ForeColor = Color.WhiteSmoke
        TextBoxIva.Location = New Point(1300, 516)
        TextBoxIva.Name = "TextBoxIva"
        TextBoxIva.ReadOnly = True
        TextBoxIva.Size = New Size(256, 20)
        TextBoxIva.TabIndex = 153
        TextBoxIva.TextAlign = HorizontalAlignment.Right
        ' 
        ' TextBoxBase
        ' 
        TextBoxBase.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxBase.BorderStyle = BorderStyle.None
        TextBoxBase.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxBase.ForeColor = Color.WhiteSmoke
        TextBoxBase.Location = New Point(1300, 473)
        TextBoxBase.Name = "TextBoxBase"
        TextBoxBase.ReadOnly = True
        TextBoxBase.Size = New Size(256, 20)
        TextBoxBase.TabIndex = 152
        TextBoxBase.TextAlign = HorizontalAlignment.Right
        ' 
        ' ButtonNuevaLinea
        ' 
        ButtonNuevaLinea.BackColor = Color.FromArgb(CByte(128), CByte(255), CByte(255))
        ButtonNuevaLinea.FlatStyle = FlatStyle.Flat
        ButtonNuevaLinea.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonNuevaLinea.ForeColor = Color.Black
        ButtonNuevaLinea.Location = New Point(728, 657)
        ButtonNuevaLinea.Name = "ButtonNuevaLinea"
        ButtonNuevaLinea.Size = New Size(113, 29)
        ButtonNuevaLinea.TabIndex = 151
        ButtonNuevaLinea.Text = "Nueva linea"
        ButtonNuevaLinea.UseVisualStyleBackColor = False
        ' 
        ' ButtonBorrarLineas
        ' 
        ButtonBorrarLineas.BackColor = Color.FromArgb(CByte(255), CByte(128), CByte(128))
        ButtonBorrarLineas.FlatStyle = FlatStyle.Flat
        ButtonBorrarLineas.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonBorrarLineas.ForeColor = Color.Black
        ButtonBorrarLineas.Location = New Point(608, 657)
        ButtonBorrarLineas.Name = "ButtonBorrarLineas"
        ButtonBorrarLineas.Size = New Size(113, 29)
        ButtonBorrarLineas.TabIndex = 150
        ButtonBorrarLineas.Text = "Borrar lineas"
        ButtonBorrarLineas.UseVisualStyleBackColor = False
        ' 
        ' ButtonSiguiente
        ' 
        ButtonSiguiente.BackColor = Color.Silver
        ButtonSiguiente.FlatStyle = FlatStyle.Flat
        ButtonSiguiente.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonSiguiente.ForeColor = Color.Black
        ButtonSiguiente.Location = New Point(1161, 657)
        ButtonSiguiente.Name = "ButtonSiguiente"
        ButtonSiguiente.Size = New Size(113, 29)
        ButtonSiguiente.TabIndex = 149
        ButtonSiguiente.Text = "Siguiente"
        ButtonSiguiente.UseVisualStyleBackColor = False
        ' 
        ' ButtonAnterior
        ' 
        ButtonAnterior.BackColor = Color.Silver
        ButtonAnterior.FlatStyle = FlatStyle.Flat
        ButtonAnterior.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonAnterior.ForeColor = Color.Black
        ButtonAnterior.Location = New Point(1040, 657)
        ButtonAnterior.Name = "ButtonAnterior"
        ButtonAnterior.Size = New Size(113, 29)
        ButtonAnterior.TabIndex = 148
        ButtonAnterior.Text = "Anterior"
        ButtonAnterior.UseVisualStyleBackColor = False
        ' 
        ' ButtonNuevoPed
        ' 
        ButtonNuevoPed.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(255))
        ButtonNuevoPed.FlatStyle = FlatStyle.Flat
        ButtonNuevoPed.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonNuevoPed.ForeColor = Color.Black
        ButtonNuevoPed.Location = New Point(315, 657)
        ButtonNuevoPed.Name = "ButtonNuevoPed"
        ButtonNuevoPed.Size = New Size(113, 29)
        ButtonNuevoPed.TabIndex = 147
        ButtonNuevoPed.Text = "Nuevo"
        ButtonNuevoPed.UseVisualStyleBackColor = False
        ' 
        ' ButtonBorrar
        ' 
        ButtonBorrar.BackColor = Color.FromArgb(CByte(255), CByte(192), CByte(192))
        ButtonBorrar.FlatStyle = FlatStyle.Flat
        ButtonBorrar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonBorrar.ForeColor = Color.Black
        ButtonBorrar.Location = New Point(195, 657)
        ButtonBorrar.Name = "ButtonBorrar"
        ButtonBorrar.Size = New Size(113, 29)
        ButtonBorrar.TabIndex = 146
        ButtonBorrar.Text = "Borrar"
        ButtonBorrar.UseVisualStyleBackColor = False
        ' 
        ' DataGridView1
        ' 
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.DataSource = ConexionBDBindingSource1
        DataGridView1.Location = New Point(74, 193)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.RowHeadersWidth = 51
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.Size = New Size(1195, 417)
        DataGridView1.TabIndex = 144
        ' 
        ' ConexionBDBindingSource1
        ' 
        ConexionBDBindingSource1.DataSource = GetType(ConexionBD)
        ' 
        ' TextBoxVendedor
        ' 
        TextBoxVendedor.Location = New Point(137, 148)
        TextBoxVendedor.Name = "TextBoxVendedor"
        TextBoxVendedor.Size = New Size(305, 25)
        TextBoxVendedor.TabIndex = 143
        ' 
        ' TextBoxIdVendedor
        ' 
        TextBoxIdVendedor.Location = New Point(75, 148)
        TextBoxIdVendedor.Name = "TextBoxIdVendedor"
        TextBoxIdVendedor.Size = New Size(54, 25)
        TextBoxIdVendedor.TabIndex = 142
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label4.Location = New Point(75, 125)
        Label4.Name = "Label4"
        Label4.Size = New Size(76, 20)
        Label4.TabIndex = 141
        Label4.Text = "Vendedor"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.Location = New Point(450, 125)
        Label6.Name = "Label6"
        Label6.Size = New Size(56, 20)
        Label6.TabIndex = 139
        Label6.Text = "Estado"
        ' 
        ' TextBoxFecha
        ' 
        TextBoxFecha.Location = New Point(608, 97)
        TextBoxFecha.Name = "TextBoxFecha"
        TextBoxFecha.Size = New Size(114, 25)
        TextBoxFecha.TabIndex = 138
        ' 
        ' TextBoxIdCliente
        ' 
        TextBoxIdCliente.Location = New Point(237, 97)
        TextBoxIdCliente.Name = "TextBoxIdCliente"
        TextBoxIdCliente.Size = New Size(54, 25)
        TextBoxIdCliente.TabIndex = 136
        ' 
        ' ButtonGuardar
        ' 
        ButtonGuardar.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        ButtonGuardar.FlatStyle = FlatStyle.Flat
        ButtonGuardar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonGuardar.ForeColor = Color.Black
        ButtonGuardar.Location = New Point(75, 657)
        ButtonGuardar.Name = "ButtonGuardar"
        ButtonGuardar.Size = New Size(113, 29)
        ButtonGuardar.TabIndex = 145
        ButtonGuardar.Text = "Guardar"
        ButtonGuardar.UseVisualStyleBackColor = False
        ' 
        ' Button1
        ' 
        Button1.BackgroundImage = CType(resources.GetObject("Button1.BackgroundImage"), Image)
        Button1.BackgroundImageLayout = ImageLayout.Zoom
        Button1.Location = New Point(195, 97)
        Button1.Name = "Button1"
        Button1.Size = New Size(29, 25)
        Button1.TabIndex = 158
        Button1.UseVisualStyleBackColor = True
        ' 
        ' TextBoxCliente
        ' 
        TextBoxCliente.Location = New Point(299, 97)
        TextBoxCliente.Name = "TextBoxCliente"
        TextBoxCliente.Size = New Size(305, 25)
        TextBoxCliente.TabIndex = 137
        ' 
        ' TextBoxFactura
        ' 
        TextBoxFactura.Location = New Point(75, 97)
        TextBoxFactura.Name = "TextBoxFactura"
        TextBoxFactura.Size = New Size(114, 25)
        TextBoxFactura.TabIndex = 135
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.Location = New Point(237, 74)
        Label2.Name = "Label2"
        Label2.Size = New Size(57, 20)
        Label2.TabIndex = 133
        Label2.Text = "Cliente"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(75, 74)
        Label1.Name = "Label1"
        Label1.Size = New Size(64, 20)
        Label1.TabIndex = 132
        Label1.Text = "Albaran"
        ' 
        ' ConexionBDBindingSource
        ' 
        ConexionBDBindingSource.DataSource = GetType(ConexionBD)
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label3.Location = New Point(608, 74)
        Label3.Name = "Label3"
        Label3.Size = New Size(49, 20)
        Label3.TabIndex = 134
        Label3.Text = "Fecha"
        ' 
        ' FrmFacturas
        ' 
        AutoScaleDimensions = New SizeF(8F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        ClientSize = New Size(1684, 761)
        ControlBox = False
        Controls.Add(TabControlModerno2)
        Controls.Add(Label17)
        Controls.Add(cboAgencias)
        Controls.Add(Label9)
        Controls.Add(DateTimePickerFecha)
        Controls.Add(LabelStock)
        Controls.Add(Label7)
        Controls.Add(LabelIva)
        Controls.Add(LabelBase)
        Controls.Add(TextBoxTotalAlb)
        Controls.Add(TextBoxIva)
        Controls.Add(TextBoxBase)
        Controls.Add(ButtonNuevaLinea)
        Controls.Add(ButtonBorrarLineas)
        Controls.Add(ButtonSiguiente)
        Controls.Add(ButtonAnterior)
        Controls.Add(ButtonNuevoPed)
        Controls.Add(ButtonBorrar)
        Controls.Add(DataGridView1)
        Controls.Add(TextBoxVendedor)
        Controls.Add(TextBoxIdVendedor)
        Controls.Add(Label4)
        Controls.Add(Label6)
        Controls.Add(TextBoxFecha)
        Controls.Add(TextBoxIdCliente)
        Controls.Add(ButtonGuardar)
        Controls.Add(Button1)
        Controls.Add(TextBoxCliente)
        Controls.Add(TextBoxFactura)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(Label3)
        Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ForeColor = Color.WhiteSmoke
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmFacturas"
        ShowIcon = False
        SizeGripStyle = SizeGripStyle.Show
        Text = "OPTIMA"
        TabControlModerno2.ResumeLayout(False)
        TabPage5.ResumeLayout(False)
        TabPage5.PerformLayout()
        TabPage6.ResumeLayout(False)
        TabPage6.PerformLayout()
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents TabControlModerno2 As TabControlModerno
    Friend WithEvents TabPage5 As TabPage
    Friend WithEvents Label25 As Label
    Friend WithEvents Label26 As Label
    Friend WithEvents TextBoxCP As TextBox
    Friend WithEvents Label27 As Label
    Friend WithEvents TextBoxPeso As TextBox
    Friend WithEvents Label28 As Label
    Friend WithEvents Label29 As Label
    Friend WithEvents TextBoxDireccion As TextBox
    Friend WithEvents TextBoxBultos As TextBox
    Friend WithEvents Label30 As Label
    Friend WithEvents ComboBoxPortes As ComboBox
    Friend WithEvents TextBoxPoblacion As TextBox
    Friend WithEvents TextBoxTracking As TextBox
    Friend WithEvents Label31 As Label
    Friend WithEvents TabPage6 As TabPage
    Friend WithEvents TextBoxAlbaranOrigen As TextBox
    Friend WithEvents Label20 As Label
    Friend WithEvents btnImportarAlbaran As Button
    Friend WithEvents TextBoxObservaciones As TextBox
    Friend WithEvents Label21 As Label
    Friend WithEvents TextBox5 As TextBox
    Friend WithEvents Button4 As Button
    Friend WithEvents Label17 As Label
    Friend WithEvents cboAgencias As ComboBox
    Friend WithEvents Label9 As Label
    Friend WithEvents DateTimePickerFecha As DateTimePicker
    Friend WithEvents LabelStock As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents LabelIva As Label
    Friend WithEvents LabelBase As Label
    Friend WithEvents TextBoxTotalAlb As TextBox
    Friend WithEvents TextBoxIva As TextBox
    Friend WithEvents TextBoxBase As TextBox
    Friend WithEvents ButtonNuevaLinea As Button
    Friend WithEvents ButtonBorrarLineas As Button
    Friend WithEvents ButtonSiguiente As Button
    Friend WithEvents ButtonAnterior As Button
    Friend WithEvents ButtonNuevoPed As Button
    Friend WithEvents ButtonBorrar As Button
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents ConexionBDBindingSource1 As BindingSource
    Friend WithEvents TextBoxVendedor As TextBox
    Friend WithEvents TextBoxIdVendedor As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents TextBoxFecha As TextBox
    Friend WithEvents TextBoxIdCliente As TextBox
    Friend WithEvents ButtonGuardar As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents TextBoxCliente As TextBox
    Friend WithEvents TextBoxFactura As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents ConexionBDBindingSource As BindingSource
    Friend WithEvents Label3 As Label
End Class
