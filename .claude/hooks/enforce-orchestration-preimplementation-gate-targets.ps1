<#
.SYNOPSIS
    Per-segment target resolution for the epic-scope decision of the preimplementation gate (issue #738).
.DESCRIPTION
    Pure string logic only: no disk, process, network, clock, or environment access, and no
    dot-source of its own. Every command segment and every path of a gated call is resolved to
    the absolute worktree target it operates on, or the call is reported unresolvable with one
    rule token, so the epic-scope decision can evaluate every target instead of the first
    segment only. Resolution rules R0 to R7 and verdict rules V1 to V7 are defined in the
    issue #732 feature folder plan, section 4.

    Dot-sourced by enforce-orchestration-preimplementation-gate-epic-scope.ps1 on both the
    Claude and the Codex surface; the four copies are byte-identical.
.NOTES
    PowerShell 7+. Call-time dependencies, loaded by each gate's existing dot-source of
    hook-command-invocation.ps1 (which itself dot-sources hook-command-scanner.ps1 and
    hook-command-payload.ps1): Read-CommandLineSegment (segment records with IsWrapperLed and
    HasLiveSubstitution), Get-CommandLineGlobalOption (the git global-option table), and
    Get-CommandLineInvocation (the token-aware git add and git commit matcher, read for its
    match count only). Never calls the command-line splitter or the command-line tokenizer
    of the sibling helpers file.
#>

# Command words that change the working directory of later segments (rule R3).
$script:OrchestrationTargetDirectoryChangeWords = @(
    'cd', 'pushd', 'popd', 'chdir', 'Set-Location', 'sl', 'Push-Location', 'Pop-Location'
)

# Environment names that relocate the git directory or work tree (rule R5).
$script:OrchestrationTargetRelocationNames = @('GIT_DIR', 'GIT_WORK_TREE', 'GIT_COMMON_DIR', 'GIT_INDEX_FILE')
# A relocation name not embedded in a longer identifier (issue #903, #732 CR-2).
$script:OrchestrationTargetRelocationPattern = '(?i)(?<![A-Za-z0-9_])(?:' +
($script:OrchestrationTargetRelocationNames -join '|') + ')(?![A-Za-z0-9_])'

# An absolute target: a drive-lettered or a rooted, non-UNC path after separator normalization.
$script:OrchestrationTargetAbsolutePattern = '^([A-Za-z]:/|/(?!/))'

# A non-local path-leg spelling, tested before normalization (issue #937): UNC, extended-length,
# device, or NT-object prefixes in either separator style, a drive-relative drive letter, a
# current-drive-rooted single backslash, a URI scheme, a ~-prefixed home path, or surrounding whitespace.
$script:OrchestrationTargetNonLocalPattern = '^(?:\s|[\\/]{2}|[\\/]\?\?[\\/]|\\(?![\\/])|[A-Za-z]:(?![\\/])' +
'|[A-Za-z][A-Za-z0-9+.-]+:|~)|\s$'

# The apply_patch markers whose path names a file the patch writes (Codex path leg).
$script:OrchestrationPatchMarkers = @('*** Add File:', '*** Update File:', '*** Delete File:', '*** Move to:')

function ConvertTo-OrchestrationTargetPath {
    <#
    .SYNOPSIS
        Normalizes a session root or a path-leg input to forward slashes.
    .DESCRIPTION
        Replaces every backslash with a forward slash and removes one trailing slash, except
        from a bare root. Never applied to a -C selector value, which is judged as written.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Path)

    $normalized = $Path.Replace('\', '/')
    if ($normalized.Length -gt 1 -and $normalized.EndsWith('/') -and $normalized -notmatch '^[A-Za-z]:/$') {
        $normalized = $normalized.Substring(0, $normalized.Length - 1)
    }
    return $normalized
}

function Get-OrchestrationPatchMarkerPath {
    <#
    .SYNOPSIS
        Returns the file paths an apply_patch text names, or an empty array.
    .DESCRIPTION
        Recognized only when the first non-empty line is `*** Begin Patch`. Each Add File,
        Update File, Delete File, and Move to marker contributes its trimmed path.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $PatchText)

    $lines = @($PatchText -split "`r?`n")
    $first = @($lines | Where-Object { $_.Trim() }) | Select-Object -First 1
    if ($null -eq $first -or $first.Trim() -cne '*** Begin Patch') {
        return [string[]]@()
    }
    $paths = [System.Collections.Generic.List[string]]::new()
    foreach ($line in $lines) {
        foreach ($marker in $script:OrchestrationPatchMarkers) {
            if ($line.StartsWith($marker, [System.StringComparison]::Ordinal)) {
                $paths.Add($line.Substring($marker.Length).Trim())
            }
        }
    }
    return [string[]]$paths.ToArray()
}

