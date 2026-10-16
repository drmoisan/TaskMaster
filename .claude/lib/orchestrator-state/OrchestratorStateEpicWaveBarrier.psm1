<#
.SYNOPSIS
    PowerShell port of the epic wave-barrier Layer 2 ordering check (issue #840).

.DESCRIPTION
    Ports validate_wave_barrier_ordering (scripts/dev_tools/_epic_orchestrator_state_wave_barrier.py)
    as reached through validate_epic_orchestrator_state_text: feature extraction, the union
    index with lifecycle-prefix normalization, the start guard, and the status and timing
    checks with status-before-timing precedence. For in-parity inputs the violation strings
    are byte-identical to the Python authority.

    The checkpoint is parsed with System.Text.Json (JsonDocument), not ConvertFrom-Json:
    ConvertFrom-Json coerces ISO-8601 strings to DateTime and reads property names
    case-insensitively, and either behavior would break parity. Properties are read through
    an ordinal, last-definition-wins enumeration, which matches Python json semantics for a
    duplicate key.

    An input this port does not evaluate fails closed with an error whose message starts
    with EPIC_WAVE_BARRIER_UNEVALUABLE:. The declared divergence classes are listed in
    tests/scripts/claude-lib/orchestrator-state/OrchestratorStateEpicWaveBarrier.Parity.Tests.ps1.

.NOTES
    PowerShell 7+. Reads no file and starts no process: the caller supplies checkpoint text
    it has already read. Mirrored byte-identically into the bundled extension payload.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Lifecycle prefixes a folder hint may carry, tested in this order; the first match is stripped.
$script:LifecyclePrefixes = @('docs/features/active/', 'docs/features/completed/', 'active/', 'completed/')

# The merge statuses that durably confirm a dependency edge, and the literal not-started status.
$script:MergedStatuses = @('merged', 'worktree_removed')
$script:NotStartedStatus = 'not_started'

# A JSON number token that Python reads as an int; any other number token is not compared.
$script:IntegerTokenPattern = '^-?(0|[1-9][0-9]*)$'

# The fail-closed token and the two violation formats of the Python authority.
$script:UnevaluableToken = 'EPIC_WAVE_BARRIER_UNEVALUABLE:'
$script:StatusViolationFormat = 'EPIC_WAVE_BARRIER_VIOLATION: {0} is treated as started while dependency {1} is not merged'
$script:TimingViolationFormat = 'EPIC_WAVE_BARRIER_VIOLATION: {0} worktree_created_at precedes dependency {1} merge_confirmed_at'

# Private: the last property of an object element whose name equals Name (ordinal), so a
# duplicate key resolves the way Python json does. Found is $false when there is none.
function Get-EpicWaveBarrierProperty {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Element,
        [Parameter(Mandatory = $true)] [string] $Name
    )

    $found = $false
    $value = $null
    foreach ($property in $Element.EnumerateObject()) {
        if ([string]::Equals($property.Name, $Name, [System.StringComparison]::Ordinal)) {
            $found = $true
            $value = $property.Value
        }
    }
    return [pscustomobject]@{ Found = $found; Value = $value }
}

# Private: the string value of a named property, or $null when it is absent or not a string.
function Get-EpicWaveBarrierString {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Element,
        [Parameter(Mandatory = $true)] [string] $Name
    )

    $property = Get-EpicWaveBarrierProperty -Element $Element -Name $Name
    if ($property.Found -and $property.Value.ValueKind -eq [System.Text.Json.JsonValueKind]::String) {
        return $property.Value.GetString()
    }
    return $null
}

# Private: strip the first matching lifecycle prefix (ordinal), as _normalize_folder_hint does.
function ConvertTo-EpicWaveBarrierFolderHint {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Value
    )

    foreach ($prefix in $script:LifecyclePrefixes) {
        if ($Value.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            return $Value.Substring($prefix.Length)
        }
    }
    return $Value
}

# Private: the canonical decimal text of an integer token; BigInteger keeps values beyond
# 64 bits exact and renders -0 as 0, as Python str(int) does.
function ConvertTo-EpicWaveBarrierIntegerText {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [string] $RawToken
    )

    $invariant = [System.Globalization.CultureInfo]::InvariantCulture
    return [System.Numerics.BigInteger]::Parse($RawToken, $invariant).ToString($invariant)
}

# Private: the union-index key of a JSON number. Python indexes an int by value; a
# non-integer token is outside parity, so it fails closed rather than being compared.
function Get-EpicWaveBarrierNumberKey {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Element,
        [Parameter(Mandatory = $true)] [string] $Role,
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Folder
    )

    $raw = $Element.GetRawText()
    if ($raw -cnotmatch $script:IntegerTokenPattern) {
        throw ("{0} feature '{1}' has a non-integer numeric {2} token '{3}', which this port does not compare" -f
            $script:UnevaluableToken, $Folder, $Role, $raw)
    }
    return 'n:' + (ConvertTo-EpicWaveBarrierIntegerText -RawToken $raw)
}

