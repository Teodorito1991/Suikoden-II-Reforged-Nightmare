namespace NightmareRebalance;

internal sealed record HallNightmareProfile(string Id,int Level,int HpPercent,double Damage,string Advice,string RewardId="power-stone");
internal static class HallNightmareRules
{
    internal static readonly HallNightmareProfile[] Profiles = {
        new("abomination",34,150,1.10,"Mixed pressure with a recovery turn. Prepare magical defense and healing."),
        new("stone-golem",55,140,1.15,"Heavy physical pressure and lighter magic. Alternate guarding and recovery."),
        new("golden-wolf",49,125,1.15,"Physical pressure with varied targeting. Protect the whole party."),
        new("rat",40,135,1.10,"Prepare poison protection and alternate pressure with recovery.","stone-of-magic"),
        new("harpy",45,130,1.10,"Protect against status and keep recovery charges in reserve.","stone-of-speed"),
        new("sierra",47,130,1.10,"Magic protection matters. Save an emergency heal.","magic-protection-stone"),
        new("harpy-second",60,120,1.10,"A high-level status trial. Prepare protection and efficient group recovery.","stone-of-speed"),
        new("worm",44,130,1.10,"Respect its limited original special attack. Guard and recover.","magic-protection-stone"),
        new("bone-dragon",56,130,1.10,"Heavy blows alternate with legal native magic. Guard the front line."),
        new("celestial-sword",32,140,1.10,"Magical pressure with recovery opportunities.","stone-of-magic"),
        new("neclord-final",50,130,1.10,"Counter native magic and status. Keep the full party protected.","magic-protection-stone"),
        new("lucia-retinue",54,120,1.08,"Remove supporting soldiers efficiently, then concentrate on Lucia.","stone-of-speed"),
        new("lucia-highland",58,125,1.10,"Limited native magic stays limited. Prepare physical and magical defense.","stone-of-magic"),
        new("gorudo",59,120,1.08,"Five guards support the commander. Area attacks reward preparation."),
        new("seed-culgan",65,120,1.08,"An elite pair: focus one commander while defending against the other.","magic-protection-stone"),
        new("double-head",25,140,1.10,"Read the original phase changes and recover between attacks.","stone-of-speed"),
        new("mist-shade",10,150,1.10,"Original changing forms and restrained added pressure.","stone-of-magic"),
        new("lucia-castle",49,135,1.10,"A short physical duel. Respect the one native magic use.","stone-of-speed"),
        new("rowd-patrol",25,130,1.08,"Break the Highland line before committing to its commander.","magic-protection-stone")
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
