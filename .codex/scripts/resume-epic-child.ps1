# Resume one launcher-authorized Codex epic child with its sealed runtime controls.
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)][string] $ReceiptPath,
    [AllowEmptyString()][string] $Prompt = '',
    [AllowEmptyString()][string] $LastMessagePath = ''
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'epic-child-launch-contract.ps1')
. (Join-Path $PSScriptRoot 'epic-child-launch-runtime.ps1')

function Test-CodexChildResumeSpecEntry {
    [OutputType([string[]])]
    param([Parameter(Mandatory)] $Receipt, [Parameter(Mandatory)] $Spec)
    $errors = [System.Collections.Generic.List[string]]::new()
    $entry = @($Spec.launches | Where-Object { [string]$_.launch_id -eq [string]$Receipt.launch_id })
    if ($entry.Count -ne 1) {
        return @('sealed launch specification must contain the receipt launch_id exactly once.')
    }
    $entry = $entry[0]
    $pairs = @(
        @('delegation_id', 'delegation_id'), @('feature_folder', 'feature_folder'),
        @('deployment_agent', 'deployment_agent'), @('model', 'model'),
        @('model_reasoning_effort', 'model_reasoning_effort'), @('permissions', 'permissions'),
        @('execution_context', 'execution_context'), @('worktree_path', 'worktree_path'),
        @('branch_name', 'branch_name')
    )
    foreach ($pair in $pairs) {
        if ([string]$entry.($pair[0]) -cne [string]$Receipt.($pair[1])) {
            $errors.Add("sealed launch specification $($pair[0]) differs from the receipt.")
        }
    }
    if (-not (Test-CodexChildIssueEqual -Left $entry.issue_num -Right $Receipt.issue_num)) {
        $errors.Add('sealed launch specification issue_num differs from the receipt.')
    }
    if ((Get-CodexChildSha256 -Value ([string]$entry.prompt)) -ne [string]$Receipt.prompt_sha256) {
        $errors.Add('sealed launch specification prompt hash differs from the receipt.')
    }
    return $errors.ToArray()
}

