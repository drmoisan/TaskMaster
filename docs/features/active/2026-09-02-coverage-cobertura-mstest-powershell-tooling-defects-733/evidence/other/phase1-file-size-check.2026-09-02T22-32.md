# Phase 1 file-size check and required extractions (P1-T14)

Timestamp: 2026-09-02T22-32

Task: [P1-T14]

Ceiling: 500 lines per file, per the File Size Limit section of
.claude/rules/general-code-change.md and the Coding Standards section of
.claude/rules/powershell.md.

## Measurement method

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script, reading each file with `[System.IO.File]::ReadAllText` and counting newline characters
with `[regex]::Matches`. Every file ends with a trailing newline, so the newline count equals
the number of content lines. This is the same counting convention the P0-T4 baseline used, so
the figures below are directly comparable with it.

EXIT_CODE: 0

## First measurement — before extraction

| File | Lines | Headroom | Verdict |
|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 502 | -2 | OVER |
| scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | 65 | 435 | OK |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 566 | -66 | OVER |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | 70 | 430 | OK |

Two of the four files exceeded the 500-line ceiling, so this task's acceptance was not met by
the first measurement and P1-T14's prescribed remedy was applied.

### Why each file went over

- Invoke-MSTestWithCoverage.Helpers.ps1 entered Phase 1 at 491 lines (P0-T4 baseline, 9 lines of
  headroom). Phase 1 added a net 11 lines to it: +1 dot-source (P1-T9), -6 from the
  Get-CoberturaCoverageSummary refactor (P1-T10), +10 for the methods union-append loop with its
  comment (P1-T11), and +6 for the package-rate recomputation with its comment (P1-T12). 491 + 11
  = 502.
- Invoke-MSTestWithCoverage.Helpers.Tests.ps1 entered Phase 1 at 498 lines (P0-T4 baseline, 2
  lines of headroom). Phase 1 added 68 lines to it: +4 for P1-T3's two package-rate assertions
  and their comment, and +64 for the new `Describe 'Merge-CoberturaClassesByFilename'` block
  holding P1-T5's and P1-T6's It cases. P1-T4 changed lines in place and added none. 498 + 68 =
  566.

## Extractions applied

P1-T14's acceptance text prescribes extracting the most recently added self-contained block in
an over-ceiling file — a Describe block, or a single function with its doc comment — into a
further sibling file, recording the extraction here, and recounting.

### Extraction 1 (test file, prescribed target)

The most recently added self-contained block in
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 is the
`Describe 'Merge-CoberturaClassesByFilename'` block added by P1-T5 and extended by P1-T6. It was
moved verbatim, with no assertion or comment change, into the new sibling file
tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1, which carries the same
`Set-StrictMode -Version Latest` and `BeforeAll` dot-source header as its source file. This
removed 64 lines (the block plus its separating blank line), leaving the source file at 502
lines — still 2 over the ceiling.

### Extraction 2 (test file, required to reach the ceiling)

Extraction 1 alone did not satisfy the acceptance clause "the recount confirms all files are at
or under 500 lines", so a second extraction was mechanically necessary. The next self-contained
block, `Describe 'Assert-CoberturaLineCoverageThreshold'` (5 single-line It cases exercising a
function unrelated to the merge path), was moved verbatim into the new sibling file
tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1, with the same header. This
removed a further 8 lines, bringing the source file to 494.

### Extraction 3 (production file)

No new function was added to scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 by Phase 1;
its growth came from inline additions inside two existing functions. The prescribed remedy's
alternative unit therefore applies: "a single function with its doc comment". The function
`Assert-CoberturaLineCoverageThreshold` was moved verbatim into the new sibling file
scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1, and Helpers.ps1 gained one dot-source
line for it, mirroring the existing dot-sources of Invoke-MSTestWithCoverage.ClosureFilter.ps1
and Invoke-MSTestWithCoverage.PackageRate.ps1. Comment-based help was added to the extracted
function; its body, parameter, and every throw message are unchanged.

`Assert-CoberturaLineCoverageThreshold` was chosen because it is the only function in the file
with no caller inside the file and no dependency on any other function in it, so the move is a
pure relocation. Its one production caller, scripts/vscode/Invoke-MSTestWithCoverage.ps1, and
the `Mock Assert-CoberturaLineCoverageThreshold` in
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 both resolve it through the Helpers.ps1
dot-source chain, and both were re-verified by the confirming test run below.

Extraction 3 pairs with extraction 2: the extracted function and its extracted tests land in a
matched pair of sibling files with corresponding names.

## Second measurement — after extraction (recount)

| File | Lines | Headroom | Verdict |
|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 469 | 31 | OK |
| scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | 65 | 435 | OK |
| scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | 56 | 444 | OK |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 494 | 6 | OK |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | 70 | 430 | OK |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | 71 | 429 | OK |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | 15 | 485 | OK |

Every file is at or under 500 lines. The four files this task is required to measure are the
first, second, fourth and fifth rows; the remaining three rows are the sibling files the
extractions created and are recorded here so the recount covers the complete post-extraction
Phase 1 file set.

## Confirming test run after the extractions

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode (the whole folder, so every test file that could be affected by moving a
production function is exercised, not only the Phase 1 scope), `Run.PassThru = $true`,
`Output.Verbosity = "Normal"`, then the explicit trailing branch
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

Counts: Passed 74, Failed 0, Skipped 0, Total 74, across 8 discovered test files.

Reconciliation against the P0-T7 baseline of 70 passed / 0 failed / 0 skipped over the same
folder: 70 + 4 = 74, the four additions being P1-T1, P1-T2, P1-T5 and P1-T6. P1-T3 and P1-T4
changed existing cases in place and add no count; the three extractions moved existing cases
between files and add no count. No test regressed, and in particular
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1, which mocks the relocated
`Assert-CoberturaLineCoverageThreshold`, passed in full.

## Impact on the plan's write set

The three sibling files created by these extractions are new paths not enumerated in the plan's
Conventions write set. They are authorized by P1-T14's own acceptance text, which directs the
executor to extract into "a further sibling file" when the ceiling is exceeded, and all three sit
under scripts/vscode/ or tests/scripts/vscode/, so they remain inside the plan's Scope
Prohibitions boundary and inside the three allowed prefixes P5-T9 checks. Later phases that
enumerate the write set should include them:

- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 — add to P5-T4's CodeCoverage.Path and
  to P5-T5's per-file coverage comparison.
- tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 — add to P5-T4's Run.Path.
- tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 — add to P5-T4's Run.Path.

## Output Summary

The first measurement found two of the four required files over the 500-line ceiling:
Invoke-MSTestWithCoverage.Helpers.ps1 at 502 and Invoke-MSTestWithCoverage.Helpers.Tests.ps1 at
566. Three extractions were applied and recorded above: the most recently added Describe block
and one further Describe block out of the test file, and one whole function with its doc comment
out of the production file, each into a new sibling file. The recount confirms all seven Phase 1
files are at or under 500 lines, the smallest remaining headroom being 6 lines on
Invoke-MSTestWithCoverage.Helpers.Tests.ps1. A confirming whole-folder Pester run returned
EXIT_CODE 0 with 74 passed, 0 failed, 0 skipped, reconciling exactly with the P0-T7 baseline of
70 plus Phase 1's four new It cases.
