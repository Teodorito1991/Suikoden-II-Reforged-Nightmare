namespace NightmareRebalance;

internal static class TrainingResultRules
{
    internal static bool CanRedirectDefeat(int war, int result, int ending, int gameOver) =>
        result == 2 && (war == 1 || (war == 3 && ending == 3 && gameOver == -1));

    internal static int VictoryPotch(int war, bool veteran, int nativeResult, int ending,
        int gameOver, bool commanderRouted, bool alliesRemain, bool fortVictory) =>
        nativeResult == 1 && gameOver == 0 && commanderRouted && alliesRemain &&
        (war switch { 1 or 3 => ending == 1, 2 => ending == 4 && fortVictory, _ => false })
            ? Potch(veteran)
            : 0;

    internal static int Potch(bool veteran) => veteran ? 1500 : 750;
}
