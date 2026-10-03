---
name: poshqc-gates-observed-outputs-for-scripts-hygiene
description: What the PoshQC MCP tools actually return for scripts/hygiene work (observed 2026-10-02, issue 961 prep): no numbers in the summary, coverage doc omits scripts/hygiene, whole-repo analyze already fails, junit is the test observable
metadata:
  type: project
---

Observed while preparing issue 961 (hygiene-guard backup-file rule), read-only runs against a clean tree:

- `run_poshqc_test` / `_format` return only `ok:true` plus one sentence, no counts. The test observable is `artifacts/pester/pester-junit.xml` (root `tests`, `errors`, `failures`; testcase names are `<Describe>.<It name>`). Baseline for `tests/scripts/hygiene` is 31 tests (7 + 19 + 5), 0 failures. The file is git-ignored, so Grep needs the explicit path.
- `artifacts/pester/powershell-coverage.xml` from the PoshQC test run does NOT contain `scripts/hygiene` in its denominator (only `.claude` and `.codex` packages). A hygiene line-coverage figure cannot be read locally; CI `_pester.yml` asserts LINE at 80 over `scripts/hygiene`. Do not write a local coverage-percentage acceptance for that folder.
- `run_poshqc_analyze` with no `scan_folders` fails with 21 pre-existing PSScriptAnalyzer issues; scoped to `["scripts/hygiene", "tests/scripts/hygiene"]` it returns ok. Always scope analyze and format.
- Format is a write-mode tool: observe it by a before/after `git hash-object` of the files, not by the tool status (plan-gate rule G7).
- The hygiene guard itself flags drive-letter user-profile paths in any tracked file outside `.claude/`, so plans and evidence for this work must use a `<worktree-root>` placeholder and never the real path.
- A worktree-isolated shell refuses every `pwsh` form, so guard-run tasks need a non-isolated executor; the orchestrator itself can still call the PoshQC MCP tools.

**How to apply:** when planning or preflighting work under `scripts/hygiene` or other folders outside the PoshQC coverage scope, scope the MCP calls, observe junit counts, and record coverage as CI-measured. See [[preflight-without-build-access-cannot-clear-a-plan]] and [[worktree-isolation-blocks-pwsh-per-agent-type]].
