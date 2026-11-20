using BepInEx.Configuration;

namespace NightmareRebalance;

internal sealed class Settings
{
    internal readonly ConfigEntry<double> EnemyHpMultiplier, EnemyDamageMultiplier, ExperienceMultiplier, PotchMultiplier;
    internal readonly ConfigEntry<double> EnemyTargetHealthThreshold;
    internal readonly ConfigEntry<bool> EnableRuneRegeneration, RequireSurvivingCharacter, EnableDebugLogging;
    internal readonly ConfigEntry<int> RuneChargesPerVictory;
    internal readonly ConfigEntry<RecoveryPriority> RuneRecoveryPriority;
    internal readonly ConfigEntry<bool> EnableBossDiagnostics, EnableScriptedBossOverrides;
    internal readonly ConfigEntry<bool> EnableBossProfiles, EnableRuneBalancing, EnableCharacterBalancing, EnableEnemyProfiles, EnableWarUnitProfiles, EnableDeveloperTools, EnableWarArena, EnableEnemyTargeting;
    internal readonly ConfigEntry<string> BossProfilePath;
    internal readonly ConfigEntry<bool> EnableBossProfileDiagnostics, FallbackToGlobalNightmare;
    internal readonly ConfigEntry<bool> EnableTrainingWar, EnableTrainingChallenge, EnableTrainingDefeatReturn, EnableTrainingWar2, EnableTrainingWar3;
    internal readonly ConfigEntry<bool> EnableTrainingVeteran, EnableTrainingRewards, EnableTrainingNormalBuild;
    internal readonly ConfigEntry<bool> EnableNativeWarRemix,RunNativeWarSelfTest,ExportNativeWarResearch,EnableNativeWarAI,EnableWarSpecializations,EnableWarTactics,EnableWarManeuvers;

    internal readonly ConfigEntry<bool> EnableStarBonds, EnablePartyIdentity;
    internal readonly ConfigEntry<bool> EnableCombatReforgedAi, RunCombatSelfTest, EnableHuntDebugTools;
    internal readonly ConfigEntry<bool> EnableHallOfEchoes, EnableHallHarpy, EnableHallPestRat, EnableHallGoldenWolf, EnableHallSierra, EnableHallHarpySecond, EnableHallStoneGolem, EnableHallAbomination, EnableHallWorm;
    internal readonly Dictionary<string,ConfigEntry<bool>> AdditionalHallTrials = new();
    internal readonly ConfigEntry<HallUnlockPolicy> HallUnlockPolicy;
    internal readonly ConfigEntry<bool> EnableNightmareEchoes, EnableNightmareStoneRewards;

