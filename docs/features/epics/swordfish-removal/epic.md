---
epic: swordfish-removal
integration_branch: epic/swordfish-removal-integration
created_at: 2026-07-10T20:05:00Z
intent:
  epic_type: enabler
  business_outcome_hypothesis: Eliminating the vendored Swordfish.NET.General project and every first-party dependency on Swordfish.NET.* removes an unmaintained third-party collection library from the build, shrinks the analyzer-exempt vendored surface, and unblocks the No-COM/testability direction by consolidating on the repository's Swordfish-free serializable collection bases.
  leading_indicators:
    - A repo-wide search for "Swordfish" returns only archived docs/memory (no first-party source or csproj reference).
    - The solution builds and tests green with UtilitiesSwordfish and UtilitiesSwordfish.Test removed from TaskMaster.sln.
    - Every persisted collection round-trips its existing on-disk JSON (MovedMails, Filters, PrefixList, SubjectMap encoder/decoder, CtfMap, FolderRemap/FilteredFolderScraping).
  nfrs:
    - On-disk serialization compatibility is preserved for every persisted collection; a migration or compatibility test is provided per persisted type.
    - No behavior regression in the QuickFiler undo flow or the SortEmail undo flow.
    - Full C# toolchain (csharpier, analyzers, nullable, MSTest) green for every child feature; changed/new code meets coverage thresholds.
features:
  - issue_num: 9001
    feature_folder: swordfish-dictionary-lineage
    depends_on: []
  - issue_num: 9002
    feature_folder: swordfish-collection-stack-lineage
    depends_on: []
  - issue_num: 309
    feature_folder: 2026-07-10-swordfish-scosorteddictionary-removal-309
    depends_on: []
  - issue_num: 310
    feature_folder: 2026-07-10-swordfish-raw-usage-cleanup-310
    depends_on: []
  - issue_num: 308
    feature_folder: 2026-07-10-swordfish-interface-project-teardown-308
    depends_on: [9001, 9002, 309, 310]
---

# Epic: Remove the Swordfish.NET.General Project from TaskMaster

- Integration branch: `epic/swordfish-removal-integration`
- Status: Planning phase IN PROGRESS — child preparation (research, spec, atomic plan,
  `PREFLIGHT: ALL CLEAR`) is delegated per child; execution awaits maintainer signal via
  `/epic-run swordfish-removal`.

> Note: `issue_num` values `9001`–`9005` and the `feature_folder` hints above are planning
> placeholders. They are back-filled with the real promoted GitHub issue numbers and active
> folder basenames as each child's preparation completes, before the kickoff artifact is
> written.

## Goal

Eliminate the vendored `UtilitiesSwordfish` project (physical file
`UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`, assembly/root namespace
`Swordfish.NET.General`) and its test project `UtilitiesSwordfish.Test`
(`UtilitiesSwordfish.NET.Test.csproj`) from the solution. Replace every first-party type that
inherits from or references a `Swordfish.NET.*` type with the repository's existing
Swordfish-free equivalents, migrate all affected tests, and remove the project references and
solution entries. On-disk serialization compatibility for every persisted collection must be
preserved.

## Shared Design — Swordfish-free replacement bases (already in the repo)

- **Hash dictionary:** `UtilitiesCS.ReusableTypeClasses.ScoDictionaryNew<TKey,TValue>`
  (derives from the clean `ConcurrentObservableDictionary`, `SmartSerializable`-based).
