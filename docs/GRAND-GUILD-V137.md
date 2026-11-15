# v1.37 Grand Guild expansion

Build `1.37.0-grand-guild.1`

DLL SHA256 `1C3762459932D79FDB8511572904CD7AB3E47502D3F0DBCEB076C98F9687273F`

| Tier | New contract | Original field and encounter | Formation and preparation | Bonus Potch | Possible fallback |
|---|---|---|---|---:|---|
| 1 | Bramble Road | Ryube Forest · `vb07_00`, 5 walking variants | Wild Boar, Spider, Woodpecker. Clear the status-capable pests before focusing the boar. | 450 | Mega Medicine, 3 uses |
| 1 | Trial Road Embers | Trial Road · `vb21_00`, 8 walking variants | Salamander, Flying Squirrel, Zombie Slug. Deal with the vulnerable squirrel and plan for magic. | 850 | Earth Orb |
| 2 | The Matilda Choir | Path to Matilda · `ve04_00`, 5 walking variants | Papa Holly, two DoReMi Elves with different original status moves. Stop the singers before taking on the frontline. | 2,200 | Stone of Magic |
| 2 | Twin Head Pack | Twin Head Pass · `vg04_00`, 3 walking variants | Air Lizard and two Killer Dogs. Protect vulnerable allies and consider broad attacks or a Unite. | 2,700 | Stone of Speed |
| 3 | The Undying Watch | Tinto Mines · `vg15_00`, 4 walking variants | Zombie Unicorn, Skeleton, Zombie, Neclordia. Prioritize enemy magic and ailments before the durable unicorn. | 3,800 | Sacrificial Jizo |
| 4 | Rakutei Vanguard | Rakutei Mountain · `vi13_00`, 8 walking variants | Sky Knight, Iron Claw, Leathercut, Pixie. Fight a late-game mixed group with fast attackers and ailment recovery. | 5,200 | Cyclone Orb |
| 4 | The Tenzan Trial | Tenzan Pass · `va11_00`, 3 walking variants | Minotaurus, Magus, Chimera. Bring magic defense and a healer, then isolate the Chimera. | 6,500 | Mother Earth Orb |

The existing Sun King, Dreamer and Thornwake contracts keep their original
rules. New entries use native monster HP and damage factors by default, with
strength determined by the formation and original enemy abilities. Tiers are
recommendations. The original story still controls travel; there are no new
chapter flags, forced quests or grinding requirements.

The attendant remains on the verified Suggestion Box corridor, `vk07` event
variant 3. No additional castle variant was admitted. The Guild shows at most
four contracts per page and follows selection across page changes with native
Up and Down bindings. Confirm accepts, and Cancel returns to the service menu.
The attached contract and its report state are session-only.

## Native encounter and reward boundaries

Each new contract uses a unique original walking table, an audited party slot
and a deliberate new mix of monsters from that table. Original slot positions,
callbacks, available moves and action legality remain authoritative.
The request requires the accepted party session, Normal difficulty, the
matching live field, an audited event variant with active walking encounters,
and the exact table pointer named by that variant. The existing original RNG
call still executes. Only the selected native party index changes.

The Potch bonus joins the original victory reward. The item is a conditional
replacement for an empty native loot slot. Original loot takes priority, and
the player still has to accept the item through the original inventory flow.
The purse and inventory keep their normal limits. Escape permits retry and
reporting grants no second payment. These rewards can be earned on subsequent
eligible victories.

## Verification

- Release build and managed regressions passed, including ten-row navigation,
  page boundaries, empty-list cancellation, reward opt-outs and old config
  loading.
- The copied game passed original-engine battle data setup and selector
  checks for all ten contracts. The native fixture verified 29 sprite atlases
  using the source monsters' actual OPD references. Field-source checks used
  the original map registry and each encounter-bearing event variant.
- Native reward calculation, original item/rune lookup and scratch inventory
  transfer passed for every reward, including native full-bag rejection.
  Escape, report, and repeat checks used synthetic lifecycle events.
- The launcher rendered ten in-engine Guild previews at 1920×1080 with the
  original frame, cursor and font. Representative captures cover three
  Hunt pages, preparation text, conditional items and existing Training
  screens. Screenshots and raw logs remain local.
- No new hunt was played end to end. The tests did not traverse full travel
  routes, play animations in a battle scene, verify live controller input,
  or obtain a victory or inventory item in an actual campaign.
