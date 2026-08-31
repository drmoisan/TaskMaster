<#
.SYNOPSIS
    Codex implementation-topology resolver, ported from the Python reference.

.DESCRIPTION
    Destination-runtime PowerShell port of
    `scripts/dev_tools/resolve_codex_topology.py`. It resolves the initial
    implementation agent for one change from deterministic scope data: the
    languages touched, the production and test file counts, the execution
    context, the cross-cutting indicator, and an optional forced root persona.

    Routing summary. A standalone, single-language change inside that language's
    canonical direct-mode budget selects the language's typed engineer on the
    `small` route. Every escalation condition selects the orchestrator on the
    `large` route. An explicit root epic persona selects itself on the `epic`
    route. The escalation conditions are evaluated in a fixed precedence order,
    and the first match wins:

      epic_child_context -> invalid_estimate -> cross_language ->
      unsupported_language -> cross_cutting -> direct_mode_disabled ->
      production_budget_exceeded

    SINGLE-IMPLEMENTATION RULE. The orchestrator-state U6.T checks
    (`OrchestratorStateCodexTopologyReceipts.psm1`) MUST call
    `Resolve-CodexTopology` and read `Get-CodexForcedRootPersona` from this
    module. They must never re-implement the language-budget table or the
    escalation precedence. This module is the one PowerShell implementation of
    the Codex topology axis.

    Every constant below is pinned to `config/orchestration-routing.json`
    (`codex_topology_policy`). Following the `ModelRouting.psm1` pattern, the
    values are hard-coded here and never read from disk, because this module is
    pushed down to consumer repositories that do not receive that config.

    Error surface, relied on by the U6.T receipt checks:
      - [System.ArgumentException] is the ValueError-equivalent surface. Its
        Message text reproduces the Python message verbatim, including Python
        tuple and repr() rendering, because inventory row U6.T10 interpolates
        the exception text into its error string.

    Documented divergence: Python raises TypeError (not ValueError) when
    `languages` is not iterable, and the receipt validator does not catch
    TypeError. This port treats a null Language argument as an empty collection,
    which routes to the `unsupported_language` escalation. The difference is
    unreachable from the U6.T checks, which reject a non-list `languages` before
    the resolver is ever called.

    The function is pure: it reads no file, starts no process, and never mutates
    its input.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The three permitted execution contexts, the sorted tuple rendering the Python
# message interpolates, and the two contexts that mark epic child work.
$script:VALID_EXECUTION_CONTEXTS = @('standalone', 'epic_preparation_child', 'epic_execution_child')
$script:VALID_EXECUTION_CONTEXTS_PYTHON_TUPLE = "('epic_execution_child', 'epic_preparation_child', 'standalone')"
$script:EPIC_CHILD_CONTEXTS = @('epic_preparation_child', 'epic_execution_child')
$script:STANDALONE_CONTEXT = 'standalone'

# The two personas that may be forced as the root of a run, and the logical agent
# every escalation routes to.
$script:FORCED_ROOT_PERSONAS = @('epic-planner', 'epic-orchestrator')
$script:ORCHESTRATOR_LOGICAL_AGENT = 'orchestrator'

# The per-language direct-mode budgets. A language absent from this table is an
# unsupported language and escalates.
$script:LANGUAGE_BUDGETS = @{
    python     = @{ direct_mode_enabled = $true; max_production_files = 3; max_test_files = 3; logical_agent = 'python-typed-engineer' }
    powershell = @{ direct_mode_enabled = $true; max_production_files = 2; max_test_files = 3; logical_agent = 'powershell-typed-engineer' }
    csharp     = @{ direct_mode_enabled = $true; max_production_files = 3; max_test_files = 3; logical_agent = 'csharp-typed-engineer' }
    typescript = @{ direct_mode_enabled = $false; max_production_files = 0; max_test_files = 0; logical_agent = 'typescript-engineer' }
}

# The integral CLR types a JSON integer can deserialize to. A CLR boolean is
# deliberately absent: Python rejects bool where an int is required.
$script:INTEGRAL_TYPES = @([int], [long], [short], [byte])


function Get-CodexForcedRootPersona {
    <#
    .SYNOPSIS
        Return the two personas that may be forced as the root of a run.
    .DESCRIPTION
        Read-only accessor for FORCED_ROOT_PERSONAS. Exported so the U6.T
        root_persona enum check reads the same set the resolver enforces instead
        of restating it.
    .OUTPUTS
        System.String[] - the forced root persona names, in declaration order.
        The names are emitted to the pipeline individually, so a caller collects
        them with @(...) in the usual way.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]@($script:FORCED_ROOT_PERSONAS)
}

