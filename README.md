# Tutorial de Instalación - Backend MVC Products

Este tutorial te guiará paso a paso para instalar y configurar el backend de la aplicación MVC Products con Entity Framework Core y SQLite.

## 📋 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

- **.NET 8.0 SDK** o superior
- **Visual Studio 2022** o **Visual Studio Code**
- **Git** (opcional, para clonar el repositorio)

### Verificar Instalación de .NET

Abre una terminal (PowerShell, CMD, o Terminal) y ejecuta:

```bash
dotnet --version
```

Deberías ver algo como: `8.0.x` o superior.

## 🚀 Instalación Paso a Paso

### Paso 1: Clonar o Descargar el Proyecto

Si tienes el proyecto en un repositorio Git:

```bash
git clone <url-del-repositorio>
cd mvcproducts
```

Si tienes el proyecto localmente, navega a la carpeta del proyecto:

```bash
cd C:\Users\camil\source\repos\mvcproducts\mvcproducts
```

### Paso 2: Restaurar Dependencias

Ejecuta el siguiente comando para descargar todas las dependencias NuGet:

```bash
dotnet restore
```

### Paso 3: Configurar la Base de Datos

#### 3.1 Crear la Migración Inicial

```bash
dotnet ef migrations add InitialCreate
```

#### 3.2 Aplicar las Migraciones a la Base de Datos

```bash
dotnet ef database update
```

Esto creará la base de datos SQLite (`products.db`) con todas las tablas necesarias.

#### 3.3 Datos Mock Automáticos

La aplicación incluye un sistema de seeding automático que carga datos de prueba al iniciar. Los datos mock incluyen:

- **10 Productos** variados (laptops, smartphones, tablets, etc.)
- **10 Usuarios** con información realista
- **15 Pedidos** con productos aleatorios y fechas distribuidas
- **Stock actualizado** según las ventas realizadas

### Paso 4: Verificar la Configuración

Revisa que el archivo `appsettings.json` tenga la configuración correcta:

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=products.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Paso 5: Ejecutar la Aplicación

```bash
dotnet run
```

La aplicación se ejecutará en:
- **HTTPS**: `https://localhost:7xxx`
- **HTTP**: `http://localhost:5xxx`

Los puertos específicos se mostrarán en la consola.

### Paso 6: Verificar que Funciona

Abre tu navegador y ve a:
- **Swagger UI**: `https://localhost:7xxx/swagger`
- **API Products**: `https://localhost:7xxx/api/Products`
- **API Usuarios**: `https://localhost:7xxx/api/Usuarios`
- **API Pedidos**: `https://localhost:7xxx/api/Pedidos`

## 🛠️ Comandos Útiles Durante el Desarrollo

### Restaurar Dependencias
```bash
dotnet restore
```

### Compilar el Proyecto
```bash
dotnet build
```

### Ejecutar en Modo Desarrollo
```bash
dotnet run
```

### Ejecutar con Hot Reload
```bash
dotnet watch run
```

### Crear Nueva Migración
```bash
dotnet ef migrations add NombreDeLaMigracion
```

### Aplicar Migraciones
```bash
dotnet ef database update
```

### Revertir Migración
```bash
dotnet ef database update NombreMigracionAnterior
```

### Eliminar Base de Datos
```bash
dotnet ef database drop
```

## 📊 Estructura de la Base de Datos

Después de ejecutar las migraciones, tendrás estas tablas con datos mock incluidos:

### **Datos Mock Incluidos:**

- **10 Productos** con información completa (nombres, descripciones, precios, stock)
- **10 Usuarios** con nombres, emails y teléfonos realistas
- **15 Pedidos** con productos aleatorios y fechas distribuidas en los últimos 60 días
- **Stock actualizado** automáticamente según las ventas realizadas

