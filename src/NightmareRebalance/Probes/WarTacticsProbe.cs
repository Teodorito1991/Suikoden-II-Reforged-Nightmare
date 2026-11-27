using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Text.Json;
namespace NightmareRebalance;
internal static class WarTacticsProbe
{
    internal static int Assertions,SelectionDifferences,LegalCandidates;
    internal static bool Passed;
    internal static readonly List<object> Maps=new();
    internal static void Run()
    {
        if(!Plugin.Options.RunNativeWarSelfTest.Value)return;
        if(!Plugin.Options.EnableWarTactics.Value||!Plugin.Options.EnableNativeWarAI.Value||!WarTacticsRuntime.Config.Enabled){Plugin.Logger.LogInfo("War Tactics native probe not run: tactical AI is disabled by configuration.");return;}
        if(war_h.g_w_war_no!=0||WarTacticsRuntime.Armed)throw new InvalidOperationException("Tactics probe is startup-only.");
        var savedUnits=w_unit.unit;int savedWar=war_h.g_w_war_no,savedMap=war_h.g_w_map_no;long savedTurn=war_sys.turn;byte savedEnd=war_sys.end_event_no;sbyte savedOver=war_sys.game_over_flag;
        var savedSelected=w_tact.selected_result;var savedResult=w_tact.result;var rng=UnityEngine.Random.state;
        int[] wars={1,2,3,6,10,13,15};
        var props=wars.Select(n=>typeof(w_dat_2).Assembly.GetType("w_dat_"+n)!.GetProperty("chara_"+n)!).ToArray();var chars=props.Select(p=>p.GetValue(null)).ToArray();
        bool enabled=WarTacticsRuntime.Config.Enabled;var quotas=WarTacticsRuntime.Config.Objectives.Select(o=>(o,o.Enabled,o.RequiredRouts)).ToArray();
        var savedEvidence=WarTacticsRuntime.Evidence;
        var units=new Il2CppReferenceArray<Il2CppReferenceArray<UNIT_TYPE>>(2);
        for(int side=0;side<2;side++){units[side]=new Il2CppReferenceArray<UNIT_TYPE>(15);for(int i=0;i<15;i++)units[side][i]=new UNIT_TYPE{chara=new Il2CppStructArray<sbyte>(new sbyte[]{-1,-1,-1})};}
        void Check(bool v){Assertions++;if(!v)throw new InvalidOperationException("War Tactics native assertion "+Assertions);}
        void Roster(int n,bool appear)
        {
            war_h.g_w_war_no=n;war_h.g_w_map_no=war_h.war2map[n];war_sys.turn=2;war_sys.end_event_no=73;war_sys.game_over_flag=0;
            var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+n)!;
            var data=(Il2CppReferenceArray<Il2CppReferenceArray<Il2CppStructArray<sbyte>>>)type.GetProperty("unit_init_data_"+n)!.GetValue(null)!;
            for(int side=0;side<2;side++)for(int i=0;i<15;i++)
            {var src=data[side][i];var u=units[side][i];u.x=src[0];u.y=src[1];u.flag=src[2];u.damage=0;for(int c=0;c<3;c++)u.chara[c]=src[3+c];if(appear&&side==1&&u.chara[0]>=0)u.flag=1;}
            WarTacticsRuntime.Armed=true;WarTacticsRuntime.Begin();
        }
        try
        {
            WarTacticsRuntime.Probe=true;TotalWarRuntime.Probe=true;w_unit.unit=units;
            foreach(int n in wars)
            {
                var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+n)!;type.GetMethod("init")!.Invoke(null,null);
                Check(WarTacticsRuntime.Audit(n));Roster(n,n is 6 or 13);Check(WarTacticsRuntime.Ready);
                if(n==10)foreach(int i in new[]{0,3,4,5})units[1][i].flag=1; // Verified opening appearance fixture only.
                // Native reachability on the real terrain. Keep staging/control untouched
                // in production; appearance here is an explicitly disposable fixture.
                for(int actor=0;actor<15;actor++)
                {
                var acting=units[1][actor];if(acting.flag!=1||w_data.CheckFieldOnStage(acting.x,acting.y)==0)continue;
                var moves=NativeTacticsBoard.Reach(1,actor);var free=new List<TACTICS_RESULT_TYPE>();
                for(int at=0;at<192;at++)if(moves[at]==1&&w_data.CheckFieldOnStage(at%16,at/16)!=0&&(w_unit.GetUnitNumber(at%16,at/16,1) is int occ&&(occ==-1||occ==actor)))free.Add(new(){act=1,tx=(byte)(at%16),ty=(byte)(at/16),target=255});
                Check(free.Count>=1);LegalCandidates+=free.Count;
                var weights=new Il2CppReferenceArray<TACTICS_HYOUKA_TYPE>(1);weights[0]=new TACTICS_HYOUKA_TYPE{var=20};
                var tail=new TACTICS_RESULT_TYPE();for(int i=0;i<free.Count;i++)free[i].next=i+1<free.Count?free[i+1]:tail;
                WarTacticsRuntime.Config.Enabled=false;WarAiRuntime.Current=new(1,actor);
                foreach(var c in free)c.hyouka=(short)w_tach.Hyouka(1,actor,weights,c);
                var beforeRandom=UnityEngine.Random.state;var baseline=w_tach.SelectResult(1,actor,free[0]);
                WarTacticsRuntime.Config.Enabled=true;var scope=new WarAiRuntime.Scope(1,actor);WarAiRuntime.Current=scope;
                foreach(var c in free)c.hyouka=(short)w_tach.Hyouka(1,actor,weights,c);
                UnityEngine.Random.state=beforeRandom;var selected=w_tach.SelectResult(1,actor,free[0]);
                Check(free.Any(c=>c.Pointer==selected.Pointer));Check(moves[selected.ty*16+selected.tx]==1);Check(selected.act==1);
                Check(!WarTacticsRuntime.AiFaulted);
                bool changed=selected.Pointer!=baseline.Pointer;if(changed)SelectionDifferences++;
                Maps.Add(new{War=n,Unit=actor,Map=war_h.g_w_map_no,LegalMoveCandidates=free.Count,
                    ManeuverScoreChanges=scope.ManeuverChanges,SelectedChanged=changed,
                    Selected=new{X=selected.tx,Y=selected.ty},Original=new{X=baseline.tx,Y=baseline.ty},FixtureOnly=true,StoryEventsExecuted=false});
                WarAiRuntime.Current=null;
                }
                if(n is 10 or 15)
                {TotalWarRuntime.Ready=false;war_sys.turn=20;Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==1);}
            }
            // War 6 template feasibility: decode-proven opening arrivals and allied
            // disappearances are represented in disposable state, not executed scripts.
            Roster(6,true);foreach(int i in new[]{5,8,9})units[0][i].flag=0;
            var estimates=new List<object>();int reachable=0,totalHp=0;
            for(int target=1;target<=11;target++)
            {
                var enemy=units[1][target];int hp=w_unit.GetUnitHP(1,target);totalHp+=hp;
                int bestCost=10000,bestTurns=10000,bestChance=0;
                for(int ally=0;ally<15;ally++)
                {
                    var u=units[0][ally];if(u.flag!=1)continue;
                    int move=w_unit.GetUnitMove(0,ally),range=w_unit.GetUnitAttackRange(0,ally);
                    var distance=NativeTacticsBoard.Costs(0,ally,enemy.x,enemy.y,Math.Max(1,range),false);
                    int cost=distance[u.y*16+u.x];if(cost>=10000)continue;
                    bestCost=Math.Min(bestCost,cost);bestTurns=Math.Min(bestTurns,(cost+Math.Max(1,move)-1)/Math.Max(1,move));
                    sbyte x=u.x,y=u.y;
                    try
                    {
                        // Hypothetical attack position belongs only to this disposable
                        // fixture. Never move a live party or advance campaign state.
                        for(int at=0;at<192;at++)if(distance[at]==0&&w_unit.GetUnitNumber(at%16,at/16,1)==-1)
                        {u.x=(sbyte)(at%16);u.y=(sbyte)(at/16);bestChance=Math.Max(bestChance,w_kekka.CalcAttackHitRate(0,(byte)ally,1,(byte)target));}
                    }
                    finally{u.x=x;u.y=y;}
                }
                if(bestCost<10000)reachable++;
                estimates.Add(new{Target=target,Hp=hp,Protected=w_chara.GetCharaZenmetu(w_unit.GetUnitChara(1,target,0))!=0,TerrainOnlyCost=bestCost,TerrainOnlyTravelTurns=bestTurns,TemplateBestNativeHitChance=bestChance});
            }
            Maps.Add(new{War6Feasibility=true,TemplateAllies=7,OpeningEnemyArrivalsRepresented=true,AlliedDisappearanceFixture=new[]{5,8,9},TerrainReachableTargets=reachable,RequiredHitsAllEleven=totalHp,Targets=estimates,IncludesOccupancyScheduling=false,IncludesPlayerRecruitment=false,GameplayFeasibilityProven=false});
            Check(reachable>=4);Check(totalHp>=11);
            Roster(3,false);var breakthrough=NativeTacticsBoard.Costs(0,0,5,2,1,false);var hero3=units[0][0];int breakthroughCost=breakthrough[hero3.y*16+hero3.x];Check(breakthroughCost<10000);
            Maps.Add(new{War3BreakthroughTerrainCost=breakthroughCost,Target=new{X=5,Y=2,Radius=1},OccupancySchedulingIncluded=false,GameplayPlayed=false});
            // Actual result pipeline: alternate goals never precede native narrative
            // deadlines; protected command remains alive, original ending preserved.
            foreach(int n in new[]{3,6,13})
            {
                Roster(n,true);TotalWarRuntime.Armed=true;TotalWarRuntime.Begin();Check(TotalWarRuntime.Ready);
                var p=WarTacticsRuntime.Config.Objectives.Single(o=>o.WarNumber==n);p.Enabled=true;
                for(int i=1;i<=p.RequiredRouts;i++)units[1][i].flag=2;
                if(n==3){units[0][0].x=4;units[0][0].y=2;Check(w_data.CheckFieldOnStage(4,2)!=0);}
                int earliest=n==3?14:n==6?9:7;war_sys.turn=earliest-1;
                Check(w_syouha.CheckSyouhai()==0);war_sys.turn=earliest;
                if(Plugin.Options.EnableNativeWarRemix.Value&&Plugin.Options.EnableWarTactics.Value)
                {Check(w_syouha.CheckSyouhai()==-1);Check(war_sys.end_event_no==(n==13?4:1));Check(units[1][0].flag==1);Check(war_sys.game_over_flag==0);}
                WarTacticsRuntime.Config.Enabled=false;TotalWarRuntime.Ending=null;WarTacticsRuntime.Evidence=new(){War=n};war_sys.end_event_no=73;
                Check(w_syouha.CheckSyouhai()==(Plugin.Options.EnableNativeWarRemix.Value?0:-1));WarTacticsRuntime.Config.Enabled=true;
            }
            Check(SelectionDifferences>=1);Check(WarTacticsRuntime.Audit(10));Check(WarTacticsRuntime.Audit(15));
        }
        finally
        {
            WarAiRuntime.Current=null;WarTacticsRuntime.Clear();WarTacticsRuntime.Probe=false;WarTacticsRuntime.Config.Enabled=enabled;WarTacticsRuntime.Evidence=savedEvidence;WarTacticsRuntime.Generation=0;WarTacticsRuntime.War=0;WarTacticsRuntime.ScoreChanges=WarTacticsRuntime.SelectedChanges=0;WarTacticsRuntime.ScoreTrace.Clear();WarTacticsRuntime.CommandTrace.Clear();
            foreach(var (p,e,q) in quotas){p.Enabled=e;p.RequiredRouts=q;}
            TotalWarRuntime.Clear();TotalWarRuntime.Probe=false;TotalWarRuntime.Generation=0;TotalWarRuntime.LastBattle=null;TotalWarRuntime.Result="Vanilla";TotalWarRuntime.Ending=null;TotalWarRuntime.Transition="No chapter";
            for(int i=0;i<props.Length;i++)props[i].SetValue(null,chars[i]);
            w_unit.unit=savedUnits;war_h.g_w_war_no=savedWar;war_h.g_w_map_no=savedMap;war_sys.turn=savedTurn;war_sys.end_event_no=savedEnd;war_sys.game_over_flag=savedOver;w_tact.selected_result=savedSelected;w_tact.result=savedResult;UnityEngine.Random.state=rng;
        }
        Check(w_unit.unit?.Pointer==savedUnits?.Pointer);Check(war_h.g_w_war_no==savedWar);Check(war_h.g_w_map_no==savedMap);Check(war_sys.turn==savedTurn);Check(war_sys.end_event_no==savedEnd);Check(war_sys.game_over_flag==savedOver);
        var rs=UnityEngine.Random.state;Check(rs.s0==rng.s0&&rs.s1==rng.s1&&rs.s2==rng.s2&&rs.s3==rng.s3);
        for(int i=0;i<props.Length;i++)Check(((Il2CppReferenceArray<WAR_CHARA_TYPE>?)props[i].GetValue(null))?.Pointer==((Il2CppReferenceArray<WAR_CHARA_TYPE>?)chars[i])?.Pointer);
        Passed=true;string directory=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","native-war");Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory,"war-tactics-probe.json"),JsonSerializer.Serialize(new{Version="1.13.0-native-war-foundation.1",Assertions,Passed,SelectionDifferences,LegalCandidates,GlobalsAndRandomRestored=true,StoryEventsExecuted=false,BattlefieldPlayed=false,Maps},new JsonSerializerOptions{WriteIndented=true}));
        Plugin.Logger.LogInfo($"War Tactics probe PASS: {Assertions} assertions; native real-map legal candidates={LegalCandidates}; selection differences={SelectionDifferences}; restored globals/RNG. Story/gameplay acceptance separate.");
    }
}
