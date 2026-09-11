# qfcqueue-enqueue-path-lacks-injectable-seams (Potential Bug)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`QfcQueue.EnqueueAsync` (0 of 46 lines covered) and `QfcQueue.LoadControllersViewersAsync` (0 of 24 lines covered) drive a live WinForms `TableLayoutPanel`, per-row `ItemViewer` construction, UI idle-call marshalling, and the `EmailMoveMonitor` hook through private members with no injectable seam, so their control flow cannot be unit-tested without a live Outlook window. Item #678 left its acceptance criterion AC20 unchecked for this reason and issue #727 sub-finding 4 recorded it as a policy gap. The maintainer decision recorded on #727 on 2026-09-11 is that the resolution is a seam, not a coverage exemption.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C# / .NET Framework 4.8 VSTO add-in, MSTest + Moq)
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` over `QuickFiler.Test.dll`; static inspection of `QuickFiler/Controllers/QfcQueue.cs` and `QuickFiler/Controllers/QfcQueue.Enqueue.cs`
- Data source or fixture: the committed Cobertura evidence for item #678 (PR #724), which reports both members at zero covered lines

## Steps to Reproduce

1. Search `QuickFiler.Test` for `EnqueueAsync`: zero references across `QfcQueueTests.cs`, `QfcQueuePurePathsTests.cs`, and `QfcQueueCoverageExpansionTests.cs`.
2. Read `QfcQueue.Enqueue.cs:71-138`. `EnqueueAsync` calls `_moveMonitor.HookItem` on a field initialised inline to `new EmailMoveMonitor()` (`QfcQueue.cs:42`), clones `_tlpTemplate` through `UiIdleCallAsync`, and awaits `LoadControllersViewersAsync` through `UiIdleAsyncCallAsync`.
3. Read `QfcQueue.Enqueue.cs:154-198`. `LoadControllersViewersAsync` calls `AddAsync` (`QfcQueue.cs:264`), which constructs a viewer and adds it to the `TableLayoutPanel` on the UI idle path. Only the item-controller construction that follows is behind a seam (`ItemControllerFactory`, added by #678).
4. Attempt to write an MSTest that exercises either member: construction of the real `TableLayoutPanel`, the real `ItemViewer`, and the real `EmailMoveMonitor` requires a message pump and Outlook COM objects, which unit-test policy prohibits.

## Expected Behavior

Both members accept mocks for every boundary they drive, following the injectable-delegate pattern already used for `ItemControllerFactory` (form 2 of the DI seam guidance in `.claude/rules/csharp.md`): the move-monitor hook, the template clone and viewer/row construction performed by `AddAsync`, and the UI idle-call marshalling. Tests then cover the argument-guard branches, the `_jobsRunning` increment and decrement, the `OperationCanceledException` and general-exception catch paths, the `CollectionChanged` raise in `finally`, the `digits` computation, and the carrier resolution per row, without a live WinForms viewer or Outlook process. Both members reach the 90% new-code floor as written.

## Actual Behavior

Neither member is reachable from a unit test. The production defaults are constructed inline from private fields, so no test can substitute them. Coverage on both members is 0%, and item #678's AC20 remains unchecked.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: item #678 review, AC20 recorded PARTIAL (PR #724); issue #727 sub-finding 4.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no incorrect production behavior. Two members on the high-confidence display path carry zero test coverage, and every future change to them is unverifiable by the unit suite.

## Suspected Cause / Notes

- `QfcQueue.cs:42` — `_moveMonitor` initialised inline; `IEmailMoveMonitor` already exists, so a constructor or settable-property seam is sufficient.
- `QfcQueue.cs:264-275` — `AddAsync` constructs the viewer and calls `AddViewerToTlp` inside `UiIdleCallAsync`; needs a delegate seam returning `QfcItemGroup` for a given `(tlp, mailItem, index)`.
- `QfcQueue.cs:474-490` — the three `UiIdleCallAsync` overloads marshal onto the UI idle path; a synchronous pass-through delegate makes them deterministic in tests.
- `QfcQueue.Enqueue.cs:33-56` — `ItemControllerFactory` is the pattern to follow. No new interface is required.
- `QfcQueue.cs` is at 439 lines and `QfcQueue.Enqueue.cs` at 200; the seams must not push either file over 500.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `EnqueueAsync` guard branches, both catch paths, `finally` bookkeeping and `CollectionChanged`; `LoadControllersViewersAsync` digit computation, carrier resolution, per-row factory invocation and `InitializeAsync` await.
- [x] Integration scenario to retest: high-confidence mode with more than one page, confirming background pages still render (manual, live Outlook).
- [x] Manual verification notes: production defaults must reproduce the previous construction expressions exactly, argument for argument, as `ItemControllerFactory` did.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
