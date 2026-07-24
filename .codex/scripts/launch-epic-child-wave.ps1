# Launch a bounded wave of worktree-bound Codex processes with sealed receipts and status.
[CmdletBinding()]
param(
    [AllowEmptyString()][string] $LaunchSpecPath = '',
    [ValidateRange(1, 8)][int] $MaxParallel = 4,
    [switch] $Supervisor,
    [switch] $Wait,
    [AllowEmptyString()][string] $RepositoryRoot = ''
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'epic-child-launch-contract.ps1')
. (Join-Path $PSScriptRoot 'epic-child-launch-runtime.ps1')

function Get-CodexChildSessionId {
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][string[]] $JsonLines)
    foreach ($line in $JsonLines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }
        try {
            $codexEvent = $line | ConvertFrom-Json -Depth 16 -ErrorAction Stop
        } catch {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: codex --json emitted malformed JSON: $_"
        }
        foreach ($candidate in @(
                $codexEvent.thread_id, $codexEvent.session_id, $codexEvent.thread.id,
                $codexEvent.payload.thread_id, $codexEvent.payload.session_id
            )) {
            if (-not [string]::IsNullOrWhiteSpace([string]$candidate)) {
                return [string]$candidate
            }
        }
    }
    return ''
}

function Get-CodexChildSealedJsonFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $Name,
        [switch] $HoldOpen,
        [scriptblock] $OpenFile = {
            param([string] $FilePath)
            [System.IO.File]::Open($FilePath, [System.IO.FileMode]::Open,
                [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read)
        }
    )
    $stream = & $OpenFile $Path
    try {
        $memory = [System.IO.MemoryStream]::new()
        try { $stream.CopyTo($memory); $bytes = $memory.ToArray() } finally { $memory.Dispose() }
        $offset = if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { 3 } else { 0 }
        try {
            $raw = [System.Text.UTF8Encoding]::new($false, $true).GetString($bytes, $offset, $bytes.Length - $offset)
        } catch {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: $Name is not valid UTF-8."
        }
        $value = ConvertFrom-CodexChildLaunchJson -Raw $raw -Name $Name
        $sha256 = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        if (-not $HoldOpen) { $stream.Dispose(); $stream = $null }
        return [pscustomobject]@{ Value = $value; Sha256 = $sha256; Stream = $stream }
    } catch {
        $stream.Dispose()
        throw
    }
}

