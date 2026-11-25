using HarmonyLib;
namespace NightmareRebalance;
// Owned-session controls. Ordinary campaign/system settings saves are untouched.
[HarmonyPatch(typeof(SystemSave),nameof(SystemSave.Save))]
internal static class TrainingNativeSave
{
    static bool Prefix(string path,string json,ref bool __result)
    {if(TrainingPersistenceRules.IsCampaignPath(path)&&HallLucaSession.Active){__result=false;return false;}if(TrainingWar.AllowNativeSave(path,json))return true;__result=false;return false;}
}
[HarmonyPatch(typeof(UISaveLoad2),nameof(UISaveLoad2.Save))]
internal static class TrainingSaveIntent
{static bool Prefix()=>!HallLucaSession.Active&&TrainingWar.AllowSaveIntent();}
[HarmonyPatch(typeof(GSD2SaveData),nameof(GSD2SaveData.LoadDataCore))]
internal static class TrainingLoaded
{static void Postfix()=>TrainingWar.NativeLoadCompleted();}
