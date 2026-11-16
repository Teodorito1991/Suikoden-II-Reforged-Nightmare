namespace NightmareRebalance;

internal sealed record GuildMenuRow(string Title, string Note);

internal sealed record GuildMenuView(string Section, GuildMenuRow[] Rows, int Focus,
    string Title, string Area, string Description, string Reward, string Condition,
    string Status, string Confirm, string Cancel, bool Preview);

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

    internal static GuildBox Row(int index, bool compact = false) =>
        compact ? new(19, 136 + index * 66, 320, 62) : new(19, 136 + index * 78, 320, 65);

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
        Add(738, 22, 307, 31, view.Preview ? "PRESENTATION PREVIEW" : "Dunan Castle", 20, "muted");
        Add(29, 103, 300, 30, view.Section, 19, "gold");
        for (int i = 0; i < view.Rows.Length; i++)
        {
            var row = Row(i, view.Rows.Length > RowsPerPage);
            string tone = i == view.Focus ? "gold" : "body";
            Add(51, row.Y + 4, 274, 31, view.Rows[i].Title, 23, tone);
            Add(51, row.Y + 34, 274, 27, view.Rows[i].Note, 16, "muted");
        }
        Add(400, 115, 646, 46, view.Title, 31);
        Add(401, 166, 639, 37, view.Area, 21, "gold");
        Add(401, 216, 634, 88, view.Description, 23);
        Add(401, 333, 634, 62, view.Reward, 22, "gold");
        Add(401, 408, 634, 67, view.Condition, 18, "muted");
        Add(29, 517, 1022, 47, view.Status, 20);
        Add(29, 568, 1022, 28, $"↑ / ↓  Navigate     Confirm  {view.Confirm}     Cancel  {view.Cancel}", 18, "gold");
        return text;
    }
}
