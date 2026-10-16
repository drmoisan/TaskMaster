<#
.SYNOPSIS
    SubagentStop hook that validates discovery artifacts referenced in a
    terminating subagent's final output.

.DESCRIPTION
    Invoked by the Claude Code SubagentStop hook under the existing broad
    generic-agent matcher group. The hook reads the SubagentStop envelope JSON
    from stdin through the shared envelope helper (subagentstop-envelope.ps1),
    takes its last_assistant_message field (the terminating subagent's last
    assistant message), scans that text for discovery-artifact path references
    via Get-DiscoveryArtifactType, and for each recognized reference (when a
    required-artifact declaration is present) invokes the discovery validator
    via Invoke-DiscoveryValidatorExe.

    Any referenced artifact that fails validation blocks the subagent's
    termination: the hook writes the reason with a
    DISCOVERY_ARTIFACT_GATE_BLOCKED: prefix to stderr and exits 2 through the
    shared envelope helper. An invalid envelope (empty, not valid JSON, not a
    JSON object, or without a string last_assistant_message) also blocks, with
    a "DISCOVERY_ARTIFACT_GATE_BLOCKED: payload anomaly" reason. The hook
    allows termination when the last assistant message is blank, when it
    references no discovery-artifact path, when the required-artifact
    declaration is absent (fail-open), or when every referenced artifact
    passes validation.

    This hook provides the authoritative, defense-in-depth check of final
    workspace state, regardless of which tool produced the artifact,
    complementing the PreToolUse gate in
    enforce-discovery-artifact-gate.ps1. Neither hook implements or
    reimplements discovery-validator logic; both only route to the validator
    CLI delivered by a separate feature and interpret its exit code and
    captured output.

.NOTES
    Requires PowerShell 7.4+ (the shared validation module uses
    `Test-Json -SchemaFile` Draft 2020-12 support). Read-only validation gate that
    invokes NO external process: validation runs in-process through
    `.claude/lib/discovery-validation/DiscoveryValidation.psm1` (issue #475).
    An unhandled error during validation blocks with exit 2 and a reason (issue #928).
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }
try { . (Join-Path $PSScriptRoot 'subagentstop-envelope.ps1') } catch { Add-HookDependencyFailure -Name 'subagentstop-envelope.ps1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/discovery-validation/DiscoveryValidation.psm1') -Force -Global -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'DiscoveryValidation.psm1' -ErrorRecord $_ }

function Invoke-DiscoveryValidatorExe {
    <#
    .SYNOPSIS
        Wrapper around the discovery-artifact validator. Mockable seam.
    .DESCRIPTION
        Delegates to the portable PowerShell implementation in
        `.claude/lib/discovery-validation/DiscoveryValidation.psm1`, keeping this
        function's name, its `-ValidatorArgs <string[]>` parameter, and its
        `@{ ExitCode; Output }` return shape unchanged so existing mocks and
        `Should -Invoke` assertions continue to bind.

        This no longer invokes a Python interpreter (issue #475). The `.claude/**`
        payload ships to destinations with no guaranteed Python, Poetry, or
        `scripts/dev_tools`, where the previous `python -m ...` call failed
        obscurely or blocked every operation.

        Success is SILENT by contract: a passing validation returns `ExitCode = 0`
        with an EMPTY `Output`. The caller denies on a non-zero exit code OR on
        non-empty output, so any success chatter here would deny a passing
        validation (defect D-2).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $ValidatorArgs
    )

    $modulePath = Join-Path -Path $PSScriptRoot `
        -ChildPath '../lib/discovery-validation/DiscoveryValidation.psm1'
    if (-not (Test-Path -LiteralPath $modulePath -PathType Leaf)) {
        return @{ ExitCode = 1; Output = "Discovery-validation module not found: $modulePath" }
    }

    Import-Module -Name $modulePath -Force -ErrorAction Stop
    return Invoke-DiscoveryArtifactValidation -ValidatorArgs $ValidatorArgs
}

function Get-DiscoveryArtifactType {
    <#
    .SYNOPSIS
        Maps a normalized file path to a discovery-artifact-type token.
    .DESCRIPTION
        Returns one of the eight validator subcommand tokens (profile,
        feature-contract, coverage-ledger, runtime-scenario, parity-matrix,
        unspecified-behavior, product-decision, evidence-reference), or $null
        when the path does not resolve to a recognized discovery-artifact
        type.

        # TODO(#9002): this is a narrow, replaceable directory/filename lookup.
        The schema-versioned directory/filename convention this mapping
        depends on is owned by #9002 and is not finalized in this branch.
        Replace this lookup once #9002 ships its versioning convention.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $normalized = $Path -replace '\\', '/'

    $typeMap = [ordered]@{
        'discovery/profile'              = 'profile'
        'discovery/feature-contract'     = 'feature-contract'
        'discovery/coverage-ledger'      = 'coverage-ledger'
        'discovery/runtime-scenario'     = 'runtime-scenario'
        'discovery/parity-matrix'        = 'parity-matrix'
        'discovery/unspecified-behavior' = 'unspecified-behavior'
        'discovery/product-decision'     = 'product-decision'
        'discovery/evidence-reference'   = 'evidence-reference'
    }

    foreach ($prefix in $typeMap.Keys) {
        if ($normalized -match "(^|/)$([regex]::Escape($prefix))") {
            return $typeMap[$prefix]
        }
    }

    return $null
}

function Get-RequiredDiscoveryArtifactDeclaration {
    <#
    .SYNOPSIS
        Reads the domain-profile required-artifact declaration, if present.
    .DESCRIPTION
        # TODO(#9001): this is a narrow, injectable RequiredArtifactPathsReader
        seam. The discovery-workspace root and which of the eight artifact
        types are "required" for a given gate are domain-profile runtime
        configuration owned by #9001, which has no shipped parser/schema in
        this branch.

        Default behavior on absence is documented inline as fail-open
        (allow/exit 0): when no domain profile is present, this function
        returns an object with Present = $false, and callers must treat that
        as "always allow, never invoke the validator" rather than as an error.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $false)]
        [scriptblock] $ProfileReader = { $null }
    )

    $declaration = & $ProfileReader
    if ($null -eq $declaration) {
        # Fail open: no domain profile / required-artifact declaration present.
        return @{ Present = $false }
    }

    return @{ Present = $true; Declaration = $declaration }
}

function Find-DiscoveryArtifactReference {
    <#
    .SYNOPSIS
        Extracts candidate discovery-artifact path references from subagent
        output text.
    .DESCRIPTION
        Splits the output text into whitespace-delimited tokens and returns
        the distinct set of tokens that resolve to a recognized
        discovery-artifact type via Get-DiscoveryArtifactType. This is a
        lightweight text scan, not a JSON/markdown parser, matching the
        precision needed for a defense-in-depth completion gate.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $OutputText
    )

    if ([string]::IsNullOrWhiteSpace($OutputText)) {
        return [string[]]@()
    }

    $tokens = $OutputText -split '\s+' | Where-Object { $_ }
    $foundReferences = New-Object System.Collections.Generic.List[string]
    foreach ($token in $tokens) {
        $trimmed = $token.Trim('`', '"', "'", ',', ';', '(', ')', '[', ']')
        if ($trimmed -and (Get-DiscoveryArtifactType -Path $trimmed)) {
            if (-not $foundReferences.Contains($trimmed)) {
                $foundReferences.Add($trimmed)
            }
        }
    }

    return [string[]]$foundReferences
}

