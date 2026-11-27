using HarmonyLib;
namespace NightmareRebalance;

// The stock data coroutine loads this sprite only for story boss index 52.
// Preserve ordinary Hall battle_no=-1 and schedule the same native loader instead.
// Its own ReleaseCheck timing-3 registration remains the cleanup authority.
[HarmonyPatch(typeof(BattleManager),"BattleDataLoad")]
[HallTrialPatch]
internal static class HallExtraTexture
{
    static void Postfix(BattleManager __instance,ref Il2CppSystem.Collections.IEnumerator __result)
    {
        var work=__instance.battle_work;
        if(!HallFirstContact.NeedsOriginalExtraTexture(work)||work.ex_texture!=null)return;
        if(__result==null)throw new InvalidOperationException("Missing native battle data coroutine.");
        var stages=new CoroutineList();
        stages.Add(__result);
        stages.Add(__instance.LoadExTexture());
        __result=stages.WaitForCoroutine();
        Plugin.Logger.LogInfo("HallFirstContact: final encounter scheduled original extra-texture loader; native timing-3 cleanup retained, battle_no=-1.");
    }
}
