<#
.SYNOPSIS
    Shared PreToolUse hook payload reader: transport, envelope parse, and strict
    nested tool_input extraction (issue #501).

.DESCRIPTION
    Every PreToolUse hook under .claude/hooks/ acquires its payload through this
    module instead of re-implementing the parse inline. The module owns three
    concerns.

    Transport. Claude Code delivers command-hook input on stdin as JSON.
    Read-ClaudeHookRawPayload reads stdin first, then falls back to the
    CLAUDE_HOOK_INPUT environment variable and then the CLAUDE_TOOL_INPUT
    environment variable; the first non-whitespace source wins. Both stdin
    operations sit behind injectable scriptblock seams so tests drive them without
    touching a .NET static and without spawning a process.

    Shape. Tool arguments are nested under the envelope's tool_input object.
    Get-ClaudeHookToolInput extracts that object strictly: a parsed payload with no
    tool_input key, or a tool_input that is null or not an object, is an envelope
    anomaly. There is deliberately no flat-root fallback, because a permissive
    dual-shape reader would keep every legacy-shaped fixture green and restore the
    silent-allow failure mode on the next contract drift.

    Anomaly classification. The parse and extract functions never return a silent
    null for malformed input; they return a typed result whose Anomaly property
    names the failure, and Get-ClaudeHookPayloadAnomalyReason maps that code to the
    deny reason text a hook emits.

    Fail-closed posture: an envelope-level anomaly is a deny, emitted as decision
    JSON at process exit code 0. Exit code 1 is non-blocking for PreToolUse, so a
    throwing hook is itself a fail-open. Property-level absence inside a well-formed
    tool_input is NOT an anomaly: that is each hook's own scope filter and keeps its
    existing allow early-return.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies, no filesystem
    access, no subprocess, no network, and no wall-clock read. Mirrored
    byte-identically under extensions/drm-copilot/resources/claude-customizations/.
#>

Set-StrictMode -Version Latest

# Anomaly codes. These are the only values Get-ClaudeHookPayloadAnomalyReason maps.
$script:AnomalyEmptyPayload = 'EmptyPayload'
$script:AnomalyUnparseableJson = 'UnparseableJson'
$script:AnomalyMissingToolInput = 'MissingToolInput'
$script:AnomalyNullToolInput = 'NullToolInput'
$script:AnomalyNonObjectToolInput = 'NonObjectToolInput'

# The nested key that carries the tool arguments in the documented envelope.
$script:ToolInputKey = 'tool_input'

# Deny reason text per anomaly code. Each string is distinct so an operator can
# tell which leg of the contract drifted from the emitted decision alone.
$script:AnomalyReasonText = @{
    'EmptyPayload'       = 'the hook received an empty payload on stdin and on both environment-variable fallbacks'
    'UnparseableJson'    = 'the hook received a payload that is not parseable JSON'
    'MissingToolInput'   = 'the hook received a JSON payload with no tool_input key (the legacy flat root shape is an envelope anomaly, not a supported payload)'
    'NullToolInput'      = 'the hook received a JSON payload whose tool_input is null'
    'NonObjectToolInput' = 'the hook received a JSON payload whose tool_input is not an object'
}

function Get-ClaudeHookPayloadAnomalyCode {
    <#
    .SYNOPSIS
        Return the anomaly codes this module can emit, so callers need not
        hard-code the literals in a second place.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]@(
        $script:AnomalyEmptyPayload,
        $script:AnomalyUnparseableJson,
        $script:AnomalyMissingToolInput,
        $script:AnomalyNullToolInput,
        $script:AnomalyNonObjectToolInput
    )
}

function Get-ClaudeHookPayloadAnomalyReason {
    <#
    .SYNOPSIS
        Map an anomaly code to the clause a hook puts in its deny reason.
    .PARAMETER Anomaly
        One of the codes returned by Get-ClaudeHookPayloadAnomalyCode.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Anomaly
    )

    if ([string]::IsNullOrWhiteSpace($Anomaly)) {
        return 'the hook received an unclassified payload anomaly'
    }
    if ($script:AnomalyReasonText.ContainsKey($Anomaly)) {
        return [string]$script:AnomalyReasonText[$Anomaly]
    }
    return ('the hook received an unrecognized payload anomaly ({0})' -f $Anomaly)
}

function ConvertTo-ClaudeHookPayloadResult {
    <#
    .SYNOPSIS
        Build the typed result every parse/extract function returns: IsValid, Value
        (the parsed envelope or extracted tool_input), and Anomaly (null when
        valid). Always an object, so no caller tests for null first.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)]
        [bool] $IsValid,

        [AllowNull()]
        [object] $Value,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $Anomaly
    )

    $normalizedAnomaly = $null
    if (-not [string]::IsNullOrWhiteSpace($Anomaly)) {
        $normalizedAnomaly = [string]$Anomaly
    }

    return [pscustomobject]@{
        IsValid = $IsValid
        Value   = $Value
        Anomaly = $normalizedAnomaly
    }
}

