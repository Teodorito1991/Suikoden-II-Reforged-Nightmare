namespace NightmareRebalance;

internal enum OptionalEncounterKind { None = 0, TowerOfDunan = 1, SecretSuperboss = 2, CirculatingPaths = 3 }

// Explicit ownership lease tying an optional mod activity (Tower of Dunan, Secret Superboss,
// or Sindar Spire Guardian) to an exact accepted Hall request and native BATTLE_WORK pointer.
// Unrelated WinEnd / NotWinEnd / battle_exit99 callbacks can never complete or fail this lease.
internal sealed class OptionalEncounterLease
{
    static long nextToken;

    internal long Token { get; }
    internal OptionalEncounterKind Kind { get; }
    internal string EncounterId { get; }
    internal string HallTrialId { get; private set; } = "";
    internal IntPtr BattleWorkPtr { get; private set; }
    internal bool Accepted { get; private set; }
    internal bool BattleActive { get; private set; }
    internal bool VictoryAwarded { get; private set; }
    internal bool Terminated { get; private set; }

    internal OptionalEncounterLease(OptionalEncounterKind kind, string encounterId)
    {
        Token = System.Threading.Interlocked.Increment(ref nextToken);
        Kind = kind;
        EncounterId = encounterId ?? "";
    }

    internal bool MarkAccepted(string hallTrialId)
    {
        if (Terminated || Accepted || string.IsNullOrWhiteSpace(hallTrialId)) return false;
        HallTrialId = hallTrialId;
        Accepted = true;
        return true;
    }

    internal bool BindBattleWork(IntPtr battleWorkPtr)
    {
        if (Terminated || !Accepted || battleWorkPtr == IntPtr.Zero || BattleWorkPtr != IntPtr.Zero)
            return false;
        BattleWorkPtr = battleWorkPtr;
        BattleActive = true;
        return true;
    }

    internal bool MatchesWork(IntPtr battleWorkPtr) =>
        !Terminated && Accepted && BattleActive && BattleWorkPtr != IntPtr.Zero && BattleWorkPtr == battleWorkPtr;

    internal bool TryClaimVictory(IntPtr battleWorkPtr)
    {
        if (!MatchesWork(battleWorkPtr) || VictoryAwarded) return false;
        VictoryAwarded = true;
        BattleActive = false;
        Terminated = true;
        return true;
    }

    internal bool TryClaimNonVictory(IntPtr battleWorkPtr)
    {
        if (!MatchesWork(battleWorkPtr) || VictoryAwarded) return false;
        BattleActive = false;
        Terminated = true;
        return true;
    }

    internal bool CancelOrTeardown()
    {
        if (Terminated) return false;
        BattleActive = false;
        Terminated = true;
        return true;
    }
}
