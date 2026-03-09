using HartfordInsurance.Domain.Services;
using HartfordInsurance.Domain.Models;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Domain.Tests;

public class PremiumCalculatorTests
{
    // Height=170cm, Weight=65kg → BMI=22.5 (Normal range, 0% loading)
    private const decimal NormalHeightCm = 170m;
    private const decimal NormalWeightKg = 65m;

    [Fact]
    public void CalculatePremium_AgeUnder40_NoAgeLoading()
    {
        // base=5000, age=30, normal BMI, no smoking, 18% GST
        var result = PremiumCalculator.CalculatePremium(5000m, 30, NormalHeightCm, NormalWeightKg, false, 18m);

        // subtotal = 5000 (no age loading). tax = 5000 × 0.18 = 900. total = 5900
        result.AgeLoading.Should().Be(0m);
        result.TaxAmount.Should().Be(900m);
        result.TotalPremium.Should().Be(5900m);
    }

    [Fact]
    public void CalculatePremium_AgeOver40_AppliesAgeLoading()
    {
        // base=10000, age=45, normal BMI, no smoking, 18% GST
        var result = PremiumCalculator.CalculatePremium(10000m, 45, NormalHeightCm, NormalWeightKg, false, 18m);

        // ageLoading = 10000 × 0.10 = 1000. subtotal = 11000
        // tax = 11000 × 0.18 = 1980. total = 12980
        result.AgeLoading.Should().Be(1000m);
        result.TaxAmount.Should().Be(1980m);
        result.TotalPremium.Should().Be(12980m);
    }

    [Fact]
    public void CalculatePremium_AtAge40Boundary_NoLoading()
    {
        // Age loading only applies for age > 40 (exclusive), so age=40 should have none
        var result = PremiumCalculator.CalculatePremium(5000m, 40, NormalHeightCm, NormalWeightKg, false, 18m);

        result.AgeLoading.Should().Be(0m);
    }

    [Fact]
    public void CalculatePremium_ObeseBMI_AppliesBmiLoading()
    {
        // Height=170cm, Weight=100kg → BMI≈34.6 (Obese, 20% risk loading)
        var result = PremiumCalculator.CalculatePremium(5000m, 30, 170m, 100m, false, 18m);

        // subtotal = 5000. bmiLoading = 5000 × 0.20 = 1000. preTax = 6000.
        // tax = 6000 × 0.18 = 1080. total = 7080
        result.RiskLoading.Should().Be(1000m);
        result.TaxAmount.Should().Be(1080m);
        result.TotalPremium.Should().Be(7080m);
    }

    [Fact]
    public void CalculatePremium_Smoker_AppliesSmokingLoading()
    {
        // base=10000, age=30, normal BMI, smoker → 25% smoking surcharge, 18% GST
        var result = PremiumCalculator.CalculatePremium(10000m, 30, NormalHeightCm, NormalWeightKg, true, 18m);

        // subtotal = 10000. smokingLoading = 10000 × 0.25 = 2500. preTax = 12500.
        // tax = 12500 × 0.18 = 2250. total = 14750
        result.RiskLoading.Should().Be(2500m);
        result.TaxAmount.Should().Be(2250m);
        result.TotalPremium.Should().Be(14750m);
    }
}
