<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmPresupuestos
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmPresupuestos))
        Label1 = New Label()
        Label2 = New Label()
        Label3 = New Label()
        TextBoxPresupuesto = New TextBox()
        TextBoxIdCliente = New TextBox()
        TextBoxCliente = New TextBox()
        TextBoxFecha = New TextBox()
        Label5 = New Label()
        Label6 = New Label()
        TextBoxObservaciones = New TextBox()
        TextBoxVendedor = New TextBox()
        TextBoxIdVendedor = New TextBox()
        Label4 = New Label()
        DataGridView1 = New DataGridView()
        ConexionBDBindingSource1 = New BindingSource(components)
        ConexionBDBindingSource = New BindingSource(components)
        ButtonSiguiente = New Button()
        ButtonAnterior = New Button()
        ButtonNuevoPresup = New Button()
        ButtonBorrar = New Button()
        ButtonGuardar = New Button()
        ButtonBorrarLineas = New Button()
        ButtonNuevaLinea = New Button()
        TextBoxBase = New TextBox()
        TextBoxIva = New TextBox()
        TextBoxTotalPresup = New TextBox()
        LabelBase = New Label()
        LabelIva = New Label()
        Label7 = New Label()
        btnBuscarPresupuesto = New Button()
        LabelStock = New Label()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(48, 44)
        Label1.Name = "Label1"
        Label1.Size = New Size(96, 20)
        Label1.TabIndex = 0
        Label1.Text = "Presupuesto"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.Location = New Point(49, 105)
        Label2.Name = "Label2"
        Label2.Size = New Size(57, 20)
        Label2.TabIndex = 1
        Label2.Text = "Cliente"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label3.Location = New Point(216, 44)
        Label3.Name = "Label3"
        Label3.Size = New Size(49, 20)
        Label3.TabIndex = 2
        Label3.Text = "Fecha"
        ' 
        ' TextBoxPresupuesto
        ' 
        TextBoxPresupuesto.Location = New Point(48, 67)
        TextBoxPresupuesto.Name = "TextBoxPresupuesto"
        TextBoxPresupuesto.Size = New Size(114, 25)
        TextBoxPresupuesto.TabIndex = 3
        ' 
        ' TextBoxIdCliente
        ' 
        TextBoxIdCliente.Location = New Point(49, 128)
        TextBoxIdCliente.Name = "TextBoxIdCliente"
        TextBoxIdCliente.Size = New Size(71, 25)
        TextBoxIdCliente.TabIndex = 4
        ' 
        ' TextBoxCliente
        ' 
        TextBoxCliente.Location = New Point(127, 128)
        TextBoxCliente.Name = "TextBoxCliente"
        TextBoxCliente.Size = New Size(357, 25)
        TextBoxCliente.TabIndex = 5
        ' 
        ' TextBoxFecha
        ' 
        TextBoxFecha.Location = New Point(216, 67)
        TextBoxFecha.Name = "TextBoxFecha"
        TextBoxFecha.Size = New Size(114, 25)
        TextBoxFecha.TabIndex = 6
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label5.Location = New Point(423, 168)
        Label5.Name = "Label5"
        Label5.Size = New Size(111, 20)
        Label5.TabIndex = 8
        Label5.Text = "Observaciones"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.Location = New Point(336, 44)
        Label6.Name = "Label6"
        Label6.Size = New Size(56, 20)
        Label6.TabIndex = 9
        Label6.Text = "Estado"
        ' 
        ' TextBoxObservaciones
        ' 
        TextBoxObservaciones.Location = New Point(423, 191)
        TextBoxObservaciones.Name = "TextBoxObservaciones"
        TextBoxObservaciones.Size = New Size(282, 25)
        TextBoxObservaciones.TabIndex = 12
        ' 
        ' TextBoxVendedor
        ' 
        TextBoxVendedor.Location = New Point(111, 191)
        TextBoxVendedor.Name = "TextBoxVendedor"
        TextBoxVendedor.Size = New Size(305, 25)
        TextBoxVendedor.TabIndex = 21
        ' 
        ' TextBoxIdVendedor
        ' 
        TextBoxIdVendedor.Location = New Point(49, 191)
        TextBoxIdVendedor.Name = "TextBoxIdVendedor"
        TextBoxIdVendedor.Size = New Size(54, 25)
        TextBoxIdVendedor.TabIndex = 20
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label4.Location = New Point(49, 168)
        Label4.Name = "Label4"
        Label4.Size = New Size(76, 20)
        Label4.TabIndex = 19
        Label4.Text = "Vendedor"
        ' 
        ' DataGridView1
        ' 
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.DataSource = ConexionBDBindingSource1
        DataGridView1.Location = New Point(47, 226)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.RowHeadersWidth = 51
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.Size = New Size(1195, 417)
        DataGridView1.TabIndex = 26
        ' 
        ' ConexionBDBindingSource1
        ' 
        ConexionBDBindingSource1.DataSource = GetType(ConexionBD)
        ' 
        ' ConexionBDBindingSource
        ' 
        ConexionBDBindingSource.DataSource = GetType(ConexionBD)
        ' 
        ' ButtonSiguiente
        ' 
        ButtonSiguiente.BackColor = Color.Silver
        ButtonSiguiente.FlatStyle = FlatStyle.Flat
        ButtonSiguiente.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonSiguiente.ForeColor = Color.Black
        ButtonSiguiente.Location = New Point(189, 649)
        ButtonSiguiente.Name = "ButtonSiguiente"
        ButtonSiguiente.Size = New Size(136, 37)
        ButtonSiguiente.TabIndex = 31
        ButtonSiguiente.Text = "Siguiente"
        ButtonSiguiente.UseVisualStyleBackColor = False
        ' 
        ' ButtonAnterior
        ' 
        ButtonAnterior.BackColor = Color.Silver
        ButtonAnterior.FlatStyle = FlatStyle.Flat
        ButtonAnterior.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonAnterior.ForeColor = Color.Black
        ButtonAnterior.Location = New Point(47, 649)
        ButtonAnterior.Name = "ButtonAnterior"
        ButtonAnterior.Size = New Size(136, 37)
        ButtonAnterior.TabIndex = 30
        ButtonAnterior.Text = "Anterior"
        ButtonAnterior.UseVisualStyleBackColor = False
        ' 
        ' ButtonNuevoPresup
        ' 
        ButtonNuevoPresup.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(255))
        ButtonNuevoPresup.FlatStyle = FlatStyle.Flat
        ButtonNuevoPresup.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonNuevoPresup.ForeColor = Color.Black
        ButtonNuevoPresup.Location = New Point(343, 720)
        ButtonNuevoPresup.Name = "ButtonNuevoPresup"
        ButtonNuevoPresup.Size = New Size(141, 37)
        ButtonNuevoPresup.TabIndex = 29
        ButtonNuevoPresup.Text = "Nuevo"
        ButtonNuevoPresup.UseVisualStyleBackColor = False
        ' 
        ' ButtonBorrar
        ' 
        ButtonBorrar.BackColor = Color.FromArgb(CByte(255), CByte(192), CByte(192))
        ButtonBorrar.FlatStyle = FlatStyle.Flat
        ButtonBorrar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonBorrar.ForeColor = Color.Black
        ButtonBorrar.Location = New Point(195, 720)
        ButtonBorrar.Name = "ButtonBorrar"
        ButtonBorrar.Size = New Size(141, 37)
        ButtonBorrar.TabIndex = 28
        ButtonBorrar.Text = "Borrar"
        ButtonBorrar.UseVisualStyleBackColor = False
        ' 
        ' ButtonGuardar
        ' 
        ButtonGuardar.BackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        ButtonGuardar.FlatStyle = FlatStyle.Flat
        ButtonGuardar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonGuardar.ForeColor = Color.Black
        ButtonGuardar.Location = New Point(48, 720)
        ButtonGuardar.Name = "ButtonGuardar"
        ButtonGuardar.Size = New Size(141, 37)
        ButtonGuardar.TabIndex = 27
        ButtonGuardar.Text = "Guardar"
        ButtonGuardar.UseVisualStyleBackColor = False
        ' 
        ' ButtonBorrarLineas
        ' 
        ButtonBorrarLineas.BackColor = Color.FromArgb(CByte(255), CByte(128), CByte(128))
        ButtonBorrarLineas.FlatStyle = FlatStyle.Flat
        ButtonBorrarLineas.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonBorrarLineas.ForeColor = Color.Black
        ButtonBorrarLineas.Location = New Point(462, 650)
        ButtonBorrarLineas.Name = "ButtonBorrarLineas"
        ButtonBorrarLineas.Size = New Size(161, 34)
        ButtonBorrarLineas.TabIndex = 32
        ButtonBorrarLineas.Text = "Borrar lineas"
        ButtonBorrarLineas.UseVisualStyleBackColor = False
        ' 
        ' ButtonNuevaLinea
        ' 
        ButtonNuevaLinea.BackColor = Color.FromArgb(CByte(128), CByte(255), CByte(255))
        ButtonNuevaLinea.FlatStyle = FlatStyle.Flat
        ButtonNuevaLinea.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonNuevaLinea.ForeColor = Color.Black
        ButtonNuevaLinea.Location = New Point(641, 650)
        ButtonNuevaLinea.Name = "ButtonNuevaLinea"
        ButtonNuevaLinea.Size = New Size(146, 34)
        ButtonNuevaLinea.TabIndex = 33
        ButtonNuevaLinea.Text = "Nueva linea"
        ButtonNuevaLinea.UseVisualStyleBackColor = False
        ' 
        ' TextBoxBase
        ' 
        TextBoxBase.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxBase.BorderStyle = BorderStyle.None
        TextBoxBase.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxBase.ForeColor = Color.WhiteSmoke
        TextBoxBase.Location = New Point(983, 646)
        TextBoxBase.Name = "TextBoxBase"
        TextBoxBase.ReadOnly = True
        TextBoxBase.Size = New Size(256, 20)
        TextBoxBase.TabIndex = 34
        TextBoxBase.TextAlign = HorizontalAlignment.Right
        ' 
        ' TextBoxIva
        ' 
        TextBoxIva.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxIva.BorderStyle = BorderStyle.None
        TextBoxIva.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxIva.ForeColor = Color.WhiteSmoke
        TextBoxIva.Location = New Point(1118, 677)
        TextBoxIva.Name = "TextBoxIva"
        TextBoxIva.ReadOnly = True
        TextBoxIva.Size = New Size(121, 20)
        TextBoxIva.TabIndex = 35
        TextBoxIva.TextAlign = HorizontalAlignment.Right
        ' 
        ' TextBoxTotalPresup
        ' 
        TextBoxTotalPresup.BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        TextBoxTotalPresup.BorderStyle = BorderStyle.None
        TextBoxTotalPresup.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxTotalPresup.ForeColor = Color.WhiteSmoke
        TextBoxTotalPresup.Location = New Point(991, 708)
        TextBoxTotalPresup.Name = "TextBoxTotalPresup"
        TextBoxTotalPresup.ReadOnly = True
        TextBoxTotalPresup.Size = New Size(256, 22)
        TextBoxTotalPresup.TabIndex = 36
        TextBoxTotalPresup.TextAlign = HorizontalAlignment.Right
        ' 
        ' LabelBase
        ' 
        LabelBase.AutoSize = True
        LabelBase.BackColor = Color.Transparent
        LabelBase.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelBase.ForeColor = Color.WhiteSmoke
        LabelBase.Location = New Point(953, 646)
        LabelBase.Name = "LabelBase"
        LabelBase.Size = New Size(124, 20)
        LabelBase.TabIndex = 37
        LabelBase.Text = "Base imponible :"
        ' 
        ' LabelIva
        ' 
        LabelIva.AutoSize = True
        LabelIva.BackColor = Color.Transparent
        LabelIva.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelIva.Location = New Point(1035, 677)
        LabelIva.Name = "LabelIva"
        LabelIva.Size = New Size(49, 20)
        LabelIva.TabIndex = 38
        LabelIva.Text = "I.V.A :"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.BackColor = Color.Transparent
        Label7.Font = New Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label7.Location = New Point(961, 709)
        Label7.Name = "Label7"
        Label7.Size = New Size(76, 21)
        Label7.TabIndex = 39
        Label7.Text = "TOTAL : "
        ' 
        ' btnBuscarPresupuesto
        ' 
        btnBuscarPresupuesto.BackgroundImage = CType(resources.GetObject("btnBuscarPresupuesto.BackgroundImage"), Image)
        btnBuscarPresupuesto.BackgroundImageLayout = ImageLayout.Zoom
        btnBuscarPresupuesto.Location = New Point(168, 67)
        btnBuscarPresupuesto.Name = "btnBuscarPresupuesto"
        btnBuscarPresupuesto.Size = New Size(33, 29)
        btnBuscarPresupuesto.TabIndex = 40
        btnBuscarPresupuesto.UseVisualStyleBackColor = True
        ' 
        ' LabelStock
        ' 
        LabelStock.AutoSize = True
        LabelStock.Font = New Font("Segoe UI Black", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        LabelStock.Location = New Point(1243, 284)
        LabelStock.Name = "LabelStock"
        LabelStock.Size = New Size(120, 17)
        LabelStock.TabIndex = 41
        LabelStock.Text = "Stock disponible:-"
        ' 
        ' FrmPresupuestos
        ' 
        AutoScaleDimensions = New SizeF(8F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        ClientSize = New Size(1829, 911)
        ControlBox = False
        Controls.Add(LabelStock)
        Controls.Add(btnBuscarPresupuesto)
        Controls.Add(Label7)
        Controls.Add(LabelIva)
        Controls.Add(LabelBase)
        Controls.Add(TextBoxTotalPresup)
        Controls.Add(TextBoxIva)
        Controls.Add(TextBoxBase)
        Controls.Add(ButtonNuevaLinea)
        Controls.Add(ButtonBorrarLineas)
        Controls.Add(ButtonSiguiente)
        Controls.Add(ButtonAnterior)
        Controls.Add(ButtonNuevoPresup)
        Controls.Add(ButtonBorrar)
        Controls.Add(ButtonGuardar)
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
        Controls.Add(TextBoxPresupuesto)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ForeColor = Color.WhiteSmoke
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmPresupuestos"
        ShowIcon = False
        SizeGripStyle = SizeGripStyle.Show
        Text = "OPTIMA"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        CType(ConexionBDBindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(ConexionBDBindingSource, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents TextBoxPresupuesto As TextBox
    Friend WithEvents TextBoxIdCliente As TextBox
    Friend WithEvents TextBoxCliente As TextBox
    Friend WithEvents TextBoxFecha As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents TextBoxObservaciones As TextBox
    Friend WithEvents TextBoxVendedor As TextBox
    Friend WithEvents TextBoxIdVendedor As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents ConexionBDBindingSource1 As BindingSource
    Friend WithEvents ConexionBDBindingSource As BindingSource
    Friend WithEvents ButtonSiguiente As Button
    Friend WithEvents ButtonAnterior As Button
    Friend WithEvents ButtonNuevoPresup As Button
    Friend WithEvents ButtonBorrar As Button
    Friend WithEvents ButtonGuardar As Button
    Friend WithEvents ButtonBorrarLineas As Button
    Friend WithEvents ButtonNuevaLinea As Button
    Friend WithEvents TextBoxBase As TextBox
    Friend WithEvents TextBoxIva As TextBox
    Friend WithEvents TextBoxTotalPresup As TextBox
    Friend WithEvents LabelBase As Label
    Friend WithEvents LabelIva As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents btnBuscarPresupuesto As Button
    Friend WithEvents LabelStock As Label
End Class
