using Microsoft.AspNetCore.Mvc;
using Sales.BFF.ApplicationCore.Abstractions;
using Sales.BFF.ApplicationCore.DTOs;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
        
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
        
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                return BadRequest(new { message = "Invalid request data" });
            }
                
            var response = await _authService.AuthenticateAsync(request);
                
            if (response == null)
            {
                _logger.LogWarning($"Failed login attempt for user: {request.Username}");
                return Unauthorized(new { message = "Invalid credentials" });
            }
                
            _logger.LogInformation($"User {request.Username} logged in successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}