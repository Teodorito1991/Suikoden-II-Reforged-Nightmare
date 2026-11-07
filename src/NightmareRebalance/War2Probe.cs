using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Text.Json;
namespace NightmareRebalance;

internal static class War2Probe
{
    internal static int Assertions;
    internal static bool Passed;
    internal static void Run()
    {
        if(!Plugin.Options.RunNativeWarSelfTest.Value)return;
        if(war_h.g_w_war_no!=0||War2Runtime.Armed||War2Runtime.Ready)throw new InvalidOperationException("War-2 probe is startup-only.");
        var savedUnits=w_unit.unit;var savedCharacters=w_dat_2.chara_2;
        int savedWar=war_h.g_w_war_no;long savedTurn=war_sys.turn;byte savedEnd=war_sys.end_event_no;sbyte savedGameOver=war_sys.game_over_flag;
        var savedPolicy=War2Runtime.Policy;int savedGeneration=War2Runtime.Generation;var savedLast=War2Runtime.LastResult;
        var units=new Il2CppReferenceArray<Il2CppReferenceArray<UNIT_TYPE>>(2);
        for(int side=0;side<2;side++)
        {units[side]=new Il2CppReferenceArray<UNIT_TYPE>(15);for(int i=0;i<15;i++)units[side][i]=new UNIT_TYPE{chara=new Il2CppStructArray<sbyte>(new sbyte[]{0,-1,-1}),control=1};}
        var characters=new Il2CppReferenceArray<WAR_CHARA_TYPE>(108);
        for(int i=0;i<108;i++)characters[i]=new WAR_CHARA_TYPE{attack=8,defense=8,status_flag=1,nouryoku=new Il2CppStructArray<war_data_h.tagSPECIAL_ABILITY>(3),kaisu=new Il2CppStructArray<byte>(3),kaisu_max=new Il2CppStructArray<byte>(3)};
        int[] leaders={0,1,9,20};for(int i=0;i<4;i++)units[0][i].chara[0]=(sbyte)leaders[i];units[1][0].chara[0]=61;
        var policy=new WarPolicy{WarNumber=2,Enabled=true,PlayerCommands=true,CommanderVictory=true,MaximumTurn=20,EnemyAi=true,WoundedTargetBonus=6,PoorOddsPenalty=4};
        void Check(bool condition){Assertions++;if(!condition)throw new InvalidOperationException("War revolution native probe assertion "+Assertions);}
        void Reset()
        {
            foreach(var row in units)foreach(var u in row){u.flag=0;u.x=0;u.y=0;u.damage=0;}
            units[0][0].flag=1;units[0][0].x=4;units[0][0].y=7;
            units[1][0].flag=1;units[1][0].x=9;units[1][0].y=2;
            war_sys.turn=2;war_sys.end_event_no=73;war_sys.game_over_flag=0;War2Runtime.Outcome=War2Outcome.Vanilla;
        }
        try
        {
            Check(War2Runtime.Audit());w_unit.unit=units;w_dat_2.chara_2=characters;war_h.g_w_war_no=2;War2Runtime.Policy=policy;Reset();
            War2Runtime.Armed=true;War2Runtime.Probe=true;War2Runtime.Begin();Check(War2Runtime.Ready);
            for(int i=0;i<4;i++)Check(units[0][i].control==(Plugin.Options.EnableNativeWarRemix.Value?0:1));Check(units[1][0].control==1);
            War2Runtime.Ready=false;War2Runtime.Probe=false;
            Check(w_syouha.CheckSyouhai()==0);Check(war_sys.end_event_no==73);
            units[1][0].flag=2;Check(w_syouha.CheckSyouhai()==0); // Native war 2 has no enemy-command victory.
            Reset();units[1][0].x=3;units[1][0].y=6;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==4);Check(war_sys.game_over_flag==0);
            Reset();units[0][0].flag=2;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);Check(war_sys.game_over_flag==0);
            Reset();war_sys.turn=30;Check(w_syouha.CheckSyouhai()==0); // No vanilla turn-three cutoff in war 2.
            War2Runtime.Probe=true;
            Reset();Check(w_syouha.CheckSyouhai()==0);Check(war_sys.end_event_no==73);
            units[1][0].flag=2;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==4);Check(war_sys.game_over_flag==0);Check(War2Runtime.Outcome==War2Outcome.TacticalVictory);
            foreach(int x in new[]{3,4,5})foreach(int y in new[]{6,7,8})
            {Reset();units[1][0].x=(sbyte)x;units[1][0].y=(sbyte)y;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==4);Check(War2Runtime.Outcome==War2Outcome.ProtectedAreaBreached);}
            Reset();units[0][0].flag=2;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);Check(war_sys.game_over_flag==0);Check(War2Runtime.Outcome==War2Outcome.AlliedDefeat);
            Reset();war_sys.turn=policy.MaximumTurn;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==4);Check(War2Runtime.Outcome==War2Outcome.OrderlyWithdrawal);
            Reset();policy.Enabled=false;units[1][0].flag=2;Check(w_syouha.CheckSyouhai()==0);Check(war_sys.end_event_no==73);policy.Enabled=true;
            Reset();war_h.g_w_war_no=3;war_sys.turn=20;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);war_h.g_w_war_no=2;
            Reset();War2Runtime.Begin();Check(War2Runtime.Outcome==War2Outcome.Vanilla);Check(War2Runtime.ContinueChecks==0);Check(War2Runtime.OutcomeChecks==0);
            // Real native evaluator, with its empty END-terminated native scoring list, plus the production postfix.
            Reset();units[1][0].x=5;units[1][0].y=4;units[0][0].x=5;units[0][0].y=5;
            units[0][1].flag=1;units[0][1].x=5;units[0][1].y=3;units[0][1].damage=1;
            characters[1].attack=8;characters[1].defense=8;
            var weights=new Il2CppReferenceArray<TACTICS_HYOUKA_TYPE>(1);weights[0]=new TACTICS_HYOUKA_TYPE{var=20};
            var healthy=new TACTICS_RESULT_TYPE{act=0,target=0,tx=5,ty=4};var wounded=new TACTICS_RESULT_TYPE{act=0,target=1,tx=5,ty=4};
            WarAiRuntime.Current=null;int nativeHealthy=w_tach.Hyouka(1,0,weights,healthy),nativeWounded=w_tach.Hyouka(1,0,weights,wounded);
            Check(nativeHealthy==0);Check(nativeWounded==0);
            WarAiRuntime.Current=new(1,0);int improvedHealthy=w_tach.Hyouka(1,0,weights,healthy),improvedWounded=w_tach.Hyouka(1,0,weights,wounded);
            if(Plugin.Options.EnableNativeWarAI.Value)Check(improvedWounded>improvedHealthy);else Check(improvedWounded==nativeWounded);
            WarAiRuntime.Current=new(0,0);Check(w_tach.Hyouka(0,0,weights,healthy)==0);
            WarAiRuntime.Current=null;Check(w_tach.Hyouka(1,0,weights,wounded)==0);
            Check(War2Runtime.Audit());
        }
        finally
        {
            WarAiRuntime.Current=null;War2Runtime.Probe=false;War2Runtime.Clear();War2Runtime.Policy=savedPolicy;War2Runtime.Generation=savedGeneration;War2Runtime.LastResult=savedLast;
            War2Runtime.PlayerUnits=War2Runtime.ContinueChecks=War2Runtime.OutcomeChecks=War2Runtime.AiChanges=0;War2Runtime.AiTrace.Clear();
            w_unit.unit=savedUnits;w_dat_2.chara_2=savedCharacters;war_h.g_w_war_no=savedWar;war_sys.turn=savedTurn;war_sys.end_event_no=savedEnd;war_sys.game_over_flag=savedGameOver;
        }
        Check(w_unit.unit?.Pointer==savedUnits?.Pointer);Check(w_dat_2.chara_2?.Pointer==savedCharacters?.Pointer);Check(war_h.g_w_war_no==savedWar);Check(war_sys.turn==savedTurn);Check(war_sys.end_event_no==savedEnd);Check(war_sys.game_over_flag==savedGameOver);Check(!War2Runtime.Ready&&!War2Runtime.Armed);
        Passed=true;
        string directory=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","native-war");Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory,"revolution-probe.json"),JsonSerializer.Serialize(new{Version="1.12.2-guild-ui.1",Assertions,Passed,GlobalsRestored=true,BattlefieldPlayed=false,StoryCallbacksExecuted=false,AiNativeEvaluatorVerified=Plugin.Options.EnableNativeWarAI.Value},new JsonSerializerOptions{WriteIndented=true}));
        Plugin.Logger.LogInfo($"Native war revolution probe PASS: {Assertions} assertions; native war-2 outcomes/control/evaluator exercised on disposable records. No chapter, ending demo or campaign callback invoked.");
    }
}
