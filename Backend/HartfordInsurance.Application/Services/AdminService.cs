using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;

namespace HartfordInsurance.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IClaimRepository _claimRepository;

    public AdminService(IUserRepository userRepository,
                        IPlanRepository planRepository,
                        IPolicyRepository policyRepository,
                        IClaimRepository claimRepository)
    {
        _userRepository = userRepository;
        _planRepository = planRepository;
        _policyRepository = policyRepository;
        _claimRepository = claimRepository;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        return new AdminDashboardDto
        {
            TotalAgents        = await _userRepository.CountByRoleAsync(Role.Agent),
            TotalClaimsOfficers = await _userRepository.CountByRoleAsync(Role.ClaimsOfficer),
            TotalCustomers     = await _userRepository.CountByRoleAsync(Role.Customer),
            TotalPolicies      = await _policyRepository.CountAllAsync(),
            PendingClaims      = await _claimRepository.CountByStatusAsync(ClaimStatus.Pending),
            TotalRevenue       = await _policyRepository.GetTotalRevenueAsync()
        };
    }

    public Task<AdminDashboardDto> GetOverviewAsync() => GetDashboardAsync();

    public async Task CreateAgentAsync(AuthDto request)
    {
        var agent = new User
        {
            FullName     = request.FullName ?? "New Agent",
            Email        = request.Email,
            PasswordHash = request.Password,
            PhoneNumber  = request.PhoneNumber,
            Role         = Role.Agent
        };
        await _userRepository.AddAsync(agent);
    }

    public async Task CreateClaimsOfficerAsync(AuthDto request)
    {
        var officer = new User
        {
            FullName     = request.FullName ?? "New Officer",
            Email        = request.Email,
            PasswordHash = request.Password,
            PhoneNumber  = request.PhoneNumber,
            Role         = Role.ClaimsOfficer
        };
        await _userRepository.AddAsync(officer);
    }

    public async Task DeleteAgentAsync(int agentId)
    {
        var agent = await _userRepository.GetByIdAsync(agentId);
        if (agent == null || agent.Role != Role.Agent) throw new Exception("Agent not found.");
        await _userRepository.RemoveAsync(agent);
    }

    public async Task DeleteClaimsOfficerAsync(int officerId)
    {
        var officer = await _userRepository.GetByIdAsync(officerId);
        if (officer == null || officer.Role != Role.ClaimsOfficer)
            throw new KeyNotFoundException("Claims Officer not found.");

        // Prevent deleting officers that are still assigned to policies
        var assignedPolicies = await _policyRepository.GetPoliciesByClaimsOfficerAsync(officerId);
        if (assignedPolicies.Any())
            throw new ArgumentException("Cannot delete this Claims Officer because they are assigned to existing customer policies.");

        await _userRepository.RemoveAsync(officer);
    }

    public async Task<int> CreatePlanWithTierAsync(CreatePlanRequestDto request)
    {
        string imageUrl = string.Empty;
        if (request.Image != null && request.Image.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "docs");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"plan_{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await request.Image.CopyToAsync(stream);
            imageUrl = $"/docs/{fileName}";
        }

        var plan = new InsurancePlan
        {
            PlanName = request.PlanName,
            Description = request.Description,
            PlanType = request.PlanType,
            ImageUrl = imageUrl
        };
        var planId = await _planRepository.AddPlanAsync(plan);

        var tier = new PlanTier
        {
            PlanId = planId,
            TierName = request.TierName,
            BasePremium = request.BasePremium,
            BaseCoverageAmount = request.BaseCoverageAmount,
            AgeLockProtection = request.AgeLockProtection,
            CoverageRestoreEnabled = request.CoverageRestoreEnabled,
            MaxRestoresPerYear = request.MaxRestoresPerYear,
            BoosterMultiplier = request.BoosterMultiplier,
            PreExistingDiseaseWaitingMonths = request.PreExistingDiseaseWaitingMonths,
            CoPaymentPercentage = request.CoPaymentPercentage,
            CommissionPercentage = request.CommissionPercentage
        };
        await _planRepository.AddTierAsync(tier);

        return planId;
    }

    public async Task UpdatePlanAsync(int planId, PlanUpdateDto request)
    {
        var plan = await _planRepository.GetByIdAsync(planId);
        if (plan == null) throw new Exception("Plan not found");

        plan.PlanName = request.PlanName;
        plan.Description = request.Description;
        plan.PlanType = request.PlanType;
        if (request.DeleteImage)
        {
            plan.ImageUrl = string.Empty;
        }
        else if (request.Image != null && request.Image.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "docs");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"plan_{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await request.Image.CopyToAsync(stream);
            plan.ImageUrl = $"/docs/{fileName}";
        }

        await _planRepository.UpdatePlanAsync(plan);

        var tier = await _planRepository.GetTierByPlanIdAsync(planId);
        if (tier != null)
        {
            tier.TierName = request.TierName;
            tier.BasePremium = request.BasePremium;
            tier.BaseCoverageAmount = request.BaseCoverageAmount;
            tier.AgeLockProtection = request.AgeLockProtection;
            tier.CoverageRestoreEnabled = request.CoverageRestoreEnabled;
            tier.MaxRestoresPerYear = request.MaxRestoresPerYear;
            tier.BoosterMultiplier = request.BoosterMultiplier;
            tier.PreExistingDiseaseWaitingMonths = request.PreExistingDiseaseWaitingMonths;
            tier.CoPaymentPercentage = request.CoPaymentPercentage;
            tier.CommissionPercentage = request.CommissionPercentage;
            
            await _planRepository.UpdateTierAsync(tier);
        }
    }

    public async Task CreateTierAsync(PlanTierDto request)
    {
        var tier = new PlanTier
        {
            PlanId = request.PlanId,
            TierName = request.TierName,
            BasePremium = request.BasePremium,
            BaseCoverageAmount = request.CoverageLimit,
            AgeLockProtection = request.AgeLockProtection,
            CoverageRestoreEnabled = request.CoverageRestoreEnabled,
            MaxRestoresPerYear = request.MaxRestoresPerYear,
            BoosterMultiplier = request.BoosterMultiplier,
            PreExistingDiseaseWaitingMonths = request.PreExistingDiseaseWaitingMonths,
            CoPaymentPercentage = request.CoPaymentPercentage,
            CommissionPercentage = request.CommissionPercentage
        };
        await _planRepository.AddTierAsync(tier);
    }

    public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync()
        => await _userRepository.GetAgentPerformanceAsync();

    public async Task<List<ClaimsOfficerPerformanceDto>> GetClaimsOfficerPerformanceAsync()
        => await _userRepository.GetClaimsOfficerPerformanceAsync();

    public async Task<List<PlanPerformanceDto>> GetPlanPerformanceAsync()
        => await _policyRepository.GetPlanPerformanceAsync();

    public async Task<List<CustomerPerformanceDto>> GetCustomerPerformanceAsync()
        => await _userRepository.GetCustomerPerformanceAsync();
}