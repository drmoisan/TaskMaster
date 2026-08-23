# Full coverage attempt timeout

Timestamp: 2026-08-04T20:14:00-04:00

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/remediation-coverage-final.cobertura.xml`

EXIT_CODE: 124

Output Summary: The repository-wide VSTest coverage invocation exceeded its 180-second command limit. `vstest.console.exe` PID 90152 and its child `testhost.exe` PID 69268 started at 20:11:56/20:11:57 and remained active with near-zero CPU. Both processes were verified by exact PID and terminated after the timeout so they could not block subsequent work. No final coverage result is claimed from this attempt. Focused regression runs completed before this attempt: UtilitiesCS.Test 25/25 and TaskMaster.Test 6/6.
