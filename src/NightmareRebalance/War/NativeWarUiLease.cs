using Il2CppInterop.Runtime;
using UnityEngine;
using DG.Tweening;
using HierarchyTable = Il2CppSystem.Collections.Generic.Dictionary<HierarchyManager.KeyGameObjectID, Framework.Tuple<string, UnityEngine.GameObject>>;

namespace NightmareRebalance;

// Only owns the freshly created root/clone objects; font, sprite, material and
// prefab assets remain borrowed. No active chapter, UI singleton or camera swap.
internal sealed class NativeWarUiLease
{
    readonly NativeWarReleaseDomain domain = new();
    readonly HierarchyTable privateRoots = new();
    readonly List<Tween> tweens = new();
    readonly List<Material> materials = new();
    readonly HashSet<IntPtr> borrowedMaterials;
    readonly GameObject root;
    Il2CppSystem.Collections.Generic.IEnumerator<WarHDHeader>? routine;
    ResourceLoader? factoryLoader;
    WarHDHeader? header;
    bool destroyRequested, completed;
    internal int Moves, CreatedTweens;
    internal int CreatedMaterials => materials.Count;
    internal bool FactoryLoaderReleased { get; private set; }
    internal WarResourceOwnership Ownership => domain.Ownership;
    internal NativeWarUiLease(HashSet<IntPtr> borrowed, GameObject source, MACHIDAT retained)
    {
        borrowedMaterials = borrowed;
        foreach (var asset in borrowed) Ownership.Observe(WarResourceKind.Asset, asset.ToInt64(), WarResourceOwner.SharedGame);
        Ownership.Observe(WarResourceKind.Asset, source.Pointer.ToInt64(), WarResourceOwner.SharedGame);
        Ownership.Observe(WarResourceKind.Asset, retained.Pointer.ToInt64(), WarResourceOwner.RetainedField);
        root = new GameObject("ReforgedNativeWarUIProbe", new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Type>(new[] { Il2CppType.Of<RectTransform>(), Il2CppType.Of<Canvas>() }));
        root.GetComponent<Canvas>().enabled = false; // Native object lifecycle, not a visual acceptance test.
        Ownership.Observe(WarResourceKind.Instance, root.Pointer.ToInt64(), WarResourceOwner.SessionInstance);
        foreach (var pair in HierarchyManager.keyObjectDictionary) privateRoots.Add(pair.Key, pair.Value);
        var key = (HierarchyManager.KeyGameObjectID)7;
        var previous = HierarchyManager.keyObjectDictionary[key];
        privateRoots[key] = new Framework.Tuple<string, GameObject>(previous.item1, root);
        Ownership.Observe(WarResourceKind.CacheBinding, privateRoots.Pointer.ToInt64(), WarResourceOwner.SessionAcquisition);
    }

    void Within(Action operation)
    {
        var original = HierarchyManager.keyObjectDictionary;
        domain.Within(() =>
        {
            try { HierarchyManager.keyObjectDictionary = privateRoots; operation(); }
            finally { HierarchyManager.keyObjectDictionary = original; }
        });
        if (HierarchyManager.keyObjectDictionary.Pointer != original.Pointer) throw new InvalidOperationException("Hierarchy owner not restored.");
    }

    internal bool Step()
    {
        if (completed) return true;
        if (destroyRequested || Moves > 120) throw new InvalidOperationException("UI lease is closed or exceeded its bounded factory stages.");
        Within(() => routine ??= WarHDHeader.LoadInstance((HierarchyManager.KeyGameObjectID)7));
        bool moved = false;
        Within(() => moved = routine!.Cast<Il2CppSystem.Collections.IEnumerator>().MoveNext()); Moves++;
        // Exact metadata field name independently verified in the installed binary.
        factoryLoader = routine!.Cast<Il2CppSystem.Object>().GetIl2CppType().GetField("<loader>5__3", Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.NonPublic)?.GetValue(routine.Cast<Il2CppSystem.Object>())?.Cast<ResourceLoader>();
        if (factoryLoader == null) throw new InvalidOperationException("Native header loader ownership missing.");
        if (routine.Current != null)
        {
            header = routine.Current;
            if (header.transform.parent.Pointer != root.transform.Pointer || header.bars?.Length != 3 || header.titleText == null || header.vsText == null)
                throw new InvalidOperationException("Native header instance/source/parent guard failed.");
            foreach (var node in header.GetComponentsInChildren<Transform>(true))
                Ownership.Observe(WarResourceKind.Instance, node.gameObject.Pointer.ToInt64(), WarResourceOwner.SessionInstance);
            foreach (var animation in header.GetComponentsInChildren<DOTweenAnimation>(true))
            {
                if (animation.target != null && !animation.target.transform.IsChildOf(root.transform))
                    throw new InvalidOperationException("Animation target is outside the owned instance.");
                if (animation.targetGO != null && !animation.targetGO.transform.IsChildOf(root.transform))
                    throw new InvalidOperationException("Animation object is outside the owned instance.");
                if (animation.tween != null && !tweens.Any(t => t.Pointer == animation.tween.Pointer)) tweens.Add(animation.tween);
            }
            CreatedTweens = tweens.Count;
            foreach (var fonts in header.GetComponentsInChildren<UIFontBehabiour>(true))
            {
                foreach (var text in fonts._texts)
                    if (text == null || !text.transform.IsChildOf(root.transform)) throw new InvalidOperationException("Borrowed font callback target.");
                foreach (var material in fonts.materialList)
                {
                    if (material == null || borrowedMaterials.Contains(material.Pointer))
                    {
                        Ownership.Observe(WarResourceKind.Instance, material?.Pointer.ToInt64() ?? -1, WarResourceOwner.Unknown);
                        throw new InvalidOperationException("Unknown/borrowed material in native destructive cleanup list.");
                    }
                    if (!materials.Any(m => m.Pointer == material.Pointer)) materials.Add(material);
                    Ownership.Observe(WarResourceKind.Instance, material.Pointer.ToInt64(), WarResourceOwner.SessionInstance);
                }
            }
        }
        if (moved) return false;
        if (header == null) throw new InvalidOperationException("Original header factory did not return an instance.");
        completed = true; return true;
    }

    internal void RequestCleanup()
    {
        if (destroyRequested) return;
        domain.RequireKnownCleanupOwnership();
        if (!completed || !Ownership.CanRelease(WarResourceKind.Instance, root.Pointer.ToInt64())) throw new InvalidOperationException("UI cleanup ownership not established.");
        destroyRequested = true;
        UnityEngine.Object.Destroy(root); // Original component OnDestroy cleans only this clone's tweens/meshes.
    }

    internal bool CleanupComplete(ResourceLoader auditAcquisition)
    {
        if (!destroyRequested || root != null) return false;
        if (tweens.Any(t => TweenExtensions.IsActive(t))) throw new InvalidOperationException("Owned header tween survived original OnDestroy.");
        if (materials.Any(m => m != null)) throw new InvalidOperationException("Owned font material survived original OnDestroy.");
        // Factory releases its own acquisition before yielding the header. After
        // releasing our separately held audit reference, BOTH handles must expire.
        if (auditAcquisition.handle.IsValid()) throw new InvalidOperationException("Audit reference must be released separately.");
        FactoryLoaderReleased = factoryLoader != null && !factoryLoader.handle.IsValid();
        if (!FactoryLoaderReleased) throw new InvalidOperationException("Factory acquisition was not balanced.");
        return true;
    }
}
