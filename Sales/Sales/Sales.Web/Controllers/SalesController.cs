using Microsoft.AspNetCore.Mvc;
using Sales.Web.ResponsesDto;
using Sales.Web.ResquestsDto;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SalesController : ControllerBase
{
    public SalesController()
    {
        
    }
 
    [HttpGet]
    [Route("all-sales")]
    public ActionResult<IEnumerable<OrderResponse>> GetAll()
        => Ok(_sales.Values);
    
    [HttpGet]
    [Route("get-by-id")]
    public ActionResult<OrderResponse> GetById([FromHeader] Guid id)
        => _sales.TryGetValue(id, out var sale) ? Ok(sale) : NotFound();
    
    [HttpPost]
    [Route("create-sale")]
    public ActionResult<OrderResponse> Create([FromBody] OrderRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var sale = new OrderResponse(
            Id: Guid.NewGuid(),
            CustomerName: request.CustomerName,
            Amount: request.Amount,
            Date: request.Date
        );

        _sales[sale.Id] = sale;

        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }
    
    [HttpPut]
    [Route("update-sale")]
    public IActionResult Update([FromHeader] Guid id, [FromBody] OrderRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!_sales.ContainsKey(id))
            return NotFound();

        _sales[id] = new SaleResponse(
            Id: id,
            CustomerName: request.CustomerName,
            Amount: request.Amount,
            Date: request.Date
        );

        return NoContent();
    }
    
    [HttpDelete]
    [Route("remove-sale")]
    public IActionResult Delete([FromHeader] Guid id)
        => _sales.TryRemove(id, out _) ? NoContent() : NotFound();

    
}
