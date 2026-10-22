# [P11-T6] Issue #473 defect 1 drain-window test, green after the atomic bag swap

Timestamp: 2026-08-26T11-30

Command:

```
# Precondition
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build      # EXIT_CODE 0, 0 errors

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~DrainBackgroundLoadingTasksAsync_AwaitsATaskAddedDuringTheDrainWindow" `
    /Logger:"trx;LogFileName=p11-t6.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p11-t6
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 1  Passed: 1`. Duration 92 ms.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p11-t6/p11-t6.trx`:

```
total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `1`, as the task's acceptance requires.

## Red-to-green transition

| Run | Tree state | Result |
|---|---|---|
| P11-T4 | `DrainBackgroundLoadingTasksAsync` seam, original snapshot-then-clear body | failed 1 — `drain.IsCompleted` was `True` |
| P11-T6 (this run) | atomic-swap drain loop | passed 1 — `drain.IsCompleted` is `False` |

The test source is byte-identical between the two runs. Only the drain body changed.

## The fixed drain body

```
internal async Task DrainBackgroundLoadingTasksAsync()
{
    ConcurrentBag<Task> drained;
    do
    {
        drained = Interlocked.Exchange(
            ref BackgroundLoadingTasks,
            new ConcurrentBag<Task>()
        );
        await Task.WhenAll(drained);
    } while (!drained.IsEmpty);
}
```

Properties the task's acceptance names:

| Requirement | State |
|---|---|
| Exactly one `Interlocked.Exchange` call in the member | yes — one call, at the single swap point. The identifier appears twice more in the member's XML documentation and nowhere else in the file. |
| No direct field reassignment outside the exchange | yes — the only other occurrence of `BackgroundLoadingTasks =` in `<CTRL>` is the field's own declaration initializer at `:85`. Both former drain sites now call the member. |
| Explicitly constructed replacement bag | yes — `new ConcurrentBag<Task>()`, not a target-typed collection expression. The generic `Interlocked.Exchange<T>` overload infers `T` from its arguments and cannot bind an expression that has no type of its own. |
| Loop repeats while the swapped-out bag was non-empty | yes — `do { ... } while (!drained.IsEmpty)` |

## Why the swap closes the window

The defect was a read-then-replace pair with a gap between them: `Task.WhenAll` enumerated the bag,
and only later did the field assignment discard it. A producer adding in that gap wrote into a bag
that had already been snapshotted and was about to be thrown away. The exchange removes the gap —
the fresh bag is installed in the same instant the old one is handed back, so every producer either
writes into a bag that will be awaited on this iteration or into the replacement, which the next
iteration picks up. The loop terminates as soon as a swap yields an empty bag.

## Field visibility: the conditional narrowing does not apply

P11-T5 narrows `BackgroundLoadingTasks` from `internal` to `private` **if and only if** the test
drives the drain member rather than the field. The condition is not met and the field stays
`internal`: the test must seed the bag and must register a producer against the live field to
create a late arrival at all, so it references `controller.BackgroundLoadingTasks` directly in its
Arrange section. Narrowing the field would break compilation of the only test that proves the fix.
The field has no reference anywhere outside `<CTRL>` and this test file, so the visibility carries
no wider cost; a follow-up could re-narrow it if the arrangement were rebuilt on the reflection
helpers in `QfcCollectionController.TestSupport.cs`.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit: 10 substitutions. No
`Deploy_*` scaffolding directory was left behind. A post-sanitisation sweep returns zero hits for
every token class recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
