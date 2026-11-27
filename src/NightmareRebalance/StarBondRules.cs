namespace NightmareRebalance;

internal sealed record StarBond(int Unite, int Function, int[] Members, string Title, string Partners,
    string Trigger, string Effect);

internal static class StarBondRules
{
    internal static readonly StarBond[] Catalogue =
    {
        new(30, 115, new[]{7,8}, "Warriors' Ward", "Hix and Tengaar",
            "Hix uses Defend at half HP or less.\nTengaar must be able to act.",
            "Up to 20 less damage from a direct physical hit."),
        new(7, 92, new[]{30,26}, "Woodland Pursuit", "Kinnison and Shiro",
            "Kinnison uses Attack on a poisoned enemy.\nShiro must be able to act.",
            "Adds up to 15 damage to the hit."),
        new(2, 87, new[]{33,32}, "Knight's Fire", "Camus and Miklotov",
            "Camus casts Fire Wall.\nMiklotov must be able to act.",
            "Up to 18 extra damage when physical defense\nis higher than magic defense."),
        new(42, 127, new[]{58,59,60}, "Bandit Finish", "Gijimu, Koyu and Lo Wen",
            "Use Bandit Attack against an enemy at half HP or less.",
            "Up to 24 extra damage.\nKnockdown follows the usual rules."),
        new(-1, 0, new[]{1,18}, "Genkaku's Legacy", "Riou and Nanami",
            "Riou or Nanami uses Defend at half HP or less.\nBoth must be able to act.",
            "Up to 20 less damage from a direct physical hit."),
        new(-2, 0, new[]{2,3}, "Mercenary Veterans", "Flik and Viktor",
            "Flik or Viktor uses Attack on a wounded enemy.\nBoth must be able to act.",
            "Adds up to 16 damage to the hit."),
        new(-3, 0, new[]{53,61}, "Arcane Resonance", "Luc and Mazus",
            "Luc or Mazus casts an offensive spell.\nBoth must be able to act.",
            "Up to 18 extra damage when physical defense\nis higher than magic defense."),
        new(-4, 0, new[]{36,45}, "Vampire Hunters", "Kahn and Sierra",
            "Kahn or Sierra uses Attack or Magic.\nBoth must be able to act.",
            "Adds up to 18 damage against high-magic\nor resilient foes."),
        new(-5, 0, new[]{11,44}, "Wandering Blades", "Georg and Killey",
            "Georg or Killey uses Attack at 75% HP or higher.\nBoth must be able to act.",
            "Adds up to 18 opening strike damage."),
        new(-6, 0, new[]{73,63}, "Rokaku Shadows", "Kasumi and Mondo",
            "Kasumi or Mondo uses Attack on an unharmed foe.\nBoth must be able to act.",
            "Adds up to 16 ambush damage.")
    };

    internal static bool Includes(StarBond bond, IReadOnlyCollection<int> party) =>
        bond.Members.All(party.Contains);

    internal static bool Wounded(int hp, int max) => hp > 0 && max > 0 && hp * 2 <= max;

    internal static int DamageBonus(int damage, int divisor, int cap) =>
        damage <= 0 ? 0 : Math.Min(damage / divisor, cap);
}
