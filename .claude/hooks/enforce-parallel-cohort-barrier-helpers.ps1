<#
.SYNOPSIS
    Dot-sourced record-resolution and barrier helpers for enforce-parallel-cohort-barrier.ps1.

.DESCRIPTION
    Holds the five resolution helpers the Layer 1 parallel cohort barrier uses once a
    delegation has been identified as a parallel-mode orchestrator call:

      - Find-ParallelCohortBarrierItemRecord: resolves the items[] record for a folder.
      - Find-ParallelCohortBarrierItemByKey: resolves an items[] record by issue_num.
      - Find-ParallelCohortBarrierCohortIndex: projects the current-generation coloring.
      - Get-ParallelCohortBarrierConflictNeighborList: collects conflict-edge neighbours.
      - Test-ParallelCohortBarrierClear: applies the per-edge barrier rule.

    They are split out of the parent hook, .claude/hooks/enforce-parallel-cohort-barrier.ps1,
    because that file stood at 499 lines -- one below the 500-line limit -- with no room
    for the shared payload-reader migration (issue #501). The split is a pure move with no
    behaviour change; the precedent is .claude/hooks/enforce-parallel-drift-gate-helpers.ps1.

    The helpers read the script-scoped configuration the parent hook assigns
    ($script:AllowedMergeStatuses), so this file is only ever dot-sourced from that hook
    and never invoked on its own.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies. Every function is
    pure: no filesystem, subprocess, network, or wall-clock access.
#>
[CmdletBinding()]
param()

function Find-ParallelCohortBarrierItemRecord {
    <#
    .SYNOPSIS
        Locate the items[] record whose feature_folder resolves to the target basename.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER FeatureFolder
        The target feature_folder basename resolved from the prompt.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $FeatureFolder
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($FeatureFolder)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'items') {
        return $null
    }

    # Scan every recorded item for a feature_folder whose basename equals the target.
    foreach ($item in @($Checkpoint.items)) {
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'feature_folder') {
            continue
        }
        $itemBasename = Get-ParallelCohortBarrierFolderBasename -FolderPath ([string]$item.feature_folder)
        if ($itemBasename -eq $FeatureFolder) {
            return $item
        }
    }
    return $null
}

function Find-ParallelCohortBarrierItemByKey {
    <#
    .SYNOPSIS
        Locate the items[] record whose issue_num equals the supplied key.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER IssueKey
        The item primary key (issue_num) rendered as a string.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $IssueKey
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($IssueKey)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'items') {
        return $null
    }

    foreach ($item in @($Checkpoint.items)) {
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'issue_num') {
            continue
        }
        if (([string]$item.issue_num) -eq $IssueKey) {
            return $item
        }
    }
    return $null
}

function Find-ParallelCohortBarrierCohortIndex {
    <#
    .SYNOPSIS
        Return the current-generation cohort index that holds the supplied item key.
    .DESCRIPTION
        The cohort projection is restricted to cohorts[] rows whose generation equals the
        top-level recolor_generation, which is the current coloring. Rows from a superseded
        generation are ignored. Returns $null when the checkpoint records no
        recolor_generation, no cohorts, or no current-generation cohort containing the key.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER IssueKey
        The item primary key (issue_num) rendered as a string.
    .OUTPUTS
        System.Int32 or $null
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $IssueKey
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($IssueKey)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'cohorts' -or $checkpointProps -notcontains 'recolor_generation') {
        return $null
    }

    $currentGeneration = [string]$Checkpoint.recolor_generation

    foreach ($cohort in @($Checkpoint.cohorts)) {
        $cohortProps = @($cohort.PSObject.Properties.Name)
        if ($cohortProps -notcontains 'index' -or $cohortProps -notcontains 'generation' -or $cohortProps -notcontains 'item_keys') {
            continue
        }
        if (([string]$cohort.generation) -ne $currentGeneration) {
            continue
        }
        $parsedIndex = 0
        if (-not [int]::TryParse(([string]$cohort.index), [ref] $parsedIndex)) {
            continue
        }
        foreach ($key in @($cohort.item_keys)) {
            if (([string]$key) -eq $IssueKey) {
                return $parsedIndex
            }
        }
    }
    return $null
}

function Get-ParallelCohortBarrierConflictNeighborList {
    <#
    .SYNOPSIS
        Return the item keys that share a conflict edge with the supplied item key.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER IssueKey
        The item primary key (issue_num) rendered as a string.
    .OUTPUTS
        System.Object[] holding each neighbor item key rendered as a string.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $IssueKey
    )

    $neighborList = @()
    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($IssueKey)) {
        return $neighborList
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'conflict_edges') {
        return $neighborList
    }

    # Edges are undirected and canonically normalized to a < b, so both endpoints are
    # inspected and the opposite endpoint is returned as the neighbor.
    foreach ($edge in @($Checkpoint.conflict_edges)) {
        $edgeProps = @($edge.PSObject.Properties.Name)
        if ($edgeProps -notcontains 'a' -or $edgeProps -notcontains 'b') {
            continue
        }
        $endpointA = [string]$edge.a
        $endpointB = [string]$edge.b
        if ($endpointA -eq $IssueKey) {
            $neighborList += $endpointB
        } elseif ($endpointB -eq $IssueKey) {
            $neighborList += $endpointA
        }
    }
    return $neighborList
}

function Test-ParallelCohortBarrierClear {
    <#
    .SYNOPSIS
        Decision logic: true only when every conflicting neighbor in a strictly prior
        current-generation cohort has merge_status merged or worktree_removed.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER ItemRecord
        The target item's own items[] record, or $null when not found.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        $ItemRecord
    )

    if ($null -eq $Checkpoint -or $null -eq $ItemRecord) {
        return $false
    }
    $itemProps = @($ItemRecord.PSObject.Properties.Name)
    if ($itemProps -notcontains 'issue_num') {
        return $false
    }

    $targetKey = [string]$ItemRecord.issue_num
    $targetIndex = Find-ParallelCohortBarrierCohortIndex -Checkpoint $Checkpoint -IssueKey $targetKey
    if ($null -eq $targetIndex) {
        # No current-generation cohort assignment: position is unknown, so fail closed.
        return $false
    }

    foreach ($neighborKey in @(Get-ParallelCohortBarrierConflictNeighborList -Checkpoint $Checkpoint -IssueKey $targetKey)) {
        $neighborIndex = Find-ParallelCohortBarrierCohortIndex -Checkpoint $Checkpoint -IssueKey $neighborKey
        if ($null -eq $neighborIndex) {
            # Not part of the current coloring, so not a strictly prior cohort neighbor.
            continue
        }
        if ($neighborIndex -ge $targetIndex) {
            # Same-cohort and later-cohort contention is not a Layer 1 concern.
            continue
        }
        $neighborRecord = Find-ParallelCohortBarrierItemByKey -Checkpoint $Checkpoint -IssueKey $neighborKey
        if ($null -eq $neighborRecord) {
            return $false
        }
        $neighborProps = @($neighborRecord.PSObject.Properties.Name)
        if ($neighborProps -notcontains 'merge_status') {
            return $false
        }
        if ($script:AllowedMergeStatuses -notcontains ([string]$neighborRecord.merge_status)) {
            return $false
        }
    }
    return $true
}
