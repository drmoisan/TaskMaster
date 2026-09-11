<#
.SYNOPSIS
    Sanctioned-removal manifest reader for the two worktree-removal gate hooks
    (issue #635).

.DESCRIPTION
    The cleanup-merged-worktrees skill records each per-worktree removal it has
    triaged, together with the verdict and the evidence behind that verdict, in a
    manifest document. This module owns reading that document and deciding whether
    a given removal target is authorized by it. Both PreToolUse gate hooks consume
    the decision; neither re-implements the parse.

    Vocabulary. Two script-scope constants carry the narrow allow-sets the
    predicate is written against, following the $script:AllowedMergeStatuses
    precedent the two gate hooks already use. The removal-disposition set holds
    exactly SAFE_TO_DELETE. The authorized-branch-state set holds exactly
    NOT_MERGED and HAS_UNIQUE_RESIDUALS, which are the two states the skill
    permanently forbids adding to the cleanup script's apply-mode allowlist and are
    therefore the durable residual this manifest exists to serve.

    Fail-closed posture. Every malformation resolves to a null parsed object or a
    false predicate result. The module raises nothing on a malformed manifest, so a
    hook consuming it always reaches its own unchanged deny path rather than
    throwing.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies, no subprocess,
    and no network. The single filesystem read and the single wall-clock read each
    sit behind an injectable seam so tests drive them without writing a temporary
    file. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Repo-relative location of the sanctioned-removal manifest. Kept as a script-scope
# constant so the read seam is the only function that names the path.
$script:CleanupWorktreeManifestPath = 'artifacts/orchestration/cleanup-worktrees-manifest.json'

# The only removal disposition that authorizes anything. Deliberately a
# single-member set rather than a bare string comparison, so a widening of the set
# is a visible edit to a named constant that a test pins.
$script:AllowedRemovalDispositions = @('SAFE_TO_DELETE')

# The branch states a manifest record may authorize a removal for. PROTECTED_CURRENT
# is never authorized, and the three merged states are excluded because the cleanup
# script's deterministic path owns them.
$script:AuthorizedBranchStates = @('NOT_MERGED', 'HAS_UNIQUE_RESIDUALS')

# The step-5 verdicts that may authorize a removal. Derived from the skill's
# five-member verdict vocabulary -- DEAD_ONE_OFF, ALREADY_SOLVED_ELSEWHERE,
# STALE_OR_CONTRADICTED, GENUINELY_NEW, STILL_RELEVANT -- with the two
# preserve-implying members removed, because the skill classifies both as content
# that must be preserved before the worktree is deleted. Recorded as the authorized
# subset rather than as the vocabulary plus an exclusion list, so a verdict added to
# the vocabulary later is not authorized by silence.
$script:AuthorizedRemovalVerdicts = @('DEAD_ONE_OFF', 'ALREADY_SOLVED_ELSEWHERE', 'STALE_OR_CONTRADICTED')

# The self-identifying discriminator and the contract version this module reads.
$script:ExpectedManifestTool = 'cleanup-merged-worktrees'
$script:ExpectedManifestSchemaVersion = 1

# Freshness bound. artifacts/ is gitignored, so a stale manifest persists after its
# run ends, and cleanup targets are ordinary long-lived worktree paths rather than
# session-stamped ones, which makes a stale collision materially re-matchable.
$script:ManifestFreshnessBound = [timespan]::FromHours(24)

function Get-CleanupWorktreeManifestContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the sanctioned-removal manifest. Tests mock this
        function (read seam).
    .DESCRIPTION
        The only filesystem read in this module. An absent manifest is an ordinary
        state rather than an error, so the function returns $null instead of
        throwing and the caller resolves it to a non-authorizing parse.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:CleanupWorktreeManifestPath -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $script:CleanupWorktreeManifestPath -Raw)
}

function Get-CleanupWorktreeManifestUtcNow {
    <#
    .SYNOPSIS
        Read the current UTC time. Tests mock this function (clock seam).
    .DESCRIPTION
        The only wall-clock read in this module. Freshness evaluation calls it once
        per predicate evaluation, so a mocked value governs the whole comparison and
        no test depends on real elapsed time.
    .OUTPUTS
        System.DateTime
    #>
    [CmdletBinding()]
    [OutputType([datetime])]
    param()

    return [datetime]::UtcNow
}

