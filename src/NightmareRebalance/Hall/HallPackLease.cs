using Il2CppInterop.Runtime;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NightmareRebalance;

internal sealed class HallPackLease
{
    readonly Stack<Il2CppSystem.Collections.IEnumerator> pending = new();
    readonly string key;
    readonly OPDResLoader.Type kind;
    ResourceLoader? loader;
    OPD_RES2? pack;
    bool started, released;

    internal HallPackLease(string key, OPDResLoader.Type kind = OPDResLoader.Type.Boss)
    {
        this.key = key;
        this.kind = kind;
    }

    internal bool Borrowed { get; private set; }
    internal bool Ready => pack != null;
    internal int Moves { get; private set; }
    internal int Records { get; private set; }
    internal bool Loaded => CacheEntry(key)?.Pointer == pack?.Pointer && pack != null;
    internal bool LoaderValid => loader?.handle.IsValid() == true;

    internal static void Require(bool yes, string description)
    {
        if (!yes) throw new InvalidOperationException(description);
    }

    internal static T InGsd2<T>(Func<T> call)
    {
        var mode = SystemObject.mode;
        try
        {
            SystemObject.Changemode(SystemObject.Mode.GSD2);
            return call();
        }
        finally { SystemObject.Changemode(mode); }
    }

    static OPD_RES2? CacheEntry(string id)
        => OPDResLoader.IsLoaded(id) ? OPDResLoader.opdData[id] : null;

    internal bool Step()
    {
        Require(!released, "Release is terminal.");
        if (Ready) return true;
        if (!started)
        {
            pack = CacheEntry(key);
            Borrowed = pack != null;
            if (Borrowed)
            {
                ValidateRecords();
                return true;
            }
            pending.Push(InGsd2(() => OPDResLoader.LoadOPDRes(kind, key)));
            started = true;
        }
        if (loader?.IsLoading == true) return false;
        Require(++Moves <= 96, "Unknown native OPD coroutine path.");
        var current = pending.Peek();
        if (InGsd2(current.MoveNext))
        {
            var yielded = current.Current;
            if (loader == null)
            {
                loader = yielded?.TryCast<ResourceLoader>();
                Require(loader != null, "Expected a native OPD acquisition.");
            }
            else
            {
                Require(yielded?.GetIl2CppType().FullName == "OPD_DATA+<LoadAtlas>d__14",
                    "Unexpected nested native OPD operation.");
                pending.Push(yielded!.Cast<Il2CppSystem.Collections.IEnumerator>());
            }
            return false;
        }
        pending.Pop();
        if (pending.Count != 0) return false;
        Require(LoaderValid && loader!.handle.Status == AsyncOperationStatus.Succeeded,
            "Native pack acquisition failed.");
        pack = loader!.Asset.TryCast<OPD_RES2>();
        ValidateRecords();
        return true;
    }

    void ValidateRecords()
    {
        var entries = pack?.opd_tbl;
        Require(Loaded && entries != null && entries.Count > 0, "Native cache binding did not match its pack.");
        for (int index = 0; index < entries!.Count; index++)
            Require(HasRecord(index, entries[index].Pointer), "Native OPD lookup identity mismatch.");
        Records = entries.Count;
    }

    internal bool HasRecord(int index, IntPtr pointer) =>
        OPDResLoader.GetData(key, index)?.Pointer == pointer && pointer != IntPtr.Zero;

    // The caller must first certify that its consumers are gone. This
    // resource-only harness never calls this method with live battle actors.
    internal void Release()
    {
        if (released) return;
        Require(Ready && Loaded, "Unknown cache identity; native removal refused.");
        released = true;
        if (Borrowed) return;
        Require(LoaderValid && loader!.Asset.Pointer == pack!.Pointer,
            "Native acquisition identity changed; removal refused.");
        InGsd2(() => { OPDResLoader.RemoveResPack(key); return true; });
        Require(!LoaderValid && CacheEntry(key) == null, "Owned pack was not released exactly once.");
    }
}
