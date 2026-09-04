# Final Clean Pass ([P3-T9])

Timestamp: 2026-09-03T12-20

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-cleanpass.2026-09-03T07-23.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; "COVERAGE LinePercent=$($r.CodeCoverage.CoveragePercent)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`

EXIT_CODE: 0

CLEAN PASS ITERATION: 1

## Emitted lines, verbatim

```
PESTER Passed=95 Failed=0 Skipped=0 Total=95
COVERAGE LinePercent=78.3312577833126
```

Output Summary: 95 passed, 0 failed, 0 skipped over the whole `tests/scripts/vscode` scope, with `Covered 78.33% / 75%. 803 analyzed Commands in 11 Files.` in Pester's own console summary. The toolchain loop completed at iteration 1: `[P3-T1]` recorded `WRITE SET REWRITTEN BY FORMATTER: NONE` and `RESTORED PATHS: NONE`; `[P3-T2]` recorded a post-change MCP analyzer count of 16 against a baseline of 16; `[P3-T3]` recorded `NEW DIAGNOSTICS: NONE`; `[P3-T4]` recorded `MCP RESULT OK: true`; and `[P3-T5]` recorded 95 passed, 0 failed. No stage failed and no Write Set file was rewritten by the formatter, so no `.iter2` iteration exists and this is the single clean pass of the whole loop.
