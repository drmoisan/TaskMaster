# quickfiler-transactiongate-permit-leak-unexcluded (Issue #882)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-transactiongate-permit-leak-unexcluded/ (Issue #882)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #882
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/882
- Last Updated: 2026-09-13
## Summary

QuickFiler's one-permit `TransactionGate` may be able to leak or late-release a permit, and nothing in the repository currently excludes that possibility. Issue 743 set out to discriminate this hypothesis (H-LEAK) from elapsed fixture cost (H-COST) by instrumented measurement, and the measurement did not discriminate them: no expiry occurred in either instrumented run, so the discriminating experiment never took place. H-LEAK was never excluded, only never observed. Issue 743's fix routes the affected tests around the gate via a UI-marshalling seam rather than answering the question, so if H-LEAK is the real mechanism the defect still exists behind the seam. This issue carries that open question forward so it is not retired along with 743's symptom.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 (the machine on which issue 743's instrumented runs were taken)
- Python version: not applicable; this is a C# / .NET Framework 4.8 defect
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook"` for the SERIAL regime, and the same command with `/Settings:TaskMaster.runsettings` (Workers 0, Scope ClassLevel) for the PARALLEL regime
- Data source or fixture: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` — the `UiThreadDispatcherFixture` / `UiThreadDispatcherTransaction` pair, whose `TransactionGate` is the subject

## Steps to Reproduce

1. Read `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` and confirm the current shape of `TransactionGate`: a `SemaphoreSlim(1, 1)` acquired by `BeginTransactionAsync` and released by `ReleaseTransactionGate` from the transaction's `Dispose`.
2. Observe that the acquisition is awaited with no timeout argument and no `CancellationToken` overload, so a caller that finds the permit held waits without bound.
3. Construct the state the hypothesis requires: an `async` test that acquires a transaction and then exceeds its MSTest `[Timeout(...)]` bound before reaching the `Dispose` that releases the permit. MSTest abandons a timed-out `async` test rather than unwinding it, so the `finally` that would release the permit is no longer observed.
4. Run any later test in the same assembly that acquires the gate.
5. The open question is whether step 4 then blocks without bound on a permit no live holder owns. This has NOT been demonstrated, and demonstrating or refuting it is the work of this issue.

## Expected Behavior

A one-permit gate either releases its permit on every path out of a transaction, including abandonment of a timed-out `async` test, or it acquires with a bounded timeout so that a lost permit surfaces as a prompt, diagnosable failure rather than an unbounded wait. A later test must not be able to block indefinitely because an earlier test was abandoned.

## Actual Behavior

Unknown, and that is the defect being filed. Issue 743's instrumented measurement was designed to settle it and did not. Both instrumented runs recorded `timeout=0` — no test was abandoned in either run. Because H-LEAK is by definition a cascade conditional on a prior expiry, the absence of any expiry meant no leak could have occurred under either hypothesis, so the pre-declared observable (`contended`, the count of acquisitions that find the permit held) read zero for a reason entirely independent of whether H-LEAK is true of this codebase. The serial reading `acquisitions=11 releases=10 contended=0` was predetermined by the absence of expiry and carries no information about the hypothesis.

**A hypothesis cannot be rejected by the absence of observations.** Issue 743's verdict artifact originally claimed H-LEAK was "REJECTED by direct observation", and a first amendment then claimed the rejection "rests entirely on the counter observable, which is a legitimate basis". Both claims were wrong and both have been withdrawn on the 743 branch; the artifact now records that neither hypothesis was discriminated. See `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md`, Correction 2.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: instrumented `GATECOUNTERS` triples from issue 743, both taken on 2026-09-13 from the same instrumented assembly on an otherwise-idle machine with Outlook closed:
  - SERIAL regime, 1394 tests: `acquisitions=11 releases=10 contended=0`, balance test passed with difference exactly 1, `timeout=0`, `failed=0`.
  - PARALLEL regime (Workers 0, Scope ClassLevel), 1395 tests: `acquisitions=19 releases=18 contended=14`, balance test passed with difference exactly 1, `timeout=0`, `failed=0`.
  - The 14 contended acquisitions in the parallel regime are live-holder queueing by distinct test classes, not leaks; the balance test confirms it. Neither regime produced the expiry that the discriminating observation requires.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium rather than High because the failure mode is confined to the test assembly and has not been shown to affect the shipped add-in, and because issue 743's seam removes the affected tests from the gate's path. Medium rather than Low because an unbounded wait on a lost permit presents as a hung or timed-out test run whose cause is not local to the failing test, which is expensive to diagnose and is a plausible contributor to the historical flake rate recorded in issues 592, 511 and 571 that 743 was opened to address.

## Suspected Cause / Notes

The load-bearing evidence is correction C2 of issue 743's spec, verified against the current tree before this issue was filed. C2 states:

> **C2 — #493 changed the owner of the serialization, not its shape.** `TransactionGate` is still a one-permit `SemaphoreSlim(1,1)`, still awaited without timeout or cancellation token, and still held from acquisition to disposal. The lead must therefore be re-tested against the current gate, not assumed closed and not assumed open.

The significance is that issue 493 is frequently cited as having closed this area. It did not. It moved the ownership of the serialization; the gate's shape — one permit, unbounded wait, held for the full span from acquisition to disposal — is unchanged, and every precondition the leak hypothesis needs is still present. Correction C1 of the same spec is also relevant to anyone searching: the identifiers `UiThreadDispatcherGate` and `SwapUiThreadDispatcher` appear in **zero** `.cs` files in the current tree, so a search on those names returns nothing and must not be read as evidence that the mechanism is gone. The live identifiers are `UiThreadDispatcherFixture` and `UiThreadDispatcherTransaction`.

Spec unknown U2 of issue 743 — whether a first expiry cascades through a leaked permit — is the precise question, and 743 records it as OPEN and UNTESTED.

**Why this issue must exist rather than being closed inside 743.** Issue 743's remedy is a UI-marshalling seam that routes the affected tests around the concrete viewer and the pump-hosted fixture. That removes the affected tests from the path where the gate is contended. It does not change the gate. If the originating mechanism was H-LEAK, the defect is still present and the seam has AVOIDED it rather than FIXED it. Closing 743 without this issue would retire the observable symptom and lose the open question with it. This is the same disposition applied to the Deedle production risk in issue 879: promote the unresolved part rather than let it close silently inside a merged item.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` and its existing test file `QfcItemController.UiThreadDispatcherFixtureTests.cs`. A test that deliberately abandons a transaction-holding `async` test and then asserts the observable state of the gate is the direct experiment 743 could not run, because 743's instrumented runs produced no expiry to observe.
- [x] Integration scenario to retest: a full `QuickFiler.Test` run in the PARALLEL regime (`/Settings:TaskMaster.runsettings`, Workers 0, Scope ClassLevel), which is the regime in which the gate is genuinely contended (14 contended acquisitions observed) and therefore the regime in which a lost permit would be reachable.
- [x] Manual verification notes: two candidate remedies are worth evaluating and they are not equivalent. (1) Acquire with a bounded timeout or a `CancellationToken`, so a lost permit surfaces as a prompt, attributable failure instead of an unbounded wait — this makes the defect diagnosable but does not prevent the leak. (2) Guarantee release on abandonment, so the permit cannot be lost in the first place. Prefer (2) where it is achievable and treat (1) as a diagnostic backstop rather than the fix.
- **Trap to avoid, stated here because the reader will not have 743's feature folder.** Do not treat a clean run as evidence of absence. A run in which `timeout=0` cannot discriminate this hypothesis at all, for exactly the reason recorded under Actual Behavior above: with no expiry there is no abandonment, and with no abandonment there is no leak under either hypothesis, so the counters read identically whether the defect exists or not. Any verification of this issue must first establish that an expiry actually occurred, and only then read the gate state. A verification artifact that reports a clean run and concludes "no leak" repeats the precise error that 743's verdict artifact had to be corrected for twice.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Cross-reference: this issue originates from **issue 743** (QuickFiler ItemViewer UI-marshalling seam), whose maintainer ratification of the AC1 negative result on 2026-09-13 was granted on the explicit condition that this issue be filed before 743's pull request merges. Issue 743's evidence tree and its pull-request body name this issue in return.
