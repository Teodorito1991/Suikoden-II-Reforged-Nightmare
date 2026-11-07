using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;
internal static class CombatEvolutionRuntime
{
 internal static EvolutionAudit Audit=new();internal static CombatEvolutionFile Config=new(){Enabled=false};internal static bool Suppress;
 internal static bool Enabled=>Config.Enabled&&!Suppress;
 internal static MONSTER_PARTY_DATA? LastCopy;internal static object? LastFormation;
 internal static int FormationApplications,UniteApplications,AdditionalBossAdmissions;
 internal static readonly Dictionary<IntPtr,string> BossTables=new();
 internal static void Initialize()
 {
  try{Audit=ContentRules.Read<EvolutionAudit>(ContentRuntime.Resource("evolution-audit.json"));Config=CombatEvolutionRules.Parse(OverhaulRuntime.Load("combat-evolution.json"),Audit,CombatReforgedRuntime.Audit);Plugin.Logger.LogInfo($"Combat Evolution: {Audit.Formations.Length} exact formation remixes, {Audit.Bosses.Length} additional bosses, {Audit.Unites.Length} guarded Unite options.");}
  catch(Exception e){Config.Enabled=false;Plugin.Logger.LogWarning("Combat Evolution disabled; previous content retained: "+e.Message);}
 }
 internal static int[] Hooks(MONSTER_PARTY_SUB_DATA d)=>new[]{d.battle_start_func_index,d.phase_start_func_index,d.phase_end_func_index,d.battle_end_func_index,d.command_func_index,d.battle_main_func_index};
 internal static MONSTER_PARTY_DATA CopyParty(MONSTER_PARTY_DATA source,int[] codes)
 {
  var copy=UnityEngine.ScriptableObject.CreateInstance<MONSTER_PARTY_DATA>();
  copy.id=source.id;copy.monster_kazu=source.monster_kazu;copy.party_flg=source.party_flg;copy.item_no=source.item_no;
  copy.monster_id=new Il2CppStructArray<sbyte>(CombatEvolutionRules.Types(codes).Select(i=>(sbyte)i).ToArray());copy.monster_code=new Il2CppStructArray<sbyte>(codes.Select(i=>(sbyte)i).ToArray());
  var d=source.data;copy.data=new MONSTER_PARTY_SUB_DATA{monster_position=new Il2CppStructArray<short>(d.monster_position.ToArray()),battle_start_func_index=d.battle_start_func_index,phase_start_func_index=d.phase_start_func_index,phase_end_func_index=d.phase_end_func_index,battle_end_func_index=d.battle_end_func_index,command_func_index=d.command_func_index,battle_main_func_index=d.battle_main_func_index,be_data=new BE_DATA{command_num=0,com_data=new Il2CppReferenceArray<BE_COM_DATA>(0)}};
  return copy;
 }
 internal static bool ApplyFormation(BATTLE_WORK w,bool compatible)
 {
  // Called after original BattleDataSetup and BEFORE BattleEffectLoad/BattleDataLoad.
  // Original asset membership, complete template signatures and every party field
  // are validated before replacing the battle-local pointer. Shared assets stay intact.
  return Enabled&&Config.EnableFormations&&ApplyAuditedFormation(w,compatible,Audit.Formations,Config.Formations);
 }
 internal static bool ApplyAuditedFormation(BATTLE_WORK w,bool compatible,FormationAudit[] audit,EvolutionSwitch[] switches)
 {
  if(!compatible||w.battle_data is not {} table||w.monster_party_data is not {} source||source.data is not {} d)return false;
  var a=audit.FirstOrDefault(a=>a.TableId==table.id&&a.PartyId==source.id);
  if(a==null||!switches.Single(f=>f.PartyId==a.PartyId).Enabled)return false;
  if(table.monster_party==null||w.monster_party_index<1||w.monster_party_index>table.monster_party.Length||table.monster_party[w.monster_party_index-1]?.Pointer!=source.Pointer||table.monster_data==null)return false;
  if(d.be_data==null||d.be_data.com_data==null||d.be_data.com_data.Length!=0||source.monster_id==null||source.monster_code==null||d.monster_position==null||!CombatEvolutionRules.FormationMatches(a,table.id,source.id,w.monster_party_index-1,source.party_flg,source.item_no,source.monster_kazu,source.monster_id.Select(x=>(int)x).ToArray(),source.monster_code.Select(x=>(int)x).ToArray(),Hooks(d),d.be_data?.command_num??0,d.monster_position.Select(x=>(int)x).ToArray()))return false;
  foreach(var m in a.Monsters)if(m.Index>=table.monster_data.Length||table.monster_data[m.Index] is not {} native||native.id!=m.Id||!CombatReforgedRuntime.Signature(native).SequenceEqual(m.Signature))return false;
  if(a.Hooks.Where((h,i)=>i>0&&h!=0).Any()||a.Hooks.Select(h=>area_func.g_area_func_name!=null&&h>=0&&h<area_func.g_area_func_name.Count?area_func.g_area_func_name[h]:"<unresolved>").Where((n,i)=>n!=a.HookNames[i]).Any())return false;
  var copy=CopyParty(source,a.NewCodes);
  if(LastCopy!=null&&LastCopy.Pointer!=source.Pointer)UnityEngine.Object.Destroy(LastCopy);
  LastCopy=copy;w.monster_party_data=copy;FormationApplications++;
  LastFormation=new{a.TableId,a.PartyId,Index=w.monster_party_index,Before=a.OldCodes,After=a.NewCodes,a.OldHp,a.NewHp,CountPreserved=true,SourceUntouched=true};
  Plugin.Logger.LogInfo($"Native formation remix: {a.TableId}/{a.PartyId}; {string.Join(",",a.OldCodes)} -> {string.Join(",",a.NewCodes)}; original count/scripts/positions retained.");return true;
 }
 internal static void Ready(BattleState s,BATTLE_WORK w,bool compatible)
 {
  BossTables.Clear();if(!Enabled||!compatible)return;
  if(Config.EnableAdditionalBosses&&CombatReforgedRuntime.Config.Enabled&&w.pm_data!=null)
  {
   var id=CoverageRuntime.Capture(w);var boss=Audit.Bosses.FirstOrDefault(a=>CombatReforgedRules.BossMatches(a,id));
   if(boss!=null&&Config.AdditionalBosses.Single(x=>x.TableId==boss.TableId).Enabled)
    foreach(var u in w.pm_data)
     if(u!=null&&!u.IsPlayer&&u.TryCast<BattleMonsterCharacter>()!=null&&u.monster_data is {} d&&d.id==boss.EnemyId&&CombatReforgedRuntime.Signature(d).SequenceEqual(boss.Signature))
     {CombatReforgedRuntime.Plans[u.Pointer]=new(d.id,boss.Behavior,true,1.0,1.0);s.StatsAllowed=true;AdditionalBossAdmissions++;Plugin.Logger.LogInfo($"Combat Evolution boss admitted: {boss.TableId}; native actions/callbacks; no extra HP or damage multiplier.");}
  }
  foreach(var plan in CombatReforgedRuntime.Plans.Where(p=>p.Value.Boss))
  {var b=CombatReforgedRuntime.Audit.Bosses.Concat(Audit.Bosses).FirstOrDefault(b=>b.EnemyId==plan.Value.Id);if(b!=null)BossTables[plan.Key]=b.TableId;}
  if(Config.EnableUniteOptions&&(s.Allowed||s.StatsAllowed))ApplyUnites(w);
 }
 internal static BossPhase? Phase(IntPtr unit)=>Enabled&&Config.EnableBossPhases&&BossTables.TryGetValue(unit,out var table)?Config.BossPhases.FirstOrDefault(p=>p.Enabled&&p.TableId==table):null;
 internal static CombatUnitPlan Plan(PM_DATA u,CombatUnitPlan p)=>Phase(u.Pointer) is {} phase?p with{Behavior=CombatEvolutionRules.Style(phase,u.hp,u.max_hp)}:p;
 internal static int Bonus(PM_DATA u,int mode)=>Phase(u.Pointer) is {} phase?CombatEvolutionRules.PhaseBonus(phase,mode,u.hp,u.max_hp,Config.PhasePreference):0;
 internal static int[] UniteSignature(UNITE_DATA u)=>new[]{u.name,u.text,u.info,u.atrb,u.player_kazu,u.func_no,u.hosei,u.m_status,u.m_status_prob,u.p_status,u.p_status_prob}.Concat(u.player_id.Select(x=>(int)x)).ToArray();
 internal static UNITE_DATA CopyUnite(UNITE_DATA u,int effect){var c=new UNITE_DATA(u);c.player_id=new Il2CppStructArray<byte>(u.player_id.ToArray());c.hosei=effect;return c;}
 internal static int ApplyUnites(BATTLE_WORK w)
 {
  if(!Enabled||!Config.EnableUniteOptions||w.unite_data_table is not {} old)return 0;
  var changes=Audit.Unites.Where(a=>Config.Unites.Single(x=>x.Id==a.Id).Enabled&&a.Id<old.Length&&old[a.Id]!=null&&UniteSignature(old[a.Id]).SequenceEqual(a.Signature)).ToArray();
  if(changes.Length==0)return 0;
  var replacement=new Il2CppReferenceArray<UNITE_DATA>(old.ToArray());foreach(var a in changes)replacement[a.Id]=CopyUnite(old[a.Id],a.Effect);
  w.unite_data_table=replacement;UniteApplications+=changes.Length;return changes.Length;
 }
 internal static object Snapshot()=>new{Enabled,FormationApplications,UniteApplications,AdditionalBossAdmissions,LastFormation,ActiveBossPhases=BossTables.Values.ToArray(),PhasePreference=Config.PhasePreference,LiveGameplayVerified=false};
}
[HarmonyPatch(typeof(BattleManager),nameof(BattleManager.BattleDataSetup))]
internal static class EvolutionFormationPatch
{
 [HarmonyPostfix]static void Postfix(BattleManager __instance){try{bool compatible=Plugin.CheckCompatibility();var w=__instance.battle_work;var source=w.monster_party_data;CombatRealityRuntime.Reset();bool applied=HuntRuntime.Formation(w,compatible)||CombatEvolutionRuntime.ApplyFormation(w,compatible)||CombatMasteryRuntime.ApplyFormation(w,compatible)||CombatRealityRuntime.ApplyFormation(w,compatible);CombatRealityRuntime.FormationResult(w,source,applied,compatible);}catch(Exception e){Plugin.Logger.LogWarning("Formation remix retained native data: "+e.Message);}}
}