function Test-CodexIntegralValue {
    <#
    .SYNOPSIS
        Report whether a value is an integer, rejecting booleans.
    .DESCRIPTION
        Private predicate mirroring the Python guard
        `isinstance(value, bool) or not isinstance(value, int)`. Python's bool is
        a subclass of int, so the reference rejects it explicitly; this port
        compares the exact CLR type so no boolean satisfies the test.
    .PARAMETER Value
        The candidate value. May be $null.
    .OUTPUTS
        System.Boolean - $true only for a non-boolean integral value.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    if ($null -eq $Value -or $Value -is [bool]) { return $false }
    foreach ($integralType in $script:INTEGRAL_TYPES) {
        if ($Value.GetType() -eq $integralType) { return $true }
    }
    return $false
}

function Get-CodexNormalizedLanguage {
    <#
    .SYNOPSIS
        Return unique, lowercased language names in stable ordinal order.
    .DESCRIPTION
        Private helper mirroring _normalize_languages. Every element must be a
        non-blank string; anything else throws the ValueError-equivalent. The
        result is deduplicated and ordinally sorted so the resolved receipt is
        order-independent, matching Python's sorted(set(...)).
    .PARAMETER Language
        The raw language collection. A null value is treated as empty.
    .OUTPUTS
        System.String[] - the normalized language names. Declared additionally as
        System.Object[] because the array is emitted through the unary-comma form
        that stops PowerShell unrolling it.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Language
    )

    $normalized = [System.Collections.Generic.HashSet[string]]::new()

    # Reject the whole collection on the first malformed member so a partially
    # valid language list can never route a delegation.
    foreach ($item in @($Language)) {
        if (-not ($item -is [string]) -or [string]::IsNullOrWhiteSpace([string]$item)) {
            throw [System.ArgumentException]::new('languages must contain non-empty strings.')
        }
        [void]$normalized.Add(([string]$item).Trim().ToLowerInvariant())
    }

    # Ordinal ordering matches Python's sorted() over the same names; PowerShell's
    # culture-aware Sort-Object would not be guaranteed to. The unary comma stops
    # PowerShell from unrolling the array on return, which would otherwise turn a
    # single-language result into a bare string and a zero-language result into
    # nothing at all.
    $sorted = [string[]]@($normalized)
    [Array]::Sort($sorted, [System.StringComparer]::Ordinal)
    return , $sorted
}

function Get-CodexEscalationReceipt {
    <#
    .SYNOPSIS
        Build a large-route orchestrator receipt for one escalation reason.
    .DESCRIPTION
        Private pure builder mirroring _orchestrator_receipt. Every escalation returns
        the same shape and differs only in routing_reason and in whether a
        language budget was known at the point of escalation.
    .PARAMETER ExecutionContext
        The validated execution context.
    .PARAMETER Language
        The normalized language names.
    .PARAMETER ProductionFileCount
        The validated production file count.
    .PARAMETER TestFileCount
        The validated test file count.
    .PARAMETER CrossCutting
        The validated cross-cutting indicator.
    .PARAMETER Reason
        The escalation reason recorded in routing_reason.
    .PARAMETER Budget
        The language budget when one was resolved, otherwise $null. When absent,
        both max file counts are reported as null.
    .OUTPUTS
        System.Collections.Hashtable - the twelve-key topology receipt.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)][string] $ExecutionContext,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]] $Language,
        [Parameter(Mandatory = $true)][int] $ProductionFileCount,
        [Parameter(Mandatory = $true)][int] $TestFileCount,
        [Parameter(Mandatory = $true)][bool] $CrossCutting,
        [Parameter(Mandatory = $true)][string] $Reason,
        [Parameter(Mandatory = $false)][AllowNull()][hashtable] $Budget = $null
    )

    return @{
        execution_context     = $ExecutionContext
        languages             = $Language
        production_file_count = $ProductionFileCount
        test_file_count       = $TestFileCount
        cross_cutting         = $CrossCutting
        root_persona          = $null
        route                 = 'large'
        topology              = 'orchestrator'
        logical_agent         = $script:ORCHESTRATOR_LOGICAL_AGENT
        routing_reason        = $Reason
        max_production_files  = $(if ($null -ne $Budget) { $Budget['max_production_files'] } else { $null })
        max_test_files        = $(if ($null -ne $Budget) { $Budget['max_test_files'] } else { $null })
    }
}

