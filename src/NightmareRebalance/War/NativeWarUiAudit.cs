using UnityEngine;
using DG.Tweening;
namespace NightmareRebalance;
// Shared audited header provenance and read-only native identity checks.
internal sealed class NativeWarUiAudit
{
    internal readonly HashSet<IntPtr> borrowedMaterials = new();
    internal TMPro.TMP_FontAsset? sharedFont;
    internal Material? sharedFontMaterial;
    void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); }
    internal void Audit(GameObject source)
    {
        var allowed = new HashSet<string> { "UnityEngine.RectTransform", "UnityEngine.CanvasGroup", "UnityEngine.CanvasRenderer", "UnityEngine.UI.Image", "TMPro.TextMeshProUGUI", "DG.Tweening.DOTweenAnimation", "UnityEngine.UI.HorizontalLayoutGroup", "UnityEngine.UI.VerticalLayoutGroup", "UnityEngine.UI.ContentSizeFitter", "UnityEngine.UI.LayoutElement", "ShareUI.Component.FontColorSetter", "UIFontBehabiour", "WarHDHeader" };
        foreach (var component in source.GetComponentsInChildren<Component>(true))
            Check(component != null && allowed.Contains(component.GetIl2CppType().FullName), "Unowned prefab component: " + component?.GetIl2CppType().FullName);
        foreach (var animation in source.GetComponentsInChildren<DOTweenAnimation>(true))
        {
            Check(animation.target == null || animation.target.transform.IsChildOf(source.transform), "Borrowed animation target.");
            Check(animation.targetGO == null || animation.targetGO.transform.IsChildOf(source.transform), "Borrowed animation object.");
            Check(!animation.autoPlay && !animation.hasOnStart && !animation.hasOnPlay && !animation.hasOnUpdate && !animation.hasOnStepComplete && !animation.hasOnComplete && !animation.hasOnTweenCreated && !animation.hasOnRewind, "Unclassified animation callback/autoplay.");
            foreach (var ev in new[] { animation.onStart, animation.onPlay, animation.onUpdate, animation.onStepComplete, animation.onComplete, animation.onTweenCreated, animation.onRewind })
                Check(ev == null || ev.GetPersistentEventCount() == 0, "Unclassified persistent Unity event.");
        }
        Check(source.GetComponent<WarHDHeader>()?.bars?.Length == 3, "Audited header layout identity.");
        foreach (var fonts in source.GetComponentsInChildren<UIFontBehabiour>(true))
        {
            Check(fonts.materialList.Count == 0 && fonts._texts.Length == 3, "Source font ownership/preinitialized-material guard.");
            foreach (var text in fonts._texts) Check(text != null && text.transform.IsChildOf(source.transform), "Source callback target is within prefab.");
        }
        foreach (var text in source.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            if (text.fontSharedMaterial != null) borrowedMaterials.Add(text.fontSharedMaterial.Pointer);
        var globalFont = SystemObject.GetLanguageFont();
        Check(globalFont != null && globalFont.material != null, "Original language font initialized before native instance creation.");
        borrowedMaterials.Add(globalFont!.material!.Pointer);
        sharedFont = globalFont; sharedFontMaterial = globalFont.material;
    }

    internal static string FontRegistry() => string.Join(",", UIFontBehabiour.m_actives.ToArray().Select(x => x.Pointer.ToInt64()).OrderBy(x => x));

    internal static string Globals()
    {
        var rows = new List<string>();
        foreach (var pair in HierarchyManager.keyObjectDictionary)
            rows.Add($"{pair.Key}:{pair.Value.Pointer}:{pair.Value.item2?.Pointer}:{(pair.Value.item2 != null ? pair.Value.item2.transform.childCount : -1)}");
        rows.Sort();
        return HierarchyManager.keyObjectDictionary.Pointer + "|" + WarRes.header?.Pointer + "|" + WarRes.uiText?.Pointer + "|" + WarRes.tips?.Pointer + "|" +
            WarRes.war_csr_data?.Pointer + "|" + WarRes.war_icon_data?.Pointer + "|" + WarRes.war_arrow_data?.Pointer + "|" + WarRes.war_face_data?.Pointer + "|" + ReleaseCheck.releaseDelegate?.Pointer + "|" + string.Join(";", rows);
    }

}
