using System.Security.Cryptography;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NightmareRebalance;

// Startup-only provenance audit. Loads original records, never executes commands.
internal static class HallProgressProbe
{
    // Preloads must explicitly identify the boss-resource category (15).
    // Completion writes must immediately follow an ordinary type-1 battle.
    static readonly (string Map,int Variant,int Event,string Digest,int Boss,int Flag,byte Mask,int BattleOffset)[] sources =
    {
        ("vh04",8,8,"9FF31D1EC2F8E39DF82DA72D71E6613E4D8C65DCC6B6DCB092E4C853440B14CE",18,0xB6,0x02,-1),
        ("vh04",8,9,"6E3D6574FEA15FBD3F7F4C8FAF95D1043D90260421DC9FDD4BF435857B0A3C53",18,0xB6,0x02,17),
        ("vi15",0,5,"748E22E235581FD94FAB7EF2D23DBE293DBF2C251C78AB101B9F30DAF86C354B",14,0x21,0x10,-1),
        ("vi15",0,4,"AC192628F023F4B1D2DB596416275E309BCCEBDD3A3FBDE9075B1825D43BF032",14,0x21,0x10,122),
        ("vc27",0,4,"D7CD818FC5CD0A5396680286E13F9BC1A3DDF2534A4ACD6D9053A87919D913A4",23,0x16,0x02,-1),
        ("vc27",0,2,"A6CD6BB65BA2E9A2912E912B2E1477729A5F075E4EB6079A8505AB75C462868A",23,0x16,0x02,322)
    };
    static int index;
    static AsyncOperationHandle<MACHIDAT>? pending;
    internal static bool Step()
    {
        if(index==sources.Length){HallUnlockProbe.Run();return true;}
        var s=sources[index];
        if(pending==null)
        {
            pending=Addressables.LoadAssetAsync<MACHIDAT>((Il2CppSystem.String)("gsd2-classic-machi-"+s.Map+"-"+s.Map));
            return false;
        }
        if(!pending.IsDone)return false;
        try
        {
            if(pending.Status!=AsyncOperationStatus.Succeeded||pending.Result?.eventdata?.mapeventdat==null)
                throw new InvalidOperationException("Hall completion witness source unavailable.");
            var e=pending.Result.eventdata.mapeventdat[s.Variant].eventdat[s.Event];
            string data=string.Join(",",e.eventForm.ToArray())+"|"+string.Join(",",e.eventCom.ToArray());
            string digest=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
            if(digest!=s.Digest)throw new InvalidOperationException("Hall completion witness source changed.");
            var code=e.eventCom.ToArray();
            bool boundary=s.BattleOffset<0
                ? code.Length>=4&&code[0]==31&&code[2]==15&&code[3]==s.Boss
                : code.Skip(s.BattleOffset).Take(6).SequenceEqual(new byte[]{83,1,240,9,(byte)s.Flag,s.Mask});
            if(!boundary)throw new InvalidOperationException("Hall boss category or post-battle completion boundary changed.");
            ProductionAssetAudit.Rows.Add(new {Kind="HallProgressSource",s.Map,s.Variant,s.Event,Digest=digest,
                s.Boss,s.Flag,s.Mask,Preload=s.BattleOffset<0,OriginalRecordLoaded=true,
                CommandsExecuted=false,CampaignLoaded=false});
            index++;
        }
        finally {Addressables.Release(pending);pending=null;}
        return false;
    }
}
