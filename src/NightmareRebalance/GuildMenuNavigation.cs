namespace NightmareRebalance;

internal enum GuildPage { Services, Hunts, Training, Difficulty, Status, Strategy }
internal enum GuildChoice { None, Accept, Start, Report, Withdraw, Leave }

internal static class GuildApproach
{
    // Native field coordinates use eight units per EVENT_OBJ coordinate.
    internal static bool AtCounter(int playerX, int playerY, int clerkX, int clerkY) =>
        Math.Abs(playerX - clerkX) <= 7 && playerY - clerkY is >= 12 and <= 22;

    internal static bool FacingClerk(int animation) => animation is 1 or 5 or 9;
}

internal sealed class GuildMenuNavigation
{
    internal GuildPage Page { get; private set; }
    internal int Focus { get; private set; }
    internal int Drill { get; private set; }
    internal int HuntCount { get; private set; }
    internal bool ConfirmReleased { get; private set; }

    internal static readonly GuildPage[] Services = { GuildPage.Hunts, GuildPage.Training, GuildPage.Status, GuildPage.Strategy };

    internal int RowCount => Page switch
    {
        GuildPage.Services => Services.Length + 1,
        GuildPage.Training => 4,
        GuildPage.Strategy => StarBondRules.Catalogue.Length,
        GuildPage.Hunts => HuntCount,
        GuildPage.Difficulty => 2,
        _ => 1
    };

    internal void Open(bool awaitingReport, int hunts)
    {
        HuntCount = Math.Max(0, hunts);
        Enter(awaitingReport ? GuildPage.Status : GuildPage.Services, 0);
    }

    void Enter(GuildPage page, int row)
    {
        Page = page;
        Focus = row;
        ConfirmReleased = false;
    }

    internal void Move(bool up, bool down)
    {
        Focus = MoveFocus(Focus, RowCount, up, down);
    }

    internal static int MoveFocus(int focus, int count, bool up, bool down) =>
        count <= 0 ? 0 : up == down ? focus : (focus + (down ? 1 : count - 1)) % count;

    internal bool Back()
    {
        switch (Page)
        {
            case GuildPage.Services: return false;
            case GuildPage.Difficulty: Enter(GuildPage.Training, Drill); break;
            case GuildPage.Hunts: Enter(GuildPage.Services, 0); break;
            case GuildPage.Training: Enter(GuildPage.Services, 1); break;
            case GuildPage.Status: Enter(GuildPage.Services, 2); break;
            case GuildPage.Strategy: Enter(GuildPage.Services, 3); break;
        }
        return true;
    }

    internal GuildChoice Confirm(bool pressed, bool held, GuildQuestState state, bool drillAvailable)
    {
        if (!pressed)
        {
            if (!held) ConfirmReleased = true;
            return GuildChoice.None;
        }
        if (!ConfirmReleased) return GuildChoice.None;
        ConfirmReleased = false;
        switch (Page)
        {
            case GuildPage.Services:
                if (Focus == Services.Length) return GuildChoice.Leave;
                Enter(Services[Focus], 0);
                return GuildChoice.None;
            case GuildPage.Hunts when HuntCount>0: return GuildChoice.Accept;
            case GuildPage.Training when drillAvailable:
                Drill = Focus;
                Enter(GuildPage.Difficulty, 0);
                return GuildChoice.None;
            case GuildPage.Difficulty: return GuildChoice.Start;
            case GuildPage.Status when state == GuildQuestState.Accepted: return GuildChoice.Withdraw;
            case GuildPage.Status when state == GuildQuestState.AwaitingReport: return GuildChoice.Report;
            case GuildPage.Status:
            case GuildPage.Strategy: Back(); break;
        }
        return GuildChoice.None;
    }
}
