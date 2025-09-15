using Sales.Application.Entities;
using Sales.Web.ResponsesDto;
using Sales.Web.ResquestsDto;

namespace Sales.Web.Mappers;

public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order)
        => new OrderResponse
        {

        };

    public static Order ToSales(this OrderRequest request)
        => new Order
        {

        };
}