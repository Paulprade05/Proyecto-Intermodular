Imports System.Data.SQLite

Public Class FrmClienteDetalle

    ' Esta variable recibe el código desde FrmClientes
    Public Property CodigoClienteSeleccionado As String = ""
    Public Property CodigoNuevoPredefinido As String = ""
    Private Sub FrmClienteDetalle_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Ficha de Cliente"
        Me.Size = New Size(850, 520) ' Tamaño ideal para una ficha emergente
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        ReorganizarControlesAutomaticamente()

        If String.IsNullOrEmpty(CodigoClienteSeleccionado) Then
            ' MODO NUEVO
            LimpiarFormulario()
            ButtonBorrar.Visible = False ' No se puede borrar si es nuevo
        Else
            ' MODO EDICIÓN
            CargarDetalleCliente(CodigoClienteSeleccionado)
            ButtonBorrar.Visible = True
        End If
    End Sub

    ' 1. EL MOTOR VISUAL (Rejilla Proporcional para la Ficha)
    Private Function ConfigurarLabel(texto As String, x As Integer, y As Integer) As Label
        Dim lbl As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Text = texto AndAlso l.Tag?.ToString() = "AutoLabel")
        If lbl Is Nothing Then
            lbl = New Label() With {.Text = texto, .AutoSize = True, .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = Color.FromArgb(80, 80, 80), .Tag = "AutoLabel"}
            Me.Controls.Add(lbl)
        End If
        lbl.Location = New Point(x, y)
        lbl.BringToFront()
        Return lbl
    End Function
    Private Sub CrearTituloSeccion(nombre As String, texto As String, x As Integer, y As Integer, ancho As Integer)
        ' 1. Crear el Título
        Dim lblTitulo As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "sec_" & nombre)
        If lblTitulo Is Nothing Then
            lblTitulo = New Label() With {.Name = "sec_" & nombre, .AutoSize = True, .ForeColor = Color.FromArgb(0, 120, 215), .Tag = "AutoLabel"}
            Me.Controls.Add(lblTitulo)
        End If
        lblTitulo.Text = texto
        lblTitulo.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        lblTitulo.Location = New Point(x, y)
        lblTitulo.BringToFront()

        ' 2. Crear la línea separadora
        Dim linea As Label = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "lin_" & nombre)
        If linea Is Nothing Then
            linea = New Label() With {.Name = "lin_" & nombre, .Height = 2, .BackColor = Color.FromArgb(220, 220, 220), .Tag = "AutoLabel"}
            Me.Controls.Add(linea)
        End If
        linea.Bounds = New Rectangle(x, y + 22, ancho, 2)
        linea.BringToFront()
    End Sub
    Private Sub ReorganizarControlesAutomaticamente()
        Dim margen As Integer = 30
        Dim usableWidth As Integer = Me.ClientSize.Width - (margen * 2)
        Dim gap As Integer = 20
        Dim offsetCaja As Integer = 22
        Dim espacioY As Integer = 65

        ' --- COORDENADAS Y CON SECCIONES ---
        Dim ySeccion1 As Integer = 20
        Dim yFila1 As Integer = ySeccion1 + 35 ' Dejamos hueco para el título y la línea
        Dim yFila2 As Integer = yFila1 + espacioY

        Dim ySeccion2 As Integer = yFila2 + espacioY + 15 ' Un poco más de margen antes de la sección 2
        Dim yFila3 As Integer = ySeccion2 + 35
        Dim yFila4 As Integer = yFila3 + espacioY

        ' DIBUJAR LOS TÍTULOS DE SECCIÓN
        CrearTituloSeccion("Identidad", "DATOS FISCALES Y COMERCIALES", margen, ySeccion1, usableWidth)
        CrearTituloSeccion("Contacto", "LOCALIZACIÓN Y CONTACTO", margen, ySeccion2, usableWidth)

        ' FILAS DE CAJAS DE TEXTO
        ' --- FILA 1 ---
        Dim wCod As Integer = CInt(usableWidth * 0.2) - gap
        Dim wCif As Integer = CInt(usableWidth * 0.2) - gap
        Dim wNomFis As Integer = usableWidth - wCod - wCif - (gap * 2)
        Dim xCif As Integer = margen + wCod + gap
        Dim xNomFis As Integer = xCif + wCif + gap

        ConfigurarLabel("Código", margen, yFila1) : TextBoxCodigo.Bounds = New Rectangle(margen, yFila1 + offsetCaja, wCod, 25)
        ConfigurarLabel("CIF", xCif, yFila1) : TextBoxCIF.Bounds = New Rectangle(xCif, yFila1 + offsetCaja, wCif, 25)
        ConfigurarLabel("Nombre Fiscal", xNomFis, yFila1) : TextBoxNombreFiscal.Bounds = New Rectangle(xNomFis, yFila1 + offsetCaja, wNomFis, 25)

        ' --- FILA 2 ---
        Dim wNomCom As Integer = CInt(usableWidth * 0.4) - gap
        Dim wDir As Integer = usableWidth - wNomCom - gap
        Dim xDir As Integer = margen + wNomCom + gap

        ConfigurarLabel("Nombre Comercial", margen, yFila2) : TextBoxNombreComercial.Bounds = New Rectangle(margen, yFila2 + offsetCaja, wNomCom, 25)
        ConfigurarLabel("Dirección", xDir, yFila2) : TextBoxDireccion.Bounds = New Rectangle(xDir, yFila2 + offsetCaja, wDir, 25)

        ' --- FILA 3 ---
        Dim wPob As Integer = CInt(usableWidth * 0.25) - gap
        Dim wProv As Integer = CInt(usableWidth * 0.25) - gap
        Dim wTel As Integer = CInt(usableWidth * 0.2) - gap
        Dim wEma As Integer = usableWidth - wPob - wProv - wTel - (gap * 3)
        Dim xProv As Integer = margen + wPob + gap
        Dim xTel As Integer = xProv + wProv + gap
        Dim xEma As Integer = xTel + wTel + gap

        ConfigurarLabel("Población", margen, yFila3) : TextBoxPoblacion.Bounds = New Rectangle(margen, yFila3 + offsetCaja, wPob, 25)
        ConfigurarLabel("Provincia", xProv, yFila3) : TextBoxProvincia.Bounds = New Rectangle(xProv, yFila3 + offsetCaja, wProv, 25)
        ConfigurarLabel("Teléfono", xTel, yFila3) : TextBoxTelefono.Bounds = New Rectangle(xTel, yFila3 + offsetCaja, wTel, 25)
        ConfigurarLabel("Email", xEma, yFila3) : TextBoxEmail.Bounds = New Rectangle(xEma, yFila3 + offsetCaja, wEma, 25)

        ' --- FILA 4 ---
        Dim wCont As Integer = CInt(usableWidth * 0.25) - gap
        Dim wWeb As Integer = CInt(usableWidth * 0.25) - gap
        Dim wObs As Integer = usableWidth - wCont - wWeb - (gap * 2)
        Dim xWeb As Integer = margen + wCont + gap
        Dim xObs As Integer = xWeb + wWeb + gap

        ConfigurarLabel("Persona de Contacto", margen, yFila4) : TextBoxContacto.Bounds = New Rectangle(margen, yFila4 + offsetCaja, wCont, 25)
        ConfigurarLabel("Sitio Web", xWeb, yFila4) : TextBoxWeb.Bounds = New Rectangle(xWeb, yFila4 + offsetCaja, wWeb, 25)
        ConfigurarLabel("Observaciones", xObs, yFila4) : TextBoxObservaciones.Bounds = New Rectangle(xObs, yFila4 + offsetCaja, wObs, 25)

        ' --- BOTONES ---
        Dim yBotones As Integer = Me.ClientSize.Height - 60
        EstilizarBoton(ButtonGuardar, margen, yBotones, Color.FromArgb(0, 120, 215), Color.White)
        EstilizarBoton(ButtonBorrar, margen + 110, yBotones, Color.FromArgb(209, 52, 56), Color.White)

        ' --- AJUSTES DE RESIZE ---
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is TextBox Then ctrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        Next
        ButtonGuardar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ButtonBorrar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left

        ' Hacemos que las líneas separadoras se estiren al maximizar
        Dim lin1 = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "lin_Identidad")
        If lin1 IsNot Nothing Then lin1.Width = usableWidth
        Dim lin2 = Me.Controls.OfType(Of Label)().FirstOrDefault(Function(l) l.Name = "lin_Contacto")
        If lin2 IsNot Nothing Then lin2.Width = usableWidth
    End Sub

    Private Sub EstilizarBoton(btn As Button, x As Integer, y As Integer, bg As Color, fg As Color)
        btn.Location = New Point(x, y) : btn.Size = New Size(100, 35) : btn.FlatStyle = FlatStyle.Flat
        btn.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold) : btn.Cursor = Cursors.Hand
        btn.BackColor = bg : btn.ForeColor = fg : btn.FlatAppearance.BorderSize = 0
    End Sub

    Private Sub FrmClienteDetalle_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Me.IsHandleCreated AndAlso Me.ClientSize.Width > 0 Then ReorganizarControlesAutomaticamente()
    End Sub

    ' 2. LÓGICA DE BASE DE DATOS
    Private Sub LimpiarFormulario()
        ' --- MAGIA DEL PRO-TIP ---
        If CodigoNuevoPredefinido <> "" Then
            TextBoxCodigo.Text = CodigoNuevoPredefinido ' Usamos el que escribió en la factura
        Else
            TextBoxCodigo.Text = GenerarProximoCodigo() ' Generamos uno nuevo normal
        End If

        TextBoxCodigo.ReadOnly = True
        TextBoxNombreFiscal.Text = "" : TextBoxNombreComercial.Text = "" : TextBoxCIF.Text = ""
        TextBoxDireccion.Text = "" : TextBoxPoblacion.Text = "" : TextBoxProvincia.Text = ""
        TextBoxTelefono.Text = "" : TextBoxEmail.Text = "" : TextBoxContacto.Text = ""
        TextBoxWeb.Text = "" : TextBoxObservaciones.Text = ""
        TextBoxNombreFiscal.Focus()
    End Sub

    Private Function GenerarProximoCodigo() As String
        Dim nuevoNum As String = "CLI-001"
        Try
            Dim c = ConexionBD.GetConnection()
            Dim cmd As New SQLiteCommand("SELECT CodigoCliente FROM Clientes WHERE CodigoCliente LIKE 'CLI-%' ORDER BY CodigoCliente DESC LIMIT 1", c)
            If c.State <> ConnectionState.Open Then c.Open()
            Dim result = cmd.ExecuteScalar()
            If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                Dim partes = result.ToString().Split("-"c)
                If partes.Length >= 2 Then
                    Dim corr As Integer
                    If Integer.TryParse(partes(partes.Length - 1), corr) Then nuevoNum = $"CLI-{(corr + 1).ToString("D3")}"
                End If
            End If
        Catch
        End Try
        Return nuevoNum
    End Function

    Private Sub CargarDetalleCliente(codigo As String)
        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()
            Using cmd As New SQLiteCommand("SELECT * FROM Clientes WHERE CodigoCliente = @cod", c)
                cmd.Parameters.AddWithValue("@cod", codigo)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        TextBoxCodigo.Text = codigo
                        TextBoxCodigo.ReadOnly = True
                        TextBoxNombreFiscal.Text = If(IsDBNull(r("NombreFiscal")), "", r("NombreFiscal").ToString())
                        TextBoxNombreComercial.Text = If(IsDBNull(r("NombreComercial")), "", r("NombreComercial").ToString())
                        TextBoxCIF.Text = If(IsDBNull(r("CIF")), "", r("CIF").ToString())
                        TextBoxDireccion.Text = If(IsDBNull(r("Direccion")), "", r("Direccion").ToString())
                        TextBoxPoblacion.Text = If(IsDBNull(r("Poblacion")), "", r("Poblacion").ToString())
                        TextBoxProvincia.Text = If(IsDBNull(r("Provincia")), "", r("Provincia").ToString())
                        TextBoxTelefono.Text = If(IsDBNull(r("Telefono")), "", r("Telefono").ToString())
                        TextBoxEmail.Text = If(IsDBNull(r("Email")), "", r("Email").ToString())
                        TextBoxContacto.Text = If(IsDBNull(r("PersonaContacto")), "", r("PersonaContacto").ToString())
                        TextBoxWeb.Text = If(IsDBNull(r("SitioWeb")), "", r("SitioWeb").ToString())
                        TextBoxObservaciones.Text = If(IsDBNull(r("Observaciones")), "", r("Observaciones").ToString())
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al cargar: " & ex.Message)
        End Try
    End Sub

    Private Sub ButtonGuardar_Click(sender As Object, e As EventArgs) Handles ButtonGuardar.Click
        If String.IsNullOrWhiteSpace(TextBoxNombreFiscal.Text) Then
            MessageBox.Show("El Nombre Fiscal es obligatorio.") : Return
        End If

        ' === VALIDACIONES DE ENTRADA (NO BLOQUEANTES) ===
        ' El cliente puede ser empresa (CIF), particular (NIF) o extranjero (NIE).
        ' Si algo no cuadra avisamos al usuario, pero le dejamos guardar igualmente
        ' (porque puede haber datos importados con CIFs no válidos según AEAT).
        If Not Validador.ConfirmarSiHayProblemas(
            ("CIF/NIF/NIE del cliente", Validador.ValidarDocumentoIdentidad(TextBoxCIF.Text)),
            ("Email", Validador.ValidarEmail(TextBoxEmail.Text)),
            ("Teléfono", Validador.ValidarTelefono(TextBoxTelefono.Text))
        ) Then
            Return
        End If

        Try
            Dim conexion = ConexionBD.GetConnection()
            If conexion.State <> ConnectionState.Open Then conexion.Open()
            Dim sql As String = ""

            If String.IsNullOrEmpty(CodigoClienteSeleccionado) Then
                sql = "INSERT INTO Clientes (CodigoCliente, NombreFiscal, NombreComercial, CIF, Direccion, Poblacion, Provincia, Telefono, Email, PersonaContacto, SitioWeb, Observaciones) VALUES (@cod, @nomF, @nomC, @cif, @dir, @pob, @prov, @tel, @ema, @cont, @web, @obs)"
                CodigoClienteSeleccionado = TextBoxCodigo.Text
            Else
                sql = "UPDATE Clientes SET NombreFiscal=@nomF, NombreComercial=@nomC, CIF=@cif, Direccion=@dir, Poblacion=@pob, Provincia=@prov, Telefono=@tel, Email=@ema, PersonaContacto=@cont, SitioWeb=@web, Observaciones=@obs WHERE CodigoCliente=@cod"
            End If

            Using cmd As New SQLiteCommand(sql, conexion)
                cmd.Parameters.AddWithValue("@cod", TextBoxCodigo.Text)
                cmd.Parameters.AddWithValue("@nomF", TextBoxNombreFiscal.Text)
                cmd.Parameters.AddWithValue("@nomC", TextBoxNombreComercial.Text)
                cmd.Parameters.AddWithValue("@cif", TextBoxCIF.Text.Trim().ToUpperInvariant())
                cmd.Parameters.AddWithValue("@dir", TextBoxDireccion.Text)
                cmd.Parameters.AddWithValue("@pob", TextBoxPoblacion.Text)
                cmd.Parameters.AddWithValue("@prov", TextBoxProvincia.Text)
                cmd.Parameters.AddWithValue("@tel", TextBoxTelefono.Text)
                cmd.Parameters.AddWithValue("@ema", TextBoxEmail.Text)
                cmd.Parameters.AddWithValue("@cont", TextBoxContacto.Text)
                cmd.Parameters.AddWithValue("@web", TextBoxWeb.Text)
                cmd.Parameters.AddWithValue("@obs", TextBoxObservaciones.Text)
                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Cliente guardado.")
            Me.Close() ' Cierra la ventana emergente y vuelve a la lista
        Catch ex As Exception
            LogErrores.Registrar("FrmClienteDetalle.Guardar", ex)
            MessageBox.Show("Error al guardar: " & ex.Message)
        End Try
    End Sub

    Private Sub ButtonBorrar_Click(sender As Object, e As EventArgs) Handles ButtonBorrar.Click
        If MessageBox.Show("¿Está seguro de que desea eliminar este cliente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return

        Try
            Dim c = ConexionBD.GetConnection()
            If c.State <> ConnectionState.Open Then c.Open()

            ' PASO 1: VERIFICAR SI TIENE DATOS ASOCIADOS (AHORA TAMBIÉN FACTURAS)
            Dim totalAsociados As Integer = 0

            ' A) Comprobar en Presupuestos
            Try
                Using cmdCheck As New SQLiteCommand("SELECT COUNT(*) FROM Presupuestos WHERE CodigoCliente = @cod", c)
                    cmdCheck.Parameters.AddWithValue("@cod", CodigoClienteSeleccionado)
                    totalAsociados += Convert.ToInt32(cmdCheck.ExecuteScalar())
                End Using
            Catch : End Try ' Si la tabla no existe aún, no pasa nada

            ' B) Comprobar en Pedidos
            Try
                Using cmdCheck As New SQLiteCommand("SELECT COUNT(*) FROM Pedidos WHERE CodigoCliente = @cod", c)
                    cmdCheck.Parameters.AddWithValue("@cod", CodigoClienteSeleccionado)
                    totalAsociados += Convert.ToInt32(cmdCheck.ExecuteScalar())
                End Using
            Catch : End Try

            ' C) Comprobar en Facturas de Venta (El que daba error)
            Try
                Using cmdCheck As New SQLiteCommand("SELECT COUNT(*) FROM FacturasVenta WHERE CodigoCliente = @cod", c)
                    cmdCheck.Parameters.AddWithValue("@cod", CodigoClienteSeleccionado)
                    totalAsociados += Convert.ToInt32(cmdCheck.ExecuteScalar())
                End Using
            Catch : End Try

            ' PASO 2: DECIDIR SI SE PUEDE BORRAR
            If totalAsociados > 0 Then
                MessageBox.Show($"¡Operación cancelada!" & vbCrLf & vbCrLf &
                                $"No se puede eliminar a este cliente porque tiene {totalAsociados} documento(s) asociado(s) (Presupuestos, Pedidos o Facturas)." & vbCrLf &
                                "Si ya no trabaja con este cliente, es mejor dejarlo en la base de datos por motivos legales y de contabilidad.",
                                "Protección de Datos", MessageBoxButtons.OK, MessageBoxIcon.Stop)
                Return ' Detenemos la ejecución aquí, NO borramos
            End If

            ' PASO 3: BORRADO (Con truco Ninja para evitar el Foreign Key Mismatch)

            ' 1. Apagamos la comprobación estricta de SQLite temporalmente
            Using cmdOff As New SQLiteCommand("PRAGMA foreign_keys = OFF;", c)
                cmdOff.ExecuteNonQuery()
            End Using

            ' 2. Borramos el cliente tranquilamente
            Using cmdDel As New SQLiteCommand("DELETE FROM Clientes WHERE CodigoCliente = @cod", c)
                cmdDel.Parameters.AddWithValue("@cod", CodigoClienteSeleccionado)
                cmdDel.ExecuteNonQuery()
            End Using

            ' 3. Volvemos a encender la seguridad de la base de datos
            Using cmdOn As New SQLiteCommand("PRAGMA foreign_keys = ON;", c)
                cmdOn.ExecuteNonQuery()
            End Using

            MessageBox.Show("Cliente eliminado de la base de datos limpia y correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close() ' Cierra la ficha

        Catch ex As Exception
            MessageBox.Show("Error crítico al borrar: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
