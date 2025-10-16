# Manual Completo - Procedimientos Almacenados en .NET

## 📋 Resumen

Este manual te guía paso a paso para implementar y usar procedimientos almacenados en tu aplicación .NET con Entity Framework Core, incluyendo configuración para SQL Server y fallback para SQLite.

---

## 🎯 Objetivos del Procedimiento Almacenado

**Problema a resolver:** Obtener un resumen de pedidos por usuario con:
- ID del pedido
- Fecha del pedido
- Número de líneas de productos
- Cantidad total de productos
- Total del pedido

**Ventajas del SP:**
- ✅ Performance optimizada
- ✅ Lógica encapsulada en la base de datos
- ✅ Reutilizable entre aplicaciones
- ✅ Seguridad a nivel de base de datos

---

## 📝 Paso a Paso Completo

### **PASO 1: Crear el Modelo para Resultados**

#### 1.1 Crear el modelo `UserOrderSummary` (`Models/Reports/UserOrderSummary.cs`)

```csharp
using Microsoft.EntityFrameworkCore;

namespace mvcproducts.Models.Reports
{
    [Keyless]
    public class UserOrderSummary
    {
        public int PedidoId { get; set; }
        public DateTime FechaPedido { get; set; }
        public int NumLineas { get; set; }
        public int CantidadTotal { get; set; }
        public decimal TotalPedido { get; set; }
    }
}
```

**¿Por qué `[Keyless]`?**
- Los resultados de SP no tienen clave primaria
- EF Core necesita saber que es una entidad sin clave

#### 1.2 Crear el DTO opcional (`Models/Dtos/UserOrderSummaryDto.cs`)

```csharp
namespace mvcproducts.Models.Dtos
{
    public class UserOrderSummaryDto
    {
        public int PedidoId { get; set; }
        public DateTime FechaPedido { get; set; }
        public int NumLineas { get; set; }
        public int CantidadTotal { get; set; }
        public decimal TotalPedido { get; set; }
    }
}
```

---

### **PASO 2: Configurar el DbContext**

#### 2.1 Agregar el DbSet en `ApplicationDbContext.cs`

```csharp
using mvcproducts.Models.Reports;

public class ApplicationDbContext : DbContext
{
    // ... otros DbSets existentes ...
    public DbSet<UserOrderSummary> UserOrderSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // ... otras configuraciones existentes ...
        
        // Configurar entidad sin clave para resultados de SP
        modelBuilder.Entity<UserOrderSummary>().HasNoKey();
    }
}
```

**¿Por qué `HasNoKey()`?**
- Confirma que la entidad no tiene clave primaria
- Necesario para mapear resultados de SP

---

### **PASO 3: Crear el Script del Procedimiento Almacenado**

#### 3.1 Crear el archivo SQL (`Database/StoredProcedures/sp_GetUserOrdersSummary.sql`)

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

**Explicación del SP:**
- `@UsuarioId`: Parámetro de entrada
- `SET NOCOUNT ON`: Optimización para no devolver conteo de filas
- `GROUP BY`: Agrupa por pedido para calcular totales
- `ORDER BY`: Ordena por fecha descendente

---

### **PASO 4: Configurar SQL Server (Opcional)**

#### 4.1 Cambiar cadena de conexión en `appsettings.json`

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=mvcproducts;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Opciones de cadena de conexión:**
- **Windows Authentication**: `Trusted_Connection=True`
- **SQL Server Authentication**: `User Id=usuario;Password=password;`
- **Azure SQL**: `Server=tcp:servidor.database.windows.net;Database=mvcproducts;User Id=usuario;Password=password;Encrypt=True;`

#### 4.2 Cambiar provider en `Program.cs`

```csharp
// Reemplazar la configuración de SQLite por SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
```

#### 4.3 Instalar el paquete NuGet (si no está instalado)

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

---

### **PASO 5: Crear el Endpoint en el Controlador**

#### 5.1 Agregar método en `PedidosController.cs`

