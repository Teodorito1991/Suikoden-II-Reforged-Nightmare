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
    static readonly (string Map,int Variant,int Event,string Digest,int Boss,int Flag,byte Mask,int BattleOffset,int FlagOffset)[] sources =
    {
        ("vh04",8,8,"9FF31D1EC2F8E39DF82DA72D71E6613E4D8C65DCC6B6DCB092E4C853440B14CE",18,0xB6,0x02,-1,-1),
        ("vh04",8,9,"6E3D6574FEA15FBD3F7F4C8FAF95D1043D90260421DC9FDD4BF435857B0A3C53",18,0xB6,0x02,17,20),
        ("vi15",0,5,"748E22E235581FD94FAB7EF2D23DBE293DBF2C251C78AB101B9F30DAF86C354B",14,0x21,0x10,-1,-1),
        ("vi15",0,4,"AC192628F023F4B1D2DB596416275E309BCCEBDD3A3FBDE9075B1825D43BF032",14,0x21,0x10,122,125),
        ("vc27",0,4,"D7CD818FC5CD0A5396680286E13F9BC1A3DDF2534A4ACD6D9053A87919D913A4",23,0x16,0x02,-1,-1),
        ("vc27",0,2,"A6CD6BB65BA2E9A2912E912B2E1477729A5F075E4EB6079A8505AB75C462868A",23,0x16,0x02,322,325),
        ("vg15",4,7,"C696A02BF3F97939C8BDE869C6EA081D9D9812B40C72F4163C7779F5DF46C5FB",17,0xC9,0x01,-1,-1),
        ("vg15",4,8,"3D6504860378DC15625CE4524E5A3C0582C6EEDA1067393842E8D9356C814F14",17,0xC9,0x01,203,209),
        ("vd14",0,0,"79158ACE5388DDCE90914B0E1FFFB99B3BF9503867BB598800FEF806F498DA8D",11,0x56,0x20,-1,-1),
        ("vd14",0,3,"A933692E1845C9FC9D1EC842D888C3E39689A6189E9B786870D8EB2D271B9556",11,0x56,0x20,643,648),
        ("vc14",0,4,"CB0442C1423E5C838728C2CCE40B5CCDEBCE38622C99B58B4B968FBD8FB38CC0",1,0x07,0x40,226,232),
        ("ve13",1,16,"8727299C229B66AB932E0B7916D105EB4C87FBB23EC5EC9BCDCD6EEB978776AF",4,0xBA,0x08,90,99),
        ("ve13",1,17,"9291FDCDF4D22365110CEA19FE1C1FEC61D9F0A859330DE03A52B48F2F612CE5",4,0xBA,0x10,90,99),
        ("ve08",3,10,"F454DA033977B3ADA653FC38FBA5385A8C1D75C25BD96215CCBCCE869CA919F2",3,0xBB,0x10,96,332),
        ("vi10",0,1,"B44AB0765542E6410EBB6C816C51040A1E07DF6B975D04CC50CA0F1583C3A844",22,0x14,0x02,1095,1105)
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
                : code.Skip(s.BattleOffset).Take(3).SequenceEqual(new byte[]{83,1,240}) &&
                  s.FlagOffset>=s.BattleOffset+3 &&
                  code.Skip(s.FlagOffset).Take(3).SequenceEqual(new byte[]{9,(byte)s.Flag,s.Mask});
            string expectedTable=s.Boss switch {18=>"boss_rat",14=>"boss_hap1",23=>"boss_gwo1",17=>"boss_gol",11=>"boss_nik",1=>"boss_ryo",4=>"boss_dor",3=>"boss_lus1",22=>"boss_gor",_=>""};
            if(BattleLoader.btl_bos_data[s.Boss]!=expectedTable)
                throw new InvalidOperationException("Hall story source no longer maps to the expected original boss.");
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
