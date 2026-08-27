# [P6-T6] Full `QuickFiler.Test` suite after the issue #469 defect 1 and 2 fix

Timestamp: 2026-08-26T10-26

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p6-t6.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p6-t6
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 949  Passed: 949`.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p6-t6/p6-t6.trx`:

```
total="949" executed="949" passed="949" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires.

## Suite-size accounting

| Run | Total | Passed | Failed |
|---|---|---|---|
| P5-T6 (end of Phase 5) | 946 | 946 | 0 |
| P6-T6 (this run) | 949 | 949 | 0 |

The delta of `+3` is exactly the three tests added by P6-T1, P6-T2 and P6-T3. No test was removed
and no previously passing test regressed.

## Flaky first attempt, retained and analysed

The first attempt at this run failed with `Total tests: 949  Passed: 948  Failed: 1`. The failing
test was `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` in
`QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, with:

```
Expected loaderInvokedTcs.Task.Wait(TimeSpan.FromSeconds(5)) to be True because the injected
RemainingEmailLoader must be invoked by the started worker, but found False.
```

That first TRX is retained at `evidence/qa-gates/p6-t6/p6-t6-attempt1-flaky.trx`. The run was
repeated once with the working tree unchanged and the same test passed, giving the 949/949 result
recorded above.

This is classified as load-induced flakiness, not a regression from this change, on four grounds:

1. **The assertion is a wall-clock wait.** It blocks for at most five seconds on a
   `TaskCompletionSource` signalled from a `BackgroundWorker` thread. Under CPU contention the
   worker does not reach the injected loader inside the budget, and the wait returns `False`
   without any behavioural change in the code under test. This machine concurrently hosts unrelated
   build and test work.
2. **No call path connects it to this change.** P6-T4 edits `GetMoveDiagnostics` in
   `QfcCollectionController` only. `InitEmailQueue` lives in the `QfcDatamodel` email-queue path
   and neither calls `GetMoveDiagnostics` nor reads `_itemGroupsToMove`.
3. **It reproduces the documented `QfcDatamodel` `BackgroundWorker` timing pattern**, in which
   worker start-up is observed through a bounded wait rather than a deterministic handshake.
4. **It passed on re-run with a byte-identical tree.** No source file, no build output, and no test
   file changed between the two attempts.

Per the plan's Conventions this is recorded rather than chased: the flake is in a different type,
in a different feature area, and this feature touches neither.

## Final-tree re-run

`p6-t6.trx` in this directory is the run taken after the AC-4 assertion literal in
`GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` was widened (see
`evidence/regression-testing/p6-t3-fail-before.2026-08-26T10-17.md`), so the suite result recorded
above is measured against exactly the tree P6-T7 commits, not against an intermediate one. That run
also reported 949/949 with `EXIT_CODE 0`, identical to the pre-widening run, which is expected
because the widening strengthens one assertion in a test that passes either way.

The toolchain state at the time of that run: `dotnet tool run csharpier check .` `EXIT_CODE 0` over
1,523 files, and `Invoke-VSBuild.ps1 -Target Build` `EXIT_CODE 0` with 0 errors.

## Host-identifier sanitisation

Both TRX files were sanitised case-insensitively before commit: 2,854 substitutions in
`p6-t6.trx` and 2,855 in `p6-t6-attempt1-flaky.trx` (the flaky run carries one additional
occurrence in the recorded failure's stack trace). Post-sanitisation both files contain zero
occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.

vstest also creates an empty `Deploy_<user> <timestamp>_<pid>` scaffolding directory inside every
results directory, whose name embeds the account name and whose `In` subdirectory is named for the
machine. These directories contain no files, so git does not track them, but they were removed
explicitly rather than relied on being untracked.