function Get-CodexChildLaunchReceipt {
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)] $Spec,
        [Parameter(Mandatory)] $Entry,
        [Parameter(Mandatory)] $AgentProfile,
        [Parameter(Mandatory)] $CodexRuntime,
        [Parameter(Mandatory)][string] $SpecPath,
        [Parameter(Mandatory)][string] $SpecSha256,
        [Parameter(Mandatory)][string] $CheckpointPath,
        [Parameter(Mandatory)][string] $CheckpointSha256,
        [Parameter(Mandatory)][string] $ReceiptPath,
        [Parameter(Mandatory)][string] $StatusPath,
        [Parameter(Mandatory)][string] $CodexHomePath,
        [Parameter(Mandatory)][string] $WaveLockPath
    )
    $authorizedAt = [datetimeoffset]::UtcNow
    return [ordered]@{
        schema_version = 2; state = 'launching'; codex_session_id = ''
        launch_id = [string]$Entry.launch_id; wave_id = [string]$Spec.wave_id
        checkpoint_kind = [string]$Spec.checkpoint_kind; wave_lock_path = $WaveLockPath
        wave_number = [int]$Spec.wave_number; max_parallel_features = [int]$Spec.max_parallel_features
        feature_folder = [string]$Entry.feature_folder; issue_num = $Entry.issue_num
        delegation_id = [string]$Entry.delegation_id; deployment_agent = [string]$Entry.deployment_agent
        model = [string]$Entry.model; model_reasoning_effort = [string]$Entry.model_reasoning_effort
        permissions = [string]$Entry.permissions; runtime_permissions = 'epic-child-workspace'
        execution_context             = [string]$Entry.execution_context
        worktree_path = [string]$AgentProfile.worktree_path; branch_name = [string]$Entry.branch_name
        integration_branch = [string]$Spec.integration_branch; integration_head = [string]$AgentProfile.integration_head
        child_head                    = [string]$AgentProfile.child_head
        trusted_repository_root       = [string]$AgentProfile.trusted_repository_root
        trusted_repository_head       = [string]$AgentProfile.trusted_repository_head
        git_common_directory          = [string]$AgentProfile.git_common_directory
        trusted_surface_objects       = $AgentProfile.trusted_surface_objects
        trusted_surface_sha256        = [string]$AgentProfile.trusted_surface_sha256
        prompt_sha256                 = Get-CodexChildSha256 -Value ([string]$Entry.prompt)
        profile_path                  = [string]$AgentProfile.profile_path
        profile_sha256                = [string]$AgentProfile.profile_sha256
        developer_instructions_sha256 = Get-CodexChildSha256 -Value ([string]$AgentProfile.developer_instructions)
        skills_config_sha256          = Get-CodexChildSha256 -Value ([string]$AgentProfile.skills_config)
        spec_path = $SpecPath; spec_sha256 = $SpecSha256
        checkpoint_path = $CheckpointPath; checkpoint_sha256 = $CheckpointSha256
        receipt_path = $ReceiptPath; status_path = $StatusPath; codex_home_path = $CodexHomePath
        codex_command_path            = [string]$CodexRuntime.CommandPath
        codex_denied_paths            = [string[]]$CodexRuntime.DeniedPaths
        authorized_at = $authorizedAt.ToString('o'); session_bound_at = ''
        expires_at                    = $authorizedAt.AddDays(7).ToString('o')
    }
}

function Get-CodexChildProcessStartInfo {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Entry,
        [Parameter(Mandatory)] $AgentProfile,
        [Parameter(Mandatory)] $Receipt,
        [Parameter(Mandatory)][string] $LastMessagePath
    )
    $codexCommandPath = [string]$Receipt.codex_command_path
    $isPowerShellShim = $codexCommandPath.EndsWith('.ps1', [System.StringComparison]::OrdinalIgnoreCase)
    $info = [System.Diagnostics.ProcessStartInfo]::new($(if ($isPowerShellShim) { 'pwsh' } else { $codexCommandPath }))
    $info.UseShellExecute = $false; $info.CreateNoWindow = $true
    $info.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    $info.RedirectStandardInput = $true; $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true; $info.WorkingDirectory = [string]$Entry.worktree_path
    if ($isPowerShellShim) {
        foreach ($argument in @('-NoProfile', '-File', $codexCommandPath)) { $info.ArgumentList.Add($argument) }
    }
    $permissionOverride = Get-CodexChildPermissionOverride -DeniedPaths ([string[]]$Receipt.codex_denied_paths)
    $projectsOverride = Get-CodexChildProjectsOverride -WorktreePath ([string]$Entry.worktree_path)
    $shellOverrides = Get-CodexChildShellEnvironmentOverrideList -WorktreePath ([string]$Entry.worktree_path)
    foreach ($argument in @(
            'exec', '--ignore-user-config', '-C', [string]$Entry.worktree_path,
            '-c', $projectsOverride, '-c', $permissionOverride,
            '-c', $shellOverrides[0], '-c', $shellOverrides[1],
            '-m', [string]$AgentProfile.model,
            '-c', "model_reasoning_effort=$($AgentProfile.model_reasoning_effort)",
            '-c', 'default_permissions="epic-child-workspace"', '-c', 'approval_policy="never"',
            '-c', ('developer_instructions=' + ([string]$AgentProfile.developer_instructions | ConvertTo-Json -Compress)),
            '-c', "skills.config=$($AgentProfile.skills_config)", '--strict-config',
            '--dangerously-bypass-hook-trust', '--json', '-o', $LastMessagePath, '-'
        )) { $info.ArgumentList.Add($argument) }
    if ($IsWindows) { $info.ArgumentList.Insert(1, 'windows.sandbox="elevated"'); $info.ArgumentList.Insert(1, '-c') }
    $environment = @{
        CODEX_EPIC_CHILD_LAUNCH_ID         = [string]$Receipt.launch_id
        CODEX_EPIC_CHILD_LAUNCH_RECEIPT    = [string]$Receipt.receipt_path
        CODEX_EPIC_CHILD_LAUNCH_SPEC       = [string]$Receipt.spec_path
        CODEX_EPIC_CHILD_EXPECTED_WORKTREE = [string]$Receipt.worktree_path
        CODEX_EPIC_CHILD_DELEGATION_ID     = [string]$Receipt.delegation_id
        CODEX_EPIC_CHILD_EXECUTION_CONTEXT = [string]$Receipt.execution_context
        CODEX_EPIC_CHILD_AGENT             = [string]$Receipt.deployment_agent
        CODEX_EPIC_CHILD_MODEL             = [string]$Receipt.model
        CODEX_EPIC_CHILD_REASONING_EFFORT  = [string]$Receipt.model_reasoning_effort
        CODEX_EPIC_CHILD_PROFILE_SHA256    = [string]$Receipt.profile_sha256
    }
    foreach ($item in $environment.GetEnumerator()) { $info.Environment[$item.Key] = $item.Value }
    $info.Environment['CODEX_HOME'] = [string]$Receipt.codex_home_path
    return $info
}

