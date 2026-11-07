using HarmonyLib;
namespace NightmareRebalance;
internal static class CombatReforgedDiagnostics
{
    internal static int DamageReceived,HealingReceived,EnemyDamage,Statuses,RuneRestored;
    internal static int InitialCharges,LastKnownCharges,Generation,InitialEnemies;
    internal static string Outcome="InProgress";
    internal static object? LastBattle, InitialFrame;
    internal static readonly List<object> CalculatedDamage=new();
    internal static object Frame(BATTLE_WORK w)=>new{Table=w.battle_data?.id,Party=w.monster_party_data?.id,Index=w.monster_party_index,PlayerCount=w.player_kazu,EnemyCount=w.monster_kazu,NativePhase=w.battle_phase_no,RegularScope=Combat.State?.Allowed??false,StatScope=Combat.State?.StatsAllowed??false,Units=w.pm_data?.Where(u=>u!=null).Take(12).Select(u=>new{Slot=u.chara_no,Player=u.IsPlayer,Character=u.chara_id,EnemyId=u.IsPlayer?null:u.monster_data?.id,Hp=u.hp,MaxHp=u.max_hp,Dead=u.IsDead,Status=u.status,Parameters=u.calc_para?.ToArray(),NativeAction=u.c_koudou,NativeMode=u.c_mode,PackedCharges=u.TryCast<BattlePlayerCharacter>()?.c_varia_dat?.mp?.ToArray()}).ToArray()};
    internal static void Calculated(int attacker,int target,int before,int after,double factor) {if(CalculatedDamage.Count<32)CalculatedDamage.Add(new{Attacker=attacker,Target=target,Before=before,After=after,Factor=factor,Meaning="Calculated enemy damage, before later hit/reaction/playback effects"});}
    internal static readonly HashSet<int> Phases=new();
    static int Charges(BATTLE_WORK w)=>w.pm_data==null?0:w.pm_data.Where(u=>u is {IsPlayer:true}).Select(u=>u.TryCast<BattlePlayerCharacter>()?.c_varia_dat).Where(d=>d?.mp?.Length==4).Sum(d=>d!.mp.Sum(v=>v>>4));
    internal static void Begin(BattleState s,BATTLE_WORK w)
    {
        CalculatedDamage.Clear();InitialFrame=Frame(w);DamageReceived=HealingReceived=EnemyDamage=Statuses=RuneRestored=0;Outcome="InProgress";Phases.Clear();Generation++;InitialEnemies=w.monster_kazu;
        try{InitialCharges=Charges(w);}catch{InitialCharges=0;}LastKnownCharges=InitialCharges;
    }
    internal static object Snapshot()
    {
        var w=BattleManager.Instance?.battle_work;bool active=w!=null&&Combat.State is {Ready:true} s&&s.Work==w.Pointer;
        int current=LastKnownCharges;try{if(active){current=Charges(w!);LastKnownCharges=current;Phases.Add(w!.battle_phase_no);}}catch{ }
        return new{Generation,Active=active,Outcome,InitialEnemies,NativePhasesObserved=Phases.OrderBy(n=>n).ToArray(),PlayerHpLostInObservedNativeReactions=DamageReceived,PlayerHpRestoredInObservedNativeReactions=HealingReceived,EnemyHpLostInObservedNativeReactions=EnemyDamage,StatusSetsObserved=Statuses,InitialCharges,CurrentCharges=current,ChargesRestoredByPlugin=RuneRestored,NetChargeReduction=InitialCharges-current,InitialFrame,CurrentFrame=active?Frame(w!):null,CalculatedDamage=CalculatedDamage.ToArray(),ResourceLimits="Net charges may include native level-up/refill effects; not gross spell expenditure",LastBattle};
    }
    internal static void Finish(BattleState? s)
    {
        if(s is not {Ready:true}||s.ReportWritten)return;
        if(Outcome=="InProgress")Outcome="ClosedWithoutVictorySignal";
        // Do not nest previous reports indefinitely.
        MaintenanceRuntime.Sample(Framework.Singleton<BATTLE_WORK>.instance,"BattleFinish");
        HuntSessionRuntime.Finish(Outcome,RuneRestored);
        LastBattle=null;LastBattle=new{Debug=new{HuntDebug.Enabled,HuntDebug.Armed,Run=HuntRuntime.Work==s.Work?HuntSessionRuntime.Last:null},Metrics=Snapshot(),Ai=CombatReforgedRuntime.Snapshot(),Evolution=CombatEvolutionRuntime.Snapshot(),Reality=CombatRealityRuntime.Snapshot(),Maintenance=MaintenanceRuntime.Snapshot(),Expansion=HuntRuntime.Snapshot()};
        if(!s.ReportWritten){s.ReportWritten=true;BattleReportHistory.Complete(LastBattle);}
        try
        {
            string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"last-battle.json"),System.Text.Json.JsonSerializer.Serialize(LastBattle,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
            Plugin.Logger.LogInfo($"Combat summary: {Outcome}; observed player HP lost={DamageReceived}, healed={HealingReceived}; native action/target changes={CombatReforgedRuntime.ModeChanges}/{CombatReforgedRuntime.TargetChanges}; restored charges={RuneRestored}.");
            CoverageRuntime.Emit(s.Report,"combat-reforged-final-metrics");
        }
        catch(Exception e){Plugin.Logger.LogWarning("Combat metrics export unavailable: "+e.Message);}
    }
    internal static string Summary()=>$"READ ONLY - F9 exports combat and war details\n\nRPG combat; native phase {BattleManager.Instance?.battle_work?.battle_phase_no}\nAudited enemy AI: {CombatReforgedRuntime.Plans.Count} units\nBoss tactics: {BossTacticsRuntime.Bindings.Count} bound; {BossTacticsRuntime.Changes} choices changed; {BossTacticsRuntime.RoutineEntries} native routine entries\nNative action choices changed: {CombatReforgedRuntime.ModeChanges}\nNative target choices changed: {CombatReforgedRuntime.TargetChanges}\n\nObserved player HP lost: {DamageReceived}\nObserved player HP healed: {HealingReceived}\nObserved enemy HP lost: {EnemyDamage}\nNew status flags observed: {Statuses}\nRune charges restored: {RuneRestored}\nOutcome: {Outcome}\n\nNative reactions only; not a battle simulation.\nF8/F9 never change combat, story or saves.\nNative war improvements retained.";
}
[HarmonyPatch(typeof(PM_DATA),nameof(PM_DATA.DamageSetReActionData))]
internal static class CombatHpObserver
{
    [HarmonyPrefix]internal static void Prefix(int mokuhyo,out (PM_DATA? Unit,int Hp) __state)
    {
        __state=(null,0);var w=BattleManager.Instance?.battle_work;
        if(w==null||Combat.State is not {Ready:true} s||s.Work!=w.Pointer||w.pm_data==null||mokuhyo<0||mokuhyo>=w.pm_data.Length||w.pm_data[mokuhyo] is not {} u)return;
        __state=(u,Math.Max(0,(int)u.hp));CombatReforgedDiagnostics.Phases.Add(w.battle_phase_no);
    }
    [HarmonyPostfix]internal static void Postfix((PM_DATA? Unit,int Hp) __state)
    {
        if(__state.Unit is not {} u)return;int delta=Math.Max(0,(int)u.hp)-__state.Hp;
        MaintenanceRuntime.Sample(Framework.Singleton<BATTLE_WORK>.instance,"ReactionReturn");
    }
}
[HarmonyPatch(typeof(PM_DATA),nameof(PM_DATA.StatusSet))]
internal static class CombatStatusObserver
{
    [HarmonyPrefix]static void Prefix(PM_DATA __instance,out uint __state)=>__state=__instance.status;
    [HarmonyPostfix]static void Postfix(PM_DATA __instance,uint __state)
    {var w=Framework.Singleton<BATTLE_WORK>.instance;if(Combat.State is {Ready:true} s&&w!=null&&s.Work==w.Pointer&&(__instance.status&~__state)!=0)CombatReforgedDiagnostics.Statuses++;MaintenanceRuntime.Status(__instance,__state);}
}

[HarmonyPatch(typeof(PM_DATA),nameof(PM_DATA.RecoverSetReActionData))]
internal static class CombatHealingObserver
{
 [HarmonyPrefix]static void Prefix(int mokuhyo,out (PM_DATA? Unit,int Hp) __state)=>CombatHpObserver.Prefix(mokuhyo,out __state);
 [HarmonyPostfix]static void Postfix((PM_DATA? Unit,int Hp) __state)=>CombatHpObserver.Postfix(__state);
}
