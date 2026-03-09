namespace HartfordInsurance.Domain.Services;
public static class CoverageManager
{
    public static decimal DeductCoverage(
        decimal currentCoverage,
        decimal approvedClaimAmount)
    {
        if (approvedClaimAmount > currentCoverage)
        {
            throw new Exception("Coverage exceeded.");
        }
        return currentCoverage - approvedClaimAmount;
    }
    public static bool CanRestore(
        bool restoreEnabled,
        int restoresUsed,
        int maxRestoresPerYear)
    {
        if (!restoreEnabled)
            return false;
        return restoresUsed < maxRestoresPerYear;
    }
    public static decimal ApplyBooster(
        decimal currentCoverage,
        decimal baseCoverage,
        int boosterMultiplier,
        bool noClaimThisYear)
    {
        if (!noClaimThisYear || boosterMultiplier <= 0)
            return currentCoverage;
            
        decimal bonus = baseCoverage * ((decimal)boosterMultiplier / 100);
        return currentCoverage + bonus;
    }
}