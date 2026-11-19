# Suikoden II: Reforged Nightmare

**Latest development milestone: v1.49 — The Grand Expansion.**
19 Hall rematch definitions (eleven additions), 19 optional Nightmare profiles and
four native growth-stone reward choices. Sixteen entries have read-only story or
explicit bonus unlocks; three remain normal-locked. New group/phase integrations
have native setup evidence; Mist Shade and Rowd's Patrol also passed earned
victory/return/Guild reopen on this DLL. Other additions remain native-only.
[Coverage, blocked records and verification](docs/GRAND-EXPANSION-V149.md).

A separate **Full Content Preview** selects all ten Hunts and compatible Hall
content without developer bypasses or changing personal settings. The conservative
[v1.48 Fresh Campaign baseline](docs/FRESH-CAMPAIGN-V148.md) and its genuine
Undying Watch/early-battle evidence remain preserved.
[Generate the full-content configuration](profiles/full-content/README.md).
Army Training remains protected-copy-only; no commander XP is advertised.

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

The opt-in training system uses three original scenarios and offers four
drills through the Guild. Choose a drill and then Standard or Veteran.
Historical tests in the copied runtime returned to Leona's tavern. On v1.32,
Veteran War 3 returned after
its timer and Veteran Commander Rout returned after an earned victory.
Commander Rout earned 1,500 Potch at the purse cap. A below-cap payout,
War 3 defeat return, and a second consecutive War 3 entry remain unverified.
Commander experience remains deferred. v1.36 captures the player's real
standing point at the new Guild corridor. A live training return there has yet
to be observed. [Current controls and evidence](docs/CASTLE-GUILD-TOTAL-WAR-V136.md).

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
