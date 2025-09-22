namespace APIGateway.ApplicationCore.DTOs;

public class LoginResponse
{
    public string Token { get; set; }
    public string Username { get; set; }
    public int ExpiresIn { get; set; }
    public DateTime ExpiresAt { get; set; }
}