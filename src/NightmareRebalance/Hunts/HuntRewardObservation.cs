using HarmonyLib;
namespace NightmareRebalance;
// Observe the real reward state machine's credit; never issue an extra credit.
internal static class HuntRewardObservation
{
 [ThreadStatic]internal static bool InNativeReward;
 internal static readonly List<object> Credits=new();
 internal static void Record(int mode,int value,PARTY_DATA? party,uint before)
 {
  if(!InNativeReward||mode!=1||party==null||Credits.Count>=32)return;
  HuntSessionRuntime.Current?.Credit(mode,before,party.gold);
  Credits.Add(new{Requested=value,Before=before,After=party.gold,ActualDelta=(long)party.gold-before,NativeCap=999999,Meaning="Observed original purse mutation inside exit_sub20; ending/field return not inferred"});
 }
}
[HarmonyPatch(typeof(BattleManager),nameof(BattleManager.exit_sub20))]
internal static class HuntPayoutScope
{
 [HarmonyPrefix]static void Prefix(BattleManager __instance,out bool __state){__state=HuntRewardObservation.InNativeReward;HuntRewardObservation.InNativeReward=HuntRuntime.Work==__instance.battle_work.Pointer&&HuntRuntime.Party==__instance.battle_work.monster_party_data?.Pointer&&HuntRuntime.ActiveRoster&&Combat.Matches(__instance)&&Combat.State is{SnapshotTaken:true}&&CombatReforgedDiagnostics.Outcome=="VictorySignal";}
 [HarmonyFinalizer]static Exception? Finalizer(bool __state,Exception? __exception){HuntRewardObservation.InNativeReward=__state;return __exception;}
}
[HarmonyPatch(typeof(G2_SYS),nameof(G2_SYS.G2_party_gold))]
internal static class HuntNativePurseObservation
{
 [HarmonyPrefix]static void Prefix(out (PARTY_DATA? Party,uint Gold) __state){var p=HuntRewardObservation.InNativeReward?Framework.Singleton<GAME_WORK>.instance?.party_data:null;__state=(p,p?.gold??0);}
 [HarmonyPostfix]static void Postfix(int mode,int val,(PARTY_DATA? Party,uint Gold) __state){try{HuntRewardObservation.Record(mode,val,__state.Party,__state.Gold);}catch(Exception e){Plugin.Debug("Purse observation unavailable: "+e.Message);}}
}
