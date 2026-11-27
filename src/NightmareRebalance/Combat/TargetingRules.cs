namespace NightmareRebalance;

internal readonly record struct TargetCandidate(int Slot,int Hp,int MaxHp,bool Player,bool Targetable);
internal static class TargetingRules
{
    internal static int PreferWounded(IEnumerable<TargetCandidate> candidates,int original,double threshold)
    {
        if(!double.IsFinite(threshold)||threshold is <=0 or >1)return original;
        var legal=candidates.Where(c=>c.Player&&c.Targetable&&c.Hp>0&&c.MaxHp>0&&c.Hp<=c.MaxHp).GroupBy(c=>c.Slot).Select(g=>g.First()).ToArray();
        if(!legal.Any(c=>c.Slot==original))return original;
        var wounded=legal.Where(c=>(double)c.Hp/c.MaxHp<=threshold).ToArray();if(wounded.Length==0)return original;
        double minimum=wounded.Min(c=>(double)c.Hp/c.MaxHp);
        if(wounded.Any(c=>c.Slot==original&&Math.Abs((double)c.Hp/c.MaxHp-minimum)<0.0000001))return original;
        return wounded.OrderBy(c=>(double)c.Hp/c.MaxHp).ThenBy(c=>c.Hp).ThenBy(c=>c.Slot).First().Slot;
    }
}
