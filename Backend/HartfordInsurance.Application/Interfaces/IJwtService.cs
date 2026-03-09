using HartfordInsurance.Domain.Entities;

namespace HartfordInsurance.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string email, string role, string fullName);
}
