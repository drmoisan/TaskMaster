# swordfish-scosorteddictionary-removal (Issue #309)

- Date captured: 2026-07-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/swordfish-scosorteddictionary-removal/ (Issue #309)
- Epic: swordfish-removal (child F3, integration branch `epic/swordfish-removal-integration`)

- Issue: #309
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/309
- Last Updated: 2026-07-11
- Work Mode: full-feature

## Problem / Why

The vendored Swordfish-based `ScoSortedDictionary<TKey,TValue>`
(`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`) derives
from the Swordfish `ConcurrentObservableSortedDictionary` and is believed to have no production
consumer — only its own definition and its test
(`UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`). It is one of the
first-party dependencies on `Swordfish.NET.*` that the swordfish-removal epic must eliminate so
the vendored `UtilitiesSwordfish` project can be torn down (epic child F5). Removing the unused
class shrinks the analyzer-exempt vendored surface and unblocks the No-COM/testability direction.

## Proposed Behavior

Confirm, with an auditable repo-wide search, that no production code references
`ScoSortedDictionary` or `ConcurrentObservableSortedDictionary`. If confirmed, delete the class
and its test. This feature's deliverable is deletion only.

Scope boundary: delete ONLY `ScoSortedDictionary` and its test (and the two matching
`<Compile Include>` entries in the classic-format `UtilitiesCS.csproj` and
`UtilitiesCS.Test.csproj`, required so the build does not reference a deleted file). Do NOT delete
the `UtilitiesSwordfish` project, remove any `ProjectReference`, touch `TaskMaster.sln`, migrate
interfaces, or touch the F1 (dictionary), F2 (collection/stack), or F4 (raw-usage) types.

## Acceptance Criteria (early draft)

- [ ] Auditable repo-wide search confirms no production consumer of `ScoSortedDictionary` /
  `ConcurrentObservableSortedDictionary`; scope, patterns, and results are recorded.
- [ ] `ScoSortedDictionary.cs` and `ScoSortedDictionary_Tests.cs` are deleted.
- [ ] The matching `<Compile Include>` entries are removed from `UtilitiesCS.csproj` and
  `UtilitiesCS.Test.csproj`.
- [ ] The solution builds and all tests pass after removal (full C# toolchain green).
- [ ] No behavior or API change to any other type; no `ProjectReference`/`.sln` change.

## Constraints & Risks

- If any production consumer exists, STOP and report it as a blocking finding rather than deleting.
- Classic (non-SDK) csproj format: a deleted `.cs` still referenced by `<Compile Include>` breaks
  the build; the Compile entries must be removed in the same change.
- On-disk serialization compatibility is not a concern for this type because it has no persisted
  production consumer (to be confirmed by research).

## Test Conditions to Consider

- [ ] Repo-wide reference search (production `.cs`, `.csproj`, and non-code references).
- [ ] Full solution build + MSTest run green after deletion.
- [ ] Coverage: deletion removes a class and its dedicated test together; verify no unrelated
  coverage regression on changed/remaining lines.

## Open Question (record finding in research)

- Is a Swordfish-free sorted dictionary wanted for future use? If yes, it is out of scope for this
  feature (it cannot inherit `ScoDictionaryNew`, which is hash-based, and would need a new clean
  base or a sort-maintaining decorator — scope separately). This feature's deliverable is deletion
  only.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create active feature folder from the template
