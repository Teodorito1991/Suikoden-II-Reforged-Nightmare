# Hall of Echoes — Echoes of the Past (v1.44)

## Scope and exact artifacts

The accepted v1.43 DLL remains preserved: `1.43.0-second-challenger.1`,
SHA256 `4A73164EC7DF9A16169BB858FE14A2BE1652505E0B63F70BBDFA3862D2554763`.
The original Steam installation was not replaced. No release or beta is published.

Owner gameplay in this milestone used candidate `1.44.0-echoes-of-the-past.1`,
SHA256 `D5508962071822CAA401A32B40293D3451475E426BB028B9A9B2ED3844EFDE2F`.
It demonstrated Golden Wolf victory → Sierra victory → second Harpy defeat,
all in one copied-runtime session using an explicit availability-testing override.

The revised candidate is `1.44.0-echoes-of-the-past.2`, SHA256
`07D7E151920F81AE81163F3618897D1A84C18C3C99F0635EDE65228CFDFF8941`.
It adds the two newly established source-backed unlock checks, original foe levels
in presentation, the startup-only event-source fingerprint audit, and clean cancellation if eligibility changes during loading.
On this final hash, the owner subsequently won Pest Rat, received 17,000 Potch
and one Poison Orb, returned to the correct Guild position and reopened the menu.
Protected flags, war state and progression remained unchanged; owned cleanup
completed. Do not transfer the earlier candidate's new-boss acceptance to this hash.

## Catalogue

| Trial | Exact native source | Original foe level / HP | Extra boss pack / scene | Reward and evidence |
|---|---|---|---|---|
| Pest Rat | boss_rat / boss_rat_00 | 40 / 5,200 | rat / bgh3 | Native Potch; possible Poison Orb. Historical full v1.43 acceptance. |
| Harpy | boss_hap1 / boss_hap1_00 | 45 / 7,400 | hap / bgi6 | Native Potch; no native item. Historical full v1.43 acceptance. |
| Golden Wolf | boss_gwo1 / boss_gwo1_00 | 49 / 3,500 | No extra boss pack / bgc8 | Owner victory: 8,500 Potch; no item. |
| Sierra | boss_sie1 / boss_sie1_00 | 47 / 2,800 | sie / bgg8 | Owner victory: 850 Potch; no item. |
| Harpy · Second Encounter | boss_hap2 / boss_hap2_00 | 60 / 12,000 | hap / bgi6 | Owner-earned defeat and stock Game Over; zero payout. Victory remains unverified. |

Levels are original monster levels, not proven party-level recommendations.
No HP, move, damage or reward multiplier was added. The new bosses retain their
original attacks and opening presentation. Their private formations detach story
ending, phase, command and main callbacks. Native battle index remains -1.
The second Harpy label distinguishes its source from the first; it is not a new
invented species name. The loaded second record's own name text is empty.

Golden Wolf's original GetBossResID mapping is null. Its trial deliberately does
not invent a pack key or acquire a rat/hap pack. Native actor resources remain
engine-owned; the Hall still owns its source reference, formation and HD scene.
An explicit SourcePrepared state separates preparation from optional pack ownership,
including cancellation. Sierra and the second Harpy use the established owned /
borrowed pack lease and the same post-consumer cleanup boundary as earlier trials.

## Story progression

Normal policy is StoryProgression. The Hall only reads persistent original flags.
No Hall victory writes an unlock flag or calls a campaign boss ending.

| Trial | Read-only witness | Original source evidence | Limitation |
|---|---|---|---|
| Harpy | event flag 0x15, mask 0x02 | vc06 variant 1 event 32 launches original boss index 14 and subsequently sets the flag. | Original-story before/after save timing not owner-tested here. |
| Sierra | event flag 0xDD, mask 0x02 | vj20 variant 4 loads original boss index 20; event 3 sets the flag after the native battle. | Same timing limitation. |
| Golden Wolf | event flag 0xDF, mask 0x40 | vj23 variant 4 loads original boss index 23; event 6 sets the flag after the native battle. | Same timing limitation. |
| Pest Rat | None proven | Investigation did not establish a dedicated or sufficient later witness. | Story-gated entry remains locked. |
| Second Harpy | None proven | Shared result and debug-preparation flags are insufficient. | Story-gated entry remains locked. |

