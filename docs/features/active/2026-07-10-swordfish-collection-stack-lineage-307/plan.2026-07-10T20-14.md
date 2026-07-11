# swordfish-collection-stack-lineage — Plan

- **Issue:** #307
- **Parent:** Epic swordfish-removal (child F2, wave 0, complexity C3)
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Branch:** `feature/swordfish-collection-stack-lineage` (off `epic/swordfish-removal-integration`)
- **Last Updated:** 2026-07-10T20-14
- **Status:** Draft
- **Plan path:** `docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/plan.2026-07-10T20-14.md`

## Required References (do not duplicate content)

- CLAUDE.md (standing instructions)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md` (C# code-change + unit-test policy)
- Spec: `docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/spec.md`
- User story: `docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/user-story.md`
- Research: `docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/research/2026-07-10T20-45-swordfish-collection-stack-lineage-research.md`

All work must comply with these policies. Apply the toolchain in the CLAUDE.md order:
csharpier → msbuild analyzers → msbuild nullable → vstest `/EnableCodeCoverage`.

## Evidence Location Invariant

All evidence artifacts resolve under
`docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/evidence/<kind>/`
with canonical kinds `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/`.
The delegation prompt supplied only canonical evidence kinds; no non-canonical path substitution
was required. Baseline coverage → `evidence/baseline/`; final-QC / post-change coverage →
`evidence/qa-gates/`. Writing to any `artifacts/**` evidence path is prohibited.

## Scope Lock — files this feature may CREATE / MODIFY / DELETE

CREATE (production, `UtilitiesCS`):
- `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.cs`
- `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.Serialization.cs`
- `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/IConcurrentObservableCollectionSeams.cs` (FS + Prompt seam interfaces)
- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs`

CREATE (tests):
- `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection_Tests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/Compatibility/CollectionRoundTrip_Tests.cs` (per-collection JSON round-trip)
- `UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStackUndoContract_Tests.cs` (undo positional-semantics regression)

MODIFY (production):
- `UtilitiesCS/EmailIntelligence/Ctf/CtfMap.cs`
- `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.cs` (+ `SubjectMapSco.Orchestration.cs` only if compile requires)
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (`Filters`, `MovedMails`, `LoadMovedMails`)
- `TaskMaster/AppGlobals/AppToDoObjects.cs` (`PrefixList`, `LoadPrefixList`)
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`
- `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` (`Filters`, `MovedMails`)
- `UtilitiesCS/Interfaces/IGlobals/IToDoObjects.cs` (`PrefixList`, `LoadPrefixList`)
- `QuickFiler/Controllers/QfcCollectionController.cs`
- `QuickFiler/Controllers/QfcDatamodel.cs`
- `QuickFiler/Controllers/QfcFormController.cs`
- `QuickFiler/Interfaces/IQfcCollectionController.cs`
- `QuickFiler/Interfaces/IQfcDatamodel.cs`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`

MODIFY (tests, re-point to clean types):
- `UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailFiler_TestSupport.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_TestSupport.cs` (mock `IToDoObjects.PrefixList`/`LoadPrefixList`)
- `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionSenderTests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
- `TaskVisualization.Test/ManageFiltersControllerTests.cs`
- `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableStatic_Tests.cs` (moot `ScoCollection` assertion)

MODIFY (build config, explicit `<Compile Include>` wiring for new `.cs`):
- `UtilitiesCS/UtilitiesCS.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

DELETE (only after grep-clean gate in Phase 7):
- `UtilitiesCS/EmailIntelligence/Recents/RecentsList.cs`
- `UtilitiesCS.Test/EmailIntelligence/RecentsList_Tests.cs`
- `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoCollection.cs`
- `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoStack.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/ScoCollection_Tests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/ScoStack_Tests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/ScoCollectionTests.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/ScoCollectionTests_UnfinishedStubs.cs`

## Non-Scope / Do-Not-Touch (scope boundary)

- Do NOT delete `UtilitiesSwordfish`, remove any `ProjectReference`, or edit `TaskMaster.sln` (F5).
- Do NOT modify `UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection.cs` or `IScoCollection2.cs`
  (F5). These are separate files from the concrete `ScoCollection.cs`; deleting the concrete class
  leaves the F5 interfaces intact.
- Do NOT modify `UtilitiesCS/Interfaces/IToDo/ISubjectMapSco.cs` — it inherits the F5-reserved
  `IScoCollection<SubjectMapEntry>`. `SubjectMapSco` does NOT declare `: ISubjectMapSco` (verified:
  only `: ScoCollection<SubjectMapEntry>` in `SubjectMapSco.cs:24` and the orchestration partial), so
  the re-base does not require implementing the F5 interface. Leave `ISubjectMapSco` and its lone
  parameter use at `QfcExplorerController.cs:275` untouched.
- Do NOT touch `ScoDictionary`/`ScoDictionaryNew` (F1) or `ScoSortedDictionary` (F3).
- Do NOT complete the four stubbed `SloLinkedList` `ISmartSerializable` members (`Deserialize<U>(loader)`,
  `Deserialize<U>(loader, askUserOnError, altLoader)`, `DeserializeAsync<U>(...)`,
  `DeserializeObject(json, settings)`); the MovedMails path uses only the file-based deserialize path.
- No new production dependencies; no JSON converter / on-disk migration; no behavior/UX change.
- `StackObjectCS.cs:13` `//TODO: Convert to ScoCollection` is a comment, not a reference — ignore.

---

### Phase 0 — Baseline Capture and Policy Review

- [x] [P0-T1] Read policy files in the policy-compliance-order sequence (CLAUDE.md → `.claude/rules/general-code-change.md` → `.claude/rules/general-unit-test.md` → `.claude/rules/csharp.md`) and write `evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four), each confirmed present.
- [x] [P0-T2] Capture the baseline repo-wide reference inventory by running `rg -n "ScoCollection<|ScoStack<" --glob '**/*.cs'` and record every hit path/line in `evidence/baseline/reference-inventory.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (count of hits and file list), establishing the deletion-gate starting set for Phase 7.
- [x] [P0-T3] Run `csharpier --check .` and write `evidence/baseline/csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (formatted/needs-format count).
- [x] [P0-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/baseline/msbuild-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (error/warning counts, build result).
- [x] [P0-T5] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `evidence/baseline/msbuild-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (nullable/type warnings-as-errors result).
- [x] [P0-T6] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /EnableCodeCoverage` and write `evidence/baseline/vstest-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a numeric coverage headline (repo-wide line% and branch%) and total passed/failed counts.

