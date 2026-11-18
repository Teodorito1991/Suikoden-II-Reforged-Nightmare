# Hall of Echoes second challenger

This development build extends the accepted v1.42 Hall lifecycle with Harpy.
The original Steam installation and accepted v1.42 fallback are preserved.

## Exact artifacts

| Artifact | Version | SHA256 |
|---|---|---|
| Current candidate | 1.43.0-second-challenger.1 | 4A73164EC7DF9A16169BB858FE14A2BE1652505E0B63F70BBDFA3862D2554763 |
| Accepted fallback | 1.42.0-hall-opt-in.1 | E4F48D8AB16AC0E224FF78AEE0689CE3E954A87E5FC47EAFA74DE508D5FFF40A |

The candidate was built with deterministic Release settings and debug symbols
disabled. Gameplay observations below belong to the candidate hash.

## Original source contracts

| Trial | Source / party | Original HP | Native pack | HD scene | Native opening / detached ending |
|---|---|---|---|---|---|
| Pest Rat | boss_rat / boss_rat_00 | 5,200 | rat | bgh3 | rat_normal_mae_func00 / rat_boss_end_func00 |
| Harpy | boss_hap1 / boss_hap1_00 | 7,400 | hap | bgi6 | hap1_normal_mae_func00 / hap1_boss_end_func00 |

The loaded supported game resolves Harpy's name from original name ID 807.
Both audited formations contain one enemy and no phase, command or main callback.
Each opening creates original battle-local presentation work. Harpy uses its
original mountain scene and opening effects.

HallTrialCatalogue supplies the stable choices and expected sources, HP,
resource mapping and scene. HallFirstContact retains the single trial owner,
native ordinary encounter request and return lifecycle. Validation compares the
original formation, roster, callback tuple and monster attack signature.

The pack is resolved through BattleLoader.GetBossResID and must equal the
catalogued key. The background must match the original source scene.
The original opening remains attached. Story ending, phase, command and main
callbacks are detached on the private formation. Original sources remain intact.
SYS_WORK.battle_no remains -1 throughout both rematches.

## Combat and rewards

Both trials retain original HP, native moves, animations and damage processing.
No new HP multiplier, action or parallel reward system was added.
Hall copies do not widen original campaign boss-policy admissions.

Pest Rat retains its native coin and item handling, including the possible
Poison Orb. Harpy's original monster record has a 4,000 coin field and three
empty item slots. Its copied formation grants no story-specific party item.
The original victory pipeline determines actual payout and respects the money
cap. Tested settings credited 34,000 Potch for Harpy and 17,000 for Pest Rat.
These observed totals depend on native reward rules and configuration.
Harpy offers no item drop. Native flee and defeat credited nothing.

## Guild and configuration

Talk to the existing Suggestion Box attendant in the supported vk07, area 10,
town 6, event variant 3. Hall of Echoes contains two rows. Up and Down move the
cursor, Confirm starts the selected challenge, and Cancel returns to Services.
The existing pagination implementation keeps the selected row visible.
A fresh press after opening the page is required.

Both CFG switches default to false.

    [Hall of Echoes]
    EnableHallOfEchoes = false
    EnableHallHarpy = false

EnableHallOfEchoes permits Pest Rat. Harpy additionally requires EnableHallHarpy.
Disabled Harpy remains visibly unavailable and cannot dispatch a battle.
Normal implementation requires no private observer, staging marker or shortcut.

## Resource and campaign ownership

Each trial acquires its own table, pack and original HD scene. The field scene
slot is retained. Native actor/manager release and BattleChapter destruction
precede Hall-owned release. Borrowed packs remain borrowed. The existing
identity checks and failure behavior for unknown owners are retained.

The trial compares all five protected flag arrays, war data, progression,
party membership and event inventory at its terminal boundary. It verifies the
original Guild position on victory or flee. Stock Game Over remains authoritative
on defeat. Original Retry autosaves are recorded separately from combat.

Private state comparisons include ordinary changes in HP, rune charges, EXP,
kill counters, affinity, inventory and currency. The first battle also applies
existing lazy character profiles. These are distinguished from Hall story
progression. No generic save restore or flag suppression was introduced.

## Verified runtime evidence

- Release compilation passed with zero warnings or errors and no CodeView
  entry or tested private-path string in the DLL.
- Managed regressions passed, including two-row focus, wrap, selection ownership,
  held-confirm protection and Back navigation.
- The exact candidate passed the original-engine production audit (5,048 checks). Both loaded
  source tables, resource mappings and native loot records were verified.
- Harpy rendered in bgi6 with its original actor and working attacks/effects.
- An Auto-Battle-earned Harpy victory credited 34,000 Potch and returned to the
  same Guild position. Flags, war data and progression matched.
- Pest Rat then earned a native victory, 17,000 Potch and a Poison Orb.
- A second Harpy encounter defeated the worn-down party through native combat.
  Stock Game Over was reached, with zero reward and unchanged protected state.
  Scene and owned pack cleanup completed.
- Stock Retry loaded a legitimate headquarters checkpoint and reconstructed the
  Guild. Its original map-step-14 autosave was observed.
- The owner completed Pest Rat victory, Harpy victory, Pest Rat victory in one
  session and reopened the Guild. Every encounter preserved the exact Guild
  position, protected flags, war data and progression, and released its owned
  scene and pack. Rewards were 17,000 plus a Poison Orb, 34,000, then 17,000 plus
  another Poison Orb. No save writes occurred.
- The owner then selected Harpy and used native flee. Guild reopening succeeded.
  The outcome receipt recorded zero Potch, unchanged protected state and full
  owned resource cleanup.

- The owner completed a further Harpy defeat, selected stock Retry, loaded
  Headquarters and returned to the reconstructed Guild. A new Pest Rat fight
  then launched and ended through native flee with zero reward. One earlier
  uncommitted request in that return sequence cancelled safely and released its
  acquisition; the next confirmed request succeeded.
- The later session recorded three native saves, all in original MapChapter
  step 14 during ordinary castle travel and Retry reconstruction. No Hall
  outcome invoked a save. The full decoded stock/Hall Retry comparison remains
  historical v1.42 evidence; v1.43 verifies the same native save boundary.
- Default-off graphics startup parsed both switches as false and installed
  zero of eleven optional Hall hook targets. It exited normally.
- All fourteen original save files matched the existing protected baseline.
  The accepted v1.42 fallback retained its exact original hash.

The second trial passed this targeted gameplay acceptance on 18 November 2026.
Both switches remain opt-in. The owner's installed Steam DLL was not replaced.

## Evidence limits

This is a development build. Whole-campaign balance, every possible action,
other headquarters variants and third-party mod combinations remain unverified.
Battle guests and additional Hall bosses remain unavailable.

Some private automation attempts missed launcher/menu inputs or stalled during
startup. These attempts are excluded from gameplay acceptance. The normal DLL
was frozen before the owner-assisted sequence. Private input/capture experiments
are excluded from the source repository and normal gameplay DLL.
No binary, installation ZIP or GitHub Release is published by this milestone.
