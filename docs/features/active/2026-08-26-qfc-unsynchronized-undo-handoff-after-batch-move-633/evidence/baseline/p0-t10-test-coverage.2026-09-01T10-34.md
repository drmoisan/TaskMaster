# Baseline test run with coverage (P0-T10)

Timestamp: 2026-09-01T10-34
Task: [P0-T10]
Working directory: WORKTREE

Command:

```
pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\baseline.cobertura.xml
```

EXIT_CODE: 0

`-SearchRoot .` is passed explicitly. Discovery is relative to this worktree, and this worktree is
itself under `.claude/worktrees/`, so discovery finds only this worktree's own test assemblies. No
`\.claude\` exclusion filter is applied: that filter belongs to runs launched from the main checkout,
and applying it here would exclude every assembly and produce a vacuous run.

The wrapper appends `/Settings`, `/InIsolation`, and `/TestCaseFilter:TestCategory!=LiveOutlook`
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1:76`). The category filter is what keeps
`LiveOutlookHookupIntegrationTests` out of the run; that class launches a real Outlook process, which
`.claude/rules/general-unit-test.md` prohibits as an external process.

Run window: started 2026-09-01T10-33-26, ended 2026-09-01T10-34-21.

## Verbatim vstest result summary

```
Test Run Successful.
Total tests: 6912
     Passed: 6912
 Total time: 37.6494 Seconds
```

## Test counts

| Metric | Value |
|---|---|
| Total | 6912 |
| Passed | 6912 |
| Failed | 0 |
| Skipped | 0 |

The run printed no `Failed:` and no `Skipped:` line, which vstest omits when the corresponding count is
zero; `Total tests` equals `Passed`, so both are 0.

## Coverage post-processing

The wrapper's trailing output (absolute worktree path replaced by the token `WORKTREE`):

```
Code coverage results: WORKTREE\coverage\baseline.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: WORKTREE\coverage\baseline.cobertura.xml
```

`Test-Path coverage\baseline.cobertura.xml` reports `True`.

Output Summary: The baseline run is green. 6912 tests executed, 6912 passed, 0 failed, 0 skipped, wall
time 37.65 seconds, wrapper exit code 0. No terminating message was produced and the
`REMEDIATION-REQUIRED: coverage wrapper prerequisite missing` branch was not taken. The wrapper reached
its post-processing stage and printed `Done.`, which is the observable signal that
`Invoke-DotnetCoverageCollection` returned without throwing and that
`Assert-CoberturaLineCoverageThreshold` did not throw; therefore `Set-Content` at script line 343 ran and
the Cobertura file on disk is the post-processed, first-party-filtered artifact rather than the raw
dotnet-coverage output. P0-T11 verifies that classification directly from the file's package list rather
than inferring it from this signal.

The known intermittently-failing `PhysicalFileInfoAdapter` test that opens the real solution file did
not fail in this run, so no re-run was required and the baseline failure set is empty.
