# swordfish-raw-usage-cleanup (Issue #310)

- Date captured: 2026-07-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/ (Issue #310)

- Issue: #310
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/310
- Last Updated: 2026-07-10
- Work Mode: full-feature
- Epic: swordfish-removal (child F4, wave 0)
- Integration branch: epic/swordfish-removal-integration

## Problem / Why

The `swordfish-removal` epic eliminates the vendored `UtilitiesSwordfish` project and every
first-party dependency on `Swordfish.NET.*`. This child (F4) removes the first-party dependencies
on Swordfish that are either raw type usages or stale/unused artifacts, without touching the Sco*
lineage classes (owned by F1/F2/F3) or the project teardown (F5). Removing these usages shrinks
the set of source files that reference `Swordfish.NET.*`, which is a precondition for F5 removing
the `ProjectReference` and solution entries.

## Proposed Behavior

Three disjoint work items, all behavior-neutral:

1. **Raw collection type swap (KbdActions).** `QuickFiler/Controllers/KbdActions.cs` uses the raw
   Swordfish `ConcurrentObservableCollection<UClass>` for its private `_list` field (constructors
   at lines ~24, ~29 and the field at line ~32). Re-point this to the repository's clean,
   Swordfish-free `ConcurrentObservableCollection`. The public `KbdActions<TKey, UClass, VDelegate>`
   API and observed behavior must remain unchanged. The swapped-in type must preserve every member
   `KbdActions` relies on, including `FindIndex(Predicate<T>)`, `Add`, `RemoveAt`, the enumerator,
   and LINQ over the collection.

2. **Unused using removal.** Remove `using Swordfish.NET.Collections;` from three files that
   reference no Swordfish type other than the using directive itself:
   `QuickFiler/Controllers/KeyboardHandler.cs` (line ~17),
   `UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs` (line ~13), and
   `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs` (line ~10).
   Each removal must be confirmed genuinely unused by a rebuild.

3. **TraceUtility literal disposition.** `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs`
   (lines ~392-393) lists the string literals `"UtilitiesSwordfish.NET.General"` and
   `"UtilitiesSwordfish.NET.Test"` in the `_projectNames` trace/diagnostic filter. Because both
   projects are removed by the epic (F5), determine the correct disposition (update or remove) and
   apply it, recording the rationale.

## Acceptance Criteria

- [ ] AC1: `QuickFiler/Controllers/KbdActions.cs` `_list` is re-pointed to the repository's clean,
  Swordfish-free `ConcurrentObservableCollection`; the file no longer references
  `Swordfish.NET.Collections`; the public `KbdActions` API is unchanged; and all existing
  `KbdActions` tests pass.
- [ ] AC2: `using Swordfish.NET.Collections;` is removed from `KeyboardHandler.cs`,
  `FlagDetails.cs`, and `FolderRemapController.cs`, and the solution rebuilds clean (proving each
  using was genuinely unused).
- [ ] AC3: The stale `"UtilitiesSwordfish.NET.General"` / `"UtilitiesSwordfish.NET.Test"` literals
  in `TraceUtility.cs` are disposed of per the researched disposition, with the rationale recorded
  in the feature documents.
- [ ] AC4: No Sco* lineage class (`ScoDictionary`/`ScoCollection`/`ScoStack`/`ScoSortedDictionary`)
  or its consumers are modified beyond the `KbdActions` raw-collection swap; the `UtilitiesSwordfish`
  project, its `ProjectReference` entries, and `TaskMaster.sln` are untouched.
- [ ] AC5: The full C# toolchain (CSharpier, .NET analyzers, nullable, MSTest) passes, and
  changed/new code meets the repository coverage thresholds with no regression on changed lines.

## Constraints & Risks

- The clean `ConcurrentObservableCollection` target named by the epic manifest
  (`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`) does not currently exist on the
  integration base; only clean `Bag` and `Dictionary` types exist there. Resolving the correct swap
  target for AC1 (and any execution-order dependency on child F2, which re-bases `ScoCollection`
  onto the clean collection) is the primary research question for this child.
- The swap must be behavior-neutral. `KbdActions` relies on `FindIndex`, which is a
  Swordfish-collection member, not a standard `IList<T>` member; the clean target must provide an
  equivalent.

## Scope Boundary

Do NOT delete the `UtilitiesSwordfish` project, remove any `ProjectReference`, touch
`TaskMaster.sln`, migrate interfaces, or modify the Sco* lineage classes or their consumers beyond
the `KbdActions` raw-collection swap listed above.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create active feature folder from the template
- [ ] Research: resolve the clean-collection swap target and TraceUtility disposition
- [ ] Author spec.md and user-story.md
- [ ] Produce and preflight the atomic plan