function Test-OrchestrationTargetDotSegment {
    # True when a forward-slash path carries a '.' or '..' segment.
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Path)

    $segments = @($Path -split '/')
    return ($segments -contains '.' -or $segments -contains '..')
}

function New-OrchestrationTargetResult {
    # Builds the FR-3 result object; an unresolved result carries no targets.
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute(
        'PSUseShouldProcessForStateChangingFunctions', '',
        Justification = 'Pure in-memory factory that changes no system state.')]
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowEmptyCollection()][string[]] $Targets = @(),
        [AllowEmptyString()][string] $Rule = '',
        [AllowEmptyString()][string] $Value = ''
    )

    if ($Rule) {
        Write-Debug "PREIMPL_TARGET_UNRESOLVABLE: ${Rule}: $Value"
        return [pscustomobject]@{
            Resolved = $false; Targets = [string[]]@(); ReasonCode = 'target-unresolvable'; Detail = "${Rule}: $Value"
        }
    }
    return [pscustomobject]@{ Resolved = $true; Targets = [string[]]$Targets; ReasonCode = ''; Detail = '' }
}

function Get-OrchestrationGitSelectorTarget {
    <#
    .SYNOPSIS
        Walks the git global options of one segment record and returns its -C targets or a failure.
    .DESCRIPTION
        Rules R5 (option form) and R6. Returns an object with Rule and Value set on failure, and
        Selectors holding every -C value otherwise (empty when the segment has no -C).
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token)

    $table = Get-CommandLineGlobalOption -CommandWord 'git'
    $gitIndex = [array]::IndexOf($Token, 'git')
    if ($gitIndex -lt 0) {
        return [pscustomobject]@{ Rule = 'git-command-word-missing'; Value = ''; Selectors = [string[]]@() }
    }
    $start = $gitIndex + 1
    $selectors = [System.Collections.Generic.List[string]]::new()
    $relocation = ''
    $unmodeled = ''
    $missing = $false
    $index = $start
    while ($index -lt $Token.Count -and $Token[$index].StartsWith('-')) {
        $name = $Token[$index]
        $value = $null
        $equals = $name.IndexOf('=')
        if ($name.StartsWith('--') -and $equals -gt 0) {
            $value = $name.Substring($equals + 1)
            $name = $name.Substring(0, $equals)
        }
        if ($table.WithArgument -ccontains $name) {
            if ($null -eq $value) {
                $index++
                if ($index -lt $Token.Count) {
                    $value = $Token[$index]
                }
                elseif ($name -ceq '-C') {
                    $missing = $true
                }
                else {
                    $value = ''
                }
            }
        } elseif ($table.Standalone -notcontains $name -and $table.Terminal -notcontains $name) {
            $unmodeled = $name
            break
        }
        $isWorktreeConfig = $name -ceq '-c' -and $null -ne $value -and
        $value.StartsWith('core.worktree', [System.StringComparison]::OrdinalIgnoreCase)
        if ($name -ceq '--git-dir' -or $name -ceq '--work-tree' -or $isWorktreeConfig) {
            if (-not $relocation) { $relocation = "$name $value".Trim() }
        }
        if ($name -ceq '-C' -and -not $missing) { $selectors.Add($value) }
        $index++
    }

    $rule = ''; $value = ''
    if ($relocation) { $rule = 'git-relocation'; $value = $relocation }
    elseif ($unmodeled) { $rule = 'git-option-unmodeled'; $value = $unmodeled }
    elseif ($missing) { $rule = 'selector-missing-value'; $value = '-C' }
    foreach ($selector in $selectors) {
        if ($rule) { break }
        if ($selector.Contains('\')) { $rule = 'selector-backslash' }
        elseif ($selector -notmatch $script:OrchestrationTargetAbsolutePattern) { $rule = 'selector-not-absolute' }
        elseif (Test-OrchestrationTargetDotSegment -Path $selector) { $rule = 'selector-dot-segment' }
        $value = $selector
    }
    if ($rule) { return [pscustomobject]@{ Rule = $rule; Value = $value; Selectors = [string[]]@() } }
    return [pscustomobject]@{ Rule = ''; Value = ''; Selectors = [string[]]$selectors.ToArray() }
}

function Get-OrchestrationCommandTarget {
    <#
    .SYNOPSIS
        Resolves every command segment and path of a gated call to its absolute target worktree.
    .DESCRIPTION
        Rules R0 to R7 (first failing rule decides). Returns Resolved, Targets (ordinal
        case-insensitive distinct, first-seen order), ReasonCode ('' or target-unresolvable),
        and Detail (a rule token, ': ', and the offending text or value).
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()][AllowEmptyString()][string] $Command = '',
        [AllowNull()][AllowEmptyCollection()][string[]] $FilePath = @(),
        [Parameter(Mandatory)][AllowEmptyString()][string] $SessionRoot
    )

    $root = ConvertTo-OrchestrationTargetPath -Path $SessionRoot
    if ($root -notmatch $script:OrchestrationTargetAbsolutePattern) {
        return (New-OrchestrationTargetResult -Rule 'session-root-not-absolute' -Value $SessionRoot)
    }
    $targets = [System.Collections.Generic.List[string]]::new()

    foreach ($rawPath in @($FilePath | Where-Object { $_ })) {
        if ($rawPath -match $script:OrchestrationTargetNonLocalPattern) {
            return (New-OrchestrationTargetResult -Rule 'path-not-local' -Value $rawPath)
        }
        $path = ConvertTo-OrchestrationTargetPath -Path $rawPath
        if ($path -notmatch $script:OrchestrationTargetAbsolutePattern) { $targets.Add($root); continue }
        if (Test-OrchestrationTargetDotSegment -Path $path) {
            return (New-OrchestrationTargetResult -Rule 'path-dot-segment' -Value $rawPath)
        }
        $targets.Add($path)
    }

    if ($Command) {
        $records = @(Read-CommandLineSegment -CommandText $Command)
        if ($records.Count -eq 0) { $targets.Add($root) }
        foreach ($record in $records) {
            $text = ([string]$record.RawText).Trim()
            if ($record.Unbalanced) { return (New-OrchestrationTargetResult -Rule 'segment-unbalanced' -Value $text) }
            if ($script:OrchestrationTargetDirectoryChangeWords -contains [string]$record.CommandWord) {
                return (New-OrchestrationTargetResult -Rule 'directory-change' -Value $text)
            }
            $wrapped = [bool]$record.IsWrapperLed -or [bool]$record.HasLiveSubstitution
            if ($wrapped) {
                $matchCount = 0
                foreach ($sub in @('add', 'commit')) {
                    $invocationArguments = @{
                        CommandText = $record.RawText; CommandWord = 'git'; SubcommandPath = @($sub)
                    }
                    $matchCount += @(Get-CommandLineInvocation @invocationArguments).Count
                }
                if ($matchCount -gt 0) { return (New-OrchestrationTargetResult -Rule 'wrapper-git' -Value $text) }
            }
            if ($text -match $script:OrchestrationTargetRelocationPattern) {
                return (New-OrchestrationTargetResult -Rule 'git-relocation' -Value $text)
            }
            if ($wrapped -or [string]$record.CommandWord -cne 'git') { $targets.Add($root); continue }
            $git = Get-OrchestrationGitSelectorTarget -Token ([string[]]@($record.Tokens))
            if ($git.Rule) { return (New-OrchestrationTargetResult -Rule $git.Rule -Value $git.Value) }
            if ($git.Selectors.Count -eq 0) {
                $targets.Add($root)
            }
            else {
                foreach ($selector in $git.Selectors) { $targets.Add($selector) }
            }
        }
    }

    $distinct = [System.Collections.Generic.List[string]]::new()
    foreach ($target in $targets) {
        $seen = $distinct | Where-Object { [string]::Equals($_, $target, [System.StringComparison]::OrdinalIgnoreCase) }
        if (-not $seen) { $distinct.Add($target) }
    }
    return (New-OrchestrationTargetResult -Targets $distinct.ToArray())
}

