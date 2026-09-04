# Post-fix Run of the New Suite ([P2-T4])

Timestamp: 2026-09-03T12-08

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Total=$($r.TotalCount)"; $r.Tests | ForEach-Object { "TEST Result=$($_.Result) Name=$($_.Name)" }; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`

EXIT_CODE: 0

## Emitted lines, verbatim (all four)

```
PESTER Passed=3 Failed=0 Total=3
TEST Result=Passed Name=includes an assembly directly beneath a search root that is itself under a .claude worktree segment
TEST Result=Passed Name=excludes a nested sibling worktree beneath a non-dot-claude search root
TEST Result=Passed Name=retains the root-level assembly and excludes a further-nested worktree beneath a dot-claude search root
```

Output Summary: All three cases pass against the fixed predicate. Read alongside `evidence/regression-testing/pre-fix-new-suite.2026-09-03T07-23.md`, which recorded `PESTER Passed=1 Failed=2 Total=3` and the `No test assemblies found` failure message against the same file and the same command, this is the fail-before and pass-after pair for spec AC2 and AC4. The symmetry-twin case `excludes a nested sibling worktree beneath a non-dot-claude search root` passed both before and after, which is the preservation property the anchored regex exists to keep.
