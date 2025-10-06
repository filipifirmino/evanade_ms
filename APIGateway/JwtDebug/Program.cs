using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var user = new { Id = "1", Username = "testuser", Role = "Admin" };

var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes("ThisIsAVeryLongSecretKeyForTestingPurposesOnly");

var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("userId", user.Id),
        new Claim("username", user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    }),
    Expires = DateTime.UtcNow.AddMinutes(1),
    Issuer = "TestIssuer",
    Audience = "TestAudience",
    SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature)
};

var token = tokenHandler.CreateToken(tokenDescriptor);
var tokenString = tokenHandler.WriteToken(token);

Console.WriteLine($"Token: {tokenString}");

var jwtToken = tokenHandler.ReadJwtToken(tokenString);
Console.WriteLine("Claims:");
foreach (var claim in jwtToken.Claims)
{
    Console.WriteLine($"  {claim.Type}: {claim.Value}");
}

Console.WriteLine($"ClaimTypes.Role = '{ClaimTypes.Role}'");