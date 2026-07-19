# Final Changed-Line Coverage Delta (P12-T5)

Timestamp: 2026-07-19T16-40

## Aggregate coverage (whole-assembly denominator via UtilitiesCS.Test + dotnet-coverage)
| Metric | Baseline (P0-T6) | Post-change (P12-T4) | Delta |
| --- | --- | --- | --- |
| Line coverage | 65.30% (67621/103562) | 65.31% (67653/103590) | +0.01% (+32 covered lines) |
| Branch coverage | 61.32% (15688/25584) | 61.35% (15693/25578) | +0.03% |

Overall coverage did not regress (it increased slightly).

## Changed-line coverage for the 63 remediated Folder/Store files
Method: `git diff -U0 dffadd5a..HEAD` added/modified line numbers per file, cross-referenced against the final
Cobertura `final-coverage.cobertura.xml` per-line `hits`.

- Changed executable lines (present in coverage instrumentation): **99**
- Covered: **96**
- **Changed-line coverage: 96.97%**

### The 3 uncovered changed lines (no regression — annotation-only edits to pre-existing statements)
1. `FolderScorer.cs` — `Prediction<string>[]? predictions = null;` (added `?` only; same statement as baseline
   `Prediction<string>[] predictions = null;`). In `AddBayesianSuggestionsAsync` (Bayesian async path not
   exercised by the unit suite). Uncovered at baseline, uncovered now.
2. `StoreWrapperController.cs` — `public void ExcludeStore_CheckedChanged(object? sender, EventArgs e) { }`
   (`object`->`object?` on an empty WinForms forwarder; not unit-tested). Uncovered before and after.
3. `StoreWrapperController.cs` — `ArchiveFS!.FolderPath = folderPath!;` (added `!` only; same assignment as
   baseline `ArchiveFS.FolderPath = folderPath;`) in the `ArchiveFS_Click` WinForms UI handler (not unit-tested).
   Uncovered before and after.

All three are annotation-only modifications (`?`/`!`) to pre-existing statements whose execution is unchanged;
an annotation cannot convert a covered line to uncovered. No new executable logic was introduced into a covered
path. The guard refinements added during remediation (e.g., FolderWrapperNodeComparer's explicit null guard,
FolderTreeSnapshot/Queries `|| ... is null` guards) all land in covered lines (per-file 100% for those files).

**Conclusion: no coverage regression on changed lines (AC4).** COM/VSTO/WinForms coverage-exempt files were
annotated without new tests and without new runtime guard statements, consistent with the CLAUDE.md exemption.
