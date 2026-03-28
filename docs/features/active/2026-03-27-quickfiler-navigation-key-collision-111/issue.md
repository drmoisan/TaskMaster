# quickfiler-navigation-key-collision (Issue #111)

- Date captured: 2026-03-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-navigation-key-collision/ (Issue #111)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #111
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/111
- Last Updated: 2026-03-27
- Work Mode: minor-audit

## Summary

QuickFiler can throw an unhandled `System.ArgumentException` while rebuilding keyboard navigation after collection items are removed or re-registered.
The failure occurs because `KbdActions` uses `KaStringAsync.KeyEquals` substring matching for stored-key identity, which can treat distinct keys such as `1`, `01`, and `10` as duplicates for the same `SourceId`.

## Environment

- OS/version: Windows
- Python version: Not applicable
- Command/flags used: QuickFiler keyboard navigation registration during collection refresh; local MSTest regression plus repository C# QA loop
- Data source or fixture: `QuickFiler.Controllers.KbdActions<string, KaStringAsync, Func<string, Task>>`

## Steps to Reproduce

1. Create a `KbdActions<string, KaStringAsync, Func<string, Task>>` instance.
2. Register a navigation action for source `Collection` with stored key `10`.
3. Register another navigation action for the same source with stored key `1`.

Equivalent production flow: `QfcCollectionController.RegisterNavigation()` re-registers collection navigation actions after item removal and eventually calls `KbdActions.Add` through `RegisterNavigationAsyncAction`.

## Expected Behavior

Distinct stored navigation keys for the same source should coexist when their literal keys differ.
Only an exact duplicate stored key for the same `SourceId` should be rejected.

## Actual Behavior

QuickFiler throws an unhandled exception during navigation rebuild:

- `System.ArgumentException: Cannot add key because it already exists. Key 1 SourceId Collection`
- Stack origin includes `QuickFiler.Controllers.KbdActions.Add`, `QfcCollectionController.RegisterNavigationAsyncAction`, `QfcCollectionController.RegisterNavigation`, and `QfcCollectionController.RemovedItemMonitor`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet:

	`System.ArgumentException: Cannot add key because it already exists. Key 1 SourceId Collection`

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`KaStringAsync.KeyEquals` uses substring matching via `Key.Contains(other)` to support live keyboard filtering.
That behavior is appropriate for runtime keyboard-input matching, but `KbdActions` originally reused it for stored-key identity checks in `Add` and `Remove`, allowing literal keys such as `1`, `01`, and `10` to collide.
Investigation confirmed that `QfcCollectionController.cs` does not need a compatibility change for this fix; the minimal production change is in `QuickFiler/Controllers/KbdActions.cs`.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas
- [x] Integration scenario to retest
- [x] Manual verification notes

- Add a regression test proving stored keys `10` and `1` can coexist for source `Collection`.
- Keep an exact-duplicate regression proving a second stored key `1` for the same source still throws `ArgumentException`.
- Preserve runtime keyboard matching semantics by keeping substring-based `KeyEquals` behavior available for `ContainsKey` and `FilterKeys`.

## Acceptance Criteria

- [x] `KbdActions<string, KaStringAsync, Func<string, Task>>` no longer treats distinct stored keys `1`, `01`, and `10` as duplicates for the same `SourceId` during storage operations.
- [x] An exact duplicate stored key for the same `SourceId` still throws `ArgumentException`.
- [x] Runtime keyboard-input matching semantics based on `KaStringAsync.KeyEquals` remain available for filtering and lookup behavior.
- [x] The repository C# QA loop passes for the change: format, analyzer build, nullable/type-check build, and MSTest with coverage.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch