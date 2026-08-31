<#
.SYNOPSIS
    Portable routing-contract completion checks (inventory family C6).

.DESCRIPTION
    Destination-runtime PowerShell port of `validate_routing_contract` in
    `scripts/dev_tools/_orchestrator_state_routing.py`, covering parity-inventory
    rows C6.1 through C6.14:

      C6.1   routing matrix carries no routes object          (returns)
      C6.2   no route selected                                (returns)
      C6.3   selected route has no routing-matrix entry       (returns)
      C6.4   required_agents must match the matrix
      C6.5   required_skills must match the matrix
      C6.6   required_mcp_tools must match the matrix
      C6.7   per required agent with no delegation receipt
      C6.8   per required skill with no acknowledged receipt
      C6.9   per required MCP tool with no successful receipt
      C6.10  local_execution_overrides empty-list rules (two message variants)
      C6.11  delegation_bypasses empty-list rules (two message variants)
      C6.12  lifecycle_operations must be a list when present
      C6.13  per non-object lifecycle operation
      C6.14  per lifecycle operation that did not use the MCP surface

    The routing matrix is read from `OrchestratorStateRoutingMatrix.psm1`, which
    implements deviation PD-1: the constants are pinned and no disk read occurs at
    validation time. The optional -RoutingMatrix override mirrors the Python
    `routing_matrix` keyword and is what makes row C6.1 reachable.

    Bug-promotion tool substitution (C6.6 and C6.9). The routing matrix records
    the feature-type promotion-entry tool `new_potential_entry` in every route's
    `required_mcp_tools`. A bug-type promotion genuinely exercises
    `new_potential_bug_entry` instead, so a bug-type checkpoint could never
    truthfully record a `new_potential_entry` receipt. When, and only when, the
    checkpoint's hyphenated `promotion-type` is exactly `"bug"`, each occurrence
    is substituted, preserving matrix order and every other tool. The substituted
    list drives both the exact-match check and the receipt-presence loop, so the
    two never disagree.

    Empty-list semantics (C6.10 and C6.11). The Python rule requires the key to
    EXIST as a list: an ABSENT key is not a list and therefore produces the
    "must be an empty list at completion" variant. That is deliberately stricter
    than the PR-creation-readiness analogue in `OrchestratorState.psm1`, which
    tolerates absence; both behaviours are preserved as-is.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the shared checkpoint-value primitives and the pinned routing matrix,
# resolved relative to this module's directory so both imports travel with the
# pushed-down pack regardless of the working directory.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateCheckpointValue.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'OrchestratorStateRoutingMatrix.psm1') -Force -ErrorAction Stop

# The promotion-entry MCP tools and the promotion type that triggers substitution.
$script:FEATURE_PROMOTION_ENTRY_TOOL = 'new_potential_entry'
$script:BUG_PROMOTION_ENTRY_TOOL = 'new_potential_bug_entry'
$script:BUG_PROMOTION_TYPE = 'bug'
$script:PROMOTION_TYPE_KEY = 'promotion-type'

# The two checkpoint fields that must exist as empty lists at completion.
$script:COMPLETION_EMPTY_LIST_KEYS = @('local_execution_overrides', 'delegation_bypasses')

# The lifecycle-operation surface every recorded operation must have used.
$script:LIFECYCLE_MCP_SURFACE = 'mcp'


function Get-CheckpointNonBlankStringList {
    <#
    .SYNOPSIS
        Return a value as a list of non-blank strings, or $null when malformed.
    .DESCRIPTION
        Private helper mirroring the `_string_list` variant used by the routing
        contract, which requires every member to be a non-blank string. A non-list
        value, or any blank or non-string member, disqualifies the whole value.
    .PARAMETER Value
        The deserialized JSON value to inspect. May be $null.
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
    foreach ($item in @($Value)) {
        if (-not ($item -is [string]) -or [string]::IsNullOrWhiteSpace([string]$item)) {
            return @{ Ok = $false; Value = [string[]]@() }
        }
    }
    return @{ Ok = $true; Value = [string[]]@($Value) }
}

