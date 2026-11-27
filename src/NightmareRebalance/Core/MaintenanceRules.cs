namespace NightmareRebalance;
internal static class MaintenanceRules
{
    // Native BattleDataSetup: float multiplication, cap32767, truncation to ushort.
    internal static int NativeHp(int hp,float rate)=>hp is <0 or >65535||!float.IsFinite(rate)||rate<=0?-1:(int)Math.Min(32767f,(float)hp*rate);
    internal static bool Signature(int[] source,int[] local,int difficulty,float rate)=>
        source.Length==47&&local.Length==47&&difficulty is 0 or 2&&rate==(difficulty==0?1f:1.2f)&&
        local[0]==NativeHp(source[0],rate)&&source.Skip(1).SequenceEqual(local.Skip(1));
    internal static double Factor(double fallback,double? pacing,double configured,double baseline)=>configured!=baseline?configured:pacing??fallback;
}
internal sealed class HpLedger
{
    readonly Dictionary<long,int> last=new();
    internal void Reset()=>last.Clear();
    internal void Baseline(long key,int hp)=>last[key]=Math.Max(0,hp);
    internal int Observe(long key,int hp){hp=Math.Max(0,hp);int delta=last.TryGetValue(key,out int before)?hp-before:0;last[key]=hp;return delta;}
}
internal sealed class BoundedHistory<T>
{
    readonly int capacity;internal readonly List<T> Items=new();
    internal BoundedHistory(int capacity){if(capacity is <10 or >32)throw new ArgumentOutOfRangeException(nameof(capacity));this.capacity=capacity;}
    internal void Add(T item){Items.Add(item);if(Items.Count>capacity)Items.RemoveRange(0,Items.Count-capacity);}
}
internal static class DiagnosticHistoryStore
{
    internal static void Append(string path,string record)
    {
        if(record.Length>262144)throw new InvalidDataException("Diagnostic record exceeds256KiB");
        using var item=System.Text.Json.JsonDocument.Parse(record);var history=new BoundedHistory<System.Text.Json.JsonElement>(12);
        if(File.Exists(path)){
            if(new FileInfo(path).Length>8388608)throw new InvalidDataException("Existing history too large; retained unchanged");
            using var doc=System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
            if(doc.RootElement.GetProperty("SchemaVersion").GetInt32()!=1)throw new InvalidDataException("Unknown history schema; retained unchanged");
            foreach(var old in doc.RootElement.GetProperty("Battles").EnumerateArray().TakeLast(12))history.Add(old.Clone());
        }
        history.Add(item.RootElement.Clone());string body=System.Text.Json.JsonSerializer.Serialize(new{SchemaVersion=1,Capacity=12,Battles=history.Items});if(body.Length>8388608)throw new InvalidDataException("History exceeds8MiB");
        string temp=path+".tmp";File.WriteAllText(temp,body);File.Move(temp,path,true);
    }
}
