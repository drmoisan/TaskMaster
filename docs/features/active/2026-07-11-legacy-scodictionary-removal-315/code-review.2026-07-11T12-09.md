# Code Review — legacy-scodictionary-removal (#315)

- Timestamp: 2026-07-11T12-09
- Reviewer: feature-review
- Diff range: d2d5e73bfbce7fb73b9d5be1601612cc01e54f09..HEAD (7184d0d1)

## Executive Summary

The diff is a clean net removal plus mechanical type-swaps. Code quality is consistent with repository
conventions: deletions are complete (class file, both dedicated test files, and all three `<Compile Include>`
entries), retargets are minimal and correct (`Add` -> `TryAdd` for the `ConcurrentObservableDictionary`-backed
successor), and comment updates accurately describe post-migration behavior. No best-practice defects of
FAIL or blocking severity were found. One informational observation is recorded. Verdict: PASS.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableNonTyped_Tests.cs | method names `IsSmartSerializable_ScoDictionaryInstance_ReturnsFalse`, `IsSmartSerializable_TypeOverload_ScoDictionary_ReturnsFalse` | Two test method identifiers retain the substring "ScoDictionary" although the body now instantiates `ConcurrentObservableCollection<int>`. | Optional: rename to reflect the new stand-in type in a future touch. | Names are cosmetic; they are not a live `ScoDictionary<>` type binding and no AC requires renaming. Non-blocking. | residual-binding-check.md; diff lines 769-790 |
| Info | ToDoModel/Data Model/People/PeopleScoDictionary.cs | line 19 (commented block) | Comment-only edit updates a fully commented-out class declaration to name `ScoDictionaryNew<>`. | None required. | Keeps stale reference text accurate; no compiled effect. | diff lines 1-13 |

No Blocking, High, Medium, or Low severity findings were identified.

## Detailed Observations

### Deletions are complete and consistent
- `SCODictionary.cs` (460 lines) removed and its `<Compile Include>` dropped from `UtilitiesCS.csproj`
  (diff lines 871-1349). Confirmed absent on disk and no bare `ScoDictionary<` type reference remains
  (word-boundary grep returned none; `WrapperScoDictionary<`, `IScoDictionary<`, and `ScoDictionaryNew<`
  are distinct retained types).
- `SCODictionary_Tests.cs` (296) and `SCODictionary_Additional_Tests.cs` (370) removed with both
  `<Compile Include>` entries dropped from `UtilitiesCS.Test.csproj` (diff lines 845-850). Legacy non-SDK
  VSTO manual csproj editing was handled correctly (no orphaned includes; clean analyzer build).

### Retargets are correct and minimal
- `SmartSerializableBase_Tests.cs` and the positive cases in `SmartSerializableNonTyped_Tests.cs` swap the
  concrete stand-in to `ScoDictionaryNew<string,int>` and correctly change `Add(key,value)` to
  `TryAdd(key,value)` to match the `ConcurrentObservableDictionary`-backed API (diff lines 731-747, 791-807).
  The in-loop CS1061 fix documented in `final-analyzer-build.md` confirms this was caught and resolved.
- The two negative cases (`IsSmartSerializable ... ReturnsFalse`, instance and type overloads) swap to the
  first-party `ConcurrentObservableCollection<int>` and add
  `using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;` (diff lines 761-790). The added
  using resolves to the first-party namespace (verified), so no new `Swordfish.NET.Collections` binding is
  introduced by the retarget.
- `SmartSerializableStatic_Tests.cs` deletes the now-redundant `ScoDictionary` negative; an equivalent
  `IsSmartSerializable_ConcurrentObservableCollection_ReturnsFalse` negative already exists in the same file
  (diff lines 817-839), so negative-path coverage of the `IsSmartSerializable` false branch is preserved.

### Comment accuracy
- `FolderScorer.cs` comment now describes `ScoDictionaryNew` (ConcurrentDictionary-backed) non-insertion-order
  enumeration and the ordinal tie-break rationale; accurate for post-migration behavior (diff lines 854-867).
- `FolderRemapController_Tests.cs` and `SubjectMapEncoder_Tests.cs` comment updates match the code they
  describe (diff lines 14-48).

### Test-policy conformance (General + C# Unit Test Policy)
- Retargeted tests keep Arrange-Act-Assert structure and FluentAssertions usage; they are deterministic,
  isolated, and add no external dependency or temporary file. MSTest/Moq/FluentAssertions frameworks are
  unchanged. Deletions remove obsolete `ScoDictionary`-specific behavior tests, which is appropriate since
  the class no longer exists.

## Verdict

PASS. Zero Blocking, High, Medium, or Low findings. Two Info observations, both non-blocking.