function Get-ResolvedRequiredMcpTool {
    <#
    .SYNOPSIS
        Resolve the promotion-entry MCP tool to the checkpoint's promotion type.
    .DESCRIPTION
        Private helper mirroring _resolve_promotion_entry_tools. Only an explicit
        bug-type promotion swaps the promotion-entry tool; a feature type, an
        absent key, a non-string value, and any other value leave the matrix list
        untouched, so feature-type and legacy checkpoints validate exactly as
        before. Matrix order is preserved.
    .PARAMETER RequiredMcpTool
        The route's declared required_mcp_tools list, in matrix order.
    .PARAMETER State
        The parsed checkpoint object, read for its hyphenated promotion-type key.
    .OUTPUTS
        System.String[] - the resolved tool list, in matrix order.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $RequiredMcpTool,

        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $promotionType = (Get-CheckpointObjectMember -Owner $State -Name $script:PROMOTION_TYPE_KEY).Value
    if (-not (Test-PythonValueEqual -Actual $promotionType -Expected $script:BUG_PROMOTION_TYPE)) {
        return [string[]]@($RequiredMcpTool)
    }

    # Substitute the bug-type promotion-entry tool for the feature-type one while
    # preserving matrix order and every other required tool exactly.
    $resolved = foreach ($tool in $RequiredMcpTool) {
        if ($tool -ceq $script:FEATURE_PROMOTION_ENTRY_TOOL) { $script:BUG_PROMOTION_ENTRY_TOOL } else { $tool }
    }
    return [string[]]@($resolved)
}

function Get-CheckpointReceiptAgentName {
    <#
    .SYNOPSIS
        Collect the agent names recorded by delegation receipts.
    .DESCRIPTION
        Private harvest mirroring _receipt_agents plus _list_receipts. The object
        form of delegation_receipts contributes through its `agents` namespace;
        any other non-list value contributes nothing. Only a non-blank string
        agent_name counts.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String[] - the recorded agent names.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $receipts = (Get-CheckpointObjectMember -Owner $State -Name 'delegation_receipts').Value
    if (Test-CheckpointObjectValue -Value $receipts) {
        $receipts = (Get-CheckpointObjectMember -Owner $receipts -Name 'agents').Value
    }

    $agents = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    if (-not (Test-CheckpointListValue -Value $receipts)) { return [string[]]@($agents) }

    # Harvest each well-formed receipt's agent name; a malformed entry is skipped
    # rather than reported, because shape errors belong to the U5 family.
    foreach ($receipt in @($receipts)) {
        if (-not (Test-CheckpointObjectValue -Value $receipt)) { continue }
        $name = (Get-CheckpointObjectMember -Owner $receipt -Name 'agent_name').Value
        if (($name -is [string]) -and -not [string]::IsNullOrWhiteSpace([string]$name)) {
            [void]$agents.Add([string]$name)
        }
    }
    return [string[]]@($agents)
}

function Get-CheckpointAcknowledgedName {
    <#
    .SYNOPSIS
        Collect acknowledged names from a receipt array with an evidence rule.
    .DESCRIPTION
        Private harvest shared by _receipt_skills and _mcp_tools, which differ
        only in the array key, the name key, and the boolean flag key. A receipt
        counts only when its name is a non-blank string, its flag is exactly the
        boolean true, and its evidence is a non-blank string. Truthy-but-not-true
        flags deliberately do not count.
    .PARAMETER State
        The parsed checkpoint object.
    .PARAMETER ArrayKey
        The checkpoint key holding the receipt array.
    .PARAMETER NameKey
        The per-receipt key holding the acknowledged name.
    .PARAMETER FlagKey
        The per-receipt key that must be exactly boolean true.
    .OUTPUTS
        System.String[] - the acknowledged names.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $true)]
        [string] $ArrayKey,

        [Parameter(Mandatory = $true)]
        [string] $NameKey,

        [Parameter(Mandatory = $true)]
        [string] $FlagKey
    )

    $names = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $receipts = (Get-CheckpointObjectMember -Owner $State -Name $ArrayKey).Value
    if (-not (Test-CheckpointListValue -Value $receipts)) { return [string[]]@($names) }

    # All three conditions must hold together: an acknowledged name, an explicit
    # true flag, and non-blank evidence. A receipt failing any one contributes
    # nothing, so the requirement it would have satisfied is reported missing.
    foreach ($receipt in @($receipts)) {
        if (-not (Test-CheckpointObjectValue -Value $receipt)) { continue }
        $name = (Get-CheckpointObjectMember -Owner $receipt -Name $NameKey).Value
        $flag = (Get-CheckpointObjectMember -Owner $receipt -Name $FlagKey).Value
        $evidence = (Get-CheckpointObjectMember -Owner $receipt -Name 'evidence').Value
        if (($name -is [string]) -and -not [string]::IsNullOrWhiteSpace([string]$name) -and
            ($flag -is [bool]) -and [bool]$flag -and
            ($evidence -is [string]) -and -not [string]::IsNullOrWhiteSpace([string]$evidence)) {
            [void]$names.Add([string]$name)
        }
    }
    return [string[]]@($names)
}

