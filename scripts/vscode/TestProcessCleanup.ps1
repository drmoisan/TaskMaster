function Stop-RepoOwnedVSTestProcess {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepoRoot
    )

    $processRecords = @(
        Get-CimInstance Win32_Process |
            Where-Object { $_.Name -in @('vstest.console.exe', 'testhost.exe') }
    )

    if ($processRecords.Count -eq 0) {
        return
    }

    $escapedRepoRoot = [Regex]::Escape($RepoRoot)
    $processIdsToStop = [System.Collections.Generic.HashSet[int]]::new()
    $pendingParentIds = [System.Collections.Generic.Queue[int]]::new()

    foreach ($processRecord in $processRecords) {
        if ($processRecord.Name -ne 'vstest.console.exe') {
            continue
        }

        if ($processRecord.CommandLine -notmatch $escapedRepoRoot) {
            continue
        }

        $processId = [int]$processRecord.ProcessId
        if ($processIdsToStop.Add($processId)) {
            $pendingParentIds.Enqueue($processId)
        }
    }

    while ($pendingParentIds.Count -gt 0) {
        $parentProcessId = $pendingParentIds.Dequeue()

        foreach ($processRecord in $processRecords) {
            $childProcessId = [int]$processRecord.ProcessId
            $childParentProcessId = [int]$processRecord.ParentProcessId

            if ($childParentProcessId -ne $parentProcessId) {
                continue
            }

            if ($processIdsToStop.Add($childProcessId)) {
                $pendingParentIds.Enqueue($childProcessId)
            }
        }
    }

    if ($processIdsToStop.Count -eq 0) {
        return
    }

    $sortedProcessIds = $processIdsToStop | Sort-Object -Descending
    Write-Verbose "Stopping stale repo-owned test processes: $($sortedProcessIds -join ', ')"

    $runningProcesses = Get-Process -Id $sortedProcessIds -ErrorAction SilentlyContinue
    if ($null -eq $runningProcesses) {
        return
    }

    if ($PSCmdlet.ShouldProcess(($sortedProcessIds -join ', '), 'Stop repo-owned test processes')) {
        $runningProcesses | Stop-Process -Force
    }
}

Set-Alias -Name Stop-RepoOwnedVSTestProcesses -Value Stop-RepoOwnedVSTestProcess -Scope Script
