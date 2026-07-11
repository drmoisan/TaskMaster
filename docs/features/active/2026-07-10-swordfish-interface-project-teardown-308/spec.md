# swordfish-interface-project-teardown — Spec

- **Issue:** #308
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/308
- **Epic:** swordfish-removal (child feature F5, wave 1; depends on F1-F4)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-10
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature
- **Authoritative research:** `research/2026-07-10T20-45-swordfish-teardown-research.md`

## Overview

The vendored `UtilitiesSwordfish` project (`UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`,
assembly/root namespace `Swordfish.NET.General`) and its test project `UtilitiesSwordfish.Test`
(`UtilitiesSwordfish.NET.Test.csproj`) are unmaintained third-party code carried in the solution.
Sibling features F1-F4 of the `swordfish-removal` epic eliminate every first-party *type*
dependency on `Swordfish.NET.*` (dictionary lineage, collection/stack lineage, `ScoSortedDictionary`
removal, and raw-usage / unused-using cleanup). Once those have landed, the only remaining Swordfish
surface is structural: two Swordfish-dependent interfaces plus one dead interface, nine
`ProjectReference` entries, two `TaskMaster.sln` project entries (declarations and configuration
rows), two project folders, and three tests that instantiate Swordfish implementations directly.

This teardown feature removes that structural surface so that a repo-wide search for "Swordfish"
over `*.cs`, `*.csproj`, and `*.sln` returns only archived docs/memory. F5 is the feature at which
the epic completes.

## Scope

F5 owns exactly the following work items. It does not re-do any F1-F4 consumer migration.

### WI-0 — Preflight precondition (assert F1-F4 have landed)

Before any teardown edit, assert the upstream features have merged:

- Repo-wide `Swordfish` references in production `*.cs` (research category A) are zero.
- `UtilitiesCS\HelperClasses\Logging\TraceUtility.cs` no longer contains the
  `UtilitiesSwordfish.NET.General` / `UtilitiesSwordfish.NET.Test` string literals (F4-owned).

If either assertion fails, halt: the upstream feature has not landed and F5 must not absorb
upstream work.

### WI-1 — Interfaces

- Remove `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs` (`IScoCollection<T>`).
- Remove `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection2.cs` (`IScoCollection2<T>`).
- Remove `UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs` (dead `ISubjectMapSco`).
- Remove the dead `UpdateForMove` method from `QuickFiler\Controllers\QfcExplorerController.cs`
  (the sole reference to `ISubjectMapSco`; it has no call site).

Disposition decision is REMOVE outright, not migrate. `IScoCollection2<T>` has zero
implementers and zero consumers. `IScoCollection<T>` is consumed only by the vestigial
`ISubjectMapSco`, which has no implementer (the concrete `SubjectMapSco` does not declare it) and
whose only reference is the never-called `UpdateForMove` method. The clean collection stack exposes
no `IConcurrentObservableBase<T>` / `IConcurrentObservableCollection<T>` equivalent, so migrating
would author new interfaces that no surviving type consumes, which violates the repository
"Simplicity first" design principle.

### WI-2 — Project references

Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from NINE csprojs:

