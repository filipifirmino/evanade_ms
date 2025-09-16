using Sales.Application.Entities;
using Sales.Web.ResponsesDto;
using Sales.Web.ResquestsDto;

namespace Sales.Web.Mappers;

public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order)
        => new OrderResponse
        {
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Items = order.Items
        };

    public static Order ToOrder(this OrderRequest request)
        => new Order
        {
            Status = request.Status,
            TotalAmount = request.TotalAmount,
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Items = request.Items
        };
}