function Resolve-OrchestrationEpicTargetVerdict {
    <#
    .SYNOPSIS
        Decides the epic-scope verdict for a resolved or unresolved target result.
    .DESCRIPTION
        Verdict rules V1 to V7. Resolves the session-root scope with an empty selector and each
        target scope with the target as selector, each distinct key (ordinal
        case-insensitive) at most once; a target equal to the session root reuses its scope.
        Returns Verdict ('none', 'deny', or 'evaluate'), ReasonCode, Detail, and Evaluations
        (one Target, IsSessionRoot, Scope entry per target when the verdict is evaluate).
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $SessionRoot,
        [Parameter(Mandatory)][pscustomobject] $TargetResult,
        [Parameter(Mandatory)][scriptblock] $ScopeResolver
    )

    $root = ConvertTo-OrchestrationTargetPath -Path $SessionRoot
    $cache = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $cache[$root] = & $ScopeResolver ''
    $evaluations = [System.Collections.Generic.List[pscustomobject]]::new()
    if ($TargetResult.Resolved) {
        foreach ($target in @($TargetResult.Targets)) {
            if (-not $cache.ContainsKey($target)) { $cache[$target] = & $ScopeResolver $target }
            $isSessionRoot = [string]::Equals($target, $root, [System.StringComparison]::OrdinalIgnoreCase)
            $evaluations.Add([pscustomobject]@{
                    Target = $target; IsSessionRoot = $isSessionRoot; Scope = $cache[$target]
                })
        }
    }

    $relevant = @($cache.Values | Where-Object { $_.IsEpicScope -or $_.Reason -eq 'target-worktree-ambiguous' })
    $code = ''; $detail = ''
    if ($relevant.Count -eq 0) {
        return [pscustomobject]@{ Verdict = 'none'; ReasonCode = ''; Detail = ''; Evaluations = @() }
    }
    if (-not $TargetResult.Resolved) {
        $code = 'target-unresolvable'; $detail = [string]$TargetResult.Detail
    }
    $ambiguous = @($evaluations | Where-Object { $_.Scope.Reason -eq 'target-worktree-ambiguous' })
    $unresolved = @($evaluations | Where-Object { $_.Scope.Reason -eq 'selector-unresolved' })
    $mixed = @($evaluations | Where-Object { -not $_.Scope.IsEpicScope })
    if (-not $code -and $ambiguous.Count -gt 0) { $code = 'target-ambiguous'; $detail = $ambiguous[0].Target }
    if (-not $code -and $unresolved.Count -gt 0) {
        $code = 'target-unresolvable'; $detail = "selector-unresolved: $($unresolved[0].Target)"
    }
    if (-not $code -and $mixed.Count -gt 0) { $code = 'target-mixed'; $detail = $mixed[0].Target }
    if ($code) {
        return [pscustomobject]@{ Verdict = 'deny'; ReasonCode = $code; Detail = $detail; Evaluations = @() }
    }
    return [pscustomobject]@{ Verdict = 'evaluate'; ReasonCode = ''; Detail = ''; Evaluations = $evaluations.ToArray() }
}

