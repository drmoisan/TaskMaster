<#
.SYNOPSIS
    Dot-sourced by enforce-pr-author-skill.ps1 to keep that file under the repository's
    500-line hard cap.

.DESCRIPTION
    Contains the epic-mode base-branch override check (Get-PrAuthorCheckpointContent and
    Test-EpicBaseBranchOverride) extracted verbatim from enforce-pr-author-skill.ps1. This is a
    structural extraction only: no logic, decision behavior, or reason-string wording changed.
#>

# Shared command-line parser (issue #545). Dot-sourced here as well as from the parent hook
# so this file keeps working when a test dot-sources it directly.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

function Get-PrAuthorCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the per-feature orchestrator checkpoint. Tests mock this
        function (read seam) for the epic-mode base-branch override check.
    .DESCRIPTION
        Returns the raw text content of the checkpoint at the supplied path, or $null when the
        file is absent. This is the injectable boundary for checkpoint content used by
        Test-EpicBaseBranchOverride; no test writes the checkpoint file to disk.

        The path is mandatory and carries no default (issue #673). A relative default was the
        second of this feature's three bindings: it made check 6 read whichever checkpoint
        occupied the process directory even when target resolution had already selected a
        different worktree, so the epic verdict could come from an unrelated item.
    .PARAMETER CheckpointPath
        The absolute path of the checkpoint belonging to the resolved worktree.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CheckpointPath
    )

    if (-not (Test-Path -LiteralPath $CheckpointPath)) {
        return $null
    }

    return (Get-Content -LiteralPath $CheckpointPath -Raw)
}

function Test-EpicBaseBranchOverride {
    <#
    .SYNOPSIS
        Sixth ordered check: enforce the epic-mode --base override for gh pr create.
    .DESCRIPTION
        Reads the per-feature checkpoint via the injectable Get-PrAuthorCheckpointContent
        seam. When the checkpoint has epic_mode == true, a gh pr create command text MUST
        contain --base <epic_context.integration_branch> with the exact branch value recorded
        in the checkpoint; a missing --base, or a --base value that does not match, is denied
        with reason EPIC_BASE_BRANCH_MISMATCH. When epic_mode is absent or false, when the
        checkpoint is unreadable, or when the command is not gh pr create (the base branch is
        set at create time, not edit time), this check is a no-op and standalone behavior is
        unchanged.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .PARAMETER CheckpointPath
        The absolute checkpoint path identity resolution selected, passed straight through to
        the read seam so this check and the orchestrator-state preflight read one file.
    .PARAMETER EpicScope
        Optional. The epic-scope result gate 1 resolved (issue #663). When it reports epic
        scope, the integration pull request must pass --base main and the per-feature
        checkpoint seam is not read; $null keeps the per-feature epic_mode rule unchanged.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText,

        [Parameter(Mandatory)]
        [string] $CheckpointPath,

        [AllowNull()]
        [object] $EpicScope
    )

    # The epic-mode base-branch override only constrains gh pr create; gh pr edit does not
    # re-target the base branch, so non-create commands are out of scope for this check.
    # The test is structural, so a quoted mention of the phrase is out of scope and a
    # relocating spelling carrying a gh global option is in scope (issue #545).
    if (-not (Test-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'create'))) {
        return $null
    }

    # Epic scope (issue #663): the epic's own integration pull request targets main. The
    # scope object comes from gate 1, so the epic checkpoint is not read a second time and
    # the per-feature checkpoint is not consulted.
    if ($null -ne $EpicScope -and $EpicScope.IsEpicScope) {
        $epicBaseValue = Get-CommandLineFlagValue -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'create') -FlagName '--base'
        if ($null -eq $epicBaseValue -or $epicBaseValue -cne 'main') {
            return "EPIC_BASE_BRANCH_MISMATCH: this epic-scope gh pr create was evaluated against $($EpicScope.CheckpointPath); the integration pull request must pass --base main."
        }
        return $null
    }

    $checkpointRaw = Get-PrAuthorCheckpointContent -CheckpointPath $CheckpointPath
    if ([string]::IsNullOrWhiteSpace($checkpointRaw)) {
        return $null
    }

    try {
        $checkpoint = $checkpointRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return $null
    }

    $checkpointProps = @($checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'epic_mode' -or -not [bool]$checkpoint.epic_mode) {
        return $null
    }

    $integrationBranch = $null
    if ($checkpointProps -contains 'epic_context' -and $null -ne $checkpoint.epic_context) {
        $epicContextProps = @($checkpoint.epic_context.PSObject.Properties.Name)
        if ($epicContextProps -contains 'integration_branch') {
            $integrationBranch = [string]$checkpoint.epic_context.integration_branch
        }
    }

    if ([string]::IsNullOrWhiteSpace($integrationBranch)) {
        return "EPIC_BASE_BRANCH_MISMATCH: checkpoint has epic_mode == true but no ``epic_context.integration_branch`` is recorded; ``gh pr create`` cannot be verified against the required ``--base`` value."
    }

    # The --base value is taken from the matched segment's tokens, so a chained segment that
    # happens to carry a --base token contributes nothing here. The comparison stays
    # case-sensitive, as the previous -cnotmatch form was, because a branch name is.
    $baseValue = Get-CommandLineFlagValue -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'create') -FlagName '--base'
    if ($null -eq $baseValue -or $baseValue -cne $integrationBranch) {
        return "EPIC_BASE_BRANCH_MISMATCH: ``gh pr create`` must pass ``--base $integrationBranch`` (``epic_context.integration_branch``) under ``epic_mode``; the command does not carry a matching ``--base`` argument."
    }

    return $null
}
