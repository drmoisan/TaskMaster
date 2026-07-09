---
name: poshqc-pester-mcp-exit-minus1
description: mcp__drm-copilot__run_poshqc_test exits -1 (4294967295) with no detail in TaskMaster worktrees; use direct Invoke-Pester as the numeric proof
metadata:
  type: project
---

The bundled `mcp__drm-copilot__run_poshqc_test` MCP tool exits `-1` (4294967295) with no stderr detail, both with and without `scan_folders`, in this TaskMaster worktree. It failed identically at baseline (before any change) and post-change during #283, so it is a pre-existing environment/bundled-runner condition, not caused by the change under test.

**Why:** the plan mandated that exact command for the PowerShell test gate, but it produces no runnable result here. `run_poshqc_format` (ok) and `run_poshqc_analyze` (exits 1 only because of pre-existing folder-wide findings) both work.

**How to apply:** still EXECUTE the mandated MCP command and record its exit code (No-SKIPPED rule), but pair it with a direct `Invoke-Pester` proof for the numeric pass/coverage headline. Direct pattern that works (Pester 5.6.1): `New-PesterConfiguration`, set `Run.Path`, `Run.PassThru=$true`, `CodeCoverage.Enabled=$true` + `CodeCoverage.Path=@(...scripts...)`, then read `$r.PassedCount`/`$r.FailedCount`/`$r.CodeCoverage.CoveragePercent`. PSScriptAnalyzer and Pester both require pwsh7 (Windows PowerShell 5.1 cannot load PSScriptAnalyzer here). See [[project_build_test_env]].