function ConvertTo-CleanupWorktreeManifestNormalizedPath {
    <#
    .SYNOPSIS
        Normalize a worktree path so both sides of a comparison share one spelling.
    .DESCRIPTION
        Replaces backslashes with forward slashes, then trims a single trailing
        slash. Applied to the command target and to every recorded worktree_path,
        so a trailing-slash target, a Windows-separator target, and a recorded
        POSIX path all compare equal.
    .PARAMETER Path
        The raw path to normalize, or $null.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ''
    }
    return ($Path -replace '\\', '/').TrimEnd('/')
}

function Find-CleanupWorktreeManifestRemovalRecord {
    <#
    .SYNOPSIS
        Locate the removals[] record whose worktree_path matches the removal target.
    .DESCRIPTION
        Parses the manifest text behind a fail-closed try/catch: unparseable text
        yields $null rather than a throw, matching the gate hooks' conventions.
        Scanning stops at the FIRST normalized path match, so two records sharing a
        path resolve on the first, following the parallel-branch precedent in the
        epic gate. A record with no worktree_path key is skipped and the scan
        continues.
    .PARAMETER Raw
        Raw manifest text, or $null when the manifest does not exist.
    .PARAMETER WorktreePath
        The removal target extracted from the command text.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Raw,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $WorktreePath
    )

    if ([string]::IsNullOrWhiteSpace($Raw) -or [string]::IsNullOrWhiteSpace($WorktreePath)) {
        return $null
    }

    try {
        $manifest = $Raw | ConvertFrom-Json
    } catch {
        $manifest = $null
    }
    if ($null -eq $manifest) {
        return $null
    }

    $manifestProperties = @($manifest.PSObject.Properties.Name)
    if ($manifestProperties -notcontains 'removals') {
        return $null
    }

    $normalizedTarget = ConvertTo-CleanupWorktreeManifestNormalizedPath -Path $WorktreePath

    foreach ($record in @($manifest.removals)) {
        if ($null -eq $record) {
            continue
        }
        $recordProperties = @($record.PSObject.Properties.Name)
        if ($recordProperties -notcontains 'worktree_path') {
            continue
        }
        $normalizedRecordPath = ConvertTo-CleanupWorktreeManifestNormalizedPath -Path ([string]$record.worktree_path)
        if ($normalizedRecordPath -eq $normalizedTarget) {
            return $record
        }
    }
    return $null
}

