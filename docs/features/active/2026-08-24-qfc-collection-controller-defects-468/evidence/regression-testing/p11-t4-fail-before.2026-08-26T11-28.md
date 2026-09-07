# [P11-T4] [expect-fail] Issue #473 defect 1 drain-window test, red before the fix

Timestamp: 2026-08-26T11-28

Command:

```
# Precondition
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build      # EXIT_CODE 0, 0 errors

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~DrainBackgroundLoadingTasksAsync_AwaitsATaskAddedDuringTheDrainWindow" `
    /Logger:"trx;LogFileName=p11-t4.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p11-t4
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

`Test Run Failed. Total tests: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p11-t4/p11-t4.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

The failed count is exactly `1`, as the task's acceptance requires.

Verbatim `<Message>` from the TRX:

```
Expected drain.IsCompleted to be False because a task added to BackgroundLoadingTasks during the
drain window is still outstanding work, so the drain must not report completion until that task has
also finished, but found True.
```

## What the red state proves

At the tree state of commit `97604063` the drain body is still the two statements extracted by the
seam:

```
await Task.WhenAll(BackgroundLoadingTasks);
BackgroundLoadingTasks = [];
```

`Task.WhenAll` enumerates the bag once, at the moment it is called. The late arrival is added to the
bag after that enumeration, so it is not part of the awaited set; the subsequent field reassignment
then drops the bag that holds it. The drain therefore reports completion — `IsCompleted == True` —
while a background task is still outstanding. That is issue #473 defect 1 exactly.

## Determinism of the test (no wall-clock wait anywhere)

| Requirement | How it is met |
|---|---|
| No `Thread.Sleep` | none in the test |
| No `Task.Delay` | none in the test |
| No real wall-clock wait | none; the only synchronisation points are two `TaskCompletionSource` completions |
| Late add lands inside the drain window | the continuation is registered on the gate **before** the drain starts, so it runs ahead of the drain's own continuation |
| The observation is settled, not raced | the continuation carries `TaskContinuationOptions.ExecuteSynchronously`, and an MTA MSTest method installs no `SynchronizationContext`, so every await in the chain resumes synchronously on the thread that calls `gate.SetResult`. By the time `SetResult` returns, the drain has either completed or committed to waiting. |
| No pending work left behind | the test completes the late arrival and awaits the drain before returning |
| No temporary file, no external dependency | none |

## Genuine fail-before

This run was executed against the real tree before any fix was written, and this artifact records
the observed non-zero exit code. No red state was back-filled after the fact.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit. Any `Deploy_*` scaffolding
directory vstest created was removed. A post-sanitisation sweep returns zero hits for every token
class recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