1. `UtilitiesCS\UtilitiesCS.csproj`
2. `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
3. `TaskMaster\TaskMaster.csproj`
4. `TaskMaster.Test\TaskMaster.Test.csproj`
5. `QuickFiler\QuickFiler.csproj`
6. `ToDoModel\ToDoModel.csproj`
7. `Tags\Tags.csproj`
8. `TaskVisualization\TaskVisualization.csproj`
9. `TaskVisualization.Test\TaskVisualization.Test.csproj`

The `Tags`, `TaskVisualization`, and `TaskVisualization.Test` references are stale: research
confirmed no `Swordfish.NET.*` / `Sco*` / `IScoCollection` / `ISubjectMapSco` / `ConcurrentObservable`
type appears in their source. The ninth reference (`TaskVisualization.Test.csproj`) is absent from
the epic/issue "eight" scope list; leaving it in place after the project folder is deleted yields a
dangling reference and a broken `TaskVisualization.Test` build, so F5 must remove it.

### WI-3 — Solution entries and project folders

- Remove from `TaskMaster.sln` the `Project(...)`/`EndProject` declarations for both GUIDs:
  - `{F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF}` (`UtilitiesSwordfish.NET.General`).
  - `{9A04D222-2B52-4E93-9B92-CC6EF54D5848}` (`UtilitiesSwordfish.NET.Test`).
- Remove from `TaskMaster.sln` the `GlobalSection(ProjectConfigurationPlatforms)` entries for both
  GUIDs (leaving them orphaned is tolerated by Visual Studio but is not a clean teardown).
- Delete the `UtilitiesSwordfish\` and `UtilitiesSwordfish.Test\` project folders via `git rm -r`
  (removes both csprojs, all vendored `*.cs`, the XAML, and the nested `UtilitiesSwordfish\Swordfish.NET.sln`).

### WI-4 — Tests

Remove the three direct-Swordfish test files, whose subject types are deleted wholesale with the
`UtilitiesSwordfish` assembly:

- `UtilitiesCS.Test\ReusableTypeClasses\ObservableDictionary_Tests.cs`
- `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionSenderTests.cs`
- `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs`

The sender-identity and lock-recursion regression intent of the latter two belongs to the clean
collection base that F2 re-bases `SubjectMapSco` onto. F5 verifies at execution that F2 carries
equivalent regression coverage against the clean base; if that coverage is absent, F5 raises a new
issue rather than authoring tests against a clean type it does not own.

## Out of Scope

The following are explicitly excluded from F5:

- **F1-F4 consumer migrations.** Re-pointing production consumers off `Swordfish.NET.*` types
  (dictionary lineage, collection/stack lineage, `ScoSortedDictionary` deletion, raw-usage and
  unused-using cleanup, `TraceUtility` string-literal updates) is owned by F1-F4. F5 verifies these
  have landed (WI-0) but does not perform them.
- **Authoring new sorted / clean-collection tests.** F5 does not write sender-identity,
  lock-recursion, or any other regression test against the clean collection base; that coverage is
  F2-owned. F5 only flags the verification.
- **Any Swordfish-free replacement interface.** F5 does not author a Swordfish-free equivalent of
  `IScoCollection<T>`, `IScoCollection2<T>`, `IConcurrentObservableBase<T>`, or
  `IConcurrentObservableCollection<T>`. The disposition is removal, not migration.
- **The ten `Sco*` legacy test files** (e.g. `ScoCollection_Tests.cs`, `SubjectMapSco_Tests.cs`).
  None reference `Swordfish` textually; they migrate with their production type under F1/F2/F3 and
  are out of F5 scope. F5 verifies no residual `Sco*` legacy test remains against a deleted type.

## Constraints & Risks

- **Build-order dependency on F1-F4.** F5 cannot remove the `ProjectReference` entries or delete the
  project folders until no first-party source references a `Swordfish.NET.*` type. The WI-0 preflight
  is the gate: if research category-A production references are non-zero, F5 halts.
- **Stale-reference confirmation.** The `Tags`, `TaskVisualization`, and `TaskVisualization.Test`
  references must be confirmed unused via an auditable search before removal. Research confirmed all
  three are stale; execution re-runs the search as evidence.
- **Solution loadability.** Removing the `Project(...)` declarations without also removing the
  `GlobalSection(ProjectConfigurationPlatforms)` rows leaves orphaned configuration entries.
  `TaskMaster.sln` has no `GlobalSection(NestedProjects)`, so there are no solution-folder nesting
  rows to update.
- **No binding redirects / props / targets.** UtilitiesSwordfish is a project reference, not a NuGet
  package; research confirmed no `app.config` `bindingRedirect`, no `packages.config` entry, and no
  `*.props`/`*.targets` reference to clean up.

## Toolchain and Coverage Policy

The full C# toolchain runs in this exact order after the teardown edits and repeats until a single
clean pass:

1. **Format:** `dotnet tool run csharpier .` (or `csharpier .`)
2. **Analyzers:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Nullable / type-check:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. **Test:** `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

