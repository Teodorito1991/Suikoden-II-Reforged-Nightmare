using System.Text.Json;
namespace NightmareRebalance;

internal sealed class CombatReforgedFile
{
    public int SchemaVersion {get;set;}=1;
    public bool Enabled {get;set;}=true;
    public bool EnableEnemyAi {get;set;}=true;
    public bool EnableBossEncounters {get;set;}=true;
    public bool EnablePacing {get;set;}=true;
    public bool EnableHealingBalance {get;set;}=true;
    public bool EnableSleepBalance {get;set;}=true;
    public int RepeatPenalty {get;set;}=28;
    public int FocusLimit {get;set;}=2;
    public double HealingThreshold {get;set;}=.65;
    public double SleepSusceptibilityMultiplier {get;set;}=1.0;
    public CombatPacing[] RolePacing {get;set;}=Array.Empty<CombatPacing>();
    public CombatSpell[] SpellEffects {get;set;}=Array.Empty<CombatSpell>();
    public CombatBossSetting[] Bosses {get;set;}=Array.Empty<CombatBossSetting>();
}
internal sealed class CombatPacing { public string Role {get;set;}="";public double HpMultiplier {get;set;}public double DamageMultiplier {get;set;} }
internal sealed class CombatSpell { public int Id {get;set;}public int Expected {get;set;}public int Effect {get;set;} }
internal sealed class CombatBossSetting { public string TableId {get;set;}="";public bool Enabled {get;set;}public double HpMultiplier {get;set;}public double DamageMultiplier {get;set;} }
internal sealed class CombatAudit { public CombatEnemyAudit[] Enemies {get;set;}=Array.Empty<CombatEnemyAudit>();public CombatBossAudit[] Bosses {get;set;}=Array.Empty<CombatBossAudit>(); }
internal class CombatEnemyAudit { public string Id {get;set;}="";public string Behavior {get;set;}="";public int[] Signature {get;set;}=Array.Empty<int>(); }
internal sealed class CombatBossAudit
{
    public string TableId {get;set;}="";public string PartyId {get;set;}="";public string EnemyId {get;set;}="";public string Behavior {get;set;}="";
    public double HpMultiplier {get;set;}public double DamageMultiplier {get;set;}public uint Flags {get;set;}
    public int[] Hooks {get;set;}=Array.Empty<int>();public string[] HookNames {get;set;}=Array.Empty<string>();public int[] Signature {get;set;}=Array.Empty<int>();
}
internal readonly record struct CombatTarget(int Slot,int Hp,int MaxHp,int Defense,int MagicDefense,int Magic,int Attack,int Focus,bool StatusUseful=true);
internal readonly record struct CombatAction(int Mode,int Type,int Power,bool Physical,bool Multi,int Status,int Probability,int Remaining,bool Legal);
internal static class CombatReforgedRules
{
    internal static readonly string[] Roles={"Bruiser","Raider","Bulwark","FragilePressure"};
    internal static readonly string[] BossIds={"boss_ryo","boss_rat","boss_hap1","boss_sie1"};
    internal static CombatReforgedFile Parse(string json)
    {
        if(json.Length>65536)throw new JsonException("Combat configuration exceeds 64 KiB.");
        using var d=JsonDocument.Parse(json);
        OverhaulRules.Keys(d.RootElement,"SchemaVersion","Enabled","EnableEnemyAi","EnableBossEncounters","EnablePacing","EnableHealingBalance","EnableSleepBalance","RepeatPenalty","FocusLimit","HealingThreshold","SleepSusceptibilityMultiplier","RolePacing","SpellEffects","Bosses");
        foreach(var a in d.RootElement.GetProperty("RolePacing").EnumerateArray())OverhaulRules.Keys(a,"Role","HpMultiplier","DamageMultiplier");
        foreach(var a in d.RootElement.GetProperty("SpellEffects").EnumerateArray())OverhaulRules.Keys(a,"Id","Expected","Effect");
        foreach(var a in d.RootElement.GetProperty("Bosses").EnumerateArray())OverhaulRules.Keys(a,"TableId","Enabled","HpMultiplier","DamageMultiplier");
        var p=ContentRules.Read<CombatReforgedFile>(json);
        bool Factor(double n)=>double.IsFinite(n)&&n is >=.5 and <=2.5;
        if(p.SchemaVersion!=1||p.RepeatPenalty is <0 or >100||p.FocusLimit is <1 or >6||!double.IsFinite(p.HealingThreshold)||p.HealingThreshold is <.1 or >.95||!double.IsFinite(p.SleepSusceptibilityMultiplier)||p.SleepSusceptibilityMultiplier is <1 or >1.5||p.RolePacing==null||p.SpellEffects==null||p.Bosses==null)throw new JsonException("Invalid combat schema or bounds.");
        if(p.RolePacing.Length!=4||p.RolePacing.Select(x=>x?.Role).Distinct().Count()!=4||p.RolePacing.Any(x=>x==null||!Roles.Contains(x.Role)||!Factor(x.HpMultiplier)||!Factor(x.DamageMultiplier)))throw new JsonException("Expected four distinct audited pacing roles.");
        if(p.SpellEffects.Length!=3||p.SpellEffects.Select(x=>x?.Id).Distinct().Count()!=3||p.SpellEffects.Any(x=>x==null||x.Id is not(6 or 12 or 34)||x.Expected!=(x.Id==12?210:100)||x.Effect is <50 or >400))throw new JsonException("Unsupported healing edit.");
        if(p.Bosses.Length!=4||p.Bosses.Select(x=>x?.TableId).Distinct().Count()!=4||p.Bosses.Any(x=>x==null||!BossIds.Contains(x.TableId)||!Factor(x.HpMultiplier)||!Factor(x.DamageMultiplier)))throw new JsonException("Unsupported boss scope or factors.");
        return p;
    }
    // Matches the inspected unmodified native base calculation; excludes affinity,
    // criticals, rune/passive effects, status defense and later scaling.
    internal static int BaseDamage(int power,int ownMagic,int defense,int magicDefense,bool physical,int jitter)
    {
        int raw=Math.Max(0,physical?power-defense:power+ownMagic-magicDefense);
        return raw+Math.Clamp(jitter,1,raw<=5?2:8);
    }
    internal static int TargetScore(string behavior,CombatTarget t,int focusLimit)
    {
        if(t.Hp<=0||t.MaxHp<=0||t.Hp>t.MaxHp)return int.MinValue;
        int hurt=100-(int)((long)t.Hp*100/t.MaxHp);
        int score=behavior switch
        {
            "Aggressive"=>hurt+Math.Max(0,100-t.Defense)/4,
            "Caster"=>Math.Max(0,200-t.MagicDefense)/3+(t.StatusUseful?20:0),
            "Cautious"=>Math.Max(0,200-t.Attack)/3+hurt/4,
            "Disruptor"=>t.Magic/2+(t.StatusUseful?20:0),
            _=>hurt/2+Math.Max(0,150-t.Defense)/3
        };
        // Soft spread pressure: never forces an illegal candidate or removes focus.
        return score-Math.Max(0,t.Focus-focusLimit+1)*50;
    }
    internal static int ChooseTarget(string role,IReadOnlyList<CombatTarget> targets,int original,int limit)
    {
        int best=original,score=int.MinValue;
        foreach(var t in targets.OrderBy(t=>t.Slot==original?0:1))
        {int n=TargetScore(role,t,limit);if(n>score){score=n;best=t.Slot;}}
        return best;
    }
    internal static int ActionScore(string role,CombatAction a,IReadOnlyList<CombatTarget> enemies,IReadOnlyList<CombatTarget> allies,int ownMagic,int previous,int repeatPenalty,double healThreshold,bool ordinary=false)
    {
        if(!a.Legal||a.Mode is <0 or >3||a.Type is not(1 or 2 or 5)||a.Probability<=0||a.Remaining==0)return int.MinValue;
        if(a.Type==5)
        {
            var hurt=allies.Where(t=>t.Hp>0&&t.MaxHp>0&&(double)t.Hp/t.MaxHp<healThreshold).ToArray();
            return hurt.Length==0?-200:90+Math.Min(80,hurt.Sum(t=>t.MaxHp-t.Hp)/4);
        }
        if(enemies.Count==0)return int.MinValue;
        int score=enemies.Max(t=>Math.Min(t.Hp,BaseDamage(a.Power,ownMagic,t.Defense,t.MagicDefense,a.Physical,4)))*100/Math.Max(1,(int)enemies.Average(t=>t.MaxHp));
        if(a.Multi)score+=Math.Min(4,enemies.Count)*8;
        if(a.Status!=0)
        {
            int useful=enemies.Count(t=>t.StatusUseful);
            score+=useful==0?-35:ordinary&&a.Multi?22*useful/enemies.Count:22;
        }
        if(role is "Caster" or "Disruptor" && !a.Physical)score+=12;
        if(role=="Cautious"&&!a.Physical)score+=10;
        if(a.Mode==previous&&a.Mode!=0)score-=repeatPenalty;
        return score;
    }
    internal static bool BossMatches(CombatBossAudit a,EncounterIdentity id)=>id.HasParty&&id.Players>0&&id.Enemies==1&&id.PartyIndex==0&&id.TableId==a.TableId&&id.PartyId==a.PartyId&&id.PartyFlags==a.Flags&&id.ScriptCommandCount==0&&id.HasEndHandler&&id.HasMainHandler==(a.Hooks[5]!=0)&&id.HookIndices.SequenceEqual(a.Hooks)&&id.HookNames.SequenceEqual(a.HookNames)&&id.EnemyIds.SequenceEqual(new[]{a.EnemyId})&&id.EnemyMaxHp.SequenceEqual(new[]{a.Signature[0]})&&id.MonsterIds.SequenceEqual(new[]{1,0,0,0,0,0})&&id.MonsterCodes.SequenceEqual(new[]{0,1,0,0,0,0});
}
