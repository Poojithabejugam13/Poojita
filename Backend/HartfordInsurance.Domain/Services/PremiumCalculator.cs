namespace HartfordInsurance.Domain.Services;

using HartfordInsurance.Domain.Models;
public static class PremiumCalculator
{
    public static PremiumResult CalculatePremium(
        decimal basePremium,
        int age,
        decimal heightCm,
        decimal weightKg,
        bool isSmoker,
        decimal taxPercentage)
    {
        // 1. Age Loading: If Age > 40 -> +10% of base premium
        decimal ageLoading = (age > 40) ? (basePremium * 0.10m) : 0;
        
        decimal subtotal = basePremium + ageLoading;

        // 2. BMI Loading
        // BMI = weight / (height in meters²)
        decimal heightInMeters = heightCm / 100m;
        decimal bmi = (heightInMeters > 0) ? (weightKg / (heightInMeters * heightInMeters)) : 0;
        
        decimal bmiFactor = 0;
        if (bmi < 18.5m) bmiFactor = 0.05m;
        else if (bmi >= 18.5m && bmi <= 24.9m) bmiFactor = 0;
        else if (bmi >= 25m && bmi <= 29.9m) bmiFactor = 0.10m;
        else if (bmi >= 30m) bmiFactor = 0.20m;

        // 3. Smoking Loading: If Smoker -> +25%
        decimal smokingFactor = isSmoker ? 0.25m : 0;

        // 4. Final Calculation
        // riskLoading = subtotal * bmiFactor + subtotal * smokingFactor
        decimal riskLoading = (subtotal * bmiFactor) + (subtotal * smokingFactor);

        decimal premiumAfterRisk = subtotal + riskLoading;
        decimal taxAmount = premiumAfterRisk * (taxPercentage / 100);
        decimal totalPremium = premiumAfterRisk + taxAmount;

        return new PremiumResult
        {
            TotalPremium = totalPremium,
            TaxAmount = taxAmount,
            AgeLoading = ageLoading,
            RiskLoading = riskLoading
        };
    }
}