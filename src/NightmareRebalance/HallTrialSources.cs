namespace NightmareRebalance;

internal static class HallTrialSources
{
    static readonly Lazy<Dictionary<string,FormationAudit>> sources=new(()=>
        ContentRules.Read<BossSourceAudit[]>(ContentRuntime.Resource("boss-resource-audit.json"))
            .Select(x=>x.Data)
            .Concat(ContentRules.Read<FormationAudit[]>(ContentRuntime.Resource("boss-revolution-audit.json")))
            .ToDictionary(x=>x.TableId));

    internal static FormationAudit Get(string tableId)=>sources.Value[tableId];
}
