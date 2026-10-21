# Phase 4 file-size check (P4-T7)

Timestamp: 2026-09-02T22-52

Task: [P4-T7]

## Command

Command: pwsh -NoProfile -Command reading each file with `Get-Content -LiteralPath` and reporting
`.Count`. This is the same physical-line idiom the P0-T4 baseline, the P3-T5 check, and the P4-T1
projection used. `Measure-Object -Line` is deliberately not used because it omits blank lines and
under-reports against the 500-line ceiling.

EXIT_CODE: 0

## Measurements

| File | Lines | Ceiling | Headroom | Verdict |
|---|---|---|---|---|
| scripts/vscode/Invoke-MSTest.ps1 | 157 | 500 | 343 | at or under |
| tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 (the file chosen by P4-T1) | 53 | 500 | 447 | at or under |

Change from the P0-T4 baseline: scripts/vscode/Invoke-MSTest.ps1 grew from 131 to 157 lines
(+26 net — the 33-line `Get-MSTestAssemblyPathList` function added by P4-T4, less the 7 lines of
inline pipeline removed by P4-T5).
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 is new, at 53 lines.

## Confirmation of the P4-T1 placement decision

tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 remains at 487 lines, unchanged since
the P4-T1 measurement, because Phase 4 added nothing to it. Had the new Describe block been placed
there instead, the file would now stand at roughly 520 lines and would violate the ceiling. The
split decision recorded in evidence/other/phase4-test-file-placement.2026-09-02T22-43.md is
therefore confirmed by the outcome.

## Output Summary

Both Phase 4 files are at or under the 500-line ceiling in .claude/rules/general-code-change.md
and .claude/rules/powershell.md: scripts/vscode/Invoke-MSTest.ps1 at 157 lines and
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 at 53 lines. No extraction was
required. tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 is unchanged at 487 lines, the
value on which the P4-T1 split decision rested.
