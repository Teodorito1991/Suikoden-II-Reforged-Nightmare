using System.Text.Json;
namespace NightmareRebalance;

internal enum CampaignObjective { CommanderDefeat, EligibleElimination, HoldArmy }
internal enum CommanderStyle { Aggressive, Defensive, Tactical, Screen, Ranged }
internal sealed class CampaignWarFile
{
    public int SchemaVersion {get;set;}=1;
    public CampaignWarPolicy[] Scenarios {get;set;}=Array.Empty<CampaignWarPolicy>();
    public WarAiProfile[] AiProfiles {get;set;}=Array.Empty<WarAiProfile>();
}
internal sealed class CampaignWarPolicy
{
    public int WarNumber {get;set;}
    public bool Enabled {get;set;}=true;
    public string Objective {get;set;}="";
    public int MaximumTurn {get;set;}
    public bool EnemyAi {get;set;}=true;
}
internal sealed class WarAiProfile
{
    public int NameId {get;set;}
    public string Branch {get;set;}="Any";
    public string Style {get;set;}="Tactical";
    public int AttackBonus {get;set;}
    public int ObjectiveWeight {get;set;}
    public int ProtectionWeight {get;set;}
    public int RiskPenalty {get;set;}
    public int AbilityBonus {get;set;}
}
internal readonly record struct CampaignAudit(int War,int Map,int Commander,int CommanderName,int TimerTurn,int TimerEnding,bool Routable);
internal enum CampaignDecision { Vanilla, Continue, CommanderDefeat, EligibleElimination, HoldCompleted, Withdrawal }
internal readonly record struct AiCandidate(int Action,int X,int Y,int Target,int TargetDamage,int Chance,int AbilityUtility);
internal readonly record struct AiUnit(int Slot,int X,int Y,int Damage,int Branch);

