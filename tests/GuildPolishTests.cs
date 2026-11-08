using NightmareRebalance;

internal static class GuildPolishTests
{
    internal static void Run()
    {
        int n = 0;
        void Check(bool ok, string why) { n++; if (!ok) throw new Exception("Guild polish: " + why); }
        foreach (var state in Enum.GetValues<GuildQuestState>())
        {
            string id = "sun-kings-retinue";
            var model = new GuildScreenModel { Selected = id, State = GuildPresentation.State(state), Objective = GuildPresentation.Objective(state, id),
                Names = GuildScreenRules.Cards.Select(x => x.Name).ToArray(), Status = "This contract is not currently available.", Bonus = "2,500", Item = "Stone of Power" };
            var labels = GuildScreenRules.Text(model);
            Check(labels.Any(x => x.Text == model.Status), "Native acceptance feedback is drawn, not hidden in a diagnostic field.");
            Check(labels.Any(x => x.Text.Contains("Possible item: Stone of Power")), "Item is possible, not guaranteed.");
            Check(labels.Any(x => x.Text.Contains("purse limit")), "Potch cap remains visible.");
            Check(labels.Any(x => x.Text.Contains("bag space")), "Inventory capacity remains authoritative.");
            Check(labels.All(x => !x.Text.Contains("acceptance pending")), "Historical QA text is not shown to players.");
            Check(!GuildPresentation.Availability(state, id, id, false, true).StartsWith("Available"), "Disabled selection never advertised as available.");
        }
        Check(GuildPresentation.Availability(GuildQuestState.Accepted, "sun-kings-retinue", "sun-kings-retinue", true, true).StartsWith("Accepted"), "Accepted contract is clear.");
        Check(GuildPresentation.Availability(GuildQuestState.AwaitingReport, "sun-kings-retinue", "sun-kings-retinue", true, true).StartsWith("Completed"), "Completion awaits report.");
        Check(GuildPresentation.Availability(GuildQuestState.Reported, "sun-kings-retinue", "sun-kings-retinue", true, true).Contains("available again"), "Reported hunt is repeatable.");
        Check(GuildPresentation.Objective(GuildQuestState.AwaitingReport, "sun-kings-retinue").Contains("no additional spoils"), "Reporting cannot promise a second payout.");
        Check(GuildPresentation.Objective(GuildQuestState.Accepted, "sun-kings-retinue").Contains("Escape leaves the contract open"), "Escape retry is clear.");
        Check(GuildPresentation.Availability(GuildQuestState.NoContract, "dreamers-circuit", null, false, true).Contains("disabled"), "Dreamer stays experimental.");
        Check(GuildPresentation.Availability(GuildQuestState.NoContract, "thornwake-patrol", null, false, true).Contains("disabled"), "Thornwake stays experimental.");
        Console.WriteLine($"PASS: Guild polish {n} assertions; visible feedback, four quest states, conditional spoils and unchanged opt-outs. Presentation/component evidence only.");
    }
}
