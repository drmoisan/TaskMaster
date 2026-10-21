# Bug: quickfiler-transactiongate-permit-leak-unexcluded (Issue #882)

- Issue: #882
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/882
- Type: bug
- Work Mode: full-bug
- Promotion Source: docs/features/potential/promoted/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded.md
- Date captured: 2026-09-13
- Last Updated: 2026-09-13
- Status: Active
- Acceptance-criteria source: spec.md (per `acceptance-criteria-tracking`, `full-bug` resolves acceptance criteria from `spec.md` only)

## Summary

QuickFiler's one-permit `TransactionGate` may be able to leak or late-release a permit, and nothing in the repository currently excludes that possibility. Issue 743 set out to discriminate this hypothesis (H-LEAK) from elapsed fixture cost (H-COST) by instrumented measurement, and the measurement did not discriminate them: no expiry occurred in either instrumented run, so the discriminating experiment never took place. H-LEAK was never excluded, only never observed. Issue 743's fix routes the affected tests around the gate via a UI-marshalling seam rather than answering the question, so if H-LEAK is the real mechanism the defect still exists behind the seam. This issue carries that open question forward so it is not retired along with 743's symptom.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 (the machine on which issue 743's instrumented runs were taken)
- Language/runtime: C# / .NET Framework 4.8
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook"` for the SERIAL regime, and the same command with `/Settings:TaskMaster.runsettings` (Workers 0, Scope ClassLevel) for the PARALLEL regime
- Data source or fixture: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` — the `UiThreadDispatcherFixture` / `UiThreadDispatcherTransaction` pair, whose `TransactionGate` is the subject

## Steps to Reproduce

1. Read `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` and confirm the current shape of `TransactionGate`: a `SemaphoreSlim(1, 1)` acquired by `BeginTransactionAsync` and released by `ReleaseTransactionGate` from the transaction's `Dispose`.
2. Observe that the acquisition is awaited with no timeout argument and no `CancellationToken` overload, so a caller that finds the permit held waits without bound.
3. Construct the state the hypothesis requires: an `async` test that acquires a transaction and then exceeds its MSTest `[Timeout(...)]` bound before reaching the `Dispose` that releases the permit. MSTest abandons a timed-out `async` test rather than unwinding it, so the `finally` that would release the permit is no longer observed.
4. Run any later test in the same assembly that acquires the gate.
5. The open question is whether step 4 then blocks without bound on a permit no live holder owns. This has NOT been demonstrated, and demonstrating or refuting it is part of the work of this issue.

## Expected Behavior

A one-permit gate either releases its permit on every path out of a transaction, including abandonment of a timed-out `async` test, or it acquires with a bounded timeout so that a lost permit surfaces as a prompt, diagnosable failure rather than an unbounded wait. A later test must not be able to block indefinitely because an earlier test was abandoned.

## Actual Behavior

Unknown, and that is the defect being filed. Issue 743's instrumented measurement was designed to settle it and did not. Both instrumented runs recorded `timeout=0` — no test was abandoned in either run. Because H-LEAK is by definition a cascade conditional on a prior expiry, the absence of any expiry meant no leak could have occurred under either hypothesis, so the pre-declared observable (`contended`, the count of acquisitions that find the permit held) read zero for a reason entirely independent of whether H-LEAK is true of this codebase. The serial reading `acquisitions=11 releases=10 contended=0` was predetermined by the absence of expiry and carries no information about the hypothesis.

A hypothesis cannot be rejected by the absence of observations. Issue 743's verdict artifact originally claimed H-LEAK was "REJECTED by direct observation", and a first amendment then claimed the rejection "rests entirely on the counter observable, which is a legitimate basis". Both claims were wrong and both have been withdrawn on the 743 branch; the artifact now records that neither hypothesis was discriminated.

## Verification performed at folder creation (2026-09-13)

Both guard sites named in the parallel-run brief were re-measured directly, and both readings are reproduced here so that no later reader has to take the brief on trust:

- In this item's worktree, at base `origin/main` commit `e6d86049e`, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` line 124 reads `await TransactionGate.WaitAsync().ConfigureAwait(false);`. Line 32 declares `private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);`. The release path is `ReleaseTransactionGate` at line 88, whose sole caller is `UiThreadDispatcherTransaction.Dispose` at line 275.
- On the `bug/quickfiler-itemviewer-ui-marshalling-seam-743` branch the identical unbounded acquisition is present at line 149 of the same file. Issue 743 therefore does not deliver this change, and this issue is genuinely outstanding rather than a duplicate of work already in flight.

## Scope Direction Carried From the Filing

The issue's Expected Behavior states the remedy disjunctively. Delivery must not be made conditional on a demonstration of H-LEAK succeeding, because that conditionality is what left issue 743's residual open. The bounded-acquisition half of the disjunction is deliverable and testable regardless of whether H-LEAK reproduces, and it is the primary deliverable. An attempt to demonstrate or refute H-LEAK deterministically is desirable supporting evidence, not a precondition for the change.

The subject file is test-assembly infrastructure, not shipped add-in production code. The repository determinism rules in `.claude/rules/general-unit-test.md` prohibit wall-clock waits in test code. A bounded `WaitAsync(TimeSpan)` on a synchronization primitive owned by a fixture is infrastructure timeout policy, not a test-body sleep, and the distinction is recorded explicitly in `spec.md` so that review does not mistake one for the other.

## Trap to Avoid

Do not treat a clean run as evidence of absence. A run in which `timeout=0` cannot discriminate this hypothesis at all: with no expiry there is no abandonment, and with no abandonment there is no leak under either hypothesis, so the counters read identically whether the defect exists or not. Any verification of this issue must first establish that an expiry actually occurred, and only then read the gate state. A verification artifact that reports a clean run and concludes "no leak" repeats the precise error that issue 743's verdict artifact had to be corrected for twice.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium rather than High because the failure mode is confined to the test assembly and has not been shown to affect the shipped add-in. Medium rather than Low because an unbounded wait on a lost permit presents as a hung or timed-out test run whose cause is not local to the failing test, which is expensive to diagnose and is a plausible contributor to the historical flake rate recorded in issues 592, 511 and 571.

## Provenance Note

Issue #882 was filed on GitHub before this feature folder was created, as condition 4 of the maintainer ratification recorded on issue 743. The MCP promotion tool `potential_to_issue` was therefore deliberately not invoked for this item, because that tool always files a new issue and a duplicate of #882 would be a defect. Only the folder-creation half of the `feature-promotion-lifecycle` skill was performed.

## Related

- Originating issue: #743 (QuickFiler ItemViewer UI-marshalling seam)
- Historical flake reports this gate is a plausible contributor to: #592, #511, #571
- Frequently miscited as closing this area, and does not: #493
