# Boss expansion in v1.27

## What changes in a Normal campaign

| Encounter | Revised decision |
|---|---|
| Sierra | Focused pressure at low boss health. Against a healthy party, the native area move gains priority between pressure windows. |
| Golden Wolf, both encounters | The original lower-power focused technique gets a stronger recovery preference. Its group spell gains priority against a healthy, heavily armoured party with weaker magic defence. |
| Second Harpy encounter | Its existing limited-use status technique gains priority against a healthy party when enough targets can still be affected. |

These are scoring preferences inside the existing native legality checks.
Recovery and area restraint yield when the available legal moves require it.
Single-target selection uses native candidates and accounts for the active
move's defence type, status and repeated focus. No damage, HP, use limits,
attack slots, animations, callbacks or rewards were changed.

Set `EnableCombatQuality` to `false` in `boss-tactics.json` to disable this layer
and the earlier Pest Rat/first Harpy layer. Existing per-boss opt-outs still apply.

## Coverage decisions

Compared the local audited source tables against the ten existing boss policies
and the separately audited Luca encounter. This is a review of the available
audited catalogue, not a complete census of every campaign boss. Double Head
and final Neclord retain their existing policies because they have main battle
callbacks. Lucia is a different character from Luca Blight. Lucia's later
records and the earlier Neclord encounters retain native behavior, with their
command callbacks outside the scope of this change. Luca stays paused.

The 51 formations, 280 regular-enemy profiles, character, rune and Unite
balance, Guild, and native story-war systems are retained. This milestone
changes the shared targeting inputs for regular encounters. It does not add
formation, progression or war tuning without gameplay evidence.

## Verification

Development DLL `1.27.0-boss-expansion.1`, SHA256
`6553AD40C0568A08153C86F94E1E1220D90CF24E38AE092678C37A1CE5FC3C7D`. This candidate is separate from the Steam installation.

Compiled cleanly and passed the managed suite. An isolated production audit
loaded original tables and exercised all ten admitted boss records, 51 remixes,
Normal and Hard regular-enemy admission and three Guild fixtures. Native selector
comparisons showed different choices for all four newly refined encounters while
retaining native RNG, move limits and damage for the same move. Per-table
comparison counts are in [the build receipt](../publication/development-build-v127.json).

The same DLL initialized at the unloaded launcher with gameplay defaults.
The audit used disposable parties and callback bindings. It did not execute
a boss fight, story callback or campaign transition. Full campaign fairness
and the two experimental Guild contracts still need actual gameplay.
