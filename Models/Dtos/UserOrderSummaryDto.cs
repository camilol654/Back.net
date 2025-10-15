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
