using TrustSystem.Backend.Data;
using static DB_TV_PAL_PACKAGE;

namespace TrustSystem.Backend.Endpoints;

public static class PlanEndpoints
{
    public static RouteGroupBuilder MapPlanEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/plans")
                       .WithParameterValidation()
                       .WithTags("Plans");

        group.MapGet("/least-price", async (APPL_INS db) =>
        {
            var procedureResults = await db.CalculateTvAmount_Async(new CalculateTvAmountInputDTO
            {
                p_period = 7,
                p_age = 1,
                p_zone_code = "1",
                p_atr_type = 1,

                p_winter_flag = "0",
                p_pol_accedent_code = "0",
                p_add_f01 = 0

            }, new CalculateTvAmountInputOutputDTO { });

            double individualPremium = procedureResults.p_prem ?? 0;
            double groupPremium = individualPremium * 10;

            procedureResults = await db.CalculateTvAmount_Async(new CalculateTvAmountInputDTO
            {
                p_period = 180,
                p_age = 1,
                p_zone_code = "11",
                p_atr_type = 11,

                p_winter_flag = "0",
                p_pol_accedent_code = "0",
                p_add_f01 = 0

            }, new CalculateTvAmountInputOutputDTO { });
            double studentPremium = procedureResults.p_prem ?? 0;

            return Results.Ok(new { individualPremium, groupPremium, studentPremium });
        })
        .WithSummary("Get Least Price of Plans");

        return group;
    }
}