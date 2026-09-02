# efc-archiveroot-boundary-sink-defects (Issue #736)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-archiveroot-boundary-sink-defects/ (Issue #736)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #736
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/736
- Last Updated: 2026-09-02
## Summary

Five consolidated findings from a code-review sweep, concentrated in `QuickFiler/Controllers/EfcFormController.cs` and `TaskMaster/AppGlobals/AppOlObjects.cs`: unguarded archive-root COM reads, an unhandled keyboard-dispatch path, a log-only error boundary, five unguarded property reads, and a crash-as-pass-condition test — grouped as one issue since they share the same two files and the same underlying theme (the EFC boundary doesn't guard against, or diagnose, the failures its own comments and sink pattern anticipate).

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C#/.NET Framework 4.8.1 WinForms VSTO add-in with Outlook COM interop
- Command/flags used: n/a — findings are from code review
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable in the usual sense — each finding below is a static code-review finding. See "Actual Behavior."

## Expected Behavior

Each finding's expected behavior is stated inline below.

## Actual Behavior

**1. `ArchiveRootPath` getter performs unguarded COM reads before validation (Source: #696).** `TaskMaster/AppGlobals/AppOlObjects.cs` `ArchiveRootPath` getter evaluates `Path.Combine(Root.FolderPath, "Archive")` and `ArchiveRoot?.FolderPath` — both live Outlook COM property reads — as arguments to `ArchiveRootPathGuard.RequireResolvedArchiveRoot` before that guard gets a chance to validate anything. Neither read is wrapped in a COM-exception guard, so a transient Outlook COM failure here throws uncaught rather than being handled by the very guard this code is structured around.

**2. `KbdExecuteAsync` has no try/catch (Source: #695, part A).** `EfcFormController.cs:894-903` — both `KbdExecuteAsync` overloads (`Func<Task>` and `System.Action`) call `action()`/`await action()` with no local exception handling. Any exception the dispatched action throws propagates uncaught from a keyboard-input dispatch path.

**3. `ActionOkAsync` hides the form before `await`, disposes only after (Source: #695, part B).** `EfcFormController.cs` `ActionOkAsync` (~lines 738-770) hides the form before its `await`, then disposes resources only afterward — a window during which the form is invisible but not yet torn down, widening the surface for a mid-sequence exception to leave the controller in an inconsistent state.

**4. Default `BoundaryErrorSink` is log-only (Source: #697).** `EfcFormController.cs:128`: `internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } = (message, exception) => logger.Error(message, exception);` — the default fault boundary for this controller only logs; it never surfaces a diagnostic to the user, even though it's invoked from four call sites (lines 456, 473, 491, 553) guarding real user-facing operations.

**5. Five unguarded `_globals.Ol.ArchiveRootPath` reads (Source: #698).** `EfcFormController.cs:529, 539, 836, 846, 987` all read `_globals.Ol.ArchiveRootPath` directly with no guard — every one of these is a call site that inherits finding 1's unguarded-COM-read risk without adding any handling of its own.

**6. Test uses a crash as its success-path barrier (Source: #699).** `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:182` asserts `await act.Should().ThrowAsync<NullReferenceException>()` as the test's pass condition — using an unhandled-exception crash as the expected/passing outcome, rather than testing for a handled, diagnosable failure mode. This test would need to change once findings 1 and 5 are fixed with proper guards (the `NullReferenceException` it currently expects should no longer occur).

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations inline above; each finding was verified directly against `origin/main` during this consolidation pass (2026-09-02).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: an unguarded COM read reachable from six call sites (finding 1 plus its five downstream readers, finding 5) combined with an unhandled keyboard-dispatch path (finding 2) means a transient Outlook COM hiccup during routine archive-root resolution can crash the EFC form outright, with no diagnosable trace beyond a log line (finding 4) — and the one test that exercises this path treats the crash as correct behavior (finding 6) rather than catching the regression.

## Suspected Cause / Notes

All six findings share one root pattern: the EFC controller boundary has a designed error-sink mechanism (`BoundaryErrorSink`) and a designed archive-root validation guard (`ArchiveRootPathGuard.RequireResolvedArchiveRoot`), but several call sites bypass both — either by reading before the guard runs (findings 1, 5) or by not routing through any sink at all (finding 2) — and the one place that should surface a failure well (finding 4) only logs it.

## Proposed Fix / Validation Ideas

- [ ] Wrap the two COM reads in `ArchiveRootPath`'s getter in a try/catch that routes through the existing guard/sink pattern instead of throwing uncaught
- [ ] Add try/catch to both `KbdExecuteAsync` overloads, routing caught exceptions through `BoundaryErrorSink`
- [ ] Reorder `ActionOkAsync` so disposal happens before (or is guaranteed via `finally` regardless of) the form-hide step
- [ ] Give `BoundaryErrorSink`'s default implementation a user-facing surface (e.g. a non-blocking notification), not just a log call
- [ ] Route the five `_globals.Ol.ArchiveRootPath` reads through a guarded accessor once finding 1 is fixed
- [ ] Update `EfcDataModelArchiveRootTests.cs:182` to assert the new guarded/handled behavior instead of asserting a crash, once findings 1 and 5 land

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
