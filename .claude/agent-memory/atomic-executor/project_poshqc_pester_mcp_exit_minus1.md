---
name: poshqc-pester-mcp-no-numeric-detail
description: mcp__drm-copilot__run_poshqc_test returns only {ok, tool, workspace_root, summary} - no exit code, no pass/fail counts, no coverage; pair it with direct Invoke-Pester for every numeric or per-test acceptance criterion
metadata:
  type: project
---

`mcp__drm-copilot__run_poshqc_test` returns a payload of exactly `{ok, tool, workspace_root, summary}`. The summary is a sentence naming the workspace and the scan-folder count. It carries **no exit code, no passed/failed/skipped counts, no per-test names, and no coverage figure**. Verified 2026-08-10 in worktree `agent-af6843b0a129fc575`, both with `scan_folders` and with it omitted (`config/poshqc-scan.json` does not exist in TaskMaster; the tool falls back to a bundled default without erroring).

`scan_folders` accepts **file paths**, not only folders — confirmed for both `run_poshqc_analyze` and `run_poshqc_test`.

The earlier "exits -1 with no detail" observation (recorded during #283) is **no longer reproducing**; the tool now returns `ok:true`. `run_poshqc_analyze` does surface failure as `ok:false` + `"Command exited with code 1."`, so the wrapper propagates non-zero exits — but `run_poshqc_analyze`'s payload likewise gives only a count ("PSScriptAnalyzer reported N issue(s)"), never the rule names.

**Why:** plans routinely write acceptance criteria like "`EXIT_CODE: 0`; `Output Summary:` records numeric passed/failed/skipped counts and coverage percentages" against these MCP tools. Those criteria are unsatisfiable from the tool output and must be flagged at preflight.

**How to apply:** run the MCP tool for the policy record (No-SKIPPED rule), and pair it with a direct run that supplies the numbers.
- Numbers: `Import-Module Pester -MinimumVersion 5.0; $c = New-PesterConfiguration; $c.Run.Path = <paths>; $c.Run.PassThru = $true; $c.Output.Verbosity = 'Detailed'; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = <production paths>; $r = Invoke-Pester -Configuration $c` then read `$r.PassedCount` / `$r.FailedCount` / `$r.SkippedCount` / `$r.CodeCoverage.CoveragePercent`.
- Pester 5.6.1 emits a command/line coverage percent only. **There is no branch-coverage metric.** A plan demanding a numeric PowerShell branch-coverage percentage is asking for a figure the tooling does not produce.
- Rule names: `Invoke-ScriptAnalyzer -Path <file>` directly.
- pwsh7 required for both. See [[project_build_test_env]] and [[project_poshqc_analyze_exit1_on_warning]].
