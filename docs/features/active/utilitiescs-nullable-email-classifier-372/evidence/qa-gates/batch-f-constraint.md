# Batch F — DO-NOT-ALTER Constraint Verification (Flags Subfolder)

Timestamp: 2026-07-19T04-40

Scope-boundary note: `Flags/` was CONFIRMED IN SCOPE at P0-T6 (5 of 5 REMEDIATE candidates emit CS86xx). This phase is applicable.

Per-file confirmation that only nullability annotations and justified `!` changed; no flag-parsing behavior change; no new `if (x is null) throw` guard.

## FlagDetails.cs
- `#nullable enable`; `CollectionChanged` event annotated nullable (`NotifyCollectionChangedEventHandler?`) — events are unassigned at construction. No behavior change.

## FlagClassNoItem.cs
- `#nullable enable`; the ten lazily-loaded fields (`_olCategories`, `_olCategorySelection`, `_categoryNames`, `_flags`, `_people`, `_projects`, `_context`, `_topic`, `_kb`, `_timer`) given `= null!` (populated on demand via the `Initializer.GetOrLoad(ref field, loader)` seam; `null!` keeps the `ref T` seam contract intact). `SelectionToOlCategories()` keeps its non-null return type with a justified `!` on the `?.ToList()` body (the GetOrLoad loader must be non-null; runtime null behavior preserved).

## FlagConsolidator.cs
- `#nullable enable`; the four `Lazy<...>` cache fields (`_asListNoPrefix`, `_asListWithPrefix`, `_asStringNoPrefix`, `_asStringWithPrefix`) given `= null!` (assigned by the refresh/init helper, not the constructor). Combine/refresh logic unchanged.

## FlagTranslator.cs
- `#nullable enable`; the four delegate fields (`_getStrFunc`, `_setStrFunc`, `_getListFunc`, `_setListFunc`) given `= null!` (set by the functional constructor via the existing `?? throw new ArgumentNullException` guards; the parameterless constructor leaves them unset). Existing guards unchanged.

## FlagParser.cs (>500 lines, NOT split)
- `#nullable enable`; seven change events (`PeopleChanged`/`ProjectsChanged`/`ProgramChanged`/`TopicsChanged`/`ContextChanged`/`KbChanged`/`PropertyChanged`) annotated nullable; `Combined` property `= null!` (set by `Initialize`). Reference-type default parameters `string value = default` and `ObservableCollection<string> value = default` annotated nullable (`string?`/`ObservableCollection<string>?`) to reflect the null default. `SplitToList(string? MainString, ...)` param annotated nullable (the body already handles `MainString is null`). The `.List = value!` assignments in the `SetXList` setters use a justified `!` because the `FlagDetails.List` setter explicitly handles null (`if (value is null)`). Local `string? AddWildcardsRet = default`.

## Interface co-annotation
- `IFlagTranslator.cs` was NOT co-annotated: the Batch F gate reached zero CS86xx without any CS8766/CS8767 implementer-mismatch. It remains EXCLUDE.

Confirmation:
- No `System.Diagnostics.CodeAnalysis` post-condition attribute added.
- No flag-parsing behavior, split/combine logic, or threshold changed; no new `if (x is null) throw` guard (AC3, AC5).
- `FlagParser.cs` (>500 lines) was NOT split.
