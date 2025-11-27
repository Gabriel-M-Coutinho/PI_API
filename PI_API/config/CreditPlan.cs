namespace PI_API.Config;

using PI_API.models;

public static class CreditPlans
{
    public static readonly Dictionary<CreditPlan, (double Price, int Credits)> Plans =
        new Dictionary<CreditPlan, (double, int)>
        {
            { CreditPlan.Basic, (19.90, 800) },
            { CreditPlan.Intermediario, (39.90, 1800) },
            { CreditPlan.Plus, (79.90, 3700 ) }
        };
}