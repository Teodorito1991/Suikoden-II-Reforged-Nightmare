# Hall of Echoes — The Nightmare Trials (v1.47)

Build: `1.47.0-nightmare-trials.2`

DLL SHA256: `7283F4B6873C6AFA8BE4277ADF6A7E09CEB42E4607C168D3AB3A381CABE211BF`

## Optional tier, existing battle engine

The eight accepted Original Echo definitions, source checks, story witnesses,
resource leases and return paths are retained. Selecting a known, configured
memory now opens a separate two-row Original Echo / Nightmare Echo confirmation.
Cancel restores the selected catalogue row. Held confirm cannot enter a tier.
Locked identities stay hidden, and disabling Nightmare does not disable Original.
No new boss rows, game mode, save format, maps or campaign difficulty are added.

EnableHallOfEchoes remains false by default. EnableNightmareEchoes is a separate
false-by-default opt-in. EnableNightmareStoneRewards defaults true but does nothing
without an enabled Nightmare trial and an earned victory. Existing individual
trial switches and StoryProgression remain authoritative.

## Battle-local rules

| Trial | Original level | Original HP | Nightmare HP | Damage relative to configured Original |
|---|---:|---:|---:|---:|
| Abomination | 34 | 4,500 | 6,750 (150%) | 110% |
| Stone Golem | 55 | 7,500 | 10,500 (140%) | 115% |
| Golden Wolf | 49 | 3,500 | 4,375 (125%) | 115% |

Prepare validates the original audited source, then uses native MONSTER_DATA.
ShallowCopy for a Hall-owned template and a private monster array. Only that
clone's HP changes. Original source HP, attack records, animations, stats, drops
and callbacks are not edited. Native setup makes its usual battle-local copy.
Existing Hall HP exemption prevents another global/profile HP pass. The owned
Nightmare template is destroyed only by the accepted post-battle cleanup path.

The final owned Nightmare increment applies at the existing native damage
submission boundary, PM_DATA.DamageSetReActionData. The earlier calculation-hook
increment has been removed entirely: some original special attacks bypass that
calculation helper. Regular and special submissions therefore receive the same
relative increment once, with a nesting fence and native damage cap. Admission
requires the live owned Nightmare actor and action's matching attacker slot,
matching work/formation/campaign and a living native player target. Original
Echoes, story battles, environmental damage and player attacks do not qualify.

The Original calculation and defense/guard layers are preserved. For example a
native submission of 180 becomes 207 at 115%; a custom special submission of
200 becomes 220 at 110%. These are bounded helper examples, not measured battle
budgets. No extra turns, guaranteed criticals, fabricated moves or level scaling
exist. The earlier candidate's actual fights exposed the helper-bypass defect;
its victories are not transferred as final-hash damage verification.

AI is admitted only for an owned, initialized Nightmare work/formation/actor with
matching campaign ownership. It reuses the existing SelectAttackMode and native
candidate-target hooks. Existing range, silence, target eligibility and remaining
use checks run before scoring. The original selected action wins equal scores;
unknown support actions retain the original choice. The new tier hook reuses an already audited reaction target; no unfamiliar native entry point is
introduced and no scripted-command or story callback is attached.

- Abomination: alternating magical/physical preference, then a single-target
  physical recovery preference on the third native round. Its original magical
  attack still has only four uses. A stronger late-game party can defeat it quickly.
- Golem: heavier magic then physical pressure, followed by preference for its
  lower-power original magic. Its source has no single-target physical move;
  the recovery rule therefore compares actual legal power and breadth instead
  of assuming a nonexistent basic attack. Range/status restrictions can change
  the available move, so this is not an invulnerability window.
- Golden Wolf: physical pressure punctuated by its original magical option.
  Targeting spreads attention among native candidates with less existing focus
  and healthier HP ratios; it does not identify or repeatedly hunt healers.

A broad attack receives a soft penalty when multiple targets are badly wounded.
No new native move-use budget is invented. Counter-based selection is not a claim
that a complete balanced campaign has been simulated or certified.

## Rare native growth reward

Original Echo rewards are unchanged. Nightmare keeps the original native Potch
and loot calculation; no extra enormous Potch bonus is added. With its reward
option enabled, an earned Nightmare victory has one 25% roll to offer Stone of
Power, only if original loot left the native item slot empty. Thus 25% is the
conditional offer chance, not a guarantee of an item reaching inventory.

