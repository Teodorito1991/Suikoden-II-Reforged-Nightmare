using HarmonyLib;
namespace NightmareRebalance;
internal static class HallLucaOpeningScope
{
    internal static HallLucaHpView? Open(BATTLE_WORK work)=>HallFirstContact.GameForOwnedLucaStage(work)!=null?
        HallLucaHpView.OpenForOwnedStage(work,HallLucaSession.CarryHp):null;
    internal static Exception? Close(Exception? error,HallLucaHpView? state){state?.Dispose();return error;}
}
[HarmonyPatch(typeof(boss_luc2),nameof(boss_luc2.luc2_special_mae_func00))]
[HallTrialPatch]
internal static class HallLucaOpening2
{
    static void Prefix(BATTLE_WORK battle_work,out HallLucaHpView? __state)=>__state=HallLucaOpeningScope.Open(battle_work);
    static Exception? Finalizer(Exception? __exception,HallLucaHpView? __state)=>HallLucaOpeningScope.Close(__exception,__state);
}
[HarmonyPatch(typeof(boss_luc3),nameof(boss_luc3.luc3_special_mae_func00))]
[HallTrialPatch]
internal static class HallLucaOpening3
{
    static void Prefix(BATTLE_WORK battle_work,out HallLucaHpView? __state)=>__state=HallLucaOpeningScope.Open(battle_work);
    static Exception? Finalizer(Exception? __exception,HallLucaHpView? __state)=>HallLucaOpeningScope.Close(__exception,__state);
}
// Observe each original threshold/finished command, without replacing it or inventing a victory.
[HarmonyPatch(typeof(boss_luc1),nameof(boss_luc1.luc1_boss_command_func00))]
[HallTrialPatch]
internal static class HallLucaNativeCommand1
{
    static void Postfix(BATTLE_WORK battle_work,int __result)=>HallLucaSession.CommandCompleted(battle_work,__result);
}
[HarmonyPatch(typeof(boss_luc2),nameof(boss_luc2.luc2_boss_command_func00))]
[HallTrialPatch]
internal static class HallLucaNativeCommand2
{
    static void Postfix(BATTLE_WORK battle_work,int __result)=>HallLucaSession.CommandCompleted(battle_work,__result);
}
