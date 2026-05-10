<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmClientes
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
        DataGridView1 = New DataGridView()
        ButtonBuscar = New Button()
        TextBoxBuscar = New TextBox()
        ButtonNuevo = New Button()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' DataGridView1
        ' 
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Location = New Point(250, 181)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(240, 150)
        DataGridView1.TabIndex = 2
        ' 
        ' ButtonBuscar
        ' 
        ButtonBuscar.Location = New Point(597, 92)
        ButtonBuscar.Name = "ButtonBuscar"
        ButtonBuscar.Size = New Size(75, 23)
        ButtonBuscar.TabIndex = 4
        ButtonBuscar.Text = "Button2"
        ButtonBuscar.UseVisualStyleBackColor = True
        ' 
        ' TextBoxBuscar
        ' 
        TextBoxBuscar.Location = New Point(241, 430)
        TextBoxBuscar.Name = "TextBoxBuscar"
        TextBoxBuscar.Size = New Size(100, 25)
        TextBoxBuscar.TabIndex = 7
        ' 
        ' ButtonNuevo
        ' 
        ButtonNuevo.Location = New Point(805, 369)
        ButtonNuevo.Name = "ButtonNuevo"
        ButtonNuevo.Size = New Size(75, 23)
        ButtonNuevo.TabIndex = 8
        ButtonNuevo.Text = "Button2"
        ButtonNuevo.UseVisualStyleBackColor = True
        ' 
        ' FrmClientes
        ' 
        AutoScaleDimensions = New SizeF(8F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        ClientSize = New Size(1684, 761)
        Controls.Add(ButtonNuevo)
        Controls.Add(TextBoxBuscar)
        Controls.Add(ButtonBuscar)
        Controls.Add(DataGridView1)
        Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ForeColor = Color.WhiteSmoke
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmClientes"
        SizeGripStyle = SizeGripStyle.Show
        Text = "OPTIMA"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents ButtonGuardar As Button
    Friend WithEvents ButtonBuscar As Button
    Friend WithEvents ButtonNuevo As Button
    Friend WithEvents TextBoxCodigo As TextBox
    Friend WithEvents TextBoxBuscar As TextBox
    Friend WithEvents TextBoxCIF As TextBox
    Friend WithEvents TextBoxDireccion As TextBox
    Friend WithEvents TextBoxPoblacion As TextBox
    Friend WithEvents TextBoxProvincia As TextBox
    Friend WithEvents TextBoxNombreComercial As TextBox
    Friend WithEvents TextBoxTelefono As TextBox
    Friend WithEvents TextBoxEmail As TextBox
    Friend WithEvents TextBoxContacto As TextBox
    Friend WithEvents TextBoxWeb As TextBox
    Friend WithEvents TextBoxObservaciones As TextBox
End Class
