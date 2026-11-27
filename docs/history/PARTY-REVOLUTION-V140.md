# 108 Stars — Party Revolution v1.40

Party Revolution revises fourteen of the eighty battle-character profiles and
adds four Star Bonds. Growth changes use the original curve table. Recruitment,
fixed runes, equipment permissions, weapon rules and the number of rune slots
still follow the original game.

## Building a party

Every revised profile trades one ordinary growth tier for another. The table
describes the change relative to v1.39. A higher growth tier affects future
native growth, with actual results depending on level and the original curve.
Existing saved stats are not recalculated. Slot numbers below refer to
positions in the existing native slot record.

| Recruit | New build choice |
|---|---|
| Hix | More physical defense, less technique. A frontline partner for Tengaar. |
| Tengaar | More magic, less strength. Her existing third rune slot unlocks at 20 instead of 22. |
| Nanami | More speed, less magic. An earlier physical or item action at the cost of spell specialization. |
| Eilie | More technique, less magic defense. A precise striker who needs more care against magic. |
| Bolgan | More HP, less technique. Sustains frontline pressure with a less reliable offense. |
| Tuta | More magic, less strength. His existing first slot opens at 16 instead of 18 for support builds. |
| Shiro | More technique, less magic defense. Retains his fast physical role and helps enable Woodland Pursuit. |
| Kinnison | More technique, less strength. Ranged follow-up damage rewards setting up Poison. |
| Miklotov | More physical defense, less magic. His existing third slot opens at 39 instead of 42. |
| Camus | More magic, less physical defense. His existing third slot opens at 34 instead of 37. |
| Nina | More magic defense, less physical defense. A caster better suited to enemy magic pressure. |
| Gijimu | More strength, less speed. Heavy offense within the Bandit trio. |
| Koyu | More technique, less physical defense. A precise skirmisher with a fragile frontline. |
| Lo Wen | More physical defense, less speed. A frontline alternative to the other Bandits. |

The profile layer preserves explicit existing custom growth, affinity and slot
rows as a whole. It also respects the previous character-module opt-outs.
Affinity codes and native rune restrictions remain part of the existing
signature checks.

## Star Bonds

On Normal, Star Bonds require distinct native combatants in the current battle
under the same BattleManager. The original Unite member order and function
must match the catalogue. Every partner must be alive and able to act. Battles
with custom original main, command or ending handlers are excluded.

| Bond | Original partners | What to do | Extra effect |
|---|---|---|---|
| Warriors' Ward | Hix and Tengaar, Warriors' Village Attack | Have Hix use Defend when he has half HP or less. Keep Tengaar able to act. | One eighth less direct physical hit damage, up to 20 less per qualifying hit. |
| Woodland Pursuit | Kinnison and Shiro, Loyal Dog Attack | Use Kinnison's normal Attack on an enemy already poisoned. | One eighth extra damage, up to 15. The original poison application is separate. |
| Knight's Fire | Camus and Miklotov, Knight Attack | Use Camus's Fire Wall against an enemy whose physical defense exceeds its magic defense. | One eighth extra damage to each qualifying target, up to 18 per target. |
| Bandit Finish | Gijimu, Koyu and Lo Wen, Bandit Attack | Use the original Unite against an enemy at half HP or less. All three must also be Unite-eligible. | One eighth extra damage, up to 24. The native Knockdown chance and resolution still apply. |

All bonuses use integer rounding. The effects adjust damage during native
calculation or reaction construction. They grant no resource refill, extra action,
extra status roll or persistent reward. Repeating a calculation starts from the
native result or original reaction argument. Losing a partner cancels the bond
immediately. Battle termination and replacement clear admission. The existing
Arcane Guard bonus applies to magic. Warriors' Ward applies to direct physical
damage and uses a separate condition.

Camus has a reason to equip a Fire rune and spend the original Fire Wall charge
on armored groups. The Bandit trio has a reason to save its original Unite for a
finishing window. The spell and Unite base coefficients, fixed status chances,
descriptions, costs and animations retain their existing values. No new rune
or Unite is needed to use either option.

## Castle Guild

Choose **Party Strategy** at the existing attendant beside the Suggestion Box.
The page distinguishes current-party combinations from other combinations. It
explains the trigger, cap and requirement for available combatants. Field party
membership is a preview. Actual combat eligibility comes from live battle
ownership, status and native data. Use the usual navigation and Cancel to
return to the services list. The five services fit on one page.

## Verification

| Native comparison | Original calculation with the bond disabled | Bond enabled |
|---|---:|---:|
| Hix, guarding at low HP | 44 | 39 |
| Kinnison, poisoned target | 178 | 193 |
| Camus, Fire Wall against an armored target | 222 | 240 |
| Bandit Attack, wounded target | 252 | 276 |

Tests used the original loaded Unite records and native damage functions.
The Kinnison and Camus fixtures supplied the Attack and Fire Wall command
contexts directly to their damage helpers. Bandit Attack executed the original
Unite reaction-construction helper on disposable combat data. The paired calls
matched RNG state and repeated damage did not compound. All four cases
checked missing partners, KO, missing trigger and the global opt-out. The
suite also checked non-victory and replacement battle admission.

Original character lookup and curve indices were checked at representative
levels 12, 30 and 55. These are native lookups and controlled damage
comparisons. Actual level-ups, full fights, action animation, menu interaction
in a loaded field and campaign balance still await gameplay verification.

Release build, managed tests, native production, teamwork, Hunt, boss and
original war regressions passed. The copied game opened the stock launcher
and rendered the Party Strategy page and eleven existing Guild previews
with original frame, cursor and font. The copied-run preview did not load a
campaign.

Development DLL `1.40.0-party-revolution.1`

SHA256 `3FECA653E109E031662DE1E90EC214C21270110A194A9A1A82506DCB5EF68B7F`
