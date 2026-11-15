# RPG Evolution v1.34

Normal is the intended campaign baseline. This pass changes selected
recruits, coefficients and regional rosters while keeping the original
recruitment conditions, rune mechanics and encounter tables.

## Building a party

| Recruits | Changes relative to v1.33 |
|---|---|
| Hix and Tengaar | Hix trades some magic growth for protection. Tengaar trades HP growth for speed and magic defence. Warriors' Village Attack gains single-target power. |
| Bolgan and Tuta | Bolgan grows into a stronger slow attacker. Tuta gains speed and opens his existing head slot at level 18 instead of 25. |
| Gengen and Gabocha | Gengen gains protection and HP growth. Gabocha gains speed and technique. |
| Shiro and Kinnison | Shiro gains strength and speed at a cost to technique. Kinnison trades some speed for technique. Loyal Dog Attack gains power. |
| Nina | Trades some strength growth for speed to cast earlier. |
| Gijimu, Koyu and Lo Wen | Gijimu gains strength, Koyu gains speed and technique, and Lo Wen gains protection. Bandit Attack gains power. |
| Vincent and Simone | Vincent gains speed and magic defence. Simone trades magic defence for strength. Narcissists Attack gains group damage. |
| Gantetsu | Gains HP growth and loses a tier of speed. |

Equipment restrictions, fixed runes, absent slots, and affinities retain their
existing values. Growth changes apply through the original level-up system.
Native verification observed the new records in the original lookup. It
did not simulate recruitment through level 99 or recalculate saved stats.

## Runes and Unites

| Spell effect | Prior → v1.34 |
|---|---|
| Fire Wall and Dancing Flames | 200 → 180 and 390 → 370 |
| Rain of Kindness and Shredding Wind | 420 → 390 and 540 → 500 |
| Angry Blow and Thunder Runner | 145 → 165 and 220 → 195 |
| Open Gate | 100 → 125 |

Lightning now offers a closer choice between concentrated and line damage.
Fire and Wind retain their area roles. Water's repeatable group healing
and familiar early area damage are slightly less efficient per charge.
Victory recovery still restores one missing charge per eligible survivor
by default. Rune-family membership, target shapes, prices and charge limits
retain their existing values.

Fire Dragon goes from 200 to 215. The Blue Drop techniques go from 150 to
175 for the focused variant and 100 to 115 for each group variant.
These are physical coefficients. Native resistance and integer rounding apply.

Six original Unites gain power through their existing coefficient field.

| Unite | Prior → v1.34 |
|---|---|
| Loyal Dog Attack | 150 → 175 |
| Trick Attack | 100 → 115 |
| Narcissists Attack | 50 → 65 |
| Warriors' Village Attack | 200 → 225 |
| Tackle Attack | 75 → 92 |
| Bandit Attack | 150 → 175 |

Native member lists, target scopes, animation functions, and status and
recovery fields are preserved. The original reaction damage helper consumed
the new values in fixtures. Full action dispatch and animation remain unplayed.

## Encounters and pacing

Seven new remixes add Woodpecker and mixed light enemies to early groups,
a Nightmare among Creepers, a Neclordia paired with a Zombie Unicorn,
a spell threat among zombies, mixed Tiger and martial enemies, and skeletons
among the later zombie ranks. This brings implemented remixes to 58.

Each remix uses enemies from its original table, preserves occupied slots and
party size, and lowers its base HP total. Imported maximum enemy level is
bounded to two levels above the original group. Battles keep their original
locations, selection weights, scripts and party-level reward fields. Native
enemy drops, experience and Potch can differ with the roster. Ordinary enemy AI
already selects their native legal moves and provides the differences in threats.
Assembly, duration and damage pressure can still vary by group and party build.

## Configuration and verification

The five switches in rpg-evolution.json control character, magic, physical
rune, Unite and formation changes independently. Character and magic revisions
apply only to legacy defaults. Custom values and omitted rows take priority.
Physical runes and unites require exact original signatures. Existing formation
and Hunt admission retains priority.

DLL version 1.34.0-rpg-evolution.1
SHA-256 BD221DF41A51FA2A466C5EE49F4BCC066005349F409CC5796DC6B1390043382F

Release build, managed tests, native production and maintenance checks, and
copied-game graphical startup passed on the candidate. The native audit
loaded the original game-data and Unite assets and verified the new coefficients
through native lookup. Original BattleDataSetup and the production copy hook
exercised all 58 formations, including the seven additions. Native physical
rune and Unite damage helpers exercised the new coefficients. Retained
checks included all sixteen boss policies and the three Hunt fixtures.

The DLL is staged in the copied test game. Its normal quit reproduced the
known Unity shutdown null-reference exception. This build has no rendered
campaign, recruitment, Unite playback or full progression acceptance.
