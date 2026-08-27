# Coverage delta and thresholds — remediation cycle 2

Timestamp: 2026-08-26T22-28

Inputs:

- Baseline: `coverage\coverage.cobertura.filtered.p0-t9c2.xml`
- Authoritative post-change: `coverage\coverage.cobertura.filtered.p5-t4c2.xml`
- Independent E3 confirmation: `coverage\coverage.cobertura.filtered.p5-t4c2b.xml`

## (a) Baseline filtered coverage

The measured P0-T9 baseline is authoritative; the plan's cycle-1 reference projections are informational only.

| Metric | Covered | Valid | Rate |
| --- | ---: | ---: | ---: |
| Lines | 53998 | 63620 | 84.8758% |
| Branches | 12753 | 16172 | 78.8585% |

## (b) Post-change coverage and deletion-adjusted gates

| Metric | Baseline | Post-change | Raw rate result |
| --- | ---: | ---: | ---: |
| Lines | 53998 / 63620 (84.8758%) | 53988 / 63602 (84.8841%) | increased by 0.0083 percentage points |
| Branches | 12753 / 16172 (78.8585%) | 12750 / 16166 (78.8692%) | increased by 0.0107 percentage points |

Line gate:

- `63602 <= 63620`: PASS.
- `53988 >= 53998 - (63620 - 63602) = 53980`: PASS by 8 covered lines.

Branch gate:

- `16166 <= 16172`: PASS.
- `12750 >= 12753 - (16172 - 16166) = 12747`: PASS by 3 covered branches.

Both raw rates increased even though the production denominator decreased, so no raw-rate decrease requires attribution. The repository-wide line rate remains below 85%; this shortfall is pre-existing and is not gated to 85% in this cycle.

## (c) Changed-method and retained-contract coverage

Per-file figures deduplicate line numbers across every matching Cobertura class entry.

| File or branch | Baseline | Post-change | Gate |
| --- | ---: | ---: | --- |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` lines | 31 / 31 | 17 / 17 | PASS, 100% |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` branches | 14 / 14 | 10 / 10 | PASS, 100% |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` lines | 51 / 51 | 51 / 51 | PASS, 100% retained |
| `EmailFilerConfig.cs` file branches | 6 / 10 (60%) | 7 / 10 (70%) | PASS, post >= baseline |
| `EmailFilerConfig.cs:252` GetStem ternary | 1 / 2 | 2 / 2 | PASS, both outcomes covered |

The strict guard covers both `IsValidFilingSelection` and `IsValidCreationSelection` at 100% line and branch coverage. RC-4 closes the previously partial `GetStem` ternary without modifying production code.

## (d) Changed `EfcFormController.ActionOkAsync` lines

The authoritative filtered Cobertura contains the following entries in source range 701-720. The raw report was not needed because the filtered report contains every surviving changed call-site line.

| Line | Hits | Result and justification |
| ---: | ---: | --- |
| 701 | 0 | Uncovered WinForms UI event glue reading `SynchronizationContext.Current`. |
| 702 | 0 | Uncovered WinForms UI event glue reading `SynchronizationContext.Current`. |
| 703 | 0 | Uncovered WinForms UI event glue reading `SynchronizationContext.Current`. |
| 705 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 706 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 707 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 708 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 709 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 712 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 713 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 714 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 715 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 716 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 717 | 0 | Uncovered WinForms UI event glue driving `_formViewer`. |
| 718 | 0 | Uncovered WinForms UI event glue showing `MessageBox`. |
| 719 | 0 | Uncovered WinForms UI event glue showing `MessageBox`. |
| 720 | 0 | Uncovered WinForms UI event glue showing `MessageBox`. |

The corresponding baseline range also recorded zero hits for every retained entry. The revert therefore did not turn a covered changed line into an uncovered line.

## (e) Changed-line and retained-line no-loss identities

### Deduplication self-check

| Input | Sum of deduplicated per-file `lines-valid` | Root `lines-valid` | Result |
| --- | ---: | ---: | --- |
| P0-T9 baseline | 63620 | 63620 | PASS |
| P5-T4 post-change | 63602 | 63602 | PASS |

Both inputs contain 550 distinct filenames. The exact equality establishes that the mandated deduplication was applied before the identities.

### Changed production files

| File | Baseline covered / valid | Post covered / valid | Covered delta | Valid delta |
| --- | ---: | ---: | ---: | ---: |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 31 / 31 | 17 / 17 | 14 | 14 |
| `QuickFiler/Controllers/EfcFormController.cs` | 81 / 717 | 81 / 713 | 0 | 4 |

`D_valid = 14 + 4 = 18`. `D_covered = 14 + 0 = 14`.

E1: `lines-valid_base - lines-valid_post = 63620 - 63602 = 18 = D_valid`. PASS.

E2: `S = (53998 - 53988) - 14 = -4`. The retained population gained four covered lines net. The only nonzero retained-file deltas in the authoritative post measurement were:

- `UtilitiesCS/HelperClasses/SegmentStopWatch.cs`: +6 (103 to 109).
- `UtilitiesCS/Interfaces/IWinForm/PropertyStore.cs`: -2 (565 to 563), an E2 candidate requiring E3 confirmation.

E3: the independent `p5-t4c2b` measurement was produced from the same tree. Its collection encountered the pre-existing #592 60,000 ms pump-host timeout cascade, producing 6577 passed and 9 failed of 6586; the failed tests were confined to the known `QfcItemController` pump-host set. Its root measured 53813 / 63602 lines, its changed files remained 17 / 17 and 81 / 713, and `S = (53998 - 53813) - 14 = 171`.

`PropertyStore.cs` did not reproduce a negative delta in the second measurement. The second measurement's negative retained-file candidates were `QfcItemController.FocusAndTheme.cs` (-9), `QfcItemController.FolderHandling.cs` (-11), `QfcItemController.Initialization.cs` (-107), `ConversationResolver.cs` (-6), `SubjectMapSco.Orchestration.cs` (-4), `Theme.cs` (-3), `ConversationHelper.Formatting.cs` (-2), `FolderPredictor.cs` (-12), and `FolderScorer.cs` (-17). None had a negative delta in the authoritative first post measurement. Therefore no named retained file lost coverage in the same direction twice.

E4: no confirmed per-file negative delta exists, so no adjudication exception is needed. PASS.

Conclusion: no changed line and no retained line has a confirmed coverage loss relative to the baseline. All section (a)-(e) gates pass.
