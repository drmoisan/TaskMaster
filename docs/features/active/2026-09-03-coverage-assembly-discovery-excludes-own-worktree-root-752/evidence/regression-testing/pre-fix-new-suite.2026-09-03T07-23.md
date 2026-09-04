# Pre-fix Run of the New Suite ([P1-T9], [P1-T10])

Timestamp: 2026-09-03T12-04

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Total=$($r.TotalCount)"; $r.Tests | ForEach-Object { "TEST Result=$($_.Result) Name=$($_.Name)" }; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`

EXIT_CODE: 1

ExpectedExitCode: 1

## Emitted lines, verbatim (all four)

```
PESTER Passed=1 Failed=2 Total=3
TEST Result=Failed Name=includes an assembly directly beneath a search root that is itself under a .claude worktree segment
TEST Result=Passed Name=excludes a nested sibling worktree beneath a non-dot-claude search root
TEST Result=Failed Name=retains the root-level assembly and excludes a further-nested worktree beneath a dot-claude search root
```

Output Summary: Against the unfixed production predicate, the two `[expect-fail]` cases fail and the symmetry-twin preservation case passes, which is exactly the fail-before shape this plan requires. The two failures are the self-exclusion defect this item fixes: the unfixed clause matches the absolute `FullName`, so every candidate under a search root that is itself beneath a `.claude` worktree segment is excluded and the discovery set is empty.

## Failure-mode proof ([P1-T10])

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"; $c.Run.PassThru = $true; $c.Filter.FullName = "*includes an assembly directly beneath a search root*"; $r = Invoke-Pester -Configuration $c; $r.Tests | Where-Object { $_.Result -eq "Failed" } | ForEach-Object { "FAILMSG " + $_.ErrorRecord.Exception.Message }; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`

EXIT_CODE: 1

ExpectedExitCode: 1

Emitted line, verbatim (exactly one):

```
FAILMSG No test assemblies found under 'C:\repo\.claude\worktrees\agent-7\.' for configuration 'Debug'. Build first.
```

Output Summary: Exactly one `FAILMSG` line was emitted, and it carries the token `No test assemblies found`, which is the error thrown at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 306 when the discovery set is empty. The failure is therefore the reported defect — every candidate under the `.claude`-rooted search root was excluded by the unanchored absolute-path predicate — and not a harness error. The `Where-Object` filter suppressed the two `NotRun` cases that `Filter.FullName` excluded, which would otherwise have contributed two empty `FAILMSG` lines.
