using System.Text.Json;
namespace NightmareRebalance;

internal sealed class WarPolicyFile
{
    public int SchemaVersion{get;set;}=1;
    public WarPolicy[] Wars{get;set;}=Array.Empty<WarPolicy>();
}
internal sealed class WarPolicy
{
    public int WarNumber{get;set;}
    public bool Enabled{get;set;}
    public bool PlayerCommands{get;set;}
    public bool CommanderVictory{get;set;}
    public int MaximumTurn{get;set;}
    public bool EnemyAi{get;set;}
    public int WoundedTargetBonus{get;set;}
    public int PoorOddsPenalty{get;set;}
}
internal enum War2Outcome { Vanilla,Continue,TacticalVictory,ProtectedAreaBreached,AlliedDefeat,OrderlyWithdrawal }
internal readonly record struct WarUnitState(int Force,int Slot,int Flag,int X,int Y,int Damage=0);
internal static class WarPolicyRules
{
    internal static WarPolicyFile Parse(string json)
    {
        if(json.Length>32768)throw new JsonException("Native war rules exceed 32 KiB.");
        using var doc=JsonDocument.Parse(json);
        OverhaulRules.Keys(doc.RootElement,"SchemaVersion","Wars");
        foreach(var node in doc.RootElement.GetProperty("Wars").EnumerateArray())OverhaulRules.Keys(node,"WarNumber","Enabled","PlayerCommands","CommanderVictory","MaximumTurn","EnemyAi","WoundedTargetBonus","PoorOddsPenalty");
        var file=ContentRules.Read<WarPolicyFile>(json);
        if(file.SchemaVersion!=1||file.Wars==null||file.Wars.Length!=1||file.Wars[0]==null)throw new JsonException("Schema 1 requires exactly the audited war-2 policy.");
        var r=file.Wars[0];
        if(r.WarNumber!=2||r.MaximumTurn is <6 or >60||r.WoundedTargetBonus is <0 or >20||r.PoorOddsPenalty is <0 or >20)throw new JsonException("Unsupported war ID or bounded policy values.");
        return file;
    }
    internal static War2Outcome Decide(WarPolicy r,bool enabled,bool scoped,int war,long turn,IReadOnlyList<WarUnitState> units)
    {
        if(!enabled||!r.Enabled||!scoped||war!=2||r.WarNumber!=2||turn<1||units.Count!=30)return War2Outcome.Vanilla;
        var slots=new HashSet<(int,int)>();
        foreach(var u in units)
            if(u.Force is <0 or >1||u.Slot is <0 or >14||u.Flag is <0 or >2||!slots.Add((u.Force,u.Slot))||(u.Flag==1&&(u.X is <0 or >15||u.Y is <0 or >11)))return War2Outcome.Vanilla;
        bool hadAllies=units.Any(u=>u.Force==0&&u.Flag!=0);var command=units.Single(u=>u.Force==1&&u.Slot==0);
        if(!hadAllies||command.Flag==0)return War2Outcome.Vanilla;
        // Match native priority: fort breach defeats even if command routes simultaneously.
        if(units.Any(u=>u.Force==1&&u.Flag==1&&u.X is >=3 and <=5&&u.Y is >=6 and <=8))return War2Outcome.ProtectedAreaBreached;
        if(!units.Any(u=>u.Force==0&&u.Flag==1))return War2Outcome.AlliedDefeat;
        if(r.CommanderVictory&&command.Flag==2)return War2Outcome.TacticalVictory;
        if(turn>=r.MaximumTurn)return War2Outcome.OrderlyWithdrawal;
        return War2Outcome.Continue;
    }
    internal static int AiScore(int original,int action,int targetFlag,int damage,int maxHp,int chance,int bonus,int penalty)
    {
        if(action!=0||targetFlag!=1||damage<0||maxHp is <1 or >9||damage>=maxHp||chance is <0 or >100)return original;
        int change=(damage>0?bonus:0)-(chance<=10?penalty:0);
        if(change==0)return original;
        return (int)Math.Clamp((long)original+change,-30000,30000);
    }
}
