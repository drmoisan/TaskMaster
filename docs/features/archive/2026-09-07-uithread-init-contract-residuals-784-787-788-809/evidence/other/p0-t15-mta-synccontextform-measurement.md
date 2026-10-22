# [P0-T15] Decision-D5 measurement — does `new SyncContextForm(); Show();` throw on an MTA thread on this host?

Timestamp: 2026-09-08T00-47

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Tests:Worker_RunWorkerCompleted_HandlesCompletionCorrectly' '/InIsolation' '/Logger:trx;LogFileName=p0t15.trx' '/ResultsDirectory:TestResults\809-p0t15'
```

`/Tests:` and `/TestCaseFilter:` are mutually exclusive in vstest and the four stalling shell-icon classes live in `UtilitiesCS.Test`, so no filter was passed here.

EXIT_CODE: 0
ExpectedExitCode: 0

`ExpectedExitCode:` is set to the observed value because this task **measures** rather than gates. Either observed outcome is a valid result of the measurement, so declaring an expectation other than the observed one would convert a measurement into a gate that decision D5 does not authorise.

MTA_INITIALIZE_OUTCOME: COMPLETED

> **CORRECTION (2026-09-08, added by the orchestrator after feature review).** The `MTA_` prefix on the
> token above is **not established and is most likely wrong**. Read the correction section at the end
> of this file before relying on any claim made here. The token is left in place rather than rewritten
> because the approved plan requires exactly one `MTA_INITIALIZE_OUTCOME:` line valued `COMPLETED` or
> `THREW`, and neither value can express "apartment not established". What this run measured is that
> `new SyncContextForm(); Show();` completed on the vstest main execution thread, whose apartment was
> never read.

## Output Summary

```
Test Run Successful.
Total tests: 1
     Passed: 1
```

TRX selected: `p0t15.trx`, `LastWriteTimeUtc` `2026-09-08T04:23:02.2600330Z`, selected as the most recently modified `.trx` under `TestResults\809-p0t15`.

TRX `ResultSummary/Counters`: `total` 1, `executed` 1, `passed` 1, `failed` 0.

| Fully-qualified test | Outcome | Duration |
|---|---|---|
| `QuickFiler.Controllers.Tests.QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` | Passed | 00:00:00.3959381 |

The TRX carries no `Message` and no `StackTrace` element for that result, because the result is `Passed`. The value of `MTA_INITIALIZE_OUTCOME` is `COMPLETED`, so no exception type or message is recorded; that record is required only on the `THREW` branch.

## The inference

The inference is exact. In a single-test run no earlier test can have consumed the latch at `UtilitiesCS/Threading/UiThread.cs:36`, so `UiThread.Init(false)` at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` necessarily entered `Initialize()`. The Act at `:346-353` invokes `QfcHomeController.Worker_RunWorkerCompleted`, whose body reaches `UiThread.Dispatcher.Invoke(...)`; `UiThread.Dispatcher` throws when `_dispatcher` is null. The two assertions at `:356-357` therefore hold only if `Initialize()` ran to completion on the MTA MSTest worker, which requires `new SyncContextForm()` at `UiThread.cs:51` and `Show()` at `:54` not to have thrown.

The test is declared with a plain `[TestMethod]` at `:325` on a class carrying no `[STATestClass]`, so the executing apartment is the MSTest default, which research R4 established as MTA from three independent in-tree sources.

Both assertions passed, so `new SyncContextForm(); Show();` **completed without throwing on an MTA thread on this execution host**.

## Consequence for the #782 narrative

The recorded #782 mechanism requires `new SyncContextForm(); Show();` to throw on a non-STA thread. This measurement shows that it does not throw on this host, so **that mechanism narrative is not reproducible as stated on this execution host**. [P6-T4] carries the reconciliation and records the disposition; the AC2 "reproduce the #782 regression scenario as a test" clause is discharged there by the forced-throw scenario driven through the factory seam rather than by the narrative.

This is a measurement of one host at one point in time. It refutes the narrative's necessary precondition on this host; it does not establish what was observed on the host where #782 was recorded.

## Correction: the apartment of this run was never established

Timestamp: 2026-09-08T04-10. Added by the orchestrator after the feature review of this delivery, and
verified independently against the tree before being written here.

**The inference recorded above is unsound, and the conclusion drawn from it is withdrawn.** Nothing in
this run read `Thread.CurrentThread.GetApartmentState()`. The apartment was inferred from research R4,
and *this same delivery falsified R4 by direct measurement*:
`../regression-testing/p2-t10-fail-before.md` quotes the verbatim TRX message
`Expected Thread.CurrentThread.GetApartmentState() to be ApartmentState.MTA {value: 1}, but found ApartmentState.STA {value: 0}.`
observed from a plain `[TestMethod]` on a plain `[TestClass]`.

Two facts settle why the `/Tests:` single-test selection does not rescue the inference. Both were
verified directly against the tree by the orchestrator rather than accepted from the review:

1. `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18` carries the only assembly-level
   `[assembly: Parallelize(...)]` in this repository. `QuickFiler.Test` carries none, so with no
   `/Settings:` passed — and this run passed none — that assembly does not parallelize at all.
2. No `.runsettings` anywhere in the repository sets `ExecutionThreadApartmentState`.

The consequence is that a test in a non-parallelizing assembly runs on the vstest main execution
thread, which on .NET Framework is STA unless overridden. Under that explanation **this run executed
STA**, no MTA measurement was taken, and the refutation of the #782 mechanism narrative does not
follow: a successful run on an STA thread says nothing about whether the construction throws on an MTA
thread.

**The status of the #782 narrative therefore reverts to UNKNOWN**, which is where decision D5 found it.
Acceptance criterion AC5 in `spec.md` has been unchecked accordingly.

This correction does not affect the delivered code. The AC2 design argument recorded in
`p6-t4-ac2-regression-reconciliation.md` was verified structurally by the review and holds whichever
value a real measurement would produce, because the AC1 precondition makes the potentially-throwing
body of `Initialize()` unreachable from any non-STA caller. Settling the measurement requires reverting
`UtilitiesCS/Threading/UiThread.cs` to its pre-fix state and re-running the probe on a thread whose
apartment is explicitly set, which is follow-up work rather than a defect in this delivery.
