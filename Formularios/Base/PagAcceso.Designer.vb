<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PagAcceso
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PagAcceso))
        TextBoxUsuario = New TextBox()
        TextBoxPassword = New TextBox()
        ButtonAcceso = New Button()
        ButtonCrearUsuario = New Button()
        ButtonVerPasswd = New Button()
        ComboBox1 = New ComboBox()
        PictureBox1 = New PictureBox()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' TextBoxUsuario
        ' 
        TextBoxUsuario.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        TextBoxUsuario.Location = New Point(162, 229)
        TextBoxUsuario.Name = "TextBoxUsuario"
        TextBoxUsuario.Size = New Size(175, 25)
        TextBoxUsuario.TabIndex = 3
        ' 
        ' TextBoxPassword
        ' 
        TextBoxPassword.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        TextBoxPassword.Location = New Point(162, 302)
        TextBoxPassword.Name = "TextBoxPassword"
        TextBoxPassword.Size = New Size(175, 25)
        TextBoxPassword.TabIndex = 4
        ' 
        ' ButtonAcceso
        ' 
        ButtonAcceso.BackColor = Color.FromArgb(CByte(40), CByte(50), CByte(70))
        ButtonAcceso.FlatAppearance.BorderSize = 0
        ButtonAcceso.FlatStyle = FlatStyle.Flat
        ButtonAcceso.Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ButtonAcceso.ForeColor = Color.White
        ButtonAcceso.Location = New Point(257, 356)
        ButtonAcceso.Name = "ButtonAcceso"
        ButtonAcceso.Size = New Size(80, 29)
        ButtonAcceso.TabIndex = 5
        ButtonAcceso.Text = "Entrar"
        ButtonAcceso.UseVisualStyleBackColor = False
        ' 
        ' ButtonCrearUsuario
        ' 
        ButtonCrearUsuario.BackColor = Color.FromArgb(CByte(40), CByte(50), CByte(70))
        ButtonCrearUsuario.FlatAppearance.BorderSize = 0
        ButtonCrearUsuario.FlatStyle = FlatStyle.Flat
        ButtonCrearUsuario.Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        ButtonCrearUsuario.ForeColor = Color.White
        ButtonCrearUsuario.Location = New Point(162, 356)
        ButtonCrearUsuario.Name = "ButtonCrearUsuario"
        ButtonCrearUsuario.Size = New Size(80, 29)
        ButtonCrearUsuario.TabIndex = 6
        ButtonCrearUsuario.Text = "Crear"
        ButtonCrearUsuario.UseVisualStyleBackColor = False
        ' 
        ' ButtonVerPasswd
        ' 
        ButtonVerPasswd.BackgroundImage = CType(resources.GetObject("ButtonVerPasswd.BackgroundImage"), Image)
        ButtonVerPasswd.BackgroundImageLayout = ImageLayout.Stretch
        ButtonVerPasswd.Location = New Point(343, 302)
        ButtonVerPasswd.Name = "ButtonVerPasswd"
        ButtonVerPasswd.Size = New Size(25, 25)
        ButtonVerPasswd.TabIndex = 7
        ButtonVerPasswd.UseVisualStyleBackColor = True
        ' 
        ' ComboBox1
        ' 
        ComboBox1.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox1.FlatStyle = FlatStyle.Flat
        ComboBox1.ForeColor = Color.Black
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"ASD", "ASD", "ADS"})
        ComboBox1.Location = New Point(162, 422)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(175, 25)
        ComboBox1.TabIndex = 8
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackgroundImage = CType(resources.GetObject("PictureBox1.BackgroundImage"), Image)
        PictureBox1.BackgroundImageLayout = ImageLayout.Zoom
        PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), Image)
        PictureBox1.Location = New Point(376, 200)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(100, 50)
        PictureBox1.TabIndex = 12
        PictureBox1.TabStop = False
        ' 
        ' PagAcceso
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Gray
        BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), Image)
        BackgroundImageLayout = ImageLayout.Stretch
        ClientSize = New Size(484, 561)
        Controls.Add(PictureBox1)
        Controls.Add(ComboBox1)
        Controls.Add(ButtonVerPasswd)
        Controls.Add(ButtonCrearUsuario)
        Controls.Add(ButtonAcceso)
        Controls.Add(TextBoxPassword)
        Controls.Add(TextBoxUsuario)
        Font = New Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        ForeColor = Color.Black
        FormBorderStyle = FormBorderStyle.Fixed3D
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MinimizeBox = False
        Name = "PagAcceso"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Optima"
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents TextBoxUsuario As TextBox
    Friend WithEvents TextBoxPassword As TextBox
    Friend WithEvents ButtonAcceso As Button
    Friend WithEvents ButtonCrearUsuario As Button
    Friend WithEvents ButtonVerPasswd As Button
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents PictureBox1 As PictureBox

End Class
