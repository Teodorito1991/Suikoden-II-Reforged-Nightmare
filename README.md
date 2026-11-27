# Suikoden II: Reforged Nightmare

An unofficial Windows Steam gameplay overhaul and expansion for **Suikoden II HD Remaster** (BepInEx 6 IL2CPP x64), built strictly on the original RPG and tactical war engines, artwork, animations, maps, and interfaces. **Normal** is the intended difficulty foundation.

> **Status:** Experimental. All native save files (`gsd2/DataXX`) stay 100% vanilla; new progression features store snapshots in a content-addressed sidecar ledger. No GitHub Release or installable beta has been published.

---

## Current Builds

| Track | Version | Status | Details |
|---|---|---|---|
| **Latest Source (Development)** | `v1.56` dev | Compiled (`0 warnings/errors`) + full managed/CI suite passing; **live gameplay playtest pending** | [v1.56 Features & Optimizations](docs/ARMY-PROGRESSION-OPTIMIZATIONS-V156.md) |
| **Installed Owner Playtest** | `1.55.0-story-bosses.1` (`C5BE…7055E`) | Launcher/plugin startup verified on Steam; Full Content + playtest opt-ins; 14 saves untouched | [v1.55 Installation Status](docs/INSTALLATION-STATUS-V155.md) |
| **Latest Accepted Checkpoint** | `1.53.0-army-training.1` (`5414…A286`) | Owner-played Commander Rout Standard victory, castle return, autosave readback & reload passed | [v1.53 Acceptance](docs/history/ARMY-TRAINING-RESULTS-V153.md) |

---

## Key Features

### RPG Combat & Party Overhaul
- **80 character profiles** and **31 spell adjustments**, plus **10 physical techniques** and **14 Unite** improvements.
- **280 regular enemy profiles** and **58 remixed formations** with guarded tactical AI, teamwork, and Arcane Guard.
- **22 audited story-boss encounter policies** (no blanket HP/damage inflation; campaign Luca and Beast Rune remain native-controlled).
- **10 Star Bonds** (4 Unite-backed + 6 lore pairings including *Riou & Nanami*, *Flik & Viktor*, *Luc & Mazus*, *Kahn & Sierra*, *Georg & Killey*, and *Kasumi & Mondo*).
- **Victory Rune Regeneration:** Restores 1 missing level 1–2 charge per eligible survivor (Full Content default).
- **Optional In-Battle Tactical HUD Pill** showing active Star Bonds and armed Morale charges.

### Castle Guild, Hunts & Hall of Echoes
- **Castle Guild Attendant** beside the Suggestion Box (`vk07`), with an optional **Multi-NPC Castle Hub** adding a flanking **Drill Sergeant** (direct Army Training) and **Echo Archivist** (direct Hall of Echoes).
- **10 Voluntary Hunt Contracts** with repeatable Potch, guarded fallback item rewards, and optional **Hunt Risk Modifiers** (*Frenzied Quarry* / *Ironclad Quarry* for `+35%` bonus Potch).
- **Hall of Echoes:** 22 supported Original boss rematches, optional Nightmare tiers with conditional growth stones, Spoiler Mode with double confirmation, and **Hall Mastery Medals** (best turn count & No-KO badges).
- **Polished Guild Menu:** Dynamic context-aware NPC greetings, Left/Right fast page-flipping, hold-to-repeat scrolling, animated cursor, breadcrumbs, XP progress bars, and a **Tactician's Codex**.

### Tactical War & Permanent Army Progression
- **Original Story-War Overhaul** (unit profiles, tactical AI, specializations, maneuvers, and audited objectives).
- **Castle Army Training:** 4 replayable native drills (*War 1*, *Fort Defense*, *Commander Rout*, *War 3*) with Standard/Veteran tiers, private flag-forking, and save-fence protection.
- **Permanent Army Progression (`v1.56` opt-in):** Commanders earn persistent XP from Army Training and Story Wars across 5 ranks (*Recruit → Marshal*), unlock **Vanguard / Bulwark / Tactician** Doctrines using safe native `SP_*` abilities, and trigger **War Unit Synergy Pairs** (*Flik+Viktor*, *Shu+Apple*, *Luc+Mazus*, *Humphrey+Georg*) — all stored in a **content-addressed save-bound sidecar ledger** (`army-ledger.json`) with zero native save edits.
- **Drill Directives & Cross-Theater Synergy (`v1.56` opt-ins):** *Iron Wall*, *No Field Medicine*, and *Forced March* drill modifiers; Veteran drills arm `+1` RPG morale rune charge and Hall Nightmare clears grant `+35 War XP`.

---

## Repository Layout

```text
├── src/NightmareRebalance/
│   ├── Core/           # Plugin bootstrap, settings, rules, coverage & maintenance
│   ├── Combat/         # RPG combat overhaul, AI, teamwork, Star Bonds & targeting
│   ├── Bosses/         # Story-boss tactics, profiles & legendary boss guards
│   ├── War/            # Tactical war overhaul, AI, Army Training & save fence
│   ├── Progression/    # Permanent Army Progression ledger, ranks, doctrines & synergy
│   ├── Guild/          # Castle Guild NPCs, menu navigation & presentation
│   ├── Hall/           # Hall of Echoes rematches, Nightmare tiers & Luca gauntlet
│   ├── Hunts/          # Hunt Guild contracts, sessions & reward tracking
│   ├── Probes/         # Native/component diagnostic probes
│   └── Content/        # Audited JSON balance & baseline definitions
├── tests/              # Deterministic managed regression suite (36+ suites)
├── profiles/           # Fresh Campaign & Full Content configuration profiles
├── tools/              # Profile generators, playtest staging & publication audit
├── docs/               # Current guides & docs/history/ milestone archive
└── publication/        # Active build manifests & publication/history/ archive
```

---

## Building & Testing from Source

Requires the supported Windows Steam build with BepInEx 6 IL2CPP x64 (see [BUILDING.md](BUILDING.md) and [INSTALL.md](INSTALL.md)):

```powershell
# Build Release DLL and run the full managed regression suite
./build.ps1

# Generate a staged configuration profile (never touches Steam or saves)
./tools/prepare-fresh-campaign.ps1 -OutputDirectory .\staging-fresh
./tools/prepare-full-content.ps1   -OutputDirectory .\staging-full
./tools/prepare-v156-playtest.ps1  -OutputDirectory .\staging-v156
```

---

## Documentation

[Docs Index](docs/README.md) · [Features](FEATURES.md) · [Configuration](CONFIGURATION.md) · [Hunt Guild](HUNT_GUILD.md) · [Known Issues](KNOWN_ISSUES.md) · [Changelog](CHANGELOG.md) · [Building](BUILDING.md) · [Contributing](CONTRIBUTING.md) · [Credits](CREDITS.md)

---

## License & Legal

Original mod code is **MIT-licensed** ([LICENSE](LICENSE)). External notices are in [licenses/](licenses) and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). No game binaries, loader, generated interop, extracted assets, or saves are supplied. Screenshots depict proprietary game content and are not relicensed as mod code. This unofficial project is not affiliated with or endorsed by KONAMI.
