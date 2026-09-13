# P7-T8 — File Size Audit After The Final Format Step

Timestamp: 2026-09-13T07-15
Task: [P7-T8]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; $paths = @(...the twelve paths below...); foreach ($p in $paths) { $p + " " + @(Get-Content -LiteralPath $p).Count }'

EXIT_CODE: 0

Each figure is the element count of the file's content lines, measured after the final format step of
the second toolchain pass, so the figures are the ones the repository's 500-line ceiling applies to.

## Recorded line counts

```
scripts/vscode/Invoke-MSTest.ps1 262
scripts/vscode/Invoke-MSTest.TrxSummary.ps1 150
scripts/vscode/Invoke-MSTestWithCoverage.ps1 438
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 471
scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 197
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 146
tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1 119
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 498
tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1 193
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 106
tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 495
tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 268
```

FILES_MEASURED: 12
MAXIMUM_RECORDED_LINE_COUNT: 498
FILES_EXCEEDING_500: 0

Every recorded integer is at most 500. The two files nearest the ceiling are
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` at 498 and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` at 495. Both are within the
limit. The first reached its size through the splatting conversion P3-T4 and P4-T3 specify, which
exists precisely so those call sites could absorb two extra arguments each without growing past the
ceiling; the second reached its size through the P7-T7 remediation, which added two tests.

## Helpers growth bound

| Figure | Value |
|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, P0-T10 `POST_FORMAT_BASELINE_LINE_COUNTS:` | 470 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, this audit | 471 |
| Growth | 1 |

The growth is exactly one line, which is the bound. That one line is the dot-source of the new
projection part file that P1-T2 adds, and nothing else was added to the helpers file.

The comparison uses the P0-T10 post-format baseline rather than the P0-T13 pre-format baseline,
because both this measurement and that baseline must be taken on a formatted tree for the difference
to attribute to this delivery rather than to a formatting change.

## Comparison against the other recorded baselines, for context

| File | P0-T10 post-format baseline | This audit | Change |
|---|---|---|---|
| `scripts/vscode/Invoke-MSTest.ps1` | 202 | 262 | +60 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 351 | 438 | +87 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 470 | 471 | +1 |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | 496 | 498 | +2 |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | 144 | 146 | +2 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | 99 | 106 | +7 |

The six files created by this delivery carry no baseline figure, because they did not exist at
Phase 0.

## Output Summary

Twelve files measured, every recorded integer at most 500, the largest being 498. The helpers file
measures 471 against its post-format baseline of 470, a growth of exactly one line, which satisfies
the at-most-one-greater bound. The audit was taken after the final format step.
