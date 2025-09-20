namespace Sales.BFF.ApplicationCore.Domain.Entities;

public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
        
    public bool IsValid() => !string.IsNullOrEmpty(Username);
}