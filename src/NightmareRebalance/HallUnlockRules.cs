namespace NightmareRebalance;

internal enum HallUnlockPolicy { StoryProgression, UnrestrictedTesting }

internal static class HallUnlockRules
{
    // vc06 variant 1, event 32: original boss index 14, followed by the
    // persistent ECEvFlgSet(0x15, 0x02). Never use the shared result flag C2/80.
    // vj20 variant 4 loads boss 20 and sets DD/02 after its battle.
    // vj23 variant 4 loads boss 23 and sets DF/40 after its battle.
    // Pest Rat and second Harpy witnesses remain unproven and fail closed.
    internal static bool StoryUnlocked(string id, System.Func<int, byte, bool> readFlag) => id switch
    {
        "harpy" => readFlag(0x15, 0x02),
        "sierra" => readFlag(0xDD, 0x02),
        "golden-wolf" => readFlag(0xDF, 0x40),
        _ => false
    };

    internal static bool Testing(HallUnlockPolicy policy, bool developer) =>
        policy == HallUnlockPolicy.UnrestrictedTesting && developer;

    internal static bool Unlocked(string id, HallUnlockPolicy policy, bool developer,
        System.Func<int, byte, bool> readFlag) => Testing(policy, developer) || StoryUnlocked(id, readFlag);

    internal static bool CanLaunch(bool hall, bool trial, bool unlocked) => hall && trial && unlocked;
}
