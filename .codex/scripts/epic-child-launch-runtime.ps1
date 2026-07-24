# Git, trust, and process-configuration helpers for epic-child launches and resumes.

$script:CodexChildTrustedSurfaces = @(
    '.codex',
    '.agents',
    'AGENTS.md',
    'config/orchestration-routing.json'
)

function Invoke-CodexChildGit {
    [CmdletBinding()]
    [OutputType([object[]])]
    param([Parameter(Mandatory)][string[]] $GitArgs)
    $output = @(& git @GitArgs 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "EPIC_CHILD_LAUNCH_BLOCKED: git $($GitArgs -join ' ') failed: $($output -join ' ')"
    }
    return $output
}

function Test-CodexChildGit {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string[]] $GitArgs)
    & git @GitArgs *> $null
    return $LASTEXITCODE -eq 0
}

function Get-CodexChildGitScalar {
    [OutputType([string])]
    param([Parameter(Mandatory)][string[]] $GitArgs)
    $output = @(Invoke-CodexChildGit -GitArgs $GitArgs)
    if ($output.Count -ne 1 -or [string]::IsNullOrWhiteSpace([string]$output[0])) {
        throw "EPIC_CHILD_LAUNCH_BLOCKED: git $($GitArgs -join ' ') did not return one value."
    }
    return ([string]$output[0]).Trim()
}

function Test-CodexChildCustomizationClean {
    [OutputType([bool])]
    param([AllowEmptyCollection()][string[]] $StatusLines)
    return @($StatusLines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }).Count -eq 0
}

function Set-CodexChildReceiptState {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory)] $Receipt,
        [Parameter(Mandatory)][ValidateSet('launching', 'active', 'completed', 'failed')][string] $State,
        [AllowEmptyString()][string] $SessionId = '',
        [Nullable[int]] $ExitCode = $null,
        [AllowEmptyString()][string] $FailureReason = ''
    )
    if (-not $PSCmdlet.ShouldProcess([string]$Receipt.receipt_path, "Set epic-child receipt state to '$State'")) {
        return
    }
    $now = [datetimeoffset]::UtcNow.ToString('o'); $Receipt.state = $State
    if ($State -eq 'launching') {
        $Receipt.resume_started_at = $now; $Receipt.expires_at = [datetimeoffset]::UtcNow.AddDays(7).ToString('o')
    } elseif ($State -eq 'active') {
        if ([string]::IsNullOrWhiteSpace($SessionId)) { throw 'EPIC_CHILD_LAUNCH_BLOCKED: active receipt requires a session id.' }
        $Receipt.codex_session_id = $SessionId; $Receipt.session_bound_at = $now
    } elseif ($State -eq 'completed') {
        $Receipt.exit_code = 0; $Receipt.completed_at = $now
    } else {
        $Receipt.exit_code = $(if ($null -eq $ExitCode) { -1 } else { [int]$ExitCode })
        $Receipt.failure_reason = $FailureReason; $Receipt.failed_at = $now
    }
    Write-CodexChildJsonAtomic -Path ([string]$Receipt.receipt_path) -Value $Receipt
}

function Get-CodexChildCommonDirectory {
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $WorktreePath)
    $raw = Get-CodexChildGitScalar -GitArgs @(
        '-C', $WorktreePath, 'rev-parse', '--path-format=absolute', '--git-common-dir'
    )
    return Get-CodexChildCanonicalPath -Path $raw -BasePath $WorktreePath
}

function Get-CodexChildSurfaceObjectMap {
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)][string] $RepositoryPath, [Parameter(Mandatory)][string] $Commit)
    $objects = [ordered]@{}
    foreach ($surface in $script:CodexChildTrustedSurfaces) {
        $objects[$surface] = Get-CodexChildGitScalar -GitArgs @(
            '-C', $RepositoryPath, 'rev-parse', "$Commit`:$surface"
        )
    }
    return $objects
}

function Test-CodexChildSurfaceObjectsEqual {
    [OutputType([bool])]
    param([Parameter(Mandatory)] $Expected, [Parameter(Mandatory)] $Actual)
    foreach ($surface in $script:CodexChildTrustedSurfaces) {
        if ([string]$Expected.$surface -ne [string]$Actual.$surface) {
            return $false
        }
    }
    return $true
}

function Get-CodexChildSurfaceFingerprint {
    [OutputType([string])]
    param([Parameter(Mandatory)] $SurfaceObjects)
    $lines = foreach ($surface in $script:CodexChildTrustedSurfaces) {
        "$surface=$([string]$SurfaceObjects.$surface)"
    }
    return Get-CodexChildSha256 -Value ($lines -join "`n")
}

