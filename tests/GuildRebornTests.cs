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
        check(Confirm()==GuildChoice.None && menu.Page==GuildPage.Echoes,"Hall provides information only");
        check(Confirm()==GuildChoice.None && menu.Page==GuildPage.Services && menu.Focus==4,"Hall has no launch action");
        Down();
        check(Confirm()==GuildChoice.Leave,"Leave follows Hall of Echoes");
        menu.Open(false,10);
        for (int row = 0; row < 4; row++) Down();
        check(Confirm()==GuildChoice.None && menu.Page==GuildPage.Echoes &&
            menu.Confirm(true,true,GuildQuestState.NoContract,true,echoAvailable:true)==GuildChoice.None,
            "Opening the experimental Hall cannot confirm its challenge");
        menu.Confirm(false,false,GuildQuestState.NoContract,true,echoAvailable:true);
        check(menu.Confirm(true,true,GuildQuestState.NoContract,true,echoAvailable:true)==GuildChoice.StartEcho &&
            menu.Confirm(true,true,GuildQuestState.NoContract,true,echoAvailable:true)==GuildChoice.None,
            "Experimental Hall requires a fresh confirm and does not repeat while held");
        menu.EchoCount=2;
        menu.Open(false,10);
        for (int row=0; row<4; row++) Down();
        Confirm();
        check(menu.Page==GuildPage.Echoes && menu.RowCount==2, "Hall has independent encounter rows");
        Down();
        check(menu.Focus==1, "Second Hall encounter can receive focus");
        menu.Confirm(false,false,GuildQuestState.NoContract,true,echoAvailable:true);
        check(menu.Confirm(true,true,GuildQuestState.NoContract,true,echoAvailable:true)==GuildChoice.StartEcho &&
            menu.Focus==1, "Hall launch preserves selected encounter");
        Down();
        check(menu.Focus==0, "Hall encounter navigation wraps");
        menu.Back();
        check(menu.Page==GuildPage.Services && menu.Focus==4, "Hall Back restores service focus");
        var flags = new byte[256];
        bool Read(int flag, byte mask) => (flags[flag] & mask) != 0;
        check(!HallUnlockRules.Unlocked("harpy",HallUnlockPolicy.StoryProgression,false,Read),"Early Harpy memory is locked");
        flags[0x21]=0x10;
        check(HallUnlockRules.StoryUnlocked("harpy",Read),"Original Harpy completion witness unlocks memory");
        flags[0x15]=0x02;flags[0xDD]=0x02;flags[0xDF]=0x40;
        check(!HallUnlockRules.StoryUnlocked("sierra",Read)&&!HallUnlockRules.StoryUnlocked("golden-wolf",Read),"Unrelated map event flags are not boss victories");
        flags[0x21]=0;
        check(!HallUnlockRules.StoryUnlocked("harpy",Read),"A numeric duel ID cannot establish a Harpy victory");
        flags[0xB6]=0x02;flags[0x16]=0x02;flags[0x21]=0x10;
        check(HallUnlockRules.StoryUnlocked("rat",Read)&&HallUnlockRules.StoryUnlocked("golden-wolf",Read),"Original boss preload and completion witnesses unlock the correct trials");
        check(!HallUnlockRules.StoryUnlocked("sierra",Read)&&!HallUnlockRules.StoryUnlocked("harpy-second",Read),"Unproven witnesses do not inherit another boss's victory");
        flags[0xC9]=1;flags[0x56]=0x20;flags[0x3D]=0x10;
        check(HallUnlockRules.StoryUnlocked("stone-golem",Read)&&HallUnlockRules.StoryUnlocked("abomination",Read),"New original post-victory witnesses");
        check(!HallUnlockRules.StoryUnlocked("worm",Read),"The first Worm's pre-battle flag does not prove victory");
        flags=new byte[256]; // An earlier loaded campaign replaces the state; no cached unlock.
        flags[0xC2]=0x80;
        check(!HallUnlockRules.StoryUnlocked("harpy",Read),"Shared battle result cannot unlock a story memory");
        foreach(var trial in HallTrialCatalogue.Entries)
        {
            check(!HallUnlockRules.Unlocked(trial.Id,HallUnlockPolicy.UnrestrictedTesting,false,Read),"Testing override requires developer opt-in");
            check(HallUnlockRules.Unlocked(trial.Id,HallUnlockPolicy.UnrestrictedTesting,true,Read),"Explicit testing availability is independent of campaign flags");
        }
        check(flags[0x15]==0&&flags[0xC2]==0x80,"Availability checks never write flags");
        check(!HallUnlockRules.CanLaunch(true,false,true)&&!HallUnlockRules.CanLaunch(false,true,true)&&
            !HallUnlockRules.CanLaunch(true,true,false),"Independent module, trial and story gates");
        check(HallTrialCatalogue.Entries.Select(x=>x.Id).Distinct().Count()==8&&
            HallTrialCatalogue.Entries[0].Id=="rat"&&HallTrialCatalogue.Entries[1].Id=="harpy", "Stable Hall IDs and eight distinct trials");
        check(HallTrialCatalogue.Entries.Take(5).All(x=>!x.RetainCombatMain)&&
            HallTrialCatalogue.Entries.Skip(5).All(x=>x.RetainCombatMain),"Existing trials keep their accepted callback roles; new visual controllers are explicit");
        menu.EchoCount=HallTrialCatalogue.Entries.Length;
        menu.Open(false,10);
        for(int i=0;i<4;i++) Down();
        Confirm();
        for(int i=0;i<HallTrialCatalogue.Entries.Length-1;i++) Down();
        check(menu.Page==GuildPage.Echoes&&menu.Focus==7,"Eight-trial navigation reaches the final selectable row");
        var echoes=HallTrialCatalogue.Entries.Select(x=>new GuildMenuRow(x.Name,"Locked")).ToArray();
        var echoPage=GuildRebornScreen.Paginate(new GuildMenuView("HALL",echoes,7,"","","","","","","","",false));
        check(echoPage.Rows.Length==4&&echoPage.Focus==3&&echoPage.Rows[3]==echoes[7],"Hall pagination keeps final selected memory visible");
        var unknown=HallPresentation.View(0,_=>false,_=>true,"Locked","Back",false);
        check(unknown.Title=="Unknown Echo"&&unknown.Rows.All(x=>x==new GuildMenuRow("???","Locked")),"Locked memories hide boss identities");
        var serviceRows=Enumerable.Range(0,6).Select(i=>new GuildMenuRow("Service "+i,"")).ToArray();
        var services=GuildRebornScreen.Paginate(new GuildMenuView("SERVICES",serviceRows,5,"","","","","","","","",false),6);
        var last=GuildRebornScreen.Row(services.Focus,services.Rows.Length);
        check(services.Rows.Length==6 && last.Y+last.H < GuildRebornScreen.List.Y+GuildRebornScreen.List.H,"All service rows fit on one page");
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
