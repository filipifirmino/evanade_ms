using Sales.Application.Enums;
using Sales.Infrastructure.Enums;

namespace Sales.Infrastructure.Mappers;

public static class StatusMapper
{
    public static Status ToOrderStatus(this StatusEntity statusEntity)
    {
        return statusEntity switch
        {
            StatusEntity.Created => Status.Created,
            StatusEntity.Confirmed => Status.Confirmed,
            StatusEntity.Cancelled => Status.Cancelled,
            StatusEntity.Failed => Status.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(statusEntity), statusEntity, "Status não mapeado")
        };
    }

    public static StatusEntity ToStatusEntity(this Status status)
    {
        return status switch
        {
            Status.Created => StatusEntity.Created,
            Status.Confirmed => StatusEntity.Confirmed,
            Status.Cancelled => StatusEntity.Cancelled,
            Status.Failed => StatusEntity.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Status não mapeado")
        };
    }
}
