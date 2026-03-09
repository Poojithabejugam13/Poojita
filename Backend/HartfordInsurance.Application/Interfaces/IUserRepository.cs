using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;

namespace HartfordInsurance.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(int id);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task RemoveAsync(User user);

    Task<int> CountByRoleAsync(Role role);

    Task<List<User>> GetUsersByRoleAsync(Role role);

    Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync();
    Task<List<ClaimsOfficerPerformanceDto>> GetClaimsOfficerPerformanceAsync();
    Task<List<CustomerPerformanceDto>> GetCustomerPerformanceAsync();
}