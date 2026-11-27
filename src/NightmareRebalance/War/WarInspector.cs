namespace NightmareRebalance;
internal static class WarInspector
{
    internal static int CurrentWar=>TotalWarRuntime.Armed||War2Runtime.Armed||WarRuntime.Active||NativeWarRemix.Active?war_h.g_w_war_no:0;
    internal static object[] Commanders()
    {
        if(CurrentWar==0||!(TotalWarRuntime.Ready||War2Runtime.Ready||WarRuntime.Active))return Array.Empty<object>();
        var units=w_unit.unit;if(units?.Length!=2)return Array.Empty<object>();
        var list=new List<object>();
        for(int side=0;side<2;side++)
        {
            if(units[side]?.Length!=15)continue;
            for(int i=0;i<15;i++)
            {
                var u=units[side][i];if(u==null||u.flag==0||u.chara==null||u.chara.Length<1||u.chara[0]<0)continue;
                int leader=u.chara[0];var source=TotalWarRuntime.Characters(CurrentWar);var c=source!=null&&leader<source.Length?source[leader]:null;
                list.Add(new{Force=side,Slot=i,SourceCharacter=leader,NameId=c?.name,Flag=u.flag,Control=u.control,X=u.x,Y=u.y,Damage=u.damage});
            }
        }
        return list.ToArray();
    }
    internal static string Objective()
    {
        int war=CurrentWar;
        bool tactics=WarTacticsRuntime.Config.Enabled&&Plugin.Options.EnableWarTactics.Value&&Plugin.Options.EnableNativeWarRemix.Value&&TotalWarRuntime.Policy is {Enabled:true};
        var goal=WarTacticsRuntime.Config.Objectives.FirstOrDefault(o=>o.WarNumber==war&&o.Enabled);
        if(tactics&&goal!=null)return war==3?$"Route command OR {goal.RequiredRouts} escorts + hero breakthrough x4..6/y1..3":war==6?$"Route {goal.RequiredRouts} eligible troops; Kiba protected":$"Route {goal.RequiredRouts} enemies; protected command excluded";
        return war switch{1=>"Route enemy command; original war-1 continuation",2=>"Protect fort x3..5/y6..8; rout Solon Jhee",3=>"Route Solon Jhee before withdrawal",6=>"Route eligible troops (slots 1..11); command protected",13=>"Keep allied command alive until hold deadline",0=>"No war active",_=>"Native staged objective; tactical enemy AI when active"};
    }
    internal static string Progress()
    {
        int war=CurrentWar;if(war==0)return "No active objective";
        var units=War2Runtime.States();int active=units.Count(u=>u.Force==0&&u.Flag==1),enemies=units.Count(u=>u.Force==1&&u.Flag==1);
        string extra=war is 3 or 6 or 13?$"; unique eligible routs {WarTacticsRuntime.Routed.Count}":war==2?$"; fort intruders {units.Count(u=>u.Force==1&&u.Flag==1&&u.X is >=3 and <=5&&u.Y is >=6 and <=8)}":"";
        return $"Allied active {active}; enemy active {enemies}{extra}";
    }
    internal static object Snapshot()=>new{SafetyWitness=NativeWarWitness.Snapshot(),WarNumber=CurrentWar,CurrentTurn=CurrentWar==0?0:war_sys.turn,Objective=Objective(),Progress=Progress(),RemixEnabled=Plugin.Options.EnableNativeWarRemix.Value,AiEnabled=Plugin.Options.EnableNativeWarAI.Value,SpecializationsEnabled=Plugin.Options.EnableWarSpecializations.Value,Commanders=Commanders(),TotalWar=TotalWarRuntime.Snapshot(),WarTactics=WarTacticsRuntime.Snapshot(),CampaignReturnVerified=false};
    internal static string Summary()
    {
        int war=CurrentWar;
        string policy=war==2?$"War-2 enabled {War2Runtime.Policy?.Enabled}; deadline {War2Runtime.Policy?.MaximumTurn}":TotalWarRuntime.Policy is {} p?$"Policy enabled {p.Enabled}; deadline {p.MaximumTurn}":"Native / retained war-1 policy";
        string leaders="No active commanders";
        if(war!=0)
        {
            var source=TotalWarRuntime.Characters(war);var rows=w_unit.unit;
            if(source!=null&&rows?.Length==2&&rows[0]?.Length==15&&rows[1]?.Length==15)
            {
                int Id(int side){var c=rows[side][0]?.chara;return c!=null&&c.Length>0&&c[0]>=0&&c[0]<source.Length?source[c[0]].name:-1;}
                leaders=$"Command NameIds: allied {Id(0)} / enemy {Id(1)}";
            }
        }
        var e=WarTacticsRuntime.Evidence;
        return $"READ ONLY - F9 exports full commanders and traces\n\nWar {war}; turn {(war==0?0:war_sys.turn)}\n{Objective()}\n{policy}\n{leaders}\n{Progress()}\n\nAI scores changed: {WarTacticsRuntime.ScoreChanges}\nStrict selected changes vs v0.8: {WarTacticsRuntime.SelectedChanges}\nOverall selected changes vs native: {TotalWarRuntime.SelectedChanges}\nEnding selected: {e.NativeEnding?.ToString()??"none"}; {e.Outcome}\nEnding playback started/finished: {e.EndingStarted}/{e.EndingFinished}\nNative finish callback: {e.FinishCallbackReturned}\nCampaign map updates after ending: {e.MapUpdatesAfterEnding}/30\nSave/reload integrity: not verified by inspector\n\nRPG 80/31/280; war profiles 21 + 9 retained.\nOriginal story engine; read-only inspection.";
    }
}
