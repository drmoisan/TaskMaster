<#
.SYNOPSIS
    Pinned routing-matrix constants and route accessors for the portable checks.

.DESCRIPTION
    Implements deliberate deviation PD-1 exactly as the feature spec records it.
    The subset of `config/orchestration-routing.json` that the completion checks
    consume - each route's `requires_pr_gate`, `requires_ci_gate`,
    `required_agents`, `required_skills`, and `required_mcp_tools` - is embedded
    here as pinned constants, following the established `ModelRouting.psm1:33-39`
    pattern.

    NO DISK READ AT VALIDATION TIME. This module never opens
    `config/orchestration-routing.json`. That file is deliberately not shipped to
    consumer repositories, and the Python reference crashes with an uncaught
    FileNotFoundError in a repository that lacks it - even on a plain validator
    call. A missing-config crash, or a blanket block, is precisely the portability
    failure this feature exists to remove, so fail-closed-on-missing-config was
    rejected in favour of pinned constants. The config is read only by the static
    config-parity Pester test, which runs in drm-copilot where the file exists and
    is the oracle that keeps these constants honest.

    Route-value resolution is exported too, because two different Python rules
    exist and both must be reproduced: the routing-contract and preparation
    checks read the raw `route_id` value (falling back to `path_selected` only
    when the `route_id` KEY is absent), while the gate helpers additionally
    require that value to be a non-blank string.

    Every accessor takes an optional -RoutingMatrix override so a caller can
    supply an alternative matrix, mirroring the Python `routing_matrix` keyword.
    The override exists for testability and for the malformed-matrix check; it is
    never used to read from disk.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input.
#>

Set-StrictMode -Version Latest

# Import the shared checkpoint-value primitives, resolved relative to this
# module's directory so the import travels with the pushed-down pack.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force

# The pinned routing-matrix subset. Each route records the two gate flags and the
# three required-name lists the completion checks consume. A gate flag of $null
# means the key is ABSENT from the config, which is semantically distinct from
# $false: an absent requires_ci_gate keeps the CI gate required, while an absent
# requires_pr_gate leaves the PR gate not required.
$script:PINNED_ROUTES = @{
    small       = @{
        requires_pr_gate   = $null
        requires_ci_gate   = $null
        required_agents    = @('atomic-planner', 'atomic-executor', 'feature-review')
        required_skills    = @('orchestrate', 'feature-promotion-lifecycle', 'atomic-plan-contract', 'acceptance-criteria-tracking', 'pr-context-artifacts', 'pr-base-branch-merge-base')
        required_mcp_tools = @('new_potential_entry', 'potential_to_issue', 'new_active_feature_folder', 'collect_pr_context', 'validate_orchestration_artifacts')
    }
    large       = @{
        requires_pr_gate   = $true
        requires_ci_gate   = $null
        required_agents    = @('task-researcher', 'prd-feature', 'atomic-planner', 'atomic-executor', 'feature-review', 'pr-author')
        required_skills    = @('orchestrate', 'feature-promotion-lifecycle', 'atomic-plan-contract', 'acceptance-criteria-tracking', 'pr-context-artifacts', 'pr-base-branch-merge-base')
        required_mcp_tools = @('new_potential_entry', 'potential_to_issue', 'new_active_feature_folder', 'collect_pr_context', 'validate_orchestration_artifacts')
    }
    remediation = @{
        requires_pr_gate   = $null
        requires_ci_gate   = $null
        required_agents    = @('atomic-planner', 'atomic-executor', 'feature-review')
        required_skills    = @('orchestrate', 'atomic-plan-contract', 'acceptance-criteria-tracking', 'pr-context-artifacts')
        required_mcp_tools = @('collect_pr_context', 'validate_orchestration_artifacts')
    }
    preparation = @{
        requires_pr_gate   = $null
        requires_ci_gate   = $false
        required_agents    = @('task-researcher', 'prd-feature', 'atomic-planner', 'atomic-executor')
        required_skills    = @('orchestrate', 'feature-promotion-lifecycle', 'atomic-plan-contract')
        required_mcp_tools = @('new_potential_entry', 'potential_to_issue', 'new_active_feature_folder', 'validate_orchestration_artifacts')
    }
    parallel    = @{
        requires_pr_gate   = $false
        requires_ci_gate   = $null
        required_agents    = @('orchestrator', 'pr-author')
        required_skills    = @('parallel-orchestrate', 'orchestrate', 'feature-promotion-lifecycle', 'atomic-plan-contract', 'acceptance-criteria-tracking', 'evidence-and-timestamp-conventions', 'pr-context-artifacts', 'pr-base-branch-merge-base')
        required_mcp_tools = @('collect_pr_context', 'validate_orchestration_artifacts')
    }
    epic        = @{
        requires_pr_gate   = $true
        requires_ci_gate   = $null
        required_agents    = @('orchestrator', 'pr-author')
        required_skills    = @('epic-orchestrate', 'orchestrate', 'feature-promotion-lifecycle', 'atomic-plan-contract', 'acceptance-criteria-tracking', 'evidence-and-timestamp-conventions', 'pr-context-artifacts', 'pr-base-branch-merge-base')
        required_mcp_tools = @('collect_pr_context', 'validate_orchestration_artifacts')
    }
}

