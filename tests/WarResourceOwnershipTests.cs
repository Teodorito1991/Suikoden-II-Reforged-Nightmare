using NightmareRebalance;

internal static class WarResourceOwnershipTests
{
    internal static void Run()
    {
        int checks = 0;
        void Check(bool ok, string label) { checks++; if (!ok) throw new Exception("War resource ownership: " + label); }
        foreach (var kind in Enum.GetValues<WarResourceKind>())
        foreach (var owner in Enum.GetValues<WarResourceOwner>())
        {
            var ledger = new WarResourceOwnership();
            ledger.Observe(kind, 1, owner);
            Check(ledger.CanClean == (owner != WarResourceOwner.Unknown), "Unknown blocks all domain cleanup.");
            Check(ledger.CanRelease(kind, 1) == (owner == WarResourceOwner.SessionAcquisition && kind is WarResourceKind.Acquisition or WarResourceKind.CacheBinding), "Only owned references and bindings can be released.");
            Check(!ledger.CanTransition, "Resource readiness cannot authorize chapter dispatch.");
        }
        var mixed = new WarResourceOwnership();
        mixed.Observe(WarResourceKind.Acquisition, 1, WarResourceOwner.SessionAcquisition);
        mixed.Observe(WarResourceKind.Asset, 2, WarResourceOwner.SharedGame);
        mixed.Observe(WarResourceKind.Asset, 3, WarResourceOwner.RetainedField);
        Check(mixed.CanRelease(WarResourceKind.Acquisition, 1), "Shared assets do not prevent releasing our own acquired reference.");
        Check(!mixed.CanRelease(WarResourceKind.Asset, 2) && !mixed.CanRelease(WarResourceKind.Asset, 3), "Borrowed assets cannot be destroyed.");
        mixed.Observe(WarResourceKind.Callback, 4, WarResourceOwner.Unknown);
        Check(!mixed.CanClean && !mixed.CanRelease(WarResourceKind.Acquisition, 1), "Unclassified callback blocks before known unloads.");
        mixed.Observe(WarResourceKind.Callback, 4, WarResourceOwner.SessionAcquisition);
        Check(mixed.Unknown == 1, "Unknown cannot silently become owned.");
        var conflict = new WarResourceOwnership();
        conflict.Observe(WarResourceKind.Acquisition, 5, WarResourceOwner.SessionAcquisition);
        conflict.Observe(WarResourceKind.Acquisition, 5, WarResourceOwner.RetainedField);
        Check(!conflict.CanClean, "Conflicting ownership blocks cleanup.");
        bool rejected = false; try { conflict.Observe(WarResourceKind.Asset, 0, WarResourceOwner.SharedGame); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Missing identity cannot support ownership evidence.");
        Console.WriteLine($"PASS: War resource ownership {checks} assertions; shared/retained references, conflicts, unknown callbacks and no transition authorization.");
    }
}
