using Microsoft.AspNetCore.Mvc;
using Sales.Application.AbstractionsGateways;
using Sales.Web.Mappers;
using Sales.Web.ResponsesDto;
using Sales.Web.ResquestsDto;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SalesController : ControllerBase
{
    private readonly IOrderGateway _orderGateway;
    public SalesController(IOrderGateway orderGateway)
    {
        _orderGateway = orderGateway;
    }
 
    [HttpGet]
    [Route("all-sales")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderGateway.GetAllOrders();
        return Ok(result.Select(x => x.ToResponse()));
    }
    
    [HttpGet]
    [Route("get-by-id")]
    public async Task<IActionResult> GetById([FromHeader] Guid id)
    {
        var result = await _orderGateway.GetOrderById(id);
        return result is null ? NotFound() : Ok(result.ToResponse());
    }
    
    [HttpPost]
    [Route("create-sale")]
    public async Task<IActionResult> Create([FromBody] OrderRequest request)
    {
        var result = await _orderGateway.AddProduct(request.ToOrder());
        return Ok(result.ToResponse());
    }
    
    [HttpPut]
    [Route("update-sale")]
    public async Task<IActionResult> Update([FromHeader] Guid id, [FromBody] OrderRequest request)
    {
       await _orderGateway.UpdateOrder(request.ToOrder());
       return Ok();
    }

    [HttpDelete]
    [Route("remove-sale")]
    public async Task<IActionResult> Delete([FromBody] OrderRequest request)
    {
        await _orderGateway.DeleteOrder(request.ToOrder());
        return Ok();
    }

    
}
