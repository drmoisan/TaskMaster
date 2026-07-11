# `swordfish-scosorteddictionary-removal` — User Story

- Issue: #309
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-10T21-10

## Story Statement

- As a maintainer of `UtilitiesCS`, I want the unused, vendored `ScoSortedDictionary` class and
  its dedicated test removed, so that the analyzer-exempt Swordfish-dependent surface shrinks and
  the `UtilitiesSwordfish` project can eventually be torn down (epic child F5) without carrying
  dead code forward.
- As a future contributor evaluating the swordfish-removal epic's progress, I want confirmation
  that `ScoSortedDictionary` has no production consumer and has been deleted, so that I do not
  have to re-audit it before proceeding with the remaining Swordfish-dependent migrations (F1,
  F2, F4).

## Problem / Why

The vendored Swordfish-based `ScoSortedDictionary<TKey,TValue>`
(`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`) derives
from the Swordfish `ConcurrentObservableSortedDictionary` and is believed to have no production
consumer — only its own definition and its test
(`UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`). It is one of the
first-party dependencies on `Swordfish.NET.*` that the swordfish-removal epic must eliminate so
the vendored `UtilitiesSwordfish` project can be torn down (epic child F5). Removing the unused
class shrinks the analyzer-exempt vendored surface and unblocks the No-COM/testability direction.

## Personas & Scenarios

- Persona: TaskMaster maintainer (repository owner responsible for the swordfish-removal epic)
  - Who they are: a maintainer working through the swordfish-removal epic's five children (F1–F5)
    to eliminate first-party dependencies on the vendored `Swordfish.NET.*` package.
  - What they care about: reducing the analyzer-exempt vendored surface, keeping the build green
    at every step, and not introducing behavior changes to unrelated types while working through
    the epic.
  - Their constraints: the epic requires F1–F4 to complete (in any order, since they touch
    disjoint source trees) before F5 can remove the `UtilitiesSwordfish` project reference and
    solution entries; each child must leave the solution buildable and fully tested on its own.
  - Their goals and frustrations: the goal is to retire dead, unmaintained vendored code with
    minimal risk; the frustration this feature avoids is discovering, mid-epic or during F5's
    teardown, that a class believed unused actually had a hidden consumer that breaks the build.
  - Their context and motivations: `ScoSortedDictionary` is the smallest and least risky of the
    five children (research confirms zero production consumers), so completing it early builds
    confidence in the epic's audit methodology for the remaining, higher-complexity children.

- Scenario: Maintainer removes the unused sorted-dictionary class
  - Who is acting: the maintainer (or an executing agent under the maintainer's direction) working
    the F3 child of the swordfish-removal epic.
  - What triggered the action: the epic's research phase produced a GO recommendation confirming
    no production consumer of `ScoSortedDictionary` or `ConcurrentObservableSortedDictionary`
    exists in first-party source, csproj files, or persisted JSON.
  - What steps they take: (1) re-verify the repo-wide search still shows zero production
    consumers immediately before deleting; (2) delete `ScoSortedDictionary.cs` and
    `ScoSortedDictionary_Tests.cs`; (3) remove the matching `<Compile Include>` entries from
    `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` in the same change; (4) run the full C#
    toolchain (csharpier, analyzers, nullable/type-check build, MSTest) and confirm every stage
    passes.
  - What obstacles or decisions occur: if the re-verification in step 1 turns up an unexpected
    consumer, the maintainer halts and reports it as a blocking finding rather than deleting; if
    the classic-csproj `<Compile Include>` entries are removed in a separate change from the file
    deletions, the build breaks with a missing-source-file error.
  - What outcome they expect: the solution builds cleanly, the full MSTest suite passes with no
    unrelated coverage regression, and the swordfish-removal epic can proceed to its remaining
    children (F1, F2, F4) and eventually F5's project teardown, unblocked by this class.

## Acceptance Criteria

- [x] An auditable repo-wide search confirms no production consumer of `ScoSortedDictionary` or `ConcurrentObservableSortedDictionary`, with scope, patterns, and results recorded
- [x] `ScoSortedDictionary.cs` is deleted
- [x] `ScoSortedDictionary_Tests.cs` is deleted
- [x] The matching `<Compile Include>` entry for `ScoSortedDictionary.cs` is removed from `UtilitiesCS.csproj`
- [x] The matching `<Compile Include>` entry for `ScoSortedDictionary_Tests.cs` is removed from `UtilitiesCS.Test.csproj`
- [x] The solution builds and all tests pass after removal (full C# toolchain green: csharpier, analyzers, nullable/type-check, MSTest)
- [x] No behavior or API change occurs to any other type
- [x] No `ProjectReference` or `TaskMaster.sln` change is made

## Non-Goals

- No new Swordfish-free sorted dictionary implementation. If a sorted, observable,
  serializable collection is wanted for future use, it is scoped separately: it cannot simply
  inherit `ScoDictionaryNew` (hash-based, no ordering guarantee) and would require a new clean
  sorted base or a sort-maintaining decorator. No current consumer has been identified that
  requires this.
- No deletion of the `UtilitiesSwordfish` project or its test project (`UtilitiesSwordfish.Test`).
  Both remain until epic child F5, which depends on F1–F4 all completing first.
- No removal of any `ProjectReference` to `UtilitiesSwordfish.NET.General.csproj` from any
  consuming csproj. That removal is scoped to F5.
- No change to `TaskMaster.sln` (project entries, solution structure). Solution-level teardown is
  scoped to F5.
- No migration of `IScoCollection` / `IScoCollection2` or any other Swordfish-dependent interface.
  Interface migration is scoped to F5.
- No changes to the F1 (`ScoDictionary` -> `ScoDictionaryNew` dictionary lineage), F2
  (`ScoCollection`/`ScoStack` collection and stack lineage), or F4 (`KbdActions` raw-usage and
  unused-`using` cleanup) types or workstreams. Each is a separate epic child with its own feature
  folder.