### Phase 1 — Create the Swordfish-free Clean Collection Base

- [x] [P1-T1] Create `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/IConcurrentObservableCollectionSeams.cs` defining the injectable filesystem and prompt seam interfaces (replacements for the `IScoCollectionFileSystem`/`IScoCollectionPrompt` seams that will be deleted with `ScoCollection.cs`), and add a matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists with the seam interfaces (read/write/exists file operations + user-error prompt), namespace `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection`; csproj wired so it compiles into `UtilitiesCS.dll`.
- [x] [P1-T2] Create `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.cs` as `public partial class ConcurrentObservableCollection<T> : ObservableCollection<T>, IList` with the non-generic `IList` surface, `FindIndex` (overloads), `FindIndices`, `Find(Predicate<T>)`, `Exists`, `event NotifyCollectionChangedEventHandler CollectionChanged` (native), and `IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs>)`, and add its `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: type builds on `System.Collections.ObjectModel.ObservableCollection<T>` (Swordfish-free); no `[JsonObject]` attribute present; `IList<T>` inherited from base; file ≤ 500 lines; csproj wired.
- [x] [P1-T3] Create `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.Serialization.cs` as the serialization partial: the `AltListLoader` delegate, file constructors (`()`, `(IEnumerable<T>)`, `(byte[])`, `(fileName, folderPath)`, `(fileName, folderPath, askUserOnError)`, `(fileName, folderPath, AltListLoader, backupFilepath, askUserOnError)`), `FilePath`/`FolderPath`/`FileName`, `Serialize()`/`Serialize(path)`, `SerializeAsync()`/`SerializeAsync(path)`, `Deserialize` overloads, `ToList()`/`FromList(IList<T>)`, and the static injectable FS/Prompt seam properties; add its `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: serialization surface matches the `ScoCollection<T>` member set the subclasses/consumers rely on (research Q4); uses `TypeNameHandling.Auto`; serializes via the inherited array contract (no `[JsonObject]`, no root `$type` wrapper); file ≤ 500 lines; csproj wired.
- [x] [P1-T4] Create `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection_Tests.cs` (MSTest + Moq + FluentAssertions) covering `FindIndex`/`FindIndices`/`Find`/`Exists`, indexer get/set, `Add`/`Insert`/`RemoveAt`/`Remove`/`Contains`/`IndexOf`/`Count`, `Subscribe` observer notification, `CollectionChanged` events, `ToList`/`FromList`, and `Serialize`/`SerializeAsync` via injected FS seam (no temp files); add its `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: tests compile and pass; the file-IO tests inject the FS seam (no filesystem/temp-file dependency); new clean-collection members exercised for the ≥ 90% new-code bar; csproj wired.
- [x] [P1-T5] Add a bare-array serialization guardrail test in `ConcurrentObservableCollection_Tests.cs` asserting `JsonConvert.SerializeObject(instance, settingsWithAuto)` produces a JSON array (starts with `[`) and round-trips element order/values via `JsonConvert.DeserializeObject<ConcurrentObservableCollection<T>>`
  - Acceptance: test passes proving the on-disk shape is a bare array with no `[JsonObject]` object wrapper.

### Phase 2 — Create the Swordfish-free Stack `SloStack<T>`

- [x] [P2-T1] Create `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs` as `public class SloStack<T> : SloLinkedList<T>, ISmartSerializable<SloStack<T>>` re-exposing the typed `ism`, `Serialize`/`SerializeThreadSafe`, typed instance `Deserialize` overloads, and a nested `Static` class with file-based `Static.Deserialize(fileName, folderPath[, askUserOnError])`, mirroring `SloLinkedList.cs:37-166` re-typed to `SloStack<T>`; add its `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: type compiles; inherited `Serialize`/`SerializeThreadSafe` serialize the concrete `SloStack` (base `ism` constructed with `this`); the four stubbed `ISmartSerializable` members are NOT completed (left inherited); csproj wired; file ≤ 500 lines.
- [x] [P2-T2] Add the positional/stack surface to `SloStack<T>`: `Push(T)`→`AddFirst` (O(1)), `Pop()`→`TakeFirst` throwing `InvalidOperationException` when empty (O(1)), `Peek()`→`First.Value` throwing when empty (O(1)), `this[int]` get via node walk (O(n)), `Peek(int)` throwing `IndexOutOfRangeException` on out-of-range (O(n)), `Pop(int)` node-walk remove-and-return with higher indices shifting down (O(n)), `TryPeek(out T)`/`TryPeek(out T,int)`, `TryPop(out T)`/`TryPop(out T,int)`
  - Acceptance: top-of-stack == index 0 == `First`; member behavior matches the legacy `ScoStack.cs:28-148` semantics (indices 0-based, position 0 == front).
- [x] [P2-T3] Add `SerializeAsync()` to `SloStack<T>` (async wrapper over the inherited synchronous serialize)
  - Acceptance: `SerializeAsync()` awaits to completion and persists the same payload as `Serialize()`.
- [x] [P2-T4] Create `UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack_Tests.cs` (MSTest + FluentAssertions) covering `Push`/`Pop()`/`Peek()`, `this[int]`, `Peek(int)`, `Pop(int)` (including the shift-down of higher indices), `TryPeek`/`TryPop` (front + indexed, success and failure paths), empty-stack throw paths, `SerializeAsync`, and a JSON array round-trip via `JsonConvert.DeserializeObject<SloStack<T>>`; add its `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: tests port the positional-semantics assertions from `ScoStack_Tests.cs`; array round-trip proves index 0 == `First`; all new `SloStack` members covered for the ≥ 90% new-code bar; no temp files; csproj wired.
- [x] [P2-T5] Add a file-based deserialize test in `SloStack_Tests.cs` exercising `SloStack<T>.Static.Deserialize(fileName, folderPath, askUserOnError:false)` through an injected filesystem seam (no temp files) and asserting element order/values
  - Acceptance: test proves the file-based path used by `LoadMovedMails` works without touching the four stubbed `ISmartSerializable` members.

### Phase 3 — Re-base `ScoCollection` Subclasses onto the Clean Collection

- [x] [P3-T1] Re-base `UtilitiesCS/EmailIntelligence/Ctf/CtfMap.cs` from `: ScoCollection<CtfMapEntry>` to `: ConcurrentObservableCollection<CtfMapEntry>`, updating the `using`/base-ctor references (`FindIndex`, `this[idx]` get, `Add`, enumeration, AltListLoader ctor)
  - Acceptance: `CtfMap` compiles against the clean-collection member surface; no `Swordfish.NET.*` reference remains in the file.
- [x] [P3-T2] Re-base `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.cs` (and `SubjectMapSco.Orchestration.cs` only if compile requires) from `: ScoCollection<SubjectMapEntry>` to `: ConcurrentObservableCollection<SubjectMapEntry>`, preserving `FindIndex`, `this[idx]` get, `Add`, `ToList()`, `Serialize()`, `CollectionChanged +=`, and the AltListLoader/file constructors
  - Acceptance: `SubjectMapSco` compiles; it does NOT declare `: ISubjectMapSco` (unchanged); no `Swordfish.NET.*` reference remains.
- [x] [P3-T3] Re-point `UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Tests.cs` from `ScoCollection<SubjectMapEntry>` and the `IScoCollectionFileSystem`/`IScoCollectionPrompt`/`ScoCollectionDependencyScope` seams to the clean-collection type and its new seam interfaces
  - Acceptance: test compiles against the clean types and passes; the in-memory FS seam replaces the deleted `ScoCollection` seams (no temp files).
- [x] [P3-T4] Add a CtfMap and SubjectMapSco JSON round-trip case to `UtilitiesCS.Test/EmailIntelligence/Compatibility/CollectionRoundTrip_Tests.cs` (create the file if not yet created) using an in-memory concrete-element array fixture (no `$type`), and add its `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: fixture is a bare JSON array of concrete `CtfMapEntry`/`SubjectMapEntry` objects; round-trip asserts element order/values and array shape stability; csproj wired.

### Phase 4 — Re-point Direct `ScoCollection<T>` Consumers and Interfaces

- [x] [P4-T1] Update `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` `Filters` return type from `ScoCollection<FilterEntry>` to `ConcurrentObservableCollection<FilterEntry>`
  - Acceptance: interface compiles; only the `Filters` member is retyped; `MovedMails` handled in Phase 5.
- [x] [P4-T2] Update `UtilitiesCS/Interfaces/IGlobals/IToDoObjects.cs` `PrefixList` and `LoadPrefixList` return types from `ScoCollection<IPrefix>` to `ConcurrentObservableCollection<IPrefix>`
  - Acceptance: interface compiles; both members retyped.
- [x] [P4-T3] Re-point `TaskMaster/AppGlobals/AppAutoFileObjects.cs` `Filters` (field + property + loader) from `ScoCollection<FilterEntry>` to `ConcurrentObservableCollection<FilterEntry>`, preserving the file constructor, `Subscribe(observer)`, and `Serialize()`
  - Acceptance: `AppAutoFileObjects.Filters` compiles against the clean collection; observer/serialize wiring unchanged.
- [x] [P4-T4] Re-point `TaskMaster/AppGlobals/AppToDoObjects.cs` `PrefixList`/`LoadPrefixList` from `ScoCollection<IPrefix>` to `ConcurrentObservableCollection<IPrefix>`, preserving the file constructor, `Count`, `Add(T)`, enumeration
  - Acceptance: compiles; member usage preserved.
- [x] [P4-T5] Re-point `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` `_mailInfoCollection`/`LoadStaging` from `ScoCollection<MinedMailInfo>` to `ConcurrentObservableCollection<MinedMailInfo>`, preserving the file constructor and enumeration
  - Acceptance: compiles; member usage preserved.
- [x] [P4-T6] Re-point the remaining test/mocks that reference `ScoCollection<...>`: `EmailDataMiner_TestSupport.cs` (`IToDoObjects.PrefixList`/`LoadPrefixList` mock members), `ClassifierGroups_Tests.cs:883` (`new ScoCollection<IPrefix>`), and `TaskVisualization.Test/ManageFiltersControllerTests.cs` (`new ScoCollection<FilterEntry>`) to `ConcurrentObservableCollection<...>`
  - Acceptance: all three test files compile against the clean collection and pass.
- [x] [P4-T7] Re-point `ConcurrentObservableCollectionSenderTests.cs` and `ConcurrentObservableCollectionLockRecursionTests.cs` from the Swordfish `ConcurrentObservableCollection` (`using Swordfish.NET.Collections`) to the clean collection, adjusting sender-identity and lock-behavior expectations to the `ObservableCollection<T>` base
  - Acceptance: both tests compile against the clean type and pass; no `using Swordfish.NET.*` remains; behaviors not reproducible on the clean base (e.g., `ReaderWriterLockSlim` recursion) are removed or re-expressed with a documented rationale.
- [x] [P4-T8] Add Filters and PrefixList JSON round-trip cases to `CollectionRoundTrip_Tests.cs` — Filters as a concrete-element array (no `$type`), PrefixList as a polymorphic-element array carrying the actual assembly-qualified `$type` of the concrete `IPrefix` implementation (read the concrete type name from the DTO source at implementation time)
  - Acceptance: both round-trips deserialize with the clean-collection consumer types, assert element order/values, and assert `$type` presence stability for the polymorphic PrefixList and `$type` absence for concrete Filters.

### Phase 5 — Migrate `ScoStack<IMovedMailInfo>` Consumers to `SloStack<IMovedMailInfo>`

- [x] [P5-T1] Update `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` `MovedMails` return type from `ScoStack<IMovedMailInfo>` to `SloStack<IMovedMailInfo>`
  - Acceptance: interface compiles; only `MovedMails` retyped.
- [x] [P5-T2] Update `QuickFiler/Interfaces/IQfcCollectionController.cs` and `QuickFiler/Interfaces/IQfcDatamodel.cs` `ScoStack<IMovedMailInfo>` member/parameter types to `SloStack<IMovedMailInfo>`
  - Acceptance: both interfaces compile with the retyped members.
- [x] [P5-T3] Re-point `QuickFiler/Controllers/QfcCollectionController.cs` (`MoveEmailsAsync` param), `QfcDatamodel.cs` (`MovedItems`), and `QfcFormController.cs` (`_movedItems` field, bind site, disposal null-out) from `ScoStack<IMovedMailInfo>` to `SloStack<IMovedMailInfo>`
  - Acceptance: all three controllers compile; the undo loop `QfcFormController.UndoDialog` still uses `Count`, `this[i]` get, `Pop(i)`, `Serialize()` with unchanged control flow.
- [x] [P5-T4] Re-point `TaskMaster/AppGlobals/AppAutoFileObjects.cs` `MovedMails` (field + property) to `SloStack<IMovedMailInfo>` and reconcile `LoadMovedMails()` construction from `new ScoStack<IMovedMailInfo>(filename, folderpath, askUserOnError:false)` to `SloStack<IMovedMailInfo>.Static.Deserialize(_defaults.FileName_MovedEmails, pythonStaging, askUserOnError:false)`, preserving the `Initialized(...)` memoization
  - Acceptance: `MovedMails` loads via the file-based `Static.Deserialize` path; no reliance on the four stubbed `ISmartSerializable` members; persistence continues via the existing `Serialize()`/`SerializeAsync()` calls at the undo/sort sites.
- [x] [P5-T5] Re-point `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` — `UndoAsync` (uses `Count`, `this[i]` get, `Pop(i)`, `Serialize()`), the `MovedMails.SerializeAsync()`/`Serialize()` call sites, and the `Push(info)` push site — from `ScoStack<IMovedMailInfo>` to `SloStack<IMovedMailInfo>`
  - Acceptance: `UndoAsync` compiles unchanged in control flow (forward index `i`, `stack[i]` read, positional `Pop(i)` without advancing `i`, final `Serialize()`); `[ExcludeFromCodeCoverage]` attribute retained.
- [x] [P5-T6] Re-point `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` push site (`AF.MovedMails.Push(info)`) to the `SloStack<IMovedMailInfo>` type
  - Acceptance: compiles; push semantics unchanged (Push → front).
- [x] [P5-T7] Migrate `UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs` and `EmailFiler_TestSupport.cs` from `ScoStack<IMovedMailInfo>` (incl. `Peek`, empty ctor) to `SloStack<IMovedMailInfo>`
  - Acceptance: both compile against `SloStack` and pass; `Peek`/positional usages map to the new members.
- [x] [P5-T8] Update `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` for the new `MovedMails` type/loader (`SloStack<IMovedMailInfo>` + `Static.Deserialize`)
  - Acceptance: compiles and passes against the migrated `MovedMails` load path.
- [x] [P5-T9] Create `UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStackUndoContract_Tests.cs` (MSTest + FluentAssertions) reproducing the undo-loop positional contract both `SortEmail.UndoAsync` and `QfcFormController.UndoDialog` depend on — forward index `i`, `stack[i]` read, positional `Pop(i)` that removes-and-returns at ordinal `i` and shifts higher indices down so the next element reprocesses at `i` — and add its `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`; write the pass-after result to `evidence/regression-testing/undo-contract.md`
  - Acceptance: tests assert the shift-and-reprocess ordinal semantics on `SloStack<IMovedMailInfo>` match the documented legacy `ScoStack` behavior; artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass counts); csproj wired.
