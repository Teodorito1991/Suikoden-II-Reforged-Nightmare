using NightmareRebalance;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
internal static class CombatEvolutionTests
{
 internal static void Run()
 {
  int n=0;void Check(bool v){n++;if(!v)throw new Exception("Evolution assertion "+n);}
  string Resource(string name){using var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("ReforgedContent."+name)!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var a=ContentRules.Read<EvolutionAudit>(Resource("evolution-audit.json"));var old=ContentRules.Read<CombatAudit>(Resource("combat-audit.json"));string json=Resource("combat-evolution.json");var cfg=CombatEvolutionRules.Parse(json,a,old);
  void Reject(Action<JsonObject> mutate){var o=JsonNode.Parse(json)!.AsObject();mutate(o);bool failed=false;try{CombatEvolutionRules.Parse(o.ToJsonString(),a,old);}catch{failed=true;}Check(failed);}
  Reject(o=>o["SchemaVersion"]=2);Reject(o=>o["PhasePreference"]=-1);Reject(o=>o["PhasePreference"]=51);Reject(o=>o["Surprise"]=true);Reject(o=>o["Formations"]![0]!["PartyId"]="boss_lus2_00");Reject(o=>o["BossPhases"]![0]!["HealthyMode"]=3);Reject(o=>o["BossPhases"]![0]!["ThresholdPercent"]=99);Reject(o=>o["AdditionalBosses"]![0]!["TableId"]="boss_nec2");Reject(o=>o["Unites"]![0]!["Id"]=45);Reject(o=>o["Unites"]!.AsArray().RemoveAt(0));
  Check(a.Formations.Length==16&&a.Bosses.Length==3&&a.Unites.Length==5);Check(a.Bosses.All(b=>b.HpMultiplier==1&&b.DamageMultiplier==1));
  foreach(var f in a.Formations)
  {
   bool Matches(string? table=null,string? party=null,int? index=null,uint? flags=null,int? item=null,int? count=null,int[]? ids=null,int[]? codes=null,int[]? hooks=null,int commands=0,int[]? positions=null)=>CombatEvolutionRules.FormationMatches(f,table??f.TableId,party??f.PartyId,index??f.Indices[0],flags??f.Flags,item??f.Item,count??f.OldCodes.Count(c=>c>0),ids??f.OldIds,codes??f.OldCodes,hooks??f.Hooks,commands,positions??f.Positions);
   Check(Matches());Check(!Matches(table:"gsd1"));Check(!Matches(party:"boss_luc1"));Check(!Matches(index:-1));Check(!Matches(flags:1));Check(!Matches(item:f.Item+1));Check(!Matches(count:0));Check(!Matches(commands:1));
   foreach(var pair in new[]{(f.OldIds,0),(f.OldCodes,1),(f.Hooks,2),(f.Positions,3)})for(int i=0;i<pair.Item1.Length;i++)
   {var changed=(int[])pair.Item1.Clone();changed[i]++;Check(!Matches(ids:pair.Item2==0?changed:null,codes:pair.Item2==1?changed:null,hooks:pair.Item2==2?changed:null,positions:pair.Item2==3?changed:null));}
   Check(f.NewCodes.Length==6&&f.OldCodes.Select(x=>x>0).SequenceEqual(f.NewCodes.Select(x=>x>0)));Check(f.NewHp<=f.OldHp*1.1);Check(f.Hooks.Skip(1).All(h=>h==0));
   Check(CombatEvolutionRules.Types(f.NewCodes).Where(x=>x>0).OrderBy(x=>x).SequenceEqual(f.NewCodes.Where(x=>x>0).Distinct().OrderBy(x=>x)));
   foreach(int code in f.NewCodes.Where(x=>x>0))Check(f.Monsters.Any(m=>m.Index==code&&m.Signature.Length==47));
  }
  foreach(var phase in cfg.BossPhases)
  for(int hp=0;hp<=100;hp++)foreach(int mode in Enumerable.Range(0,4))
  {int score=CombatEvolutionRules.PhaseBonus(phase,mode,hp,100,cfg.PhasePreference);Check(score is >=0 and <=24);Check(hp>0||score==0);Check(hp<=0||CombatEvolutionRules.Style(phase,hp,100)==(hp<=phase.ThresholdPercent?phase.WoundedBehavior:phase.HealthyBehavior));}
  foreach(var u in a.Unites){Check(u.Effect>u.Signature[6]);Check(u.Signature.Length==11+u.Signature[4]);Check(u.Id is 12 or 13 or 14 or 17 or 18);}
  Console.WriteLine($"PASS: Combat Evolution {n} assertions; exact formation boundaries, preserved spawn slots/HP budget, boss action scope, guarded Unite identities and strict independent configuration.");
 }
}
