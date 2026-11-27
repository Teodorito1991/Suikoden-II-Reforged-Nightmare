using System.Text.Json;
namespace NightmareRebalance;
internal sealed class BossSourceAudit{public FormationAudit Data{get;set;}=new();public int CommandCount{get;set;}public int CommandArrayLength{get;set;}}
internal sealed class RealityRewardAudit{public string PartyId{get;set;}="";public int BeforePotch{get;set;}public int AfterPotch{get;set;}public int[] BeforeLevels{get;set;}=Array.Empty<int>();public int[] AfterLevels{get;set;}=Array.Empty<int>();public bool SourcePartyItemUnchanged{get;set;}public bool EnemyDropIdentitiesMayChange{get;set;}}
internal sealed class RealityAudit{public FormationAudit[] Formations{get;set;}=Array.Empty<FormationAudit>();public PhysicalRuneAudit[] Runes{get;set;}=Array.Empty<PhysicalRuneAudit>();public RealityRewardAudit[] Rewards{get;set;}=Array.Empty<RealityRewardAudit>();public int[] RatParameters{get;set;}=Array.Empty<int>();}
internal sealed class CombatRealityFile
{
 public int SchemaVersion{get;set;}=1;public bool Enabled{get;set;}=true;public bool EnableFormations{get;set;}=true;public bool EnablePhysicalRunes{get;set;}=true;public bool EnablePestRatTactics{get;set;}=true;public int PestRatPressureBonus{get;set;}=12;public int PestRatRecoveryBonus{get;set;}=24;public int PestRatRepeatStatusPenalty{get;set;}=20;public EvolutionSwitch[] Formations{get;set;}=Array.Empty<EvolutionSwitch>();public EvolutionUniteSwitch[] Runes{get;set;}=Array.Empty<EvolutionUniteSwitch>();
}
internal static class CombatRealityRules
{
 internal static CombatRealityFile Parse(string json,RealityAudit a)
 {
  if(json.Length>65536)throw new JsonException("Reality config too large.");using var d=JsonDocument.Parse(json);var keys=new[]{"SchemaVersion","Enabled","EnableFormations","EnablePhysicalRunes","EnablePestRatTactics","PestRatPressureBonus","PestRatRecoveryBonus","PestRatRepeatStatusPenalty","Formations","Runes"};if(!d.RootElement.TryGetProperty("PestRatRepeatStatusPenalty",out _))keys=keys.Where(k=>k!="PestRatRepeatStatusPenalty").ToArray();OverhaulRules.Keys(d.RootElement,keys);foreach(var x in d.RootElement.GetProperty("Formations").EnumerateArray())OverhaulRules.Keys(x,"PartyId","Enabled");foreach(var x in d.RootElement.GetProperty("Runes").EnumerateArray())OverhaulRules.Keys(x,"Id","Enabled");var c=ContentRules.Read<CombatRealityFile>(json);
  if(c.SchemaVersion!=1||c.PestRatPressureBonus is <0 or >40||c.PestRatRecoveryBonus is <0 or >40||c.PestRatRepeatStatusPenalty is <0 or >40||c.Formations==null||c.Runes==null||c.Formations.Any(x=>x==null)||c.Runes.Any(x=>x==null)||!c.Formations.Select(x=>x.PartyId).OrderBy(x=>x).SequenceEqual(a.Formations.Select(x=>x.PartyId).OrderBy(x=>x))||!c.Runes.Select(x=>x.Id).OrderBy(x=>x).SequenceEqual(a.Runes.Select(x=>x.Id).OrderBy(x=>x)))throw new JsonException("Unsupported Reality configuration/identity.");return c;
 }
 internal static int RatBonus(BossCadence c,int mode,int hp,int max,int recovery,int pressure,int relief)=>hp<=0||max<=0||!c.Enabled?0:recovery>0?(mode==c.RecoveryMode?Math.Clamp(relief,0,40):0):(long)hp*100<=(long)max*c.ThresholdPercent&&mode==c.PressureMode?Math.Clamp(pressure,0,40):0;
}
