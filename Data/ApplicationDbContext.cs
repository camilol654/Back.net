using Microsoft.EntityFrameworkCore;
using mvcproducts.Models;

namespace mvcproducts.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoProducto> PedidoProductos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración del modelo Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(50);
            });

            // Configuración del modelo Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                
                // Índice único para el email
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configuración del modelo Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Relación con Usuario
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
                
                // Relación con Pedido
                entity.HasOne(e => e.Pedido)
                      .WithMany(p => p.PedidoProductos)
                      .HasForeignKey(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Relación con Product
                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.PedidoProductos)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Índice compuesto para evitar duplicados
                entity.HasIndex(e => new { e.PedidoId, e.ProductoId }).IsUnique();
            });
        }
    }
}
