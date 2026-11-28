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

        Check(Reforged2Rules.Skirmishes.Length == 10, "10 Custom Skirmish scenarios defined");
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

        // 12. Reforged 2.5: Momentum/Overdrive, Execute, Riposte, Affixes, Castle Upgrades, Mutators, Grade, Streak, Bestiary & Bounties
        int mom = Reforged2Rules.AddMomentum(75, 25, out bool triggeredOd);
        Check(mom == 100 && triggeredOd, "Momentum reaches 100 and triggers Dunan Overdrive");
        Check(Reforged2Rules.ApplyOverdriveAttack(100) == 120 && Reforged2Rules.ApplyOverdriveDefense(100) == 85, "Overdrive +20% ATK / -15% damage taken");
        var exec = Reforged2Rules.EvaluateExecuteFinisher(100, 18, 100);
        Check(exec.Name == "Execute Finisher" && exec.BonusDamage == 15, "Execute Finisher +15% on targets <= 20% HP");
        Check(Reforged2Rules.EvaluateExecuteFinisher(100, 21, 100).BonusDamage == 0, "Execute Finisher does not proc above 20% HP");
        Check(Reforged2Rules.ShouldArmRiposte(20, 80, 100) && !Reforged2Rules.ShouldArmRiposte(19, 81, 100), "Riposte arms on heavy hit >= 20% max HP");
        Check(Reforged2Rules.ApplyRiposte(100) == 120, "Riposte grants +20% counter damage");
        Check(Reforged2Rules.WeaponPerk(6, masterBlacksmiths: true).TierName == "Keen Edge" && Reforged2Rules.WeaponPerk(14, masterBlacksmiths: true).TierName == "Masterwork", "Master Blacksmiths shifts weapon perks 2 levels earlier");
        Check(Reforged2Rules.ApexAffixDamageMultiplier(ApexAffix.Frenzied) == 1.10 && Reforged2Rules.ApexAffixPotchMultiplier(ApexAffix.Golden) == 2.50, "Apex Elite Affix multipliers");
        Check(Reforged2Rules.ApplyApexIncomingReduction(100, ApexAffix.Armored, false) == 90 && Reforged2Rules.ApplyApexIncomingReduction(100, ApexAffix.WardBlessed, true) == 90, "Armored & Ward-Blessed Apex reductions");
        Check(Reforged2Rules.BattleGrade(0, 2, 0, 0) == "S" && Reforged2Rules.BattleGrade(0, 5, 0, 0) == "A" && Reforged2Rules.BattleGrade(1, 3, 2, 0) == "B" && Reforged2Rules.BattleGrade(2, 3, 0, 0) == "C", "Battle Grade S/A/B/C calculation");
        Check(Reforged2Rules.StreakPotchMultiplier(4) == 1.0 && Reforged2Rules.StreakPotchMultiplier(5) == 1.10 && Reforged2Rules.StreakPotchMultiplier(10) == 1.20 && Reforged2Rules.StreakStartingMomentum(10) == 10, "Victory Streak Potch & Momentum bonuses");
        var bloodMoonFlik = Reforged2Rules.ApplyMutator(flik, WarMutator.BloodMoon);
        Check(bloodMoonFlik.Attack == flik.Attack + 2 && bloodMoonFlik.Defense == flik.Defense - 1, "Blood Moon War Mutator +2 ATK / -1 DEF");
        Check(Reforged2Rules.BestiaryMilestonesFor(9) == 0 && Reforged2Rules.BestiaryMilestonesFor(10) == 1 && Reforged2Rules.BestiaryMilestonesFor(25) == 2, "Bestiary Commendation milestones every 10 species");
        Check(Reforged2Rules.CastleUpgrades.Length == 8 && Reforged2Rules.FieldBounties.Length == 5, "8 Castle Upgrades and 5 rotating Field Bounties");
        menu.OpenDirect(GuildPage.Codex, 10);
        for (int i = 0; i < 4; i++) menu.Move(false, true);
        Check(Confirm() == GuildChoice.CycleMutator, "Codex row 4 cycles War Mutator");
        var ledger = new ArmyLedgerState();
        ledger.CastleUpgrades.Add("field-medic-corps");
        ledger.Bestiary["m01"] = new BestiaryEntry { Name = "Highland Soldier", Kills = 3, HighestLevel = 12, WeaknessHint = "Balanced" };
        var clonedLedger = ledger.Clone();
        Check(clonedLedger.CastleUpgrades.Contains("field-medic-corps") && clonedLedger.Bestiary["m01"].Kills == 3, "Ledger Clone preserves CastleUpgrades and Bestiary");

        // 13. Reforged 3.0: Elemental Overload, First Strike, Boss Enrage, 10 Dunan Honors & Tier II Upgrades
        var overload = Reforged2Rules.EvaluateArcaneChain(100, spellsCastThisTurn: 3);
        Check(overload.Name == "Elemental Overload" && overload.BonusDamage == 30, "3rd+ spell in a turn escalates to Elemental Overload (+30%)");
        var fs = Reforged2Rules.EvaluateFirstStrike(100, turn: 0, firstActionUsed: false);
        Check(fs.Name == "First Strike" && fs.BonusDamage == 12, "First Strike grants +12% opener bonus on turn 0/1");
        Check(Reforged2Rules.EvaluateFirstStrike(100, turn: 0, firstActionUsed: true).BonusDamage == 0, "First Strike only triggers once per battle");
        Check(Reforged2Rules.Honors.Length == 15, "15 Dunan Honors defined");
        ledger.TotalDrillsWon = 5;
        ledger.ApexPacksCleared = 3;
        int commBefore = ledger.Commendations;
        var honorsUnlocked = Reforged2Rules.EvaluateNewHonors(ledger, currentWinStreak: 10);
        Check(honorsUnlocked.Count == 4 && ledger.Commendations == commBefore + 8, "Dunan Honors auto-unlock and award +2 Commendations each");
        Check(Reforged2Rules.EvaluateNewHonors(ledger, currentWinStreak: 10).Count == 0, "Dunan Honors do not double-award");
        Check(Reforged2Rules.BossEnrageDamageMultiplier == 1.10 && Reforged2Rules.BossEnragePotchMultiplier == 1.15, "Boss Enrage 25% HP mechanical multipliers");

        // 14. Reforged 3.5: Armor Break Focus Fire, Formation Synergies, Grand Armory & Tier III Honors
        Check(Reforged2Rules.ApplyArmorBreak(100, 1) == 105 && Reforged2Rules.ApplyArmorBreak(100, 3) == 115 && Reforged2Rules.ApplyArmorBreak(100, 5) == 115, "Armor Break stacks 1..3 (+5%..+15%)");
        Check(Reforged2Rules.ApplyPhalanxDefense(100, 3) == 94 && Reforged2Rules.ApplyPhalanxDefense(100, 2) == 100, "Phalanx front-row defense requires 3 living front allies");
        Check(Reforged2Rules.ApplyCrossfireAttack(100, 3) == 106 && Reforged2Rules.ApplyCrossfireAttack(100, 2) == 100, "Crossfire back-row offense requires 3 living back allies");
        Check(Reforged2Rules.ApplyGrandArmory(100, true) == 105 && Reforged2Rules.ApplyGrandArmory(100, false) == 100, "Grand Armory permanent +5% damage");
        ledger.TotalCombosLanded = 25;
        ledger.TotalOverdrivesTriggered = 10;
        ledger.TotalSRanks = 15;
        var tier3Honors = Reforged2Rules.EvaluateNewHonors(ledger, currentWinStreak: 0);
        Check(tier3Honors.Count == 3, "Combo Master, Overdrive Legend & S-Rank Virtuoso honors unlock");

        // 15. Reforged 4.0: Spellblade Resonance & Last Stand Aura
        var sb = Reforged2Rules.EvaluateSpellbladeResonance(100, spellCastThisTurn: true);
        Check(sb.Name == "Spellblade Resonance" && sb.BonusDamage == 12, "Spellblade Resonance +12% physical follow-up after spell");
        Check(Reforged2Rules.EvaluateSpellbladeResonance(100, spellCastThisTurn: false).BonusDamage == 0, "Spellblade Resonance requires a spell cast this turn");
        Check(!Reforged2Rules.IsLastStandActive(2) && Reforged2Rules.IsLastStandActive(3), "Last Stand Aura activates at 3+ KO allies");
        Check(Reforged2Rules.ApplyLastStandAttack(100, 3) == 115 && Reforged2Rules.ApplyLastStandDefense(100, 3) == 90, "Last Stand Aura grants +15% ATK / -10% damage taken");

        // 16. Reforged 5.0: Character Talents, War Branch Override, Nemesis System & Endless Tower of Dunan
        int zerkDmg = Reforged5Rules.ApplyTalentPhysical(100, CharacterTalent.Berserker, 3, out int lifesteal);
        Check(zerkDmg == 114 && lifesteal == 6, "Berserker Rank III+ grants +14% physical DMG and 5% lifesteal");
        Check(Reforged5Rules.ApplyTalentMagic(100, CharacterTalent.Arcanist, 3) == 116, "Arcanist Rank III+ grants +16% magic DMG");
        Check(Reforged5Rules.ApplyTalentDefense(100, CharacterTalent.Bulwark, 3) == 86, "Bulwark Rank III+ reduces damage taken by 14%");
        Check(Reforged5Rules.MomentumBonusForTalent(20, CharacterTalent.Tactician) == 30, "Tactician generates +50% extra Momentum");
        var archerFlik = Reforged5Rules.ApplyBranchOverride(flik, WarBranchOverride.Archer);
        Check(archerFlik.Branch == "Archer", "Custom War Branch Override reassigns commander branch");
        var nem1 = Reforged5Rules.PromoteNemesis(null, "m99", "Highland Capt", 1);
        var nem2 = Reforged5Rules.PromoteNemesis(nem1, "m99", "Highland Capt", 2);
        Check(nem1.Tier == 1 && nem2.Tier == 2 && Reforged5Rules.NemesisCommendationReward(2) == 3, "Nemesis promotion escalates tier and Commendation bounty");
        Check(Reforged5Rules.ShouldAmbushWithNemesis(0) && Reforged5Rules.ShouldAmbushWithNemesis(19) && !Reforged5Rules.ShouldAmbushWithNemesis(20), "Nemesis 20% ambush boundary");
        Check(Reforged5Rules.TowerRelics.Length == 12, "12 Roguelite Tower Relics defined");
        var draft = Reforged5Rules.DraftRelics(new[] { "vampiric-fang" }, seed: 42);
        Check(draft.Length == 3 && !draft.Any(r => r.Id == "vampiric-fang"), "Tower Relic draft rolls 3 unique unowned relics");
        Check(Reforged5Rules.TowerCashOutPotch(5, goldenChalice: true) == 3750 && Reforged5Rules.TowerCashOutCommendations(5, goldenChalice: true) == 6, "Tower Cash-Out Potch & Commendations with Golden Chalice");
        menu.OpenDirect(GuildPage.Codex, 10);
        for (int i = 0; i < 6; i++) menu.Move(false, true);
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.PartyTalents, "Codex row 6 opens Party Talents");
        Check(Confirm() == GuildChoice.CycleTalent, "Confirm inside Party Talents cycles character talent");
        Check(menu.Back() && menu.Page == GuildPage.Codex && menu.Focus == 6, "Back from Party Talents restores Codex focus");
        menu.Move(false, true);
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.TowerOfDunan, "Codex row 7 opens Tower of Dunan");
        Check(Confirm() == GuildChoice.TowerAction, "Confirm inside Tower triggers TowerAction");

        // 17. Reforged 6.0: Party Talent Sets, Tower Warden Legendary Relics, Blood Oath, Nemesis Escalation, War Squads & Chronicle
        Check(Reforged5Rules.EvaluateTalentSet(new[] { CharacterTalent.Berserker, CharacterTalent.Arcanist, CharacterTalent.Bulwark, CharacterTalent.Tactician }) == TalentSetBonus.BalancedVanguard, "Balanced Vanguard set bonus activates with 1 of each Talent");
        Check(Reforged5Rules.EvaluateTalentSet(new[] { CharacterTalent.Berserker, CharacterTalent.Berserker, CharacterTalent.Berserker }) == TalentSetBonus.WarbandFury, "Warband Fury activates with 3+ Berserkers");
        Check(Reforged5Rules.EvaluateTalentSet(new[] { CharacterTalent.Arcanist, CharacterTalent.Arcanist, CharacterTalent.Arcanist }) == TalentSetBonus.ArcaneConclave, "Arcane Conclave activates with 3+ Arcanists");
        Check(Reforged5Rules.EvaluateTalentSet(new[] { CharacterTalent.Bulwark, CharacterTalent.Bulwark, CharacterTalent.Bulwark }) == TalentSetBonus.IronCitadel, "Iron Citadel activates with 3+ Bulwarks");
        Check(Reforged5Rules.IsWardenFloor(5) && !Reforged5Rules.IsWardenFloor(4) && Reforged5Rules.LegendaryRelics.Length == 3, "Warden Floors every 5th floor with 3 Legendary Relics");
        var wardenDraft = Reforged5Rules.DraftRelicsForFloor(5, Array.Empty<string>(), 1);
        Check(wardenDraft.Length == 3 && wardenDraft.All(r => r.Name.Contains("★")), "Warden Floor 5 drafts Legendary Relics");
        Check(Reforged5Rules.TowerCashOutPotch(5, false, bloodOath: true) == 3750 && Reforged5Rules.TowerCashOutCommendations(5, false, bloodOath: true) == 7, "Blood Oath grants 1.5x Potch and +2 bonus Commendations");
        Check(Reforged5Rules.NemesisAmbushChance(0) == 20 && Reforged5Rules.NemesisAmbushChance(3) == 50 && Reforged5Rules.NemesisAmbushChance(10) == 80, "Nemesis ambush chance escalates 20% -> 80%");
        string chronicle = Reforged5Rules.BuildChronicleMarkdown(ledger);
        Check(chronicle.Contains("# Suikoden II: Reforged Nightmare") && chronicle.Contains("Dunan Honors"), "Campaign Chronicle exports valid Markdown summary");
        menu.OpenDirect(GuildPage.Codex, 10);
        for (int i = 0; i < 5; i++) menu.Move(false, true);
        Check(Confirm() == GuildChoice.ExportChronicle, "Codex row 5 exports Campaign Chronicle");
        for (int i = 0; i < 3; i++) menu.Move(false, true);
        Check(Confirm() == GuildChoice.None && menu.Page == GuildPage.WarSquads, "Codex row 8 opens War Squads Branch Builder");
        Check(Confirm() == GuildChoice.CycleBranch, "Confirm inside War Squads cycles branch override");
        Check(Reforged2Rules.EvaluateExecuteFinisher(100, 35, 100, executionerSeal: true).BonusDamage == 15 && Reforged2Rules.EvaluateExecuteFinisher(100, 36, 100, executionerSeal: true).BonusDamage == 0, "Executioner Seal expands Execute Finisher threshold to <= 35% HP");
        Check(Reforged2Rules.ApplyRiposte(100, bulwark: true) == 130 && Reforged2Rules.ApplyRiposte(100, bulwark: false) == 120, "Bulwark Talent boosts Riposte to +30%");

        // 18. Audit Regression Tests: Large 64+ snapshot ledger (>2MB) & unstackable relic draft exhaustion
        var largeIndex = new ArmyLedgerIndex();
        var bigState = new ArmyLedgerState { Commendations = 42, HighestTowerFloor = 12 };
        for (int e = 0; e < 180; e++)
            bigState.Bestiary[$"mon_{e:D3}"] = new BestiaryEntry { Name = $"Highland Elite Soldier Type {e:D3}", Kills = e + 1, ApexKills = e % 3, HighestLevel = 45, WeaknessHint = "Weak: Magic" };
        for (int s = 0; s < 64; s++)
            ArmyProgressionRules.BindSnapshot(largeIndex, $"SAVEHASH_{s:D4}_ABCDEF0123456789", bigState, s + 1);
        string bigJson = ArmyProgressionRules.SerializeIndex(largeIndex);
        var reloadedLarge = ArmyProgressionRules.ParseIndex(bigJson);
        Check(reloadedLarge.Snapshots.Count == 64 && reloadedLarge.Snapshots["SAVEHASH_0063_ABCDEF0123456789"].Bestiary.Count == 180,
            "64+ populated snapshots serialize and reload cleanly without silent 2MB truncation");
        var allOwnedRelics = Reforged5Rules.TowerRelics.Select(r => r.Id).ToArray();
        Check(Reforged5Rules.DraftRelics(allOwnedRelics, seed: 7).Length == 0,
            "Tower Relic draft never re-offers already-owned unstackable relics");

        // 19. Reforged 7.0: True Nightmare Combat Rework (Shields, Stagger, Charge, Traits, Hazards, Anti-Overleveling, Fatigue)
        Check(NightmareReworkRules.InitialShields(true, false, PackTrait.None) == 5 && NightmareReworkRules.InitialShields(false, true, PackTrait.None) == 3 && NightmareReworkRules.InitialShields(false, false, PackTrait.Shielded) == 2, "Boss/Elite/Shielded Guard Shield counts");
        Check(NightmareReworkRules.ApplyShieldAndStaggerDamage(100, shields: 2, staggered: false) == 65 && NightmareReworkRules.ApplyShieldAndStaggerDamage(100, shields: 0, staggered: true) == 150, "Shielded -35% / Staggered +50% damage math");
        Check(NightmareReworkRules.ShieldBreakAmount(true, false) == 2 && NightmareReworkRules.ShieldBreakAmount(false, false) == 1, "Weakness/Combo breaks 2 shields; normal hit breaks 1");
        Check(NightmareReworkRules.ShouldChargeThisTurn(2, true, false, false) && !NightmareReworkRules.ShouldChargeThisTurn(2, true, false, true), "Boss charges on turn 2 unless Staggered");
        Check(NightmareReworkRules.AntiOverlevelHpMultiplier(30, 20) > 1.4 && NightmareReworkRules.AntiOverlevelHpMultiplier(20, 20) == 1.0, "Anti-overleveling scales underleveled enemies up");
        Check(NightmareReworkRules.BattleFatigueMultiplier(5, ZoneHazard.Calm) == 1.0 && NightmareReworkRules.BattleFatigueMultiplier(8, ZoneHazard.Calm) > 1.15 && NightmareReworkRules.BattleFatigueMultiplier(4, ZoneHazard.BloodMist) > 1.0, "Battle Fatigue ramps on long fights and earlier in Blood Mist");
        Check(NightmareReworkRules.ComputeThornedReflect(100, 50) == 12 && NightmareReworkRules.ComputeThornedReflect(100, 10) == 9, "Thorned reflects 12% and never kills attacker");
        Check(NightmareReworkRules.ComputeVampiricHeal(100) == 35 && NightmareReworkRules.ComputeDefendHeal(200) == 20, "Vampiric 35% leech & Defend 10% heal");

        // 20. Reforged 7.5: Living Castle Hub & Multi-Page NPC Dialogue System
        var kaelenScript = CastleDialogueScripts.Build(CastleNpcRole.TowerKeeper, ledger, towerPendingDraft: true, towerFloor: 3);
        Check(kaelenScript.Speaker == "Tower Keeper Kaelen" && kaelenScript.Pages.Length == 2 && kaelenScript.Choices[0].ActionId == "open-tower", "Kaelen Tower Keeper reactive Relic Draft dialogue");
        ledger.ActiveNemesis = new NemesisRecord { Name = "Highland Capt", Title = "the Unbroken", Tier = 2, Affix = ApexAffix.Frenzied };
        var lyraScript = CastleDialogueScripts.Build(CastleNpcRole.FieldScout, ledger, false, 0);
        Check(lyraScript.Speaker == "Field Scout Lyra" && lyraScript.Pages[0].Contains("Highland Capt the Unbroken"), "Lyra Field Scout reacts dynamically to active Nemesis");
        var orinScript = CastleDialogueScripts.Build(CastleNpcRole.WarChronicler, ledger, false, 0);
        Check(orinScript.Speaker == "War Chronicler Orin" && orinScript.Choices.Any(c => c.ActionId == "export-chronicle"), "Orin War Chronicler dialogue & chronicle export choice");
        var dlg = new CastleDialogueSession();
        dlg.Open(kaelenScript, instantReveal: true);
        Check(dlg.Active && !dlg.AtChoices && dlg.PageIndex == 0, "Dialogue opens at page 0");
        Check(dlg.StepConfirm(false, false) == null, "Releasing confirm unlocks dialogue advance");
        Check(dlg.StepConfirm(true, true) == null && dlg.PageIndex == 1 && !dlg.AtChoices, "Confirm advances to page 1 (typewriter active)");
        Check(dlg.StepConfirm(false, false) == null && dlg.StepConfirm(true, true) == null && dlg.AtChoices, "Confirm fast-completes typewriter and reveals choices");
        dlg.Move(false, true);
        Check(dlg.ChoiceFocus == 1, "Down moves choice cursor");
        Check(dlg.StepConfirm(false, false) == null && dlg.StepConfirm(true, true) == "lore-tower", "Confirm selects highlighted dialogue choice");

        // 21. Reforged 8.0: Secret Superbosses (True Luca, Omega Beast Rune, Eternal Neclord, Awakened Bone Dragon)
        Check(SuperbossRules.All.Length == 4 && SuperbossRules.Find("true-luca") != null && SuperbossRules.Find("omega-beast") != null &&
              SuperbossRules.Find("eternal-neclord") != null && SuperbossRules.Find("awakened-dragon") != null, "All 4 Secret Superbosses registered");
        var lucaSpec = SuperbossRules.Find("true-luca")!;
        Check(SuperbossRules.EvaluatePhase(lucaSpec, 10000, 10000) == 1 && SuperbossRules.EvaluatePhase(lucaSpec, 6000, 10000) == 2 &&
              SuperbossRules.EvaluatePhase(lucaSpec, 2500, 10000) == 3, "True Luca 3-phase HP thresholds (100/65/30%)");
        Check(SuperbossRules.PhaseDamageMultiplier(lucaSpec, 3, 3) > SuperbossRules.PhaseDamageMultiplier(lucaSpec, 3, 2) &&
              SuperbossRules.PhaseDamageMultiplier(lucaSpec, 2, 1) > SuperbossRules.PhaseDamageMultiplier(lucaSpec, 1, 1), "Phase & ultimate turn damage escalation");
        var necSpec = SuperbossRules.Find("eternal-neclord")!;
        Check(SuperbossRules.EvaluatePhase(necSpec, 6000, 10000) == 1 && SuperbossRules.EvaluatePhase(necSpec, 4500, 10000) == 2, "Sindar 2-phase threshold (50%)");
        var strangerScript = CastleDialogueScripts.Build(CastleNpcRole.SuperbossSeer, ledger, false, 0);
        Check(strangerScript.Speaker == "Hooded Stranger" && strangerScript.Choices.Length == 7 && strangerScript.Choices[0].ActionId == "open-sajah" && strangerScript.Choices[1].ActionId == "confirm-superboss:true-luca", "Hooded Stranger offers Sajah Cut Dungeon + all 4 Secret Superbosses + lore");
        var confirmScript = CastleDialogueScripts.SuperbossConfirmScript("omega-beast", ledger);
        Check(confirmScript.Choices[0].ActionId == "launch-superboss:omega-beast", "Superboss seal confirmation launches exact encounter");
        foreach (var bossSpec in SuperbossRules.All) ledger.DefeatedSuperbosses.Add(bossSpec.Id);
        Check(SuperbossRules.AllCleared(ledger.DefeatedSuperbosses) && ledger.Clone().DefeatedSuperbosses.Count == 4, "Superboss victories persist and clone cleanly");
        var clearedStranger = CastleDialogueScripts.Build(CastleNpcRole.SuperbossSeer, ledger, false, 0);
        Check(clearedStranger.Pages[0].Contains("Sovereign of Dunan"), "Hooded Stranger reacts when all 4 Secret Superbosses are vanquished");

        // 22. Reforged 8.5: Cut Sajah Dungeon Restoration ("Tower of Circulating Paths")
        Check(CirculatingPathsRules.Floors.Length == 6 && CirculatingPathsRules.Floors.Sum(f => f.OrbsOnFloor) == 8, "Sajah Cut Dungeon has 6 floors totaling exactly 8 Red Orbs");
        Check(CirculatingPathsRules.OrbProgressBar(3) == "●●●○○○○○" && CirculatingPathsRules.OrbProgressBar(8) == "●●●●●●●●", "Red Orb progress bar renders 0..8 orbs");
        Check(CirculatingPathsRules.CycleDamageBonus(2) > 1.09 && CirculatingPathsRules.CyclePotchMultiplier(2) > 1.29, "Cycle Rank blessings scale party DMG & Potch");
        var floor1Dlg = CastleDialogueScripts.CirculatingPathsScript(1, 0, false, false, false, 0);
        Check(floor1Dlg.Choices.Any(c => c.ActionId == "sajah-search") && floor1Dlg.Choices.Any(c => c.ActionId == "sajah-guardian"), "Floor 1 dialogue offers Sindar Alcove search & Stairwell Guardian");
        var summitDlg = CastleDialogueScripts.CirculatingPathsScript(6, 8, true, true, true, 1);
        Check(summitDlg.Speaker == "Rune of Circulating Paths" && summitDlg.Choices[0].ActionId == "sajah-cycle-yes", "Summit violet crystal offers Rune of Circulating Paths Cycle blessing");
        ledger.CirculatingCycleRank = 2; ledger.CirculatingClears = 2;
        Check(ledger.Clone().CirculatingCycleRank == 2 && ledger.Clone().CirculatingClears == 2, "Circulating Cycle Rank persists & clones cleanly");

        Console.WriteLine($"PASS: Reforged 8.5 full suite {checks} checks.");
    }
}
