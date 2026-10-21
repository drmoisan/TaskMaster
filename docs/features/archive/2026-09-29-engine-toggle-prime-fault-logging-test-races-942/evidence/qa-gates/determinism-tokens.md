# Determinism-token constraints (issue 942)

Timestamp: 2026-09-30T07-52
Task: P3-T11
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $added = @(git diff -U0 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- TaskMaster.Test/Ribbon | Where-Object { $_.StartsWith("+") -and -not $_.StartsWith("+++") }); ... ADDED-TOKEN counts ...; git diff --exit-code 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings; git diff --exit-code 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs TaskMaster/Ribbon/RibbonController.EngineCommands.cs' (the P3-T11 payload; the worktree path was composed inside the payload by concatenation, which does not change any computed value)
EXIT_CODE: 0

Output Summary:
- ADDED-LINE-COUNT: 89 (the 77-line new partial plus the 12 added Harness lines; at least 60)
- ADDED-TOKEN [Thread.Sleep] = 0
- ADDED-TOKEN [Task.Delay] = 0
- ADDED-TOKEN [DoNotParallelize] = 0
- ADDED-TOKEN [[Timeout] = 0
- ADDED-TOKEN [Timeout=] = 0
- ADDED-TOKEN [DateTime.Now] = 0
- ADDED-TOKEN [DateTime.UtcNow] = 0
- ADDED-TOKEN [Stopwatch] = 0
- ADDED-TOKEN [.Wait(] = 0
- ADDED-TOKEN [.Result] = 0
- ADDED-TOKEN [ManualResetEvent] = 0
- ADDED-TOKEN [SemaphoreSlim] = 0
- ADDED-TOKEN [GetTempFileName] = 0
- ADDED-TOKEN [GetTempPath] = 0
- ADDED-TOKEN [TaskScheduler] = 0
- ADDED-TOKEN [while (] = 0
- ADDED-TOKEN [for (] = 0
- ADDED-TOKEN [Retry] = 0
- ADDED-TOKEN [GetResult(] = 0
- ADDED-TOKEN [DateTimeOffset] = 0
- RUNSETTINGS_DIFF_EXIT=0
- NONGOAL_FILES_DIFF_EXIT=0
- Every ADDED-TOKEN count is 0; the run-settings files and the two non-goal files are unchanged from the merge base.
