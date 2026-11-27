# Suikoden II: Reforged Nightmare

**Latest source development (uninstalled): v1.56 — Army Progression, Drill Directives, Cross-Theater Synergy & Optimizations.**
Adds opt-in persistent commander XP, military ranks (Recruit → Marshal),
Vanguard/Bulwark/Tactician Doctrines backed by a content-addressed save-bound
sidecar ledger (zero native save edits), tactical Drill Directives, RPG ↔ War
Cross-Theater Synergy, Castle Guild Roster/Codex views, and hot-path combat/war/
guild optimizations. Compiled and managed-tested; live gameplay acceptance is
pending. [Design, configuration and verification limits](docs/ARMY-PROGRESSION-OPTIMIZATIONS-V156.md).

**Installed for the owner's full gameplay playtest: exact v1.55.** Full Content
plus selectable Nightmare/growth rewards and the explicitly unverified training
defeat-return opt-in are applied. Spoiler Mode starts OFF; its normal warning and
second confirmation remain required. The actual Steam launcher loaded the plugin
and closed normally; all fourteen original saves are unchanged. No campaign was started.
[Installed modules, evidence and limitations](docs/INSTALLATION-STATUS-V155.md).
No new DLL/version, installable beta or GitHub Release was produced.

**First campaign recommendation: unchanged v1.55 + Full Content, in-game Normal.**
[Readiness decision and challenge watchlist](docs/FIRST-CAMPAIGN-READINESS.md) ·
[Short setup and fallback guide](docs/FIRST-CAMPAIGN-SETUP.md). No new DLL or tuning
was justified in the final review. Full managed/config checks passed; exact-C5
native evidence is preserved. The candidate is ready for owner campaign feedback,
not certified for every boss/optional path. The later installation is recorded
above; campaign start remains the owner's action. Note the supported castle
variant, session-only contracts and three
Hall entries without automatic story witnesses. No developer tools are required.

**Latest development: v1.55 — Story Boss Difficulty Completion.**
Full Content on Normal now selects22 audited story-boss encounter policies (up
from13). Lucia's two earlier records, final Neclord, Mist Shade, Gorudo, Worm/Moth
and three later Lucia variants gain existing tactical choices. A new command/owner/
callback guard protects the legacy major policies. No HP/damage increase. Original
Luca and Beast Rune controllers remain native; Hall rematches are separate.
[Before/after, exclusions and native evidence](docs/STORY-BOSS-COVERAGE-V155.md).
This is native/component and launcher verification, not new story-battle acceptance.
Personal settings are not migrated; generate the separate Full Content profile.

**Latest development: v1.54 — Great Balance Audit.** Native/source comparison finds
uneven campaign pressure, with role-based enemy scaling rather than stacked global
multipliers. Full Content now recovers one level1–2 rune charge per eligible victory;
levels3–4 retain their resource cost. Existing configurations default to the legacy
level4 ceiling unless deliberately changed. No character/enemy stats or rewards changed.
[Quantitative report](docs/BALANCE-AUDIT-V154.md) ·
[Nine-stage pre-change baseline](docs/BALANCE-BASELINE-V154.md) ·
[All80 character comparisons](docs/BALANCE-CHARACTERS-V154.md).
Automated/native component checks and startup are separate from gameplay acceptance;
v1.53 below remains the latest owner-accepted exact DLL. No full-campaign certificate.

**Latest accepted development checkpoint: v1.53 — Army Training.**
Standard Commander Rout passed an earned victory, correct castle return, real
native autosave/readback and reload on the exact recorded DLL. Full Content
Preview now selects four independent native drills with supported Standard/Veteran
choices and750/1,500-Potch qualifying rewards, alongside22 Original Hall entries,
10 Hunts and the RPG/original story-war overhaul. Only Commander Rout Standard was
played on this DLL; other drills/tiers retain historical evidence. Original campaign
prerequisites remain. Defeat return and Spoiler Mode are OFF; Nightmare is optional.
Commander XP is unimplemented. [Exact acceptance](docs/ARMY-TRAINING-RESULTS-V153.md).

Generate the [Full Content Preview](profiles/full-content/README.md) to a new folder;
it does not install a DLL or overwrite personal settings. The separate conservative
Fresh Campaign profile remains unchanged. No whole-campaign certification or beta
publication. Original Luca and boosted-party Beast Rune evidence retain their own
historical DLL hashes; they were not replayed for this training acceptance.

