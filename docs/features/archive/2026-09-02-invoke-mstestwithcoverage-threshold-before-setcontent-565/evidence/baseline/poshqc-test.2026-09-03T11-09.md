Timestamp: 2026-09-03T11-09
Command: mcp__drm-copilot__run_poshqc_test (scan_folders: tests/scripts/vscode); paired direct run: pwsh -NoProfile -Command 'Import-Module Pester -MinimumVersion 5.0; $c = New-PesterConfiguration; $c.Run.Path = "<abs>/tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "<abs>/scripts/vscode/Invoke-MSTestWithCoverage.ps1"; $c.CodeCoverage.OutputPath = "<abs evidence path>/pester-coverage.2026-09-03T11-09.xml"; $r = Invoke-Pester -Configuration $c; ...'
EXIT_CODE: 0
Output Summary: MCP Result: ok:true. Passed=92 Failed=0 Skipped=0.
MainScriptCommands=111 Executed=100 Percent=90.09 (line coverage of
scripts/vscode/Invoke-MSTestWithCoverage.ps1). branch coverage: not emitted by Pester 5.

MCP tool result: {"ok":true,"tool":"run_poshqc_test","workspace_root":"<item-worktree-root>","summary":"Ran bundled PoshQC test against '<item-worktree-root>' with 1 selected scan folder(s)."}

Direct Pester run summary (verbatim tail lines):
Tests completed in 15.88s
Tests Passed: 92, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
Passed=92 Failed=0 Skipped=0
MainScriptCommands=111 Executed=100 Percent=90.09

Pester Coverage Artifact: docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/evidence/baseline/pester-coverage.2026-09-03T11-09.xml

Suite-composition drift note: Pester discovery found 10 files (92 tests) under
tests/scripts/vscode, not the 5 files the plan's Conventions section describes. This is expected
drift: issue #733 / PR #748 added new test files (confirmed present:
Invoke-MSTest.AssemblyDiscovery.Tests.ps1, Invoke-MSTest.Main.Tests.ps1, and others) to the
reconciled tree after this plan was authored. This does not affect the fix's scope (the plan's
Scope Prohibitions list only the two files this plan edits) and Failed=0 confirms no regression
across the full, larger suite as it exists today.
