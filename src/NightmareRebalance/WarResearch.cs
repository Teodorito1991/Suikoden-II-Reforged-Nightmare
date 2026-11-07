using BepInEx;
using System.Reflection;
using System.Text.Json;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace NightmareRebalance;

internal static class WarResearch
{
    internal static void Export()
    {
        if(!Plugin.Options.ExportNativeWarResearch.Value)return;
        // Read static definitions only. Never invoke per-war init, chapter entry or GAME_WORK.
        string directory=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","native-research");Directory.CreateDirectory(directory);
        for(int war=1;war<=16;war++)
        {
            var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+war)!;
            var data=new Dictionary<string,object?>();
            foreach(var property in type.GetProperties(BindingFlags.Static|BindingFlags.Public))
            {
                var value=property.GetValue(null);
                if(value is Il2CppStructArray<byte> bytes)data[property.Name]=bytes.ToArray();
                else if(value is Il2CppReferenceArray<Il2CppStructArray<byte>> arrays)data[property.Name]=arrays.Select(a=>a?.ToArray()).ToArray();
                else if(value is Il2CppReferenceArray<EVENT_CHECK_TABLE_TYPE> checks)data[property.Name]=checks.Select(c=>c==null?null:new{Event=c.event_no,Bytes=c.event_check_p?.ToArray()}).ToArray();
                else if(value is Il2CppReferenceArray<Il2CppReferenceArray<Il2CppStructArray<sbyte>>> units)data[property.Name]=units.Select(side=>side.Select(u=>u.ToArray()).ToArray()).ToArray();
            }
            data["WarNumber"]=war;data["MapNumber"]=war_h.war2map[war];
            File.WriteAllText(Path.Combine(directory,$"war{war}-definitions.json"),JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));
        }
        Plugin.Logger.LogInfo("Read-only native war definition export complete; no chapter, unit initialization or campaign callback invoked.");
        // Audited init methods only allocate/assign their own chara_N array; no
        // GameWork, chapter, unit-init, RNG or story calls. Restore the sole mutable
        // static reference in finally; never run this in a campaign chapter.
        if(war_h.g_w_war_no!=0||WarRuntime.Active||War2Runtime.Armed)throw new InvalidOperationException("Definition audit is startup-only.");
        foreach(int war in new[]{3,6,10,13,15})
        {
            var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+war)!;
            var property=type.GetProperty("chara_"+war)!;var saved=property.GetValue(null);
            try
            {
                type.GetMethod("init")!.Invoke(null,null);
                var chars=(Il2CppReferenceArray<WAR_CHARA_TYPE>)property.GetValue(null)!;
                var records=chars.Select((c,i)=>new{Index=i,c.name,c.force_type,c.attack,c.defense,c.status_flag,Abilities=c.nouryoku.ToArray(),Uses=c.kaisu.ToArray()}).ToArray();
                File.WriteAllText(Path.Combine(directory,$"war{war}-characters.json"),JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));
            }
            finally{property.SetValue(null,saved);}
        }
        Plugin.Logger.LogInfo("Candidate definition audit complete; all five chara array references restored. No battlefield or campaign invoked.");
    }
}
