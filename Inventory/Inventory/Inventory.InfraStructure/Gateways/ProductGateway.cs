using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Entities;
using Inventory.InfraStructure.Entitys;
using Inventory.InfraStructure.Mapper;
using Inventory.InfraStructure.Repositories.Abstractions;
using Inventory.InfraStructure.Tools;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Inventory.InfraStructure.Gateways;

public class ProductGateway : IProductGateway
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductGateway> _logger;
    public ProductGateway(IProductRepository repository, ILogger<ProductGateway> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<Product> AddProduct(Product product)
    {
        try
        {
            var result =  await _repository.AddAsync(product.ToProductEntity());
            return result.ToProduct();
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Error inserting record into database ", ex);
        }
    }

    public async Task UpdateProduct(Product product)
    {
        try
        {
            await _repository.UpdateAsync(product.ToProductEntity());
            _logger.LogInformation("Update produtc {@product}", product);
            return;
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Error updating record into database ", ex);
        }
    }

    public async Task DeleteProduct(Product product)
    {
        try
        {
            await _repository.DeleteAsync(product.ToProductEntity());
            _logger.LogInformation("Delelete produtc: {@product}", product);
            return;
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Error Deleting record into database ", ex);
        }
    }

    public async Task<Product?> GetProductById(Guid productId)
    {
        try
        {
            var result = await _repository.GetByIdAsync(productId);
            return result.ToProduct();
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Error Deleting record into database ", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetAllProducts()
    {
        try
        {
            var result = await _repository.GetAllAsync();
            return result.Select(x => x.ToProduct());
        }
        catch (SqlException ex)
        {
            throw new DataAccessException($"Error geting all record into database ", ex);
        }
    }
    
}