# quickfiler-controller-lifecycle-disposal-defects (Issue #731)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-controller-lifecycle-disposal-defects/ (Issue #731)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #731
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/731
- Last Updated: 2026-09-02
- Work Mode: full-bug

## Summary

Five consolidated findings from a blast-radius review of open bug reports, all clustered on QuickFiler's collection-controller/queue lifecycle and disposal surface. Consolidated into one issue rather than five because they share overlapping files and a fix for one is likely to touch the same region as another.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C#/.NET Framework 4.8.1 WinForms VSTO add-in
- Command/flags used: n/a — findings are from static code review
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable in the usual sense — each sub-finding below is a static code-review finding with its own reachability note.

## Expected Behavior

Each sub-finding's expected behavior is stated inline below.

## Actual Behavior

**1. `EmailMoveMonitor` is constructed three separate times instead of shared.** `QfcCollectionController.cs:83`, `QfcDatamodel.cs:103`, and `QfcQueue.cs:40` each declare `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` independently. Three collaborating types that operate on the same move-tracking concern hold three separate monitor instances rather than one shared one, which can let move-hook state diverge between them. Confirmed unchanged on `origin/main`. *(Source: #620.)*

**2. `QfcFormController.SetupDisposal.cs` `Cleanup()` doesn't cancel or await `_undoConsumerTask`.** `Cleanup()` (line ~210) disposes `_undoQueue` and invokes `_parentCleanup`, but never touches `_undoConsumerTask` — the background undo consumer can keep running against a disposed queue after cleanup returns. Confirmed unchanged on `origin/main`. *(Source: #621.)*

**3. `QfcRemainingQueueAdmission.cs` has a dead `scoreLoader` field.** The constructor null-checks its `scoreLoader` parameter (`if (scoreLoader is null) throw new ArgumentNullException(...)`) but the parameter is never assigned to a field or invoked anywhere else in the file — confirmed by an exact 3-occurrence count (declaration, guard check, throw) with no fourth use. Whatever this parameter was meant to wire up is currently inert. *(Source: #622.)*

**4. `QfcCollectionController.cs`'s reentrancy counter has an unsynchronized guard read.** `removespecificcontrolgroupcounter` (line ~909) is a plain `static int`. Writes go through `Interlocked.Increment`/`Decrement` (lines ~913, ~1008), but the guard read at line ~991 (`if (removespecificcontrolgroupcounter > 1)`) is a bare unsynchronized read of a non-`volatile` field — on some JIT/architecture combinations this can observe a stale cached value rather than the true current count. Confirmed unchanged on `origin/main`. *(Source: #634.)*

**5. `QfcFormController.SetupDisposal.cs` coverage debt.** Same file as finding 2 — flagged separately as under-covered. No `docs/features/active/*-683` folder exists yet; only a promoted potential record. Whatever fix lands for finding 2 should be expected to move this file's coverage, so it's worth re-measuring together rather than as an independent pass. *(Source: #683.)*

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations inline above, each independently re-verified against `origin/main` before this consolidation.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: none of the five is confirmed live under current production call patterns, but finding 2 (uncancelled undo-consumer task) and finding 4 (unsynchronized reentrancy read) are both real correctness gaps one caller change away from live, and finding 1 (unshared monitor) is an architectural inconsistency worth closing before it masks a real bug.

## Suspected Cause / Notes

Each finding traces to a specific original issue, cited inline above. All five were independently re-verified against current `origin/main` (not just carried over from the original reports) as part of this consolidation pass on 2026-09-02.

## Proposed Fix / Validation Ideas

- [ ] Share one `IEmailMoveMonitor` instance across `QfcCollectionController`, `QfcDatamodel`, and `QfcQueue` (constructor injection or a shared owner), or document explicitly why three instances are intentional
- [ ] `QfcFormController.SetupDisposal.Cleanup()`: cancel and await `_undoConsumerTask` before/alongside disposing `_undoQueue`
- [ ] `QfcRemainingQueueAdmission`: either wire `scoreLoader` into real use or remove the dead parameter and its guard
- [ ] Mark `removespecificcontrolgroupcounter` `volatile`, or replace the plain read at line ~991 with an `Interlocked.CompareExchange`/`Volatile.Read`
- [ ] Re-measure `QfcFormController.SetupDisposal.cs` coverage once the `Cleanup()` fix lands

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