function Get-CodexChildResumeContext {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Path)
    if (-not [System.IO.Path]::IsPathFullyQualified($Path)) {
        throw 'EPIC_CHILD_RESUME_BLOCKED: ReceiptPath must be an absolute path.'
    }
    $canonicalReceipt = Get-CodexChildCanonicalPath -Path $Path -BasePath (Get-Location).Path
    $receipt = ConvertFrom-CodexChildLaunchJson -Raw (Get-Content -Raw -LiteralPath $canonicalReceipt) -Name 'launch receipt'
    $errors = [System.Collections.Generic.List[string]]::new()
    $lifecycleErrors = if ([string]$receipt.state -eq 'active') {
        @(Get-CodexChildActiveReceiptErrorList -Receipt $receipt)
    } else {
        @(Get-CodexChildTerminalReceiptErrorList -Receipt $receipt)
    }
    foreach ($receiptError in $lifecycleErrors) {
        $errors.Add($receiptError)
    }
    if ((Get-CodexChildCanonicalPath -Path ([string]$receipt.receipt_path) -BasePath (Get-Location).Path) -ne $canonicalReceipt) {
        $errors.Add('receipt_path does not identify the loaded sealed receipt.')
    }
    $worktree = Get-CodexChildCanonicalPath -Path ([string]$receipt.worktree_path) `
        -BasePath ([string]$receipt.trusted_repository_root)
    $top = Get-CodexChildGitScalar -GitArgs @('-C', $worktree, 'rev-parse', '--show-toplevel')
    if ((Get-CodexChildCanonicalPath -Path $top -BasePath $worktree) -ne $worktree) {
        $errors.Add('worktree_path is not the exact Git worktree root.')
    }
    if ((Get-CodexChildCommonDirectory -WorktreePath $worktree) -ne [string]$receipt.git_common_directory) {
        $errors.Add('worktree no longer shares the sealed Git common directory.')
    }
    $branch = Get-CodexChildGitScalar -GitArgs @('-C', $worktree, 'branch', '--show-current')
    if ($branch -cne [string]$receipt.branch_name) { $errors.Add('live branch differs from the sealed receipt.') }
    $head = Get-CodexChildGitScalar -GitArgs @('-C', $worktree, 'rev-parse', 'HEAD')
    foreach ($ancestor in @([string]$receipt.integration_head, [string]$receipt.child_head)) {
        if (-not (Test-CodexChildGit -GitArgs @('-C', $worktree, 'merge-base', '--is-ancestor', $ancestor, $head))) {
            $errors.Add('worktree HEAD no longer descends from the sealed launch ancestry.')
        }
    }
    $trustedObjects = Get-CodexChildSurfaceObjectMap -RepositoryPath ([string]$receipt.trusted_repository_root) `
        -Commit ([string]$receipt.trusted_repository_head)
    $currentObjects = Get-CodexChildSurfaceObjectMap -RepositoryPath $worktree -Commit $head
    if (-not (Test-CodexChildSurfaceObjectsEqual -Expected $receipt.trusted_surface_objects -Actual $trustedObjects) -or
        -not (Test-CodexChildSurfaceObjectsEqual -Expected $receipt.trusted_surface_objects -Actual $currentObjects) -or
        (Get-CodexChildSurfaceFingerprint -SurfaceObjects $currentObjects) -ne [string]$receipt.trusted_surface_sha256) {
        $errors.Add('trusted Codex instructions, skills, configuration, or routing surfaces have changed.')
    }
    $surfaceStatus = @(Invoke-CodexChildGit -GitArgs @(
            '-C', $worktree, 'status', '--porcelain=v1', '--untracked-files=all', '--',
            '.codex', '.agents', 'AGENTS.md', 'config/orchestration-routing.json'
        ))
    if (-not (Test-CodexChildCustomizationClean -StatusLines $surfaceStatus)) {
        $errors.Add('trusted customization surfaces are not clean against worktree HEAD.')
    }
    $profilePath = Get-CodexChildCanonicalPath -Path ([string]$receipt.profile_path) -BasePath $worktree
    $expectedProfile = Get-CodexChildCanonicalPath -Path ".codex/agents/$($receipt.deployment_agent).toml" -BasePath $worktree
    $profileRaw = Get-Content -Raw -LiteralPath $profilePath
    $agentProfile = ConvertFrom-CodexAgentProfile -ProfileRaw $profileRaw
    if ($profilePath -ne $expectedProfile -or
        (Get-FileHash -LiteralPath $profilePath -Algorithm SHA256).Hash.ToLowerInvariant() -cne [string]$receipt.profile_sha256 -or
        [string]$agentProfile.name -cne [string]$receipt.deployment_agent -or [string]$agentProfile.model -cne [string]$receipt.model -or
        [string]$agentProfile.model_reasoning_effort -cne [string]$receipt.model_reasoning_effort -or
        [string]$agentProfile.default_permissions -cne [string]$receipt.permissions -or
        (Get-CodexChildSha256 -Value ([string]$agentProfile.developer_instructions) -ne [string]$receipt.developer_instructions_sha256) -or
        (Get-CodexChildSha256 -Value ([string]$agentProfile.skills_config) -ne [string]$receipt.skills_config_sha256)) {
        $errors.Add('checked-in generated deployment profile differs from the sealed receipt.')
    }
    $specPath = Get-CodexChildCanonicalPath -Path ([string]$receipt.spec_path) -BasePath ([string]$receipt.trusted_repository_root)
    if ((Get-FileHash -LiteralPath $specPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne [string]$receipt.spec_sha256) {
        $errors.Add('sealed launch specification hash has changed.')
    }
    $spec = ConvertFrom-CodexChildLaunchJson -Raw (Get-Content -Raw -LiteralPath $specPath) -Name 'launch spec'
    if ([string]$receipt.checkpoint_kind -cne [string]$spec.checkpoint_kind) {
        $errors.Add('checkpoint_kind differs from the sealed launch specification.')
    }
    $expectedLockPath = Get-CodexChildSemanticWaveLockPath -Spec $spec -ArtifactRoot (Split-Path $specPath -Parent)
    $receiptLockPath = Get-CodexChildCanonicalPath -Path ([string]$receipt.wave_lock_path) `
        -BasePath ([string]$receipt.trusted_repository_root)
    if ($receiptLockPath -cne $expectedLockPath) {
        $errors.Add('wave_lock_path does not match the sealed semantic wave identity.')
    }
    foreach ($specError in @(Test-CodexChildResumeSpecEntry -Receipt $receipt -Spec $spec)) {
        $errors.Add($specError)
    }
    $codexRuntime = Get-CodexChildCommandContext
    $codexHome = Get-CodexChildCanonicalPath -Path ([string]$receipt.codex_home_path) `
        -BasePath (Get-CodexChildAuthorityRoot)
    if (-not $codexHome.StartsWith((Get-CodexChildAuthorityRoot) + [System.IO.Path]::DirectorySeparatorChar,
            [System.StringComparison]::OrdinalIgnoreCase) -or -not (Test-Path -LiteralPath $codexHome -PathType Container)) {
        $errors.Add('isolated CODEX_HOME is missing or outside launcher authority.')
    }
    $expectedDeniedPaths = [string[]]@($codexRuntime.DeniedPaths) + @($codexHome)
    if ([string]$codexRuntime.CommandPath -ne [string]$receipt.codex_command_path -or
        (@($expectedDeniedPaths) | ConvertTo-Json -Compress) -ne (@($receipt.codex_denied_paths) | ConvertTo-Json -Compress)) {
        $errors.Add('Codex executable or package paths differ from the sealed runtime deny set.')
    }
    if ($errors.Count -gt 0) { throw "EPIC_CHILD_RESUME_BLOCKED: $($errors -join ' ')" }
    return [pscustomobject]@{ Receipt = $receipt; Profile = $agentProfile; CodexRuntime = $codexRuntime }
}

