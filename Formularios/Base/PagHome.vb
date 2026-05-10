Imports System.Data.SQLite
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Drawing.Imaging ' <--- NECESARIO PARA CAMBIAR COLORES
Public Class PagHome
    Private formularioActivo As Form = Nothing
    ' Importamos la función nativa de Windows para congelar el dibujo
    Private Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    Private Const WM_SETREDRAW As Integer = &HB
    ' Vigilante para las copias automáticas
    Private WithEvents RelojBackups As New Timer()
    Private ultimoBackupRealizado As String = "" ' Para evitar que haga 60 copias en el mismo minuto
    Private Sub PagHome_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 1. CONFIGURACIÓN DEL MENÚ LATERAL (TREEVIEW)
        ' Fijamos el ancho y lo anclamos a la izquierda para que ocupe todo el alto
        TvNavegacion.Dock = DockStyle.Left
        TvNavegacion.Width = 330 ' Un ancho cómodo para leer

        ' 2. CONFIGURACIÓN DEL LOGO
        ' Truco: Si quieres el logo DEBAJO del menú, lo ideal sería usar un Panel contenedor.
        ' Si no quieres cambiar el diseño ahora, un truco rápido es anclarlo abajo a la izquierda:
        'PictureBoxLogoHome.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        PictureBoxLogoHome.Visible = False
        ' 3. CONFIGURACIÓN DEL PANEL PRINCIPAL (Vital para ser Responsive)
        ' Esto hace que el panel gris ocupe TODO el espacio que sobra a la derecha
        Panel.Dock = DockStyle.Fill

        ' 4. ALINEACIÓN DEL TEXTO DE LA EMPRESA
        ' Esto corrige los espacios en blanco que tenías en el MenuStrip
        NombreEmpresa.Alignment = ToolStripItemAlignment.Right
        CargarDatosEmpresa()

        ' 1. CONFIGURACIÓN VISUAL DEL FORMULARIO
        Me.DoubleBuffered = True

        ' 2. CONFIGURACIÓN DEL MENUSTRIP (BARRA SUPERIOR)
        Dim miMenu = MenuStripVacio

        miMenu.Renderer = New RenderizadorMenuSutil(New ColoresMenuModerno())
        miMenu.BackColor = Color.FromArgb(40, 50, 70)
        miMenu.ForeColor = Color.WhiteSmoke
        miMenu.Font = New Font("Segoe UI", 11, FontStyle.Regular)

        ' 1. Quitamos el relleno al contenedor principal para que los botones toquen los bordes
        miMenu.AutoSize = False
        miMenu.Height = 55
        miMenu.Padding = New Padding(10, 0, 10, 0)

        ' AÑADIR DESPLEGABLE DE INFORMES DINÁMICAMENTE
        Dim btnInformes As ToolStripMenuItem = Nothing
        For Each itm As ToolStripItem In miMenu.Items
            If TypeOf itm Is ToolStripMenuItem AndAlso itm.Text.Trim().ToLower() = "informes" Then
                btnInformes = DirectCast(itm, ToolStripMenuItem)
                Exit For
            End If
        Next

        If btnInformes IsNot Nothing Then
            btnInformes.DropDownItems.Clear()

            ' Quitamos la franja gris fea de la izquierda
            Dim menuDesplegable = DirectCast(btnInformes.DropDown, ToolStripDropDownMenu)
            menuDesplegable.ShowImageMargin = False
            menuDesplegable.ShowCheckMargin = False
            menuDesplegable.BackColor = Color.FromArgb(40, 50, 70)

            ' --- VENTAS Y FACTURACIÓN ---
            btnInformes.DropDownItems.Add("Listado de Facturas Emitidas (IVA)", Nothing, AddressOf InformeFacturasEmitidas_Click)
            btnInformes.DropDownItems.Add("Ranking de Ventas por Cliente", Nothing, AddressOf InformeVentasCliente_Click)
            btnInformes.DropDownItems.Add("Comisiones por Vendedor", Nothing, AddressOf InformeVentasVendedor_Click)

            btnInformes.DropDownItems.Add("-") ' Separador

            ' --- TESORERÍA Y OPERACIONES ---
            btnInformes.DropDownItems.Add("Facturas Pendientes de Cobro", Nothing, AddressOf InformePendientesCobro_Click)
            btnInformes.DropDownItems.Add("Pedidos Pendientes de Servir", Nothing, AddressOf InformePedidosPendientes_Click)
            btnInformes.DropDownItems.Add("Hoja de Rutas y Envíos Diarios", Nothing, AddressOf InformeRutas_Click)

            btnInformes.DropDownItems.Add("-") ' Separador

            ' --- ALMACÉN E INVENTARIO ---
            btnInformes.DropDownItems.Add("Inventario Valorado", Nothing, AddressOf InformeInventarioValorado_Click)
            btnInformes.DropDownItems.Add("Alerta de Stock Mínimo", Nothing, AddressOf InformeStockMinimo_Click)
            btnInformes.DropDownItems.Add("Top Artículos Más Vendidos", Nothing, AddressOf InformeArticulosMasVendidos_Click)

            ' (El estilo de los sub-botones se aplica abajo, en el bucle general que recorre
            '  todos los items del MenuStrip — evitamos duplicarlo aquí.)
        End If
        ' AÑADIR DESPLEGABLE DE CONFIGURACIÓN DINÁMICAMENTE
        Dim btnConfiguracion As ToolStripMenuItem = Nothing
        For Each itm As ToolStripItem In miMenu.Items
            If TypeOf itm Is ToolStripMenuItem AndAlso (itm.Text.Trim().ToLower() = "configuracion" OrElse itm.Text.Trim().ToLower() = "configuración") Then
                btnConfiguracion = DirectCast(itm, ToolStripMenuItem)
                Exit For
            End If
        Next

        If btnConfiguracion IsNot Nothing Then
            btnConfiguracion.DropDownItems.Clear()

            Dim menuDesplegableConf = DirectCast(btnConfiguracion.DropDown, ToolStripDropDownMenu)
            menuDesplegableConf.ShowImageMargin = False
            menuDesplegableConf.ShowCheckMargin = False
            menuDesplegableConf.BackColor = Color.FromArgb(40, 50, 70)

            ' SOLO LAS 3 OPCIONES QUE HAS PEDIDO
            btnConfiguracion.DropDownItems.Add("🏢 Datos de la Empresa", Nothing, AddressOf ConfigDatosEmpresa_Click)
            btnConfiguracion.DropDownItems.Add("👥 Usuarios y Permisos", Nothing, AddressOf ConfigUsuarios_Click)
            btnConfiguracion.DropDownItems.Add("💾 Copias de Seguridad", Nothing, AddressOf ConfigBackups_Click)

            ' (El estilo de los sub-botones se aplica abajo, en el bucle general que recorre
            '  todos los items del MenuStrip — evitamos duplicarlo aquí.)
        End If
        ' Bucle único que estiliza TODOS los items del MenuStrip (principales y submenús).
        ' Antes había bloques duplicados que aplicaban Padding y luego eran sobrescritos aquí.
        For Each item As ToolStripItem In miMenu.Items
            item.ForeColor = Color.WhiteSmoke

            ' Le damos relleno interno al botón principal para que la zona clickeable mida los 55px.
            item.Padding = New Padding(12, 16, 12, 16)

            ' Estilo de los items del desplegable (si los hay)
            If TypeOf item Is ToolStripMenuItem Then
                Dim menu As ToolStripMenuItem = DirectCast(item, ToolStripMenuItem)
                For Each subItem As ToolStripItem In menu.DropDownItems
                    subItem.ForeColor = Color.WhiteSmoke
                    subItem.Font = New Font("Segoe UI", 10.5F, FontStyle.Regular)
                    subItem.Padding = New Padding(20, 5, 20, 5)
                Next
            End If

            ' Estilo de los TextBox que estén en la barra (campo búsqueda, nombre empresa, etc.)
            If TypeOf item Is ToolStripTextBox Then
                item.BackColor = Color.FromArgb(40, 50, 70)
                item.ForeColor = Color.WhiteSmoke
            End If
        Next

        ' 3. CONFIGURACIÓN DEL TREEVIEW
        TvNavegacion.DrawMode = TreeViewDrawMode.OwnerDrawText
        TvNavegacion.BackColor = Color.FromArgb(40, 50, 70)
        TvNavegacion.ForeColor = Color.WhiteSmoke
        TvNavegacion.Font = New Font("Segoe UI", 11, FontStyle.Regular)
        TvNavegacion.ItemHeight = 45
        TvNavegacion.ShowLines = False
        TvNavegacion.ShowPlusMinus = False
        TvNavegacion.FullRowSelect = True
        TvNavegacion.BorderStyle = BorderStyle.None

        ' 4. CARGA DE DATOS
        Dim metodoSetStyle = GetType(Control).GetMethod("SetStyle",
    System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)

        metodoSetStyle.Invoke(Panel, New Object() {ControlStyles.AllPaintingInWmPaint Or
            ControlStyles.UserPaint Or ControlStyles.DoubleBuffer, True})
        ' 5. MAGIA INICIAL: ABRIR EL DASHBOARD POR DEFECTO
        AbrirFormulario(New FrmDashboard())
        ' Arrancamos el vigilante de copias de seguridad. Comprobamos cada 15 segundos para
        ' asegurarnos de que aunque el sistema esté ocupado no nos saltamos el minuto exacto
        ' de la hora programada.
        RelojBackups.Interval = 15000
        RelojBackups.Start()
    End Sub
    ' Abre un formulario hijo dentro del panel contenedor ajustando su tamaño automáticamente.
    Private Sub AbrirFormularioEnPanel(ByVal formHijo As Form)
        ' 1. Limpieza: Si ya hay un formulario abierto, lo quitamos
        If Me.Panel.Controls.Count > 0 Then
            ' Opcional: Cerrar el anterior para liberar memoria
            Dim formAnterior As Form = TryCast(Me.Panel.Controls(0), Form)
            If formAnterior IsNot Nothing Then formAnterior.Close()

            Me.Panel.Controls.Clear()
        End If

        ' 2. Configuración "Responsive":
        ' Le decimos que no es una ventana independiente (TopLevel = False)
        formHijo.TopLevel = False
        ' Quitamos los bordes de ventana para que parezca parte del panel
        formHijo.FormBorderStyle = FormBorderStyle.None
        ' ¡ESTO ES LO MÁS IMPORTANTE! Dock = Fill hace que se estire con el panel
        formHijo.Dock = DockStyle.Fill

        ' 3. Inyección:
        Me.Panel.Controls.Add(formHijo)
        Me.Panel.Tag = formHijo
        formHijo.Show()
    End Sub
    Private Sub TvNavegacion_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles TvNavegacion.NodeMouseClick
        ' Si hacen clic con el botón derecho, lo ignoramos
        If e.Button <> MouseButtons.Left Then Return

        ' Quitamos espacios sobrantes por si acaso con .Trim()
        Select Case e.Node.Text.Trim()

            ' --- VENTAS ---
            Case "Presupuestos", "Presupuesto"
                AbrirFormulario(New FrmPresupuestos())
            Case "Pedidos", "Pedido"
                AbrirFormulario(New FrmPedidos())
            Case "Facturas", "Factura"
                AbrirFormulario(New FrmFacturas())
            Case "Albaranes", "Albaran", "Albarán"
                AbrirFormulario(New FrmAlbaranes())
            ' --- COMPRAS ---
            Case "Pedidos a proveedor"
                AbrirFormulario(New FrmPedidosCompra())
            Case "Albaranes entrada"
                AbrirFormulario(New FrmAlbaranesCompra())
            Case "Facturas compra"
                AbrirFormulario(New FrmFacturasCompra())
            ' --- TERCEROS ---
            Case "Clientes", "Cliente"
                AbrirFormulario(New FrmClientes())
            Case "Proveedores", "Proveedor"
                AbrirFormulario(New FrmProveedores())

            ' --- ALMACEN ---
            Case "Artículos", "Articulos", "Artículo", "Articulo" ' <--- CUBRIMOS TODAS LAS OPCIONES
                AbrirFormulario(New frmArticulos())
            Case "Familias", "Familia"
                Dim frm As New FrmFamilias()
                frm.ShowDialog()
            Case "Movimientos de almacen", "Movimientos de almacén"
                AbrirFormulario(New FrmMovimientos())

            ' --- TABLAS ---
            Case "Vendedores", "Vendedor"
                Dim frm As New FrmVendedores()
                frm.ShowDialog()
            Case "Formas de pago", "Forma de pago"
                Dim frm As New FrmFormPag()
                frm.ShowDialog()
            Case "Rutas", "Ruta"
                Dim frm As New FrmRutas()
                frm.ShowDialog()
            Case "Agencias", "Agencia"
                Dim frm As New FrmAgencias()
                frm.ShowDialog()
        End Select
    End Sub
    Private Sub TvNavegacion_DrawNode(sender As Object, e As DrawTreeNodeEventArgs) Handles TvNavegacion.DrawNode
        ' 1. CONFIGURACIÓN DE COLORES
        Dim colorFondo As Color = Color.FromArgb(40, 50, 70)
        Dim colorSeleccion As Color = Color.FromArgb(55, 65, 85) ' Hover/Selección más sutil
        Dim colorAcento As Color = Color.FromArgb(0, 120, 215)
        Dim colorTexto As Color

        Dim isSelected As Boolean = (e.State And TreeNodeStates.Selected) <> 0
        Dim rectCompleto As New Rectangle(0, e.Bounds.Y, TvNavegacion.Width, e.Bounds.Height)

        ' 2. PINTAR FONDO DE LA FILA
        Using pincelFondo As New SolidBrush(If(isSelected, colorSeleccion, colorFondo))
            e.Graphics.FillRectangle(pincelFondo, rectCompleto)
        End Using

        ' 3. BARRITA LATERAL IZQUIERDA (INDICADOR DE SELECCIÓN)
        If isSelected Then
            Using pincelBarra As New SolidBrush(colorAcento)
                e.Graphics.FillRectangle(pincelBarra, New Rectangle(0, e.Bounds.Y, 4, e.Bounds.Height))
            End Using
        End If
        ' 4. DEFINIR ESTILO SEGÚN NIVEL (PADRE O HIJO)
        Dim fuenteNodo As Font
        Dim sangria As Integer = e.Node.Level * 25 ' 25px de margen por nivel

        If e.Node.Level = 0 Then
            ' Nodos Raíz (Ventas, Compras, Almacén...)
            ' SUBIMOS A 12 puntos
            fuenteNodo = New Font("Segoe UI", 12, FontStyle.Bold)
            colorTexto = If(isSelected, Color.White, Color.FromArgb(150, 160, 180))
        Else
            ' Nodos Hijos (Artículos, Familias...)
            ' SUBIMOS A 11.5 o 12 puntos (un pelín más fino que el padre)
            fuenteNodo = New Font("Segoe UI", 11.5F, FontStyle.Regular)
            colorTexto = Color.WhiteSmoke
        End If

        ' 5. DIBUJAR ICONO (CON FILTRO BLANCO)
        Dim xPosIcono As Integer = 12 + sangria
        Dim anchoIcono As Integer = 0

        If TvNavegacion.ImageList IsNot Nothing AndAlso e.Node.ImageIndex >= 0 Then
            Dim img As Image = TvNavegacion.ImageList.Images(e.Node.ImageIndex)
            Dim yPosIcono As Integer = e.Bounds.Y + ((e.Bounds.Height - img.Height) \ 2)

            ' Matriz para convertir icono negro en blanco puro
            Dim matrix As New System.Drawing.Imaging.ColorMatrix(New Single()() {
                New Single() {-1, 0, 0, 0, 0},
                New Single() {0, -1, 0, 0, 0},
                New Single() {0, 0, -1, 0, 0},
                New Single() {0, 0, 0, 1, 0},
                New Single() {1, 1, 1, 0, 1}
            })
            ' IMPORTANTE: ImageAttributes debe liberarse o se acumula GDI handles en cada repintado
            Using attr As New System.Drawing.Imaging.ImageAttributes()
                attr.SetColorMatrix(matrix)

                e.Graphics.DrawImage(img, New Rectangle(xPosIcono, yPosIcono, img.Width, img.Height),
                                     0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attr)
            End Using

            anchoIcono = img.Width + 10
        End If

        ' 6. PINTAR TEXTO FINAL
        Dim rectTexto As New Rectangle(xPosIcono + anchoIcono, e.Bounds.Y, rectCompleto.Width - (xPosIcono + anchoIcono), rectCompleto.Height)

        TextRenderer.DrawText(e.Graphics, e.Node.Text, fuenteNodo, rectTexto, colorTexto,
                              TextFormatFlags.VerticalCenter Or TextFormatFlags.Left)

        ' Liberar memoria de la fuente creada al vuelo
        fuenteNodo.Dispose()
    End Sub
    ' 3. EVENTO DE PINTADO (Copia y pega este evento en tu código)
    ' Evento que salta al clicar un nodo del árbol


    ' --- MÉTODO PRO PARA GESTIONAR FORMULARIOS EN EL PANEL ---
    ' Variable global en PagHome para saber qué tenemos en pantalla


    Public Sub AbrirFormulario(formularioHijo As Form)
        ' 1. Si ya estamos en el formulario solicitado, volvemos al Dashboard.
        '    Esto da un comportamiento "toggle": clic en la sección donde ya estás te lleva
        '    de vuelta al inicio. La excepción es el propio Dashboard — clicar el Dashboard
        '    estando en él no hace nada (no tiene sentido cerrarlo y abrirlo de nuevo).
        If formularioActivo IsNot Nothing AndAlso formularioActivo.GetType() = formularioHijo.GetType() Then
            formularioHijo.Dispose() ' El formulario que recibimos ya no se usa, liberamos memoria
            If TypeOf formularioActivo Is FrmDashboard Then Return
            ' No estamos en el Dashboard → cerramos el actual y abrimos el Dashboard
            formularioHijo = New FrmDashboard()
        End If

        ' 2. CONGELAMOS el panel para que no se vea nada de lo que pasa dentro
        SendMessage(Panel.Handle, WM_SETREDRAW, 0, 0)

        Try
            ' 3. Cerramos el formulario anterior si lo había
            If formularioActivo IsNot Nothing Then
                formularioActivo.Close()
            End If
            formularioActivo = formularioHijo

            ' 4. PREPARAMOS EL HIJO (Él sigue creyendo que se está dibujando, pero Windows no lo muestra)
            formularioActivo.TopLevel = False
            formularioActivo.FormBorderStyle = FormBorderStyle.None
            formularioActivo.Dock = DockStyle.Fill

            Panel.Controls.Clear()
            Panel.Controls.Add(formularioActivo)
            formularioActivo.Show()

            ' 5. FORZAMOS AL HIJO A REORGANIZARSE (Aquí es donde ocurría el fantasma)
            formularioActivo.Refresh()

        Finally
            ' 6. LIBERAMOS el panel y ordenamos que se pinte TODO DE GOLPE
            SendMessage(Panel.Handle, WM_SETREDRAW, 1, 0)
            Panel.Refresh()
        End Try
    End Sub




    Private Sub CargarDatosEmpresa()
        Try
            ' 1. Preparamos la consulta
            ' Seleccionamos el campo Logo de la empresa (asumimos que es la ID 1)
            Dim query As String = "SELECT Logo FROM Empresa WHERE ID = 1"
            Dim conexion = ConexionBD.GetConnection()

            Using cmd As New SQLiteCommand(query, conexion)
                ' 2. Ejecutamos la consulta usando ExecuteScalar
                ' (ExecuteScalar es ideal cuando solo quieres recuperar UN dato, como una imagen)
                Dim resultado = cmd.ExecuteScalar()

                ' 3. Verificamos que no sea nulo (por si la empresa no tiene logo aún)
                If resultado IsNot Nothing AndAlso Not IsDBNull(resultado) Then

                    ' 4. TRUCO DE MAGIA: Convertir Bytes -> Imagen
                    ' Convertimos el resultado genérico a un array de bytes
                    Dim bytesImagen As Byte() = DirectCast(resultado, Byte())

                    ' Creamos un "stream" (flujo de memoria) con esos bytes
                    Using ms As New MemoryStream(bytesImagen)
                        ' La clase Image crea la foto a partir de ese flujo
                        PictureBoxLogoHome.Image = Image.FromStream(ms)
                    End Using

                    ' Ajuste visual opcional: para que la foto no se deforme
                    PictureBoxLogoHome.SizeMode = PictureBoxSizeMode.Zoom
                Else
                    ' Si no hay logo en la BD, puedes limpiar el PictureBox o poner uno por defecto
                    PictureBoxLogoHome.Image = Nothing
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Error al cargar el logo: " & ex.Message)
        End Try

        Try
            Dim query As String = "SELECT NombreFiscal, CIF FROM Empresa WHERE ID = 1"
            Dim conexion = ConexionBD.GetConnection()

            Using cmd As New SQLiteCommand(query, conexion)
                Dim reader = cmd.ExecuteReader
                If reader.Read() Then
                    ' Construimos el saludo: empresa + usuario logueado (con rol entre paréntesis)
                    Dim textoEmpresa As String = reader("NombreFiscal").ToString & " : " & reader("CIF").ToString
                    Dim textoUsuario As String = ""
                    If Not String.IsNullOrEmpty(ComunSesionActual.Usuario) Then
                        Dim rolText As String = If(String.IsNullOrEmpty(ComunSesionActual.Rol), "Usuario", ComunSesionActual.Rol)
                        textoUsuario = $"   |   {ComunSesionActual.Usuario} ({rolText})"
                    End If

                    Dim textoCompleto As String = textoEmpresa & textoUsuario
                    NombreEmpresa.Text = textoCompleto

                    ' === AUTOAJUSTE DE ANCHO ===
                    ' El control 'NombreEmpresa' es un ToolStripTextBox con ancho fijo de 200px en el Designer.
                    ' Como ahora el texto es más largo (empresa + usuario + rol), antes se cortaba por la izquierda.
                    ' Calculamos el ancho que necesita el texto y redimensionamos el control en runtime.
                    Try
                        Using g As Graphics = Me.CreateGraphics()
                            Dim tamañoTexto As SizeF = g.MeasureString(textoCompleto, NombreEmpresa.Font)
                            ' Añadimos un pequeño margen (20 px) y un máximo de 700 px para no comerse media barra
                            Dim anchoNecesario As Integer = Math.Min(700, CInt(tamañoTexto.Width) + 20)
                            ' Solo crecemos, nunca encogemos por debajo del tamaño original (200)
                            If anchoNecesario > NombreEmpresa.Width Then
                                NombreEmpresa.Size = New Size(anchoNecesario, NombreEmpresa.Height)
                            End If
                        End Using
                    Catch
                        ' Si el cálculo falla, dejamos el control como estaba.
                    End Try
                End If
            End Using
        Catch ex As Exception
            LogErrores.Registrar("PagHome.CargarDatosEmpresa", ex)
            MessageBox.Show("No se ha podido conectar a la base de datos o la tabla se encuentra vacía " & ex.Message)
        End Try
    End Sub

    Protected Overrides ReadOnly Property CreateParams As System.Windows.Forms.CreateParams
        Get
            ' Forzamos el uso del tipo específico de Windows Forms
            Dim cp As System.Windows.Forms.CreateParams = MyBase.CreateParams
            ' Aplicamos el estilo extendido para evitar parpadeos (WS_EX_COMPOSITED)
            cp.ExStyle = cp.ExStyle Or &H2000000
            Return cp
        End Get
    End Property


    ' EVENTOS CLIC DE LOS INFORMES

    ' 1. VENTAS
    Private Sub InformeFacturasEmitidas_Click(sender As Object, e As EventArgs)
        ' Vital para el contable. Filtra entre dos fechas y saca Base Imponible, IVA y Total. Se exporta a Excel para pagar impuestos.
        AbrirFormularioEnPanel(New FrmInformeFacturas())
    End Sub

    Private Sub InformeVentasCliente_Click(sender As Object, e As EventArgs)
        ' Abre el Ranking
        AbrirFormulario(New FrmInformeRankingClientes())
    End Sub

    Private Sub InformeVentasVendedor_Click(sender As Object, e As EventArgs)
        ' Suma las ventas de cada comercial para calcular sus comisiones a final de mes.
        AbrirFormulario(New FrmInformeComisionesVendedor())
    End Sub

    ' 2. OPERACIONES
    Private Sub InformePendientesCobro_Click(sender As Object, e As EventArgs)
        ' Las facturas cuyo "Estado" no sea 'Pagado'. Básico para llamar a los clientes morosos.
        'AbrirFormulario(New FrmInformePendientesCobro())
    End Sub

    Private Sub InformePedidosPendientes_Click(sender As Object, e As EventArgs)
        ' Para el mozo de almacén. Qué pedidos nos han hecho y aún no se han convertido en Albarán/Enviado.
        'AbrirFormulario(New FrmInformePedidosPendientes())
    End Sub

    Private Sub InformeRutas_Click(sender As Object, e As EventArgs)
        ' Se imprime por las mañanas para darle al repartidor la lista de direcciones y bultos agrupados por "Ruta".
        'AbrirFormulario(New FrmInformeRutas())
    End Sub

    ' 3. ALMACÉN
    Private Sub InformeInventarioValorado_Click(sender As Object, e As EventArgs)
        ' Multiplica el Stock Actual x Precio de Coste de cada artículo. Obligatorio presentarlo a Hacienda a final de año.
        AbrirFormularioEnPanel(New FrmInformeInventario())
    End Sub

    Private Sub InformeStockMinimo_Click(sender As Object, e As EventArgs)
        ' Muestra solo los artículos cuyo Stock Actual sea menor o igual a 2 (o lo que tú definas). Sirve para saber qué hay que comprar hoy.
        AbrirFormularioEnPanel(New FrmInformeStockMinimo)
    End Sub

    Private Sub InformeArticulosMasVendidos_Click(sender As Object, e As EventArgs)
        ' Estadística pura. Agrupa las líneas de factura por artículo para saber cuáles son los productos estrella.
        AbrirFormularioEnPanel(New FrmInformeTopArticulos)
    End Sub
    ' EVENTOS CLIC DE CONFIGURACIÓN
    Private Sub ConfigDatosEmpresa_Click(sender As Object, e As EventArgs)
        Dim frm As New FrmConfiguracion()
        frm.IrAPestana(0) ' Pestaña Empresa
        frm.ShowDialog()
        CargarDatosEmpresa() ' Refrescamos el logo al cerrar
    End Sub
    Private Sub ConfigUsuarios_Click(sender As Object, e As EventArgs)
        Dim frm As New FrmConfiguracion()
        frm.IrAPestana(1) ' Pestaña Usuarios
        frm.ShowDialog()
    End Sub

    Private Sub ConfigBackups_Click(sender As Object, e As EventArgs)
        Dim frm As New FrmConfiguracion()
        frm.IrAPestana(2) ' Pestaña Copias de Seguridad
        frm.ShowDialog()
    End Sub
    ' MOTOR DE COPIAS DE SEGURIDAD EN SEGUNDO PLANO
    ' Antes este método tenía la lógica de copia inline, con un par de bugs:
    '   - Usaba una ruta de origen "BD\Optima.db" que no existe (el archivo real está en
    '     AppDomain.CurrentDomain.BaseDirectory). Por eso las copias automáticas nunca se hacían.
    '   - Usaba File.Copy mientras la BD estaba abierta, lo que puede dejar el destino corrupto.
    ' Ahora delegamos en GestorBackups, que usa "VACUUM INTO" (la forma correcta y atómica).
    Private Sub RelojBackups_Tick(sender As Object, e As EventArgs) Handles RelojBackups.Tick
        Dim horaActual As String = DateTime.Now.ToString("HH:mm")

        ' Si ya hemos hecho la copia en este minuto exacto, no hacemos nada más
        If ultimoBackupRealizado = DateTime.Now.ToString("yyyyMMdd_HHmm") Then Return

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' Consultamos qué quiere el usuario
            Dim sql As String = "SELECT RutaBackup, AutoBackup, FrecuenciaBackup, HoraBackup, DiasBackup FROM Empresa WHERE ID = 1"
            Using cmd As New SQLiteCommand(sql, c)
                Using r As SQLiteDataReader = cmd.ExecuteReader()
                    If r.Read() Then

                        ' 1. ¿Están activadas las copias automáticas?
                        Dim isAuto As Boolean = If(IsDBNull(r("AutoBackup")), False, Convert.ToBoolean(r("AutoBackup")))
                        If Not isAuto Then Return

                        ' 2. ¿Es la hora correcta?
                        Dim horaConfigurada As String = If(IsDBNull(r("HoraBackup")), "", r("HoraBackup").ToString())
                        If horaActual <> horaConfigurada Then Return

                        ' 3. Si es Semanal, ¿toca hoy?
                        Dim frec As String = If(IsDBNull(r("FrecuenciaBackup")), "Diaria", r("FrecuenciaBackup").ToString())
                        If frec = "Semanal" Then
                            Dim dias As String = If(IsDBNull(r("DiasBackup")), "", r("DiasBackup").ToString())
                            ' .NET usa 0 para Domingo. Nuestro UI usa 0 para Lunes. Lo adaptamos:
                            Dim diaHoy As Integer = CInt(DateTime.Now.DayOfWeek) - 1
                            If diaHoy = -1 Then diaHoy = 6

                            If Not dias.Split(","c).Contains(diaHoy.ToString()) Then Return
                        End If

                        ' ¡BINGO! TOCA HACER COPIA DE SEGURIDAD AHORA MISMO
                        Dim rutaBase As String = If(IsDBNull(r("RutaBackup")), "", r("RutaBackup").ToString().Trim())

                        ' Delegamos en el gestor centralizado: él se encarga de crear la carpeta,
                        ' usar VACUUM INTO y limpiar las copias antiguas.
                        Dim resultado = GestorBackups.HacerCopiaAutomatica(rutaBase)
                        If resultado IsNot Nothing Then
                            ultimoBackupRealizado = DateTime.Now.ToString("yyyyMMdd_HHmm")
                        End If
                    End If
                End Using
            End Using
        Catch ex As Exception
            ' Si falla (porque la BD está ocupada), no asustamos al usuario con un MessageBox,
            ' pero sí dejamos rastro en el log para poder diagnosticar después.
            LogErrores.Registrar("PagHome.RelojBackups_Tick", ex)
        End Try
    End Sub
