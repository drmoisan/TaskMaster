# Baseline Line Count — StoresWrapperTests.cs (Pre-Split)

- Timestamp: 2026-07-08T00-08
- Command (as specified by plan P0-T5): `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs' | Measure-Object -Line).Lines`
- EXIT_CODE: 0
- Output Summary: The plan-specified `Measure-Object -Line` command returned **599**, not the
  expected 688. Root cause: PowerShell's `Measure-Object -Line` counts each pipeline string as a
  text block using newline-detection semantics, and an empty string (blank line) yields a 0-line
  count rather than 1. The file contains 89 blank lines (confirmed via
  `(Get-Content ... | Where-Object { $_ -eq '' }).Count` = 89), and 688 - 89 = 599, exactly
  matching the discrepancy. This is a known `Measure-Object -Line` quirk with `Get-Content`
  string-array input, not a file-content issue.

## Deviation and Corrected Command

- Deviation: substituted `(Get-Content <path>).Count` for the plan-specified
  `(Get-Content <path> | Measure-Object -Line).Lines` because the latter undercounts blank lines.
  This is a mechanical tooling correction, not a change in task intent — the acceptance criterion
  is the accurate current line count of the file, and `.Count` and `wc -l` (git-bash) both agree.
- Corrected command: `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs').Count`
- Corrected EXIT_CODE: 0
- Corrected result: **688** (matches plan's expected value and cross-checked with `wc -l` = 688).

Applying this same corrected command for line-count verification is used consistently across
P0-T5, P0-T6, P1-T7, P1-T8, and P2-T7 for this reason.
