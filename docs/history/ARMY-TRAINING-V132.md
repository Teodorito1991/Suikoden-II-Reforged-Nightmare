# Army Training v1.32

At Leona's tavern, highlight the training row. Left and Right cycle through
each drill and its available difficulty tiers. Check the right panel before
confirming. With all drills and tiers available, one Left press from the
default War 1 Standard selection takes you to War 3 Veteran. Each original scenario must already
be completed in the campaign.

Standard uses the existing training rules. Veteran adds the established
enemy tactics evaluator to the original units and their legal actions.
It uses terrain, nearby allies and exposed targets. Both tiers use the
same original formations, maps and battle rules.

Routing the enemy commander with allies still on the board earns 750 Potch
on Standard or 1,500 on Veteran. Defeat, a breached fort and a timed ending
without a commander rout pay nothing. Fort Defense uses the existing
commander-rout ending when allowed by the War 2 policy. Potch is credited
once through the game’s purse routine, after teardown and the return audit.
The native purse limit applies.

## Deployment and persistence

The drills remain opt-in in the copied runtime. In the Native War configuration,
`EnableTrainingVeteran` and `EnableTrainingRewards` independently enable the two
new features. Veteran also requires the native war AI and tactics options
and the audited scenario. War 3 loss interception requires
`EnableTrainingDefeatReturn` and the observed native defeat result, ending 3
and its game-over flag.

Potch persists through the standard game save. Commander XP is deferred.
A separate progression file needs stable campaign identity through slot copies
and branching saves. Using these slots or temporary counters would allow
progress to transfer to an unrelated campaign. The drills grant no story-war
stat bonuses.

## Verification

- Candidate `1.32.0-army-training.2`, SHA-256
  `5BB9D7FB0A44B7738D81181CC4CB9992205B876E023CB6DA868A6A6E462F0945`.
- Release build, managed regression and copied-game startup passed. The new
  managed checks cover qualifying routs, timed endings, fort breaches, defeat
  handling and the training reward card.
- The v1.31 player test established a complete War 3 normal return. v1.32
  gameplay acceptance requires an earned defeat, another entry and return,
  and a commander rout with an observed purse credit. A clean return compares
  five flag arrays, character, party and war data before granting intentional Potch.

- Owner playtest on the exact v1.32 candidate confirmed the corrected Left/Right
  selection and native War 3 Veteran entry. Tactical AI admitted the native
  battlefield. The stock timer ended the drill with the enemy commander still
  active. The game returned to the tavern, recorded zero Potch and enabled
  another request. All five restored flag arrays matched. The compared game,
  war, character and party records differed only in position and clocks.
- War 3's original scripted sequence and deadline prevented earning a defeat
  in this run. The War 3 defeat redirect and a second consecutive War 3
  entry remain unverified.
- In the same session, the owner entered Veteran Commander Rout, routed the
  commander and returned through native ending 1. The receipt recorded
  1,500 Potch earned and zero credited. The disposable checkpoint already
  held the native 999,999 Potch maximum. The two drills generated one
  receipt each. A positive wallet increase below the cap remains unobserved
  on this DLL. The native cap applies independently of item inventory.
- Both returns recorded matched destinations, restored flag equality and
  zero save calls before restoration. The audited records differed only
  in position and clocks.