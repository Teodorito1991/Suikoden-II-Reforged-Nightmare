using HarmonyLib;
namespace NightmareRebalance;
internal static class CombatRealityRuntime
{
 internal static RealityAudit Audit=new();internal static CombatRealityFile Config=new(){Enabled=false};internal static bool Suppress,ProbeScores;internal static string ProbeBuild="";
 internal static bool Enabled=>Config.Enabled&&!Suppress;internal static int FormationApplications,RuneApplications;internal static readonly List<object> Admissions=new(),CandidateScores=new();internal static readonly HashSet<string> Seen=new();internal static readonly Dictionary<IntPtr,int> SelectorCalls=new();
 internal static void Initialize(){try{Audit=ContentRules.Read<RealityAudit>(ContentRuntime.Resource("reality-audit.json"));Config=CombatRealityRules.Parse(OverhaulRuntime.Load("combat-reality.json"),Audit);Plugin.Logger.LogInfo($"Combat Reality: {Audit.Formations.Length} additional formations, {Audit.Runes.Length} physical rune options, production admission diagnostics.");}catch(Exception e){Config.Enabled=false;Plugin.Logger.LogWarning("Combat Reality disabled: "+e.Message);}}
 internal static void Event(string module,string decision,string id,object? detail=null)
 {if(CombatReforgedRuntime.Probe)return;string key=module+decision+id;if(Admissions.Count<128&&Seen.Add(key))Admissions.Add(new{Module=module,Decision=decision,Id=id,Detail=detail});}
 internal static void Reset(){HuntRuntime.Reset();MaintenanceRuntime.Reset();Admissions.Clear();Seen.Clear();SelectorCalls.Clear();}
 internal static bool ApplyFormation(BATTLE_WORK w,bool compatible){if(!Enabled||!Config.EnableFormations)return false;bool result=CombatEvolutionRuntime.ApplyAuditedFormation(w,compatible,Audit.Formations,Config.Formations);if(result)FormationApplications++;return result;}
 internal static void FormationResult(BATTLE_WORK w,MONSTER_PARTY_DATA? original,bool applied,bool compatible)
 {
  string id=original?.id??"<none>";var a=CombatEvolutionRuntime.Audit.Formations.Concat(CombatMasteryRuntime.Audit.Formations).Concat(Audit.Formations).FirstOrDefault(x=>x.TableId==w.battle_data?.id&&x.PartyId==id);
  bool enabled=a==null?false:CombatEvolutionRuntime.Audit.Formations.Contains(a)?CombatEvolutionRuntime.Enabled&&CombatEvolutionRuntime.Config.EnableFormations&&CombatEvolutionRuntime.Config.Formations.Single(x=>x.PartyId==id).Enabled:CombatMasteryRuntime.Audit.Formations.Contains(a)?CombatMasteryRuntime.Enabled&&CombatMasteryRuntime.Config.EnableFormations&&CombatMasteryRuntime.Config.Formations.Single(x=>x.PartyId==id).Enabled:Enabled&&Config.EnableFormations&&Config.Formations.Single(x=>x.PartyId==id).Enabled;
  Event("Formation",applied?"Applied":!compatible?"CompatibilityExcluded":a==null?"IntentionallyExcluded":!enabled?"DisabledByConfiguration":"RejectedUnexpectedSignature",id,new{Table=w.battle_data?.id,Index=w.monster_party_index,DeclaredCount=original?.monster_kazu,SourceCodes=original?.monster_code?.Select(x=>(int)x).ToArray(),Commands=original?.data?.be_data?.command_num});
 }
 internal static void Ready(BattleState s,BATTLE_WORK w)
 {
  if(w.pm_data==null)return;
  foreach(var u in w.pm_data.Where(u=>u!=null&&!u.IsPlayer))
  {
   var data=u.monster_data;string id=data?.id??"<none>";var expected=CombatReforgedRuntime.Audit.Enemies.FirstOrDefault(x=>x.Id==id);bool admitted=CombatReforgedRuntime.Plans.ContainsKey(u.Pointer);
   string decision=!CombatReforgedRuntime.Config.Enabled?"DisabledByConfiguration":admitted?"Admitted":!s.Allowed?"IntentionallyExcluded":expected!=null?"RejectedUnexpectedSignature":"IntentionallyExcluded";
   Event("CombatAdmission",decision,id,new{RegularEligible=s.Allowed,StatsEligible=s.StatsAllowed,TemplateHp=data?.hp,ExpectedHp=expected?.Signature[0],DeclaredAttacks=data?.attack_data_num,Slots=data?.attack_data?.Length,NativeCommandScript=w.monster_cm_func!=null,NativeMainScript=w.monster_bm_func!=null,Provenance=DifficultyProvenance.Evidence(data),ScriptExecutionBypassConfirmed=false});
  }
  Event("CharacterProfiles",Plugin.Options.EnableCharacterBalancing.Value?"ConfiguredAwaitingLookup":"DisabledByConfiguration","battle");Event("SpellEffects",Plugin.Options.EnableRuneBalancing.Value?"ConfiguredAwaitingLookup":"DisabledByConfiguration","battle");
  Event("EnemyAI",CombatReforgedRuntime.AiActive(w)?"Enabled":"DisabledByConfiguration","battle");Event("RuneRegeneration",Plugin.Options.EnableRuneRegeneration.Value?(s.Allowed?"EligibleAwaitingVictory":"IntentionallyExcluded"):"DisabledByConfiguration","battle");Event("PestRatTactics",Enabled&&Config.EnablePestRatTactics?"Enabled":"DisabledByConfiguration","battle");
 }
 internal static int Bonus(PM_DATA u,int mode)
 {if(!Enabled||!Config.EnablePestRatTactics||CombatMasteryRuntime.Cadence(u) is not {} c||c.TableId!="boss_rat")return 0;int n=CombatRealityRules.RatBonus(c,mode,u.hp,u.max_hp,CombatMasteryRuntime.Recovery.GetValueOrDefault(u.Pointer),Config.PestRatPressureBonus,Config.PestRatRecoveryBonus);if(mode==1&&CombatReforgedRuntime.Previous.GetValueOrDefault(u.Pointer,-1)==1)n-=Config.PestRatRepeatStatusPenalty;return n;}
 internal static void Score(PM_DATA u,int mode,int oldScore,int extra)
 {if(ProbeScores&&CandidateScores.Count<512)CandidateScores.Add(new{Build=ProbeBuild,Mode=mode,Hp=u.hp,MaxHp=u.max_hp,OldScore=oldScore,RealityBonus=extra,FinalScore=oldScore+extra,Recovery=CombatMasteryRuntime.Recovery.GetValueOrDefault(u.Pointer)});}
 internal static void Selected(PM_DATA u,int original,int selected){MaintenanceRuntime.Ai("NativeSelection",u.chara_no,new{Before=original,After=selected,SelectedActionChanged=original!=selected});SelectorCalls[u.Pointer]=SelectorCalls.GetValueOrDefault(u.Pointer)+1;Event("NativeSelector",original!=selected?"SelectedCommandChanged":"EvaluatedUnchanged",u.monster_data?.id??"unknown",new{Before=original,After=selected});}
 internal static bool Spell(int id,MAGI_DATA? d)
 {
  if(!Enabled||!Config.EnablePhysicalRunes||!Plugin.Options.EnableRuneBalancing.Value||d?.info==null||d.status==null||Audit.Runes.FirstOrDefault(x=>x.Id==id) is not {} a||!Config.Runes.Single(x=>x.Id==id).Enabled)return false;var sig=CombatMasteryRuntime.RuneSignature(d);if(sig.Length!=a.Signature.Length)return false;int value=sig[3];sig[3]=a.Signature[3];if(!sig.SequenceEqual(a.Signature)||value!=a.Signature[3]&&value!=a.Effect)return false;if(d.eff_val==a.Effect)return false;d.eff_val=(short)a.Effect;RuneApplications++;Event("PhysicalRune","Applied",id.ToString());return true;
 }
 internal static object Snapshot()=>new{Enabled,ModuleSettings=new{Characters=Plugin.Options.EnableCharacterBalancing.Value,Spells=Plugin.Options.EnableRuneBalancing.Value,EnemyProfiles=Plugin.Options.EnableEnemyProfiles.Value,CombatAi=Plugin.Options.EnableCombatReforgedAi.Value,RuneRegeneration=Plugin.Options.EnableRuneRegeneration.Value,Evolution=CombatEvolutionRuntime.Enabled,Mastery=CombatMasteryRuntime.Enabled,Reality=Enabled},FormationApplications,RuneApplications,Admissions=Admissions.ToArray(),Capacity=128,ScriptedBypassIsNotInferred=true};
}
[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.AttackDataMakeNormal))]
internal static class RealityActionObservation
{
 [HarmonyPrefix]static void Prefix(BattleMonsterCharacter __instance,out int __state)=>__state=CombatRealityRuntime.SelectorCalls.GetValueOrDefault(__instance.Pointer);
 [HarmonyPostfix]static void Postfix(BattleMonsterCharacter __instance,int __state){try{if(!CombatReforgedRuntime.Probe&&CombatRealityRuntime.SelectorCalls.GetValueOrDefault(__instance.Pointer)==__state)CombatRealityRuntime.Event("NativeAction","NoSelectorObserved",__instance.monster_data?.id??"unknown",new{PossibleScriptOrNoDecision=true,ScriptBypassConfirmed=false});}catch(Exception e){CombatRealityRuntime.Event("Diagnostics","InspectionUnavailable","native-action",e.Message);}}
}