**Campaign development baseline** is v1.40, with DLL
`1.40.0-party-revolution.1`. Fourteen revised recruits and four conditional
Star Bonds give Normal parties new ways to prepare and fight. The Castle Guild's
Party Strategy page shows combinations, their triggers and their effects.
[Party Revolution profiles and verification](docs/PARTY-REVOLUTION-V140.md).
[Hall of Echoes engineering handoff](docs/HALL-OF-ECHOES-ENGINEERING-HANDOFF-V141.md)
records the earlier native fixture investigation. Battle guests remain unavailable.
[Tactical profiles and v1.39 evidence](docs/TACTICAL-REVOLUTION-V139.md).
The source repository is public. The installable beta remains unpublished.
[Castle Guild and Total War changes](docs/CASTLE-GUILD-TOTAL-WAR-V136.md).
[Hunt catalogue and verification](docs/GRAND-GUILD-V137.md).
[Recommended campaign setup and v1.38 verification](docs/NIGHTMARE-CAMPAIGN-V138.md).

An unofficial Windows Steam gameplay overhaul and expansion for **Suikoden II HD
Remaster**, using the original RPG and tactical war engines, artwork, animations,
maps and interfaces. Normal is the intended difficulty foundation. Optional
hunting rewards preparation and grinding without becoming mandatory.

**Overall campaign status: experimental.** [Known issues](KNOWN_ISSUES.md).

## What changes

- **80 character profiles** and **31 spell adjustments** for more build choices.
- **280 regular enemy profiles** and **58 remixed formations**, with guarded native
  action/target selection, damage, healing and encounter changes.
- **22 boss encounter policies**, including six scripted encounter records with
  attack-only changes. Solo Luca has a native dispatcher fixture and remains
  disabled in campaign play. [Boss evidence](docs/LEGENDARY-BOSSES-V135.md).
- **Ten physical techniques** and **fourteen Unite** improvements.
- Victory rune regeneration: one missing charge per eligible survivor, starting
  at the lowest depleted spell level by default.
- The **Castle Guild attendant beside the Suggestion Box** in the audited
  headquarters variant.
- Voluntary contracts, original-engine battles, repeatable Potch and guarded item
  rewards. Accept, travel, hunt, return and report without developer hotkeys.
- Original-engine story-war unit, AI and objective modifications.

## Castle Army Training

The native Guild offers War1 Training, Fort Defense, Commander Rout and War3
Training, gated by their original story completions and audited definitions.
Standard/Veteran qualifying victories earn750/1,500 Potch through the native purse.
Defeat, timed endings without a commander rout and invalid returns do not pay.
The v1.53 normal-build return/save mechanism passed Commander Rout Standard, actual
autosave readback and reload. It does not require a developer gameplay harness.
Independent test I/O protection was used only to preserve the Steam install/saves.
Full Preview selects the four drills; individual opt-outs remain. Native defeat is
unchanged with the optional redirect OFF. No commander XP or training stat bonus.
[Results and remaining limits](docs/ARMY-TRAINING-RESULTS-V153.md).

## Working from source

Read the build guide for the supported Windows Steam build and BepInEx
dependencies. GitHub's Code ZIP contains source files for developers. Normal
is the baseline for campaign testing. [Build guide](BUILDING.md).

## What has actually been played

On **1.12.2-guild-ui.1**, a protected user-controlled Normal session demonstrated
Sun King's Retinue: NPC interaction, readable selection, automatic Cave encounter,
two genuine victories, native Stone of Power receipt, rune recovery, field return,
report-back and repeat entry. The second reward actually credited **3,478 Potch**;
the first met the native money cap.

That evidence belongs to DLL SHA256:
`0B520FDD9182B6C024FE96376D0F284BE249DF93AF2E748DC331D21324910F46`.
It does not certify the whole campaign or later development-build gameplay. Dreamer's Circuit and
Thornwake Patrol are native-tested prototypes, disabled in the proposed defaults.
[Exact-build evidence](docs/ACCEPTANCE.md).

## A look at the hunt

![Sun King and Sorcerer in the original RPG engine](docs/media/historical-hunt-debug-battle.png)

*Historical user-supplied hunt-debug screenshot. Damage protection was ON. This
illustrates the engine, not fair balance or acceptance of either candidate DLL;
its exact DLL identity is not independently recorded.*

[Historical reward screen and pending captures](docs/media/README.md).
No synthetic mockup is presented as gameplay proof.

## Documentation

[Features](FEATURES.md) · [Configuration](CONFIGURATION.md) · [Hunt Guild](HUNT_GUILD.md)
· [Known issues](KNOWN_ISSUES.md) · [Changelog](CHANGELOG.md)
· [Building](BUILDING.md) · [Contributing](CONTRIBUTING.md) · [Credits](CREDITS.md)

Original mod code is **MIT-licensed**, approved by the owner. External notices are
retained in [licenses](licenses); see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
No game binaries, loader, generated interop, extracted assets or saves are supplied.
Screenshots depict proprietary game content and are not relicensed as mod code.
This unofficial project is not affiliated with or endorsed by KONAMI.
