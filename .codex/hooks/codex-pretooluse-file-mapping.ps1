<#
.SYNOPSIS
    Shared Codex PreToolUse transport parsing and tool_input-to-file-edit mapping.

.DESCRIPTION
    Dot-sourced by every hook registered under the .codex/config.toml matcher
    ^(apply_patch|Edit|Write)$. It provides the two transport concerns those
    hooks previously duplicated with drift:

      - ConvertFrom-CodexPreToolUsePayload: validates and parses the single JSON
        object a Codex PreToolUse hook receives on stdin. It throws only for
        genuinely un-processable input, and every message it throws begins with
        the caller-supplied hook name so each handler's stderr names itself.

      - ConvertTo-CodexFileEditInput: maps a parsed payload to zero or more
        file-edit records for the admitted tool names apply_patch, Edit, and
        Write. Well-formed input that names nothing the caller governs maps to an
        EMPTY array, which callers translate to allow (exit 0, no stdout) rather
        than to a transport failure.

    Separating these concerns is required because enforce-checkpoint-monotonic.ps1
    and enforce-completion-consistency.ps1 are already near the 500-line cap and
    cannot absorb inline mapping logic.

    PUBLIC SURFACE. Consuming hooks call exactly the two functions named above
    and nothing else. ConvertTo-CodexAddedLineText, Test-CodexGovernedPath, and
    Resolve-CodexUpdatedFileContent are INTERNAL helpers of
    ConvertTo-CodexFileEditInput; they exist only because the repository's code
    standards require long branching logic to be factored into small, focused
    functions rather than inlined, and no hook calls them directly.

    This script performs no policy evaluation. Each consuming hook keeps its own
    allow/deny policy functions unchanged and applies them to the records
    returned here.

.NOTES
    Compatible with PowerShell 7+.

    Entrypoint-free by design: this file contains only script-scoped constants
    and function definitions, so dot-sourcing it has no side effects and no
    exit-code semantics. The precedent is enforce-completion-helpers.ps1, which
    is likewise dot-sourced and carries no entrypoint. Consequently this file
    never reads standard input and never reads any legacy Claude environment
    variable; reading the payload is the calling hook's responsibility, and the
    Pester parity suite asserts that no legacy environment read appears here.
#>
[CmdletBinding()]
param()

# Tool names the ^(apply_patch|Edit|Write)$ PreToolUse matcher admits. Any other
# well-formed tool name maps to no records, which the caller treats as allow.
$script:CodexAdmittedToolNames = @('apply_patch', 'Edit', 'Write')

# Splits an apply_patch command into one match per touched file. The lookahead
# terminates each file body at the next file marker or at '*** End Patch', so a
# multi-file patch yields one match per file. Lifted verbatim from the
# per-handler implementations this module replaces.
$script:CodexApplyPatchFileRegex = '(?ms)^\*\*\* (?<operation>Add|Update|Delete) File:\s*(?<path>.+?)\r?\n(?<body>.*?)(?=^\*\*\* (?:(?:Add|Update|Delete) File:|End Patch)\s*|\z)'

# Locates a rename destination inside a single file body.
$script:CodexApplyPatchMoveRegex = '(?m)^\*\*\* Move to:\s*(?<path>.+?)\s*$'

# Splits an apply_patch Update body into hunks on its @@ headers.
$script:CodexApplyPatchHunkRegex = '(?m)^@@[^\r\n]*\r?\n'