function Get-CodexChildResumeStartInfo {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Context,
        [AllowEmptyString()][string] $ResumePrompt = '',
        [AllowEmptyString()][string] $OutputPath = ''
    )
    $receipt = $Context.Receipt; $agentProfile = $Context.Profile
    $commandPath = [string]$Context.CodexRuntime.CommandPath
    $isPowerShellShim = $commandPath.EndsWith('.ps1', [System.StringComparison]::OrdinalIgnoreCase)
    $info = [System.Diagnostics.ProcessStartInfo]::new($(if ($isPowerShellShim) { 'pwsh' } else { $commandPath }))
    $info.UseShellExecute = $false; $info.WorkingDirectory = [string]$receipt.worktree_path
    if ($isPowerShellShim) { foreach ($item in @('-NoProfile', '-File', $commandPath)) { $info.ArgumentList.Add($item) } }
    $permissionOverride = Get-CodexChildPermissionOverride -DeniedPaths ([string[]]$receipt.codex_denied_paths)
    $projectsOverride = Get-CodexChildProjectsOverride -WorktreePath ([string]$receipt.worktree_path)
    $shellOverrides = Get-CodexChildShellEnvironmentOverrideList -WorktreePath ([string]$receipt.worktree_path)
    foreach ($item in @(
            'exec', 'resume', '--ignore-user-config', '-c', $projectsOverride, '-c', $permissionOverride,
            '-c', $shellOverrides[0], '-c', $shellOverrides[1],
            '-m', [string]$receipt.model, '-c', "model_reasoning_effort=$($receipt.model_reasoning_effort)",
            '-c', 'default_permissions="epic-child-workspace"', '-c', 'approval_policy="never"',
            '-c', ('developer_instructions=' + ([string]$agentProfile.developer_instructions | ConvertTo-Json -Compress)),
            '-c', "skills.config=$($agentProfile.skills_config)", '--strict-config',
            '--dangerously-bypass-hook-trust', '--json'
        )) { $info.ArgumentList.Add($item) }
    if ($IsWindows) { $info.ArgumentList.Insert(2, 'windows.sandbox="elevated"'); $info.ArgumentList.Insert(2, '-c') }
    if (-not [string]::IsNullOrWhiteSpace($OutputPath)) { $info.ArgumentList.Add('-o'); $info.ArgumentList.Add($OutputPath) }
    $info.ArgumentList.Add([string]$receipt.codex_session_id)
    if (-not [string]::IsNullOrWhiteSpace($ResumePrompt)) { $info.ArgumentList.Add($ResumePrompt) }
    $environment = @{
        CODEX_EPIC_CHILD_LAUNCH_ID         = [string]$receipt.launch_id
        CODEX_EPIC_CHILD_LAUNCH_RECEIPT    = [string]$receipt.receipt_path
        CODEX_EPIC_CHILD_LAUNCH_SPEC       = [string]$receipt.spec_path
        CODEX_EPIC_CHILD_EXPECTED_WORKTREE = [string]$receipt.worktree_path
        CODEX_EPIC_CHILD_DELEGATION_ID     = [string]$receipt.delegation_id
        CODEX_EPIC_CHILD_EXECUTION_CONTEXT = [string]$receipt.execution_context
        CODEX_EPIC_CHILD_AGENT             = [string]$receipt.deployment_agent
        CODEX_EPIC_CHILD_MODEL             = [string]$receipt.model
        CODEX_EPIC_CHILD_REASONING_EFFORT  = [string]$receipt.model_reasoning_effort
        CODEX_EPIC_CHILD_PROFILE_SHA256    = [string]$receipt.profile_sha256
        CODEX_EPIC_CHILD_SESSION_ID        = [string]$receipt.codex_session_id
    }
    foreach ($item in $environment.GetEnumerator()) { $info.Environment[$item.Key] = $item.Value }
    $info.Environment['CODEX_HOME'] = [string]$receipt.codex_home_path
    return $info
}

