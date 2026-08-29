# efcformcontroller-five-unguarded-archive-root-reads (Issue #698)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efcformcontroller-five-unguarded-archive-root-reads/ (Issue #698)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #698
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/698
- Last Updated: 2026-08-29
## Summary

`EfcFormController` reads `_globals.Ol.ArchiveRootPath` at five places, none of them guarded, so an
unresolvable or cross-store archive root still raises `InvalidOperationException` from each. Issue 638
guarded only the three reads in `EfcDataModel`.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Python version: not applicable; this is a .NET Framework 4.8.1 C# WinForms component
- Command/flags used: not reproducible from a command line; requires the Email Filer Combined form
- Data source or fixture: an Outlook profile whose archive root is unresolvable or resolves across stores

## Steps to Reproduce

1. Open Outlook with a profile whose archive root is unresolvable or resolves to a different store.
2. Open the Email Filer Combined form.
3. Exercise a path that reaches any of the five reads, including the breadcrumb bind path.

## Expected Behavior

Each read is routed through the same guarded helper shape issue 638 introduced, so an unresolvable or
cross-store archive root produces a redacted diagnostic or a defined no-op rather than an unhandled
exception.

## Actual Behavior

`InvalidOperationException` is raised at each unguarded read. Four of the five are absorbed by the
log-only boundary sinks reported separately, so they present as a silent no-op; the fifth sits on the
breadcrumb bind path.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not captured; four of the five failures are visible only in log4net output, which is itself the subject of the companion report.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Verified citations, re-derived at 2026-08-29 against commit ecdb1c84:

- `QuickFiler/Controllers/EfcFormController.cs:529` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:539` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:836` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:846` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:987` — `await _router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token);`
- `QuickFiler/Controllers/EfcDataModel.cs:280-297` — `TryGetArchiveRoot`, the helper shape issue 638 introduced and the one these five sites would adopt.

Issue 638 left these untouched because widening the change would have taken the diff outside the
footprint its AC18 pins, and because each site would have required a different test arrangement.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: route all five sites through a `TryGetArchiveRoot`-shaped helper, or through a shared helper promoted to a location both controllers can reach, and add the equivalent regression tests.
- [ ] Integration scenario to retest: the breadcrumb bind path under an unresolvable archive root.
- [ ] Manual verification notes: confirm no diagnostic text carries a mailbox address or the archive path.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Origin: deferred non-goal (c) of issue 638. Proposed labels: bug, quickfiler, outlook-interop, follow-up.