function Invoke-DiscoveryArtifactGateValidation {
    <#
    .SYNOPSIS
        Reads the SubagentStop envelope and returns an Ok/Message validation
        result for a discovery-artifact completion gate.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $InputText,

        [Parameter(Mandatory = $false)]
        [scriptblock] $RequiredArtifactReader = { Get-RequiredDiscoveryArtifactDeclaration }
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'DISCOVERY_ARTIFACT_GATE_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }
    $envelope = if ($PSBoundParameters.ContainsKey('InputText')) { Read-SubagentStopEnvelope -InputText $InputText } else { Read-SubagentStopEnvelope }
    if (-not $envelope.IsValid) { return @{ Ok = $false; Message = "DISCOVERY_ARTIFACT_GATE_BLOCKED: payload anomaly - $(Get-SubagentStopEnvelopeAnomalyReason -Anomaly $envelope.Anomaly)" } }
    $assistantMessage = [string]$envelope.AssistantText

    $references = Find-DiscoveryArtifactReference -OutputText $assistantMessage
    if ($references.Count -eq 0) {
        return @{ Ok = $true; Message = $null }
    }

    $requiredDeclaration = & $RequiredArtifactReader
    if (-not $requiredDeclaration.Present) {
        # Fail open: no domain profile / required-artifact declaration present.
        return @{ Ok = $true; Message = $null }
    }

    foreach ($reference in $references) {
        $artifactType = Get-DiscoveryArtifactType -Path $reference
        $validatorResult = Invoke-DiscoveryValidatorExe -ValidatorArgs @($artifactType, $reference)
        $hasErrorOutput = -not [string]::IsNullOrWhiteSpace($validatorResult['Output'])
        if ($validatorResult['ExitCode'] -ne 0 -or $hasErrorOutput) {
            return @{ Ok = $false; Message = "DISCOVERY_ARTIFACT_GATE_BLOCKED: $($validatorResult['Output'])" }
        }
    }

    return @{ Ok = $true; Message = $null }
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('DISCOVERY_ARTIFACT_GATE_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'DISCOVERY_ARTIFACT_GATE_BLOCKED:'
if ($null -ne $dependencyDecision) { [Console]::Error.WriteLine($dependencyDecision.Reason); exit $dependencyDecision.ExitCode }

try { $result = Invoke-DiscoveryArtifactGateValidation }
catch { Exit-SubagentStopBlock -Message ('DISCOVERY_ARTIFACT_GATE_BLOCKED: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed.' + "`n" + 'This condition is not repairable by the subagent. Report it to the operator and halt.') }
if (-not $result.Ok) { Exit-SubagentStopBlock -Message $result.Message }
exit 0
