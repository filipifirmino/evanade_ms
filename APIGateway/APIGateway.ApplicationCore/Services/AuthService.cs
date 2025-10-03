using Microsoft.Extensions.Logging;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.Domain.Entities;
using APIGateway.ApplicationCore.DTOs;
using APIGateway.ApplicationCore.Exceptions;

namespace APIGateway.ApplicationCore.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;
    
    // Simulação de usuários em memória - em produção seria um banco de dados
    private readonly Dictionary<string, User> _users = new()
    {
        {
            "admin", new User
            {
                Id = "1",
                Username = "admin",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        },
        {
            "user", new User
            {
                Id = "2", 
                Username = "user",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            }
        }
    };
    
    // Simulação de senhas - em produção seria hash + salt
    private readonly Dictionary<string, string> _passwords = new()
    {
        { "admin", "admin123" },
        { "user", "user123" }
    };

    public AuthService(ITokenService tokenService, ILogger<AuthService> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Tentativa de autenticação para usuário: {Username}", request.Username);

            if (!ValidateCredentials(request.Username, request.Password))
            {
                _logger.LogWarning("Credenciais inválidas para usuário: {Username}", request.Username);
                return null;
            }

            var user = _users[request.Username];
            var token = _tokenService.GenerateToken(user);
            var expirationMinutes = 60; // Configurável via appsettings

            var response = new LoginResponse
            {
                Token = token,
                Username = user.Username,
                ExpiresIn = expirationMinutes * 60, // em segundos
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            _logger.LogInformation("Usuário autenticado com sucesso: {Username}", request.Username);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante autenticação do usuário: {Username}", request.Username);
            throw new AuthenticationException("Erro interno durante autenticação");
        }
    }

    public async Task<User> ValidateTokenAsync(string token)
    {
        try
        {
            return await _tokenService.ValidateTokenAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante validação do token");
            return null;
        }
    }

    public bool ValidateCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;

        return _passwords.TryGetValue(username, out var storedPassword) && 
               storedPassword == password;
    }
}