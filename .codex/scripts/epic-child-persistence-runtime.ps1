# Atomic receipt persistence and semantic wave-lock helpers for Codex epic children.

function Write-CodexChildJsonCreateNew {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)] $Value,
        [scriptblock] $EnsureDirectory = {
            param([string] $DirectoryPath)
            [System.IO.Directory]::CreateDirectory($DirectoryPath) | Out-Null
        },
        [scriptblock] $OpenFile = {
            param([string] $FilePath)
            [System.IO.File]::Open($FilePath, [System.IO.FileMode]::CreateNew,
                [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
        }
    )
    & $EnsureDirectory (Split-Path $Path -Parent)
    $stream = & $OpenFile $Path
    try {
        $writer = [System.IO.StreamWriter]::new($stream, [System.Text.UTF8Encoding]::new($false))
        try {
            $writer.Write(($Value | ConvertTo-Json -Depth 32)); $writer.Flush()
            if ($stream -is [System.IO.FileStream]) { $stream.Flush($true) } else { $stream.Flush() }
        } finally { $writer.Dispose() }
    } finally { $stream.Dispose() }
}

function Write-CodexChildJsonAtomic {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Path, [Parameter(Mandatory)] $Value)
    [System.IO.Directory]::CreateDirectory((Split-Path $Path -Parent)) | Out-Null
    $temporaryPath = "$Path.tmp.$([guid]::NewGuid().ToString('N'))"
    try {
        Write-CodexChildJsonCreateNew -Path $temporaryPath -Value $Value
        [System.IO.File]::Move($temporaryPath, $Path, $true)
    } finally {
        if (Test-Path -LiteralPath $temporaryPath) { Remove-Item -LiteralPath $temporaryPath -Force }
    }
}

function Enter-CodexChildWaveLock {
    [OutputType([System.IO.Stream])]
    param(
        [Parameter(Mandatory)][string] $Path,
        [scriptblock] $OpenLock = {
            param([string] $LockPath)
            [System.IO.File]::Open($LockPath, [System.IO.FileMode]::OpenOrCreate,
                [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
        }
    )
    try {
        $stream = & $OpenLock $Path
    } catch {
        throw "EPIC_CHILD_LAUNCH_BLOCKED: semantic wave lock is already held: $Path"
    }
    $writer = $null
    try {
        $stream.SetLength(0)
        $writer = [System.IO.StreamWriter]::new($stream, [System.Text.UTF8Encoding]::new($false), 1024, $true)
        $writer.Write("pid=$PID`nacquired_at=$([datetimeoffset]::UtcNow.ToString('o'))")
        $writer.Flush()
        if ($stream -is [System.IO.FileStream]) { $stream.Flush($true) } else { $stream.Flush() }
        $writer.Dispose()
        $writer = $null
        return $stream
    } catch {
        if ($null -ne $writer) { $writer.Dispose() }
        $stream.Dispose()
        throw "EPIC_CHILD_LAUNCH_BLOCKED: semantic wave lock initialization failed: $Path $_"
    }
}

function Get-CodexChildSemanticWaveLockPath {
    [OutputType([string])]
    param([Parameter(Mandatory)] $Spec, [Parameter(Mandatory)][string] $ArtifactRoot)
    $launchRoot = Split-Path $ArtifactRoot -Parent
    $branchKey = (Get-CodexChildSha256 -Value ([string]$Spec.integration_branch)).Substring(0, 16)
    $name = "$($Spec.checkpoint_kind).wave-$([int]$Spec.wave_number).$branchKey.lock"
    return Join-Path (Join-Path $launchRoot '.locks') $name
}

function Assert-CodexChildAuthorityOutsideRepository {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $AuthorityPath,
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [scriptblock] $ResolveExistingAncestor = {
            param([string] $Candidate)
            $current = [System.IO.Path]::GetFullPath($Candidate)
            while (-not (Test-Path -LiteralPath $current -PathType Container)) {
                $parent = Split-Path $current -Parent
                if ([string]::IsNullOrWhiteSpace($parent) -or $parent -eq $current) { throw 'no existing authority ancestor.' }
                $current = $parent
            }
            $item = Get-Item -LiteralPath $current -Force
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                return $item.ResolveLinkTarget($true).FullName
            }
            return $item.FullName
        },
        [scriptblock] $IsInsideGit = {
            param([string] $Candidate)
            Test-CodexChildGit -GitArgs @('-C', $Candidate, 'rev-parse', '--is-inside-work-tree')
        }
    )
    $comparison = if ($IsWindows) { [System.StringComparison]::OrdinalIgnoreCase } else { [System.StringComparison]::Ordinal }
    $authority = [System.IO.Path]::GetFullPath($AuthorityPath)
    $repository = [System.IO.Path]::GetFullPath($RepositoryRoot)
    if ($authority.Equals($repository, $comparison) -or
        $authority.StartsWith($repository + [System.IO.Path]::DirectorySeparatorChar, $comparison)) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: isolated CODEX_HOME authority is inside RepositoryRoot.'
    }
    $ancestor = & $ResolveExistingAncestor $authority
    if (& $IsInsideGit $ancestor) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: isolated CODEX_HOME authority resolves inside a Git repository.'
    }
}
