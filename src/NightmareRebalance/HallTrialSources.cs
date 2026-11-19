namespace NightmareRebalance;

internal static class HallTrialSources
{
    static readonly Lazy<Dictionary<string,FormationAudit>> sources=new(()=>
        ContentRules.Read<BossSourceAudit[]>(ContentRuntime.Resource("boss-resource-audit.json"))
            .Select(x=>x.Data)
            .Concat(ContentRules.Read<FormationAudit[]>(ContentRuntime.Resource("boss-revolution-audit.json")))
            .Concat(ContentRules.Read<BossSourceAudit[]>(ContentRuntime.Resource("legendary-boss-audit.json")).Select(x=>x.Data))
            .Concat(ContentRules.Read<FormationAudit[]>(ContentRuntime.Resource("hall-bulk-source-audit.json")))
            .GroupBy(x=>x.TableId).ToDictionary(x=>x.Key,x=>x.First()));

    internal static FormationAudit Get(string tableId)=>sources.Value[tableId];
}
