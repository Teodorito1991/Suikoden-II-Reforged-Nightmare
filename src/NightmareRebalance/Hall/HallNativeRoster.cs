using EventOverlayClass;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;

// Uses the native party-window roster builder on owned records, not its event/window owner.
internal static class HallNativeRoster
{
    internal sealed record Candidate(byte Id,string Name,int Level,int Hp);
    internal static Candidate[] Read(GAME_WORK game)
    {
        if(game?.party_data?.party_cha_no is not {Length:8}||game.chara_data?.c_varia_dat==null)
            throw new InvalidOperationException("Native party records are unavailable.");
        var p=new Overlay_partychg.PCCON{pst=new Il2CppReferenceArray<SP_PARTY>(6),ist=new Il2CppReferenceArray<SP_PARTY>(82)};
        // Native builder creates its own SP_PARTY entries and reads original recruitment/absence rules.
        string party=LitJson.JsonMapper.ToJson(game.party_data);
        byte[] flags=game.chara_flag.ToArray();
        Overlay_partychg.PCpartyAllTblSet(p);
        var currentIds=game.party_data.party_cha_no.Take(6).Where(c=>c!=0).ToHashSet();
        var ids=currentIds
            .Concat(p.ist?.Take(Math.Clamp(p.inin,0,82)).Where(x=>x!=null).Select(x=>x.cno)??Array.Empty<byte>()).Distinct();
        var result=ids.Where(c=>c>0&&c<game.chara_data.c_varia_dat.Length&&(currentIds.Contains(c)||G2_SYS.G2_cha_flag(4,c)==1&&G2_SYS.G2_cha_flag(9,c)!=1)&&G2_SYS.G2_chara_size(c)!=3&&game.chara_data.c_varia_dat[c].now_hp>0)
            .Select(c=>new Candidate(c,G2_SYS.G2_name_get(c),game.chara_data.c_varia_dat[c].level,game.chara_data.c_varia_dat[c].now_hp))
            .OrderByDescending(c=>c.Level).ThenBy(c=>c.Id).ToArray();
        if(party!=LitJson.JsonMapper.ToJson(game.party_data)||!flags.SequenceEqual(game.chara_flag))
            throw new InvalidOperationException("Roster enumeration changed native party or recruitment state.");
        Plugin.Logger.LogInfo($"Luca native roster: reserves={p.inin}, current={currentIds.Count}, healthy eligible={result.Length}, Riou={result.Any(c=>c.Id==1)}; party/recruitment unchanged.");
        return result;
    }
}
