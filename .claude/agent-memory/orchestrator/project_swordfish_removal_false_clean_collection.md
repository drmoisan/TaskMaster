---
name: swordfish-removal-false-clean-collection
description: swordfish-removal epic manifest claims a clean ConcurrentObservableCollection "already in the repo" that does NOT exist on the integration base; affects F2 and F4 planning
metadata:
  type: project
---

The `swordfish-removal` epic manifest (`docs/features/epics/swordfish-removal/epic.md`,
"Shared Design — Swordfish-free replacement bases (already in the repo)") asserts a clean
`ConcurrentObservableCollection` exists at `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`.
Verified false on `origin/epic/swordfish-removal-integration`: that namespace folder has only `Bag`
and `Dictionary`; the only production `ConcurrentObservableCollection<T>` is the vendored Swordfish
type in `UtilitiesSwordfish/Collections/` (the type being removed). `ScoCollection<T>` inherits that
Swordfish type.

**Why:** Both F2 (collection/stack lineage) and F4 (raw-usage cleanup) reference "the clean
collection" as a precondition. F4 (#310) resolved this by swapping `KbdActions._list` to
`System.Collections.Generic.List<UClass>` (satisfies the load-bearing `FindIndex(Predicate<T>)` +
all used members, behavior-neutral, keeps F4 `depends_on: []` accurate). F2 still assumes a clean
`ConcurrentObservableCollection` to re-base `ScoCollection`/`ScoStack` onto — F2 must either CREATE
that clean base or its plan inherits the same false premise.

**How to apply:** When preparing/executing F2 or F5, or running the epic-orchestrator, do not assume
the clean `ConcurrentObservableCollection` exists — ground-truth it. The epic manifest "Shared
Design" note should be corrected (either point at the F2-introduced type or note KbdActions uses
`List<T>`). General lesson: ground-truth every epic-manifest "already in the repo" claim before
planning against it. See [[feedback_plan_phase0_paths_are_stale_in_epic_children]].
