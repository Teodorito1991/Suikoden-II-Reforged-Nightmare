namespace NightmareRebalance;

internal enum GuildPage { Services, Hunts, Training, Difficulty, Status, Strategy, Echoes, EchoDifficulty, SpoilerWarning, LucaParties, ArmyRoster, Codex }
internal enum GuildChoice { None, Accept, Start, Report, Withdraw, Leave, StartEcho, SetSpoilers, RestoreStory, ChooseLuca, CycleDoctrine, CycleDirective, CycleHuntModifier }
internal enum CastleNpcRole { GuildAttendant, DrillSergeant, EchoArchivist }

internal static class GuildApproach
{
    // Native field coordinates use eight units per EVENT_OBJ coordinate.
    internal static bool AtCounter(int playerX, int playerY, int clerkX, int clerkY) =>
        Math.Abs(playerX - clerkX) <= 7 && playerY - clerkY is >= 12 and <= 22;

    internal static bool FacingClerk(int animation) => animation is 1 or 5 or 9;
}

internal static class GuildGauntletInput
{
    // EventMain also advances native return/fade work: never fence transitional map steps.
    internal static bool CanHold(bool active,bool idle,bool release,int mapStep,int eventStep)=>
        active&&idle&&!release&&mapStep==20&&eventStep==2;
}

internal sealed class MenuRepeatTimer
{
    internal const int InitialDelayFrames = 20;
    internal const int RepeatIntervalFrames = 6;
    int heldFrames;

    internal void Reset() => heldFrames = 0;

    internal bool Step(bool pressed, bool held)
    {
        if (pressed)
        {
            heldFrames = 1;
            return true;
        }
        if (!held)
        {
            heldFrames = 0;
            return false;
        }
        heldFrames++;
        return heldFrames >= InitialDelayFrames && (heldFrames - InitialDelayFrames) % RepeatIntervalFrames == 0;
    }
}

internal sealed class GuildMenuNavigation
{
    internal GuildPage Page { get; private set; }
    internal int Focus { get; private set; }
    internal int Drill { get; private set; }
    internal int HuntCount { get; private set; }
    internal bool ConfirmReleased { get; private set; }

    internal static readonly GuildPage[] Services = { GuildPage.Hunts, GuildPage.Training, GuildPage.Status, GuildPage.Strategy, GuildPage.Echoes };

    internal int Echo { get; private set; }
    internal int EchoCount { get; set; } = 1;
    internal int SpoilerRow {get;set;}=-1;

    internal const int TrainingDrillCount = 4;
    internal const int RosterRow = 4;
    internal const int DirectiveRow = 5;
    internal const int CodexRow = 6;
    internal int ArmyRosterCount { get; set; } = 1;

    internal int LucaChoiceCount;
    internal void BeginLuca(int count){LucaChoiceCount=count;Enter(GuildPage.LucaParties,0);}
    internal int RowCount => Page switch
    {
        GuildPage.Services => Services.Length + 1,
        GuildPage.Training => 7,
        GuildPage.ArmyRoster => Math.Max(1, ArmyRosterCount),
        GuildPage.Codex => 4,
        GuildPage.Strategy => StarBondRules.Catalogue.Length,
        GuildPage.Hunts => HuntCount,
        GuildPage.LucaParties=>LucaChoiceCount,
        GuildPage.Difficulty or GuildPage.EchoDifficulty or GuildPage.SpoilerWarning => 2,
        GuildPage.Echoes => EchoCount,
        _ => 1
    };

    internal void Open(bool awaitingReport, int hunts)
    {
        HuntCount = Math.Max(0, hunts);
        Enter(awaitingReport ? GuildPage.Status : GuildPage.Services, 0);
    }

