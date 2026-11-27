using HarmonyLib;
namespace NightmareRebalance;
internal static class HuntFieldContext
{
 internal static readonly NativeFieldWindow Window=new();
 internal static bool Active=>Window.IsCurrent(UnityEngine.Time.frameCount);
}
[HarmonyPatch(typeof(MapChapter),nameof(MapChapter.Update))]
internal static class HuntFieldUpdate
{
 [HarmonyPostfix]static void Postfix(){HuntFieldContext.Window.Record(UnityEngine.Time.frameCount);HuntSessionRuntime.FieldUpdate();}
}
[HarmonyPatch(typeof(MapChapter),nameof(MapChapter.OnDestroy))]
internal static class HuntFieldDestroy
{
 [HarmonyPrefix]static void Prefix(){HuntAccess.LeavingField();HuntGuildRuntime.Clear();}
}
