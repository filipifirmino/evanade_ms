using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Entities;
using Inventory.Web.Mappers;
using Inventory.Web.RequestsDto;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class ProductController(IProductGateway productGateway, ILogger<ProductController> logger) : ControllerBase
{
    [HttpGet]
    [Route("all-products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await productGateway.GetAllProducts();
        return Ok(products.Select(x => x.ToProductResponse()));
    }

    [HttpGet]
    [Route("product-by-id")]
    public async Task<IActionResult> GetProductById([FromHeader] Guid id)
    {
        logger.LogInformation("Get product with id: {id}", id);
        var product = await productGateway.GetProductById(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }
    
    [HttpGet]
    [Route("quantity-available-product-by-id")]
    public async Task<IActionResult> GetQuantityAvailableProductById([FromHeader] Guid id)
    {
        logger.LogInformation("Get quantity product with id: {id}", id);
        var product = await productGateway.GetProductById(id);
        if (product == null)
            return NotFound();
        return Ok(product.GetStockAvailable());
    }

    [HttpPost]
    [Route("create-product")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest product)
    {
        var createdProduct = await productGateway.AddProduct(product.ToProduct());
        return Ok(createdProduct.ToProductResponse());
    }
    
    [HttpPut]
    [Route("update-product")]
    public async Task<IActionResult> UpdateProduct([FromBody] ProductRequest product)
    {
        await productGateway.UpdateProduct(product.ToProduct());
        return Ok();
    }

    [HttpDelete]
    [Route("remove-product")]
    public async Task<IActionResult> DeleteProduct([FromBody] Product product)
    {
        await productGateway.DeleteProduct(product);
        return Ok();
    }

}