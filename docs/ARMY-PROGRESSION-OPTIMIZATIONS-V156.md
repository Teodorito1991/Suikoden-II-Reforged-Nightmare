# v1.56 (Development) — Permanent Army Progression, Drill Directives, Cross-Theater Synergy & Hot-Path Optimizations

> **Verification boundary:** Compiled cleanly in `Release` against the supported
> Steam/BepInEx IL2CPP interop (`0 warnings, 0 errors`) and passed the full
> managed regression suite (including 39 new Army Progression, ledger, directive,
> synergy, and Guild UI navigation checks) plus profile and publication audits.
> **Live in-game war/training/Hall/save-reload acceptance is pending playtesting.**
> The owner's installed `v1.55` Steam DLL (`C5BE2F48…7055E`), personal configs,
> and 14 original saves are untouched.

---

## 1. Permanent Army Progression (Save-Bound Content-Addressed Ledger)

In [v1.52 Army Legacy](ARMY-LEGACY-V152.md), persistent commander progression was
deferred because `WAR_DATA` contains no commander XP field and naive slot-numbered
sidecars break when players branch or reload saves.

This release introduces a **content-addressed save-bound sidecar ledger**
(`BepInEx/NightmareRebalance/army-ledger.json`, default **OFF** via
`EnableArmyProgression = false`):

- **Zero native save mutations:** `gsd2/DataXX` files stay 100% vanilla and
  remain readable even if the mod is uninstalled.
- **True branch identity:** On every successful campaign `SystemSave.Save`, the
  mod hashes the exact native campaign records (`game_data`, `war_data`,
  `chara_data`, `party_data`, and all five flag arrays) using SHA256 and binds
  an immutable snapshot of the live `ArmyLedgerState` to that fingerprint
  (atomic temp-file replace, LRU-pruned to 256 snapshots).
- **Exact load restoration:** On `GSD2SaveData.LoadDataCore`, the loaded
  campaign's fingerprint restores the exact matching snapshot. Reloading an
  earlier save automatically rolls commander XP back; unknown or vanilla saves
  start cleanly at Recruit / 0 XP.

### Ranks & XP Curve

| Rank | XP Required | Permanent War Bonus (on disposable `WAR_CHARA_TYPE` clones) |
|---|---|---|
| **Recruit** | 0 | Base profile |
| **Lieutenant** | 100 | `+1 ATK` |
| **Captain** | 250 | `+1 DEF` + **Tier 1 Doctrine ability** |
| **General** | 500 | `+1 ATK` + `+1 Assist` + **Tier 2 Doctrine ability** |
| **Marshal** | 900 | `+1 ATK`, `+1 DEF` |

All stats and abilities remain strictly clamped inside `OverhaulRules` bounds
(`Attack 0..20`, `Defense 0..20`, `AssistAttack 0..5`, `AssistDefense 0..5`,
at most 3 distinct safe native `SP_*` abilities, active uses `0..3`) and
validated against native delegates by `WarRuntime.Usable`.

### Commander Doctrines

Assigned per commander in the Castle Guild (**Army Training → Commander Roster**):

- **Vanguard:** Captain unlocks `SP_MOUNT` (or `SP_CRITICAL` if already mounted);
  General grants `+1 Assist ATK` and `SP_CRITICAL` (or `+1 ATK`).
- **Bulwark:** Captain unlocks `SP_HP_PLUS` (or `+1 DEF`); General grants
  `+1 Assist DEF` and `SP_BODY_GUARD` (or `+1 DEF`).
- **Tactician:** Captain grants `+1` active use on an existing active ability
  (or unlocks `SP_SCOUT`); General grants `+1 Assist DEF` and `SP_SEE_THROUGH`.

### Earning Commander XP

- **Army Training Victory:** `50 XP` (Standard) / `90 XP` (Veteran) + `+20 XP`
  if the unit did not rout. Subcommanders earn `75%` of leader XP (rounded).
  Multiplied by any fulfilled **Drill Directive**.
- **Story War Victory:** `60 XP` + `+20 XP` survival bonus (`75%` for
  subcommanders) awarded on audited native story-war victories when
  `EnableArmyProgression = true`.
- **Hall of Echoes Nightmare Clear** *(with `EnableCrossTheaterSynergy = true`)*:
  `+35 XP` to surviving party members who have a war commander profile.

---

## 2. Drill Directives (Optional Training Modifiers)

Opt-in via `EnableDrillDirectives = false` under `[Army Progression]` and cycled
in **Castle Guild → Army Training → Drill Directive**:

| Directive | Tactical Effect (disposable clones only) | XP Multiplier | Potch Multiplier |
|---|---|---|---|
| **Standard Rules** | No extra restriction | `1.00×` | `1.00×` |
| **Iron Wall** | Enemy war units gain `+2 DEF` (capped at 20) | `1.35×` | `1.25×` |
| **No Field Medicine** | Allied `SP_MEDICAL1` / `SP_MEDICAL2` uses set to `0` | `1.40×` | `1.25×` |
| **Forced March** | Must rout the enemy commander within **6 turns** | `1.50×` | `1.35×` |