function Set-CodexChildResumeWaveStatus {
    [CmdletBinding(SupportsShouldProcess)]
    param([Parameter(Mandatory)] $Receipt)
    if (-not $PSCmdlet.ShouldProcess([string]$Receipt.status_path, 'Update epic-child wave status from resume receipt')) {
        return
    }
    $status = ConvertFrom-CodexChildLaunchJson -Raw `
    (Get-Content -Raw -LiteralPath ([string]$Receipt.status_path)) -Name 'wave status'
    $property = $status.launches.PSObject.Properties[[string]$Receipt.launch_id]
    if ($null -eq $property) { throw 'EPIC_CHILD_RESUME_BLOCKED: wave status lacks the receipt launch_id.' }
    $entry = $property.Value
    $entry.state = [string]$Receipt.state; $entry.codex_session_id = [string]$Receipt.codex_session_id
    $entry.receipt_path = [string]$Receipt.receipt_path
    if ([string]$Receipt.state -eq 'completed') {
        $entry.exit_code = 0; $entry.completed_at = [string]$Receipt.completed_at
    } elseif ([string]$Receipt.state -eq 'failed') {
        $entry.exit_code = [int]$Receipt.exit_code; $entry.failed_at = [string]$Receipt.failed_at
    }
    $status.updated_at = [datetimeoffset]::UtcNow.ToString('o')
    Write-CodexChildJsonAtomic -Path ([string]$Receipt.status_path) -Value $status
}

if ($MyInvocation.InvocationName -eq '.') { return }
$context = Get-CodexChildResumeContext -Path $ReceiptPath
$outputPath = if ([string]::IsNullOrWhiteSpace($LastMessagePath)) {
    Join-Path (Split-Path ([string]$context.Receipt.receipt_path) -Parent) "$($context.Receipt.launch_id).resume.last-message.txt"
} else { Get-CodexChildCanonicalPath -Path $LastMessagePath -BasePath ([string]$context.Receipt.worktree_path) }
if (-not $PSCmdlet.ShouldProcess([string]$context.Receipt.worktree_path, "Resume Codex epic child $($context.Receipt.launch_id)")) { return }
$receipt = $context.Receipt
$waveLock = Enter-CodexChildWaveLock -Path ([string]$receipt.wave_lock_path)
$authOwned = $false
$process = $null; $processStarted = $false
try {
    Restore-CodexChildIsolatedAuth -HomePath ([string]$receipt.codex_home_path) `
        -OriginalAuthPath ([string]$context.CodexRuntime.OriginalAuthPath) -Confirm:$false
    $authOwned = $true
    $probeInfo = Get-CodexChildSandboxProbeStartInfo -WorktreePath ([string]$receipt.worktree_path) `
        -CodexHomePath ([string]$receipt.codex_home_path) -CommandPath ([string]$context.CodexRuntime.CommandPath) `
        -DeniedPaths ([string[]]$receipt.codex_denied_paths) `
        -DeniedProbePath ([string]$context.CodexRuntime.OriginalAuthPath)
    Assert-CodexChildSandboxPreflight -StartInfo $probeInfo
    Set-CodexChildReceiptState -Receipt $receipt -State launching
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = Get-CodexChildResumeStartInfo -Context $context -ResumePrompt $Prompt -OutputPath $outputPath
    if (-not $process.Start()) { throw 'failed to start Codex resume process.' }
    $processStarted = $true
    Set-CodexChildReceiptState -Receipt $receipt -State active -SessionId ([string]$receipt.codex_session_id)
    $process.WaitForExit()
    if ($process.ExitCode -eq 0) {
        Set-CodexChildReceiptState -Receipt $receipt -State completed -ExitCode 0
    } else {
        Set-CodexChildReceiptState -Receipt $receipt -State failed -ExitCode $process.ExitCode `
            -FailureReason 'Codex resume process returned a nonzero exit code.'
    }
    Set-CodexChildResumeWaveStatus -Receipt $receipt
    $exitCode = $process.ExitCode
} catch {
    if ($processStarted -and -not $process.HasExited) {
        $process.Kill($true); $process.WaitForExit()
    }
    if ([string]$receipt.state -in @('launching', 'active')) {
        $failureExitCode = if ($processStarted -and $process.HasExited) { [int]$process.ExitCode } else { -1 }
        Set-CodexChildReceiptState -Receipt $receipt -State failed -ExitCode $failureExitCode -FailureReason ([string]$_)
        Set-CodexChildResumeWaveStatus -Receipt $receipt
    }
    throw "EPIC_CHILD_RESUME_BLOCKED: $_"
} finally {
    try {
        if ($authOwned) {
            Remove-CodexChildIsolatedAuth -HomePath ([string]$receipt.codex_home_path) -Confirm:$false
        }
    } finally {
        $waveLock.Dispose()
    }
}
exit $exitCode
