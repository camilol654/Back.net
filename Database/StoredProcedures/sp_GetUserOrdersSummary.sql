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
