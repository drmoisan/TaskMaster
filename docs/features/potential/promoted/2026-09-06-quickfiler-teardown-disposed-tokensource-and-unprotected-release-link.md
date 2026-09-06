# quickfiler-teardown-disposed-tokensource-and-unprotected-release-link (Issue #793)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-teardown-disposed-tokensource-and-unprotected-release-link/ (Issue #793)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #793
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/793
- Last Updated: 2026-09-06
## Summary

Two residual teardown defects found by the #791 feature review (code-review.2026-09-06T15-31.md findings N1 and N2). (1) `QfcHomeController.Cleanup()` now disposes `_tokenSource` but does not null the field, and the same `CancellationTokenSource` is shared with the datamodel and the form controller, whose `Cleanup()` and `QuiesceLoaderAsync()` both call `_tokenSource?.Cancel()`; a call after disposal throws `ObjectDisposedException`. (2) `QfcFormController.Cleanup()` invokes `_parentCleanup?.Invoke()` as its last statement with no `try`/`finally`, so a throw from the viewer dispose immediately before it skips the ribbon release callback, contradicting the "release invoked exactly once regardless of which stage threw" invariant recorded in the #791 spec.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: static review of branch bug/quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791 at 59536368
- Data source or fixture: none (code review finding)

## Steps to Reproduce

1. Read QuickFiler/Controllers/QfcHomeController.cs `Cleanup()` (around line 389 on the #791 branch): `_tokenSource?.Dispose()` with no `_tokenSource = null;`.
2. Read QuickFiler/Controllers/QfcDatamodel.cs `Cleanup()` and QfcDatamodel.QueueProcessing.cs `QuiesceLoaderAsync()`: both open with `_tokenSource?.Cancel()` on the shared source.
3. Read QuickFiler/Controllers/QfcFormController.SetupDisposal.cs `Cleanup()` (line 251 disposes the viewer; line 259 invokes `_parentCleanup`), with no `finally`.

## Expected Behavior

- Disposing the shared token source cannot cause a later `Cancel()` on a still-referenced copy to throw; the field is nulled after disposal, or ownership of the source is single and the sharers hold only the token.
- The ribbon release callback runs from `QfcFormController.Cleanup()` under a `finally`, so a throwing viewer dispose cannot skip it.

## Actual Behavior

- `Cleanup()` disposes the source and leaves the field set; a second `Cancel()` through any sharer throws `ObjectDisposedException`. Today this is unreachable only because `QfcFormController.Cleanup()` nulls `_parentCleanup` and `_parent` first and `RibbonController` never calls `QfcHomeController.Cleanup()` directly. Before #791 the source was never disposed, so the throw is newly possible.
- `_parentCleanup?.Invoke()` at SetupDisposal.cs:259 is unprotected; that file was an explicit non-goal of #791 (AC5), which is why the fix was not applied there.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; static finding. Source: docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/code-review.2026-09-06T15-31.md (N1, N2).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: both are latent today, but each breaks the single-release-callback invariant #791 established once any future caller reaches the unguarded paths.

## Suspected Cause / Notes

- N1 was introduced by #791's `QfcHomeController.Cleanup()` hardening (dispose added without nulling); the sharing of one `CancellationTokenSource` across three controllers predates it.
- N2 predates #791 and sits in QfcFormController.SetupDisposal.cs, which #791 kept out of scope.
- Related: #791, #731 (controller lifecycle disposal).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `QfcHomeController.Cleanup()` called twice does not throw; a sharer's `Cancel()` after home-controller cleanup does not throw; `QfcFormController.Cleanup()` invokes `_parentCleanup` exactly once when viewer dispose throws.
- [ ] Integration scenario to retest: Cancel then immediate relaunch from the ribbon.
- [ ] Manual verification notes: none beyond the #791 runbook.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
