using System.Text.Json;
namespace NightmareRebalance;
internal sealed class EvolutionAudit
{
 public FormationAudit[] Formations{get;set;}=Array.Empty<FormationAudit>();public CombatBossAudit[] Bosses{get;set;}=Array.Empty<CombatBossAudit>();public UniteAudit[] Unites{get;set;}=Array.Empty<UniteAudit>();
}
internal sealed class FormationMonster{public int Index{get;set;}public string Id{get;set;}="";public int[] Signature{get;set;}=Array.Empty<int>();}
internal sealed class FormationAudit
{
 public string TableId{get;set;}="";public string PartyId{get;set;}="";public int[] Indices{get;set;}=Array.Empty<int>();public int[] OldCodes{get;set;}=Array.Empty<int>();public int[] OldIds{get;set;}=Array.Empty<int>();public int[] NewCodes{get;set;}=Array.Empty<int>();public uint Flags{get;set;}public int Item{get;set;}public int[] Hooks{get;set;}=Array.Empty<int>();public string[] HookNames{get;set;}=Array.Empty<string>();public int[] Positions{get;set;}=Array.Empty<int>();public FormationMonster[] Monsters{get;set;}=Array.Empty<FormationMonster>();public int OldHp{get;set;}public int NewHp{get;set;}
}
internal sealed class UniteAudit{public int Id{get;set;}public int Effect{get;set;}public int[] Signature{get;set;}=Array.Empty<int>();}
internal sealed class EvolutionSwitch{public string PartyId{get;set;}="";public bool Enabled{get;set;}=true;}
internal sealed class EvolutionBossSwitch{public string TableId{get;set;}="";public bool Enabled{get;set;}=true;}
internal sealed class EvolutionUniteSwitch{public int Id{get;set;}public bool Enabled{get;set;}=true;}
internal sealed class BossPhase
{
 public string TableId{get;set;}="";public bool Enabled{get;set;}=true;public int ThresholdPercent{get;set;}=50;public int HealthyMode{get;set;}public int WoundedMode{get;set;}public string HealthyBehavior{get;set;}="";public string WoundedBehavior{get;set;}="";
}
internal sealed class CombatEvolutionFile
{
 public int SchemaVersion{get;set;}=1;public bool Enabled{get;set;}=true;public bool EnableFormations{get;set;}=true;public bool EnableBossPhases{get;set;}=true;public bool EnableAdditionalBosses{get;set;}=true;public bool EnableUniteOptions{get;set;}=true;public int PhasePreference{get;set;}=24;public EvolutionSwitch[] Formations{get;set;}=Array.Empty<EvolutionSwitch>();public EvolutionBossSwitch[] AdditionalBosses{get;set;}=Array.Empty<EvolutionBossSwitch>();public EvolutionUniteSwitch[] Unites{get;set;}=Array.Empty<EvolutionUniteSwitch>();public BossPhase[] BossPhases{get;set;}=Array.Empty<BossPhase>();
}
internal static class CombatEvolutionRules
{
 internal static CombatEvolutionFile Parse(string json,EvolutionAudit audit,CombatAudit previous)
 {
  if(json.Length>65536)throw new JsonException("Evolution config exceeds 64 KiB.");using var doc=JsonDocument.Parse(json);
  OverhaulRules.Keys(doc.RootElement,"SchemaVersion","Enabled","EnableFormations","EnableBossPhases","EnableAdditionalBosses","EnableUniteOptions","PhasePreference","Formations","AdditionalBosses","Unites","BossPhases");
  foreach(var x in doc.RootElement.GetProperty("Formations").EnumerateArray())OverhaulRules.Keys(x,"PartyId","Enabled");
  foreach(var x in doc.RootElement.GetProperty("AdditionalBosses").EnumerateArray())OverhaulRules.Keys(x,"TableId","Enabled");
  foreach(var x in doc.RootElement.GetProperty("Unites").EnumerateArray())OverhaulRules.Keys(x,"Id","Enabled");
  foreach(var x in doc.RootElement.GetProperty("BossPhases").EnumerateArray())OverhaulRules.Keys(x,"TableId","Enabled","ThresholdPercent","HealthyMode","WoundedMode","HealthyBehavior","WoundedBehavior");
  var c=ContentRules.Read<CombatEvolutionFile>(json);
  bool Same<T>(IEnumerable<T> a,IEnumerable<T> b)=>a.OrderBy(x=>x).SequenceEqual(b.OrderBy(x=>x));
  if(c.SchemaVersion!=1||c.PhasePreference is <0 or >50||c.Formations==null||c.AdditionalBosses==null||c.Unites==null||c.BossPhases==null)throw new JsonException("Invalid evolution schema/bounds.");
  if(c.Formations.Any(x=>x==null)||!Same(c.Formations.Select(x=>x.PartyId),audit.Formations.Select(x=>x.PartyId))||c.AdditionalBosses.Any(x=>x==null)||!Same(c.AdditionalBosses.Select(x=>x.TableId),audit.Bosses.Select(x=>x.TableId))||c.Unites.Any(x=>x==null)||!Same(c.Unites.Select(x=>x.Id),audit.Unites.Select(x=>x.Id)))throw new JsonException("Unsupported or duplicate content identity.");
  var bosses=previous.Bosses.Concat(audit.Bosses).ToArray();
  if(c.BossPhases.Any(x=>x==null)||!Same(c.BossPhases.Select(x=>x.TableId),bosses.Select(x=>x.TableId)))throw new JsonException("Unsupported boss phase identity.");
  var styles=new[]{"Aggressive","Caster","Cautious","Disruptor","Opportunist","Alternating","Attrition"};
  foreach(var x in c.BossPhases){var b=bosses.Single(b=>b.TableId==x.TableId);bool Mode(int n)=>n is >=0 and <=3&&b.Signature[3+n*11+1]>0;if(x.ThresholdPercent is <20 or >80||!Mode(x.HealthyMode)||!Mode(x.WoundedMode)||!styles.Contains(x.HealthyBehavior)||!styles.Contains(x.WoundedBehavior))throw new JsonException("Boss phase must use audited positive-probability moves and bounded thresholds.");}
  return c;
 }
 internal static bool Wounded(BossPhase phase,int hp,int max)=>max>0&&hp>0&&(long)hp*100<=(long)max*phase.ThresholdPercent;
 internal static int PhaseBonus(BossPhase phase,int mode,int hp,int max,int preference)=>hp<=0||max<=0||!phase.Enabled?0:mode==(Wounded(phase,hp,max)?phase.WoundedMode:phase.HealthyMode)?Math.Clamp(preference,0,50):0;
 internal static string Style(BossPhase phase,int hp,int max)=>Wounded(phase,hp,max)?phase.WoundedBehavior:phase.HealthyBehavior;
 internal static int[] Types(int[] codes)=>codes.Where(c=>c>0).Distinct().Concat(Enumerable.Repeat(0,6)).Take(6).ToArray();
 internal static bool FormationMatches(FormationAudit a,string table,string party,int index,uint flags,int item,int count,int[] ids,int[] codes,int[] hooks,int commands,int[] positions)=>a.TableId==table&&a.PartyId==party&&a.Indices.Contains(index)&&flags==a.Flags&&item==a.Item&&count==a.OldCodes.Count(c=>c>0)&&ids.SequenceEqual(a.OldIds)&&codes.SequenceEqual(a.OldCodes)&&hooks.SequenceEqual(a.Hooks)&&commands==0&&positions.SequenceEqual(a.Positions);
}
