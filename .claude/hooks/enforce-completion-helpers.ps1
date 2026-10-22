<#
.SYNOPSIS
    Dot-sourced helper functions for enforce-completion-consistency.ps1.

.DESCRIPTION
    Provides testable validation helpers used by the completion-consistency
    PreToolUse hook:

      - Test-IsValidIssueNum: rejects sentinel/placeholder and non-digit issue
        numbers; accepts digits-only strings.
      - Test-IsValidFeatureFolder: rejects sentinel/placeholder feature folders
        and folders not anchored under docs/features/active/<segment>; optionally
        verifies on-disk existence through an injectable scriptblock seam.
      - Get-EditReplaceAllFlag: returns the boolean value of the replace_all
        property of an Edit tool input; the string 'true' is honoured and the
        string 'false' is not.
      - Invoke-SingleOccurrenceEdit: pure single-occurrence patch of a text;
        reports a failure cause instead of replacing when old_string is empty,
        absent, or ambiguous.

    This script is dot-sourced by enforce-completion-consistency.ps1. It contains
    no entrypoint logic, so dot-sourcing it in tests has no side effects.

.NOTES
    Compatible with PowerShell 7+.
#>
[CmdletBinding()]
param()

# Sentinel/placeholder values that must never satisfy a presence check.
$script:CompletionEvidenceSentinels = @('n/a', 'none', 'tbd')

function Test-IsValidIssueNum {
    <#
    .SYNOPSIS
        Returns $true only for a digits-only issue number.
    .DESCRIPTION
        Returns $false when the value is empty, whitespace-only, a sentinel
        (n/a, none, tbd; case-insensitive), or contains any non-digit character.
        Returns $true only when the trimmed value matches ^\d+$.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $false
    }
    $trimmed = $Value.Trim()
    # Sentinel placeholders are explicitly rejected even though they are
    # non-empty strings; the comparison is case-insensitive.
    if ($script:CompletionEvidenceSentinels -contains $trimmed.ToLowerInvariant()) {
        return $false
    }
    return $trimmed -match '^\d+$'
}

function Test-IsValidFeatureFolder {
    <#
    .SYNOPSIS
        Returns $true only for a sentinel-free feature folder anchored under
        docs/features/active/ with a non-empty trailing segment that exists.
    .DESCRIPTION
        Returns $false when the value is empty, whitespace-only, or a sentinel
        (n/a, none, tbd; case-insensitive). Requires the value to start with
        'docs/features/active/' and to carry at least one additional non-empty
        path segment after that prefix. Invokes the injectable FolderExistsCheck
        scriptblock (default Test-Path -PathType Container) and returns $false
        when it reports the folder does not exist.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Value,

        [Parameter(Mandatory = $false)]
        [scriptblock] $FolderExistsCheck = { param($p) Test-Path -LiteralPath $p -PathType Container }
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $false
    }
    $trimmed = $Value.Trim()
    if ($script:CompletionEvidenceSentinels -contains $trimmed.ToLowerInvariant()) {
        return $false
    }

    $prefix = 'docs/features/active/'
    $normalized = $trimmed -replace '\\', '/'
    if (-not $normalized.StartsWith($prefix)) {
        return $false
    }

    # Require a non-empty segment after the active/ prefix so the bare prefix is
    # not accepted as a valid folder.
    $suffix = $normalized.Substring($prefix.Length).TrimEnd('/')
    if ([string]::IsNullOrWhiteSpace($suffix)) {
        return $false
    }

    return [bool](& $FolderExistsCheck $normalized)
}

