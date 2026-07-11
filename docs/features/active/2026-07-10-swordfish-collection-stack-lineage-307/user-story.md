# `swordfish-collection-stack-lineage` — User Story

- Issue: #307
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-10T21-15
- Work Mode: full-feature
- Epic: swordfish-removal (child F2, enabler)

## Story Statement

- As the maintainer of TaskMaster, I want every first-party collection and stack that derives from
  or consumes the vendored Swordfish `ScoCollection<T>`/`ScoStack<T>` re-based onto Swordfish-free
  equivalents, so that the epic's teardown child (F5) can remove the `UtilitiesSwordfish` project
  reference and the No-COM/testability direction is unblocked.
- As the maintainer, I want on-disk JSON compatibility preserved for MovedMails, Filters,
  PrefixList, CtfMap, and SubjectMapSco, so that existing users' persisted files continue to load
  after the migration without a converter or manual data migration.
- As a QuickFiler/SortEmail user, I want the undo flows to behave exactly as before, so that
  reversing recent mail moves works identically after the collection-base swap.

## Problem / Why

The vendored `Swordfish.NET.General` project is unmaintained third-party code that the
No-COM/testability direction seeks to remove. Two vendored-based collection types anchor a tree of
first-party dependents:

- `ScoCollection<T>` (`UtilitiesCS\...\SCO\ScoCollection.cs`:
  `ScoCollection<T> : ConcurrentObservableCollection<T>, IList<T>, IList` [Swordfish]).
- `ScoStack<T>` (`UtilitiesCS\...\SCO\ScoStack.cs`: `ScoStack<T> : ScoCollection<T>`).

Until every first-party type that derives from or consumes these two types is re-based onto
Swordfish-free equivalents, F5 cannot remove the `UtilitiesSwordfish` project reference. F2 performs
the collection + stack lineage migration. This is enabler work: it delivers no new end-user feature,
but it removes an unmaintained third-party collection library from the dependency surface, shrinks
the analyzer-exempt vendored code, and consolidates on the repository's Swordfish-free serializable
collection bases — the epic's stated business-outcome hypothesis.

## Enabler Business-Outcome Alignment

This feature contributes to the epic's leading indicators without changing product behavior:

- Reduces the set of first-party files referencing `Swordfish.NET.*` (collection/stack half),
  moving the repo toward a `Swordfish` search that returns only archived docs/memory.
- Preserves every persisted collection's on-disk JSON, satisfying the epic NFR that on-disk
  serialization compatibility is preserved for every persisted collection with a compatibility test
  per persisted type.
- Preserves the QuickFiler and SortEmail undo flows, satisfying the epic NFR of no undo-flow
  behavior regression.

## Personas & Scenarios

- Persona: TaskMaster maintainer (Dan Moisan).
  - Who: owns the build, the vendored-code removal effort, and the No-COM/testability direction.
  - Cares about: a smaller, maintained dependency surface; unblocking F5; not breaking users'
    persisted data or undo behavior.
  - Constraints: no new production dependencies; no behavior/UX change; full C# toolchain must stay
    green; new code must meet coverage thresholds.
  - Goals/frustrations: wants the Swordfish dependency gone but cannot risk silent deserialization
    failures on existing on-disk files or a regression in the undo flows.

- Persona: end user of the QuickFiler / SortEmail undo flow.
  - Who: uses TaskMaster to move mail and occasionally reverse recent moves.
  - Cares about: undo reversing exactly the moves they expect, in the expected order.
  - Constraints: relies on the persisted MovedMails history loading correctly across sessions.

- Scenario: undo after the migration.
  - Actor: an end user reversing recent mail moves.
  - Trigger: the user invokes undo (RibbonController → `SortEmail.UndoAsync`, or the QuickFiler
    `UndoDialog`).
  - Steps: the loop reads the top-of-stack item at index 0, prompts, and on confirmation removes and
    returns exactly that ordinal item via `Pop(i)` (without advancing `i`, so the next item shifts
    into place and is reprocessed), then persists via `Serialize()`.
  - Obstacle avoided: the new `SloStack<IMovedMailInfo>` must implement `this[int]` and `Pop(int)`
    with the same ordinal semantics as the legacy `ScoStack`, and top-of-stack must remain index 0.
  - Expected outcome: undo behaves identically to before the migration.