```csharp
using Microsoft.Data.SqlClient;
using mvcproducts.Models.Reports;

[HttpGet("resumen-usuario/{usuarioId}")]
public async Task<ActionResult<IEnumerable<UserOrderSummary>>> GetResumenUsuario(int usuarioId)
{
    // Detectar el proveedor de base de datos
    var provider = _context.Database.ProviderName ?? string.Empty;

    if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        // Ejecutar SP en SQL Server
        var usuarioParam = new SqlParameter("@UsuarioId", usuarioId);
        var resultados = await _context.UserOrderSummaries
            .FromSqlRaw("EXEC dbo.sp_GetUserOrdersSummary @UsuarioId", usuarioParam)
            .ToListAsync();
        return resultados;
    }

    // Fallback LINQ para SQLite u otros proveedores
    var resumen = await _context.Pedidos
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

    return resumen;
}
```

**Explicación del código:**
- **Detección de provider**: Verifica si es SQL Server
- **Ejecución del SP**: Usa `FromSqlRaw` con parámetros
- **Fallback LINQ**: Misma lógica pero con LINQ para SQLite
- **Parámetros seguros**: Usa `SqlParameter` para evitar SQL injection

---

### **PASO 6: Instalar el Procedimiento en SQL Server**

#### 6.1 Conectar a SQL Server

**Opción A: SQL Server Management Studio (SSMS)**
1. Abrir SSMS
2. Conectar al servidor local o remoto
3. Expandir "Databases" → tu base de datos

**Opción B: Azure Data Studio**
1. Abrir Azure Data Studio
2. Conectar al servidor
3. Abrir la base de datos

#### 6.2 Ejecutar el script del SP

1. Abrir una nueva consulta
2. Copiar y pegar el contenido de `sp_GetUserOrdersSummary.sql`
3. Ejecutar el script (F5)

**Verificar que se creó:**
```sql
-- Verificar que el SP existe
SELECT name FROM sys.procedures WHERE name = 'sp_GetUserOrdersSummary'
```

---

### **PASO 7: Aplicar Migraciones en SQL Server**

#### 7.1 Eliminar base de datos SQLite (si existe)

```bash
# Si tienes datos importantes, haz backup primero
dotnet ef database drop
```

#### 7.2 Aplicar migraciones en SQL Server

```bash
dotnet ef database update
```

#### 7.3 Verificar que las tablas se crearon

```sql
-- Verificar tablas creadas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'
```

---

### **PASO 8: Probar el Procedimiento Almacenado**

#### 8.1 Ejecutar la aplicación

```bash
dotnet run
```

#### 8.2 Probar el endpoint

**Con curl:**
```bash
curl -X GET "https://localhost:7xxx/api/Pedidos/resumen-usuario/1"
```

**Con Swagger:**
1. Ir a `https://localhost:7xxx/swagger`
2. Expandir `GET /api/Pedidos/resumen-usuario/{usuarioId}`
3. Hacer clic en "Try it out"
4. Ingresar un `usuarioId` (ej: 1)
5. Hacer clic en "Execute"

#### 8.3 Probar directamente en SQL Server

```sql
-- Ejecutar el SP directamente
EXEC dbo.sp_GetUserOrdersSummary @UsuarioId = 1
```

---

### **PASO 9: Verificar el Funcionamiento**

#### 9.1 Respuesta esperada del endpoint

```json
[
  {
    "pedidoId": 15,
    "fechaPedido": "2024-10-08T10:30:00",
    "numLineas": 3,
    "cantidadTotal": 5,
    "totalPedido": 1250.00
  },
  {
    "pedidoId": 12,
    "fechaPedido": "2024-10-05T14:20:00",
    "numLineas": 2,
    "cantidadTotal": 3,
    "totalPedido": 899.99
  }
]
```

#### 9.2 Verificar que funciona con SQLite

Si cambias de vuelta a SQLite, el endpoint seguirá funcionando usando LINQ como fallback.

---

## 🔧 Configuraciones Adicionales

