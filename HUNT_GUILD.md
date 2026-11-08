# The Castle Hunt Guild

Sun King's Retinue has complete gameplay acceptance on **1.12.2-guild-ui.1**.
v1.19 refines its presentation and feedback; it has no separate user-assisted quest session.
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
| Sun King's Retinue |Cave of the Wind, vd06_00| Enabled; accepted exact v1.12.2 quest |
| Dreamer's Circuit |Audited Sindar Ruins field vc12_00| Disabled; native-only |
| Thornwake Patrol |Greenhill City area, ve03_00; precise route unplayed| Disabled; native-only |

The attendant is limited to **vk08 tavern, event variant1**. Other castle stages
may have no attendant. No persistent Guild rank, training XP or war skirmish board
is implemented. No campaign flag or original save field stores contract state.

## v1.19 presentation

Contract details separate destination, targets, preparation and victory spoils.
Unavailable contracts remain visible for browsing, with a clear unavailable prompt;
failed acceptance now appears in the screen's message line. Confirm/cancel retain
the original game's bindings. Cancel closes the panel without withdrawing a hunt.

Dreamer and Thornwake stay experimental and disabled by default. Thornwake's region
uses the original map registry and save-location text alias (3052 → 432, Greenhill
City); the exact walking route has not been played. No encounter, balance, reward
or drop probability was changed. Conditional item offers still depend on native
loot, eligibility and inventory handling; reporting grants nothing extra.

The final development DLL is `0EF0B648CC897C2119ED6919DA250FD1FDFFD1CAC5F8F536FB8BA8B5A8A1BEA7`. Original frame/font/cursor rendering was
exercised in an unloaded preview. Offline previews are not gameplay screenshots,
and no current-hash field, controller or complete-quest acceptance is claimed.
