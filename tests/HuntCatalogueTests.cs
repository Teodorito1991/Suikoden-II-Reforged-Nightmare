using NightmareRebalance;
using System.Text.Json;
internal static class HuntCatalogueTests
{
 internal static void Run()
 {
  int n=0;void Check(bool value,string label){n++;if(!value)throw new Exception("Hunt catalogue: "+label);}
  string Resource(string name){using var s=typeof(HuntCatalogueTests).Assembly.GetManifestResourceStream("ReforgedContent."+name)!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var definitions=ContentRules.Read<HuntDefinition[]>(Resource("hunt-catalogue-audit.json"));var config=HuntCatalogueRules.Parse(Resource("hunt-catalogue.json"),definitions);var legacy=ExpansionRules.Parse(Resource("first-expansion.json"));
  void Invalid(Action<HuntCatalogueFile> edit){var c=HuntCatalogueRules.Parse(Resource("hunt-catalogue.json"),definitions);edit(c);bool bad=false;try{HuntCatalogueRules.Parse(JsonSerializer.Serialize(c),definitions);}catch(JsonException){bad=true;}Check(bad,"Invalid configuration rejected");}
  Check(definitions.Length==3&&definitions.Select(x=>x.Audit.TableId).Distinct().Count()==3,"Three distinct native tables");Check(config.SelectedHuntId=="sun-kings-retinue"&&!legacy.EnableExperimentalHunt,"Preserved Sun selection and distributed global opt-in");
  Check(config.Hunts[0].Enabled&&config.Hunts.Skip(1).All(x=>!x.Enabled),"Unplayed contracts disabled on fresh installs");
  var previous=ContentRules.Read<FormationAudit>(Resource("hunt-audit.json"));Check(JsonSerializer.Serialize(previous)==JsonSerializer.Serialize(definitions[0].Audit),"Sun source audit byte-equivalent serialized values");
  foreach(var d in definitions){var a=d.Audit;var c=config.Hunts.Single(x=>x.Id==d.Id);
   Check(a.Indices.Length==1&&a.Indices[0]<d.PartyCount,"Exact source index");Check(a.NewCodes.Count(x=>x>0)==a.OldCodes.Count(x=>x>0)&&a.NewCodes.Select(x=>x>0).SequenceEqual(a.OldCodes.Select(x=>x>0)),"Occupied slots and count preserved");
   Check(a.Hooks.Skip(1).All(x=>x==0)&&a.Item==0&&a.Flags==0,"No outcome scripts or drops replaced");Check(a.Monsters.All(m=>m.Signature.Length==47),"Full source signatures");Check(d.Roster.Length==a.NewCodes.Count(x=>x>0)&&HuntCatalogueRules.RosterMatches(d,d.Roster),"Duplicate-aware roster accepted");Check(!HuntCatalogueRules.RosterMatches(d,d.Roster.Take(d.Roster.Length-1)),"Missing unit rejected");Check(!HuntCatalogueRules.RosterMatches(d,d.Roster.Append(d.Roster[0])),"Extra unit rejected");
   Check(HuntCatalogueRules.Bonus(d,c,legacy)==d.VictoryBonusPotch,"Default bonus");c.VictoryBonusPotch=5432;Check(HuntCatalogueRules.Bonus(d,c,legacy)==5432,"Explicit bonus priority");c.VictoryBonusPotch=0;Check(HuntCatalogueRules.Bonus(d,c,legacy)==0,"Zero bonus opt-out");
   c.DamageMultiplier=0.75;Check(HuntCatalogueRules.Damage(d,c,legacy)==0.75,"Independent damage override");
   foreach(var u in d.Units){Check(a.Monsters.Any(m=>m.Id==u.Id)&&a.NewCodes.Any(code=>a.Monsters.Any(m=>m.Index==code&&m.Id==u.Id)),"Only roster members configured");Check(HuntCatalogueRules.Unit(d,c,legacy,u.Id).HpMultiplier==u.HpMultiplier,"Default factor");var custom=new HuntUnitRule{Id=u.Id,HpMultiplier=0.8,Behavior="Cautious"};c.Units=new[]{custom};Check(HuntCatalogueRules.Unit(d,c,legacy,u.Id)==custom,"Explicit unit priority");c.Units=Array.Empty<HuntUnitRule>();}
  }
  legacy.SunKingHpMultiplier=2.3;legacy.SorcererHpMultiplier=1.7;legacy.HuntDamageMultiplier=1.9;legacy.VictoryBonusPotch=7777;var sun=definitions[0];var sc=HuntCatalogueRules.Parse(Resource("hunt-catalogue.json"),definitions).Hunts[0];Check(HuntCatalogueRules.Bonus(sun,sc,legacy)==7777&&HuntCatalogueRules.Damage(sun,sc,legacy)==1.9&&HuntCatalogueRules.Unit(sun,sc,legacy,"vd06_00_02").HpMultiplier==2.3&&HuntCatalogueRules.Unit(sun,sc,legacy,"vd06_00_03").HpMultiplier==1.7,"Existing user settings inherited");
  Invalid(c=>c.SelectedHuntId="unknown");Invalid(c=>c.SchemaVersion=2);Invalid(c=>c.Hunts=c.Hunts.Take(2).ToArray());Invalid(c=>c.Hunts[1].Id=c.Hunts[0].Id);Invalid(c=>c.Hunts[0].VictoryBonusPotch=-1);Invalid(c=>c.Hunts[0].VictoryBonusPotch=1000001);Invalid(c=>c.Hunts[0].DamageMultiplier=0);Invalid(c=>c.Hunts[0].DamageMultiplier=6);Invalid(c=>c.Hunts[0].Units=new[]{new HuntUnitRule{Id="foreign",HpMultiplier=1,Behavior="Cautious"}});Invalid(c=>c.Hunts[0].Units=new[]{new HuntUnitRule{Id="vd06_00_02",HpMultiplier=1,Behavior="Unknown"}});Invalid(c=>c.Hunts[0].Units=new[]{new HuntUnitRule{Id="vd06_00_02",HpMultiplier=6,Behavior="Caster"}});Invalid(c=>c.Hunts[0].Units=new[]{new HuntUnitRule{Id="vd06_00_02",HpMultiplier=1,Behavior="Caster"},new HuntUnitRule{Id="vd06_00_02",HpMultiplier=1,Behavior="Caster"}});
  bool unknown=false;try{HuntCatalogueRules.Parse(Resource("hunt-catalogue.json").Replace("\"SchemaVersion\"","\"Unknown\""),definitions);}catch(JsonException){unknown=true;}Check(unknown,"Unknown keys fail closed");
  var poison=definitions.Single(x=>x.Id=="thornwake-patrol");Check(poison.Roster.Count(x=>x=="ve03_00_03")==2&&!HuntCatalogueRules.RosterMatches(poison,poison.Roster.Distinct()),"Duplicate Holly Elves require two admitted instances");
  Console.WriteLine($"PASS: Hunt catalogue {n} assertions; native definitions, exact rosters, legacy settings, overrides and fail-closed configuration. No rendered battle acceptance.");
 }
}
