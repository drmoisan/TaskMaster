---
name: project-441-cobertura-arithmetic-plan-seams
description: "#441/#478 Cobertura arithmetic plan seams: AC-18's two-file pin collides with the 500-line test-file ceiling, the branch ratio is defect-invariant, and PoshQC's cited settings file does not exist"
metadata:
  type: project
---

Planning seams found while writing the atomic plan for
`docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441` (epic
`build-ci-coverage-gate-fidelity`, wave 0, PowerShell only).

1. **AC-18 vs the 500-line ceiling — a real conflict.** AC-18 pins the diff to exactly two source
   files (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`,
   `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`). The test file is 223 lines;
   six fixtures written in the file's existing verbose here-string style plus five helper unit tests
   projects to ~548 lines. Splitting into a third test file is the obvious fix and it **breaks
   AC-18**. The plan resolves it with per-block line budgets and a mandate to collapse
   `<methods>`/`<method>`/`<lines>` wrapper elements onto single lines inside the here-strings.
2. **The branch ratio does not discriminate.** For `QfcHomeController`, 8/12 and 12/18 both equal
   0.666667, and the F1 line-rate is 0.666667 before and after. A fixture asserting only
   `line-rate`/`branch-rate` passes against the defective code. Assert `lines-valid`/`lines-covered`
   and `branches-valid`/`branches-covered`.
3. **`scripts/powershell/PoshQC/settings/pester.runsettings.psd1` does not exist in this worktree** —
   `scripts/powershell/` is absent entirely, though `.claude/rules/powershell.md` and the spec both
   cite the path. Do not write a plan task that depends on that file; have the executor record the
   settings source the MCP server actually uses.
4. **The baseline is an A/B over a committed document, not a suite run.** Re-running MSTest would
   confound the fix with `dotnet-coverage` denominator nondeterminism. Fixed input:
   `.../424/evidence/baseline/coverage-baseline.cobertura.xml` (raw generator output, carries ground
   truth 79957/56124/23109/13472 in its own root attributes; pre-fix `LinesValid` is 161086).
5. **`Helpers.ps1:219` is correct and off-limits**; only `:122` is defective. Because the formatter
   runs over the whole file, the byte-identity gate for the `:217-268` union builder must be
   re-verified *after* the final format pass, not only after the edit.

**Why:** these four points each cost a re-derivation or would have produced an unexecutable plan.

**How to apply:** reuse item 1's shape whenever an AC pins a file set and the target test file is
within ~250 lines of the 500 ceiling — surface the collision in the plan rather than letting the
executor discover it mid-phase. Related: [[literal-call-clauses-block-file-size-tightening]],
[[csharpier-repowide-format-breaks-zero-diff-acs]],
[[feedback_postformat_file_size_audit]].
