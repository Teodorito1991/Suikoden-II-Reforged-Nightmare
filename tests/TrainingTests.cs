using NightmareRebalance;

internal static class TrainingTests
{
    internal static void Run()
    {
        static void Check(bool result, string message)
        {
            if (!result) throw new InvalidOperationException(message);
        }

        Check(TrainingResultRules.CanRedirectDefeat(3, 2, 3, -1), "War 3 defeat return");
        Check(!TrainingResultRules.CanRedirectDefeat(3, 2, 1, -1), "Unknown War 3 ending");
        Check(!TrainingResultRules.CanRedirectDefeat(3, 2, 3, 0), "Missing War 3 defeat flag");
        Check(!TrainingResultRules.CanRedirectDefeat(3, 1, 3, -1), "Ordinary return");
        Check(!TrainingResultRules.CanRedirectDefeat(2, 2, 3, -1), "Unverified War 2 interception");
        Check(TrainingResultRules.VictoryPotch(3, true, 1, 1, 0, true, true, false) == 1500, "Veteran victory");
        Check(TrainingResultRules.VictoryPotch(1, false, 1, 1, 0, false, true, false) == 0, "Timed ending");
        Check(TrainingResultRules.VictoryPotch(3, true, 2, 3, -1, true, true, false) == 0, "Redirected defeat");
        Check(TrainingResultRules.VictoryPotch(2, false, 1, 4, 0, true, true, false) == 0, "Fort breach");
        Check(TrainingResultRules.VictoryPotch(2, false, 1, 4, 0, true, false, true) == 0, "Allied rout");
        Check(TrainingResultRules.VictoryPotch(2, false, 1, 4, 0, true, true, true) == 750, "Fort victory");

        var labels = GuildScreenRules.Text(new GuildScreenModel
        {
            TrainingName = "Commander Rout", State = "Veteran", Bonus = "1,500"
        });
        Check(labels.Any(label => label.Text.Contains("1,500 Potch")), "Training reward card");
        Check(labels.All(label => !label.Text.Contains("Possible item")), "No hunt loot on training card");
        Check(TrainingResultRules.TrainingCasualty(2) == 1, "Training death staged as native retreat");
        Check(TrainingResultRules.TrainingCasualty(1) == 1, "Native retreat unchanged");
        Check(TrainingResultRules.TrainingCasualty(0) == 0, "No casualty unchanged");
        Console.WriteLine("PASS: training reward and defeat classification and Guild presentation.");
    }
}
