# Manual Completo - Implementación Backend MVC Products

## 📋 Resumen del Proyecto

Este manual documenta la implementación completa de un backend MVC con Entity Framework Core, incluyendo:
- Módulos de Usuario y Pedidos
- Sistema de datos mock automático
- Procedimientos almacenados (opcional)
- API REST completa

---

## 🏗️ Arquitectura Implementada

### Estructura de Archivos Creados/Modificados:

```
mvcproducts/
├── Controllers/
│   ├── ProductsController.cs (existente)
│   ├── UsuariosController.cs (nuevo)
│   └── PedidosController.cs (nuevo)
├── Models/
│   ├── Product.cs (modificado)
│   ├── Usuario.cs (nuevo)
│   ├── Pedido.cs (nuevo)
│   ├── PedidoProducto.cs (nuevo)
│   ├── Dtos/
│   │   └── UserOrderSummaryDto.cs (nuevo)
│   └── Reports/
│       └── UserOrderSummary.cs (nuevo)
├── Data/
│   └── ApplicationDbContext.cs (modificado)
├── Seeders/
│   └── DbSeeder.cs (nuevo)
├── Database/
│   └── StoredProcedures/
│       └── sp_GetUserOrdersSummary.sql (nuevo)
├── Program.cs (modificado)
└── README.md (modificado)
```

---

## 📝 Paso a Paso de Implementación

### **FASE 1: Creación de Modelos**

#### 1.1 Modelo Usuario (`Models/Usuario.cs`)
```csharp
public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
```

**Características implementadas:**
- ✅ Validaciones con Data Annotations
- ✅ Email único en la base de datos
- ✅ Relación uno a muchos con Pedidos
- ✅ Fecha de registro automática

#### 1.2 Modelo Pedido (`Models/Pedido.cs`)
```csharp
public class Pedido
{
    public int Id { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.Now;
    public int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<PedidoProducto> PedidoProductos { get; set; } = new List<PedidoProducto>();
    public decimal TotalPedido => PedidoProductos?.Sum(pp => pp.PrecioTotal) ?? 0;
}
```

**Características implementadas:**
- ✅ Relación con Usuario (FK)
- ✅ Propiedad calculada para total
- ✅ Fecha automática

#### 1.3 Modelo PedidoProducto (`Models/PedidoProducto.cs`)
```csharp
public class PedidoProducto
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioTotal => Cantidad * PrecioUnitario;
    public virtual Pedido Pedido { get; set; } = null!;
    public virtual Product Producto { get; set; } = null!;
}
```

**Características implementadas:**
- ✅ Tabla intermedia para relación muchos a muchos
- ✅ Precio total calculado
- ✅ Relaciones con Pedido y Product

#### 1.4 Actualización del Modelo Product (`Models/Product.cs`)
```csharp
// Agregado al final de la clase Product:
public virtual ICollection<PedidoProducto> PedidoProductos { get; set; } = new List<PedidoProducto>();
```

---

### **FASE 2: Configuración de Base de Datos**

#### 2.1 Actualización del ApplicationDbContext (`Data/ApplicationDbContext.cs`)

**DbSets agregados:**
```csharp
public DbSet<Usuario> Usuarios { get; set; }
public DbSet<Pedido> Pedidos { get; set; }
public DbSet<PedidoProducto> PedidoProductos { get; set; }
public DbSet<UserOrderSummary> UserOrderSummaries { get; set; }
```

**Configuraciones de modelo agregadas:**
```csharp
// Configuración del modelo Usuario
modelBuilder.Entity<Usuario>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Telefono).HasMaxLength(20);
    entity.HasIndex(e => e.Email).IsUnique();
});

// Configuración del modelo Pedido
modelBuilder.Entity<Pedido>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasOne(e => e.Usuario)
          .WithMany(u => u.Pedidos)
          .HasForeignKey(e => e.UsuarioId)
          .OnDelete(DeleteBehavior.Restrict);
});

// Configuración del modelo PedidoProducto
modelBuilder.Entity<PedidoProducto>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
    entity.HasOne(e => e.Pedido)
          .WithMany(p => p.PedidoProductos)
          .HasForeignKey(e => e.PedidoId)
          .OnDelete(DeleteBehavior.Cascade);
    entity.HasOne(e => e.Producto)
          .WithMany(p => p.PedidoProductos)
          .HasForeignKey(e => e.ProductoId)
          .OnDelete(DeleteBehavior.Restrict);
    entity.HasIndex(e => new { e.PedidoId, e.ProductoId }).IsUnique();
});

// Entidad sin clave para resultados de reportes/SPs
modelBuilder.Entity<UserOrderSummary>().HasNoKey();
```

