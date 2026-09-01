# wpfuidispatchertests-ungated-static-swap (Issue #648)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/wpfuidispatchertests-ungated-static-swap/ (Issue #648)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #648
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/648
- Last Updated: 2026-08-27
- Work Mode: minor-audit

## Summary

`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` swaps the process-wide static
`UtilitiesCS.Threading.UiThread._dispatcher` to a running WPF dispatcher by raw reflection
(`typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)` then
`field.SetValue(null, dispatcher)`), restores it in a plain `finally`, and participates in neither of
the two locks introduced by #493. After #493 lands it remains an ungated mutator of the same static
and can still lose an update against a transaction held by the QuickFiler pump fixtures.

Unlike the originating #493 defect, this call site **does** restore the previous value, so it is a
lesser, distinct concern rather than a recurrence of the no-restore bug. What it lacks is
participation in the lock protocol: it never acquires `UiThreadDispatcherFixture.FieldLock`, so its
read-modify-write can interleave with a fixture transaction, and its restore is an unconditional
write rather than the fixture's `ReferenceEquals` compare-then-write.

Proposed fix: route the swap through the shared fixture that #493 created —
`await UiThreadDispatcherFixture.BeginTransactionAsync()`, then `transaction.Install(dispatcher)`,
and replace the `finally` restore with `transaction.Dispose()`, which restores conditionally and then
releases the gate, in that order. Do not reintroduce a second reflection lookup;
`UiThreadDispatcherFixture` is intended to be the single owner of every mutation of that static made
from this assembly's owned files, and #493's AC-4 gates that uniqueness.
`UiThreadDispatcherFixture` and `UiThreadDispatcherTransaction` live in
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`; both are `internal` to
`QuickFiler.Test`, so no new grant or assembly reference is needed.

This was recorded as accepted residual risk **R-1** of #493 and its § Rollout & Follow-up item 3
asked that it be promoted as its own small issue once the shared fixture exists. It now exists.

Out of scope: the cross-assembly mutators in `UtilitiesCS.Test` (`ProgressTracker_Tests.cs`,
`ProgressTrackerAsync_Tests.cs`, `IdleAsyncQueue_Tests.cs`) mutate the same process-wide static and
are **not** covered here. No test-side lock inside `QuickFiler.Test` can reach them. They are
accepted residual risk R-2 of #493 and overlap #584.

References: motivating fix #493; adjacent open issue on the same static #584; originating defect
report #230.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 (defect is environment-independent; it is a test-isolation
  defect in source, not a platform behavior)
- Python version: n/a — C# / .NET Framework 4.8 (`QuickFiler.Test`, MSTest)
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`
- Data source or fixture: `QfcItemControllerTestSupport.StartRunningDispatcher()`

## Steps to Reproduce

1. Inspect `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, test
   `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread`.
2. Observe the raw reflection write to `UiThread._dispatcher` and the unconditional `finally` restore,
   with no acquisition of `UiThreadDispatcherFixture.FieldLock` or `TransactionGate`.
3. Note that after #493, all mutations from the owned files hold `FieldLock` for the whole
   read-modify-write, so this site is the remaining ungated writer inside `QuickFiler.Test`.

Note: the race is dormant under current CI settings, so a deterministic red run is not expected
without forcing class-level parallelism (see Impact / Severity).

## Expected Behavior

Every mutation of `UiThread._dispatcher` originating in `QuickFiler.Test` goes through
`UiThreadDispatcherFixture`, holds `FieldLock` for the entire read-modify-write, and restores
conditionally via `ReferenceEquals` compare-then-write.

## Actual Behavior

`WpfUiDispatcherTests.cs` mutates the static directly by reflection without holding either lock, and
restores unconditionally. A concurrent fixture transaction can therefore be clobbered, and this
site's restore can overwrite a value another transaction installed.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no failing run is attached. The defect is a latent ordering hazard that is dormant under
  the CI settings described below; it is evidenced by source inspection rather than a red test.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low. That assembly runs sequentially in CI (`.github/workflows/_mstest-coverage.yml` supplies no
`/Settings:`), so the race is dormant there; it is reachable only under the repo runsettings, which
force `<Scope>ClassLevel</Scope>` with `Workers=0`. The swap is single-class and short-lived. This is
a small, bounded change.

## Suspected Cause / Notes

The file predates the shared fixture introduced by #493, so it had no gated path available when it was
written. Files to inspect: `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` and
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `WpfUiDispatcherTests` must keep asserting that `Invoke`, `InvokeAsync`,
      and `BeginInvoke` marshal onto the dispatcher thread; behavior must not change.
- [x] Integration scenario to retest: full `QuickFiler.Test` run, plus a run under the repo
      runsettings (`ClassLevel`, `Workers=0`) to exercise concurrent classes.
- [x] Manual verification notes: confirm exactly one reflection lookup of `_dispatcher` remains in
      `QuickFiler.Test` after the change, and that `UiThread._dispatcher` is unchanged after the suite.

## Acceptance Criteria

Authored on 2026-08-31 during promotion-to-active remediation. The promoted record carried no
`## Acceptance Criteria` section, which the `minor-audit` work mode requires as the sole
acceptance-criteria source, so this section was added before planning began. Every count below was
derived exhaustively against `origin/main` at commit `2b85134b42872e405602e6064e02dc9cda6c319b` and
cross-checked by two independent search methods.

- [ ] AC-1 — Single reflection owner. After the change, the quoted literal `"_dispatcher"` appears on
      exactly one line beneath `QuickFiler.Test/`, and that line is in
      `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`. The baseline is
      two lines: that fixture, and `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`.
- [ ] AC-2 — No reflection remains in the test file.
      `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` contains no occurrence of `GetField`, no
      occurrence of `SetValue`, and no `using System.Reflection;` directive.
- [ ] AC-3 — Swap routed through the shared fixture. The test obtains its gate from
      `UiThreadDispatcherFixture.BeginTransactionAsync()`, installs the running dispatcher through the
      returned `UiThreadDispatcherTransaction`, and restores by disposing that transaction rather than
      by writing the field. The test method is declared `async Task` because the gate is awaited.
- [ ] AC-4 — Behavior preserved. `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread`
      still asserts that `Invoke`, `InvokeAsync`, and `BeginInvoke` each execute their delegate on the
      dispatcher's own thread, and the body of `Construction_YieldsAnIUiDispatcher` is unchanged.
- [ ] AC-5 — Tests green with no regression. A scoped run of `QuickFiler.Test.dll` restricted to
      `WpfUiDispatcherTests` reports zero failed tests with both of that class's tests passing, and a
      full `QuickFiler.Test.dll` run reports zero failed tests with a passed count no lower than the
      Phase 0 baseline recorded under `evidence/baseline/`.
- [ ] AC-6 — Scope boundary held. The branch diff against `origin/main` changes exactly one path with
      a `.cs` extension, `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, and changes no path
      beneath `UtilitiesCS.Test/` or `UtilitiesCS/`. The three out-of-scope cross-assembly mutators
      named under Summary remain untouched.
- [ ] AC-7 — Toolchain green and evidence complete. The CSharpier check, the analyzer rebuild, and
      the nullable rebuild each report zero errors and introduce no new finding relative to the
      Phase 0 baseline capture; and the canonical evidence tree carries the Phase 0 baseline
      artifacts, the Phase 2 final-QC artifacts, and a fail-before record under
      `evidence/regression-testing/` that is either a recorded failing run or a schema-valid
      `fail-before-exception` dossier stating why a deterministic red run is structurally impossible.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
