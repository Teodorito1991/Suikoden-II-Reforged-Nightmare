namespace NightmareRebalance;

internal sealed record DialogueChoiceSpec(string Label, string ActionId);

internal sealed record DialogueScript(string Speaker, string RoleTitle, string[] Pages, DialogueChoiceSpec[] Choices);

// Pure state machine and reactive dialogue scripts for Castle Hub NPCs.
internal sealed class CastleDialogueSession
{
    internal DialogueScript? Script { get; private set; }
    internal int PageIndex { get; private set; }
    internal int ChoiceFocus { get; private set; }
    internal bool ConfirmReleased { get; private set; }
    long pageOpenedAtMs;
    bool lineRevealed;

    internal bool Active => Script != null;
    internal bool AtChoices => Script != null && PageIndex >= Script.Pages.Length - 1 && Script.Choices.Length > 0 && IsPageFullyRevealed();
    internal string CurrentPage => Script != null && PageIndex >= 0 && PageIndex < Script.Pages.Length ? Script.Pages[PageIndex] : "";

    internal bool IsPageFullyRevealed()
    {
        if (lineRevealed) return true;
        string full = CurrentPage;
        if (full.Length == 0) return true;
        long elapsed = Math.Max(0, Environment.TickCount64 - pageOpenedAtMs);
        if (elapsed * 65 / 1000 >= full.Length) { lineRevealed = true; return true; }
        return false;
    }

    internal string VisiblePageText()
    {
        string full = CurrentPage;
        if (lineRevealed || full.Length == 0) return full;
        long elapsed = Math.Max(0, Environment.TickCount64 - pageOpenedAtMs);
        int chars = (int)Math.Min(full.Length, Math.Max(1, elapsed * 65 / 1000));
        if (chars >= full.Length) lineRevealed = true;
        return full[..chars];
    }

    internal void Open(DialogueScript script, bool instantReveal = false)
    {
        Script = script;
        PageIndex = 0;
        ChoiceFocus = 0;
        ConfirmReleased = false;
        pageOpenedAtMs = Environment.TickCount64;
        lineRevealed = instantReveal;
    }

    internal void Close()
    {
        Script = null;
        PageIndex = 0;
        ChoiceFocus = 0;
        ConfirmReleased = false;
        lineRevealed = false;
    }

    internal void Move(bool up, bool down)
    {
        if (!AtChoices || Script == null) return;
        ChoiceFocus = GuildMenuNavigation.MoveFocus(ChoiceFocus, Script.Choices.Length, up, down);
    }

    internal string? StepConfirm(bool pressed, bool held)
    {
        if (!Active || Script == null) return null;
        if (!pressed)
        {
            if (!held) ConfirmReleased = true;
            return null;
        }
        if (!ConfirmReleased) return null;
        ConfirmReleased = false;

        if (!IsPageFullyRevealed())
        {
            lineRevealed = true;
            return null;
        }
        if (PageIndex < Script.Pages.Length - 1)
        {
            PageIndex++;
            pageOpenedAtMs = Environment.TickCount64;
            lineRevealed = false;
            return null;
        }
        if (Script.Choices.Length == 0)
        {
            Close();
            return "close";
        }
        string action = Script.Choices[Math.Clamp(ChoiceFocus, 0, Script.Choices.Length - 1)].ActionId;
        return action;
    }
}

internal static class CastleDialogueScripts
{
    internal static DialogueScript Build(CastleNpcRole role, ArmyLedgerState ledger, bool towerPendingDraft, int towerFloor) => role switch
    {
        CastleNpcRole.TowerKeeper => BuildKaelen(ledger, towerPendingDraft, towerFloor),
        CastleNpcRole.FieldScout => BuildLyra(ledger),
        CastleNpcRole.WarChronicler => BuildOrin(ledger),
        CastleNpcRole.SuperbossSeer => BuildHoodedStranger(ledger),
        _ => new("Guild Attendant", "Dunan Castle Guild", new[] { "Welcome to the Castle Guild. How may I assist you?" },
            new[] { new DialogueChoiceSpec("Leave", "close") })
    };

