namespace NightmareRebalance;

internal static class HallAvailability
{
    internal static bool Testing => HallUnlockRules.Testing(Plugin.Options.HallUnlockPolicy.Value,
        Plugin.Options.EnableDeveloperTools.Value);

    internal static bool Configured(int selection) => selection switch
    {
        0 => Plugin.Options.EnableHallPestRat.Value,
        1 => Plugin.Options.EnableHallHarpy.Value,
        2 => Plugin.Options.EnableHallGoldenWolf.Value,
        3 => Plugin.Options.EnableHallSierra.Value,
        4 => Plugin.Options.EnableHallHarpySecond.Value,
        _ => false
    };

    internal static bool Unlocked(int selection)
    {
        if (selection < 0 || selection >= HallTrialCatalogue.Entries.Length) return false;
        if (Testing) return true;
        var game = Framework.Singleton<GAME_WORK>.instance;
        if (game?.event_flag == null) return false;
        return HallUnlockRules.StoryUnlocked(HallTrialCatalogue.Entries[selection].Id,
            (flag, mask) => flag >= 0 && flag < game.event_flag.Length && game.eventFlgCHK(flag, mask));
    }

    internal static bool CanSelect(int selection) => HallUnlockRules.CanLaunch(
        Plugin.Options.EnableHallOfEchoes.Value, Configured(selection), Unlocked(selection));

    internal static string Reason(int selection) => !Unlocked(selection)
        ? "A memory yet to be made. Continue your journey."
        : !Configured(selection) ? "This rematch is disabled in the Hall settings."
        : Testing ? "Testing availability · Story flags are unchanged." : "Story memory recalled · Rematch available.";
}
