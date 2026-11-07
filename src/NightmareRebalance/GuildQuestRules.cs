namespace NightmareRebalance;

internal enum GuildQuestState { NoContract, Accepted, AwaitingReport, Reported }

// Session-owned state. Never hydrated from a save, diagnostic journal or slot name.
// No currency, inventory, engine callbacks or persistence writes belong here.
internal sealed class GuildQuest
{
    readonly Queue<string> receipts = new();
    public GuildQuestState State { get; private set; }
    public string? ContractId { get; private set; }
    public string? LastReportedId { get; private set; }
    public string? RunningReceipt { get; private set; }
    public string? CompletedReceipt { get; private set; }
    public long Completions { get; private set; }
    public long Reports { get; private set; }
    public bool FieldReturnObserved { get; private set; }

    internal bool Accept(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || State is not (GuildQuestState.NoContract or GuildQuestState.Reported)) return false;
        ContractId = id; RunningReceipt = CompletedReceipt = null; FieldReturnObserved = false;
        State = GuildQuestState.Accepted; return true;
    }

    internal bool Withdraw()
    {
        if (State != GuildQuestState.Accepted) return false;
        ContractId = RunningReceipt = CompletedReceipt = null; FieldReturnObserved = false;
        State = GuildQuestState.NoContract; return true;
    }

    internal bool Started(string huntId, string receipt)
    {
        if (State != GuildQuestState.Accepted || ContractId != huntId || string.IsNullOrWhiteSpace(receipt) ||
            RunningReceipt != null || receipts.Contains(receipt)) return false;
        RunningReceipt = receipt; return true;
    }

    internal bool Finished(HuntRunProgress run)
    {
        if (!run.Finished || State != GuildQuestState.Accepted || run.HuntId != ContractId ||
            RunningReceipt != run.RunId || receipts.Contains(run.RunId)) return false;
        RunningReceipt = null;
        receipts.Enqueue(run.RunId); while (receipts.Count > 32) receipts.Dequeue();
        if (!run.VictorySignal || !run.RewardsCompleted || run.Outcome != "VictoryRewardsCompleted") return false;
        CompletedReceipt = run.RunId; Completions++; State = GuildQuestState.AwaitingReport; return true;
    }

    internal bool FieldReturned(HuntRunProgress run)
    {
        if (State != GuildQuestState.AwaitingReport || CompletedReceipt != run.RunId || !run.FieldChapterResumed || FieldReturnObserved) return false;
        FieldReturnObserved = true; return true;
    }

    internal bool Report()
    {
        if (State != GuildQuestState.AwaitingReport || !FieldReturnObserved) return false;
        LastReportedId = ContractId; ContractId = null; Reports++; State = GuildQuestState.Reported; return true;
    }
}
