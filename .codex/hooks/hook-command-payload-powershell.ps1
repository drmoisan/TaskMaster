<#
.SYNOPSIS
    PowerShell payload extraction and AST adapter for the payload-aware command iterator.
.DESCRIPTION
    Issue #824. Reads the arguments of a pwsh or powershell invocation, extracts the
    -Command / -CommandWithArgs payload or decodes the -EncodedCommand payload (base64
    UTF-16LE), and turns a PowerShell payload into iterator records by parsing it with
    [System.Management.Automation.Language.Parser]::ParseInput. Parsing never executes the
    payload. A parse error yields one Unbalanced record; a decode failure is reported to the
    caller, which marks the wrapper record Opaque with reason DecodeFailure.

    Pure string logic only: no disk, process, network, clock, or environment access. It is
    dot-sourced by hook-command-invocation.ps1, after hook-command-payload.ps1, whose record
    builder and wrapper expansion it calls.
#>

# Host flags consumed before the command form (design item 4). The standalone flags take no
# value; the value flags take exactly one following token.
$script:CommandLinePowerShellStandaloneFlags = @(
    '-noprofile', '-nop', '-nologo', '-nol', '-noninteractive', '-noni', '-noexit', '-noe',
    '-login', '-l', '-mta', '-sta'
)
$script:CommandLinePowerShellValueFlags = @(
    '-executionpolicy', '-ep', '-ex', '-workingdirectory', '-wd', '-windowstyle', '-w',
    '-inputformat', '-if', '-outputformat', '-of', '-configurationname'
)

function ConvertFrom-CommandLineEncodedCommand {
    <#
    .SYNOPSIS
        Decode an -EncodedCommand argument (base64 of UTF-16LE text).
    .DESCRIPTION
        Returns $null when the argument is not valid base64 or does not decode to whole
        UTF-16 code units; the caller reports that as DecodeFailure.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $EncodedText)

    try {
        $bytes = [System.Convert]::FromBase64String($EncodedText)
    } catch [System.FormatException] {
        return $null
    }
    if ($bytes.Length -eq 0 -or ($bytes.Length % 2) -ne 0) { return $null }
    return [System.Text.Encoding]::Unicode.GetString($bytes)
}

function Get-CommandLinePowerShellPayload {
    <#
    .SYNOPSIS
        Extract the payload from the arguments that follow a pwsh or powershell word.
    .DESCRIPTION
        Consumes the modeled host flags case-insensitively, then reads -c, -cwa,
        -commandwithargs, any prefix of -command of length 4 or more, -e, -ec, or any prefix
        of -encodedcommand of length 3 or more. Any other dash token, a non-dash token, or a
        missing payload yields Opaque with reason NoPayload; an undecodable encoded payload
        yields Opaque with reason DecodeFailure.
    .OUTPUTS
        System.Management.Automation.PSCustomObject with Payload, Opaque, OpaqueReason.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Word)

    $index = 0
    while ($index -lt $Word.Count -and $Word[$index].StartsWith('-')) {
        $flag = $Word[$index].ToLowerInvariant()
        $remaining = [string[]]@($Word | Select-Object -Skip ($index + 1))
        if ($script:CommandLinePowerShellStandaloneFlags -contains $flag) { $index++; continue }
        if ($script:CommandLinePowerShellValueFlags -contains $flag) { $index += 2; continue }

        $payload = $null
        if ($flag -eq '-cwa' -or $flag -eq '-commandwithargs') {
            if ($remaining.Count -gt 0) { $payload = $remaining[0] }
        } elseif ($flag -eq '-c' -or ($flag.Length -ge 4 -and '-command'.StartsWith($flag))) {
            if ($remaining.Count -gt 0) { $payload = $remaining -join ' ' }
        } elseif ($flag -eq '-e' -or $flag -eq '-ec' -or ($flag.Length -ge 3 -and '-encodedcommand'.StartsWith($flag))) {
            if ($remaining.Count -gt 0) {
                $decoded = ConvertFrom-CommandLineEncodedCommand -EncodedText $remaining[0]
                if ($null -eq $decoded) {
                    return [pscustomobject]@{ Payload = ''; Opaque = $true; OpaqueReason = 'DecodeFailure' }
                }
                $payload = $decoded
            }
        }
        if (-not [string]::IsNullOrWhiteSpace($payload)) {
            return [pscustomobject]@{ Payload = $payload; Opaque = $false; OpaqueReason = '' }
        }
        break
    }
    return [pscustomobject]@{ Payload = ''; Opaque = $true; OpaqueReason = 'NoPayload' }
}

