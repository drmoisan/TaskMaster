# qfc-collection-background-task-and-catch-defects (Issue #473)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collection-background-task-and-catch-defects/ (Issue #473)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #473
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/473
- Last Updated: 2026-08-08
## Summary

Two concurrency and error-handling defects in `QfcCollectionController`: replacing the
`BackgroundLoadingTasks` bag reference after awaiting it discards any task added in the interim, and
`TryMoveEmailByGroupAsync` produces two misleading error logs for one failure while also swallowing
cancellation.

## Environment

- OS/version: n/a (logic defects, reproducible wherever QuickFiler runs)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

**Defect 1 — `BackgroundLoadingTasks` reset race (`:398-399`, `:492-493`)**

1. `internal ConcurrentBag<Task> BackgroundLoadingTasks = [];` is declared at `:80`.
2. Both `:398-399` and `:492-493` perform the sequence
   `await Task.WhenAll(BackgroundLoadingTasks); BackgroundLoadingTasks = [];`
3. That assignment replaces the bag **reference**, it does not clear the existing bag.
4. Any `Add` performed by a concurrently running load between the `WhenAll` completing and the
   assignment executing lands in the old bag.
5. The old bag is then dropped, so that task is never awaited and its completion is never observed.
   An exception it raises becomes an unobserved task exception.

**Defect 2 — `TryMoveEmailByGroupAsync` double-log and swallowed cancellation (`:2236-2258`)**

1. `TryGetItemGroupByIndex` at `:2260-2270` can return `null`.
2. That null reaches `TryMoveEmailByGroupAsync`, where line 2240 dereferences it and throws a
   `NullReferenceException`.
3. The broad `catch (System.Exception)` at `:2242` catches it and logs an error.
4. Execution continues to line 2247, which dereferences the same null and throws again.
5. The second broad catch at `:2249` logs a second error.
6. One root cause therefore produces two misleading log entries and no clear failure signal.
7. Both broad catches also swallow `OperationCanceledException`, so a cancelled move is reported as
   an error rather than as a cancellation.

## Expected Behavior

1. Tasks added to the background-loading set should always be awaited; the set should be cleared in a
   way that cannot drop a concurrently added task, or the add/await boundary should be synchronized.
2. A single root failure should produce a single, accurate log entry, and cancellation should
   propagate as cancellation rather than being caught as a generic error.

## Actual Behavior

1. A task added during the reset window is silently discarded and never awaited; any exception it
   raises is unobserved.
2. A null group produces two `NullReferenceException` log entries; a cancelled move is logged as an
   error.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at `QfcCollectionController.cs:80` (field declaration),
  `:398-399` and `:492-493` (the reset sequence), and `:2236-2258` (the two catch blocks).
  Discovered during preparation research for issue #454 (epic #136, child F11); full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  sections E16 and E17.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Defect 1's window is narrow and `BackgroundLoadingTasks` has no consumer outside this file, so a fix
is locally contained. Defect 2 is a diagnosability problem rather than a functional one, but it makes
genuine move failures harder to attribute and hides cancellation.

## Suspected Cause / Notes

Defect 1 is the common "reassign instead of clear" mistake with concurrent collections. Defect 2 is a
consequence of catching broadly and then continuing rather than returning; note that the repository
code-change policy explicitly discourages broad catches that do not re-raise or add context.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: assert a task added during the reset window is still awaited; assert a
      null group yields exactly one log entry; assert `OperationCanceledException` propagates.
- [x] Integration scenario to retest: cancel a move mid-flight and confirm it is reported as
      cancelled, not as an error.
- [x] Manual verification notes: prefer an early return after the first catch over allowing execution
      to fall through to the second dereference.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
