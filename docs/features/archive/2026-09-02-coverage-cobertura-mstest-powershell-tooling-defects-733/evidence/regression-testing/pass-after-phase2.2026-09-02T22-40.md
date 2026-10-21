# Phase 2 pass-after run (P2-T4)

Timestamp: 2026-09-02T22-40

Task: [P2-T4]

## Production change under test

[P2-T3] added a fourth clause to the discovery `Where-Object` predicate inside
`Invoke-MSTestWithCoverageMain` in scripts/vscode/Invoke-MSTestWithCoverage.ps1, in the same
single-quoted style as the sibling `\obj\` and `\ref\` clauses. The outer `@(...)` wrapping of the
discovery pipeline is unchanged, per this plan's Scope Prohibitions. The file parses with zero
parse errors (verified with `[System.Management.Automation.Language.Parser]::ParseFile`).

## Command 1 — Direct Pester run over tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 (absolute path within the item
worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then the explicit trailing
branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

Counts: Passed 27, Failed 0, Skipped 0, Total 27. Pester version 5.6.1. Run duration 2.16s.

Observed:

```
Describing Invoke-MSTestWithCoverageMain
  [+] excludes assemblies discovered under a .claude worktree segment 57ms (57ms|1ms)
```

The test asserts `$script:capturedTestAssembly | Should -Be @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll')`.
Pester's `-Be` compares arrays element-wise, so the passing verdict establishes that the captured
`-TestAssembly` array is exactly that one-element array: the ordinary path only, with the
`.claude` worktree path removed.

## Command 2 — Direct evaluation of the new clause against both fixture paths

To record the captured array contents explicitly rather than only by the assertion verdict, the
new pattern was read back out of the production file (line 301 of
scripts/vscode/Invoke-MSTestWithCoverage.ps1, split on `notmatch` and trimmed of its surrounding
single quotes) and applied to the same two fixture paths the test supplies.

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper reading the pattern from the
production file with `Get-Content -LiteralPath`, then filtering the two fixture strings with
`Where-Object { $_ -notmatch $pattern }`.

EXIT_CODE: 0

Output:

```
PatternFromFile=<the new .claude segment pattern, read verbatim from the production file>
SurvivorCount=1
Survivor=C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
```

## Output Summary

The P2-T1 regression test now passes against the P2-T3 production change, and the captured
`-TestAssembly` array contains only the ordinary path
`C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`; the `.claude` worktree path
`C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` is excluded.
All 27 tests in the file pass with zero failed and zero skipped, up from 26 passed / 1 failed on
the P2-T2 expect-fail run, so no sibling test regressed. Absolute host paths naming the item
worktree were replaced with their repository-relative equivalents; the `C:\repo\...` strings are
the test's own synthetic fixture values, not host paths.
