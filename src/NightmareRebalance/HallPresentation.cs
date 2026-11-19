namespace NightmareRebalance;

internal static class HallPresentation
{
    internal static GuildMenuView View(int focus, System.Func<int,bool> unlocked,
        System.Func<int,bool> configured, string reason, string back, bool preview)
    {
        var trials=HallTrialCatalogue.Entries;
        focus=System.Math.Clamp(focus,0,trials.Length-1);
        var selected=trials[focus];
        bool known=unlocked(focus), available=known&&configured(focus);
        var rows=trials.Select((trial,index)=>new GuildMenuRow(
            unlocked(index)?trial.Name:"???",
            !unlocked(index)?"Locked":configured(index)?"Normal":"Disabled")).ToArray();
        return GuildRebornScreen.Paginate(new("HALL OF ECHOES",rows,focus,
            known?selected.Name:"Unknown Echo",available?"Normal rematch":"Unavailable",
            known?selected.Description:"A memory yet to be made. Continue your journey.","",
            known?selected.Rewards:"",reason,available?"Begin rematch":"Unavailable",back,preview));
    }
}
