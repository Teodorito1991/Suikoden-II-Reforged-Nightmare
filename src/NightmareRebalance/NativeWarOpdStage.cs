using Il2CppInterop.Runtime;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using OpdCache = Il2CppSystem.Collections.Generic.Dictionary<string, OPD_RES2>;

namespace NightmareRebalance;

// Drives just the original War01 OPD sub-coroutine used by WarResLoad. Every
// native MoveNext has bounded cache/release ownership; no scope spans a frame.
internal sealed class NativeWarOpdStage
{
    readonly NativeWarReleaseDomain domain = new();
    readonly OpdCache cache = new();
    readonly Stack<Il2CppSystem.Collections.IEnumerator> stack = new();
    readonly int thread = Environment.CurrentManagedThreadId;
    ResourceLoader? acquisition;
    string? sourceFingerprint;
    bool started, registered, finished, cleanupAttempted;
    internal int Moves { get; private set; }
    internal int AtlasNoOpStages { get; private set; }
    internal bool Complete => finished;
    internal bool HandleReleased { get; private set; }
    internal bool OriginalCacheRestored { get; private set; }
    internal WarResourceOwnership Ownership => domain.Ownership;

    internal NativeWarOpdStage(MACHIDAT retained)
    {
        Ownership.Observe(WarResourceKind.Asset, retained.Pointer.ToInt64(), WarResourceOwner.RetainedField);
        RuntimeHelpers.RunClassConstructor(typeof(OPDResLoader).TypeHandle);
    }

    void Within(Action action)
    {
        if (thread != Environment.CurrentManagedThreadId) throw new InvalidOperationException("OPD stage thread mismatch.");
        var original = OPDResLoader.opdData;
        var originalMode = SystemObject.mode;
        domain.Within(() =>
        {
            // The unloaded launcher starts with GSD1 resource routing. This
            // inspected native setter changes only the transient selector; never
            // leave it changed across a yield or use it to dispatch a chapter.
            try
            {
                SystemObject.Changemode(SystemObject.Mode.GSD2);
                OPDResLoader.opdData = cache;
                action();
            }
            finally { OPDResLoader.opdData = original; SystemObject.Changemode(originalMode); }
        });
        if (OPDResLoader.opdData?.Pointer != original?.Pointer || SystemObject.mode != originalMode) throw new InvalidOperationException("OPD cache/resource selector not restored.");
        OriginalCacheRestored = true;
    }

    internal bool Step()
    {
        if (finished) return true;
        if (!started)
        {
            Within(() => stack.Push(OPDResLoader.LoadOPDRes(OPDResLoader.Type.War01, "war_01")));
            started = true;
        }
        if (acquisition != null && !registered)
        {
            if (acquisition.IsLoading) return false;
            if (!acquisition.handle.IsValid() || acquisition.handle.Status != AsyncOperationStatus.Succeeded || acquisition.Asset?.name != "war_01")
                throw new InvalidOperationException($"Original War01 OPD acquisition failed (valid={acquisition.handle.IsValid()}, status={(acquisition.handle.IsValid() ? acquisition.handle.Status.ToString() : "invalid")}, asset={acquisition.Asset?.name ?? "null"}).");
            var data = acquisition.Asset!.Cast<OPD_RES2>();
            if (data.map?.Count != 1 || data.opd_tbl?.Count != 19) throw new InvalidOperationException("War01 source signature rejected.");
            // Acquire ONE reference; all nested asset objects remain borrowed/read-only.
            domain.Register(acquisition, WarResourceOwner.SessionAcquisition);
            Ownership.Observe(WarResourceKind.Asset, data.Pointer.ToInt64(), WarResourceOwner.SharedGame);
            foreach (var opd in data.opd_tbl!)
            {
                if (opd == null) throw new InvalidOperationException("Missing War01 sprite record.");
                Ownership.Observe(WarResourceKind.Asset, opd.Pointer.ToInt64(), WarResourceOwner.SharedGame);
            }
            sourceFingerprint = SourceFingerprint(data);
            registered = true;
        }
        if (Moves > 96) throw new InvalidOperationException("Unexpected OPD coroutine depth/stages.");
        bool moved = false;
        Within(() => moved = stack.Peek().MoveNext());
        Moves++;
        if (!moved) stack.Pop();
        else
        {
            var current = stack.Peek().Current;
            if (acquisition == null)
            {
                acquisition = current?.TryCast<ResourceLoader>() ?? throw new InvalidOperationException("Expected original native loader yield.");
                // Wait for the real async callback outside all swapped-cache scopes.
                return false;
            }
            // On this supported build LoadAtlas(folder) yields an empty coroutine.
            // No arbitrary nested work is admitted: exact native class provenance.
            var nested = current?.TryCast<Il2CppSystem.Collections.IEnumerator>();
            if (nested == null || current!.GetIl2CppType().FullName != "OPD_DATA+<LoadAtlas>d__14")
            {
                Ownership.Observe(WarResourceKind.Callback, current?.Pointer.ToInt64() ?? -1, WarResourceOwner.Unknown);
                throw new InvalidOperationException("Unknown nested OPD operation; transition and cleanup blocked.");
            }
            AtlasNoOpStages++;
            stack.Push(nested);
        }
        if (stack.Count != 0) return false;
        if (!registered || AtlasNoOpStages != 19 || cache.Count != 1) throw new InvalidOperationException("Incomplete original OPD publication.");
        Within(() =>
        {
            if (!OPDResLoader.IsLoaded("war_01")) throw new InvalidOperationException("Original cache publication missing.");
            var data = acquisition!.Asset.Cast<OPD_RES2>();
            for (int i = 0; i < 19; i++)
                if (OPDResLoader.GetData("war_01", i)?.Pointer != data.opd_tbl[i].Pointer) throw new InvalidOperationException("Native OPD lookup differs.");
            if (SourceFingerprint(data) != sourceFingerprint) throw new InvalidOperationException("Borrowed OPD source was mutated.");
            Ownership.Observe(WarResourceKind.CacheBinding, cache.Pointer.ToInt64(), WarResourceOwner.SessionAcquisition);
        });
        finished = true;
        return true;
    }

