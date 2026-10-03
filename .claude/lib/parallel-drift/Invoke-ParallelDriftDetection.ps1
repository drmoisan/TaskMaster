<#
.SYNOPSIS
    Detect radius drift for one in-flight parallel item and print one JSON object.

.DESCRIPTION
    Destination-runtime entry point (issue #763) for the drift-detection step of
    /parallel-orchestrate, and the PowerShell port of
    scripts/dev_tools/parallel_drift_detection_cli.py. It needs PowerShell 7 and
    no Python interpreter:

        pwsh -NoProfile -NonInteractive -File .claude/lib/parallel-drift/Invoke-ParallelDriftDetection.ps1 `
            -ItemKey <issue_num> [-CheckpointPath <path>] [-ConfigPath <path>] `
            [-At <yyyy-MM-ddTHH-mm>] [-ComputedAt <yyyy-MM-ddTHH-mm>] <CHANGED_PATH>...

    The script reads the parallel checkpoint and the blast-radius truth table,
    evaluates drift with the pure functions of ParallelDrift.psm1, and writes one
    JSON object to stdout with the nine keys result, item_key, at, computed_at,
    escaped_paths, newly_conflicting_pairs, halted_item_keys, drift_event, and
    observed_radius. Keys are sorted ordinally and every array stays an array.
    No file is written and no git command is run: the caller supplies the changed
    paths.

    JSON is read with System.Text.Json rather than ConvertFrom-Json, which turns
    timestamp-shaped strings into DateTime values and does not keep the integer
    and boolean distinction the shape guards rely on. No parameter is Mandatory,
    so a missing -ItemKey exits 2 instead of prompting.

    Exit codes: 0 on success; 1 when an input is missing or malformed, with one
    stderr line prefixed "parallel drift detection failed: "; 2 on a usage error
    (a missing or non-integer -ItemKey, or a remaining argument that begins with
    -), with one stderr line prefixed "parallel drift detection usage error: ".

.PARAMETER ItemKey
    The issue_num of the item whose diff is evaluated. Required.

.PARAMETER CheckpointPath
    The parallel checkpoint; defaults to
    artifacts/orchestration/parallel-orchestrator-state.json. A relative path
    resolves against the current location.

.PARAMETER ConfigPath
    The blast-radius truth table; defaults to config/blast-radius.json.

.PARAMETER At
    The timestamp recorded on the drift event; defaults to the current UTC time
    formatted yyyy-MM-ddTHH-mm, the only clock read.

.PARAMETER ComputedAt
    The timestamp recorded on the observed radius; defaults to the resolved At.

.PARAMETER ChangedPath
    The observed changed paths, positional and variadic; may be empty.
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $ItemKey,
    [string] $CheckpointPath,
    [string] $ConfigPath,
    [string] $At,
    [string] $ComputedAt,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $ChangedPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'ParallelDrift.psm1') -Force -ErrorAction Stop

$script:DriftDefaultCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'
$script:DriftDefaultConfigPath = 'config/blast-radius.json'
$script:DriftTimestampFormat = 'yyyy-MM-ddTHH-mm'
$script:DriftUsagePrefix = 'parallel drift detection usage error: '
$script:DriftFailurePrefix = 'parallel drift detection failed: '
$script:DriftItemKeyPattern = '^-?[0-9]+$'
$script:DriftIntegerTypeName = @('System.Int16', 'System.Int32', 'System.Int64', 'System.Byte',
    'System.SByte', 'System.UInt16', 'System.UInt32', 'System.UInt64')

function Read-ParallelDriftJsonText {
    <#
    .SYNOPSIS
        Read one document as UTF-8 text; the script's only filesystem read and its test seam.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([string] $Path)

    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
}

function Get-ParallelDriftUtcNow {
    <#
    .SYNOPSIS
        Return the current UTC instant; the script's only clock read and its test seam.
    #>
    [CmdletBinding()]
    [OutputType([datetime])]
    param()

    return [datetime]::UtcNow
}

