<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmFacturasCompra
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
        Label9 = New Label()
        DateTimePickerVencimiento = New DateTimePicker()
        ConexionBDBindingSource1 = New BindingSource(components)
        ConexionBDBindingSource = New BindingSource(components)
        LabelStock = New Label()
        Label7 = New Label()
        LabelIva = New Label()
        LabelBase = New Label()
        TextBoxTotalFac = New TextBox()
        TextBoxIva = New TextBox()
        TextBoxBase = New TextBox()
        ButtonNuevaLinea = New Button()
        ButtonBorrarLineas = New Button()
        ButtonSiguiente = New Button()
        ButtonAnterior = New Button()
        ButtonNuevoFac = New Button()
        ButtonBorrar = New Button()
        ButtonGuardar = New Button()
        btnBuscarFactura = New Button()
        DataGridView1 = New DataGridView()
        TextBoxComprador = New TextBox()
        TextBoxIdComprador = New TextBox()
        Label4 = New Label()
        Label6 = New Label()
        TextBoxFecha = New TextBox()
        TextBoxProveedor = New TextBox()
        TextBoxIdProveedor = New TextBox()
        TextBoxFactura = New TextBox()
        Label3 = New Label()
        Label2 = New Label()
        Label1 = New Label()
        TextBoxAlbaranOrigen = New TextBox()
        Label8 = New Label()
        btnImportarAlbaran = New Button()
        TextBoxObservaciones = New TextBox()
        Label5 = New Label()
        TextBoxFacProveedor = New TextBox()
        Label10 = New Label()
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label9.Location = New Point(698, 44)
        Label9.Name = "Label9"
        Label9.Size = New Size(140, 20)
        Label9.TabIndex = 110
        Label9.Text = "Fecha vencimiento"
        ' 
        ' DateTimePickerVencimiento
        ' 
        DateTimePickerVencimiento.Format = DateTimePickerFormat.Short
        DateTimePickerVencimiento.Location = New Point(701, 67)
        DateTimePickerVencimiento.Name = "DateTimePickerVencimiento"
        DateTimePickerVencimiento.Size = New Size(125, 25)
        DateTimePickerVencimiento.TabIndex = 109
        ' 
        ' ConexionBDBindingSource1
        ' 
        ConexionBDBindingSource1.DataSource = GetType(ConexionBD)
        ' 
        ' ConexionBDBindingSource
        ' 
        ConexionBDBindingSource.DataSource = GetType(ConexionBD)
        ' 
        ' LabelStock
        ' 
        LabelStock.AutoSize = True
        LabelStock.Font = New Font("Segoe UI Black", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelStock.Location = New Point(1243, 405)
        LabelStock.Name = "LabelStock"
        LabelStock.Size = New Size(120, 17)
        LabelStock.TabIndex = 106
        LabelStock.Text = "Stock disponible:-"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label7.Location = New Point(1243, 532)
        Label7.Name = "Label7"
        Label7.Size = New Size(76, 21)
        Label7.TabIndex = 104
        Label7.Text = "TOTAL : "
        ' 
        ' LabelIva
        ' 
        LabelIva.AutoSize = True
        LabelIva.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelIva.Location = New Point(1243, 486)
        LabelIva.Name = "LabelIva"
        LabelIva.Size = New Size(49, 20)
        LabelIva.TabIndex = 103
        LabelIva.Text = "I.V.A :"
        ' 
        ' LabelBase
        ' 
        LabelBase.AutoSize = True
        LabelBase.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelBase.Location = New Point(1243, 443)
        LabelBase.Name = "LabelBase"
        LabelBase.Size = New Size(124, 20)
        LabelBase.TabIndex = 102
        LabelBase.Text = "Base imponible :"
        ' 
        ' TextBoxTotalFac
        ' 
        TextBoxTotalFac.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxTotalFac.BorderStyle = BorderStyle.None
        TextBoxTotalFac.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxTotalFac.ForeColor = Color.WhiteSmoke
        TextBoxTotalFac.Location = New Point(1273, 531)
        TextBoxTotalFac.Name = "TextBoxTotalFac"
        TextBoxTotalFac.ReadOnly = True
        TextBoxTotalFac.Size = New Size(256, 22)
        TextBoxTotalFac.TabIndex = 101
        TextBoxTotalFac.TextAlign = HorizontalAlignment.Right
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
        TextBoxIva.TabIndex = 100
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
        TextBoxBase.TabIndex = 99
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
        ButtonNuevaLinea.TabIndex = 98
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
        ButtonBorrarLineas.TabIndex = 97
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
        ButtonSiguiente.TabIndex = 96
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
        ButtonAnterior.TabIndex = 95
        ButtonAnterior.Text = "Anterior"
        ButtonAnterior.UseVisualStyleBackColor = False
        ' 
        ' ButtonNuevoFac
        ' 
        ButtonNuevoFac.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(255))
        ButtonNuevoFac.FlatStyle = FlatStyle.Flat
        ButtonNuevoFac.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonNuevoFac.ForeColor = Color.Black
        ButtonNuevoFac.Location = New Point(288, 627)
        ButtonNuevoFac.Name = "ButtonNuevoFac"
        ButtonNuevoFac.Size = New Size(113, 29)
        ButtonNuevoFac.TabIndex = 94
        ButtonNuevoFac.Text = "Nuevo"
        ButtonNuevoFac.UseVisualStyleBackColor = False
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
        ButtonBorrar.TabIndex = 93
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
        ButtonGuardar.TabIndex = 92
        ButtonGuardar.Text = "Guardar"
        ButtonGuardar.UseVisualStyleBackColor = False
        ' 
        ' btnBuscarFactura
        ' 
        btnBuscarFactura.BackColor = Color.FromArgb(CByte(0), CByte(120), CByte(215))
        btnBuscarFactura.FlatStyle = FlatStyle.Flat
        btnBuscarFactura.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        btnBuscarFactura.ForeColor = Color.White
        btnBuscarFactura.Location = New Point(168, 67)
        btnBuscarFactura.Name = "btnBuscarFactura"
        btnBuscarFactura.Size = New Size(29, 25)
        btnBuscarFactura.TabIndex = 111
        btnBuscarFactura.Text = "🔍"
        btnBuscarFactura.UseVisualStyleBackColor = False
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
        DataGridView1.TabIndex = 91
        ' 
        ' TextBoxComprador
        ' 
        TextBoxComprador.Location = New Point(110, 118)
        TextBoxComprador.Name = "TextBoxComprador"
        TextBoxComprador.Size = New Size(305, 25)
        TextBoxComprador.TabIndex = 90
        ' 
        ' TextBoxIdComprador
        ' 
        TextBoxIdComprador.Location = New Point(48, 118)
        TextBoxIdComprador.Name = "TextBoxIdComprador"
        TextBoxIdComprador.Size = New Size(54, 25)
        TextBoxIdComprador.TabIndex = 89
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label4.Location = New Point(48, 95)
        Label4.Name = "Label4"
        Label4.Size = New Size(90, 20)
        Label4.TabIndex = 88
        Label4.Text = "Comprador"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.Location = New Point(711, 95)
        Label6.Name = "Label6"
        Label6.Size = New Size(56, 20)
        Label6.TabIndex = 86
        Label6.Text = "Estado"
        ' 
        ' TextBoxFecha
        ' 
        TextBoxFecha.Location = New Point(584, 67)
        TextBoxFecha.Name = "TextBoxFecha"
        TextBoxFecha.Size = New Size(114, 25)
        TextBoxFecha.TabIndex = 84
        ' 
        ' TextBoxProveedor
        ' 
        TextBoxProveedor.Location = New Point(272, 67)
        TextBoxProveedor.Name = "TextBoxProveedor"
        TextBoxProveedor.Size = New Size(305, 25)
        TextBoxProveedor.TabIndex = 83
        ' 
        ' TextBoxIdProveedor
        ' 
        TextBoxIdProveedor.Location = New Point(210, 67)
        TextBoxIdProveedor.Name = "TextBoxIdProveedor"
        TextBoxIdProveedor.Size = New Size(54, 25)
        TextBoxIdProveedor.TabIndex = 82
        ' 
        ' TextBoxFactura
        ' 
        TextBoxFactura.Location = New Point(48, 67)
        TextBoxFactura.Name = "TextBoxFactura"
        TextBoxFactura.Size = New Size(114, 25)
        TextBoxFactura.TabIndex = 81
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label3.Location = New Point(584, 44)
        Label3.Name = "Label3"
        Label3.Size = New Size(110, 20)
        Label3.TabIndex = 80
        Label3.Text = "Fecha emisión"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.Location = New Point(210, 44)
        Label2.Name = "Label2"
        Label2.Size = New Size(85, 20)
        Label2.TabIndex = 79
        Label2.Text = "Proveedor"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(48, 44)
        Label1.Name = "Label1"
        Label1.Size = New Size(64, 20)
        Label1.TabIndex = 77
        Label1.Text = "Factura"
        ' 
        ' TextBoxAlbaranOrigen
        ' 
        TextBoxAlbaranOrigen.Location = New Point(865, 118)
        TextBoxAlbaranOrigen.Name = "TextBoxAlbaranOrigen"
        TextBoxAlbaranOrigen.Size = New Size(114, 25)
        TextBoxAlbaranOrigen.TabIndex = 124
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label8.Location = New Point(865, 95)
        Label8.Name = "Label8"
        Label8.Size = New Size(70, 20)
        Label8.TabIndex = 125
        Label8.Text = "Albarán"
        ' 
        ' btnImportarAlbaran
        ' 
        btnImportarAlbaran.BackColor = Color.FromArgb(CByte(0), CByte(120), CByte(215))
        btnImportarAlbaran.FlatStyle = FlatStyle.Flat
        btnImportarAlbaran.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        btnImportarAlbaran.ForeColor = Color.White
        btnImportarAlbaran.Location = New Point(985, 118)
        btnImportarAlbaran.Name = "btnImportarAlbaran"
        btnImportarAlbaran.Size = New Size(29, 25)
        btnImportarAlbaran.TabIndex = 127
        btnImportarAlbaran.Text = "🔍"
        btnImportarAlbaran.UseVisualStyleBackColor = False
        ' 
        ' TextBoxObservaciones
        ' 
        TextBoxObservaciones.Location = New Point(422, 118)
        TextBoxObservaciones.Name = "TextBoxObservaciones"
        TextBoxObservaciones.Size = New Size(282, 25)
        TextBoxObservaciones.TabIndex = 122
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label5.Location = New Point(422, 95)
        Label5.Name = "Label5"
        Label5.Size = New Size(111, 20)
        Label5.TabIndex = 123
        Label5.Text = "Observaciones"
        ' 
        ' TextBoxFacProveedor
        ' 
        TextBoxFacProveedor.Location = New Point(1030, 67)
        TextBoxFacProveedor.Name = "TextBoxFacProveedor"
        TextBoxFacProveedor.Size = New Size(150, 25)
        TextBoxFacProveedor.TabIndex = 130
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label10.Location = New Point(1030, 44)
        Label10.Name = "Label10"
        Label10.Size = New Size(160, 20)
        Label10.TabIndex = 131
        Label10.Text = "Nº Fac. Proveedor"
        ' 
        ' FrmFacturasCompra
        ' 
        AutoScaleDimensions = New SizeF(8F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        ClientSize = New Size(1696, 761)
        ControlBox = False
        Controls.Add(Label10)
        Controls.Add(TextBoxFacProveedor)
        Controls.Add(Label5)
        Controls.Add(TextBoxObservaciones)
        Controls.Add(btnImportarAlbaran)
        Controls.Add(Label8)
        Controls.Add(TextBoxAlbaranOrigen)
        Controls.Add(Label9)
        Controls.Add(DateTimePickerVencimiento)
        Controls.Add(LabelStock)
        Controls.Add(Label7)
        Controls.Add(LabelIva)
        Controls.Add(LabelBase)
        Controls.Add(TextBoxTotalFac)
        Controls.Add(TextBoxIva)
        Controls.Add(TextBoxBase)
        Controls.Add(ButtonNuevaLinea)
        Controls.Add(ButtonBorrarLineas)
        Controls.Add(ButtonSiguiente)
        Controls.Add(ButtonAnterior)
        Controls.Add(ButtonNuevoFac)
        Controls.Add(ButtonBorrar)
        Controls.Add(ButtonGuardar)
        Controls.Add(btnBuscarFactura)
        Controls.Add(DataGridView1)
        Controls.Add(TextBoxComprador)
        Controls.Add(TextBoxIdComprador)
        Controls.Add(Label4)
        Controls.Add(Label6)
        Controls.Add(TextBoxFecha)
        Controls.Add(TextBoxProveedor)
        Controls.Add(TextBoxIdProveedor)
        Controls.Add(TextBoxFactura)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ForeColor = Color.WhiteSmoke
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmFacturasCompra"
        ShowIcon = False
        SizeGripStyle = SizeGripStyle.Show
        Text = "Optima - Facturas de Compra"
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label9 As Label
    Friend WithEvents DateTimePickerVencimiento As DateTimePicker
    Friend WithEvents ConexionBDBindingSource1 As BindingSource
    Friend WithEvents ConexionBDBindingSource As BindingSource
    Friend WithEvents LabelStock As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents LabelIva As Label
    Friend WithEvents LabelBase As Label
    Friend WithEvents TextBoxTotalFac As TextBox
    Friend WithEvents TextBoxIva As TextBox
    Friend WithEvents TextBoxBase As TextBox
    Friend WithEvents ButtonNuevaLinea As Button
    Friend WithEvents ButtonBorrarLineas As Button
    Friend WithEvents ButtonSiguiente As Button
    Friend WithEvents ButtonAnterior As Button
    Friend WithEvents ButtonNuevoFac As Button
    Friend WithEvents ButtonBorrar As Button
    Friend WithEvents ButtonGuardar As Button
    Friend WithEvents btnBuscarFactura As Button
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents TextBoxComprador As TextBox
    Friend WithEvents TextBoxIdComprador As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents TextBoxFecha As TextBox
    Friend WithEvents TextBoxProveedor As TextBox
    Friend WithEvents TextBoxIdProveedor As TextBox
    Friend WithEvents TextBoxFactura As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents TextBoxAlbaranOrigen As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents btnImportarAlbaran As Button
    Friend WithEvents TextBoxObservaciones As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents TextBoxFacProveedor As TextBox
    Friend WithEvents Label10 As Label
End Class
