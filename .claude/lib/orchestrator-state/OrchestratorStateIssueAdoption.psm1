<#
.SYNOPSIS
    Issue-adoption record validation and receipt waivers (inventory row C6.15).

.DESCRIPTION
    Destination-runtime PowerShell port of `resolve_issue_adoption` in
    `scripts/dev_tools/_orchestrator_state_issue_adoption.py`. The optional
    checkpoint object `issue_adoption` records that the orchestration adopted a
    GitHub issue that existed before orchestration started. A valid record
    waives the successful-receipt requirement for `potential_to_issue` and,
    optionally, for the checkpoint's promotion-entry tool.

    Invariants:
      - When the `issue_adoption` member is absent, no error is produced and no
        tool is waived, so existing checkpoints validate exactly as before.
      - Errors accumulate in the fixed rule order of the feature specification.
      - Fail-closed: WaivedTools is empty whenever Errors is non-empty.
      - Every string comparison is ordinal and case-sensitive.
      - Messages interpolate only non-blank `waived_tools` entries and the route id.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the shared checkpoint-value primitives, resolved relative to this
# module's directory so the import travels with the pushed-down pack.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force -ErrorAction Stop

$script:ISSUE_ADOPTION_KEY = 'issue_adoption'
$script:POTENTIAL_TO_ISSUE_TOOL = 'potential_to_issue'
$script:FEATURE_PROMOTION_ENTRY_TOOL = 'new_potential_entry'
$script:BUG_PROMOTION_ENTRY_TOOL = 'new_potential_bug_entry'
$script:ORIGIN_VALUES = [string[]]@('transferred', 'filed_before_orchestration', 'epic_decomposition')
$script:VERIFIED_VIA_VALUES = [string[]]@('gh_issue_view', 'gh_api_get', 'github_mcp_issue_read')

# The closed set of tools an adoption record may waive: the issue-creation tool
# and the two promotion-type-specific promotion-entry tools.
$script:WAIVABLE_TOOLS = [string[]]@($script:POTENTIAL_TO_ISSUE_TOOL, $script:FEATURE_PROMOTION_ENTRY_TOOL, $script:BUG_PROMOTION_ENTRY_TOOL)
$script:PROMOTION_ENTRY_TOOLS = [string[]]@($script:FEATURE_PROMOTION_ENTRY_TOOL, $script:BUG_PROMOTION_ENTRY_TOOL)
$script:POTENTIAL_RECORD_PREFIX = 'docs/features/potential/'
$script:POTENTIAL_RECORD_SUFFIX = '.md'
# Origins whose promotion-entry waiver may omit the potential_record key (rule 9).
$script:RECORD_OPTIONAL_ORIGINS = [string[]]@('transferred', 'filed_before_orchestration')
$script:ISSUE_NUM_PATTERN = '\A[1-9][0-9]*\z'

$script:ERROR_NOT_OBJECT = 'Checkpoint issue_adoption must be an object when present.'
$script:ERROR_ISSUE_NUM_FORMAT = 'Checkpoint issue_adoption.issue_num must be a string of decimal digits without a leading zero.'
$script:ERROR_ISSUE_NUM_MISMATCH = 'Checkpoint issue_adoption.issue_num must equal the checkpoint issue-num.'
$script:ERROR_ISSUE_URL = 'Checkpoint issue_adoption.issue_url must end with /issues/ followed by issue_num.'
$script:ERROR_ORIGIN = 'Checkpoint issue_adoption.origin must be one of transferred, filed_before_orchestration, epic_decomposition.'
$script:ERROR_VERIFIED_VIA = 'Checkpoint issue_adoption.verified_via must be one of gh_issue_view, gh_api_get, github_mcp_issue_read.'
$script:ERROR_VERIFIED_AT = 'Checkpoint issue_adoption.verified_at must be present.'
$script:ERROR_EVIDENCE = 'Checkpoint issue_adoption.evidence must be a non-empty string.'
$script:ERROR_WAIVED_TOOLS_SHAPE = 'Checkpoint issue_adoption.waived_tools must be a non-empty list of tool names.'
$script:ERROR_WAIVED_TOOLS_INCLUDE = 'Checkpoint issue_adoption.waived_tools must include potential_to_issue.'


