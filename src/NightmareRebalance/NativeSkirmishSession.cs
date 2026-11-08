using Il2CppInterop.Runtime;
using UnityEngine;

namespace NightmareRebalance;

// Owns the verified resource-only envelope. It cannot own a live field/chapter.
// Child adapters retain their independently audited release domains and ledgers.
internal sealed class NativeSkirmishSession
{
    internal enum Phase { Created, FieldAsset, Opd, HeaderAsset, Header, Ready, DestroyHeader, Release, Disposed, Blocked }
    static NativeSkirmishSession? owner;
    readonly int thread = Environment.CurrentManagedThreadId;
    readonly NativeWarUiAudit audit = new();
    readonly string baseline = NativeWarUiAudit.Globals();
    readonly string resourceBaseline = NativeWarLifecycleProbe.Fingerprint();
    readonly string fontBaseline = NativeWarUiAudit.FontRegistry();
    readonly IntPtr originalOpd = OPDResLoader.opdData?.Pointer ?? IntPtr.Zero;
    ResourceLoader? sentinel, prefab;
    NativeWarOpdStage? opd;
    NativeWarUiLease? ui;
    NativeWarReleaseDomain? auditDomain;
    bool stop;
    int destroyFrame;
    internal Guid Id { get; } = Guid.NewGuid();
    internal Phase State { get; private set; }
    internal bool Ready => State == Phase.Ready;
    internal bool Disposed => State == Phase.Disposed;
    internal bool HadHeader { get; private set; }
    internal int OpdMoves => opd?.Moves ?? 0;
    internal int Tweens => ui?.CreatedTweens ?? 0;
    internal int Materials => ui?.CreatedMaterials ?? 0;
    internal bool OpdReleased => opd?.HandleReleased == true;
    internal bool HeaderReleased => ui?.FactoryLoaderReleased == true;
    internal int Unknown => (opd?.Ownership.Unknown ?? 0) + (ui?.Ownership.Unknown ?? 0) + (auditDomain?.Ownership.Unknown ?? 0);
    internal string? Failure { get; private set; }
    internal static bool Busy => owner != null;

    internal NativeSkirmishSession()
    {
        if (!NativeSkirmishProbe.Requested || owner != null || NativeWarReleaseDomain.Busy)
            throw new InvalidOperationException("An isolated session already owns the resource envelope or admission is unavailable.");
        owner = this;
    }

    internal void RequestStop()
    {
        CheckOwner();
        if (Disposed) return;
        stop = true; // Pending native acquisitions are drained to verified boundaries.
    }

    void CheckOwner()
    {
        if (thread != Environment.CurrentManagedThreadId || (!Disposed && owner != this) || State == Phase.Blocked)
            throw new InvalidOperationException("Session ownership is unavailable; no cleanup retry permitted.");
    }

    void Preserved()
    {
        if (NativeWarLifecycleProbe.Fingerprint() != resourceBaseline || NativeWarUiAudit.Globals() != baseline || (OPDResLoader.opdData?.Pointer ?? IntPtr.Zero) != originalOpd || NativeWarReleaseDomain.Busy)
            throw new InvalidOperationException("Original resource/cache owner changed outside a bounded native step.");
    }

    internal void Step()
    {
        CheckOwner();
        if (Disposed) return;
        try
        {
            Preserved();
            switch (State)
            {
                case Phase.Created:
                    sentinel = ResourceLoader.LoadAssetAsync<MACHIDAT>("gsd2-classic-machi-vk08-vk08");
                    State = Phase.FieldAsset; break;
                case Phase.FieldAsset:
                    if (sentinel!.IsLoading) return;
                    if (!sentinel.handle.IsValid() || !CastleGuildRuntime.Matches(sentinel.Asset.Cast<MACHIDAT>()))
                        throw new InvalidOperationException("Retained sentinel source mismatch (this is an asset, NOT a live field).");
                    opd = new NativeWarOpdStage(sentinel.Asset.Cast<MACHIDAT>());
                    State = Phase.Opd; break;
                case Phase.Opd:
                    if (!opd!.Step()) return;
                    if (stop) { State = Phase.Release; break; }
                    auditDomain = new NativeWarReleaseDomain();
                    prefab = ResourceLoader.LoadAssetAsync<GameObject>("share-hd-ui-war-ui_war_header");
                    State = Phase.HeaderAsset; break;
                case Phase.HeaderAsset:
                    if (prefab!.IsLoading) return;
                    if (!prefab.handle.IsValid() || prefab.Asset?.name != "UI_War_Header") throw new InvalidOperationException("Original header source mismatch.");
                    audit.Audit(prefab.Asset!.Cast<GameObject>());
                    auditDomain!.Register(prefab, WarResourceOwner.SessionAcquisition);
                    ui = new NativeWarUiLease(audit.borrowedMaterials, prefab.Asset.Cast<GameObject>(), sentinel!.Asset.Cast<MACHIDAT>());
                    State = Phase.Header; break;
                case Phase.Header:
                    if (!ui!.Step()) return;
                    HadHeader = true; State = Phase.Ready; break;
                case Phase.Ready:
                    if (!stop) return;
                    if (Unknown != 0) throw new InvalidOperationException("Unknown ownership blocks session disposal.");
                    ui!.RequestCleanup(); ui.RequestCleanup();
                    destroyFrame = Time.frameCount; State = Phase.DestroyHeader; break;
                case Phase.DestroyHeader:
                    if (Time.frameCount - destroyFrame < 3) return;
                    auditDomain!.Close(); auditDomain.Close();
                    if (!ui!.CleanupComplete(prefab!)) return;
                    if (NativeWarUiAudit.FontRegistry() != fontBaseline || audit.sharedFont == null || audit.sharedFontMaterial == null ||
                        SystemObject.GetLanguageFont().Pointer != audit.sharedFont.Pointer || audit.sharedFont.material.Pointer != audit.sharedFontMaterial.Pointer)
                        throw new InvalidOperationException("Original font callback/material owner was not restored.");
                    State = Phase.Release; break;
                case Phase.Release:
                    if (Unknown != 0) throw new InvalidOperationException("Unknown session release ownership.");
                    opd!.Close(); opd.Close();
                    if (!opd.HandleReleased || !sentinel!.handle.IsValid()) throw new InvalidOperationException("Ordered war release consumed an unrelated acquisition.");
                    Preserved();
                    sentinel.Unload();
                    if (sentinel.handle.IsValid()) throw new InvalidOperationException("Session sentinel acquisition survived release.");
                    if (NativeWarUiAudit.FontRegistry() != fontBaseline) throw new InvalidOperationException("Font registration leak.");
                    State = Phase.Disposed; owner = null; break;
            }
            Preserved();
        }
        catch (Exception e)
        {
            Failure = e.GetType().Name + ": " + e.Message;
            State = Phase.Blocked; // Keep exclusive owner. No second session or speculative release.
            throw;
        }
    }
}
