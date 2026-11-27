using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;
internal static class CombatMasteryRuntime
{
 internal static MasteryAudit Audit=new();internal static CombatMasteryFile Config=new(){Enabled=false};internal static bool Suppress;
 internal static bool Enabled=>Config.Enabled&&!Suppress;
 internal static readonly Dictionary<IntPtr,int> Recovery=new();internal static int FormationApplications,RuneApplications,UniteApplications,CadenceSelections;
 internal static void Initialize()
 {
  try{Audit=ContentRules.Read<MasteryAudit>(ContentRuntime.Resource("mastery-audit.json"));Config=CombatMasteryRules.Parse(OverhaulRuntime.Load("combat-mastery.json"),Audit,CombatReforgedRuntime.Audit.Bosses.Concat(CombatEvolutionRuntime.Audit.Bosses).ToArray());Plugin.Logger.LogInfo($"Combat Mastery: {Audit.Formations.Length} additional formations, {Audit.Runes.Length} physical runes, {Audit.Unites.Length} Unite options, {Config.BossCadence.Length} boss cadence profiles.");}
  catch(Exception e){Config.Enabled=false;Plugin.Logger.LogWarning("Combat Mastery disabled; older modules retained: "+e.Message);}
 }
 internal static bool ApplyFormation(BATTLE_WORK w,bool compatible)
 {if(!Enabled||!Config.EnableFormations)return false;bool result=CombatEvolutionRuntime.ApplyAuditedFormation(w,compatible,Audit.Formations,Config.Formations);if(result)FormationApplications++;return result;}
 internal static BossCadence? Cadence(PM_DATA u)=>Enabled&&Config.EnableBossCadence&&CombatEvolutionRuntime.BossTables.TryGetValue(u.Pointer,out string? table)?Config.BossCadence.FirstOrDefault(x=>x.Enabled&&x.TableId==table):null;
 internal static int Bonus(PM_DATA u,int mode)=>Cadence(u) is {} c?CombatMasteryRules.CadenceBonus(c,mode,u.hp,u.max_hp,Recovery.GetValueOrDefault(u.Pointer),Config.CadencePreference):0;
 internal static void Selected(PM_DATA u,int mode)
 {
  if(Cadence(u) is not {} c)return;int old=Recovery.GetValueOrDefault(u.Pointer);int next=CombatMasteryRules.NextRecovery(c,mode,old);Recovery[u.Pointer]=next;CadenceSelections++;
  CombatReforgedRuntime.Record(new{Kind="BossCadence",c.TableId,SelectedNativeMode=mode,RecoveryBefore=old,RecoveryAfter=next,Hp=u.hp,MaxHp=u.max_hp,SelectionIsNotTurn=true});
 }
 internal static int[] RuneSignature(MAGI_DATA d)=>new[]{d.name,d.text,(int)d.atrb,d.eff_val,d.uni_no,d.uni_magic}.Concat(d.info.Select(x=>(int)x)).Concat(d.status.Select(x=>(int)x)).ToArray();
 internal static bool Spell(int id,MAGI_DATA? d)
 {
  if(!Enabled||!Config.EnablePhysicalRunes||!Plugin.Options.EnableRuneBalancing.Value||d?.info==null||d.status==null||Audit.Runes.FirstOrDefault(x=>x.Id==id) is not {} a||!Config.Runes.Single(x=>x.Id==id).Enabled)return false;
  var sig=RuneSignature(d);if(sig.Length!=a.Signature.Length)return false;int value=sig[3];sig[3]=a.Signature[3];if(!sig.SequenceEqual(a.Signature)||value!=a.Signature[3]&&value!=a.Effect)return false;
  if(d.eff_val==a.Effect)return false;d.eff_val=(short)a.Effect;RuneApplications++;return true;
 }
 internal static int ApplyUnites(BATTLE_WORK w)
 {
  if(!Enabled||!Config.EnableUniteOptions||w.unite_data_table is not {} old)return 0;
  var changes=Audit.Unites.Where(a=>Config.Unites.Single(x=>x.Id==a.Id).Enabled&&a.Id<old.Length&&old[a.Id]!=null&&CombatEvolutionRuntime.UniteSignature(old[a.Id]).SequenceEqual(a.Signature)).ToArray();if(changes.Length==0)return 0;
  var copy=new Il2CppReferenceArray<UNITE_DATA>(old.ToArray());foreach(var a in changes)copy[a.Id]=CombatEvolutionRuntime.CopyUnite(old[a.Id],a.Effect);w.unite_data_table=copy;UniteApplications+=changes.Length;return changes.Length;
 }
 internal static object Snapshot()=>new{Enabled,FormationApplications,RuneApplications,UniteApplications,CadenceSelections,ActiveCadence=Recovery.Count,RenderedCombatVerified=false};
}