function Read-ClaudeHookRawPayload {
    <#
    .SYNOPSIS
        Acquire the raw hook payload text, stdin first.

    .DESCRIPTION
        Ordering: stdin, then the CLAUDE_HOOK_INPUT environment variable, then the
        CLAUDE_TOOL_INPUT environment variable. The first non-whitespace source
        wins; when every source is empty or whitespace the function returns an empty
        string, which callers classify as the EmptyPayload anomaly.

        The stdin read is guarded by a redirect probe. An unguarded
        [Console]::In.ReadToEnd() blocks indefinitely on a non-redirected console,
        which would hang the documented manual/CLI invocation of validate-bash.ps1.
        The guard lives in this function body rather than inside the read seam's
        default so a test can drive both polarities by injection. A stdin read that
        throws falls back rather than propagating (persist-session-id.ps1 precedent).

    .PARAMETER ReadStandardInput
        Seam for the stdin read. Default is the bare [Console]::In.ReadToEnd().
    .PARAMETER TestStandardInputRedirected
        Seam for the redirect probe. Default is [Console]::IsInputRedirected. When
        it evaluates falsey, stdin is treated as empty and the env fallback runs.
    .PARAMETER HookInputFallback
        Seam for the first environment fallback.
    .PARAMETER ToolInputFallback
        Seam for the second environment fallback.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [scriptblock] $ReadStandardInput = { [Console]::In.ReadToEnd() },

        [scriptblock] $TestStandardInputRedirected = { [Console]::IsInputRedirected },

        [AllowNull()]
        [AllowEmptyString()]
        [string] $HookInputFallback = $env:CLAUDE_HOOK_INPUT,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputFallback = $env:CLAUDE_TOOL_INPUT
    )

    $raw = ''

    $isRedirected = $false
    try {
        $isRedirected = [bool](& $TestStandardInputRedirected)
    } catch {
        $isRedirected = $false
    }

    if ($isRedirected) {
        try {
            $raw = [string](& $ReadStandardInput)
        } catch {
            $raw = ''
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($raw)) {
        return $raw
    }

    if (-not [string]::IsNullOrWhiteSpace($HookInputFallback)) {
        return [string]$HookInputFallback
    }

    if (-not [string]::IsNullOrWhiteSpace($ToolInputFallback)) {
        return [string]$ToolInputFallback
    }

    return ''
}

function ConvertFrom-ClaudeHookEnvelope {
    <#
    .SYNOPSIS
        Parse raw hook payload text into the envelope object.

    .DESCRIPTION
        Strips a UTF-8 byte-order mark and normalizes CRLF before parsing, because
        stdin text arrives with either line ending and a BOM survives a piped read.
        Never returns a silent null for malformed input: an empty or whitespace-only
        payload yields the EmptyPayload anomaly and unparseable text yields the
        UnparseableJson anomaly.

    .PARAMETER Raw
        The raw payload text from Read-ClaudeHookRawPayload.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (typed result)
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Raw
    )

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyEmptyPayload
    }

    # A BOM decoded into the string appears as U+FEFF and makes ConvertFrom-Json fail.
    $text = ([string]$Raw).TrimStart([char]0xFEFF)
    $text = $text.Replace("`r`n", "`n")

    if ([string]::IsNullOrWhiteSpace($text)) {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyEmptyPayload
    }

    try {
        $envelope = $text | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyUnparseableJson
    }

    if ($null -eq $envelope) {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyUnparseableJson
    }

    return ConvertTo-ClaudeHookPayloadResult -IsValid $true -Value $envelope -Anomaly $null
}

function Test-ClaudeHookEnvelopeHasKey {
    <#
    .SYNOPSIS
        Report whether a parsed JSON object (PSCustomObject or dictionary) carries a
        named key, without throwing under StrictMode.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        [object] $Envelope,

        [Parameter(Mandatory)]
        [string] $Name
    )

    if ($null -eq $Envelope) {
        return $false
    }
    if ($Envelope -is [System.Collections.IDictionary]) {
        return $Envelope.Contains($Name)
    }
    if ($Envelope -isnot [psobject]) {
        return $false
    }
    return (@($Envelope.PSObject.Properties.Name) -contains $Name)
}

function Get-ClaudeHookEnvelopeValue {
    <#
    .SYNOPSIS
        Read a named value off a parsed JSON object, returning null when the key is
        absent rather than throwing under StrictMode.
    .OUTPUTS
        System.Object or null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        [object] $Envelope,

        [Parameter(Mandatory)]
        [string] $Name
    )

    if (-not (Test-ClaudeHookEnvelopeHasKey -Envelope $Envelope -Name $Name)) {
        return $null
    }
    if ($Envelope -is [System.Collections.IDictionary]) {
        return $Envelope[$Name]
    }
    return $Envelope.PSObject.Properties[$Name].Value
}