The installed native EVENTCON delegate table and ECEvFlgSet parameter handling
were inspected without executing event bytecode. The final native audit reloads
and fingerprints all five relevant original event records. This is source/native
resource evidence, not a played original story sequence. The shared C2/80 result
flag and boss-debug flag bindings were explicitly rejected as independent unlocks.

Unlocks are read from the current GAME_WORK on every check. Loading an earlier
campaign does not preserve an in-memory unlock cache. Save persistence relies on
the game's existing event flags, not a new file or save field. Managed cases cover
early/late/reset states and rejection of another boss's flag. A complete original
story-save before/after/reload acceptance remains outstanding.

## Configuration and Guild presentation

All changes apply to the existing Suggestion Box Guild interaction and supported
vk07 / area 10 / town 6 / variant 3. No NPC relocation or castle variant was added.
Five entries paginate four per page. Up/Down retain the selected row; Cancel
returns to services. Locked entries show ??? and a nonspoiling explanation.
Unlocked entries show the original foe level, preparation advice and native loot.
Story availability is independent of per-trial configuration.

```
[Hall of Echoes]
EnableHallOfEchoes = false
EnableHallPestRat = true
EnableHallHarpy = false
EnableHallGoldenWolf = false
EnableHallSierra = false
EnableHallHarpySecond = false
HallUnlockPolicy = StoryProgression
```

Existing explicit settings are not overwritten. Pest Rat's additional per-trial
switch defaults true to preserve the old global opt-in; the new progression gate
still applies. Until its completion witness is established, a legacy opt-in alone
will no longer make Pest Rat available under StoryProgression.

UnrestrictedTesting additionally requires Developer / EnableDeveloperTools=true.
Both must be explicitly chosen. It bypasses only Hall story availability, does not
bypass per-trial/module/source/context guards, and never writes campaign state.
The request and final native handoff both recheck availability. This override is
for controlled development, not a proven campaign unlock solution.
Restart after changing Hall configuration.

## Verification boundaries

- Release build and retained managed tests pass.
- Revised exact-hash production audit passes with all five original boss sources
  and all five original event-source fingerprints loaded; no story commands run.
- Final exact-hash separate-desktop preview produces 15 screens with original
  frame/cursor/font, verified startup guard and balanced asset load/release. Locked
  memories, unlocked Harpy and page two were visually reviewed. These are launcher
  presentation previews, not played quests. An initial harness run used the wrong
  historical guard and failed its evidence check; the corrected run passed.
- Earlier candidate owner play demonstrates Golden Wolf and Sierra victory,
  native rewards, same-position Guild return, consumer teardown and scene release.
- Final-hash Pest Rat regression passed through an earned victory, native rewards,
  castle return and owner-confirmed Guild reopening under the testing override.
- Second Harpy rendered native combat and an earned defeat reached stock Game Over.
  Its cleanup completed; no Potch or recovery was awarded.
- All three tested outcomes report unchanged five flag arrays, war data and
  protected progression. No broad flag restoration was used.
- Cross-boss switching succeeded in that session. Second Harpy victory/rewards
  are not established. New bosses' flee and Retry loops were not individually
  repeated; earlier v1.43 evidence remains historical.
- The full campaign, original-story unlock timing and other castle variants are
  not newly accepted. No balance claim follows from the level-60 defeat.

## Code references

- HallTrialCatalogue: stable IDs, exact original sources, optional pack and scene.
- HallUnlockRules / HallAvailability: read-only witnesses, developer override,
  independent configuration and pre-handoff eligibility.
- HallPresentation / HuntGuildOverlay: hidden identities and paginated details.
- HallFirstContact: existing native request/return and optional-pack preparation.
- HallProgressProbe / ProductionAssetAudit: startup-only original source validation.

Raw traces, saves, original assets, extracted text and proprietary listings stay
outside Git. Sanitized verification is recorded in publication/development-build-v144.json.