function Get-CodexChildTrustedProfile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $WorktreePath,
        [Parameter(Mandatory)][string] $ChildHead,
        [Parameter(Mandatory)][string] $AgentName
    )
    $relativePath = ".codex/agents/$AgentName.toml"
    $profilePath = Join-Path $WorktreePath $relativePath
    $expectedBlob = Get-CodexChildGitScalar -GitArgs @(
        '-C', $WorktreePath, 'rev-parse', "$ChildHead`:$relativePath"
    )
    $stream = [System.IO.File]::Open($profilePath, [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read)
    try {
        $workingBlob = Get-CodexChildGitScalar -GitArgs @(
            '-C', $WorktreePath, 'hash-object', '--', $relativePath
        )
        if ($workingBlob -cne $expectedBlob) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: generated profile bytes differ from child HEAD: $profilePath"
        }
        $memory = [System.IO.MemoryStream]::new()
        try { $stream.CopyTo($memory); $bytes = $memory.ToArray() } finally { $memory.Dispose() }
    } finally {
        $stream.Dispose()
    }
    $offset = if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { 3 } else { 0 }
    try {
        $raw = [System.Text.UTF8Encoding]::new($false, $true).GetString($bytes, $offset, $bytes.Length - $offset)
    } catch {
        throw "EPIC_CHILD_LAUNCH_BLOCKED: generated profile is not valid UTF-8: $profilePath"
    }
    return [pscustomobject]@{
        Path = $profilePath; Raw = $raw
        Sha256 = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    }
}

