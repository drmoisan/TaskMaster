<#
.SYNOPSIS
    Codex deployment resolver, ported from the Python reference implementation.

.DESCRIPTION
    Destination-runtime PowerShell port of
    `scripts/dev_tools/resolve_codex_deployment.py`. It resolves the exact Codex
    deployment agent, model, and reasoning effort for one delegation from the
    logical agent, the assessed complexity band, the execution context, and the
    orchestration complexity ceiling. C3 defaults to Terra/high and elevates to
    Sol/high only for an epic child or a C4 orchestration ceiling; the epic
    planner and epic orchestrator personas are always forced to Sol/ultra. No
    model alias and no silent fallback is accepted.

    SINGLE-IMPLEMENTATION RULE. The orchestrator-state U6.X checks
    (`OrchestratorStateCodexModelReceipts.psm1`) MUST call `Resolve-CodexDeployment`
    from this module. They must never re-implement the profile table, the C3
    overlay rule, or the forced-persona rule. This module is the one PowerShell
    implementation of the Codex deployment axis.

    Every constant below is pinned to `config/orchestration-routing.json`
    (`codex_model_policy`) by a static config-parity check in the Python
    reference. Following the `ModelRouting.psm1` pattern, the values are hard-coded
    here and never read from disk, because this module is pushed down to consumer
    repositories that do not receive `config/orchestration-routing.json`.

    Error surface, relied on by the U6.X receipt checks:
      - [System.ArgumentException] is the ValueError-equivalent surface. Its
        Message text reproduces the Python message verbatim, including Python
        tuple and repr() rendering, because inventory row U6.X5 interpolates the
        exception text into its error string.
      - [System.InvalidOperationException] is the ModelUnavailableError-equivalent
        surface. The Python receipt validator catches only ValueError, so this
        exception deliberately does NOT satisfy the U6.X5 branch; it can only be
        raised when a caller supplies an availability set, which the checks never do.

    The function is pure: it reads no file, starts no process, and never mutates
    its input.
#>

Set-StrictMode -Version Latest

# The complexity-band vocabulary, ordered lowest to highest. Pinned to BAND_ORDER
# in scripts/dev_tools/compute_complexity_floor.py.
$script:BAND_ORDER = @('C1', 'C2', 'C3', 'C4')

# Rendered form of the Python BAND_ORDER tuple, used verbatim inside the
# ValueError-equivalent message so the ported text matches character for character.
$script:BAND_ORDER_PYTHON_TUPLE = "('C1', 'C2', 'C3', 'C4')"

# The three permitted execution contexts, and the sorted tuple rendering the
# Python message interpolates.
$script:VALID_EXECUTION_CONTEXTS = @('standalone', 'epic_preparation_child', 'epic_execution_child')
$script:VALID_EXECUTION_CONTEXTS_PYTHON_TUPLE = "('epic_execution_child', 'epic_preparation_child', 'standalone')"

# The execution contexts that elevate a C3 delegation, and the ceiling band that
# does so independently of context.
$script:C3_ELEVATED_EXECUTION_CONTEXTS = @('epic_preparation_child', 'epic_execution_child')
$script:C3_ELEVATED_CEILING = 'C4'
$script:C3_BAND = 'C3'

# The agent families for which a generated per-band deployment agent exists, and
# the logical-to-family alias map.
$script:GENERATED_AGENT_FAMILIES = @(
    'orchestrator',
    'atomic-planner',
    'atomic-executor',
    'feature-reviewer',
    'task-researcher',
    'prd-feature',
    'pr-author',
    'python-typed-engineer',
    'powershell-typed-engineer',
    'csharp-typed-engineer',
    'typescript-engineer'
)
$script:LOGICAL_AGENT_ALIASES = @{ 'feature-review' = 'feature-reviewer' }

# The per-band deployment profiles (suffix, model, reasoning effort).
$script:BASE_PROFILES = @{
    C1 = @{ suffix = 'c1'; model = 'gpt-5.6-luna'; model_reasoning_effort = 'low' }
    C2 = @{ suffix = 'c2'; model = 'gpt-5.6-terra'; model_reasoning_effort = 'medium' }
    C3 = @{ suffix = 'c3'; model = 'gpt-5.6-terra'; model_reasoning_effort = 'high' }
    C4 = @{ suffix = 'c4'; model = 'gpt-5.6-sol'; model_reasoning_effort = 'max' }
}