---

## 3. Cross-Theater Synergy (RPG ↔ War ↔ Hall)

Opt-in via `EnableCrossTheaterSynergy = false`:

- **War → RPG:** Completing a qualifying **Veteran** Army Training drill arms
  `+1` **Morale Bonus Charge** (up to 3 stored). Your next ordinary RPG victory
  consumes 1 charge to grant `+1` extra rune charge recovery (still respecting
  `RuneRecoveryMaxLevel` and all survival/victory gates).
- **Hall → War:** Completing a **Hall of Echoes (Nightmare)** trial awards
  `+35 War XP` to participating party commanders.

---

## 4. Castle Guild Additions

Accessible inside **Army Training** without shifting any existing top-level
`Services` row indices:

- **Commander Roster:** Paginated list of tracked commanders showing Rank, XP,
  next rank threshold, Drills/Story/Nightmare clears, and Doctrine cycling.
- **Drill Directive:** Cycles active tactical directive for the next drill.
- **Tactician's Codex:** Read-only campaign telemetry summarizing Army
  Progression totals, Cross-Theater Morale charges, Hunt completions, and active
  Star Bonds in your current 6-person party.
- **NPC & Menu UX Polish:**
  - Context-aware **Guild Attendant NPC Banner** (`GuildPresentation.NpcBanner`)
    showing dynamic greetings and live badges (`[!] Report Ready`,
    `[•] Active Hunt`, `[★] Morale +N`, or veteran roster count) when you
    approach the attendant.
  - **Left / Right fast page-flipping** (`GuildMenuNavigation.MovePage`) across
    paginated Hunt, Hall of Echoes, Army Training, and Commander Roster lists,
    with dynamic `← / → Page` footer hints.
  - **Hold-to-repeat Up/Down scrolling** (`MenuRepeatTimer`) for smooth browsing
    of long lists.
  - **Visual polish:** Animated native cursor bob (disabled during render smoke),
    top-right nested breadcrumbs, gold-framed Commander XP progress bar in
    `Commander Roster`, and semantic row note colors (`success` emerald for
    completed/active/high-rank items, `alert` amber for locked/unavailable/OFF).
  - **Multi-NPC Castle Hub** *(opt-in via `[Castle Guild] EnableExpandedCastleNpcs = false`)*:
    Stations a **Drill Sergeant** (opens Army Training directly) and an **Echo
    Archivist** (opens Hall of Echoes directly) beside the Guild Attendant in
    `vk07` using audited sprites and collision-free tile guards.
  - **Pre-Battle Readiness Strip:** Displays live party average level, active
    Star Bonds, and armed Morale charges on Hunt Contract and Hall Echo
    confirmation cards.
- **Expanded Lore Star Bonds (`StarBondRules`, `StarBondRuntime`):** Added 3
  classic pairings alongside the 4 existing Unite-backed bonds:
  - **Genkaku's Legacy** (Riou & Nanami): Guarding at ≤50% HP reduces direct
    physical hit damage by up to 20.
  - **Mercenary Veterans** (Flik & Viktor): Attacking a wounded enemy adds up to
    16 damage.
  - **Arcane Resonance** (Luc & Mazus): Offensive spells deal up to 18 extra
    damage against targets with higher physical defense than magic defense.

---

## 5. Hot-Path Performance Optimizations

- **Streaming startup binary verification (`Plugin.cs`):** Replaced
  `File.ReadAllBytes` with streaming `File.OpenRead` + `SHA256.Create()` to
  avoid loading the full `GameAssembly.dll` into managed RAM at startup.
- **Cached Harmony compatibility targets (`Plugin.cs`, `CastleGuildRuntime.cs`):**
  Eliminated repeated attribute reflection and `AccessTools.Method` lookups on
  every battle/war init and every frame inside `CastleGuildRuntime.Idle`.
- **Combat AI hot paths (`CombatReforgedRuntime.cs`):** Hoisted invariant ally
  target lists out of the 4-action scoring loop, removed LINQ ordering
  allocations, and added early exit to the 128× native target-discovery loop once
  all living legal players are found (or after 24 stale draws).
- **War Tactics AI allocations & lookups (`NativeTacticsBoard.cs`, `TotalWarRuntime.cs`, `War2Runtime.cs`, `TotalWarAi.cs`, `WarTacticsRuntime.cs`):**
  Made direction vectors `static readonly`, reused scratch `Il2CppStructArray<byte>`
  buffers across hostile reachability scans, cached enemy commander `IntPtr` sets
  per war (`O(1)` lookup in `Specialize`), and replaced `O(N²)` `FindIndex`
  candidate scans with a `Dictionary<long, int>` index map.

---

## 6. Configuration

All new gameplay features default to `false` to preserve conservative and Full
Content profile invariants:

```ini
[Army Progression]
EnableArmyProgression = false
EnableDrillDirectives = false
EnableCrossTheaterSynergy = false
```
