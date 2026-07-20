# Final QC — CS8714 Constraint Consistency and AC6 Exemption (P9-T9)

Timestamp: 2026-07-19T22-03

## Commands

- `grep -l "where TKey : notnull" <3 bases + 4 waiver consumers>`
- `grep -c "where TKey : notnull" <ScoDictionaryStatic, ConcurrentObservableBag, ScBag>`
- `git log --oneline <merge-base>..HEAD -- UtilitiesCS/NewtonsoftHelpers/<file>` (authorship attribution)
- `grep -c "#nullable enable" <3 exempt WinForms files>`

## Constraint PRESENT (`where TKey : notnull`)

Three truly generic dictionary bases:
- `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Dictionary/ConcurrentObservableDictionary.cs` — present
- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs` — present
- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/ScDictionary.cs` — present

Four epic-authorized cross-child waiver consumers:
- `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` — present (Option A, Batch 6)
- `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` — present (Option A-prime, Batch 6)
- `UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs` — present (Option A'', Batch 8, this session)
- `UtilitiesCS/NewtonsoftHelpers/ScDictionaryConverter.cs` — present (Option A'', Batch 8, this session)

## Constraint ABSENT (must NOT be constrained)

- `ScoDictionaryStatic.cs` — 0 (non-generic `public static class` of `Type` extension methods; no
  `TKey` type parameter; the constraint is mechanically inapplicable. This documents the plan's
  "four generic bases" wording deviation: only three are truly generic.)
- `ConcurrentObservableBag.cs` — 0 (`ConcurrentBag<T>`-based; takes `T`; no `notnull` requirement).
- `ScBag.cs` — 0 (`ConcurrentBag<T>`-based; takes `T`; no `notnull` requirement).

## NewtonsoftHelpers modification scope (attributed to #366)

`#366`-authored commits (`feat(366)` / working-tree edits) modified exactly the FOUR waiver
consumers in NewtonsoftHelpers: `ScoDictionaryConverter.cs` + `WrapperScoDictionary.cs` (committed
in Batch 6, ddbe93b9) and `WrapperScDictionary.cs` + `ScDictionaryConverter.cs` (this session's
working-tree constraint edits). No other NewtonsoftHelpers file was modified by #366.

The remaining ~15 NewtonsoftHelpers files (e.g. `AllInclusiveBinder.cs`, `AppGlobalsConverter.cs`,
`MonoExtension.cs`, `WrapperPeopleScoDictionaryNew.cs`) that appear in a merge-base-with-`main` diff
were `#nullable enable`'d by the SEPARATE sibling child #367 (commit c9284b30,
`fix(367): remediate nullable-reference-type debt in UtilitiesCS/NewtonsoftHelpers`), which is
integrated onto this branch. They are out of #366 scope and out of #366 authorship.

## AC6 exemption — three WinForms files remain null-oblivious

- `NewSmartSerializable/Config/ConfigViewer.Designer.cs` — 0 `#nullable enable`
- `NewSmartSerializable/Config/ConfigViewer.cs` — 0 `#nullable enable`
- `NewSmartSerializable/Config/ConfigGroupBox.cs` — 0 `#nullable enable`

All three exempt WinForms files carry no `#nullable enable` pragma (WinForms exemption (b)); they
were not opted in and are not cross-blocked by #366's opt-in (AC6).
