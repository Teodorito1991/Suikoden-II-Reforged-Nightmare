using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Security.Cryptography;
using System.Text.Json;

namespace NightmareRebalance;

internal static class NativeWarRemix
{
    internal static bool Armed,Active,DefinitionsVerified;
    internal static int ContinueChecks,Victories,Defeats,ProbeAssertions;
    internal static bool ProbePassed;
    internal static NativeWarDecision LastDecision;
    static bool probing;
    internal static bool AuditDefinitions()
    {
        bool Hash(Il2CppStructArray<byte> bytes,string expected)=>bytes!=null&&Convert.ToHexString(SHA256.HashData(bytes.ToArray()))==expected;
        // Shipped tables are never changed. Refuse the remix if another mod changed these scripts.
        return Hash(w_dat_1.event_op_0,"84083BEC0E2C86BEB5FD496BE0021EEF121E847C70C339C147665FE0B7B99D8F")
            &&Hash(w_dat_1.event_end_0,"AE1DCEA1040D7A7B29AEE1E05326C79D1470896D6DD9321CF3B0F593B02F31FC")
            &&Hash(w_dat_1.event_end_1,"360C43818C938351680EA6270E69DA7BCFA5932800A6CC8F5784493EBF200999")
            &&Hash(w_dat_1.event_7,"8EB6CE1EB3419FDFA4384E3E83DCEA81CC48CAB6D25337AF39697F874B5244A1")
            &&w_dat_1.event_data_1 is {Length:>3}
            &&w_dat_1.event_data_1[1]?.Pointer==w_dat_1.event_end_0.Pointer
            &&w_dat_1.event_data_1[3]?.Pointer==w_dat_1.event_end_1.Pointer
            &&war_h.war2map is {Length:>1}&&war_h.war2map[1]==1;
    }
    internal static void Initialize()
    {
        DefinitionsVerified=AuditDefinitions();
        if(!DefinitionsVerified)Plugin.Logger.LogWarning("Native war remix disabled: war 1 script/map fingerprint mismatch.");
        else Plugin.Logger.LogInfo("Native war 1 scripts verified. Turn-three auto-ending can be replaced; original ending/campaign handlers remain installed.");
    }
    internal static void Arm(){Clear();Armed=true;}
    internal static void Begin()
    {
        Active=Armed&&Plugin.Options.EnableNativeWarRemix.Value&&war_h.g_w_war_no==1&&DefinitionsVerified&&AuditDefinitions()&&Plugin.CheckCompatibility();
        // Verified war-1 command is character slot 61 (Solon Jhee). Permanent death immunity
        // is native; annihilation immunity is a separate bit and would make this objective impossible.
        if(Active&&(w_unit.GetUnitChara(1,0,0)!=61||w_chara.GetCharaZenmetu(61)!=0))
        {Active=false;Plugin.Logger.LogWarning("Native war remix disabled for this battle: enemy command identity or routability changed.");}
        ContinueChecks=Victories=Defeats=0;LastDecision=NativeWarDecision.Vanilla;
        if(Active)Plugin.Logger.LogInfo("Native War Remix ACTIVE: war 1. Continue after turn 3 until enemy command routes or allied army falls. Original map/UI/AI/end scripts retained.");
    }
    internal static void Clear(){Armed=Active=false;LastDecision=NativeWarDecision.Vanilla;}
    internal static bool Check(ref int result)
    {
        if(!DefinitionsVerified||!(Active||probing)||(!probing&&!Plugin.Options.EnableNativeWarRemix.Value)||war_h.g_w_war_no!=1||war_sys.turn<=3)return true;
        var roster=w_unit.unit;
        if(roster==null||roster.Length!=2||roster[0]==null||roster[1]==null||roster[0].Length!=15||roster[1].Length!=15||roster[1][0]==null)return true;
        var allies=new int[15];
        for(int i=0;i<15;i++){if(roster[0][i]==null)return true;allies[i]=w_unit.GetUnitFlag(0,i);}
        var decision=NativeWarRules.Decide(true,true,war_h.g_w_war_no,war_sys.turn,w_unit.GetUnitFlag(1,0),allies);
        if(decision==NativeWarDecision.Vanilla)return true;
        LastDecision=decision;
        if(decision==NativeWarDecision.Continue)
        {
            result=0;
            if(!probing&&ContinueChecks++==0)Plugin.Logger.LogInfo("Native war 1: timed withdrawal suppressed; tactical phase continues through the original engine.");
            return false; // No native flags or callbacks touched on the continuation path.
        }
        // Exact native war-1 result contract: event 1 normal ending; event 3 allied defeat + game-over flag.
        // warmain consumes this selection itself. We never call/skip scene, casualty or progression handlers.
        war_sys.end_event_no=decision==NativeWarDecision.Victory?(byte)1:(byte)3;
        if(decision==NativeWarDecision.Defeat)war_sys.game_over_flag=-1;
        result=-1;
        if(!probing)
        {
            if(decision==NativeWarDecision.Victory)Victories++;else Defeats++;
            Plugin.Logger.LogInfo($"Native war 1 tactical {decision}: handing ending {war_sys.end_event_no} back to the native engine; campaign processing is unchanged.");
        }
        return false;
    }
    internal static object Snapshot()=>new{Enabled=Plugin.Options.EnableNativeWarRemix.Value,DefinitionsVerified,Armed,Active,ContinueChecks,Victories,Defeats,LastDecision=LastDecision.ToString(),ProbeAssertions,ProbePassed};

