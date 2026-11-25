# Great Balance Audit — v1.54

## Verdict and scope

**Inconsistent, with substantially stronger ordinary enemies and richer tactical choices, but no evidence-based claim that every story boss is substantially harder.** The clearest proven weakness was renewable high-level magic. v1.54 fixes it in the intended Full Content profile without changing enemy HP, character growth or rewards.

This is a quantitative component/source audit, not a full campaign balance certification. [The baseline](BALANCE-BASELINE-V154.md) was written before balancing changes. It contains the actual scaling paths and nine campaign-stage comparison table. [All80 character comparisons](BALANCE-CHARACTERS-V154.md) cover four levels each. Source baseline is accepted v1.53; its gameplay acceptance remains attached to its own hash.

## Effective difficulty and progression

- Normal is the intended game selection. Most audited enemies use role replacements: HP×1.20–1.60 and eligible damage×1.20–1.40. Global1.60/1.45 are fallbacks, not another multiplication after those role factors.
- For a100HP enemy: Bruiser135, Raider125, Bulwark160, Fragile Pressure120. A raw50 hit becomes65,70,60,63 respectively (midpoint away from zero). Native DEF/MDEF are unchanged. Additional conditional guard effects can reduce incoming damage.
- Hard increases source HP by native1.2 before the selected mod HP factor; truncation precedes mod rounding. The same100HP examples become162,150,192,144. Same-stat raw native damage did not change in the sampled Normal/Hard calls. This does **not** establish the full Hard action/controller pipeline as identical. Major tactical policies/Hunts are Normal-only. Hard is not recommended as a straightforward second multiplier.
- No extra encounter-frequency modification was found.58 guarded formation remixes change legal combinations while preserving original count/position/encounter-source boundaries. Source-table weights and world travel gates remain native.
- Native EXP curve/level catch-up and level/stat caps are unchanged; Full Preview uses EXP×1.0 and Potch×1.0. The generic example's Potch×0.85 is not the Full profile. No invented XP ceilings, daily caps or mandatory Hunt progression.

## Ranked risks

No CRITICAL defect was established. Severity refers to evidence and impact, not a request to nerf every strong option.

| Rank | Severity | Finding / evidence | Decision |
|---|---|---|---|
|1|HIGH, addressed in Full profile|One high-level spell per eligible win could be completely free when lower tiers were full; pure rule counterexample and native completion boundary reproduced.|Full recovery now ends at level2. Legacy4 remains available/config-compatible.|
|2|MEDIUM, direct source|Boss policy count is not universal scaling/activation. Full deliberately leaves MajorBosses and four Legendary switches OFF; scripted/special paths need separate admission.|Document actual scope. Do not silently activate opt-ins or weaken story guards.|
|3|MEDIUM, native-growth model|Earlier rune slots and physical/magic/DEF growth buffs can offset enemy pressure; several base-stat gains exceed30–50 points at queriedLv55.|Preserve identities; actual equipment/affinity/multi-hit parties needed before targeted nerfs.|
|4|MEDIUM, formula + assumptions|Late enemies can inflict roughly280–300 projected damage against the chosen DEF130/MDEF120 vector; focus or multiple attackers can overwhelm weak parties.|Keep targeting/recovery windows; these are not proven unavoidable wipes.|
|5|MEDIUM, source/native components|Statuses, action cadence and mixed formations threaten action economy; source status bits do not establish live application probability or permanent lock loops.|Native legality/finite uses/repeat penalties preserved; no guessed blanket status nerf.|
|6|MEDIUM, direct economy|Repeatable Hall/Hunts/stones can make optimized parties very powerful; Spoiler Mode deliberately opens later rewards early.|Intended optional progression, openly selected. Native capacity/caps and victory receipts remain; no daily caps.|
|7|MEDIUM, native lookup + unknown travel|High weapon prices and six-person upgrade needs may strain ordinary progression despite full native Potch.|Price examples below; no payout cut or grind requirement added without shop/travel budgets.|
|8|MEDIUM, evidence gap|Luca/Beast accepted functional tests, some with unusually strong parties, do not prove Normal story balance.|No boss replay requested and no HP inflation. Actual campaign review remains.|
|9|MEDIUM, source + historical war play|Upgraded commanders can aid the player as well as the enemy; forced story outcomes remain and limit tactical difficulty.|Preserve scripted boundaries; no unsupported promise that all wars are hard.|
|10|LOW, direct/native cap|Full purse means earned Potch is credited0; native loot priority/full inventory can prevent fallback item receipt.|Correct native behavior; reward presentation remains conditional.|

