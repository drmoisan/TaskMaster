<#
.SYNOPSIS
    Pre-tool-use hook for Claude Code that blocks Mermaid diagrams with named structural defects.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Write or Edit
    operation. It reads the tool input from the CLAUDE_TOOL_INPUT environment variable
    (JSON with 'file_path' plus 'content' for Write, or 'old_string'/'new_string' for
    Edit) and applies two independent gates:

      1. Syntax gate. On a Write of a '.mmd'/'.mermaid' file, the whole file is one
         diagram. On a Write of a Markdown file, every fenced ```mermaid block is a
         diagram. Each diagram is validated by .claude/lib/mermaid/MermaidValidation.psm1
         and a defect of a checked class produces a deny naming the class and the line.
      2. Managed-diagram gate. A '.mmd'/'.mermaid' file whose ON-DISK frontmatter carries
         'id:' is connected to the Mermaid Chart sync workflow and must not be hand
         edited. This is a property of the target file rather than of the payload, so it
         applies to Edit as well as Write, and the opt-out marker never suppresses it.

    The gate's contract is "rejects the named defect classes", never "proves validity".
    Blocking a valid diagram is worse than missing an invalid one, so the hook declines
    to judge rather than rejecting whenever it cannot classify content confidently:

      - empty, absent, or unparseable CLAUDE_TOOL_INPUT: allow;
      - missing 'file_path', or a path outside the '.mmd'/'.mermaid'/Markdown scope: allow;
      - the validation module absent from disk: allow;
      - an Edit payload (the syntax check needs the whole file, which an
        old_string/new_string fragment cannot supply): allow;
      - a Markdown file carrying no ```mermaid fence: allow;
      - a ```mermaid fence nested inside another open fence, which is documentation
        showing example Mermaid rather than a diagram: skip that block;
      - a fence immediately preceded by '<!-- mermaid-validator: ignore -->': skip that
        block, and only that block.

    DELIBERATE DIVERGENCE FROM enforce-evidence-locations.ps1: that hook throws on
    malformed CLAUDE_TOOL_INPUT JSON and its entry point exits 1. This hook allows
    instead. The difference is intentional and must not be "fixed" into a hard failure:
    a content gate that hard-fails on input it cannot parse converts an unparseable
    payload into a blocked write, which is the false-positive failure mode this feature
    exists to avoid. The evidence-location hook gates a path, which is always parseable
    when present; this hook gates content, which is not.

    The extension scope check runs before any content scan, so a write outside the
    Mermaid scope pays only the JSON parse.

.NOTES
    Compatible with PowerShell 7+.
    This script must not modify any state; it is a read-only validation gate.
    It emits compact hookSpecificOutput JSON on stdout and exits 0 in every case,
    never a non-zero exit and never the {"decision":"block"} shape.
    It invokes no Python and starts no subprocess.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest

$script:MermaidModulePath = Join-Path -Path $PSScriptRoot -ChildPath '../lib/mermaid/MermaidValidation.psm1'
$script:MermaidSkillPointer = 'See .claude/skills/mermaid-diagram/SKILL.md.'
$script:MermaidSyncPointer = 'Change it through the Mermaid Chart sync workflow in VS Code (Mermaid Chart extension: Sync Diagram with Mermaid, then Review Mermaid Sync) and pull the synced result instead of hand-editing. See .claude/rules/mermaid.md.'

function Import-MermaidValidationModule {
    <#
    .SYNOPSIS
        Imports the validation module, returning $false when it is absent.
    .DESCRIPTION
        A consumer repository that receives this hook without the library must not be
        bricked, so a missing module fails open rather than throwing.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()

    if (-not (Test-Path -LiteralPath $script:MermaidModulePath -PathType Leaf)) { return $false }

    try {
        Import-Module -Name $script:MermaidModulePath -Force -ErrorAction Stop
    } catch {
        return $false
    }

    return $true
}

function Get-MermaidOnDiskContent {
    <#
    .SYNOPSIS
        Reads the current on-disk content of a target file, or $null when unavailable.
    .DESCRIPTION
        The named wrapper seam for the managed-diagram gate. Pester mocks this function
        rather than the filesystem, so no test needs a temporary file.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) { return $null }
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }

    try {
        return [string](Get-Content -LiteralPath $Path -Raw -ErrorAction Stop)
    } catch {
        return $null
    }
}

function Get-MermaidToolInputField {
    <#
    .SYNOPSIS
        Reads one field from the parsed tool input, or $null when it is absent.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $InputObject,

        [Parameter(Mandatory)]
        [string] $Name
    )

    if ($null -eq $InputObject) { return $null }

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) { return $null }

    return $property.Value
}

function Test-MermaidDiagramFilePath {
    <#
    .SYNOPSIS
        Returns $true when the path names a standalone Mermaid diagram file.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $FilePath
    )

    $normalized = $FilePath -replace '\\', '/'
    return [bool]($normalized -imatch '\.(mmd|mermaid)$')
}

function Test-MermaidMarkdownFilePath {
    <#
    .SYNOPSIS
        Returns $true when the path names a Markdown file that may carry a fence.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $FilePath
    )

    $normalized = $FilePath -replace '\\', '/'
    return [bool]($normalized -imatch '\.(md|markdown)$')
}

function Get-MermaidAllowDecision {
    <#
    .SYNOPSIS
        Builds the explicit-allow decision.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param()

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName      = 'PreToolUse'
            permissionDecision = 'allow'
        }
    }
}

function Get-MermaidDenyDecision {
    <#
    .SYNOPSIS
        Builds a deny decision carrying the supplied token-prefixed reason.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Get-MermaidValidationBlockedReason {
    <#
    .SYNOPSIS
        Formats the syntax-deny reason from a structured validation result.
    .DESCRIPTION
        The reason names the defect class, the line number, and the corrective pointer,
        because a deny a reader cannot act on is indistinguishable from a broken gate.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath,

        [Parameter(Mandatory)]
        $Result,

        [AllowEmptyString()]
        [string] $Location = ''
    )

    $finding = @($Result.Findings)[0]
    $declared = if ([string]::IsNullOrWhiteSpace([string]$Result.DiagramType)) { 'no diagram type' } else { "'$($Result.DiagramType)'" }
    $where = if ([string]::IsNullOrWhiteSpace($Location)) { '' } else { " ($Location)" }

    return "MERMAID_VALIDATION_BLOCKED: '$FilePath'$where declares $declared and has a Mermaid syntax defect: $($finding.Class) at line $($finding.Line): $($finding.Message). $script:MermaidSkillPointer"
}

function Get-MermaidManagedDiagramBlockedReason {
    <#
    .SYNOPSIS
        Formats the managed-diagram deny reason.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath
    )

    return "MERMAID_MANAGED_DIAGRAM_BLOCKED: '$FilePath' is a Mermaid Chart-managed diagram: its on-disk frontmatter carries an 'id:' marker, so a hand-edit would be overwritten by the next sync. $script:MermaidSyncPointer"
}

function Get-MermaidMarkdownBlockDecision {
    <#
    .SYNOPSIS
        Validates every eligible fenced block of a Markdown payload.
    .DESCRIPTION
        Returns a deny decision for the first block carrying a defect, or $null when
        every block is either accepted or skipped. A nested block is documentation
        showing example Mermaid; an opted-out block carries the documented marker.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath,

        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Content
    )

    foreach ($block in @(Get-MermaidFenceBlock -Content $Content)) {
        if ($block.IsNested -or $block.IsOptedOut) { continue }

        $result = Test-MermaidDiagram -Content $block.Content -LineOffset ($block.BodyStartLine - 1)
        if ($result.Verdict -ne 'Invalid') { continue }

        $location = "the mermaid fence opening at line $($block.StartLine)"
        return Get-MermaidDenyDecision -Reason (Get-MermaidValidationBlockedReason -FilePath $FilePath -Result $result -Location $location)
    }

    return $null
}

function Invoke-MermaidValidationDecision {
    <#
    .SYNOPSIS
        Parses the Claude Code tool-input JSON and returns the allow-or-deny decision.
    .DESCRIPTION
        The pure decision function, separated from the thin entry point so Pester
        exercises the logic directly. Returns $null when the call is none of this hook's
        business (out of scope, unparseable, or a Markdown file with no fence), which the
        entry point treats as a silent allow.
    .PARAMETER ToolInputRaw
        The raw JSON string from $env:CLAUDE_TOOL_INPUT.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $ToolInputRaw
    )

    if ([string]::IsNullOrWhiteSpace($ToolInputRaw)) { return $null }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        # Fail open. See the DELIBERATE DIVERGENCE note in this file's header before
        # changing this to a throw.
        return $null
    }

    $filePath = [string](Get-MermaidToolInputField -InputObject $toolInput -Name 'file_path')
    if ([string]::IsNullOrWhiteSpace($filePath)) { return $null }

    # Scope check first: a write outside the Mermaid scope pays only the JSON parse.
    $isDiagramFile = Test-MermaidDiagramFilePath -FilePath $filePath
    $isMarkdownFile = Test-MermaidMarkdownFilePath -FilePath $filePath
    if (-not ($isDiagramFile -or $isMarkdownFile)) { return $null }

    if (-not (Import-MermaidValidationModule)) { return $null }

    # Managed-diagram gate: a property of the target file, so Edit is covered without
    # reconstructing the post-edit content, and the opt-out marker cannot suppress it.
    if ($isDiagramFile) {
        $onDisk = Get-MermaidOnDiskContent -Path $filePath
        if (-not [string]::IsNullOrWhiteSpace([string]$onDisk) -and (Test-MermaidManagedDiagram -Content ([string]$onDisk))) {
            return Get-MermaidDenyDecision -Reason (Get-MermaidManagedDiagramBlockedReason -FilePath $filePath)
        }
    }

    $content = Get-MermaidToolInputField -InputObject $toolInput -Name 'content'
    if ($null -eq $content) {
        # Edit payload: old_string/new_string is a fragment, not the resulting file, so
        # the syntax check cannot run. The next Write catches a regression.
        return Get-MermaidAllowDecision
    }

    if ($isDiagramFile) {
        $result = Test-MermaidDiagram -Content ([string]$content)
        if ($result.Verdict -eq 'Invalid') {
            return Get-MermaidDenyDecision -Reason (Get-MermaidValidationBlockedReason -FilePath $filePath -Result $result)
        }

        return Get-MermaidAllowDecision
    }

    $blocks = @(Get-MermaidFenceBlock -Content ([string]$content))
    if ($blocks.Count -eq 0) { return $null }

    $decision = Get-MermaidMarkdownBlockDecision -FilePath $filePath -Content ([string]$content)
    if ($null -ne $decision) { return $decision }

    return Get-MermaidAllowDecision
}

function Invoke-MermaidValidationEntryPoint {
    <#
    .SYNOPSIS
        Thin entry point: writes the decision JSON to stdout and nothing else.
    .DESCRIPTION
        The function emits only the JSON, and the caller exits 0 unconditionally. An
        entry point that also returned a status code would place that code in the same
        output stream as the JSON, so the caller would consume the JSON instead of
        printing it and the decision would never reach Claude Code.
    .PARAMETER ToolInputRaw
        The raw JSON string from $env:CLAUDE_TOOL_INPUT.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $ToolInputRaw = $env:CLAUDE_TOOL_INPUT
    )

    $decision = Invoke-MermaidValidationDecision -ToolInputRaw $ToolInputRaw
    if ($null -eq $decision) { return }

    $decision | ConvertTo-Json -Compress -Depth 5
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

Invoke-MermaidValidationEntryPoint -ToolInputRaw $env:CLAUDE_TOOL_INPUT

# Exit 0 on allow and on deny alike: the decision travels in the JSON, never in the
# exit code.
exit 0
