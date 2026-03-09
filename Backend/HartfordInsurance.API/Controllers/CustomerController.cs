using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/customer")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // Dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var customerId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _customerService.GetDashboardAsync(customerId));
    }

    // View Plans + Tiers
    [AllowAnonymous]
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
        => Ok(await _customerService.GetPlansWithTiersAsync());

    // Request Policy
    [HttpPost("request-policy")]
    public async Task<IActionResult> RequestPolicy([FromBody] RequestPolicyDto request)
    {
        request.CustomerId = int.Parse(User.FindFirstValue("userId")!);
        await _customerService.RequestPolicyAsync(request);
        return Ok("Policy request submitted for approval.");
    }

    // My Policies
    [HttpGet("policies")]
    public async Task<IActionResult> GetMyPolicies()
    {
        var customerId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _customerService.GetMyPoliciesAsync(customerId));
    }

    // Raise Claim
    [HttpPost("raise-claim")]
    public async Task<IActionResult> RaiseClaim([FromBody] ClaimDto request)
    {
        await _customerService.RaiseClaimAsync(request);
        return Ok("Claim submitted successfully.");
    }

    // My Claims
    [HttpGet("claims")]
    public async Task<IActionResult> GetMyClaims()
    {
        var customerId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _customerService.GetMyClaimsAsync(customerId));
    }

    // Upload Document
    [HttpPost("upload-document")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        var customerId = int.Parse(User.FindFirstValue("userId")!);
        var url = await _customerService.UploadDocumentAsync(file, customerId);
        return Ok(new { url });
    }

    // Make Payment
    [HttpPost("make-payment")]
    public async Task<IActionResult> MakePayment([FromBody] MakePaymentDto request)
    {
        var invoiceUrl = await _customerService.MakePaymentAsync(request);
        return Ok(new { invoiceUrl, message = "Payment successful." });
    }

    [HttpGet("policies/{policyId}")]
    public async Task<IActionResult> GetPolicy(int policyId)
    {
        var customerId = int.Parse(User.FindFirstValue("userId")!);
        return Ok(await _customerService.GetPolicyByPolicyIdAsync(policyId, customerId));
    }

    // Renew Policy
    [HttpPost("renew-policy/{policyId}")]
    public async Task<IActionResult> RenewPolicy(int policyId)
    {
        var customerId = int.Parse(User.FindFirstValue("userId")!);
        await _customerService.RenewPolicyAsync(policyId, customerId);
        return Ok("Policy successfully renewed for 1 year!");
    }
}