## Coordinated changes

| Setting / code | Before | After |
|---|---|---|
|Full Content recovery|1 total charge; lowest depleted among levels1–4|1 total charge; lowest depleted among levels1–2|
|New RuneRecoveryMaxLevel|Absent|Validated1–4, default4 for existing/fresh configurations; Full explicitly2|
|HighestDepletedLevel|Could select level4|Respects the same configured maximum|
|Spillover above selected tier|Implicitly possible|No spillover, even when lower tiers are full or recovery budget is large|
|Native audit inventory fixture|Called native item search before scratch character/cache owners existed; logged interop exceptions|Complete disposable owners initialized before inventory calls; no production inventory routine changed|

At Full2, a party with full low-level charges and depleted level3/4 receives no charge back. A depleted level1/2 still receives one total, subject to existing survival/eligibility rules. Escape, defeat, interruption, foreign managers and duplicate completion remain excluded. Native charge maximums, inns, other recovery and optional stat grinding are unchanged. No personal setting is overwritten or silently migrated.

No enemy/boss stats, damage multipliers, character profiles, magic powers, native story callbacks, Hall rewards, Hunt rewards, training save protection or commander XP systems changed.

## Characters, weapons and defensive builds

All80 profiles were resolved and compared, including14 Party Identity overrides over15 RPG Evolution revisions over the base profiles. A custom row takes priority; removed rows are not reinserted. Growth nibbles are curve selectors, not percentage multipliers. Native lookup/table samples establish relative gains, not starting/recruitment stats. Compatibility checks on fixed records remain strict; mixed old-save profiles can be rejected safely rather than overwritten.

High physical offence and multi-hit/critical combinations retain native caps, hit checks and equipment legality. The native Double-Edged outgoing factor is2×; two copies trigger the presence branch rather than4×. This does not certify every native Fury/critical/multi-hit interaction or its incoming tradeoff. Earlier slots make native passive combinations available sooner. Those combinations require live equipment/party evidence before balancing changes.

Star Bonds are modest conditional additions, not permanent global multipliers: guardian reduction min(damage/8,20), poisoned-target archer bonus min(damage/8,15), fire-battlemage bonus min(damage/8,18), bandit bonus min(damage/8,24). They require exact paired/triple roster, action, status/health/native Unite identities and Normal. Arcane Guard adds conditional magic protection while actually defending; guarding consumes a turn. No immortal-party loop was demonstrated.

### Native player damage examples

These are original calculation calls on synthetic disposable actor stats, with no passive runes/criticals/status/party history. PhysicalAttack is the already calculated combat parameter0; it must not be mistaken for base STR before weapon calculation. Parameter7 was varied by the harness and did not add weapon power in these calls. Weapon upgrades affect the upstream calculated attack, not an independently assumed second addition here.

| Final physical attack / enemy DEF | Native eight-seed damage range | Interpretation |
|---|---|---|
|30 /8|23–30|Opening-style counterfactual, not an observed starting character|
|95 /45|51–58|Matched intermediate input|
|160 /110|51–58|Higher defence absorbs the extra attack; higher level alone does not guarantee more damage|

These samples demonstrate formula sensitivity, not realistic endgame weapon builds. The nine-stage baseline's net damage budgets are separate explicit estimates. DEF improvements are meaningful because native physical damage can be reduced sharply; magical pressure checks MDEF separately.

## Magic, healing, physical runes and Unites

Under neutral tested affinity, native magic damage follows adjusted spell effect + caster magic − target MDEF, floored at0; tested guard status reduces it to one third. Native healing helper uses adjusted effect + magic. Affinity, immunity, target rules, action/effect controllers and HP clamps still apply. Example: effect180 with magic70 and MDEF50 yields200 damage; healing helper yields250. This is not a guaranteed250 received HP if the target lacks that much missing HP.

31 effective spell fields follow.32000 denotes a special original full-heal value; it is not a literal32000HP comparison. Drops of Kindness/Healing Wind therefore become finite healing, not enormous buffs relative to vanilla. Spell fields are not final damage.