- [x] [P5-T10] Add a MovedMails polymorphic-element JSON round-trip case to `CollectionRoundTrip_Tests.cs` using an in-memory array fixture where each element carries the actual assembly-qualified `$type` of the concrete `IMovedMailInfo` implementation (read from the DTO source at implementation time), deserializing to `SloStack<IMovedMailInfo>`
  - Acceptance: round-trip asserts element order/values, `$type` presence stability, and that index 0 == top-of-stack after replay via `Add`→`AddLast`.

### Phase 6 — Delete `RecentsList<T>` Dead Code

- [x] [P6-T1] Verify no production consumer of the TYPE `RecentsList<T>` remains by running `rg -n "RecentsList<" --glob '**/*.cs'` and confirming hits are limited to `RecentsList.cs`, `RecentsList_Tests.cs`, and commented blocks in `AppAutoFileObjects.cs`; record the result in `evidence/regression-testing/recentslist-deadcode-check.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming the type is unreferenced in live production code (the `AppAutoFileObjects.RecentsList` PROPERTY is `SloLinkedList<string>` and is unaffected).
- [x] [P6-T2] Delete `UtilitiesCS/EmailIntelligence/Recents/RecentsList.cs` and its `<Compile Include>` item in `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file removed and csproj entry removed; solution still builds.
- [x] [P6-T3] Delete `UtilitiesCS.Test/EmailIntelligence/RecentsList_Tests.cs` and its `<Compile Include>` item in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: file removed and csproj entry removed; test project still builds.

