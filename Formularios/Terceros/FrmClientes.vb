Imports System.Data.SQLite

Public Class FrmClientes

    Private Sub FrmClientes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        FrmPresupuestos.EstilizarGrid(DataGridView1)
        ConfigurarEstilosExtraGrid() ' Magia del fondo oscuro y filas alternas
        ReorganizarPantalla()
        InicializarPlaceholder()
        CargarListaClientes()
    End Sub

    ' 1. DISEÑO DE LA PANTALLA PRINCIPAL (Modo Premium)

    ' Función que crea etiquetas especiales por código (Corregida)
    Private Function CrearEtiquetaEspecial(nombre As String, textoInicial As String, tamano As Single, colorTexto As Color, x As Integer, y As Integer) As Label
        Dim lbl As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = nombre)

        If lbl Is Nothing Then
            lbl = New Label()
            lbl.Name = nombre
            lbl.Text = textoInicial ' Solo lo ponemos la primera vez
            lbl.AutoSize = True
            lbl.BackColor = Color.Transparent
            Me.Controls.Add(lbl)
        End If

        lbl.Font = New Font("Segoe UI", tamano, FontStyle.Bold)
        lbl.ForeColor = colorTexto
        lbl.Location = New Point(x, y)
        lbl.BringToFront()

        Return lbl
    End Function
    ' 4. EFECTOS VISUALES (Dar vida a la interfaz)

    ' Variable para recordar qué fila estaba iluminada antes
    Private _ultimoIndiceFilaHover As Integer = -1

    Private Sub DataGridView1_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellMouseEnter
        ' Si el ratón entra en una fila válida (no la cabecera)
        If e.RowIndex >= 0 Then
            ' Si nos hemos movido a una fila distinta de la anterior
            If e.RowIndex <> _ultimoIndiceFilaHover Then
                ' Restauramos la fila anterior a su color original (si había una)
                If _ultimoIndiceFilaHover >= 0 AndAlso _ultimoIndiceFilaHover < DataGridView1.Rows.Count Then
                    ' El color depende de si es par o impar (zebra striping)
                    If _ultimoIndiceFilaHover Mod 2 = 0 Then
                        DataGridView1.Rows(_ultimoIndiceFilaHover).DefaultCellStyle.BackColor = Color.White
                    Else
                        DataGridView1.Rows(_ultimoIndiceFilaHover).DefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250)
                    End If
                End If

                ' Iluminamos la nueva fila con un color de "selección suave"
                DataGridView1.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.FromArgb(220, 235, 255) ' Un azul muy clarito

                ' Recordamos que esta es la fila iluminada ahora
                _ultimoIndiceFilaHover = e.RowIndex
            End If
        End If
    End Sub

    Private Sub DataGridView1_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellMouseLeave
        ' Cuando el ratón sale de la tabla, restauramos la última fila que estaba iluminada
        If _ultimoIndiceFilaHover >= 0 AndAlso _ultimoIndiceFilaHover < DataGridView1.Rows.Count Then
            If _ultimoIndiceFilaHover Mod 2 = 0 Then
                DataGridView1.Rows(_ultimoIndiceFilaHover).DefaultCellStyle.BackColor = Color.White
            Else
                DataGridView1.Rows(_ultimoIndiceFilaHover).DefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250)
            End If
            _ultimoIndiceFilaHover = -1
        End If
    End Sub
    Private Sub ReorganizarPantalla()
        Dim margen As Integer = 30

        ' 1. TÍTULO GIGANTE
        CrearEtiquetaEspecial("lblTituloMain", "Directorio de Clientes", 20, Color.White, margen, 20)

        ' 2. BUSCADOR Y BOTONES
        Dim yControles As Integer = 75
        TextBoxBuscar.Bounds = New Rectangle(margen, yControles, 300, 27)
        TextBoxBuscar.Font = New Font("Segoe UI", 10)

        EstilizarBoton(ButtonBuscar, margen + 310, yControles - 4, Color.FromArgb(85, 85, 85), Color.White)
        ButtonBuscar.Text = "Buscar"

        EstilizarBoton(ButtonNuevo, margen + 420, yControles - 4, Color.FromArgb(0, 120, 215), Color.White)
        ButtonNuevo.Text = "+ Nuevo Cliente"
        ButtonNuevo.Width = 140

        ' 3. LA TABLA
        Dim yTabla As Integer = 130
        Dim margenAbajo As Integer = 50
        DataGridView1.Bounds = New Rectangle(margen, yTabla, Me.ClientSize.Width - (margen * 2), Me.ClientSize.Height - yTabla - margenAbajo)

        ' 4. BARRA DE ESTADO INFERIOR
        Dim yEstado As Integer = DataGridView1.Bottom + 10
        CrearEtiquetaEspecial("lblEstado", "Cargando datos...", 10, Color.LightGray, margen, yEstado)

        ' 5. ANCHORS (Responsive)
        TextBoxBuscar.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        ButtonBuscar.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        ButtonNuevo.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        Dim lblEst = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "lblEstado")
        If lblEst IsNot Nothing Then lblEst.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
    End Sub
    ' 5. PLACEHOLDER DEL BUSCADOR
    ' Texto que queremos mostrar cuando está vacío
    Private Const TEXTO_PLACEHOLDER As String = "🔍 Buscar por Nombre, CIF o Código..."

    ' Esto se ejecuta al cargar el formulario para poner el texto inicial
    Private Sub InicializarPlaceholder()
        TextBoxBuscar.Text = TEXTO_PLACEHOLDER
        TextBoxBuscar.ForeColor = Color.Gray
    End Sub

    ' Cuando el usuario hace clic en la caja
    Private Sub TextBoxBuscar_Enter(sender As Object, e As EventArgs) Handles TextBoxBuscar.Enter
        If TextBoxBuscar.Text = TEXTO_PLACEHOLDER Then
            TextBoxBuscar.Text = ""
            TextBoxBuscar.ForeColor = Color.Black ' Vuelve a color normal para escribir
        End If
    End Sub

    ' Cuando el usuario sale de la caja sin escribir nada
    Private Sub TextBoxBuscar_Leave(sender As Object, e As EventArgs) Handles TextBoxBuscar.Leave
        If String.IsNullOrWhiteSpace(TextBoxBuscar.Text) Then
            TextBoxBuscar.Text = TEXTO_PLACEHOLDER
            TextBoxBuscar.ForeColor = Color.Gray
        End If
    End Sub
    Private Sub ConfigurarEstilosExtraGrid()
        ' Fondo transparente (del color del formulario) y sin bordes feos
        DataGridView1.BackgroundColor = Me.BackColor
        DataGridView1.BorderStyle = BorderStyle.None

        ' Quitar la columna lateral vacía
        DataGridView1.RowHeadersVisible = False

        ' Filas alternas para mejor legibilidad
        DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250)

        ConfigurarColumnasGrid()
    End Sub

    Private Sub ConfigurarColumnasGrid()
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.ReadOnly = True
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        DataGridView1.Columns.Clear()

        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CodigoCliente", .DataPropertyName = "CodigoCliente", .HeaderText = "Código", .Width = 90})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CIF", .DataPropertyName = "CIF", .HeaderText = "CIF", .Width = 100})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "NombreFiscal", .DataPropertyName = "NombreFiscal", .HeaderText = "Nombre Fiscal", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Poblacion", .DataPropertyName = "Poblacion", .HeaderText = "Población", .Width = 150})
        DataGridView1.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Telefono", .DataPropertyName = "Telefono", .HeaderText = "Teléfono", .Width = 120})
    End Sub

    Private Sub EstilizarBoton(btn As Button, x As Integer, y As Integer, bg As Color, fg As Color)
        btn.Location = New Point(x, y) : btn.Size = New Size(100, 35) : btn.FlatStyle = FlatStyle.Flat
        btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold) : btn.Cursor = Cursors.Hand
        btn.BackColor = bg : btn.ForeColor = fg : btn.FlatAppearance.BorderSize = 0
    End Sub

    ' 2. LÓGICA DE BÚSQUEDA Y APERTURA DE FICHA

    Private Sub CargarListaClientes(Optional filtro As String = "")
        Try
            Dim conexion = ConexionBD.GetConnection()
            Dim sql As String = "SELECT CodigoCliente, NombreFiscal, CIF, Poblacion, Telefono FROM Clientes "

            If Not String.IsNullOrEmpty(filtro) Then
                sql &= "WHERE CodigoCliente LIKE @filtro OR NombreFiscal LIKE @filtro "
            End If
            sql &= "ORDER BY CodigoCliente ASC"

            Using cmd As New SQLiteCommand(sql, conexion)
                If Not String.IsNullOrEmpty(filtro) Then
                    cmd.Parameters.AddWithValue("@filtro", "%" & filtro & "%")
                End If

                Dim da As New SQLiteDataAdapter(cmd)
                Dim dt As New DataTable()
                da.Fill(dt)
                DataGridView1.DataSource = dt

                ' ACTUALIZAR LA BARRA DE ESTADO
                Dim lblEst = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "lblEstado")
                If lblEst IsNot Nothing Then
                    lblEst.Text = $"Total clientes listados: {dt.Rows.Count}"
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar la lista: " & ex.Message)
        End Try
    End Sub

    ' 2. LÓGICA DE BÚSQUEDA (Con Búsqueda en Vivo y Filtro Vacío)
    Private Sub EjecutarBusqueda()
        Dim textoBusqueda As String = TextBoxBuscar.Text.Trim()

        ' Si la caja está vacía O tiene el texto de ayuda gris, cargamos todo
        If textoBusqueda = "" OrElse textoBusqueda = TEXTO_PLACEHOLDER Then
            CargarListaClientes("") ' Pasa vacío para que no filtre
        Else
            CargarListaClientes(textoBusqueda)
        End If
    End Sub

    ' Cuando el usuario hace clic en el botón
    Private Sub ButtonBuscar_Click(sender As Object, e As EventArgs) Handles ButtonBuscar.Click
        EjecutarBusqueda()
    End Sub

    ' Cuando el usuario pulsa Enter
    Private Sub TextBoxBuscar_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBoxBuscar.KeyDown
        If e.KeyCode = Keys.Enter Then
            EjecutarBusqueda()
            e.SuppressKeyPress = True ' Evita el ruidito de Windows al pulsar Enter
        End If
    End Sub

    ' ¡MAGIA! Búsqueda en Vivo: Si el usuario borra todo, la tabla se llena sola al instante
    Private Sub TextBoxBuscar_TextChanged(sender As Object, e As EventArgs) Handles TextBoxBuscar.TextChanged
        ' Solo actualizamos en vivo si la caja se ha quedado vacía (para que no colapse la base de datos buscando letra a letra)
        If TextBoxBuscar.Text.Trim() = "" Then
            CargarListaClientes("")
        End If
    End Sub

    ' 3. ABRIR EL MENÚ DE GESTIÓN (Ficha Emergente)
    Private Sub ButtonNuevo_Click(sender As Object, e As EventArgs) Handles ButtonNuevo.Click
        Dim frmFicha As New FrmClienteDetalle()
        frmFicha.CodigoClienteSeleccionado = ""  ' Vacío significa NUEVO
        frmFicha.ShowDialog()
        CargarListaClientes()
    End Sub

    Private Sub DataGridView1_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellDoubleClick
        If e.RowIndex >= 0 Then
            Dim codigoSel As String = DataGridView1.Rows(e.RowIndex).Cells("CodigoCliente").Value.ToString()
            Dim frmFicha As New FrmClienteDetalle()
            frmFicha.CodigoClienteSeleccionado = codigoSel
            frmFicha.ShowDialog()
            CargarListaClientes()
        End If
    End Sub

    Private Sub FrmClientes_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Me.IsHandleCreated AndAlso Me.ClientSize.Width > 0 Then ReorganizarPantalla()
    End Sub
End Class