function Get-CommandLinePowerShellNode {
    <#
    .SYNOPSIS
        Return the command and non-command nodes of a parsed PowerShell payload in source order.
    .DESCRIPTION
        Every CommandAst anywhere in the tree, every top-level statement that is not a
        pipeline made only of CommandAst elements, and every CommandExpressionAst pipeline
        element. Sorted by start offset; a non-command node sorts before a command node at the
        same offset, and duplicate non-command extents are dropped.
    .OUTPUTS
        System.Management.Automation.PSCustomObject[] with Node and IsCommand.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject[]])]
    param([Parameter(Mandatory)][System.Management.Automation.Language.ScriptBlockAst] $Ast)

    $items = [System.Collections.Generic.List[pscustomobject]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new()
    $isTopLevel = {
        param($node)
        $node -is [System.Management.Automation.Language.StatementAst] -and
        $node.Parent -is [System.Management.Automation.Language.NamedBlockAst] -and
        $node.Parent.Parent -is [System.Management.Automation.Language.ScriptBlockAst] -and
        $null -eq $node.Parent.Parent.Parent
    }
    foreach ($statement in $Ast.FindAll($isTopLevel, $true)) {
        $pureCommand = $statement -is [System.Management.Automation.Language.PipelineAst] -and
        @($statement.PipelineElements | Where-Object { $_ -isnot [System.Management.Automation.Language.CommandAst] }).Count -eq 0
        if (-not $pureCommand -and $seen.Add("$($statement.Extent.StartOffset):$($statement.Extent.EndOffset)")) {
            $items.Add([pscustomobject]@{ Node = $statement; IsCommand = $false })
        }
    }
    $isExpressionElement = { param($node) $node -is [System.Management.Automation.Language.CommandExpressionAst] -and $node.Parent -is [System.Management.Automation.Language.PipelineAst] }
    foreach ($expression in $Ast.FindAll($isExpressionElement, $true)) {
        if ($seen.Add("$($expression.Extent.StartOffset):$($expression.Extent.EndOffset)")) {
            $items.Add([pscustomobject]@{ Node = $expression; IsCommand = $false })
        }
    }
    foreach ($command in $Ast.FindAll({ param($node) $node -is [System.Management.Automation.Language.CommandAst] }, $true)) {
        $items.Add([pscustomobject]@{ Node = $command; IsCommand = $true })
    }
    return [pscustomobject[]]@($items | Sort-Object -Property @{ Expression = { $_.Node.Extent.StartOffset } }, @{ Expression = { $_.IsCommand } })
}

function Add-CommandLinePowerShellRecord {
    <#
    .SYNOPSIS
        Parse a PowerShell payload and append one iterator record per command and non-command node.
    .DESCRIPTION
        A CommandAst record's tokens are the element values: the Value of a string constant,
        the text of a parameter, and the extent text of anything else. The record is Literal
        only when every element is a string constant or a parameter without an argument and
        the command carries no redirection. A CommandAst whose name is itself a wrapper is
        expanded in turn. A parse error yields a single Unbalanced record carrying the payload.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][hashtable] $Context,
        [Parameter(Mandatory)][AllowEmptyString()][string] $Text,
        [Parameter(Mandatory)][int] $Depth,
        [Parameter(Mandatory)][AllowEmptyString()][string] $Wrapper,
        [Parameter(Mandatory)][int] $RootIndex,
        [Parameter(Mandatory)][AllowEmptyString()][string] $RootRawText,
        [Parameter(Mandatory)][bool] $ArgumentInjected
    )

    $common = @{
        Context = $Context; Depth = $Depth; Origin = 'WrapperPayload'; Wrapper = $Wrapper; Dialect = 'PowerShell'
        RootIndex = $RootIndex; RootRawText = $RootRawText; Delimiter = ''; ArgumentInjected = $ArgumentInjected
    }
    $parseTokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseInput($Text, [ref]$parseTokens, [ref]$parseErrors)
    if (@($parseErrors).Count -gt 0) {
        $null = Add-CommandLineInvocationRecord @common -RawText $Text -Tokens @() -CommandWord '' -Literal $false -Unbalanced $true
        return
    }

    foreach ($item in @(Get-CommandLinePowerShellNode -Ast $ast)) {
        if (-not $item.IsCommand) {
            $null = Add-CommandLineInvocationRecord @common -RawText $item.Node.Extent.Text -Tokens @() -CommandWord '' -Literal $false -Unbalanced $false
            continue
        }
        $command = $item.Node
        $tokens = [System.Collections.Generic.List[string]]::new()
        $literal = $command.Redirections.Count -eq 0
        foreach ($element in $command.CommandElements) {
            if ($element -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                $tokens.Add($element.Value)
            } elseif ($element -is [System.Management.Automation.Language.CommandParameterAst]) {
                $tokens.Add($element.Extent.Text)
                if ($null -ne $element.Argument) { $literal = $false }
            } else {
                $tokens.Add($element.Extent.Text)
                $literal = $false
            }
        }
        $nameIsConstant = $command.CommandElements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst]
        $commandWord = if ($nameIsConstant) { ConvertTo-CommandLineLeafWord -Word $tokens[0] } else { $tokens[0] }
        $record = Add-CommandLineInvocationRecord @common -RawText $command.Extent.Text -Tokens $tokens.ToArray() -CommandWord $commandWord -Literal $literal -Unbalanced $false
        if ($nameIsConstant) {
            Add-CommandLineWrapperExpansion -Context $Context -Record $record -Word $tokens.ToArray()
        }
    }
}