End Class

' CLASE PARA PERSONALIZAR LOS COLORES DEL MENÚ (RENDERER)
Public Class ColoresMenuModerno
    Inherits ProfessionalColorTable

    ' 1. COLORES BASE 
    Private ReadOnly colorFondo As Color = Color.FromArgb(40, 50, 70)       ' Fondo general
    Private ReadOnly colorSeleccion As Color = Color.FromArgb(55, 65, 85)   ' Hover sutil (Igual que tu TreeView)
    Private ReadOnly colorBorde As Color = Color.FromArgb(20, 30, 50)       ' Borde exterior oscuro
    Private ReadOnly colorSeparador As Color = Color.FromArgb(70, 80, 100)  ' Línea separadora

    ' --- FONDOS ---
    Public Overrides ReadOnly Property MenuStripGradientBegin As Color
        Get
            Return colorFondo
        End Get
    End Property

    Public Overrides ReadOnly Property MenuStripGradientEnd As Color
        Get
            Return colorFondo
        End Get
    End Property

    Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
        Get
            Return colorFondo
        End Get
    End Property

    ' --- HOVER Y SELECCIÓN ---
    Public Overrides ReadOnly Property MenuItemSelected As Color
        Get
            Return colorSeleccion
        End Get
    End Property

    Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
        Get
            Return colorSeleccion
        End Get
    End Property

    Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
        Get
            Return colorSeleccion
        End Get
    End Property

    Public Overrides ReadOnly Property MenuItemPressedGradientBegin As Color
        Get
            Return colorSeleccion
        End Get
    End Property

    Public Overrides ReadOnly Property MenuItemPressedGradientEnd As Color
        Get
            Return colorSeleccion
        End Get
    End Property

    ' --- BORDES Y MÁRGENES ---
    Public Overrides ReadOnly Property MenuItemBorder As Color
        Get
            Return Color.Transparent ' Quita el borde azul de Windows al pasar el ratón
        End Get
    End Property

    Public Overrides ReadOnly Property MenuBorder As Color
        Get
            Return colorBorde ' Borde general de la cajita desplegable
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
        Get
            Return colorFondo
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
        Get
            Return colorFondo
        End Get
    End Property

    Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
        Get
            Return colorFondo
        End Get
    End Property

    ' --- SEPARADOR PLANO ---
    Public Overrides ReadOnly Property SeparatorDark As Color
        Get
            Return colorSeparador
        End Get
    End Property

    Public Overrides ReadOnly Property SeparatorLight As Color
        Get
            Return colorFondo ' Evita el efecto 3D antiguo
        End Get
    End Property