function ConvertFrom-ParallelDriftJsonElement {
    <#
    .SYNOPSIS
        Convert one System.Text.Json element to a PowerShell value.
    .DESCRIPTION
        Objects become ordinal (case-sensitive) hashtables, arrays become object
        arrays, integral numbers become Int64, other numbers Double, and strings
        stay strings unchanged.
    #>
    # The @{} literal is case-insensitive, and JSON object keys are case-sensitive,
    # so an object is built as a hashtable with the ordinal comparer. The rule
    # below flags every case-sensitive hashtable construction.
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseLiteralInitializerForHashtable', '', Justification = 'JSON object keys are case-sensitive; the @{} literal is case-insensitive, so an ordinal-comparer hashtable is required (issue #763, D1).')]
    [CmdletBinding()]
    [OutputType([hashtable], [System.Object[]], [string], [long], [double], [bool])]
    param([System.Text.Json.JsonElement] $Element)

    # Route by JSON kind; the literal kinds (true, false, null) fall through last.
    switch ($Element.ValueKind) {
        ([System.Text.Json.JsonValueKind]::Object) {
            $table = [hashtable]::new([System.StringComparer]::Ordinal)
            # Convert every property; a repeated name keeps the last value, as Python does.
            foreach ($property in $Element.EnumerateObject()) {
                $table[$property.Name] = ConvertFrom-ParallelDriftJsonElement -Element $property.Value
            }
            return $table
        }
        ([System.Text.Json.JsonValueKind]::Array) {
            $list = [System.Collections.Generic.List[object]]::new()
            # Convert every entry in order; a nested array stays one entry.
            foreach ($entry in $Element.EnumerateArray()) { $list.Add((ConvertFrom-ParallelDriftJsonElement -Element $entry)) }
            return , $list.ToArray()
        }
        ([System.Text.Json.JsonValueKind]::String) { return $Element.GetString() }
        ([System.Text.Json.JsonValueKind]::Number) {
            $integral = [long]0
            if ($Element.TryGetInt64([ref] $integral)) { return $integral }
            return $Element.GetDouble()
        }
        ([System.Text.Json.JsonValueKind]::True) { return $true }
        ([System.Text.Json.JsonValueKind]::False) { return $false }
        default { return $null }
    }
}

function ConvertFrom-ParallelDriftJson {
    <#
    .SYNOPSIS
        Parse JSON text with System.Text.Json into PowerShell values.
    .DESCRIPTION
        An array root is returned with the unary comma so it is not unrolled.
    #>
    [CmdletBinding()]
    [OutputType([hashtable], [System.Object[]], [object])]
    param([string] $Text)

    $document = [System.Text.Json.JsonDocument]::Parse($Text)
    try {
        $value = ConvertFrom-ParallelDriftJsonElement -Element $document.RootElement
    } finally {
        $document.Dispose()
    }
    if ($value -is [array]) { return , $value }
    return $value
}

function Write-ParallelDriftJsonValue {
    <#
    .SYNOPSIS
        Write one value to a JSON writer, sorting object keys ordinally at every depth.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param([System.Text.Json.Utf8JsonWriter] $Writer, [AllowNull()][object] $Value)

    # Route by runtime type. Strings are tested before IEnumerable because a string
    # enumerates its characters, and dictionaries before IEnumerable for the same reason.
    if ($null -eq $Value) { $Writer.WriteNullValue(); return }
    if ($Value -is [bool]) { $Writer.WriteBooleanValue($Value); return }
    if ($Value -is [string]) { $Writer.WriteStringValue($Value); return }
    if ($script:DriftIntegerTypeName -contains $Value.GetType().FullName) { $Writer.WriteNumberValue([long]$Value); return }
    if ($Value -is [double] -or $Value -is [single] -or $Value -is [decimal]) { $Writer.WriteNumberValue([double]$Value); return }
    if ($Value -is [System.Collections.IDictionary]) {
        $name = [System.Collections.Generic.List[string]]::new()
        foreach ($key in $Value.Keys) { $name.Add([string]$key) }
        $name.Sort([System.StringComparer]::Ordinal)
        $Writer.WriteStartObject()
        # Emit the properties in ordinal key order.
        foreach ($key in $name) {
            $Writer.WritePropertyName($key)
            Write-ParallelDriftJsonValue -Writer $Writer -Value $Value[$key]
        }
        $Writer.WriteEndObject()
        return
    }
    if ($Value -is [System.Collections.IEnumerable]) {
        $Writer.WriteStartArray()
        # Emit each entry; a nested array is written as a nested JSON array.
        foreach ($entry in $Value) { Write-ParallelDriftJsonValue -Writer $Writer -Value $entry }
        $Writer.WriteEndArray()
        return
    }
    throw "ConvertTo-ParallelDriftJson cannot serialize a value of type $($Value.GetType().FullName)."
}

function ConvertTo-ParallelDriftJson {
    <#
    .SYNOPSIS
        Serialize a value to indented JSON with ordinally sorted keys; every array stays an array.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()][object] $Value)

    $stream = [System.IO.MemoryStream]::new()
    $options = [System.Text.Json.JsonWriterOptions]@{
        Indented = $true
        Encoder  = [System.Text.Encodings.Web.JavaScriptEncoder]::UnsafeRelaxedJsonEscaping
    }
    $writer = [System.Text.Json.Utf8JsonWriter]::new($stream, $options)
    try {
        Write-ParallelDriftJsonValue -Writer $writer -Value $Value
        $writer.Flush()
        return [System.Text.Encoding]::UTF8.GetString($stream.ToArray())
    } finally {
        $writer.Dispose()
        $stream.Dispose()
    }
}

function Resolve-ParallelDriftPath {
    <#
    .SYNOPSIS
        Return a rooted path unchanged and join a relative path to the current location.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([string] $Path)

    if ([System.IO.Path]::IsPathRooted($Path)) { return $Path }
    return Join-Path -Path (Get-Location).Path -ChildPath $Path
}

