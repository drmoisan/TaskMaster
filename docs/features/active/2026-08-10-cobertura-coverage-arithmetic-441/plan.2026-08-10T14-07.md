# 2026-08-10-cobertura-coverage-arithmetic-441 (Plan)

- **Issue:** #441 (also closes #478)
- **Work Mode:** full-bug
- **AC Source:** `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Acceptance Criteria (AC-1 .. AC-20)
- **Epic:** `build-ci-coverage-gate-fidelity` (wave 0)
- **Integration Branch:** `epic/build-ci-coverage-gate-fidelity-integration`
- **Feature Branch:** `bug/cobertura-coverage-arithmetic-441`
- **Base Commit:** `edf3d34c`
- **Worktree Root (`<ROOT>`):** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348`
- **Feature Folder (`<FEATURE>`):** `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T14-07
- **Status:** Ready for preflight
- **Version:** 1.0

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

**Evidence location invariant (non-overridable).** Every artifact this plan produces lives under `<FEATURE>/evidence/<kind>/` where `<kind>` is one of `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`. No `artifacts/` evidence path is permitted. Each command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. `<TS>` below means an ISO-8601 stamp in the form `yyyy-MM-ddTHH-mm`.

**Toolchain (PowerShell only).** Format -> analyze -> test, per `.claude/rules/powershell.md`. Type checking is **not applicable to PowerShell** and no type-check task appears in this plan. Every MCP toolchain call passes `workspace_root` = `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348`.

1. `mcp__drm-copilot__run_poshqc_format`
2. `mcp__drm-copilot__run_poshqc_analyze`
3. `mcp__drm-copilot__run_poshqc_test`

Restart from step 1 whenever any step fails or changes files. **No C# toolchain command appears in this plan** (csharpier, msbuild, vstest are all out of scope; no C# file is touched). In particular `/p:Nullable=enable` is a known-defective documented command (issue #522) and must not be invoked.

## Non-Goals (hard scope boundaries — encode as gates, not as prose)

1. **No coverage threshold may be re-tuned.** Threshold reconciliation is owned by child feature #494 (wave 2). The corrected repository-wide line rate for the #424 sample is ~85.0317% against an 85% floor — a 0.03 pp margin. That observation is **recorded as a handoff to #494** (P5-T4) and acted upon nowhere. No task in this plan modifies a threshold in any file.
2. **No edits to `CLAUDE.md` or anything under `.claude/rules/`.** Gated by P4-T9.
3. **No `[ExcludeFromCodeCoverage]` / nested-lambda work.** That is #457, a separate dependent child (wave 1).
4. **No change to `scripts/vscode/Invoke-MSTestWithCoverage.ps1`,** including its missing `\.claude\` discovery exclusion. Gated by P2-T6.
5. **`Invoke-MSTestWithCoverage.Helpers.ps1:219` and the whole union builder at `:217-268` stay byte-identical.** Editing `:219` destroys the working half of the merge. Gated by P2-T5 and re-verified after formatting by P4-T10.
6. **Do not merge or strip `<methods>` subtrees; do not recompute package-level rates.** Pinned by fixture F6.
7. **No PR creation, CI monitoring, or feature review in this plan.** Those are performed later by `epic-orchestrator`.

## The Change (settled — do not re-derive)

- Exactly one defective site: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:122`, `$cls.SelectNodes('.//lines/line')`.
- `Helpers.ps1:219` (`$classNode.SelectNodes('./lines/line')`) is already correct and off-limits.
- `Helpers.ps1:270-273` (the `$classSummaryXml` synthetic-document block) is removed and replaced by a direct helper call on `$mergedClassNode`. `$mergedClassNode` is an orphan clone owned by `$XmlDocument`; child-axis XPath on an orphan element works normally, so no `ImportNode` is needed.
- One new pure helper `Get-CoberturaClassLineSummary -ClassNode [System.Xml.XmlElement]` returns `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches`.
- Branch arithmetic is **in scope**: the branch accumulator at `:128-131` sits inside the loop being fixed.

## Fixture-Design Trap (mandatory)

For the `QfcHomeController` class the branch *ratio* is unchanged by the double count (8/12 and 12/18 both equal 0.666667) while the *counts* are inflated 50%. F1's line-rate is likewise `0.666667` both before and after the fix. **F1 and F2 must assert on `lines-valid`/`lines-covered` and `branches-valid`/`branches-covered`. An assertion on `line-rate` or `branch-rate` alone passes against the defective code and is therefore not a regression test.**

