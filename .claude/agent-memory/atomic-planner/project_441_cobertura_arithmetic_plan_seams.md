---
name: project-441-cobertura-arithmetic-plan-seams
description: "#441/#478 Cobertura arithmetic plan seams: AC-18's two-file pin collides with the 500-line test-file ceiling, the branch ratio is defect-invariant, PoshQC's cited settings file does not exist, StrictMode makes any missing XML attribute (branch AND complexity) in a fixture throw, and a blanket EXIT_CODE schema AC breaks on narrative artifacts"
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

6. **`Set-StrictMode -Version Latest` makes a fixture's *missing* XML attribute throw.** Line 1 of
   `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` sets it, and it propagates into
   the production functions dot-sourced in `BeforeAll`. `Helpers.ps1:128` reads `$line.branch` by bare
   property access, so a `<line>` element authored without a `branch` attribute fails with
   `The property 'branch' cannot be found on this object` instead of the pinned fail-before value.
   Every pre-existing fixture carries `branch="False"`, which hides the hazard. Measured 2026-08-10
   with Pester 5.6.1: with the attribute the same fixture yields the intended
   `Expected: '3' But was: '6'`. When a plan specifies fixture XML by prose, enumerate **every**
   attribute the production code reads, and specify `GetAttribute('x')` over `$node.x` in any
   construction rule the plan dictates.

7. **The StrictMode attribute hazard is not confined to `branch`.** Round-2 preflight found a second
   instance: `Merge-CoberturaClassesByFilename` sums group complexity at `Helpers.ps1:277-281` via
   the bare read `$_.complexity`, so a *merge* fixture (two `<class>` elements sharing one
   `filename`) whose classes omit `complexity` throws
   `The property 'complexity' cannot be found on this object` — before **and** after the fix, since
   the fix replaces only `:270-273`. One fixture-attribute rule per attribute is not enough: sweep
   every bare `$x.attr` read on **every** code path the fixture reaches, including the ones the fix
   does not touch. Single-class fixtures never enter that loop and are unaffected.
8. **A "every artifact carries `Timestamp`/`Command`/`EXIT_CODE`" AC is self-contradictory** as soon
   as the plan writes a narrative artifact (AC status summary, branch-test map, handoff record) that
   records no command. Scope the schema clause to *command-step* artifacts and require narrative
   artifacts to carry `Timestamp` plus individual enumeration in the final evidence sweep. Otherwise
   the check-off task must either lie or stall.

**Why:** these points each cost a re-derivation or would have produced an unexecutable plan;
items 6, 7 and 8 were caught only at preflight and would otherwise have made Phase 1's
`[expect-fail]` acceptances, Phase 3's green gate, and the AC-16 check-off respectively
unsatisfiable.

**How to apply:** reuse item 1's shape whenever an AC pins a file set and the target test file is
within ~250 lines of the 500 ceiling — surface the collision in the plan rather than letting the
executor discover it mid-phase. Related: [[literal-call-clauses-block-file-size-tightening]],
[[csharpier-repowide-format-breaks-zero-diff-acs]],
[[feedback_postformat_file_size_audit]].