| ID / spell | Native effect field | Base mod | Effective Full Preview |
|---|---:|---:|---:|
| 1 Flaming Arrows | 60 | 80 | 80 |
| 2 Fire Wall | 150 | 200 | 180 |
| 3 Dancing Flames | 300 | 390 | 370 |
| 4 Explosion | 700 | 780 | 780 |
| 5 Final Flame | 900 | 1050 | 1050 |
| 6 Drops of Kindness | 32000 (special full-heal value) | 100 | 180 |
| 8 Rain of Kindness | 300 | 420 | 390 |
| 12 Healing Wind | 32000 (special full-heal value) | 210 | 240 |
| 13 Shredding Wind | 450 | 540 | 500 |
| 15 Shining Wind | 500 | 580 | 580 |
| 19 Earthquake | 800 | 980 | 980 |
| 21 Angry Blow | 100 | 145 | 165 |
| 22 Thunder Runner | 120 | 220 | 195 |
| 23 Bolt of Wrath | 600 | 640 | 640 |
| 24 Lightning Ball | 1000 | 1040 | 1040 |
| 25 Thunder Storm | 1200 | 1250 | 1250 |
| 27 Black Shadow | 500 | 580 | 580 |
| 29 Judgment | 1500 | 1350 | 1350 |
| 30 Scolding | 30 | 110 | 110 |
| 32 Exorcism | 400 | 560 | 560 |
| 33 Scream | 300 | 360 | 360 |
| 34 Great Blessing | 70 | 100 | 120 |
| 35 Shining Light | 130 | 210 | 210 |
| 36 Battle Oath | 300 | 360 | 360 |
| 37 Sign of Forgiveness | 2000 | 1900 | 1900 |
| 43 Set! | 150 | 240 | 240 |
| 45 Stealer of Souls | 300 | 380 | 380 |
| 47 Open Gate | 50 | 100 | 125 |
| 48 Kings Road | 150 | 260 | 260 |
| 49 Pale Palace | 500 | 650 | 650 |
| 50 Empty World | 900 | 1100 | 1100 |

There are10 distinct improved physical techniques across Mastery/Reality/RPG, not just the old six: IDs80 200→225;82 100→115;83 100→125;84 200→220;85 200→230;86 200→215;90 150→175;91 100→115;92 100→115;97 200→225. ID namespaces matter: MAGI82 is not equipment-item82 Double-Edged. Native resistance doubles/halves/nullifies the appropriate physical technique before coefficient rounding; native recoil/status restrictions remain.

There are14 distinct improved Unite coefficient records:12/13/14 50→65;17 150→165;18 100→120;24/38/39 50→65;7 150→175;15 100→115;16 50→65;30 200→225;35 75→92;42 150→175. They apply to separate IDs; coefficients are not repeatedly multiplied across modules. A100 native physical result at coefficient225 yields225 before other native target/reaction conditions. Combined actions consume their original participants and preserve aftermath; coefficient size alone is not DPS per party turn. No proof that every enhanced Unite trivializes bosses was found.

Victory recovery was the proven exploitable interaction: powerful area magic plus freely restored top charges, not a failure of the native spell formula. Full2 retains useful low-level healing/damage sustain and makes high-tier choices matter. This does not limit permanent character power from optional grinding.

## Economy and optional progression

Original weapon prices were read through actual G2_arms_val on disposable character data: stored weapon levels1/4/8/12/15 returned300/2,700/11,000/43,000/70,000 Potch. These are lookup quotes, not completed purchases or cumulative upgrade costs. Six identical43,000 quotes total258,000; six70,000 quotes420,000. Actual party progression needs cumulative purchases, armour costs, loot resale and accessible blacksmith/shop tiers; this audit did not establish a complete campaign affordability ledger. No price hook was found and no prices were changed.

Ten Hunts reuse original EXP/Potch/items plus contract bonuses; the accepted Sun loop historically credited3,478, but that is a particular source formation/bonus result, not every Hunt payout. Dreamer adds1,200 and Thornwake3,000. Hall native reward examples include Golden Wolf8,500 and first Harpy34,000; repeat farming is intentional. Full native Potch multiplier1.0 precedes eligible custom Hunt bonus. Native Prosperity changes original money before the bonus; it does not repeatedly double the bonus. Native purse cap999999 remains.

Hunt fallback items require an eligible victory, original loot slot availability and native inventory acceptance. Reporting only changes quest state, never pays again. Escape/defeat grant no victory payout. Optional Nightmare stone offers use25% conditional rolls and lose priority to original loot. Four eligible independent rolls have about68.4% probability of at least one offer (1−0.75^4), only if all other eligibility conditions hold. No inventory receipt is guaranteed by that figure. Native permanent-stat cap255 remains; unlimited earned repeats are intentional.