# The three per-route list names the completion and routing-contract checks read.
$script:ROUTE_LIST_NAMES = @('required_agents', 'required_skills', 'required_mcp_tools')


function Get-OrchestratorStateRoutingMatrix {
    <#
    .SYNOPSIS
        Return the pinned routing matrix in the shape the Python matrix has.
    .DESCRIPTION
        Returns a hashtable with a single `routes` member, mirroring the top-level
        shape of config/orchestration-routing.json so the accessors and the
        malformed-matrix check operate on the same structure whether the matrix is
        the pinned default or a caller-supplied override. No file is read.
    .OUTPUTS
        System.Collections.Hashtable with a `routes` member.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    return @{ routes = $script:PINNED_ROUTES }
}

function Get-OrchestratorStateRoutingMatrixRouteMap {
    <#
    .SYNOPSIS
        Return a matrix's routes mapping, or $null when the matrix is malformed.
    .DESCRIPTION
        Private-shape accessor mirroring the Python `matrix.get("routes")` guard.
        A matrix whose `routes` member is absent or is not a mapping yields $null,
        which the routing-contract check reports as a malformed matrix.
    .PARAMETER RoutingMatrix
        The matrix to inspect. When omitted, the pinned matrix is used.
    .OUTPUTS
        System.Collections.Hashtable, or $null when the matrix carries no routes
        mapping.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    $matrix = if ($null -ne $RoutingMatrix) { $RoutingMatrix } else { Get-OrchestratorStateRoutingMatrix }
    if (-not $matrix.ContainsKey('routes')) { return $null }
    $routes = $matrix['routes']
    if ($routes -isnot [hashtable]) { return $null }
    return $routes
}

function Get-OrchestratorStateRoute {
    <#
    .SYNOPSIS
        Return one route's pinned entry, or $null when the route is unknown.
    .DESCRIPTION
        Accessor mirroring the Python `routes.get(route_id)` lookup plus its
        `isinstance(raw_route, dict)` guard. A null or unknown route id, or a
        malformed matrix, yields $null.
    .PARAMETER RouteId
        The route identifier. May be $null.
    .PARAMETER RoutingMatrix
        Optional matrix override. When omitted, the pinned matrix is used.
    .OUTPUTS
        System.Collections.Hashtable, or $null when the route is unknown.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $RouteId,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    if ([string]::IsNullOrEmpty($RouteId)) { return $null }
    $routes = Get-OrchestratorStateRoutingMatrixRouteMap -RoutingMatrix $RoutingMatrix
    if ($null -eq $routes -or -not $routes.ContainsKey($RouteId)) { return $null }
    $route = $routes[$RouteId]
    if ($route -isnot [hashtable]) { return $null }
    return $route
}

function Get-OrchestratorStateRawRouteValue {
    <#
    .SYNOPSIS
        Return the checkpoint's raw route value without a string requirement.
    .DESCRIPTION
        Reproduces the Python expression `state.get("route_id",
        state.get("path_selected"))`. The distinction matters: when the `route_id`
        KEY is present its value is used even if that value is null, and only an
        ABSENT `route_id` key falls back to `path_selected`. The preparation
        terminal check compares this raw value directly.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.Object - the raw route value, which may be $null or a non-string.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $routeIdField = Get-CheckpointObjectMember -Owner $State -Name 'route_id'
    if ($routeIdField.Present) { return $routeIdField.Value }
    return (Get-CheckpointObjectMember -Owner $State -Name 'path_selected').Value
}

function Get-OrchestratorStateSelectedRouteId {
    <#
    .SYNOPSIS
        Return the checkpoint's selected route id, or $null when unusable.
    .DESCRIPTION
        Reproduces the Python `_selected_route_id` helper: the raw route value is
        usable only when it is a non-blank string. Every gate accessor and the
        phase-completeness check resolve the route through this rule.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String - the route id, or $null when absent, non-string, or blank.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $value = Get-OrchestratorStateRawRouteValue -State $State
    if (-not ($value -is [string]) -or [string]::IsNullOrWhiteSpace([string]$value)) { return $null }
    return [string]$value
}

