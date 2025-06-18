
# SAP Assistant Blazor

Frontend desarrollado con **Blazor WebAssembly** para facilitar la interacción con los servicios backend del asistente inteligente de SAP B1.  
Permite a los usuarios ejecutar consultas SQL, gestionar conexiones a bases de datos y visualizar resultados en dashboards interactivos.

---

## 🚀 Características Principales

- Gestión de conexiones a SAP HANA y SQL Server.  
- Ejecución de consultas SQL seguras (solo `SELECT`).  
- Visualización de resultados en tablas dinámicas.  
- Dashboards interactivos con KPIs y drill-down.  
- Integración prevista de autenticación JWT.  
- Interfaz moderna utilizando MudBlazor.  

---

## 📦 Requisitos

- .NET 6 o superior  
- Blazor WebAssembly  
- MudBlazor (librería de componentes UI)  
- API REST del backend (FastAPI)  

---

## 📁 Estructura del Proyecto

```plaintext
sap_assistant_blazor/
├── Pages/                   # Páginas principales (Login, Dashboard, Assistant)
├── Shared/                  # Componentes compartidos (Layout, NavMenu)
├── Services/                # Llamadas a la API REST
├── wwwroot/                 # Archivos estáticos (CSS, JS)
└── Program.cs / Startup.cs  # Configuración de la aplicación
```

---

## 📚 Funcionalidades

### Gestión de Conexiones

- Alta, edición y eliminación de conexiones.
- Selección de conexión activa para la ejecución de consultas.
- Validación de conexiones (pendiente de implementación).

### Ejecución de Consultas SQL

- Entrada de consultas en lenguaje natural.
- Visualización de resultados en tablas con filtros y ordenamiento.
- Exportación de resultados (pendiente).

### Dashboards Interactivos

**Objetivo:**  
Permitir a los usuarios visualizar información clave a través de KPIs y gráficos interactivos, facilitando la toma de decisiones.

**Funcionalidades:**

- Visualización de KPIs Estándar:
  - Productos más vendidos.  
  - Clientes con mayor facturación.  
  - Pedidos pendientes por estado.  

- Tipos de Gráficos:
  - Barras, Pastel, Líneas (MudBlazor Charts o Chart.js).  

- Drill-Down Completo:
  - Profundización en cada KPI hasta llegar al dato fuente.  

- Visualización de la Consulta SQL:
  - Mostrar la consulta SQL generada por cada KPI.  

**Estado Actual:**

- Página de dashboards creada, pendiente de integración completa con los endpoints de backend.

**Pendiente:**

- Implementar la visualización de los primeros 3 KPIs básicos.  
- Habilitar la navegación interactiva (drill-down).  
- Permitir al usuario seleccionar los KPIs desde un catálogo.  

---

## 🔐 Seguridad

- Se prevé la integración de autenticación JWT.
- Actualmente, la aplicación no cuenta con control de sesiones seguro (uso limitado a entornos de pruebas).
- Existe la página `/demo` para acceder a una versión de prueba del asistente. Desde allí se puede abrir la interfaz `/demo/chat`, similar a ChatGPT, para realizar consultas. Debe utilizarse únicamente en entornos de pruebas y no implementa control de sesiones seguro.

---

## 📅 Estado Actual del Proyecto

**Implementado:**

- Interfaz básica para la gestión de conexiones.  
- Ejecución de consultas con visualización de resultados.  
- Estructura base de dashboards.  

**Pendiente:**

- Integrar autenticación y autorización de usuarios.  
- Mejorar la experiencia de usuario (UI/UX).  
- Implementar dashboards con visualización de KPIs reales.  
- Implementar logs y manejo avanzado de errores.  

---

## ✅ Próximas Mejoras

- Middleware de autenticación con JWT.  
- Endpoint `/test_connection` para validación de credenciales.  
- Integrar exportación de resultados a Excel o CSV.  
- Implementar selección de KPIs desde un catálogo predefinido.  
- Agregar soporte multilenguaje.  

---

## 🧠 Licencia

Uso interno / proyecto privado. Adaptable a entornos empresariales.