function ConvertFrom-CodexPreToolUsePayload {
    <#
    .SYNOPSIS
        Parses and validates one Codex PreToolUse stdin payload.

    .DESCRIPTION
        Converts the raw JSON string a PreToolUse hook read from stdin into an
        object, throwing only when the input cannot be processed at all. The
        caller converts a throw into exit 2 with the message on stderr.

        This function deliberately performs NO tool-name assertion. Admission is
        the matcher's job in .codex/config.toml, and narrowing it here is the
        defect that made every Edit and Write payload exit 2. Tool-name routing
        belongs to ConvertTo-CodexFileEditInput, which maps unadmitted names to
        no records so the caller allows.

    .PARAMETER PayloadRaw
        The raw stdin text. Empty, whitespace-only, and null are all rejected.
        AllowEmptyString is required so the explicit, hook-named error below runs
        instead of a generic parameter-binding failure whose message would not
        name the hook.

    .PARAMETER HookName
        The calling hook's name. Every thrown message begins with this value so
        a handler that shares this module still reports itself in its stderr.

    .PARAMETER RequireSessionId
        When present, a missing, empty, or whitespace-only session_id is also
        rejected. Only the batch-budget hooks need this, because they key
        per-session state by session_id.

    .OUTPUTS
        The parsed payload object.

    .NOTES
        Throws for exactly four conditions and nothing else: empty or
        whitespace-only input, invalid JSON, missing or null tool_input, and
        (only under -RequireSessionId) a missing or empty session_id.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $PayloadRaw,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $HookName,

        [Parameter()]
        [switch] $RequireSessionId
    )

    if ([string]::IsNullOrWhiteSpace($PayloadRaw)) {
        throw "$HookName hook input is empty."
    }

    try {
        $payload = $PayloadRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "$HookName hook input is malformed JSON: $_"
    }

    # A JSON literal such as 'null', a bare string, or a number parses without
    # error but carries no tool_input, so both cases fail through one check.
    if ($null -eq $payload -or
        $payload.PSObject.Properties.Name -notcontains 'tool_input' -or
        $null -eq $payload.tool_input) {
        throw "$HookName hook input is missing tool_input."
    }

    if ($RequireSessionId -and [string]::IsNullOrWhiteSpace([string]$payload.session_id)) {
        throw "$HookName hook input is missing session_id."
    }

    return $payload
}

