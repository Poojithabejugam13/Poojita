namespace HartfordInsurance.Application.DTOs;

// Used by CustomerController.MakePayment endpoint
public class MakePaymentDto
{
    public int PolicyId { get; set; }
    public decimal Amount { get; set; }
}
