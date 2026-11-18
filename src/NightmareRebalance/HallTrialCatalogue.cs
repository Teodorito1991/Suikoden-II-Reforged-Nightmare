namespace NightmareRebalance;

internal sealed record HallTrialDefinition(string Id, string Name, string TableId,
    string PartyId, int Hp, string PackId, string SceneId, string Description, string Rewards);

internal static class HallTrialCatalogue
{
    internal static readonly HallTrialDefinition[] Entries =
    {
        new("rat", "Pest Rat", "boss_rat", "boss_rat_00", 5200, "rat", "bgh3",
            "Challenge Pest Rat at its original HP.", "Native Potch · Possible Poison Orb"),
        new("harpy", "Harpy", "boss_hap1", "boss_hap1_00", 7400, "hap", "bgi6",
            "Prepare for physical pressure and the Harpy's original status attack.",
            "Native Potch · No native item drop")
    };
}
