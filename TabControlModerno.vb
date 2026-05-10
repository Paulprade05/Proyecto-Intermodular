Imports System.Drawing
Imports System.Windows.Forms

' control de pestañas hecho a mano porque el de toda la vida me dejaba bordes
' blancos horribles y con el modo oscuro quedaba cutre. lo pinto yo todo
Public Class TabControlModerno
    Inherits TabControl

    Public Sub New()
        ' con UserPaint le digo que pinto yo, sino windows me mete cosas raras
        Me.SetStyle(ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.ItemSize = New Size(120, 30)
        Me.SizeMode = TabSizeMode.Fixed
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics

        ' primero pinto todo el fondo del mismo color que el form
        ' asi el hueco que queda al lado de las pestañas no canta
        Dim colorFondoGeneral As Color = Color.FromArgb(45, 45, 48)
        g.Clear(colorFondoGeneral)

        ' ahora recorro las pestañas una a una y las pinto
        For i As Integer = 0 To Me.TabCount - 1
            Dim tabRect As Rectangle = Me.GetTabRect(i)
            Dim isSelected As Boolean = (Me.SelectedIndex = i)
            Dim texto As String = Me.TabPages(i).Text

            ' segun esté seleccionada o no le pongo un color u otro
            Dim colorFondoPestaña As Color
            Dim colorTexto As Color

            If isSelected Then
                colorFondoPestaña = Color.FromArgb(70, 75, 80)  ' la activa, mas clarita
                colorTexto = Color.White
            Else
                colorFondoPestaña = colorFondoGeneral ' las que no, se funden con el fondo
                colorTexto = Color.Gray
            End If

            ' relleno el rectangulo
            Using b As New SolidBrush(colorFondoPestaña)
                g.FillRectangle(b, tabRect)
            End Using

            ' y encima el texto centrado. uso TextRenderer que va mejor que DrawString
            TextRenderer.DrawText(g, texto, Me.Font, tabRect, colorTexto, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        Next
    End Sub
End Class