function Read-ParallelDriftDocument {
    <#
    .SYNOPSIS
        Read and parse one document and require a JSON object root.
    .DESCRIPTION
        Port of load_mapping; Label names the document in the error message.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.IDictionary])]
    param([string] $Path, [string] $Label)

    $value = ConvertFrom-ParallelDriftJson -Text (Read-ParallelDriftJsonText -Path (Resolve-ParallelDriftPath -Path $Path))
    if ($value -isnot [System.Collections.IDictionary]) { throw "$Label at $Path must be a JSON object." }
    return $value
}

function ConvertTo-ParallelDriftCliOutcome {
    <#
    .SYNOPSIS
        Build the result record the guard writes and exits with.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([int] $ExitCode, [string] $Stdout = '', [string] $Stderr = '')

    return [pscustomobject]@{ ExitCode = $ExitCode; Stdout = $Stdout; Stderr = $Stderr }
}

function Invoke-ParallelDriftCli {
    <#
    .SYNOPSIS
        Validate the arguments, run detection, and return ExitCode, Stdout, and Stderr.
    .DESCRIPTION
        Takes the script's parameters. An omitted ChangedPath (which pwsh -File
        leaves null, and the guard passes by name) is normalized to an empty list
        before any validation, so it yields no_escape rather than a blank entry.
        Usage errors are reported before any file is read; every read or data
        error, and every error the pure functions throw, becomes exit 1 with the
        failure prefix.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([string] $ItemKey, [string] $CheckpointPath, [string] $ConfigPath, [string] $At, [string] $ComputedAt, [string[]] $ChangedPath)

    if ($null -eq $ChangedPath) { $ChangedPath = [string[]]@() }

    # Usage checks come first and exit 2 without reading any file.
    $parsedKey = [long]0
    if ([string]::IsNullOrWhiteSpace($ItemKey)) {
        return ConvertTo-ParallelDriftCliOutcome -ExitCode 2 -Stderr "$($script:DriftUsagePrefix)-ItemKey is required."
    }
    $isInteger = $ItemKey -match $script:DriftItemKeyPattern -and [long]::TryParse($ItemKey, [ref] $parsedKey)
    if (-not $isInteger) {
        return ConvertTo-ParallelDriftCliOutcome -ExitCode 2 -Stderr "$($script:DriftUsagePrefix)-ItemKey must be an integer; got '$ItemKey'."
    }
    $unrecognized = @($ChangedPath | Where-Object { $null -ne $_ -and $_.StartsWith('-') })
    if ($unrecognized.Count -gt 0) {
        return ConvertTo-ParallelDriftCliOutcome -ExitCode 2 -Stderr "$($script:DriftUsagePrefix)unrecognized parameter '$($unrecognized[0])'."
    }

    # Defaults are resolved at this I/O boundary so the pure functions never read a clock.
    if ([string]::IsNullOrEmpty($CheckpointPath)) { $CheckpointPath = $script:DriftDefaultCheckpointPath }
    if ([string]::IsNullOrEmpty($ConfigPath)) { $ConfigPath = $script:DriftDefaultConfigPath }
    if ([string]::IsNullOrEmpty($At)) {
        $At = (Get-ParallelDriftUtcNow).ToString($script:DriftTimestampFormat, [System.Globalization.CultureInfo]::InvariantCulture)
    }
    if ([string]::IsNullOrEmpty($ComputedAt)) { $ComputedAt = $At }

    # One boundary handler for every data defect, so the caller sees one exit code
    # and one stderr line instead of an unhandled error record.
    try {
        $state = Read-ParallelDriftDocument -Path $CheckpointPath -Label 'Parallel checkpoint'
        $config = Read-ParallelDriftDocument -Path $ConfigPath -Label 'Blast-radius config'
        $result = Get-ParallelDriftResult -State $state -Config $config -ItemKey $parsedKey -ChangedPath $ChangedPath -At $At -ComputedAt $ComputedAt
        $json = ConvertTo-ParallelDriftJson -Value $result
    } catch {
        return ConvertTo-ParallelDriftCliOutcome -ExitCode 1 -Stderr "$($script:DriftFailurePrefix)$($_.Exception.Message)"
    }
    return ConvertTo-ParallelDriftCliOutcome -ExitCode 0 -Stdout $json
}

# Dot-sourcing loads the functions for a test host; any other invocation runs
# detection. The six parameters are passed by name rather than by splatting
# $PSBoundParameters, which under CmdletBinding can carry common parameters.
if ($MyInvocation.InvocationName -ne '.') {
    $outcome = Invoke-ParallelDriftCli -ItemKey $ItemKey -CheckpointPath $CheckpointPath -ConfigPath $ConfigPath `
        -At $At -ComputedAt $ComputedAt -ChangedPath $ChangedPath
    if (-not [string]::IsNullOrEmpty($outcome.Stdout)) { [Console]::Out.WriteLine($outcome.Stdout) }
    if (-not [string]::IsNullOrEmpty($outcome.Stderr)) { [Console]::Error.WriteLine($outcome.Stderr) }
    exit $outcome.ExitCode
}