internal static class TotalWarRules
{
    internal static readonly CampaignAudit[] Audits={new(3,9,61,143,14,1,true),new(6,11,72,88,9,1,false),new(13,8,66,81,7,4,false)};
    internal static CampaignWarFile Parse(string json)
    {
        if(json.Length>65536)throw new JsonException("Total-war policy exceeds 64 KiB.");
        using var doc=JsonDocument.Parse(json);OverhaulRules.Keys(doc.RootElement,"SchemaVersion","Scenarios","AiProfiles");
        foreach(var n in doc.RootElement.GetProperty("Scenarios").EnumerateArray())OverhaulRules.Keys(n,"WarNumber","Enabled","Objective","MaximumTurn","EnemyAi");
        foreach(var n in doc.RootElement.GetProperty("AiProfiles").EnumerateArray())OverhaulRules.Keys(n,"NameId","Branch","Style","AttackBonus","ObjectiveWeight","ProtectionWeight","RiskPenalty","AbilityBonus");
        var f=ContentRules.Read<CampaignWarFile>(json);
        if(f.SchemaVersion!=1||f.Scenarios==null||f.AiProfiles==null||f.Scenarios.Length!=3||f.AiProfiles.Length>32)throw new JsonException("Unsupported total-war schema/count.");
        var wars=new HashSet<int>();
        foreach(var r in f.Scenarios)
        {
            if(r==null||!wars.Add(r.WarNumber))throw new JsonException("Null/duplicate scenario.");
            var a=Audits.SingleOrDefault(a=>a.War==r.WarNumber);
            string expected=r.WarNumber==3?"CommanderDefeat":r.WarNumber==6?"EligibleElimination":"HoldArmy";
            if(a.War==0||r.Objective!=expected||r.MaximumTurn<a.TimerTurn+1||r.MaximumTurn>40)throw new JsonException("Unaudited objective/war or turn bound.");
        }
        var ids=new HashSet<string>();
        foreach(var a in f.AiProfiles)
            if(a==null||!ids.Add($"{a.NameId}/{a.Branch}")||a.NameId is not (143 or 145 or 144 or 88 or 81 or 146 or 3369)||a.Branch is not ("Any" or "Infantry" or "Archer" or "Magicians")||!Enum.TryParse<CommanderStyle>(a.Style,out var style)||!Enum.IsDefined(typeof(CommanderStyle),style)||new[]{a.AttackBonus,a.ObjectiveWeight,a.ProtectionWeight,a.RiskPenalty,a.AbilityBonus}.Any(v=>v is <0 or >12))throw new JsonException("Unverified AI identity/style or weight (0..12).");
        return f;
    }
    internal static CampaignDecision Decide(CampaignWarPolicy r,bool enabled,bool ready,int war,long turn,IReadOnlyList<WarUnitState> units)
    {
        var audit=Audits.SingleOrDefault(a=>a.War==war);
        if(!enabled||!ready||!r.Enabled||r.WarNumber!=war||audit.War==0||turn<1||units.Count!=30)return CampaignDecision.Vanilla;
        var slots=new HashSet<(int,int)>();
        foreach(var u in units)if(u.Force is <0 or >1||u.Slot is <0 or >14||u.Flag is <0 or >2||!slots.Add((u.Force,u.Slot))||(u.Flag==1&&(u.X is <0 or >15||u.Y is <0 or >11)))return CampaignDecision.Vanilla;
        var hero=units.Single(u=>u.Force==0&&u.Slot==0);var enemy=units.Single(u=>u.Force==1&&u.Slot==0);
        // Native common and scenario-specific loss checks keep priority.
        if(hero.Flag==2||enemy.Flag==2||hero.Flag==0||enemy.Flag==0||!units.Any(u=>u.Force==0&&u.Flag==1))return CampaignDecision.Vanilla;
        if(war==6&&turn>=audit.TimerTurn&&units.Where(u=>u.Force==1&&u.Slot is >=1 and <=11).All(u=>u.Flag==2))return CampaignDecision.EligibleElimination;
        if(turn>=r.MaximumTurn)return war==13?CampaignDecision.HoldCompleted:CampaignDecision.Withdrawal;
        return CampaignDecision.Continue;
    }
    internal static int Distance(int x,int y,int tx,int ty)=>Math.Abs(x-tx)+Math.Abs(y-ty);
    internal static int Score(int original,WarAiProfile p,AiUnit actor,AiCandidate c,int goalX,int goalY,int homeX,int homeY,IReadOnlyList<AiUnit> friends,IReadOnlyList<AiUnit> enemies)
    {
        if(c.Action is <0 or >2||c.X is <0 or >15||c.Y is <0 or >11||actor.X is <0 or >15||actor.Y is <0 or >11||!Enum.TryParse<CommanderStyle>(p.Style,out var style))return original;
        if(c.Action==0&&(c.Target is <0 or >14||c.Chance is <0 or >100))return original;
        int delta=0;
        if(c.Action==0)
        {
            delta+=p.AttackBonus;
            if(c.Target==0)delta+=style==CommanderStyle.Aggressive?p.ObjectiveWeight*2:p.ObjectiveWeight;
            if(c.TargetDamage>0)delta+=2;
            if(actor.Damage>0&&c.Chance<50)delta-=p.RiskPenalty;
            if(c.Chance<=10)delta-=p.RiskPenalty;
        }
        if(c.Action==2&&c.AbilityUtility>0)delta+=p.AbilityBonus*Math.Min(10,c.AbilityUtility)/10;
        // Score destinations already enumerated by native reachability; never make a move.
        int before=Distance(actor.X,actor.Y,goalX,goalY),after=Distance(c.X,c.Y,goalX,goalY);
        if(style is CommanderStyle.Aggressive or CommanderStyle.Tactical)delta+=Math.Clamp(before-after,-3,3)*p.ObjectiveWeight;
        if(style==CommanderStyle.Defensive)delta+=(Math.Min(6,Distance(actor.X,actor.Y,homeX,homeY))-Math.Min(6,Distance(c.X,c.Y,homeX,homeY)))*p.ProtectionWeight;
        if(style==CommanderStyle.Screen)
        {
            var ranged=friends.Where(u=>u.Slot!=actor.Slot&&u.Branch is 1 or 2).OrderBy(u=>Distance(actor.X,actor.Y,u.X,u.Y)).ThenBy(u=>u.Slot).FirstOrDefault();
            if(friends.Any(u=>u.Slot==ranged.Slot&&u.Slot!=actor.Slot&&u.Branch is 1 or 2))
                delta+=(Math.Min(4,Math.Abs(Distance(actor.X,actor.Y,ranged.X,ranged.Y)-1))-Math.Min(4,Math.Abs(Distance(c.X,c.Y,ranged.X,ranged.Y)-1)))*p.ProtectionWeight;
            delta+=Math.Clamp(before-after,-2,2)*p.ObjectiveWeight;
        }
        if(style==CommanderStyle.Ranged||actor.Damage>0)
        {
            int dangerBefore=enemies.Count(u=>Distance(actor.X,actor.Y,u.X,u.Y)<=1),dangerAfter=enemies.Count(u=>Distance(c.X,c.Y,u.X,u.Y)<=1);
            delta+=(dangerBefore-dangerAfter)*p.RiskPenalty;
        }
        return delta==0?original:(int)Math.Clamp((long)original+delta,-30000,30000);
    }
}
