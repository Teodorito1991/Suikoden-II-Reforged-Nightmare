using HarmonyLib;
using Il2CppInterop.Runtime;
using System.Runtime.CompilerServices;
using ReleaseTable = Il2CppSystem.Collections.Generic.Dictionary<ReleaseCheck.ReleaseTimming, ReleaseCheck.ReleaseInfo>;

namespace NightmareRebalance;

// A bounded adapter for the real ReleaseCheck registration/WarChapter teardown seam.
// Deliberately NOT a live-session sandbox: no ownership inferred from timing bucket 4.
internal sealed class NativeWarReleaseDomain
{
    static NativeWarReleaseDomain? entered;
    readonly ReleaseTable table = new();
    readonly HashSet<IntPtr> owned = new();
    readonly int thread = Environment.CurrentManagedThreadId;
    bool closed, faulted;
    internal int Registrations { get; private set; }
    internal int NativeTeardowns { get; private set; }
    internal bool TeardownAttempted => closed;
    internal bool Faulted => faulted;
    internal static bool Busy => entered != null;

    static void Guard()
    {
        if (!NativeWarBridgeProbe.Requested || !UnityEngine.Application.isBatchMode ||
            !Environment.GetCommandLineArgs().Contains("--reforged-test-exit") ||
            Combat.State != null || WarRuntime.Active || WarTacticsRuntime.Armed ||
            war_h.g_w_war_no != 0 || NativeWarWitness.Live ||
            Framework.Singleton<GAME_WORK>.instance?.sys_work?.mcon != null ||
            GRChapterManager.GRInstance?.activeChapter is MapChapter or WarChapter)
            throw new InvalidOperationException("Release domains require an explicitly isolated unloaded probe.");
    }

    internal NativeWarReleaseDomain() { Guard(); }

    // No yield, no coroutine, no chapter manager update may cross this scope.
    // Native calls below have bounded, independently inspected bodies on SupportedBuild.
    void CheckCanEnter()
    {
        if (Environment.CurrentManagedThreadId != thread || entered != null || faulted)
            throw new InvalidOperationException("Release domain is busy, faulted or on a different thread.");
        Guard();
    }

    internal void Within(Action operation)
    {
        CheckCanEnter();
        RuntimeHelpers.RunClassConstructor(typeof(ReleaseCheck).TypeHandle);
        var retained = ReleaseCheck.releaseDelegate;
        entered = this;
        try { ReleaseCheck.releaseDelegate = table; operation(); }
        catch { faulted = true; throw; }
        finally { ReleaseCheck.releaseDelegate = retained; entered = null; }
    }

    internal void Register(ResourceLoader loader)
    {
        CheckCanEnter();
        if (closed || faulted || loader == null || loader.IsLoading || loader.IsScene ||
            !loader.handle.IsValid() || loader.Asset == null || owned.Contains(loader.Pointer))
            throw new InvalidOperationException("Only a distinct, completed, explicitly owned asset load can be registered.");
        // Caller transfers ONE acquired ResourceLoader reference; never register borrowed
        // game loaders, scene handles, unknown callbacks or shared cache entries.
        Within(() => ReleaseCheck.AddLoader(loader, (ReleaseCheck.ReleaseTimming)4));
        owned.Add(loader.Pointer); Registrations++;
    }

    internal bool Close()
    {
        if (closed) return false;
        CheckCanEnter();
        // The inspected GSD2 destructor only invokes ReleaseCheck timing 4. This raw
        // scratch object is NEVER registered with Chapter or sent Start/Update.
        RuntimeHelpers.RunClassConstructor(typeof(WarChapter).TypeHandle);
        var scratch = new WarChapter(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<WarChapter>.NativeClassPtr));
        closed = true; // Never retry an unload after an uncertain exception.
        Within(scratch.OnDestroy);
        NativeTeardowns++;
        if (table.Count != 0) throw new InvalidOperationException("Native teardown left unconsumed release entries.");
        owned.Clear();
        return true;
    }

    internal bool Empty => table.Count == 0;
}
