---
name: swordfish-removal-epic-incidental-coverage-sideeffect
description: deleting a UtilitiesCS wrapper class that was the sole in-assembly consumer of a vendored Swordfish base type drops that base type's incidental coverage as measured by UtilitiesCS.Test; non-blocking, expected across F1/F2/F4/F5
metadata:
  type: project
---

During F3 (#309, `ScoSortedDictionary` removal) of the swordfish-removal epic, per-class
Cobertura diffing between baseline and post-change `UtilitiesCS.Test.dll` coverage runs
found 4 vendored `UtilitiesSwordfish/Collections/*.cs` classes (`BinarySorter`,
`ConcurrentObservableBase`, `ConcurrentObservableSortedDictionary`,
`DoubleLinkListIndexNode`) whose covered-line count DROPPED after deleting
`ScoSortedDictionary.cs` + its test, even though `UtilitiesSwordfish/**` itself was
untouched.

**Why:** `ScoSortedDictionary` was the only production consumer, reachable from
`UtilitiesCS.Test.dll`, of `ConcurrentObservableSortedDictionary` and its internal
collaborators. Its 23 deleted tests incidentally exercised those vendored base-class code
paths as a side effect of testing the wrapper. Removing the wrapper removes that incidental
exercise — the vendored code's OWN dedicated test project (`UtilitiesSwordfish.Test`) is
unaffected and still covers it directly.

**How to apply (for F1/F2/F4/F5 or any future Swordfish-wrapper deletion):**
- Expect this exact pattern: deleting a `ScoXxx`/`Sco*` wrapper class in `UtilitiesCS` will
  likely drop the incidental coverage of whichever `UtilitiesSwordfish/Collections/*.cs`
  base type(s) it derived from or delegated to, as measured by a `UtilitiesCS.Test.dll`-only
  coverage run.
- This is NOT a blocking regression: (1) it's confined to `UtilitiesSwordfish/**`, out of
  scope for every F1-F4 child (only F5 touches that project, and even then only its
  `ProjectReference`/`.sln` entries, not its source); (2) `UtilitiesSwordfish.Test` remains
  the authoritative, untouched, dedicated test suite for that vendored code; (3) it's a
  desired side effect of the epic's stated goal (shrinking the Swordfish-dependent surface).
- Do the per-class Cobertura diff anyway (script pattern: parse `<class filename=... ><lines><line hits=...>`
  from both baseline/post-change XML, compare `(package, filename, class)` keyed covered-line
  counts) — restrict the "must be zero regressions" check to the first-party package
  actually in scope (e.g. `UtilitiesCS` for F1-F4), and separately disclose (non-blocking)
  any `Swordfish.NET.General`-package deltas with this same root-cause explanation rather
  than omitting them from the coverage-delta evidence artifact.
- Related: [[project_dotnet_coverage_denominator_nondeterminism]], [[project_coverage_firstparty_denominator_method]].
