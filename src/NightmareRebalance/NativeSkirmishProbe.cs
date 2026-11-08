using UnityEngine;
using System.Text.Json;

namespace NightmareRebalance;

public sealed class NativeSkirmishProbe : MonoBehaviour
{
    internal static bool Requested => Plugin.Options.RunNativeWarSelfTest.Value && Plugin.Options.RunCombatSelfTest.Value && Application.isBatchMode &&
        Environment.GetCommandLineArgs().Contains("--reforged-skirmish-session-probe") && Environment.GetCommandLineArgs().Contains("--reforged-test-exit") &&
        Environment.GetCommandLineArgs().Contains("--reforged-private-startup-smoke") && File.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "PRIVATE_DO_NOT_UPLOAD.txt"));
    internal static bool Scheduled;
    NativeSkirmishSession? session;
    readonly List<object> records = new();
    readonly HashSet<Guid> ids = new();
    float started;
    bool finished, cancellationRequested;
    int runtimeErrors;
    Application.LogCallback? observer;
    public NativeSkirmishProbe(IntPtr p) : base(p) { }

    public void Update()
    {
        if (finished || !Requested) return;
        try
        {
            if (started == 0)
            {
                started = Time.realtimeSinceStartup; Application.runInBackground = true;
                observer = (Application.LogCallback)(Action<string, string, LogType>)((_, _, type) => { if (type is LogType.Exception or LogType.Error or LogType.Assert) runtimeErrors++; });
                Application.add_logMessageReceived(observer); return;
            }
            if (Time.realtimeSinceStartup - started < 2) return;
            if (Time.realtimeSinceStartup - started > 70) throw new TimeoutException("Integrated native session exceeded its disposable runtime budget.");
            if (session == null)
            {
                session = new NativeSkirmishSession();
                if (!ids.Add(session.Id)) throw new InvalidOperationException("Session identity reused.");
                bool rejected = false;
                try { _ = new NativeSkirmishSession(); } catch (InvalidOperationException) { rejected = true; }
                if (!rejected) throw new InvalidOperationException("Concurrent session owner admitted.");
                cancellationRequested = false;
            }
            // Two complete combined lifecycles, then interruption while native OPD
            // acquisition is pending. Drain only verified resource stages; no combat.
            if (records.Count == 2 && session.State == NativeSkirmishSession.Phase.Opd && !cancellationRequested)
            { session.RequestStop(); cancellationRequested = true; }
            session.Step();
            if (session.Ready) session.RequestStop();
            if (!session.Disposed) return;
            session.RequestStop(); session.Step(); // Idempotent post-disposal calls.
            if (NativeSkirmishSession.Busy || runtimeErrors != 0 || session.Unknown != 0 || !session.OpdReleased ||
                (records.Count < 2 && (!session.HadHeader || !session.HeaderReleased || session.Tweens != 11 || session.Materials != 3)))
                throw new InvalidOperationException("Integrated cleanup/registry/reference receipt failed.");
            records.Add(new { Session = session.Id, OpdNativeMoves = session.OpdMoves, session.HadHeader, session.OpdReleased,
                session.HeaderReleased, session.Tweens, session.Materials, session.Unknown, InterruptedDuringOpd = cancellationRequested,
                Disposed = session.Disposed, ResourceOwnerReleased = !NativeSkirmishSession.Busy, RuntimeErrors = runtimeErrors });
            session = null;
            if (records.Count == 3) Finish(true, null);
        }
        catch (Exception e) { Plugin.Logger.LogError("Integrated native session failed: " + e.Message); Finish(false, e.GetType().Name + ": " + e.Message); }
    }

    void Finish(bool passed, string? failure)
    {
        finished = true;
        if (observer != null) Application.remove_logMessageReceived(observer);
        try
        {
            string dir = Path.Combine(BepInEx.Paths.BepInExRootPath, "ReforgedNightmare", "native-war"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "integrated-session-probe.json"), JsonSerializer.Serialize(new { Version = "1.18.0-field-observation.1", Passed = passed, Failure = failure,
                Sessions = records, RuntimeErrors = runtimeErrors, FailedPhase = session?.State.ToString(), GraphicsEnabled = !Environment.GetCommandLineArgs().Contains("-nographics"),
                LauncherEnabled = WarBoundaryRules.CanLaunch, ActiveWarChapter = false, GenuineFieldRetained = false, FieldReturn = false, EarnedWarOutcome = false, CampaignLoaded = false }, new JsonSerializerOptions { WriteIndented = true }));
            Plugin.Logger.LogInfo($"Integrated native session {(passed ? "PASS" : "FAIL")}: {records.Count} disposed sessions; no tactical battle/field return.");
        }
        finally { Application.Quit(); }
    }
}