function Start-CodexChildProcess {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory)] $Entry,
        [Parameter(Mandatory)] $AgentProfile,
        [Parameter(Mandatory)] $Receipt,
        [Parameter(Mandatory)][string] $ArtifactRoot
    )
    $base = Join-Path $ArtifactRoot ([string]$Entry.launch_id)
    if (-not $PSCmdlet.ShouldProcess([string]$Entry.worktree_path, "Start Codex epic child $($Entry.launch_id)")) { return $null }
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = Get-CodexChildProcessStartInfo -Entry $Entry -AgentProfile $AgentProfile `
        -Receipt $Receipt -LastMessagePath "$base.last-message.txt"
    $started = $false
    try {
        if (-not $process.Start()) { throw "failed to start $($Entry.launch_id)." }
        $started = $true
        $errorTask = $process.StandardError.ReadToEndAsync()
        $process.StandardInput.Write([string]$Entry.prompt); $process.StandardInput.Close()
        $prefixLines = [System.Collections.Generic.List[string]]::new()
        $sessionId = ''; $deadline = [datetimeoffset]::UtcNow.AddSeconds(30)
        while ([string]::IsNullOrWhiteSpace($sessionId) -and $prefixLines.Count -lt 16) {
            $remaining = [Math]::Max(1, [int]($deadline - [datetimeoffset]::UtcNow).TotalMilliseconds)
            $readTask = $process.StandardOutput.ReadLineAsync()
            if (-not $readTask.Wait($remaining)) { throw 'session id timeout.' }
            if ($null -eq $readTask.Result) { break }
            $prefixLines.Add($readTask.Result)
            $sessionId = Get-CodexChildSessionId -JsonLines @($readTask.Result)
        }
        if ([string]::IsNullOrWhiteSpace($sessionId)) { throw 'no usable Codex session id was emitted.' }
        Set-CodexChildReceiptState -Receipt $Receipt -State active -SessionId $sessionId
        return [pscustomobject]@{
            Entry = $Entry; Process = $process; ErrorTask = $errorTask; SessionId = $sessionId
            OutputTask = $process.StandardOutput.ReadToEndAsync(); ExitTask = $process.WaitForExitAsync()
            OutputPrefix = [string]::Join([Environment]::NewLine, $prefixLines) + [Environment]::NewLine
            BasePath = $base; StartedAt = [datetimeoffset]::UtcNow.ToString('o'); Receipt = $Receipt
        }
    } catch {
        if ($started -and -not $process.HasExited) { $process.Kill($true) }
        Set-CodexChildReceiptState -Receipt $Receipt -State failed -FailureReason ([string]$_)
        throw "EPIC_CHILD_LAUNCH_BLOCKED: $($Entry.launch_id) failed during startup: $_"
    }
}

function Get-CodexChildTerminalStatusEntry {
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)] $Child, [Parameter(Mandatory)][string] $ReceiptPath)
    $wrapper = Join-Path ([string]$Child.Entry.worktree_path) '.codex/scripts/resume-epic-child.ps1'
    $quotedWrapper = "'" + $wrapper.Replace("'", "''") + "'"
    $quotedReceipt = "'" + $ReceiptPath.Replace("'", "''") + "'"
    $status = [ordered]@{
        state              = $(if ($Child.Process.ExitCode -eq 0) { 'completed' } else { 'failed' })
        pid = $Child.Process.Id; exit_code = $Child.Process.ExitCode
        codex_session_id = $Child.SessionId; receipt_path = $ReceiptPath
        resume_script_path = $wrapper
        resume_command     = "& $quotedWrapper -ReceiptPath $quotedReceipt"
        stdout_path = "$($Child.BasePath).stdout.jsonl"; stderr_path = "$($Child.BasePath).stderr.log"
    }
    if ($Child.Process.ExitCode -eq 0) {
        $status.completed_at = [string]$Child.Receipt.completed_at
    } else {
        $status.failed_at = [string]$Child.Receipt.failed_at
    }
    return $status
}

function Get-CodexChildResumeStatus {
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)] $Child, [Parameter(Mandatory)][string] $ReceiptPath)
    [System.IO.File]::WriteAllText("$($Child.BasePath).stdout.jsonl", $Child.OutputPrefix + $Child.OutputTask.Result)
    [System.IO.File]::WriteAllText("$($Child.BasePath).stderr.log", $Child.ErrorTask.Result)
    if ($Child.Process.ExitCode -eq 0) {
        Set-CodexChildReceiptState -Receipt $Child.Receipt -State completed -ExitCode 0
    } else {
        Set-CodexChildReceiptState -Receipt $Child.Receipt -State failed `
            -ExitCode $Child.Process.ExitCode -FailureReason 'Codex child process returned a nonzero exit code.'
    }
    Remove-CodexChildIsolatedAuth -HomePath ([string]$Child.Receipt.codex_home_path) -Confirm:$false
    return Get-CodexChildTerminalStatusEntry -Child $Child -ReceiptPath $ReceiptPath
}

