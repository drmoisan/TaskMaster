# breadcrumb-router-segment-index-unvalidated-host-crash (Issue #498)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-router-segment-index-unvalidated-host-crash/ (Issue #498)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #498
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/498
- Last Updated: 2026-08-08
## Summary

An out-of-range `segmentIndex` in a `segmentDoubleClick` breadcrumb message passes codec validation,
throws `ArgumentOutOfRangeException` deep in `BreadcrumbRow.CollapseAfter`, and escapes the
`async void` host-event boundary in `BreadcrumbBridgeRouter.OnHostMessageReceived`, which catches
only `BreadcrumbMessageException`. On .NET Framework 4.8 an exception rethrown on the captured
`SynchronizationContext` from an `async void` method is unhandled, so a malformed message from the
WebView2 document can terminate the Outlook host process.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2)
- Command/flags used: n/a — reached through the EfcViewer folder-list breadcrumb surface
- Data source or fixture: any breadcrumb row whose `segmentIndex` exceeds the row's segment count

## Steps to Reproduce

1. Open the EfcViewer folder list so the breadcrumb surface is hosted and
   `EfcFormController.ConfigureBreadcrumbControl` (`QuickFiler/Controllers/EfcFormController.cs:834-854`)
   has wired a `BreadcrumbBridgeRouter` to a `WebView2BreadcrumbHost`.
2. Have the hosted document post the message
   `{"type":"segmentDoubleClick","rowId":"row-1","segmentIndex":99}` for a row that has fewer than
   100 segments.
3. Observe the add-in host process.

## Expected Behavior

The router's own XML doc comment at `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:151-154` states
the contract: a malformed payload should "fail fast with the codec's `BreadcrumbMessageException`
(already logged) and leave state unchanged." An out-of-range index should therefore be rejected and
logged, leaving row state untouched and the host running.

## Actual Behavior

`ArgumentOutOfRangeException` propagates out of `async void OnHostMessageReceived` and is rethrown on
the captured `SynchronizationContext` (the Outlook UI thread in production), producing an unhandled
exception and a host-process crash. The documented contract at `:151-154` is false for this input.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `System.ArgumentOutOfRangeException` originating in `BreadcrumbRow.CollapseAfter`
  (`QuickFiler/Controllers/BreadcrumbRow.cs:111-118`).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Rationale: the failure mode is host-process termination rather than a degraded feature, and the
input arrives from the hosted WebView2 document rather than from trusted in-process code.

## Suspected Cause / Notes

Verified call chain:

1. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:169` —
   `if (row.CollapseAfter(message.SegmentIndex!.Value))`, with no range check. The null-forgiving
   operator asserts only that the codec validated *presence*.
2. `QuickFiler/Controllers/BreadcrumbMessageCodec.cs:100` and `:142-158` — `OptionalInt` validates
   only that the token is a JSON integer; `:103-106` validates only that the field is present for
   `segmentDoubleClick`. **The codec performs no range validation.**
3. `QuickFiler/Controllers/BreadcrumbRow.cs:111-118` — `CollapseAfter` throws
   `ArgumentOutOfRangeException` when `segmentIndex < 0 || segmentIndex >= _segments.Count`.
4. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:193` — the `catch` clause is
   `BreadcrumbMessageException`-only, so the exception escapes the `async void` boundary.

Discovered during preparation research for epic #136 child F12 (issue #495), recorded in
`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/research/2026-08-08T02-10-breadcrumb-bridge-router.md`
as LD-1. Not a duplicate of #440 (arrow-key tree semantics), #458 (host handler retention on pooled
viewers), #462 (drop-down `closePending`), or #488 (ItemViewer breadcrumb pipeline lifecycle).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a failing regression test first, per the repository Bugfix Workflow,
      driving a `segmentDoubleClick` message with an out-of-range `segmentIndex` through
      `BreadcrumbBridgeRouter` and asserting no exception escapes and row state is unchanged.
- [ ] Integration scenario to retest: EfcViewer folder-list breadcrumb double-click on a
      multi-segment row, confirming normal collapse still works after the guard is added.
- [ ] Manual verification notes: two candidate fixes — add a range guard at
      `BreadcrumbBridgeRouter.cs:169`, or widen the `catch` at `:193`. The guard is preferable because
      it preserves the documented "leave state unchanged" contract rather than merely suppressing the
      throw. Either is an observable behavior change and so was out of scope for #495, whose epic
      carries a no-behavior-change NFR.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
