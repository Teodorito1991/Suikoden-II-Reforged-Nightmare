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
        menu.Move(false, true); // QuartermasterRow
        menu.Move(false, true); // CodexRow
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.Codex, "CodexRow opens Tactician's Codex");
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.Training && menu.Focus == GuildMenuNavigation.CodexRow, "Codex confirm returns to Training");

        // 7. Menu UX: Left/Right page-flipping, hold-to-repeat timer, NPC banners, semantic tones
        Check(GuildMenuNavigation.MovePageFocus(0, 22, 4, false, true) == 4, "Right flips to page 2");
        Check(GuildMenuNavigation.MovePageFocus(20, 22, 4, false, true) == 0, "Right wraps from final page to page 1");
        Check(GuildMenuNavigation.MovePageFocus(0, 22, 4, true, false) == 20, "Left wraps from page 1 to final page");
        var repeat = new MenuRepeatTimer();
        Check(repeat.Step(true, true), "Initial press fires immediately");
        int repeats = 0;
        for (int f = 0; f < 30; f++) if (repeat.Step(false, true)) repeats++;
        Check(repeats == 2, "Hold-to-repeat fires after initial delay at steady interval");
        repeat.Reset();
        Check(!repeat.Step(false, false), "Release resets repeat timer");

        var (repHeader, repGreeting) = GuildPresentation.NpcBanner(GuildQuestState.AwaitingReport, "sun-kings-retinue", 0, 0, false);
        Check(repHeader.Contains("Report Ready") && repGreeting.Contains("Sun King's Retinue"), "NPC banner highlights completed hunt report");
        var (morHeader, _) = GuildPresentation.NpcBanner(GuildQuestState.NoContract, null, 2, 3, true);
        Check(morHeader.Contains("Morale +2"), "NPC banner highlights armed cross-theater morale");
        Check(GuildRebornScreen.NoteTone("Completed — report to Guild") == "success", "Completed note uses success tone");
        Check(GuildRebornScreen.NoteTone("Unavailable in settings") == "alert", "Unavailable note uses alert tone");
        Check(GuildMenuNavigation.Breadcrumb(GuildPage.ArmyRoster).Contains("Roster"), "Breadcrumb identifies nested Roster page");

        // 8. Multi-NPC Castle Hub, Readiness Strip, Story War XP & Expanded Star Bonds
        var (dsHeader, dsGreeting) = GuildPresentation.NpcBanner(GuildQuestState.NoContract, null, 1, 2, true, CastleNpcRole.DrillSergeant);
        Check(dsHeader.Contains("Drill Sergeant") && dsHeader.Contains("Morale +1") && dsGreeting.Contains("drill"), "Drill Sergeant NPC banner");
        var (eaHeader, eaGreeting) = GuildPresentation.NpcBanner(GuildQuestState.NoContract, null, 0, 0, false, CastleNpcRole.EchoArchivist);
        Check(eaHeader.Contains("Echo Archivist") && eaGreeting.Contains("Hall of Echoes"), "Echo Archivist NPC banner");

        menu.OpenDirect(GuildPage.Training, 10);
        Check(menu.Page == GuildPage.Training && menu.Focus == 0, "Drill Sergeant opens Army Training directly");
        menu.OpenDirect(GuildPage.Echoes, 10);
        Check(menu.Page == GuildPage.Echoes && menu.Focus == 0, "Echo Archivist opens Hall of Echoes directly");

        string ready = GuildPresentation.ReadinessStrip(42, 2, 1);
        Check(ready.Contains("Lv 42") && ready.Contains("2 Star Bonds") && ready.Contains("Morale +1"), "Pre-battle readiness strip formats level, bonds and morale");

        var storyState = new ArmyLedgerState();
        var storyAward = ArmyProgressionRules.AwardStoryWarXp(storyState, new[] { new CommanderParticipation(3, "Flik", true, true) });
        Check(storyAward.CommandersUpdated == 1 && storyState.Commanders[3].Xp == 80 && storyState.TotalStoryWarsWon == 1, "Story war victory awards 80 XP to surviving leader");

        Check(StarBondRules.Catalogue.Length == 10, "Expanded Star Bond catalogue has 10 bonds");
        Check(StarBondRules.Includes(StarBondRules.Catalogue.Single(b => b.Unite == -1), new[] { 1, 18 }), "Riou + Nanami bond matches");
        Check(StarBondRules.Includes(StarBondRules.Catalogue.Single(b => b.Unite == -2), new[] { 2, 3 }), "Flik + Viktor bond matches");
        Check(StarBondRules.Includes(StarBondRules.Catalogue.Single(b => b.Unite == -3), new[] { 53, 61 }), "Luc + Mazus bond matches");
        Check(StarBondRules.Includes(StarBondRules.Catalogue.Single(b => b.Unite == -4), new[] { 36, 45 }), "Kahn + Sierra bond matches");
        Check(StarBondRules.Includes(StarBondRules.Catalogue.Single(b => b.Unite == -5), new[] { 11, 44 }), "Georg + Killey bond matches");
        Check(StarBondRules.Includes(StarBondRules.Catalogue.Single(b => b.Unite == -6), new[] { 73, 63 }), "Kasumi + Mondo bond matches");

        // 9. Hall Mastery Medals, Hunt Risk Modifiers & War Unit Synergy Pairs
        var medalState = new ArmyLedgerState();
        ArmyProgressionRules.RecordHallClear(medalState, "stone-golem", nightmare: false, turns: 6, noKo: true);
        Check(ArmyProgressionRules.HallMedalTag(medalState.HallRecords["stone-golem"]) == "✓ 6T No-KO", "Original Hall medal recorded");
        ArmyProgressionRules.RecordHallClear(medalState, "stone-golem", nightmare: true, turns: 4, noKo: true);
        Check(ArmyProgressionRules.HallMedalTag(medalState.HallRecords["stone-golem"]) == "★ NM 4T No-KO", "Nightmare Hall medal takes display priority");
        ArmyProgressionRules.RecordHallClear(medalState, "stone-golem", nightmare: true, turns: 7, noKo: false);
        Check(medalState.HallRecords["stone-golem"].BestNightmareTurns == 4 && medalState.HallRecords["stone-golem"].NoKoNightmare, "Best turns and No-KO badge are monotonic");
        Check(GuildRebornScreen.NoteTone("★ NM 4T No-KO") == "success", "Hall medal note uses success tone");

        Check(ArmyProgressionRules.HuntModifierStatFactor(HuntRiskModifier.FrenziedQuarry, hp: false) == 1.20, "Frenzied Quarry +20% damage");
        Check(ArmyProgressionRules.HuntModifierStatFactor(HuntRiskModifier.IroncladQuarry, hp: true) == 1.25, "Ironclad Quarry +25% HP");
        Check(ArmyProgressionRules.HuntModifierBonusPotch(2000, HuntRiskModifier.FrenziedQuarry) == 2700, "Hunt Risk Modifier +35% bonus Potch");

        var mercPair = ArmyProgressionRules.FindWarSynergy(3, new[] { 3, 4 });
        Check(mercPair != null && mercPair.Title == "Mercenary Command", "Flik + Viktor war synergy detected");
        var flikSynergy = ArmyProgressionRules.ApplyWarSynergy(flik, mercPair);
        Check(flikSynergy.Attack == flik.Attack + 1 && flikSynergy.AssistAttack == flik.AssistAttack + 1, "War synergy applies bounded bonuses");

        menu.OpenDirect(GuildPage.Codex, 10);
        menu.Move(false, true);
        menu.Move(false, true);
        Check(Confirm() == GuildChoice.CycleHuntModifier, "Codex Hunt row cycles Hunt Risk Modifier");

        // 10. Extended War Scenarios, Custom Skirmishes, Quartermaster, Hunter Rank, Weapon Perks, Combos & Telegraphs
        foreach (int w in new[] { 6, 13 })
        {
            Check(TrainingResultRules.VictoryPotch(w, false, 1, 1, 0, true, true, false) == 750, $"War {w} Standard victory pays 750");
            Check(TrainingResultRules.VictoryPotch(w, true, 1, 4, 0, false, true, false) == 1500, $"War {w} Veteran objective ending pays 1500");
            Check(TrainingResultRules.VictoryPotch(w, true, 2, 1, -1, false, false, false) == 0, $"War {w} defeat pays 0");
        }

        Check(Reforged2Rules.Skirmishes.Length == 5, "5 Custom Skirmish scenarios defined");
        var grandBoost = Reforged2Rules.ApplySkirmishEnemyBoost(flik, Reforged2Rules.Skirmishes[2]);
        Check(grandBoost.Attack == flik.Attack + 2 && grandBoost.Defense == flik.Defense + 2 && grandBoost.Abilities.Contains("SP_CRITICAL"), "Grand Army Skirmish boosts enemy commanders within caps");

        Check(Reforged2Rules.QuartermasterStock.Length == 10, "Quartermaster stocks all 10 audited rare items");
        Check(Reforged2Rules.HunterRankForClears(0) == HunterRank.Unranked && Reforged2Rules.HunterRankForClears(1) == HunterRank.Bronze && Reforged2Rules.HunterRankForClears(15) == HunterRank.Master, "Hunter Rank progression");
        Check(Reforged2Rules.HunterPotchBonusMultiplier(HunterRank.Master) == 1.25, "Master Hunter +25% Potch multiplier");

        Check(Reforged2Rules.ApplyWeaponAttackPerk(100, 7) == 100, "Weapon Lv 7 has no milestone perk");
        Check(Reforged2Rules.ApplyWeaponAttackPerk(100, 8) == 106, "Weapon Lv 8 Keen Edge +6% ATK");
        Check(Reforged2Rules.ApplyWeaponAttackPerk(100, 16) == 114 && Reforged2Rules.ApplyWeaponDefensePerk(100, 16) == 90, "Weapon Lv 16 Masterwork +14% ATK / -10% phys taken");

        var shatter = Reforged2Rules.EvaluatePhysicalCombo(100, targetSleeping: true, targetBalloon: false, targetPoisoned: false);
        Check(shatter.Name == "Shatter Strike" && shatter.BonusDamage == 20, "Shatter Strike combo bonus");
        var chain = Reforged2Rules.EvaluateArcaneChain(100, spellsCastThisTurn: 2);
        Check(chain.Name == "Arcane Chain" && chain.BonusDamage == 18, "Arcane Chain 2nd-spell combo bonus");

        string p1 = Reforged2Rules.BossPhaseBanner("Neclord", 4500, 10000, 0, out int ph1);
        string p2 = Reforged2Rules.BossPhaseBanner("Neclord", 2000, 10000, ph1, out int ph2);
        Check(ph1 == 1 && p1.Contains("fury") && ph2 == 2 && p2.Contains("Desperation"), "Boss phase telegraphs trigger at 50% and 25%");

        menu.OpenDirect(GuildPage.Training, 10);
        for (int i = 0; i < GuildMenuNavigation.QuartermasterRow; i++) menu.Move(false, true);
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.Quartermaster, "QuartermasterRow opens Quartermaster Shop");
        Check(Confirm() == GuildChoice.BuyQuartermaster, "Confirm inside Quartermaster triggers BuyQuartermaster");
        Check(menu.Back() && menu.Page == GuildPage.Training && menu.Focus == GuildMenuNavigation.QuartermasterRow, "Back from Quartermaster restores focus");

        // 11. Apex Encounters, Desperation Surge, Allied War Stances, Weakness Hints & Quartermaster NPC
        Check(Reforged2Rules.ShouldSpawnApex(0) && Reforged2Rules.ShouldSpawnApex(9) && !Reforged2Rules.ShouldSpawnApex(10), "Apex 10% roll boundary");
        Check(Reforged2Rules.ShouldArmDesperation(25, 100) && !Reforged2Rules.ShouldArmDesperation(26, 100) && !Reforged2Rules.ShouldArmDesperation(0, 100), "Desperation Surge arms on surviving <= 25% HP");
        Check(Reforged2Rules.ApplyDesperationSurge(100) == 125, "Desperation Surge +25% damage");
        Check(Reforged2Rules.TacticalWeaknessHint(120, 80) == "Weak: Magic" && Reforged2Rules.TacticalWeaknessHint(80, 120) == "Weak: Phys" && Reforged2Rules.TacticalWeaknessHint(90, 95) == "Balanced", "Tactical weakness hints");
        var assaultFlik = Reforged2Rules.ApplyStance(flik, ArmyStance.AllOutAssault);
        Check(assaultFlik.Attack == flik.Attack + 1 && assaultFlik.Defense == flik.Defense - 1, "All-Out Assault stance +1 ATK / -1 DEF");
        var (qmHeader, qmGreeting) = GuildPresentation.NpcBanner(GuildQuestState.NoContract, null, 0, 0, true, CastleNpcRole.Quartermaster);
        Check(qmHeader == "Castle Quartermaster" && qmGreeting.Contains("Commendations"), "Castle Quartermaster NPC banner");
        menu.OpenDirect(GuildPage.Codex, 10);
        menu.Move(false, true);
        Check(Confirm() == GuildChoice.CycleStance, "Codex row 1 cycles Allied War Stance");

        Console.WriteLine($"PASS: Reforged 2.0 full suite {checks} checks.");
    }
}
