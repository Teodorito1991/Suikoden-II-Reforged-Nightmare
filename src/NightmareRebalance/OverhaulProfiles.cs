using System.Text.Json;

namespace NightmareRebalance;

internal sealed class EnemyProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public int ExpectedHp { get; set; }
    public int ExpectedLevel { get; set; }
    public int NameId { get; set; }
    public double HpMultiplier { get; set; }
    public double DamageMultiplier { get; set; }
}
internal sealed class EnemyFile
{
    public int SchemaVersion { get; set; }
    public EnemyProfile[] Profiles { get; set; } = Array.Empty<EnemyProfile>();
}
internal sealed class CommanderProfile
{
    public int NameId { get; set; }
    public string Name { get; set; } = "";
    public string Branch { get; set; } = "Any";
    public byte Attack { get; set; }
    public byte Defense { get; set; }
    public byte AssistAttack { get; set; }
    public byte AssistDefense { get; set; }
    public string[] Abilities { get; set; } = Array.Empty<string>();
    public byte[] Uses { get; set; } = Array.Empty<byte>();
}
internal sealed class CommanderFile
{
    public int SchemaVersion { get; set; }
    public CommanderProfile[] Profiles { get; set; } = Array.Empty<CommanderProfile>();
}
internal static class OverhaulRules
{
    internal static readonly string[] Abilities = { "SP_NONE", "SP_MOUNT", "SP_CHARGE", "SP_AIMING", "SP_FLAME_SPEAR", "SP_SHINING_SHIELD", "SP_HP_PLUS", "SP_CRITICAL", "SP_FORCE_MOVE", "SP_SCOUT", "SP_FOREST_WALK", "SP_MAGIC_WIND1", "SP_MAGIC_FIRE1", "SP_MAGIC_THUNDER1", "SP_MAGIC_WIND2", "SP_SEE_THROUGH", "SP_KIN_SLAYER", "SP_MEDICAL1", "SP_MEDICAL2", "SP_THROUGH_ROAD", "SP_BODY_GUARD", "SP_CHEAR_UP", "SP_INVESTIGATION", "SP_INVENTION", "SP_CONFUSED_FIGHT", "SP_FLYING" };
    internal static bool SafeAbility(int id) => id >= 0 && id < Abilities.Length && id is not (2 or 8 or 14 or 16);
    internal static bool ActiveAbility(int id) => id is 3 or 4 or 5 or 9 or 11 or 12 or 13 or 17 or 18 or 21 or 23;
    internal static void Keys(JsonElement obj, params string[] keys)
    {
        if (obj.ValueKind != JsonValueKind.Object) throw new JsonException("Expected object.");
        var names=new HashSet<string>();
        foreach(var p in obj.EnumerateObject()) if(!keys.Contains(p.Name)||!names.Add(p.Name)) throw new JsonException("Unknown/duplicate key: "+p.Name);
        if(!names.SetEquals(keys)) throw new JsonException("Missing required key.");
    }
    static JsonDocument Document(string json)
    { if(json.Length>262144)throw new JsonException("File exceeds 256 KiB.");return JsonDocument.Parse(json); }
    internal static EnemyFile Enemies(string json, EnemyFile baseline)
    {
        using var doc=Document(json);Keys(doc.RootElement,"SchemaVersion","Profiles");
        foreach(var row in doc.RootElement.GetProperty("Profiles").EnumerateArray()) Keys(row,"Id","Name","Role","ExpectedHp","ExpectedLevel","NameId","HpMultiplier","DamageMultiplier");
        var file=ContentRules.Read<EnemyFile>(json);
        if(file.SchemaVersion!=1||file.Profiles==null||file.Profiles.Length>280)throw new JsonException("Invalid enemy schema/count.");
        var ids=new HashSet<string>(StringComparer.Ordinal);
        foreach(var e in file.Profiles)
        {
            if(e==null||e.Id==null||!ids.Add(e.Id))throw new JsonException("Null/duplicate enemy.");
            var b=baseline.Profiles.SingleOrDefault(b=>b.Id==e.Id);
            if(b==null||e.ExpectedHp!=b.ExpectedHp||e.ExpectedLevel!=b.ExpectedLevel||e.NameId!=b.NameId||e.Name==null||e.Name.Length>120||e.Role==null||e.Role.Length>80||
                !double.IsFinite(e.HpMultiplier)||!double.IsFinite(e.DamageMultiplier)||e.HpMultiplier is <0.25 or >4||e.DamageMultiplier is <0.25 or >4)throw new JsonException("Unverified identity or invalid enemy factor (0.25..4).");
        }
        return file;
    }
    internal static bool EnemyEligible(EnemyProfile profile,bool regularScope,bool realEnemy,int hp,int name,int level) =>
        regularScope&&realEnemy&&hp==profile.ExpectedHp&&name==profile.NameId&&level==profile.ExpectedLevel;
    internal static CommanderFile Commanders(string json, CommanderFile baseline)
    {
        using var doc=Document(json);Keys(doc.RootElement,"SchemaVersion","Profiles");
        foreach(var row in doc.RootElement.GetProperty("Profiles").EnumerateArray()) Keys(row,"NameId","Name","Branch","Attack","Defense","AssistAttack","AssistDefense","Abilities","Uses");
        var file=ContentRules.Read<CommanderFile>(json);
        if(file.SchemaVersion!=1||file.Profiles==null||file.Profiles.Length>64)throw new JsonException("Invalid commander schema/count.");
        var ids=new HashSet<string>();
        foreach(var c in file.Profiles)
        {
            if(c==null||!baseline.Profiles.Any(b=>b.NameId==c.NameId&&b.Branch==c.Branch)||!ids.Add($"{c.NameId}/{c.Branch}")||c.Name==null||c.Name.Length>120||
                c.Attack>20||c.Defense>20||c.AssistAttack>5||c.AssistDefense>5||c.Abilities==null||c.Uses==null||c.Abilities.Length>3||c.Uses.Length!=c.Abilities.Length)throw new JsonException("Invalid/unverified commander or bounds (stats 0..20, assist 0..5, slots <=3).");
            var abilities=new HashSet<string>();
            for(int i=0;i<c.Abilities.Length;i++)
            {
                int id=Array.IndexOf(Abilities,c.Abilities[i]);
                if(!SafeAbility(id)||!abilities.Add(c.Abilities[i])||c.Uses[i]>3||(!ActiveAbility(id)&&c.Uses[i]!=0))throw new JsonException("Unsafe/duplicate ability or invalid uses (active 0..3, passive 0).");
            }
        }
        return file;
    }
    internal static CommanderProfile? Select(CommanderFile file,int name,string branch) => file.Profiles.FirstOrDefault(c=>c.NameId==name&&c.Branch==branch)??file.Profiles.FirstOrDefault(c=>c.NameId==name&&c.Branch=="Any");
}
