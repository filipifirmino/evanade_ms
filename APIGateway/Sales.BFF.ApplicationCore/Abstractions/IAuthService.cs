using Sales.BFF.ApplicationCore.Domain.Entities;
using Sales.BFF.ApplicationCore.DTOs;

namespace Sales.BFF.ApplicationCore.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> AuthenticateAsync(LoginRequest request);
    Task<User> ValidateTokenAsync(string token);
    bool ValidateCredentials(string username, string password);
}