function ConvertTo-CodexFileEditInput {
    <#
    .SYNOPSIS
        Maps a parsed Codex PreToolUse payload to zero or more file-edit records.

    .DESCRIPTION
        Admits the tool names apply_patch, Edit, and Write. Every other
        well-formed tool name, and every admitted tool name whose tool_input
        names no file, maps to an EMPTY array so the caller allows.

        Each emitted record carries:

          file_path   - the path the operation ultimately affects; for an
                        apply_patch rename this is the '*** Move to:'
                        destination, matching the pre-fix adapters.
          source_path - the path the operation started from; equal to file_path
                        except for a rename.
          operation   - Add, Update, Delete, Edit, or Write.
          content, old_string, new_string - whichever fields the operation has.

        Both path properties are emitted because the consuming policies split on
        this point: enforce-evidence-locations and the two batch-budget hooks
        evaluated BOTH sides of a rename before this refactor, while the purity
        and checkpoint hooks evaluated only the resulting file. Carrying both
        keeps every consuming policy's inputs identical to the pre-fix
        behaviour.

        Direct-mapped tool_input (anything carrying an explicit file_path) is
        copied property-for-property, because the pre-fix adapters returned the
        raw tool_input object and downstream policies read fields such as
        content, old_string, and new_string from it.

    .PARAMETER Payload
        The object returned by ConvertFrom-CodexPreToolUsePayload.

    .PARAMETER ResolveUpdateContent
        When present, an apply_patch Update is reconstructed against the on-disk
        source file so the caller sees complete post-patch content. Only the two
        checkpoint hooks need this, and it applies ONLY to paths matching
        -GovernedPath.

    .PARAMETER GovernedPath
        The repository-relative path the caller governs, matched with the same
        '(^|/)<path>$' anchoring the checkpoint hooks use. When
        -ResolveUpdateContent is supplied, an Update touching a path outside this
        value yields NO record at all, so a patch that merely happens to touch
        other files allows instead of failing. That closes the latent defect in
        which any unreadable update source made the hook exit 2 even when it
        governed none of the patched files.

    .OUTPUTS
        An array of file-edit records; empty when nothing maps.

    .NOTES
        Reconstruction failure for a GOVERNED path yields a record with empty
        content rather than a throw. Empty content is the signal each checkpoint
        policy already treats as an un-validatable checkpoint and fails closed
        on, so a governed reconstruction failure stays a deny and never becomes
        exit 2.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $Payload,

        [Parameter()]
        [switch] $ResolveUpdateContent,

        [Parameter()]
        [AllowEmptyString()]
        [string] $GovernedPath = ''
    )

    $records = [System.Collections.Generic.List[object]]::new()

    if ($null -eq $Payload -or $null -eq $Payload.tool_input) {
        return $records.ToArray()
    }

    # Admission is by tool name only. An unadmitted name is not an error: the
    # caller allows, because the matcher decides what reaches the hook.
    $toolName = [string]$Payload.tool_name
    if ($script:CodexAdmittedToolNames -notcontains $toolName) {
        return $records.ToArray()
    }

    $toolInput = $Payload.tool_input
    $toolInputProperties = @($toolInput.PSObject.Properties.Name)

    # Direct mapping first: any tool_input carrying a usable file_path maps
    # without patch parsing, which is how every pre-fix adapter behaved for
    # Edit-shaped and Write-shaped input regardless of tool name.
    if ($toolInputProperties -contains 'file_path') {
        $filePath = [string]$toolInput.file_path
        if ([string]::IsNullOrWhiteSpace($filePath)) {
            return $records.ToArray()
        }

        # Copy every supplied field so downstream policies keep reading the same
        # properties they read from the raw tool_input before this refactor.
        $record = [ordered]@{}
        foreach ($property in $toolInput.PSObject.Properties) {
            $record[$property.Name] = $property.Value
        }
        $record['file_path'] = $filePath
        $record['source_path'] = $filePath
        $record['operation'] = $toolName

        $records.Add([pscustomobject]$record)
        return $records.ToArray()
    }

    if ($toolInputProperties -notcontains 'command') {
        return $records.ToArray()
    }

    $command = [string]$toolInput.command
    if ([string]::IsNullOrWhiteSpace($command)) {
        return $records.ToArray()
    }

    $fileMatches = [regex]::Matches($command, $script:CodexApplyPatchFileRegex)
    if ($fileMatches.Count -eq 0) {
        return $records.ToArray()
    }

    # Build one record per file the patch touches, resolving rename destinations
    # and per-operation content so each consuming policy receives the same shape
    # its own adapter produced before extraction.
    foreach ($fileMatch in $fileMatches) {
        $sourcePath = ([string]$fileMatch.Groups['path'].Value).Trim()
        $operation = [string]$fileMatch.Groups['operation'].Value
        $body = ([string]$fileMatch.Groups['body'].Value) -replace '\r\n', "`n"

        $targetPath = $sourcePath
        $moveMatch = [regex]::Match($body, $script:CodexApplyPatchMoveRegex)
        if ($moveMatch.Success) {
            $targetPath = ([string]$moveMatch.Groups['path'].Value).Trim()
        }

        # Content derivation is per operation. Delete yields empty content, which
        # is the deletion signal the checkpoint policies fail closed on. Add
        # yields the added lines. Update yields the added lines unless the caller
        # asked for on-disk reconstruction.
        $content = ''
        if ($operation -eq 'Add') {
            $content = ConvertTo-CodexAddedLineText -Body $body
        } elseif ($operation -eq 'Update') {
            if (-not $ResolveUpdateContent) {
                $content = ConvertTo-CodexAddedLineText -Body $body
            } else {
                $isGoverned = (Test-CodexGovernedPath -Path $sourcePath -GovernedPath $GovernedPath) -or
                (Test-CodexGovernedPath -Path $targetPath -GovernedPath $GovernedPath)
                if (-not $isGoverned) {
                    # Ungoverned update: emit nothing so the caller allows instead
                    # of reading an unrelated file it does not govern.
                    continue
                }
                $content = Resolve-CodexUpdatedFileContent -SourcePath $sourcePath -Body $body
            }
        }

        $records.Add([pscustomobject]@{
                file_path   = $targetPath
                source_path = $sourcePath
                operation   = $operation
                content     = $content
            })
    }

    return $records.ToArray()
}

