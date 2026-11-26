# Full Content Preview — v1.55

The owner's installed full-playtest selection also merges
[`full-playtest-opt-in.cfg`](full-playtest-opt-in.cfg): selectable Nightmare,
conditional growth-stone rewards and **unverified** eligible training defeat
return. Spoiler Mode still starts OFF. This separate overlay does not change the
generator's safer defaults below. [Installation evidence](../../docs/INSTALLATION-STATUS-V155.md).

Configuration proposal, not an installation or whole-campaign certificate.

Story tactics now select22 audited encounter records, up from13. This includes
Lucia1/2, final Neclord and the six supported Legendary records. They use original
legal moves, Normal/source/callback guards and native endings. HP/damage factors
are unchanged. Original campaign Luca and Beast Rune controllers remain native.
These activations are native/component-tested, with rendered story acceptance
still pending the owner's campaign. Individual JSON switches remain reversible.
[Before/after and exclusions](../../docs/STORY-BOSS-COVERAGE-V155.md).

Victory rune recovery restores one missing level1–2 charge per eligible survivor.
Level3–4 spells retain their native resource cost; inns/other native recovery remain.
This explicit profile uses RuneRecoveryMaxLevel=2; existing configurations retain
the legacy default4 unless changed. No enemy HP/reward inflation.
[Quantitative audit and limitations](../../docs/BALANCE-AUDIT-V154.md).

Choose Normal. Generate to a new empty directory:

```powershell
./tools/prepare-full-content.ps1 -OutputDirectory <empty-folder>
```

Selects all10 Hunts, core RPG/Party Revolution/Star Bonds, guarded original story
wars and all22 supported Original Hall definitions, including Original Luca and
Beast Rune. Source, compatibility, scene and resource guards still apply.
Story Progression is retained and Spoiler Mode defaults OFF. Sierra, second Harpy
and first Worm have no verified story witness; the confirmed player-facing
Unlock All Bosses preference can reveal them without changing campaign flags.
It never enables an independently disabled implementation. Unsafe/unimplemented
boss records are not selectable content.

Original Luca has historical exact-DLL complete three-team acceptance. Its
unchanged implementation is selected with experimental defeat/interruption and
Nightmare limits. Beast Rune's historical v1.51.1 victory/return passed with an
explicitly boosted disposable party; normal-party balance is unproven. It is a
late-game Bonus Challenge after Gorudo, not an individual final-boss clear claim.

Nightmare is independently optional: merge `nightmare-opt-in.cfg` only if wanted
and choose the tier in the Hall. Conditional stone offers respect original loot
priority and native inventory capacity. Contracts reset on load/restart.

Army Training now selects War1 Training, Fort Defense, Commander Rout and War3
Training, with their original completion prerequisites. Standard and supported
Veteran tiers remain selectable; qualifying victories earn750/1,500 Potch. Exact
v1.53 Commander Rout Standard victory/return/autosave/readback/reload passed.
All drills share the same production session/save boundary; scenario ending and
reward guards remain independent. Other current-build drill/tier paths were not
played in this pass. No materially different save entry was identified for their
ordinary completion. Individual switches allow any drill to be disabled.

EnableTrainingDefeatReturn remains OFF: training defeat uses native Game Over,
not an unaccepted current-build redirect. No commander XP. Developer tools,
damage protection, unsafe scripted switches and the retired arena remain OFF.
The shipped Settings/config.example defaults stay conservative; this explicit
Full Content profile is the opt-in. Personal configurations are not overwritten.

No DLL installation, save modification or overwrite occurs when generating this
profile. The separate Fresh Campaign baseline remains unchanged.
[Current training acceptance and limits](../../docs/ARMY-TRAINING-RESULTS-V153.md).
[Historical Hall integration evidence](../../docs/FEATURE-COMPLETION-V1511.md).
