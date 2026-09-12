---
name: poshqc-mcp-measurement-limits
description: What the PoshQC MCP tools can and cannot prove — no scanned-file list, no file count, no BRANCH counter, no coverage parameter, a hooks-only coverage allow-list, and a non-zero PSScriptAnalyzer baseline in scripts/vscode/
metadata:
  type: reference
---

Measurement limits of `mcp__drm-copilot__run_poshqc_format` / `_analyze` / `_test`. Every PowerShell atomic plan that writes a numeric acceptance clause must be built against these facts, not against assumed tool behaviour.

**Why:** Plan #432 shipped four Blocking preflight defects at once because its acceptance clauses assumed a scanned-file list, a branch-coverage number, a coverage parameter, and an analyzer-clean baseline — none of which exist. Three of the four made the plan structurally unable to terminate in PASS regardless of work quality.

**How to apply:**

1. **Return shape is `{ok, tool, workspace_root, summary, stderr_excerpt}`.** On success `summary` is a fixed template string. No file list, **no count of files scanned**, no per-file diagnostics, no test counts, no coverage values. `Invoke-PoshQCFormat`'s per-file `Formatted:` / `Already formatted:` lines go to the information stream and never reach the caller, so "the number of files scanned" is unobtainable — record `ok` + `summary` verbatim and determine "did it modify anything" from `git status --porcelain` taken immediately after. Never write "names any file already carrying a diagnostic", "appears in the scanned-file output", or "listing every diagnostic attributable to it" — those are unevaluable. Prove scan membership by **scoped invocation and observable effect** (`scan_folders: ["<dir>"]`; `IN SCOPE` = `ok: true` with a summary naming `1 selected scan folder(s)`, since `Resolve-PoshQCScanFolder` throws `Failed to resolve scan folder '<f>'` otherwise — a "no files found" condition is NOT observable), and attribute diagnostics by **count delta** against a recorded baseline.

2. **`run_poshqc_analyze` writes no artifact file. `run_poshqc_test` writes two**, inside the executor's worktree:
   - `artifacts/pester/pester-junit.xml` — per-`<testsuite name=...>` full path, `tests`, `failures`, `skipped`. This is the only name-level proof of suite discovery.
   - `artifacts/pester/powershell-coverage.xml` — JaCoCo; report-level and per-`<class sourcefilename=...>` counters, **but only for the files the bundled config instruments** (see item 6).
   These are **read sources**, not evidence write locations; copy the numbers into `<FEATURE>/evidence/<kind>/`. See [[evidence-path-normalization]].

   The tool's own exit code is not a reliable gate: it has been observed exiting `-1` (`4294967295`) with no detail in TaskMaster worktrees, at baseline as well as post-change. Gate the test stage on **zero `failures` and zero `skipped` in `pester-junit.xml`** and record the exit code as a baseline-compared observation, exactly as the analyze stage is handled. A plan that hard-requires `EXIT_CODE: 0` from `run_poshqc_test` can be unsatisfiable from the first run.

3. **PowerShell branch coverage does not exist.** Pester's JaCoCo writer emits only `INSTRUCTION`, `LINE`, `METHOD`, `CLASS`; `type="BRANCH"` occurs zero times. The `>= 75%` branch floor in `.claude/rules/powershell.md` and `.claude/rules/general-unit-test.md` is unmeasurable for PowerShell. Gate on **line** coverage only (`covered / (covered + missed)` from the report-level LINE counter) and record the gap as an out-of-scope observation with a literal limitation line — not `UNVERIFIED`/`N/A`, which plans usually declare invalid.

4. **`run_poshqc_test` has no coverage parameter.** Its schema is `workspace_root` + `scan_folders` only; coverage is emitted unconditionally. "with coverage enabled" names an argument that does not exist.

5. **`scripts/vscode/` is NOT analyzer-clean** (16 issues scoped, 24 repo-wide, both exit 1, as of 2026-08-07); `tests/scripts/vscode/` is clean at 0. Research and specs in this repo have asserted the opposite. Never write `EXIT_CODE: 0` as an analyze acceptance clause unless the plan is allowed to modify those files — pair a baseline-delta gate with a scoped zero-count assertion over the clean directory instead, and define "stage fails" in the Toolchain Contract as *exceeds baseline or modified a file*, or the restart rule becomes a non-terminating loop. Related: [[project_coverage_threshold_conflict_claude_md_vs_general_unit_test]].

6. **Coverage is measured for hooks only — `scripts/vscode/` is never instrumented.** `run_poshqc_test` loads the BUNDLED `PoshQC/settings/pester.runsettings.psd1`, whose `CodeCoverage.Path` is a fixed allow-list of drm-copilot paths; `Invoke-PoshQCTest` prunes the entries that do not exist here and keeps the survivors. In TaskMaster the surviving `<package>` values are only `.claude/hooks`, `.claude/lib/model-routing`, `.claude/lib/orchestrator-state`, `.codex/hooks` — 26 `sourcefilename` values, none under `scripts/vscode/`, with a report-level LINE counter that has read `missed=2315 covered=0` (0.0%). There is no repo-local override: `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` and `config/poshqc-scan.json` are both absent, despite `.claude/rules/powershell.md` line 18 naming the former.

   Consequence for planning: the "repository-wide PowerShell line coverage" figure is a **hooks-only** figure — usable for a no-regression comparison, never as proof that new `scripts/vscode/` code is covered. To gate a new script at `>= 85%` line, plan a **supplementary direct `Invoke-Pester` run**: `New-PesterConfiguration` with `Run.Path` = the test folder, `CodeCoverage.Enabled = $true`, `CodeCoverage.Path` = the specific production files, `OutputFormat = 'JaCoCo'`, `OutputPath` = an artifact under `<FEATURE>/evidence/qa-gates/`. Prove that mechanism works in **Phase 0** against an existing suite and script (halt if Pester v5 / `New-PesterConfiguration` / the expected `<class sourcefilename=...>` element is missing) rather than discovering it in the final QC phase.

7. **`Select-String` has no `-Recurse` parameter.** `(Get-Command Select-String).Parameters.Keys -contains 'Recurse'` is `False`. A repo-wide text search clause must be written `Get-ChildItem -Path . -Recurse -File | Where-Object { $_.FullName -notmatch '[\\/]\.git[\\/]' } | Select-String -Pattern '<p>' -List`. Plans have shipped the invalid `Select-String -Path . -Pattern '<p>' -Recurse` form.
