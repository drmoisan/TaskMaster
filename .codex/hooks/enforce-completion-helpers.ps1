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
