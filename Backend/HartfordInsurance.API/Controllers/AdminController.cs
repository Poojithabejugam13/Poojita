using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // Dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
        => Ok(await _adminService.GetOverviewAsync());

    // Create Agent
    [HttpPost("agents")]
    public async Task<IActionResult> CreateAgent([FromBody] AuthDto request)
    {
        await _adminService.CreateAgentAsync(request);
        return Ok("Agent created successfully.");
    }

    // Delete Agent
    [HttpDelete("agents/{id}")]
    public async Task<IActionResult> DeleteAgent(int id)
    {
        await _adminService.DeleteAgentAsync(id);
        return Ok(new { message = "Agent deleted successfully." });
    }

    // Create Claims Officer
    [HttpPost("claimsofficers")]
    public async Task<IActionResult> CreateClaimsOfficer([FromBody] AuthDto request)
    {
        await _adminService.CreateClaimsOfficerAsync(request);
        return Ok("Claims officer created successfully.");
    }

    // Delete Claims Officer
    [HttpDelete("claimsofficers/{id}")]
    public async Task<IActionResult> DeleteClaimsOfficer(int id)
    {
        await _adminService.DeleteClaimsOfficerAsync(id);
        return Ok(new { message = "Claims officer deleted successfully." });
    }

    // Create Plan
    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromForm] CreatePlanRequestDto request)
    {
        var planId = await _adminService.CreatePlanWithTierAsync(request);
        return Ok(planId);
    }

    // Update Plan
    [HttpPut("plans/{id}")]
    public async Task<IActionResult> UpdatePlan(int id, [FromForm] PlanUpdateDto request)
    {
        await _adminService.UpdatePlanAsync(id, request);
        return Ok("Plan updated successfully.");
    }

    // Create Tier
    [HttpPost("tiers")]
    public async Task<IActionResult> CreateTier([FromBody] PlanTierDto request)
    {
        await _adminService.CreateTierAsync(request);
        return Ok("Tier created successfully.");
    }

    // Agent Performance
    [HttpGet("agent-performance")]
    public async Task<IActionResult> GetAgentPerformance()
        => Ok(await _adminService.GetAgentPerformanceAsync());

    // Claims Officer Performance
    [HttpGet("officer-performance")]
    public async Task<IActionResult> GetClaimsOfficerPerformance()
        => Ok(await _adminService.GetClaimsOfficerPerformanceAsync());

    // Plan Performance
    [HttpGet("plan-performance")]
    public async Task<IActionResult> GetPlanPerformance()
        => Ok(await _adminService.GetPlanPerformanceAsync());
    // Customer Performance
    [HttpGet("customer-performance")]
    public async Task<IActionResult> GetCustomerPerformance()
        => Ok(await _adminService.GetCustomerPerformanceAsync());
}