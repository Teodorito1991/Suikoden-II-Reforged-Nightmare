namespace NightmareRebalance;
internal static class HallSpoilerMode
{
    internal const string Warning="WARNING: MAJOR STORY SPOILERS!";
    internal const string Disclosure="Unlocking all bosses will reveal enemies, encounters and final bosses from throughout Suikoden II, including the ending.\n\nRecommended only for players who have already completed the original game.\n\nAre you sure you want to unlock all bosses?";
    internal static bool Set(bool enabled)
    {
        bool previous=Plugin.Options.UnlockAllBosses.Value;bool autosave=Plugin.Options.PreferenceFile.SaveOnConfigSet;Plugin.Options.PreferenceFile.SaveOnConfigSet=false;
        try{Plugin.Options.UnlockAllBosses.Value=enabled;Plugin.Options.PreferenceFile.Save();
            CastleGuildRuntime.Status=enabled?"Spoiler Mode ON. Story flags are unchanged.":"Story Progression restored.";return true;}
        catch(Exception e){Plugin.Options.UnlockAllBosses.Value=previous;CastleGuildRuntime.Status="The preference could not be saved.";Plugin.Logger.LogWarning("Hall preference save failed: "+e.Message);return false;}
        finally{Plugin.Options.PreferenceFile.SaveOnConfigSet=autosave;}
    }
    internal static GuildMenuView View(int focus,bool enabled,bool preview=false)=>new("SPOILER MODE",
        new[]{new GuildMenuRow("Cancel","Keep the current preference"),new GuildMenuRow(enabled?"Restore Story Progression":"Yes, unlock all bosses","Save this reversible mod preference")},focus,
        enabled?"Restore Story Progression":Warning,"",enabled?"Restore the normal story-based availability rules?":Disclosure,
        "","Only story locks change. Disabled and unsupported trials remain unavailable.","No original save or story flag is modified.",focus==1?"Confirm":"Cancel","Back",preview);
}
