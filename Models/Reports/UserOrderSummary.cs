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
