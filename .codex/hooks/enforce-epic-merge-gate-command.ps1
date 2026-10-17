<#
.SYNOPSIS
    Scope predicate and pull-request selector reader for the epic merge gate (issue #952).
.DESCRIPTION
    Dot-sourced by enforce-epic-merge-gate.ps1 on both runtimes; this file is byte-identical in
    .claude/hooks and .codex/hooks. Under a merge queue every gh pr merge spelling (--merge, -m,
    --squash, -s, --rebase, -r, --auto, --admin, or no method flag) adds the pull request to the
    queue, and the gh api mutations enqueuePullRequest, enablePullRequestAutoMerge, and
    mergePullRequest and the REST pulls/<n>/merge endpoint also reach the base branch. All of
    them are in scope. A gh pr merge whose only action flag is --disable-auto is out of scope.

    Pure string logic only: no disk, process, network, clock, or environment access. The file
    dot-sources and imports nothing; it consumes Get-CommandLineInvocation and
    Get-CommandLineStructuralMatch, which the gate loads from hook-command-invocation.ps1 first.

    Known limitation (issue #952 code review CR-5): the gh api scope reads the command text
    only. A GraphQL mutation supplied from a file (gh api graphql -F query=@mutation.graphql,
    or gh api --input payload.json) is not visible to this text-based scope and is not gated.
#>

# gh pr merge flags that perform or schedule a merge. Any of them keeps a command in scope even
# when it also carries --disable-auto.
$script:EpicMergeGateActionFlags = @('--merge', '-m', '--squash', '-s', '--rebase', '-r', '--auto', '--admin')

# gh pr merge flags, including the inherited --repo, that consume the following token as a value.
$script:EpicMergeGateValueFlags = @('-b', '--body', '-F', '--body-file', '-t', '--subject', '-A', '--author-email', '--match-head-commit', '-R', '--repo')

# GraphQL mutations that merge, enqueue, or schedule the merge of a pull request.
$script:EpicMergeGateMutationNames = @('enqueuePullRequest', 'enablePullRequestAutoMerge', 'mergePullRequest')

# The REST merge endpoint path; group 1 is the pull-request number.
$script:EpicMergeGateRestMergePattern = '(?:^|/)pulls/(\d+)/merge(?:$|[^\w-])'

function Get-EpicMergeGateFlagName {
    <#
    .SYNOPSIS
        Return the flag name of a dash-led token without any '=value' suffix.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Token)

    $split = $Token.IndexOf('=')
    if ($split -gt 0) { return $Token.Substring(0, $split) }
    return $Token
}

function ConvertFrom-EpicMergeGateSelector {
    <#
    .SYNOPSIS
        Convert a gh pr merge selector to a pull-request number, or $null.
    .DESCRIPTION
        Accepts an all-digit selector or a URL whose path ends in /pull/<n>. A branch-name
        selector carries no number and yields $null.
    .OUTPUTS
        System.Nullable[int]
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Selector)

    $digits = $null
    if ($Selector -match '^\d+$') {
        $digits = $Selector
    } elseif ($Selector -match '/pull/(\d+)/?(?:[?#].*)?$') {
        $digits = $Matches[1]
    }
    $number = 0
    if ($null -ne $digits -and [int]::TryParse($digits, [ref] $number)) { return $number }
    return $null
}

function Test-EpicMergeGateDisableAutoOnly {
    <#
    .SYNOPSIS
        Report whether a Structural gh pr merge match carries --disable-auto and no action flag.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][pscustomobject] $Match)

    $tokens = [string[]]@($Match.Segment.Tokens)
    $hasDisableAuto = $false
    for ($index = $Match.OperandIndex; $index -lt $tokens.Count; $index++) {
        $name = Get-EpicMergeGateFlagName -Token $tokens[$index]
        if ($script:EpicMergeGateActionFlags -ccontains $name) { return $false }
        if ($name -ceq '--disable-auto') { $hasDisableAuto = $true }
    }
    return $hasDisableAuto
}

function Test-EpicMergeGateApiMutationText {
    <#
    .SYNOPSIS
        Report whether gh api text names a merge or enqueue mutation or the REST merge endpoint.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $Text)

    if ([string]::IsNullOrEmpty($Text)) { return $false }
    foreach ($name in $script:EpicMergeGateMutationNames) {
        if ($Text.IndexOf($name, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) { return $true }
    }
    return [regex]::IsMatch($Text, $script:EpicMergeGateRestMergePattern)
}

function Test-EpicMergeGateCommandInScope {
    <#
    .SYNOPSIS
        Report whether a command line is a gated merge or enqueue action (issue #952).
    .DESCRIPTION
        True for any gh pr merge match, Structural or Indeterminate, unless the match is Structural
        and its only action flag is --disable-auto; true for any gh api match whose tokens name a
        merge or enqueue mutation or a pulls/<n>/merge path. Text that only mentions these words
        produces no match and stays out of scope. An Indeterminate gh pr merge match stays in scope
        (fail closed).
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText)

    foreach ($match in @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'merge'))) {
        if ($match.Status -ne 'Structural' -or -not (Test-EpicMergeGateDisableAutoOnly -Match $match)) { return $true }
    }
    foreach ($match in @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('api'))) {
        if (Test-EpicMergeGateApiMutationText -Text (@($match.Segment.Tokens) -join ' ')) { return $true }
    }
    return $false
}

function Get-EpicMergeGateSelectorPrNumber {
    <#
    .SYNOPSIS
        Resolve the pull-request number of a gated merge or enqueue command, or $null.
    .DESCRIPTION
        For gh pr merge, only the first Structural match is read. Value flags are skipped with
        their values, other flags are skipped, an equals-joined --merge=<digits> yields its
        digits (an equals-joined value that is not a number is skipped like any other flag),
        and the first non-flag token (or the token after a bare '--') is the selector.
        For gh api, the number comes from a pulls/<n>/merge token. Any other command yields
        $null, which the gate handles through its no-number branches.
    .OUTPUTS
        System.Nullable[int]
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param([Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText)

    $match = Get-CommandLineStructuralMatch -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'merge')
    if ($null -ne $match) {
        $tokens = [string[]]@($match.Segment.Tokens)
        $index = $match.OperandIndex
        while ($index -lt $tokens.Count) {
            $token = $tokens[$index]
            if ($token -ceq '--') {
                if ($index + 1 -lt $tokens.Count) { return ConvertFrom-EpicMergeGateSelector -Selector $tokens[$index + 1] }
                return $null
            }
            if ($token.StartsWith('-') -and $token.Length -gt 1) {
                $name = Get-EpicMergeGateFlagName -Token $token
                if ($name -ceq '--merge' -and $token.Length -gt $name.Length) {
                    $equalsNumber = ConvertFrom-EpicMergeGateSelector -Selector $token.Substring($name.Length + 1)
                    if ($null -ne $equalsNumber) { return $equalsNumber }
                }
                $index += if ($script:EpicMergeGateValueFlags -ccontains $token) { 2 } else { 1 }
                continue
            }
            return ConvertFrom-EpicMergeGateSelector -Selector $token
        }
        return $null
    }

    foreach ($apiMatch in @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('api'))) {
        foreach ($token in @($apiMatch.Segment.Tokens)) {
            $rest = [regex]::Match([string]$token, $script:EpicMergeGateRestMergePattern)
            $number = 0
            if ($rest.Success -and [int]::TryParse($rest.Groups[1].Value, [ref] $number)) { return $number }
        }
    }
    return $null
}
