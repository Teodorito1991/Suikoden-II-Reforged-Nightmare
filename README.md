# Suikoden II: Reforged Nightmare

**Latest development checkpoint: v1.52 — Army Legacy save-contract fixes.**
v1.53 is awaiting the single real Commander Rout/restored-autosave acceptance run;
execution approval is unavailable in the current session. No new DLL or default
training enablement. [Short acceptance plan](docs/ARMY-TRAINING-ACCEPTANCE-V153.md).

Rejected training saves now finish their native failure callbacks and cannot
invalidate a good return just by being attempted. Native-entry/component checks
passed; a current-build played drill/restored autosave remains unobserved after
automatic approval review blocked the test-copy setup. Normal-build training stays
OFF; persistent commander XP remains unimplemented. The Full Content Preview
retains22 supported Original echoes,10 Hunts and the RPG/original story-war overhaul.
[Exact evidence and remaining limits](docs/ARMY-LEGACY-V152.md).
Original Luca and boosted-party Beast Rune evidence retain their historical DLLs.

The separate **Full Content Preview** selects ten Hunts and compatible Hall content.
The [v1.48 Fresh Campaign baseline](docs/FRESH-CAMPAIGN-V148.md) and v1.49 fallback
are preserved. [Generate configurations](profiles/full-content/README.md).
Army Training retains its default-off normal-build ownership contract. Historical
Commander Rout return/autosave evidence and current native-entry rejection tests
are separate. One genuine v1.52 restored autosave check remains pending. There is
no commander XP or persistent training stat system.

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

The existing original-engine system offers four drills on three maps, with
Standard/Veteran choices and qualifying750/1,500-Potch rewards. Historical native
wins, defeats and returns retain their recorded DLLs. v1.50's normal-contract
Commander Rout win/return and copied autosave comparison passed; v1.52 adds real
native-entry rejection/failure-callback fixtures. It has no new played drill or
restored autosave after the test-copy preparation was blocked. Normal-build training
therefore remains OFF in generated campaign profiles. Commander XP/story-war
training bonuses are unimplemented. [Current evidence](docs/ARMY-LEGACY-V152.md).

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