    static DialogueScript BuildHoodedStranger(ArmyLedgerState ledger)
    {
        var defeated = ledger.DefeatedSuperbosses ?? new HashSet<string>(StringComparer.Ordinal);
        int cleared = SuperbossRules.All.Count(s => defeated.Contains(s.Id));
        var pages = new List<string>(2);
        if (cleared >= SuperbossRules.All.Length)
        {
            pages.Add("Impossible... You have broken all four forbidden seals and stand as the Sovereign of Dunan!");
            pages.Add("Even True Luca, the Omega Beast Rune, and the Sindar Lords fell before you. Yet their echoes remain should you wish to test your steel again.");
        }
        else if (cleared > 0)
        {
            pages.Add($"So... you survived. {cleared}/{SuperbossRules.All.Length} forbidden seals have already yielded to your blade.");
            pages.Add("Each vanquished Secret Superboss leaves behind a permanent Trophy that empowers your entire party. Which seal will you break next?");
        }
        else
        {
            pages.Add("...You have the eyes of someone who finds ordinary battles too tame. Good.");
            pages.Add("Hidden beyond the Hall of Echoes lie four forbidden seals—True Luca Blight, the Omega Beast Rune, Eternal Neclord, and the Awakened Bone Dragon. Break a seal, and claim their permanent Trophy.");
        }

        var choices = new List<DialogueChoiceSpec>(7);
        string sajahTag = ledger.CirculatingCycleRank > 0 ? $" [Cycle {ledger.CirculatingCycleRank}]" : " [Walkable Sindar Ruins]";
        choices.Add(new DialogueChoiceSpec($"Enter Tower of Circulating Paths{sajahTag}", "warp-sajah"));
        foreach (var s in SuperbossRules.All)
        {
            string tag = defeated.Contains(s.Id) ? " [CLEARED]" : " [SEALED]";
            choices.Add(new DialogueChoiceSpec($"{s.Name}{tag}", "confirm-superboss:" + s.Id));
        }
        choices.Add(new DialogueChoiceSpec("Inspect Superboss Trophies", "lore-superboss"));
        choices.Add(new DialogueChoiceSpec("Step Back", "close"));
        return new("Hooded Stranger", "Keeper of Forbidden Seals", pages.ToArray(), choices.ToArray());
    }

    internal static DialogueScript CirculatingPathsScript(int floor, int orbs, bool alcoveSearched, bool shrineVisited, bool guardianDefeated, bool summitReady, int cycleRank, string? statusNote = null)
    {
        if (summitReady)
        {
            return new("Rune of Circulating Paths", $"Sajah Summit · Cycle {cycleRank}", new[]
            {
                "A radiant violet crystal floats above the Sanctum altar, humming with the power of the cut Sajah spire.",
                "\"You who have walked the six halls and gathered the eight crimson orbs... Do you seek a new path? Embrace the Cycle to permanently empower your army (+5% Party DMG & +15% Potch per Cycle, +100,000 Potch, +25 Commendations).\""
            }, new[]
            {
                new DialogueChoiceSpec("Embrace the New Cycle (Cycle +1 Blessing)", "sajah-cycle-yes"),
                new DialogueChoiceSpec("Claim the Sindar Vault (+75,000 Potch & +15 Commendations)", "sajah-cycle-vault"),
                new DialogueChoiceSpec("Return to Castle HQ", "close")
            });
        }

        var spec = CirculatingPathsRules.Floor(floor);
        string bar = CirculatingPathsRules.OrbProgressBar(orbs);
        var pages = new List<string>(2);
        if (!string.IsNullOrEmpty(statusNote)) pages.Add(statusNote);
        pages.Add($"{spec.Atmosphere}\nRed Orbs: {bar} ({orbs}/{CirculatingPathsRules.TotalRedOrbs})  ·  Guardian: {spec.GuardianName} ({spec.GuardianMechanicName})");

        var choices = new List<DialogueChoiceSpec>(6);
        if (!alcoveSearched && spec.OrbsOnFloor > 0)
            choices.Add(new DialogueChoiceSpec($"Explore Sindar Puzzle (+{spec.OrbsOnFloor} Red Orb(s) & Cache)", "sajah-puzzle"));
        if (!shrineVisited)
        {
            string cost = spec.ShrinePotchCost > 0 ? $"{spec.ShrinePotchCost:N0} Potch" : $"{spec.ShrineCommCost} Comm.";
            choices.Add(new DialogueChoiceSpec($"Secret Chamber: {spec.ShrineTitle} ({cost})", "sajah-shrine"));
        }
        if (!guardianDefeated)
            choices.Add(new DialogueChoiceSpec($"Fight Guardian: {spec.GuardianName} [{spec.GuardianMechanicName}]", "sajah-guardian"));
        else
            choices.Add(new DialogueChoiceSpec(floor < CirculatingPathsRules.TotalFloors ? $"Ascend to Floor {floor + 1}/6" : "Approach the Violet Crystal", "sajah-ascend"));
        choices.Add(new DialogueChoiceSpec("Read Ancient Sindar Inscription", "sajah-lore"));
        choices.Add(new DialogueChoiceSpec("Return to Castle (Progress Saved)", "close"));
        return new(spec.Name, $"Tower of Circulating Paths · Floor {floor}/6", pages.ToArray(), choices.ToArray());
    }

