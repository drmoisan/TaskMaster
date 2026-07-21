# `swordfish-raw-usage-cleanup` — User Story

- Issue: #310
- Owner: drmoisan
- Status: Ready for planning
- Last Updated: 2026-07-10T20-30

## Story Statement

- As a maintainer of TaskMaster, I want the remaining first-party raw usages of `Swordfish.NET.*`
  types and stale Swordfish-referencing artifacts removed from `KbdActions.cs`,
  `KeyboardHandler.cs`, `FlagDetails.cs`, `FolderRemapController.cs`, and `TraceUtility.cs`, so that
  the source-file surface referencing `Swordfish.NET.*` shrinks to only the Sco* lineage classes
  (owned by epic children F1–F3), which is a precondition for epic child F5 to remove the
  `UtilitiesSwordfish` project's `ProjectReference` entries and solution entries.
- As a maintainer reviewing this change, I want the collection-type swap in `KbdActions.cs` to be
  provably behavior-neutral, so that I can approve the change without re-validating the keyboard
  action dispatch behavior it supports.

## Problem / Why

The `swordfish-removal` epic eliminates the vendored `UtilitiesSwordfish` project and every
first-party dependency on `Swordfish.NET.*`. Before the project itself can be removed (F5), every
first-party source file that references a Swordfish type must stop doing so. This feature (F4)
handles the raw-usage and stale-artifact dependencies that are not part of the Sco* lineage
migrations owned by F1, F2, and F3: a private collection field in `KbdActions`, three unused
`using` directives, and two dead trace-filter string literals.

## Personas & Scenarios

- Persona: TaskMaster maintainer (Dan Moisan)
  - Owns the `swordfish-removal` epic and reviews each child's changes before merge to the
    integration branch.
  - Cares about the epic's cross-cutting acceptance criteria: a repo-wide `Swordfish` search
    eventually returning only archived docs/memory, and no behavior regression anywhere the
    migrated types are used.
  - Constraint: F4 must not expand into Sco* lineage work or project-reference/solution teardown —
    those are separately scoped children.
  - Goal: merge F4 with confidence that the swap is behavior-neutral and does not create an
    unplanned dependency on F2.
- Scenario: Reviewing the F4 change set
  - The maintainer opens the F4 branch diff and sees the `KbdActions._list` field re-typed from the
    raw Swordfish collection to `List<UClass>`, three `using` deletions, and two literal deletions
    in `TraceUtility.cs`.
  - The maintainer checks that the existing `KbdActions` regression tests pass unchanged, that the
    solution rebuilds clean, and that the spec's Research Resolution section explains why the swap
    target is `List<UClass>` rather than the epic manifest's named (but nonexistent) clean
    `ConcurrentObservableCollection`.
  - The maintainer either accepts the resolved decision or exercises the override path documented
    in the decision record, redirecting F4 to depend on F2.
  - Expected outcome: the maintainer approves the merge, and the epic manifest's inaccurate
    "Shared Design" note is corrected as a follow-up epic-level action.

## Acceptance Criteria

- [x] Done when `QuickFiler/Controllers/KbdActions.cs` `_list` is re-pointed to
  `System.Collections.Generic.List<UClass>`, the file no longer references
  `Swordfish.NET.Collections`, the public `KbdActions` API is unchanged, and all existing
  `KbdActions` tests pass. (AC1)
- [x] Done when `using Swordfish.NET.Collections;` is removed from `KeyboardHandler.cs`,
  `FlagDetails.cs`, and `FolderRemapController.cs`, and the solution rebuilds clean. (AC2)
- [x] Done when the stale `"UtilitiesSwordfish.NET.General"` / `"UtilitiesSwordfish.NET.Test"`
  literals in `TraceUtility.cs` are deleted, with the rationale recorded in the feature documents.
  (AC3)
- [x] Done when no Sco* lineage class or its consumers are modified beyond the `KbdActions` swap,
  and the `UtilitiesSwordfish` project, its `ProjectReference` entries, and `TaskMaster.sln` remain
  untouched. (AC4)
- [x] Done when the full C# toolchain (CSharpier, .NET analyzers, nullable, MSTest) passes, and
  changed/new code meets the repository coverage thresholds with no regression on changed lines.
  (AC5)

## Non-Goals

- No migration or modification of Sco* lineage classes (`ScoDictionary`, `ScoCollection`,
  `ScoStack`, `ScoSortedDictionary`) — those belong to epic children F1, F2, and F3.
- No removal of the `UtilitiesSwordfish` project, its `ProjectReference` entries, or edits to
  `TaskMaster.sln` — those belong to epic child F5.
- No new Swordfish-free general-purpose collection type is authored by this feature.
- No behavior or UX change beyond the internal, behavior-neutral collection-type swap.
