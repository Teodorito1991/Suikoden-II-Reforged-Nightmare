using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;

// A stage borrows native item definitions, never the exploration party's mutable cache cells.
internal sealed class HallPartyEquipment
{
    readonly GAME_WORK game;
    readonly Il2CppReferenceArray<HAVE_DATA> original,owned;
    bool restored;
    internal HallPartyEquipment(GAME_WORK g,OVIT_DATA source,IEnumerable<int> members)
    {
        game=g;original=g.have_data;
        if(original?.Length!=176)throw new InvalidOperationException("Native equipment cache shape changed.");
        owned=new Il2CppReferenceArray<HAVE_DATA>(176);
        for(int i=0;i<176;i++)
        {
            var item=original[i];
            owned[i]=new HAVE_DATA{kind=item.kind,no=item.no,union_data=item.union_data};
        }
        game.have_data=owned;
        var previousSource=g.ovit_data;
        // Native G2_data_get reads the GAME_WORK association even with an explicit argument.
        g.ovit_data=previousSource??source;
        try
        {
            foreach(int id in members)
            {
                var v=g.chara_data.c_varia_dat[id];
                foreach(byte n in v.bogu_eqp)Ensure(source,1,n);
                foreach(byte n in v.mon_eqp)Ensure(source,2,n);
                Ensure(source,2,v.buki_mon);
                foreach(var item in v.item_eqp)if(item!=null)Ensure(source,(item.use_cnt&0x70)>>4,item.item_no);
            }
        }
        catch{Restore();throw;}
        finally{g.ovit_data=previousSource;}
    }
    void Ensure(OVIT_DATA source,int kind,int no)
    {
        if(no==0||G2_SYS.G2_item_search(kind,no,0)>=0)return;
        if(!owned.Any(x=>x.no==0))throw new InvalidOperationException("Native stage equipment cache is full.");
        var data=G2_SYS.G2_data_get(source,kind,no)??throw new InvalidOperationException($"Missing original equipment {kind}/{no}.");
        G2_SYS.G2_have_data_put(kind,no,data);
        if(G2_SYS.G2_item_search(kind,no,0)<0)throw new InvalidOperationException("Original equipment registration failed.");
    }
    internal void Restore()
    {
        if(restored)return;
        if(game.have_data?.Pointer!=owned.Pointer)throw new InvalidOperationException("Stage equipment owner changed.");
        // Native loot can add a held item while this stage cache is active. Retain its
        // original definition in the exploration cache; never alter the inventory itself.
        var held=game.party_data.party_item.Where(x=>x!=null&&x.item_no!=0)
            .Select(x=>(Kind:(x.use_cnt&0x70)>>4,No:(int)x.item_no)).Distinct().ToArray();
        var additions=held.Where(x=>!original.Any(c=>c.kind==x.Kind&&c.no==x.No))
            .Select(x=>(x.Kind,x.No,Data:owned.FirstOrDefault(c=>c.kind==x.Kind&&c.no==x.No)?.union_data)).ToArray();
        if(additions.Any(x=>x.Data==null)||original.Count(x=>x.no==0)<additions.Length)
            throw new InvalidOperationException("Cannot retain native loot definition in restored equipment cache.");
        game.have_data=original;
        foreach(var x in additions)G2_SYS.G2_have_data_put(x.Kind,x.No,x.Data!);
        restored=true;
    }
}
