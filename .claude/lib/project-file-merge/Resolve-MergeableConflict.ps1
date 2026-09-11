<#
.SYNOPSIS
    Resolve mechanically-mergeable project-file conflicts in a worktree.

.DESCRIPTION
    Entry point for the parent-side project-file merge step of issue #643. It
    lists the conflicted paths of a worktree left by an in-progress merge,
    refuses the whole set unless every path belongs to the mergeable class, and
    otherwise rewrites each conflicted file as the keyed union of the two sides.

    The script stages nothing and creates no revision: the caller owns those
    steps, so a resolution this script produces is always inspectable before it
    becomes history. It writes one compressed JSON object to stdout and exits 0
    for both verdicts; the verdict is that object's result field. A non-zero exit
    means an unexpected error, whose message is on stderr.

.PARAMETER Worktree
    Absolute or relative path to the worktree holding the conflicted merge.

.PARAMETER ConfigPath
    Truth table to read mergeable_paths from. Defaults to the worktree's own
    config/blast-radius.json.
#>
param(
    [string] $Worktree,
    [string] $ConfigPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The merge module is imported before the grammar module it nests. Reversing the
# two would leave the grammar unavailable here: the nested -Force import inside
# ProjectFileMerge.psm1 removes an already-loaded grammar module and re-imports it
# into that module's own scope, taking its commands out of this scope.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'ProjectFileMerge.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'ProjectFileMergeGrammar.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '../blast-radius/BlastRadiusConflict.psm1') -Force -ErrorAction Stop

# The three bytes a UTF-8 byte-order mark occupies, kept as a constant so the
# reader and the writer agree on one prefix.
$script:ByteOrderMark = [byte[]] @(0xEF, 0xBB, 0xBF)

# Splits a text into lines that each retain their own terminator, which is what
# lets a CRLF file survive a merge unchanged.
$script:LinePattern = '[^\r\n]*(?:\r\n|\n|\r)'

function Invoke-GitExe {
    <#
    .SYNOPSIS
        The single executable seam of this script.
    .DESCRIPTION
        Every git invocation goes through here, so a test mocks one function
        rather than an executable. A non-zero exit throws with the output.
    #>
    [CmdletBinding()]
    # The unary comma wraps the result so the pipeline does not unroll a
    # one-element collection, which makes the emitted object an Object[]
    # carrying strings rather than a bare string[]. Both are declared.
    [OutputType([string[]], [System.Object[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]] $GitArgs)

    $output = & git @GitArgs 2>&1
    # A failed git call is never a resolvable state, so it stops the run.
    if ($LASTEXITCODE -ne 0) {
        throw ('git {0} exited {1}: {2}' -f ($GitArgs -join ' '), $LASTEXITCODE, ($output -join ' '))
    }
    return , @($output | ForEach-Object { [string] $_ })
}

function Read-ConflictedFile {
    <#
    .SYNOPSIS
        Read a conflicted file as bytes, a mark flag, and terminator-bearing lines.
    .DESCRIPTION
        Decoding is strict UTF-8. A file that does not decode returns $null, which
        the caller turns into an escalation rather than a lossy rewrite.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory = $true)][string] $Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq $script:ByteOrderMark[0] -and $bytes[1] -eq $script:ByteOrderMark[1] -and $bytes[2] -eq $script:ByteOrderMark[2]
    try {
        $text = [System.Text.UTF8Encoding]::new($false, $true).GetString($bytes)
    } catch [System.Text.DecoderFallbackException] {
        return $null
    }
    # The decoded mark is metadata, not content; the writer restores it.
    if ($hasBom) { $text = $text.Substring(1) }

    $line = [System.Collections.Generic.List[string]]::new()
    foreach ($match in [regex]::Matches($text, $script:LinePattern)) { $line.Add($match.Value) }
    # A final line with no terminator is not matched above and is added here.
    $consumed = ($line -join '').Length
    if ($consumed -lt $text.Length) { $line.Add($text.Substring($consumed)) }

    return [pscustomobject]@{ Bytes = $bytes; HasBom = $hasBom; Line = $line.ToArray() }
}

function Write-MergedFile {
    <#
    .SYNOPSIS
        Write merged lines back over the conflicted file.
    .DESCRIPTION
        The byte array is assembled before the ShouldProcess guard, so -WhatIf
        exercises the whole assembly and suppresses only the write itself.
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    [OutputType([void])]
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line,
        [Parameter(Mandatory = $true)][bool] $HasBom
    )

    $body = [System.Text.UTF8Encoding]::new($false).GetBytes($Line -join '')
    $payload = [System.Collections.Generic.List[byte]]::new()
    # A file that carried a mark keeps it, because dropping it is a byte change
    # no reviewer asked for.
    if ($HasBom) { $payload.AddRange($script:ByteOrderMark) }
    $payload.AddRange($body)
    if ($PSCmdlet.ShouldProcess($Path, 'Write the merged project file')) {
        [System.IO.File]::WriteAllBytes($Path, $payload.ToArray())
    }
}

