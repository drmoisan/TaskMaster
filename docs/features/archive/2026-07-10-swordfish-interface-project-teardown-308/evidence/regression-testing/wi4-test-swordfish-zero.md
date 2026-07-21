# WI-4 — Direct-Swordfish Test Removal Verification (P2-T4)

- **Timestamp:** 2026-07-11T13-07
- **Feature:** swordfish-interface-project-teardown (#308), F5

## Removals performed (WI-4)

- `git rm UtilitiesCS.Test/ReusableTypeClasses/ObservableDictionary_Tests.cs` (P2-T1)
- `git rm UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionSenderTests.cs` (P2-T2)
- `git rm UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs` (P2-T3)
- Removed the three corresponding `<Compile Include>` entries from `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

## Verification

- **Command:** `git grep -n "using Swordfish" -- "UtilitiesCS.Test/*.cs"`
- **EXIT_CODE:** 1
- **Output Summary:** zero matches — no residual direct-Swordfish `using` directive remains under `UtilitiesCS.Test/`. Completes AC-11.

- **Command:** `grep -n "<removed test filenames>" UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- **EXIT_CODE:** 1
- **Output Summary:** zero matches — no residual `<Compile Include>` for the three removed test files.

## Note — surviving documentary comment mentions (handled at P5-T6)

A broad `git grep -n "Swordfish" -- "UtilitiesCS.Test/*.cs"` returns four DOCUMENTARY comment
mentions in surviving F1/F2-authored test files (not bindings, not `using` directives):

- `ConcurrentObservableCollection_Tests.cs:16` — `/// Unit tests for the Swordfish-free ...`
- `ConcurrentObservableCollection_Tests.cs:83` — `// Preserves the legacy Swordfish FindIndices ...`
- `SloStack_Tests.cs:12` — `/// Unit tests for the Swordfish-free ...`
- `ScoDictionaryNew_OnDiskCompatibility_Tests.cs:11` — `/// Swordfish-free ... lineage`

Plus one production documentary comment at
`UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.cs:14`.

These prose comments reference the removed library conceptually. The literal P5-T6 / AC-13 verification
(`rg "Swordfish" -g "*.cs" -g "*.csproj" -g "*.sln"` returns zero) requires them reconciled; they are
reworded minimally at P5-T6 as a mechanically-necessary step to satisfy the epic-completion gate.

## Verdict

WI-4 test removal complete. Delivers AC-11 (no residual direct-Swordfish test usage).
