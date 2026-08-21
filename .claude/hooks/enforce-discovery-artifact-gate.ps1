<#
.SYNOPSIS
    Pre-tool-use hook that enforces discovery-artifact completion gates by
    invoking the discovery validators.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on Write or Edit operations
    (matcher "Write|Edit"). The hook reads $env:CLAUDE_TOOL_INPUT JSON
    containing file_path, and either content (Write) or old_string/new_string
    (Edit).

    For a Write call whose file_path resolves to a recognized discovery
    artifact type, and when a required-artifact declaration for that type is
    present, the hook invokes the discovery validator CLI via
    Invoke-DiscoveryValidatorExe and maps a non-zero exit code or non-empty
    error output to a deny decision. Edit calls are allowed unconditionally
    (Edit supplies only a partial patch, not full file content, so it cannot
    be reliably validated here; the SubagentStop gate is the authoritative
    backstop). A file_path that does not resolve to a recognized discovery
    artifact type, or a required-artifact declaration that is absent, results
    in an allow decision without invoking the validator (fail-open).

    This hook never implements or reimplements discovery-validator logic; it
    only routes to the validator CLI delivered by a separate feature and
    interprets the CLI's exit code and captured output.

.NOTES
    Requires PowerShell 7.4+ (the shared validation module uses
    `Test-Json -SchemaFile` Draft 2020-12 support). Read-only validation gate that
    invokes NO external process: validation runs in-process through
    `.claude/lib/discovery-validation/DiscoveryValidation.psm1` (issue #475).
#>
[CmdletBinding()]
param()

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

        Default behavior on absence is documented inline as fail-open (allow):
        when no domain profile is present, this function returns an object
        with Present = $false, and callers must treat that as "always allow,
        never invoke the validator" rather than as an error.
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

function Invoke-DiscoveryArtifactGateDecision {
    <#
    .SYNOPSIS
        Parses CLAUDE_TOOL_INPUT and returns an allow-or-deny decision for a
        discovery-artifact completion gate.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw,

        [Parameter(Mandatory = $false)]
        [scriptblock] $RequiredArtifactReader = { Get-RequiredDiscoveryArtifactDeclaration }
    )

    if (-not $ToolInputRaw) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "enforce-discovery-artifact-gate hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    $filePath = $toolInput.file_path
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    # Edit calls supply only old_string/new_string (a partial patch), not full
    # file content. They are allowed unconditionally; the SubagentStop gate is
    # the authoritative backstop for artifacts touched by Edit.
    $toolInputProps = @($toolInput.PSObject.Properties.Name)
    $hasContent = $toolInputProps -contains 'content' -and $null -ne $toolInput.content
    if (-not $hasContent) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $artifactType = Get-DiscoveryArtifactType -Path $filePath
    if (-not $artifactType) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $requiredDeclaration = & $RequiredArtifactReader
    if (-not $requiredDeclaration.Present) {
        # Fail open: no domain profile / required-artifact declaration present.
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $result = Invoke-DiscoveryValidatorExe -ValidatorArgs @($artifactType, $filePath)
    $hasErrorOutput = -not [string]::IsNullOrWhiteSpace($result.Output)
    if ($result.ExitCode -ne 0 -or $hasErrorOutput) {
        $reason = "DISCOVERY_ARTIFACT_GATE_BLOCKED: $($result.Output)"
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = $reason
            }
        }
    }

    return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-DiscoveryArtifactGateDecision -ToolInputRaw $env:CLAUDE_TOOL_INPUT
}
catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
