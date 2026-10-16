<#
.SYNOPSIS
    Generic SubagentStop hook for agents that must advertise exact artifact paths.

.DESCRIPTION
    Validates that the SubagentStop envelope read from stdin carries a non-empty
    last_assistant_message, each required artifact token is advertised in that
    text, the advertised path matches the configured contract regex, and the file
    exists on disk.

    Each required artifact spec must use:
        token|regex|description

    Example:
        spec-path|^docs/features/active/.+/spec\.md$|spec artifact

.NOTES
    Reads the SubagentStop envelope from stdin through Read-SubagentStopEnvelope
    (subagentstop-envelope.ps1); the assistant text comes from the helper's
    AssistantText. A non-repairable condition blocks once: when StopHookActive is
    set, the re-entered stop is allowed. Exits 0 to allow termination; a block is
    written to stderr with exit 2 through Exit-SubagentStopBlock. A missing
    AgentName or RequiredArtifact argument exits 2 inline and blocks once. The
    dependency guard and the E1 helper are guarded loads: a failure blocks with
    exit 2, naming the file. Filesystem existence checks go through
    Test-ArtifactFile so tests can mock the boundary without writing temporary
    files.
    An unhandled error during validation blocks with exit 2 and a reason (issue #928).
#>

[CmdletBinding()]
param(
    [string] $AgentName,

    [string[]] $RequiredArtifact
)
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }

Set-StrictMode -Version Latest
try { . (Join-Path $PSScriptRoot 'subagentstop-envelope.ps1') } catch { Add-HookDependencyFailure -Name 'subagentstop-envelope.ps1' -ErrorRecord $_ }
$ErrorActionPreference = 'Stop'
$script:SubagentStopReportAndHaltLine = 'This condition is not repairable by the subagent. Report it to the operator and halt.'

function Test-ArtifactFile {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    return [bool](Test-Path -LiteralPath $Path -PathType Leaf)
}

function ConvertTo-ArtifactSpec {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Spec
    )

    $parts = $Spec -split '\|', 3
    if ($parts.Count -ne 3) {
        throw "Invalid artifact spec '$Spec'. Expected format: token|regex|description."
    }

    return @{
        Token       = $parts[0].Trim()
        Pattern     = $parts[1].Trim()
        Description = $parts[2].Trim()
    }
}

function Get-ArtifactPathFromOutput {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $AgentOutput,

        [Parameter(Mandatory = $true)]
        [string] $Token
    )

    $escapedToken = [regex]::Escape($Token)
    $pattern = "$escapedToken\s*[:=]\s*[""']?([^\s""'\)`]+)|\[$escapedToken\]\(([^)]+)\)"
    $match = [regex]::Match($AgentOutput, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) {
        return $null
    }

    $value = if ($match.Groups[1].Success) { $match.Groups[1].Value } else { $match.Groups[2].Value }
    $value = $value.Trim()
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $null
    }

    return $value
}

function Invoke-RequiredArtifactOutputValidation {
    <#
    .SYNOPSIS
        Reads the SubagentStop envelope, validates the assistant text, and applies the re-entry policy.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $InputText,
        [string] $AgentName,
        [string[]] $RequiredArtifact
    )

    $envelope = if ($PSBoundParameters.ContainsKey('InputText')) { Read-SubagentStopEnvelope -InputText $InputText } else { Read-SubagentStopEnvelope }
    if (-not $envelope.IsValid) { $decision = @{ Ok = $false; Repairable = $false; Message = ('{0} payload anomaly - {1}' -f ("$AgentName hook:"), (Get-SubagentStopEnvelopeAnomalyReason -Anomaly $envelope.Anomaly)) } }
    else {
        $agentOutput = $envelope.AssistantText
        $decision = Get-RequiredArtifactOutputBlockDecision -AgentOutput ([string]$agentOutput) -AgentName $AgentName -RequiredArtifact $RequiredArtifact
    }
    if ($decision.Ok) { return @{ Ok = $true; Message = $null } }
    if (-not $decision.Repairable) {
        if ($envelope.StopHookActive) { return @{ Ok = $true; Message = $null } }
        if (-not $decision.Message.Contains('to the operator and halt.')) { $decision.Message = $decision.Message + "`n" + $script:SubagentStopReportAndHaltLine }
    }
    return @{ Ok = $false; Message = $decision.Message }
}

function Get-RequiredArtifactOutputBlockDecision {
    <#
    .SYNOPSIS
        Applies the required-artifact checks to the assistant text and returns the block decision.
    .DESCRIPTION
        Returns @{ Ok = $true; Message = $null } to allow, or a hashtable with Ok = $false, a
        Repairable boolean, and Message. A malformed RequiredArtifact spec is a registration
        defect the subagent cannot clear, so it is not repairable; the other blocks are.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $AgentOutput,
        [string] $AgentName,
        [string[]] $RequiredArtifact
    )

    if ([string]::IsNullOrWhiteSpace($agentOutput)) {
        return @{ Ok = $false; Repairable = $true; Message = "$AgentName hook: agent output is empty (last_assistant_message carries no text); artifact paths must be reported before termination." }
    }

    $errors = [System.Collections.Generic.List[string]]::new()
    foreach ($specText in $RequiredArtifact) {
        try {
            $spec = ConvertTo-ArtifactSpec -Spec $specText
        }
        catch {
            return @{ Ok = $false; Repairable = $false; Message = "$AgentName hook: $($_.Exception.Message)" }
        }

        $path = Get-ArtifactPathFromOutput -AgentOutput $agentOutput -Token $spec.Token
        if ($null -eq $path) {
            $errors.Add("missing $($spec.Token): <path> for the required $($spec.Description)")
            continue
        }

        if (-not [regex]::IsMatch($path, $spec.Pattern)) {
            $errors.Add("$($spec.Token) '$path' does not match the required location for the $($spec.Description)")
            continue
        }

        if (-not (Test-ArtifactFile -Path $path)) {
            $errors.Add("$($spec.Token) '$path' was advertised for the $($spec.Description) but no file exists at that location")
        }
    }

    if ($errors.Count -gt 0) {
        $message = "$AgentName hook: required artifact validation failed:`n  - " + ($errors -join "`n  - ")
        return @{ Ok = $false; Repairable = $true; Message = $message }
    }

    return @{ Ok = $true; Message = $null }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('validate-required-artifact-output: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'validate-required-artifact-output:'
if ($null -ne $dependencyDecision) { [Console]::Error.WriteLine($dependencyDecision.Reason); exit $dependencyDecision.ExitCode }
if ([string]::IsNullOrWhiteSpace($AgentName) -or $null -eq $RequiredArtifact -or $RequiredArtifact.Count -eq 0) {
    $argumentEnvelope = Read-SubagentStopEnvelope
    if ($argumentEnvelope.StopHookActive) { exit 0 }
    [Console]::Error.WriteLine("validate-required-artifact-output hook: AgentName and at least one RequiredArtifact spec are required.`n$($script:SubagentStopReportAndHaltLine)")
    exit 2
}
try { $result = Invoke-RequiredArtifactOutputValidation -AgentName $AgentName -RequiredArtifact $RequiredArtifact }
catch { Exit-SubagentStopBlock -Message ('validate-required-artifact-output: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed.' + "`n" + $script:SubagentStopReportAndHaltLine) }
if (-not $result.Ok) { Exit-SubagentStopBlock -Message $result.Message }
exit 0
