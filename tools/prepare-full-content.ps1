param([Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'prepare-fresh-campaign.ps1') -OutputDirectory $OutputDirectory -Profile full-content