function Write-CodexChildWaveStatus {
    param([string] $Path, [string] $WaveId, [string] $State, $Statuses, [AllowEmptyString()][string] $Failure = '')
    Write-CodexChildJsonAtomic -Path $Path -Value ([ordered]@{
            schema_version = 2; wave_id = $WaveId; supervisor_pid = $PID
            state = $State; failure = $Failure; launches = $Statuses
            updated_at = [datetimeoffset]::UtcNow.ToString('o')
        })
}

function Invoke-CodexChildWaveSupervisor {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Spec, [Parameter(Mandatory)][System.Collections.IDictionary] $Profiles,
        [Parameter(Mandatory)] $CodexRuntime, [Parameter(Mandatory)][string] $SpecPath,
        [Parameter(Mandatory)][string] $CheckpointPath, [Parameter(Mandatory)][string] $ArtifactRoot,
        [Parameter(Mandatory)][string] $SpecSha256, [Parameter(Mandatory)][string] $CheckpointSha256,
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][int] $MaxParallel
    )
    [System.IO.Directory]::CreateDirectory($ArtifactRoot) | Out-Null
    $statusPath = Join-Path $ArtifactRoot "wave.$($Spec.wave_id).status.json"
    $lockPath = Get-CodexChildSemanticWaveLockPath -Spec $Spec -ArtifactRoot $ArtifactRoot
    [System.IO.Directory]::CreateDirectory((Split-Path $lockPath -Parent)) | Out-Null
    $lock = Enter-CodexChildWaveLock -Path $lockPath
    $statuses = [ordered]@{}; $running = [System.Collections.Generic.List[object]]::new()
    $queue = [System.Collections.Generic.Queue[object]]::new(); $failure = $null
    $currentEntry = $null; $currentHome = ''; $currentReceipt = $null
    $specSeal = $null; $checkpointSeal = $null
    try {
        $specSeal = Get-CodexChildSealedJsonFile -Path $SpecPath -Name 'launch spec' -HoldOpen
        $checkpointSeal = Get-CodexChildSealedJsonFile -Path $CheckpointPath -Name 'epic checkpoint' -HoldOpen
        if ([string]$specSeal.Sha256 -cne $SpecSha256 -or
            [string]$checkpointSeal.Sha256 -cne $CheckpointSha256) {
            throw 'EPIC_CHILD_LAUNCH_BLOCKED: launch spec or checkpoint changed after validation.'
        }
        $Spec = $specSeal.Value; $checkpoint = $checkpointSeal.Value
        $lockedRuntime = Get-CodexChildRuntimeContext -Spec $Spec -RepositoryRoot $RepositoryRoot
        $lockedErrors = Test-CodexChildLaunchSpec -Spec $Spec -Checkpoint $checkpoint -RepositoryRoot $RepositoryRoot `
            -ProfilesByKey $lockedRuntime.Profiles -LiveBranchesByWorktree $lockedRuntime.Branches
        if ($lockedErrors.Count -gt 0) {
            throw "EPIC_CHILD_LAUNCH_BLOCKED: $($lockedErrors -join ' ')"
        }
        $Profiles = $lockedRuntime.Profiles
        foreach ($entry in @($Spec.launches)) {
            $queue.Enqueue($entry); $statuses[[string]$entry.launch_id] = [ordered]@{ state = 'queued' }
        }
        $specHash = [string]$specSeal.Sha256; $checkpointHash = [string]$checkpointSeal.Sha256
        Write-CodexChildWaveStatus -Path $statusPath -WaveId $Spec.wave_id -State running -Statuses $statuses
        while ($queue.Count -gt 0 -or $running.Count -gt 0) {
            while ($queue.Count -gt 0 -and $running.Count -lt $MaxParallel) {
                $entry = $queue.Dequeue(); $currentEntry = $entry
                $receiptPath = Join-Path $ArtifactRoot "$($entry.launch_id).receipt.json"
                $profileKey = Get-CodexChildProfileKey -WorktreePath ([string]$entry.worktree_path) `
                    -AgentName ([string]$entry.deployment_agent) -RepositoryRoot $RepositoryRoot
                $agentProfile = $Profiles[$profileKey]
                $currentHome = New-CodexChildIsolatedHome -RepositoryRoot $RepositoryRoot `
                    -WaveId ([string]$Spec.wave_id) -LaunchId ([string]$entry.launch_id) `
                    -OriginalAuthPath ([string]$CodexRuntime.OriginalAuthPath) -Confirm:$false
                $launchRuntime = [pscustomobject]@{
                    CommandPath      = [string]$CodexRuntime.CommandPath
                    DeniedPaths      = [string[]]@($CodexRuntime.DeniedPaths) + @($currentHome)
                    OriginalAuthPath = [string]$CodexRuntime.OriginalAuthPath
                }
                $probeInfo = Get-CodexChildSandboxProbeStartInfo -WorktreePath ([string]$entry.worktree_path) `
                    -CodexHomePath $currentHome -CommandPath ([string]$launchRuntime.CommandPath) `
                    -DeniedPaths ([string[]]$launchRuntime.DeniedPaths) `
                    -DeniedProbePath ([string]$launchRuntime.OriginalAuthPath)
                Assert-CodexChildSandboxPreflight -StartInfo $probeInfo
                $receipt = Get-CodexChildLaunchReceipt -Spec $Spec -Entry $entry -AgentProfile $agentProfile `
                    -CodexRuntime $launchRuntime -SpecPath $SpecPath -SpecSha256 $specHash `
                    -CheckpointPath $CheckpointPath -CheckpointSha256 $checkpointHash `
                    -ReceiptPath $receiptPath -StatusPath $statusPath -CodexHomePath $currentHome `
                    -WaveLockPath $lockPath
                Write-CodexChildJsonCreateNew -Path $receiptPath -Value $receipt
                $currentReceipt = $receipt
                $child = Start-CodexChildProcess -Entry $entry -AgentProfile $agentProfile -Receipt $receipt -ArtifactRoot $ArtifactRoot
                $running.Add($child)
                $currentEntry = $null; $currentHome = ''; $currentReceipt = $null
                $statuses[[string]$entry.launch_id] = [ordered]@{
                    state = 'running'; pid = $child.Process.Id; codex_session_id = $child.SessionId
                    receipt_path = $receiptPath; started_at = $child.StartedAt
                }
                Write-CodexChildWaveStatus -Path $statusPath -WaveId $Spec.wave_id -State running -Statuses $statuses
            }
            if ($running.Count -eq 0) { continue }
            [System.Threading.Tasks.Task]::WaitAny([System.Threading.Tasks.Task[]]@($running | ForEach-Object { $_.ExitTask })) | Out-Null
            foreach ($child in @($running | Where-Object { $_.ExitTask.IsCompleted })) {
                $receiptPath = Join-Path $ArtifactRoot "$($child.Entry.launch_id).receipt.json"
                $statuses[[string]$child.Entry.launch_id] = Get-CodexChildResumeStatus -Child $child -ReceiptPath $receiptPath
                $running.Remove($child) | Out-Null
                Write-CodexChildWaveStatus -Path $statusPath -WaveId $Spec.wave_id -State running -Statuses $statuses
            }
        }
    } catch {
        $failure = $_
    } finally {
        $cleanupErrors = [System.Collections.Generic.List[string]]::new()
        if ($null -ne $currentEntry) {
            $failedStatus = [ordered]@{ state = 'failed'; failure = [string]$failure }
            if ($null -ne $currentReceipt -and [string]$currentReceipt.state -ceq 'failed') {
                $failedStatus.receipt_path = [string]$currentReceipt.receipt_path
                $failedStatus.codex_session_id = [string]$currentReceipt.codex_session_id
                $failedStatus.exit_code = [int]$currentReceipt.exit_code
                $failedStatus.failed_at = [string]$currentReceipt.failed_at
            }
            $statuses[[string]$currentEntry.launch_id] = $failedStatus
            if (-not [string]::IsNullOrWhiteSpace($currentHome)) {
                try { Remove-CodexChildIsolatedHome -HomePath $currentHome -Confirm:$false } catch {
                    $cleanupErrors.Add("failed to remove unlaunched isolated home: $_")
                }
            }
        }
        foreach ($child in @($running)) {
            $childExitCode = -1
            try {
                if (-not $child.Process.HasExited) { $child.Process.Kill($true); $child.Process.WaitForExit() }
                if ($child.Process.HasExited) { $childExitCode = [int]$child.Process.ExitCode }
            } catch {
                $cleanupErrors.Add("failed to terminate child $($child.Entry.launch_id): $_")
            }
            try {
                Set-CodexChildReceiptState -Receipt $child.Receipt -State failed -ExitCode $childExitCode `
                    -FailureReason 'Wave supervisor aborted the running child.'
            } catch {
                $cleanupErrors.Add("failed to seal child receipt $($child.Entry.launch_id): $_")
            }
            try {
                Remove-CodexChildIsolatedAuth -HomePath ([string]$child.Receipt.codex_home_path) -Confirm:$false
            } catch {
                $cleanupErrors.Add("failed to remove child auth $($child.Entry.launch_id): $_")
            }
            $statuses[[string]$child.Entry.launch_id] = [ordered]@{
                state = 'failed'; outcome = 'aborted'; pid = $child.Process.Id
                exit_code = [int]$child.Receipt.exit_code; failed_at = [string]$child.Receipt.failed_at
                codex_session_id = $child.SessionId
                receipt_path = [string]$child.Receipt.receipt_path; failure = [string]$failure
            }
        }
        while ($queue.Count -gt 0) {
            $entry = $queue.Dequeue(); $statuses[[string]$entry.launch_id] = [ordered]@{ state = 'cancelled' }
        }
        $failed = @($statuses.Values | Where-Object { $_.state -in @('failed', 'cancelled') }).Count -gt 0
        $finalState = if ($null -ne $failure -or $failed -or $cleanupErrors.Count -gt 0) { 'failed' } else { 'completed' }
        $failureParts = [System.Collections.Generic.List[string]]::new()
        if ($null -ne $failure) { $failureParts.Add([string]$failure) }
        foreach ($cleanupError in $cleanupErrors) { $failureParts.Add($cleanupError) }
        try {
            Write-CodexChildWaveStatus -Path $statusPath -WaveId $Spec.wave_id -State $finalState `
                -Statuses $statuses -Failure ($failureParts -join ' | ')
        } catch {
            $cleanupErrors.Add("failed to write final wave status: $_")
        }
        try {
            if ($null -ne $checkpointSeal -and $null -ne $checkpointSeal.Stream) { $checkpointSeal.Stream.Dispose() }
        } catch { $cleanupErrors.Add("failed to release checkpoint seal: $_") }
        try {
            if ($null -ne $specSeal -and $null -ne $specSeal.Stream) { $specSeal.Stream.Dispose() }
        } catch { $cleanupErrors.Add("failed to release spec seal: $_") }
        try { $lock.Dispose() } catch { $cleanupErrors.Add("failed to release semantic wave lock: $_") }
        if ($cleanupErrors.Count -gt 0) {
            $cleanupFailure = [System.Exception]::new("EPIC_CHILD_LAUNCH_BLOCKED: cleanup failures: $($cleanupErrors -join ' | ')")
            if ($null -eq $failure) { $failure = $cleanupFailure } else {
                $failure = [System.AggregateException]::new('Epic-child wave and cleanup failed.',
                    [System.Exception[]]@($failure.Exception, $cleanupFailure))
            }
        }
    }
    if ($null -ne $failure) { throw $failure }
    return $statusPath
}

if ($MyInvocation.InvocationName -eq '.') { return }
if ([string]::IsNullOrWhiteSpace($LaunchSpecPath)) { throw 'EPIC_CHILD_LAUNCH_BLOCKED: LaunchSpecPath is required.' }
$scriptRepositoryRoot = Get-CodexChildCanonicalPath -Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -BasePath $PSScriptRoot
if (-not [string]::IsNullOrWhiteSpace($RepositoryRoot) -and
    (Get-CodexChildCanonicalPath -Path $RepositoryRoot -BasePath (Get-Location).Path) -ne $scriptRepositoryRoot) {
    throw 'EPIC_CHILD_LAUNCH_BLOCKED: RepositoryRoot must equal the repository containing this launcher.'
}
$repositoryRootPath = $scriptRepositoryRoot
$specPath = Get-CodexChildCanonicalPath -Path $LaunchSpecPath -BasePath $repositoryRootPath
$launchBase = Get-CodexChildCanonicalPath -Path 'artifacts/orchestration/epic-child-launches' -BasePath $repositoryRootPath
if (-not $specPath.StartsWith($launchBase + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'EPIC_CHILD_LAUNCH_BLOCKED: launch spec must be under the repository launch-artifact root.'
}
$specSeal = Get-CodexChildSealedJsonFile -Path $specPath -Name 'launch spec'
$spec = $specSeal.Value
$artifactRoot = Get-CodexChildCanonicalPath -Path (Join-Path $launchBase ([string]$spec.wave_id)) -BasePath $repositoryRootPath
if ((Split-Path $specPath -Parent) -ne $artifactRoot) { throw 'EPIC_CHILD_LAUNCH_BLOCKED: launch spec parent must match wave_id.' }
$checkpointPath = Get-CodexChildCanonicalPath -Path ([string]$spec.checkpoint_path) -BasePath $repositoryRootPath
$expectedCheckpoint = Get-CodexChildCanonicalPath -Path "artifacts/orchestration/$($spec.checkpoint_kind)-state.json" -BasePath $repositoryRootPath
if ($checkpointPath -ne $expectedCheckpoint) { throw 'EPIC_CHILD_LAUNCH_BLOCKED: checkpoint_path must be the canonical repository epic checkpoint.' }
$checkpointSeal = Get-CodexChildSealedJsonFile -Path $checkpointPath -Name 'epic checkpoint'
$checkpoint = $checkpointSeal.Value
$runtime = Get-CodexChildRuntimeContext -Spec $spec -RepositoryRoot $repositoryRootPath
$errors = Test-CodexChildLaunchSpec -Spec $spec -Checkpoint $checkpoint -RepositoryRoot $repositoryRootPath `
    -ProfilesByKey $runtime.Profiles -LiveBranchesByWorktree $runtime.Branches
if ($errors.Count -gt 0) { throw "EPIC_CHILD_LAUNCH_BLOCKED: $($errors -join ' ')" }
$effectiveMaximum = Get-CodexChildEffectiveMaximum -RequestedMaximum $MaxParallel -SpecMaximum ([int]$spec.max_parallel_features)
$codexRuntime = Get-CodexChildCommandContext
if ($Supervisor -or $Wait) {
    Invoke-CodexChildWaveSupervisor -Spec $spec -Profiles $runtime.Profiles -CodexRuntime $codexRuntime `
        -SpecPath $specPath -CheckpointPath $checkpointPath -ArtifactRoot $artifactRoot `
        -SpecSha256 ([string]$specSeal.Sha256) -CheckpointSha256 ([string]$checkpointSeal.Sha256) `
        -RepositoryRoot $repositoryRootPath -MaxParallel $effectiveMaximum | Write-Output
    exit 0
}
$supervisorInfo = [System.Diagnostics.ProcessStartInfo]::new('pwsh')
$supervisorInfo.UseShellExecute = $false; $supervisorInfo.CreateNoWindow = $true
$supervisorInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
foreach ($argument in @('-NoProfile', '-File', $PSCommandPath, '-LaunchSpecPath', $specPath,
        '-RepositoryRoot', $repositoryRootPath, '-MaxParallel', [string]$effectiveMaximum, '-Supervisor')) {
    $supervisorInfo.ArgumentList.Add($argument)
}
$supervisorProcess = [System.Diagnostics.Process]::Start($supervisorInfo)
if ($null -eq $supervisorProcess) { throw 'EPIC_CHILD_LAUNCH_BLOCKED: failed to start the background supervisor.' }
[ordered]@{
    wave_id = [string]$spec.wave_id; supervisor_pid = $supervisorProcess.Id
    effective_max_parallel = $effectiveMaximum
    status_path            = Join-Path $artifactRoot "wave.$($spec.wave_id).status.json"
} | ConvertTo-Json -Compress | Write-Output