### **Products**
- `Id` (int, PK)
- `Name` (string, required)
- `Description` (string, nullable)
- `Price` (decimal)
- `Stock` (int)
- `Category` (string, nullable)
- `CreatedDate` (datetime)
- `UpdatedDate` (datetime, nullable)

### **Usuarios**
- `Id` (int, PK)
- `NombreCompleto` (string, required)
- `Email` (string, required, unique)
- `Telefono` (string, nullable)
- `FechaRegistro` (datetime)

### **Pedidos**
- `Id` (int, PK)
- `FechaPedido` (datetime)
- `UsuarioId` (int, FK)

### **PedidoProductos**
- `Id` (int, PK)
- `PedidoId` (int, FK)
- `ProductoId` (int, FK)
- `Cantidad` (int)
- `PrecioUnitario` (decimal)

## 🔧 Solución de Problemas Comunes

### Error: "Entity Framework tools not found"

Instala las herramientas de Entity Framework:

```bash
dotnet tool install --global dotnet-ef
```

### Error: "Database locked"

1. Cierra la aplicación si está ejecutándose
2. Elimina el archivo `products.db`
3. Ejecuta nuevamente: `dotnet ef database update`

### Error: "Migration already exists"

Si necesitas recrear las migraciones:

```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Error de Conexión

Verifica que:
1. El archivo `appsettings.json` tenga la cadena de conexión correcta
2. No haya otro proceso usando la base de datos
3. Tengas permisos de escritura en la carpeta del proyecto

## 📝 Pruebas de la API

### Crear un Usuario
```bash
curl -X POST "https://localhost:7xxx/api/Usuarios" \
  -H "Content-Type: application/json" \
  -d '{
    "nombreCompleto": "Juan Pérez",
    "email": "juan@example.com",
    "telefono": "+1234567890"
  }'
```

### Crear un Producto
```bash
curl -X POST "https://localhost:7xxx/api/Products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop Gaming",
    "description": "Laptop para gaming de alta gama",
    "price": 1500.00,
    "stock": 10,
    "category": "Electrónicos"
  }'
```

### Crear un Pedido
```bash
curl -X POST "https://localhost:7xxx/api/Pedidos" \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": 1,
    "pedidoProductos": [
      {
        "productoId": 1,
        "cantidad": 2
      }
    ]
  }'
```

## 🎯 Endpoints Disponibles

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/Products` | Listar todos los productos |
| GET | `/api/Products/{id}` | Obtener producto por ID |
| POST | `/api/Products` | Crear nuevo producto |
| PUT | `/api/Products/{id}` | Actualizar producto |
| DELETE | `/api/Products/{id}` | Eliminar producto |
| GET | `/api/Usuarios` | Listar todos los usuarios |
| GET | `/api/Usuarios/{id}` | Obtener usuario por ID |
| POST | `/api/Usuarios` | Crear nuevo usuario |
| PUT | `/api/Usuarios/{id}` | Actualizar usuario |
| DELETE | `/api/Usuarios/{id}` | Eliminar usuario |
| GET | `/api/Pedidos` | Listar todos los pedidos |
| GET | `/api/Pedidos/{id}` | Obtener pedido por ID |
| GET | `/api/Pedidos/usuario/{id}` | Pedidos por usuario |
| POST | `/api/Pedidos` | Crear nuevo pedido |
| PUT | `/api/Pedidos/{id}` | Actualizar pedido |
| DELETE | `/api/Pedidos/{id}` | Eliminar pedido |

## 📚 Recursos Adicionales

- [Documentación de .NET](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core Web API](https://docs.microsoft.com/aspnet/core/web-api/)
- [Swagger/OpenAPI](https://swagger.io/)

## 🆘 Soporte

Si encuentras algún problema durante la instalación:

1. Verifica que todos los requisitos estén instalados
2. Revisa los logs de error en la consola
3. Asegúrate de que no haya conflictos de puertos
4. Consulta la documentación oficial de .NET

¡Tu backend está listo para usar! 🎉