function Test-OrchestratorStateRouteRequiresPrGate {
    <#
    .SYNOPSIS
        Report whether a route requires the completion PR gate.
    .DESCRIPTION
        Mirrors `route_requires_pr_gate`. The gate applies only when the route
        exists and its `requires_pr_gate` value is exactly the boolean true, so a
        missing route id, an unknown route, and an absent flag all report false.
    .PARAMETER RouteId
        The route identifier. May be $null.
    .PARAMETER RoutingMatrix
        Optional matrix override. When omitted, the pinned matrix is used.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $RouteId,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    $route = Get-OrchestratorStateRoute -RouteId $RouteId -RoutingMatrix $RoutingMatrix
    if ($null -eq $route -or -not $route.ContainsKey('requires_pr_gate')) { return $false }
    return (($route['requires_pr_gate'] -is [bool]) -and [bool]$route['requires_pr_gate'])
}

function Test-OrchestratorStateRouteRequiresCiGate {
    <#
    .SYNOPSIS
        Report whether a route requires the completion CI gate.
    .DESCRIPTION
        Mirrors `route_requires_ci_gate`. Only an explicit boolean false opts a
        route out, so a missing route id, an unknown route, and an absent flag all
        keep the CI gate required. The asymmetry with the PR gate is deliberate
        and is the historical behaviour the Python reference preserves.
    .PARAMETER RouteId
        The route identifier. May be $null.
    .PARAMETER RoutingMatrix
        Optional matrix override. When omitted, the pinned matrix is used.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $RouteId,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    $route = Get-OrchestratorStateRoute -RouteId $RouteId -RoutingMatrix $RoutingMatrix
    if ($null -eq $route -or -not $route.ContainsKey('requires_ci_gate')) { return $true }
    return -not (($route['requires_ci_gate'] -is [bool]) -and -not [bool]$route['requires_ci_gate'])
}

function Get-OrchestratorStateRouteRequiredList {
    <#
    .SYNOPSIS
        Return one of a route's three required-name lists.
    .DESCRIPTION
        Mirrors the Python `_route_list` helper: a route that does not carry the
        named list, or carries a value that is not a list of non-blank strings,
        contributes an empty list rather than an error.
    .PARAMETER RouteId
        The route identifier. May be $null.
    .PARAMETER ListName
        One of required_agents, required_skills, required_mcp_tools.
    .PARAMETER RoutingMatrix
        Optional matrix override. When omitted, the pinned matrix is used.
    .OUTPUTS
        System.String[] - the required names in matrix order, possibly empty.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $RouteId,

        [Parameter(Mandatory = $true)]
        [ValidateSet('required_agents', 'required_skills', 'required_mcp_tools')]
        [string] $ListName,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    $route = Get-OrchestratorStateRoute -RouteId $RouteId -RoutingMatrix $RoutingMatrix
    if ($null -eq $route -or -not $route.ContainsKey($ListName)) { return [string[]]@() }

    # A malformed list contributes nothing, matching the Python helper's
    # None-to-empty-list conversion rather than raising.
    $value = $route[$ListName]
    if ($value -isnot [System.Array]) { return [string[]]@() }
    foreach ($item in $value) {
        if (-not ($item -is [string]) -or [string]::IsNullOrWhiteSpace([string]$item)) { return [string[]]@() }
    }
    return [string[]]@($value)
}

function Get-OrchestratorStateRouteListName {
    <#
    .SYNOPSIS
        Return the three per-route required-name list names.
    .DESCRIPTION
        Read-only accessor so the routing-contract check and the config-parity
        test iterate one declared set of list names instead of restating it.
    .OUTPUTS
        System.String[] - the three list names.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]@($script:ROUTE_LIST_NAMES)
}

# The matrix, the route lookup, both gate predicates, the required-list accessor,
# and both route-value resolvers are exported for the completion-checks and
# routing-contract modules and for the static config-parity test.
Export-ModuleMember -Function `
    Get-OrchestratorStateRoutingMatrix, `
    Get-OrchestratorStateRoutingMatrixRouteMap, `
    Get-OrchestratorStateRoute, `
    Get-OrchestratorStateRawRouteValue, `
    Get-OrchestratorStateSelectedRouteId, `
    Test-OrchestratorStateRouteRequiresPrGate, `
    Test-OrchestratorStateRouteRequiresCiGate, `
    Get-OrchestratorStateRouteRequiredList, `
    Get-OrchestratorStateRouteListName
