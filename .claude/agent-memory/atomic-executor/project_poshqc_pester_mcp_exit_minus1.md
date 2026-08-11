---
name: poshqc-mcp-tools-report-no-verdict
description: mcp__drm-copilot__run_poshqc_test returns ok:true with no test counts/exit code, so any "EXIT_CODE 0"/"N failures" acceptance built on it is vacuous; pair with direct Invoke-Pester
metadata:
  type: project
---

`mcp__drm-copilot__run_poshqc_test` carries **no verdict**. Measured 2026-08-10 in a TaskMaster worktree with `scan_folders=["scripts/vscode","tests/scripts/vscode"]`, the entire payload was:

```
{"ok":true,"tool":"run_poshqc_test","workspace_root":"...","summary":"Ran bundled PoshQC test against '...' with 2 selected scan folder(s)."}
```

No test counts, no per-test names, no exit code, no failure detail. It reports *that it ran*, not *what happened*. (An earlier session recorded it exiting `-1`; the tool now returns `ok:true`. Either way it yields no result data — do not treat `ok:true` as "tests passed".)

**Why:** plans routinely write acceptance criteria like `EXIT_CODE: 0`, "zero failures", or "F1 fails with 6/4" against this command. Every one of those is vacuous (passes unconditionally) or unsatisfiable (the datum does not exist). An `[expect-fail]` task is the worst case: the tool returns `ok:true` while the tests are red.

**How to apply:** still execute the mandated MCP command and record its `ok`/`summary` verbatim (No-SKIPPED rule), but attribute every numeric verdict to a direct Pester run. Verified working recipe (Pester 5.6.1, pwsh7, `Set-Location $root` first — the config takes repo-relative paths):
`New-PesterConfiguration`; `Run.Path`; `Run.PassThru=$true`; `CodeCoverage.Enabled=$true`; `CodeCoverage.Path`; `CodeCoverage.OutputFormat='JaCoCo'`; `CodeCoverage.OutputPath`. Then `$r.TotalCount/PassedCount/FailedCount`, per-test detail via `$r.Tests | ForEach-Object { $_.Result; $_.ExpandedName; $_.ErrorRecord.Exception.Message }`, and JaCoCo counters from the XML. Pester emits INSTRUCTION/LINE/METHOD/CLASS and **no BRANCH** counter.

`run_poshqc_analyze` **does** carry a verdict (`ok:false` + `PSScriptAnalyzer reported N issue(s).`), but only a count — no file or rule. `scan_folders` genuinely narrows it (`["tests/scripts/vscode"]` -> ok:true; adding `scripts/vscode` -> 16 issues), so scoping cannot rescue a folder that already carries findings; use direct `Invoke-ScriptAnalyzer` for a per-file, per-rule baseline. See [[project_build_test_env]].
