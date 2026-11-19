# Fresh Campaign configuration proposal

Generate into an empty staging directory with tools/prepare-fresh-campaign.ps1.
Nothing is installed automatically. Read docs/FRESH-CAMPAIGN-V148.md before use.
Choose Normal in the original game. Existing personal settings stay untouched.

The full CFG and module-overrides.json define the baseline. selection.json names
the two recommended enabled Hunts; the other eight stay independently available
in source/config. nightmare-opt-in.cfg is an optional key merge, not a full CFG.
Do not combine the profile with developer/testing availability or damage cheats.
