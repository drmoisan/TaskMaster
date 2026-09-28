<#
.SYNOPSIS
    Pure epic readiness predicates for epic-scope gate decisions (issue #663).

.DESCRIPTION
    Evaluates the epic checkpoint artifacts/orchestration/epic-orchestrator-state.json
    for the two epic-scope legs that previously had no epic seam: the integration pull
    request (gate 1) and the preimplementation gate's command and path legs (gate 4).
    Each predicate returns an empty string when the checkpoint is ready, or the name of
    the first failed conjunct, so a gate can name the conjunct in its denial reason.

    The shared conjunct names (route_id, epic_feature_folder, epic_manifest_path,
    integration_branch, features) match Get-EpicOrchestrationReadinessFailure in
    .claude/hooks/enforce-orchestration-preimplementation-gate-modes.ps1, without its
    delegation-specific target-record conjuncts. That hook-local file is neither imported
    nor modified.

.NOTES
    PowerShell 7+. Pure: no import, no filesystem read, no subprocess, no network, no
    clock read, and no environment read. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
    AUTHORITY: PowerShell-authoritative. The epic readiness predicates have no Python reference implementation, and no enforcement hook invokes Python (issue #663, D5).
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The only route id that describes an epic checkpoint. Compared case-sensitively.
$script:EpicRouteId = 'epic'

# The feature merge states that count as integrated. Any other value, including a
# missing one, means a child has not landed on the integration branch.
$script:TerminalMergeStatuses = @('merged', 'worktree_removed')

# An epic manifest lives under the epics tree; the pattern is tested after backslash
# normalisation so either separator spelling is accepted.
$script:EpicManifestPattern = '(^|/)docs/features/epics/'

function Get-EpicReadinessPropertyValue {
    # Pure: the named property of an object, or $null when the object or the property
    # is absent. StrictMode forbids reading a missing property directly.
    [OutputType([object])]
    param([AllowNull()][object] $InputObject, [string] $Name)

    if ($null -eq $InputObject) {
        return $null
    }
    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    return $property.Value
}

function Get-EpicReadinessFeatureList {
    # Pure: the features array as a list, empty when it is absent or empty.
    [OutputType([object[]])]
    param([AllowNull()][object] $Checkpoint)

    $features = Get-EpicReadinessPropertyValue -InputObject $Checkpoint -Name 'features'
    if ($null -eq $features) {
        return , @()
    }
    return , @($features)
}

function Get-EpicPrCreationReadinessFailure {
    <#
    .SYNOPSIS
        Name the first failed conjunct of the epic integration-PR readiness shape, or ''.
    .DESCRIPTION
        Conjuncts, in order: checkpoint-absent (null checkpoint); route_id (not exactly
        epic); integration_branch (empty, or different from a non-empty HeadBranch,
        case-sensitive); features (absent or empty); merge_status (any feature whose
        merge_status is not merged or worktree_removed).
    .PARAMETER Checkpoint
        The parsed epic checkpoint object, or $null.
    .PARAMETER HeadBranch
        The --head branch of the pull request; not compared when empty.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Checkpoint,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $HeadBranch = ''
    )

    if ($null -eq $Checkpoint) {
        return 'checkpoint-absent'
    }
    if ([string](Get-EpicReadinessPropertyValue -InputObject $Checkpoint -Name 'route_id') -cne $script:EpicRouteId) {
        return 'route_id'
    }
    $integrationBranch = [string](Get-EpicReadinessPropertyValue -InputObject $Checkpoint -Name 'integration_branch')
    # Decide on the integration branch: it must be present, and when the caller names a
    # head branch the two must be identical.
    if ([string]::IsNullOrWhiteSpace($integrationBranch)) {
        return 'integration_branch'
    } elseif (-not [string]::IsNullOrWhiteSpace($HeadBranch) -and ($integrationBranch -cne $HeadBranch)) {
        return 'integration_branch'
    }
    $features = Get-EpicReadinessFeatureList -Checkpoint $Checkpoint
    if ($features.Count -eq 0) {
        return 'features'
    }
    # Every child feature must have landed; the first non-terminal status fails the set.
    foreach ($feature in $features) {
        $status = [string](Get-EpicReadinessPropertyValue -InputObject $feature -Name 'merge_status')
        if ($script:TerminalMergeStatuses -notcontains $status) {
            return 'merge_status'
        }
    }
    return ''
}

function Get-EpicCommandLegReadinessFailure {
    <#
    .SYNOPSIS
        Name the first failed conjunct of the epic command-leg readiness shape, or ''.
    .DESCRIPTION
        Conjuncts, in order: checkpoint-absent; route_id; epic_feature_folder (empty);
        epic_manifest_path (empty, or not under docs/features/epics/ after backslash
        normalisation); integration_branch (empty); features (absent or empty);
        merge-in-progress (no merge in progress in the effective worktree, D2).
    .PARAMETER Checkpoint
        The parsed epic checkpoint object, or $null.
    .PARAMETER MergeInProgress
        Whether MERGE_HEAD exists in the effective worktree's git directory.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Checkpoint,

        [Parameter(Mandatory = $true)]
        [bool] $MergeInProgress
    )

    if ($null -eq $Checkpoint) {
        return 'checkpoint-absent'
    }
    if ([string](Get-EpicReadinessPropertyValue -InputObject $Checkpoint -Name 'route_id') -cne $script:EpicRouteId) {
        return 'route_id'
    }
    if ([string]::IsNullOrWhiteSpace([string](Get-EpicReadinessPropertyValue -InputObject $Checkpoint -Name 'epic_feature_folder'))) {
        return 'epic_feature_folder'
    }
    $manifestPath = ([string](Get-EpicReadinessPropertyValue -InputObject $Checkpoint -Name 'epic_manifest_path')) -replace '\\', '/'
    if ([string]::IsNullOrWhiteSpace($manifestPath) -or $manifestPath -notmatch $script:EpicManifestPattern) {
        return 'epic_manifest_path'
    }
    if ([string]::IsNullOrWhiteSpace([string](Get-EpicReadinessPropertyValue -InputObject $Checkpoint -Name 'integration_branch'))) {
        return 'integration_branch'
    }
    if ((Get-EpicReadinessFeatureList -Checkpoint $Checkpoint).Count -eq 0) {
        return 'features'
    }
    if (-not $MergeInProgress) {
        return 'merge-in-progress'
    }
    return ''
}

Export-ModuleMember -Function Get-EpicPrCreationReadinessFailure, Get-EpicCommandLegReadinessFailure
