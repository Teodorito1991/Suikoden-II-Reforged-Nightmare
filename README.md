# Suikoden II: Reforged Nightmare

**A familiar world. More demanding battles. A reason to go hunting.**

An unofficial Windows Steam gameplay overhaul and expansion for **Suikoden II HD
Remaster**, using the original RPG and tactical war engines, artwork, animations,
maps and interfaces. Normal is the intended difficulty foundation. Optional
hunting rewards preparation and grinding without becoming mandatory.

**Pre-publication beta preparation. Overall campaign: experimental.**
Main development source is **v1.13 Native War Foundation**. The recommended first
beta uses the unchanged **v1.12.2 Castle Hunt Guild DLL**, which has stronger exact-
binary gameplay evidence. No public release has been approved or published.
[Compare both candidates](docs/RELEASE-CANDIDATES.md).

## What changes

- **80 character profiles** and **31 spell adjustments** for more build choices.
- **280 regular enemy profiles** and **51 remixed formations**, with guarded native
  action/target selection, damage, healing and encounter changes.
- Boss tactical policies, **six physical rune** and **eight Unite** improvements.
- Victory rune regeneration: one missing charge per eligible survivor, starting
  at the lowest depleted spell level by default.
- A real **Hunt Guild attendant inside Leona's tavern** in the audited castle variant.
- Voluntary contracts, original-engine battles, repeatable Potch and guarded item
  rewards. Accept, travel, hunt, return and report without developer hotkeys.
- Original-engine story-war unit, AI and objective modifications.

**Repeatable native war skirmishes, commander XP and persistent army training are
NOT playable features.** v1.13 adds an isolated flag-write rehearsal and read-only
field-owner prerequisites. Its independent launcher remains unavailable. The
retired custom Unity arena is not part of the playable mod.

## Start here

1. Read [INSTALL.md](INSTALL.md) for the exact supported game and loader.
2. Install the reviewed installation ZIP while closed; use **Normal**.
3. Begin an experimental campaign or use a backed-up compatible save.
4. At headquarters, visit [the Hunt Guild](HUNT_GUILD.md). Ordinary contracts need
   no F6, F10 or Ctrl shortcut. Loading/restarting clears session-only contracts.

Players need no Codex, Visual Studio or developer script. GitHub's Code ZIP is
source, **not** the installation package.

## What has actually been played

On **1.12.2-guild-ui.1**, a protected user-controlled Normal session demonstrated
Sun King's Retinue: NPC interaction, readable selection, automatic Cave encounter,
two genuine victories, native Stone of Power receipt, rune recovery, field return,
report-back and repeat entry. The second reward actually credited **3,478 Potch**;
the first met the native money cap.

That evidence belongs to DLL SHA256:
`0B520FDD9182B6C024FE96376D0F284BE249DF93AF2E748DC331D21324910F46`.
It does not certify the whole campaign or v1.13 gameplay. Dreamer's Circuit and
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