If any step fails or auto-fixes files, restart from step 1.

Coverage policy (CLAUDE.md, this worktree): repo-wide line coverage `>= 80%` on the testable
denominator; changed/new code `>= 90%`; MSTest + Moq + FluentAssertions. Removing the three
direct-Swordfish tests does not reduce coverage of surviving first-party code, because their
subject types are deleted wholesale with the `UtilitiesSwordfish` assembly (both numerator and
denominator lose those lines). F5 owes no coverage backfill for these removals; regression coverage
of the clean collection base is F2's responsibility.

## Acceptance Criteria

Each item below is individually verifiable and maps 1:1 to a work item (WI-0..WI-4) or a
cross-cutting gate. All remain unchecked until the corresponding work is delivered and verified;
this document is authored in planning mode and no criterion is checked off here.

### WI-0 — Preflight precondition

- [ ] AC-1: Preflight asserts zero `Swordfish` references in production `*.cs` (research category A);
      if non-zero, F5 halts as an upstream (F1-F4) defect.
- [ ] AC-2: Preflight asserts `UtilitiesCS\HelperClasses\Logging\TraceUtility.cs` contains no
      `UtilitiesSwordfish.NET.General` / `UtilitiesSwordfish.NET.Test` string literal.

### WI-1 — Interfaces

- [ ] AC-3: `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs` is removed.
- [ ] AC-4: `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection2.cs` is removed.
- [ ] AC-5: `UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs` is removed.
- [ ] AC-6: The dead `UpdateForMove` method (the sole `ISubjectMapSco` reference) is removed from
      `QuickFiler\Controllers\QfcExplorerController.cs`, leaving no dangling symbol.

### WI-2 — Project references

- [ ] AC-7: All NINE `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` entries are removed
      (`UtilitiesCS`, `UtilitiesCS.Test`, `TaskMaster`, `TaskMaster.Test`, `QuickFiler`, `ToDoModel`,
      `Tags`, `TaskVisualization`, `TaskVisualization.Test`), with an auditable search confirming the
      `Tags` / `TaskVisualization` / `TaskVisualization.Test` references were stale before removal.

### WI-3 — Solution entries and project folders

- [ ] AC-8: The `TaskMaster.sln` `Project(...)`/`EndProject` declarations for
      `{F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF}` and `{9A04D222-2B52-4E93-9B92-CC6EF54D5848}` are removed.
- [ ] AC-9: The `TaskMaster.sln` `GlobalSection(ProjectConfigurationPlatforms)` entries for both
      GUIDs are removed (no orphaned configuration rows remain).
