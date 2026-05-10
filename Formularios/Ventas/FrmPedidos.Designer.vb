<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmPedidos
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmPedidos))
        LabelStock = New Label()
        Label7 = New Label()
        LabelIva = New Label()
        LabelBase = New Label()
        TextBoxTotalPed = New TextBox()
        TextBoxIva = New TextBox()
        TextBoxBase = New TextBox()
        ButtonNuevaLinea = New Button()
        ButtonBorrarLineas = New Button()
        ButtonSiguiente = New Button()
        ButtonAnterior = New Button()
        ButtonNuevoPresup = New Button()
        ButtonBorrar = New Button()
        ButtonGuardar = New Button()
        ConexionBDBindingSource = New BindingSource(components)
        ConexionBDBindingSource1 = New BindingSource(components)
        btnBuscarPedido = New Button()
        DataGridView1 = New DataGridView()
        TextBoxVendedor = New TextBox()
        TextBoxIdVendedor = New TextBox()
        Label4 = New Label()
        TextBoxObservaciones = New TextBox()
        Label6 = New Label()
        Label5 = New Label()
        TextBoxFecha = New TextBox()
        TextBoxCliente = New TextBox()
        TextBoxIdCliente = New TextBox()
        TextBoxPedido = New TextBox()
        Label3 = New Label()
        Label2 = New Label()
        Label1 = New Label()
        TextBoxIdPresupuesto = New TextBox()
        Label8 = New Label()
        DateTimePickerFecha = New DateTimePicker()
        Label9 = New Label()
        btnBuscarPresupuesto = New Button()
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' LabelStock
        ' 
        LabelStock.AutoSize = True
        LabelStock.Font = New Font("Segoe UI Black", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelStock.Location = New Point(1250, 281)
        LabelStock.Name = "LabelStock"
        LabelStock.Size = New Size(120, 17)
        LabelStock.TabIndex = 71
        LabelStock.Text = "Stock disponible:-"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label7.Location = New Point(1243, 532)
        Label7.Name = "Label7"
        Label7.Size = New Size(76, 21)
        Label7.TabIndex = 69
        Label7.Text = "TOTAL : "
        ' 
        ' LabelIva
        ' 
        LabelIva.AutoSize = True
        LabelIva.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelIva.Location = New Point(1243, 486)
        LabelIva.Name = "LabelIva"
        LabelIva.Size = New Size(49, 20)
        LabelIva.TabIndex = 68
        LabelIva.Text = "I.V.A :"
        ' 
        ' LabelBase
        ' 
        LabelBase.AutoSize = True
        LabelBase.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelBase.Location = New Point(1243, 443)
        LabelBase.Name = "LabelBase"
        LabelBase.Size = New Size(124, 20)
        LabelBase.TabIndex = 67
        LabelBase.Text = "Base imponible :"
        ' 
        ' TextBoxTotalPed
        ' 
        TextBoxTotalPed.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxTotalPed.BorderStyle = BorderStyle.None
        TextBoxTotalPed.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxTotalPed.ForeColor = Color.WhiteSmoke
        TextBoxTotalPed.Location = New Point(1273, 531)
        TextBoxTotalPed.Name = "TextBoxTotalPed"
        TextBoxTotalPed.ReadOnly = True
        TextBoxTotalPed.Size = New Size(256, 22)
        TextBoxTotalPed.TabIndex = 66
        TextBoxTotalPed.TextAlign = HorizontalAlignment.Right
        ' 
        ' TextBoxIva
        ' 
        TextBoxIva.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxIva.BorderStyle = BorderStyle.None
        TextBoxIva.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxIva.ForeColor = Color.WhiteSmoke
        TextBoxIva.Location = New Point(1273, 486)
        TextBoxIva.Name = "TextBoxIva"
        TextBoxIva.ReadOnly = True
        TextBoxIva.Size = New Size(256, 20)
        TextBoxIva.TabIndex = 65
        TextBoxIva.TextAlign = HorizontalAlignment.Right
        ' 
        ' TextBoxBase
        ' 
        TextBoxBase.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxBase.BorderStyle = BorderStyle.None
        TextBoxBase.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxBase.ForeColor = Color.WhiteSmoke
        TextBoxBase.Location = New Point(1273, 443)
        TextBoxBase.Name = "TextBoxBase"
        TextBoxBase.ReadOnly = True
        TextBoxBase.Size = New Size(256, 20)
        TextBoxBase.TabIndex = 64
        TextBoxBase.TextAlign = HorizontalAlignment.Right
        ' 
        ' ButtonNuevaLinea
        ' 
        ButtonNuevaLinea.BackColor = Color.FromArgb(CByte(128), CByte(255), CByte(255))
        ButtonNuevaLinea.FlatStyle = FlatStyle.Flat
        ButtonNuevaLinea.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonNuevaLinea.ForeColor = Color.Black
        ButtonNuevaLinea.Location = New Point(701, 627)
        ButtonNuevaLinea.Name = "ButtonNuevaLinea"
        ButtonNuevaLinea.Size = New Size(113, 29)
        ButtonNuevaLinea.TabIndex = 63
        ButtonNuevaLinea.Text = "Nueva linea"
        ButtonNuevaLinea.UseVisualStyleBackColor = False
        ' 
        ' ButtonBorrarLineas
        ' 
        ButtonBorrarLineas.BackColor = Color.FromArgb(CByte(255), CByte(128), CByte(128))
        ButtonBorrarLineas.FlatStyle = FlatStyle.Flat
        ButtonBorrarLineas.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonBorrarLineas.ForeColor = Color.Black
        ButtonBorrarLineas.Location = New Point(581, 627)
        ButtonBorrarLineas.Name = "ButtonBorrarLineas"
        ButtonBorrarLineas.Size = New Size(113, 29)
        ButtonBorrarLineas.TabIndex = 62
        ButtonBorrarLineas.Text = "Borrar lineas"
        ButtonBorrarLineas.UseVisualStyleBackColor = False
        ' 
        ' ButtonSiguiente
        ' 
        ButtonSiguiente.BackColor = Color.Silver
        ButtonSiguiente.FlatStyle = FlatStyle.Flat
        ButtonSiguiente.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonSiguiente.ForeColor = Color.Black
        ButtonSiguiente.Location = New Point(1134, 627)
        ButtonSiguiente.Name = "ButtonSiguiente"
        ButtonSiguiente.Size = New Size(113, 29)
        ButtonSiguiente.TabIndex = 61
        ButtonSiguiente.Text = "Siguiente"
        ButtonSiguiente.UseVisualStyleBackColor = False
        ' 
        ' ButtonAnterior
        ' 
        ButtonAnterior.BackColor = Color.Silver
        ButtonAnterior.FlatStyle = FlatStyle.Flat
        ButtonAnterior.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonAnterior.ForeColor = Color.Black
        ButtonAnterior.Location = New Point(1013, 627)
        ButtonAnterior.Name = "ButtonAnterior"
        ButtonAnterior.Size = New Size(113, 29)
        ButtonAnterior.TabIndex = 60
        ButtonAnterior.Text = "Anterior"
        ButtonAnterior.UseVisualStyleBackColor = False
        ' 
        ' ButtonNuevoPresup
        ' 
        ButtonNuevoPresup.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(255))
        ButtonNuevoPresup.FlatStyle = FlatStyle.Flat
        ButtonNuevoPresup.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonNuevoPresup.ForeColor = Color.Black
        ButtonNuevoPresup.Location = New Point(288, 627)
        ButtonNuevoPresup.Name = "ButtonNuevoPresup"
        ButtonNuevoPresup.Size = New Size(113, 29)
        ButtonNuevoPresup.TabIndex = 59
        ButtonNuevoPresup.Text = "Nuevo"
        ButtonNuevoPresup.UseVisualStyleBackColor = False
        ' 
        ' ButtonBorrar
        ' 
        ButtonBorrar.BackColor = Color.FromArgb(CByte(255), CByte(192), CByte(192))
        ButtonBorrar.FlatStyle = FlatStyle.Flat
        ButtonBorrar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonBorrar.ForeColor = Color.Black
        ButtonBorrar.Location = New Point(168, 627)
        ButtonBorrar.Name = "ButtonBorrar"
        ButtonBorrar.Size = New Size(113, 29)
        ButtonBorrar.TabIndex = 58
        ButtonBorrar.Text = "Borrar"
        ButtonBorrar.UseVisualStyleBackColor = False
        ' 
        ' ButtonGuardar
        ' 
        ButtonGuardar.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        ButtonGuardar.FlatStyle = FlatStyle.Flat
        ButtonGuardar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonGuardar.ForeColor = Color.Black
        ButtonGuardar.Location = New Point(48, 627)
        ButtonGuardar.Name = "ButtonGuardar"
        ButtonGuardar.Size = New Size(113, 29)
        ButtonGuardar.TabIndex = 57
        ButtonGuardar.Text = "Guardar"
        ButtonGuardar.UseVisualStyleBackColor = False
        ' 
        ' ConexionBDBindingSource
        ' 
        ConexionBDBindingSource.DataSource = GetType(ConexionBD)
        ' 
        ' ConexionBDBindingSource1
        ' 
        ConexionBDBindingSource1.DataSource = GetType(ConexionBD)
        ' 
        ' btnBuscarPedido
        ' 
        btnBuscarPedido.BackgroundImage = CType(resources.GetObject("btnBuscarPedido.BackgroundImage"), Image)
        btnBuscarPedido.BackgroundImageLayout = ImageLayout.Zoom
        btnBuscarPedido.Location = New Point(168, 67)
        btnBuscarPedido.Name = "btnBuscarPedido"
        btnBuscarPedido.Size = New Size(29, 25)
        btnBuscarPedido.TabIndex = 70
        btnBuscarPedido.UseVisualStyleBackColor = True
        ' 
        ' DataGridView1
        ' 
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.DataSource = ConexionBDBindingSource1
        DataGridView1.Location = New Point(47, 163)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.Size = New Size(1195, 417)
        DataGridView1.TabIndex = 56
        ' 
        ' TextBoxVendedor
        ' 
        TextBoxVendedor.Location = New Point(110, 118)
        TextBoxVendedor.Name = "TextBoxVendedor"
        TextBoxVendedor.Size = New Size(305, 25)
        TextBoxVendedor.TabIndex = 55
        ' 
        ' TextBoxIdVendedor
        ' 
        TextBoxIdVendedor.Location = New Point(48, 118)
        TextBoxIdVendedor.Name = "TextBoxIdVendedor"
        TextBoxIdVendedor.Size = New Size(54, 25)
        TextBoxIdVendedor.TabIndex = 54
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label4.Location = New Point(48, 95)
        Label4.Name = "Label4"
        Label4.Size = New Size(76, 20)
        Label4.TabIndex = 53
        Label4.Text = "Vendedor"
        ' 
        ' TextBoxObservaciones
        ' 
        TextBoxObservaciones.Location = New Point(422, 118)
        TextBoxObservaciones.Name = "TextBoxObservaciones"
        TextBoxObservaciones.Size = New Size(282, 25)
        TextBoxObservaciones.TabIndex = 51
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.Location = New Point(711, 95)
        Label6.Name = "Label6"
        Label6.Size = New Size(56, 20)
        Label6.TabIndex = 50
        Label6.Text = "Estado"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label5.Location = New Point(422, 95)
        Label5.Name = "Label5"
        Label5.Size = New Size(111, 20)
        Label5.TabIndex = 49
        Label5.Text = "Observaciones"
        ' 
        ' TextBoxFecha
        ' 
        TextBoxFecha.Location = New Point(584, 67)
        TextBoxFecha.Name = "TextBoxFecha"
        TextBoxFecha.Size = New Size(114, 25)
        TextBoxFecha.TabIndex = 48
        ' 
        ' TextBoxCliente
        ' 
        TextBoxCliente.Location = New Point(272, 67)
        TextBoxCliente.Name = "TextBoxCliente"
        TextBoxCliente.Size = New Size(305, 25)
        TextBoxCliente.TabIndex = 47
        ' 
        ' TextBoxIdCliente
        ' 
        TextBoxIdCliente.Location = New Point(210, 67)
        TextBoxIdCliente.Name = "TextBoxIdCliente"
        TextBoxIdCliente.Size = New Size(54, 25)
        TextBoxIdCliente.TabIndex = 46
        ' 
        ' TextBoxPedido
        ' 
        TextBoxPedido.Location = New Point(48, 67)
        TextBoxPedido.Name = "TextBoxPedido"
        TextBoxPedido.Size = New Size(114, 25)
        TextBoxPedido.TabIndex = 45
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label3.Location = New Point(584, 44)
        Label3.Name = "Label3"
        Label3.Size = New Size(49, 20)
        Label3.TabIndex = 44
        Label3.Text = "Fecha"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.Location = New Point(210, 44)
        Label2.Name = "Label2"
        Label2.Size = New Size(57, 20)
        Label2.TabIndex = 43
        Label2.Text = "Cliente"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(48, 44)
        Label1.Name = "Label1"
        Label1.Size = New Size(57, 20)
        Label1.TabIndex = 42
        Label1.Text = "Pedido"
        ' 
        ' TextBoxIdPresupuesto
        ' 
        TextBoxIdPresupuesto.Location = New Point(865, 118)
        TextBoxIdPresupuesto.Name = "TextBoxIdPresupuesto"
        TextBoxIdPresupuesto.Size = New Size(114, 25)
        TextBoxIdPresupuesto.TabIndex = 72
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label8.Location = New Point(865, 95)
        Label8.Name = "Label8"
        Label8.Size = New Size(96, 20)
        Label8.TabIndex = 73
        Label8.Text = "Presupuesto"
        ' 
        ' DateTimePickerFecha
        ' 
        DateTimePickerFecha.Format = DateTimePickerFormat.Short
        DateTimePickerFecha.Location = New Point(704, 67)
        DateTimePickerFecha.Name = "DateTimePickerFecha"
        DateTimePickerFecha.Size = New Size(125, 25)
        DateTimePickerFecha.TabIndex = 74
        DateTimePickerFecha.Value = New Date(2026, 1, 30, 0, 0, 0, 0)
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label9.Location = New Point(701, 44)
        Label9.Name = "Label9"
        Label9.Size = New Size(107, 20)
        Label9.TabIndex = 75
        Label9.Text = "Fecha entrega"
        ' 
        ' btnBuscarPresupuesto
        ' 
        btnBuscarPresupuesto.BackgroundImage = CType(resources.GetObject("btnBuscarPresupuesto.BackgroundImage"), Image)
        btnBuscarPresupuesto.BackgroundImageLayout = ImageLayout.Zoom
        btnBuscarPresupuesto.Location = New Point(985, 118)
        btnBuscarPresupuesto.Name = "btnBuscarPresupuesto"
        btnBuscarPresupuesto.Size = New Size(29, 25)
        btnBuscarPresupuesto.TabIndex = 76
        btnBuscarPresupuesto.UseVisualStyleBackColor = True
        ' 
        ' FrmPedidos
        ' 
        AutoScaleDimensions = New SizeF(8F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        ClientSize = New Size(1696, 761)
        ControlBox = False
        Controls.Add(btnBuscarPresupuesto)
        Controls.Add(Label9)
        Controls.Add(DateTimePickerFecha)
        Controls.Add(Label8)
        Controls.Add(TextBoxIdPresupuesto)
        Controls.Add(LabelStock)
        Controls.Add(Label7)
        Controls.Add(LabelIva)
        Controls.Add(LabelBase)
        Controls.Add(TextBoxTotalPed)
        Controls.Add(TextBoxIva)
        Controls.Add(TextBoxBase)
        Controls.Add(ButtonNuevaLinea)
        Controls.Add(ButtonBorrarLineas)
        Controls.Add(ButtonSiguiente)
        Controls.Add(ButtonAnterior)
        Controls.Add(ButtonNuevoPresup)
        Controls.Add(ButtonBorrar)
        Controls.Add(ButtonGuardar)
        Controls.Add(btnBuscarPedido)
        Controls.Add(DataGridView1)
        Controls.Add(TextBoxVendedor)
        Controls.Add(TextBoxIdVendedor)
        Controls.Add(Label4)
        Controls.Add(TextBoxObservaciones)
        Controls.Add(Label6)
        Controls.Add(Label5)
        Controls.Add(TextBoxFecha)
        Controls.Add(TextBoxCliente)
        Controls.Add(TextBoxIdCliente)
        Controls.Add(TextBoxPedido)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ForeColor = Color.WhiteSmoke
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmPedidos"
        ShowIcon = False
        SizeGripStyle = SizeGripStyle.Show
        Text = "Optima"
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents LabelStock As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents LabelIva As Label
    Friend WithEvents LabelBase As Label
    Friend WithEvents TextBoxTotalPedido As TextBox
    Friend WithEvents TextBoxIva As TextBox
    Friend WithEvents TextBoxBase As TextBox
    Friend WithEvents ButtonNuevaLinea As Button
    Friend WithEvents ButtonBorrarLineas As Button
    Friend WithEvents ButtonSiguiente As Button
    Friend WithEvents ButtonAnterior As Button
    Friend WithEvents ButtonNuevoPresup As Button
    Friend WithEvents ButtonBorrar As Button
    Friend WithEvents ButtonGuardar As Button
    Friend WithEvents ConexionBDBindingSource As BindingSource
    Friend WithEvents ConexionBDBindingSource1 As BindingSource
    Friend WithEvents btnBuscarPedido As Button
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents TextBoxVendedor As TextBox
    Friend WithEvents TextBoxIdVendedor As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents TextBoxObservaciones As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents TextBoxFecha As TextBox
    Friend WithEvents TextBoxCliente As TextBox
    Friend WithEvents TextBoxIdCliente As TextBox
    Friend WithEvents TextBoxPedidos As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents TextBoxTotalPed As TextBox
    Friend WithEvents TextBoxPedido As TextBox
    Friend WithEvents TextBoxIdPresupuesto As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents DateTimePickerFecha As DateTimePicker
    Friend WithEvents Label9 As Label
    Friend WithEvents btnBuscarPresupuesto As Button
End Class
