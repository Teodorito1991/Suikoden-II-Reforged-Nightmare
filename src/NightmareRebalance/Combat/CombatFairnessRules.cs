namespace NightmareRebalance;

// Inspected GSD2 calculations, deliberately separate from the fairness policy.
// These are not replacements for native combat. Native comparisons are required
// before adding further formula branches to a released runtime feature.
internal static class CombatFairnessRules
{
    internal static int CriticalThreshold(int technique,int luck,bool criticalRune=false)
    {
        int chance=Math.Clamp((Math.Max(0,technique)+Math.Max(0,luck)/2)/16,3,25);
        return Math.Clamp(criticalRune?chance*3/2:chance,0,100);
    }
    // get_rand(99) produces 0..98; not a percentage roll out of 100.
    internal static bool Critical(int threshold,int nativeRoll)=>nativeRoll>=0&&nativeRoll<99&&nativeRoll<threshold;
    internal static int EnemyInitiative(int speed,int nativeRoll,int actionCountCode=0)
    {
        speed=Math.Max(0,speed);int radius=speed/10;
        int n=speed+radius-(radius==0?0:Math.Clamp(nativeRoll,0,radius*2-1));
        return actionCountCode==1?n*90/100:actionCountCode==2?n*80/100:n;
    }
    internal static int PlayerInitiative(int speed,bool nativeHaste)=>Math.Max(0,speed)*(nativeHaste?2:1);
    // Classification is a review heuristic. It is never evidence of fairness.
    internal static string Review(int runs,int victories,int stalls,double rounds,double hpFractionRemaining,double healingCharges)
    {
        if(runs<=0||victories<0||stalls<0||victories+stalls>runs||!double.IsFinite(rounds)||!double.IsFinite(hpFractionRemaining)||!double.IsFinite(healingCharges))return "Invalid";
        double win=(double)victories/runs;
        if(stalls>runs*.1)return "StallReview";
        if(win<.35)return "OverwhelmingReview";
        if(win<.9||hpFractionRemaining<.25)return "Dangerous";
        if(rounds<=2&&hpFractionRemaining>=.8&&healingCharges<.5)return "Easy";
        return "Challenging";
    }
}