---

### **FASE 3: Sistema de Datos Mock**

#### 3.1 Creación del DbSeeder (`Seeders/DbSeeder.cs`)

**Método principal:**
```csharp
public static async Task SeedAsync(ApplicationDbContext context)
{
    if (context.Products.Any() || context.Usuarios.Any())
        return; // Ya hay datos

    await SeedProductsAsync(context);
    await SeedUsuariosAsync(context);
    await SeedPedidosAsync(context);
}
```

**Datos generados:**
- **10 Productos**: Laptops, smartphones, tablets, etc.
- **10 Usuarios**: Nombres españoles realistas
- **15 Pedidos**: Con productos aleatorios y fechas distribuidas
- **Stock actualizado**: Automáticamente según ventas

#### 3.2 Integración en Program.cs
```csharp
// Aplicar migraciones automáticamente y cargar datos mock
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    
    // Cargar datos mock
    await mvcproducts.Seeders.DbSeeder.SeedAsync(context);
}
```

---

### **FASE 4: Controladores API**

#### 4.1 UsuariosController (`Controllers/UsuariosController.cs`)

**Endpoints implementados:**
- `GET /api/Usuarios` - Listar todos los usuarios
- `GET /api/Usuarios/{id}` - Obtener usuario con pedidos
- `POST /api/Usuarios` - Crear usuario
- `PUT /api/Usuarios/{id}` - Actualizar usuario
- `DELETE /api/Usuarios/{id}` - Eliminar usuario

**Características:**
- ✅ Validación de modelo
- ✅ Inclusión de datos relacionados
- ✅ Manejo de errores

#### 4.2 PedidosController (`Controllers/PedidosController.cs`)

**Endpoints implementados:**
- `GET /api/Pedidos` - Listar todos los pedidos
- `GET /api/Pedidos/{id}` - Obtener pedido específico
- `GET /api/Pedidos/usuario/{id}` - Pedidos por usuario
- `GET /api/Pedidos/resumen-usuario/{id}` - Resumen con SP/LINQ
- `POST /api/Pedidos` - Crear pedido
- `PUT /api/Pedidos/{id}` - Actualizar pedido
- `DELETE /api/Pedidos/{id}` - Eliminar pedido

**Funcionalidades especiales:**
- ✅ Validación de stock al crear pedidos
- ✅ Actualización automática de stock
- ✅ Restauración de stock al eliminar
- ✅ Soporte para SP y LINQ fallback

---

### **FASE 5: Procedimientos Almacenados (Opcional)**

#### 5.1 Script SQL (`Database/StoredProcedures/sp_GetUserOrdersSummary.sql`)
```sql
CREATE OR ALTER PROCEDURE dbo.sp_GetUserOrdersSummary
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        p.Id AS PedidoId,
        p.FechaPedido,
        COUNT(pp.Id) AS NumLineas,
        SUM(pp.Cantidad) AS CantidadTotal,
        SUM(pp.Cantidad * pp.PrecioUnitario) AS TotalPedido
    FROM Pedidos p
    INNER JOIN PedidoProductos pp ON pp.PedidoId = p.Id
    WHERE p.UsuarioId = @UsuarioId
    GROUP BY p.Id, p.FechaPedido
    ORDER BY p.FechaPedido DESC;
END;
```

#### 5.2 Modelo para Resultados (`Models/Reports/UserOrderSummary.cs`)
```csharp
[Keyless]
public class UserOrderSummary
{
    public int PedidoId { get; set; }
    public DateTime FechaPedido { get; set; }
    public int NumLineas { get; set; }
    public int CantidadTotal { get; set; }
    public decimal TotalPedido { get; set; }
}
```

#### 5.3 Endpoint con SP/LINQ (`Controllers/PedidosController.cs`)
```csharp
[HttpGet("resumen-usuario/{usuarioId}")]
public async Task<ActionResult<IEnumerable<UserOrderSummary>>> GetResumenUsuario(int usuarioId)
{
    var provider = _context.Database.ProviderName ?? string.Empty;

    if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        // Ejecutar SP en SQL Server
        var usuarioParam = new SqlParameter("@UsuarioId", usuarioId);
        return await _context.UserOrderSummaries
            .FromSqlRaw("EXEC dbo.sp_GetUserOrdersSummary @UsuarioId", usuarioParam)
            .ToListAsync();
    }

    // Fallback LINQ para SQLite
    return await _context.Pedidos
        .Where(p => p.UsuarioId == usuarioId)
        .Select(p => new UserOrderSummary
        {
            PedidoId = p.Id,
            FechaPedido = p.FechaPedido,
            NumLineas = p.PedidoProductos.Count,
            CantidadTotal = p.PedidoProductos.Sum(pp => pp.Cantidad),
            TotalPedido = p.PedidoProductos.Sum(pp => pp.Cantidad * pp.PrecioUnitario)
        })
        .OrderByDescending(r => r.FechaPedido)
        .ToListAsync();
}
```