function Get-CodexChildRuntimeContext {
    [CmdletBinding()]
    param([Parameter(Mandatory)] $Spec, [Parameter(Mandatory)][string] $RepositoryRoot)
    $rootTop = Get-CodexChildGitScalar -GitArgs @('-C', $RepositoryRoot, 'rev-parse', '--show-toplevel')
    $rootTop = Get-CodexChildCanonicalPath -Path $rootTop -BasePath $RepositoryRoot
    if ($rootTop -ne $RepositoryRoot) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: RepositoryRoot must be the trusted Git worktree root.'
    }
    $rootCommon = Get-CodexChildCommonDirectory -WorktreePath $RepositoryRoot
    $rootHead = Get-CodexChildGitScalar -GitArgs @('-C', $RepositoryRoot, 'rev-parse', 'HEAD')
    $rootBranch = Get-CodexChildGitScalar -GitArgs @('-C', $RepositoryRoot, 'branch', '--show-current')
    if ($rootBranch -cne [string]$Spec.integration_branch) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: RepositoryRoot must be checked out on the integration branch.'
    }
    $rootStatus = @(Invoke-CodexChildGit -GitArgs @(
            '-C', $RepositoryRoot, 'status', '--porcelain=v1', '--untracked-files=all'
        ))
    if (-not (Test-CodexChildCustomizationClean -StatusLines $rootStatus)) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: RepositoryRoot must be completely clean at its committed integration tip.'
    }
    $rootSurfaces = Get-CodexChildSurfaceObjectMap -RepositoryPath $RepositoryRoot -Commit $rootHead
    $integrationRef = "refs/heads/$([string]$Spec.integration_branch)"
    $integrationHead = Get-CodexChildGitScalar -GitArgs @(
        '-C', $RepositoryRoot, 'rev-parse', '--verify', $integrationRef
    )
    if ($rootHead -ne $integrationHead) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: RepositoryRoot HEAD must equal the integration branch tip.'
    }
    $profiles = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
    $pathComparer = if ($IsWindows) { [System.StringComparer]::OrdinalIgnoreCase } else { [System.StringComparer]::Ordinal }
    $branches = [System.Collections.Generic.Dictionary[string, string]]::new($pathComparer)
    foreach ($entry in @($Spec.launches)) {
        if (-not (Test-CodexChildActiveFeatureFolder -Path ([string]$entry.feature_folder))) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: feature_folder is not a final active feature identity: $($entry.feature_folder)"
        }
        if ([string]$entry.deployment_agent -notmatch '^[a-z0-9][a-z0-9._-]{0,79}$') {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: deployment_agent is not a safe generated profile name: $($entry.deployment_agent)"
        }
        $worktree = Get-CodexChildCanonicalPath -Path ([string]$entry.worktree_path) -BasePath $RepositoryRoot
        if ($worktree -eq $RepositoryRoot -or -not (Test-Path -LiteralPath $worktree -PathType Container)) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: child path must be a distinct existing worktree: $worktree"
        }
        $activeRoot = Get-CodexChildCanonicalPath -Path 'docs/features/active' -BasePath $RepositoryRoot
        $rootFeatureFolder = Get-CodexChildCanonicalPath -Path ([string]$entry.feature_folder) -BasePath $RepositoryRoot
        $childFeatureFolder = Get-CodexChildCanonicalPath -Path ([string]$entry.feature_folder) -BasePath $worktree
        $folderComparison = if ($IsWindows) { [System.StringComparison]::OrdinalIgnoreCase } else { [System.StringComparison]::Ordinal }
        if (-not $rootFeatureFolder.StartsWith($activeRoot + [System.IO.Path]::DirectorySeparatorChar, $folderComparison) -or
            -not (Test-Path -LiteralPath $rootFeatureFolder -PathType Container) -or
            -not (Test-Path -LiteralPath $childFeatureFolder -PathType Container)) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: final active feature_folder must exist in RepositoryRoot and child HEAD: $($entry.feature_folder)"
        }
        $top = Get-CodexChildGitScalar -GitArgs @('-C', $worktree, 'rev-parse', '--show-toplevel')
        if ((Get-CodexChildCanonicalPath -Path $top -BasePath $RepositoryRoot) -ne $worktree) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: path is not the root of its Git worktree: $worktree"
        }
        if ((Get-CodexChildCommonDirectory -WorktreePath $worktree) -ne $rootCommon) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: child does not share RepositoryRoot's Git common directory: $worktree"
        }
        $branch = Get-CodexChildGitScalar -GitArgs @('-C', $worktree, 'branch', '--show-current')
        if ($branch -cne [string]$entry.branch_name) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: child branch does not match the launch entry: $worktree"
        }
        $status = @(Invoke-CodexChildGit -GitArgs @(
                '-C', $worktree, 'status', '--porcelain=v1', '--untracked-files=all'
            ))
        if (-not (Test-CodexChildCustomizationClean -StatusLines $status)) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: child worktree must be completely clean: $worktree"
        }
        $childHead = Get-CodexChildGitScalar -GitArgs @('-C', $worktree, 'rev-parse', 'HEAD')
        if (-not (Test-CodexChildGit -GitArgs @(
                    '-C', $worktree, 'merge-base', '--is-ancestor', $integrationHead, $childHead
                ))) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: child HEAD does not descend from the integration branch: $worktree"
        }
        $childSurfaces = Get-CodexChildSurfaceObjectMap -RepositoryPath $worktree -Commit $childHead
        if (-not (Test-CodexChildSurfaceObjectsEqual -Expected $rootSurfaces -Actual $childSurfaces)) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: child customizations differ from trusted RepositoryRoot HEAD: $worktree"
        }
        $agentName = [string]$entry.deployment_agent
        $profileData = Get-CodexChildTrustedProfile -WorktreePath $worktree -ChildHead $childHead -AgentName $agentName
        $profilePath = [string]$profileData.Path; $profileRaw = [string]$profileData.Raw
        $agentProfile = ConvertFrom-CodexAgentProfile -ProfileRaw $profileRaw
        $metadata = [ordered]@{
            profile_path            = $profilePath
            profile_sha256          = [string]$profileData.Sha256
            worktree_path           = $worktree
            child_head              = $childHead
            trusted_repository_root = $RepositoryRoot
            trusted_repository_head = $rootHead
            git_common_directory    = $rootCommon
            integration_head        = $integrationHead
            trusted_surface_objects = $rootSurfaces
            trusted_surface_sha256  = Get-CodexChildSurfaceFingerprint -SurfaceObjects $rootSurfaces
        }
        foreach ($item in $metadata.GetEnumerator()) {
            $agentProfile | Add-Member -NotePropertyName $item.Key -NotePropertyValue $item.Value
        }
        $profileKey = Get-CodexChildProfileKey -WorktreePath $worktree -AgentName $agentName `
            -RepositoryRoot $RepositoryRoot
        $profiles[$profileKey] = $agentProfile
        $branches[$worktree] = $branch
    }
    return [pscustomobject]@{ Profiles = $profiles; Branches = $branches }
}

function ConvertTo-CodexChildTomlString {
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $Value)
    $escaped = $Value.Replace('\', '\\').Replace('"', '\"').Replace("`b", '\b').
    Replace("`t", '\t').Replace("`n", '\n').Replace("`f", '\f').Replace("`r", '\r')
    return '"' + $escaped + '"'
}

