# efc-archive-root-getter-unguarded-against-com-failure (Issue #696)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-archive-root-getter-unguarded-against-com-failure/ (Issue #696)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #696
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/696
- Last Updated: 2026-08-29
## Summary

`IOlObjects.ArchiveRootPath` makes two live Outlook COM reads before it can validate anything, so a
disconnected or restarting Outlook process raises `COMException` rather than the
`InvalidOperationException` that the archive-root validator raises. Callers that guard only the
validator's exception remain exposed to the COM failure mode.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Python version: not applicable; this is a .NET Framework 4.8.1 C# component
- Command/flags used: not reproducible from a command line; requires a live Outlook session
- Data source or fixture: a live Outlook profile whose store goes offline or whose RPC server becomes unavailable

## Steps to Reproduce

1. Open Outlook with the TaskMaster add-in loaded and an Explorer window active.
2. Put the mailbox store into a disconnected or restarting state so a COM read of `Root.FolderPath` fails.
3. Invoke any QuickFiler path that reads the archive root, for example a move or an open-folder action.

## Expected Behavior

A COM failure reading the archive root is handled at one place, at the `AppOlObjects` boundary, and
either surfaces as a redacted user-facing diagnostic in the same shape the validator failure already
uses, or propagates by an explicit and documented decision.

## Actual Behavior

`COMException` propagates from the getter. Issue 638 narrowed its new guard to
`InvalidOperationException` and added the regression test
`MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` pinning that propagation, so the
narrowing is a recorded decision rather than an oversight. The decision about the COM failure mode was
deferred to this follow-up.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not captured; the condition requires a live Outlook fault and no unit-testable seam exists at the getter today.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Verified citations, re-derived at 2026-08-29 against commit ecdb1c84:

- `TaskMaster/AppGlobals/AppOlObjects.cs:253-267` — the `ArchiveRootPath` getter.
- `TaskMaster/AppGlobals/AppOlObjects.cs:260` — `Path.Combine(Root.FolderPath, "Archive")`, a live COM read of `Root.FolderPath`.
- `TaskMaster/AppGlobals/AppOlObjects.cs:261` — `ArchiveRoot?.FolderPath`, a second live COM read.
- `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44` and `:56` — the only two throw sites, both raising `InvalidOperationException`.
- `QuickFiler/Controllers/EfcDataModel.cs:287` — the narrowed `catch (InvalidOperationException ex)` added by issue 638.

Deciding this at the `AppOlObjects` boundary rather than at each call site avoids duplicating the
choice across the eight archive-root call sites that exist across `EfcDataModel` and
`EfcFormController`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: an injectable seam over the two COM reads in the `ArchiveRootPath` getter, so a COM failure can be simulated without a live Outlook process.
- [ ] Integration scenario to retest: a move and an open-folder action while the store is disconnected.
- [ ] Manual verification notes: confirm no mailbox address or archive path appears in any resulting user-facing diagnostic.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Origin: deferred non-goal (a) of issue 638. Proposed labels: bug, quickfiler, outlook-interop, follow-up.
