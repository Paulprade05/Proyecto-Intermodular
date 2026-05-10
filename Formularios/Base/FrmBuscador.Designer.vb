<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmBuscador
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmBuscador))
        TextBoxBuscar = New TextBox()
        Label1 = New Label()
        Panel1 = New Panel()
        ButtonAceptar = New Button()
        ButtonCancelar = New Button()
        DataGridView1 = New DataGridView()
        Panel1.SuspendLayout()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' TextBoxBuscar
        ' 
        TextBoxBuscar.BackColor = Color.Silver
        TextBoxBuscar.BorderStyle = BorderStyle.None
        TextBoxBuscar.Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        TextBoxBuscar.Location = New Point(86, 31)
        TextBoxBuscar.Margin = New Padding(4)
        TextBoxBuscar.Name = "TextBoxBuscar"
        TextBoxBuscar.Size = New Size(129, 18)
        TextBoxBuscar.TabIndex = 0
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(30, 32)
        Label1.Margin = New Padding(4, 0, 4, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(48, 17)
        Label1.TabIndex = 1
        Label1.Text = "Buscar"
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.FromArgb(CByte(40), CByte(50), CByte(70))
        Panel1.Controls.Add(ButtonAceptar)
        Panel1.Controls.Add(ButtonCancelar)
        Panel1.Controls.Add(TextBoxBuscar)
        Panel1.Controls.Add(Label1)
        Panel1.Dock = DockStyle.Top
        Panel1.Location = New Point(0, 0)
        Panel1.Margin = New Padding(4)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(784, 84)
        Panel1.TabIndex = 3
        ' 
        ' ButtonAceptar
        ' 
        ButtonAceptar.BackColor = Color.Silver
        ButtonAceptar.FlatStyle = FlatStyle.Flat
        ButtonAceptar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonAceptar.ForeColor = Color.Black
        ButtonAceptar.Location = New Point(627, 31)
        ButtonAceptar.Name = "ButtonAceptar"
        ButtonAceptar.Size = New Size(113, 29)
        ButtonAceptar.TabIndex = 63
        ButtonAceptar.Text = "Aceptar"
        ButtonAceptar.UseVisualStyleBackColor = False
        ' 
        ' ButtonCancelar
        ' 
        ButtonCancelar.BackColor = Color.Silver
        ButtonCancelar.FlatStyle = FlatStyle.Flat
        ButtonCancelar.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        ButtonCancelar.ForeColor = Color.Black
        ButtonCancelar.Location = New Point(506, 31)
        ButtonCancelar.Name = "ButtonCancelar"
        ButtonCancelar.Size = New Size(113, 29)
        ButtonCancelar.TabIndex = 62
        ButtonCancelar.Text = "Cancelar"
        ButtonCancelar.UseVisualStyleBackColor = False
        ' 
        ' DataGridView1
        ' 
        DataGridView1.BackgroundColor = Color.White
        DataGridView1.BorderStyle = BorderStyle.None
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Dock = DockStyle.Fill
        DataGridView1.Location = New Point(0, 84)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.ReadOnly = True
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.Size = New Size(784, 377)
        DataGridView1.TabIndex = 4
        ' 
        ' FrmBuscador
        ' 
        AutoScaleDimensions = New SizeF(9F, 21F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.WhiteSmoke
        ClientSize = New Size(784, 461)
        Controls.Add(DataGridView1)
        Controls.Add(Panel1)
        Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(4)
        Name = "FrmBuscador"
        StartPosition = FormStartPosition.CenterParent
        Text = "FrmBuscador"
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents TextBoxBuscar As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents ButtonAceptar As Button
    Friend WithEvents ButtonCancelar As Button
End Class
