# [P2-T10] Production PowerShell file sizes

Timestamp: 2026-08-11T01-24
Command: `wc -l scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
EXIT_CODE: 0

| File | Lines | Ceiling | Strictly below 500 | Headroom |
|---|---|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` (new) | **387** | 500 | yes | 113 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (2 edits) | **457** | 500 | yes | 43 |

Ceiling source: `.claude/rules/general-code-change.md` § File Size Limit and
`.claude/rules/powershell.md` § Coding Standards.

`Invoke-MSTestWithCoverage.Helpers.ps1` moved from 455 lines (the `[P0-T5]` post-#441 baseline) to
457 — exactly the two added lines of `[P2-T8]` and `[P2-T9]`, with no removed lines.

The `[P0-T5]` measurement predicted this outcome: 455 lines with 45 lines of headroom, versus roughly
110 lines of new filter logic, is why the separate-file decision in `spec.md` § Implementation
strategy stands. The new file's actual size is 387 lines, which would have taken the helpers module
to 842 lines had it been added there.

This check runs again after the final formatting pass as `[P3-T10]`, because formatting can change
line counts.

## Output Summary

Both production PowerShell files are strictly below the 500-line ceiling: 387 and 457 lines.
