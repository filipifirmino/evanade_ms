using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Entities;
using Inventory.InfraStructure.Repositories.Abstractions;

namespace Inventory.InfraStructure.Repositories;

public class ProductRepository(DataContext context) 
    : RepositoreBase<ProductEntity>(context), IProductRepository;