namespace NightmareRebalance;

internal enum HallUnlockPolicy { StoryProgression, UnrestrictedTesting }

internal static class HallUnlockRules
{
    // Audited category-15 boss loads followed by ordinary event battles and
    // dedicated persistent completion writes. Numeric map/duel IDs are not bosses.
    internal static bool HasWitness(string id) => id is "rat" or "harpy" or "golden-wolf" or "stone-golem" or "abomination" or "double-head" or "bone-dragon" or "lucia-retinue" or "gorudo" || IsBonus(id);

    // Bonus memories are not claims of an individual story victory.
    // Their prerequisites use independently audited native completion witnesses.
    internal static bool IsBonus(string id)=>id is "celestial-sword" or "neclord-final" or
        "lucia-highland" or "seed-culgan" or "mist-shade" or "lucia-castle" or "rowd-patrol";

    internal static string BonusMilestone(string id)=>id is "mist-shade" or "rowd-patrol" ? "Double Head" :
        id=="celestial-sword"?"Abomination":"Gorudo";

    internal static bool StoryUnlocked(string id, System.Func<int, byte, bool> readFlag) => id switch
    {
        "rat" => readFlag(0xB6, 0x02),        // vh04 / variant 8 / event 9
        "harpy" => readFlag(0x21, 0x10),     // vi15 / variant 0 / event 4
        "golden-wolf" => readFlag(0x16, 0x02), // vc27 / variant 0 / event 2
        "stone-golem" => readFlag(0xC9, 0x01), // vg15 / variant 4 / event 8
        "abomination" => readFlag(0x56, 0x20), // vd14 / variant 0 / event 3
        "double-head" => readFlag(0x07, 0x40),
        "bone-dragon" => readFlag(0xBA, 0x08) || readFlag(0xBA, 0x10),
        "lucia-retinue" => readFlag(0xBB, 0x10),
        "gorudo" => readFlag(0x14, 0x02),
        "mist-shade" or "rowd-patrol" => readFlag(0x07,0x40),
        "celestial-sword" => readFlag(0x56,0x20),
        _ when IsBonus(id) => readFlag(0x14, 0x02),
        _ => false // Sierra, second Harpy and first Worm: no proven victory witness.
    };

    internal static bool Testing(HallUnlockPolicy policy, bool developer) =>
        policy == HallUnlockPolicy.UnrestrictedTesting && developer;

    internal static bool Unlocked(string id, HallUnlockPolicy policy, bool developer,
        System.Func<int, byte, bool> readFlag) => Testing(policy, developer) || StoryUnlocked(id, readFlag);

    internal static bool CanLaunch(bool hall, bool trial, bool unlocked) => hall && trial && unlocked;
}