    internal static DialogueScript CirculatingPuzzleScript(int floor)
    {
        var spec = CirculatingPathsRules.Floor(floor);
        return new($"Sindar Puzzle — {spec.ShortTag}", $"Tower of Circulating Paths · Floor {floor}/6", new[]
        {
            spec.PuzzlePrompt
        }, new[]
        {
            new DialogueChoiceSpec(spec.PuzzleChoiceSafe, "sajah-puzzle-safe"),
            new DialogueChoiceSpec(spec.PuzzleChoiceRisky + " (Risky · Bonus Potch)", "sajah-puzzle-risky"),
            new DialogueChoiceSpec("Step Back", "open-sajah")
        });
    }

    internal static DialogueScript SuperbossConfirmScript(string id, ArmyLedgerState ledger)
    {
        var spec = SuperbossRules.Find(id);
        if (spec == null) return BuildHoodedStranger(ledger);
        bool done = ledger.DefeatedSuperbosses != null && ledger.DefeatedSuperbosses.Contains(spec.Id);
        string rewardLine = done
            ? $"Trophy already claimed ({spec.TrophyName}: {spec.TrophySummary}). Rematch awards +3 Bonus Commendations."
            : $"First-Clear Reward: {spec.TrophyName} ({spec.TrophySummary}), +{spec.FirstClearPotch:N0} Potch & +{spec.FirstClearCommendations} Commendations.";
        return new("Hooded Stranger", spec.Subtitle, new[]
        {
            spec.SealWarning,
            rewardLine
        }, new[]
        {
            new DialogueChoiceSpec($"Break the Seal — Challenge {spec.Name}", "launch-superboss:" + spec.Id),
            new DialogueChoiceSpec("Choose Another Seal", "back-superboss"),
            new DialogueChoiceSpec("Leave", "close")
        });
    }

    static DialogueScript BuildKaelen(ArmyLedgerState ledger, bool pendingDraft, int activeFloor)
    {
        var pages = new List<string>(3);
        if (pendingDraft)
        {
            pages.Add("Ah, you return victorious! The Tower's echoes have crystallized into three Relics awaiting your choice.");
            pages.Add("Step into the draft before ascending further—or claim your spoils now while fortune favors you.");
        }
        else if (activeFloor > 0)
        {
            pages.Add($"The Tower of Dunan remembers your ascent—Floor {activeFloor} stands cleared, and deeper echoes stir above.");
            pages.Add("Every fifth floor guards a Warden whose defeat grants a Legendary Relic. Push onward, or cash out safely.");
        }
        else if (ledger.HighestTowerFloor >= 5)
        {
            pages.Add($"Few warriors ever break past the Floor 5 Warden. Your record stands at Floor {ledger.HighestTowerFloor}—impressive indeed.");
            pages.Add("If you seek a fiercer trial, swear the Blood Oath before ascending for 1.5x Potch and bonus Commendations.");
        }
        else
        {
            pages.Add("Beyond this corridor lies the Tower of Dunan—an endless gauntlet of remembered foes and ancient Relics.");
            pages.Add("Each cleared floor lets you draft a Relic for that run. Fall in battle, and the run ends—know when to Cash Out!");
        }
        return new("Tower Keeper Kaelen", "Keeper of the Endless Spire", pages.ToArray(), new[]
        {
            new DialogueChoiceSpec(pendingDraft ? "Draft Tower Relic" : "Enter Tower of Dunan", "open-tower"),
            new DialogueChoiceSpec("Ask about Warden Relics", "lore-tower"),
            new DialogueChoiceSpec("Leave", "close")
        });
    }

    static DialogueScript BuildLyra(ArmyLedgerState ledger)
    {
        var pages = new List<string>(3);
        if (ledger.ActiveNemesis is {} nem)
        {
            pages.Add($"Commander! Urgent word from the border scouts—{nem.Name} {nem.Title} (Tier {nem.Tier}) is hunting our patrols!");
            pages.Add($"It carries the {Reforged2Rules.ApexAffixShort(nem.Affix)} affix. Bring it down to claim +{Reforged5Rules.NemesisCommendationReward(nem.Tier)} Commendations and 2.5x Potch!");
        }
        else
        {
            var bounty = Reforged2Rules.ActiveBounty(ledger.ActiveBountyIndex);
            var rank = Reforged2Rules.HunterRankForClears(ledger.HuntsCompleted);
            pages.Add($"Good to see you, {Reforged2Rules.HunterRankTitle(rank)}. The roads around Dunan have grown far more dangerous lately.");
            pages.Add($"Active Field Bounty: {bounty.Title} ({ledger.ActiveBountyProgress}/{bounty.Target}). Keep your squad's Talents sharp!");
        }
        return new("Field Scout Lyra", "Dunan Recon Corps", pages.ToArray(), new[]
        {
            new DialogueChoiceSpec("Open Party Talents (108 Stars)", "open-talents"),
            new DialogueChoiceSpec("Ask about Shields & Nightmare Tactics", "lore-tactics"),
            new DialogueChoiceSpec("Leave", "close")
        });
    }