function Get-OrchestrationEpicTargetDenyReason {
    <#
    .SYNOPSIS
        Builds the deny reason of an epic-scope target verdict or a not-ready target.
    .DESCRIPTION
        One string per code, behind the unchanged PREIMPLEMENTATION_GATE_BLOCKED prefix:
        target-unresolvable (Detail is the rule detail), target-ambiguous and target-mixed
        (Detail is the target), and target-not-ready (Detail is the target, with the
        consulted checkpoint path and the failed readiness predicate).
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string] $ReasonCode,
        [AllowEmptyString()][string] $Detail = '',
        [AllowEmptyString()][string] $CheckpointPath = '',
        [AllowEmptyString()][string] $Failure = ''
    )

    $prefix = "PREIMPLEMENTATION_GATE_BLOCKED: ${ReasonCode}: "
    switch ($ReasonCode) {
        'target-ambiguous' {
            return $prefix + "more than one worktree claims the epic integration branch for target $Detail. " +
            'Implementation operations in epic scope require an unambiguous target worktree.'
        }
        'target-mixed' {
            return $prefix + "target $Detail is not in the epic scope that governs this operation. " +
            'Implementation operations in epic scope may not also target a worktree outside it.'
        }
        'target-not-ready' {
            return $prefix + "the epic-scope target $Detail was evaluated against $CheckpointPath, " +
            "and the failed readiness predicate is '$Failure'. " +
            'Implementation operations in epic scope require that checkpoint to satisfy every ' +
            'readiness predicate, and a production path may be staged or edited only while a merge is in progress.'
        }
    }
    return $prefix + "the target worktree of this epic-scope operation cannot be resolved ($Detail). " +
    'Implementation operations in epic scope require every command segment and path to name ' +
    'an absolute, resolvable target.'
}

