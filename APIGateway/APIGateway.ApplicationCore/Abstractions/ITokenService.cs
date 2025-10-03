using APIGateway.ApplicationCore.Domain.Entities;

namespace APIGateway.ApplicationCore.Abstractions;

public interface ITokenService
{
    string GenerateToken(User user);
    Task<User> ValidateTokenAsync(string token);
}