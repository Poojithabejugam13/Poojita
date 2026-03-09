using System.Collections.Generic;
using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace HartfordInsurance.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDashboardDto> GetDashboardAsync(int customerId);

    Task<List<PlanDto>> GetPlansWithTiersAsync();

    Task RequestPolicyAsync(RequestPolicyDto request);

    Task<List<CustomerPolicy>> GetMyPoliciesAsync(int customerId);

    Task RaiseClaimAsync(ClaimDto request);

    Task<List<ClaimDto>> GetMyClaimsAsync(int customerId);

    Task<string> UploadDocumentAsync(IFormFile file, int customerId);

    Task<string> MakePaymentAsync(MakePaymentDto request);

    Task RenewPolicyAsync(int policyId, int customerId);

    Task<PolicyDetailDto> GetPolicyByPolicyIdAsync(int policyId, int customerId);
}