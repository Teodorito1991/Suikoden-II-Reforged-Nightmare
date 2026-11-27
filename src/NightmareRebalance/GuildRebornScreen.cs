namespace NightmareRebalance;

internal sealed record GuildMenuRow(string Title, string Note);

internal sealed record GuildMenuView(string Section, GuildMenuRow[] Rows, int Focus,
    string Title, string Area, string Description, string Reward, string Condition,
    string Status, string Confirm, string Cancel, bool Preview,
    string Breadcrumb = "", float? ProgressFraction = null);

internal static class GuildRebornScreen
{
    internal const float Width = 1080, Height = 610;
    internal static readonly GuildBox Heading = new(0, 0, 1080, 76);
    internal static readonly GuildBox List = new(0, 90, 358, 400);
    internal static readonly GuildBox Detail = new(372, 90, 708, 400);
    internal static readonly GuildBox Footer = new(0, 504, 1080, 106);
    internal const int RowsPerPage = 4;

    internal static GuildMenuView Paginate(GuildMenuView view, int pageSize = RowsPerPage)
    {
        if (view.Rows.Length <= pageSize) return view;
        int page = view.Focus / pageSize;
        int pages = (view.Rows.Length + pageSize - 1) / pageSize;
        return view with
        {
            Section = $"{view.Section}    {page + 1}/{pages}",
            Focus = view.Focus % pageSize,
            Rows = view.Rows.Skip(page * pageSize).Take(pageSize).ToArray()
        };
    }

    internal static GuildBox Row(int index, int count) => count switch
    {
        > 5 => new(19, 136 + index * 55, 320, 52),
        5 => new(19, 136 + index * 66, 320, 62),
        _ => new(19, 136 + index * 78, 320, 65)
    };

    internal static float Scale(int width, int height) =>
        Math.Max(.1f, Math.Min((width - 48) / Width, (height - 48) / Height));

    internal static IReadOnlyList<GuildDrawText> Text(GuildMenuView view)
    {
        var text = new List<GuildDrawText>();
        void Add(float x, float y, float w, float h, string value, int size, string tone = "body")
        {
            if (value.Length != 0) text.Add(new(new(x,y,w,h),value,size,tone));
        }
        Add(29, 16, 665, 45, "Castle Guild", 35);
        string corner = view.Preview ? "PRESENTATION PREVIEW"
            : !string.IsNullOrEmpty(view.Breadcrumb) ? view.Breadcrumb : "Dunan Castle";
        Add(638, 22, 407, 31, corner, 19, "muted");
        Add(29, 103, 300, 30, view.Section, 19, "gold");
        for (int i = 0; i < view.Rows.Length; i++)
        {
            var row = Row(i, view.Rows.Length);
            bool dense = view.Rows.Length > 5;
            string tone = i == view.Focus ? "gold" : "body";
            string noteTone = NoteTone(view.Rows[i].Note);
            Add(51, row.Y + 4, 274, dense ? 26 : 31, view.Rows[i].Title, dense ? 21 : 23, tone);
            Add(51, row.Y + (dense ? 30 : 34), 274, dense ? 22 : 27, view.Rows[i].Note, dense ? 15 : 16, noteTone);
        }
        Add(400, 115, 646, 46, view.Title, 31);
        Add(401, 166, 639, 37, view.Area, 21, "gold");
        Add(401, 216, 634, 88, view.Description, 23);
        Add(401, 333, 634, 62, view.Reward, 22, "gold");
        Add(401, 408, 634, 67, view.Condition, 18, "muted");
        Add(29, 517, 1022, 47, view.Status, 20);
        string pageHint = view.Section.Contains('/') ? "     ← / →  Page" : "";
        Add(29, 568, 1022, 28, $"↑ / ↓  Navigate{pageHint}     Confirm  {view.Confirm}     Cancel  {view.Cancel}", 18, "gold");
        return text;
    }

    internal static string NoteTone(string note)
    {
        if (string.IsNullOrEmpty(note)) return "muted";
        if (note.Contains("Completed", StringComparison.OrdinalIgnoreCase) ||
            note.Contains("In your party", StringComparison.OrdinalIgnoreCase) ||
            note.Contains("Ready", StringComparison.OrdinalIgnoreCase) ||
            note.Contains("Marshal", StringComparison.OrdinalIgnoreCase) ||
            note.Contains("General", StringComparison.OrdinalIgnoreCase))
            return "success";
        if (note.Contains("Unavailable", StringComparison.OrdinalIgnoreCase) ||
            note.Contains("Locked", StringComparison.OrdinalIgnoreCase) ||
            note.Contains("Disabled", StringComparison.OrdinalIgnoreCase) ||
            note.Contains("OFF", StringComparison.Ordinal))
            return "alert";
        return "muted";
    }
}
