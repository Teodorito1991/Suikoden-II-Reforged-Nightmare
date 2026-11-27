namespace NightmareRebalance;
internal static class TrainingPersistenceRules
{
    // Suikoden II campaign slots only; settings, Suikoden I and backups are untouched.
    internal static bool IsCampaignPath(string? path)=>path!=null&&System.Text.RegularExpressions.Regex.IsMatch(
        path.Replace('\\','/'),@"(?:^|/)gsd2/Data[0-9]+$",System.Text.RegularExpressions.RegexOptions.CultureInvariant|System.Text.RegularExpressions.RegexOptions.IgnoreCase);
}
