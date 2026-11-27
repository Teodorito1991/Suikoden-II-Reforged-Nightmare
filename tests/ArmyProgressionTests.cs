using NightmareRebalance;

internal static class ArmyProgressionTests
{
    internal static void Run()
    {
        int checks = 0;
        void Check(bool ok, string message)
        {
            checks++;
            if (!ok) throw new InvalidOperationException("ArmyProgressionTests: " + message);
        }

        // 1. Rank thresholds
        Check(ArmyProgressionRules.RankForXp(0) == ArmyRank.Recruit, "0 XP is Recruit");
        Check(ArmyProgressionRules.RankForXp(99) == ArmyRank.Recruit, "99 XP is Recruit");
        Check(ArmyProgressionRules.RankForXp(100) == ArmyRank.Lieutenant, "100 XP is Lieutenant");
        Check(ArmyProgressionRules.RankForXp(250) == ArmyRank.Captain, "250 XP is Captain");
        Check(ArmyProgressionRules.RankForXp(500) == ArmyRank.General, "500 XP is General");
        Check(ArmyProgressionRules.RankForXp(900) == ArmyRank.Marshal, "900 XP is Marshal");

        // 2. Doctrine progression & OverhaulRules bounds
        var flik = new CommanderProfile
        {
            NameId = 3, Name = "Flik", Branch = "Any",
            Attack = 8, Defense = 7, AssistAttack = 2, AssistDefense = 1,
            Abilities = new[] { "SP_MOUNT" }, Uses = new byte[] { 0 }
        };
        var vanguardGeneral = new CommanderProgress { NameId = 3, Name = "Flik", Xp = 550, Doctrine = ArmyDoctrine.Vanguard };
        var upgradedFlik = ArmyProgressionRules.ApplyProgression(flik, vanguardGeneral);
        Check(upgradedFlik.Attack >= 10 && upgradedFlik.Attack <= 20, "Vanguard General ATK bounded");
        Check(upgradedFlik.Defense == 8, "Captain DEF bonus applied");
        Check(upgradedFlik.AssistAttack == 3, "Vanguard General +1 Assist ATK");
        Check(upgradedFlik.Abilities.Contains("SP_MOUNT") && upgradedFlik.Abilities.Contains("SP_CRITICAL"), "No duplicate SP_MOUNT; unlocks SP_CRITICAL");
        Check(upgradedFlik.Abilities.Length == upgradedFlik.Abilities.Distinct().Count() && upgradedFlik.Abilities.Length <= 3, "Distinct bounded abilities");

        var tuta = new CommanderProfile
        {
            NameId = 23, Name = "Tuta", Branch = "Any",
            Attack = 3, Defense = 5, AssistAttack = 0, AssistDefense = 2,
            Abilities = new[] { "SP_MEDICAL1" }, Uses = new byte[] { 2 }
        };
        var tacticianGeneral = new CommanderProgress { NameId = 23, Name = "Tuta", Xp = 600, Doctrine = ArmyDoctrine.Tactician };
        var upgradedTuta = ArmyProgressionRules.ApplyProgression(tuta, tacticianGeneral);
        Check(upgradedTuta.Uses[0] == 3, "Tactician boosts active SP_MEDICAL1 uses up to cap 3");
        Check(upgradedTuta.Abilities.Contains("SP_SEE_THROUGH"), "Tactician General unlocks SP_SEE_THROUGH");
        Check(upgradedTuta.AssistDefense == 3, "Tactician General +1 Assist DEF");

        var maxed = new CommanderProfile
        {
            NameId = 99, Name = "CapTest", Branch = "Any",
            Attack = 19, Defense = 19, AssistAttack = 5, AssistDefense = 5,
            Abilities = new[] { "SP_MOUNT", "SP_CRITICAL", "SP_HP_PLUS" }, Uses = new byte[] { 0, 0, 0 }
        };
        var marshal = new CommanderProgress { NameId = 99, Name = "CapTest", Xp = 1200, Doctrine = ArmyDoctrine.Bulwark };
        var bounded = ArmyProgressionRules.ApplyProgression(maxed, marshal);
        Check(bounded.Attack == 20 && bounded.Defense == 20 && bounded.AssistAttack == 5 && bounded.AssistDefense == 5, "Marshal stats never exceed OverhaulRules caps");
        Check(bounded.Abilities.Length == 3, "Ability count never exceeds 3");

        // 3. Drill Directives
        var ironEnemy = ArmyProgressionRules.ApplyDirective(flik, DrillDirective.IronWall, isEnemy: true);
        var ironAlly = ArmyProgressionRules.ApplyDirective(flik, DrillDirective.IronWall, isEnemy: false);
        Check(ironEnemy.Defense == flik.Defense + 2 && ironAlly.Defense == flik.Defense, "Iron Wall buffs enemy DEF only");

        var noMedAlly = ArmyProgressionRules.ApplyDirective(tuta, DrillDirective.NoMedicine, isEnemy: false);
        var noMedEnemy = ArmyProgressionRules.ApplyDirective(tuta, DrillDirective.NoMedicine, isEnemy: true);
        Check(noMedAlly.Uses[0] == 0 && noMedEnemy.Uses[0] == 2, "No Field Medicine zeroes allied SP_MEDICAL uses only");

        Check(ArmyProgressionRules.IsDirectiveFulfilled(DrillDirective.ForcedMarch, 6), "Forced March fulfilled at turn 6");
        Check(!ArmyProgressionRules.IsDirectiveFulfilled(DrillDirective.ForcedMarch, 7), "Forced March missed at turn 7");
        Check(ArmyProgressionRules.AdjustedPotch(1500, DrillDirective.ForcedMarch, 5) == 2025, "Forced March 1.35x Potch bonus");
        Check(ArmyProgressionRules.AdjustedPotch(1500, DrillDirective.ForcedMarch, 8) == 1500, "Missed Forced March pays base Potch");

        // 4. Training XP, Subcommanders, and Cross-Theater Synergy
        var state = new ArmyLedgerState();
        var participants = new[]
        {
            new CommanderParticipation(3, "Flik", Leader: true, Survived: true),
            new CommanderParticipation(4, "Viktor", Leader: false, Survived: true),
            new CommanderParticipation(11, "Humphrey", Leader: true, Survived: false)
        };
        var summary = ArmyProgressionRules.AwardTrainingXp(state, participants, veteran: true, DrillDirective.IronWall, 5, enableCrossTheaterSynergy: true);
        Check(summary.CommandersUpdated == 3 && summary.DirectiveFulfilled, "All three distinct commanders awarded");
        Check(state.Commanders[3].Xp == 149, "Veteran + survivor + Iron Wall 1.35x leader XP = round(110*1.35)=149");
        Check(state.Commanders[4].Xp == 111, "Subcommander earns 75% of leader XP = round(110*1.35*0.75)=111");
        Check(state.Commanders[11].Xp == 122, "Routed leader earns base*1.35 = round(90*1.35)=122");
        Check(summary.RanksGained == 3, "All three promoted to Lieutenant");
        Check(state.MoraleBonusCharges == 1, "Veteran drill arms +1 cross-theater morale charge");

        var hallSummary = ArmyProgressionRules.AwardHallNightmareXp(state, new[] { (3, "Flik"), (23, "Tuta") });
        Check(hallSummary.CommandersUpdated == 2 && state.Commanders[3].Xp == 184 && state.Commanders[23].Xp == 35 && state.HallNightmareClears == 1,
            "Hall Nightmare clear awards +35 War XP to party commanders");

        // 5. Content-addressed save-bound ledger & branch isolation
        var index = new ArmyLedgerIndex();
        string hashA = ArmyProgressionRules.ComputeFingerprint(new[] { "{\"gold\":100}" }, new[] { new byte[] { 1, 2, 3 } });
        string hashB = ArmyProgressionRules.ComputeFingerprint(new[] { "{\"gold\":200}" }, new[] { new byte[] { 1, 2, 3 } });
        Check(hashA != hashB && hashA.Length == 64, "Distinct saves produce distinct SHA256 fingerprints");

        ArmyProgressionRules.BindSnapshot(index, hashA, state, utcTicks: 1000);
        // Advance state further and bind to branch B
        state.Commanders[3].Xp = 950;
        ArmyProgressionRules.BindSnapshot(index, hashB, state, utcTicks: 2000);

        string json = ArmyProgressionRules.SerializeIndex(index);
        var reloaded = ArmyProgressionRules.ParseIndex(json);
        var branchA = ArmyProgressionRules.ResolveSnapshot(reloaded, hashA);
        var branchB = ArmyProgressionRules.ResolveSnapshot(reloaded, hashB);
        var unknown = ArmyProgressionRules.ResolveSnapshot(reloaded, "0000");
        Check(branchA.Commanders[3].Xp == 184, "Reloading older save restores exact pre-branch XP");
        Check(branchB.Commanders[3].Xp == 950, "Branch B preserves independent Marshal XP");
        Check(unknown.Commanders.Count == 0, "Unknown/vanilla save starts cleanly at 0 XP");

        // 6. Guild Menu navigation for Roster, Directive, Codex
        var menu = new GuildMenuNavigation { ArmyRosterCount = 3 };
        GuildChoice Confirm() { menu.Confirm(false, false, GuildQuestState.NoContract, true); return menu.Confirm(true, true, GuildQuestState.NoContract, true); }
        menu.Open(false, 10);
        menu.Move(false, true); // Training
        Confirm();
        for (int i = 0; i < GuildMenuNavigation.RosterRow; i++) menu.Move(false, true);
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.ArmyRoster, "Roster row opens ArmyRoster page");
        Check(Confirm() == GuildChoice.CycleDoctrine, "Confirm inside ArmyRoster cycles Doctrine");
        Check(menu.Back() && menu.Page == GuildPage.Training && menu.Focus == GuildMenuNavigation.RosterRow, "Back from ArmyRoster restores RosterRow focus");
        menu.Move(false, true); // DirectiveRow
        Check(Confirm() == GuildChoice.CycleDirective && menu.Page == GuildPage.Training, "DirectiveRow cycles Directive in place");
        menu.Move(false, true); // CodexRow
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.Codex, "CodexRow opens Tactician's Codex");
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.Training && menu.Focus == GuildMenuNavigation.CodexRow, "Codex confirm returns to Training");

        Console.WriteLine($"PASS: Army Progression {checks} checks (ranks, doctrines, directives, synergy, content-addressed ledger, Guild menu).");
    }
}
