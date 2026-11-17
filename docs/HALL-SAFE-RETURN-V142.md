# Hall of Echoes safe return

Development build `1.42.0-hall-recovery.2`

DLL SHA256 `95956B88BBDB3090E14333213B41A32F1B103D2E1B843F018FFB80601136E3F4`

## Return ownership

- On defeat, after native battle release and chapter destruction, the trial
  retires the matching Reforged combat state. The original Game Over path can
  bypass `BattleEndWait`, leaving state that prevents Castle Guild insertion
  on the next map initialization. The cleanup checks the released trial's
  battle work identity and the old Guild owner.
- Original chapter result 1 covers both victory and a field return without
  victory. The Hall now observes `MACHICON.OnEndBattle` for its exact field
  and interpreter after teardown, then checks the return position and
  releases its resources. This also allows cleanup after leaving the fight
  early. The original victory callback remains authoritative for payout.

## Persistence evidence

The copied-runtime trace captured the write to Suikoden II's `Data0` on the
new `MapChapter` at `MachiMain` step 14, after selecting the stock Retry
checkpoint. The two traced save layers were `SystemSave.Save` and
`SteamService.Save`. The game wrote the native save payload and the file
hash changed before field control returned. The original executable has
an autosave path from `MachiMain` via `AutoSaveCheck`, `UISaveLoad2.Save` and
`GSD2SaveData.SaveFile`. V1.41's inert high-level counters did not establish
the absence of native saves.

The prior-area and prior-town history changed on the first checkpoint load
before a Hall request, and the same change recurred during Retry. The older
broad before/after comparison included this normal map initialization.

The ordinary overworld battle and the Hall battle both used real native
defeat, followed by Retry to the same numbered Headquarters checkpoint.
Each recovered chapter wrote one new autosave via the same native save
path. The trace counted no write from either battle's defeat itself.
Both decoded Retry autosaves differed from the selected checkpoint in
the save timestamp, play and event timers, and the same native return
position update. The two autosaves differed from each other only in the
timestamp and timers. Whole character records and party records matched
the common checkpoint, including roster, formation, stats, equipment,
runes, health, charges, status and character progression. The flag
arrays, shop and war data, currency and inventory also matched.

The earlier unmatched comparison used autosaves from different points
in the campaign and could not attribute party lineup or mutable character
differences to the encounter. The matched control found none.

## Rewards

Pest Rat retains the original monster payout and item table. The monster has
2,000 raw Potch and Poison Orb in its native drop table. The ordinary native
monster reward calculation and configured Potch multiplier determine the
result. The Hall supplies no supplemental payout or story reward and uses
the original drop roll, inventory handling and Potch cap.

With a 100,000-Potch test wallet, the owner won twice without reloading.
Each fight credited 17,000 Potch and added one Poison Orb slot. Each returned
to the original player position, passed the five flag-array, war data and
selected progression comparisons, and released the owned HD scene and
Pest Rat pack. The private observer recorded zero save writes during these
two wins. Full-bag behavior remains unobserved in a rendered Hall battle.

## Exact-build Retry

On `1.42.0-hall-recovery.2`, a native Pest Rat defeat awarded nothing and
released its owned battle resources. The Hall cleared the old combat
state, native Retry loaded Headquarters with a new field owner, and the
owner interacted with the restored attendant to start Pest Rat again.
That rematch ended by fleeing, returned to the same Guild location,
awarded nothing and released its owned resources. The earlier two
consecutive wins establish the victory and reward path on this same DLL.

This artifact is only available in the protected experimental build.
The managed regressions and the exact DLL's 5,046-check native
production audit passed. The startup audit loaded no campaign.

## Normal-build option

Build the current source in Release with the usual project command and
local game references. The normal build is `1.42.0-hall-opt-in.1` with DLL
SHA256 `E4F48D8AB16AC0E224FF78AEE0689CE3E954A87E5FC47EAFA74DE508D5FFF40A`.
It introduces `[Hall of Echoes] EnableHallOfEchoes` with default `false`.
The option is read at startup. The game still requires the audited build,
Normal difficulty and a compatible, idle Castle Guild at the audited
location. The Hall installs its exclusive Harmony hooks only when opted
in. A conflicting Hall-only target disables Hall access at startup.
The live feature has no dependency on private paths, markers or observer
assemblies. The test observer remains separate and disposable.

### Final acceptance of the frozen normal DLL

Source commit `17a571bd1abdd597ba6f83d00df8c0a663d9dfdc` produced the exact
normal DLL identified above. No gameplay code changed for this acceptance.
The disposable game used the original menus and combat controls.

| Check | Result |
|---|---|
| Earned victory | Native Auto-Battle defeated Pest Rat. The test wallet started at 100,000 Potch and received exactly 17,000. Native loot inserted one Poison Orb in this run. Riou returned to the same spot and opened the Guild again. No save write was recorded. |
| Native defeat and Retry | A separate test started with one character at 1 HP. Native damage triggered stock Game Over. It awarded nothing. Retry restored Headquarters and one attendant. The rematch ended through native flee, paid nothing and returned to the Guild, which reopened normally. |
| Campaign and resources | All checked flag arrays, war data and progression fields matched. The rendered encounters used battle index `-1` and released their owned HD scene and Pest Rat pack after native teardown. |
| Retry autosave | One native autosave followed loading the same Headquarters checkpoint as the matched ordinary-battle control. The decoded files differed only in timestamp and play/event timers. |
| Startup dependencies | A separate graphical startup enabled Hall through configuration with no private observer, markers or launch arguments. Gameplay tests kept the isolation observer in the copied runtime to protect saves. |
| Disabled behavior | With Hall off, the Guild showed the trial as unavailable and opened Hunt Contracts normally. The native audit found zero of 11 Hall hook targets carrying a Hall patch. Ordinary encounter fixtures passed. |

The normal-build DLL also passed the managed suite and its 5,046-check
native production audit with the gate off. Pest Rat remains an explicit
opt-in for the supported Windows Steam build. Native item rolls and
inventory rules apply. Full-bag Hall gameplay remains unobserved.

### Sanitized local rebuild

Two clean local Release builds from the same source produced identical
DLLs with SHA256
`8B64C5A88B3A06F262ECF46035ACECAA059E7E17E037672E276DC0E893514A61`.
Use `-t:Rebuild -p:DebugType=None -p:DebugSymbols=false -p:Deterministic=true`
and a separate output directory. The PE has no CodeView record and the
binary scan found no personal Windows path pattern. This reproducibility
check covered the same source, machine and toolchain. The sanitized hash
is distinct from the gameplay-accepted DLL. It separately passed
startup and the 5,046-check native production audit with Hall off and
zero Hall hooks installed. Rendered gameplay acceptance applies to the
frozen normal DLL.

No player package was published. Pest Rat is the only trial.