### **Configuración para Diferentes Entornos**

#### `appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=mvcproducts_dev;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### `appsettings.Production.json`
```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=prod-server;Database=mvcproducts_prod;User Id=app_user;Password=secure_password;Encrypt=True;"
  }
}
```

### **Configuración de Logging para SP**

```csharp
// En Program.cs, agregar logging detallado
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging(); // Solo para desarrollo
    options.EnableDetailedErrors(); // Solo para desarrollo
});
```

---

## 🚨 Solución de Problemas Comunes

### **Error: "Invalid object name 'sp_GetUserOrdersSummary'"**

**Causa:** El SP no existe en la base de datos
**Solución:**
1. Verificar que ejecutaste el script del SP
2. Verificar que estás en la base de datos correcta
3. Verificar permisos de usuario

### **Error: "Must declare the scalar variable '@UsuarioId'"**

**Causa:** Parámetro no declarado correctamente
**Solución:**
```csharp
// Usar SqlParameter correctamente
var usuarioParam = new SqlParameter("@UsuarioId", usuarioId);
```

### **Error: "The data reader is incompatible with the specified 'UserOrderSummary'"**

**Causa:** Los nombres de columnas del SP no coinciden con el modelo
**Solución:**
1. Verificar que los nombres en el SP coincidan con las propiedades del modelo
2. Usar alias en el SP si es necesario

### **Error: "Provider not supported"**

**Causa:** Usando SP en SQLite (no soportado)
**Solución:**
- El código ya tiene fallback LINQ para SQLite
- Verificar que la detección de provider funciona correctamente

---

## 📊 Comparación: SP vs LINQ

| Aspecto | Stored Procedure | LINQ |
|---------|------------------|------|
| **Performance** | ⭐⭐⭐⭐⭐ Optimizado | ⭐⭐⭐⭐ Bueno |
| **Portabilidad** | ⭐⭐ Solo SQL Server | ⭐⭐⭐⭐⭐ Multi-provider |
| **Mantenimiento** | ⭐⭐⭐ En base de datos | ⭐⭐⭐⭐⭐ En código |
| **Debugging** | ⭐⭐ Difícil | ⭐⭐⭐⭐⭐ Fácil |
| **Reutilización** | ⭐⭐⭐⭐ Entre apps | ⭐⭐⭐ En la app |

---

## 🎯 Casos de Uso Recomendados

### **Usar SP cuando:**
- ✅ Necesitas máxima performance
- ✅ La lógica es compleja y se reutiliza
- ✅ Requieres seguridad a nivel de base de datos
- ✅ Trabajas solo con SQL Server

### **Usar LINQ cuando:**
- ✅ Necesitas portabilidad entre proveedores
- ✅ La lógica cambia frecuentemente
- ✅ Prefieres mantener todo en código
- ✅ Usas SQLite o otros proveedores

---

## ✅ Checklist de Implementación

- [ ] Modelo `UserOrderSummary` creado con `[Keyless]`
- [ ] DbSet agregado en `ApplicationDbContext`
- [ ] Configuración `HasNoKey()` en `OnModelCreating`
- [ ] Script del SP creado y ejecutado en SQL Server
- [ ] Cadena de conexión configurada para SQL Server
- [ ] Provider cambiado a `UseSqlServer`
- [ ] Endpoint creado con detección de provider
- [ ] Migraciones aplicadas en SQL Server
- [ ] SP probado directamente en SQL Server
- [ ] Endpoint probado con datos reales
- [ ] Fallback LINQ verificado con SQLite

---

## 🚀 Resultado Final

Al completar todos los pasos tendrás:

1. **Procedimiento almacenado** funcionando en SQL Server
2. **Endpoint API** que usa el SP automáticamente
3. **Fallback LINQ** para SQLite y otros proveedores
4. **Sistema híbrido** que se adapta al proveedor de base de datos
5. **Documentación completa** del proceso

¡El procedimiento almacenado está completamente implementado y funcionando! 🎉
