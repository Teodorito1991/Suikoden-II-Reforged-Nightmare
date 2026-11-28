# Suikoden II: Reforged Nightmare

An unofficial Windows Steam gameplay overhaul and expansion for **Suikoden II HD Remaster** (BepInEx 6 IL2CPP x64), built strictly on the original RPG and tactical war engines, artwork, animations, maps, and interfaces. **Normal** is the intended difficulty foundation.

> **Status:** Experimental. All native save files (`gsd2/DataXX`) stay 100% vanilla; all Reforged progression (Commanders, Commendations, Castle Upgrades, Bestiary, Honors, and Bounties) is stored in a content-addressed sidecar ledger (`army-ledger.json`). No GitHub Release or installable beta has been published.

---

## Current Builds

| Track | Version | Status | Details |
|---|---|---|---|
| **Latest Source (Development)** | `9.0.0-stabilization.2` | Compiled (`0 warnings/errors`) + managed/profile/audit verification passing; **live gameplay acceptance pending** | [Changelog](CHANGELOG.md) |
| **Installed Owner Playtest Fallback** | `1.55.0-story-bosses.1` (`C5BE…7055E`) | Launcher/plugin startup verified on Steam; Full Content + playtest opt-ins; 14 saves untouched | [v1.55 Installation Status](docs/INSTALLATION-STATUS-V155.md) |
| **Latest Accepted Checkpoint** | `1.53.0-army-training.1` (`5414…A286`) | Owner-played Commander Rout Standard victory, castle return, autosave readback & reload passed | [v1.53 Acceptance](docs/history/ARMY-TRAINING-RESULTS-V153.md) |

---

## Honest Feature-Status Matrix (`9.0.0-stabilization.1`)

| Subsystem | Default | Verification Level | Known Limits / Notes |
|---|---|---|---|
| **Core RPG Rebalance, Star Bonds, Hunts, Original Hall (22), Luca Gauntlet, Story Wars, Native Army Training** | Preserved (`config.example.cfg`) | Managed + profile + historical native/gameplay checkpoints (`v1.53`–`v1.55`) | Retains strict `vk07` Hall invariants and untouched native saves |
| **Army Progression Ledger (`army-ledger.json`)** | `EnableArmyProgression = false` | Managed regression tests (600+ save slot retention, `.bak` recovery, fail-closed corrupt write-block) | Live async `SystemSave.Save` callback ordering awaits owner save/reload test |
| **Owned Optional Encounters (Tower of Dunan, 4 Secret Superbosses, Sindar Spire Guardians)** | Opt-in via Castle Hub / Hall | Managed `OptionalEncounterLease` regression tests (exact-once payout, unrelated-battle isolation, abort teardown) | Live in-game boss completion & Tower post-victory heal await owner playtest |
| **True Nightmare Combat Rework (Shields, Stagger, Turn-Scoped Charge, Traits, Hazards, Defend-Heal, Fatigue)** | `EnableTrueNightmareRework = false` | Managed `EnemyChargeTracker` AoE/Defend/Interrupt & rule tests | `EnableAntiOverlevelScaling` is now independently configurable (`false` default) |
| **Sindar Spire Gauntlet (6 Logical Stages, 8 Red Orbs, Puzzles, Shrines)** | Accessible from `vk07` Hooded Stranger | Managed rules & dialogue tests | **Mod-authored 6-stage logical gauntlet inspired by Sajah cut-dungeon lore** — NOT recovered original cut maps or 6 distinct native floors |
| **Multi-Room Castle NPCs (`vk01`–`vk08`)** | `EnableExperimentalCastleRoomNpcs = false` | Build-verified with `BGCON.EventMapAtariCheckF1` + `tokusyu_atari` trigger-clearance & generic `v*` 4×12 sprite filter | Explicitly gated experimental; live room-by-room visual check pending |
| **Sindar Ruins (`vc12`) Native Map Warp** | `EnableExperimentalSindarDungeonWarp = false` | Build-verified with idle/save/script/war preconditions | Explicitly gated experimental; reuses native `vc12` with logical floor state; live warp/return acceptance pending |

---

## Key Features