    internal void OpenDirect(GuildPage page, int hunts)
    {
        HuntCount = Math.Max(0, hunts);
        Enter(page, 0);
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

    internal void MovePage(bool left, bool right, int pageSize = GuildRebornScreen.RowsPerPage)
    {
        Focus = MovePageFocus(Focus, RowCount, pageSize, left, right);
    }

    internal static int MoveFocus(int focus, int count, bool up, bool down) =>
        count <= 0 ? 0 : up == down ? focus : (focus + (down ? 1 : count - 1)) % count;

    internal static int MovePageFocus(int focus, int count, int pageSize, bool left, bool right)
    {
        if (count <= pageSize || pageSize <= 0 || left == right) return count <= 0 ? 0 : Math.Clamp(focus, 0, count - 1);
        int pages = (count + pageSize - 1) / pageSize;
        int currentPage = Math.Clamp(focus, 0, count - 1) / pageSize;
        int nextPage = (currentPage + (right ? 1 : pages - 1)) % pages;
        return Math.Min(nextPage * pageSize, count - 1);
    }

    internal static string Breadcrumb(GuildPage page) => page switch
    {
        GuildPage.Hunts => "Dunan Castle  ·  Hunts",
        GuildPage.Training => "Dunan Castle  ·  Army Training",
        GuildPage.Difficulty => "Army Training  >  Difficulty",
        GuildPage.ArmyRoster => "Army Training  >  Roster",
        GuildPage.Codex => "Army Training  >  Codex",
        GuildPage.Status => "Dunan Castle  ·  Guild Status",
        GuildPage.Strategy => "Dunan Castle  ·  Party Strategy",
        GuildPage.Echoes => "Dunan Castle  ·  Hall of Echoes",
        GuildPage.EchoDifficulty => "Hall of Echoes  >  Tier",
        GuildPage.SpoilerWarning => "Hall of Echoes  >  Spoiler Mode",
        GuildPage.LucaParties => "Hall of Echoes  >  Luca Teams",
        _ => "Dunan Castle"
    };

    internal bool Back()
    {
        switch (Page)
        {
            case GuildPage.Services: return false;
            case GuildPage.LucaParties: Enter(GuildPage.EchoDifficulty,0);break;
            case GuildPage.EchoDifficulty: Enter(GuildPage.Echoes, Echo); break;
            case GuildPage.SpoilerWarning: Enter(GuildPage.Echoes, EchoCount-1); break;
            case GuildPage.Difficulty: Enter(GuildPage.Training, Drill); break;
            case GuildPage.ArmyRoster: Enter(GuildPage.Training, RosterRow); break;
            case GuildPage.Codex: Enter(GuildPage.Training, CodexRow); break;
            case GuildPage.Hunts: Enter(GuildPage.Services, 0); break;
            case GuildPage.Training: Enter(GuildPage.Services, 1); break;
            case GuildPage.Status: Enter(GuildPage.Services, 2); break;
            case GuildPage.Strategy: Enter(GuildPage.Services, 3); break;
            case GuildPage.Echoes: Enter(GuildPage.Services, 4); break;
        }
        return true;
    }

    internal GuildChoice Confirm(bool pressed, bool held, GuildQuestState state, bool drillAvailable, bool echoAvailable = false, bool nightmareAvailable = false)
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
            case GuildPage.Training when Focus == RosterRow:
                Enter(GuildPage.ArmyRoster, 0);
                return GuildChoice.None;
            case GuildPage.Training when Focus == DirectiveRow:
                return GuildChoice.CycleDirective;
            case GuildPage.Training when Focus == CodexRow:
                Enter(GuildPage.Codex, 0);
                return GuildChoice.None;
            case GuildPage.Training when Focus < TrainingDrillCount && drillAvailable:
                Drill = Focus;
                Enter(GuildPage.Difficulty, 0);
                return GuildChoice.None;
            case GuildPage.ArmyRoster:
                return GuildChoice.CycleDoctrine;
            case GuildPage.Codex when Focus == 2:
                return GuildChoice.CycleHuntModifier;
            case GuildPage.Codex:
                Back();
                return GuildChoice.None;
            case GuildPage.Difficulty: return GuildChoice.Start;
            case GuildPage.Echoes when Focus==SpoilerRow:
                Enter(GuildPage.SpoilerWarning,0); return GuildChoice.None;
            case GuildPage.SpoilerWarning:
                if(Focus==1)return GuildChoice.SetSpoilers; Back();return GuildChoice.None;
            case GuildPage.Echoes when echoAvailable:
                Echo=Focus; Enter(GuildPage.EchoDifficulty,0); return GuildChoice.None;
            case GuildPage.LucaParties:return GuildChoice.ChooseLuca;
            case GuildPage.EchoDifficulty when echoAvailable && (Focus==0||nightmareAvailable): return GuildChoice.StartEcho;
            case GuildPage.EchoDifficulty: return GuildChoice.None;
            case GuildPage.Status when state == GuildQuestState.Accepted: return GuildChoice.Withdraw;
            case GuildPage.Status when state == GuildQuestState.AwaitingReport: return GuildChoice.Report;
            case GuildPage.Status:
            case GuildPage.Strategy:
            case GuildPage.Echoes: Back(); break;
        }
        return GuildChoice.None;
    }
}
