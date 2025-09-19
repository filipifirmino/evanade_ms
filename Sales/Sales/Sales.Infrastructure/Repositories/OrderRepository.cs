using Sales.Infrastructure.Configure;
using Sales.Infrastructure.Entities;
using Sales.Infrastructure.Repositories.Abstractions;

namespace Sales.Infrastructure.Repositories;

public class OrderRepository(DataContext context) 
    : RepositoryBase<OrderEntity>(context), IOrderRepository
{
    private readonly DataContext _context = context;
}