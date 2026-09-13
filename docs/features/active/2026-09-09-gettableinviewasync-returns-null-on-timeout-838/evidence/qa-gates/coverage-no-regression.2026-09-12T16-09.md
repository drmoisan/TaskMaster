# P4-T9 — No-regression comparison and the report-only repository-wide figures

Timestamp: 2026-09-13T03-18

Command: a single pwsh payload that reads the baseline Cobertura document at `Join-Path $env:TEMP "taskmaster-838\baseline-tests\coverage.cobertura.xml"` and the final one at `Join-Path $env:TEMP "taskmaster-838\final-tests\coverage.cobertura.xml"`, applies the fixed per-file aggregation rule to `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` in both, and reads each document's root `line-rate` attribute.

EXIT_CODE: 0

## Per-file no-regression comparison (blocking clause)

```
BASELINE_TA_COVERED=255
BASELINE_TA_TOTAL=281
FINAL_TA_COVERED=265
FINAL_TA_TOTAL=283
```

`FINAL_TA_COVERED` is 265 and `BASELINE_TA_COVERED` is 255, so the acceptance clause that the final covered count be greater than or equal to the baseline covered count holds with ten additional lines covered. Both sides matched nine class nodes for the file, so the two figures were produced by the identical aggregation and are comparable. The total rose from 281 to 283 because the change adds executable lines to the file; the covered count rose by more than the total did, so the file's coverage improved on both an absolute and a proportional reading.

## Denominator R (repository-wide, report-only)

```
BASELINE_R_LINE_RATE=0.7062982158637655
FINAL_R_LINE_RATE=0.706684498364101
```

These two figures are report-only observations and gate nothing. No merge-base coverage baseline exists for this feature, so a repository-wide blocking threshold cannot be shown satisfiable, and the plan therefore records the repository-wide figure rather than asserting a floor against it. The figures are stated here so that a reader can see the direction of travel: the repository-wide line rate moved from 0.7062982158637655 to 0.706684498364101, a small increase, which is the expected effect of adding covered production lines and five passing tests to a codebase of this size.

The two denominators are not interchangeable. Denominator R is the document-level line rate over all instrumented modules minus the seven module exclusions `coverage.config` carries and minus the test-assembly exclusion P0-T19 derived. Denominator C, recorded by P4-T8 at 100 percent, is the changed-and-added-line figure and is the only blocking coverage figure in this change. A figure computed one way is not comparable to a figure computed the other way.

Output Summary: the blocking no-regression clause holds, with the file under fix rising from 255 covered of 281 to 265 covered of 283. The two repository-wide figures are recorded as report-only, are labelled denominator R, and gate nothing, for the reason stated above.
