using System.Security.Cryptography;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NightmareRebalance;

// Startup-only provenance audit. Loads original records, never executes commands.
internal static class HallProgressProbe
{
    static readonly (string Map,int Variant,int Event,string Digest)[] sources =
    {
        ("vc06",1,32,"1A5D3367E702FF09EFD57F6AD7635F22852D0DFDFF3E6714C0E54758EA1C6B48"),
        ("vj20",4,5,"38E1DE0D19557CCC983C24E206B0DF2BF23847FDD2F5862B5D08044437C4029D"),
        ("vj20",4,3,"241B1B1AA9D7EDF4FC1DE729C0FE841BE19F5D9F260F4B44802D39614A38DC57"),
        ("vj23",4,8,"910701AFE74ADE5FE4405F4CE0FED5E3F39CDF0777E093139875FEB01B59E427"),
        ("vj23",4,6,"DC37ED84AD1A8D6CCC7DFAE9073328776A940F0400903DB95B88947C302236FA")
    };
    static int index;
    static AsyncOperationHandle<MACHIDAT>? pending;
    internal static bool Step()
    {
        if(index==sources.Length)return true;
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
            ProductionAssetAudit.Rows.Add(new {Kind="HallProgressSource",s.Map,s.Variant,s.Event,Digest=digest,
                OriginalRecordLoaded=true,CommandsExecuted=false,CampaignLoaded=false});
            index++;
        }
        finally {Addressables.Release(pending);pending=null;}
        return false;
    }
}
