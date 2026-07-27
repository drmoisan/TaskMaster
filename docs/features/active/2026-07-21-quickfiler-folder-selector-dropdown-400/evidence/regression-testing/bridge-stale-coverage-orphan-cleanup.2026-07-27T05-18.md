# P8-T73 orphaned VSTest process cleanup

- Timestamp: 2026-07-27T05:18:57Z
- Scope: Operational cleanup after the failed P8-T73 aggregate run. No source, test, project, coverage, settings, filter, exclusion, threshold, or postprocessor file was changed.

## Read-only target verification

Command:

```powershell
Get-CimInstance Win32_Process -Filter "ProcessId=254944 OR ProcessId=259952" |
    Select-Object ProcessId, ParentProcessId, Name, CreationDate, CommandLine
Get-CimInstance Win32_Process |
    Where-Object { $_.ParentProcessId -in @(254944, 259952) } |
    Select-Object ProcessId, ParentProcessId, Name, CreationDate, CommandLine
```

EXIT_CODE: 0

Output Summary:

- PID `254944` was the orphaned `vstest.console.exe` process started at `2026-07-27T01:12:46-04:00`.
- Its command line was the exact P8-T73 eight-assembly command with the canonical runsettings, `/InIsolation`, `TestCategory!=LiveOutlook`, detailed console logger, canonical results directory, and requested run-1 TRX filename.
- PID `259952` was its `testhost.exe` child started at `2026-07-27T01:12:47-04:00`.
- The recorded parent PowerShell PID `255660` no longer existed. The timed-out shell command had exited without terminating these descendant processes.

## Exact process-tree cleanup

Command:

```powershell
$targetPids = @(259952, 254944)
$targets = Get-CimInstance Win32_Process |
    Where-Object { $_.ProcessId -in $targetPids }
$unexpected = $targets |
    Where-Object {
        $_.CreationDate -lt [datetime]'2026-07-27T01:12:40-04:00' -or
        $_.CreationDate -gt [datetime]'2026-07-27T01:13:00-04:00' -or
        ($_.Name -notin @('testhost.exe', 'vstest.console.exe'))
    }
if ($unexpected) {
    throw 'Resolved process targets no longer match the verified orphaned VSTest tree.'
}
Stop-Process -Id 259952 -Force -ErrorAction Stop
Stop-Process -Id 254944 -Force -ErrorAction SilentlyContinue
```

EXIT_CODE: 1

Output Summary:

The exact verified testhost/VSTest tree exited during cleanup. The nonzero shell result is retained and is not treated as a passing gate.

## Post-cleanup verification

Command:

```powershell
Get-Process -Id 259952, 254944 -ErrorAction SilentlyContinue
Get-CimInstance Win32_Process |
    Where-Object {
        $_.ParentProcessId -in @(259952, 254944) -or
        $_.ProcessId -in @(259952, 254944)
    }
```

EXIT_CODE: 0

Output Summary:

No process or descendant remained for either verified P8-T73 PID. The cleanup removed only the orphaned issue-#400 VSTest process tree.
