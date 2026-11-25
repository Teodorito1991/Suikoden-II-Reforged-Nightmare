namespace NightmareRebalance;

internal static class HallPresentation
{
    internal static string RewardName(string id)=>id switch {
        "stone-of-magic"=>"Stone of Magic","stone-of-speed"=>"Stone of Speed",
        "magic-protection-stone"=>"Stone of Magic Protection",_=>"Stone of Power"};
    internal static GuildMenuView View(int focus, System.Func<int,bool> unlocked,
        System.Func<int,bool> configured, string reason, string back, bool preview, bool spoilers=false)
    {
        var trials=HallTrialCatalogue.Entries;
        focus=System.Math.Clamp(focus,0,trials.Length);
        if(focus==trials.Length)return GuildRebornScreen.Paginate(new("HALL OF ECHOES",
            trials.Select((t,i)=>new GuildMenuRow(unlocked(i)?t.Name:"???",configured(i)?"Original / tiers":"Disabled"))
                .Append(new GuildMenuRow(spoilers?"Restore Story Progression":"Unlock All Bosses",spoilers?"Spoiler Mode is ON":"Spoiler Mode is OFF")).ToArray(),focus,
            spoilers?"Restore Story Progression":"Unlock All Bosses","Spoiler access preference",
            spoilers?"Return to the original story-based availability rules. No campaign flags are changed.":"Reveal supported boss memories regardless of story progress. A major-spoiler warning and separate confirmation follow.",
            "","Disabled trials and all native battle safety guards remain in force.",reason,"Review",back,preview));
        var selected=trials[focus];
        bool known=unlocked(focus), available=known&&configured(focus);
        var rows=trials.Select((trial,index)=>new GuildMenuRow(
            unlocked(index)?trial.Name:"???",
            !unlocked(index)?"Locked":configured(index)?"Original / tiers":"Disabled"))
            .Append(new GuildMenuRow(spoilers?"Restore Story Progression":"Unlock All Bosses",spoilers?"Spoiler Mode is ON":"Spoiler Mode is OFF")).ToArray();
        return GuildRebornScreen.Paginate(new("HALL OF ECHOES",rows,focus,
            known?selected.Name:"Unknown Echo",available?(HallUnlockRules.IsBonus(selected.Id)?$"Bonus rematch · After {HallUnlockRules.BonusMilestone(selected.Id)}":"Normal rematch"):"Unavailable",
            known?selected.Description:"A memory yet to be made. Continue your journey.","",
            known?selected.Rewards:"",reason,available?"Choose tier":"Unavailable",back,preview));
    }
    internal static GuildMenuView Difficulty(int index,int focus,bool unlocked,bool configured,bool nightmare,bool stones,bool preview=false,string message="")
    {
        var d=HallTrialCatalogue.Entries[Math.Clamp(index,0,HallTrialCatalogue.Entries.Length-1)];
        var p=HallNightmareRules.Find(d.Id);
        bool advanced=focus==1, available=unlocked&&configured&&(!advanced||nightmare&&p!=null);
        return new("ECHO DIFFICULTY",new[]{new GuildMenuRow("Original Echo","Recall the original battle"),
            new GuildMenuRow("Nightmare Echo",nightmare&&p!=null?"Advanced optional challenge":"Unavailable")},focus,
            unlocked?d.Name:"Unknown Echo",advanced&&p!=null?(d.Id=="beast-rune"?"Nightmare Echo · Native multi-part controller":$"Nightmare Echo · Original foe level {p.Level}"):"Original Echo · Normal campaign",
            !unlocked?"A memory yet to be made.":advanced?(p?.Advice??"No advanced version is offered."):d.Description,
            !unlocked?"":advanced&&p!=null&&stones?$"Native rewards + 25% {RewardName(p.RewardId)} offer if no loot rolls.":d.Rewards,
            !unlocked?"Continue your journey.":advanced&&p!=null?$"HP {p.HpPercent}% · Damage {p.Damage:0.00}× Original. Native inventory applies.":"Original HP, attacks and native rewards are preserved.",
            message.Length!=0?message:available?"Confirm to enter. Cancel returns to the Hall catalogue.":"This tier is unavailable. Original Echo remains separately accessible.",
            available?"Begin rematch":"Unavailable","Back",preview);
    }
}
