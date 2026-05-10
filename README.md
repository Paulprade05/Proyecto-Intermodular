# 🚀 OPTIMA - Sistema de Gestión Empresarial (ERP)

### *Trabajo de Fin de Grado (TFG) - Desarrollo de Aplicaciones Multiplataforma*

**Desarrollado por:** Paul Andrei  
**Entorno de Desarrollo:** Visual Studio 2022  
**Lenguaje:** Visual Basic .NET (`VB.NET`)  
**Tecnología de Interfaz:** Windows Forms (`WinForms`)  
**Base de Datos:** SQLite

---

## 📝 Descripción del Proyecto

OPTIMA es una solución ERP integral diseñada para centralizar y optimizar los flujos de trabajo operativos de una pyme. Este proyecto cubre el ciclo completo de negocio, desde la gestión de inventario y la relación con terceros, hasta el proceso completo de ventas y facturación.

El software destaca por su **trazabilidad documental**, permitiendo la conversión inteligente de documentos (Presupuesto ➔ Pedido ➔ Albarán ➔ Factura) y un motor de informes personalizado.

---

## 🖼️ Interfaz del Proyecto (Capturas de Pantalla)

### 🔐 Acceso y Panel Principal
> Diseño centrado en la usabilidad y carga optimizada sin parpadeos.

| Pantalla de Acceso (Login) | Panel de Control (Dashboard) |
| :---: | :---: |
| <img src="https://github.com/user-attachments/assets/77fb8e18-4aaf-4387-a041-4889babe8098" width="300"/> | <img src="https://github.com/user-attachments/assets/f58d9b6c-bc9d-4f72-ad34-cd482ba8b49e" width="500"/> |

---

### 💰 Módulo de Ventas
Contempla el flujo completo desde la oferta comercial hasta la emisión de la factura legal.

<details>
<summary>📸 Ver capturas del Ciclo de Ventas</summary>

#### 1. Presupuesto
<img src="https://github.com/user-attachments/assets/fc079f8d-c203-4218-a03a-4ea4c9bcde26" width="100%" />

#### 2. Pedido
<img src="https://github.com/user-attachments/assets/8c16d0db-6ddd-4cbc-aaa0-7521fe922770" width="100%" />

#### 3. Albarán
<img src="https://github.com/user-attachments/assets/83ade9db-ea03-413b-8d2d-9773552ffa47" width="100%" />

#### 4. Factura
<img src="https://github.com/user-attachments/assets/5f9a9cfc-d09b-4a49-8291-5fe1e0c28f02" width="100%" />

</details>

---

### 👥 Módulo de Terceros
Gestión detallada de las entidades con las que interactúa la empresa.

<details>
<summary>📸 Ver capturas de Clientes y Proveedores</summary>

#### Ficha de Cliente
<img src="https://github.com/user-attachments/assets/ca124c82-c7ef-40ec-8140-06868c25fe37" width="100%" />

#### Ficha de Proveedor
<img src="https://github.com/user-attachments/assets/cea24bfd-a9e2-449c-bc52-9529e95e83bd" width="100%" />

</details>

---

### 📦 Módulo de Almacén
Control de inventario físico, clasificación por familias y registro de movimientos de stock.

<details>
<summary>📸 Ver capturas de Inventario</summary>

#### Artículos
<img src="https://github.com/user-attachments/assets/d8865513-5720-40df-8a95-755c7c27e934" width="100%" />

#### Gestión de Familias
<img src="https://github.com/user-attachments/assets/15347745-97e9-4237-ad3d-c237bf61c712" width="400" />

#### Movimientos de Almacén
<img src="https://github.com/user-attachments/assets/14d8d8a3-f479-4f4f-91a5-ed0aed6b50ea" width="100%" />

</details>

---

### ⚙️ Módulo de Tablas (Configuración)
Parametrización global de los elementos maestros del sistema.

<details>
<summary>📸 Ver capturas de Tablas Maestras</summary>

| Vendedor | Formas de Pago |
| :---: | :---: |
| <img src="https://github.com/user-attachments/assets/84f12cad-6975-4ade-86c1-d7126d08c197" width="350"/> | <img src="https://github.com/user-attachments/assets/ded243bc-6062-4a7e-89fc-c8b6b685bbd4" width="350"/> |

| Rutas Logísticas | Agencias de Transporte |
| :---: | :---: |
| <img src="https://github.com/user-attachments/assets/d5603a33-7057-412c-b674-de400733cbbf" width="350"/> | <img src="https://github.com/user-attachments/assets/837d5c34-9ac9-46cb-b683-effd753c9d52" width="350"/> |

</details>

---

## 🛠️ Hitos Técnicos

* **Arquitectura de Impresión:** Motor propio basado en `GDI+` para generar informes PDF profesionales con logotipos dinámicos.
* **Optimización de UI:** Uso de la API nativa de Windows (`SendMessage` / `WM_SETREDRAW`) para garantizar una carga de formularios suave y sin parpadeos (*flickering*).
* **Base de Datos Portable:** Implementación de SQLite para asegurar la portabilidad total del sistema sin dependencias de servidor pesadas.

---

*Este proyecto es propiedad intelectual de Paul Andrei y ha sido desarrollado como Trabajo de Fin de Grado.*
