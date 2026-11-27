# v1.36 Total War Revolution and Castle Guild Reborn

Development version `1.36.0-total-war-guild.1`

DLL SHA256 `9B3E8EAE969F436BE9607A09F25A2C50C40E4A87DF4DCC0B5050BAB799621003`

## Original wars

The existing War Tactics evaluator adds bounded preferences to legal candidates
in Wars 1, 2, 3, 6, 10, 13 and 15. It considers the difference between the
unit's current exposure and the proposed position. Infantry can screen threatened
ranged allies and guard the commander. Ranged squads favor firing positions
outside immediate threat. A wounded attacker discounts a poor trade against a
target that appears able to counter. A credible finishing attack gets priority
until enough squads are already committed. The existing native ability evaluator
still supplies utility, uses and friendly-victim checks.

| War | Tactical emphasis |
|---|---|
| 1 | Preserve a line and avoid poor exchanges. Opponents can exploit an exposed squad. |
| 2 | Add progress through native terrain toward the fort, with a penalty for walking into concentrated fire. |
| 3 | Value a reachable advance while respecting the scenario's original staging. |
| 6 | Screen ranged units and guard the commander when the player threatens the line. |
| 13 | Combine forward pressure with stronger commander protection. |
| 10 and 15 | Use smaller scoring adjustments and keep all staged outcome, control and reinforcement decisions under existing handlers. |

The change applies to story AI and compatible Veteran drills through the same
native evaluator. Standard Training uses its existing rules. The new layer is
independently controlled by `EnableWarManeuvers`. Allied unit stats and legal
commands still use the existing specializations. No new recruitment, casualty,
reinforcement or story-ending handlers were introduced.

In disposable boards on the seven original maps, the native probe evaluated
398 reachable movement candidates. The new preference changed 286 scores.
The combined tactics selected 48 alternatives compared with the prior native
evaluation path without the tactics overlay. The comparison measures decisions
on the fixture boards. It does not measure tactical turns played, actual
casualties, campaign transitions or the effect of a particular stat build.

## Castle Guild

The new attendant is on the second-floor Suggestion Box corridor outside the
meeting area. The engine resolves it as area 10, town 6, `vk07` event variant
3, background map 1. The original box has both visible and alternate-state
records at event coordinates **26, 26**. Its original event leads to the
registered Suggestion Box overlay. The Guild appends one stationary original
sprite actor at **29, 28**, southeast of the box. Approach from the south and
face north to talk.

The exact source and cache-ownership guards remain in force. The fixture loads
the original room, verifies the box's route and native collision samples for the
clerk and approach, then clones the room. It compares all original objects and
script records and rejects the former tavern and other room versions. Native
events see Confirm before the Guild. The Guild opens only if the native tick
leaves the exploration state idle and the player is still facing the clerk.
Native event teardown releases the mod-owned clone.

The old tavern insertion has been replaced. This build supports this one
verified event variant. The similar `vk35` room lacks the audited
letter-reading route and is excluded. The scene has not been visited in a live
campaign with this DLL.

Army Training keeps the previously verified native transition. It records the
player's actual coordinates and current map/variant/cache slot before the fade,
then verifies that corridor identity and the saved position at the return
boundary. Training still requires the copied runtime's save-root guard, the
opt-in and the appropriate campaign progress. The content test did not launch a
training drill from the new corridor or observe a return there.

## Using the new menu

- **Hunt Contracts** shows destination, enemies, difficulty, availability and
  victory spoils. The special item is conditional on the native loot slot and
  inventory space.
- **Army Training** lists War 1, Fort Defense, Commander Rout and War 3.
  Select the drill, then Standard or Veteran. The details explain availability,
  the objective and the reward conditions. A qualifying win gives 750 or 1,500
  Potch, subject to the native purse cap. Timed endings and defeats award zero.
- **Guild Status** shows the current contract, objective and the report or
  withdrawal action. Reporting pays no additional reward.
- **Leave** closes the menu. Cancel goes back one screen and closes at the
  service list. Each new screen requires a released Confirm before another
  action can trigger. Navigation uses the game's existing bindings.

## Verification

The Release build compiled with zero warnings or errors. The managed suite
covered all service paths, both tiers, blocked cases, the confirm fence,
the new approach geometry and 16:9, 4:3 and ultrawide layout bounds. The
retained production native fixtures passed, including the v1.35 boss policies,
58 remixes, RPG Evolution and all three hunt integrations.

The copied game reached the original launcher and loaded the verified original
frame, cursor and font. The hidden graphics preview timed out with zero GUI
draws, so there is no screenshot or rendered-menu acceptance for this build.
The graphics process exited normally with the known Unity TextMeshPro shutdown
exception and JobTempAlloc diagnostic. No campaign was loaded in these checks.