End Class

' RENDERIZADOR PERSONALIZADO PARA UN HOVER SUTIL Y TEXTO CLARO
Public Class RenderizadorMenuSutil
    Inherits ToolStripProfessionalRenderer

    Private ReadOnly colorHoverSutil As Color = Color.FromArgb(50, 60, 80)

    Public Sub New(tablaColores As ProfessionalColorTable)
        MyBase.New(tablaColores)
    End Sub

    Protected Overrides Sub OnRenderMenuItemBackground(e As ToolStripItemRenderEventArgs)
        ' Si es la barra principal y está seleccionada pero no abierta
        If e.Item.IsOnDropDown = False AndAlso e.Item.Selected AndAlso Not e.Item.Pressed Then
            Dim rect As New Rectangle(0, 0, e.Item.Width, e.Item.Height)
            Using pincelSutil As New SolidBrush(colorHoverSutil)
                e.Graphics.FillRectangle(pincelSutil, rect)
            End Using
        Else
            ' Para el desplegable usamos los colores de ColoresMenuModerno
            MyBase.OnRenderMenuItemBackground(e)
        End If
    End Sub

    ' ¡ESTO ES VITAL! Fuerza a que el texto siempre sea blanco humo, incluso al pasar el ratón
    Protected Overrides Sub OnRenderItemText(e As ToolStripItemTextRenderEventArgs)
        e.TextColor = Color.WhiteSmoke
        MyBase.OnRenderItemText(e)
    End Sub
End Class
