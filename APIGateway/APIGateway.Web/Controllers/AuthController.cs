using Microsoft.AspNetCore.Mvc;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.DTOs;

namespace APIGateway.Web.Controllers;

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
                _logger.LogWarning("Failed login attempt for user: {RequestUsername}", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }
                
            _logger.LogInformation("User {RequestUsername} logged in successfully", request.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}