function Test-AdoptionNonBlankString {
    <#
    .SYNOPSIS
        Report whether a value is a string with non-whitespace content.
    .PARAMETER Value
        The deserialized JSON value to inspect. May be $null.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    return (($Value -is [string]) -and -not [string]::IsNullOrWhiteSpace([string]$Value))
}

function Get-AdoptionIssueIdentityError {
    <#
    .SYNOPSIS
        Validate issue_num and issue_url (rules 2 and 3).
    .PARAMETER Adoption
        The issue_adoption object.
    .PARAMETER State
        The parsed checkpoint object, read for its hyphenated issue-num key.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $Adoption,

        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $rawIssueNum = (Get-CheckpointObjectMember -Owner $Adoption -Name 'issue_num').Value
    $issueNum = $null
    if (($rawIssueNum -is [string]) -and ([string]$rawIssueNum -cmatch $script:ISSUE_NUM_PATTERN)) {
        $issueNum = [string]$rawIssueNum
    }

    if ($null -ceq $issueNum) {
        $errors.Add($script:ERROR_ISSUE_NUM_FORMAT)
    }
    else {
        # The equality check applies only when the checkpoint records its own
        # issue number as a string; other shapes are validated elsewhere.
        $checkpointIssueNum = (Get-CheckpointObjectMember -Owner $State -Name 'issue-num').Value
        if (($checkpointIssueNum -is [string]) -and -not [string]::Equals([string]$checkpointIssueNum, $issueNum, [System.StringComparison]::Ordinal)) {
            $errors.Add($script:ERROR_ISSUE_NUM_MISMATCH)
        }
    }

    $issueUrl = (Get-CheckpointObjectMember -Owner $Adoption -Name 'issue_url').Value
    $urlIsValid = ($null -cne $issueNum) -and ($issueUrl -is [string]) -and ([string]$issueUrl).EndsWith("/issues/$issueNum", [System.StringComparison]::Ordinal)
    if (-not $urlIsValid) {
        $errors.Add($script:ERROR_ISSUE_URL)
    }
    return $errors.ToArray()
}

function Get-AdoptionProvenanceError {
    <#
    .SYNOPSIS
        Validate origin, verified_via, verified_at, and evidence (rules 4 to 7).
    .PARAMETER Adoption
        The issue_adoption object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $Adoption
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    $origin = (Get-CheckpointObjectMember -Owner $Adoption -Name 'origin').Value
    if (-not (($origin -is [string]) -and ($script:ORIGIN_VALUES -ccontains [string]$origin))) {
        $errors.Add($script:ERROR_ORIGIN)
    }

    $verifiedVia = (Get-CheckpointObjectMember -Owner $Adoption -Name 'verified_via').Value
    if (-not (($verifiedVia -is [string]) -and ($script:VERIFIED_VIA_VALUES -ccontains [string]$verifiedVia))) {
        $errors.Add($script:ERROR_VERIFIED_VIA)
    }

    # Presence-only: any non-null value other than a blank string passes,
    # including a System.DateTime produced by ConvertFrom-Json.
    $verifiedAt = (Get-CheckpointObjectMember -Owner $Adoption -Name 'verified_at').Value
    if (($null -ceq $verifiedAt) -or (($verifiedAt -is [string]) -and [string]::IsNullOrWhiteSpace([string]$verifiedAt))) {
        $errors.Add($script:ERROR_VERIFIED_AT)
    }

    if (-not (Test-AdoptionNonBlankString -Value (Get-CheckpointObjectMember -Owner $Adoption -Name 'evidence').Value)) {
        $errors.Add($script:ERROR_EVIDENCE)
    }
    return $errors.ToArray()
}

