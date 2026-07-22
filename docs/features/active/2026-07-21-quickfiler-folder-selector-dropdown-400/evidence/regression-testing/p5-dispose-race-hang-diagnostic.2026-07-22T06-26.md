# P5 dispose-race broader-harness diagnostic

Timestamp: 2026-07-22T06:26:05.9299648Z

Command: `& { $workspace='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25'; $owned=@(Get-CimInstance Win32_Process | Where-Object { ($_.Name -in @('vstest.console.exe','testhost.exe','testhost.net481.x86.exe')) -and $_.CommandLine -like "*$workspace*" }); Write-Output "OWNED_TEST_PROCESSES=$($owned.Count)"; $owned | Select-Object ProcessId,ParentProcessId,Name,CommandLine | Format-List }`

EXIT_CODE: 0

Output Summary: The final bounded process audit returned `OWNED_TEST_PROCESSES=0`. The required rebuilt P5-T41 filter completed normally with all 12 tests passing, 0 failures/skips, and a 2.3171-second total. A separate diagnostic filter for `BreadcrumbDropDownLifecycleConcurrencyTests|BreadcrumbPendingOpenCloseTests` exceeded its explicit 15-second deadline and was terminated after 19.035 seconds. Individual isolation identified `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup` as the hanging legacy test. The timeout left workspace-owned VSTest PID 76276 and testhost PID 72008; both exact PIDs were inspected, stopped, and verified absent before this audit. A related isolated legacy case, `Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation`, passed, while `Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation` failed at line 117 because `Surface.DisposeCount` was 0 when immediate invalidation had already settled the shared open task false. These broader tests use an unpumped host-neutral async context and are outside the four-file P5-T36 batch; this artifact records the diagnostic result without treating it as P5-T41 acceptance evidence.
