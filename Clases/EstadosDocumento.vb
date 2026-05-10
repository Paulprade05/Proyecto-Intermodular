' constantes con los estados que puede tener cada documento
' antes los tenia escritos a mano en cada formulario y un dia escribi "Convertdo"
' sin la i en un sitio y ya no me cuadraban los estados. me tiré una tarde buscando
' el bug. con constantes el compilador me avisa si me equivoco
'
' tambien vale para si algun dia quiero cambiar "Convertido" por otra palabra,
' lo cambio aqui y ya
Public Module EstadosDocumento

    ' presupuestos
    Public Const PRESUPUESTO_PENDIENTE As String = "Pendiente"
    Public Const PRESUPUESTO_ACEPTADO As String = "Aceptado"
    Public Const PRESUPUESTO_RECHAZADO As String = "Rechazado"
    Public Const PRESUPUESTO_CONVERTIDO As String = "Convertido" ' cuando ya hice un pedido a partir de el

    ' pedidos de venta
    Public Const PEDIDO_PENDIENTE As String = "Pendiente"
    Public Const PEDIDO_SERVIDO As String = "Servido"   ' cuando ya hay un albaran
    Public Const PEDIDO_CANCELADO As String = "Cancelado"

    ' albaranes de venta
    Public Const ALBARAN_PENDIENTE As String = "Pendiente"
    Public Const ALBARAN_ENVIADO As String = "Enviado"
    Public Const ALBARAN_ENTREGADO As String = "Entregado"
    Public Const ALBARAN_FACTURADO As String = "Facturado"   ' cuando ya hay factura

    ' facturas de venta
    Public Const FACTURA_PENDIENTE As String = "Pendiente"
    Public Const FACTURA_COBRADA As String = "Cobrada"
    Public Const FACTURA_VENCIDA As String = "Vencida"
    Public Const FACTURA_CANCELADA As String = "Cancelada"

    ' pedidos de compra (cuando soy yo el que compra)
    Public Const PEDIDO_COMPRA_PENDIENTE As String = "Pendiente"
    Public Const PEDIDO_COMPRA_ENVIADO As String = "Enviado al Proveedor"
    Public Const PEDIDO_COMPRA_PARCIAL As String = "Recibido Parcial"
    Public Const PEDIDO_COMPRA_RECIBIDO As String = "Recibido"

    ' albaranes de compra
    Public Const ALBARAN_COMPRA_PENDIENTE As String = "Pendiente"
    Public Const ALBARAN_COMPRA_RECIBIDO As String = "Recibido"
    Public Const ALBARAN_COMPRA_INCIDENCIA As String = "Recibido con incidencia"
    Public Const ALBARAN_COMPRA_FACTURADO As String = "Facturado"

    ' facturas de compra
    Public Const FACTURA_COMPRA_PENDIENTE As String = "Pendiente"
    Public Const FACTURA_COMPRA_PAGADA As String = "Pagada"
    Public Const FACTURA_COMPRA_VENCIDA As String = "Vencida"

    ' movimientos del almacen (entrar/salir cosas del stock)
    Public Const MOV_ENTRADA As String = "ENTRADA"
    Public Const MOV_SALIDA As String = "SALIDA"
    Public Const MOV_AJUSTE_MAS As String = "AJUSTE+"
    Public Const MOV_AJUSTE_MENOS As String = "AJUSTE-"

End Module
