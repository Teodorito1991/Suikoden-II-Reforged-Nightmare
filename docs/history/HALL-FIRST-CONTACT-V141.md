# Hall of Echoes First Contact

Experimental build `1.41.0-first-contact.1`

DLL SHA256 `01B9CF3E9E2FE9A075A49F3003F04F7B06A2F44248472C1BD8EE22B9CEB47B29`

## Rendered gameplay in a copied runtime

| Check | Observation |
| --- | --- |
| Entry | The original talk/confirm controls opened the Castle Guild at the Suggestion Box. Hall of Echoes → Pest Rat dispatched an original native battle from audited castle map `vk07`, area 10, town 6, variant 3. |
| Combat | Pest Rat and Riou rendered on original HD field `bgh3`. Both actors took turns and dealt damage. Shining Light, Battle Oath and the rat’s attack effects played. The automated test supplied ordinary game inputs and used native Auto-Battle. |
| Victory | Trial one ended in original victory processing and returned to position (232, 240) in the Guild corridor. Movement and talking to the attendant worked. The native result offered 17,000 Potch and delivered a Poison Orb. Actual Potch credit was zero at the 999,999 cap. |
| Repeat and defeat | Trial two started from the same Guild visit without a reload. Native damage produced a defeat and the original Game Over screen. The trial awarded no additional loot or Potch. A second victory was unobserved. |
| Retry | The original Retry path loaded the copied headquarters checkpoint and movement worked. The attendant was absent on returning to the Suggestion Box corridor immediately after that reload. |
| Campaign | All five flag arrays, serialized war data and selected progression fields matched at the terminal checkpoints. `battle_no` remained `-1`. The high-level save and autosave counters stayed at zero, but the observer recorded one lower-level write during the session and the disposable `Data0` changed. All 14 original save files matched their pre-test hashes. Prior-area and prior-town history changed in a broader trace whose baseline preceded the Guild interaction. The write's exact caller and the history changes' relationship to the rematch remain open. |

These results came from original rendered gameplay driven by automated controls.
Owner gameplay acceptance of the Hall and a full campaign regression remain open.
The managed regression suite and the exact DLL's 5,046-check native production
audit passed. Those fixtures exercise production hooks and original data in an
unloaded game and do not add rendered gameplay coverage.

## Resource ownership and transition

- `HallFirstContact.cs` validates the Guild field owner and requests native
  battle setup from an audited clone of the original Pest Rat definition.
  It retains the native opening, uses original Pest Rat HP and leaves
  story-specific ending callbacks out of the rematch.
- The trial acquires `bgh3` in a free scene slot and verifies the loader owns
  that slot before selecting it during original chapter startup. It keeps the
  ordinary encounter boss index at `-1`.
- `HallPackLease.cs` distinguishes a pack acquired by the trial from a
  preexisting cache entry. The rat attack consumed OPD record 0 through
  a trial- and battle-owner-checked lookup. Both live trials acquired their
  own pack. Borrowed-pack preservation was checked in the earlier isolated
  acquisition test.
- After native manager release and chapter destruction, the trial verifies
  the terminal destination and absence of the battle work owner. It releases
  its HD scene first, then its owned OPD pack and source acquisition. Both
  rendered trials reached this boundary. The bounded checks cover these
  tracked resources. A system-wide leak analysis was outside this experiment.

## Build and release gate

The regular build excludes the First Contact implementation. The development
build uses the regular Release build with `HallFirstContact=true` and a
separate output directory. The development entry also requires a copied
runtime, explicit launch opt-in, an armed disposable workspace marker and a
compatible local-only test guard. The test guard, save and raw traces stay
outside the published source tree.

There is no supplemental payout or report reward. The rematch uses the
original victory result and inventory handling.

Keep Hall trials unavailable in normal play. Before considering activation,
identify the lower-level save writer, resolve the missing attendant after
stock Retry, check a win below the Potch cap, and complete a second victory
with the same ownership and campaign checks.
