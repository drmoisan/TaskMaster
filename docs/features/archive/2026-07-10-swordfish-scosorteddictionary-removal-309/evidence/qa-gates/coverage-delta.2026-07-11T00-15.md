# Phase 2 — Coverage Delta: Baseline vs. Post-Change vs. Changed-Code (P2-T5)

- Timestamp: 2026-07-11T00-25

## (a) P0-T5 Baseline Figures

- `UtilitiesCS.dll` module line-coverage: **88.19%** (`line-rate="0.881887026691496"`)
- Overall line-coverage for the run: **60.65%** (`line-rate="0.6064725675584235"`, lines-covered=98590, lines-valid=162563)

Source: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/baseline/baseline-coverage.cobertura.xml`

## (b) P2-T4 Post-Change Figures

- `UtilitiesCS.dll` module line-coverage: **88.23%** (`line-rate="0.8822898745854838"`)
- Overall line-coverage for the run: **60.54%** (`line-rate="0.6054059721005958"`, lines-covered=98169, lines-valid=162154)

Source: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/qc-coverage.2026-07-11T00-15.cobertura.xml`

## (c) Changed-Code Assessment (per-class comparison, not just aggregate percentages)

A per-`<class>` line-hit comparison was run across both Cobertura XML files (script:
session scratchpad `compare_coverage_all.py`, parses every `<class filename=".../lines/line hits=...">`
in both baseline and post-change XML and diffs covered-line counts by `(package, filename, class)`
key). This is the correct methodology per project memory
`project_dotnet_coverage_denominator_nondeterminism`: a single repo-wide aggregate
percentage is not trustworthy for a no-regression proof on its own because
`dotnet-coverage`'s cross-assembly merge denominator can shift run-to-run; per-class line
counts are the stable, trustworthy unit of comparison.

**`UtilitiesCS` package (the first-party production module in scope for this change),
excluding the deleted `ScoSortedDictionary.cs` itself:**
- 1276 classes present at baseline; all 1275 remaining classes (excluding `ScoSortedDictionary`'s
  own 1 class) were compared.
- **Zero regressions**: no `UtilitiesCS` class's covered-line count decreased.
- Aggregate baseline (excl. `ScoSortedDictionary.cs`): covered=35382, valid=40107 -> 88.22%
- Aggregate post-change: covered=35386, valid=40107 -> 88.23%
- The deleted `ScoSortedDictionary.cs` itself contributed 136 covered / 168 valid lines at
  baseline; both numbers are removed entirely from the denominator post-change (the file no
  longer exists), which is the expected and correct effect of a deletion-only change.

**All packages, repo-wide, excluding `ScoSortedDictionary`-named classes/files:**
- 8920 total classes compared. 15 classes present only in baseline were classes belonging to
  the two deleted files (`ScoSortedDictionary.cs` — 2 compiler-generated classes in
  `UtilitiesCS`; `ScoSortedDictionary_Tests.cs` — 13 compiler-generated/test classes in
  `UtilitiesCS.Test`), confirming the deletion is complete and produces no orphaned coverage
  entries.
- **4 regressions found, all confined to the vendored, out-of-scope `Swordfish.NET.General`
  package** (`UtilitiesSwordfish/**`, explicitly excluded from this plan's Scope Lock and
  never modified by this change):
  - `UtilitiesSwordfish/Collections/BinarySorter.cs` (`BinarySorter<TKey>`): 31 -> 0 covered lines
  - `UtilitiesSwordfish/Collections/ConcurrentObservableBase.cs` (`ConcurrentObservableBase<T>`): 153 -> 152 covered lines
  - `UtilitiesSwordfish/Collections/ConcurrentObservableSortedDictionary.cs` (`ConcurrentObservableSortedDictionary<TKey, TValue>`): 25 -> 0 covered lines
  - `UtilitiesSwordfish/Collections/DoubleLinkListIndexNode.cs` (`DoubleLinkListIndexNode`): 53 -> 25 covered lines

  **Root cause and disposition:** `ScoSortedDictionary` was the sole production consumer,
  within the `UtilitiesCS`/`UtilitiesCS.Test` assemblies, of the vendored
  `ConcurrentObservableSortedDictionary` base class and its internal collaborators
  (`BinarySorter`, `DoubleLinkListIndexNode`, shared `ConcurrentObservableBase` code paths).
  `ScoSortedDictionary_Tests.cs`'s 23 tests incidentally exercised those vendored base-class
  code paths as a side effect of testing `ScoSortedDictionary`. Removing both files removes
  that incidental exercise, so this `UtilitiesCS.Test.dll`-scoped coverage run no longer hits
  those lines in the vendored Swordfish assembly.

  This is **not a regression in dedicated test coverage** for the affected Swordfish types:
  `UtilitiesSwordfish/**` has its own dedicated test project, `UtilitiesSwordfish.Test`
  (confirmed present, e.g. `UtilitiesSwordfish.Test/ObservableSortedDictionaryTest.xaml.cs`
  directly instantiates and tests `ConcurrentObservableSortedDictionary`), which is untouched
  and out of scope for this plan and was not included in this evidence run (the plan's P0-T5/
  P2-T4 tasks scope test execution to `UtilitiesCS.Test.dll` only, per their literal command
  text). No production behavior of any Swordfish type changed — only the incidental,
  cross-assembly coverage measurement captured by this particular test run shifted, as an
  expected side effect of removing the only production consumer that indirectly exercised it
  from inside `UtilitiesCS.Test`.

  This finding is disclosed here for auditability but is classified as **non-blocking**
  because: (1) it is confined entirely to vendored, out-of-scope files this plan is
  explicitly prohibited from modifying (Scope Lock); (2) it reflects a change in incidental
  cross-assembly test exercise, not a removal of any dedicated test or any change to
  production behavior; (3) it is a direct, expected, and desired consequence of the epic's
  stated goal (`user-story.md`: "the analyzer-exempt Swordfish-dependent surface shrinks");
  (4) the first-party, testable-denominator module in scope (`UtilitiesCS.dll`) shows zero
  regressions, confirmed above.

## Conclusion

**No coverage regression on remaining first-party (in-scope) lines.** The `UtilitiesCS.dll`
module — the only production module touched by this change — shows zero per-class
regressions and a stable-to-slightly-improved aggregate rate (88.22% -> 88.23%, excluding
the deleted file). A disclosed, non-blocking, out-of-scope side effect exists in vendored
`UtilitiesSwordfish` coverage as measured incidentally by the `UtilitiesCS.Test.dll` run
(see (c) above); it does not represent a BLOCKING finding under this plan's Scope Lock and
does not affect any file this plan is authorized to touch.
