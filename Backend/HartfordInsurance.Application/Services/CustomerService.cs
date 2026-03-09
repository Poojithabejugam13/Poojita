using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Domain.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;

namespace HartfordInsurance.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IPlanRepository _planRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IUserRepository _userRepository;

    public CustomerService(IPlanRepository planRepository,
                           IPolicyRepository policyRepository,
                           IClaimRepository claimRepository,
                           IUserRepository userRepository)
    {
        _planRepository = planRepository;
        _policyRepository = policyRepository;
        _claimRepository = claimRepository;
        _userRepository = userRepository;
    }

    public async Task<CustomerDashboardDto> GetDashboardAsync(int customerId)
    {
        var activePolicies = await _policyRepository.GetPoliciesByCustomerAsync(customerId);
        var mainPolicy = activePolicies.OrderByDescending(p => p.IssueDate).FirstOrDefault();
        
        string agentName = "Not Assigned";
        string agentPhone = "N/A";
        string officerName = "Not Assigned";
        string officerPhone = "N/A";

        if (mainPolicy != null)
        {
            var agent = await _userRepository.GetByIdAsync(mainPolicy.AgentId);
            if (agent != null)
            {
                agentName = agent.FullName;
                agentPhone = agent.PhoneNumber ?? "N/A";
            }

            var officer = await _userRepository.GetByIdAsync(mainPolicy.ClaimsOfficerId);
            if (officer != null)
            {
                officerName = officer.FullName;
                officerPhone = officer.PhoneNumber ?? "N/A";
            }
        }
        else
        {
            var allAgents = await _userRepository.GetUsersByRoleAsync(Role.Agent);
            if (allAgents.Any())
            {
                int roundRobinIndex = customerId % allAgents.Count;
                var assignedAgent = allAgents[roundRobinIndex];
                agentName = assignedAgent.FullName;
                agentPhone = assignedAgent.PhoneNumber ?? "N/A";
            }

            var allOfficers = await _userRepository.GetUsersByRoleAsync(Role.ClaimsOfficer);
            if (allOfficers.Any())
            {
                int roundRobinIndex = customerId % allOfficers.Count;
                var assignedOfficer = allOfficers[roundRobinIndex];
                officerName = assignedOfficer.FullName;
                officerPhone = assignedOfficer.PhoneNumber ?? "N/A";
            }
        }

        return new CustomerDashboardDto
        {
            ActivePolicies = await _policyRepository.CountActivePoliciesAsync(customerId),
            PendingClaims = await _claimRepository.CountPendingClaimsAsync(customerId),
            TotalCoverageAmount = await _policyRepository.GetTotalCoverageAsync(customerId),
            AssignedAgentName = agentName,
            AssignedAgentPhone = agentPhone,
            AssignedOfficerName = officerName,
            AssignedOfficerPhone = officerPhone
        };
    }

    public async Task<List<PlanDto>> GetPlansWithTiersAsync()
        => await _planRepository.GetPlansWithTiersAsync();

    public async Task RequestPolicyAsync(RequestPolicyDto request)
    {
        var tier = await _planRepository.GetTierByIdAsync(request.TierId);

        if (tier == null || tier.PlanId != request.PlanId)
            throw new Exception("Selected tier does not belong to this plan.");

        // Assign a Claims Officer (round-robin, stable per customer)
        var officers = await _userRepository.GetUsersByRoleAsync(Role.ClaimsOfficer);
        int officerId = 0;

        if (officers.Any())
        {
            var existingPolicies = await _policyRepository.GetPoliciesByCustomerAsync(request.CustomerId);
            var lastPolicy = existingPolicies.OrderByDescending(p => p.IssueDate).FirstOrDefault();

            if (lastPolicy != null && lastPolicy.ClaimsOfficerId > 0)
            {
                officerId = lastPolicy.ClaimsOfficerId;
            }
            else
            {
                int roundRobinIndex = request.CustomerId % officers.Count;
                officerId = officers[roundRobinIndex].Id;
            }
        }

        // Assign an Agent
        if (request.AgentId <= 0)
        {
            var existingPolicies = await _policyRepository.GetPoliciesByCustomerAsync(request.CustomerId);
            var lastPolicy = existingPolicies.OrderByDescending(p => p.IssueDate).FirstOrDefault();

            if (lastPolicy != null && lastPolicy.AgentId > 0)
            {
                // Customer has bought before, maintain the same Agent
                request.AgentId = lastPolicy.AgentId;
            }
            else
            {
                // Customer's first policy -> Round Robin Assignment
                var allAgents = await _userRepository.GetUsersByRoleAsync(Role.Agent);
                if (allAgents.Any())
                {
                    int roundRobinIndex = request.CustomerId % allAgents.Count;
                    request.AgentId = allAgents[roundRobinIndex].Id;
                }
            }
        }

        var agent = await _userRepository.GetByIdAsync(request.AgentId)
            ?? throw new Exception("No agents available for assignment.");

        var premium = PremiumCalculator.CalculatePremium(
            tier.BasePremium,
            request.Age,
            request.HeightCm,
            request.WeightKg,
            request.IsSmoker,
            18m); // 18% GST

        decimal commissionAmount = 0;
        decimal commissionPercentage = 0;

        if (agent.Role == Role.Agent)
        {
            commissionPercentage = tier.CommissionPercentage;
            commissionAmount = premium.TotalPremium * (tier.CommissionPercentage / 100);
        }

        var policy = new CustomerPolicy
        {
            PlanId = request.PlanId,
            TierId = request.TierId,
            AgentId = request.AgentId,
            CustomerId = request.CustomerId,
            ClaimsOfficerId = officerId,
            EntryAge = request.Age,
            BasePremium = tier.BasePremium,
            AgeLoading = premium.AgeLoading,
            TaxAmount = premium.TaxAmount,
            TotalPremium = premium.TotalPremium,
            CommissionPercentage = commissionPercentage,
            CommissionAmount = commissionAmount,
            AnnualMaxCoverage = tier.BaseCoverageAmount,
            RemainingCoverageAmount = tier.BaseCoverageAmount,
            NomineeName = request.Nominees?.FirstOrDefault()?.NomineeName ?? string.Empty,
            NomineeRelation = request.Nominees?.FirstOrDefault()?.Relationship ?? string.Empty,
            IssueDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            Status = PolicyStatus.PendingApproval,
            
            // Simplified Flow Fields
            PlanType = request.PlanType,
            PolicyStartDate = request.PolicyStartDate,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MobileNumber = request.MobileNumber,
            Address = request.Address,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            IsSmoker = request.IsSmoker,
            PreExistingDiseases = request.PreExistingDiseases
        };

        await _policyRepository.AddAsync(policy);

        if (request.Nominees != null && request.Nominees.Any())
        {
            foreach (var nom in request.Nominees)
            {
                var nominee = new Nominee
                {
                    PolicyId = policy.PolicyId,
                    NomineeName = nom.NomineeName,
                    Relationship = nom.Relationship,
                    PercentageShare = nom.PercentageShare,
                    NomineeAge = 30 // Defaulting as UI doesn't collect Age
                };
                await _policyRepository.AddNomineeAsync(nominee);
            }
        }

        if (!string.IsNullOrEmpty(request.DocumentUrl))
        {
            var doc = new Document
            {
                PolicyId = policy.PolicyId,
                DocumentType = "Policy_Proof",
                FilePath = request.DocumentUrl
            };
            await _claimRepository.AddDocumentAsync(doc);
        }
    }

    public async Task<List<CustomerPolicy>> GetMyPoliciesAsync(int customerId)
        => await _policyRepository.GetPoliciesByCustomerAsync(customerId);

    public async Task RaiseClaimAsync(ClaimDto request)
    {
        var claim = new Claim
        {
            PolicyId    = request.PolicyId,
            ClaimAmount = request.ClaimAmount,
            ClaimReason = request.Reason,
            Status      = ClaimStatus.Pending
        };
        await _claimRepository.AddAsync(claim);

        if (!string.IsNullOrEmpty(request.DocumentUrl))
        {
            var doc = new Document
            {
                ClaimId = claim.ClaimId,
                PolicyId = claim.PolicyId,
                DocumentType = "Claim_Evidence",
                FilePath = request.DocumentUrl
            };
            await _claimRepository.AddDocumentAsync(doc);
        }
    }

    public async Task<List<ClaimDto>> GetMyClaimsAsync(int customerId)
    {
        var dtos = await _claimRepository.GetClaimsByCustomerAsync(customerId);
        foreach (var dto in dtos)
        {
            var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);
            if (policy == null) continue;

            var tier = await _planRepository.GetTierByIdAsync(policy.TierId);
            if (tier == null) continue;

            DateTime eligibleDate = policy.IssueDate.AddMonths(tier.PreExistingDiseaseWaitingMonths);
            if (dto.CreatedAt < eligibleDate)
            {
                dto.IsPedWaitingViolated = true;
            }
        }
        return dtos;
    }

    public async Task<string> UploadDocumentAsync(IFormFile file, int customerId)
    {
        if (file == null || file.Length == 0)
            throw new Exception("No file provided.");

        var uploadsFolder = Path.Combine("wwwroot", "docs");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{customerId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        await _claimRepository.AddDocumentAsync(new Document
        {
            DocumentType = file.ContentType,
            FilePath     = filePath
        });

        return $"/docs/{fileName}";
    }

    public async Task<string> MakePaymentAsync(MakePaymentDto request)
    {
        var policy = await _policyRepository.GetByIdAsync(request.PolicyId)
            ?? throw new KeyNotFoundException("Policy not found.");
        
        var plan = await _planRepository.GetByIdAsync(policy.PlanId);
        var tier = await _planRepository.GetTierByIdAsync(policy.TierId);
        var customer = await _userRepository.GetByIdAsync(policy.CustomerId);

        var payment = new Payment
        {
            PolicyId      = request.PolicyId,
            AmountPaid    = request.Amount,
            PaymentStatus = PaymentStatus.Paid
        };

        await _policyRepository.AddPaymentAsync(payment);
        await _policyRepository.ActivatePolicyAsync(request.PolicyId);

        // Calculate Breakdown for Invoice (Itemized)
        decimal monthlyBase = policy.BasePremium / 12;
        decimal monthlyLoading = policy.AgeLoading / 12;
        decimal subtotal = monthlyBase + monthlyLoading;
        decimal gst = subtotal * 0.18m;
        decimal total = subtotal + gst;

        string invoiceNumber = $"INV-{request.PolicyId}-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        // Generate Professional Invoice Document
        string invoiceHtml = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: 'Segoe UI', Arial, sans-serif; color: #333; line-height: 1.6; margin: 0; padding: 40px; background-color: #f9f9f9; }}
                .invoice-box {{ max-width: 800px; margin: auto; padding: 40px; border-radius: 12px; background: #fff; box-shadow: 0 4px 20px rgba(0,0,0,0.05); }}
                .header {{ display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #4B2E83; padding-bottom: 20px; margin-bottom: 30px; }}
                .logo {{ color: #4B2E83; font-size: 28px; font-weight: 800; letter-spacing: -1px; }}
                .status-badge {{ background: #e6f4ea; color: #1e7e34; padding: 6px 12px; border-radius: 20px; font-weight: 700; font-size: 12px; text-transform: uppercase; }}
                .details-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 40px; margin-bottom: 40px; }}
                .section-header {{ font-size: 12px; font-weight: 700; color: #999; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 8px; }}
                .info-text {{ font-size: 15px; font-weight: 600; color: #111; }}
                table {{ width: 100%; border-collapse: collapse; margin-bottom: 30px; }}
                th {{ text-align: left; padding: 12px; border-bottom: 1px solid #eee; font-size: 13px; color: #666; }}
                td {{ padding: 16px 12px; border-bottom: 1px solid #f8f8f8; font-size: 14px; }}
                .amount-col {{ text-align: right; }}
                .summary {{ width: 300px; margin-left: auto; }}
                .summary-row {{ display: flex; justify-content: space-between; padding: 6px 0; font-size: 14px; }}
                .total-row {{ display: flex; justify-content: space-between; padding: 12px 0; border-top: 2px solid #4B2E83; margin-top: 10px; font-size: 18px; font-weight: 800; color: #4B2E83; }}
                .footer {{ text-align: center; margin-top: 50px; font-size: 12px; color: #999; }}
            </style>
        </head>
        <body>
            <div class='invoice-box'>
                <div class='header'>
                    <div class='logo'>HARTFORD <span style='font-weight:300; color:#555;'>INSURANCE</span></div>
                    <div class='status-badge'>Payment Successful</div>
                </div>

                <div class='details-grid'>
                    <div>
                        <div class='section-header'>Billed To</div>
                        <div class='info-text'>{customer?.FullName ?? "Valued Customer"}</div>
                        <div class='info-text' style='font-weight:400; color:#666;'>Policy: {plan?.PlanName}</div>
                        <div class='info-text' style='font-weight:400; color:#666;'>Tier: {tier?.TierName}</div>
                    </div>
                    <div style='text-align: right;'>
                        <div class='section-header'>Invoice Details</div>
                        <div class='info-text'>{invoiceNumber}</div>
                        <div class='info-text' style='font-weight:400; color:#666;'>Date: {DateTime.UtcNow:MMMM dd, yyyy}</div>
                        <div class='info-text' style='font-weight:400; color:#666;'>Policy ID: POL-{request.PolicyId}</div>
                    </div>
                </div>

                <table>
                    <thead>
                        <tr>
                            <th>Description</th>
                            <th class='amount-col'>Amount</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>
                                <strong>Basic Health Coverage Monthly Premium</strong><br/>
                                <span style='font-size: 12px; color:#999;'>Standard rate for {plan?.PlanName} - {tier?.TierName} tier</span>
                            </td>
                            <td class='amount-col'>{monthlyBase:C}</td>
                        </tr>
                        <tr>
                            <td>
                                <strong>Age Loading Factor</strong><br/>
                                <span style='font-size: 12px; color:#999;'>Adjusted based on entry age: {policy.EntryAge}</span>
                            </td>
                            <td class='amount-col'>{monthlyLoading:C}</td>
                        </tr>
                    </tbody>
                </table>

                <div class='summary'>
                    <div class='summary-row'>
                        <span>Subtotal</span>
                        <span>{subtotal:C}</span>
                    </div>
                    <div class='summary-row'>
                        <span>GST (18%)</span>
                        <span>{gst:C}</span>
                    </div>
                    <div class='total-row'>
                        <span>Total Paid</span>
                        <span>{total:C}</span>
                    </div>
                </div>

                <div class='footer'>
                    <p>This is a computer-generated invoice and does not require a physical signature.</p>
                    <p>&copy; {DateTime.UtcNow.Year} Hartford Health Insurance. Protected by Global Security Standards.</p>
                </div>
            </div>
        </body>
        </html>";

        var uploadsFolder = Path.Combine("wwwroot", "docs");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"invoice_pol{request.PolicyId}_{Guid.NewGuid()}.html";
        var filePath = Path.Combine(uploadsFolder, fileName);
        await File.WriteAllTextAsync(filePath, invoiceHtml);

        var invoiceUrl = $"/docs/{fileName}";

        await _claimRepository.AddDocumentAsync(new Document
        {
            PolicyId = request.PolicyId,
            DocumentType = "Payment_Invoice",
            FilePath = invoiceUrl
        });

        return invoiceUrl;
    }

    public async Task RenewPolicyAsync(int policyId, int customerId)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId);
        if (policy == null || policy.CustomerId != customerId)
            throw new Exception("Policy not found or unauthorized.");

        if (policy.Status == PolicyStatus.PendingApproval || policy.Status == PolicyStatus.Rejected)
            throw new Exception("Cannot renew a pending or rejected policy.");

        var tier = await _planRepository.GetTierByIdAsync(policy.TierId);
        if (tier == null) throw new Exception("Tier information missing.");

        // Recalculate Premium — respecting AgeLockProtection
        int currentAge = policy.EntryAge + (DateTime.UtcNow.Year - policy.CreatedAt.Year);
        
        // If AgeLock is enabled: use the original entry age (premium never increases due to aging)
        // If AgeLock is disabled: use the current age (premium increases if customer crosses age 40+)
        int ageForPremium = tier.AgeLockProtection ? policy.EntryAge : currentAge;

        var premium = PremiumCalculator.CalculatePremium(
            tier.BasePremium,
            ageForPremium,
            policy.HeightCm,
            policy.WeightKg,
            policy.IsSmoker,
            18m); // 18% GST

        // Coverage Booster Logic (Applied to AnnualMaxCoverage)
        var claimsInLastYear = await _claimRepository.GetClaimsByPolicyAsync(policyId);
        bool noClaims = !claimsInLastYear.Any(c => c.CreatedAt > DateTime.UtcNow.AddYears(-1) && c.Status == ClaimStatus.Approved);

        policy.AnnualMaxCoverage = CoverageManager.ApplyBooster(policy.AnnualMaxCoverage, tier.BaseCoverageAmount, tier.BoosterMultiplier, noClaims);
        
        // Coverage Refill - Remaining coverage becomes the new annual max
        policy.RemainingCoverageAmount = policy.AnnualMaxCoverage;
        
        // Reset Restores on renewal
        policy.RestoresUsedThisYear = 0; 

        // Update Policy metadata
        policy.BasePremium = tier.BasePremium;
        policy.AgeLoading = premium.AgeLoading;
        policy.TaxAmount = premium.TaxAmount;
        policy.TotalPremium = premium.TotalPremium;
        policy.ExpiryDate = (policy.ExpiryDate > DateTime.UtcNow ? policy.ExpiryDate : DateTime.UtcNow).AddYears(1);
        policy.Status = PolicyStatus.PendingApproval;

        await _policyRepository.UpdateAsync(policy);
    }

    public async Task<PolicyDetailDto> GetPolicyByPolicyIdAsync(int policyId, int customerId)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId);
        if (policy == null || policy.CustomerId != customerId)
            throw new Exception("Policy not found or unauthorized.");

        var nominees = await _policyRepository.GetNomineesByPolicyIdAsync(policyId);

        return new PolicyDetailDto
        {
            Policy = policy,
            Nominees = nominees.Select(n => new NomineeDto
            {
                NomineeName = n.NomineeName,
                Relationship = n.Relationship,
                PercentageShare = n.PercentageShare
            }).ToList()
        };
    }
}