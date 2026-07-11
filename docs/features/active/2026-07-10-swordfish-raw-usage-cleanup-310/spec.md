# swordfish-raw-usage-cleanup — Spec

- **Issue:** #310
- **Parent (optional):** epic `swordfish-removal`, child F4, wave 0
- **Owner:** drmoisan
- **Last Updated:** 2026-07-10T20-30
- **Status:** Ready for planning
- **Version:** 0.2

## Overview

The `swordfish-removal` epic eliminates the vendored `UtilitiesSwordfish` project and every
first-party dependency on `Swordfish.NET.*`. This child (F4) removes the first-party dependencies
on Swordfish that are either raw type usages or stale/unused artifacts, without touching the Sco*
lineage classes (owned by F1/F2/F3) or the project teardown (F5). Removing these usages shrinks the
set of source files that reference `Swordfish.NET.*`, which is a precondition for F5 removing the
`ProjectReference` entries and solution entries.

This feature is document-and-plan preparation work followed by execution of three disjoint,
behavior-neutral work items. No behavior change, UX change, or new production dependency is in
scope.

## Scope

### In scope

1. Re-point `QuickFiler/Controllers/KbdActions.cs` private field `_list` away from the raw
   Swordfish `ConcurrentObservableCollection<UClass>` to `System.Collections.Generic.List<UClass>`
   (see Research Resolution below).
2. Remove `using Swordfish.NET.Collections;` from three files that reference no other Swordfish
   type: `QuickFiler/Controllers/KeyboardHandler.cs` (line ~17),
   `UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs` (line ~13), and
   `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs` (line ~10).
3. Delete the two stale `_projectNames` trace-filter literals `"UtilitiesSwordfish.NET.General"`
   (line ~392) and `"UtilitiesSwordfish.NET.Test"` (line ~393) in
   `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs`.

### Out of scope

- Any Sco* lineage class (`ScoDictionary`, `ScoCollection`, `ScoStack`, `ScoSortedDictionary`) or
  its consumers, beyond the `KbdActions` raw-collection swap in item 1 above. These are owned by
  epic children F1, F2, and F3.
- The `UtilitiesSwordfish` project, any `ProjectReference` entry, and `TaskMaster.sln`. These are
  owned by epic child F5.
- Interface migration (`IScoCollection`, `IScoCollection2`).
- Any new production dependency or new Swordfish-free general-purpose collection type.

## Behavioral Specification

### Work item 1 — Raw collection type swap (KbdActions)

`KbdActions<TKey, UClass, VDelegate>` currently declares its private `_list` field, and the two
constructors that initialize it (parameterless and `IEnumerable<UClass>`-accepting), using the raw
Swordfish `ConcurrentObservableCollection<UClass>`. The field is re-typed to
`System.Collections.Generic.List<UClass>`. All call sites that use `_list` — `Add`,
`RemoveAt(int)`, `GetEnumerator`, LINQ operators, and `FindIndex(Predicate<UClass>)` (used at the
two call sites that locate an entry by predicate) — continue to compile and behave identically
because `List<UClass>` provides every one of these members natively. The `using
Swordfish.NET.Collections;` directive in this file is removed once no Swordfish type remains
referenced. The public `KbdActions<TKey, UClass, VDelegate>` API surface (constructor signatures,
public method signatures, and observable behavior as exercised by callers) is unchanged; `_list` is
`private` and is never exposed as a Swordfish-typed member.

### Work item 2 — Unused using removal

Three files reference `Swordfish.NET.Collections` only through an otherwise-unused `using`
directive:

