using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using mvcproducts.Data;
using mvcproducts.Models;
using mvcproducts.Models.Reports;

namespace mvcproducts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pedidos/resumen-usuario/5
        [HttpGet("resumen-usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<UserOrderSummary>>> GetResumenUsuario(int usuarioId)
        {
            // Si el proveedor es SQL Server, intentamos ejecutar el SP
            var provider = _context.Database.ProviderName ?? string.Empty;

            if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                var usuarioParam = new SqlParameter("@UsuarioId", usuarioId);
                var resultados = await _context.UserOrderSummaries
                    .FromSqlRaw("EXEC dbo.sp_GetUserOrdersSummary @UsuarioId", usuarioParam)
                    .ToListAsync();
                return resultados;
            }

            // Fallback para SQLite u otros proveedores: calcular con LINQ
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

        // GET: api/Pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.PedidoProductos)
                .ThenInclude(pp => pp.Producto)
                .ToListAsync();
        }

        // GET: api/Pedidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.PedidoProductos)
                .ThenInclude(pp => pp.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return pedido;
        }

        // GET: api/Pedidos/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosByUsuario(int usuarioId)
        {
            var pedidos = await _context.Pedidos
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Usuario)
                .Include(p => p.PedidoProductos)
                .ThenInclude(pp => pp.Producto)
                .ToListAsync();

            return pedidos;
        }

        // POST: api/Pedidos
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el usuario existe
            var usuario = await _context.Usuarios.FindAsync(pedido.UsuarioId);
            if (usuario == null)
            {
                return BadRequest("El usuario especificado no existe");
            }

            // Verificar que todos los productos existen y tienen stock suficiente
            foreach (var pedidoProducto in pedido.PedidoProductos)
            {
                var producto = await _context.Products.FindAsync(pedidoProducto.ProductoId);
                if (producto == null)
                {
                    return BadRequest($"El producto con ID {pedidoProducto.ProductoId} no existe");
                }

                if (producto.Stock < pedidoProducto.Cantidad)
                {
                    return BadRequest($"Stock insuficiente para el producto {producto.Name}. Stock disponible: {producto.Stock}");
                }

                // Usar el precio actual del producto
                pedidoProducto.PrecioUnitario = producto.Price;
            }

            pedido.FechaPedido = DateTime.Now;
            _context.Pedidos.Add(pedido);

            // Actualizar el stock de los productos
            foreach (var pedidoProducto in pedido.PedidoProductos)
            {
                var producto = await _context.Products.FindAsync(pedidoProducto.ProductoId);
                producto!.Stock -= pedidoProducto.Cantidad;
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
        }

        // PUT: api/Pedidos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Entry(pedido).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Pedidos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.PedidoProductos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // Restaurar el stock de los productos
            foreach (var pedidoProducto in pedido.PedidoProductos)
            {
                var producto = await _context.Products.FindAsync(pedidoProducto.ProductoId);
                if (producto != null)
                {
                    producto.Stock += pedidoProducto.Cantidad;
                }
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }
    }
}