function Test-ClaudeHookObjectValue {
    <#
    .SYNOPSIS
        Report whether a parsed JSON value is an object rather than a scalar or an
        array. ConvertFrom-Json produces a PSCustomObject for a JSON object, a
        native scalar for a scalar, and an array for a JSON array; only the first is
        a usable tool_input.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        [object] $Value
    )

    if ($null -eq $Value) {
        return $false
    }
    if ($Value -is [System.Collections.IDictionary]) {
        return $true
    }
    if ($Value -is [string] -or $Value -is [bool] -or $Value -is [ValueType]) {
        return $false
    }
    if ($Value -is [System.Collections.IEnumerable]) {
        return $false
    }
    return ($Value -is [psobject])
}

function Get-ClaudeHookToolInput {
    <#
    .SYNOPSIS
        Extract the envelope's nested tool_input object. Strict: no flat-root
        fallback.

    .DESCRIPTION
        A parsed payload with no tool_input key is the MissingToolInput anomaly,
        which is exactly what the legacy flat root shape produces. A tool_input
        present but null is NullToolInput; present but not an object is
        NonObjectToolInput. Absence of a property inside a well-formed tool_input is
        not handled here at all: that stays each hook's own scope filter.

    .PARAMETER Envelope
        The parsed envelope object from ConvertFrom-ClaudeHookEnvelope.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (typed result)
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()]
        [object] $Envelope
    )

    if ($null -eq $Envelope) {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyMissingToolInput
    }

    if (-not (Test-ClaudeHookEnvelopeHasKey -Envelope $Envelope -Name $script:ToolInputKey)) {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyMissingToolInput
    }

    $toolInput = Get-ClaudeHookEnvelopeValue -Envelope $Envelope -Name $script:ToolInputKey

    if ($null -eq $toolInput) {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyNullToolInput
    }

    if (-not (Test-ClaudeHookObjectValue -Value $toolInput)) {
        return ConvertTo-ClaudeHookPayloadResult -IsValid $false -Value $null -Anomaly $script:AnomalyNonObjectToolInput
    }

    return ConvertTo-ClaudeHookPayloadResult -IsValid $true -Value $toolInput -Anomaly $null
}

function Get-ClaudeHookToolInputString {
    <#
    .SYNOPSIS
        Read a named string property off an already-extracted tool_input object.

    .DESCRIPTION
        Property-level tolerance lives here: an absent property returns an empty
        string, which every hook already treats as "out of my scope, allow". This
        helper exists only so the hooks do not each re-implement the StrictMode-safe
        property probe.

    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [object] $ToolInput,

        [Parameter(Mandatory)]
        [string] $Name
    )

    $value = Get-ClaudeHookEnvelopeValue -Envelope $ToolInput -Name $Name
    if ($null -eq $value) {
        return ''
    }
    return [string]$value
}

function Resolve-ClaudeHookToolInput {
    <#
    .SYNOPSIS
        One-call convenience: raw text in, typed tool_input result out.

    .DESCRIPTION
        Composes ConvertFrom-ClaudeHookEnvelope and Get-ClaudeHookToolInput and
        additionally surfaces the parsed envelope on the Envelope member, because
        enforce-epic-invocation-origin.ps1 needs the envelope root (agent_type)
        alongside the nested tool_input. Returns the envelope-parse anomaly unchanged
        when the parse fails, so a hook maps a single Anomaly value to a single deny
        reason.

    .PARAMETER Raw
        The raw payload text from Read-ClaudeHookRawPayload.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Raw
    )

    $parsed = ConvertFrom-ClaudeHookEnvelope -Raw $Raw
    if (-not $parsed.IsValid) {
        return [pscustomobject]@{
            IsValid  = $false
            Value    = $null
            Envelope = $null
            Anomaly  = $parsed.Anomaly
        }
    }

    $extracted = Get-ClaudeHookToolInput -Envelope $parsed.Value
    return [pscustomobject]@{
        IsValid  = $extracted.IsValid
        Value    = $extracted.Value
        Envelope = $parsed.Value
        Anomaly  = $extracted.Anomaly
    }
}

Export-ModuleMember -Function `
    Read-ClaudeHookRawPayload, `
    ConvertFrom-ClaudeHookEnvelope, `
    Get-ClaudeHookToolInput, `
    Resolve-ClaudeHookToolInput, `
    Get-ClaudeHookEnvelopeValue, `
    Test-ClaudeHookEnvelopeHasKey, `
    Test-ClaudeHookObjectValue, `
    Get-ClaudeHookToolInputString, `
    Get-ClaudeHookPayloadAnomalyCode, `
    Get-ClaudeHookPayloadAnomalyReason