function ConvertTo-CodexAddedLineText {
    <#
    .SYNOPSIS
        INTERNAL. Returns the added ('+') lines of an apply_patch file body.

    .PARAMETER Body
        The LF-normalized body of one apply_patch file section.

    .OUTPUTS
        The added lines joined with LF, or an empty string when there are none.

    .NOTES
        '+++' lines are unified-diff headers, not content, and are excluded. LF
        is the join character because the checkpoint policies compare against
        LF-normalized file text and the purity patterns are newline-agnostic.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Body
    )

    # Collect the inserted lines in order, dropping the '+' marker itself.
    $addedLines = foreach ($line in ($Body -split "`n")) {
        if ($line.StartsWith('+') -and -not $line.StartsWith('+++')) {
            $line.Substring(1)
        }
    }

    return ($addedLines -join "`n")
}

function Test-CodexGovernedPath {
    <#
    .SYNOPSIS
        INTERNAL. Returns $true when a patched path is the caller's governed path.

    .PARAMETER Path
        The path taken from an apply_patch file marker.

    .PARAMETER GovernedPath
        The repository-relative path the caller governs; empty governs nothing.

    .OUTPUTS
        $true when the normalized path equals the governed path or ends with it
        on a segment boundary. Uses the same '(^|/)<path>$' anchoring as the
        checkpoint hooks' Test-IsCheckpointPath, so absolute and relative forms
        both match.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Path,

        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $GovernedPath
    )

    if ([string]::IsNullOrWhiteSpace($GovernedPath) -or [string]::IsNullOrWhiteSpace($Path)) {
        return $false
    }

    $normalizedPath = $Path -replace '\\', '/'
    $normalizedGoverned = ($GovernedPath -replace '\\', '/').Trim('/')
    return ($normalizedPath -match "(^|/)$([regex]::Escape($normalizedGoverned))$")
}

function Resolve-CodexUpdatedFileContent {
    <#
    .SYNOPSIS
        INTERNAL. Reconstructs post-patch content for a governed Update.

    .DESCRIPTION
        Reads the source file, then applies each hunk in memory. Nothing is
        written to disk. Returns an empty string when the source cannot be read
        or a hunk does not apply, because empty content is the signal the
        governed policies already fail closed on. This function never throws, so
        a governed reconstruction failure becomes a deny rather than exit 2.

    .PARAMETER SourcePath
        The path to read the pre-patch content from.

    .PARAMETER Body
        The LF-normalized body of the Update file section.

    .OUTPUTS
        The reconstructed content, or an empty string on any failure.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $SourcePath,

        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Body
    )

    if (-not (Test-Path -LiteralPath $SourcePath -PathType Leaf)) {
        return ''
    }

    try {
        $content = (Get-Content -Raw -LiteralPath $SourcePath) -replace '\r\n', "`n"
    } catch {
        Write-Verbose "Unable to read update source '$SourcePath': $($_.Exception.Message)"
        return ''
    }

    $hunks = @([regex]::Split($Body, $script:CodexApplyPatchHunkRegex) | Where-Object { $_ -match '\S' })

    # Apply each hunk by locating its pre-image and substituting its post-image.
    # A hunk whose pre-image is absent means the patch does not apply to this
    # source, so reconstruction fails closed with empty content.
    foreach ($hunk in $hunks) {
        $oldLines = [System.Collections.Generic.List[string]]::new()
        $newLines = [System.Collections.Generic.List[string]]::new()

        # Classify each hunk line into the pre-image, the post-image, or both.
        foreach ($line in ($hunk -split "`n")) {
            if ($line -match '^\*\*\* (?:Move to|End of File)') {
                continue
            }
            if ($line.StartsWith('+') -and -not $line.StartsWith('+++')) {
                $newLines.Add($line.Substring(1))
            } elseif ($line.StartsWith('-') -and -not $line.StartsWith('---')) {
                $oldLines.Add($line.Substring(1))
            } elseif ($line.StartsWith(' ')) {
                $oldLines.Add($line.Substring(1))
                $newLines.Add($line.Substring(1))
            } else {
                $oldLines.Add($line)
                $newLines.Add($line)
            }
        }

        $oldText = $oldLines -join "`n"
        $newText = $newLines -join "`n"
        $index = $content.IndexOf($oldText, [System.StringComparison]::Ordinal)
        if ($index -lt 0) {
            return ''
        }
        $content = $content.Substring(0, $index) + $newText + $content.Substring($index + $oldText.Length)
    }

    return $content
}
