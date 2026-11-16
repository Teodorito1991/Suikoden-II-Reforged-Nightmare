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
            "Up to 24 extra damage.\nKnockdown follows the usual rules.")
    };

    internal static bool Includes(StarBond bond, IReadOnlyCollection<int> party) =>
        bond.Members.All(party.Contains);

    internal static bool Wounded(int hp, int max) => hp > 0 && max > 0 && hp * 2 <= max;

    internal static int DamageBonus(int damage, int divisor, int cap) =>
        damage <= 0 ? 0 : Math.Min(damage / divisor, cap);
}