- `QuickFiler/Controllers/KeyboardHandler.cs` (line ~17)
- `UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs` (line ~13)
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs` (line ~10)

Each `using Swordfish.NET.Collections;` line is deleted. No other line in these files changes. A
full rebuild after each removal confirms the file compiles clean with no unresolved-type error,
which is the proof that the directive was genuinely unused (the namespace exposes no extension
methods, so no silent behavior loss is possible from removing it).

### Work item 3 — TraceUtility literal disposition

`UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` lists the string literals
`"UtilitiesSwordfish.NET.General"` (line ~392) and `"UtilitiesSwordfish.NET.Test"` (line ~393) in
the `_projectNames` trace/diagnostic filter list. Disposition: **delete both literals**. Rationale:
both strings are the assembly simple-names of the two Swordfish projects the epic deletes in F5;
once those projects no longer exist in the solution, the entries are dead filter names that can
never match anything at runtime. `List<string>.Contains`-style membership checks against
`_projectNames` are per-element independent, so removing these two entries is behavior-neutral for
every surviving project name in the list — no other filter behavior changes.

## Research Resolution

The epic manifest's "Shared Design" section names a clean, Swordfish-free
`ConcurrentObservableCollection` "already in the repo" at
`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*` as the intended swap target for work item
1. This type **does not exist** on the integration base (`epic/swordfish-removal-integration`):
that namespace folder contains only clean `Bag` and `Dictionary` types; the only production
`ConcurrentObservableCollection<T>` present is the vendored Swordfish type in
`UtilitiesSwordfish/Collections/ConcurrentObservableCollection.cs`, which is the type this epic
removes. This spec does not describe the swap as targeting the named clean
`ConcurrentObservableCollection`, because doing so would describe a swap onto a type that is not on
the base.

The resolved swap target, per
`docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/research/swap-target-decision-record.md`,
is `System.Collections.Generic.List<UClass>`. This target was selected because it satisfies every
member `KbdActions` relies on — the two constructors, `Add`, `RemoveAt(int)`, `GetEnumerator`, LINQ,
and the load-bearing `FindIndex(Predicate<UClass>)` — with zero new code and no shim, and because
`KbdActions` uses no observable/concurrent surface of the Swordfish type (`_list` is `private`, the
class exposes no `CollectionChanged` event, and no cross-thread mutation of `_list` occurs; all
registration is setup-time). The swap is therefore behavior-neutral. `List<T>` is already in scope
via the file's existing `using System.Collections.Generic;` directive.

The decision record also identifies and rejects two alternatives: (a) depending on epic child F2 to
introduce a clean `ConcurrentObservableCollection` with `FindIndex(Predicate<T>)` first, which was
rejected because the type does not exist yet, F2's charter is re-basing `ScoCollection`/`ScoStack`
rather than committing to a new general-purpose concurrent-observable collection, and it would
introduce a real execution-order dependency contradicting F4's manifest `depends_on: []` and wave-0
placement; and (c) F4 authoring the clean type itself, rejected as C3-scale work exceeding F4's C2
charter and risking duplication of a type F2 should own.

### Epic-level finding (F4 -> F2)

Under the resolved decision, F4 remains independent (wave 0, `depends_on: []` accurate) and
Swordfish-free with no cross-feature dependency. **The epic manifest's "Shared Design" note is
inaccurate as written and should be corrected by the epic-orchestrator**: either point it at a
clean concurrent-observable type F2 introduces, or record that `KbdActions` consolidates on
`List<T>` because it uses no observable/concurrent surface. An override path exists: if the
maintainer instead requires `KbdActions` to consolidate on the epic's clean concurrent-observable
base, alternative (a) above should be adopted, and F4's `depends_on` must then be corrected to
include F2, moving F4 to wave 1. This spec assumes the resolved decision (`List<UClass>`) unless
and until the maintainer or epic-orchestrator exercises the override path.

## Constraints & Risks

- The swap must remain behavior-neutral; `KbdActions` relies on `FindIndex`, a Swordfish-collection
  member not present on standard `IList<T>`/`Collection<T>`/`ObservableCollection<T>` — `List<T>`
  is the only BCL collection that provides it natively, closing this risk with zero new code.
- Scope creep risk: this child must not touch Sco* lineage classes/consumers beyond the `KbdActions`
  swap, the `UtilitiesSwordfish` project, any `ProjectReference`, or `TaskMaster.sln`. These are
  explicitly owned by other epic children (F1/F2/F3, F5).
- Coordination risk: if the epic-orchestrator or maintainer later exercises the override path
  described above, F4's dependency graph and wave placement change, and this spec's Research
  Resolution section would need to be revised accordingly.

## Implementation Strategy

- Implementation scope: three disjoint, behavior-neutral source edits — one field re-typing (plus
  its constructors) in `KbdActions.cs`, three `using`-directive deletions, and a two-literal
  deletion in `TraceUtility.cs`. No new classes, functions, or commands are introduced.
- Dependency changes: none. No new package or project reference is added; `using
  Swordfish.NET.Collections;` is removed from four files total (`KbdActions.cs` plus the three
  unused-using files) as a result of work items 1 and 2.
- Logging/telemetry: none added. Work item 3 removes two dead entries from an existing trace
  filter; no new logging is introduced.
- Rollout plan: no feature flag or staged deploy is applicable. This is an internal refactor with
  no externally observable behavior change; rollout is a normal merge to the epic integration
  branch after the toolchain and regression net pass.

## Testing & Verification

- The existing `KbdActions` regression tests (`QuickFiler.Test/Controllers/KbdActionsTests.cs` and
  `KbdActionsRemainingBranchesTests.cs`) already pin the `FindIndex`/`Add`/`RemoveAt` branches
  exercised by work item 1. These tests are the verification net for the collection swap; they must
  pass unchanged (no test edits) after the swap, which itself confirms the swap did not alter
  observable behavior.
- No new unit tests are required for any of the three work items. Work items 1, 2, and 3 are all
  behavior-neutral: item 1 is covered by the existing `KbdActions` tests noted above; items 2 and 3
  are proven by a full solution rebuild succeeding with no unresolved-reference or unused-directive
  regression.
- The full C# toolchain (CSharpier formatting, .NET analyzers, nullable-reference-type analysis,
  MSTest) must pass in the required order — format, analyze, type-check, test — for every changed
  file. Changed/new code must meet the repository coverage thresholds with no regression on changed
  lines, per the General Unit Test Policy and C# Unit Test Policy in `CLAUDE.md`.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Existing `KbdActions` tests pass unchanged (regression net for work item 1)
- [ ] Edge cases and error handling covered by existing tests (no new edge cases introduced)
- [ ] Docs updated (this spec, user-story.md, decision record cross-referenced)
- [ ] Toolchain pass completed (CSharpier -> .NET analyzers -> nullable -> MSTest)

## Acceptance Criteria

- [ ] AC1: `QuickFiler/Controllers/KbdActions.cs` `_list` is re-pointed to
  `System.Collections.Generic.List<UClass>`; the file no longer references
  `Swordfish.NET.Collections`; the public `KbdActions` API is unchanged; and all existing
  `KbdActions` tests pass.
- [ ] AC2: `using Swordfish.NET.Collections;` is removed from `KeyboardHandler.cs`,
  `FlagDetails.cs`, and `FolderRemapController.cs`, and the solution rebuilds clean (proving each
  using was genuinely unused).
- [ ] AC3: The stale `"UtilitiesSwordfish.NET.General"` / `"UtilitiesSwordfish.NET.Test"` literals
  in `TraceUtility.cs` are deleted per the disposition recorded in the Research Resolution section
  above.
- [ ] AC4: No Sco* lineage class (`ScoDictionary`/`ScoCollection`/`ScoStack`/`ScoSortedDictionary`)
  or its consumers are modified beyond the `KbdActions` raw-collection swap; the `UtilitiesSwordfish`
  project, its `ProjectReference` entries, and `TaskMaster.sln` are untouched.
- [ ] AC5: The full C# toolchain (CSharpier, .NET analyzers, nullable, MSTest) passes, and
  changed/new code meets the repository coverage thresholds with no regression on changed lines.