function Resolve-CodexTopology {
    <#
    .SYNOPSIS
        Resolve the initial Codex implementation agent from scope data.
    .DESCRIPTION
        Faithful PowerShell port of resolve_codex_topology
        (scripts/dev_tools/resolve_codex_topology.py). Validates the execution
        context, the language collection, the two file counts, and the
        cross-cutting indicator, then applies the forced-root-persona branch or
        the fixed escalation precedence documented in the module header.
    .PARAMETER Language
        The languages the change touches. Normalized to unique lowercase names.
    .PARAMETER ProductionFileCount
        The production file count. Must be a non-boolean integer.
    .PARAMETER TestFileCount
        The test file count. Must be a non-boolean integer.
    .PARAMETER ExecutionContext
        One of standalone, epic_preparation_child, epic_execution_child.
    .PARAMETER CrossCutting
        Whether the change is cross-cutting. Must be a boolean.
    .PARAMETER RootPersona
        An optional forced root persona, which requires a standalone context.
    .OUTPUTS
        System.Collections.Hashtable with the twelve resolved keys:
        execution_context, languages, production_file_count, test_file_count,
        cross_cutting, root_persona, route, topology, logical_agent,
        routing_reason, max_production_files, max_test_files.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Language,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $ProductionFileCount,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $TestFileCount,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ExecutionContext,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [object] $CrossCutting = $false,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [object] $RootPersona = $null
    )

    if ($script:VALID_EXECUTION_CONTEXTS -cnotcontains $ExecutionContext) {
        throw [System.ArgumentException]::new(
            "execution_context must be one of $($script:VALID_EXECUTION_CONTEXTS_PYTHON_TUPLE), found '$ExecutionContext'."
        )
    }
    $context = $ExecutionContext
    $languages = Get-CodexNormalizedLanguage -Language $Language

    # Counts are validated before any routing so a malformed estimate can never
    # be silently coerced into a route decision.
    if (-not (Test-CodexIntegralValue -Value $ProductionFileCount)) {
        throw [System.ArgumentException]::new('production_file_count must be an integer.')
    }
    if (-not (Test-CodexIntegralValue -Value $TestFileCount)) {
        throw [System.ArgumentException]::new('test_file_count must be an integer.')
    }
    if (-not ($CrossCutting -is [bool])) {
        throw [System.ArgumentException]::new('cross_cutting must be a boolean.')
    }
    $productionCount = [int]$ProductionFileCount
    $testCount = [int]$TestFileCount
    $isCrossCutting = [bool]$CrossCutting

    # A forced root persona short-circuits all escalation logic and selects
    # itself, but only from a standalone context.
    if ($null -ne $RootPersona) {
        if (-not ($RootPersona -is [string]) -or ($script:FORCED_ROOT_PERSONAS -cnotcontains [string]$RootPersona)) {
            throw [System.ArgumentException]::new("Unsupported Codex root persona: '$RootPersona'.")
        }
        if ($context -cne $script:STANDALONE_CONTEXT) {
            throw [System.ArgumentException]::new('A forced root persona requires standalone context.')
        }
        return @{
            execution_context     = $context
            languages             = $languages
            production_file_count = $productionCount
            test_file_count       = $testCount
            cross_cutting         = $isCrossCutting
            root_persona          = [string]$RootPersona
            route                 = 'epic'
            topology              = 'epic_persona'
            logical_agent         = [string]$RootPersona
            routing_reason        = 'forced_root_persona'
            max_production_files  = $null
            max_test_files        = $null
        }
    }

    # Escalation precedence. The first matching condition wins, so the order of
    # these guards is part of the contract, not an implementation detail: epic
    # child work escalates before any estimate is trusted, an invalid estimate
    # escalates before language analysis, and the budget checks run last because
    # they require a resolved single-language budget.
    $escalationArgument = @{
        ExecutionContext    = $context
        Language            = $languages
        ProductionFileCount = $productionCount
        TestFileCount       = $testCount
        CrossCutting        = $isCrossCutting
    }

    if ($script:EPIC_CHILD_CONTEXTS -ccontains $context) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'epic_child_context'
    }
    if ($productionCount -le 0 -or $testCount -lt 0) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'invalid_estimate'
    }
    if ($languages.Count -gt 1) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'cross_language'
    }
    if ($languages.Count -ne 1) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'unsupported_language'
    }
    if (-not $script:LANGUAGE_BUDGETS.ContainsKey($languages[0])) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'unsupported_language'
    }

    $budget = $script:LANGUAGE_BUDGETS[$languages[0]]
    if ($isCrossCutting) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'cross_cutting' -Budget $budget
    }
    if (-not $budget['direct_mode_enabled']) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'direct_mode_disabled' -Budget $budget
    }
    if ($productionCount -gt $budget['max_production_files']) {
        return Get-CodexEscalationReceipt @escalationArgument -Reason 'production_budget_exceeded' -Budget $budget
    }

    return @{
        execution_context     = $context
        languages             = $languages
        production_file_count = $productionCount
        test_file_count       = $testCount
        cross_cutting         = $isCrossCutting
        root_persona          = $null
        route                 = 'small'
        topology              = 'typed_engineer'
        logical_agent         = $budget['logical_agent']
        routing_reason        = 'within_language_budget'
        max_production_files  = $budget['max_production_files']
        max_test_files        = $budget['max_test_files']
    }
}

# The resolver and the forced-root-persona accessor are exported; the validation,
# normalization, and receipt-construction helpers stay private so no consumer can
# bypass the single entry point.
Export-ModuleMember -Function Resolve-CodexTopology, Get-CodexForcedRootPersona