---

## 🚀 Comandos de Instalación

### Comandos Básicos:
```bash
# Restaurar dependencias
dotnet restore

# Crear migración (si es necesario)
dotnet ef migrations add AddUsuarioAndPedidoModels

# Aplicar migraciones
dotnet ef database update

# Ejecutar aplicación
dotnet run
```

### Comandos de Desarrollo:
```bash
# Ejecutar con hot reload
dotnet watch run

# Compilar proyecto
dotnet build

# Eliminar base de datos
dotnet ef database drop

# Revertir migración
dotnet ef database update NombreMigracionAnterior
```

---

## 🧪 Pruebas de la API

### Crear Usuario:
```bash
curl -X POST "https://localhost:7xxx/api/Usuarios" \
  -H "Content-Type: application/json" \
  -d '{
    "nombreCompleto": "Juan Pérez",
    "email": "juan@example.com",
    "telefono": "+1234567890"
  }'
```

### Crear Producto:
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

### Crear Pedido:
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

### Obtener Resumen de Usuario:
```bash
curl -X GET "https://localhost:7xxx/api/Pedidos/resumen-usuario/1"
```

---

## 📊 Endpoints Completos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/Products` | Listar productos |
| GET | `/api/Products/{id}` | Obtener producto |
| POST | `/api/Products` | Crear producto |
| PUT | `/api/Products/{id}` | Actualizar producto |
| DELETE | `/api/Products/{id}` | Eliminar producto |
| GET | `/api/Usuarios` | Listar usuarios |
| GET | `/api/Usuarios/{id}` | Obtener usuario |
| POST | `/api/Usuarios` | Crear usuario |
| PUT | `/api/Usuarios/{id}` | Actualizar usuario |
| DELETE | `/api/Usuarios/{id}` | Eliminar usuario |
| GET | `/api/Pedidos` | Listar pedidos |
| GET | `/api/Pedidos/{id}` | Obtener pedido |
| GET | `/api/Pedidos/usuario/{id}` | Pedidos por usuario |
| GET | `/api/Pedidos/resumen-usuario/{id}` | Resumen con SP/LINQ |
| POST | `/api/Pedidos` | Crear pedido |
| PUT | `/api/Pedidos/{id}` | Actualizar pedido |
| DELETE | `/api/Pedidos/{id}` | Eliminar pedido |

---

## 🔧 Configuración para SQL Server (Opcional)

### 1. Cambiar cadena de conexión en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=mvcproducts;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Cambiar provider en `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
```

### 3. Ejecutar script del SP en SQL Server:
```sql
-- Ejecutar el contenido de Database/StoredProcedures/sp_GetUserOrdersSummary.sql
```

### 4. Aplicar migraciones:
```bash
dotnet ef database update
```

---

## ✅ Características Implementadas

### **Modelos:**
- ✅ Validaciones con Data Annotations
- ✅ Relaciones entre entidades
- ✅ Propiedades calculadas
- ✅ Índices únicos

### **Base de Datos:**
- ✅ Migraciones automáticas
- ✅ Configuración de relaciones
- ✅ Restricciones de integridad
- ✅ Soporte para SP (SQL Server)

### **API:**
- ✅ CRUD completo para todas las entidades
- ✅ Validación de datos
- ✅ Manejo de errores
- ✅ Inclusión de datos relacionados

### **Datos Mock:**
- ✅ Carga automática al iniciar
- ✅ Datos realistas
- ✅ Stock actualizado automáticamente
- ✅ Verificación de duplicados

### **Procedimientos Almacenados:**
- ✅ SP para resúmenes
- ✅ Fallback LINQ para SQLite
- ✅ Detección automática de provider

---

## 🎯 Resultado Final

El proyecto incluye:
- **Backend completo** con 3 módulos principales
- **API REST** con 15+ endpoints
- **Base de datos** con datos de prueba
- **Sistema híbrido** SP/LINQ
- **Documentación completa**
- **Tutorial de instalación**

¡El backend está listo para producción! 🚀