function Get-AdoptionWaivedToolList {
    <#
    .SYNOPSIS
        Return the waived tools when they form a non-empty list of non-blank strings.
    .PARAMETER Value
        The deserialized waived_tools value. May be $null.
    .OUTPUTS
        System.Collections.Hashtable with keys Ok (bool) and Value (string[]).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    if (-not (Test-CheckpointListValue -Value $Value)) {
        return @{ Ok = $false; Value = [string[]]@() }
    }
    $items = @($Value)
    if ($items.Count -ceq 0) {
        return @{ Ok = $false; Value = [string[]]@() }
    }
    foreach ($item in $items) {
        if (-not (Test-AdoptionNonBlankString -Value $item)) {
            return @{ Ok = $false; Value = [string[]]@() }
        }
    }
    return @{ Ok = $true; Value = [string[]]$items }
}

function Get-AdoptionWaivedToolError {
    <#
    .SYNOPSIS
        Validate each waived tool entry and the mandatory issue-creation waiver (rule 8).
    .PARAMETER WaivedTool
        The well-formed waived tool list, in checkpoint order.
    .PARAMETER RouteId
        The selected route id, interpolated into route errors.
    .PARAMETER RequiredMcpTool
        The route's required MCP tools after promotion-type resolution.
    .PARAMETER SuccessfulTool
        Tools that hold a successful MCP receipt.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $WaivedTool,

        [Parameter(Mandatory = $true)]
        [string] $RouteId,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $RequiredMcpTool,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $SuccessfulTool
    )

    $errors = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($tool in $WaivedTool) {
        # Each entry receives only the first rule it violates, in rule order.
        if ($seen.Contains($tool)) {
            $errors.Add("Checkpoint issue_adoption.waived_tools lists a tool more than once: $tool.")
        }
        elseif ($script:WAIVABLE_TOOLS -cnotcontains $tool) {
            $errors.Add("Checkpoint issue_adoption.waived_tools names a tool that cannot be waived: $tool.")
        }
        elseif ($RequiredMcpTool -cnotcontains $tool) {
            $errors.Add("Checkpoint issue_adoption.waived_tools names a tool that is not required by route ${RouteId}: $tool.")
        }
        elseif ($SuccessfulTool -ccontains $tool) {
            $errors.Add("Checkpoint issue_adoption.waived_tools names a tool that has a successful MCP receipt: $tool.")
        }
        [void]$seen.Add($tool)
    }

    if (-not $seen.Contains($script:POTENTIAL_TO_ISSUE_TOOL)) {
        $errors.Add($script:ERROR_WAIVED_TOOLS_INCLUDE)
    }
    return $errors.ToArray()
}

function Get-AdoptionPotentialRecordError {
    <#
    .SYNOPSIS
        Require a potential record when a promotion-entry tool is waived (rule 9).
    .PARAMETER WaivedTool
        The well-formed waived tool list, in checkpoint order.
    .PARAMETER PotentialRecord
        The deserialized potential_record value. May be $null.
    .PARAMETER Origin
        The deserialized origin value. May be $null. When the potential_record
        key is absent and the origin is transferred or filed_before_orchestration,
        nothing is reported.
    .PARAMETER RecordPresent
        Whether the potential_record key is present. A present record, including
        $null, is always validated.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $WaivedTool,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $PotentialRecord,

        [Parameter()]
        [AllowNull()]
        [object] $Origin,

        [Parameter(Mandatory = $true)]
        [bool] $RecordPresent
    )

    if (-not $RecordPresent -and ($Origin -is [string]) -and ($script:RECORD_OPTIONAL_ORIGINS -ccontains [string]$Origin)) {
        return [string[]]@()
    }

    $recordIsValid = $false
    if ($PotentialRecord -is [string]) {
        $record = [string]$PotentialRecord
        $recordIsValid = $record.StartsWith($script:POTENTIAL_RECORD_PREFIX, [System.StringComparison]::Ordinal) -and $record.EndsWith($script:POTENTIAL_RECORD_SUFFIX, [System.StringComparison]::Ordinal)
    }
    if ($recordIsValid) { return [string[]]@() }

    $errors = [System.Collections.Generic.List[string]]::new()
    $reported = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($tool in $WaivedTool) {
        if (($script:PROMOTION_ENTRY_TOOLS -ccontains $tool) -and -not $reported.Contains($tool)) {
            $errors.Add("Checkpoint issue_adoption.potential_record must name a markdown file under docs/features/potential/ when waiving $tool.")
            [void]$reported.Add($tool)
        }
    }
    return $errors.ToArray()
}