## Test-File Line Budget (mandatory)

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is 223 lines today; the 500-line ceiling in `.claude/rules/general-code-change.md` applies to test code. AC-18 pins the change to exactly two source files, so **the overflow must not be resolved by adding a third test file.** Write the six fixture here-strings compactly: collapse `<methods>`, `<method>` and the method's `<lines>` wrapper onto single lines and keep one `<line>` element per line only inside the class-level `<lines>` rollup. Per-block budgets: F1 <= 24, F2 <= 28, F3 <= 34, F4 <= 26, F5 <= 24, F6 <= 34, helper unit-test `Describe` <= 80. Enforced by P3-T9 (pre-format) and P4-T5 (post-format).

### Phase 0 — Baseline Capture and Policy Reads

- [ ] [P0-T1] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348\CLAUDE.md` in full. Acceptance: file read and its path recorded in the Phase 0 artifact file list.
- [ ] [P0-T2] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348\.claude\rules\general-code-change.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [ ] [P0-T3] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348\.claude\rules\general-unit-test.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [ ] [P0-T4] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348\.claude\rules\powershell.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [ ] [P0-T5] Read `<ROOT>\docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\spec.md` in full and record that it contains exactly 20 unchecked AC items numbered AC-1 through AC-20. Acceptance: the count 20 is recorded in the Phase 0 artifact.
- [ ] [P0-T6] Read `<ROOT>\docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\research\2026-08-10T14-20-cobertura-arithmetic-research.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [ ] [P0-T7] Read `<ROOT>\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` and `<ROOT>\tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and record their pre-change line counts. Acceptance: recorded counts are exactly 357 and 223; any other value halts the plan for re-baselining.
- [ ] [P0-T8] Write `<FEATURE>/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` (CLAUDE.md -> general-code-change.md -> general-unit-test.md -> powershell.md), and the explicit list of every file read in P0-T1..P0-T7. Acceptance: the file exists and contains all three fields plus seven file paths.
- [ ] [P0-T9] Record the git baseline to `<FEATURE>/evidence/baseline/git-baseline.<TS>.md`: current branch, `git rev-parse HEAD`, `git rev-parse edf3d34c`, and `git status --porcelain`. Acceptance: artifact records the branch as `bug/cobertura-coverage-arithmetic-441` and captures the porcelain output verbatim. The HEAD sha is recorded as an observation only and is never used as a later expectation.

```powershell
Set-Location 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348'
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
git rev-parse edf3d34c
git status --porcelain
```

- [ ] [P0-T10] Verify both committed sample documents exist and are readable at `<ROOT>\docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml` and `...\evidence\qa-gates\coverage-final.cobertura.xml`. Acceptance: both paths resolve; if either is absent the plan halts (the A/B evidence method depends on them).
- [ ] [P0-T11] Capture the PRE-CHANGE generator-parity A/B against unmodified `Helpers.ps1` and write `<FEATURE>/evidence/baseline/prechange-generator-parity.<TS>.md`. Acceptance: the artifact records `LinesValid` exactly `161086` plus `LinesCovered`, `BranchesValid` and `BranchesCovered` as concrete integers (no placeholder), alongside the input document's own root attributes `79957 / 56124 / 23109 / 13472`. This is a deterministic A/B over a fixed committed input, not a test-suite run.

```powershell
$root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348'
. (Join-Path $root 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$sample = Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml'
[xml]$doc = Get-Content -LiteralPath $sample -Raw -Encoding UTF8
'INPUT root: lines-valid={0} lines-covered={1} branches-valid={2} branches-covered={3}' -f `
    $doc.coverage.'lines-valid', $doc.coverage.'lines-covered', $doc.coverage.'branches-valid', $doc.coverage.'branches-covered'
Get-CoberturaCoverageSummary -XmlDocument $doc | Format-List
```

- [ ] [P0-T12] Capture the PRE-CHANGE package-filtered A/B by reprocessing `coverage-final.cobertura.xml` through `ConvertTo-KoverageCoberturaXml`, and write `<FEATURE>/evidence/baseline/prechange-package-filtered.<TS>.md`. Acceptance: the artifact records `lines-valid = 110849`, `lines-covered = 94937`, `line-rate = 0.856453` as concrete values. Allow a generous timeout: the input is ~186,913 lines and the `[xml]` cast materializes a full DOM.

```powershell
$root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348'
. (Join-Path $root 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$sample = Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\qa-gates\coverage-final.cobertura.xml'
$content = Get-Content -LiteralPath $sample -Raw -Encoding UTF8
[xml]$out = ConvertTo-KoverageCoberturaXml -XmlContent $content -RepoRoot $root -PathSeparator '\'
'lines-valid={0} lines-covered={1} line-rate={2} branches-valid={3} branches-covered={4} branch-rate={5}' -f `
    $out.coverage.'lines-valid', $out.coverage.'lines-covered', $out.coverage.'line-rate', `
    $out.coverage.'branches-valid', $out.coverage.'branches-covered', $out.coverage.'branch-rate'
```

- [ ] [P0-T13] Determine and record the exact `mcp__drm-copilot__run_poshqc_test` invocation that yields coverage figures (parameter name and value), and record whether `mcp__drm-copilot__run_poshqc_format` and `..._analyze` accept a path/scope parameter. Write `<FEATURE>/evidence/baseline/poshqc-tool-surface.<TS>.md`. Acceptance: the artifact names the exact coverage-enabling invocation used by P0-T16 and P4-T3, and states explicitly whether path scoping is available. Note that `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` does **not** exist in this worktree; record the settings source the MCP server actually uses.
- [ ] [P0-T14] Run `mcp__drm-copilot__run_poshqc_format` (`workspace_root` = `<ROOT>`), scoped to `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` if the tool supports scoping. Write `<FEATURE>/evidence/baseline/poshqc-format.<TS>.md`. Acceptance: artifact records `EXIT_CODE: 0` and the count of files changed. If any file outside those two paths was modified, restore it with `git checkout -- <path>` and record the restoration in the artifact.
- [ ] [P0-T15] Run `mcp__drm-copilot__run_poshqc_analyze` (`workspace_root` = `<ROOT>`). Write `<FEATURE>/evidence/baseline/poshqc-analyze.<TS>.md`. Acceptance: artifact records `EXIT_CODE:` and the baseline finding count for the two in-scope files.
- [ ] [P0-T16] Run `mcp__drm-copilot__run_poshqc_test` (`workspace_root` = `<ROOT>`) with the coverage invocation recorded in P0-T13. Write `<FEATURE>/evidence/baseline/pester-baseline.<TS>.md`. Acceptance: the artifact records suite totals (passed/failed/skipped), the fact that all **eight** pre-existing `It` blocks in `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` pass, and numeric baseline line-coverage and branch-coverage percentages for `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. No placeholder values.

### Phase 1 — Regression Fixtures Authored and Demonstrated Red

Bugfix Workflow (`CLAUDE.md` § Bugfix Workflow) applies: the regression tests come first and must be demonstrated failing against unmodified `Helpers.ps1`. All six fixtures are **new** `It` blocks appended inside the existing `Describe 'ConvertTo-KoverageCoberturaXml'` block in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. No existing block may be modified. Every fixture uses an inline single-quoted here-string (`@'` ... `'@`), creates no file on disk, uses no mock, and passes `-ProjectNames` explicitly for determinism.

- [ ] [P1-T1] [expect-fail] Add fixture **F1** (issue #441, lines) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`: one package, one class; `<methods>` with one `<method>` carrying lines 10 (`hits=1`), 11 (`hits=0`), 12 (`hits=1`); class-level `<lines>` carrying the identical three. Acceptance: the block asserts root `lines-valid` = `'3'`, `lines-covered` = `'2'` and `line-rate` = `'0.666667'`, and the block is <= 24 lines. It must assert the counts, not the rate alone.
- [ ] [P1-T2] [expect-fail] Add fixture **F2** (issue #441, branches): as F1 plus line 12 carrying `branch="True" condition-coverage="50% (1/2)"` with a `<conditions>` child on **both** axes. Acceptance: the block asserts root `branches-valid` = `'2'` and `branches-covered` = `'1'`; it contains **no** assertion on `branch-rate` alone; the block is <= 28 lines.
- [ ] [P1-T3] [expect-fail] Add fixture **F3** (issue #478, merge): two classes with the same `filename`; primary `Ns.Foo` with `<methods>` lines 56,57,58 (`hits=1`) and class-level `<lines>` 56,57,58; sibling `Ns.Foo.<>c` with `<methods>` lines 12,13 (`hits=0`) and class-level `<lines>` 12,13. Acceptance: the block asserts the merged class `line-rate` = `'0.6'` and that the merged class-level `<lines>` has exactly five `line` children numbered 12, 13, 56, 57, 58 in ascending order; the block is <= 34 lines.
- [ ] [P1-T4] [expect-fail] Add fixture **F4** (`max(hits)` dedup): one class where line 5 appears in `.ctor ()` with `hits=1` and in `.ctor (int)` with `hits=0`, and class-level `<lines>` has line 5 with `hits=1`. Acceptance: the block asserts root `lines-valid` = `'1'` and `lines-covered` = `'1'`; the block is <= 26 lines.
- [ ] [P1-T5] Add fixture **F5** (rollup-absent guard): one class with `<methods>` carrying lines 20 (`hits=1`) and 21 (`hits=0`) and **no class-level `<lines>` element at all**. Acceptance: the block asserts root `lines-valid` = `'2'` and `lines-covered` = `'1'`; the block is <= 24 lines. F5 passes both before and after the fix and is therefore **not** tagged `[expect-fail]`.
- [ ] [P1-T6] Add fixture **F6** (structure preservation): reuse the F3 document. Acceptance: the block asserts the merged class still carries a `<methods>` element with exactly one `<method>` child (the primary's), and that every merged class-level `<line>` retains its input `hits` value (12 -> `'0'`, 13 -> `'0'`, 56 -> `'1'`, 57 -> `'1'`, 58 -> `'1'`); the block is <= 34 lines. F6 passes both before and after the fix and is therefore **not** tagged `[expect-fail]`.
- [ ] [P1-T7] [expect-fail] Run `mcp__drm-copilot__run_poshqc_test` (`workspace_root` = `<ROOT>`) against unmodified `Helpers.ps1` and write `<FEATURE>/evidence/regression-testing/fail-before-f1-f4.<TS>.md`. Acceptance: the artifact records F1 failing with 6/4, F2 failing with 4/2, F3 failing with `'0.75'`, F4 failing with 3/2, F5 and F6 passing, all eight pre-existing blocks passing, and `EXIT_CODE:` non-zero. Exactly four failures are expected; any other failure count halts the plan.
- [ ] [P1-T8] Verify no existing test block was modified and no production file has changed yet. Acceptance: `git diff --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` reports **0 deletions**, and `git diff --name-only edf3d34c -- scripts` returns empty. Record both outputs in `<FEATURE>/evidence/regression-testing/fail-before-f1-f4.<TS>.md`.

```powershell
git diff --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
git diff --name-only edf3d34c -- scripts
```

### Phase 2 — Minimal Fix in Invoke-MSTestWithCoverage.Helpers.ps1

- [ ] [P2-T1] Add the new pure function `Get-CoberturaClassLineSummary` to `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, inserted immediately after `Get-CoberturaLineConditionCoverageParts` (which ends at pre-change line 165) and before `function Merge-CoberturaClassesByFilename`. Acceptance: the function has `[CmdletBinding()]`, `[OutputType([pscustomobject])]`, a single mandatory `[System.Xml.XmlElement]$ClassNode` parameter, and returns exactly `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches`. It performs no I/O and mutates nothing in the source document.

Construction rule (each map entry carries `Node`, `Hits`, `Branch`, `Covered`, `Total`; `Branch` is required to express the "branch=True if either" semantics of AC-6):

```powershell
  # 1. Enumerate ./lines/line (class-level rollup) THEN ./methods/method/lines/line.
  # 2. Key by [int]$node.number. On a repeat key:
  #      Hits    = max(existing, candidate)
  #      Branch  = $true if either entry has branch="True"
  #      Covered/Total taken from the entry with the larger Total, tie-broken by larger Covered,
  #      via the existing pure helper Get-CoberturaLineConditionCoverageParts.
  # 3. TotalLines      = $lineMap.Count
  #    CoveredLines    = count of entries whose Hits -gt 0
  #    TotalBranches   = sum of Total   over entries whose Branch is $true
  #    CoveredBranches = sum of Covered over entries whose Branch is $true
```

- [ ] [P2-T2] Replace the inner loop body of `Get-CoberturaCoverageSummary` (pre-change lines 122-132) with one call to `Get-CoberturaClassLineSummary` per class, accumulating the four returned totals. Acceptance: the function keeps its `[xml]$XmlDocument` signature and its `throw 'Cobertura XML does not contain a <packages> node.'` guard verbatim, keeps the `LineRate`/`BranchRate` rounding (`[math]::Round($covered / $total, 6)`) and the `'0'` zero-denominator fallback, and no longer contains a descendant-axis line selection.

```powershell
foreach ($cls in $pkg.SelectNodes('.//class')) {
    $classSummary = Get-CoberturaClassLineSummary -ClassNode $cls
    $totalLines += $classSummary.TotalLines
    $coveredLines += $classSummary.CoveredLines
    $totalBranches += $classSummary.TotalBranches
    $coveredBranches += $classSummary.CoveredBranches
}
```

- [ ] [P2-T3] Remove the `$classSummaryXml` synthetic-document block at pre-change lines 270-273 in `Merge-CoberturaClassesByFilename` and set the merged class's `line-rate` / `branch-rate` from a direct `Get-CoberturaClassLineSummary` call on `$mergedClassNode`. Acceptance: the token `$classSummaryXml` no longer appears anywhere in the file, no `ImportNode` call remains in that function, and the two rate strings are produced by the identical rounding and zero-denominator expression used in `Get-CoberturaCoverageSummary`. Do **not** introduce a second new function to share the formatting: `spec.md` § Proposed Fix specifies exactly one new helper. Add a short comment recording why the expression is duplicated.

```powershell
$mergedSummary = Get-CoberturaClassLineSummary -ClassNode $mergedClassNode
  # Rate formatting must match Get-CoberturaCoverageSummary exactly; existing assertions
  # such as line-rate | Should -Be '1' depend on the rounding and the '0' fallback.
$mergedLineRate = if ($mergedSummary.TotalLines -gt 0) { [string]([math]::Round($mergedSummary.CoveredLines / $mergedSummary.TotalLines, 6)) } else { '0' }
$mergedBranchRate = if ($mergedSummary.TotalBranches -gt 0) { [string]([math]::Round($mergedSummary.CoveredBranches / $mergedSummary.TotalBranches, 6)) } else { '0' }
$mergedClassNode.SetAttribute('line-rate', $mergedLineRate)
$mergedClassNode.SetAttribute('branch-rate', $mergedBranchRate)
```

- [ ] [P2-T4] Verify the defect is removed at its one site. Acceptance: a grep for the literal `.//lines/line` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` returns **0 matches**, a grep for `$classSummaryXml` returns **0 matches**, and greps for `./lines/line` and `./methods/method/lines/line` inside `Get-CoberturaClassLineSummary` each return **1 match**.
- [ ] [P2-T5] Verify the union builder is untouched. Acceptance: in `git diff -U0 -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, no hunk's old-side range intersects pre-change lines 217-268, and the literal `foreach ($lineNode in @($classNode.SelectNodes('./lines/line')))` still occurs exactly once. Record the hunk headers in `<FEATURE>/evidence/qa-gates/union-builder-byte-identity.<TS>.md`.
- [ ] [P2-T6] Verify `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is unchanged. Acceptance: `git diff --name-only edf3d34c -- scripts/vscode/Invoke-MSTestWithCoverage.ps1` returns empty output.

### Phase 3 — Green Verification and Helper Unit Tests

- [ ] [P3-T1] Run `mcp__drm-copilot__run_poshqc_test` (`workspace_root` = `<ROOT>`) and confirm F1-F6 all pass. Write `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: the artifact records all six fixtures passing with the post-fix values from `spec.md` § Test Strategy and `EXIT_CODE: 0`.
- [ ] [P3-T2] Verify zero existing tests broke. Acceptance: the P3-T1 run shows all **eight** pre-existing `It` blocks passing (including `lines-valid | Should -Be '3'`), and `git diff --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` still reports **0 deletions**. Record both in the P3-T1 artifact.
- [ ] [P3-T3] Add a new `Describe 'Get-CoberturaClassLineSummary'` block to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` with the first precedence-branch `It`: candidate `Total` **greater** than existing. Acceptance: the block builds a minimal `<class>` element inline, calls `Get-CoberturaClassLineSummary -ClassNode` directly, and asserts the candidate's `condition-coverage` values are retained.
- [ ] [P3-T4] Add the second precedence-branch `It`: `Total` **equal** and `Covered` **greater**. Acceptance: the block asserts the candidate's values are retained.
- [ ] [P3-T5] Add the third precedence-branch `It`: **neither** condition holds. Acceptance: the block asserts the existing entry's values are retained.
- [ ] [P3-T6] Add a boundary `It`: a `<class>` element with neither a `<lines>` element nor a `<methods>` element. Acceptance: the block asserts `TotalLines` = 0, `CoveredLines` = 0, `TotalBranches` = 0, `CoveredBranches` = 0 and that no exception is thrown.
- [ ] [P3-T7] Add an error-handling `It`: `Get-CoberturaCoverageSummary` over a document with no `//packages` node. Acceptance: the block asserts it still throws `'Cobertura XML does not contain a <packages> node.'`.
- [ ] [P3-T8] Re-run `mcp__drm-copilot__run_poshqc_test` (`workspace_root` = `<ROOT>`) after the unit-test additions and write `<FEATURE>/evidence/regression-testing/helper-unit-tests.<TS>.md`. Acceptance: zero failures and `EXIT_CODE: 0`.
- [ ] [P3-T9] Check the pre-format test-file line budget. Acceptance: `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is <= 480 lines. If it exceeds 480, compact the here-strings per the Test-File Line Budget section — do **not** create a third test file, which would break AC-18.

### Phase 4 — Final QA Loop and Scope Gates

Type checking is not applicable to PowerShell and is intentionally absent from this loop (`.claude/rules/powershell.md` step 3). Each of P4-T1, P4-T2 and P4-T3 is an unconditional command task; `EXIT_CODE: SKIPPED` is not a valid outcome for any of them.

- [ ] [P4-T1] Run `mcp__drm-copilot__run_poshqc_format` with `workspace_root` = `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348`, scoped to the two in-scope files if the tool supports scoping. Write `<FEATURE>/evidence/qa-gates/poshqc-format.<TS>.md`. Acceptance: `EXIT_CODE: 0` recorded plus the count of files changed. If any file outside the two in-scope paths was modified, restore it with `git checkout -- <path>`, record the restoration, and restart this phase from P4-T1.
- [ ] [P4-T2] Run `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root` = `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348`. Write `<FEATURE>/evidence/qa-gates/poshqc-analyze.<TS>.md`. Acceptance: `EXIT_CODE: 0` and **zero findings** on `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`.
- [ ] [P4-T3] Run `mcp__drm-copilot__run_poshqc_test` with `workspace_root` = `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348`, using the coverage invocation recorded in P0-T13. Write `<FEATURE>/evidence/qa-gates/pester-final.<TS>.md`. Acceptance: `EXIT_CODE: 0`, zero failures, and numeric post-change line-coverage and branch-coverage percentages for `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` recorded in `Output Summary:`. No placeholder values.
- [ ] [P4-T4] Confirm a single clean pass. Acceptance: one consecutive execution of P4-T1 -> P4-T2 -> P4-T3 in which format changed 0 files, analyze reported 0 findings on the two in-scope files, and test reported 0 failures. If any step failed or changed files, restart from P4-T1 and record each attempt as its own artifact.
- [ ] [P4-T5] Run the post-format file-size audit and write `<FEATURE>/evidence/qa-gates/file-size-audit.<TS>.md`. Acceptance: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` < 500 lines and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` < 500 lines, both counts recorded numerically.

```powershell
Get-ChildItem -LiteralPath `
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1', `
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1' |
    ForEach-Object { '{0}: {1}' -f $_.Name, (Get-Content -LiteralPath $_.FullName).Count }
```

- [ ] [P4-T6] Write the coverage delta artifact `<FEATURE>/evidence/qa-gates/coverage-delta.<TS>.md` comparing the P0-T16 baseline against the P4-T3 post-change figures for `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Acceptance: the artifact records baseline line/branch coverage, post-change line/branch coverage, and confirms post-change line coverage >= 85% and branch coverage >= 75% (`.claude/rules/general-unit-test.md`) with no regression versus baseline. Where `CLAUDE.md` and `general-unit-test.md` differ, the stricter figure is recorded; no threshold is modified anywhere.
- [ ] [P4-T7] Write `<FEATURE>/evidence/other/helper-branch-test-map.<TS>.md` mapping every branch of `Get-CoberturaClassLineSummary` to the named test that exercises it (new-key insert, repeat-key `max(hits)`, repeat-key branch promotion, precedence `Total` greater, precedence `Total` equal / `Covered` greater, precedence neither, empty class). Acceptance: every listed branch names at least one `It` block, satisfying the >= 90% new-code expectation in `CLAUDE.md` § UT2 by explicit enumeration.
- [ ] [P4-T8] Run the scope-lock diff gate and write `<FEATURE>/evidence/qa-gates/scope-lock.<TS>.md`. Acceptance: `git diff --name-only edf3d34c -- scripts tests` lists **exactly** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and nothing else. The gate is scoped to `scripts` and `tests` deliberately: `docs/` and `.claude/agent-memory/` are tracked and legitimately change during this work.

```powershell
git diff --name-only edf3d34c -- scripts tests
```

- [ ] [P4-T9] Run the no-threshold-change gate and write `<FEATURE>/evidence/qa-gates/threshold-no-change.<TS>.md`. Acceptance: `git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config` returns empty output, and a grep of the two changed source files for the tokens `85`, `90`, `75` in a threshold context returns no newly introduced threshold statement.

```powershell
git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config
```

- [ ] [P4-T10] Re-verify union-builder byte identity **after** formatting and append the result to `<FEATURE>/evidence/qa-gates/union-builder-byte-identity.<TS>.md`. Acceptance: `git diff -U0 -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` still shows no hunk whose old-side range intersects pre-change lines 217-268. If the formatter reflowed any line in that range, restore those lines to their pre-change bytes and restart from P4-T1.

### Phase 5 — Post-Change Evidence and Oracle Verification

If any figure in P5-T1 or P5-T2 does not match its required value, return to Phase 2, correct the implementation, and re-execute Phases 3, 4 and 5 in full.

- [ ] [P5-T1] Re-run the generator-parity A/B command from P0-T11 against the fixed `Helpers.ps1` and write `<FEATURE>/evidence/qa-gates/postchange-generator-parity.<TS>.md`. Acceptance: the artifact records `LinesValid = 79957`, `LinesCovered = 56124`, `BranchesValid = 23109`, `BranchesCovered = 13472` exactly, reproducing the input document's own root attributes.
- [ ] [P5-T2] Re-run the package-filtered A/B command from P0-T12 against the fixed `Helpers.ps1` and write `<FEATURE>/evidence/qa-gates/postchange-package-filtered.<TS>.md`. Acceptance: the artifact records `lines-valid = 62345`, `lines-covered = 53013` and `line-rate = 0.850317`, alongside the pre-change values 110849 / 94937 / 0.856453.
- [ ] [P5-T3] Write the consolidated A/B delta artifact `<FEATURE>/evidence/qa-gates/coverage-arithmetic-delta.<TS>.md`. Acceptance: the artifact tabulates pre-change versus post-change for both experiments using the concrete integers captured in P0-T11, P0-T12, P5-T1 and P5-T2, and states that each pre-change figure is strictly greater than its post-change counterpart.
- [ ] [P5-T4] Write the threshold handoff record `<FEATURE>/evidence/other/threshold-handoff-494.<TS>.md`. Acceptance: the artifact states as fact that the corrected repository-wide line rate for the #424 committed sample is 85.0317% against the uniform 85% line floor in `.claude/rules/general-unit-test.md` — a margin of 0.03 percentage points — identifies child feature #494 as the owner of threshold reconciliation, and states explicitly that this feature proposes and makes no threshold change.
- [ ] [P5-T5] Audit evidence locations and schema. Acceptance: every artifact produced by this plan resides under `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,issue-updates,other}/`, each command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, and a repository search confirms no artifact was written under any `artifacts/` path. Record the audit in `<FEATURE>/evidence/other/evidence-location-audit.<TS>.md`.

### Phase 6 — Follow-Up Issue Filing

Each follow-up is filed through the MCP promotion lifecycle (`mcp__drm-copilot__new_potential_bug_entry` then `mcp__drm-copilot__potential_to_issue`), not left as prose. None of the four is fixed in this change.

- [ ] [P6-T1] File follow-up candidate 1: package-level `line-rate` / `branch-rate` are never recomputed after package filtering and class merging in `ConvertTo-KoverageCoberturaXml`, leaving stale values consumed by `scripts/temp-extract-coverage.ps1:47`. Acceptance: a GitHub issue number is returned and recorded.
- [ ] [P6-T2] File follow-up candidate 2: a merged Cobertura class retains only the primary class's `<methods>`, so the emitted document's methods do not account for all of its class-level lines; merging carries a duplicate `(name, signature)` hazard on compiler-generated sibling classes. Acceptance: a GitHub issue number is returned and recorded.
- [ ] [P6-T3] File follow-up candidate 3: `scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` lacks a `\.claude\` discovery exclusion, so `-SearchRoot .` descends into `.claude\worktrees\agent-*\**` and picks up stale sibling-worktree assemblies. Acceptance: a GitHub issue number is returned and recorded.
- [ ] [P6-T4] File follow-up candidate 4: `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36` records an incorrect generalization — root attributes are deduped only in raw `dotnet-coverage` output, not in post-processed `ConvertTo-KoverageCoberturaXml` artifacts. Acceptance: a GitHub issue number is returned and recorded.
- [ ] [P6-T5] Write `<FEATURE>/evidence/issue-updates/followups-441.<TS>.md` recording all four issue numbers, their titles, `PostedAs:`, and the GitHub URLs. Acceptance: the artifact lists exactly four issue numbers, one per candidate.
- [ ] [P6-T6] Confirm none of the four follow-ups was fixed in this change. Acceptance: the P4-T8 scope-lock output still lists exactly two source files, and `git diff --name-only edf3d34c -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/temp-extract-coverage.ps1` returns empty output.

### Phase 7 — Acceptance Criteria Check-Off and Commit

AC source is `<FEATURE>/spec.md` § Acceptance Criteria (work mode `full-bug`). Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, change only `- [ ]` to `- [x]` and never alter criterion text. One AC per task, each citing its own evidence pointer.

- [ ] [P7-T1] Check off **AC-1** (generator parity) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/postchange-generator-parity.<TS>.md`. Acceptance: AC-1 is `[x]` and the cited artifact shows 79957 / 56124 / 23109 / 13472.
- [ ] [P7-T2] Check off **AC-2** (pre-change figure) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/baseline/prechange-generator-parity.<TS>.md`. Acceptance: AC-2 is `[x]` and the cited artifact shows `LinesValid = 161086` plus three concrete integers each strictly greater than its AC-1 counterpart.
- [ ] [P7-T3] Check off **AC-3** (package-filtered A/B) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/postchange-package-filtered.<TS>.md`. Acceptance: AC-3 is `[x]` and the cited artifact shows 62345 / 53013 / 0.850317 against 110849 / 94937 / 0.856453.
- [ ] [P7-T4] Check off **AC-4** (per-file merged rate) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-4 is `[x]` and F3 passes with `line-rate` = `'0.6'` and five ascending line children 12, 13, 56, 57, 58.
- [ ] [P7-T5] Check off **AC-5** (branch counts deduplicated) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-5 is `[x]`, F2 asserts `branches-valid` = `'2'` and `branches-covered` = `'1'`, and no branch assertion in the suite relies on `branch-rate` alone.
- [ ] [P7-T6] Check off **AC-6** (helper contract) in `<FEATURE>/spec.md`, citing `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `<FEATURE>/evidence/other/helper-branch-test-map.<TS>.md`. Acceptance: AC-6 is `[x]` and the helper matches the stated signature, enumeration order, key rule, precedence rule and five returned properties.
- [ ] [P7-T7] Check off **AC-7** (defect removed at its one site) in `<FEATURE>/spec.md`, citing the P2-T4 grep results. Acceptance: AC-7 is `[x]` and `.//lines/line` returns 0 matches in the production file.
- [ ] [P7-T8] Check off **AC-8** (correct site untouched) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/union-builder-byte-identity.<TS>.md`. Acceptance: AC-8 is `[x]` and the artifact records the post-format re-verification from P4-T10.
- [ ] [P7-T9] Check off **AC-9** (delegation replaced) in `<FEATURE>/spec.md`, citing the P2-T4 grep for `$classSummaryXml`. Acceptance: AC-9 is `[x]` and the token returns 0 matches.
- [ ] [P7-T10] Check off **AC-10** (structure preserved) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-10 is `[x]` and F6 passes.
- [ ] [P7-T11] Check off **AC-11** (six fixtures present and passing) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-11 is `[x]`, all six fixtures pass, and none creates a file on disk or mocks an arithmetic path.
- [ ] [P7-T12] Check off **AC-12** (fail-before evidence) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/fail-before-f1-f4.<TS>.md`. Acceptance: AC-12 is `[x]` and the artifact records F1 6/4, F2 4/2, F3 `'0.75'`, F4 3/2 against unmodified `Helpers.ps1`.
- [ ] [P7-T13] Check off **AC-13** (helper precedence branches covered) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/helper-unit-tests.<TS>.md`. Acceptance: AC-13 is `[x]` and all three precedence-branch tests pass.
- [ ] [P7-T14] Check off **AC-14** (zero existing tests broken) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/baseline/pester-baseline.<TS>.md` and `<FEATURE>/evidence/qa-gates/pester-final.<TS>.md`. Acceptance: AC-14 is `[x]`, all eight pre-existing blocks pass in both runs, and the test-file diff shows 0 deletions.
- [ ] [P7-T15] Check off **AC-15** (toolchain green) in `<FEATURE>/spec.md`, citing the three P4 artifacts `poshqc-format.<TS>.md`, `poshqc-analyze.<TS>.md` and `pester-final.<TS>.md`. Acceptance: AC-15 is `[x]` and the artifacts record a single clean pass.
- [ ] [P7-T16] Check off **AC-16** (canonical evidence locations) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/other/evidence-location-audit.<TS>.md`. Acceptance: AC-16 is `[x]` and the audit shows no `artifacts/` evidence path and complete schema fields.
- [ ] [P7-T17] Check off **AC-17** (no threshold re-tuned) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/threshold-no-change.<TS>.md` and `<FEATURE>/evidence/other/threshold-handoff-494.<TS>.md`. Acceptance: AC-17 is `[x]` and the diff gate returned empty output.
- [ ] [P7-T18] Check off **AC-18** (scope boundary held) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/scope-lock.<TS>.md`. Acceptance: AC-18 is `[x]` and the gate lists exactly the two in-scope source files.
- [ ] [P7-T19] Check off **AC-19** (file ceiling) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/file-size-audit.<TS>.md`. Acceptance: AC-19 is `[x]` and `Invoke-MSTestWithCoverage.Helpers.ps1` is recorded under 500 lines.
- [ ] [P7-T20] Check off **AC-20** (follow-ups filed) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/issue-updates/followups-441.<TS>.md`. Acceptance: AC-20 is `[x]` and four issue numbers are recorded, none of them fixed in this change.
- [ ] [P7-T21] Write the AC status summary to `<FEATURE>/evidence/other/ac-status-summary.<TS>.md` in the format required by `.claude/skills/acceptance-criteria-tracking/SKILL.md` (Source, Total AC items, Checked off, Remaining, Items remaining). Acceptance: the artifact reports Source = `<FEATURE>/spec.md`, Total = 20, and lists any unchecked item explicitly.
- [ ] [P7-T22] Commit all changes on branch `bug/cobertura-coverage-arithmetic-441` with the message `fix(coverage): dedupe Cobertura line and branch arithmetic (#441, #478)`. Acceptance: `git status --porcelain` returns empty output after the commit, and `git show --stat HEAD` lists the two source files plus the feature documents and evidence artifacts. No PR is created and no CI run is triggered from this plan.
