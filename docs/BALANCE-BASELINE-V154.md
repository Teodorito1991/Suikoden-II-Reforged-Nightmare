# Great Balance Audit — pre-change baseline

Baseline: accepted v1.53 source at `4cd5f2675899ba0e328b762476f11abbaab43213`; accepted DLL `54144F2F61644F6B632F4513B2A4F30C240033AFD68F6F80CE1453051F7BA286`. Prepared before balancing edits. Full Content Preview on in-game Normal is the target.

**Verdict: inconsistent, with demonstrably stronger enemies but an exploitable high-level rune recovery economy. Whole-campaign fairness is not established.** No campaign was played for this audit.

## Evidence and method

A separately compiled diagnostic baseline used unchanged v1.53 gameplay plus explicit unloaded instrumentation. It is not the accepted binary. Genuine source asset loads, native growth-index lookups, native damage calculations on disposable actors, observed-copy admission and guarded stat application are component evidence. No battle scene, earned victory, story callback or save was executed. Native damage uses eight fixed seeds per mode. Scaling projected over raw damage is labeled as projection; it is not a claim that every controller enters the live hook. Audit-positive configurations enable otherwise optional boss policies only to test their guards; this does not enable them in Full Preview.

Coverage: 280 native enemy stat records, 560 Normal/Hard guarded stat cases, 492 same-stat native Normal/Hard damage comparisons, 320 growth samples (80 profiles × four levels). All 492 raw damage comparisons were identical. Native difficulty changed HP; no extra 1.2 damage factor was observed in these raw calls. The complete Hard controller/action path is not certified.

## Final effective scaling

| Scope | HP relative to original Normal | Damage entering the eligible hook |
|---|---:|---:|
| Bruiser | 1.35 | 1.30 |
| Raider | 1.25 | 1.40 |
| Bulwark | 1.60 | 1.20 |
| Fragile Pressure | 1.20 | 1.25 |
| Unprofiled eligible regular fallback | 1.60 | 1.45 |
| Double Head / first Harpy | 1.15 | 1.10 |
| Pest Rat | 1.20 | 1.15 |
| Sierra | 1.10 | 1.10 |
| Golden Wolf / second Harpy / first Worm stat plans | 1.00 | 1.00 |

Default role pacing REPLACES the corresponding embedded enemy profile or fallback; it does not multiply them. Deliberately customized per-enemy profile values take priority. Hunt-specific plans replace the selected factors. Exactly-once actor ledgers protect HP; outermost damage-call scope prevents nested calculation scaling twice. Source templates and enemy DEF/MDEF are not globally increased.

Normal HP = roundAway(source HP × selected factor), capped32767. Hard HP = roundAway(truncateFloat(source HP ×1.2, capped32767) × selected factor), capped32767. Example100HP Bruiser:135 Normal,162 Hard. A raw50-damage Bruiser hit becomes65 on either same-stat native calculation; native Hard HP is not an additional damage multiplier. Conditional defensive/bond reactions can reduce the result further. Negative/zero results stay unchanged; damage cap9999.

## Nine-stage quantitative comparison

Stages are campaign comparison brackets, not guaranteed chronological map access or encounter placement. Levels, party HP and outgoing damage are explicit illustrative assumptions, not observed saves, recommended level requirements or measured equipment curves. Damage DEF/MDEF budgets are native-fixture inputs:24/22 for enemy level≤15;68/65 for≤35;130/120 later. This overprotects some opening parties. No auto-level/equipment simulation is implied.

Native hit = the most damaging sampled legal mode of this representative enemy, not weighted average damage per round. Projected hit includes its selected factor. Hits-to-KO assumes repeated maximum hits with no healing/guard/critical/status; outgoing attack counts assume a fixed net single-target damage budget and no misses. Six characters, target spread, status, Unites and spell area effects change encounter turns materially.

