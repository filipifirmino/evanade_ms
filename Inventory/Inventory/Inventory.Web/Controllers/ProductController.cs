using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Entities;
using Inventory.Web.Mappers;
using Inventory.Web.RequestsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductController(IProductGateway productGateway, ILogger<ProductController> logger) : ControllerBase
{
    [HttpGet]
    [Route("all-products")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await productGateway.GetAllProducts();
            return Ok(products.Select(x => x.ToProductResponse()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar todos os produtos");
            throw; // O middleware global irá capturar e tratar
        }
    }

    [HttpGet]
    [Route("product-by-id")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetProductById([FromHeader] Guid id)
    {
        try
        {
            logger.LogInformation("Get product with id: {id}", id);
            var product = await productGateway.GetProductById(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar produto com id: {id}", id);
            throw; // O middleware global irá capturar e tratar
        }
    }
    
    [HttpGet]
    [Route("quantity-available-product-by-id")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetQuantityAvailableProductById([FromHeader] Guid id)
    {
        try
        {
            logger.LogInformation("Get quantity product with id: {id}", id);
            var product = await productGateway.GetProductById(id);
            if (product == null)
                return NotFound();
            return Ok(product.GetStockAvailable());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar quantidade disponível do produto com id: {id}", id);
            throw; // O middleware global irá capturar e tratar
        }
    }

    [HttpPost]
    [Route("create-product")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest product)
    {
        try
        {
            var createdProduct = await productGateway.AddProduct(product.ToProduct());
            return Ok(createdProduct.ToProductResponse());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar produto: {@product}", product);
            throw; // O middleware global irá capturar e tratar
        }
    }
    
    [HttpPut]
    [Route("update-product")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct([FromBody] ProductRequest product)
    {
        try
        {
            await productGateway.UpdateProduct(product.ToProduct());
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar produto: {@product}", product);
            throw; // O middleware global irá capturar e tratar
        }
    }

    [HttpDelete]
    [Route("remove-product")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct([FromBody] Product product)
    {
        try
        {
            await productGateway.DeleteProduct(product);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir produto: {@product}", product);
            throw; // O middleware global irá capturar e tratar
        }
    }

}