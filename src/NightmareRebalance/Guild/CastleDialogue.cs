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

    internal bool Active => Script != null;
    internal bool AtChoices => Script != null && PageIndex >= Script.Pages.Length - 1 && Script.Choices.Length > 0;
    internal string CurrentPage => Script != null && PageIndex >= 0 && PageIndex < Script.Pages.Length ? Script.Pages[PageIndex] : "";

    internal void Open(DialogueScript script)
    {
        Script = script;
        PageIndex = 0;
        ChoiceFocus = 0;
        ConfirmReleased = false;
    }

    internal void Close()
    {
        Script = null;
        PageIndex = 0;
        ChoiceFocus = 0;
        ConfirmReleased = false;
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

        if (PageIndex < Script.Pages.Length - 1)
        {
            PageIndex++;
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
        _ => new("Guild Attendant", "Dunan Castle Guild", new[] { "Welcome to the Castle Guild. How may I assist you?" },
            new[] { new DialogueChoiceSpec("Leave", "close") })
    };

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

        _ => new("War Chronicler Orin", "Castle Historian", new[]
        {
            "In the War Room, pairing old comrades—Flik with Viktor, Shu with Apple, Luc with Mazus, or Humphrey with Georg—ignites War Synergies.",
            "You can also retrain enlisted commanders into Infantry, Archer, or Magician squads before launching Custom Skirmishes I through X.",
            "Whenever you save at an inn or crystal—or ask me directly—I preserve your entire legacy in chronicle.md!"
        }, new[] { new DialogueChoiceSpec("Open War Squads Builder", "open-warsquads"), new DialogueChoiceSpec("Leave", "close") })
    };
}
