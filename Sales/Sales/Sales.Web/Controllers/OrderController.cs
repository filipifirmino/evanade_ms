using Microsoft.AspNetCore.Mvc;
using Sales.Application.AbstractionsGateways;
using Sales.Application.UseCases.Abstractions;
using Sales.Web.Mappers;
using Sales.Web.ResponsesDto;
using Sales.Web.ResquestsDto;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderGateway _orderGateway;
    private readonly IOrderProcess _orderProcess;
    
    
    public OrderController(IOrderGateway orderGateway, IOrderProcess orderProcess)
    {
        _orderGateway = orderGateway;
        _orderProcess = orderProcess;
    }
 
    [HttpGet]
    [Route("all-order")]
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
    [Route("create-order")]
    public async Task<IActionResult> Create([FromBody] OrderRequest request)
    {
        var result = await _orderProcess.HandleOrder(request.ToOrder());

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result.Value.ToResponse());
    }
    
    [HttpPut]
    [Route("update-order")]
    public async Task<IActionResult> Update([FromHeader] Guid id, [FromBody] OrderRequest request)
    {
        await _orderGateway.UpdateOrder(request.ToOrder());
        return Ok(new { message = "Order updated successfully" });
    }

    [HttpDelete]
    [Route("remove-order")]
    public async Task<IActionResult> Delete([FromBody] OrderRequest request)
    {
        await _orderGateway.DeleteOrder(request.ToOrder());
        return Ok(new { message = "Order deleted successfully" });
    }

    
}