function Get-CompletionEmptyListError {
    <#
    .SYNOPSIS
        Return the empty-list-at-completion errors for one key (rows C6.10/C6.11).
    .DESCRIPTION
        Private helper mirroring _validate_empty_list_field. The key must EXIST as
        a list: an absent key, a null value, and any non-list value all produce the
        "must be an empty list at completion" variant, while a present non-empty
        list produces the "must be empty at completion" variant.
    .PARAMETER State
        The parsed checkpoint object.
    .PARAMETER Key
        The checkpoint key to check.
    .OUTPUTS
        System.String[] - zero or one error string.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $true)]
        [string] $Key
    )

    $value = (Get-CheckpointObjectMember -Owner $State -Name $Key).Value
    if (-not (Test-CheckpointListValue -Value $value)) {
        return [string[]]@("Checkpoint $Key must be an empty list at completion.")
    }
    if (@($value).Count -gt 0) {
        return [string[]]@("Checkpoint $Key must be empty at completion.")
    }
    return [string[]]@()
}

function Get-LifecycleOperationError {
    <#
    .SYNOPSIS
        Return the lifecycle-operation errors (rows C6.12-C6.14).
    .DESCRIPTION
        Private helper mirroring _validate_lifecycle_operations. The key is
        optional: an absent or null value contributes nothing. A present non-list
        value is malformed, and each recorded operation must be an object whose
        surface is exactly the MCP surface.
    .PARAMETER State
        The parsed checkpoint object.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    $operations = (Get-CheckpointObjectMember -Owner $State -Name 'lifecycle_operations').Value
    if ($null -eq $operations) { return $errors.ToArray() }
    if (-not (Test-CheckpointListValue -Value $operations)) {
        $errors.Add('Checkpoint lifecycle_operations must be a list when present.')
        return $errors.ToArray()
    }

    # Report each malformed or non-MCP operation with its own index.
    $index = 0
    foreach ($operation in @($operations)) {
        if (-not (Test-CheckpointObjectValue -Value $operation)) {
            $errors.Add("Checkpoint lifecycle_operations #$index must be an object.")
            $index++
            continue
        }
        $surface = (Get-CheckpointObjectMember -Owner $operation -Name 'surface').Value
        if (-not (Test-PythonValueEqual -Actual $surface -Expected $script:LIFECYCLE_MCP_SURFACE)) {
            $errors.Add("Checkpoint lifecycle_operations #$index did not use MCP surface.")
        }
        $index++
    }

    return $errors.ToArray()
}