function Get-OrchestratorStateIssueAdoptionResult {
    <#
    .SYNOPSIS
        Validate the checkpoint's issue_adoption record and resolve its waivers.
    .DESCRIPTION
        Public entry mirroring resolve_issue_adoption. Absent key: no errors and
        no waivers. A present value that is not an object (null included) yields
        only the not-an-object error. Otherwise rules 2 to 9 accumulate errors in
        order, and the waived-tool set is populated only when no error was found.
    .PARAMETER State
        The parsed checkpoint object.
    .PARAMETER RouteId
        The selected route id, interpolated into route errors.
    .PARAMETER RequiredMcpTool
        The route's required MCP tools after promotion-type resolution.
    .PARAMETER SuccessfulTool
        Tools that hold a successful MCP receipt in the checkpoint.
    .OUTPUTS
        System.Collections.Hashtable with keys Errors (string[]) and WaivedTools (string[]).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $true)]
        [string] $RouteId,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $RequiredMcpTool,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $SuccessfulTool
    )

    $member = Get-CheckpointObjectMember -Owner $State -Name $script:ISSUE_ADOPTION_KEY
    if (-not $member.Present) {
        return @{ Errors = [string[]]@(); WaivedTools = [string[]]@() }
    }

    $adoption = $member.Value
    if (-not (Test-CheckpointObjectValue -Value $adoption)) {
        return @{ Errors = [string[]]@($script:ERROR_NOT_OBJECT); WaivedTools = [string[]]@() }
    }

    $errors = [System.Collections.Generic.List[string]]::new()
    $errors.AddRange([string[]]@(Get-AdoptionIssueIdentityError -Adoption $adoption -State $State))
    $errors.AddRange([string[]]@(Get-AdoptionProvenanceError -Adoption $adoption))

    $waived = Get-AdoptionWaivedToolList -Value (Get-CheckpointObjectMember -Owner $adoption -Name 'waived_tools').Value
    if (-not $waived.Ok) {
        # A malformed list stops rule 8 and skips rule 9 entirely.
        $errors.Add($script:ERROR_WAIVED_TOOLS_SHAPE)
        return @{ Errors = $errors.ToArray(); WaivedTools = [string[]]@() }
    }

    $errors.AddRange([string[]]@(Get-AdoptionWaivedToolError -WaivedTool $waived.Value -RouteId $RouteId -RequiredMcpTool $RequiredMcpTool -SuccessfulTool $SuccessfulTool))
    $recordMember = Get-CheckpointObjectMember -Owner $adoption -Name 'potential_record'
    $origin = (Get-CheckpointObjectMember -Owner $adoption -Name 'origin').Value
    $errors.AddRange([string[]]@(Get-AdoptionPotentialRecordError -WaivedTool $waived.Value -PotentialRecord $recordMember.Value -Origin $origin -RecordPresent $recordMember.Present))

    if ($errors.Count -gt 0) {
        return @{ Errors = $errors.ToArray(); WaivedTools = [string[]]@() }
    }

    # The waived-tool set is the distinct entries in first-occurrence order.
    $distinct = [System.Collections.Generic.List[string]]::new()
    foreach ($tool in $waived.Value) {
        if (-not $distinct.Contains($tool)) { $distinct.Add($tool) }
    }
    return @{ Errors = [string[]]@(); WaivedTools = $distinct.ToArray() }
}

# Only the family entry point is exported; the rule helpers stay private.
Export-ModuleMember -Function Get-OrchestratorStateIssueAdoptionResult
