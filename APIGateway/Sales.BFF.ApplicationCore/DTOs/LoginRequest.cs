namespace Sales.BFF.ApplicationCore.DTOs;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
        
    public bool IsValid() => 
        !string.IsNullOrWhiteSpace(Username) && 
        !string.IsNullOrWhiteSpace(Password);
}