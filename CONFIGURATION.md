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
| Native War / EnableWarManeuvers |true| Additional legal-candidate scoring in audited wars and Veteran drills |
| Native War / EnableTrainingVeteran, EnableTrainingRewards |true| Independent options for Veteran tactics and earned training Potch in the copied runtime |
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
| combat-reforged.json | Enabled; enemy AI, boss encounters, pacing, healing, sleep, EnableTeamwork and EnableArcaneGuard |
| combat-evolution.json | Enabled; formations, boss phases, additional bosses, Unites |
| combat-mastery.json | Enabled; formations, physical runes, Unites, boss cadence |
| combat-reality.json | Enabled; formations, physical runes, Pest Rat tactics |
| boss-tactics.json | Enabled; EnableExistingBosses=true, EnableMajorBosses=false |
| rpg-evolution.json | Separate character, magic, physical rune, Unite and formation toggles. Custom legacy character and spell values keep priority. |
| boss-revolution.json | Enabled; six v1.33 policies with individual Enabled switches |
| legendary-bosses.json | `EnableMistShade`, `EnableGorudo`, `EnableWormMoth`, `EnableLaterLucia`; each defaults to true |
| war-unit-profiles.json / war-specializations.json | Original commander/branch definitions |
| native-war-rules.json / total-war-rules.json | Original scenario objectives and AI |
| war-tactics.json | Enabled, EnabledWars, tactical weights/objectives |
| native-war-witness.json | Enabled=false; read-only diagnostics, never launches |
| castle-guild.json | Enabled=true, AutoRequestAcceptedContract=true, EnableFieldReissue=true in the recommended example |
| first-expansion.json | EnableExperimentalHunt=true; Sun HP/damage and VictoryBonusPotch=2500 |
| hunt-catalogue.json | All ten enabled in the recommended v1.38 example; independent Enabled and Potch, damage and unit overrides |
| hunt-guild.json | EnableSelector=true, EnableVictoryItems=true; fallback reward rules |

Other legacy boss definitions are under `BepInEx/config/NightmareRebalance`.
Setting a later module's Enabled=false removes only that layer; it is not a universal
vanilla toggle. Restart after edits. Copy only missing files when updating.

`EnableTeamwork` and `EnableArcaneGuard` default to true when absent. Set
either to false independently to opt out. Both apply only to
[the audited Normal formations and Hunts](docs/TACTICAL-REVOLUTION-V139.md).
Formation teamwork also requires existing Combat Reforged enemy AI.

## Optional content

Dreamer's Circuit and Thornwake Patrol are enabled in fresh v1.38 defaults.
Keep or explicitly change their `Enabled` values in any existing file. Set any
contract's `Enabled` to false to hide its acceptance. In an
older three-contract file the new rows take their bundled defaults in memory.
Existing selections, flags, overrides and fallback-item choices retain priority.
The loader does not rewrite the existing JSON. To customize a new entry, copy
that entry from the current configuration example.

Add `"EnableFieldReissue": true` to an older `castle-guild.json` to use the
local contract option. The setting is off when omitted. Open the original
party menu at an eligible hunting ground and use the on-screen prompt. It
takes a fresh contract for the current party. Contract completions still clear
on load. [Recommended Normal profile](docs/NIGHTMARE-CAMPAIGN-V138.md).
The earlier Lucia and final Neclord policies require `EnableMajorBosses=true`.
The v1.35 later Lucia records have their own `EnableLaterLucia` switch. All
v1.35 policies also require Normal and the existing combat AI and boss encounter
controls. Set the relevant family switch to false and restart to use the original
selector for that family. Campaign Luca tactics are unavailable.

Disable hunting with first-expansion.json's EnableExperimentalHunt=false; hide the
attendant separately with castle-guild.json's Enabled=false. Disable custom fallback
item preparation with EnableVictoryItems=false. Native drops still belong to the
game. Reporting pays nothing again. Contracts are session-only; selector preference
persistence is not campaign quest persistence. Castle Army Training has its own
opt-in and does not award commander XP. Its copied-runtime requirement uses the
stock transition trace marker and the active disposable save-root redirect.
The Guild menu lists campaign prerequisites for the four drills.
[Current training controls](docs/CASTLE-GUILD-TOTAL-WAR-V136.md).

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
