using HartfordInsurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "Agent")]
[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService)
    {
        _agentService = agentService;
    }
    // Dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var agentId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _agentService.GetDashboardAsync(agentId));
    }

    // Pending Policy Requests
    [HttpGet("pending-requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var agentId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _agentService.GetPendingPolicyRequestsAsync(agentId));
    }

    // Approve Policy
    [HttpPut("{policyId}/approve")]
    public async Task<IActionResult> ApprovePolicy(int policyId, [FromBody] HartfordInsurance.Application.DTOs.ApproveRequestDto dto)
    {
        await _agentService.ApprovePolicyAsync(policyId, dto.Reason);
        return Ok(new { message = "Policy approved successfully." });
    }

    // Reject Policy
    [HttpPut("{policyId}/reject")]
    public async Task<IActionResult> RejectPolicy(int policyId, [FromBody] HartfordInsurance.Application.DTOs.RejectRequestDto dto)
    {
        await _agentService.RejectPolicyAsync(policyId, dto.Reason);
        return Ok(new { message = "Policy rejected." });
    }

    // Update Status
    [HttpPut("{policyId}/status")]
    public async Task<IActionResult> UpdateStatus(int policyId, [FromBody] HartfordInsurance.Domain.Enums.PolicyStatus status)
    {
        await _agentService.UpdatePolicyStatusAsync(policyId, status);
        return Ok(new { message = "Policy status updated successfully." });
    }

    // Commission Details
    [HttpGet("commission")]
    public async Task<IActionResult> GetCommission()
    {
        var agentId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _agentService.GetCommissionDetailsAsync(agentId));
    }

    // Sold Policies
    [HttpGet("sold-policies")]
    public async Task<IActionResult> GetSoldPolicies()
    {
        var agentId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _agentService.GetSoldPoliciesAsync(agentId));
    }

    [HttpGet("my-customers")]
    public async Task<IActionResult> GetMyCustomers()
    {
        var agentId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _agentService.GetMyCustomersAsync(agentId));
    }

    [HttpPut("{policyId}/expire")]
    public async Task<IActionResult> ExpirePolicy(int policyId)
    {
        await _agentService.ExpirePolicyAsync(policyId);
        return Ok(new { message = "Policy expired successfully." });
    }

    [HttpPut("{policyId}/cancel")]
    public async Task<IActionResult> CancelPolicy(int policyId)
    {
        await _agentService.CancelPolicyAsync(policyId);
        return Ok(new { message = "Policy cancelled successfully." });
    }
}