    static DialogueScript BuildOrin(ArmyLedgerState ledger)
    {
        var pages = new List<string>(3);
        var topCmd = ledger.Commanders.Values.OrderByDescending(c => c.Xp).FirstOrDefault();
        int honors = ledger.UnlockedHonors?.Count ?? 0;
        int species = ledger.Bestiary?.Count ?? 0;
        if (topCmd != null)
        {
            string rankTitle = ArmyProgressionRules.RankTitle(ArmyProgressionRules.RankForXp(topCmd.Xp));
            pages.Add($"The ink barely dries before new deeds arrive! {rankTitle} {topCmd.Name}'s exploits already inspire the garrison.");
        }
        else
        {
            pages.Add("I chronicle the rise of the Dunan Army—every skirmish won, every beast catalogued, every honor earned.");
        }
        pages.Add($"Our records show {honors}/{Reforged2Rules.Honors.Length} Dunan Honors unlocked and {species} species logged in the Bestiary.");
        int sbCleared = ledger.DefeatedSuperbosses?.Count ?? 0;
        if (sbCleared >= SuperbossRules.All.Length)
            pages.Add("The bards will sing of this for a thousand years—you broke all four Forbidden Seals and claimed the title Sovereign of Dunan!");
        else if (sbCleared > 0)
            pages.Add($"Whispers spread through the keep! You have vanquished {sbCleared}/{SuperbossRules.All.Length} of the Hooded Stranger's Secret Superbosses.");
        return new("War Chronicler Orin", "Castle Historian", pages.ToArray(), new[]
        {
            new DialogueChoiceSpec("Open War Squads Builder", "open-warsquads"),
            new DialogueChoiceSpec("Export Campaign Chronicle", "export-chronicle"),
            new DialogueChoiceSpec("Hear a Dunan War Tale", "lore-war"),
            new DialogueChoiceSpec("Leave", "close")
        });
    }

    internal static DialogueScript LoreScript(string actionId) => actionId switch
    {
        "lore-tower" => new("Tower Keeper Kaelen", "Keeper of the Endless Spire", new[]
        {
            "Standard Relics stack across a single ascent—pair Vampiric Fang with Berserkers, or Arcane Battery with an Arcane Conclave.",
            "Every fifth floor, a Tower Warden drops a Legendary Relic: Bright Shield Echo, Black Sword Echo, or True Rune Surge.",
            "Remember: Tower Relics vanish once you Cash Out or fall, so never gamble more floors than your party can survive!"
        }, new[] { new DialogueChoiceSpec("Enter Tower of Dunan", "open-tower"), new DialogueChoiceSpec("Leave", "close") }),

        "lore-tactics" => new("Field Scout Lyra", "Dunan Recon Corps", new[]
        {
            "Tough foes now raise Guard Shields (◆) that cut your damage by 35%. Strike their Weakness or land Combos to shatter 2 shields at once!",
            "Once broken, a STAGGERED foe takes +50% damage, deals -40% damage, and loses any Charged Devastation attack.",
            "Never ignore Defend! Guarding an incoming hit heals 10% max HP, grants +20 Momentum, and cuts Charged attacks by 65%."
        }, new[] { new DialogueChoiceSpec("Open Party Talents", "open-talents"), new DialogueChoiceSpec("Leave", "close") }),

        "lore-superboss" => new("Hooded Stranger", "Forbidden Seal Trophies", new[]
        {
            "Crown of the Mad Prince (True Luca): Permanent +10% Party Phys & Combo DMG, +15% Momentum Gain.",
            "Echo of the 27th Rune (Omega Beast): Permanent +12% Party Magic/Rune DMG & -10% All DMG Taken.",
            "Chalice of Eternal Night (Neclord): +8% Party Lifesteal.  Fang of the Sindar (Bone Dragon): +12% All Party DMG & +1 Combo Shield Break."
        }, new[] { new DialogueChoiceSpec("Return to the Seals", "back-superboss"), new DialogueChoiceSpec("Leave", "close") }),

        _ => new("War Chronicler Orin", "Castle Historian", new[]
        {
            "In the War Room, pairing old comrades—Flik with Viktor, Shu with Apple, Luc with Mazus, or Humphrey with Georg—ignites War Synergies.",
            "You can also retrain enlisted commanders into Infantry, Archer, or Magician squads before launching Custom Skirmishes I through X.",
            "Whenever you save at an inn or crystal—or ask me directly—I preserve your entire legacy in chronicle.md!"
        }, new[] { new DialogueChoiceSpec("Open War Squads Builder", "open-warsquads"), new DialogueChoiceSpec("Leave", "close") })
    };
}
