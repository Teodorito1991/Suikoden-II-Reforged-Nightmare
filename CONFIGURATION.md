# Configuration

## Current v1.51.1 integration

Full Content Preview selects all22 supported Original Hall definitions and10 Hunts.
Original Luca's accepted implementation is unchanged and its exact DLL preserved.
Beast Rune victory/return/cleanup passed with a deliberately boosted disposable
party: this verifies mechanics, not normal-party balance. StoryProgression and
per-entry safety/configuration gates remain; Spoiler Mode defaults OFF and requires
confirmation. The three unresolved story witnesses can use that explicit mode.
Nightmare stays optional. Training's normal-build direct-save contract remains
OFF pending current-build restored autosave and fault-path acceptance. No XP.
[Current evidence and blockers](docs/FEATURE-COMPLETION-V1511.md).

Earlier sections below retain their milestone-specific historical claims.

## v1.51 current checkpoint

Original Luca's three-team gauntlet now has one exact-build owner completion,
private HP carry, final earned win, clean returns and original-party restoration.
22 Hall definitions/22 optional Nightmare profiles; Nightmare Luca and failure/
repeat-run acceptance remain open. `EnableHall_luca_gauntlet` is an opt-in.
**Unlock All Bosses** is a reversible CFG preference (default OFF), with an explicit
spoiler warning and second confirmation. It changes availability only, preserving
per-entry opt-outs. Use **Restore Story Progression** to reverse it. Party drafts
store revalidated IDs, not progression. Training's new native-save checks remain
experimental/default OFF; no commander XP. Beast Rune victory remains unverified.
[Exact evidence and limits](docs/LAST-CHALLENGE-V151.md).

The sections below retain their milestone-specific historical evidence.

## v1.49 Full Content Preview

Run `tools/prepare-full-content.ps1 -OutputDirectory <empty-folder>` to generate
a separate proposal. No installation, save discovery or overwrite occurs. All10
Hunts and16 legitimate story/bonus Hall entries are opted in. Nightmare is a separate
optional overlay; new per-entry CFG keys use `EnableHall_<stable_id_with_underscores>`.
All11 additions default false outside this proposal. Existing eight keys retain
their defaults. `EnableNightmareStoneRewards` now allows the selected profile's
audited growth stone, not only Power. Its25% offer remains conditional on empty
native loot and inventory acceptance. No developer bypass/unsafe scripted switch or
normal-build Army Training is enabled. [Exact choices](profiles/full-content/README.md).


Close the game before editing. JSON is read at startup. Keep valid syntax, types
and audited IDs. Unknown source records fail closed; there is no guard bypass.
Public defaults are packaging-only proposals, distinct from developer embedded
fallbacks and the user's existing settings. No local user configuration changed.

## Fresh Campaign proposal (v1.48)

Use `tools/prepare-fresh-campaign.ps1 -OutputDirectory <empty-staging-folder>`.
It generates configuration only and refuses existing content. It never discovers
or changes an installed game. [Decisions and instructions](docs/FRESH-CAMPAIGN-V148.md).
The proposal selects Normal, native EXP/Potch rates (1.00, unlike the legacy 0.85
Potch example below), core RPG and guarded story-war systems, Sun/Undying Watch,
and five source-backed Original Echoes. Scripted Legendary changes, protected
Army Training and Nightmare tiers are off. All definitions remain in source;
additional content is independently configurable. Existing personal files and
embedded defaults are unchanged. Review/merge deliberately before use.

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
| Hall of Echoes / EnableHallOfEchoes |false| Opt-in Hall rematches in the Castle Guild. Restart required |
| Hall of Echoes / EnableHallHarpy |false| Independent first Harpy opt-in; corrected witness is 21/10. Restart required |
| Hall of Echoes / EnableHallPestRat |true| Independent Pest Rat permission; witness is B6/02 under StoryProgression |
| Hall of Echoes / EnableHallGoldenWolf |false| Experimental native Golden Wolf rematch; corrected witness is 16/02 |
| Hall of Echoes / EnableHallSierra |false| Experimental native Sierra rematch; no proven normal story unlock; developer testing only |
| Hall of Echoes / EnableHallHarpySecond |false| Experimental second Harpy; no proven normal story unlock; developer testing only |
| Hall of Echoes / EnableHallStoneGolem |false| Native Stone Golem; source-backed C9/01 witness; restart required |
| Hall of Echoes / EnableHallAbomination |false| Native Abomination; source-backed 56/20 witness; restart required |
| Hall of Echoes / EnableHallWorm |false| Native first Worm; normal unlock unresolved, developer testing only |
| Hall of Echoes / EnableNightmareEchoes |false| Separate advanced tiers for Golden Wolf, Stone Golem and Abomination; original availability still required |
| Hall of Echoes / EnableNightmareStoneRewards |true| One 25% Stone of Power offer roll after an earned Nightmare victory with no native loot; original inventory acceptance required |
| Hall of Echoes / HallUnlockPolicy |StoryProgression| UnrestrictedTesting requires EnableDeveloperTools and changes only availability, never flags |
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

## Party Revolution

The BepInEx **Party Revolution** section contains `EnablePartyIdentity` and
`EnableStarBonds`. Both default to true and can be disabled independently.
Identity revisions apply at startup only to bundled character rows and respect
the existing character-module toggles. They use native growth and slot
progression. Star Bonds apply only in eligible Normal battles and can be
reviewed through **Party Strategy** at the Castle Guild.
[Profiles, requirements and caps](docs/PARTY-REVOLUTION-V140.md).

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

## v1.50 optional complex encounters and training

`EnableHall_worm_moth` and `EnableHall_beast_rune` default false independently.
Both require original source admission and read-only completion witnesses; neither
sets story flags. Final-boss victory/return remains unverified. Nightmare is a separate
opt-in and retains native controllers, with120% HP/1.08× relative damage once.
No Luca launch option is exposed.

`EnableTrainingNormalBuild=false` preserves the historical protected-harness gate.
Explicit true selects the experimental private-flag/native-return/save-fence contract
and also requires `EnableTrainingWar` and `EnableTrainingDefeatReturn`. Scenario,
Veteran and rewards options remain separate. Never interpret a startup or fixture
as full ordinary-install acceptance. The native post-return map autosave bypasses the GSD2 save-file hook; its five flags, war and party payloads passed the tested case, but direct-write
fault handling and other outcomes remain unverified. Keep this option off for a conservative campaign.
An unsafe return requires restart/reload before saving. No commander progression or new save bytes are written.
