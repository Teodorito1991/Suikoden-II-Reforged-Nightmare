using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Text.Json;
namespace NightmareRebalance;

internal static class TotalWarProbe
{
    internal static int Assertions,DecisionDifferences;
    internal static bool Passed;
    internal static void Run()
    {
        if(!Plugin.Options.RunNativeWarSelfTest.Value)return;
        if(war_h.g_w_war_no!=0||TotalWarRuntime.Armed||War2Runtime.Armed)throw new InvalidOperationException("Total War probe is startup-only.");
        var savedUnits=w_unit.unit;int savedWar=war_h.g_w_war_no;long savedTurn=war_sys.turn;
        byte savedEnd=war_sys.end_event_no;sbyte savedOver=war_sys.game_over_flag;
        var savedSelected=w_tact.selected_result;var savedResult=w_tact.result;
        var random=UnityEngine.Random.state;
        var properties=TotalWarRules.Audits.Select(a=>typeof(w_dat_2).Assembly.GetType("w_dat_"+a.War)!.GetProperty("chara_"+a.War)!).ToArray();
        var savedChars=properties.Select(p=>p.GetValue(null)).ToArray();
        var policies=TotalWarRuntime.Rules.Scenarios.Select(p=>(p,p.Enabled,p.EnemyAi)).ToArray();
        var units=new Il2CppReferenceArray<Il2CppReferenceArray<UNIT_TYPE>>(2);
        for(int side=0;side<2;side++){units[side]=new Il2CppReferenceArray<UNIT_TYPE>(15);for(int i=0;i<15;i++)units[side][i]=new UNIT_TYPE{chara=new Il2CppStructArray<sbyte>(new sbyte[]{-1,-1,-1})};}
        void Check(bool value){Assertions++;if(!value)throw new InvalidOperationException("Total War native assertion "+Assertions);}
        void Reset(CampaignAudit a)
        {
            foreach(var row in units)foreach(var u in row){u.flag=0;u.x=u.y=0;u.damage=0;u.chara[0]=-1;}
            units[0][0].flag=1;units[0][0].x=3;units[0][0].y=8;units[0][0].chara[0]=0;
            units[1][0].flag=1;units[1][0].x=10;units[1][0].y=2;units[1][0].chara[0]=(sbyte)a.Commander;
            war_h.g_w_war_no=a.War;war_sys.turn=2;war_sys.end_event_no=73;war_sys.game_over_flag=0;
            TotalWarRuntime.Ready=false;TotalWarRuntime.Ending=null;TotalWarRuntime.Result="Vanilla";
        }
        try
        {
            TotalWarRuntime.Probe=true;w_unit.unit=units;
            foreach(var p in TotalWarRuntime.Specializations.Profiles)
                foreach(string ability in p.Abilities)
                {
                    int id=Array.IndexOf(OverhaulRules.Abilities,ability);Check(OverhaulRules.SafeAbility(id));
                    if(OverhaulRules.ActiveAbility(id))
                    {Check(w_chara.GetTokusyuInitFunc(id)!=null);Check(w_chara.GetTokusyuActFunc(id)!=null);Check(w_chara.GetTokusyuCheckFunc(id)!=null);Check(w_chara.GetTokusyuFinishFunc(id)!=null);Check(w_chara.GetTokusyuSelectFunc(id)!=null);}
                }
            foreach(var a in TotalWarRules.Audits)
            {
                var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+a.War)!;
                // Verified allocation-only initializer; sole mutable static reference
                // is chara_N, restored in finally. No chapter/map/story initializer.
                type.GetMethod("init")!.Invoke(null,null);Check(TotalWarRuntime.Audit(a.War));
                var chars=TotalWarRuntime.Characters(a.War)!;Check(chars[a.Commander].name==a.CommanderName);Check(((chars[a.Commander].status_flag&2)==0)==a.Routable);
                Reset(a);war_sys.turn=a.TimerTurn;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==a.TimerEnding);
                Reset(a);TotalWarRuntime.Armed=true;TotalWarRuntime.Begin();Check(TotalWarRuntime.Ready);Check(TotalWarRuntime.ActiveWar==a.War);
                war_sys.turn=a.TimerTurn;int actual=w_syouha.CheckSyouhai();
                Check(actual==(Plugin.Options.EnableNativeWarRemix.Value?0:-1));Check(war_sys.end_event_no==(Plugin.Options.EnableNativeWarRemix.Value?73:a.TimerEnding));Check(war_sys.game_over_flag==0);
                var policy=TotalWarRuntime.Policy!;
                war_sys.turn=policy.MaximumTurn;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==a.TimerEnding);Check(war_sys.game_over_flag==0);
                Reset(a);TotalWarRuntime.Armed=true;TotalWarRuntime.Begin();units[0][0].flag=2;
                Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==3);Check(war_sys.game_over_flag==-1);
                Reset(a);TotalWarRuntime.Begin();units[1][0].flag=2;
                Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);Check(war_sys.game_over_flag==0);
                Reset(a);TotalWarRuntime.Begin();policy.Enabled=false;war_sys.turn=a.TimerTurn;
                Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==a.TimerEnding);policy.Enabled=true;
                Reset(a);TotalWarRuntime.Begin();war_sys.turn=2;Check(w_syouha.CheckSyouhai()==0);
                TotalWarRuntime.Ending=4;TotalWarRuntime.AiChanges=4;TotalWarRuntime.Begin();Check(TotalWarRuntime.Ending==null);Check(TotalWarRuntime.AiChanges==0);
                if(a.War==6)
                {
                    for(int i=1;i<=11;i++)units[1][i].flag=2;
                    war_sys.turn=a.TimerTurn;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);
                    if(Plugin.Options.EnableNativeWarRemix.Value)Check(TotalWarRuntime.Result=="EligibleElimination");
                    Check(units[1][0].flag==1);Check(chars[a.Commander].status_flag==3);
                }
                // New disposable specialization uses audited enemy pointer only.
                var spec=TotalWarRuntime.Specialize(chars[a.Commander]);
                if(Plugin.Options.EnableWarSpecializations.Value)Check(spec!=null);else Check(spec==null);
                Check(TotalWarRuntime.Specialize(chars[0])==null);
                if(spec!=null){byte atk=chars[a.Commander].attack;var copy=WarRuntime.Copy(chars[a.Commander],spec);Check(copy.Pointer!=chars[a.Commander].Pointer);Check(chars[a.Commander].attack==atk);Check(copy.status_flag==chars[a.Commander].status_flag);}
            }
            // Unmodified timer battle: production scope must not spill into war 10.
            var last=TotalWarRules.Audits[2];Reset(last);TotalWarRuntime.Armed=true;TotalWarRuntime.Begin();war_h.g_w_war_no=10;war_sys.turn=20;
            Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);
            // Actual native Hyouka and SelectResult: a tactical commander moves toward
            // the allied command instead of the initially better original candidate.
            var a3=TotalWarRules.Audits[0];Reset(a3);TotalWarRuntime.Armed=true;TotalWarRuntime.Begin();
            units[1][0].x=8;units[1][0].y=7;units[0][0].x=10;units[0][0].y=7;
            var weights=new Il2CppReferenceArray<TACTICS_HYOUKA_TYPE>(1);weights[0]=new TACTICS_HYOUKA_TYPE{var=20};
            TACTICS_RESULT_TYPE Candidate(int x,int y)=>new(){act=1,tx=(byte)x,ty=(byte)y,target=255};
            var away=Candidate(7,7);var toward=Candidate(9,7);var sentinel=Candidate(8,7);away.next=toward;toward.next=sentinel;
            WarAiRuntime.Current=null;away.hyouka=1;toward.hyouka=0;
            Check(w_tach.SelectResult(1,0,away).Pointer==away.Pointer);
            WarAiRuntime.Current=new(1,0);
            int aScore=w_tach.Hyouka(1,0,weights,away),bScore=w_tach.Hyouka(1,0,weights,toward);
            away.hyouka=(short)(1+aScore);toward.hyouka=(short)bScore;
            if(Plugin.Options.EnableNativeWarAI.Value)
            {Check(bScore>aScore+1);Check(w_tach.SelectResult(1,0,away).Pointer==toward.Pointer);DecisionDifferences++;}
            else{Check(aScore==0&&bScore==0);}
            Check(away.tx==7&&away.ty==7&&away.act==1);Check(toward.tx==9&&toward.ty==7&&toward.act==1);
            // Defensive Kiba prefers his original headquarters over chasing forward.
            var a6=TotalWarRules.Audits[1];Reset(a6);TotalWarRuntime.Armed=true;TotalWarRuntime.Begin();units[1][0].x=11;units[1][0].y=3;
            WarAiRuntime.Current=new(1,0);var home=Candidate(12,3);var chase=Candidate(10,3);
            int hs=w_tach.Hyouka(1,0,weights,home),cs=w_tach.Hyouka(1,0,weights,chase);
            if(Plugin.Options.EnableNativeWarAI.Value)Check(hs>cs);else Check(hs==cs);
            chase.next=home;home.next=Candidate(11,3);chase.hyouka=(short)(1+cs);home.hyouka=(short)hs;
            if(Plugin.Options.EnableNativeWarAI.Value){Check(w_tach.SelectResult(1,0,chase).Pointer==home.Pointer);DecisionDifferences++;}
            // Screening infantry and ranged risk avoidance use actual native score
            // hooks/selection too; fixtures do not claim native map reachability.
            Reset(a3);TotalWarRuntime.Armed=true;TotalWarRuntime.Begin();
            units[0][0].x=12;units[0][0].y=4;
            units[1][1].flag=1;units[1][1].chara[0]=66;units[1][1].x=8;units[1][1].y=4;
            units[1][2].flag=1;units[1][2].chara[0]=69;units[1][2].x=10;units[1][2].y=4;
            var screenAway=Candidate(7,4);var screenNear=Candidate(9,4);screenAway.next=screenNear;screenNear.next=Candidate(8,4);
            WarAiRuntime.Current=new(1,1);int sa=w_tach.Hyouka(1,1,weights,screenAway),sn=w_tach.Hyouka(1,1,weights,screenNear);
            screenAway.hyouka=(short)(1+sa);screenNear.hyouka=(short)sn;
            if(Plugin.Options.EnableNativeWarAI.Value){Check(sn>sa+1);Check(w_tach.SelectResult(1,1,screenAway).Pointer==screenNear.Pointer);DecisionDifferences++;}
            units[1][2].x=8;units[0][0].x=10;
            var risky=Candidate(9,4);var safe=Candidate(7,4);risky.next=safe;safe.next=Candidate(8,4);
            WarAiRuntime.Current=new(1,2);int rs=w_tach.Hyouka(1,2,weights,risky),ss=w_tach.Hyouka(1,2,weights,safe);
            risky.hyouka=(short)(1+rs);safe.hyouka=(short)ss;
            if(Plugin.Options.EnableNativeWarAI.Value){Check(ss>rs+1);Check(w_tach.SelectResult(1,2,risky).Pointer==safe.Pointer);DecisionDifferences++;}
            units[0][0].x=10;units[0][0].y=7;units[1][3].flag=1;units[1][3].chara[0]=62;units[1][3].x=8;units[1][3].y=7;
            var retreat=Candidate(7,7);var advance=Candidate(9,7);retreat.next=advance;advance.next=Candidate(8,7);
            WarAiRuntime.Current=new(1,3);int retreatScore=w_tach.Hyouka(1,3,weights,retreat),advanceScore=w_tach.Hyouka(1,3,weights,advance);
            retreat.hyouka=(short)(1+retreatScore);advance.hyouka=(short)advanceScore;
            if(Plugin.Options.EnableNativeWarAI.Value){Check(advanceScore>retreatScore+1);Check(w_tach.SelectResult(1,3,retreat).Pointer==advance.Pointer);DecisionDifferences++;}
            var activePolicy=TotalWarRuntime.Policy!;bool oldAi=activePolicy.EnemyAi;activePolicy.EnemyAi=false;
            Check(w_tach.Hyouka(1,3,weights,advance)==0);activePolicy.EnemyAi=oldAi;
            // Scope exit and disabled AI do not inherit tactical deltas.
            WarAiRuntime.Current=null;Check(w_tach.Hyouka(1,2,weights,safe)==0);
            Check(!TotalWarRuntime.Rules.Scenarios.Any(p=>p.WarNumber is 10 or 15));
        }
        finally
        {
            WarAiRuntime.Current=null;TotalWarRuntime.Clear();TotalWarRuntime.Probe=false;TotalWarRuntime.Generation=0;TotalWarRuntime.LastBattle=null;TotalWarRuntime.AiTrace.Clear();TotalWarRuntime.AiChanges=TotalWarRuntime.SelectedChanges=0;TotalWarRuntime.Result="Vanilla";TotalWarRuntime.Ending=null;TotalWarRuntime.Transition="No chapter";
            foreach(var (p,enabled,ai) in policies){p.Enabled=enabled;p.EnemyAi=ai;}
            for(int i=0;i<properties.Length;i++)properties[i].SetValue(null,savedChars[i]);
            w_unit.unit=savedUnits;war_h.g_w_war_no=savedWar;war_sys.turn=savedTurn;war_sys.end_event_no=savedEnd;war_sys.game_over_flag=savedOver;
            w_tact.selected_result=savedSelected;w_tact.result=savedResult;UnityEngine.Random.state=random;
        }
        Check(w_unit.unit?.Pointer==savedUnits?.Pointer);Check(war_h.g_w_war_no==savedWar);Check(war_sys.turn==savedTurn);Check(war_sys.end_event_no==savedEnd);Check(war_sys.game_over_flag==savedOver);
        var restored=UnityEngine.Random.state;Check(restored.s0==random.s0&&restored.s1==random.s1&&restored.s2==random.s2&&restored.s3==random.s3);
        for(int i=0;i<properties.Length;i++)Check(((Il2CppReferenceArray<WAR_CHARA_TYPE>?)properties[i].GetValue(null))?.Pointer==((Il2CppReferenceArray<WAR_CHARA_TYPE>?)savedChars[i])?.Pointer);
        Check(w_tact.selected_result?.Pointer==savedSelected?.Pointer);Check(w_tact.result?.Pointer==savedResult?.Pointer);Check(!TotalWarRuntime.Ready&&!TotalWarRuntime.Armed);
        Passed=true;
        string directory=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","native-war");Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory,"total-war-probe.json"),JsonSerializer.Serialize(new{Version="1.13.0-native-war-foundation.1",Assertions,Passed,DecisionDifferences,GlobalsRestored=true,RandomStateRestored=true,BattlefieldPlayed=false,StoryCallbacksExecuted=false},new JsonSerializerOptions{WriteIndented=true}));
        Plugin.Logger.LogInfo($"Total War probe PASS: {Assertions} assertions; actual native later-war outcomes, evaluator and selection. Decision differences={DecisionDifferences}; globals/RNG restored. No ending playback or campaign callback tested.");
    }
}
