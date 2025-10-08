using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvcproducts.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        [Display(Name = "Fecha del Pedido")]
        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public int UsuarioId { get; set; }

        // Propiedad de navegación hacia Usuario
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        // Propiedad de navegación hacia los productos del pedido
        public virtual ICollection<PedidoProducto> PedidoProductos { get; set; } = new List<PedidoProducto>();

        // Propiedad calculada para el total del pedido
        [NotMapped]
        [Display(Name = "Total del Pedido")]
        [DataType(DataType.Currency)]
        public decimal TotalPedido => PedidoProductos?.Sum(pp => pp.PrecioTotal) ?? 0;
    }
}