# Private: render a resolved dependency reference as Python str() renders the raw value.
# Only strings, booleans, and integer numbers resolve, so no other kind reaches this point.
function Format-EpicWaveBarrierReference {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Element
    )

    $kind = $Element.ValueKind
    if ($kind -eq [System.Text.Json.JsonValueKind]::String) { return $Element.GetString() }
    if ($kind -eq [System.Text.Json.JsonValueKind]::True) { return 'True' }
    if ($kind -eq [System.Text.Json.JsonValueKind]::False) { return 'False' }
    return (ConvertTo-EpicWaveBarrierIntegerText -RawToken $Element.GetRawText())
}

# Private: the start guard of feature_has_started. A string worktree_created_at (empty
# included) starts a feature; otherwise only the exact string not_started leaves it unstarted.
function Test-EpicWaveBarrierFeatureStarted {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Feature
    )

    if ($null -ne (Get-EpicWaveBarrierString -Element $Feature -Name 'worktree_created_at')) { return $true }
    $status = Get-EpicWaveBarrierProperty -Element $Feature -Name 'merge_status'
    if (-not $status.Found) { return $true }
    $isNotStarted = ($status.Value.ValueKind -eq [System.Text.Json.JsonValueKind]::String) -and
    [string]::Equals($status.Value.GetString(), $script:NotStartedStatus, [System.StringComparison]::Ordinal)
    return (-not $isNotStarted)
}

# Private: whether a dependency's merge_status is a string equal (ordinal) to a merged status.
# An absent or non-string status arrives as $null, which the [string] binding turns into an
# empty string, and an empty string is not a merged status.
function Test-EpicWaveBarrierMerged {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Status
    )

    foreach ($merged in $script:MergedStatuses) {
        if ([string]::Equals($Status, $merged, [System.StringComparison]::Ordinal)) { return $true }
    }
    return $false
}

# Private: the union-index key of a feature's issue_num, or $null when it is absent, null,
# or a string (a string issue_num is never matched by a non-string reference).
function Get-EpicWaveBarrierIssueKey {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Feature,
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Folder
    )

    $issue = Get-EpicWaveBarrierProperty -Element $Feature -Name 'issue_num'
    if (-not $issue.Found) { return $null }
    $kind = $issue.Value.ValueKind
    # Routing table: an array or object cannot be hashed by the authority (fail closed);
    # booleans key as 1 and 0 because Python treats True == 1; numbers key by value.
    if ($kind -eq [System.Text.Json.JsonValueKind]::Array -or $kind -eq [System.Text.Json.JsonValueKind]::Object) {
        throw ("{0} feature '{1}' has an issue_num that is an array or object, which the Python authority cannot index" -f
            $script:UnevaluableToken, $Folder)
    }
    if ($kind -eq [System.Text.Json.JsonValueKind]::True) { return 'n:1' }
    if ($kind -eq [System.Text.Json.JsonValueKind]::False) { return 'n:0' }
    if ($kind -eq [System.Text.Json.JsonValueKind]::Number) {
        return (Get-EpicWaveBarrierNumberKey -Element $issue.Value -Role 'issue_num' -Folder $Folder)
    }
    return $null
}

