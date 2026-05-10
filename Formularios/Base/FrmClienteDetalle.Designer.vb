<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmClienteDetalle
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
        TextBoxCodigo = New TextBox()
        TextBoxNombreFiscal = New TextBox()
        TextBoxNombreComercial = New TextBox()
        TextBoxCIF = New TextBox()
        TextBoxDireccion = New TextBox()
        TextBoxPoblacion = New TextBox()
        TextBoxProvincia = New TextBox()
        TextBoxTelefono = New TextBox()
        TextBoxEmail = New TextBox()
        TextBoxContacto = New TextBox()
        TextBoxWeb = New TextBox()
        TextBoxObservaciones = New TextBox()
        ButtonBorrar = New Button()
        ButtonGuardar = New Button()
        SuspendLayout()
        ' 
        ' TextBoxCodigo
        ' 
        TextBoxCodigo.Location = New Point(179, 214)
        TextBoxCodigo.Name = "TextBoxCodigo"
        TextBoxCodigo.Size = New Size(100, 23)
        TextBoxCodigo.TabIndex = 0
        ' 
        ' TextBoxNombreFiscal
        ' 
        TextBoxNombreFiscal.Location = New Point(350, 214)
        TextBoxNombreFiscal.Name = "TextBoxNombreFiscal"
        TextBoxNombreFiscal.Size = New Size(100, 23)
        TextBoxNombreFiscal.TabIndex = 1
        ' 
        ' TextBoxNombreComercial
        ' 
        TextBoxNombreComercial.Location = New Point(358, 222)
        TextBoxNombreComercial.Name = "TextBoxNombreComercial"
        TextBoxNombreComercial.Size = New Size(100, 23)
        TextBoxNombreComercial.TabIndex = 2
        ' 
        ' TextBoxCIF
        ' 
        TextBoxCIF.Location = New Point(366, 230)
        TextBoxCIF.Name = "TextBoxCIF"
        TextBoxCIF.Size = New Size(100, 23)
        TextBoxCIF.TabIndex = 3
        ' 
        ' TextBoxDireccion
        ' 
        TextBoxDireccion.Location = New Point(374, 238)
        TextBoxDireccion.Name = "TextBoxDireccion"
        TextBoxDireccion.Size = New Size(100, 23)
        TextBoxDireccion.TabIndex = 4
        ' 
        ' TextBoxPoblacion
        ' 
        TextBoxPoblacion.Location = New Point(382, 246)
        TextBoxPoblacion.Name = "TextBoxPoblacion"
        TextBoxPoblacion.Size = New Size(100, 23)
        TextBoxPoblacion.TabIndex = 5
        ' 
        ' TextBoxProvincia
        ' 
        TextBoxProvincia.Location = New Point(390, 254)
        TextBoxProvincia.Name = "TextBoxProvincia"
        TextBoxProvincia.Size = New Size(100, 23)
        TextBoxProvincia.TabIndex = 6
        ' 
        ' TextBoxTelefono
        ' 
        TextBoxTelefono.Location = New Point(398, 262)
        TextBoxTelefono.Name = "TextBoxTelefono"
        TextBoxTelefono.Size = New Size(100, 23)
        TextBoxTelefono.TabIndex = 7
        ' 
        ' TextBoxEmail
        ' 
        TextBoxEmail.Location = New Point(406, 270)
        TextBoxEmail.Name = "TextBoxEmail"
        TextBoxEmail.Size = New Size(100, 23)
        TextBoxEmail.TabIndex = 8
        ' 
        ' TextBoxContacto
        ' 
        TextBoxContacto.Location = New Point(414, 278)
        TextBoxContacto.Name = "TextBoxContacto"
        TextBoxContacto.Size = New Size(100, 23)
        TextBoxContacto.TabIndex = 9
        ' 
        ' TextBoxWeb
        ' 
        TextBoxWeb.Location = New Point(422, 286)
        TextBoxWeb.Name = "TextBoxWeb"
        TextBoxWeb.Size = New Size(100, 23)
        TextBoxWeb.TabIndex = 10
        ' 
        ' TextBoxObservaciones
        ' 
        TextBoxObservaciones.Location = New Point(430, 294)
        TextBoxObservaciones.Name = "TextBoxObservaciones"
        TextBoxObservaciones.Size = New Size(100, 23)
        TextBoxObservaciones.TabIndex = 11
        ' 
        ' ButtonBorrar
        ' 
        ButtonBorrar.Location = New Point(363, 214)
        ButtonBorrar.Name = "ButtonBorrar"
        ButtonBorrar.Size = New Size(75, 23)
        ButtonBorrar.TabIndex = 13
        ButtonBorrar.Text = "Borrar"
        ButtonBorrar.UseVisualStyleBackColor = True
        ' 
        ' ButtonGuardar
        ' 
        ButtonGuardar.Location = New Point(288, 68)
        ButtonGuardar.Name = "ButtonGuardar"
        ButtonGuardar.Size = New Size(75, 23)
        ButtonGuardar.TabIndex = 14
        ButtonGuardar.Text = "Guardar"
        ButtonGuardar.UseVisualStyleBackColor = True
        ' 
        ' FrmClienteDetalle
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(ButtonGuardar)
        Controls.Add(ButtonBorrar)
        Controls.Add(TextBoxObservaciones)
        Controls.Add(TextBoxWeb)
        Controls.Add(TextBoxContacto)
        Controls.Add(TextBoxEmail)
        Controls.Add(TextBoxTelefono)
        Controls.Add(TextBoxProvincia)
        Controls.Add(TextBoxPoblacion)
        Controls.Add(TextBoxDireccion)
        Controls.Add(TextBoxCIF)
        Controls.Add(TextBoxNombreComercial)
        Controls.Add(TextBoxNombreFiscal)
        Controls.Add(TextBoxCodigo)
        Name = "FrmClienteDetalle"
        StartPosition = FormStartPosition.CenterScreen
        Text = "FrmClienteDetalle"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents TextBoxCodigo As TextBox
    Friend WithEvents TextBoxNombreFiscal As TextBox
    Friend WithEvents TextBoxNombreComercial As TextBox
    Friend WithEvents TextBoxCIF As TextBox
    Friend WithEvents TextBoxDireccion As TextBox
    Friend WithEvents TextBoxPoblacion As TextBox
    Friend WithEvents TextBoxProvincia As TextBox
    Friend WithEvents TextBoxTelefono As TextBox
    Friend WithEvents TextBoxEmail As TextBox
    Friend WithEvents TextBoxContacto As TextBox
    Friend WithEvents TextBoxWeb As TextBox
    Friend WithEvents TextBoxObservaciones As TextBox
    Friend WithEvents ButtonBorrar As Button
    Friend WithEvents ButtonGuardar As Button
End Class
