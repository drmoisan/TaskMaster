<#
.SYNOPSIS
    Shared dependency-failure recording and blocking-result construction for registered hooks (issue #786).

.DESCRIPTION
    Dot-sourced by every registered PreToolUse and SubagentStop hook as the first statement after its
    param() block, inside a try whose catch sets $script:HookDependencyGuardLoadFailed. Every guarded
    module load and sibling dot-source of the hook records a failure through Add-HookDependencyFailure;
    the hook's decision function and its tail return Get-HookDependencyFailureDecision when a failure is
    recorded, so a missing or broken dependency blocks instead of failing open.

    The failure list lives in the hook's script scope and is created only when it is absent, so a hook
    that dot-sources another hook entry keeps the records made before the second dot-source.

.NOTES
    PowerShell 7+. Import-free: it loads no module and dot-sources no file. It uses built-in cmdlets and
    .NET types only, writes to no output stream, and calls no function defined by a hook or by a guarded
    dependency. Every absence test reads the list through Get-Variable, so it holds under strict mode.
    Byte-identical at .claude/hooks, .codex/hooks, and both bundle mirrors.
#>

if ($null -eq (Get-Variable -Name HookDependencyFailures -Scope Script -ValueOnly -ErrorAction SilentlyContinue)) { $script:HookDependencyFailures = [System.Collections.Generic.List[object]]::new() }

function Add-HookDependencyFailure {
    <#
    .SYNOPSIS
        Records one dependency that failed to load.
    .PARAMETER Name
        The file name of the module or sibling script that failed to load.
    .PARAMETER ErrorRecord
        The caught error; its first exception line is kept. When omitted, the message reads
        'no error record'.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [System.Management.Automation.ErrorRecord] $ErrorRecord
    )

    if ($null -eq (Get-Variable -Name HookDependencyFailures -Scope Script -ValueOnly -ErrorAction SilentlyContinue)) { $script:HookDependencyFailures = [System.Collections.Generic.List[object]]::new() }
    $message = 'no error record'
    if ($null -ne $ErrorRecord) { $message = ([string]$ErrorRecord.Exception.Message -split "`r?`n")[0] }
    [void]$script:HookDependencyFailures.Add([pscustomobject]@{ Name = $Name; Message = $message })
}

function Test-HookDependencyFailure {
    <#
    .SYNOPSIS
        Returns $true when at least one dependency failure is recorded.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()

    $variable = Get-Variable -Name HookDependencyFailures -Scope Script -ErrorAction SilentlyContinue
    return ($null -ne $variable -and $null -ne $variable.Value -and $variable.Value.Count -gt 0)
}

function Get-HookDependencyFailureReason {
    <#
    .SYNOPSIS
        Builds the blocking reason from the hook's prefix and the first recorded failure.
    .PARAMETER ReasonPrefix
        The hook's existing leading token or prose prefix, ending with a colon.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $ReasonPrefix
    )

    $first = (Get-Variable -Name HookDependencyFailures -Scope Script -ErrorAction SilentlyContinue).Value[0]
    return "$ReasonPrefix the dependency '$($first.Name)' failed to load ($($first.Message)); the gate fails closed."
}

function Get-HookDependencyFailureDecision {
    <#
    .SYNOPSIS
        Returns the blocking result for a recorded dependency failure, or $null when none is recorded.
    .PARAMETER HookEvent
        PreToolUse returns the deny decision in the hookSpecificOutput shape; SubagentStop returns an
        object carrying ExitCode 2 and the Reason the hook writes to stderr.
    .PARAMETER ReasonPrefix
        The hook's existing leading token or prose prefix, ending with a colon.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary], [pscustomobject])]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('PreToolUse', 'SubagentStop')]
        [string] $HookEvent,

        [Parameter(Mandatory)]
        [string] $ReasonPrefix
    )

    if (-not (Test-HookDependencyFailure)) {
        return $null
    }
    $reason = Get-HookDependencyFailureReason -ReasonPrefix $ReasonPrefix
    if ($HookEvent -eq 'SubagentStop') {
        return [pscustomobject]@{ ExitCode = 2; Reason = $reason }
    }
    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $reason
        }
    }
}