### RPG Combat Overhaul (Reforged 2.0 – 4.0)
- **80 character profiles**, **31 spell adjustments**, **10 physical techniques**, and **14 Unite** improvements.
- **280 regular enemy profiles**, **58 remixed formations**, and **22 audited story-boss encounter policies** (campaign Luca and Beast Rune remain native-controlled).
- **10 Star Bonds** (4 Unite-backed + 6 lore pairings) and **Victory Rune Regeneration**.
- **Party Momentum & Dunan Overdrive:** Combos, Surges, Ripostes, and boss hits charge a `0–100%` gauge; reaching `100%` triggers **Dunan Overdrive** (`+20%` damage dealt, `-15%` damage taken).
- **Tactical Combos & Synergies:** *Shatter Strike* (Sleep), *Venom Rend* (Poison), *Arcane Chain* (2nd spell), *Elemental Overload* (3rd+ spell, `+30%`), *Spellblade Resonance* (spell → physical follow-up), *Execute Finisher* (`<= 20%` HP), *First Strike* (Turn 0/1 opener), *Armor Break* (`[BRK x1..x3]` focus-fire stacks), *Desperation Surge* (`<= 25%` HP), *Riposte* (`>= 20%` max HP hit), *Phalanx* / *Crossfire* row formations, and *Last Stand Aura* (3+ KOs).
- **Weapon Milestone Perks** (Lv 8 / 12 / 16, or Lv 6 / 10 / 14 with *Master Blacksmiths*), **Boss Phase Telegraphs & 25% HP Enrage**, **Wild Apex Packs with Elite Affixes** (*Frenzied*, *Armored*, *Ward-Blessed*, *Golden*), **Battle Grade (`S/A/B/C`) + MVP**, and **Victory Streaks**.

### Castle Guild, Quartermaster & Progression
- **Classic Suikoden II Guild UI** with a **4-NPC Castle Corridor Hub**: Guild Attendant, **Drill Sergeant**, **Echo Archivist**, and **Castle Quartermaster**.
- **Dunan Commendations & Quartermaster:** Exchange Commendations for **8 Permanent Castle Upgrades** (*Field Medic Corps*, *Master Blacksmiths*, *Scout Network*, *Tactical Reserve*, *War College*, *Imperial Treasury*, *Arcane Academy*, *Grand Armory*) and **10 audited rare Runes, Growth Stones, and supplies**.
- **10 Voluntary Hunt Contracts** with Hunter Rank (*Bronze → Master*) and **Hunt Risk Modifiers** (`+35%` Potch), plus **5 Rotating Field Bounties**.
- **Hall of Echoes:** 22 supported Original boss rematches, optional Nightmare tiers with conditional growth stones, Spoiler Mode, and **Hall Mastery Medals**.
- **Hunter's Bestiary & 15 Persistent Dunan Honors:** Auto-tracks defeated species (`+1` Commendation per 10 species) and campaign achievements (`+2` Commendations each).

### Tactical War & Custom Skirmishes
- **Original Story-War Overhaul** (unit profiles, tactical AI, specializations, maneuvers, and audited objectives).
- **Army Training & 10 Custom Skirmishes (I–X):** Replayable native drills plus 10 custom escalating war skirmishes (up to *Skirmish X: Dunan Finale*, `6,000` Potch & `+8` Commendations) under private flag-forking, automatic `sub_to_leader` restoration, and save-fence protection.
- **Permanent Army Progression:** 5 Commander Ranks (*Recruit → Marshal*), **Vanguard / Bulwark / Tactician** Doctrines, **Allied War Stances**, **War Mutators** (*Blood Moon*, *Iron Attrition*), **Drill Directives**, **War Unit Synergy Pairs**, and **Live War Command Bar**.

---

## In-Game Hotkeys

| Key | Action |
|---|---|
| `F2` | Toggle **Clean HUD Mode** (hide/restore corner tactical pill & War Command Bar) |
| `F3` | Toggle **Reforged 4.0 Quick Guide + Active Field Bounty** card |
| `F4` | Toggle **Battle Tactical Scanner** (enemy names, levels, weaknesses, `[BRK]` stacks) + **Party Buff Strip** |
| `F5` | Cycle **Hunter's Bestiary → 15 Dunan Honors → Commander's Dossier → Off** |
| `F6` | Accept Local Field Hunt contract |
| `← / →` | Fast page-flip inside Castle Guild menus |

---

## Repository Layout

```text
├── src/NightmareRebalance/
│   ├── Core/           # Plugin bootstrap, settings, rules, coverage & maintenance
│   ├── Combat/         # RPG combat overhaul, AI, teamwork, Star Bonds & Reforged 2.0-4.0 runtime
│   ├── Bosses/         # Story-boss tactics, profiles & legendary boss guards
│   ├── War/            # Tactical war overhaul, AI, Army Training, Skirmishes & save fence
│   ├── Progression/    # Army Progression ledger, ranks, doctrines, Castle Upgrades, Bestiary & Honors
│   ├── Guild/          # Castle Guild 4-NPC hub, menu navigation & presentation
│   ├── Hall/           # Hall of Echoes rematches, Nightmare tiers & Luca gauntlet
│   ├── Hunts/          # Hunt Guild contracts, sessions & tactical HUD/overlay renderer
│   ├── Probes/         # Native/component diagnostic probes
│   └── Content/        # Audited JSON balance & baseline definitions
├── tests/              # Deterministic managed regression suite (40+ suites)
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
