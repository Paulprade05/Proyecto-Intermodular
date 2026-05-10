<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmProveedorDetalle
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
        ButtonGuardar = New Button()
        ButtonBorrar = New Button()
        TextBoxCodigo = New TextBox()
        TextBoxDireccion = New TextBox()
        TextBoxCif = New TextBox()
        TextBoxPoblacion = New TextBox()
        TextBoxNombreFiscal = New TextBox()
        TextBoxTelefono = New TextBox()
        TextBoxNombreComercial = New TextBox()
        TextBoxProvincia = New TextBox()
        TextBoxWeb = New TextBox()
        TextBoxEmail = New TextBox()
        TextBoxContacto = New TextBox()
        TextBoxObservaciones = New TextBox()
        SuspendLayout()
        ' 
        ' ButtonGuardar
        ' 
        ButtonGuardar.Location = New Point(222, 141)
        ButtonGuardar.Name = "ButtonGuardar"
        ButtonGuardar.Size = New Size(75, 23)
        ButtonGuardar.TabIndex = 0
        ButtonGuardar.Text = "Guardar"
        ButtonGuardar.UseVisualStyleBackColor = True
        ' 
        ' ButtonBorrar
        ' 
        ButtonBorrar.Location = New Point(363, 214)
        ButtonBorrar.Name = "ButtonBorrar"
        ButtonBorrar.Size = New Size(75, 23)
        ButtonBorrar.TabIndex = 1
        ButtonBorrar.Text = "Borrar"
        ButtonBorrar.UseVisualStyleBackColor = True
        ' 
        ' TextBoxCodigo
        ' 
        TextBoxCodigo.Location = New Point(148, 202)
        TextBoxCodigo.Name = "TextBoxCodigo"
        TextBoxCodigo.Size = New Size(100, 23)
        TextBoxCodigo.TabIndex = 2
        ' 
        ' TextBoxDireccion
        ' 
        TextBoxDireccion.Location = New Point(350, 214)
        TextBoxDireccion.Name = "TextBoxDireccion"
        TextBoxDireccion.Size = New Size(100, 23)
        TextBoxDireccion.TabIndex = 3
        ' 
        ' TextBoxCif
        ' 
        TextBoxCif.Location = New Point(222, 270)
        TextBoxCif.Name = "TextBoxCif"
        TextBoxCif.Size = New Size(100, 23)
        TextBoxCif.TabIndex = 4
        ' 
        ' TextBoxPoblacion
        ' 
        TextBoxPoblacion.Location = New Point(338, 241)
        TextBoxPoblacion.Name = "TextBoxPoblacion"
        TextBoxPoblacion.Size = New Size(100, 23)
        TextBoxPoblacion.TabIndex = 5
        ' 
        ' TextBoxNombreFiscal
        ' 
        TextBoxNombreFiscal.Location = New Point(311, 185)
        TextBoxNombreFiscal.Name = "TextBoxNombreFiscal"
        TextBoxNombreFiscal.Size = New Size(100, 23)
        TextBoxNombreFiscal.TabIndex = 6
        ' 
        ' TextBoxTelefono
        ' 
        TextBoxTelefono.Location = New Point(406, 270)
        TextBoxTelefono.Name = "TextBoxTelefono"
        TextBoxTelefono.Size = New Size(100, 23)
        TextBoxTelefono.TabIndex = 7
        ' 
        ' TextBoxNombreComercial
        ' 
        TextBoxNombreComercial.Location = New Point(417, 164)
        TextBoxNombreComercial.Name = "TextBoxNombreComercial"
        TextBoxNombreComercial.Size = New Size(100, 23)
        TextBoxNombreComercial.TabIndex = 8
        ' 
        ' TextBoxProvincia
        ' 
        TextBoxProvincia.Location = New Point(300, 283)
        TextBoxProvincia.Name = "TextBoxProvincia"
        TextBoxProvincia.Size = New Size(100, 23)
        TextBoxProvincia.TabIndex = 9
        ' 
        ' TextBoxWeb
        ' 
        TextBoxWeb.Location = New Point(549, 222)
        TextBoxWeb.Name = "TextBoxWeb"
        TextBoxWeb.Size = New Size(100, 23)
        TextBoxWeb.TabIndex = 10
        ' 
        ' TextBoxEmail
        ' 
        TextBoxEmail.Location = New Point(374, 319)
        TextBoxEmail.Name = "TextBoxEmail"
        TextBoxEmail.Size = New Size(100, 23)
        TextBoxEmail.TabIndex = 11
        ' 
        ' TextBoxContacto
        ' 
        TextBoxContacto.Location = New Point(500, 299)
        TextBoxContacto.Name = "TextBoxContacto"
        TextBoxContacto.Size = New Size(100, 23)
        TextBoxContacto.TabIndex = 12
        ' 
        ' TextBoxObservaciones
        ' 
        TextBoxObservaciones.Location = New Point(582, 80)
        TextBoxObservaciones.Name = "TextBoxObservaciones"
        TextBoxObservaciones.Size = New Size(100, 23)
        TextBoxObservaciones.TabIndex = 13
        ' 
        ' FrmProveedorDetalle
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        ClientSize = New Size(800, 450)
        Controls.Add(TextBoxObservaciones)
        Controls.Add(TextBoxContacto)
        Controls.Add(TextBoxEmail)
        Controls.Add(TextBoxWeb)
        Controls.Add(TextBoxProvincia)
        Controls.Add(TextBoxNombreComercial)
        Controls.Add(TextBoxTelefono)
        Controls.Add(TextBoxNombreFiscal)
        Controls.Add(TextBoxPoblacion)
        Controls.Add(TextBoxCif)
        Controls.Add(TextBoxDireccion)
        Controls.Add(TextBoxCodigo)
        Controls.Add(ButtonBorrar)
        Controls.Add(ButtonGuardar)
        Name = "FrmProveedorDetalle"
        StartPosition = FormStartPosition.CenterScreen
        Text = "FrmProveedorDetalle"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents ButtonGuardar As Button
    Friend WithEvents ButtonBorrar As Button
    Friend WithEvents TextBoxCodigo As TextBox
    Friend WithEvents TextBoxDireccion As TextBox
    Friend WithEvents TextBoxCif As TextBox
    Friend WithEvents TextBoxPoblacion As TextBox
    Friend WithEvents TextBoxNombreFiscal As TextBox
    Friend WithEvents TextBoxTelefono As TextBox
    Friend WithEvents TextBoxNombreComercial As TextBox
    Friend WithEvents TextBoxProvincia As TextBox
    Friend WithEvents TextBoxWeb As TextBox
    Friend WithEvents TextBoxEmail As TextBox
    Friend WithEvents TextBoxContacto As TextBox
    Friend WithEvents TextBoxObservaciones As TextBox
End Class
