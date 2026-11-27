using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;

// Prepare native graphics before lending any exploration-party association to the battle.
internal sealed class HallPartyLease
{
    readonly GAME_WORK game;readonly PARTY_DATA party;readonly long epoch;
    readonly Il2CppStructArray<byte> original,originalStatus,owned,ownedStatus;
    readonly Il2CppStructArray<ushort> originalCounts,originalTimes,ownedCounts,ownedTimes;
    readonly Il2CppStructArray<uint> originalGraphics,ownedGraphics;
    readonly Il2CppReferenceArray<P_GRAPH_DATA> originalModels,ownedModels;
    readonly Dictionary<int,P_GRAPH_DATA> acquired=new();
    readonly List<Il2CppSystem.Action<P_GRAPH_DATA>> callbacks=new();
    readonly int[] members;int loading;bool started,restored;
    internal bool Bound{get;private set;}
    internal readonly byte[] OriginalMembers;
    internal static P_GRAPH_DATA? Cached(int id)
    {
        var cache=PlayerDataLoader.loadPlayerDataList;
        return cache!=null&&cache.TryGetValue(id,out var data)?data:null;
    }
    internal HallPartyLease(GAME_WORK g,int[] members)
    {
        if(members.Length!=6||members.Distinct().Count()!=6)throw new InvalidOperationException("Six distinct native party members required.");
        game=g;this.members=members;party=g.party_data;epoch=NativeWarBoundary.Epoch;
        if(!PlayerDataLoader.IsLoadEnd)throw new InvalidOperationException("A native character load is already active.");
        original=party.party_cha_no;OriginalMembers=original.ToArray();originalStatus=party.status;
        originalCounts=party.st_cnt;originalTimes=party.hoka_time;originalGraphics=party.cha_gra_adrs;
        originalModels=party.cha_gra_adrs_graphdata;
        if(original.Length!=8||originalStatus.Length!=6||originalModels.Length<6)
            throw new InvalidOperationException("Native party slot shape changed.");
        owned=new Il2CppStructArray<byte>(members.Select(c=>(byte)c).Concat(new byte[2]).ToArray());
        ownedStatus=new Il2CppStructArray<byte>(6);ownedCounts=new Il2CppStructArray<ushort>(originalCounts.Length);
        ownedTimes=new Il2CppStructArray<ushort>(originalTimes.Length);ownedGraphics=new Il2CppStructArray<uint>(originalGraphics.ToArray());
        ownedModels=new Il2CppReferenceArray<P_GRAPH_DATA>(originalModels.ToArray());
    }
    bool SameOwner=>NativeWarBoundary.Epoch==epoch&&Framework.Singleton<GAME_WORK>.instance?.Pointer==game.Pointer&&game.party_data?.Pointer==party.Pointer;
    internal bool Ready
    {
        get
        {
            if(!SameOwner||restored)throw new InvalidOperationException("Character preparation owner changed.");
            if(!started)
            {
                started=true;
                for(int i=0;i<6;i++)
                {
                    int slot=i,id=members[i];var borrowed=Cached(id);
                    if(borrowed!=null){ownedModels[slot]=borrowed;continue;}
                    loading++;
                    Il2CppSystem.Action<P_GRAPH_DATA> callback=(Action<P_GRAPH_DATA>)(data=>
                    {
                        if(!SameOwner||restored||Bound||data==null)
                            throw new InvalidOperationException("Character loader returned to an invalid preparation owner.");
                        acquired.Add(id,data);ownedModels[slot]=data;loading--;
                    });
                    callbacks.Add(callback);
                    HallFirstContactDriver.Instance!.StartCoroutine(PlayerDataLoader.LoadPlayerData(id,false,callback));
                }
            }
            return loading==0&&PlayerDataLoader.IsLoadEnd&&Enumerable.Range(0,6).All(i=>
                ownedModels[i]!=null&&Cached(members[i])?.Pointer==ownedModels[i].Pointer);
        }
    }
    internal void Bind()
    {
        if(Bound)return;
        if(!Ready||party.party_cha_no?.Pointer!=original.Pointer||party.status?.Pointer!=originalStatus.Pointer||
           party.st_cnt?.Pointer!=originalCounts.Pointer||party.hoka_time?.Pointer!=originalTimes.Pointer||
           party.cha_gra_adrs?.Pointer!=originalGraphics.Pointer||party.cha_gra_adrs_graphdata?.Pointer!=originalModels.Pointer)
            throw new InvalidOperationException("Exploration party changed during character preparation.");
        party.party_cha_no=owned;party.status=ownedStatus;party.st_cnt=ownedCounts;party.hoka_time=ownedTimes;
        party.cha_gra_adrs=ownedGraphics;party.cha_gra_adrs_graphdata=ownedModels;Bound=true;
    }
    internal void Restore()
    {
        if(restored)return;
        if(!SameOwner||loading!=0)throw new InvalidOperationException("Native party lease owner changed or load is pending.");
        if(Bound)
        {
            if(party.party_cha_no?.Pointer!=owned.Pointer||party.status?.Pointer!=ownedStatus.Pointer||party.st_cnt?.Pointer!=ownedCounts.Pointer||
               party.hoka_time?.Pointer!=ownedTimes.Pointer||party.cha_gra_adrs?.Pointer!=ownedGraphics.Pointer||party.cha_gra_adrs_graphdata?.Pointer!=ownedModels.Pointer)
                throw new InvalidOperationException("Native party lease associations changed; restoration refused.");
            party.party_cha_no=original;party.status=originalStatus;party.st_cnt=originalCounts;party.hoka_time=originalTimes;
            party.cha_gra_adrs=originalGraphics;party.cha_gra_adrs_graphdata=originalModels;
        }
        restored=true;
    }
    // Only after native actors and the battle scene have gone, or an unbound preparation was cancelled.
    internal void ReleaseGraphics()
    {
        if(!restored||loading!=0||!PlayerDataLoader.IsLoadEnd)throw new InvalidOperationException("Stage graphics still have native consumers.");
        foreach(var pair in acquired)
        {
            var current=Cached(pair.Key);
            if(current==null)continue;
            if(current.Pointer!=pair.Value.Pointer)throw new InvalidOperationException("Native character cache was replaced by another owner.");
            if(originalModels.Any(p=>p?.Pointer==current.Pointer))throw new InvalidOperationException("Refusing release of retained field graphics.");
            PlayerDataLoader.Remove(pair.Key,false);
        }
        acquired.Clear();callbacks.Clear();
    }
}