- **Collection:** ~~clean `ConcurrentObservableCollection` in
  `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`~~ — **CORRECTED (F4 preparation,
  issue #310):** verified false on the integration base. That namespace holds only
  `Bag`/`Dictionary`; the only `ConcurrentObservableCollection<T>` in the repo is the vendored
  Swordfish type being removed. F2 must CREATE the clean collection base it re-bases
  `ScoCollection`/`ScoStack` subclasses onto (or select an existing suitable base); it must not
  assume a pre-existing clean `ConcurrentObservableCollection`. Evidence:
  `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/research/swap-target-decision-record.md`.
- **Ordered/observable/serializable list:** `SloLinkedList<T>`
  (derives from `LockingObservableLinkedList<T>`, `SmartSerializable`-based).
- **Sorted dictionary:** NONE exists Swordfish-free. Do not assume one; deletion of the unused
  `ScoSortedDictionary` is the scoped action (see F3).

Sequence so that base-type replacements (F1, F2) precede consumer rewiring, and teardown (F5)
is last. Preparation for all five children is document-and-plan work only, so it runs
concurrently; the ordering constraint applies to execution waves.

## Decomposition and Waves

| Wave | issue_num (placeholder) | Feature folder (hint) | Complexity | Scope |
|---|---|---|---|---|
| 0 | 9001 | `swordfish-dictionary-lineage` | C3 | Dictionary lineage: re-point `ScoDictionary` consumers to `ScoDictionaryNew`; reconcile constructor/deserialize shape; preserve on-disk JSON. |
| 0 | 9002 | `swordfish-collection-stack-lineage` | C3 | Collection + Stack lineage: re-base `ScoCollection`/`ScoStack` subclasses onto the clean collection and `SloLinkedList`; add positional stack surface + async serialize; preserve `MovedMails` on-disk JSON. |
| 0 | 309 | `2026-07-10-swordfish-scosorteddictionary-removal-309` | C1 | Confirm no production consumer of `ScoSortedDictionary`, then delete the class and its test. |
| 0 | 310 | `2026-07-10-swordfish-raw-usage-cleanup-310` | C2 | Re-point `KbdActions` raw `ConcurrentObservableCollection` to `List<UClass>` (see decision record); remove unused `using Swordfish.NET.Collections;`; delete stale `TraceUtility` string literals. |
| 1 | 308 | `2026-07-10-swordfish-interface-project-teardown-308` | C3 | Remove `IScoCollection`/`IScoCollection2`/dead `ISubjectMapSco`; remove `ProjectReference` to `UtilitiesSwordfish.NET.General.csproj` from 9 csprojs (incl. stale `TaskVisualization.Test.csproj` reference found in research); remove project entries from `TaskMaster.sln`; delete project folders; migrate/remove tests referencing Swordfish types. |

Wave assignment is longest-path layering over the dependency DAG:
`wave(f) = 0` when `depends_on(f)` is empty, else `1 + max(wave(d))`. F1–F4 have no
dependencies (wave 0); F5 depends on all of F1–F4 (wave 1). The graph is acyclic and every
`depends_on` reference resolves.

### Dependency rationale

F5 (teardown) depends on F1, F2, F3, and F4 because the project reference to
`UtilitiesSwordfish.NET.General.csproj` and the `UtilitiesSwordfish` solution entries cannot be
removed until no first-party source file references a `Swordfish.NET.*` type. F5 also migrates
the Swordfish-dependent interfaces and the tests that exercise Swordfish types directly, both of
which follow the production migrations in F1/F2. F1, F2, F3, and F4 touch disjoint source trees
and carry no build-order constraint between them, so they run in parallel in wave 0.

## Workstream Detail

### F1 — Dictionary lineage (ScoDictionary -> ScoDictionaryNew)

Legacy type: `UtilitiesCS\...\SCO\SCODictionary.cs`
(`ScoDictionary<TKey,TValue> : ConcurrentObservableDictionary` [Swordfish]).
Production consumers to re-point to `ScoDictionaryNew<TKey,TValue>`:
`AppToDoObjects` (`_dictRemap`, `FilteredFolderScraping`, `FolderRemap`), `SubjectMapEncoder`
(`_encoder`, `_decoder`), `FolderScorer` (`_folderNameScores`), and the `IToDoObjects`
interface members `FilteredFolderScraping` and `FolderRemap`. Confirm `PeopleScoDictionary.cs`
is inert (commented-out reference). Reconcile the legacy `filename,folderpath` constructor
against `ScoDictionaryNew`'s `Static.Deserialize` / converter-based path, and preserve on-disk
JSON for each persisted dictionary.

### F2 — Collection + Stack lineage (ScoCollection/ScoStack)

Legacy types: `ScoCollection<T>` (`: ConcurrentObservableCollection<T>, IList<T>, IList`
[Swordfish]) and `ScoStack<T>` (`: ScoCollection<T>`). Re-base `CtfMap`, `RecentsList<T>`, and
`SubjectMapSco` onto the clean collection; re-point direct `ScoCollection<T>` consumers
(`AppAutoFileObjects.Filters`, `AppToDoObjects.PrefixList`, `OlFolderClassifierGroup`, and the
`IAppAutoFileObjects`/`IToDoObjects` interface members). Replace `ScoStack<IMovedMailInfo>` (used
by QuickFiler `QfcCollectionController`/`QfcDatamodel`/`QfcFormController` and by
`AppAutoFileObjects.MovedMails`/`SortEmail.UndoAsync`) with a `SloLinkedList`-based stack, not
`ScBag` — both undo loops read `stack[i]` and call positional `Pop(i)`, which require an ordered,
indexable, observable base.

Implementation gaps to close on the `SloLinkedList`-based stack:
1. Add positional surface absent from `LockingObservableLinkedList`: `this[int]` indexer,
   `Peek(int)`, `Pop(int)`, `TryPeek`/`TryPop` (front and indexed) via O(n) node walks;
   `Push` -> `AddFirst`, `Pop()`/`Peek()` -> `TakeFirst`/`First` are O(1).
2. Add `SerializeAsync()` (`SortEmail` calls `MovedMails.SerializeAsync()`).
3. Complete `SloLinkedList`'s stubbed `ISmartSerializable` members on the deserialize paths the
   stack exercises.
4. Reconcile construction: legacy `new ScoStack<...>(filename, folderpath, askUserOnError)` vs
   the `Static.Deserialize` pattern already used for `RecentsList` in `AppAutoFileObjects`.
5. Preserve on-disk JSON compatibility for the persisted `MovedMails` undo history.

**Open question for research:** whether `RecentsList<T> : ScoCollection<T>` is still consumed or
is dead code superseded by the `SloLinkedList<string>` `RecentsList` in `AppAutoFileObjects`.

### F3 — ScoSortedDictionary removal

`ScoSortedDictionary.cs` (`: ConcurrentObservableSortedDictionary` [Swordfish]) has no known
production consumer (only its definition and `ScoSortedDictionary_Tests.cs`). Confirm no
consumer, then delete the class and its test. A Swordfish-free sorted type, if wanted for future
use, is scoped separately; it cannot inherit `ScoDictionaryNew` (hash) and needs a new clean base
or a sort-maintaining decorator.

### F4 — Raw-usage and unused-using cleanup

Real swap: `KbdActions.cs` uses raw `ConcurrentObservableCollection<UClass>` — re-point to
`System.Collections.Generic.List<UClass>` per the F4 decision record (the "clean
`ConcurrentObservableCollection`" named in the original brief does not exist; `List<T>` natively
supplies the load-bearing `FindIndex(Predicate<T>)` and every other member the private `_list`
field uses, with no `CollectionChanged` or cross-thread mutation to preserve). Remove unused
`using Swordfish.NET.Collections;` from `KeyboardHandler.cs`, `FlagDetails.cs`, and
`FolderRemapController.cs` (delete and rebuild to confirm). Delete the stale
`"UtilitiesSwordfish.NET.General"`/`"UtilitiesSwordfish.NET.Test"` string literals in
`TraceUtility.cs`'s trace filter.

### F5 — Interfaces, project references, solution teardown

Remove (not migrate — per F5 research the interfaces are removable) `IScoCollection.cs` (uses
`Swordfish.NET.Collections`), `IScoCollection2.cs` (uses `Swordfish.NET.General.Collections`),
the dead `ISubjectMapSco`, and the dead `QfcExplorerController.UpdateForMove` method. Remove the
`ProjectReference` to `UtilitiesSwordfish.NET.General.csproj` from **nine** csprojs:
`UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, `TaskMaster.csproj`, `TaskMaster.Test.csproj`,
`QuickFiler.csproj`, `ToDoModel.csproj`, `Tags.csproj`, `TaskVisualization.csproj`, and
`TaskVisualization.Test.csproj` (the ninth is a stale reference found in F5 research, absent
from the original brief; Tags/TaskVisualization references confirmed stale — open question 3
resolved). Remove the `UtilitiesSwordfish` (`{F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF}`) and
`UtilitiesSwordfish.Test` project entries from `TaskMaster.sln` — both the `Project(...)`
declarations and the `GlobalSection(ProjectConfigurationPlatforms)` entries for both GUIDs — and
delete the two project folders. Execution carries a WI-0 HALT gate asserting F1–F4 have landed
before teardown. Migrate/remove tests referencing Swordfish
types directly (`ObservableDictionary_Tests.cs`, `ConcurrentObservableCollectionSenderTests.cs`,
`ConcurrentObservableCollectionLockRecursionTests.cs`, and any residual `Sco*` legacy tests not
already handled in F1–F3).

## Cross-cutting Acceptance Criteria

- Solution builds with `UtilitiesSwordfish`/`UtilitiesSwordfish.Test` removed and no remaining
  `Swordfish.NET.*` reference in first-party source or csproj files (repo-wide `Swordfish` search
  returns only archived docs/memory).
- Every persisted collection round-trips its existing on-disk JSON (MovedMails, Filters,
  PrefixList, SubjectMap encoder/decoder, CtfMap, FolderRemap/FilteredFolderScraping); a migration
  or compatibility test is provided per persisted type.
- Full C# toolchain passes in order: csharpier -> analyzers -> nullable -> MSTest. Coverage
  thresholds hold for changed/new code.
- No behavior regression in the QuickFiler undo flow or the SortEmail undo flow.

## Open Questions (resolved during child preparation research)

1. Is `RecentsList<T> : ScoCollection<T>` still consumed, or dead code superseded by the
   `SloLinkedList` `RecentsList`? (F2 research)
2. Is a Swordfish-free sorted dictionary wanted for future use, or is deletion of
   `ScoSortedDictionary` sufficient? (F3 research) — **RESOLVED (F3 preparation, issue #309):**
   deletion is sufficient. F3 research confirmed no production consumer of
   `ScoSortedDictionary`/`ConcurrentObservableSortedDictionary`, no JSON payload embeds the type
   name, and a Swordfish-free sorted type (which cannot inherit hash-based `ScoDictionaryNew`) is
   scoped separately if ever wanted.
3. Are the Tags/TaskVisualization project references genuinely unused? (F5 research) —
   **RESOLVED (F5 preparation, issue #308):** yes, both are stale, and research found a ninth
   stale reference in `TaskVisualization.Test.csproj` not listed in the original brief. All nine
   are removed in the F5 plan.
4. Are on-disk JSON payloads type-name-embedded (`TypeNameHandling.Auto`) such that a type rename
   breaks deserialization? If so, plan explicit migration/converter work. (F1 and F2 research)

## Non-Goals

- No new Swordfish-free sorted dictionary implementation (scoped separately if wanted).
- No behavior or UX changes beyond the collection-base migration.
- No new production dependencies.

## Preparation Deliverables (per child, before execution)

1. Promoted GitHub issue and active feature folder.
2. Research artifact under `<feature-folder>/research/`.
3. Completed `spec.md` and `user-story.md`.
4. Approved atomic plan at the canonical `plan.*.md` path in the feature folder.
5. `PREFLIGHT: ALL CLEAR` from atomic-executor preflight-only validation.

Execution (worktrees, wave scheduling, PRs, integration-to-main PR) starts only on maintainer
signal, via `epic-orchestrator` per `.claude/skills/epic-orchestrate/SKILL.md`.
