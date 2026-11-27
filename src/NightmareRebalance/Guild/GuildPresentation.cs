namespace NightmareRebalance;

// Player text only. No quest transitions, rewards or native input belong here.
internal static class GuildPresentation
{
    internal static string State(GuildQuestState state) => state switch
    {
        GuildQuestState.Accepted => "Accepted",
        GuildQuestState.AwaitingReport => "Completed — awaiting report",
        GuildQuestState.Reported => "Reported — ready again",
        _ => "Available"
    };
    internal static string Objective(GuildQuestState state, string id) => state switch
    {
        GuildQuestState.Accepted => "Seek your quarry in " + GuildScreenRules.Card(id).Destination + ". Win the hunt, then return here. Escape leaves the contract open.",
        GuildQuestState.AwaitingReport => "The quarry is defeated. Report to the attendant. Reporting records your success; it grants no additional spoils.",
        GuildQuestState.Reported => "Your report is recorded. Take this contract again, or choose another quarry. Hunting is optional and repeatable.",
        _ => "Choose a quarry, prepare your party and travel to its hunting ground. Win, then return to the Guild."
    };
    internal static string Availability(GuildQuestState state, string id, string? active, bool enabled, bool requests)
    {
        bool legacyExperiment = id is "dreamers-circuit" or "thornwake-patrol";
        if (!enabled) return legacyExperiment ? "Experimental — disabled" : "Unavailable in settings";
        if (!requests) return "Requests disabled";
        if (state == GuildQuestState.AwaitingReport) return id == active ? "Completed — report to Guild" : "Report your hunt first";
        if (state == GuildQuestState.Accepted) return id == active ? "Accepted — ready to hunt" : "Finish or withdraw first";
        if (state == GuildQuestState.Reported && id == active) return "Reported — available again";
        return "Open — check travel";
    }
    internal static string Prompt(GuildQuestState state, bool action, bool available) => action
        ? state == GuildQuestState.Accepted ? "Confirm: withdraw" : state == GuildQuestState.AwaitingReport ? "Confirm: report completion" : "Confirm: close"
        : !available ? "Contract unavailable — browse another or close"
        : state == GuildQuestState.Accepted ? "Contract active — choose Withdraw to change it"
        : state == GuildQuestState.AwaitingReport ? "Choose Report below to finish"
        : "Up / Down: browse    Confirm: accept";

    internal static (string Header, string Greeting) NpcBanner(
        GuildQuestState state,
        string? contractId,
        int moraleCharges,
        int trackedCommanders,
        bool progressionEnabled,
        CastleNpcRole role = CastleNpcRole.GuildAttendant)
    {
        if (role == CastleNpcRole.DrillSergeant)
        {
            if (moraleCharges > 0)
                return ($"Drill Sergeant  [★] Morale +{moraleCharges}", "The garrison is fired up! Ready for another drill or Doctrine review?");
            if (progressionEnabled && trackedCommanders > 0)
                return ("Drill Sergeant", $"{trackedCommanders} veteran commander(s) on the roster. Speak to run drills or set Directives.");
            return ("Drill Sergeant", "Want to sharpen your war tactics? Speak to enter Army Training directly.");
        }
        if (role == CastleNpcRole.EchoArchivist)
        {
            return ("Echo Archivist", "Memories of past battles stir in the Hall of Echoes. Speak to challenge an Echo.");
        }
        if (state == GuildQuestState.AwaitingReport)
        {
            string name = string.IsNullOrEmpty(contractId) ? "your quarry" : GuildScreenRules.Card(contractId).Name;
            return ("Guild Attendant  [!] Report Ready", $"Back from {name}! Speak with me to file your report.");
        }
        if (state == GuildQuestState.Accepted && !string.IsNullOrEmpty(contractId))
        {
            var card = GuildScreenRules.Card(contractId);
            return ("Guild Attendant  [•] Active Hunt", $"{card.Name} awaits in {card.Destination}. Speak for drills or status.");
        }
        if (moraleCharges > 0)
            return ($"Guild Attendant  [★] Morale +{moraleCharges}", "The garrison is fired up! Your next RPG victory restores an extra rune charge.");
        if (progressionEnabled && trackedCommanders > 0)
            return ("Guild Attendant", $"Welcome back. {trackedCommanders} veteran commander(s) stand ready in the roster.");
        return ("Guild Attendant", "Welcome to the Dunan Castle Guild — Hunts, Army Drills, and Echoes.");
    }

    internal static string ReadinessStrip(int avgLevel, int activeStarBonds, int moraleCharges)
    {
        var parts = new List<string>(3);
        if (avgLevel > 0) parts.Add($"Party Avg Lv {avgLevel}");
        parts.Add(activeStarBonds == 1 ? "1 Star Bond active" : $"{activeStarBonds} Star Bonds active");
        if (moraleCharges > 0) parts.Add($"Morale +{moraleCharges}");
        return "Readiness: " + string.Join("  ·  ", parts);
    }
}
