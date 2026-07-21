# Code Review — swordfish-collection-stack-lineage (#307, epic F2)

- Timestamp: 2026-07-11T00-32
- Reviewer: feature-reviewer
- Scope: three-dot merge-base diff `origin/epic/swordfish-removal-integration...HEAD`
- Verdict: **PASS** (0 Blocking findings; 3 non-blocking observations)

## Executive Summary

F2 replaces the two vendored-anchored collection/stack types with clean, Swordfish-free equivalents
and re-points every first-party consumer. The new `ConcurrentObservableCollection<T>` is built on
`System.Collections.ObjectModel.ObservableCollection<T>` and re-exposes the `ScoCollection` search,
list-conversion, observer, and serialization surface; `SloStack<T>` extends `SloLinkedList<T>` with
the positional stack members the undo loops require. The migration is type-only at the call sites
(unchanged control flow), preserves the bare-JSON-array on-disk shape, and preserves the undo
positional contract. The clean types are host-neutral and well covered (new-code line coverage
98.0%). The MovedMails construction is reconciled to `SloStack<IMovedMailInfo>.Static.Deserialize`,
and legacy `ScoCollection`/`ScoStack`/`RecentsList` are deleted after a verified no-consumer gate.
No correctness defects were found. Three low/informational observations are recorded below.

## Correctness Review of Key Elements

### Clean `ConcurrentObservableCollection<T>`
- Derives from `ObservableCollection<T>` and implements non-generic `IList`, supplying the
  `IList<T>` + `IList` surface natively (`this[int]`, `Add`, `Insert`, `RemoveAt`, `Remove`,
  `Contains`, `IndexOf`, `Count`, `CopyTo`, `IsReadOnly`, enumeration).
- Search helpers (`Find`, `FindIndex` overloads, `FindIndices` overloads, `Exists`) delegate to
  `IListExtensions`, matching the surface the Sco subclasses invoke via `base.`.
- `Subscribe(IObserver<...>)` replays current elements as `Add` notifications on subscribe (matching
  prior Swordfish observable semantics) and unsubscribes via a disposable token; `OnCollectionChanged`
  forwards to observers after raising the native event. Null observer is guarded. Correct.
- Thread-safety change is deliberate and documented: no `ReaderWriterLockSlim` on the mutate path;
  events raise synchronously; write paths run under `Task.Run` and no consumer requires concurrent
  multi-writer semantics (spec §Technical Approach). Reasonable and lower-risk than the vendored
  lock-recursion hazard it replaces.

### `SloStack<T> : SloLinkedList<T>`
- Top-of-stack == index 0 == `First`. `Push`→`AddFirst`, `Pop()`→`TakeFirst`, `Peek()`→`First.Value`
  (O(1)); indexed members walk via `NodeAt` (O(n)). `Pop(int)` removes the node at the ordinal and
  relies on linked-list removal to shift higher indices down. Semantics verified by
  `SloStackUndoContract_Tests` and `SloStack_Tests`.
- Empty-stack and out-of-range throws match the legacy contract (`InvalidOperationException` on empty
  `Pop`/`Peek`; `IndexOutOfRangeException` from `NodeAt`), asserted in tests.
- `Try*` variants return `false` with `default` out-value rather than throwing. Correct guard shape.
- Typed `ISmartSerializable<SloStack<T>>` re-exposed via `new` members over an `ism` built with
  `this`, so inherited serialization serializes the concrete `SloStack`. The four `SloLinkedList`
  `ISmartSerializable` stubs remain `NotImplementedException` by design (off the MovedMails
  file-based path); explicit-interface stubs keep them off the public surface. Matches spec §2.
- `Static.Deserialize(fileName, folderPath, askUserOnError)` provides the file-based load used by
  `LoadMovedMails`. Correct.

### Re-bases and re-points
- `CtfMap`, `SubjectMapSco` (incl. `AltListLoader`) re-based onto the clean collection and compile
  against `FindIndex`/indexer/`Add`/`ToList`/`CollectionChanged`/AltListLoader ctor.
