# quickfiler-itemviewer-ui-marshalling-seam (Issue #743)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-itemviewer-ui-marshalling-seam/ (Issue #743)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #743
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/743
- Last Updated: 2026-09-02
## Summary

`QuickFiler.Test` pump-hosted `QfcItemController`/`WebView2BreadcrumbHost` tests expire at the 60-second `PumpTimeoutMs` harness bound under CPU contention because the production code under test (`QuickFiler/Viewers/ItemViewer.cs`) has no injectable UI-marshalling seam; the fix requires a `QuickFiler/` production-code change and is out of scope for issue #729 (test-determinism-and-hygiene-debt), which is a test-only consolidation. This finding was previously tracked standalone as issue #711, closed 2026-09-02 as "superseded by consolidated issue #729"; #729's research concluded finding 4 cannot be closed deterministically within that item and must be re-promoted here so it is not silently dropped a second time.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C# MSTest suite
- Command/flags used: `vstest.console.exe` over discovered `*.Test.dll` with `/EnableCodeCoverage /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- Data source or fixture: `QuickFiler.Test` pump-host and dispatcher fixtures (`QfcItemController.InitializationTests*.cs`, `.SeamFactoryTests.cs`, `.ViewerSetupTests.cs`, `WebView2BreadcrumbHostTests.cs`)

## Steps to Reproduce

1. Run the full discovered `QuickFiler.Test` set with `/EnableCodeCoverage` while the machine is under concurrent load.
2. Observe pump-hosted `QfcItemController`/`WebView2BreadcrumbHost` tests fail at approximately 60 seconds each.
3. Re-run the same command against a byte-identical tree with the machine idle.
4. Observe the same tests pass.

## Expected Behavior

Per `.claude/rules/general-unit-test.md`'s Determinism Infrastructure section, test outcomes must not depend on host load. Pump-hosted `QfcItemController`/`WebView2BreadcrumbHost` tests should complete deterministically regardless of CPU contention, either by scaling the harness bound to the environment or by allowing a synchronous fake to replace the real message pump for the members under test.

## Actual Behavior

Research performed under issue #729 (2026-09-02) confirmed: `PumpTimeoutMs` (declared 4 times across `QfcItemController.InitializationTests.cs:38`, `.SeamFactoryTests.cs:327`, `.ViewerSetupTests.cs:34`, `WebView2BreadcrumbHostTests.cs:25`) is used in exactly 19 places, all as the argument of an MSTest `[Timeout(...)]` attribute, and never as a wait/poll duration — the test logic itself contains no `Thread.Sleep`, `Task.Delay`, `Stopwatch`, or polling loop. The load-sensitivity is not a test-determinism defect in the MSTest-visible sense; it is that the *real elapsed cost* of constructing the production object graph under test can exceed the 60-second harness bound under contention.

No test-only fix removes this, for four reasons documented in the #729 research artifact (`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md`, section 4.2):

1. `QuickFiler/Viewers/ItemViewer.cs:23-29` reads `SynchronizationContext.Current` and `Dispatcher.CurrentDispatcher` directly at construction (`_context = SynchronizationContext.Current;`), with no injectable seam; `UiSyncContext` (`ItemViewer.cs:59-62`, `IItemViewer.cs:37`) exposes it read-only. `QfcItemController.ViewerSetup.cs:64,320,331,336` await it directly.
2. The dominant fixture cost is a real WinForms control tree plus two handle-created `WebView2` children (`QfcItemController.InitializationTests.Part2.cs:74-84`), which only a real Win32 message loop can service — a fake `SynchronizationContext` cannot substitute for `Control.Handle`/`BeginInvoke` marshalling.
3. `[DoNotParallelize]` would be a no-op: `QuickFiler.Test` already runs serially (no `[assembly: Parallelize]`, CI passes no `/Settings:`).
4. Removing `[Timeout]` would trade a bounded, diagnosable failure for an unbounded CI hang on a genuine deadlock (documented rationale at `QfcItemController.InitializationTests.cs:33-37`).

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: each failure is recorded with an elapsed time of approximately 60 seconds and a `[Timeout]` message rather than an assertion-failure message, consistent with issue #711's original report.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no incorrect production behavior; test-suite reliability/determinism debt that risks intermittent CI failures on a coverage-enabled or loaded runner, consistent with the closed #711 report this issue re-promotes.

## Suspected Cause / Notes

Root cause: `QuickFiler/Viewers/ItemViewer.cs` and the `QfcItemController` members it backs have no injectable UI-marshalling abstraction (an `IUiDispatcher`/`SynchronizationContext` seam, or an interface over the `WebView2` control accepted by `WebView2BreadcrumbHost`'s constructor). Until such a seam exists, the pump-hosted tests must construct real WinForms/WebView2 objects and are therefore coupled to real message-pump timing. This is a `QuickFiler/` production-code change, which was out of scope for issue #729 (test-only item; `QuickFiler/` production sources are owned by a different parallel work item in that run).

## Proposed Fix / Validation Ideas

- [ ] Give `QfcItemController`/`ItemViewer` an injectable `IUiDispatcher` (or equivalent `SynchronizationContext` provider) seam so tests can substitute a synchronous fake instead of relying on a live message pump.
- [ ] Introduce an interface over the `WebView2` control that `WebView2BreadcrumbHost` accepts via constructor injection, so its handle-creation cost is not incurred in pump-hosted unit tests.
- [ ] Once a seam exists, re-evaluate whether `PumpTimeoutMs = 60000` can be lowered, since the remaining wait would then be a controllable fake rather than a real Win32 message loop.
- [ ] Do not remove `[Timeout(PumpTimeoutMs)]` outright; it is a documented deadlock guard, not the load-sensitivity source.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
