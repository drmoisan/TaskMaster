# WI-2 — Stale ProjectReference Confirmation (P3-T1)

- **Timestamp:** 2026-07-11T13-15
- **Feature:** swordfish-interface-project-teardown (#308), F5

## Broad search (plan-specified pattern)

- **Command:** `git grep -nE "Sco|IScoCollection|ISubjectMapSco|Swordfish|ConcurrentObservable" -- "<dir>/*.cs"` for `Tags/`, `TaskVisualization/`, `TaskVisualization.Test/`
- **EXIT_CODE:** 0 (matches found — all classified below as non-Swordfish)

The broad pattern matches clean first-party names that were re-based Swordfish-free by F1-F4, plus
substring false positives. None binds to a `Swordfish.NET.*` type:

| Path | Matches | Classification |
|---|---|---|
| `Tags/` | only `RemoveNamespaceAttributes` / `inScopePrefixes` / `inScopeNs` in `My Project/MyNamespace.Static.2.Designer.cs` | FALSE POSITIVE — substring "Sco" inside "inScope"/"Scope" |
| `TaskVisualization/` | same `inScope*` Designer matches | FALSE POSITIVE |
| `TaskVisualization.Test/` | `IPeopleScoDictionaryNew`, `ScoCollection{FilterEntry}` (comment), `ConcurrentObservableCollection<FilterEntry>` | CLEAN first-party types from `UtilitiesCS` (not Swordfish) |

## Precise search (Swordfish / removed interfaces only)

- **Command:** `git grep -nE "Swordfish|IScoCollection\b|IScoCollection2\b|ISubjectMapSco\b" -- "Tags/*.cs" "TaskVisualization/*.cs" "TaskVisualization.Test/*.cs"`
- **EXIT_CODE:** 1
- **Output Summary:** ZERO matches — no `Swordfish.NET.*` type and no removed-interface symbol appears in any of the three projects.

## Binding confirmation for TaskVisualization.Test clean-type usages

- `TaskVisualization.Test/ManageFiltersControllerTests.cs:8` → `using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;` — `ConcurrentObservableCollection<FilterEntry>` resolves to the clean first-party base in `UtilitiesCS`, NOT `Swordfish.NET.Collections`.
- `TaskVisualization.Test/AutoAssignPeopleTests.cs:9` → `using UtilitiesCS;` — `IPeopleScoDictionaryNew` is a first-party clean type.

## Verdict

The `Tags`, `TaskVisualization`, and `TaskVisualization.Test` `UtilitiesSwordfish.NET.General.csproj`
ProjectReferences are STALE: none of the three projects references any vendored Swordfish type. Their
collection/dictionary usages resolve to clean first-party `UtilitiesCS` types. Provides the AC-7
stale-reference evidence; safe to remove the references (P3-T8, P3-T9, P3-T10).
