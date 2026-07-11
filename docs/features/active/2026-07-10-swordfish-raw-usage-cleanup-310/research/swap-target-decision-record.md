# F4 Decision Record: KbdActions collection swap target and unused-using cleanup

- Timestamp: 2026-07-10T20-30
- Status: Accepted (recommendation for maintainer/epic-orchestrator review)
- Scope: epic child F4 (swordfish-raw-usage-cleanup, Issue #310), swordfish-removal epic

## Context

The epic manifest ("Shared Design") states a clean, Swordfish-free `ConcurrentObservableCollection`
"already in the repo" at `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`. Verified
against the integration base: that type does not exist. That namespace folder contains only `Bag`
and `Dictionary` types; the only production `ConcurrentObservableCollection<T>` is the vendored
Swordfish type in `UtilitiesSwordfish/Collections/ConcurrentObservableCollection.cs` (the type the
epic removes).

## Decision

Swap `QuickFiler/Controllers/KbdActions.cs` `_list` from the raw Swordfish
`ConcurrentObservableCollection<UClass>` to `System.Collections.Generic.List<UClass>`.

Evidence-based justification:
- `KbdActions._list` is `private` and uses only an ordered/indexable list surface: two
  constructors (`()` and `(IEnumerable<UClass>)`), `Add`, `RemoveAt(int)`, `GetEnumerator`, LINQ,
  and `FindIndex(Predicate<UClass>)` (call sites 81, 126).
- `FindIndex(Predicate<T>)` is the load-bearing member. It is a Swordfish-collection member, not an
  `IList<T>`/`Collection<T>`/`ObservableCollection<T>` member. Of BCL collections only
  `System.Collections.Generic.List<T>` provides it natively, so `List<T>` closes the swap with zero
  new code and no shim.
- `KbdActions` exposes no `CollectionChanged` and performs no cross-thread mutation of `_list`
  (registration is setup-time), so the observable/concurrent semantics of the Swordfish type are
  unused by this consumer. The swap is behavior-neutral.
- `List<T>` is already in scope via `using System.Collections.Generic;` (line 4); the swap removes
  `using Swordfish.NET.Collections;` (line 10).

## Alternatives considered

- (a) Depend on child F2 to introduce a clean `ConcurrentObservableCollection` (with a
  `FindIndex(Predicate<T>)` member), then swap `KbdActions` onto it. Rejected as the default because
  the type does not exist yet, F2's charter is re-basing `ScoCollection`/`ScoStack` (not committing
  to a general concurrent-observable collection with `FindIndex`), and it introduces a real
  execution-order dependency that contradicts F4's manifest `depends_on: []` and wave-0 placement.
- (c) F4 authors the clean type itself. Rejected: C3-scale work exceeding F4's C2 charter; risks
  duplicating the type F2 should own.

## Consequences and epic-level finding (F4 -> F2)

- Under this decision F4 stays independent (wave 0, `depends_on: []` accurate) and Swordfish-free
  without any cross-feature dependency.
- The epic manifest "Shared Design" note is inaccurate and should be corrected by the
  epic-orchestrator: either point it at the F2-introduced clean type, or record that `KbdActions`
  consolidates on `List<T>` because it uses no observable/concurrent surface.
- Override path: if the maintainer requires `KbdActions` to consolidate on the epic's clean
  concurrent-observable base, adopt alternative (a); F4's `depends_on` must then be corrected to
  include F2 and F4 moves to wave 1.

## Other F4 work items (behavior-neutral, no dependency)

- Remove `using Swordfish.NET.Collections;` from `KeyboardHandler.cs:17`, `FlagDetails.cs:13`, and
  `FolderRemapController.cs:10`. Each file references no Swordfish type other than the using line;
  the namespace exposes no extension methods; removal is confirmed by rebuild.
- `TraceUtility.cs`: delete the two `_projectNames` filter literals
  `"UtilitiesSwordfish.NET.General"` (line 392) and `"UtilitiesSwordfish.NET.Test"` (line 393).
  They are assembly simple-names of the two Swordfish projects the epic deletes; once the projects
  are gone the entries are dead filter names that can never match. `List<string>.Contains`
  membership is per-element independent, so removal is behavior-neutral for every surviving name.

## Verification net

- `QuickFiler.Test/Controllers/KbdActionsTests.cs` and `KbdActionsRemainingBranchesTests.cs` pin the
  `FindIndex`/`Add`/`RemoveAt` branches and must stay green unchanged after the swap.
- Full C# toolchain (CSharpier -> .NET analyzers -> nullable -> MSTest) is the confirmation for the
  unused-using removals and the TraceUtility literal removal; no new unit tests are required for
  those behavior-neutral changes.