function Resolve-OrchestrationEpicTargetOutcome {
    <#
    .SYNOPSIS
        Decides the shared epic-scope outcome of a target verdict (issue #903, #732 CR-3).
    .DESCRIPTION
        Pure decision logic shared by the Claude and the Codex epic-scope gates; reads no file,
        process, or environment value. Returns Outcome 'none' (not epic scope), 'deny' (with
        the deny Reason), or 'allow' (empty Reason). The caller supplies the readiness predicate,
        invoked as & $ReadinessFailure $evaluation.Scope; the first evaluation with a non-empty
        failure denies, and the session-root wording is unchanged.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][pscustomobject] $Verdict,
        [Parameter(Mandatory)][scriptblock] $ReadinessFailure
    )

    if ($Verdict.Verdict -eq 'none') {
        return [pscustomobject]@{ Outcome = 'none'; Reason = '' }
    }
    if ($Verdict.Verdict -eq 'deny') {
        $reason = Get-OrchestrationEpicTargetDenyReason -ReasonCode $Verdict.ReasonCode -Detail $Verdict.Detail
        return [pscustomobject]@{ Outcome = 'deny'; Reason = $reason }
    }
    foreach ($evaluation in $Verdict.Evaluations) {
        $scope = $evaluation.Scope
        $failure = & $ReadinessFailure $evaluation.Scope
        if (-not $failure) {
            continue
        }
        if ($evaluation.IsSessionRoot) {
            $reason = 'PREIMPLEMENTATION_GATE_BLOCKED: this epic-scope operation was evaluated against ' +
            "$($scope.CheckpointPath), and the failed readiness predicate is '$failure'. " +
            'Implementation operations in epic scope require that checkpoint to satisfy every ' +
            'readiness predicate, and a production path may be staged or edited only while a merge is in progress.'
            return [pscustomobject]@{ Outcome = 'deny'; Reason = $reason }
        }
        $reasonArguments = @{
            ReasonCode     = 'target-not-ready'
            Detail         = $evaluation.Target
            CheckpointPath = $scope.CheckpointPath
            Failure        = $failure
        }
        return [pscustomobject]@{ Outcome = 'deny'; Reason = (Get-OrchestrationEpicTargetDenyReason @reasonArguments) }
    }
    return [pscustomobject]@{ Outcome = 'allow'; Reason = '' }
}
