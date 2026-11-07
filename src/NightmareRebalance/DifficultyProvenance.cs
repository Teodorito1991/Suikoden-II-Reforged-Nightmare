using HarmonyLib;
namespace NightmareRebalance;
internal static class DifficultyProvenance
{
    internal sealed record Copy(BATTLE_WORK Work,BATTLE_DATA Table,MONSTER_DATA Source,MONSTER_DATA Local,int Difficulty,float Rate,int[] Signature);
    static readonly Dictionary<IntPtr,Copy> copies=new();
    [ThreadStatic]static BATTLE_WORK? setup;
    internal static bool StrictProbe;
    internal static bool LegacyFixture=>CombatReforgedRuntime.Probe&&!StrictProbe;
    internal static bool Difficulty(out int value,out float rate)
    {value=-1;rate=0;var g=Framework.Singleton<GAME_WORK>.instance?.game_data;if(g==null)return false;value=g.difficulty;if(value is not(0 or 2))return false;rate=ConstShare.GetDifficultyRate(value);return rate==(value==0?1f:1.2f);}
    internal static BATTLE_WORK? Start(BATTLE_WORK w){var previous=setup;copies.Clear();setup=w;return previous;}
    internal static void End(BATTLE_WORK? previous)=>setup=previous;
    internal static void Observe(MONSTER_DATA source,MONSTER_DATA local)
    {
        if(setup is not {} w||w.battle_data is not {} table||source.Pointer==local.Pointer||table.monster_data==null||!table.monster_data.Any(x=>x?.Pointer==source.Pointer)||!Difficulty(out int d,out float r))return;
        var sig=CombatReforgedRuntime.Signature(source);if(sig.Length!=47||!CombatReforgedRuntime.Signature(local).SequenceEqual(sig))return;
        if(copies.Count<512)copies[local.Pointer]=new(w,table,source,local,d,r,sig);
    }
    internal static bool Matches(BATTLE_WORK w,MONSTER_DATA local,int[] expected)
    {
        if(LegacyFixture)return CombatReforgedRuntime.Signature(local).SequenceEqual(expected);
        if(!copies.TryGetValue(local.Pointer,out var c)||c.Work.Pointer!=w.Pointer||c.Table.Pointer!=w.battle_data?.Pointer||c.Local.id!=c.Source.id||!c.Signature.SequenceEqual(expected)||!Difficulty(out int d,out float r)||d!=c.Difficulty||r!=c.Rate)return false;
        if(w.monster_data==null||!w.monster_data.Any(x=>x?.Pointer==local.Pointer)||c.Table.monster_data==null||!c.Table.monster_data.Any(x=>x?.Pointer==c.Source.Pointer)||!CombatReforgedRuntime.Signature(c.Source).SequenceEqual(expected))return false;
        return MaintenanceRules.Signature(expected,CombatReforgedRuntime.Signature(local),d,r);
    }
    internal static bool Regular(BATTLE_WORK w,MONSTER_DATA local,int[] expected)=>Matches(w,local,expected);
    internal static object Evidence(MONSTER_DATA? local)=>local!=null&&copies.TryGetValue(local.Pointer,out var c)?new{VerifiedCopyObserved=true,c.Difficulty,c.Rate,SourceHp=c.Signature[0],LocalHp=local.hp,Table=c.Table.id}:new{VerifiedCopyObserved=false};
}
[HarmonyPatch(typeof(BattleManager),nameof(BattleManager.BattleDataSetup))]
internal static class DifficultySetupScope
{
    [HarmonyPrefix]static void Prefix(BattleManager __instance,out (BATTLE_WORK? Provenance,BATTLE_WORK? Selection) __state){HuntAccess.BeginSetup();HuntLaunchProof.Setup(__instance.battle_work);__state=(DifficultyProvenance.Start(__instance.battle_work),HuntAccess.SetupWork);HuntAccess.SetupWork=__instance.battle_work;}
    [HarmonyFinalizer]static Exception? Finalizer(BattleManager __instance,(BATTLE_WORK? Provenance,BATTLE_WORK? Selection) __state,Exception? __exception){HuntAccess.EndSetup(__instance.battle_work,__exception);HuntAccess.SetupWork=__state.Selection;DifficultyProvenance.End(__state.Provenance);return __exception;}
}
[HarmonyPatch(typeof(MONSTER_DATA),nameof(MONSTER_DATA.ShallowCopy))]
internal static class DifficultyCopyObservation
{
    [HarmonyPostfix]static void Postfix(MONSTER_DATA __instance,MONSTER_DATA __result){try{if(__result!=null)DifficultyProvenance.Observe(__instance,__result);}catch(Exception e){Plugin.Debug("Source provenance unavailable: "+e.Message);}}
}
