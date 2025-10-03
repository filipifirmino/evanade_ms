using APIGateway.ApplicationCore.Domain.Entities;
using APIGateway.ApplicationCore.DTOs;

namespace APIGateway.ApplicationCore.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> AuthenticateAsync(LoginRequest request);
    Task<User> ValidateTokenAsync(string token);
    bool ValidateCredentials(string username, string password);
}