namespace NightmareRebalance;

internal sealed record HallNightmareProfile(string Id,int Level,int HpPercent,double Damage,string Advice);
internal static class HallNightmareRules
{
    internal static readonly HallNightmareProfile[] Profiles = {
        new("abomination",34,150,1.10,"Mixed pressure with a recovery turn. Prepare magical defense and healing."),
        new("stone-golem",55,140,1.15,"Heavy physical pressure and lighter magic. Alternate guarding and recovery."),
        new("golden-wolf",49,125,1.15,"Physical pressure with varied targeting. Protect the whole party.")
    };
    internal static HallNightmareProfile? Find(string id)=>Profiles.FirstOrDefault(x=>x.Id==id);
    internal static int Hp(int native,HallNightmareProfile p)=>Math.Clamp(native*p.HpPercent/100,1,32767);
    internal static int DamageAmount(int native,double factor)=>Math.Clamp((int)Math.Round(native*factor,MidpointRounding.AwayFromZero),0,9999);
    // Scores apply only after the existing original range, silence, target and use checks.
    internal static int Score(string id,int round,CombatAction action,IReadOnlyList<CombatTarget> targets)
    {
        bool recovery=round%3==2;
        int score=action.Power/4;
        if(id=="golden-wolf") score+=(action.Physical==(round%3!=1))?160:40;
        else if(recovery) score=-action.Power+(!action.Multi?260:0)+(!action.Physical?20:0);
        else score+=(round%2==0?!action.Physical:action.Physical)?180:30;
        if(action.Multi&&targets.Count(t=>t.Hp*3<t.MaxHp)>=2)score-=220;
        if(action.Status!=0&&!targets.Any(t=>t.StatusUseful))score-=300;
        return score;
    }
    internal static bool BonusEligible(bool nightmare,bool win,bool attempted,bool payoutStarted,bool empty)=>
        nightmare&&win&&!attempted&&!payoutStarted&&empty;
    internal const int StoneChancePercent=25;
}
