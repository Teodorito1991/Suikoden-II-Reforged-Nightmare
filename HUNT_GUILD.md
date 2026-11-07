# The Castle Hunt Guild

Sun King's Retinue has complete gameplay acceptance on **1.12.2-guild-ui.1**.
v1.13 preserves its implementation but has no separate user-assisted quest session.
See [exact-build evidence](docs/ACCEPTANCE.md).

## Take a contract

1. Reach headquarters through the ordinary story. Visit Leona's tavern in the
   audited castle variant and find the extra stationary Guild attendant.
2. Form a healthy party through **Leona before accepting**. Bring healing,
   physical/magical options and inventory space. No precise minimum party level
   has been calibrated for this elite encounter.
3. Talk using the game's normal confirm control. Navigate/confirm/cancel through
   the game's bound controls. Accept **Sun King's Retinue** and close the panel.
4. Travel normally to **Cave of the Wind**, without loading another save.
5. Walk in the eligible Cave field. The contract automatically requests the next
   eligible legitimate walking encounter. **No F6/F10/Ctrl is needed.**
6. Fight Sun King and Sorcerer in the original RPG engine, finish native victory
   rewards, and return to the field.
7. Return to the attendant, **report completion**, then accept again if desired.

## State, rewards and retries

No Contract → Accepted → Completed/Awaiting Report → Reported → available again.
Accepting does not immediately launch combat. Reporting grants no extra money,
item, XP or rune recovery. Native battle rewards remain authoritative.

Sun's configured bonus is **2,500 Potch**. A total3,478 reward and actual3,478
credit were observed under the accepted session settings; totals depend on
configuration and the native money cap. No daily cap or artificial grind ceiling
is added.

Stone of Power is a **fallback when the original loot slot is empty**, not a
guaranteed extra item alongside rolled loot. Original drops retain priority.
Native inventory capacity/decline UI applies. An offer is not proof of receipt;
two native inventory receipts were observed on v1.12.2.

Escape leaves the accepted contract retryable and grants no victory bonus.
Unrelated or scripted encounters must not become the hunt. Withdrawal is available
through the Guild. **Actual loading/restarting clears session-only contracts**,
including unreported completion. A remembered selection is not saved quest progress.
Never accept at castle and load a Cave save as a shortcut.

| Contract | Destination | Public proposal |
|---|---|---|
| Sun King's Retinue |Cave of the Wind, vd06_00| Enabled; accepted exactv1.12.2 quest |
| Dreamer's Circuit |Audited Sindar Ruins field vc12_00| Disabled; native-only |
| Thornwake Patrol |Audited forest field ve03_00; English location label pending| Disabled; native-only |

The attendant is limited to **vk08 tavern, event variant1**. Other castle stages
may have no attendant. No persistent Guild rank, training XP or war skirmish board
is implemented. No campaign flag or original save field stores contract state.
