namespace Sales.Application.AbstractionsGateways;

public interface IHttpGateway
{
    Task<int> GetProductStockQuantity(Guid productId);
}