# Private: the object entries of a list-valued features property, in order; empty when the
# root is not an object or features is absent or not a list.
function Get-EpicWaveBarrierFeatureEntry {
    [CmdletBinding()]
    [OutputType([System.Text.Json.JsonElement[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Root
    )

    $entries = [System.Collections.Generic.List[System.Text.Json.JsonElement]]::new()
    if ($Root.ValueKind -ne [System.Text.Json.JsonValueKind]::Object) { return , $entries.ToArray() }
    $features = Get-EpicWaveBarrierProperty -Element $Root -Name 'features'
    if (-not $features.Found -or $features.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::Array) {
        return , $entries.ToArray()
    }
    # Keep only object-shaped entries, as _extract_features does.
    foreach ($item in $features.Value.EnumerateArray()) {
        if ($item.ValueKind -eq [System.Text.Json.JsonValueKind]::Object) { $entries.Add($item) }
    }
    return , $entries.ToArray()
}

# Private: resolve one depends_on element to a feature folder through the union index, or
# $null. A string consults the folder hints only; a boolean or number consults issue_num only.
function Resolve-EpicWaveBarrierReference {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [System.Text.Json.JsonElement] $Dependency,
        [Parameter(Mandatory = $true)] [System.Collections.Generic.Dictionary[string, string]] $ByHint,
        [Parameter(Mandatory = $true)] [System.Collections.Generic.Dictionary[string, string]] $ByIssue,
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Folder
    )

    $kind = $Dependency.ValueKind
    $key = $null
    $map = $ByIssue
    if ($kind -eq [System.Text.Json.JsonValueKind]::String) {
        $key = ConvertTo-EpicWaveBarrierFolderHint -Value $Dependency.GetString()
        $map = $ByHint
    }
    elseif ($kind -eq [System.Text.Json.JsonValueKind]::True) { $key = 'n:1' }
    elseif ($kind -eq [System.Text.Json.JsonValueKind]::False) { $key = 'n:0' }
    elseif ($kind -eq [System.Text.Json.JsonValueKind]::Number) {
        $key = Get-EpicWaveBarrierNumberKey -Element $Dependency -Role 'depends_on reference' -Folder $Folder
    }
    if ($null -eq $key -or -not $map.ContainsKey($key)) { return $null }
    return $map[$key]
}

# Private: the violation lines for the object entries, in feature and depends_on order.
function Get-EpicWaveBarrierViolation {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyCollection()] [System.Text.Json.JsonElement[]] $Entry
    )

    $ordinal = [System.StringComparer]::Ordinal
    $byFolder = [System.Collections.Generic.Dictionary[string, System.Text.Json.JsonElement]]::new($ordinal)
    $byHint = [System.Collections.Generic.Dictionary[string, string]]::new($ordinal)
    $byIssue = [System.Collections.Generic.Dictionary[string, string]]::new($ordinal)

    # Build the lookups: every string folder by name, and every non-empty string folder by
    # its normalized hint and its issue_num. A later duplicate replaces an earlier one.
    foreach ($feature in $Entry) {
        $folder = Get-EpicWaveBarrierString -Element $feature -Name 'feature_folder'
        if ($null -eq $folder) { continue }
        $byFolder[$folder] = $feature
        if ($folder.Length -eq 0) { continue }
        $byHint[(ConvertTo-EpicWaveBarrierFolderHint -Value $folder)] = $folder
        $issueKey = Get-EpicWaveBarrierIssueKey -Feature $feature -Folder $folder
        if ($null -ne $issueKey) { $byIssue[$issueKey] = $folder }
    }

    # Check every started feature with a list depends_on; each edge yields at most one line,
    # and the status violation takes precedence over the timing violation.
    foreach ($feature in $Entry) {
        $folder = Get-EpicWaveBarrierString -Element $feature -Name 'feature_folder'
        $dependsOn = Get-EpicWaveBarrierProperty -Element $feature -Name 'depends_on'
        if ($null -eq $folder -or -not $dependsOn.Found -or $dependsOn.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::Array) { continue }
        if (-not (Test-EpicWaveBarrierFeatureStarted -Feature $feature)) { continue }
        $created = Get-EpicWaveBarrierString -Element $feature -Name 'worktree_created_at'
        foreach ($dependency in $dependsOn.Value.EnumerateArray()) {
            $resolved = Resolve-EpicWaveBarrierReference -Dependency $dependency -ByHint $byHint -ByIssue $byIssue -Folder $folder
            if ($null -eq $resolved -or -not $byFolder.ContainsKey($resolved)) { continue }
            $target = $byFolder[$resolved]
            $reference = Format-EpicWaveBarrierReference -Element $dependency
            if (-not (Test-EpicWaveBarrierMerged -Status (Get-EpicWaveBarrierString -Element $target -Name 'merge_status'))) {
                $script:StatusViolationFormat -f $folder, $reference
                continue
            }
            $confirmed = Get-EpicWaveBarrierString -Element $target -Name 'merge_confirmed_at'
            if ($null -ne $confirmed -and $null -ne $created -and [string]::CompareOrdinal($confirmed, $created) -gt 0) {
                $script:TimingViolationFormat -f $folder, $reference
            }
        }
    }
}

function Get-OrchestratorStateEpicWaveBarrierError {
    <#
    .SYNOPSIS
        Return the EPIC_WAVE_BARRIER_VIOLATION lines for an epic checkpoint's text.
    .DESCRIPTION
        Parses the text with System.Text.Json default options and applies the Layer 2
        ordering check. Returns an empty array when there is no violation, when the root is
        not an object, or when features is absent or not a list. Throws an error whose
        message starts with EPIC_WAVE_BARRIER_UNEVALUABLE: for text System.Text.Json does
        not parse (including NaN or Infinity literals and nesting deeper than 64 levels), an
        array or object issue_num, and a non-integer number token that the check consults.
    .PARAMETER CheckpointText
        The raw epic checkpoint JSON text.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $CheckpointText
    )

    $document = $null
    try {
        $document = [System.Text.Json.JsonDocument]::Parse($CheckpointText, [System.Text.Json.JsonDocumentOptions]::new())
    }
    catch {
        throw ('{0} the checkpoint is not parseable by System.Text.Json with default options (a non-standard literal such as NaN or Infinity, or nesting deeper than 64 levels): {1}' -f
            $script:UnevaluableToken, $_.Exception.Message)
    }
    try {
        $entries = Get-EpicWaveBarrierFeatureEntry -Root $document.RootElement
        return , [string[]] @(Get-EpicWaveBarrierViolation -Entry $entries)
    }
    finally {
        $document.Dispose()
    }
}

Export-ModuleMember -Function Get-OrchestratorStateEpicWaveBarrierError
