using NightmareRebalance;
using System.Text.Json;
internal static class BossTacticsTests
{
 internal static void Run(){int count=0;void Check(bool x,string m){count++;if(!x)throw new Exception("Boss tactics "+m);}string Read(string n){using var s=typeof(BossTacticsTests).Assembly.GetManifestResourceStream("ReforgedContent."+n)!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var audit=ContentRules.Read<FormationAudit[]>(Read("boss-tactics-audit.json"));var c=BossTacticsRules.Parse(Read("boss-tactics.json"),audit);Check(!c.EnableMajorBosses&&c.EnableExistingBosses,"Unplayed major tactics require explicit opt-in on fresh installs");Check(c.Patterns.Length==10,"Ten audited patterns");Check(c.Patterns.Count(x=>BossTacticsRules.Major(x.TableId))==3,"Only three major AI additions");
  foreach(var p in c.Patterns){var a=audit.Single(x=>x.TableId==p.TableId);var m=a.Monsters.Single(x=>x.Id==p.EnemyId);Check(m.Signature.Length==47,"Full original attack signature");
   for(int counter=0;counter<256;counter++){Check(BossTacticsRules.Window(p,counter,100,100)==(counter%p.Period==1?"Recovery":"Adaptive"),"No opening healthy pressure");Check(BossTacticsRules.Window(p,counter,50,100)==(counter%p.Period==0?"Pressure":counter%p.Period==1?"Recovery":"Adaptive"),"Wounded cadence");}
   var targets=new[]{new CombatTarget(0,500,600,150,40,80,120,0)};
   foreach(int mode in new[]{p.PressureMode,p.RecoveryMode,p.AdaptiveMode}){var action=new CombatAction(mode,1,100,true,false,0,100,255,true);Check(BossTacticsRules.Score(p,110,1,40,100,action,targets,100)!=int.MinValue,"Legal scoring");Check(BossTacticsRules.Score(p,110,1,40,100,action with{Legal=false},targets,100)==int.MinValue,"Never creates legal moves");}
   Check(a.Hooks[3]>0,"Native ending callback retained");
  }
  void Invalid(Action<BossTacticsFile> change){var q=BossTacticsRules.Parse(Read("boss-tactics.json"),audit);change(q);bool bad=false;try{BossTacticsRules.Parse(JsonSerializer.Serialize(q),audit);}catch(JsonException){bad=true;}Check(bad,"Invalid configuration rejected");}
  Invalid(x=>x.Preference=121);Invalid(x=>x.Patterns[0].PressureMode=3);Invalid(x=>x.Patterns[7].AdaptiveMode=1);Invalid(x=>x.Patterns[0].Period=1);Invalid(x=>x.Patterns[0].TableId="boss_nec1");
  Console.WriteLine($"PASS: Boss Reforged {count} assertions; native-counter cadence, original positive-weight moves, opt-out bounds. No boss gameplay claim.");
 }
}
