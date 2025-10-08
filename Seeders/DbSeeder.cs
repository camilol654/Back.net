using mvcproducts.Data;
using mvcproducts.Models;

namespace mvcproducts.Seeders
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Verificar si ya hay datos
            if (context.Products.Any() || context.Usuarios.Any())
            {
                return; // Ya hay datos, no hacer seeding
            }

            await SeedProductsAsync(context);
            await SeedUsuariosAsync(context);
            await SeedPedidosAsync(context);
        }

        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Laptop Gaming ASUS ROG",
                    Description = "Laptop para gaming de alta gama con RTX 4070",
                    Price = 1599.99m,
                    Stock = 15,
                    Category = "Electrónicos",
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Description = "Smartphone premium con cámara de 48MP",
                    Price = 1199.99m,
                    Stock = 25,
                    Category = "Smartphones",
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                new Product
                {
                    Name = "Samsung Galaxy S24 Ultra",
                    Description = "Smartphone Android con S Pen incluido",
                    Price = 1299.99m,
                    Stock = 20,
                    Category = "Smartphones",
                    CreatedDate = DateTime.Now.AddDays(-20)
                },
                new Product
                {
                    Name = "MacBook Pro M3",
                    Description = "Laptop profesional con chip M3 de Apple",
                    Price = 1999.99m,
                    Stock = 10,
                    Category = "Electrónicos",
                    CreatedDate = DateTime.Now.AddDays(-15)
                },
                new Product
                {
                    Name = "AirPods Pro 2",
                    Description = "Auriculares inalámbricos con cancelación de ruido",
                    Price = 249.99m,
                    Stock = 50,
                    Category = "Audio",
                    CreatedDate = DateTime.Now.AddDays(-10)
                },
                new Product
                {
                    Name = "iPad Air 5",
                    Description = "Tablet con chip M1 y pantalla Liquid Retina",
                    Price = 599.99m,
                    Stock = 30,
                    Category = "Tablets",
                    CreatedDate = DateTime.Now.AddDays(-5)
                },
                new Product
                {
                    Name = "Sony WH-1000XM5",
                    Description = "Auriculares over-ear con cancelación de ruido líder",
                    Price = 399.99m,
                    Stock = 18,
                    Category = "Audio",
                    CreatedDate = DateTime.Now.AddDays(-3)
                },
                new Product
                {
                    Name = "Nintendo Switch OLED",
                    Description = "Consola portátil con pantalla OLED de 7 pulgadas",
                    Price = 349.99m,
                    Stock = 22,
                    Category = "Gaming",
                    CreatedDate = DateTime.Now.AddDays(-1)
                },
                new Product
                {
                    Name = "Dell XPS 13",
                    Description = "Ultrabook premium con pantalla InfinityEdge",
                    Price = 1299.99m,
                    Stock = 12,
                    Category = "Electrónicos",
                    CreatedDate = DateTime.Now.AddDays(-7)
                },
                new Product
                {
                    Name = "Apple Watch Series 9",
                    Description = "Smartwatch con GPS y monitor de salud avanzado",
                    Price = 399.99m,
                    Stock = 35,
                    Category = "Wearables",
                    CreatedDate = DateTime.Now.AddDays(-2)
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsuariosAsync(ApplicationDbContext context)
        {
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    NombreCompleto = "María González Rodríguez",
                    Email = "maria.gonzalez@email.com",
                    Telefono = "+34 612 345 678",
                    FechaRegistro = DateTime.Now.AddDays(-60)
                },
                new Usuario
                {
                    NombreCompleto = "Carlos López Martínez",
                    Email = "carlos.lopez@email.com",
                    Telefono = "+34 623 456 789",
                    FechaRegistro = DateTime.Now.AddDays(-45)
                },
                new Usuario
                {
                    NombreCompleto = "Ana Fernández Sánchez",
                    Email = "ana.fernandez@email.com",
                    Telefono = "+34 634 567 890",
                    FechaRegistro = DateTime.Now.AddDays(-30)
                },
                new Usuario
                {
                    NombreCompleto = "David Ruiz García",
                    Email = "david.ruiz@email.com",
                    Telefono = "+34 645 678 901",
                    FechaRegistro = DateTime.Now.AddDays(-25)
                },
                new Usuario
                {
                    NombreCompleto = "Laura Jiménez Pérez",
                    Email = "laura.jimenez@email.com",
                    Telefono = "+34 656 789 012",
                    FechaRegistro = DateTime.Now.AddDays(-20)
                },
                new Usuario
                {
                    NombreCompleto = "Miguel Torres Díaz",
                    Email = "miguel.torres@email.com",
                    Telefono = "+34 667 890 123",
                    FechaRegistro = DateTime.Now.AddDays(-15)
                },
                new Usuario
                {
                    NombreCompleto = "Sofia Herrera Moreno",
                    Email = "sofia.herrera@email.com",
                    Telefono = "+34 678 901 234",
                    FechaRegistro = DateTime.Now.AddDays(-10)
                },
                new Usuario
                {
                    NombreCompleto = "Javier Morales Castro",
                    Email = "javier.morales@email.com",
                    Telefono = "+34 689 012 345",
                    FechaRegistro = DateTime.Now.AddDays(-5)
                },
                new Usuario
                {
                    NombreCompleto = "Elena Vargas Romero",
                    Email = "elena.vargas@email.com",
                    Telefono = "+34 690 123 456",
                    FechaRegistro = DateTime.Now.AddDays(-3)
                },
                new Usuario
                {
                    NombreCompleto = "Roberto Silva Aguilar",
                    Email = "roberto.silva@email.com",
                    Telefono = "+34 601 234 567",
                    FechaRegistro = DateTime.Now.AddDays(-1)
                }
            };

            context.Usuarios.AddRange(usuarios);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPedidosAsync(ApplicationDbContext context)
        {
            // Obtener usuarios y productos para crear pedidos realistas
            var usuarios = context.Usuarios.ToList();
            var productos = context.Products.ToList();

            if (!usuarios.Any() || !productos.Any())
                return;

            var random = new Random();
            var pedidos = new List<Pedido>();

            // Crear pedidos para diferentes usuarios
            for (int i = 0; i < 15; i++)
            {
                var usuario = usuarios[random.Next(usuarios.Count)];
                var fechaPedido = DateTime.Now.AddDays(-random.Next(1, 60));
                
                var pedido = new Pedido
                {
                    UsuarioId = usuario.Id,
                    FechaPedido = fechaPedido,
                    PedidoProductos = new List<PedidoProducto>()
                };

                // Agregar 1-4 productos aleatorios al pedido
                var cantidadProductos = random.Next(1, 5);
                var productosSeleccionados = productos.OrderBy(x => random.Next()).Take(cantidadProductos);

                foreach (var producto in productosSeleccionados)
                {
                    var cantidad = random.Next(1, 4); // 1-3 unidades por producto
                    
                    pedido.PedidoProductos.Add(new PedidoProducto
                    {
                        ProductoId = producto.Id,
                        Cantidad = cantidad,
                        PrecioUnitario = producto.Price
                    });
                }

                pedidos.Add(pedido);
            }

            context.Pedidos.AddRange(pedidos);
            await context.SaveChangesAsync();

            // Actualizar el stock de los productos según los pedidos
            await UpdateProductStockAsync(context);
        }

        private static async Task UpdateProductStockAsync(ApplicationDbContext context)
        {
            var productos = context.Products.ToList();
            var pedidoProductos = context.PedidoProductos.ToList();

            foreach (var producto in productos)
            {
                var cantidadVendida = pedidoProductos
                    .Where(pp => pp.ProductoId == producto.Id)
                    .Sum(pp => pp.Cantidad);

                producto.Stock = Math.Max(0, producto.Stock - cantidadVendida);
            }

            await context.SaveChangesAsync();
        }
    }
}
