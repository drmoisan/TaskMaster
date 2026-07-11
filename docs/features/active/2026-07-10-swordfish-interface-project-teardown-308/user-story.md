# swordfish-interface-project-teardown — User Story

- **Issue:** #308
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/308
- **Epic:** swordfish-removal (child feature F5, wave 1; depends on F1-F4)
- **Owner:** drmoisan
- **Status:** Draft
- **Last Updated:** 2026-07-10
- **Work Mode:** full-feature
- **Authoritative research:** `research/2026-07-10T20-45-swordfish-teardown-research.md`

## Story Statement

- **As a** maintainer of the TaskMaster solution eliminating the vendored `UtilitiesSwordfish`
  dependency,
- **I want** the structural remnants of `Swordfish.NET.General` removed after F1-F4 have eliminated
  all first-party Swordfish *type* usage — the two Swordfish-dependent interfaces (plus the dead
  `ISubjectMapSco`), all nine `ProjectReference` entries, both `TaskMaster.sln` project entries, both
  project folders, and the three tests that instantiate Swordfish types directly,
- **So that** an unmaintained third-party collection library no longer ships in the build, a
  repo-wide search for "Swordfish" over `*.cs`/`*.csproj`/`*.sln` returns only archived docs/memory,
  and the epic completes with the solution building and testing green.

## Problem / Why

The vendored `UtilitiesSwordfish` project (`UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`,
assembly/root namespace `Swordfish.NET.General`) and its test project `UtilitiesSwordfish.Test`
are unmaintained third-party code carried in the solution. Sibling features F1-F4 of the
`swordfish-removal` epic eliminate every first-party *type* dependency on `Swordfish.NET.*`
(dictionary lineage, collection/stack lineage, `ScoSortedDictionary` removal, and raw-usage /
unused-using cleanup). Once those land, the remaining Swordfish surface is structural: interfaces,
project references, solution entries, project folders, and three direct-Swordfish tests. This
teardown feature removes that structural surface. It is the feature at which the epic completes.

## Persona & Scenario

- **Persona:** the solution maintainer.
  - Cares about a clean, buildable solution with no unmaintained vendored code and no analyzer-exempt
    surface that cannot be reasoned about.
  - Constrained by build-order: the teardown cannot proceed until F1-F4 have removed all first-party
    Swordfish type usage, or the removal breaks the build.
  - Goal: reach a state where a repo-wide "Swordfish" code search returns only archived docs/memory
    and the full C# toolchain passes green.

- **Scenario:** after F1-F4 have merged, the maintainer runs the F5 preflight. It confirms that
  production `*.cs` files carry no `Swordfish` reference and that `TraceUtility.cs` no longer holds
  the `UtilitiesSwordfish.NET.*` literals. With the precondition satisfied, the maintainer removes the
  two Swordfish-dependent interfaces and the dead `ISubjectMapSco` (and the never-called
  `UpdateForMove` method that referenced it), strips the nine `ProjectReference` entries, removes both
  `TaskMaster.sln` project declarations and their configuration rows, deletes the two project folders,
  and removes the three direct-Swordfish tests. The maintainer then runs the full toolchain in order,
  confirms the solution builds and tests green with both UtilitiesSwordfish projects gone, and
  verifies the repo-wide "Swordfish" code search returns only archived docs/memory. If the preflight
  had failed, the maintainer halts, because the upstream feature had not yet landed.

## Acceptance Criteria

These criteria are consistent with `spec.md` (identical AC-1..AC-16 mapping). All remain unchecked;
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

## Non-Goals

- **F1-F4 consumer migrations.** Re-pointing production consumers off `Swordfish.NET.*` types is
  owned by F1-F4. F5 verifies these have landed (WI-0) but does not perform them.
- **Authoring new sorted / clean-collection tests.** F5 does not write regression tests against the
  clean collection base; that coverage is F2-owned. F5 only flags the verification.
- **Any Swordfish-free replacement interface.** F5 does not author a Swordfish-free equivalent of the
  removed interfaces or their bases; the disposition is removal, not migration.
- **The ten `Sco*` legacy test files.** None reference `Swordfish` textually; they migrate with their
  production type under F1/F2/F3 and are out of F5 scope.
