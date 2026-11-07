using System.Text.Json;
namespace NightmareRebalance;
internal sealed class PhysicalRuneAudit{public int Id{get;set;}public int Effect{get;set;}public int[] Signature{get;set;}=Array.Empty<int>();}
internal sealed class MasteryAudit{public FormationAudit[] Formations{get;set;}=Array.Empty<FormationAudit>();public PhysicalRuneAudit[] Runes{get;set;}=Array.Empty<PhysicalRuneAudit>();public UniteAudit[] Unites{get;set;}=Array.Empty<UniteAudit>();}
internal sealed class BossCadence{public string TableId{get;set;}="";public bool Enabled{get;set;}=true;public int ThresholdPercent{get;set;}=60;public int PressureMode{get;set;}public int RecoveryMode{get;set;}public int RecoverySelections{get;set;}=1;}
internal sealed class CombatMasteryFile
{
 public int SchemaVersion{get;set;}=1;public bool Enabled{get;set;}=true;public bool EnableFormations{get;set;}=true;public bool EnablePhysicalRunes{get;set;}=true;public bool EnableUniteOptions{get;set;}=true;public bool EnableBossCadence{get;set;}=true;public int CadencePreference{get;set;}=40;
 public EvolutionSwitch[] Formations{get;set;}=Array.Empty<EvolutionSwitch>();public EvolutionUniteSwitch[] Runes{get;set;}=Array.Empty<EvolutionUniteSwitch>();public EvolutionUniteSwitch[] Unites{get;set;}=Array.Empty<EvolutionUniteSwitch>();public BossCadence[] BossCadence{get;set;}=Array.Empty<BossCadence>();
}
internal static class CombatMasteryRules
{
 internal static CombatMasteryFile Parse(string json,MasteryAudit audit,CombatBossAudit[] bosses)
 {
  if(json.Length>65536)throw new JsonException("Mastery config exceeds 64 KiB.");using var doc=JsonDocument.Parse(json);
  OverhaulRules.Keys(doc.RootElement,"SchemaVersion","Enabled","EnableFormations","EnablePhysicalRunes","EnableUniteOptions","EnableBossCadence","CadencePreference","Formations","Runes","Unites","BossCadence");
  foreach(var x in doc.RootElement.GetProperty("Formations").EnumerateArray())OverhaulRules.Keys(x,"PartyId","Enabled");
  foreach(string kind in new[]{"Runes","Unites"})foreach(var x in doc.RootElement.GetProperty(kind).EnumerateArray())OverhaulRules.Keys(x,"Id","Enabled");
  foreach(var x in doc.RootElement.GetProperty("BossCadence").EnumerateArray())OverhaulRules.Keys(x,"TableId","Enabled","ThresholdPercent","PressureMode","RecoveryMode","RecoverySelections");
  var c=ContentRules.Read<CombatMasteryFile>(json);bool Same<T>(IEnumerable<T> a,IEnumerable<T> b)=>a.OrderBy(x=>x).SequenceEqual(b.OrderBy(x=>x));
  if(c.SchemaVersion!=1||c.CadencePreference is <0 or >60||c.Formations==null||c.Runes==null||c.Unites==null||c.BossCadence==null)throw new JsonException("Invalid Mastery schema/bounds.");
  if(c.Formations.Any(x=>x==null)||!Same(c.Formations.Select(x=>x.PartyId),audit.Formations.Select(x=>x.PartyId))||c.Runes.Any(x=>x==null)||!Same(c.Runes.Select(x=>x.Id),audit.Runes.Select(x=>x.Id))||c.Unites.Any(x=>x==null)||!Same(c.Unites.Select(x=>x.Id),audit.Unites.Select(x=>x.Id))||c.BossCadence.Any(x=>x==null)||!Same(c.BossCadence.Select(x=>x.TableId),bosses.Select(x=>x.TableId)))throw new JsonException("Unsupported/duplicate Mastery identities.");
  foreach(var x in c.BossCadence){var b=bosses.Single(b=>b.TableId==x.TableId);bool Mode(int n)=>n is >=0 and <=3&&b.Signature[3+n*11+1]>0;if(x.ThresholdPercent is <20 or >80||x.RecoverySelections is <1 or >3||x.PressureMode==x.RecoveryMode||!Mode(x.PressureMode)||!Mode(x.RecoveryMode))throw new JsonException("Cadence requires distinct audited native moves and bounded timing.");}
  return c;
 }
 internal static bool AttackLayout(int slots,int declared)=>slots==4&&declared is 0 or 4;
 internal static int CadenceBonus(BossCadence c,int mode,int hp,int max,int recovery,int preference)
 {
  if(!c.Enabled||hp<=0||max<=0)return 0;int n=Math.Clamp(preference,0,60);
  if(recovery>0)return mode==c.RecoveryMode?n:mode==c.PressureMode?-n:0;
  return (long)hp*100<=(long)max*c.ThresholdPercent&&mode==c.PressureMode?n:0;
 }
 internal static int NextRecovery(BossCadence c,int selected,int current)=>selected==c.PressureMode?c.RecoverySelections:Math.Max(0,current-1);
 // Native PlayerCalcSpcDamage: normal physical damage, then special resistance, then eff_val/100.
 internal static int PhysicalRuneDamage(int nativePhysical,int resistance,int effect)
 {int n=resistance==1?nativePhysical*2:resistance==2?nativePhysical/2:resistance==3?0:nativePhysical;return Math.Max(0,n*effect/100);}
}
