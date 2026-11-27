# Luca Blight encounter foundation

Development build `1.21.0-luca-foundation.1`, SHA256 `92335E7432D2805536A0409ABB366EB6E155D05EE45F4F8B0670F432863B1E8D`.

## Record and callback audit

The installed remaster's original names resolve name IDs 825, 828 and 829 to Luca Blight. The latter two resolve through aliases to 825. Lucia's `boss_lus1` and `boss_lus2` tables remain separate.

| Battle table | Luca actor | Enemy roster | Opening / command / ending registry indices |
| --- | --- | --- | --- |
| `boss_luc1` | `boss_luc1_02` | Luca and five White Wolf Soldiers | 1370 / 1372 / 1371 |
| `boss_luc2` | `boss_luc2_02` | Luca and three White Wolf Soldiers | 1377 / 1379 / 1378 |
| `boss_luc3` | `boss_luc3_00` | Luca alone | 1384 / none / 1385 |

The first two endings copy the actor's current HP into `GAME_DATA.luka_hp`. The second and third openings copy that field back into the battle-local actor HP. All three templates specify 6,500 HP, so the template HP alone cannot describe the later fights. The read and write sites establish the intended handoff order. Exact player party ownership and the event interpreter's continuation between fights remain unverified.

The first two command callbacks examine Luca's current and maximum HP while the native battle counter is at most four. Their thresholds are floor(two thirds of max HP) and floor(one third of max HP), respectively. At the threshold they set their own `luc_damage_flg` and return 1. Beyond that counter window they test the actor's finished flag. The callbacks are outcome gates, distinct from the attack selector. A supplied counter value in a fixture does not establish a count of completed rounds or Luca's attacks per round.

The first two endings distinguish escape, defeat and victory flags and consult the damage flag on the victory branch. Their event writes use slot 0x60 masks 0x20 and 0x40 in the first encounter, and slot 0x60 mask 0x80 and slot 0x61 mask 0x01 in the second. The third ending calls native sound command 17. All three openings call the original sound, field camera and effect-loading paths. Their continuation includes the white flash and field restoration routines. Battle ending, campaign continuation and cinematic playback were inspected statically. Player party assignments, event branches after each result, and how the last battle leads into the subsequent story remain open.

## Native selector evidence

The three original loaded actors share three weighted attack slots. The fourth slot has zero weight. The table weights are 50 / 20 / 30, and the three available slots each have a use-limit field of zero. Slot 0 has the single-target shape. Slots 1 and 2 have the native 0x400 and 0x800 target shapes. Selector return values for those latter moves do not establish all recipients of an executed action.

Each of two unloaded isolated runs checked loaded IDs, signatures, rosters, positions through the source guard, and original callback registry entries. The six-player scratch fixtures exercised the game's action selector and target selector in eight six-step sequences per record. Paired runs with existing enemy AI off and on returned identical modes, target slots and final RNG states. Per actor, the original selector returned modes 0 / 1 / 2 a total of 21 / 11 / 16 times in these fixtures. The fixtures varied actor HP and supplied the counter. They used synthetic player stats and callback-presence stubs. Original story callbacks, attacks, damage application and round execution were outside the fixture.

The production checks retained all ten previous boss policies, including Pest Rat and first Harpy. The managed suite passed. Original saves, the installed v1.13 DLL and configuration hashes matched before and after. Both copied runtimes exited normally.

## Gameplay scope

This milestone adds an audit and executable regression baseline. Luca has no selectable tactics option. EnableMajorBosses still covers Lucia and final Neclord. A future Luca option needs a verified live dispatch boundary that separates discretionary selection from scripted actions, including the real party context, attack count and timing, and the HP handoff around the outcome callbacks. The third record has no command callback in its table and is the narrowest candidate for that work. Its HP carryover, opening effects and post-battle story still require verification.