function Get-OrchestratorStateRoutingContractError {
    <#
    .SYNOPSIS
        Return the routing-contract errors (inventory rows C6.1-C6.14).
    .DESCRIPTION
        Public entry mirroring validate_routing_contract. The first three rows are
        terminal: a malformed matrix, an unselected route, and an unknown route
        each return a single error, because none of the later checks can be
        evaluated without a resolved route entry. The remaining rows accumulate.
    .PARAMETER State
        The parsed checkpoint object.
    .PARAMETER RoutingMatrix
        Optional matrix override. When omitted, the pinned matrix is used. A
        matrix carrying no routes mapping is what makes row C6.1 reachable.
    .OUTPUTS
        System.String[] - zero or more error strings.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [psobject] $State,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [hashtable] $RoutingMatrix = $null
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # C6.1: a matrix with no routes mapping cannot resolve any route.
    $routes = Get-OrchestratorStateRoutingMatrixRouteMap -RoutingMatrix $RoutingMatrix
    if ($null -eq $routes) {
        return [string[]]@('Routing matrix missing routes object.')
    }

    # C6.2: the checkpoint must name a route as a non-blank string.
    $routeId = Get-OrchestratorStateSelectedRouteId -State $State
    if ($null -eq $routeId) {
        return [string[]]@('Checkpoint route_id or path_selected must select a route.')
    }

    # C6.3: the named route must exist in the matrix.
    $route = Get-OrchestratorStateRoute -RouteId $routeId -RoutingMatrix $RoutingMatrix
    if ($null -eq $route) {
        return [string[]]@("Checkpoint selected route has no routing-matrix entry: $routeId.")
    }

    $requiredAgents = @(Get-OrchestratorStateRouteRequiredList -RouteId $routeId -ListName 'required_agents' -RoutingMatrix $RoutingMatrix)
    $requiredSkills = @(Get-OrchestratorStateRouteRequiredList -RouteId $routeId -ListName 'required_skills' -RoutingMatrix $RoutingMatrix)
    $requiredMcpTools = @(Get-ResolvedRequiredMcpTool -State $State `
            -RequiredMcpTool ([string[]]@(Get-OrchestratorStateRouteRequiredList -RouteId $routeId -ListName 'required_mcp_tools' -RoutingMatrix $RoutingMatrix)))

    # C6.4 to C6.6: the checkpoint's own declared lists must equal the matrix
    # lists exactly, in order. A malformed list is a mismatch, not a shape error.
    $declaredLists = @(
        @{ Key = 'required_agents'; Expected = $requiredAgents },
        @{ Key = 'required_skills'; Expected = $requiredSkills },
        @{ Key = 'required_mcp_tools'; Expected = $requiredMcpTools }
    )
    foreach ($declared in $declaredLists) {
        $actual = Get-CheckpointNonBlankStringList -Value (Get-CheckpointObjectMember -Owner $State -Name $declared.Key).Value
        if (-not $actual.Ok -or -not (Test-PythonValueEqual -Actual $actual.Value -Expected ([string[]]$declared.Expected))) {
            $errors.Add("Checkpoint $($declared.Key) must match routing matrix for route $routeId.")
        }
    }

    # C6.7 to C6.9: every required agent, skill, and MCP tool must be evidenced by
    # a receipt, in matrix order so the report is deterministic.
    $actualAgents = @(Get-CheckpointReceiptAgentName -State $State)
    foreach ($agent in $requiredAgents) {
        if ($actualAgents -cnotcontains $agent) {
            $errors.Add("Checkpoint missing required agent receipt: $agent.")
        }
    }

    $actualSkills = @(Get-CheckpointAcknowledgedName -State $State -ArrayKey 'skill_receipts' -NameKey 'skill' -FlagKey 'required')
    foreach ($skill in $requiredSkills) {
        if ($actualSkills -cnotcontains $skill) {
            $errors.Add("Checkpoint missing required skill receipt: $skill.")
        }
    }

    $actualTools = @(Get-CheckpointAcknowledgedName -State $State -ArrayKey 'mcp_call_receipts' -NameKey 'tool' -FlagKey 'ok')
    foreach ($tool in $requiredMcpTools) {
        if ($actualTools -cnotcontains $tool) {
            $errors.Add("Checkpoint missing successful MCP receipt: $tool.")
        }
    }

    # C6.10 to C6.14: the two empty-list fields and the lifecycle operations.
    foreach ($key in $script:COMPLETION_EMPTY_LIST_KEYS) {
        $errors.AddRange([string[]]@(Get-CompletionEmptyListError -State $State -Key $key))
    }
    $errors.AddRange([string[]]@(Get-LifecycleOperationError -State $State))

    return $errors.ToArray()
}

# Only the family entry point is exported; the harvest and field helpers stay
# private so no consumer can evaluate a subset of the contract.
Export-ModuleMember -Function Get-OrchestratorStateRoutingContractError
