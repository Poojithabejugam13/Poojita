using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HartfordInsurance.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountByRoleAsync(Role role)
        => await _context.Users.CountAsync(u => u.Role == role);

    public async Task<List<User>> GetUsersByRoleAsync(Role role)
        => await _context.Users.Where(u => u.Role == role).ToListAsync();

    public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync()
        => await (
            from u in _context.Users
            where u.Role == Role.Agent
            join p in _context.CustomerPolicies on u.Id equals p.AgentId into policies
            from pol in policies.DefaultIfEmpty()
            group pol by new { u.Id, u.FullName } into g
            select new AgentPerformanceDto
            {
                AgentId = g.Key.Id,
                AgentName = g.Key.FullName,
                PoliciesSold = g.Count(x => x != null),
                TotalCommissionEarned = g.Sum(x => x != null ? x.CommissionAmount : 0)
            }
        ).ToListAsync();

    public async Task<List<ClaimsOfficerPerformanceDto>> GetClaimsOfficerPerformanceAsync()
        => await (
            from u in _context.Users
            where u.Role == Role.ClaimsOfficer
            // Link to claims via the policies assigned to the officer
            join pol in _context.CustomerPolicies on u.Id equals pol.ClaimsOfficerId into policies
            from p in policies.DefaultIfEmpty()
            join clm in _context.Claims on p.PolicyId equals clm.PolicyId into claims
            from c in claims.DefaultIfEmpty()
            group c by new { u.Id, u.FullName } into g
            select new ClaimsOfficerPerformanceDto
            {
                ClaimsOfficerId = g.Key.Id,
                ClaimsOfficerName = g.Key.FullName,
                ApprovedClaims = g.Count(x => x != null && x.Status == ClaimStatus.Approved),
                RejectedClaims = g.Count(x => x != null && x.Status == ClaimStatus.Rejected),
                TotalClaimsProcessed = g.Count(x => x != null && (x.Status == ClaimStatus.Approved || x.Status == ClaimStatus.Rejected))
            }
        ).ToListAsync();

    public async Task<List<CustomerPerformanceDto>> GetCustomerPerformanceAsync()
        => await (
            from u in _context.Users
            where u.Role == Role.Customer
            join pol in _context.CustomerPolicies on u.Id equals pol.CustomerId into policies
            from p in policies.DefaultIfEmpty()
            join clm in _context.Claims on p.PolicyId equals clm.PolicyId into claims
            from c in claims.DefaultIfEmpty()
            group new { p, c } by new { u.Id, u.FullName, u.Email } into g
            select new CustomerPerformanceDto
            {
                CustomerId = g.Key.Id,
                CustomerName = g.Key.FullName,
                Email = g.Key.Email,
                ActivePolicies = g.Where(x => x.p != null && x.p.Status == PolicyStatus.Active).Select(x => x.p.PolicyId).Distinct().Count(),
                TotalClaims = g.Where(x => x.c != null).Select(x => x.c.ClaimId).Distinct().Count()
            }
        ).ToListAsync();
}