namespace NightmareRebalance;

internal enum WarResourceOwner { Unknown, SessionAcquisition, SharedGame, RetainedField, SessionInstance }
internal enum WarResourceKind { Acquisition, Asset, CacheBinding, Callback, Instance }

// Ownership refers to an acquired reference or binding, never ownership of all
// objects reachable from an asset. Conflicting observations remain unknown.
internal sealed class WarResourceOwnership
{
    readonly Dictionary<(WarResourceKind, long), WarResourceOwner> entries = new();
    internal void Observe(WarResourceKind kind, long identity, WarResourceOwner owner)
    {
        if (identity == 0) throw new ArgumentException("Missing native identity.");
        var key = (kind, identity);
        if (entries.TryGetValue(key, out var old) && old != owner) owner = WarResourceOwner.Unknown;
        entries[key] = owner;
    }
    internal int Unknown => entries.Count(x => x.Value == WarResourceOwner.Unknown);
    internal bool CanClean => Unknown == 0;
    internal bool CanRelease(WarResourceKind kind, long identity) => CanClean &&
        entries.TryGetValue((kind, identity), out var owner) &&
        ((kind is WarResourceKind.Acquisition or WarResourceKind.CacheBinding && owner == WarResourceOwner.SessionAcquisition) ||
        (kind == WarResourceKind.Instance && owner == WarResourceOwner.SessionInstance));
    internal int Count(WarResourceOwner owner) => entries.Count(x => x.Value == owner);
    internal bool CanTransition => false; // No resource receipt grants a chapter transition.
}