- Direct consumers re-pointed: `AppAutoFileObjects.Filters`, `AppToDoObjects.PrefixList`,
  `OlFolderClassifierGroup._mailInfoCollection`/`LoadStaging`, and interface return types
  (`IAppAutoFileObjects.Filters`, `IToDoObjects.PrefixList`/`LoadPrefixList`). Type-only edits;
  control flow unchanged.
- `Filters` change-serialization cast updated to the clean type (`AppAutoFileObjects.cs:498`).

### MovedMails `Static.Deserialize` reconciliation
- `LoadMovedMails()` replaces `new ScoStack<IMovedMailInfo>(filename, folderpath, askUserOnError)`
  with `SloStack<IMovedMailInfo>.Static.Deserialize(_defaults.FileName_MovedEmails, pythonStaging,
  askUserOnError: false)`, mirroring the Recents migration precedent and exercising only the
  implemented file-based deserialize path. Comment documents the intent. Correct.

### On-disk JSON compatibility
- The clean collection carries no `[JsonObject]` and remains an `IEnumerable`/`IList<T>`, so
  Newtonsoft selects a `JsonArrayContract` and serializes a bare array (guardrail held).
  `CollectionRoundTrip_Tests` asserts bare-array shape (`StartWith("[")`, no `[JsonObject]` object
  wrapper) for CtfMap, SubjectMapSco, Filters; `$type` absence for concrete-element arrays; and
  per-element `$type` presence for the polymorphic PrefixList and MovedMails arrays. Element DTO
  names are unchanged (containers renamed, not elements), so `$type` strings are stable.
- `MovedMails_RoundTrips_...IndexZeroIsTop` proves array head deserializes to top-of-stack via the
  `Add`→`AddLast` replay, matching the ordering guarantee in spec §Data & State.

### Undo positional contract (`Pop(int)` shift-and-reprocess)
- `SortEmail.UndoAsync` and `QfcFormController.Actions.cs` (`UndoDialog`) both use forward index `i`,
  read `stack[i]`, and on confirmation call `stack.Pop(i)` **without advancing `i`** (so the shifted
  element is reprocessed), then `Serialize()`. The loops are behavior-identical to the legacy version;
  only the field/parameter type changed to `SloStack<IMovedMailInfo>`. The contract these loops
  depend on is directly asserted by `SloStackUndoContract_Tests`
  (`UndoLoop_ConfirmAll_...`, `UndoLoop_MixedConfirmAndSkip_...`, `PopAtOrdinal_ShiftsHigherIndices
  Down_...`). Preserved.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | TaskMaster/AppGlobals/AppToDoObjects.cs | whole file (503 lines) | File exceeds the 500-line limit (baseline 502, +1 from a type-only re-point) | Split in a follow-up outside F2 scope | Pre-existing over-limit; F2 did not introduce the condition | `wc -l` baseline 502 vs head 503 |
| Info | UtilitiesCS/.../ConcurrentObservableCollection.Serialization.cs | 33-37 | `ConcurrentObservableCollection(byte[])` discards the `DeserializeJson(byte[])` return, yielding an empty instance | Leave as-is (behavior preservation); revisit if a byte[] populate path is ever needed | Ported verbatim from legacy `ScoCollection`; no first-party populate-via-byte[] consumer | merge-base `ScoCollection.cs:72-76`; test doc `ConcurrentObservableCollectionSerialization_Tests.cs:34-35` |
| Info | UtilitiesCS/.../IConcurrentObservableCollectionSeams.cs | 35, 49 | Two default seam classes carry `[ExcludeFromCodeCoverage]` | Keep; the exclusion targets only thin host-bound wiring | Compliant under CLAUDE.md WinForms/I/O exemption (precedence over general-unit-test.md 85%/no-exclude stance) | `evidence/qa-gates/coverage-delta.md` |

## Design / Best-Practice Notes (positive)
- Serialization surface split into a `.Serialization.cs` partial keeps the primary type file at 169
  lines and cohesive.
- Positional stack members are concentrated in a dedicated subclass, containing the new O(n) coverage
  denominator in one small, fully unit-testable, host-neutral type (spec §Technical Approach).
- Injectable `IConcurrentObservableCollectionFileSystem`/`...Prompt` seams keep I/O testable without
  touching the real filesystem, consistent with the repo's isolation policy.
