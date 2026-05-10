<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmProveedores
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
        ButtonNuevo = New Button()
        TextBoxBuscar = New TextBox()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' DataGridView1
        ' 
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Location = New Point(130, 129)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(240, 150)
        DataGridView1.TabIndex = 0
        ' 
        ' ButtonBuscar
        ' 
        ButtonBuscar.Location = New Point(425, 206)
        ButtonBuscar.Name = "ButtonBuscar"
        ButtonBuscar.Size = New Size(75, 23)
        ButtonBuscar.TabIndex = 1
        ButtonBuscar.Text = "Buscar"
        ButtonBuscar.UseVisualStyleBackColor = True
        ' 
        ' ButtonNuevo
        ' 
        ButtonNuevo.Location = New Point(425, 235)
        ButtonNuevo.Name = "ButtonNuevo"
        ButtonNuevo.Size = New Size(75, 23)
        ButtonNuevo.TabIndex = 2
        ButtonNuevo.Text = "Nuevo"
        ButtonNuevo.UseVisualStyleBackColor = True
        ' 
        ' TextBoxBuscar
        ' 
        TextBoxBuscar.Location = New Point(361, 373)
        TextBoxBuscar.Name = "TextBoxBuscar"
        TextBoxBuscar.Size = New Size(100, 23)
        TextBoxBuscar.TabIndex = 3
        ' 
        ' FrmProveedores
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(70), CByte(75), CByte(80))
        ClientSize = New Size(800, 450)
        Controls.Add(TextBoxBuscar)
        Controls.Add(ButtonNuevo)
        Controls.Add(ButtonBuscar)
        Controls.Add(DataGridView1)
        Name = "FrmProveedores"
        Text = "FrmProveedores"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents ButtonBuscar As Button
    Friend WithEvents ButtonNuevo As Button
    Friend WithEvents TextBoxBuscar As TextBox
End Class
