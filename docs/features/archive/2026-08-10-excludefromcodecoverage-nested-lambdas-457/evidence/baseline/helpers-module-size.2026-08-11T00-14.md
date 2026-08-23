# [P0-T5] Post-#441 size of the helpers module

Timestamp: 2026-08-11T00-14
Command: `wc -l scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
EXIT_CODE: 0

## Measurement

| File | Lines | Ceiling | Headroom |
|---|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 455 | 500 | 45 |

Ceiling source: `.claude/rules/general-code-change.md` § File Size Limit ("No production code, test
code, or reusable script file may exceed 500 lines") and `.claude/rules/powershell.md` § Coding
Standards ("Keep scripts cohesive and under 500 lines").

## Bearing on the research §8.7 new-file decision

Research §8.7 and `spec.md` § Implementation strategy both record the pre-#441 size as 357 lines and
require the actual post-#441 size to be confirmed before the new-file decision is revisited. The
measured post-#441 size is 455 lines: #441 added 98 lines, and the remaining headroom is 45 lines.
The research sketch estimates roughly 110 lines of new filter logic. 110 > 45, so the new-file
decision recorded in `spec.md` § Implementation strategy stands and is confirmed by measurement, not
assumed.

The two permitted edits fixed by `[P2-T8]` and `[P2-T9]` add exactly two lines, taking the file to
457 lines and leaving 43 lines of headroom. `[P2-T10]` and `[P3-T10]` re-measure this file, the new
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, and every test file after the final
formatting pass, because formatting can change line counts.

Related measurements recorded for the same ceiling:

| File | Lines at baseline | Headroom |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (not modified by this feature) | 348 | 152 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 468 | 32 |

The 32-line headroom on the helpers test file constrains regression case 6 (`[P1-T10]`), which is
added to that file. `[P1-T12]`'s pre-authorized split applies to
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` only; the plan authorizes no
split of the helpers test file, so case 6 must be authored within the available headroom.

## Output Summary

`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is 455 lines post-#441, 45 lines below the
500-line ceiling. The separate-file decision for the closure filter is confirmed by measurement.
