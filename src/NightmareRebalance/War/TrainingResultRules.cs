namespace NightmareRebalance;

internal static class TrainingResultRules
{
    internal static bool CanRedirectDefeat(int war, int result, int ending, int gameOver) =>
        result == 2 && (war == 1 || (war == 3 && ending == 3 && gameOver == -1));

    internal static int VictoryPotch(int war, bool veteran, int nativeResult, int ending,
        int gameOver, bool commanderRouted, bool alliesRemain, bool fortVictory) =>
        nativeResult == 1 && gameOver == 0 && alliesRemain &&
        (war switch
        {
            1 or 3 => commanderRouted && ending == 1,
            2 => commanderRouted && ending == 4 && fortVictory,
            6 or 10 or 13 or 15 => commanderRouted || ending is 1 or 4,
            _ => false
        })
            ? Potch(veteran)
            : 0;

    internal static int Potch(bool veteran) => veteran ? 1500 : 750;

    // w_dead.CheckDead stages war_sys.sibou_flag: 0 none, 1 retreat, 2 permanent
    // death. Training never stages a death; it uses the native retreat instead.
    internal static byte TrainingCasualty(byte staged) => staged == 2 ? (byte)1 : staged;
}