- Scenario: loading persisted collections after the migration.
  - Actor: TaskMaster startup / the maintainer verifying compatibility.
  - Trigger: the application loads MovedMails, Filters, PrefixList, CtfMap, and SubjectMapSco from
    existing on-disk files.
  - Steps: each file (a bare JSON array under `TypeNameHandling.Auto`) deserializes into the clean
    replacement type; polymorphic-element collections (MovedMails, PrefixList) carry per-element
    `$type` that is unchanged because element DTO names are not renamed.
  - Expected outcome: all five collections load with identical element order and values; no converter
    or manual migration is needed.

## Acceptance Criteria

- [x] As the maintainer, the Swordfish-free clean `ConcurrentObservableCollection<T>` base exists
      (created in F2, built on `ObservableCollection<T>`) with the full member surface the Sco
      subclasses and consumers require, so that the collection re-base can proceed without pulling in
      Swordfish.
- [ ] As the maintainer, every `ScoCollection<T>` subclass and direct consumer (`CtfMap`,
      `SubjectMapSco`, `AppAutoFileObjects.Filters`, `AppToDoObjects.PrefixList`,
      `OlFolderClassifierGroup`, and the `IAppAutoFileObjects.Filters` / `IToDoObjects.PrefixList` /
      `LoadPrefixList` interface members) is re-based onto the clean collection, so that no
      collection dependent rides Swordfish.
- [ ] As the maintainer, `SloStack<T> : SloLinkedList<T>` provides the positional surface the undo
      loops require (`this[int]`, `Peek(int)`, `Pop(int)`, `TryPeek`/`TryPop` front and indexed,
      `Push`→AddFirst, `Pop()`/`Peek()`→TakeFirst/First, top-of-stack == index 0) plus
      `SerializeAsync()` and file-based `Static.Deserialize`, so that the stack lineage is
      Swordfish-free.
- [ ] As the maintainer, every `ScoStack<IMovedMailInfo>` consumer (QuickFiler controllers and
      interfaces, `AppAutoFileObjects.MovedMails`/`LoadMovedMails`, `SortEmail`, `EmailFiler`,
      `IAppAutoFileObjects.MovedMails`) is migrated to `SloStack<IMovedMailInfo>` with construction
      reconciled to the file-based `Static.Deserialize` pattern, so that the MovedMails undo history
      no longer depends on Swordfish.
- [ ] As an end user, a JSON round-trip compatibility test exists per persisted collection
      (MovedMails, Filters, PrefixList, CtfMap, SubjectMapSco), so that existing on-disk files load
      unchanged after the migration.
- [ ] As an end user, the QuickFiler and SortEmail undo flows are preserved with no regression, so
      that reversing recent mail moves works identically.
- [ ] As the maintainer, the dead `RecentsList<T>` type and its test are deleted (not migrated), so
      that no effort is spent re-basing code already superseded by `AppAutoFileObjects.RecentsList :
      SloLinkedList<string>`.
- [ ] As the maintainer, legacy `ScoCollection.cs`/`ScoStack.cs` and their direct tests are removed
      only after re-pointing leaves them unreferenced, so that the collection/stack half of the
      Swordfish surface is gone without prematurely breaking a still-referenced type.
- [ ] As the maintainer, the full C# toolchain passes (csharpier → analyzers → nullable → MSTest)
      and new `SloStack`/clean-collection members meet the new-code coverage bar, so that the
      migration lands green and well-covered.

## Non-Goals

- No deletion of the `UtilitiesSwordfish` project, `ProjectReference` removal, or `TaskMaster.sln`
  edits (reserved for F5).
- No migration of `IScoCollection`/`IScoCollection2` (F5).
- No changes to `ScoDictionary`/`ScoDictionaryNew` (F1) or `ScoSortedDictionary` (F3).
- No JSON converter or on-disk migration work (research verified none is required).
- No new production dependencies; no behavior or UX changes beyond the migration.