### Phase 7 — Delete Legacy `ScoCollection`/`ScoStack` After Grep-Clean Gate

- [ ] [P7-T1] Run the deletion gate `rg -n "ScoCollection<|ScoStack<" --glob '**/*.cs'` and confirm the only remaining hits are the F5-reserved interface files (`UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection.cs`, `IScoCollection2.cs`) and `UtilitiesCS/Interfaces/IToDo/ISubjectMapSco.cs` (inherits F5 `IScoCollection<SubjectMapEntry>`); record in `evidence/regression-testing/deletion-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; no first-party production or test reference to `ScoCollection<`/`ScoStack<` remains outside the listed F5-reserved interface files. If any other hit remains, STOP and re-point it before proceeding.
- [ ] [P7-T2] Update `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableStatic_Tests.cs:38-41` — remove or rewrite the assertion that `ScoCollection<int>` does not implement `ISmartSerializable<>` (moot once `ScoCollection` is deleted)
  - Acceptance: the moot assertion is removed/rewritten; the test class still compiles and passes.
- [ ] [P7-T3] Delete `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoCollection.cs` and `ScoStack.cs`, removing their `<Compile Include>` items from `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: both files and csproj entries removed; `UtilitiesCS` builds.
- [ ] [P7-T4] Delete the legacy direct tests `UtilitiesCS.Test/ReusableTypeClasses/ScoCollection_Tests.cs`, `ScoStack_Tests.cs`, `ScoCollectionTests.cs`, and `ScoCollectionTests_UnfinishedStubs.cs`, removing their `<Compile Include>` items from `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: files and csproj entries removed; `UtilitiesCS.Test` builds; representative round-trip coverage now lives in the clean-collection and `CollectionRoundTrip_Tests` suites.

### Phase 8 — Final QC Loop and Coverage Verification

- [ ] [P8-T1] Run `csharpier .` (apply formatting), then `csharpier --check .`, and write `evidence/qa-gates/csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (all files formatted, 0 needing changes). If formatting changed files, restart the loop from this task.
- [ ] [P8-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/qa-gates/msbuild-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (0 analyzer errors, build succeeded).
- [ ] [P8-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `evidence/qa-gates/msbuild-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (0 nullable/type warnings-as-errors, build succeeded).
- [ ] [P8-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /EnableCodeCoverage` and write `evidence/qa-gates/vstest-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change coverage headline (repo-wide line% and branch%), all tests passed, 0 failed. No `SKIPPED`.
- [ ] [P8-T5] Compute and record new/changed-code coverage for the new `ConcurrentObservableCollection<T>` (+ serialization partial + seams) and `SloStack<T>` members from the Phase 8 coverage run, and write the delta/threshold report to `evidence/qa-gates/coverage-delta.md`
  - Acceptance: artifact reports baseline coverage (from Phase 0 `evidence/baseline/vstest-coverage.md`), post-change coverage (P8-T4), and new/changed-code coverage; asserts new `SloStack`/clean-collection members meet ≥ 90% new-code and ≥ 85% line / ≥ 75% branch, and repo-wide floor did not regress. If any threshold is unmet, outcome is remediation-required (not PASS).
- [ ] [P8-T6] Re-run the full toolchain loop (P8-T1 → P8-T4) once more if any prior QC task changed files, and confirm a single clean pass across csharpier → analyzers → nullable → vstest
  - Acceptance: one clean end-to-end pass recorded with no file changes and no failures; if any step changed files or failed, the loop restarts from P8-T1.

---

## Acceptance-Criteria Traceability (spec.md)

- Clean `ConcurrentObservableCollection<T>` created with full surface → P1-T1, P1-T2, P1-T3.
- Bare-array serialization guardrail (no `[JsonObject]`) → P1-T3, P1-T5.
- `CtfMap`/`SubjectMapSco` re-based → P3-T1, P3-T2.
- Direct `ScoCollection<T>` consumers re-pointed (Filters, PrefixList, OlFolderClassifierGroup) → P4-T3, P4-T4, P4-T5.
- Interface members `IAppAutoFileObjects.Filters`, `IToDoObjects.PrefixList`/`LoadPrefixList` → P4-T1, P4-T2.
- `SloStack<T>` positional surface + top-of-stack == index 0 → P2-T1, P2-T2.
- `SloStack<T>` `SerializeAsync` + typed `ISmartSerializable`/`Static.Deserialize` → P2-T1, P2-T3.
- All `ScoStack<IMovedMailInfo>` consumers migrated → P5-T1..P5-T8.
- MovedMails construction reconciled to file-based `Static.Deserialize` → P5-T4.
- Per-collection JSON round-trip tests (MovedMails, Filters, PrefixList, CtfMap, SubjectMapSco) → P3-T4, P4-T8, P5-T10.
- Undo behavior preserved (SortEmail.UndoAsync, QfcFormController.UndoDialog) → P5-T3, P5-T5, P5-T9.
- `RecentsList<T>` dead code removed → P6-T1, P6-T2, P6-T3.
- Legacy `ScoCollection.cs`/`ScoStack.cs` + direct tests deleted after grep-clean gate → P7-T1, P7-T3, P7-T4.
- Migrated tests compile/pass against clean types → P3-T3, P4-T6, P4-T7, P5-T7, P5-T8, P7-T2.
- New members meet new-code coverage bar → P1-T4, P2-T4, P8-T5.
- Full toolchain passes in order → P8-T1..P8-T6.
- Scope boundary held (no Swordfish deletion / ProjectReference / sln / F1/F3/F5 changes) → Non-Scope section + P7-T1 gate.

## Open Questions / Notes

- Exact assembly-qualified `$type` strings for the concrete `IMovedMailInfo` and `IPrefix`
  implementations must be read from the DTO source when authoring the polymorphic round-trip
  fixtures (P4-T8, P5-T10). Research flagged these cannot be derived from code alone without a
  sample production file.
- Thread-safety: the clean `ObservableCollection<T>` base does not reproduce the Swordfish
  `ReaderWriterLockSlim` lock-recursion model (documented hazard at `AppAutoFileObjects.cs:588-609`).
  Current write paths run under `Task.Run`; confirm no consumer depends on concurrent multi-writer
  semantics during P4-T7 lock-recursion test re-expression.
- `AF.RecentsList.AddRecent(...)` (`SortItemsToExistingFolder.cs:160`) resolves against the
  `SloLinkedList<string>` property and is outside F2 scope; confirm it is present before relying on
  the Recents path in any test.
