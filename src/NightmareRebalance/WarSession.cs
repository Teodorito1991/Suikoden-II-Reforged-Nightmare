namespace NightmareRebalance;

// Scope/cache lifecycle only. This is not a battlefield simulator or an arena launcher.
internal sealed class WarSession<T> where T:class
{
    readonly Dictionary<long,T> records=new();
    internal bool Armed {get;private set;}
    internal bool Active {get;private set;}
    internal int Generation {get;private set;}
    internal int Count=>records.Count;
    internal void Arm(){Clear();Armed=true;}
    internal void Begin(bool enabled,int warNumber)
    {records.Clear();Generation++;Active=Armed&&enabled&&warNumber is >=1 and <=16;}
    internal bool TryGet(long key,out T? record)
    {record=null;return Active&&records.TryGetValue(key,out record);}
    internal bool Store(long key,T record)
    {if(!Active||records.Count>=512||records.ContainsKey(key))return false;records.Add(key,record);return true;}
    internal void Clear(){records.Clear();Armed=false;Active=false;}
}
