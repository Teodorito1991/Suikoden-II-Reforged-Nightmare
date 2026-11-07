using HarmonyLib;
namespace NightmareRebalance;
// Observe the native walking encounter branch; never request or launch a battle.
internal static class HuntLaunchProof
{
 [ThreadStatic]internal static IntPtr Checking;
 static IntPtr pendingTable,work,table;
 internal static bool FixtureProof;
 internal static bool PendingWalkingRequest=>pendingTable!=IntPtr.Zero;
 internal static void ClearRequest(){pendingTable=IntPtr.Zero;}
 internal static void Requested(EVENTCON caller,byte type)
 {
  bool walking=type==0&&Checking==caller.Pointer;
  var source=Framework.Singleton<GAME_WORK>.instance?.sys_work?.battle_data;
  CastleGuildRuntime.WalkingRequest(caller,source,walking);
  HuntAccess.Requested(source,walking);
  if(!walking)return;
  if(source!=null)pendingTable=source.Pointer;
 }
 internal static void Setup(BATTLE_WORK w){work=w.Pointer;table=pendingTable;pendingTable=IntPtr.Zero;FixtureProof=false;}
 internal static bool Matches(BATTLE_WORK w)=>work==w.Pointer&&table!=IntPtr.Zero&&w.battle_data?.Pointer==table;
 internal static bool MatchesSource(BATTLE_WORK w,BATTLE_DATA? source)=>work==w.Pointer&&table!=IntPtr.Zero&&source?.Pointer==table;
 internal static void FixtureRequest(BATTLE_DATA source)
 {if(!DifficultyProvenance.StrictProbe||!Plugin.Options.RunCombatSelfTest.Value||!UnityEngine.Application.isBatchMode)throw new InvalidOperationException("Fixture request is batch-probe only");pendingTable=source.Pointer;}
 internal static void Fixture(BATTLE_WORK w)
 {if(!DifficultyProvenance.StrictProbe||!Plugin.Options.RunCombatSelfTest.Value||!UnityEngine.Application.isBatchMode)throw new InvalidOperationException("Fixture proof is batch-probe only");work=w.Pointer;table=w.battle_data.Pointer;FixtureProof=true;}
 internal static void Clear(){pendingTable=work=table=IntPtr.Zero;FixtureProof=false;}
}
[HarmonyPatch(typeof(EVENTCON),nameof(EVENTCON.EventBatlleCheck))]
internal static class HuntWalkingEncounterScope
{
 [HarmonyPrefix]static void Prefix(EVENTCON __instance,out IntPtr __state){__state=HuntLaunchProof.Checking;HuntLaunchProof.Checking=__instance.Pointer;}
 [HarmonyFinalizer]static Exception? Finalizer(IntPtr __state,Exception? __exception){HuntLaunchProof.Checking=__state;if(__exception!=null)HuntLaunchProof.ClearRequest();return __exception;}
}
[HarmonyPatch(typeof(EVENTCON),nameof(EVENTCON.EventBatlleSet))]
internal static class HuntNativeRequestObserver
{
 [HarmonyPrefix]static void Prefix()=>HuntLaunchProof.ClearRequest();
 [HarmonyPostfix]static void Postfix(EVENTCON __instance,byte btyp){try{HuntLaunchProof.Requested(__instance,btyp);}catch{HuntLaunchProof.ClearRequest();}}
}