Army Training qualifying rewards are750 Standard /1,500 Veteran, victory-only and exactly once under the existing outcome ownership. No RPG EXP, commander XP or permanent army training stats are granted. Current historical save/return acceptance is preserved; this audit did not change or retest save persistence. Currency farming is a transparent optional advantage, not a hidden campaign requirement.

EXP multiplier remains1.0 on eligible paths. Enemy-level-dependent native EXP progression remains; no new exact EXP-per-character totals are invented because actual party levels, division/catch-up and victory context were not simulated. No evidence supports reducing EXP/Potch or making normal story progression depend on Hunts.

## Original story-war assessment

21 commander/branch profiles use disposable battle-local copies;9 specializations select distinct profiles rather than accumulating permanent bonuses. Active abilities must retain original init/action/check/finish/select delegates. Selected authored examples: Flik ATK8/DEF7; Viktor10/7; Humphrey7/11; Georg12/7; specialized Solon9/9, Seed11/6 and Kiba8/13. These are configured targets subject to runtime admission, not observed before/after vanilla stat deltas for every scenario.

Tactics/maneuvers cover wars1,2,3,6,10,13,15. They score native legal candidate actions using local threat, support, focus, commander exposure, ability utility and repeated-move penalties. This changes choices rather than creating guaranteed hit rules or another combat engine. Staged wars10/15 receive local maneuver changes, not invented recruitment/ending outcomes. Terrain and original legality remain native.

War2 retains audited fort/commander ending paths and20-turn bound. Separate scenarios3/6/13 retain maximum-turn24/18/12 policies with objective prerequisites; tactical fallback rout thresholds2/4/4 require their original turn/hero conditions. These are not unconditional early-win commands. Some story outcomes remain scripted, so 'harder army stats' does not establish a freely winnable/losable scenario. Original casualty/recruitment/campaign writes remain story-owned. Training uses its own verified return/save contract and does not confer permanent commander growth.

Native/component and historical player-controlled drills establish working mechanics. They do not prove a balanced full story-war campaign. No war code, objectives, casualty handling or save mechanism was modified during v1.54.

## Evidence limits and first-campaign checks

- No new gameplay victory/defeat, Hall/Luca/Beast replay or complete campaign was performed. v1.53 accepted gameplay remains historical exact-hash evidence.
- Quantitative stage levels/HP/equipment/net-hit budgets are assumptions. Status probability, criticals, targeting sequences, actual shop tiers and fight duration need the owner's normal campaign.
- Raw same-stat Hard comparisons are native evidence; complete Hard scripted controller coverage is not certified. Use Normal.
- Watch early small parties, repeated statuses, first actual enhanced Unite/passive combinations, high-affinity casters and late enemy focus. Report a concrete battle/party when an issue appears; no new test marathon is required.
- The update does not install itself, start the owner's campaign, alter personal configurations or publish a beta/release.

## Verification and artifact

Build `1.54.0-balance-audit.1`, SHA256 `9A6643D7955E0F9B39B681950BABBA2F29CA4B30231AFF1B5D9320A38F8DDB40`. Release rebuild is byte-identical. Retained managed suite, both generated profiles and native production audit passed. 492 same-stat Normal/Hard damage comparisons matched; six actual native BattleDataSetup calls across early/middle/late tables yielded22 monster rows with unchanged source parameters and correct native HP.80 profile comparisons/four levels, native weapon-price lookup and guarded recovery completion passed. Full Content graphics startup loaded successfully, selftests/developer tools OFF, recovery level2, no campaign load. Known shutdown-only font NRE/JobTempAlloc warning remains; native audit and BepInEx startup logged zero errors.

Final exact-hash verification is recorded in [the sanitized milestone record](../publication/development-build-v154.json). Managed/native tests are component evidence; Full Content startup is not rendered campaign acceptance. The accepted v1.53 fallback, original14 saves, installed plugin and personal configurations remain preserved. No beta/release changes.

Reproduce managed checks with `dotnet run --project tests -c Release`, `./tools/test-fresh-campaign.ps1`, `./tools/test-full-content.ps1` and `python tools/audit-publication.py`. Native verification requires the supported locally owned game/interop plus explicit unloaded production-audit flags; those proprietary files and private native traces are not in this repository.

Copied test runtime closed normally. Automatic approval review rejected its reset with only “blocked by policy”; no reset retry or alternate route was used. The closed copy retains the candidate/Full profile under its startup-only guard. Steam, original saves and personal settings remain untouched.