function Test-CleanupWorktreeManifestAuthorizesRemoval {
    <#
    .SYNOPSIS
        Decide whether the sanctioned-removal manifest authorizes removing a
        worktree.
    .DESCRIPTION
        Evaluates conditions 1 through 9 of the specification's allow predicate in
        order: parse, tool and schema version, freshness against the injected
        clock, removals present and non-empty, first normalized path match, removal
        disposition, evidence, verdict, and branch state. Conditions 1 through 4
        are evaluated before the path scan so an absent or malformed manifest costs
        one existence test and returns to the caller's deny path.

        The function returns a boolean and raises nothing: every malformation
        resolves to $false. It never reads the preserved_files array, which belongs
        to a different consumer and must not influence any gate decision.

        Condition 10, checkpoint exclusion, is NOT evaluated here. It is the
        caller's obligation, because the two gate hooks own the checkpoint seams.
    .PARAMETER WorktreePath
        The removal target extracted from the command text.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $WorktreePath
    )

    if ([string]::IsNullOrWhiteSpace($WorktreePath)) {
        return $false
    }

    # Condition 1 -- the manifest exists, is readable, and parses as JSON.
    $raw = Get-CleanupWorktreeManifestContent
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return $false
    }
    try {
        $manifest = $raw | ConvertFrom-Json
    } catch {
        $manifest = $null
    }
    if ($null -eq $manifest) {
        return $false
    }
    $manifestProperties = @($manifest.PSObject.Properties.Name)

    # Condition 2 -- the self-identifying discriminator and the contract version.
    if ($manifestProperties -notcontains 'tool' -or $manifestProperties -notcontains 'schema_version') {
        return $false
    }
    if ($manifest.tool -isnot [string] -or $manifest.tool -cne $script:ExpectedManifestTool) {
        return $false
    }
    $schemaVersion = $manifest.schema_version
    if ($schemaVersion -isnot [int] -and $schemaVersion -isnot [long]) {
        return $false
    }
    if ([long]$schemaVersion -ne [long]$script:ExpectedManifestSchemaVersion) {
        return $false
    }

    # Condition 3 -- freshness, measured against the injected clock only.
    if ($manifestProperties -notcontains 'generated_at') {
        return $false
    }
    $generatedAt = [datetime]::MinValue
    $parseStyles = [System.Globalization.DateTimeStyles]::AdjustToUniversal -bor [System.Globalization.DateTimeStyles]::AssumeUniversal
    $isTimestampParsed = [datetime]::TryParse(
        [string]$manifest.generated_at,
        [cultureinfo]::InvariantCulture,
        $parseStyles,
        [ref] $generatedAt)
    if (-not $isTimestampParsed) {
        return $false
    }
    $utcNow = Get-CleanupWorktreeManifestUtcNow
    if ($generatedAt -gt $utcNow) {
        return $false
    }
    if (($utcNow - $generatedAt) -gt $script:ManifestFreshnessBound) {
        return $false
    }

    # Condition 4 -- removals is present, is an array, and is non-empty.
    if ($manifestProperties -notcontains 'removals') {
        return $false
    }
    $removals = $manifest.removals
    if ($null -eq $removals -or $removals -isnot [System.Collections.IList]) {
        return $false
    }
    if (@($removals).Count -lt 1) {
        return $false
    }

    # Condition 5 -- the first record whose normalized path matches the target.
    $record = Find-CleanupWorktreeManifestRemovalRecord -Raw $raw -WorktreePath $WorktreePath
    if ($null -eq $record) {
        return $false
    }
    $recordProperties = @($record.PSObject.Properties.Name)

    # Condition 6 -- the removal disposition is in the single-member allowed set.
    if ($recordProperties -notcontains 'removal_disposition') {
        return $false
    }
    if ($script:AllowedRemovalDispositions -cnotcontains ([string]$record.removal_disposition)) {
        return $false
    }

    # Condition 7 -- the evidence justification is a present, non-empty string.
    if ($recordProperties -notcontains 'evidence') {
        return $false
    }
    if ($record.evidence -isnot [string] -or [string]::IsNullOrWhiteSpace($record.evidence)) {
        return $false
    }

    # Condition 8 -- the verdict is one the skill does not classify as preserve.
    if ($recordProperties -notcontains 'verdict') {
        return $false
    }
    if ($script:AuthorizedRemovalVerdicts -cnotcontains ([string]$record.verdict)) {
        return $false
    }

    # Condition 9 -- the branch state is one of the two durable-residual states.
    if ($recordProperties -notcontains 'branch_state') {
        return $false
    }
    if ($script:AuthorizedBranchStates -cnotcontains ([string]$record.branch_state)) {
        return $false
    }

    return $true
}

function Test-CleanupManifestCheckpointCoversPath {
    <#
    .SYNOPSIS
        Report whether an orchestration checkpoint records the removal target at
        all.
    .DESCRIPTION
        Condition 10 of the specification's allow predicate. This is a PRESENCE
        test, deliberately not an authorization test: a record matching the target
        makes the manifest branch inapplicable regardless of that record's
        merge_status, so a removal the checkpoint does not authorize still reaches
        the gate's existing deny. Reusing the gates' merge_status predicate here
        would reopen the one path by which the manifest could widen what the gates
        protect.
    .PARAMETER Checkpoint
        Parsed checkpoint object, or $null when absent or unreadable.
    .PARAMETER RecordArrayName
        Name of the record array to scan: features for the epic checkpoint, items
        for the parallel checkpoint.
    .PARAMETER WorktreePath
        The removal target extracted from the command text.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string] $RecordArrayName,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $WorktreePath
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($WorktreePath)) {
        return $false
    }
    $checkpointProperties = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProperties -notcontains $RecordArrayName) {
        return $false
    }

    $normalizedTarget = ConvertTo-CleanupWorktreeManifestNormalizedPath -Path $WorktreePath

    foreach ($record in @($Checkpoint.PSObject.Properties[$RecordArrayName].Value)) {
        if ($null -eq $record) {
            continue
        }
        $recordProperties = @($record.PSObject.Properties.Name)
        if ($recordProperties -notcontains 'worktree_path') {
            continue
        }
        $normalizedRecordPath = ConvertTo-CleanupWorktreeManifestNormalizedPath -Path ([string]$record.worktree_path)
        if ($normalizedRecordPath -eq $normalizedTarget) {
            return $true
        }
    }
    return $false
}

Export-ModuleMember -Function `
    Get-CleanupWorktreeManifestContent, `
    Get-CleanupWorktreeManifestUtcNow, `
    ConvertTo-CleanupWorktreeManifestNormalizedPath, `
    Find-CleanupWorktreeManifestRemovalRecord, `
    Test-CleanupWorktreeManifestAuthorizesRemoval, `
    Test-CleanupManifestCheckpointCoversPath
