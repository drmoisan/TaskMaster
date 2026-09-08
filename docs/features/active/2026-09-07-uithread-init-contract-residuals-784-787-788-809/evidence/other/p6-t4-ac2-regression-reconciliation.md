# [P6-T4] Reconciliation of the AC2 regression clause against the decision-D5 measurement

Timestamp: 2026-09-08T03-08

Source read: `MTA_INITIALIZE_OUTCOME:` in `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/p0-t15-mta-synccontextform-measurement.md`.

Measured value: **`COMPLETED`**.

> **CORRECTION (2026-09-08, added by the orchestrator after feature review).** The refutation asserted
> in Disposition A below is **withdrawn**. The `[P0-T15]` run never read the executing thread's
> apartment state, and this same delivery falsified the research premise the `MTA` label rested on, so
> that run most likely executed STA and no MTA measurement was taken. The status of the #782 mechanism
> narrative is **UNKNOWN**, not refuted. The two tree-verified facts behind this are recorded in the
> correction section of `p0-t15-mta-synccontextform-measurement.md`.
>
> The section "Why the AC2 design is safe whichever value was measured" below is **unaffected**. The
> feature review verified it structurally against the head tree, and it was written from the outset to
> stand independently of the measured value, which is what decision D5 required. The delivered code and
> tests need no change. `spec.md` AC5 has been unchecked because its measurement clause is not
> established.

## Disposition A

The measured value is `COMPLETED`, so Disposition A applies and Disposition B is not recorded.

**The #782 mechanism narrative is refuted on this host.** That narrative requires `new SyncContextForm(); Show();` to throw when executed on an MTA thread; the measurement shows that it does not. [P0-T15] established this by running `QuickFiler.Controllers.Tests.QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` as a single test against the unmodified tree, where no earlier test can have consumed the latch at `UtilitiesCS/Threading/UiThread.cs:36`, so the two assertions at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:356-357` could hold only if `Initialize()` ran to completion on the MTA MSTest worker. The test passed.

This is a measurement of one host at one point in time. It refutes the narrative's necessary precondition here; it does not establish what was observed on the host where #782 was recorded.

**The AC2 clause "the #782 regression scenario is reproduced as a test" is discharged by the forced-throw scenario driven through the factory seam.** Because the recorded mechanism is not reproducible on this host, a test that claimed to reproduce it literally would assert nothing about a real failure mode and would be vacuous. The clause is instead satisfied by making `Initialize()` fail deterministically through `UiThread.SyncContextFormFactory`, which is the seam [P1-T4] introduced for exactly this purpose.

The anti-retry-storm test is:

`UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests.AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory`

Its mechanism is an **invocation count**. It fails the first `Init()` through a throwing fake, records `FakeUiCaptureSource.ConstructionCount`, then reads `UiThread.AutoScaleFactor` from a dedicated MTA thread and asserts two things: that the read threw an `InvalidOperationException` whose message starts with `UiThread.NonStaInitMessagePrefix`, and that the construction count did not increase. The second assertion is the direct statement of "no retry storm": the storm the #782 record describes is repeated construction of the capture object, and the count is the number of times that construction happened.

**The assertion is an invocation count and never a wall-clock duration.** A duration assertion would be a timing hack, is prohibited by `.claude/rules/csharp.md` under Prohibited Behaviors and by the determinism requirements of `.claude/rules/general-unit-test.md`, and would be sensitive to host speed. The invocation count is deterministic on any host.

Fail-before and pass-after evidence for that test is recorded in `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/regression-testing/p2-t10-fail-before.md` and `.../p3-t6-pass-after.md`. It failed against the pre-fix tree with `Expected InvalidOperationException.Message to be System.InvalidOperationException, but found <null>.` and passes after the fix.

## Why the AC2 design is safe whichever value was measured

The AC1 precondition makes `new SyncContextForm()` at `UtilitiesCS/Threading/UiThread.cs:51`, now line 72 after this delivery's edits, **unreachable from any non-STA caller**. `Initialize()` is reachable from exactly four places: the two direct `Init()` calls and the two lazy getters. With the apartment check as the first statement of `Init()`, a non-STA reader of either lazy getter fails at one `GetApartmentState()` read and a `throw`, and never reaches the capture-object construction or `Show()`. The expensive, potentially-throwing body is therefore unreachable from any thread-pool thread, which is where thread-pool starvation would have to originate. On the STA thread itself `Initialize()` succeeds, verified by the two `[STATestClass]` viewer tests recorded in `.../evidence/qa-gates/p4-t4-utilitiescs-tests.md`, so no retry loop engages there either. The `lock (InitLock)` additionally serializes concurrent first attempts, which the previous `Interlocked.Exchange` latch never did, closing the pre-existing #782 finding C04 race.

That argument stands independently of whether the recorded #782 mechanism was ever real: it removes the mechanism if the mechanism exists, and costs nothing if it does not. This is why decision D5 required the narrative to be measured rather than assumed, and why the delivery does not depend on the outcome of that measurement.
