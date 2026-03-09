using HartfordInsurance.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HartfordInsurance.Infrastructure.Security;

public class JwtHelper : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryHours;

    public JwtHelper(IConfiguration config)
    {
        _secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _issuer = config["Jwt:Issuer"] ?? "HartfordInsurance";
        _audience = config["Jwt:Audience"] ?? "HartfordInsurance";
        _expiryHours = int.Parse(config["Jwt:ExpiryHours"] ?? "24");
    }

    public string GenerateToken(int userId, string email, string role, string fullName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, email),
            new System.Security.Claims.Claim(ClaimTypes.Role, role),
            new System.Security.Claims.Claim(ClaimTypes.Name, fullName),
            new System.Security.Claims.Claim("role", role),
            new System.Security.Claims.Claim("name", fullName),
            new System.Security.Claims.Claim("FullName", fullName),
            new System.Security.Claims.Claim("userId", userId.ToString())
        };
        var token = new JwtSecurityToken(_issuer, _audience, claims,
            expires: DateTime.UtcNow.AddHours(_expiryHours),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
