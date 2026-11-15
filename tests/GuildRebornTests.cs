using NightmareRebalance;

internal static class GuildRebornTests
{
    internal static void Run(Action<bool,string> check)
    {
        var menu = new GuildMenuNavigation();
        GuildChoice Confirm(GuildQuestState state = GuildQuestState.NoContract, bool available = true)
        {
            menu.Confirm(false,false,state,available);
            return menu.Confirm(true,true,state,available);
        }
        void Down() => menu.Move(false,true);

        for (int hunt = 0; hunt < 3; hunt++)
        {
            menu.Open(false);
            check(menu.Confirm(true,true,GuildQuestState.NoContract,true) == GuildChoice.None &&
                menu.Page == GuildPage.Services, "Opening confirm cannot activate a service");
            Confirm();
            for (int row = 0; row < hunt; row++) Down();
            check(menu.Page == GuildPage.Hunts && menu.Focus == hunt &&
                Confirm() == GuildChoice.Accept && menu.Back(), "Every hunt reachable");
        }
        for (int drill = 0; drill < 4; drill++)
        {
            menu.Open(false);
            Down();
            Confirm();
            for (int row = 0; row < drill; row++) Down();
            check(Confirm(available:false) == GuildChoice.None && menu.Page == GuildPage.Training,
                "Unavailable drill cannot advance");
            Confirm();
            check(menu.Page == GuildPage.Difficulty && menu.Drill == drill && menu.Focus == 0 &&
                menu.Confirm(true,true,GuildQuestState.NoContract,true) == GuildChoice.None,
                "Drill selection cannot also start Standard");
            check(Confirm() == GuildChoice.Start, "Standard starts through explicit confirm");
            Down();
            check(Confirm() == GuildChoice.Start && menu.Focus == 1, "Veteran reachable as a separate row");
            check(menu.Back() && menu.Page == GuildPage.Training && menu.Focus == drill &&
                menu.Back() && !menu.Back(), "Cancel returns through the correct parent");
        }
        menu.Open(true);
        check(Confirm(GuildQuestState.AwaitingReport)==GuildChoice.Report, "Completed hunt opens report directly");
        menu.Open(false);
        Down(); Down(); Confirm();
        check(Confirm(GuildQuestState.Accepted)==GuildChoice.Withdraw, "Withdraw lives in Guild Status");
        check(GuildApproach.AtCounter(232,240,232,224) && !GuildApproach.AtCounter(208,224,232,224) &&
            !GuildApproach.AtCounter(252,224,232,224), "Approach excludes the original box and door side");
        check(GuildApproach.FacingClerk(1) && GuildApproach.FacingClerk(5) &&
            !GuildApproach.FacingClerk(0) && !GuildApproach.FacingClerk(3), "Confirm is scoped to north-facing native poses");

        foreach (var (width,height) in new[] {(1920,1080),(1024,768),(3440,1440),(1280,720)})
        {
            float scale = GuildRebornScreen.Scale(width,height);
            check(GuildRebornScreen.Width*scale <= width-47 && GuildRebornScreen.Height*scale <= height-47,
                "Guild viewport fits the native-style screen at supported aspect ratios");
        }
        var actor = new TacticalUnit(3,4,4,1,2,3,false);
        var facts = new TacticalFacts(4,2,0,0,0,0,0,0,0,false,true,1);
        var exposed = new WarManeuver(1,0,0,0,2,1,true);
        check(WarTacticsRules.Maneuver(40,6,actor,0,30,facts,exposed) < 40,
            "Wounded unit avoids a low-odds counterattack");
        check(WarTacticsRules.Maneuver(40,10,actor,0,30,facts,exposed) >= 30 &&
            WarTacticsRules.Maneuver(40,12,actor,0,30,facts,exposed) == 40,
            "Staged war bonus bounded and unknown wars excluded");
        check(WarTacticsRules.Maneuver(40,2,actor,1,100,facts with { ImmediateThreat = 0, Threat = 1 },
            exposed with { CurrentImmediate = 1, TargetCounters = false }) > 40,
            "Fort pressure rewards safe terrain-aware progress");
    }
}
