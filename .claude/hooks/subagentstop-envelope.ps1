<#
.SYNOPSIS
    Shared SubagentStop envelope reader and exit-2 block helper (issue #897, epic subagentstop-envelope).

.DESCRIPTION
    Dot-sourced by the SubagentStop validators under .claude/hooks. It is not registered as a hook in
    either settings.json copy. It imports nothing (no Import-Module, no using module, no dot-source),
    reads no environment variable, and writes nothing to the success or warning streams; the only
    stream it ever writes is standard error, from Exit-SubagentStopBlock.

    The governing contract is the section "## Authoritative Helper Contract" of
    docs/features/archive/subagentstop-envelope-core-897/spec.md (issue #897; at
    docs/features/active/subagentstop-envelope-core-897/ until the archive move). Where this file
    and any other document differ, that section governs.

.NOTES
    PowerShell 7.5 or later (ConvertFrom-Json -DateKind). Byte-identical bundled mirror at
    extensions/drm-copilot/resources/claude-customizations/.claude/hooks/subagentstop-envelope.ps1.
#>

function Get-SubagentStopEnvelopeAnomalyReason {
    <#
    .SYNOPSIS
        Returns the reason clause for one SubagentStop envelope anomaly code.
    .DESCRIPTION
        Consumers write '<PREFIX> payload anomaly - <clause>', where <PREFIX> is the validator's own
        reason prefix. Any value that is not one of the six codes (including $null, an empty string, and
        a case variant) is a consumer programming error and throws System.ArgumentException naming the
        rejected value. No [ValidateSet()] is used, so the exception is an ArgumentException rather than a
        parameter-binding exception.
    .PARAMETER Anomaly
        One of EmptyPayload, UnparseableJson, NonObjectEnvelope, MissingAssistantMessage,
        NullAssistantMessage or NonStringAssistantMessage (case-sensitive).
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter()]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Anomaly
    )

    switch -CaseSensitive ($Anomaly) {
        'EmptyPayload' { return 'the SubagentStop payload on stdin was empty' }
        'UnparseableJson' { return 'the SubagentStop payload is not valid JSON' }
        'NonObjectEnvelope' { return 'the SubagentStop payload root is not a JSON object' }
        'MissingAssistantMessage' { return 'the SubagentStop payload has no last_assistant_message field' }
        'NullAssistantMessage' { return 'the SubagentStop payload last_assistant_message is null' }
        'NonStringAssistantMessage' { return 'the SubagentStop payload last_assistant_message is not a string' }
        default { throw [System.ArgumentException]::new("Unknown SubagentStop envelope anomaly code: '$Anomaly'.", 'Anomaly') }
    }
}

function Read-SubagentStopEnvelope {
    <#
    .SYNOPSIS
        Reads and classifies a SubagentStop hook envelope from -InputText or from standard input.
    .DESCRIPTION
        When -InputText is bound (including an empty string or $null) the function parses that text and
        does not read standard input. When -InputText is not bound the function reads standard input to
        end; a manual run without redirected stdin therefore waits for end-of-input (Ctrl+Z then Enter on
        Windows, Ctrl+D elsewhere). A read failure is treated as empty input. One leading BOM (U+FEFF) is
        stripped before any other evaluation. The function never throws and never exits; every failure
        maps to one of six anomaly codes, and the derived fields are read from any object root regardless
        of validity.
    .PARAMETER InputText
        The raw envelope text. Omit the parameter to read standard input.
    .OUTPUTS
        Exactly one PSCustomObject with the properties IsValid, Anomaly, Envelope, AssistantText,
        StopHookActive, AgentType and HookEventName, in that order.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter()]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $InputText
    )

    if ($PSBoundParameters.ContainsKey('InputText')) {
        $text = $InputText
    }
    else {
        try {
            $text = [Console]::In.ReadToEnd()
        }
        catch {
            $text = ''
        }
    }

    if ($null -ne $text -and $text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) {
        $text = $text.Substring(1)
    }

    $isValid = $false
    $anomaly = $null
    $envelope = $null
    $assistantText = ''
    $stopHookActive = $false
    $agentType = ''
    $hookEventName = ''

    if ([string]::IsNullOrWhiteSpace($text)) {
        $anomaly = 'EmptyPayload'
    }
    else {
        $parsed = $null
        $parseFailed = $false
        try {
            $parsed = ConvertFrom-Json -InputObject $text -NoEnumerate -DateKind String -ErrorAction Stop
        }
        catch {
            $parseFailed = $true
        }

        if ($parseFailed) {
            $anomaly = 'UnparseableJson'
        }
        elseif (-not ($parsed -is [System.Management.Automation.PSCustomObject])) {
            # The [pscustomobject] accelerator resolves to PSObject and would match non-objects; the full type name does not.
            $anomaly = 'NonObjectEnvelope'
        }
        else {
            $envelope = $parsed

            $messageProperty = @($parsed.PSObject.Properties | Where-Object { $_.Name -ceq 'last_assistant_message' })
            if ($messageProperty.Count -eq 0) {
                $anomaly = 'MissingAssistantMessage'
            }
            elseif ($null -eq $messageProperty[0].Value) {
                $anomaly = 'NullAssistantMessage'
            }
            elseif ($messageProperty[0].Value -is [string]) {
                $isValid = $true
                $assistantText = $messageProperty[0].Value
            }
            else {
                $anomaly = 'NonStringAssistantMessage'
            }

            $stopHookProperty = @($parsed.PSObject.Properties | Where-Object { $_.Name -ceq 'stop_hook_active' })
            if ($stopHookProperty.Count -gt 0 -and $stopHookProperty[0].Value -is [bool]) {
                $stopHookActive = $stopHookProperty[0].Value
            }

            $agentTypeProperty = @($parsed.PSObject.Properties | Where-Object { $_.Name -ceq 'agent_type' })
            if ($agentTypeProperty.Count -gt 0 -and $agentTypeProperty[0].Value -is [string]) {
                $agentType = $agentTypeProperty[0].Value
            }

            $hookEventProperty = @($parsed.PSObject.Properties | Where-Object { $_.Name -ceq 'hook_event_name' })
            if ($hookEventProperty.Count -gt 0 -and $hookEventProperty[0].Value -is [string]) {
                $hookEventName = $hookEventProperty[0].Value
            }
        }
    }

    return [pscustomobject][ordered]@{
        IsValid        = $isValid
        Anomaly        = $anomaly
        Envelope       = $envelope
        AssistantText  = $assistantText
        StopHookActive = $stopHookActive
        AgentType      = $agentType
        HookEventName  = $hookEventName
    }
}

function Exit-SubagentStopBlock {
    <#
    .SYNOPSIS
        Blocks a SubagentStop by writing the reason to standard error and exiting with code 2.
    .DESCRIPTION
        Exit code 2 is the only exit code that blocks a SubagentStop; the stderr text is delivered to the
        subagent as its next instruction. A null or empty Message fails parameter binding before the
        body runs, so no exit occurs in that case.
    .PARAMETER Message
        The block reason written to standard error.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $Message
    )

    [Console]::Error.WriteLine($Message)
    exit 2
}
