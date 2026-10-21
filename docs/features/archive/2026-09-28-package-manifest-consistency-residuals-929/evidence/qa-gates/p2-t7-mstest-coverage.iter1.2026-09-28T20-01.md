# P2-T7 — C# QC step 4, MSTest with coverage (iteration 1) — FAILED, loop restarts as iter2

Timestamp: 2026-09-30T10-53
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot .' (run detached with its combined output redirected to a session scratch log outside the repository; the script and its arguments are unchanged)
EXIT_CODE: 1
Output Summary:
- "Total tests: 7346" / "Passed: 7345" / "Failed: 1" / "Test Run Failed."
- The runner then threw at Invoke-MSTestWithCoverage.ps1 line 262: "MSTest with coverage failed with exit code 1"; no "First-party coverage:" line, no "Coverage projection: " line and no "Test-result summary: " line were printed, so no projection or summary was copied for this iteration.
- Failed test: RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces (QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs, line 192 via StartHeldOpenLoader line 172)
- Error message: "Expected entered.Task.Wait(TimeSpan.FromSeconds(5)) to be True because the started worker must reach the injected loader, but found False."
- "Coverage output" and "Results File" lines printed absolute paths under <execution-worktree-root>\coverage\ (placeholders applied, convention 4).

Attribution: this change edits no C# source file. The only QuickFiler.Test change removes two Exists()-guarded Import elements whose package folder was never restored (P0-T5 ALTCOVER-RESTORED False), so the compiled test assembly is unaffected. The same test passed in the P0-T12 baseline (7346 of 7346). The failure is a five-second wall-clock wait inside the test that did not complete under a full parallel run, which is a pre-existing timing dependence outside the Write Set; it is reported to the caller and is not repaired on this branch.

Loop action (convention 8): P2-T7 failed, so the loop restarts at P2-T1 with the artifact suffix iter2. No Write Set file is changed by the restart.