- [ ] AC-10: The `UtilitiesSwordfish\` and `UtilitiesSwordfish.Test\` project folders are deleted
      (`git rm -r`), including both csprojs, all vendored `*.cs`, the XAML, and the nested
      `UtilitiesSwordfish\Swordfish.NET.sln`.

### WI-4 — Tests

- [ ] AC-11: The three direct-Swordfish test files are removed
      (`ObservableDictionary_Tests.cs`, `ConcurrentObservableCollectionSenderTests.cs`,
      `ConcurrentObservableCollectionLockRecursionTests.cs`).
- [ ] AC-12: F2 is confirmed to carry sender-identity and lock-recursion regression coverage against
      the clean collection base; if absent, a new issue is raised (F5 does not author the coverage).

### Cross-cutting (epic completes here)

- [ ] AC-13: A repo-wide `Swordfish` search over `*.cs`, `*.csproj`, and `*.sln` returns only
      archived docs/memory (research categories A-E are zero).
- [ ] AC-14: The solution builds green with both `UtilitiesSwordfish` and `UtilitiesSwordfish.Test`
      removed and no unresolved type reference.
- [ ] AC-15: The full C# toolchain passes in order (csharpier -> analyzers -> nullable -> MSTest)
      with all four steps green in a single final pass.
- [ ] AC-16: Coverage thresholds hold for changed/new code (repo-wide line coverage `>= 80%` on the
      testable denominator; changed/new code `>= 90%`), with no coverage regression on surviving
      first-party code attributable to the removed tests.

## Verification Approach (per criterion)

Commands below are the exact search/build/test steps used to verify each AC. Grep patterns follow
ripgrep syntax; `Swordfish` searches exclude Markdown and `.claude/agent-memory/**`.

- **AC-1:** `rg "Swordfish" -g "*.cs"` scoped to production source; confirm zero matches outside
  F5's own interface/test targets (categories B/C). Any category-A match halts F5.
- **AC-2:** `rg "UtilitiesSwordfish\.NET\.(General|Test)" UtilitiesCS\HelperClasses\Logging\TraceUtility.cs`
  returns no matches.
- **AC-3 / AC-4 / AC-5:** confirm each interface file no longer exists (`git status` shows deletion);
  `rg "IScoCollection2?\b|ISubjectMapSco" -g "*.cs"` returns zero matches repo-wide.
- **AC-6:** `rg "UpdateForMove" QuickFiler` returns no match in `QfcExplorerController.cs`; solution
  compiles with no unresolved `ISubjectMapSco` symbol.
- **AC-7:** `rg "UtilitiesSwordfish\.NET\.General\.csproj" -g "*.csproj"` returns zero matches;
  before removal, `rg "Sco|IScoCollection|ISubjectMapSco|Swordfish|ConcurrentObservable" -g "*.cs"`
  under `Tags\`, `TaskVisualization\`, and `TaskVisualization.Test\` returns zero matches (stale-reference evidence).
- **AC-8 / AC-9:** `rg "F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF|9A04D222-2B52-4E93-9B92-CC6EF54D5848" TaskMaster.sln`
  returns zero matches (no declaration and no configuration row for either GUID).
- **AC-10:** `git status` confirms both project folders are deleted; the directories no longer exist
  on disk.
- **AC-11:** `git status` confirms the three test files are deleted; `rg "using Swordfish" -g "*.cs"`
  under `UtilitiesCS.Test\` returns zero matches.
- **AC-12:** inspect F2 deliverables for equivalent sender-identity / lock-recursion coverage against
  the clean collection base; record the finding, and open a new issue if the coverage is missing.
- **AC-13:** `rg "Swordfish" -g "*.cs" -g "*.csproj" -g "*.sln"` returns only archived docs/memory
  (matches, if any, are limited to Markdown and `.claude/agent-memory/**`, which are excluded from the
  code globs above; the code-glob search returns zero).
- **AC-14 / AC-15:** run the four toolchain steps in order (csharpier -> analyzers msbuild ->
  nullable msbuild -> `vstest.console.exe /EnableCodeCoverage`); confirm the solution builds and all
  tests pass with both UtilitiesSwordfish projects removed. Restart from step 1 on any failure or
  auto-fix.
- **AC-16:** inspect the `vstest.console.exe /EnableCodeCoverage` output; confirm repo-wide line
  coverage `>= 80%` on the testable denominator and `>= 90%` for changed/new code, with no regression
  on surviving first-party lines.

## Definition of Done

- [ ] All acceptance criteria (AC-1 through AC-16) verified and checked off.
- [ ] Full C# toolchain pass completed (csharpier -> analyzers -> nullable -> MSTest) with a single
      clean final pass.
- [ ] Repo-wide `Swordfish` search over `*.cs`/`*.csproj`/`*.sln` returns only archived docs/memory.
- [ ] Evidence artifacts (preflight search output, stale-reference search output, toolchain logs,
      coverage report) written to the canonical `<FEATURE>/evidence/<kind>/` locations.
- [ ] Supporting documents (this spec, `user-story.md`, epic manifest status) updated to reflect
      delivery.