    internal void Close()
    {
        if (cleanupAttempted) return;
        if (!Ownership.CanClean) throw new InvalidOperationException("Unknown ownership blocks OPD cleanup.");
        if (!finished) throw new InvalidOperationException("Pending OPD initialization is not a completed cleanup receipt.");
        domain.RequireKnownCleanupOwnership();
        if (!Ownership.CanRelease(WarResourceKind.CacheBinding, cache.Pointer.ToInt64())) throw new InvalidOperationException("Cache binding is not owned.");
        cleanupAttempted = true;
        // Remove only our private association. RemoveResPack would call DeleteAtlas
        // on borrowed OPD_DATA objects, so it is intentionally NOT a cleanup shortcut.
        Within(() => { if (!cache.Remove("war_01")) throw new InvalidOperationException("Owned cache binding absent."); });
        domain.Close(); // Actual WarChapter.OnDestroy -> ReleaseInfo -> Unload.
        HandleReleased = acquisition != null && !acquisition.handle.IsValid();
        if (!HandleReleased || !domain.Empty || cache.Count != 0) throw new InvalidOperationException("OPD cleanup incomplete.");
    }

    internal static bool RehearseFault(MACHIDAT retained)
    {
        var originalCache = OPDResLoader.opdData;
        var originalMode = SystemObject.mode;
        var originalRelease = ReleaseCheck.releaseDelegate;
        var test = new NativeWarOpdStage(retained);
        bool thrown = false;
        try { test.Within(() => throw new ApplicationException("Injected OPD scope failure")); }
        catch (ApplicationException) { thrown = true; }
        return thrown && OPDResLoader.opdData?.Pointer == originalCache?.Pointer && SystemObject.mode == originalMode &&
            ReleaseCheck.releaseDelegate?.Pointer == originalRelease?.Pointer && !NativeWarReleaseDomain.Busy;
    }

    static string SourceFingerprint(OPD_RES2 data)
    {
        var rows = new List<string>();
        foreach (var opd in data.opd_tbl)
            rows.Add($"{opd.Pointer}:{opd.atlas?.Pointer}:{opd.normal_atlas?.Pointer}:{opd.sprites?.Pointer}:{opd.sprites?.Count}:{opd.normal_sprites?.Pointer}:{opd.normal_sprites?.Count}:{opd.anime_table?.Pointer}:{opd.anime_table?.Count}:{opd.pattern_table?.Pointer}:{opd.pattern_table?.Count}");
        return string.Join(";", rows); // Private assertion only; never published.
    }
}
