using System.Text.Json;
namespace NightmareRebalance;
internal sealed class WarTacticsFile
{
    public int SchemaVersion {get;set;}=1;
    public bool Enabled {get;set;}
    public int[] EnabledWars {get;set;}=Array.Empty<int>();
    public int ThreatWeight {get;set;}
    public int SupportWeight {get;set;}
    public int FocusWeight {get;set;}
    public int CommanderWeight {get;set;}
    public int AbilityWeight {get;set;}
    public int StallPenalty {get;set;}
    public TacticalObjective[] Objectives {get;set;}=Array.Empty<TacticalObjective>();
}
internal sealed class TacticalObjective
{
    public int WarNumber {get;set;}
    public bool Enabled {get;set;}
    public int RequiredRouts {get;set;}
}
internal readonly record struct TacticalUnit(int Slot,int X,int Y,int Damage,int Hp,int Range,bool Support);
internal readonly record struct TacticalFacts(int Threat,int ImmediateThreat,int NearbyAllies,int CoveredSupport,int Focus,int CommanderExposure,int Utility,int Victims,int FriendlyVictims,bool RepeatedMove,bool Progress,int NeededHits=2);
internal readonly record struct WarManeuver(int CurrentThreat, int CurrentImmediate, int ScreenGain,
    int CommanderGuardGain, int Approach, int HostilesInRange, bool TargetCounters);
internal static class WarTacticsRules
{
    internal static readonly int[] Supported={1,2,3,6,10,13,15};
    internal static WarTacticsFile Parse(string json)
    {
        if(json.Length>32768)throw new JsonException("War tactics config exceeds 32 KiB.");
        using var d=JsonDocument.Parse(json);
        OverhaulRules.Keys(d.RootElement,"SchemaVersion","Enabled","EnabledWars","ThreatWeight","SupportWeight","FocusWeight","CommanderWeight","AbilityWeight","StallPenalty","Objectives");
        foreach(var e in d.RootElement.GetProperty("Objectives").EnumerateArray())OverhaulRules.Keys(e,"WarNumber","Enabled","RequiredRouts");
        var f=ContentRules.Read<WarTacticsFile>(json);
        if(f.SchemaVersion!=1||f.EnabledWars==null||f.EnabledWars.Distinct().Count()!=f.EnabledWars.Length||f.EnabledWars.Any(n=>!Supported.Contains(n))||f.Objectives==null||f.Objectives.Length!=3||f.Objectives.Select(o=>o?.WarNumber).Distinct().Count()!=3||new[]{f.ThreatWeight,f.SupportWeight,f.FocusWeight,f.CommanderWeight,f.AbilityWeight,f.StallPenalty}.Any(n=>n is <0 or >8))throw new JsonException("Unsupported tactics schema, scope or weights (0..8).");
        foreach(var o in f.Objectives)if(o==null||o.WarNumber is not (3 or 6 or 13)||o.RequiredRouts<1||o.RequiredRouts>(o.WarNumber==3?7:o.WarNumber==6?11:14))throw new JsonException("Unsupported objective or rout count.");
        return f;
    }
    internal static bool Objective(TacticalObjective p,int war,long turn,int routs,bool heroAlive,bool enemyAlive,int heroX,int heroY)
    {
        if(!p.Enabled||p.WarNumber!=war||!heroAlive||!enemyAlive||routs<p.RequiredRouts)return false;
        return war switch{3=>turn>=14&&heroX is >=4 and <=6&&heroY is >=1 and <=3,6=>turn>=9,13=>turn>=7,_=>false};
    }
    internal static int Score(int original,WarTacticsFile p,TacticalUnit actor,int action,int chance,TacticalFacts f)
    {
        if(!p.Enabled||action is <0 or >2||actor.Hp<=0||actor.Damage>=actor.Hp||actor.Damage<0||chance is <0 or >100)return original;
        int wounded=actor.Damage>0?2:1;
        int delta=-Math.Min(6,f.Threat)*p.ThreatWeight*wounded-Math.Min(4,f.ImmediateThreat)*p.ThreatWeight;
        // Local mutual support reduces isolation but never erases overwhelming threat.
        delta+=Math.Min(2,f.NearbyAllies)*p.SupportWeight;
        if(actor.Range<=1)delta+=Math.Min(2,f.CoveredSupport)*p.SupportWeight;
        if(action==0)
        {
            if(chance<=15)delta-=p.ThreatWeight*4;
            else if(chance<35&&actor.Damage>0)delta-=p.ThreatWeight*3;
            if(chance>=35)delta+=(f.Focus<Math.Max(1,f.NeededHits)?Math.Min(2,f.Focus):-Math.Min(3,f.Focus))*p.FocusWeight;
            if(chance>=35)delta+=Math.Min(3,f.CommanderExposure)*p.CommanderWeight;
        }
        if(action==2)
        {
            if(f.Utility<=0||f.FriendlyVictims>0)delta-=p.AbilityWeight*3;
            else delta+=Math.Min(3,Math.Max(1,f.Victims))*p.AbilityWeight;
        }
        if(action==1&&f.RepeatedMove&&!f.Progress)delta-=p.StallPenalty*3;
        return delta==0?original:(int)Math.Clamp((long)original+delta,-30000,30000);
    }

    // Add a bounded preference to candidates supplied by the native evaluator.
    // Staged wars use local positioning only and gain no outcome or roster rules.
    internal static int Maneuver(int score, int war, TacticalUnit actor, int action,
        int chance, TacticalFacts facts, WarManeuver move)
    {
        if (!Supported.Contains(war) || action is <0 or >2 || actor.Hp <= actor.Damage ||
            actor.Damage < 0 || chance is <0 or >100) return score;

        bool staged = war is 10 or 15;
        bool ranged = actor.Range > 1;
        bool vulnerable = actor.Hp - actor.Damage <= 1;
        int exposure = facts.ImmediateThreat - move.CurrentImmediate;
        int delta = action switch
        {
            0 => (chance >= 45 && facts.Focus < facts.NeededHits ? 3 : 0)
                + (chance >= 55 && facts.NeededHits == 1 && facts.Focus == 0 ? 7 : 0)
                - (move.TargetCounters && (vulnerable || chance < 35) ? 10 : 0)
                - (facts.Focus >= facts.NeededHits ? 5 : 0),
            1 => Math.Clamp(move.Approach, -2, 2) * (war is 2 or 3 or 13 ? 4 : 2)
                + Math.Clamp(move.ScreenGain, -2, 2) * (war is 6 or 13 ? 5 : 3)
                + Math.Clamp(move.CommanderGuardGain, -1, 1) * (war is 6 or 13 or 15 ? 6 : 3)
                + (ranged && facts.ImmediateThreat == 0 && move.HostilesInRange > 0 ? 5 : 0)
                - (facts.RepeatedMove && !facts.Progress && exposure >= 0 ? 6 : 0),
            _ => facts.FriendlyVictims == 0 && facts.Utility > 0 && facts.Victims >= 2 ? 6 : 0
        };
        delta -= Math.Clamp(exposure, -2, 3) * (vulnerable ? 6 : ranged ? 4 : 2);
        if (facts.Threat >= 3 && facts.Threat > move.CurrentThreat)
            delta -= vulnerable || actor.Slot == 0 ? 8 : 4;
        if (staged) delta /= 2;
        int limit = staged ? 10 : 23;
        return (int)Math.Clamp((long)score + Math.Clamp(delta, -limit, limit), -30000, 30000);
    }
}
