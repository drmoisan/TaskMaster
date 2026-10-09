---
name: project-poshqc-analyze-flags-new-verb-test-helpers-and-comma-return
description: PoshQC analyze fails on New-* helper functions inside Pester BeforeAll (ShouldProcess rule) and on unary-comma returns whose OutputType says string[]; MCP gives only an issue count
metadata:
  type: project
---

The bundled PoshQC analyzer (mcp__drm-copilot__run_poshqc_analyze) returned `ok: false` with only "PSScriptAnalyzer reported 7 issue(s)" on issue #985. A plain `Invoke-ScriptAnalyzer -Path <file>` (default rules) over the Write Set reproduced exactly the same 7 findings, so it is a usable diagnosis route when the MCP tool gives no detail.

The two finding classes:
- PSUseShouldProcessForStateChangingFunctions (Warning) on test-file helper functions named `New-*` defined inside a Pester `BeforeAll` (fixture builders such as New-AppConfigText). Fix: name fixture builders with the `Get-` verb.
- PSUseOutputTypeCorrectly (Information, still counted) on a private helper that returns `, [string[]]$value` with `[OutputType([string[]])]`: the analyzer infers Object[]. Fix: emit values unwrapped with `[OutputType([string])]` and have callers collect with `@(...)`.

**Why:** each costs a full PowerShell loop restart (format, analyze, test, coverage) under the plan's C7 rule.
**How to apply:** when writing new Pester helpers or module helpers, use Get- verbs and avoid unary-comma returns from the start; when the MCP analyze reports only a count, run Invoke-ScriptAnalyzer per file to list them. See also [[project-rehearsal-merge-x-ours-drops-adjacent-manifest-edits]].
