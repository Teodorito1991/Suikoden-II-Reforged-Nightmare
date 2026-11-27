# Hall of Echoes — Echoes Earned (v1.45)

## Result and important correction

This milestone corrects story availability rather than changing boss combat.
The v1.44 unlock interpretation was wrong: ordinary map loads with numbers 20
and 23 were mistaken for Sierra and Golden Wolf resource loads. The original
loader selects boss resources only for category 15. Its ordinary event battle
mode must also be distinguished from mode 2; matching a numeric index alone
cannot identify a boss. v1.44's first Harpy witness was therefore insufficient too.
Original source fingerprints passing did not establish those semantic links.

Historical v1.44 reports and artifact hashes are preserved. Their unlock claims
are superseded by this report. Accepted native Hall battle results are unaffected.

Build: `1.45.0-echoes-earned.1`

DLL SHA256: `CADC047632A5948BA7277360D34ED4FC6D3EEE0E08DDEA7B43F01EC663E56843`

## Correct read-only progression witnesses

| Trial | Persistent flag / mask | Actual boss preload | Victory continuation |
|---|---|---|---|
| Pest Rat | 0xB6 / 0x02 | vh04, variant 8, event 8: category 15, index 18 | Same variant event 9: ordinary native battle, then the completion write |
| First Harpy | 0x21 / 0x10 | vi15, variant 0, event 5: category 15, index 14 | Same variant event 4: ordinary native battle, then the completion write |
| Golden Wolf | 0x16 / 0x02 | vc27, variant 0, event 4: category 15, index 23 | Same variant event 2: ordinary native battle, then the completion write |
| Sierra | Unresolved | Actual source preload is vg07, variant 0, event 20, category 15/index 20 | Available surrounding flags do not yet prove the individual earned victory; normal-play gate remains unavailable |
| Second Harpy | Unresolved | No original campaign launch established | A tempting numbered load in vh02 is category 13, not a boss load. Do not declare the source unused or invent a mandatory milestone |

The original `MACHICON._LoadMACHIDAT` dispatch sends category 15 to
`BattleLoader.LoadBattleData`. `ECFieldCdRead` supplies the original preload;
`ECEventBattleGo` / `EventBatlleSet` then consume the preloaded original boss
index in ordinary battle mode. The three verified scripts place the persistent
completion write immediately after that battle. Native defeat uses NotWinEnd /
Game Over rather than executing the normal field continuation. No Hall ending
callback is reattached and no original event script is executed by a fixture.

HallProgressProbe now validates all six original preload/completion records,
their full fingerprints, the boss-resource category and the post-battle boundary.
HallUnlockProbe exercises the actual production availability function with native
GAME_WORK.eventFlgCHK on an owned scratch instance: old unrelated witnesses are
rejected, correct witnesses unlock, configuration opt-outs work, and a replaced
flag array immediately revokes eligibility. Original singleton and settings are
restored before returning. These are native gate fixtures, not story victories.

Archived early and later copied snapshots separately contain absent/set bits for
these three witnesses. They are not asserted to be paired saves from one campaign.
The owner also loaded a later headquarters checkpoint and entered Golden Wolf and
first Harpy with StoryProgression active, without the availability override.
A fresh original-boss before/after playthrough and an actual earlier-save reload
followed by a Guild visit have not been performed. No new save format, sidecar or
persistent unlock cache exists; production checks always read current GAME_WORK.

Sierra and second Harpy remain ??? in normal play. Their trial implementations
are retained. UnrestrictedTesting still requires developer tools and affects only
Hall availability; it does not write campaign flags or bypass source/context checks.
Their unavailable message now explains that these echoes are not available in
normal play yet instead of suggesting further story progress will certainly help.

## Native gameplay evidence on this exact DLL

- Golden Wolf: owner-earned victory under StoryProgression, 8,500 Potch, no item,
  same-position Guild return and owner-confirmed reopening.
- First Harpy: owner-earned victory under StoryProgression, 34,000 Potch, no item,
  native pack consumption and clean Guild return/reopening.
- Sierra: owner-earned victory under the explicit testing override, 850 Potch,
  no item, native pack consumption, clean return and Guild reopening.
- Second Harpy: owner-earned victory under the testing override, 127,500 Potch,
  no item, native HP 12,000 → 6,612 during combat, original hap animation record
  consumed, observed party damage 978 and healing 850. Native teardown and
  same-position Guild return passed. No forced outcome or combat-stat fixture.
- Immediately afterward Pest Rat launched and fled natively, paid zero Potch,
  added no Poison Orb and restored no charges. The owner reopened the Guild.
  This confirms post-Harpy cross-boss re-entry on the same exact DLL.

The protected five flag arrays, war data and progression snapshots are unchanged
for the observed wins. Source, pack and scene ownership receipts show native
setup and teardown. Stats, attacks, battle engine, rewards, Game Over and Retry
code are unchanged from v1.44. The second Harpy retains its original level 60 and
12,000 HP. No invulnerability, forced outcome or monster-HP edit is introduced.

The stronger existing copied checkpoint has party levels 55–75 and native weapon
levels up to 16. Character, equipment, rune, inventory and campaign fields are
unchanged from its archived source. Its prior isolated Normal conversion changed
only the difficulty scalar. Historical stat acquisition was not independently
replayed. The existing private wallet fixture supplies payout headroom before
combat; it changes no combat stats and does not award trial rewards.

## Cancellation and presentation

The v1.44 engine lifecycle remains unchanged. Availability is checked at selection,
on every precommit loading tick, and immediately before the native request.
A revoked selection becomes an abort, drains known pending acquisitions, and uses
the established cleanup path. Native scratch tests prove eligibility revocation;
a real mid-load story-state change was not induced. Do not call that an observed
rendered loading-cancellation test.

Existing pagination, input fence, original frame/font/cursor and trial IDs remain
unchanged. Original foe level remains labeled separately from party advice.
Configuration-disabled and story-locked states remain independent. No new boss,
difficulty tier, progression currency or reward engine was added.

## Preservation and release status

EnableHallOfEchoes remains false by default. All existing individual switches and
StoryProgression default are preserved. No installed Steam DLL/configuration or
original save is replaced. v1.43 and v1.44 exact artifacts remain available as
fallbacks. The RPG overhaul, Hunts, character/rune systems and wars are unchanged.
No historical Game Over or save investigation was reopened.

Managed regressions and exact-build native production auditing pass. A pre-existing
character-30 source mismatch warning occurs in this saved checkpoint, as it did in
v1.44; the existing guard leaves that record unchanged. It is not a Hall Harmony
rejection, and this milestone does not override it or certify the entire campaign.
Evidence limits, copied-runtime outcomes, final source synchronization and graphics checks
are recorded separately. Whole-campaign balance and the two unresolved story
witnesses remain outstanding. Raw saves, logs, native listings and original assets
remain outside Git. Source synchronization is not binary-release authorization.
