# Phase 3 file-size check (P3-T5)

Timestamp: 2026-09-02T22-43

Task: [P3-T5]

## Command

Command: pwsh -NoProfile -Command reading each file with `Get-Content -LiteralPath` and reporting
`.Count`. This is the same physical-line idiom the P0-T4 baseline used. `Measure-Object -Line` is
deliberately not used: it omits blank lines and therefore under-reports a file-size audit against
the 500-line ceiling.

EXIT_CODE: 0

## Measurements

| File | Lines | Ceiling | Headroom | Verdict |
|---|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 413 | 500 | 87 | at or under |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 486 | 500 | 14 | at or under |

Change from the P0-T4 baseline: the production file grew from 389 to 413 lines (+24, the two
comment-based-help addenda added by P3-T1 and P3-T2), and the test file grew from 443 to 486 lines
(+43, the single It added by P3-T3).

## Output Summary

Both Phase 3 files are at or under the 500-line ceiling in .claude/rules/general-code-change.md
and .claude/rules/powershell.md. No extraction was required. The test file's remaining headroom is
14 lines, which is noted here because Phase 3 makes no further additions to it; no later phase in
this plan writes to either file.