Item 21, kind 0, use count 1 is matched to the existing audited original item.
The original field-use path obtains a random 1–3 Strength increase from its
native effect bound, then calls the original parameter operation. Native scratch
execution verifies permanent Strength addition and saturation at 255 after
repeated additions. This does not replace item-use UI or bypass native stat caps.

HallStoneReward is a per-trial pending-offer ledger. Repeated preparation neither
rerolls nor directly grants inventory. A recalculation can restore the same
pending offer before payout begins. BattleItemGet seals it; it cannot create
another offer afterward. Let Go, defeat, interrupted loading and Original Echo
have no eligible roll. Reporting or reopening the Guild grants nothing.
Original rolled loot always has priority. Native item UI and inventory acceptance
remain responsible for actual receipt and full-inventory handling. No sidecar,
account currency, daily cap or permanent reward counter is added.

## Verification boundaries

Exact-build managed regressions and native production audit passed. Native clone
checks cover all three profiles twice, original template preservation and a
second native copy retaining the once-adjusted HP. The reward fixture uses
synthetic victory eligibility, a controlled native RNG seed, real item lookup,
original inventory insertion and native full-inventory rejection. It separately
checks no offer without eligibility, native loot priority, no duplicate RNG roll,
pending recalculation, sealed handoff and original Strength-cap behavior. It does
not claim a player-earned victory or a rendered item receipt.

The separate-desktop renderer produced 22 screenshots on this exact DLL. The new
tier cards use original frame/font/cursor, preserve layout bounds and show original
foe level, HP/damage differences and truthful conditional rewards. Previews are
labeled and are not campaign screenshots.

### Actual rendered acceptance

The exact final revision .2 has five owner-earned Nightmare victories: one Stone
Golem and four Abomination wins, with native Potch credits of 50,000 and 10,000
per observed victory respectively. All seven observed returns, including Nightmare
Abomination Let Go and Original Abomination Let Go, preserved the five protected
flag arrays, war data and progression snapshot and completed tracked owned cleanup.
The owner reopened the Guild and repeated Abomination after crossing tiers.
Original Abomination returned to its unchanged 4,500 HP; Nightmare had 6,750 HP,
and Golem had 10,500 HP. Native battle_no remained -1.

Golem's native submissions visibly received the 15% increment (for example 78→90
and 130→150). Abomination reached the same boundary at 10%; with this strong party
its 1/2-point hits rounded back to 1/2. This proves boundary execution, not a larger
visible HP loss against this party. Its legal action/target choices changed, and
Golem used both heavier physical attacks and lighter original magic. It is not
proof that all intended magical choices or every limited-use branch executed.

The fifth revised-build victory rolled 4/100 and prepared the stone. The owner
accepted it; the protected return receipt recorded item 21 inventory count +1.
Earlier rolls missed, and no nonvictory made an item roll or paid Potch. No chance
was forced during these owner fights. The native full-inventory rejection remains
a controlled scratch test, not a rendered full-bag acceptance session.

Golden Wolf was won on candidate .1 (64C3B5B4...) together with the other two
Nightmare trials. That candidate exposed the special-attack helper bypass and
is preserved only as historical evidence. Wolf was not replayed on revision .2;
its profile/source/AI are unchanged, but its new common reaction path has only
component evidence plus the other two exact-hash rendered battles. Do not transfer
its earlier victory as an exact-final-hash win.

The prepared existing checkpoint has levels 55–75 and native weapons up to 16.
No test-time party combat stats or forced outcome were injected. The private wallet
fixture supplies currency headroom. It does not award trial rewards. No reasonable
midgame-equivalent party or new-tier defeat was played. Global Hall and Nightmare
remain opt-in; full campaign pacing, economy and difficulty remain unverified.

Preparatory runs exposed missing scratch-only parameter buffers/character-info
references, then test-configuration opt-outs required by the retained audit.
Those failed runs are not final-hash passes. One initial copied launch was
redirected by Steam to the installed executable; no campaign was loaded, its
startup windows closed normally, and subsequent runs used the correct copied
runtime with explicit Steam application environment. No installed DLL was replaced.
Original saves and the exact v1.46 fallback are preserved. Raw logs, assets,
screenshots, saves and native listings remain outside the public source repository.
No beta, installation ZIP or GitHub Release is authorized by this milestone.
