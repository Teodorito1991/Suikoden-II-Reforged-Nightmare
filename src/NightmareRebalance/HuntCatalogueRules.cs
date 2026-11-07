using System.Text.Json;
namespace NightmareRebalance;
internal sealed class HuntUnitRule
{
 public string Id{get;set;}="";public double HpMultiplier{get;set;}=1;public string Behavior{get;set;}="";
}
internal sealed class HuntDefinition
{
 public string Id{get;set;}="";public string Name{get;set;}="";public string Field{get;set;}="";public int PartyCount{get;set;}
 public FormationAudit Audit{get;set;}=new();public HuntUnitRule[] Units{get;set;}=Array.Empty<HuntUnitRule>();
 public double DamageMultiplier{get;set;}=1;public int VictoryBonusPotch{get;set;}
 internal string[] Roster=>Audit.NewCodes.Where(x=>x>0).Select(x=>Audit.Monsters.Single(m=>m.Index==x).Id).OrderBy(x=>x).ToArray();
}
internal sealed class HuntOverride
{
 public string Id{get;set;}="";public bool Enabled{get;set;}=false;public int? VictoryBonusPotch{get;set;}
 public double? DamageMultiplier{get;set;}public HuntUnitRule[] Units{get;set;}=Array.Empty<HuntUnitRule>();
}
internal sealed class HuntCatalogueFile
{
 public int SchemaVersion{get;set;}=1;public string SelectedHuntId{get;set;}="sun-kings-retinue";public HuntOverride[] Hunts{get;set;}=Array.Empty<HuntOverride>();
}
internal static class HuntCatalogueRules
{
 internal static readonly string[] Behaviors={"Aggressive","Caster","Cautious","Disruptor","Opportunist","Alternating","Attrition"};
 internal static HuntCatalogueFile Parse(string json,HuntDefinition[] definitions)
 {
  if(json.Length>32768)throw new JsonException("Hunt catalogue exceeds 32KiB");using var doc=JsonDocument.Parse(json);
  OverhaulRules.Keys(doc.RootElement,"SchemaVersion","SelectedHuntId","Hunts");
  foreach(var x in doc.RootElement.GetProperty("Hunts").EnumerateArray()){
   OverhaulRules.Keys(x,"Id","Enabled","VictoryBonusPotch","DamageMultiplier","Units");
   foreach(var u in x.GetProperty("Units").EnumerateArray())OverhaulRules.Keys(u,"Id","HpMultiplier","Behavior");
  }
  var c=ContentRules.Read<HuntCatalogueFile>(json);
  if(c.SchemaVersion!=1||c.Hunts==null||c.Hunts.Any(x=>x==null)||!c.Hunts.Select(x=>x.Id).OrderBy(x=>x).SequenceEqual(definitions.Select(x=>x.Id).OrderBy(x=>x))||!definitions.Any(x=>x.Id==c.SelectedHuntId))throw new JsonException("Unknown, missing or duplicate hunt identity");
  foreach(var x in c.Hunts){var d=definitions.Single(d=>d.Id==x.Id);
   if(x.VictoryBonusPotch is <0 or >1000000||x.DamageMultiplier is {} f&&(!double.IsFinite(f)||f<0.5||f>5)||x.Units==null||x.Units.Any(u=>u==null)||x.Units.Select(u=>u.Id).Distinct().Count()!=x.Units.Length)throw new JsonException("Invalid hunt overrides");
   foreach(var u in x.Units)if(!d.Units.Any(v=>v.Id==u.Id)||!double.IsFinite(u.HpMultiplier)||u.HpMultiplier<0.5||u.HpMultiplier>5||!Behaviors.Contains(u.Behavior))throw new JsonException("Invalid hunt unit rule");
  }return c;
 }
 internal static bool RosterMatches(HuntDefinition d,IEnumerable<string> ids)=>d.Roster.SequenceEqual(ids.OrderBy(x=>x));
 internal static HuntUnitRule Unit(HuntDefinition d,HuntOverride c,ExpansionFile legacy,string id)
 {
  var u=c.Units.FirstOrDefault(x=>x.Id==id)??d.Units.Single(x=>x.Id==id);
  if(d.Id!="sun-kings-retinue"||c.Units.Any(x=>x.Id==id))return u;
  return new(){Id=id,Behavior=u.Behavior,HpMultiplier=id=="vd06_00_02"?legacy.SunKingHpMultiplier:legacy.SorcererHpMultiplier};
 }
 internal static int Bonus(HuntDefinition d,HuntOverride c,ExpansionFile legacy)=>c.VictoryBonusPotch??(d.Id=="sun-kings-retinue"?legacy.VictoryBonusPotch:d.VictoryBonusPotch);
 internal static double Damage(HuntDefinition d,HuntOverride c,ExpansionFile legacy)=>c.DamageMultiplier??(d.Id=="sun-kings-retinue"?legacy.HuntDamageMultiplier:d.DamageMultiplier);
}
