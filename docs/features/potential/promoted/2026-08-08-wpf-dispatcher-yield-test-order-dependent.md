# wpf-dispatcher-yield-test-order-dependent (Issue #508)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/wpf-dispatcher-yield-test-order-dependent/ (Issue #508)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #508
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/508
- Last Updated: 2026-08-08
## Summary

`UtilitiesCS.Test\OutlookObjects\Folder\WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` fails intermittently on the full-suite run. The test asserts that `WpfDispatcherYield.YieldAsync` throws `InvalidOperationException` when no WPF `Dispatcher` is associated with the current thread, but under class-level parallel execution the pooled worker thread it lands on may already have a `Dispatcher` attached by an earlier test, so no exception is thrown and the assertion fails.

## Environment

- OS/version: Windows 11
- Runtime: .NET Framework 4.8.1
- Command/flags used: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug` (full-suite run, class-level parallelization, 24 workers)
- Data source or fixture: none

## Steps to Reproduce

1. Build the solution in Debug.
2. Run the full MSTest suite across all `*.Test.dll` assemblies with parallelization enabled, as CI and the local coverage script both do.
3. Repeat the run.

Observed across two consecutive baseline runs on `bug/ribbon-engine-readiness-guard-503` at merge-base `003c5715`:

- Run 1: `Total tests: 6293, Passed: 6291, Failed: 2`
- Run 2: `Failed: 1` — `YieldAsync_WithoutDispatcher_RemainsStrict`

The failure count changed between runs with no intervening code change, which establishes non-determinism rather than a deterministic regression.

## Expected Behavior

The test produces the same result on every run regardless of which other tests executed first or which pooled thread it runs on, per `.claude/rules/general-unit-test.md` Core Principles 1 (Independence) and 4 (Determinism).

## Actual Behavior

```csharp
[TestMethod]
public async Task YieldAsync_WithoutDispatcher_RemainsStrict()
{
    var dispatcherYield = new WpfDispatcherYield();

    await dispatcherYield
        .Invoking(item => item.YieldAsync(CancellationToken.None))
        .Should()
        .ThrowAsync<InvalidOperationException>();
}
```

(`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:28-37`.)

The "without dispatcher" precondition is an ambient property of the executing thread, not something the test arranges. `Dispatcher.CurrentDispatcher` creates and caches a dispatcher for the calling thread on first access, so any earlier test on the same pooled worker that touches a WPF dispatcher leaves one attached, and this test's precondition silently evaporates.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet:

```text
  Failed YieldAsync_WithoutDispatcher_RemainsStrict [162 ms]
  Error Message:
     Failed: 1
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The suite is not reliably green at baseline, which undermines every downstream quality gate: an agent or developer cannot distinguish "my change broke a test" from "the suite is flaky". It also produces spurious CI failures and encourages re-running until green, which is the failure mode the determinism rule exists to prevent.

## Suspected Cause / Notes

The test depends on ambient thread state rather than arranging its own precondition. The correct shape is to run the "no dispatcher" assertion on a thread the test itself owns and can guarantee is dispatcher-free, or to seam the dispatcher lookup behind an injectable accessor so the absent-dispatcher case can be arranged explicitly rather than inherited.

Note that `.claude/rules/csharp.md` prohibits "adding sleeps, retries, or timing hacks to mask flaky behavior", so `[Retry]`-style mitigation is not an acceptable fix. `.claude/rules/general-unit-test.md` requires seam-based mocking for external boundaries, which points at the injectable-accessor approach.

The sibling test `YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield` is unaffected because it cancels before the dispatcher is consulted.

Found while capturing the merge-base baseline for issue #503 (ribbon engine readiness guard); out of scope for that fix. Recorded there as a known pre-existing baseline condition so it is not misattributed to #503.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: arrange the dispatcher-free precondition explicitly (dedicated owned thread, or an injected dispatcher accessor returning null) so the assertion is deterministic.
- [x] Integration scenario to retest: run the full suite repeatedly and confirm a stable pass count across runs.
- [x] Manual verification notes: confirm no `[DoNotParallelize]` blanket suppression is used as the fix, since that hides the dependency rather than removing it.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
