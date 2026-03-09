using HartfordInsurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "ClaimsOfficer")]
[ApiController]
[Route("api/claimsofficer")]
public class ClaimsOfficerController : ControllerBase
{
    private readonly IClaimService _claimsService;

    public ClaimsOfficerController(IClaimService claimsService)
    {
        _claimsService = claimsService;
    }

    // Dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
        => Ok(await _claimsService.GetDashboardAsync());

    // Pending Claims
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingClaims()
        => Ok(await _claimsService.GetPendingClaimsAsync());

    // Approve Claim
    [HttpPut("{claimId}/approve")]
    public async Task<IActionResult> ApproveClaim(int claimId, [FromBody] HartfordInsurance.Application.DTOs.ApproveRequestDto? dto)
    {
        try
        {
            await _claimsService.ApproveClaimAsync(claimId, dto?.Reason);
            return Ok(new { message = "Claim approved successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Reject Claim
    [HttpPut("{claimId}/reject")]
    public async Task<IActionResult> RejectClaim(int claimId, [FromBody] HartfordInsurance.Application.DTOs.RejectRequestDto dto)
    {
        try
        {
            await _claimsService.RejectClaimAsync(claimId, dto.Reason);
            return Ok(new { message = "Claim rejected." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-customers")]
    public async Task<IActionResult> GetMyCustomers()
    {
        var officerId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _claimsService.GetMyCustomersAsync(officerId));
    }
}