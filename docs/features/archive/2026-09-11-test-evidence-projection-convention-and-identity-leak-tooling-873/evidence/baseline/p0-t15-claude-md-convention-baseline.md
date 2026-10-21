# P0-T15 — Convention-Absence Baseline In CLAUDE.md

Timestamp: 2026-09-13T05-04
Task: [P0-T15]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; $raw = Get-Content -Raw -LiteralPath "CLAUDE.md"; ([regex]::Matches($raw, [regex]::Escape(<literal>))).Count for each of the three literals; then Select-String -SimpleMatch "vstest.console.exe" over the same file'
EXIT_CODE: 0

Matching is case-sensitive: `[regex]::Matches` with no option argument is case-sensitive, and each
literal is escaped before matching.

## The three counts, three bare integers

HEADING_COUNT: 0
RESULTSDIRECTORY_SWITCH_COUNT: 0
LOGGER_SWITCH_COUNT: 0

Literals counted, exactly as this plan will later author them:

- the exact heading line `## Committed Test Evidence Format`
- the switch literal `/ResultsDirectory:`
- the switch literal `/Logger:trx;LogFileName=`

## The two test-console toolchain steps, verbatim

Line 390:

```
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
```

Line 408:

```
4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
```

The first sits under the `### CUT3. C# Toolchain Command Selection` heading and the second under the
`## C# Toolchain (run in this exact order)` heading. Both are the steps the new results-directory and
log-file-name requirement attaches to.

## Output Summary

Three integers recorded, each of which is 0: the section heading this plan will author is absent, and
neither switch literal occurs anywhere in `CLAUDE.md`. Two verbatim toolchain-step lines are recorded
above. Because all three counts are 0, this task does not halt, and the Phase 5 after-state check is
falsifiable: a non-zero count after the edit is attributable to this delivery and to nothing else.

EXIT_CODE: 0
