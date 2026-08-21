# qfc-collection-controller-unreachable-load-paths (Issue #468)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collection-controller-unreachable-load-paths/ (Issue #468)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #468
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/468
- Last Updated: 2026-08-08
## Summary

Twelve members of `QfcCollectionController`, totalling roughly 227 lines, have no caller anywhere in
the solution. They are dead code that inflates the coverage denominator and, in one case, hides a
real defect behind an unreachable entry point.

## Environment

- OS/version: n/a (dead-code finding, established by static reference search)
- Python version: n/a
- Command/flags used: repository-wide reference search across `QuickFiler`, `QuickFiler.Test`, and
  all other projects in `TaskMaster.sln`
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

1. Search the whole solution for callers of each of the following members of
   `QuickFiler/Controllers/QfcCollectionController.cs`:
   - `WireUpKeyboardHandler` (`:1254`)
   - `AnyOpenDropDownsAsync` (`:1324`)
   - `LoadGroups_02cAsync` (`:587`)
   - `LoadGroups_02bAsync` (`:635`)
   - `LoadGroup_03bAsync` (`:654`)
   - `LoadConversationsAndFoldersAsync` (`:761`)
   - `LoadItemGroup` (`:776`)
   - `LoadSequentialAsync` (`:827`)
   - `LoadGroupSequential` (`:842`)
   - `CacheTlpForMove` (`:865`)
   - `SwapTlp` (`:870`)
   - `CaptureTlpTemplate` (`:1991`)
2. Confirm no production or test caller exists for any of them.
3. Note `LoadGroups_02bAsync` is referenced only from a commented-out line at `:402`.
4. Note the field `_templateTlp` (`:70`) is written only by the dead `CaptureTlpTemplate`, so it is
   dead state as well.

## Expected Behavior

Production code should not retain unreachable members. Every member should either have a caller or be
removed, so that the coverage denominator reflects code that can actually run.

## Actual Behavior

Roughly 227 lines of unreachable code sit in the largest file in the repository. Under epic #136's
per-file 80% line-coverage target these lines must either be covered by tests that exercise code no
production path reaches, or be removed.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed by solution-wide reference search during preparation research for issue #454
  (epic #136, child F11); full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  section E1.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low functional risk, since none of it executes. The cost is real but indirect: a substantial share of
the file's coverage denominator is code that cannot be exercised through any production path, and one
of these members hides an active defect.

## Suspected Cause / Notes

The `Load*` cluster appears to be a superseded loading strategy left in place after the current async
load path was introduced. The commented-out reference at `:402` supports that reading.

Related finding: `WireUpKeyboardHandler` (`:1254`) is the entry point for the duplicate `KaKey`
registration recorded in issue #444. Because it has no caller, that defect is **dormant, not live** —
production wires keys through `WireUpAsyncKeyboardHandler` (`:1275-1280`) and `RegisterAsyncKeyActions`
(`:1282-1291`), which register `Keys.Up` and `Keys.Down` exactly once each. Removing this dead member
would resolve #444 as a side effect; the two should be scheduled together.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: none — the correct resolution is removal, not new tests.
- [x] Integration scenario to retest: exercise the full QuickFiler load, conversation-expansion, and
      move flows after removal to confirm no reflective or late-bound caller was missed.
- [x] Manual verification notes: several of these members are `public`, so removal is a public-API
      change. That is why it is deferred out of issue #454, whose epic carries a no-behavior-change
      constraint. Schedule alongside issue #444.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