| Stage / source | Assumed player Lv / HP / net hit | Enemy source Lv; HP vanilla→Normal / Hard | Enemy DEF/MDEF | Native hit→projected; hits to KO | Assumed attacks to kill vanilla→mod | Native Potch per enemy |
|---|---|---|---|---|---|---|
| Opening / Highland `va01_00_00` | 2–6 / 80 / 25 | 2; 15→20 / 24 | 6/14 | 2→3; 27 | 1→1 | 50 |
| Fortress / early recruitment; Ryube source `vb07_00_00` | 8–15 / 150 / 70 | 10; 300→480 / 576 | 20/15 | 44→53; 3 | 5→7 | 250 |
| Muse-era / Sindar source `vc12_00_00` | 15–22 / 230 / 110 | 16; 270→432 / 518 | 26/37 | 35→42; 6 | 3→4 | 400 |
| HQ / Cave of the Wind `vd06_00_00` | 20–28 / 300 / 160 | 21; 135→182 / 219 | 45/47 | 70→91; 4 | 1→2 | 550 |
| Midgame / Greenhill region `ve03_00_00` | 25–35 / 380 / 220 | 15; 45→61 / 73 | 25/28 | 22→29; 14 | 1→1 | 100 |
| Later battles / Matilda source `ve04_00_00` | 30–40 / 440 / 260 | 15; 45→61 / 73 | 25/28 | 22→29; 16 | 1→1 | 150 |
| Luca-era comparison / Twin Head Pass `vg04_00_00` | 32–42 / 480 / 300 | 42; 350→473 / 567 | 63/98 | 75→98; 5 | 2→2 | 2000 |
| Late dungeons / Tinto Mines `vg15_00_00` | 40–50 / 550 / 340 | 43; 210→284 / 340 | 75/82 | 38→49; 12 | 1→1 | 384 |
| Final approaches / late world table `wj_00_00` | 55–65 / 650 / 430 | 58; 1500→2400 / 2880 | 106/135 | 248→298; 3 | 4→6 | 15000 |

The low Greenhill sample is intentionally included: native regional tables contain weaker enemies too. This is not evidence that all midgame encounters are weak. Conversely, late attacks can remove a substantial fraction of the assumed650HP. No blanket HP increase follows from either example. Encounter selection frequency is unchanged by the reviewed hooks;58 remixes retain source encounter weights/count and audited positioning while changing legal composition. Consecutive-status fairness and complete battle duration remain live-play questions.

## Confirmed high-severity issue before changes

**HIGH — renewable top-level magic.** With packed charges `[0x77,0x55,0x33,0x01]` (only level4 depleted), one victory restores level4 to`0x11`. Repeating one level4 cast per eligible win returns to the same state indefinitely. LowestDepletedLevel only chooses order; it does not protect high spell tiers. This is independent of party level, equipment, RNG and the native growth model. It undermines dungeon resource budgeting and can combine with up to six casters. Escape/nonvictory and duplicate callbacks correctly grant nothing.

## Boss campaign versus optional Hall

Scripted bosses do not receive global stat scaling simply because their policy exists. Combat initially excludes custom main/ending handlers; exact source/roster/callback admission can grant StatsAllowed without granting ordinary victory recovery. AI policies are separately bound, and controller-specific attacks may bypass the generic calculation hook. Native legality, finite uses and scripted-command priority are retained.

The10-policy family and6 Boss Revolution policies are not16 blanket stat boosts. Full Preview retains EnableMajorBosses=false (Lucia1/2 and final Neclord off) and all four Legendary switches=false (Mist Shade, Gorudo, Worm/Moth, later Lucia). These deliberate opt-outs are preserved. Actual Luca campaign records remain native; accepted Hall three-party Luca does not replace the story sequence. Never count historical Lucia policies as Luca.

Core story HP plans: Double Head2500→2875; Pest Rat5200→6240; first Harpy7400→8510; Sierra2800→3080. Golden Wolf3500, second Harpy12000 and Worm4200 retain source HP through their1.0 plans. Native source values alone do not establish player-appropriate difficulty. Boss turns equal HP divided by actual sustained party damage, which is not known from a loaded record. At explicitly assumed party damage400/800/1200 per round,6240HP Rat takes16/8/6 rounds;8510HP Harpy22/11/8. Status, phase rules and recovery alter this.

Hall Original preserves its original HP source instead of applying the global actor HP boost. Original eligible damage paths may still use configured boss factors; it is not an entirely vanilla mode. Optional Nightmare clones HP and applies its configured extra damage once at the native reaction boundary, including special paths. It is not multiplied at both calculation and reaction. Full Preview Nightmare is OFF. Accepted boosted Beast Rune victory is functional evidence only; no ordinary-party difficulty conclusion. Luca Hall HP handoff and teams are unchanged.

## Remaining audit sections

The final report adds complete character/spell appendices, economy/war analysis, ranked risks, coordinated changes and final verification. This baseline remains a separate historical pre-change document.

Subsequent v1.54 native setup cross-check: six original BattleDataSetup calls (Normal/Hard on va01_00,vc12_00,va11_00),22 monster rows, confirm native HP and unchanged remaining combat parameters. This supports the baseline path; it is final-build component evidence, not an earlier accepted battle.
