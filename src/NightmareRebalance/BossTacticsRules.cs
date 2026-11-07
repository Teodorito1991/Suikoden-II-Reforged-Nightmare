using System.Text.Json;
namespace NightmareRebalance;
internal sealed class BossPattern
{
 public string TableId{get;set;}="";public string EnemyId{get;set;}="";public bool Enabled{get;set;}=true;
 public int ThresholdPercent{get;set;}=60;public int Period{get;set;}=3;public int PressureMode{get;set;}public int RecoveryMode{get;set;}public int AdaptiveMode{get;set;}
}
internal sealed class BossTacticsFile
{
 public int SchemaVersion{get;set;}=1;public bool Enabled{get;set;}=true;public bool EnableMajorBosses{get;set;}=false;public bool EnableExistingBosses{get;set;}=true;public int Preference{get;set;}=110;
 public BossPattern[] Patterns{get;set;}=Array.Empty<BossPattern>();
}
internal static class BossTacticsRules
{
 internal static bool Major(string table)=>table is "boss_lus1" or "boss_lus2" or "boss_nec3";
 internal static BossTacticsFile Parse(string text,FormationAudit[] audit)
 {
  if(text.Length>65536)throw new JsonException("Boss tactics exceeds64KiB.");using var doc=JsonDocument.Parse(text);
  OverhaulRules.Keys(doc.RootElement,"SchemaVersion","Enabled","EnableMajorBosses","EnableExistingBosses","Preference","Patterns");
  foreach(var x in doc.RootElement.GetProperty("Patterns").EnumerateArray())OverhaulRules.Keys(x,"TableId","EnemyId","Enabled","ThresholdPercent","Period","PressureMode","RecoveryMode","AdaptiveMode");
  var c=ContentRules.Read<BossTacticsFile>(text);
  if(c.SchemaVersion!=1||c.Preference is <0 or >120||c.Patterns==null||c.Patterns.Any(x=>x==null)||!c.Patterns.Select(x=>x.TableId).OrderBy(x=>x).SequenceEqual(audit.Select(x=>x.TableId).OrderBy(x=>x)))throw new JsonException("Unexpected boss identities/schema/preference.");
  foreach(var x in c.Patterns){var a=audit.Single(a=>a.TableId==x.TableId);var m=a.Monsters.FirstOrDefault(m=>m.Id==x.EnemyId);bool Mode(int n)=>n is >=0 and <4&&m?.Signature.Length==47&&m.Signature[4+n*11]>0&&m.Signature[6+n*11]>0;
   if(x.ThresholdPercent is <25 or >75||x.Period is <3 or >5||x.PressureMode==x.RecoveryMode||!Mode(x.PressureMode)||!Mode(x.RecoveryMode)||!Mode(x.AdaptiveMode))throw new JsonException("Pattern requires original damaging positive-weight moves and bounded cadence.");}
  return c;
 }
 // battle_kaisuu is native-maintained. Merely evaluating more choices cannot advance it.
 internal static string Window(BossPattern p,int counter,int hp,int max)
 {
  if(hp<=0||max<=0)return "Inactive";int n=Math.Max(0,counter)%p.Period;
  return n==0&&((long)hp*100<=(long)max*p.ThresholdPercent)?"Pressure":n==1?"Recovery":"Adaptive";
 }
 internal static int Score(BossPattern p,int preference,int counter,int hp,int max,CombatAction action,IReadOnlyList<CombatTarget> targets,int prior)
 {
  if(prior==int.MinValue||!action.Legal)return int.MinValue;
  int n=Math.Clamp(preference,0,120);string window=Window(p,counter,hp,max);
  int desired=window=="Pressure"?p.PressureMode:window=="Recovery"?p.RecoveryMode:p.AdaptiveMode;
  int score=prior/4+(action.Mode==desired?(window=="Adaptive"?n/3:n):0);
  // A recovery window reduces the appeal of the native pressure move, not damage itself.
  if(window=="Recovery"&&action.Mode==p.PressureMode)score-=n;
  if(action.Status!=0&&targets.Count>0){int useful=targets.Count(t=>t.StatusUseful);score+=useful==0?-n:Math.Min(24,useful*4);}
  if(targets.Count>0){int defense=targets.Sum(t=>Math.Clamp(t.Defense,0,999))/targets.Count,magic=targets.Sum(t=>Math.Clamp(t.MagicDefense,0,999))/targets.Count;int gap=action.Physical?magic-defense:defense-magic;score+=Math.Clamp(gap,-60,60)/3;if(window=="Adaptive"&&gap>=30)score+=n;}
  return score;
 }
}
