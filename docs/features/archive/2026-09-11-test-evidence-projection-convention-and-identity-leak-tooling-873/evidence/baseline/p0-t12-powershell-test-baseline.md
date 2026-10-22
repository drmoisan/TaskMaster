# P0-T12 — PowerShell Test Baseline With Coverage

Timestamp: 2026-09-13T05-01
Task: [P0-T12]

Scan scope: `scripts/vscode` and `tests/scripts/vscode`, passed explicitly as `scan_folders`.

## Step 1 — MCP test invocation

Tool: mcp__drm-copilot__run_poshqc_test
Command: mcp__drm-copilot__run_poshqc_test with workspace_root set to this worktree and scan_folders
set to ["scripts/vscode", "tests/scripts/vscode"]
MCP_RESULT_OK_FLAG: true
MCP result summary: `Ran bundled PoshQC test against '<worktree>' with 2 selected scan folder(s).`

The MCP tool returns no counts, so it is paired unconditionally with the direct run below, which
supplies them.

## Step 2 — Paired direct Pester run over the test folder

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; Import-Module Pester -MinimumVersion 5.0; $cfg = New-PesterConfiguration; $cfg.Run.Path = "tests/scripts/vscode"; $cfg.Run.PassThru = $true; $cfg.Output.Verbosity = "None"; $cfg.CodeCoverage.Enabled = $true; $cfg.CodeCoverage.Path = "scripts/vscode"; $cfg.CodeCoverage.OutputFormat = "JaCoCo"; $cfg.CodeCoverage.OutputPath = "docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/baseline/p0-t12-powershell-coverage-baseline.jacoco.xml"; $r = Invoke-Pester -Configuration $cfg; ...; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'

EXIT_CODE: 0

The explicit exit statement is mandatory and is present: Pester ignores `Run.Exit` by default, so a
direct run sets no process exit code of its own and an `EXIT_CODE:` field would otherwise be
meaningless.

Pester version: 5.6.1
Coverage output format: JaCoCo
Coverage scope: `scripts/vscode`
Coverage output path:
`docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/baseline/p0-t12-powershell-coverage-baseline.jacoco.xml`

### Counts line, verbatim

```
PESTER_COUNTS: passed=103 failed=0 skipped=0
```

PESTER_PASSED_COUNT: 103
PESTER_FAILED_COUNT: 0
PESTER_SKIPPED_COUNT: 0

### Incidental console output produced by the tests, recorded so it is not mistaken for a defect

The run printed four `Using vstest.console: C:\repo\vstest.console.exe` / `Discovered 1 test
assemblies.` pairs, one `Using MSBuild: ...` line, one `Sync-PackageReferences: All HintPaths are up
to date` line, and one deprecation warning about an `-EnableNullable` switch. These come from
existing tests that exercise the entry points with mocked executable seams; the printed
`C:\repo\...` value is a test fixture value, not a real path. No external executable was launched:
`git status --porcelain` over `*.csproj`, `*.props`, `*.targets`, `*.config`, `scripts` and `tests`
was empty after the run, so no project file and no script was modified.

## Headline coverage, derived from the emitted JaCoCo document

Method as the task specifies: count the document's `line` elements whose covered-instruction
attribute `ci` is greater than zero, against the total number of `line` elements.

JACOCO_LINE_ELEMENTS_TOTAL: 716
JACOCO_LINE_ELEMENTS_COVERED: 551
HEADLINE_LINE_COVERAGE_PERCENT: 76.96

This figure is the whole of `scripts/vscode`, which includes several scripts that carry no test file
at all, so it is a folder-wide baseline rather than a per-file measurement and it is recorded as a
comparison point, not as a gate result. The delivery's own new-code coverage obligation is measured
per new part file in Phase 7 under AC21.

## Output Summary

MCP_RESULT_OK_FLAG: true. Direct run EXIT_CODE: 0 with 103 passed, 0 failed, 0 skipped. Headline
line coverage over `scripts/vscode` is 76.96 percent, from 551 covered of 716 line elements. This is
the population the Phase 7 passed-count comparison reads, because that comparison runs over exactly
`tests/scripts/vscode`.

EXIT_CODE: 0
