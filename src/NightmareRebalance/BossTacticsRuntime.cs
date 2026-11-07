namespace NightmareRebalance;
// Additional AI admission never widens HP, damage, reward or story eligibility.
internal static class BossTacticsRuntime
{
 internal static FormationAudit[] Audit=Array.Empty<FormationAudit>();internal static BossTacticsFile Config=new(){Enabled=false};
 internal sealed record Binding(BATTLE_WORK Work,MONSTER_PARTY_DATA Party,BossPattern Pattern,MONSTER_DATA Data,int[] Signature);
 internal static readonly Dictionary<IntPtr,Binding> Bindings=new();internal static readonly List<object> Events=new();
 internal static int Selections,Changes,RoutineEntries;internal static bool Suppress;
 internal static void Initialize(){try{Audit=ContentRules.Read<FormationAudit[]>(ContentRuntime.Resource("boss-tactics-audit.json"));Config=BossTacticsRules.Parse(OverhaulRuntime.Load("boss-tactics.json"),Audit);Plugin.Logger.LogInfo("Boss Reforged: seven retained encounters plus guarded Luca lus1/lus2 and Neclord nec3 action-only tactics; native callbacks untouched.");}catch(Exception e){Config.Enabled=false;Plugin.Logger.LogWarning("Boss tactics disabled: "+e.Message);}}
 internal static void Clear(){Bindings.Clear();Events.Clear();Selections=Changes=RoutineEntries=0;}
 internal static bool Enabled(BossPattern p)=>Config.Enabled&&!Suppress&&p.Enabled&&(BossTacticsRules.Major(p.TableId)?Config.EnableMajorBosses:Config.EnableExistingBosses);
 internal static void Record(object value){if(!CombatReforgedRuntime.Probe&&Events.Count<64)Events.Add(value);}
 internal static bool Source(BATTLE_WORK w,FormationAudit a)
 {
  var t=w.battle_data;var p=w.monster_party_data;var d=p?.data;
  if(t?.id!=a.TableId||p==null||d?.be_data?.com_data==null||t.monster_party==null||t.monster_data==null||w.monster_data==null||w.pm_data==null||w.monster_party_index<1||w.monster_party_index>t.monster_party.Length||t.monster_party[w.monster_party_index-1]?.Pointer!=p.Pointer)return false;
  if((w.monster_be_func!=null)!=(a.Hooks[3]!=0)||(w.monster_bm_func!=null)!=(a.Hooks[5]!=0)||(w.monster_cm_func!=null)!=(a.Hooks[4]!=0))return false;
  if(!CombatEvolutionRules.FormationMatches(a,t.id,p.id,w.monster_party_index-1,p.party_flg,p.item_no,p.monster_kazu,p.monster_id.Select(x=>(int)x).ToArray(),p.monster_code.Select(x=>(int)x).ToArray(),CombatEvolutionRuntime.Hooks(d),d.be_data.command_num,d.monster_position.Select(x=>(int)x).ToArray())||d.be_data.com_data.Length!=0)return false;
  if(a.Hooks.Select(h=>area_func.g_area_func_name!=null&&h>=0&&h<area_func.g_area_func_name.Count?area_func.g_area_func_name[h]:"<unresolved>").Where((n,i)=>n!=a.HookNames[i]).Any())return false;
  foreach(var m in a.Monsters)if(m.Index>=t.monster_data.Length||t.monster_data[m.Index] is not {} source||source.id!=m.Id||!CombatReforgedRuntime.Signature(source).SequenceEqual(m.Signature))return false;
  var enemies=w.pm_data.Where(x=>x!=null&&!x.IsPlayer&&x.TryCast<BattleMonsterCharacter>()!=null).ToArray();var ids=a.OldCodes.Where(x=>x>0).Select(code=>a.Monsters.Single(m=>m.Index==code).Id).OrderBy(x=>x);
  if(w.monster_kazu!=enemies.Length||!enemies.Select(x=>x.monster_data?.id).OrderBy(x=>x).SequenceEqual(ids))return false;
  foreach(var u in enemies){var m=a.Monsters.Single(m=>m.Id==u.monster_data.id);if(!CombatReforgedRuntime.Signature(u.monster_data).SequenceEqual(m.Signature)||!DifficultyProvenance.Matches(w,u.monster_data,m.Signature))return false;}
  return true;
 }
 internal static void Ready(BATTLE_WORK w,bool compatible)
 {
  Bindings.Clear();if(!compatible||!Config.Enabled||Suppress||!CombatReforgedRuntime.Config.Enabled||!CombatReforgedRuntime.Config.EnableBossEncounters||!DifficultyProvenance.Difficulty(out int difficulty,out float rate)||difficulty!=0||rate!=1f)return;
  try{var p=Config.Patterns.FirstOrDefault(p=>p.TableId==w.battle_data?.id);if(p==null||!Enabled(p))return;var a=Audit.Single(a=>a.TableId==p.TableId);
   if(!Source(w,a)){Record(new{Kind="BossTacticsRejected",p.TableId,Reason="Source/roster/callback/signature/provenance"});return;}
   // Keep all prior boss opt-outs and explicit profile/stat priorities.
   if(!BossTacticsRules.Major(p.TableId)&&!w.pm_data.Any(u=>u!=null&&CombatReforgedRuntime.Plans.TryGetValue(u.Pointer,out var old)&&old.Boss&&old.Id==p.EnemyId))return;
   foreach(var u in w.pm_data.Where(u=>u!=null&&!u.IsPlayer&&u.monster_data?.id==p.EnemyId))Bindings[u.Pointer]=new(w,w.monster_party_data,p,u.monster_data,a.Monsters.Single(m=>m.Id==p.EnemyId).Signature);
   Record(new{Kind="BossTacticsAdmitted",p.TableId,Mode="AI only",StatsEligibilityChanged=false,CallbacksChanged=false});
  }catch(Exception e){Bindings.Clear();Plugin.Debug("Boss tactics retained native encounter: "+e.Message);}
 }
 internal static bool Bound(PM_DATA u)=>Config.Enabled&&!Suppress&&Bindings.TryGetValue(u.Pointer,out var b)&&Enabled(b.Pattern)&&b.Work.Pointer==CombatReforgedRuntime.Work&&b.Work.monster_party_data?.Pointer==b.Party.Pointer&&u.monster_data?.Pointer==b.Data.Pointer&&CombatReforgedRuntime.Signature(b.Data).SequenceEqual(b.Signature);
 internal static bool Plan(PM_DATA u,out CombatUnitPlan plan){plan=null!;if(!Bound(u))return false;var b=Bindings[u.Pointer];plan=new(b.Pattern.EnemyId,"Alternating",true,null,null);return true;}
 internal static int Score(BATTLE_WORK w,PM_DATA u,CombatAction action,IReadOnlyList<CombatTarget> targets,int prior){var b=Bindings[u.Pointer];int score=BossTacticsRules.Score(b.Pattern,Config.Preference,w.battle_kaisuu,u.hp,u.max_hp,action,targets,prior);return score;}
 internal static void Selected(BATTLE_WORK w,PM_DATA u,int before,int after)
 {if(!Bound(u))return;Selections++;if(before!=after)Changes++;var p=Bindings[u.Pointer].Pattern;Record(new{Kind="BossTacticsSelected",p.TableId,NativeCounter=w.battle_kaisuu,Window=BossTacticsRules.Window(p,w.battle_kaisuu,u.hp,u.max_hp),Before=before,After=after,Changed=before!=after,Executed=false});}
 internal static void NativeEntry(PM_DATA u,string source){if(CombatReforgedRuntime.Probe||!Bound(u)||source is not("MonsterCalcAttack" or "MonsterCalcTokushu" or "MonsterCalcKaifuku"))return;RoutineEntries++;Record(new{Kind="BossNativeRoutineEntered",Bindings[u.Pointer].Pattern.TableId,Slot=u.chara_no,Mode=u.c_mode,Command=u.c_koudou,Source=source,AnimationCompletionObserved=false});}
 internal static object Snapshot()=>new{Config.Enabled,Config.EnableMajorBosses,Config.EnableExistingBosses,Bindings=Bindings.Values.Select(x=>x.Pattern.TableId).ToArray(),Selections,Changes,RoutineEntries,Events=Events.ToArray(),NativeActionsExecutedInferred=false,StoryCallbacksChanged=false,StatScalingAdded=false};
}