function ConvertTo-ResolutionResult {
    <#
    .SYNOPSIS
        Build the single result record this script emits.
    .DESCRIPTION
        The three keys are the whole published contract, so they are written in
        one place rather than at each return site.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory = $true)][string] $ResultName,
        [AllowEmptyCollection()][object[]] $Resolved = @(),
        [AllowEmptyCollection()][AllowEmptyString()][string[]] $EscalatePath = @()
    )

    return [ordered]@{ result = $ResultName; resolved = $Resolved; escalate_paths = $EscalatePath }
}

function Invoke-MergeableConflictResolution {
    <#
    .SYNOPSIS
        Resolve every conflicted path of a worktree, or escalate the whole set.
    .DESCRIPTION
        The set is all-or-nothing. One non-mergeable path escalates before any
        file is read, and one unresolvable file escalates before any file is
        written, so a partially merged worktree is not a reachable state.
    .PARAMETER Worktree
        The worktree holding the conflicted merge.
    .PARAMETER ConfigPath
        Truth table carrying mergeable_paths; the worktree default is used when
        this is blank.
    .OUTPUTS
        An ordered record carrying result, resolved, and escalate_paths.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory = $true)][string] $Worktree,
        [AllowNull()][AllowEmptyString()][string] $ConfigPath
    )

    if ([string]::IsNullOrWhiteSpace($ConfigPath)) { $ConfigPath = Join-Path -Path $Worktree -ChildPath 'config/blast-radius.json' }
    $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    $mergeable = @(Get-ConfigMergeablePath -Config $config)

    # The listing is bound before it is filtered: Invoke-GitExe returns one array
    # object, so piping it directly would hand the whole array to the filter.
    $listed = Invoke-GitExe -GitArgs @('-C', $Worktree, 'diff', '--name-only', '--diff-filter=U')
    $conflicted = @($listed | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $outside = @($conflicted | Where-Object { -not (Test-MergeablePath -Entry $_ -MergeablePath $mergeable) })
    # Classification happens first so a non-mergeable set costs no file read.
    if ($outside.Count -gt 0) { return (ConvertTo-ResolutionResult -ResultName 'escalate' -EscalatePath $outside) }

    $pending = [System.Collections.Generic.List[object]]::new()
    foreach ($path in $conflicted) {
        $kind = Get-ProjectFileKind -Path $path
        if ($null -eq $kind) { return (ConvertTo-ResolutionResult -ResultName 'escalate' -EscalatePath @($path)) }
        $file = Read-ConflictedFile -Path (Join-Path -Path $Worktree -ChildPath $path)
        if ($null -eq $file) { return (ConvertTo-ResolutionResult -ResultName 'escalate' -EscalatePath @($path)) }

        $outcome = Merge-ConflictedText -Line $file.Line -Kind $kind
        if ($outcome.Escalate) { return (ConvertTo-ResolutionResult -ResultName 'escalate' -EscalatePath @($path)) }

        # The three stages are the only authority on what the merge must keep.
        # Each is bound directly, because Invoke-GitExe already returns one array
        # and an array subexpression around it would nest that array inside another.
        $ours = Invoke-GitExe -GitArgs @('-C', $Worktree, 'show', ":2:$path")
        $theirs = Invoke-GitExe -GitArgs @('-C', $Worktree, 'show', ":3:$path")
        $base = Invoke-GitExe -GitArgs @('-C', $Worktree, 'show', ":1:$path")
        $isKept = Test-NeverDropPostCondition -MergedLine $outcome.Lines -OursLine $ours -TheirsLine $theirs -BaseLine $base -Kind $kind
        if (-not $isKept) { return (ConvertTo-ResolutionResult -ResultName 'escalate' -EscalatePath @($path)) }

        $pending.Add([pscustomobject]@{
                Path   = $path
                HasBom = $file.HasBom
                Line   = $outcome.Lines
                Record = [ordered]@{
                    path                      = $path
                    entries_added_from_ours   = @($outcome.EntriesAddedFromOurs)
                    entries_added_from_theirs = @($outcome.EntriesAddedFromTheirs)
                    version_resolutions       = @($outcome.VersionResolutions)
                }
            })
    }

    # Every path resolved, so the worktree can be rewritten in one pass.
    foreach ($item in $pending) {
        Write-MergedFile -Path (Join-Path -Path $Worktree -ChildPath $item.Path) -Line $item.Line -HasBom $item.HasBom
    }
    return (ConvertTo-ResolutionResult -ResultName 'resolved' -Resolved @($pending | ForEach-Object { $_.Record }))
}

# Dot-sourcing loads the functions for a test host; any other invocation runs the
# step. The guard is the precedent set by Invoke-ReleaseReconciliation.ps1.
if ($MyInvocation.InvocationName -ne '.') {
    if ([string]::IsNullOrWhiteSpace($Worktree)) { throw 'Worktree is required.' }
    $result = Invoke-MergeableConflictResolution -Worktree $Worktree -ConfigPath $ConfigPath
    Write-Output (ConvertTo-Json -InputObject $result -Depth 6 -Compress)
    exit 0
}