    internal static void SelfTest()
    {
        if(!Plugin.Options.RunNativeWarSelfTest.Value)return;
        if(Armed||Active||war_h.g_w_war_no!=0)throw new InvalidOperationException("Native war self-test is startup-only; no active war may exist.");
        var original=w_unit.unit;int war=war_h.g_w_war_no;long turn=war_sys.turn;byte ending=war_sys.end_event_no;sbyte gameOver=war_sys.game_over_flag;
        var last=LastDecision;
        var rows=new Il2CppReferenceArray<Il2CppReferenceArray<UNIT_TYPE>>(2);
        for(int side=0;side<2;side++){rows[side]=new Il2CppReferenceArray<UNIT_TYPE>(15);for(int i=0;i<15;i++)rows[side][i]=new UNIT_TYPE();}
        void Check(bool condition){ProbeAssertions++;if(!condition)throw new InvalidOperationException("Native war self-test assertion "+ProbeAssertions);}
        var cases=new List<object>();
        try
        {
            if(!DefinitionsVerified)throw new InvalidOperationException("Cannot probe unmatched native scripts.");
            w_unit.unit=rows;war_h.g_w_war_no=1;
            // Compare the real IL2CPP original with the Harmony detour on disposable native armies.
            foreach(long t in new long[]{1,3,4,8,50})
            {
                rows[0][0].flag=1;rows[1][0].flag=1;war_sys.turn=t;war_sys.end_event_no=73;war_sys.game_over_flag=0;
                probing=false;int vanilla=w_syouha.CheckSyouhai();Check(vanilla==(t>3?-1:0));Check(war_sys.end_event_no==(t>3?1:73));
                war_sys.end_event_no=73;probing=true;int remix=w_syouha.CheckSyouhai();Check(remix==0);Check(war_sys.end_event_no==73);Check(war_sys.game_over_flag==0);
                cases.Add(new{Turn=t,VanillaResult=vanilla,RemixResult=remix});
            }
            // Real native getter and patched result contract: enemy commander route -> normal ending.
            rows[1][0].flag=2;war_sys.turn=8;Check(w_unit.GetUnitFlag(1,0)==2);
            Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);Check(war_sys.game_over_flag==0);
            // Native all-allies-lost contract remains a real defeat, not the old timeout success.
            rows[0][0].flag=2;rows[1][0].flag=1;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==3);Check(war_sys.game_over_flag==-1);
            // An unrelated war must still execute the original. War 3 has its own late timeout, ending 1.
            rows[0][0].flag=1;war_h.g_w_war_no=3;war_sys.turn=20;war_sys.game_over_flag=0;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);
            Check(AuditDefinitions());
        }
        finally
        {
            probing=false;w_unit.unit=original;war_h.g_w_war_no=war;war_sys.turn=turn;war_sys.end_event_no=ending;war_sys.game_over_flag=gameOver;LastDecision=last;
        }
        Check(w_unit.unit?.Pointer==original?.Pointer);Check(war_h.g_w_war_no==war);Check(war_sys.turn==turn);Check(war_sys.end_event_no==ending);Check(war_sys.game_over_flag==gameOver);
        ProbePassed=true;
        var folder=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","native-war");Directory.CreateDirectory(folder);
        // Only summaries; no proprietary script bytes or campaign state exported.
        File.WriteAllText(Path.Combine(folder,"native-probe.json"),JsonSerializer.Serialize(new{Version="1.13.0-native-war-foundation.1",Cases=cases,Assertions=ProbeAssertions,GlobalsRestored=true,CampaignLoaded=false,BattlefieldRendered=false},new JsonSerializerOptions{WriteIndented=true}));
        Plugin.Logger.LogInfo($"Native war remix probe PASS: {ProbeAssertions} assertions; actual IL2CPP outcome checks on disposable armies, original globals restored. No map launched or campaign transition tested.");
    }
}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.Start))]
internal static class NativeRemixChapterStart
{[HarmonyPrefix]static void Prefix()=>NativeWarRemix.Arm();}
[HarmonyPatch(typeof(w_battle),nameof(w_battle.InitBattle))]
internal static class NativeRemixBattleBegin
{[HarmonyPostfix]static void Postfix()=>NativeWarRemix.Begin();}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.OnDestroy))]
internal static class NativeRemixChapterExit
{[HarmonyPostfix]static void Postfix()=>NativeWarRemix.Clear();}
[HarmonyPatch(typeof(w_syouha),nameof(w_syouha.CheckSyouhai))]
internal static class NativeRemixOutcome
{[HarmonyPrefix]static bool Prefix(ref int __result)=>NativeWarRemix.Check(ref __result);}
