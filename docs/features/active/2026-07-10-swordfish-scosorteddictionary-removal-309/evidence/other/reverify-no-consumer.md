# Phase 1 — Re-Verify No Consumer (P1-T1)

- Timestamp: 2026-07-10T23:40

## Commands Run (repo root, immediately before deletion)

1. `grep -rn "ScoSortedDictionary" --include="*.cs" .`
2. `grep -rn "ScoSortedDictionary" --include="*.csproj" .`
3. `grep -rn "ConcurrentObservableSortedDictionary" --include="*.cs" .`

## Results

### Command 1 — `ScoSortedDictionary` in `*.cs`

- `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs` — the class's own definition (constructors, `CreateEmpty`, `DeserializeJson`, etc.), 12 lines.
- `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs` — the class's own dedicated test file, 34 hits (test method bodies + helper `OverrideScoSortedDictionaryField`).

No other `*.cs` file anywhere in the repository references `ScoSortedDictionary`.

### Command 2 — `ScoSortedDictionary` in `*.csproj`

- `UtilitiesCS/UtilitiesCS.csproj:1047` — `<Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs" />`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj:414` — `<Compile Include="ReusableTypeClasses\ScoSortedDictionary_Tests.cs" />`

Exactly the two `<Compile Include>` entries named in the plan's scope lock. No other csproj references it.

### Command 3 — `ConcurrentObservableSortedDictionary` in `*.cs`

- `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs:17` — `ScoSortedDictionary<TKey, TValue> : ConcurrentObservableSortedDictionary<TKey, TValue>` (the deletion target's own base-class reference).
- `UtilitiesSwordfish/Collections/ConcurrentObservableSortedDictionary.cs` — the unrelated Swordfish base-type file itself (definition, 2 hits: class declaration + constructor). This file is explicitly out of scope (`UtilitiesSwordfish/**` is in the plan's "Out of scope" list) and is NOT deleted or modified.
- `UtilitiesSwordfish.Test/ObservableSortedDictionaryTest.xaml.cs:44` — an unrelated Swordfish-internal WPF test harness that instantiates `ConcurrentObservableSortedDictionary` directly (not `ScoSortedDictionary`); this is a test of the vendored base type itself, also out of scope and untouched.

## Conclusion

Zero genuine production consumers of `ScoSortedDictionary` (or of `ConcurrentObservableSortedDictionary` outside its own vendored definition/test) were found beyond: the class's own definition file, its own dedicated test file, the two `<Compile Include>` build entries naming those two files, and the unrelated `UtilitiesSwordfish/Collections/ConcurrentObservableSortedDictionary.cs` base-type file (plus its own out-of-scope Swordfish test harness). This confirms the research finding (`research/research.2026-07-10T21-10.md`, GO recommendation) still holds immediately before deletion. Phase 1 deletion tasks (P1-T3 through P1-T6) are cleared to proceed. **No BLOCKED condition.**
