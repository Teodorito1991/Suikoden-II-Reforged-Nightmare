# Hall of Echoes safe return

Development build `1.42.0-hall-recovery.2`

DLL SHA256 `95956B88BBDB3090E14333213B41A32F1B103D2E1B843F018FFB80601136E3F4`

## Return ownership

- On defeat, after native battle release and chapter destruction, the trial
  retires the matching Reforged combat state. The original Game Over path can
  bypass `BattleEndWait`, leaving state that prevents Castle Guild insertion
  on the next map initialization. The cleanup checks the released trial's
  battle work identity and the old Guild owner.
- Original chapter result 1 covers both victory and a field return without
  victory. The Hall now observes `MACHICON.OnEndBattle` for its exact field
  and interpreter after teardown, then checks the return position and
  releases its resources. This also allows cleanup after leaving the fight
  early. The original victory callback remains authoritative for payout.

## Persistence evidence

The copied-runtime trace captured the write to Suikoden II's `Data0` on the
new `MapChapter` at `MachiMain` step 14, after selecting the stock Retry
checkpoint. The two traced save layers were `SystemSave.Save` and
`SteamService.Save`. The game wrote the native save payload and the file
hash changed before field control returned. The original executable has
an autosave path from `MachiMain` via `AutoSaveCheck`, `UISaveLoad2.Save` and
`GSD2SaveData.SaveFile`. V1.41's inert high-level counters did not establish
the absence of native saves.

The prior-area and prior-town history changed on the first checkpoint load
before a Hall request, and the same change recurred during Retry. The older
broad before/after comparison included this normal map initialization.

The ordinary overworld battle and the Hall battle both used real native
defeat, followed by Retry to the same numbered Headquarters checkpoint.
Each recovered chapter wrote one new autosave via the same native save
path. The trace counted no write from either battle's defeat itself.
Both decoded Retry autosaves differed from the selected checkpoint in
the save timestamp, play and event timers, and the same native return
position update. The two autosaves differed from each other only in the
timestamp and timers. Whole character records and party records matched
the common checkpoint, including roster, formation, stats, equipment,
runes, health, charges, status and character progression. The flag
arrays, shop and war data, currency and inventory also matched.

The earlier unmatched comparison used autosaves from different points
in the campaign and could not attribute party lineup or mutable character
differences to the encounter. The matched control found none.

## Rewards

Pest Rat retains the original monster payout and item table. The monster has
2,000 raw Potch and Poison Orb in its native drop table. The ordinary native
monster reward calculation and configured Potch multiplier determine the
result. The Hall supplies no supplemental payout or story reward and uses
the original drop roll, inventory handling and Potch cap.

With a 100,000-Potch test wallet, the owner won twice without reloading.
Each fight credited 17,000 Potch and added one Poison Orb slot. Each returned
to the original player position, passed the five flag-array, war data and
selected progression comparisons, and released the owned HD scene and
Pest Rat pack. The private observer recorded zero save writes during these
two wins. Full-bag behavior remains unobserved in a rendered Hall battle.

## Exact-build Retry

On `1.42.0-hall-recovery.2`, a native Pest Rat defeat awarded nothing and
released its owned battle resources. The Hall cleared the old combat
state, native Retry loaded Headquarters with a new field owner, and the
owner interacted with the restored attendant to start Pest Rat again.
That rematch ended by fleeing, returned to the same Guild location,
awarded nothing and released its owned resources. The earlier two
consecutive wins establish the victory and reward path on this same DLL.

This artifact is only available in the protected experimental build.
The managed regressions and the exact DLL's 5,046-check native
production audit passed. The startup audit loaded no campaign.