function Get-CodexChildCommandContext {
    [CmdletBinding()]
    param()
    $commands = @(Get-Command codex -All -ErrorAction Stop)
    foreach ($name in @('OPENAI_API_KEY', 'AZURE_OPENAI_API_KEY', 'CODEX_API_KEY', 'OPENAI_ACCESS_TOKEN')) {
        if (-not [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: environment-based Codex authentication is not supported for isolated children: $name"
        }
    }
    $commandPath = [string]$commands[0].Source
    $pathComparer = if ($IsWindows) { [System.StringComparer]::OrdinalIgnoreCase } else { [System.StringComparer]::Ordinal }
    $denied = [System.Collections.Generic.HashSet[string]]::new($pathComparer)
    foreach ($command in $commands) {
        if (-not [string]::IsNullOrWhiteSpace([string]$command.Source)) {
            $denied.Add([System.IO.Path]::GetFullPath([string]$command.Source)) | Out-Null
        }
    }
    $commandRoot = Split-Path $commandPath -Parent
    $packageRoot = Join-Path $commandRoot 'node_modules/@openai/codex'
    if (Test-Path -LiteralPath $packageRoot -PathType Container) {
        $denied.Add([System.IO.Path]::GetFullPath($packageRoot)) | Out-Null
    }
    $codexHome = if ([string]::IsNullOrWhiteSpace([string]$env:CODEX_HOME)) {
        Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)) '.codex'
    } else {
        [string]$env:CODEX_HOME
    }
    $authPath = Join-Path $codexHome 'auth.json'
    if (-not (Test-Path -LiteralPath $authPath -PathType Leaf)) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: isolated children require file-based Codex authentication under CODEX_HOME.'
    }
    $denied.Add([System.IO.Path]::GetFullPath($authPath)) | Out-Null
    $denied.Add([System.IO.Path]::GetFullPath($codexHome)) | Out-Null
    foreach ($name in @('app-server-control', 'app-server-daemon')) {
        $path = Join-Path $codexHome $name
        if (Test-Path -LiteralPath $path) {
            $denied.Add([System.IO.Path]::GetFullPath($path)) | Out-Null
        }
    }
    return [pscustomobject]@{
        CommandPath      = $commandPath
        DeniedPaths      = [string[]]@($denied | Sort-Object)
        CredentialRoot   = [System.IO.Path]::GetFullPath($codexHome)
        OriginalAuthPath = [System.IO.Path]::GetFullPath($authPath)
    }
}

function Get-CodexChildPermissionOverride {
    [OutputType([string])]
    param([Parameter(Mandatory)][string[]] $DeniedPaths)
    $entries = foreach ($path in $DeniedPaths) {
        $key = ConvertTo-CodexChildTomlString -Value $path
        "$key = 'deny'"
    }
    return "permissions.epic-child-workspace={ extends='orchestrator-workspace', filesystem={ $($entries -join ', ') } }"
}

function Get-CodexChildProjectsOverride {
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $WorktreePath)
    $path = ConvertTo-CodexChildTomlString -Value $WorktreePath
    return "projects={ $path = { trust_level='trusted' } }"
}

function Get-CodexChildAuthorityRoot {
    [OutputType([string])]
    param()
    $base = if ($IsWindows) { [Environment]::GetFolderPath('LocalApplicationData') } else {
        [System.IO.Path]::GetTempPath()
    }
    return [System.IO.Path]::GetFullPath((Join-Path $base 'OpenAI/CodexEpicChildren'))
}

