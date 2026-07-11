# swordfish-interface-project-teardown (Issue #308)

- Date captured: 2026-07-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/swordfish-interface-project-teardown/ (Issue #308)
- Epic: swordfish-removal (child feature F5, wave 1)

- Issue: #308
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/308
- Last Updated: 2026-07-11
- Work Mode: full-feature

## Problem / Why

The vendored `UtilitiesSwordfish` project (`UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`,
assembly/root namespace `Swordfish.NET.General`) and its test project `UtilitiesSwordfish.Test`
are unmaintained third-party code carried in the solution. Child features F1-F4 of the
swordfish-removal epic eliminate every first-party *type* dependency on `Swordfish.NET.*`
(dictionary lineage, collection/stack lineage, `ScoSortedDictionary` removal, and raw-usage /
unused-using cleanup). Once those land, the remaining Swordfish surface is structural: two
Swordfish-dependent interfaces, eight `ProjectReference` entries, two solution project entries,
two project folders, and a small number of tests that exercise the Swordfish implementations
directly. This teardown feature removes that structural surface so a repo-wide search for
"Swordfish" returns only archived docs/memory.

## Proposed Behavior

Complete the removal of the `UtilitiesSwordfish.NET.General` project and its test project after
F1-F4 have eliminated all first-party Swordfish type usage:

- Migrate or remove `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs` (uses
  `Swordfish.NET.Collections`) and `IScoCollection2.cs` (uses `Swordfish.NET.General.Collections`).
- Remove the `ProjectReference` to `UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj` from
  `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, `TaskMaster.csproj`, `TaskMaster.Test.csproj`,
  `QuickFiler.csproj`, `ToDoModel.csproj`, `Tags.csproj`, and `TaskVisualization.csproj`.
- Remove the `UtilitiesSwordfish` (`{F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF}`) and
  `UtilitiesSwordfish.Test` project entries from `TaskMaster.sln`, and delete the two project
  folders.
- Migrate or remove tests referencing Swordfish types directly.

## Acceptance Criteria (early draft)

> Scope refined by research artifact `research/2026-07-10T20-45-swordfish-teardown-research.md`.
> The authoritative acceptance criteria for this full-feature work live in `spec.md` and
> `user-story.md`. This list is kept consistent with the research findings.

- [ ] Interface disposition applied: `IScoCollection.cs`, `IScoCollection2.cs`, and the dead
      `ISubjectMapSco.cs` removed outright, and the dead `QfcExplorerController.UpdateForMove`
      method (its sole `ISubjectMapSco` reference) removed.
- [ ] All NINE `ProjectReference` entries to `UtilitiesSwordfish.NET.General.csproj` removed
      (the eight named plus `TaskVisualization.Test.csproj`, which research confirmed also
      carries a stale reference).
- [ ] `UtilitiesSwordfish` and `UtilitiesSwordfish.Test` removed from `TaskMaster.sln`, including
      both the `Project(...)` declarations and the `GlobalSection(ProjectConfigurationPlatforms)`
      entries; both project folders deleted.
- [ ] The three direct-Swordfish tests removed (`ObservableDictionary_Tests.cs`,
      `ConcurrentObservableCollectionSenderTests.cs`, `ConcurrentObservableCollectionLockRecursionTests.cs`).
- [ ] Repo-wide `Swordfish` search over `*.cs`/`*.csproj`/`*.sln` returns only archived docs/memory
      (no first-party source or project reference).
- [ ] Solution builds and tests green with both UtilitiesSwordfish projects removed; full C#
      toolchain passes in order (csharpier -> analyzers -> nullable -> MSTest).

## Constraints & Risks

- This is the teardown feature; it assumes F1-F4 have already removed all first-party Swordfish
  TYPE usage. It must not re-do their consumer migrations.
- The Tags and TaskVisualization project references must be confirmed unused (no Swordfish type in
  their source) with an auditable search before removal.
- Removing solution entries and project folders must keep the solution loadable and buildable.

## Test Conditions to Consider

- [ ] Full-solution build after project-reference and solution-entry removal.
- [ ] MSTest run across affected test projects after test migration/removal.
- [ ] Repo-wide `Swordfish` grep returns only archived docs/memory.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/swordfish-interface-project-teardown/` folder from the template
