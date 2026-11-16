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

        for (int hunt = 0; hunt < 10; hunt++)
        {
            menu.Open(false,10);
            check(menu.Confirm(true,true,GuildQuestState.NoContract,true) == GuildChoice.None &&
                menu.Page == GuildPage.Services, "Opening confirm cannot activate a service");
            Confirm();
            for (int row = 0; row < hunt; row++) Down();
            check(menu.Page == GuildPage.Hunts && menu.Focus == hunt &&
                Confirm() == GuildChoice.Accept && menu.Back(), "Every hunt reachable");
        }
        var rows = Enumerable.Range(0, 10).Select(i => new GuildMenuRow("Contract "+i,"")).ToArray();
        for (int focus = 0; focus < rows.Length; focus++)
        {
            var view = new GuildMenuView("HUNT CONTRACTS", rows, focus, "", "", "", "", "", "", "", "", false);
            var page = GuildRebornScreen.Paginate(view);
            check(page.Rows.Length <= 4 && page.Rows[page.Focus] == rows[focus] &&
                page.Section.EndsWith($"{focus / 4 + 1}/3"), "Page boundary keeps selected contract visible");
        }
        menu.Open(false,10);
        Confirm();
        menu.Move(true,false);
        check(menu.Focus==9,"Up wraps to final contract");
        Down();
        check(menu.Focus==0,"Down wraps to first contract");
        menu.Open(false,0);
        Confirm();
        Down();
        check(menu.Page==GuildPage.Hunts && Confirm()==GuildChoice.None && menu.Back(),"Empty catalogue remains closable");
        for (int drill = 0; drill < 4; drill++)
        {
            menu.Open(false,10);
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
        menu.Open(true,10);
        check(Confirm(GuildQuestState.AwaitingReport)==GuildChoice.Report, "Completed hunt opens report directly");
        menu.Open(false,10);
        Down(); Down(); Confirm();
        check(Confirm(GuildQuestState.Accepted)==GuildChoice.Withdraw, "Withdraw lives in Guild Status");
        menu.Open(false,10);
        Down(); Down(); Down(); Confirm();
        check(menu.Page==GuildPage.Strategy && menu.RowCount==StarBondRules.Catalogue.Length,"Strategy follows existing services");
        Down(); Down(); Down();
        check(menu.Focus==3 && Confirm()==GuildChoice.None && menu.Page==GuildPage.Services,"Strategy confirm returns");
        Down();
        check(Confirm()==GuildChoice.Leave,"Leave is reachable from the fifth service");
        var serviceRows=Enumerable.Range(0,5).Select(i=>new GuildMenuRow("Service "+i,"")).ToArray();
        var services=GuildRebornScreen.Paginate(new GuildMenuView("SERVICES",serviceRows,4,"","","","","","","","",false),5);
        var last=GuildRebornScreen.Row(services.Focus,services.Rows.Length>GuildRebornScreen.RowsPerPage);
        check(services.Rows.Length==5 && last.Y+last.H < GuildRebornScreen.Footer.Y,"All service rows fit on one page");
        foreach (var bond in StarBondRules.Catalogue)
        {
            check(StarBondRules.Includes(bond,bond.Members),"Exact Star Bond roster");
            check(!StarBondRules.Includes(bond,bond.Members.Skip(1).ToArray()),"Missing partner excluded");
        }
        check(StarBondRules.DamageBonus(20000,8,24)==24 && StarBondRules.DamageBonus(0,8,24)==0 &&
            StarBondRules.Wounded(50,100) && !StarBondRules.Wounded(0,100),"Conditional bonus bounds");
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