function New-CodexChildIsolatedHome {
    [CmdletBinding(SupportsShouldProcess)]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $WaveId,
        [Parameter(Mandatory)][string] $LaunchId,
        [Parameter(Mandatory)][string] $OriginalAuthPath,
        [scriptblock] $PathExists = { param([string] $Path) Test-Path -LiteralPath $Path },
        [scriptblock] $CreateDirectory = { param([string] $Path) [System.IO.Directory]::CreateDirectory($Path) | Out-Null },
        [scriptblock] $CopyFile = { param([string] $Source, [string] $Destination) [System.IO.File]::Copy($Source, $Destination, $false) },
        [scriptblock] $DeleteDirectory = { param([string] $Path) [System.IO.Directory]::Delete($Path, $true) },
        [scriptblock] $AssertAuthority = {
            param([string] $Authority, [string] $Repository)
            Assert-CodexChildAuthorityOutsideRepository -AuthorityPath $Authority -RepositoryRoot $Repository
        }
    )
    $repositoryKey = Get-CodexChildSha256 -Value $RepositoryRoot
    $isolatedHome = [System.IO.Path]::GetFullPath((Join-Path (Get-CodexChildAuthorityRoot) `
                "$repositoryKey/$WaveId/$LaunchId"))
    & $AssertAuthority $isolatedHome $RepositoryRoot
    if (& $PathExists $isolatedHome) {
        throw "EPIC_CHILD_LAUNCH_BLOCKED: isolated CODEX_HOME already exists: $isolatedHome"
    }
    if ($PSCmdlet.ShouldProcess($isolatedHome, 'Create isolated epic-child CODEX_HOME')) {
        try {
            & $CreateDirectory $isolatedHome
            & $CopyFile $OriginalAuthPath (Join-Path $isolatedHome 'auth.json')
        } catch {
            $creationError = $_
            try {
                if (& $PathExists $isolatedHome) {
                    & $DeleteDirectory $isolatedHome
                }
            } catch {
                throw "EPIC_CHILD_LAUNCH_BLOCKED: isolated CODEX_HOME creation failed and cleanup also failed: $creationError Cleanup: $_"
            }
            throw $creationError
        }
    }
    return $isolatedHome
}

function Restore-CodexChildIsolatedAuth {
    [CmdletBinding(SupportsShouldProcess)]
    param([Parameter(Mandatory)][string] $HomePath, [Parameter(Mandatory)][string] $OriginalAuthPath)
    $target = Join-Path $HomePath 'auth.json'
    if (Test-Path -LiteralPath $target) {
        throw 'EPIC_CHILD_RESUME_BLOCKED: isolated auth copy already exists before resume.'
    }
    if ($PSCmdlet.ShouldProcess($target, 'Restore isolated Codex authentication')) {
        $source = $null; $destination = $null; $ownsDestination = $false
        try {
            $source = [System.IO.File]::OpenRead($OriginalAuthPath)
            $destination = [System.IO.File]::Open($target, [System.IO.FileMode]::CreateNew,
                [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
            $ownsDestination = $true
            $source.CopyTo($destination); $destination.Flush($true)
        } catch {
            $copyError = $_
            if ($null -ne $destination) { $destination.Dispose(); $destination = $null }
            if ($null -ne $source) { $source.Dispose(); $source = $null }
            if ($ownsDestination -and (Test-Path -LiteralPath $target)) {
                [System.IO.File]::Delete($target)
            }
            throw $copyError
        } finally {
            if ($null -ne $destination) { $destination.Dispose() }
            if ($null -ne $source) { $source.Dispose() }
        }
    }
}

function Remove-CodexChildIsolatedAuth {
    [CmdletBinding(SupportsShouldProcess)]
    param([Parameter(Mandatory)][string] $HomePath)
    $target = Join-Path $HomePath 'auth.json'
    if ((Test-Path -LiteralPath $target) -and $PSCmdlet.ShouldProcess($target, 'Remove isolated Codex authentication')) {
        [System.IO.File]::Delete($target)
    }
}

function Remove-CodexChildIsolatedHome {
    [CmdletBinding(SupportsShouldProcess)]
    param([Parameter(Mandatory)][string] $HomePath)
    $authorityRoot = Get-CodexChildAuthorityRoot
    $canonical = [System.IO.Path]::GetFullPath($HomePath)
    if (-not $canonical.StartsWith($authorityRoot + [System.IO.Path]::DirectorySeparatorChar,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: refusing to remove a CODEX_HOME outside launcher authority.'
    }
    if ((Test-Path -LiteralPath $canonical) -and
        $PSCmdlet.ShouldProcess($canonical, 'Remove failed epic-child CODEX_HOME')) {
        [System.IO.Directory]::Delete($canonical, $true)
    }
}

function Get-CodexChildShellEnvironmentOverrideList {
    [OutputType([string[]])]
    param([Parameter(Mandatory)][string] $WorktreePath)
    $isolatedHome = Join-Path $WorktreePath '.codex/epic-child-no-auth'
    return @(
        'shell_environment_policy.exclude=["OPENAI_API_KEY","AZURE_OPENAI_API_KEY","CODEX_API_KEY","OPENAI_ACCESS_TOKEN"]',
        "shell_environment_policy.set={ CODEX_HOME=$(ConvertTo-CodexChildTomlString -Value $isolatedHome) }"
    )
}

. (Join-Path $PSScriptRoot 'epic-child-persistence-runtime.ps1')
. (Join-Path $PSScriptRoot 'epic-child-sandbox-preflight.ps1')
