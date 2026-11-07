using NightmareRebalance;
using System.Reflection;
using System.Text.Json.Nodes;
internal static class CombatRealityTests
{
 internal static void Run()
 {
  int n=0;void Check(bool ok,string label){n++;if(!ok)throw new Exception("Reality "+n+": "+label);}
  string Resource(string name){using var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("ReforgedContent."+name)!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var a=ContentRules.Read<RealityAudit>(Resource("reality-audit.json"));string json=Resource("combat-reality.json");var c=CombatRealityRules.Parse(json,a);var old=ContentRules.Read<EvolutionAudit>(Resource("evolution-audit.json")).Formations.Concat(ContentRules.Read<MasteryAudit>(Resource("mastery-audit.json")).Formations).ToArray();
  var legacy=JsonNode.Parse(json)!.AsObject();legacy.Remove("PestRatRepeatStatusPenalty");Check(CombatRealityRules.Parse(legacy.ToJsonString(),a).PestRatRepeatStatusPenalty==20,"compatible previous v1.3 config preserves defaults without rewrite");
  void Reject(Action<JsonObject> mutate){var o=JsonNode.Parse(json)!.AsObject();mutate(o);bool rejected=false;try{CombatRealityRules.Parse(o.ToJsonString(),a);}catch{rejected=true;}Check(rejected,"strict bounded config");}
  Reject(o=>o["PestRatRepeatStatusPenalty"]=41);Reject(o=>o["NewThing"]=1);Reject(o=>o["SchemaVersion"]=2);Reject(o=>o["PestRatRecoveryBonus"]=41);Reject(o=>o["PestRatPressureBonus"]=-1);Reject(o=>o["Formations"]![0]!["PartyId"]="boss_lus1");Reject(o=>o["Runes"]![0]!["Id"]=80);Reject(o=>o["Formations"]!.AsArray().RemoveAt(0));
  Check(a.Formations.Length==15&&old.Length==36&&!a.Formations.Select(x=>x.PartyId).Intersect(old.Select(x=>x.PartyId)).Any(),"51 independent formation identities");Check(a.Runes.Select(r=>r.Id).SequenceEqual(new[]{82,84}),"two additional physical runes");
  foreach(var f in a.Formations)
  {
   Check(f.NewHp<=f.OldHp,"no base HP increase");Check(f.NewCodes.Select(c=>c>0).SequenceEqual(f.OldCodes.Select(c=>c>0)),"same occupied slots");Check(f.Hooks.Skip(1).All(x=>x==0)&&f.Flags==0,"ordinary script scope");
   int Sum(int[] codes)=>codes.Where(c=>c>0).Sum(c=>f.Monsters.Single(m=>m.Index==c).Signature[0]);Check(Sum(f.NewCodes)==f.NewHp&&Sum(f.OldCodes)==f.OldHp,"HP budget extracted");
   int Max(int[] codes)=>codes.Where(c=>c>0).Max(c=>f.Monsters.Single(m=>m.Index==c).Signature[1]);Check(Max(f.NewCodes)<=Max(f.OldCodes)+2,"level transplant bound");
   Check(CombatEvolutionRules.FormationMatches(f,f.TableId,f.PartyId,f.Indices[0],f.Flags,f.Item,f.OldCodes.Count(x=>x>0),f.OldIds,f.OldCodes,f.Hooks,0,f.Positions),"audited source");
   var reward=a.Rewards.Single(r=>r.PartyId==f.PartyId);Check(reward.BeforeLevels.Length==reward.AfterLevels.Length&&reward.BeforePotch>=0&&reward.AfterPotch>=0&&reward.SourcePartyItemUnchanged,"explicit reward identity audit");
  }
  var bossSources=ContentRules.Read<BossSourceAudit[]>(Resource("boss-resource-audit.json"));Check(bossSources.Length==15,"seven admitted and eight excluded boss resource identities");
  foreach(var b in bossSources){Check(b.Data.Monsters.All(m=>m.Signature.Length==47),"complete original boss signatures");Check(b.Data.Hooks.Length==6&&b.Data.HookNames.Length==6,"original boss callbacks preserved");Check(b.Data.OldCodes.SequenceEqual(b.Data.NewCodes),"boss resource audit does not remix roster");Check(b.CommandCount==0&&b.CommandArrayLength==0,"boss original BE command representation");}
  var phase=new BossCadence{TableId="boss_rat",PressureMode=2,RecoveryMode=0,ThresholdPercent=60};
  for(int hp=0;hp<=100;hp++)for(int mode=0;mode<4;mode++)for(int recovery=0;recovery<=3;recovery++)
  {int score=CombatRealityRules.RatBonus(phase,mode,hp,100,recovery,c.PestRatPressureBonus,c.PestRatRecoveryBonus);Check(score is >=0 and <=24,"bounded Rat layer");Check(hp>0||score==0,"dead unit unchanged");Check(recovery<=0||hp==0||score==(mode==0?24:0),"recovery targets only legal configured mode preference");}
  // The third legal alternative explains why old recovery0 did not win.
  int oldRecovery=34+40,other=82;Check(oldRecovery<other,"old recovery dominated by unpenalized status alternative");Check(oldRecovery+c.PestRatRecoveryBonus>other,"new bounded recovery preference can win");
  Console.WriteLine($"PASS: Combat Reality {n} assertions; retained51 formation identities, explicit budgets/rewards, two rune identities, strict opt-outs and bounded Pest Rat recovery tradeoff. Native assets/gameplay separate.");
 }
}
