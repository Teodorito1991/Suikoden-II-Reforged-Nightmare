using BepInEx;
using HarmonyLib;
using System.Text.Json;
namespace NightmareRebalance;
internal static class MaintenanceRuntime
{
    internal static Dictionary<string,EnemyProfile> Baseline=new();
    static readonly HpLedger hp=new();static readonly List<object> events=new(),charges=new();
    internal static int EnemyRoutineEntries,PlayerRoutineEntries;
    [ThreadStatic]static ActionContext? current;
    internal sealed record ActionContext(int Actor,int? Target,int Command,int Mode,int? ActionType,string Source,string? ActionPointer,int? DamageType);
    internal static int Dropped,ActorUnknownHpChanges;internal static bool Sampling;
    internal static void Initialize()=>Baseline=ContentRules.Read<EnemyFile>(ContentRuntime.Resource("enemy-profiles.json")).Profiles.ToDictionary(x=>x.Id);
    internal static bool Active(BATTLE_WORK? w)=>w!=null&&Combat.State is {Ready:true} s&&s.Work==w.Pointer&&!CombatReforgedRuntime.Probe;
    internal static void Reset(){hp.Reset();events.Clear();charges.Clear();Dropped=ActorUnknownHpChanges=EnemyRoutineEntries=PlayerRoutineEntries=0;current=null;}
    static void Add(object value){if(events.Count<256)events.Add(value);else Dropped++;}
    internal static void Ai(string stage,int actor,object detail){if(CombatReforgedRuntime.Probe)return;Add(new{Kind="AI",Stage=stage,Actor=actor,Detail=detail});}
    internal static void BaselineHp(BATTLE_WORK w){if(w.pm_data==null)return;foreach(var u in w.pm_data)if(u!=null)hp.Baseline(u.Pointer.ToInt64(),u.hp);Charges("Initialized",w);}
    internal static void Charges(string stage,BATTLE_WORK w)
    {if(!Active(w)||charges.Count>=8||w.pm_data==null)return;charges.Add(new{Stage=stage,Units=w.pm_data.Where(u=>u is {IsPlayer:true}).Select(u=>new{Slot=u.chara_no,Character=u.chara_id,u.hp,u.max_hp,Dead=u.IsDead,PackedCharges=u.TryCast<BattlePlayerCharacter>()?.c_varia_dat?.mp?.ToArray()}).ToArray()});}
    internal static ActionContext? Enter(PM_DATA actor,string source,ACTION_DATA? action=null,int? target=null,int? damageType=null)
    {var prior=current;var w=Framework.Singleton<BATTLE_WORK>.instance;if(!Active(w))return prior;BossTacticsRuntime.NativeEntry(actor,source);Sample(w,"BeforeNativeAction");if(actor.IsPlayer)PlayerRoutineEntries++;else EnemyRoutineEntries++;current=new(actor.chara_no,target,actor.c_koudou,actor.c_mode,action?.action_type,source,action?.Pointer.ToString("X"),damageType);Add(new{Kind="NativeActionEntered",Context=current,Meaning="Native routine entered; animation/visual completion not inferred"});return prior;}
    internal static void Exit(ActionContext? prior,Exception? exception=null){try{Sample(Framework.Singleton<BATTLE_WORK>.instance,"AfterNativeAction");if(current!=null&&Active(Framework.Singleton<BATTLE_WORK>.instance))Add(new{Kind="NativeRoutineReturned",Context=current,Succeeded=exception==null,ExceptionType=exception?.GetType().Name,AnimationCompletionInferred=false});}finally{current=prior;}}
    internal static void Raw(int actor,int target,int damage,string method)
    {if(Active(Framework.Singleton<BATTLE_WORK>.instance))Add(new{Kind="NativeCalculatedDamage",Actor=actor,Target=target,Damage=damage,Method=method,Context=current,ActualHpChange=false});}
    internal static void Scaled(int actor,int target,int before,int after,double factor)
    {if(Active(Framework.Singleton<BATTLE_WORK>.instance))Add(new{Kind="NightmareDamageFactor",Actor=actor,Target=target,Before=before,After=after,Factor=factor,Context=current,ActualHpChange=false});}
    internal static void Critical(PM_DATA actor,int target,bool result){if(Active(Framework.Singleton<BATTLE_WORK>.instance))Add(new{Kind="NativeCriticalDecision",Actor=actor.chara_no,Target=target,Result=result,Context=current,HpChangeNotInferred=true});}
    internal static void Status(PM_DATA unit,uint before){if(Active(Framework.Singleton<BATTLE_WORK>.instance)&&unit.status!=before)Add(new{Kind="NativeStatusChange",Target=unit.chara_no,Before=before,After=unit.status,Context=current});}
    internal static void Sample(BATTLE_WORK? w,string source)
    {
        if(Sampling||!Active(w)||w!.pm_data==null)return;Sampling=true;
        try{foreach(var u in w.pm_data){if(u==null)continue;int delta=hp.Observe(u.Pointer.ToInt64(),u.hp);if(delta==0)continue;if(current==null)ActorUnknownHpChanges++;
            if(u.IsPlayer){if(delta<0)CombatReforgedDiagnostics.DamageReceived-=delta;else CombatReforgedDiagnostics.HealingReceived+=delta;}else if(delta<0)CombatReforgedDiagnostics.EnemyDamage-=delta;
            Add(new{Kind="ActualHpChange",Target=u.chara_no,Player=u.IsPlayer,Character=u.chara_id,EnemyId=u.IsPlayer?null:u.monster_data?.id,Before=Math.Max(0,(int)u.hp)-delta,After=Math.Max(0,(int)u.hp),HpLoss=Math.Max(0,-delta),Healing=Math.Max(0,delta),StatusAfter=u.status,Source=source,Context=current,ActorKnown=current!=null,Critical="NotObserved",CalculatedDamageLinked=false,Meaning=current==null?"Unattributed net HP change between observations":"HP delta within observed native action scope"});
        }}catch(Exception e){Plugin.Debug("HP observation unavailable: "+e.Message);}finally{Sampling=false;}
    }
    internal static object Snapshot()=>new{EnemyRoutineEntries,PlayerRoutineEntries,ObservedActionSelectors=CombatRealityRuntime.SelectorCalls.Values.Sum(),ActionEntryIsNotAnimationCompletion=true,Events=events.ToArray(),ChargeSnapshots=charges.ToArray(),EventCapacity=256,Dropped,ActorUnknownHpChanges,HpObservation="Ledger reconciles native scope and update observations; unobserved opposing changes between samples can net out",CriticalEvidence="Only returned MonsterCriticalCheck decisions; no critical inferred from damage",FullActionPlaybackVerified=false};
}
internal static class BattleReportHistory
{
    internal static void Complete(object report)
    {
        if(Plugin.Options.RunCombatSelfTest.Value&&Environment.GetCommandLineArgs().Contains("--reforged-test-exit"))return;
        try{
            string dir=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);
            string record=JsonSerializer.Serialize(new{Version=Plugin.BuildLabel,CompletedUtc=DateTime.UtcNow,Completed=true,Report=report});
            DiagnosticHistoryStore.Append(Path.Combine(dir,"battle-history.json"),record);
        }catch(Exception e){Plugin.Logger.LogWarning("Battle history write skipped; gameplay retained: "+e.Message);}
    }
}
[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.MonsterCriticalCheck))]
internal static class MaintenanceCriticalObserver
{
    [HarmonyPostfix]static void Postfix(BattleMonsterCharacter __instance,int mokuhyo,bool __result)=>MaintenanceRuntime.Critical(__instance,mokuhyo,__result);
}
[HarmonyPatch(typeof(BattleManager),nameof(BattleManager.AutoUpdate))]
internal static class MaintenanceTick
{
    [HarmonyPrefix]static void Prefix(BattleManager __instance)=>MaintenanceRuntime.Sample(__instance.battle_work,"BeforeNativeUpdate");
    [HarmonyPostfix]static void Postfix(BattleManager __instance)=>MaintenanceRuntime.Sample(__instance.battle_work,"AfterNativeUpdate");
}
[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.MonsterCalcAttack))]
internal static class MaintenanceMonsterAction
{
    [HarmonyPrefix]internal static void Prefix(BattleMonsterCharacter __instance,System.Reflection.MethodBase __originalMethod,out MaintenanceRuntime.ActionContext? __state)=>__state=MaintenanceRuntime.Enter(__instance,__originalMethod.Name,__instance.action_data);
    [HarmonyFinalizer]internal static Exception? Finalizer(MaintenanceRuntime.ActionContext? __state,Exception? __exception){MaintenanceRuntime.Exit(__state,__exception);return __exception;}
}
[HarmonyPatch(typeof(PM_DATA),nameof(PM_DATA.DamageSetReActionData))]
internal static class MaintenanceDamageAction
{
    [HarmonyPrefix]static void Prefix(PM_DATA __instance,ACTION_DATA action_data,int mokuhyo,byte damage_type,out MaintenanceRuntime.ActionContext? __state)=>__state=MaintenanceRuntime.Enter(__instance,"DamageSetReActionData",action_data,mokuhyo,damage_type);
    [HarmonyFinalizer]internal static Exception? Finalizer(MaintenanceRuntime.ActionContext? __state,Exception? __exception){MaintenanceRuntime.Exit(__state,__exception);return __exception;}
}
[HarmonyPatch(typeof(PM_DATA),nameof(PM_DATA.RecoverSetReActionData))]
internal static class MaintenanceHealingAction
{
    [HarmonyPrefix]static void Prefix(PM_DATA __instance,ACTION_DATA action_data,int mokuhyo,out MaintenanceRuntime.ActionContext? __state)=>__state=MaintenanceRuntime.Enter(__instance,"RecoverSetReActionData",action_data,mokuhyo);
    [HarmonyFinalizer]internal static Exception? Finalizer(MaintenanceRuntime.ActionContext? __state,Exception? __exception){MaintenanceRuntime.Exit(__state,__exception);return __exception;}
}

[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.MonsterCalcTokushu))]
internal static class MaintenanceMonsterSpecial
{
 [HarmonyPrefix]static void Prefix(BattleMonsterCharacter __instance,System.Reflection.MethodBase __originalMethod,out MaintenanceRuntime.ActionContext? __state)=>__state=MaintenanceRuntime.Enter(__instance,__originalMethod.Name,__instance.action_data);
 [HarmonyFinalizer]static Exception? Finalizer(MaintenanceRuntime.ActionContext? __state,Exception? __exception){MaintenanceRuntime.Exit(__state,__exception);return __exception;}
}

[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.MonsterCalcKaifuku))]
internal static class MaintenanceMonsterHealing
{
 [HarmonyPrefix]static void Prefix(BattleMonsterCharacter __instance,System.Reflection.MethodBase __originalMethod,out MaintenanceRuntime.ActionContext? __state)=>__state=MaintenanceRuntime.Enter(__instance,__originalMethod.Name,__instance.action_data);
 [HarmonyFinalizer]static Exception? Finalizer(MaintenanceRuntime.ActionContext? __state,Exception? __exception){MaintenanceRuntime.Exit(__state,__exception);return __exception;}
}
