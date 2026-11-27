param(
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'

$fullScript = Join-Path $PSScriptRoot 'prepare-full-content.ps1'
& $fullScript -OutputDirectory $OutputDirectory

$cfgPath = Join-Path $OutputDirectory 'BepInEx/config/nightmare.suikoden2.reforged.cfg'
$overlay = @"

[Army Progression]
EnableArmyProgression = true
EnableDrillDirectives = true
EnableCrossTheaterSynergy = true

[Native War]
EnableTrainingExtendedWars = true

[Castle Guild]
EnableExpandedCastleNpcs = true
EnableHuntModifiers = true
EnableGuildMenuAudio = false
GuildAudioMoveId = -1
GuildAudioConfirmId = -1
GuildAudioCancelId = -1

[Combat Reforged]
EnableBattleTacticalHud = true
EnableCombatCombos = true
EnableWeaponMilestonePerks = true
EnableBossTelegraphs = true
EnableApexEncounters = true
EnableDesperationSurge = true
"@

Add-Content -LiteralPath $cfgPath -Value $overlay -Encoding UTF8

# Refresh SHA256SUMS.txt so the staged v1.56 playtest bundle stays self-verifying.
$sumsPath = Join-Path $OutputDirectory 'SHA256SUMS.txt'
$files = Get-ChildItem -LiteralPath $OutputDirectory -Recurse -File |
    Where-Object { $_.FullName -ne $sumsPath } |
    Sort-Object FullName
$lines = foreach ($f in $files) {
    $rel = $f.FullName.Substring($OutputDirectory.TrimEnd('\', '/').Length + 1).Replace('\', '/')
    $hash = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash
    "$hash  $rel"
}
Set-Content -LiteralPath $sumsPath -Value $lines -Encoding ASCII

Write-Output "v1.56 playtest profile generated in $OutputDirectory. No Steam install or saves modified."
