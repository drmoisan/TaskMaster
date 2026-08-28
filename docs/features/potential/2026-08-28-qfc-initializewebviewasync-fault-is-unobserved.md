# qfc-initializewebviewasync-fault-is-unobserved (Potential Bug)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`QfcItemController.InitializeWebViewAsync` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:48`)
returns a `Task` that **three of its four production call sites discard**, so any exception it raises becomes an
unobserved task exception rather than a diagnostic anyone sees. The method is the sole entry point for WebView2
environment creation, core initialization, and — at `ViewerSetup.cs:112` — the call to `EnsureBreadcrumbPipeline()`.
Issue #488's D5 fix makes that path newly capable of throwing `ObjectDisposedException` when the pipeline is built
against a viewer whose teardown has begun, which converts a previously silent leak into a fault that is itself
silently swallowed.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1, VSTO / WinForms)
- Command/flags used: n/a — identified by source reading during issue #488 execution, discharging research §3.5
- Data source or fixture: `QuickFiler/Controllers/QfcItemController.Initialization.cs` call sites

## Steps to Reproduce

1. Drive a `QfcItemController` through any of the three fire-and-forget initialization paths listed under
   "Suspected Cause / Notes".
2. Arrange for `InitializeWebViewAsync` to fault — for example by disposing the `ItemViewer` before the posted
   continuation reaches `EnsureBreadcrumbPipeline()`, which after #488's D5 fix throws `ObjectDisposedException`.
3. Observe that no exception surfaces to the caller, no log entry is written by the call site, and initialization
   silently completes as far as any observer can tell.

## Expected Behavior

A faulted `InitializeWebViewAsync` should be observed by its caller — awaited, `ContinueWith`-observed, or routed to
the repository's logging pattern — so that a failure during WebView2 initialization is diagnosable rather than
invisible.

## Actual Behavior

The task is discarded at three of the four call sites, so the fault is never observed:

| Call site | Form | Observed? |
| --- | --- | --- |
| `QfcItemController.Initialization.cs:192` | `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);` | **no** — discarded, and additionally wrapped in a WPF `DispatcherOperation` |
| `QfcItemController.Initialization.cs:256` | `await InitializeWebViewAsync();` | yes — awaited into the enclosing async method's task |
| `QfcItemController.Initialization.cs:288` | `_ = InitializeWebViewAsync();` | **no** — discarded |
| `QfcItemController.Initialization.cs:324` | `_ = InitializeWebViewAsync();` | **no** — discarded |

On .NET Framework 4.5 and later an unobserved task exception no longer terminates the process by default, so the
fault is finalized away with no observable effect at all.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log — that is the defect. Identified by source reading, recorded in
  `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/d5-faulted-task-observation.md`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The severity comes from the failure mode rather than the likelihood. A WebView2 initialization failure — a missing
runtime, a locked cache directory, a disposed viewer — produces no diagnostic on three of four paths, so the
breadcrumb surface simply never appears and the cause is unavailable to anyone triaging it.

## Suspected Cause / Notes

The three discarding sites were written as deliberate fire-and-forget dispatches, with the comment "Fire and forget
WebView initialization" at `Initialization.cs:191`. The intent — not blocking initialization on a WebView2 round trip
— is sound; discarding the fault is the part that is not.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and
`QuickFiler/Controllers/QfcItemController.Initialization.cs` are owned by feature
`qfc-item-controller-defects-484`, so this was **not** fixed inside #488. Research §3.5 required that if the task
proves unobserved the correct response is a new issue against `ViewerSetup.cs`, **not** a weakening of D5's guard.
D5's guard is delivered unweakened.

`EfcItemController.cs:97` and `:153` use `Task.Run(() => InitializeWebViewAsync());` against that class's own
same-named method and discard the returned task too; whether that belongs in the same fix is worth evaluating.

Options worth evaluating:

- Attach a continuation at each fire-and-forget site that routes a fault to the project's logging pattern.
- Introduce a small `FireAndForget(Task, ILogger)` helper so the three sites share one observation policy.
- Subscribe `TaskScheduler.UnobservedTaskException` at the add-in boundary as a backstop only.

Files to inspect: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`,
`QuickFiler/Controllers/QfcItemController.Initialization.cs`, `QuickFiler/Controllers/EfcItemController.cs`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test that forces `InitializeWebViewAsync` to fault at the mocked web-view seam and asserts the fault is observed and logged rather than discarded
- [ ] Integration scenario to retest: dispose an `ItemViewer` mid-initialization and confirm the resulting `ObjectDisposedException` reaches a log
- [ ] Manual verification notes: confirm the three fire-and-forget sites still do not block initialization after the change

## Next Step

Promote to a GitHub issue against `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, referenced from issue
#488's D5 acceptance criterion.
