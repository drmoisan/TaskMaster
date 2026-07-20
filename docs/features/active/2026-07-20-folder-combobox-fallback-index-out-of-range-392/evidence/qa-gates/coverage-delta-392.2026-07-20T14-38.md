Timestamp: 2026-07-20T14-38

## Coverage delta: baseline (P0-T12) vs. post-change (P2-T4), scoped to `QfcItemController.FolderHandling.cs`

| Scope | Baseline line-rate | Post-change line-rate | Baseline branch-rate | Post-change branch-rate |
|---|---|---|---|---|
| `QuickFiler` package (whole assembly) | 73.67% | 73.68% | 64.53% | 64.62% |
| `QfcItemController.FolderHandling.cs` (class, file-scoped) | 91.55% | 91.89% | 71.05% | 73.81% |
| `AssignFolderComboBox()` method | 88.46% | 89.29% | 85.71% | 87.5% |
| `PopulateAndSelectFolder(...)` method | 100% | 100% | 100% | 100% |

**No regression on any of the above rows** — every post-change figure is greater than or equal to
its corresponding baseline figure.

## New/changed-code coverage (the two edited statements, per AC-5's >= 90% target)

Changed lines (per `git diff`, P1-T5 and P1-T6):
- `QfcItemController.FolderHandling.cs:201-205` — the `else` branch of `AssignFolderComboBox()`,
  containing the new `_folderHandler.FolderArray.Length == 1 ? 0 : 1` conditional
  (source spans 202-204; the compiler emits reported Cobertura sequence points on lines 201, 203,
  204, 205 for this block — line 202 alone, an incomplete partial expression line, has no
  independent sequence point).
- `QfcItemController.FolderHandling.cs:230-231` — the new
  `predeterminedIndex >= 0 ? predeterminedIndex : (folderArray.Length == 1 ? 0 : 1)` conditional
  assignment (the compiler emits the sequence point on line 231; line 230 alone has no independent
  sequence point).

Post-change Cobertura per-line data for these reported sequence points (from
`final-coverage.cobertura.xml`):

```
<line number="201" hits="1" branch="False" />
<line number="203" hits="1" branch="False" />
<line number="204" hits="1" branch="False" />
<line number="205" hits="1" branch="False" />
<line number="231" hits="1" branch="False" />
```

All 5 reported lines show `hits="1"` — **100% line coverage on the new/changed code**, exceeding the
>= 90% target from AC-5.

Each ternary's two logical paths (`FolderArray.Length == 1` vs. `> 1`) are independently exercised by
tests (this Cobertura conversion does not expose branch-level granularity on these specific lines,
so line-hit evidence is supplemented with the specific tests that drive each path):
- `Length == 1` path: `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing`
  and `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero` (both new,
  P1-T2/P1-T3).
- `Length > 1` (else) path: `PopulateAndSelectFolder_AllMissingPredetermined_SelectsIndexOne` and
  `AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer` (both pre-existing,
  unchanged).

## Explicit PASS/FAIL Statement

- No regression on changed lines: **PASS**.
- >= 90% coverage on new/changed code: **PASS** (100% observed).