# The profile a C3 delegation elevates to when the overlay applies.
$script:C3_ELEVATED_PROFILE = @{ suffix = 'c3-elevated'; model = 'gpt-5.6-sol'; model_reasoning_effort = 'high' }

# The personas whose profile is forced regardless of band, context, or ceiling.
# Their deployment agent is the logical agent itself (empty suffix).
$script:FORCED_PERSONA_PROFILES = @{
    'epic-planner'      = @{ suffix = ''; model = 'gpt-5.6-sol'; model_reasoning_effort = 'ultra' }
    'epic-orchestrator' = @{ suffix = ''; model = 'gpt-5.6-sol'; model_reasoning_effort = 'ultra' }
}


function Assert-CodexBand {
    <#
    .SYNOPSIS
        Return a valid complexity band or throw the field-specific ValueError text.
    .DESCRIPTION
        Private helper mirroring _validate_band. The thrown message reproduces the
        Python text exactly, including the tuple rendering of BAND_ORDER and the
        repr() rendering of the offending value, because U6.X5 interpolates it.
    .PARAMETER Value
        The candidate band value.
    .PARAMETER FieldName
        The field name to name in the message.
    .OUTPUTS
        System.String - the validated band.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Value,

        [Parameter(Mandatory = $true)]
        [string] $FieldName
    )

    if ($script:BAND_ORDER -cnotcontains $Value) {
        throw [System.ArgumentException]::new(
            "$FieldName must be one of $($script:BAND_ORDER_PYTHON_TUPLE), found '$Value'."
        )
    }
    return $Value
}

function Assert-CodexExecutionContext {
    <#
    .SYNOPSIS
        Return a valid execution context or throw the ValueError-equivalent text.
    .DESCRIPTION
        Private helper mirroring _validate_context, reproducing the Python message
        including the sorted-tuple rendering of the permitted contexts.
    .PARAMETER Value
        The candidate execution-context value.
    .OUTPUTS
        System.String - the validated execution context.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Value
    )

    if ($script:VALID_EXECUTION_CONTEXTS -cnotcontains $Value) {
        throw [System.ArgumentException]::new(
            "execution_context must be one of $($script:VALID_EXECUTION_CONTEXTS_PYTHON_TUPLE), found '$Value'."
        )
    }
    return $Value
}

function Get-CodexC3OverlayReason {
    <#
    .SYNOPSIS
        Return the deterministic C3 elevation reason, or $null when none applies.
    .DESCRIPTION
        Private helper mirroring _select_c3_overlay_reason. Both conditions
        together report the combined reason; the ordering matters because the
        combined reason must win over either single reason.
    .PARAMETER ExecutionContext
        The validated execution context.
    .PARAMETER OrchestrationComplexityCeiling
        The validated orchestration ceiling band.
    .OUTPUTS
        System.String - the reason, or $null.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $ExecutionContext,

        [Parameter(Mandatory = $true)]
        [string] $OrchestrationComplexityCeiling
    )

    $epicContext = $script:C3_ELEVATED_EXECUTION_CONTEXTS -ccontains $ExecutionContext
    $c4Ceiling = ($OrchestrationComplexityCeiling -ceq $script:C3_ELEVATED_CEILING)

    if ($epicContext -and $c4Ceiling) { return 'epic_context_and_c4_ceiling' }
    if ($epicContext) { return 'epic_context' }
    if ($c4Ceiling) { return 'c4_orchestration_ceiling' }
    return $null
}

