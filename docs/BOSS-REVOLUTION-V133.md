# Boss Revolution v1.33

The six new Normal-mode policies score the attacks already available to each
boss. The game retains its attack range, use limits, targeting, animations,
damage formulas, and story callbacks.

| Encounter | New behavior | Changed native selections |
|---|---|---|
| Celestial Sword | Leans into its physical column attack when the party is vulnerable. Uses its group techniques in pressure windows. | 18 / 36 |
| Worm | Alternates its physical column attack with a limited group spell when wounded and facing a healthy party. | 6 / 36 |
| Abomination | Uses focused pressure between group attacks. Favors its limited group spell against healthy parties with low magic defence relative to armour. | 13 / 36 |
| Stone Golem | Favors its lighter group technique during recovery. Reserves its stronger spell for pressure windows and weighs the party's defence gap. | 26 / 36 |
| Bone Dragon | Spends its two-use area spell preferentially in wounded pressure windows. Favors the less powerful area attack during recovery. | 26 / 36 |
| Seed, alongside Culgan | Favors focused attacks and saves its limited area strike for pressure windows. Culgan retains his native decisions. | 16 / 36 |

Each policy matches the original table, party, roster, position, callbacks,
source signatures and observed Normal battle-local copies. The six records
have empty command tables and no command or phase hooks. Main and ending
callbacks retain their original bindings. Original target selection stays
in place for all six bosses. Native comparison fixtures checked legal
selection, exhausted uses, callback bindings, rejection paths and RNG state.

Ordinary enemies now assign partial value to a group status move when only
part of the party can still receive the status. This uses the existing active
status mask. Hunt battles and boss scoring keep their prior status calculation.
Formation and stat multipliers stay at their established settings.

Luca's three battles, Mist Shade's transformation, the transforming Worm
encounter, the scripted late Poison Moth, Gorudo, later Lucia records, early
Neclord and the final multi-part boss stay outside these admissions. Their
phase, command or encounter dependencies require separate gameplay evidence.
The ten earlier policies keep their existing controls.

## Configuration

Set Enabled to false in boss-revolution.json to disable all six policies,
or set Enabled to false on an individual entry. The file is separate from
boss-tactics.json and the global boss and enemy AI controls still apply.
The copied game generated the new defaults and retained its earlier opt-ins.

## Verification

DLL version: 1.33.0-boss-revolution.1
SHA-256: 069BDEBAA62F932C598F78782D95FBF181FD403CD8740B93679A8B540BD86E02

The Release build and managed suite passed. The exact DLL passed the isolated
production and maintenance probes, including all sixteen boss policies, 51
formation remixes, and the three hunt fixtures. Counts above are original
engine selector comparisons with disposable party and callback fixtures.
Rendered boss play, original story callbacks and campaign balance remain
untested on this DLL.

The same DLL reached the graphical launcher in the copied game with the
ordinary test settings restored. Unity logged a shutdown null-reference
exception after the normal quit request, consistent with the older copied
graphics shutdown limitation. The verified DLL is staged in the copied game.
