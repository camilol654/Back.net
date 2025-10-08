using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvcproducts.Models
{
    public class PedidoProducto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El pedido es obligatorio")]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio Unitario")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        // Propiedad calculada para el precio total de este producto en el pedido
        [NotMapped]
        [Display(Name = "Precio Total")]
        [DataType(DataType.Currency)]
        public decimal PrecioTotal => Cantidad * PrecioUnitario;

        // Propiedades de navegación
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Product Producto { get; set; } = null!;
    }
}

