#Requires -Version 7.0
<#
.SYNOPSIS
    Resolve the Codex deployment agent, model, and reasoning effort (issue #697).

.DESCRIPTION
    PowerShell CLI equivalent of `python -m scripts.dev_tools.resolve_codex_deployment`
    for destinations without a Python toolchain. Accepts the same GNU-style flags,
    imports CodexDeployment.psm1 from .codex/lib/codex-routing (published) or
    .claude/lib/codex-routing (self-hosting), and prints the same JSON receipt.
    Exit codes: 0 success, 1 resolver error or missing module, 2 usage error.

.EXAMPLE
    pwsh -NoProfile -File .codex/scripts/Resolve-CodexDeployment.ps1 --logical-agent orchestrator --complexity-band C2 --execution-context standalone --orchestration-complexity-ceiling C3
#>

. (Join-Path $PSScriptRoot 'codex-routing-cli-common.ps1')

function Invoke-CodexDeploymentCli {
    <#
    .SYNOPSIS
        Run the deployment CLI over a token list and return its streams and exit code.
    .PARAMETER Arguments
        CLI tokens, for example @('--logical-agent', 'orchestrator', '--complexity-band', 'C2', ...).
    .PARAMETER ModuleCandidates
        Ordered CodexDeployment.psm1 candidate paths; the first existing one is used.
    .OUTPUTS
        PSCustomObject with ExitCode, Stdout, and Stderr.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Arguments,
        [string[]] $ModuleCandidates = (Get-CodexRoutingModuleCandidate -ScriptRoot $PSScriptRoot -ModuleName 'CodexDeployment.psm1')
    )

    $program = 'Resolve-CodexDeployment.ps1'
    $bands = @('C1', 'C2', 'C3', 'C4')
    # Flags, requirement, and choices match build_parser() in
    # scripts/dev_tools/resolve_codex_deployment.py.
    $specification = [ordered]@{
        '--logical-agent'                    = @{ Key = 'logical_agent'; Kind = 'value'; Required = $true; Default = $null }
        '--complexity-band'                  = @{ Key = 'complexity_band'; Kind = 'value'; Required = $true; Choices = $bands; Default = $null }
        '--execution-context'                = @{
            Key      = 'execution_context'
            Kind     = 'value'
            Required = $true
            Choices  = @('epic_execution_child', 'epic_preparation_child', 'standalone')
            Default  = $null
        }
        '--orchestration-complexity-ceiling' = @{ Key = 'orchestration_complexity_ceiling'; Kind = 'value'; Required = $true; Choices = $bands; Default = $null }
    }

    $parsed = ConvertFrom-CodexRoutingArgument -Arguments $Arguments -Specification $specification
    if ($null -ne $parsed.Error) {
        return New-CodexRoutingCliResult -ExitCode 2 -Stderr "${program}: $($parsed.Error)`n"
    }
    $modulePath = Resolve-CodexRoutingModulePath -Candidate $ModuleCandidates
    if ($null -eq $modulePath) {
        return New-CodexRoutingCliResult -ExitCode 1 -Stderr "CODEX_ROUTING_MODULE_NOT_FOUND: none of the candidate module paths exist: $($ModuleCandidates -join '; ')`n"
    }

    $module = Import-Module -Name $modulePath -Force -PassThru -ErrorAction Stop
    $resolver = $module.ExportedCommands['Resolve-CodexDeployment']
    $values = $parsed.Values
    # The module throws ArgumentException for every invalid combination; that is
    # the equivalent of the Python ValueError and maps to exit 1.
    try {
        $receipt = & $resolver `
            -LogicalAgent $values['logical_agent'] `
            -ComplexityBand $values['complexity_band'] `
            -ExecutionContext $values['execution_context'] `
            -OrchestrationComplexityCeiling $values['orchestration_complexity_ceiling']
    } catch [System.ArgumentException] {
        return New-CodexRoutingCliResult -ExitCode 1 -Stderr "$($_.Exception.Message)`n"
    }
    return New-CodexRoutingCliResult -ExitCode 0 -Stdout ((ConvertTo-CodexRoutingJson -InputObject $receipt) + "`n")
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$cliResult = Invoke-CodexDeploymentCli -Arguments ([string[]]$args)
[Console]::Out.Write($cliResult.Stdout)
[Console]::Error.Write($cliResult.Stderr)
exit $cliResult.ExitCode
