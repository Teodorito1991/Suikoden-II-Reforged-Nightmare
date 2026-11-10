# Configuration

Close the game before editing. JSON is read at startup. Keep valid syntax, types
and audited IDs. Unknown source records fail closed; there is no guard bypass.
Public defaults are packaging-only proposals, distinct from developer embedded
fallbacks and the user's existing settings. No local user configuration changed.

## Main CFG

`BepInEx/config/nightmare.suikoden2.reforged.cfg`

| Section / setting | Public value | Purpose |
|---|---|---|
| Difficulty / EnemyHpMultiplier |1.60| Fallback HP; specific final profiles may override |
| Difficulty / EnemyDamageMultiplier |1.45| Fallback enemy calculated damage |
| Difficulty / ExperienceMultiplier |1.00| Original XP rate |
| Difficulty / PotchMultiplier |0.85| Regular native reward factor |
| Rune Regeneration / EnableRuneRegeneration |true| Completed victory only |
| Rune Regeneration / RuneChargesPerVictory |1| Total budget per eligible character, not per spell level |
| Rune Regeneration / RuneRecoveryPriority |LowestDepletedLevel| Also supports HighestDepletedLevel |
| Rune Regeneration / RequireSurvivingCharacter |true| Excludes defeated/incapacitated participants |
| Features / EnableCharacterBalancing |true| Character profiles |
| Features / EnableRuneBalancing |true| Base spell layer; physical runes have separate JSON controls |
| Features / EnableEnemyProfiles, EnableEnemyTargeting |true| Regular enemy profiles/targeting |
| Combat Reforged / EnableCombatReforgedAi |true| Native RPG selection layer |
| Features / EnableWarUnitProfiles |true| Original-war commander profiles |
| Native War / EnableNativeWarRemix |true| Guarded story-war objectives |
| Native War / EnableNativeWarAI |true| Legal native war evaluation |
| Native War / EnableWarSpecializations, EnableWarTactics |true| Additional original-war layers |
| Developer / EnableDeveloperTools |false| Optional diagnostics/shortcuts; not needed for Guild |
| Developer / EnableHuntDebugTools |false| Keep hunt protection unavailable in ordinary play |
| Developer / ExportNativeWarResearch |false| Developer static export only |
| Combat Reforged / RunCombatSelfTest |false| Startup probe, never ordinary play |
| Native War / RunNativeWarSelfTest |false| Startup probe, never ordinary play |
| Arena / EnableWarArena |false| Retired setting; cannot revive the arena |

The final enemy factor can come from a profile, pacing or hunt policy; the fallback
is not a promise that every factor multiplies together. Custom profile priority
and exactly-once rules remain intact. Experience/hunt rewards are separate controls.
Legacy EnableBossProfiles/EnableScriptedBossOverrides are false publicly and are
distinct from newer boss-tactics JSON. Do not enable every switch indiscriminately.

## Module JSON under BepInEx/config/ReforgedNightmare

| File | Controls |
|---|---|
| characters.json / magic.json | Per-character/spell authored values |
| enemy-profiles.json | Exact-ID regular profiles |
| combat-reforged.json | Enabled; enemy AI, boss encounters, pacing, healing, sleep |
| combat-evolution.json | Enabled; formations, boss phases, additional bosses, Unites |
| combat-mastery.json | Enabled; formations, physical runes, Unites, boss cadence |
| combat-reality.json | Enabled; formations, physical runes, Pest Rat tactics |
| boss-tactics.json | Enabled; EnableExistingBosses=true, EnableMajorBosses=false |
| war-unit-profiles.json / war-specializations.json | Original commander/branch definitions |
| native-war-rules.json / total-war-rules.json | Original scenario objectives and AI |
| war-tactics.json | Enabled, EnabledWars, tactical weights/objectives |
| native-war-witness.json | Enabled=false; read-only diagnostics, never launches |
| castle-guild.json | Enabled=true, AutoRequestAcceptedContract=true |
| first-expansion.json | EnableExperimentalHunt=true; Sun HP/damage and VictoryBonusPotch=2500 |
| hunt-catalogue.json | Sun enabled; Dreamer/Thornwake disabled; per-hunt overrides |
| hunt-guild.json | EnableSelector=true, EnableVictoryItems=true; fallback reward rules |

Other legacy boss definitions are under `BepInEx/config/NightmareRebalance`.
Setting a later module's Enabled=false removes only that layer; it is not a universal
vanilla toggle. Restart after edits. Copy only missing files when updating.

## Optional content

Unplayed contracts require an explicit Enabled opt-in for that catalogue entry.
Major Lucia/Neclord policies require EnableMajorBosses=true. Both have native-only
coverage and are not recommended for the first public campaign.

Disable hunting with first-expansion.json's EnableExperimentalHunt=false; hide the
attendant separately with castle-guild.json's Enabled=false. Disable custom fallback
item preparation with EnableVictoryItems=false. Native drops still belong to the
game. Reporting pays nothing again. Contracts are session-only; selector preference
persistence is not campaign quest persistence. No setting enables skirmishes or XP.

## Development-only war bridge

v1.14 adds no gameplay switch, commander reward or contract option. Its native
resource probe requires explicit unloaded batch arguments and an isolated runtime;
RunNativeWarSelfTest=true alone does not schedule it. Existing examples remain
unchanged and no user opt-ins are migrated. Live launching stays unavailable.

v1.15 adds no player-facing setting or migration. The resource lifecycle probe is
also disabled in ordinary play and cannot grant chapter-launch permission. Shared
or unknown cleanup ownership is not overridden by a JSON setting.


## v1.16 development note

v1.16 introduces no player setting or configuration migration. The native UI probe is an explicit developer-only unloaded test path, not a new menu or enabled feature. Independent skirmish launching remains hard-disabled; existing opt-ins are preserved.


## v1.17 development note

v1.17 adds no player setting, migration or changed defaults. Existing RPG/Guild/war settings are preserved; the independent launch gate is still hard-disabled.


## v1.18 development note

No player setting/default/migration added. Passive observer requires explicit private-test arguments; it does not run in ordinary gameplay. Field retention and independent launching stay hard-disabled.


## v1.19 development note

v1.19 changes no user/default settings. Dreamer and Thornwake remain disabled by default; deliberate existing opt-ins are preserved. Item labels describe conditional fallback loot, not guaranteed grants. Original purse/inventory handling and session-only quests remain.


## v1.20 development note

Boss tactics now accepts optional EnableCombatQuality (default true) for Pest Rat, both Harpy encounters, Sierra and both Golden Wolf encounters. Missing key in existing files is valid. Set false to use the base boss tactics without the combat-quality layer; all previous opt-outs still apply. Major means Lucia lus1/lus2 and final Neclord nec3, NOT Luca Blight. Actual Luca tables luc1/luc2/luc3 are not covered. Boss tactics stay Normal-only; Hard regular-enemy compatibility is separate.