function Resolve-CodexDeployment {
    <#
    .SYNOPSIS
        Resolve the exact Codex deployment agent, model, and reasoning effort.
    .DESCRIPTION
        Faithful PowerShell port of resolve_codex_deployment
        (scripts/dev_tools/resolve_codex_deployment.py). Validates both bands and
        the execution context, rejects a ceiling below the band, then selects the
        profile: a forced persona wins outright; otherwise a C3 delegation may
        elevate under the overlay rule and every other band reads the base table.
        The deployment agent name is the resolved family plus the profile suffix.
    .PARAMETER LogicalAgent
        The logical agent name being delegated to.
    .PARAMETER ComplexityBand
        The assessed complexity band, one of C1..C4.
    .PARAMETER ExecutionContext
        One of standalone, epic_preparation_child, epic_execution_child.
    .PARAMETER OrchestrationComplexityCeiling
        The orchestration ceiling band, one of C1..C4, at or above ComplexityBand.
    .PARAMETER AvailableModel
        Optional availability set. When supplied and the routed model is absent
        from it, the resolver throws rather than falling back to another model.
    .OUTPUTS
        System.Collections.Hashtable with the nine resolved keys: logical_agent,
        deployment_agent, complexity_band, execution_context,
        orchestration_complexity_ceiling, c3_overlay_applied, c3_overlay_reason,
        model, model_reasoning_effort.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $LogicalAgent,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ComplexityBand,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ExecutionContext,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $OrchestrationComplexityCeiling,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [string[]] $AvailableModel = $null
    )

    $band = Assert-CodexBand -Value $ComplexityBand -FieldName 'complexity_band'
    $ceiling = Assert-CodexBand -Value $OrchestrationComplexityCeiling -FieldName 'orchestration_complexity_ceiling'
    $context = Assert-CodexExecutionContext -Value $ExecutionContext

    # The ceiling is an upper bound on the delegation band; a ceiling below the
    # band is an inconsistent pair rather than a clamp.
    if ($script:BAND_ORDER.IndexOf($band) -gt $script:BAND_ORDER.IndexOf($ceiling)) {
        throw [System.ArgumentException]::new(
            "orchestration_complexity_ceiling must be greater than or equal to complexity_band, found $ceiling below $band."
        )
    }

    # Routing table: a forced persona ignores band, context, and ceiling entirely
    # and keeps its own name as the deployment agent; every other agent resolves
    # through the family alias map, the C3 overlay rule, and the base profiles.
    if ($script:FORCED_PERSONA_PROFILES.ContainsKey($LogicalAgent)) {
        $deploymentProfile = $script:FORCED_PERSONA_PROFILES[$LogicalAgent]
        $deploymentAgent = $LogicalAgent
        $overlayReason = $null
    } else {
        $deploymentFamily = $LogicalAgent
        if ($script:LOGICAL_AGENT_ALIASES.ContainsKey($LogicalAgent)) {
            $deploymentFamily = $script:LOGICAL_AGENT_ALIASES[$LogicalAgent]
        }
        if ($script:GENERATED_AGENT_FAMILIES -cnotcontains $deploymentFamily) {
            throw [System.ArgumentException]::new("Unsupported Codex logical agent: '$LogicalAgent'.")
        }

        # The overlay is a C3-only rule; every other band reads the base table.
        $overlayReason = $null
        if ($band -ceq $script:C3_BAND) {
            $overlayReason = Get-CodexC3OverlayReason -ExecutionContext $context -OrchestrationComplexityCeiling $ceiling
        }
        $deploymentProfile = if ($overlayReason) { $script:C3_ELEVATED_PROFILE } else { $script:BASE_PROFILES[$band] }
        $deploymentAgent = "$deploymentFamily-$($deploymentProfile['suffix'])"
    }

    # An availability set that omits the routed model is a hard failure: silent
    # fallback to a different model is prohibited.
    if ($null -ne $AvailableModel -and $AvailableModel -cnotcontains $deploymentProfile['model']) {
        throw [System.InvalidOperationException]::new(
            "model_unavailable: required Codex model '$($deploymentProfile['model'])' is unavailable; silent fallback is prohibited."
        )
    }

    return @{
        logical_agent                    = $LogicalAgent
        deployment_agent                 = $deploymentAgent
        complexity_band                  = $band
        execution_context                = $context
        orchestration_complexity_ceiling = $ceiling
        c3_overlay_applied               = ($null -ne $overlayReason)
        c3_overlay_reason                = $overlayReason
        model                            = $deploymentProfile['model']
        model_reasoning_effort           = $deploymentProfile['model_reasoning_effort']
    }
}

# Only the resolver is exported; the validation and overlay helpers are private so
# no consumer can bypass the single entry point.
Export-ModuleMember -Function Resolve-CodexDeployment
