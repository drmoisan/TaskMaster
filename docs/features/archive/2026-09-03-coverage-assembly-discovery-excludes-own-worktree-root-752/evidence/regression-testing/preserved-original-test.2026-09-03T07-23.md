# Preserved Original Regression Test ([P2-T5])

Timestamp: 2026-09-03T12-09

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1"; $c.Run.PassThru = $true; $c.Filter.FullName = "*excludes assemblies discovered under a .claude worktree segment*"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) NotRun=$($r.NotRunCount) Total=$($r.TotalCount)"; $r.Tests | Where-Object { $_.Result -ne "NotRun" } | ForEach-Object { "TEST Result=$($_.Result) Name=$($_.Name)" }; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`

EXIT_CODE: 0

## Emitted lines, verbatim

```
PESTER Passed=1 Failed=0 NotRun=26 Total=27
TEST Result=Passed Name=excludes assemblies discovered under a .claude worktree segment
```

Output Summary: The untouched original regression test at `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` line 416 passes against the fixed predicate. The `NotRun=26 Total=27` figures are recorded as an observation: Pester 5's `Filter.FullName` does not reduce `TotalCount`, so all 27 `It` blocks in that file are still discovered and the 26 non-selected ones are returned with `Result` of `NotRun`; the `Where-Object` filter suppressed those 26 from the `TEST` lines. Those two figures are deliberately not a pass/fail condition, because an unrelated later change adding or removing an `It` block elsewhere in that file would move both without saying anything about this test. This result is what makes the anchored form of the replacement regex mandatory: with an unanchored regex applied to a relative path, this test fails, because `GetRelativePath` returns the nested sibling worktree as `.claude\worktrees\agent-1\...` with no leading separator.