function Test-RouteRequiresPrGate {
    <#
    .SYNOPSIS
        Returns $true when the payload's selected route opts into the PR gate.
    .DESCRIPTION
        Resolves the route id from the payload (route_id, falling back to
        path_selected), looks it up in the routing matrix returned by the
        injectable RoutingMatrixReader seam, and returns $true only when that
        route's requires_pr_gate value is the boolean $true. A missing route id,
        an unknown route, a matrix without routes, or a missing/false
        requires_pr_gate returns $false. This generalizes the former issue-232
        special-casing into a route-driven check.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $Payload,

        [Parameter(Mandatory = $false)]
        [scriptblock] $RoutingMatrixReader = {
            $configPath = Join-Path $PSScriptRoot '../../config/orchestration-routing.json'
            if (-not (Test-Path -LiteralPath $configPath)) { return $null }
            Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
        }
    )

    if ($null -eq $Payload) {
        return $false
    }

    # Resolve the selected route id, preferring route_id over path_selected.
    $routeId = ''
    if ($Payload.PSObject.Properties.Name -contains 'route_id') {
        $routeId = ([string]$Payload.route_id).Trim()
    }
    if (-not $routeId -and ($Payload.PSObject.Properties.Name -contains 'path_selected')) {
        $routeId = ([string]$Payload.path_selected).Trim()
    }
    if (-not $routeId) {
        return $false
    }

    $matrix = & $RoutingMatrixReader
    if ($null -eq $matrix -or -not ($matrix.PSObject.Properties.Name -contains 'routes')) {
        return $false
    }
    $routes = $matrix.routes
    if ($null -eq $routes -or -not ($routes.PSObject.Properties.Name -contains $routeId)) {
        return $false
    }
    $route = $routes.$routeId
    if ($null -eq $route -or -not ($route.PSObject.Properties.Name -contains 'requires_pr_gate')) {
        return $false
    }
    return ([bool]$route.requires_pr_gate -eq $true)
}

function Get-EditReplaceAllFlag {
    <#
    .SYNOPSIS
        Returns the boolean value of the replace_all property of an Edit tool input.
    .DESCRIPTION
        Returns $false when the tool input is null or has no replace_all property.
        Returns the value when it is already a boolean. Returns $true for a string
        whose trimmed, case-insensitive value is 'true'. Returns $false for every
        other value. No [bool] cast is used because [bool]'false' is $true in
        PowerShell.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $ToolInput
    )

    if ($null -eq $ToolInput -or -not ($ToolInput.PSObject.Properties.Name -contains 'replace_all')) {
        return $false
    }

    $value = $ToolInput.replace_all
    if ($value -is [bool]) {
        return $value
    }
    if ($value -is [string]) {
        return $value.Trim() -ieq 'true'
    }
    return $false
}

function Invoke-SingleOccurrenceEdit {
    <#
    .SYNOPSIS
        Applies an old_string to new_string patch to a text in memory and reports
        a failure cause when the patch cannot be applied unambiguously.
    .DESCRIPTION
        Pure function with no I/O. Normalizes CRLF to LF in all three strings. An
        empty OldString yields Failure 'no-old_string'. Zero ordinal occurrences
        yield Failure 'old_string-not-found'. When ReplaceAll is true every
        occurrence is replaced by ordinal String.Replace. Otherwise a second
        ordinal search that starts after the end of the first match yields Failure
        'old_string-ambiguous' when it finds a match (overlapping candidates are
        counted once), and no match yields a single splice by Substring. No regular
        expression is used, so a '$' in NewString stays literal. Returns an object
        with Content and Failure; Content is $null on failure and Failure is $null
        on success.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text,

        [Parameter(Mandatory)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $OldString,

        [Parameter(Mandatory)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $NewString,

        [Parameter(Mandatory = $false)]
        [bool] $ReplaceAll = $false
    )

    $normalizedText = $Text.Replace("`r`n", "`n")
    $normalizedOld = $OldString.Replace("`r`n", "`n")
    $normalizedNew = $NewString.Replace("`r`n", "`n")

    if ($normalizedOld.Length -eq 0) {
        return [pscustomobject]@{ Content = $null; Failure = 'no-old_string' }
    }

    $comparison = [System.StringComparison]::Ordinal
    $firstIndex = $normalizedText.IndexOf($normalizedOld, $comparison)
    if ($firstIndex -lt 0) {
        return [pscustomobject]@{ Content = $null; Failure = 'old_string-not-found' }
    }

    if ($ReplaceAll) {
        return [pscustomobject]@{ Content = $normalizedText.Replace($normalizedOld, $normalizedNew); Failure = $null }
    }

    $afterFirst = $firstIndex + $normalizedOld.Length
    if ($normalizedText.IndexOf($normalizedOld, $afterFirst, $comparison) -ge 0) {
        return [pscustomobject]@{ Content = $null; Failure = 'old_string-ambiguous' }
    }

    $spliced = $normalizedText.Substring(0, $firstIndex) + $normalizedNew + $normalizedText.Substring($afterFirst)
    return [pscustomobject]@{ Content = $spliced; Failure = $null }
}