    internal Settings(ConfigFile config)
    {
        foreach(var trial in HallTrialCatalogue.Entries.Skip(8))
            AdditionalHallTrials.Add(trial.Id, config.Bind("Hall of Echoes", "EnableHall_"+trial.Id.Replace('-','_'), false,
                "Optional "+trial.Name+" rematch. Exact original source and read-only story unlock required. Restart required."));
        EnableTrainingNormalBuild=config.Bind("Native War",nameof(EnableTrainingNormalBuild),false,
            "Experimental production training contract: private flag fork, owned native return and save fence. Requires EnableTrainingWar and defeat return. No commander XP.");
        EnableNightmareEchoes = config.Bind("Hall of Echoes", nameof(EnableNightmareEchoes), false, "Optional data-driven advanced tiers for compatible Hall entries. Never alters story bosses or Original Echoes.");
        EnableNightmareStoneRewards = config.Bind("Hall of Echoes", nameof(EnableNightmareStoneRewards), true, "25% chance to offer the tier's audited growth stone after an earned Nightmare victory if no native loot rolled. Native inventory acceptance required.");
        HallUnlockPolicy = config.Bind("Hall of Echoes", nameof(HallUnlockPolicy), NightmareRebalance.HallUnlockPolicy.StoryProgression,
            "StoryProgression reads proven original completion flags. UnrestrictedTesting requires EnableDeveloperTools and only bypasses Hall availability; never writes progression.");
        EnableHallPestRat = config.Bind("Hall of Echoes", nameof(EnableHallPestRat), true,
            "Permit Pest Rat when the Hall and progression gate are enabled. Existing global Hall opt-ins are preserved.");
        EnableHallGoldenWolf = config.Bind("Hall of Echoes", nameof(EnableHallGoldenWolf), false,
            "Experimental Golden Wolf rematch. Requires the Hall and a proven story gate or explicit developer testing override.");
        EnableHallSierra = config.Bind("Hall of Echoes", nameof(EnableHallSierra), false,
            "Experimental Sierra rematch. Requires the Hall and a proven story gate or explicit developer testing override.");
        EnableHallHarpySecond = config.Bind("Hall of Echoes", nameof(EnableHallHarpySecond), false,
            "Experimental second Harpy rematch. Requires the Hall and a proven story gate or explicit developer testing override.");
        EnableHallStoneGolem = config.Bind("Hall of Echoes", nameof(EnableHallStoneGolem), false,
            "Experimental native Stone Golem rematch. Requires the Hall and original story completion or explicit developer testing availability.");
        EnableHallAbomination = config.Bind("Hall of Echoes", nameof(EnableHallAbomination), false,
            "Experimental native Abomination rematch. Requires the Hall and original story completion or explicit developer testing availability.");
        EnableHallWorm = config.Bind("Hall of Echoes", nameof(EnableHallWorm), false,
            "Experimental first Worm rematch. No proven normal story witness; explicit developer testing availability required.");
        EnableHallHarpy = config.Bind("Hall of Echoes", nameof(EnableHallHarpy), false,
            "Enable the experimental Harpy rematch. Requires EnableHallOfEchoes and a restart.");
        EnableHallOfEchoes = config.Bind("Hall of Echoes", nameof(EnableHallOfEchoes), false,
            "Enable the experimental Hall rematches in the Castle Guild. Requires a restart.");
        EnableHuntDebugTools = config.Bind("Developer", nameof(EnableHuntDebugTools), false, "Explicit hunt-test damage protection hotkey F7. Disabled by default; restart required. Never changes levels or saves.");
        EnableStarBonds = config.Bind("Party Revolution", nameof(EnableStarBonds), true, "Conditional Star Bonds in eligible Normal battles.");
        EnablePartyIdentity = config.Bind("Party Revolution", nameof(EnablePartyIdentity), true, "Revised native growth and existing rune slot thresholds. Restart required.");
        EnableCombatReforgedAi = config.Bind("Combat Reforged", nameof(EnableCombatReforgedAi), true, "Audited native enemy action/target choices; config in combat-reforged.json. Restart required.");
        RunCombatSelfTest = config.Bind("Combat Reforged", nameof(RunCombatSelfTest), false, "Startup-only disposable native combat probes; no scene, story or save. Leave false for normal play.");
        EnableWarTactics = config.Bind("Native War", nameof(EnableWarTactics), true, "Native terrain/reachability-aware tactical scoring and audited alternate objectives. Independent war-tactics.json settings; no campaign flag or command injection.");
        EnableWarManeuvers = config.Bind("Native War", nameof(EnableWarManeuvers), true, "Additional legal-candidate scoring for original wars and Veteran drills.");
        ConfigEntry<double> Mult(string key, double value) => config.Bind("Difficulty", key, value,
            new ConfigDescription("Suikoden II standard combat only. Restart after editing. Rounded away from zero at midpoint; capped to field capacity.", new AcceptableValueRange<double>(0, 20)));
        EnemyHpMultiplier = Mult(nameof(EnemyHpMultiplier), 1.60);
        EnemyTargetHealthThreshold = config.Bind("Enemy AI", nameof(EnemyTargetHealthThreshold), 0.50, new ConfigDescription("Prefer legal targets at or below this HP fraction.",new AcceptableValueRange<double>(0.05,1.0)));
        EnemyDamageMultiplier = Mult(nameof(EnemyDamageMultiplier), 1.45);
        ExperienceMultiplier = Mult(nameof(ExperienceMultiplier), 1.00);
        PotchMultiplier = Mult(nameof(PotchMultiplier), 0.85);
        EnableRuneRegeneration = config.Bind("Rune Regeneration", nameof(EnableRuneRegeneration), true, "Recover a total budget of charges per eligible combat participant after completed victory.");
        RuneChargesPerVictory = config.Bind("Rune Regeneration", nameof(RuneChargesPerVictory), 1,
            new ConfigDescription("Total charges per character, distributed across levels in priority order.", new AcceptableValueRange<int>(0, 60)));
        RuneRecoveryPriority = config.Bind("Rune Regeneration", nameof(RuneRecoveryPriority), RecoveryPriority.LowestDepletedLevel, "LowestDepletedLevel or HighestDepletedLevel.");
        RequireSurvivingCharacter = config.Bind("Rune Regeneration", nameof(RequireSurvivingCharacter), true, "Exclude HP <= 0 and characters considered dead/incapacitated by vanilla PM_DATA.IsDead at victory. False includes them but never revives or heals them.");
        EnableDebugLogging = config.Bind("Diagnostics", nameof(EnableDebugLogging), false, "Log battle decisions and actual charge changes; no per-frame logs.");
        EnableBossDiagnostics = config.Bind("Diagnostics", nameof(EnableBossDiagnostics), false, "Stage A: bounded JSONL encounter reports under BepInEx/NightmareRebalance/diagnostics. Restart required.");
        EnableScriptedBossOverrides = config.Bind("Features", nameof(EnableScriptedBossOverrides), false, "Stage A: permit HP/damage only for compiled verified identities selected in config/NightmareRebalance/scripted-battle-allowlist.json. Restart required.");
        EnableBossProfiles = config.Bind("Features", nameof(EnableBossProfiles), false, "Stage B: final vanilla-relative boss HP/damage profiles; never bypasses Stage A scope. Restart required.");
        BossProfilePath = config.Bind("Boss Profiles", nameof(BossProfilePath), "NightmareRebalance/boss-profiles.json", "JSON path relative to BepInEx/config, or an absolute path. Read once at startup.");
        EnableBossProfileDiagnostics = config.Bind("Boss Profiles", nameof(EnableBossProfileDiagnostics), false, "Write profile selection, fallbacks and actual before/after factors into the bounded Stage A JSONL journal. Restart required.");
        FallbackToGlobalNightmare = config.Bind("Boss Profiles", nameof(FallbackToGlobalNightmare), true, "For exact verified bosses lacking an active value: true uses global factors; false uses vanilla 1.0. Profiles disabled always preserves Stage A. Ordinary/unknown enemies always retain global rules.");
        EnableWarArena = config.Bind("Arena", nameof(EnableWarArena), false, "Retired legacy setting. Custom arena is excluded from this plugin regardless of this value.");
        EnableWarSpecializations = config.Bind("Native War", nameof(EnableWarSpecializations), true, "Additive audited enemy commander/branch profiles for wars 2,3,6,13 only; preserves the existing 21 profiles. Requires EnableWarUnitProfiles; restart required.");
        EnableNativeWarAI = config.Bind("Native War", nameof(EnableNativeWarAI), true, "Audited wars 2,3,6,13: native legal movement/attack/ability evaluation with commander strategies; independent of outcome remix.");
        ExportNativeWarResearch = config.Bind("Developer", nameof(ExportNativeWarResearch), false, "Developer-only read-only static war script export. No campaign entry or edits.");
        EnableTrainingWar = config.Bind("Native War", nameof(EnableTrainingWar), false, "Optional native War 1 training. Requires the normal-build ownership contract or protected harness.");
        EnableTrainingWar2 = config.Bind("Native War", nameof(EnableTrainingWar2), false, "Optional native War 2 fort defense; training ownership and original completion required.");
        EnableTrainingWar3 = config.Bind("Native War", nameof(EnableTrainingWar3), false, "Optional original War 3; training ownership and original completion required.");
        EnableTrainingChallenge = config.Bind("Native War", nameof(EnableTrainingChallenge), false, "Experimental War 1 commander-rout challenge in the disposable copy.");
        EnableTrainingVeteran = config.Bind("Native War", nameof(EnableTrainingVeteran), true, "Offer Veteran tactics for audited training drills.");
        EnableTrainingRewards = config.Bind("Native War", nameof(EnableTrainingRewards), true, "Award native Potch once for routing the enemy commander in a completed training drill.");
        EnableTrainingDefeatReturn = config.Bind("Native War", nameof(EnableTrainingDefeatReturn), false, "Experimental return after native training defeat in the disposable copy.");
        EnableNativeWarRemix = config.Bind("Native War", nameof(EnableNativeWarRemix), true, "Audited scenario remixes for native wars 1,2,3,6,13. Original ending/campaign scripts retained; per-war configuration and experimental acceptance limits in docs.");
        RunNativeWarSelfTest = config.Bind("Native War", nameof(RunNativeWarSelfTest), false, "Developer startup-only native outcome probe on disposable armies. Restores all replaced globals. Does not launch a map or load a save. Leave false for normal play.");
        EnableEnemyTargeting = config.Bind("Features", nameof(EnableEnemyTargeting), true, "Raider ordinary enemies prefer wounded legal candidates in the native normal single-target fallback selector. No boss/action-table edits.");
        EnableEnemyProfiles = config.Bind("Features", nameof(EnableEnemyProfiles), true, "Exact-ID final vanilla-relative HP/damage profiles for eligible ordinary enemies. Restart required.");
        EnableWarUnitProfiles = config.Bind("Features", nameof(EnableWarUnitProfiles), true, "Disposable in-battle commander stat/ability profiles. Original story scripts remain. Restart required.");
        EnableDeveloperTools = config.Bind("Developer", nameof(EnableDeveloperTools), true, "F8 toggles a read-only diagnostic overlay; F9 exports a snapshot. Does not launch an arena or alter saves.");
        EnableRuneBalancing = config.Bind("Features", nameof(EnableRuneBalancing), true, "Apply ReforgedNightmare/magic.json: base spell effects and scoped Wind of Sleep susceptibility. Restart required.");
        EnableCharacterBalancing = config.Bind("Features", nameof(EnableCharacterBalancing), true, "Apply ReforgedNightmare/characters.json: growth, affinities and existing rune slot thresholds. New game recommended. Restart required.");
    }
}
