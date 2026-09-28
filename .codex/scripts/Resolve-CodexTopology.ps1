#Requires -Version 7.0
<#
.SYNOPSIS
    Resolve the initial Codex implementation agent from scope data (issue #697).

.DESCRIPTION
    PowerShell CLI equivalent of `python -m scripts.dev_tools.resolve_codex_topology`
    for destinations without a Python toolchain. Accepts the same GNU-style flags,
    imports CodexTopology.psm1 from .codex/lib/codex-routing (published) or
    .claude/lib/codex-routing (self-hosting), and prints the same JSON receipt.
    Exit codes: 0 success, 1 resolver error or missing module, 2 usage error.

.EXAMPLE
    pwsh -NoProfile -File .codex/scripts/Resolve-CodexTopology.ps1 --language python --production-file-count 2 --test-file-count 3 --execution-context standalone
#>

. (Join-Path $PSScriptRoot 'codex-routing-cli-common.ps1')

function Invoke-CodexTopologyCli {
    <#
    .SYNOPSIS
        Run the topology CLI over a token list and return its streams and exit code.
    .PARAMETER Arguments
        CLI tokens, for example @('--language', 'python', '--execution-context', 'standalone').
    .PARAMETER ModuleCandidates
        Ordered CodexTopology.psm1 candidate paths; the first existing one is used.
    .OUTPUTS
        PSCustomObject with ExitCode, Stdout, and Stderr.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Arguments,
        [string[]] $ModuleCandidates = (Get-CodexRoutingModuleCandidate -ScriptRoot $PSScriptRoot -ModuleName 'CodexTopology.psm1')
    )

    $program = 'Resolve-CodexTopology.ps1'
    # Flags, kinds, defaults, and choices match build_parser() in
    # scripts/dev_tools/resolve_codex_topology.py.
    $specification = [ordered]@{
        '--language'              = @{ Key = 'language'; Kind = 'append'; Required = $false; Default = $null }
        '--production-file-count' = @{ Key = 'production_file_count'; Kind = 'int'; Required = $false; Default = 0 }
        '--test-file-count'       = @{ Key = 'test_file_count'; Kind = 'int'; Required = $false; Default = 0 }
        '--execution-context'     = @{
            Key      = 'execution_context'
            Kind     = 'value'
            Required = $true
            Choices  = @('epic_execution_child', 'epic_preparation_child', 'standalone')
            Default  = $null
        }
        '--cross-cutting'         = @{ Key = 'cross_cutting'; Kind = 'switch'; Required = $false; Default = $false }
        '--root-persona'          = @{
            Key      = 'root_persona'
            Kind     = 'value'
            Required = $false
            Choices  = @('epic-orchestrator', 'epic-planner')
            Default  = $null
        }
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
    $resolver = $module.ExportedCommands['Resolve-CodexTopology']
    $values = $parsed.Values
    # The module throws ArgumentException for every invalid combination; that is
    # the equivalent of the Python ValueError and maps to exit 1.
    try {
        $receipt = & $resolver `
            -Language ([string[]]$values['language']) `
            -ProductionFileCount $values['production_file_count'] `
            -TestFileCount $values['test_file_count'] `
            -ExecutionContext $values['execution_context'] `
            -CrossCutting ([bool]$values['cross_cutting']) `
            -RootPersona $values['root_persona']
    } catch [System.ArgumentException] {
        return New-CodexRoutingCliResult -ExitCode 1 -Stderr "$($_.Exception.Message)`n"
    }
    return New-CodexRoutingCliResult -ExitCode 0 -Stdout ((ConvertTo-CodexRoutingJson -InputObject $receipt) + "`n")
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$cliResult = Invoke-CodexTopologyCli -Arguments ([string[]]$args)
[Console]::Out.Write($cliResult.Stdout)
[Console]::Error.Write($cliResult.Stderr)
exit $cliResult.ExitCode
