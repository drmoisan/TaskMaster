# 2026-08-10-cobertura-coverage-arithmetic-441 (Plan)

- **Issue:** #441 (also closes #478)
- **Work Mode:** full-bug
- **AC Source:** `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Acceptance Criteria (AC-1 .. AC-20)
- **Epic:** `build-ci-coverage-gate-fidelity` (wave 0)
- **Integration Branch:** `epic/build-ci-coverage-gate-fidelity-integration`
- **Feature Branch:** `bug/cobertura-coverage-arithmetic-441`
- **Base Commit:** `edf3d34c`
- **Feature Folder (`<FEATURE>`):** `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T18-24
- **Status:** Ready for preflight
- **Version:** 1.1

**`<ROOT>` is a placeholder, not a literal path.** This plan is authored in one worktree and executed later, by `epic-orchestrator`, in a different one. Every absolute path below is therefore expressed as `<ROOT>\...`. Before running any task, resolve `<ROOT>` once to the absolute path of the worktree the executing agent is actually running in, and use that value for the remainder of the plan:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
```

Acceptance for the resolution itself: `$root` names a directory that contains both `scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` and `docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\spec.md`. If either is absent, halt — the executor is in the wrong worktree. The resolved value is recorded in the Phase 0 artifact by P0-T8. Do not substitute a path copied from this document's revision history.

**Working directory does not persist between tool invocations.** Every command snippet in this plan therefore begins by re-resolving `$root` and running `Set-Location $root`. Relative paths inside these snippets are repo-root-relative by construction and are only correct after that `Set-Location`. A snippet run without it will fail or, worse, resolve against an unrelated directory.

**Base-commit note.** `edf3d34c` remains the correct diff base after the epic integration tip was merged into this branch. Verified 2026-08-10T18-24: `git merge-base --is-ancestor edf3d34c HEAD` succeeds, and `git diff --name-only edf3d34c HEAD -- scripts tests` and `... -- CLAUDE.md .claude/rules coverage.config` both return empty. The merge added only a sibling feature's `docs/` folder, an epic-manifest edit and agent-memory files, none of which any gate in this plan inspects.

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

**Evidence location invariant (non-overridable).** Every artifact this plan produces lives under `<FEATURE>/evidence/<kind>/` where `<kind>` is one of `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`. No `artifacts/` evidence path is permitted. Each command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. `<TS>` below means an ISO-8601 stamp in the form `yyyy-MM-ddTHH-mm`.

**Toolchain (PowerShell only).** Format -> analyze -> test, per `.claude/rules/powershell.md`. Type checking is **not applicable to PowerShell** and no type-check task appears in this plan. Every MCP toolchain call passes `workspace_root` = the resolved `<ROOT>`.

1. `mcp__drm-copilot__run_poshqc_format`
2. `mcp__drm-copilot__run_poshqc_analyze`
3. `mcp__drm-copilot__run_poshqc_test`

**Every MCP toolchain call in this plan MUST pass `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.** `config/poshqc-scan.json` does not exist in this repository, so the unscoped default set is not repo-defined and must not be relied on.

**Scoping does not make the analyzer call clean, and the analyzer MCP call is not the gate.** Verified 2026-08-10: `run_poshqc_analyze` returns `ok:false` with `PSScriptAnalyzer reported 16 issue(s)` for **any** scope that includes `scripts/vscode`, including the scope this plan mandates; scoped to `tests/scripts/vscode` alone it returns `ok:true`. All 16 findings are pre-existing, they span six files, and exactly one of them is in a file this feature touches:

| File | Rule | Count |
| --- | --- | --- |
| `Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | 3 |
| `Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | 3 |
| `Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | 2 |
| **`Invoke-MSTestWithCoverage.Helpers.ps1`** | **PSUseSingularNouns** | **1** |
| `Invoke-Restore.ps1` | PSAvoidUsingWriteHost | 1 |
| `Invoke-VSBuild.ps1` | PSAvoidUsingWriteHost | 1 |
| `Invoke-VSBuild.ps1` | PSUseSingularNouns | 2 |
| `Sync-PackageReferences.ps1` | PSAvoidUsingWriteHost | 3 |

**The MCP analyze call therefore cannot itself serve as the gate at any scope.** The gate is the per-file `Invoke-ScriptAnalyzer` breakdown in P0-T15 and P4-T2; the MCP call is executed and its `ok` and `summary` recorded verbatim to satisfy the No-SKIPPED rule. Its non-zero exit is expected at every invocation and is never on its own a failure.

**`scripts/powershell/PoshQC/settings/pester.runsettings.psd1` does not exist in this repository.** The bundled MCP PoshQC server supplies its own settings. Verified 2026-08-10T18-24: `run_poshqc_test` with `scan_folders` = `["tests/scripts/vscode"]` returns `ok:true` regardless. Do not attempt to create, restore, or point at that settings file.

Restart from step 1 whenever any step fails or changes files. **No C# toolchain command appears in this plan** (csharpier, msbuild, vstest are all out of scope; no C# file is touched). In particular `/p:Nullable=enable` is a known-defective documented command (issue #522) and must not be invoked.

## Verified Environment Facts (established 2026-08-10T18-24; do not re-derive, do not silently override)

These four facts were measured in a clean checkout of this branch. Each one previously appeared in this plan as a different value, and each wrong value would have halted or falsely failed execution.

| Fact | Verified value | Command used |
| --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` line count | **357** | `(Get-Content -LiteralPath ...).Count` |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` line count | **222** (not 223) | `(Get-Content -LiteralPath ...).Count` |
| Pre-existing PSScriptAnalyzer findings on `Invoke-MSTestWithCoverage.Helpers.ps1` | **exactly 1** — `PSUseSingularNouns`, line 146, `Get-CoberturaLineConditionCoverageParts` | `Invoke-ScriptAnalyzer -Path scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` |
| Pre-existing PSScriptAnalyzer findings on `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | **0** | `run_poshqc_analyze` scoped to `tests/scripts/vscode` returns `ok:true` |

The single `PSUseSingularNouns` finding is **out of scope and must not be fixed.** Clearing it requires renaming the exported function `Get-CoberturaLineConditionCoverageParts`, which `spec.md` § Implementation strategy lists as **Unmodified** and which § Technical specifications forbids ("No exported function signature changes"). It is baselined in Phase 0 and excluded by name from the Phase 4 analyzer gate.

## Test Verdict and Coverage Measurement Contract (verified; supersedes any assumption that the MCP test tool reports a verdict or coverage)

**`run_poshqc_test` carries no verdict.** Verified 2026-08-10: with `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]` it returns `{"ok":true,...,"summary":"Ran bundled PoshQC test against '<root>' with 2 selected scan folder(s)."}` and nothing else — no counts, no test names, no exit code, no failure detail. It returns `ok:true` whether the suite is green or red, and it writes no coverage artifact into the workspace (`git status --porcelain` is empty immediately afterwards). **Every pass/fail verdict, failure count and per-fixture actual value in this plan is therefore attributed to the direct Pester run, never to the MCP call.** The MCP call is still executed at every point the plan names it, and its `ok` and `summary` fields are recorded verbatim in the corresponding artifact to satisfy the No-SKIPPED rule; `EXIT_CODE:` in every test artifact means `PESTER_EXIT_CODE` from the direct run, not the MCP payload.

The direct run supplies everything the gates need and is verified to work:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
Import-Module Pester -MinimumVersion 5.0 -Force
$coverageXmlPath = Join-Path $root '<FEATURE>\evidence\<kind>\pester-coverage.<TS>.xml'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $coverageXmlPath) | Out-Null
$c = New-PesterConfiguration
$c.Run.Path                    = 'tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1'
$c.Run.PassThru                = $true
$c.CodeCoverage.Enabled        = $true
$c.CodeCoverage.Path           = 'scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1'
$c.CodeCoverage.OutputFormat   = 'JaCoCo'
$c.CodeCoverage.OutputPath     = $coverageXmlPath
$c.Output.Verbosity            = 'Detailed'
$c.Should.ErrorAction          = 'Continue'   # report EVERY failed assertion in an It, not just the first
$r = Invoke-Pester -Configuration $c
'Total={0} Passed={1} Failed={2}' -f $r.TotalCount, $r.PassedCount, $r.FailedCount
$r.Tests | ForEach-Object {
    '{0} :: {1}{2}' -f $_.Result, $_.ExpandedName, $(
        if ($_.Result -ne 'Passed') { ' :: ' + $_.ErrorRecord.Exception.Message } else { '' })
}
'PESTER_EXIT_CODE=' + $(if ($r.FailedCount -gt 0) { 1 } else { 0 })
([xml](Get-Content -LiteralPath $coverageXmlPath -Raw)).report.counter |
    ForEach-Object { '{0}: missed={1} covered={2}' -f $_.type, $_.missed, $_.covered }
```

Verified pre-change result on this branch: Pester **5.6.1**; **8 passed, 0 failed** (the eight pre-existing `It` blocks); JaCoCo counters `INSTRUCTION 170/192`, `LINE 146 covered / 19 missed` = **88.48%**, `METHOD 7/7`, `CLASS 1/1`.

`$c.Should.ErrorAction = 'Continue'` is required, not optional. Pester 5 defaults it to `Stop`, which aborts an `It` at its **first** failed assertion; under that default a fail-before run would report only F1's `lines-valid` actual and never its `lines-covered` actual, making the paired pre-fix figures (6/4, 4/2, 3/2) unobservable and P1-T7's acceptance unsatisfiable. Setting it to `Continue` reports every failed assertion within an `It`. It does **not** change `FailedCount`, which counts tests rather than assertions, so every count-based acceptance in this plan is unaffected.

**A `FailedCount` of 0 is never sufficient on its own.** A run that discovers no tests also reports zero failures. Every test acceptance in this plan therefore pairs `FailedCount` with an expected `TotalCount`, and a `TotalCount` of 0 fails the gate.

**Branch coverage is not measurable for PowerShell in this repository.** Pester 5.6.1 emits `INSTRUCTION`, `LINE`, `METHOD` and `CLASS` counters and **no `BRANCH` counter**. The >= 75% branch floor in `.claude/rules/general-unit-test.md` therefore has no available instrument here. This is recorded as an auditable negative-evidence claim (P4-T6), not passed over silently and not used to justify a threshold change. The coverage XML written by these runs is **evidence**, not a temporary file, and lives under `<FEATURE>/evidence/<kind>/` like every other artifact; the "no temporary files" rule in `.claude/rules/general-unit-test.md` constrains test code, which creates nothing on disk in this plan.

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

## Fixture XML Attribute Rule (mandatory — measured 2026-08-10)

Every `<line>` element in every fixture authored by this plan — Phase 1 fixtures F1-F6 and
the inline `<class>` elements built by P3-T3..P3-T6 — MUST carry an explicit `branch`
attribute on whichever of the `<methods>/<method>/<lines>` axis and the class-level
`<lines>` axis the fixture actually contains. Use `branch="False"` unless the fixture
specification calls for `branch="True"`.

This is not stylistic. `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:1`
sets `Set-StrictMode -Version Latest`, which propagates into the production functions
dot-sourced in `BeforeAll`. Under that mode, property access on a missing XML attribute
throws instead of returning `$null`, and `Helpers.ps1:128` reads `$line.branch` by bare
property access. Measured against unmodified `Helpers.ps1`: an F1-shaped fixture without
`branch` fails with `The property 'branch' cannot be found on this object`, whereas the
same fixture with `branch="False"` fails with the intended `Expected: '3' But was: '6'` /
`Expected: '2' But was: '4'`. All eight pre-existing fixtures already carry
`branch="False"` on every line, which is why the hazard is invisible from the current file.

Every `<class>` element in a fixture that exercises the class-merge path — F3 and F6,
the two fixtures with two classes sharing one `filename` — MUST additionally carry an
explicit `complexity` attribute (any integer, e.g. `complexity="1"`).
`Merge-CoberturaClassesByFilename` sums group complexity at `Helpers.ps1:277-281` via
the bare property read `$_.complexity`. Measured 2026-08-10 against unmodified
`Helpers.ps1`: an F3-shaped fixture whose `<class>` elements omit `complexity` fails
with `The property 'complexity' cannot be found on this object` instead of the intended
`Expected: '0.6' But was: '0.75'`. It fails the same way *after* the fix, because P2-T3
replaces only pre-change lines 270-273 and leaves the complexity accumulator in place,
so this also breaks P3-T1's 14/14 green gate. F1, F2, F4 and F5 have a single class
each, never enter that loop, and do not need the attribute. Every pre-existing merge
fixture in the test file already carries `complexity`, which is why the hazard is
invisible from the current file.

Adding the attribute changes no line count and therefore does not affect the per-block
budgets in § Test-File Line Budget.

## Test-File Line Budget (mandatory)

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is 222 lines today (verified 2026-08-10T18-24); the 500-line ceiling in `.claude/rules/general-code-change.md` applies to test code. AC-18 pins the change to exactly two source files, so **the overflow must not be resolved by adding a third test file.** Write the six fixture here-strings compactly: collapse `<methods>`, `<method>` and the method's `<lines>` wrapper onto single lines and keep one `<line>` element per line only inside the class-level `<lines>` rollup. Per-block budgets: F1 <= 24, F2 <= 28, F3 <= 34, F4 <= 26, F5 <= 24, F6 <= 34, helper unit-test `Describe` <= 80. Enforced by P3-T9 (pre-format) and P4-T5 (post-format).

### Phase 0 — Baseline Capture and Policy Reads

- [x] [P0-T1] Read `<ROOT>\CLAUDE.md` in full. Acceptance: file read and its path recorded in the Phase 0 artifact file list.
- [x] [P0-T2] Read `<ROOT>\.claude\rules\general-code-change.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [x] [P0-T3] Read `<ROOT>\.claude\rules\general-unit-test.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [x] [P0-T4] Read `<ROOT>\.claude\rules\powershell.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [x] [P0-T5] Read `<ROOT>\docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\spec.md` in full and record that it contains exactly 20 unchecked AC items numbered AC-1 through AC-20. Acceptance: the count 20 is recorded in the Phase 0 artifact.
- [x] [P0-T6] Read `<ROOT>\docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\research\2026-08-10T14-20-cobertura-arithmetic-research.md` in full. Acceptance: file read and path recorded in the Phase 0 artifact file list.
- [x] [P0-T7] Read `<ROOT>\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` and `<ROOT>\tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and record their pre-change line counts, measured with the exact command below so the figure is reproducible. Acceptance: the recorded counts are exactly **357** and **222**. Any other value halts the plan for re-baselining. The test-file figure is 222, not 223: verified 2026-08-10T18-24 by both `(Get-Content).Count` and `wc -l`, with the file confirmed to terminate in a newline.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
foreach ($p in @(
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')) {
    '{0}: {1}' -f $p, (Get-Content -LiteralPath (Join-Path $root $p)).Count
}
```

- [x] [P0-T8] Write `<FEATURE>/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` (CLAUDE.md -> general-code-change.md -> general-unit-test.md -> powershell.md), the **resolved absolute value of `<ROOT>`** together with the `git rev-parse --show-toplevel` output it came from, and the explicit list of every file read in P0-T1..P0-T7. Acceptance: the file exists and contains all four fields plus seven file paths, and the recorded `<ROOT>` is the worktree the executor is actually running in.
- [x] [P0-T9] Record the git baseline to `<FEATURE>/evidence/baseline/git-baseline.<TS>.md`: current branch, `git rev-parse HEAD`, `git rev-parse edf3d34c`, and `git status --porcelain`. Acceptance: artifact records the branch as `bug/cobertura-coverage-arithmetic-441`, confirms `git merge-base --is-ancestor edf3d34c HEAD` succeeds, and captures the porcelain output verbatim. The HEAD sha is recorded as an observation only and is never used as a later expectation. **If the branch name does not match, halt** — the worktree was provisioned incorrectly. Do not create, switch, or rename a branch from within this plan; branch provisioning belongs to `epic-orchestrator`.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
git rev-parse edf3d34c
git merge-base --is-ancestor edf3d34c HEAD; 'ancestor-check-exit=' + $LASTEXITCODE
git status --porcelain
```

- [x] [P0-T10] Verify both committed sample documents exist and are readable at `<ROOT>\docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml` and `...\evidence\qa-gates\coverage-final.cobertura.xml`. Acceptance: both paths resolve; if either is absent the plan halts (the A/B evidence method depends on them).
- [x] [P0-T11] Capture the PRE-CHANGE generator-parity A/B against unmodified `Helpers.ps1` and write `<FEATURE>/evidence/baseline/prechange-generator-parity.<TS>.md`. Acceptance: the artifact records `LinesValid` exactly `161086` plus `LinesCovered`, `BranchesValid` and `BranchesCovered` as concrete integers (no placeholder), alongside the input document's own root attributes `79957 / 56124 / 23109 / 13472`. This is a deterministic A/B over a fixed committed input, not a test-suite run.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
. (Join-Path $root 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$sample = Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml'
[xml]$doc = Get-Content -LiteralPath $sample -Raw -Encoding UTF8
'INPUT root: lines-valid={0} lines-covered={1} branches-valid={2} branches-covered={3}' -f `
    $doc.coverage.'lines-valid', $doc.coverage.'lines-covered', $doc.coverage.'branches-valid', $doc.coverage.'branches-covered'
Get-CoberturaCoverageSummary -XmlDocument $doc | Format-List
```

- [x] [P0-T12] Capture the PRE-CHANGE package-filtered A/B by reprocessing `coverage-final.cobertura.xml` through `ConvertTo-KoverageCoberturaXml`, and write `<FEATURE>/evidence/baseline/prechange-package-filtered.<TS>.md`. Acceptance: the artifact records `lines-valid = 110849`, `lines-covered = 94937`, `line-rate = 0.856453` as concrete values. Allow a generous timeout: the input is ~186,913 lines and the `[xml]` cast materializes a full DOM.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
. (Join-Path $root 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$sample = Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\qa-gates\coverage-final.cobertura.xml'
$content = Get-Content -LiteralPath $sample -Raw -Encoding UTF8
[xml]$out = ConvertTo-KoverageCoberturaXml -XmlContent $content -RepoRoot $root -PathSeparator '\'
'lines-valid={0} lines-covered={1} line-rate={2} branches-valid={3} branches-covered={4} branch-rate={5}' -f `
    $out.coverage.'lines-valid', $out.coverage.'lines-covered', $out.coverage.'line-rate', `
    $out.coverage.'branches-valid', $out.coverage.'branches-covered', $out.coverage.'branch-rate'
```

- [x] [P0-T13] Confirm the tool surface described in § Coverage Measurement Contract still holds in the executing worktree, and write `<FEATURE>/evidence/baseline/poshqc-tool-surface.<TS>.md`. Acceptance: the artifact records (a) that `mcp__drm-copilot__run_poshqc_format`, `..._analyze` and `..._test` all accept a `scan_folders` array and that this plan passes `["scripts/vscode", "tests/scripts/vscode"]` to each; (b) that `run_poshqc_test` returns a summary string and writes no coverage artifact, evidenced by an empty `git status --porcelain` immediately after a scoped call; (c) that `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` and `config/poshqc-scan.json` are both absent and that the bundled MCP settings are used instead; and (d) the installed Pester version, which must be >= 5.0. If any of (a)-(d) differs from the recorded values, halt and re-baseline rather than improvising an invocation.
- [x] [P0-T14] Run `mcp__drm-copilot__run_poshqc_format` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`. Write `<FEATURE>/evidence/baseline/poshqc-format.<TS>.md`. Acceptance: artifact records `EXIT_CODE:` and the set of files changed, measured by the two-instrument method below. If any file other than the two in-scope paths and `.claude/agent-memory/**` was modified, restore it with `git checkout -- <path>` and record the restoration in the artifact. `.claude/agent-memory/**` is tracked in this repository and is legitimately written by the executing agent; exclude it from the out-of-scope changed-file set and never `git checkout --` it. `EXIT_CODE:` records `0` when the payload is `ok:true` and `1` otherwise, and the payload's `summary` is quoted verbatim. See P4-T1 for the two-instrument snippet; the same method applies here.
- [x] [P0-T15] Run `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`, then run `Invoke-ScriptAnalyzer` directly on each of the two in-scope files to obtain a per-file, per-rule breakdown the MCP summary does not provide. Write `<FEATURE>/evidence/baseline/poshqc-analyze.<TS>.md`. Acceptance: the artifact records `EXIT_CODE:` for the MCP call and an explicit **per-file baseline finding list** for the two in-scope files. The expected baseline, verified 2026-08-10T18-24, is exactly one finding on `Invoke-MSTestWithCoverage.Helpers.ps1` (`PSUseSingularNouns`, line 146, `Get-CoberturaLineConditionCoverageParts`) and zero on `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. This recorded set is the baseline that P4-T2 diffs against. Do not fix the `PSUseSingularNouns` finding — see § Verified Environment Facts. **Record the baseline keyed on `(ScriptName, RuleName, Severity, Message)` and record `Line` as an observation only, excluded from the key.** P2-T2 shortens `Get-CoberturaCoverageSummary`'s inner loop, which shifts every later declaration upward, so the `PSUseSingularNouns` finding will not still be on line 146 after the change. A line-number move on an otherwise-identical finding is not a new finding.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
foreach ($p in @(
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')) {
    Invoke-ScriptAnalyzer -Path (Join-Path $root $p) |
        Select-Object ScriptName, RuleName, Severity, Line, Message | Format-List
}
```

- [x] [P0-T16] Capture the baseline test run and numeric coverage. Run `mcp__drm-copilot__run_poshqc_test` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]` for the suite verdict, then run the direct Pester capture from § Coverage Measurement Contract with `OutputPath` = `<FEATURE>/evidence/baseline/pester-coverage-baseline.<TS>.xml` for the numbers. Write `<FEATURE>/evidence/baseline/pester-baseline.<TS>.md`. Acceptance: the artifact records suite totals (passed/failed/skipped), the fact that all **eight** pre-existing `It` blocks in `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` pass, and the four JaCoCo counters (`INSTRUCTION`, `LINE`, `METHOD`, `CLASS`) as concrete missed/covered integers plus the derived baseline **line-coverage percentage**. No placeholder values. The expected baseline, verified 2026-08-10T18-24, is 8 passed / 0 failed and `LINE` 146 covered / 19 missed = 88.48%. Record explicitly that Pester emits **no `BRANCH` counter**, listing the counters actually present as the proof.

- [x] [P0-T17] Verify the plan is running against the amended, satisfiable spec rather than a superseded one. Read `<FEATURE>/spec.md` § Acceptance Criteria. Acceptance: AC-15 contains the phrase `absent from the recorded Phase 0 baseline`, and AC-16 enumerates `issue-updates` and `other` among the permitted evidence sub-paths. If either phrase is absent, **halt** — the plan's Phase 4 and Phase 7 gates were written against amended AC text and would otherwise check off criteria that cannot be satisfied. Do not edit `spec.md` to make this task pass; a mismatch is a planning defect requiring a plan and spec revision, not an execution workaround.
- [x] [P0-T18] Prove `spec.md` Assumption 2 (method-level line numbers are a subset of the class-level rollup) on both committed sample documents **before** any implementation, so that a document drift fails at baseline rather than after Phase 2. Perform a read-only streaming pass over each sample with an `XmlReader`, applying the union and `max(hits)` rule this plan specifies, and write `<FEATURE>/evidence/baseline/assumption2-subset-proof.<TS>.md`. Acceptance: for `coverage-baseline.cobertura.xml` the artifact records class-level distinct = 79957, union distinct = 79957, union covered = 56124, union branches valid/covered = 23109/13472, and **method-only line keys = 0**; for `coverage-final.cobertura.xml` it records class-level distinct = 62345, union distinct = 62345, union covered = 53013, and **method-only line keys = 0**. Use a streaming reader, not an `[xml]` cast: these documents are 17 MB and 10 MB. A non-zero method-only key count is a **spec-level** finding — it means the union design and the class-level oracle disagree — and requires a plan and spec revision. It is explicitly **not** an implementation defect and must **not** trigger the Phase 5 return-to-Phase-2 loop.

### Phase 1 — Regression Fixtures Authored and Demonstrated Red

Bugfix Workflow (`CLAUDE.md` § Bugfix Workflow) applies: the regression tests come first and must be demonstrated failing against unmodified `Helpers.ps1`. All six fixtures are **new** `It` blocks appended inside the existing `Describe 'ConvertTo-KoverageCoberturaXml'` block in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. No existing block may be modified. Every fixture uses an inline single-quoted here-string (`@'` ... `'@`), creates no file on disk, uses no mock, and passes `-ProjectNames` explicitly for determinism.

- [x] [P1-T1] [expect-fail] Add fixture **F1** (issue #441, lines) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`: one package, one class; `<methods>` with one `<method>` carrying lines 10 (`hits=1`), 11 (`hits=0`), 12 (`hits=1`); class-level `<lines>` carrying the identical three. Acceptance: the block asserts root `lines-valid` = `'3'`, `lines-covered` = `'2'` and `line-rate` = `'0.666667'`, and the block is <= 24 lines. It must assert the counts, not the rate alone.
- [x] [P1-T2] [expect-fail] Add fixture **F2** (issue #441, branches): as F1 (lines 10 and 11 retain `branch="False"`) plus line 12 carrying `branch="True" condition-coverage="50% (1/2)"` with a `<conditions>` child on **both** axes. Acceptance: the block asserts root `branches-valid` = `'2'` and `branches-covered` = `'1'`; it contains **no** assertion on `branch-rate` alone; the block is <= 28 lines.
- [x] [P1-T3] [expect-fail] Add fixture **F3** (issue #478, merge): two classes with the same `filename`; primary `Ns.Foo` with `<methods>` lines 56,57,58 (`hits=1`) and class-level `<lines>` 56,57,58; sibling `Ns.Foo.<>c` with `<methods>` lines 12,13 (`hits=0`) and class-level `<lines>` 12,13. Acceptance: the block asserts the merged class `line-rate` = `'0.6'` and that the merged class-level `<lines>` has exactly five `line` children numbered 12, 13, 56, 57, 58 in ascending order; the block is <= 34 lines.
- [x] [P1-T4] [expect-fail] Add fixture **F4** (`max(hits)` dedup): one class where line 5 appears in `.ctor ()` with `hits=1` and in `.ctor (int)` with `hits=0`, and class-level `<lines>` has line 5 with `hits=1`. Acceptance: the block asserts root `lines-valid` = `'1'` and `lines-covered` = `'1'`; the block is <= 26 lines.
- [x] [P1-T5] Add fixture **F5** (rollup-absent guard): one class with `<methods>` carrying lines 20 (`hits=1`) and 21 (`hits=0`) and **no class-level `<lines>` element at all**. Acceptance: the block asserts root `lines-valid` = `'2'` and `lines-covered` = `'1'`; the block is <= 24 lines. F5 passes both before and after the fix and is therefore **not** tagged `[expect-fail]`.
- [x] [P1-T6] Add fixture **F6** (structure preservation): reuse the F3 document. Acceptance: the block asserts the merged class still carries a `<methods>` element with exactly one `<method>` child (the primary's), and that every merged class-level `<line>` retains its input `hits` value (12 -> `'0'`, 13 -> `'0'`, 56 -> `'1'`, 57 -> `'1'`, 58 -> `'1'`); the block is <= 34 lines. F6 passes both before and after the fix and is therefore **not** tagged `[expect-fail]`.
- [x] [P1-T7] [expect-fail] Run `mcp__drm-copilot__run_poshqc_test` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]` against unmodified `Helpers.ps1` and record its `ok` and `summary` verbatim as non-probative, **then** run the direct Pester capture from § Test Verdict and Coverage Measurement Contract for the actual verdict. Write `<FEATURE>/evidence/regression-testing/fail-before-f1-f4.<TS>.md`. Acceptance: the direct run reports `FailedCount` = **4** and `PassedCount` = **10**, and its per-test listing names F1, F2, F3 and F4 as the four failures with `ErrorRecord` messages showing 6/4, 4/2, `'0.75'` and 3/2 respectively, while F5, F6 and all eight pre-existing blocks are listed `Passed`; `EXIT_CODE: 1` (`PESTER_EXIT_CODE`). Any other failure count halts the plan. The MCP payload must not be used to establish any of these values — it returns `ok:true` whether the suite is green or red.
- [x] [P1-T8] Verify the fixtures were actually written, that no existing test block was modified, and that no production file has changed yet. Acceptance: `git diff HEAD --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` emits **exactly one** record whose additions field is **>= 100** and whose deletions field is **exactly 0**; an empty result **fails** this gate, because an unmodified file also emits nothing and would otherwise satisfy a bare "0 deletions" reading while proving the fixtures were never written. The `HEAD` operand makes the check immune to staging. `git diff --name-only edf3d34c -- scripts` returns empty. Append both outputs to the artifact created by P1-T7, reusing its `<TS>` — do not create a second file.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff HEAD --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
git diff --name-only edf3d34c -- scripts
```

### Phase 2 — Minimal Fix in Invoke-MSTestWithCoverage.Helpers.ps1

- [x] [P2-T1] Add the new pure function `Get-CoberturaClassLineSummary` to `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, inserted immediately after `Get-CoberturaLineConditionCoverageParts` (which ends at pre-change line 165) and before `function Merge-CoberturaClassesByFilename`. Acceptance: the function has `[CmdletBinding()]`, `[OutputType([pscustomobject])]`, a single mandatory `[System.Xml.XmlElement]$ClassNode` parameter, and returns exactly `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches`. It performs no I/O and mutates nothing in the source document.

Construction rule (each map entry carries `Node`, `Hits`, `Branch`, `Covered`, `Total`; `Branch` is required to express the "branch=True if either" semantics of AC-6):

```powershell
  # 1. Enumerate ./lines/line (class-level rollup) THEN ./methods/method/lines/line.
  # 2. Key by [int]$node.number. On a repeat key:
  #      Hits    = max(existing, candidate)
  #      Branch  = $true if either entry has branch="True", read as
  #                $node.GetAttribute('branch') -eq 'True' (NOT $node.branch).
  #                Bare property access throws under Set-StrictMode -Version Latest when the
  #                attribute is absent; the existing union builder at :236 already uses
  #                GetAttribute for exactly this reason. Read hits the same way.
  #      Covered/Total taken from the entry with the larger Total, tie-broken by larger Covered,
  #      via the existing pure helper Get-CoberturaLineConditionCoverageParts.
  # 3. TotalLines      = $lineMap.Count
  #    CoveredLines    = count of entries whose Hits -gt 0
  #    TotalBranches   = sum of Total   over entries whose Branch is $true
  #    CoveredBranches = sum of Covered over entries whose Branch is $true
```

- [x] [P2-T2] Replace the inner loop body of `Get-CoberturaCoverageSummary` (pre-change lines 122-132) with one call to `Get-CoberturaClassLineSummary` per class, accumulating the four returned totals. Acceptance: the function keeps its `[xml]$XmlDocument` signature and its `throw 'Cobertura XML does not contain a <packages> node.'` guard verbatim, keeps the `LineRate`/`BranchRate` rounding (`[math]::Round($covered / $total, 6)`) and the `'0'` zero-denominator fallback, and no longer contains a descendant-axis line selection.

```powershell
foreach ($cls in $pkg.SelectNodes('.//class')) {
    $classSummary = Get-CoberturaClassLineSummary -ClassNode $cls
    $totalLines += $classSummary.TotalLines
    $coveredLines += $classSummary.CoveredLines
    $totalBranches += $classSummary.TotalBranches
    $coveredBranches += $classSummary.CoveredBranches
}
```

- [x] [P2-T3] Remove the `$classSummaryXml` synthetic-document block at pre-change lines 270-273 in `Merge-CoberturaClassesByFilename` and set the merged class's `line-rate` / `branch-rate` from a direct `Get-CoberturaClassLineSummary` call on `$mergedClassNode`. Acceptance: the token `$classSummaryXml` no longer appears anywhere in the file, no `ImportNode` call remains in that function, and the two rate strings are produced by the identical rounding and zero-denominator expression used in `Get-CoberturaCoverageSummary`. Do **not** introduce a second new function to share the formatting: `spec.md` § Proposed Fix specifies exactly one new helper. Add a short comment recording why the expression is duplicated.

```powershell
$mergedSummary = Get-CoberturaClassLineSummary -ClassNode $mergedClassNode
  # Rate formatting must match Get-CoberturaCoverageSummary exactly; existing assertions
  # such as line-rate | Should -Be '1' depend on the rounding and the '0' fallback.
$mergedLineRate = if ($mergedSummary.TotalLines -gt 0) { [string]([math]::Round($mergedSummary.CoveredLines / $mergedSummary.TotalLines, 6)) } else { '0' }
$mergedBranchRate = if ($mergedSummary.TotalBranches -gt 0) { [string]([math]::Round($mergedSummary.CoveredBranches / $mergedSummary.TotalBranches, 6)) } else { '0' }
$mergedClassNode.SetAttribute('line-rate', $mergedLineRate)
$mergedClassNode.SetAttribute('branch-rate', $mergedBranchRate)
```

- [x] [P2-T4] Verify the defect is removed at its one site, **using fixed-string search only** (`rg -F` or `Select-String -SimpleMatch`) over `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Acceptance: the literal `'.//lines/line'` (including its surrounding single quotes) returns **0** matches file-wide; the literal `$classSummaryXml` returns **0** matches file-wide; the literal `'./lines/line'` (including quotes) returns **exactly 2** matches file-wide — one inside `Get-CoberturaClassLineSummary` and the pre-existing one in the union builder; and the literal `'./methods/method/lines/line'` (including quotes) returns **exactly 1**. Regex search must not be used here: an unescaped `.` matches any character, so the pattern `./lines/line` also matches the substring `d/lines/line` inside `./methods/method/lines/line` and a correct implementation would return 2 where the gate expected 1.
- [x] [P2-T5] Verify the union builder is untouched. Acceptance: in `git diff HEAD -U0 -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, no hunk's old-side range intersects pre-change lines 217-268, and the literal `foreach ($lineNode in @($classNode.SelectNodes('./lines/line')))` still occurs exactly once. The `HEAD` operand makes the check immune to staging. Record the hunk headers in `<FEATURE>/evidence/qa-gates/union-builder-byte-identity.<TS>.md`.
- [x] [P2-T6] Verify `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is unchanged. Acceptance: `git diff --name-only edf3d34c -- scripts/vscode/Invoke-MSTestWithCoverage.ps1` returns empty output.

### Phase 3 — Green Verification and Helper Unit Tests

- [x] [P3-T1] Run `mcp__drm-copilot__run_poshqc_test` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`, recording its `ok` and `summary` separately and explicitly marked **non-probative**, then run the direct Pester capture from § Test Verdict and Coverage Measurement Contract for the verdict. Write `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: the direct run reports `FailedCount` = 0, `PassedCount` = **14** and `TotalCount` = **14** — eight pre-existing blocks plus F1-F6; the five helper unit tests added by P3-T3..P3-T7 do not exist yet, so 19 is not the expected figure at this point. F1-F6 are individually listed `Passed` in the per-test output with their post-fix values from `spec.md` § Test Strategy recorded; `EXIT_CODE: 0` (`PESTER_EXIT_CODE`). A `TotalCount` of 0 fails this gate: `FailedCount` = 0 alone is also satisfied by a run that discovered no tests.
- [x] [P3-T2] Verify zero existing tests broke. Acceptance: the P3-T1 direct-run per-test listing names all **eight** pre-existing `It` blocks individually with `Result = Passed` (including the block asserting `lines-valid | Should -Be '3'`), quoted verbatim into the artifact; and `git diff HEAD --numstat -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` emits **exactly one** record whose deletions field is **exactly 0** and whose additions field is **>= 100**. An empty result fails this gate. Record both in the P3-T1 artifact.
- [x] [P3-T3] Add a new `Describe 'Get-CoberturaClassLineSummary'` block to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` with the first precedence-branch `It`: candidate `Total` **greater** than existing. Acceptance: the block builds a minimal `<class>` element inline, calls `Get-CoberturaClassLineSummary -ClassNode` directly, and asserts the candidate's `condition-coverage` values are retained.
- [x] [P3-T4] Add the second precedence-branch `It`: `Total` **equal** and `Covered` **greater**. Acceptance: the block asserts the candidate's values are retained.
- [x] [P3-T5] Add the third precedence-branch `It`: **neither** condition holds. Acceptance: the block asserts the existing entry's values are retained.
- [x] [P3-T6] Add a boundary `It`: a `<class>` element with neither a `<lines>` element nor a `<methods>` element. Acceptance: the block asserts `TotalLines` = 0, `CoveredLines` = 0, `TotalBranches` = 0, `CoveredBranches` = 0 and that no exception is thrown.
- [x] [P3-T7] Add an error-handling `It` **inside the existing `Describe 'ConvertTo-KoverageCoberturaXml'` block**, not inside the new `Describe 'Get-CoberturaClassLineSummary'` block created by P3-T3: `Get-CoberturaCoverageSummary` over a document with no `//packages` node. Acceptance: the block asserts it still throws `'Cobertura XML does not contain a <packages> node.'`, and it does not count against the 80-line budget for the `Get-CoberturaClassLineSummary` `Describe`.
- [x] [P3-T8] Re-run `mcp__drm-copilot__run_poshqc_test` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]` after the unit-test additions, recording its `ok` and `summary` as non-probative, then run the direct Pester capture for the verdict. Write `<FEATURE>/evidence/regression-testing/helper-unit-tests.<TS>.md`. Acceptance: the direct run reports `FailedCount` = 0, `PassedCount` = **19** and `TotalCount` = **19** — eight pre-existing blocks, F1-F6, and the five blocks added by P3-T3..P3-T7 — with every added precedence-branch and boundary `It` listed `Passed` by name; `EXIT_CODE: 0` (`PESTER_EXIT_CODE`). A `TotalCount` of 0 fails this gate.
- [x] [P3-T9] Check the pre-format test-file line budget. Acceptance: `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is <= 480 lines. If it exceeds 480, compact the here-strings per the Test-File Line Budget section — do **not** create a third test file, which would break AC-18.

### Phase 4 — Final QA Loop and Scope Gates

Type checking is not applicable to PowerShell and is intentionally absent from this loop (`.claude/rules/powershell.md` step 3). Each of P4-T1, P4-T2 and P4-T3 is an unconditional command task; `EXIT_CODE: SKIPPED` is not a valid outcome for any of them.

- [x] [P4-T1] Run `mcp__drm-copilot__run_poshqc_format` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`. Write `<FEATURE>/evidence/qa-gates/poshqc-format.<TS>.md`. Acceptance: artifact records `EXIT_CODE:` plus the set of files changed. If any file other than the two in-scope paths and `.claude/agent-memory/**` was modified, restore it with `git checkout -- <path>`, record the restoration, and restart this phase from P4-T1. `.claude/agent-memory/**` is tracked in this repository and is legitimately written by the executing agent; exclude it from the out-of-scope changed-file set and never `git checkout --` it. **The restart is bounded.** `scan_folders` covers all of `scripts/vscode`, so the formatter may deterministically rewrite an unrelated pre-existing file such as `Install-RepoDotNetSdk.ps1`; restore-and-restart would then never terminate and P4-T4 would be unsatisfiable. If P0-T14 recorded the same out-of-scope file as formatter-modified, treat it as pre-existing formatting drift: restore it, record it in the artifact as known non-blocking drift, and proceed **without** restarting. Only an out-of-scope file that was *not* modified at P0-T14 triggers a restart. `EXIT_CODE:` records `0` when the payload is `ok:true` and `1` otherwise, and the payload's `summary` is quoted verbatim.

**Change detection uses two instruments, because neither alone is sufficient.** The MCP payload reports no changed-file count at all. And by Phase 4 both in-scope files are already ` M` relative to `HEAD`, so if the formatter rewrites one of them its `git status --porcelain` line is *identical* before and after and the porcelain difference is empty — the gate would report "0 files changed" precisely when a file did change. Therefore: detect **in-scope** changes by SHA-256 content hash before and after the call, and use the porcelain before/after difference **only** to detect modification of files outside the two in-scope paths, which are clean and so do appear as new entries.

The two halves below run in **separate tool invocations** with the MCP format call between them, and shell state does not persist across invocations. Part 1 therefore **prints** its values and they are transcribed into the artifact; part 2 re-measures and is compared against the transcribed values, not against a live variable.

Part 1, before the format call:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
$targets = @('scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
             'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')
foreach ($t in $targets) {
    '{0}: before={1}' -f $t, (Get-FileHash -LiteralPath (Join-Path $root $t) -Algorithm SHA256).Hash
}
'--- porcelain before ---'
(git status --porcelain) | Out-String
```

Now run `mcp__drm-copilot__run_poshqc_format`. Then part 2:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
$targets = @('scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
             'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')
foreach ($t in $targets) {
    '{0}: after={1}' -f $t, (Get-FileHash -LiteralPath (Join-Path $root $t) -Algorithm SHA256).Hash
}
'--- porcelain after ---'
(git status --porcelain) | Out-String
```

Compare the two hash listings and the two porcelain listings **in the artifact**. An in-scope file whose `before` and `after` hashes differ was changed by the formatter; a porcelain entry present only in the `after` listing, other than under `.claude/agent-memory/**`, is an out-of-scope modification.
- [x] [P4-T2] Run `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`, then re-run the per-file `Invoke-ScriptAnalyzer` breakdown from P0-T15. Write `<FEATURE>/evidence/qa-gates/poshqc-analyze.<TS>.md`. Acceptance: **zero NEW findings** on `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` relative to the P0-T15 baseline — that is, the post-change per-file finding set is a subset of the baseline set. **The comparison is keyed on `(ScriptName, RuleName, Severity, Message)` and excludes `Line`**, because P2-T2 shortens `Get-CoberturaCoverageSummary`'s inner loop and will shift the `PSUseSingularNouns` finding off line 146; a line-number move on an otherwise-identical finding is not a new finding. The single pre-existing `PSUseSingularNouns` finding on `Get-CoberturaLineConditionCoverageParts` is **expected to persist** and its persistence is not a failure; clearing it would require renaming an exported function that `spec.md` marks Unmodified. A zero-findings acceptance is deliberately **not** used here because it is unsatisfiable within scope. The MCP call's non-zero exit is **expected at every invocation** and does not fail this gate: 16 pre-existing findings exist under `scripts/vscode` across six files, of which exactly one is in an in-scope file. Only a finding absent from the P0-T15 baseline fails this gate.
- [x] [P4-T3] Run `mcp__drm-copilot__run_poshqc_test` with `workspace_root` = `<ROOT>` and `scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`, recording its `ok` and `summary` as non-probative, then run the direct Pester capture from § Test Verdict and Coverage Measurement Contract with `OutputPath` = `<FEATURE>/evidence/qa-gates/pester-coverage-final.<TS>.xml`. Write `<FEATURE>/evidence/qa-gates/pester-final.<TS>.md`. Acceptance: the direct run reports `FailedCount` = 0 with `PassedCount` = `TotalCount` = **19 + N**, where **N** is the number of additional `It` blocks added under the P4-T6 remediation path and is **0** on the first pass through this phase; the value of N in force is stated in the artifact. A `TotalCount` of 0, or any `TotalCount` below 19, fails this gate, since zero discovery also yields zero failures. `EXIT_CODE: 0` (`PESTER_EXIT_CODE`); and the four JaCoCo counters plus the derived post-change **line-coverage percentage** for `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` are recorded in `Output Summary:` as concrete integers. No placeholder values. Branch coverage is recorded as tool-unsupported per P4-T6, not as a number.
- [x] [P4-T4] Confirm a single clean pass. Acceptance: one consecutive execution of P4-T1 -> P4-T2 -> P4-T3 in which format changed 0 files — evidenced by **identical SHA-256 hashes for both in-scope files before and after the P4-T1 invocation** and an empty `git status --porcelain` before/after difference for every other path except `.claude/agent-memory/**` — analyze reported no new findings on the two in-scope files relative to the P0-T15 baseline, and the direct Pester run reported `FailedCount` = 0 with `PassedCount` = `TotalCount` = 19 + N (N as defined in P4-T3; N = 0 unless the P4-T6 remediation path has fired). If any step failed or changed files, restart from P4-T1 and record each attempt as its own artifact.
- [x] [P4-T5] Run the post-format file-size audit and write `<FEATURE>/evidence/qa-gates/file-size-audit.<TS>.md`. Acceptance: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` < 500 lines and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` < 500 lines, both counts recorded numerically.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
Get-ChildItem -LiteralPath `
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1', `
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1' |
    ForEach-Object { '{0}: {1}' -f $_.Name, (Get-Content -LiteralPath $_.FullName).Count }
```

- [x] [P4-T6] Write the coverage delta artifact `<FEATURE>/evidence/qa-gates/coverage-delta.<TS>.md` comparing the P0-T16 baseline against the P4-T3 post-change figures for `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Acceptance, line coverage: the artifact records baseline and post-change `LINE` counters as concrete missed/covered integers plus both derived percentages, and confirms post-change line coverage is **>= 85%** (`.claude/rules/general-unit-test.md`) with **no regression** versus the baseline of 88.48%. Where `CLAUDE.md` and `general-unit-test.md` differ, the stricter figure is recorded; no threshold is modified anywhere. Acceptance, branch coverage: the >= 75% branch floor is **not measurable with the available instrument**, and the artifact must say so as an auditable negative-evidence claim rather than reporting a number or silently omitting the floor. Record `SearchScope:` the Pester JaCoCo report at the P4-T3 `OutputPath`; `SearchPatterns:` `report/counter[@type='BRANCH']`; `SearchResult:` `none`, together with the full list of counter types the report does contain (`INSTRUCTION`, `LINE`, `METHOD`, `CLASS`) and the Pester version. State explicitly that this limitation is a property of the PowerShell coverage tooling, is not caused by this change, and is **not** grounds for altering any threshold — thresholds are owned by #494. Acceptance, new-code coverage: the artifact additionally records the first and last line numbers of `Get-CoberturaClassLineSummary` in the post-change file, the count of JaCoCo `<line>` records in that range with `ci > 0` and with `ci = 0`, and the derived new-code line-coverage percentage, which must be **>= 90%** per `CLAUDE.md` § UT2 and must show no regression on changed lines per `.claude/rules/powershell.md`. `$first` and `$last` must be recorded in the artifact as concrete integers, and a zero-length line set **fails** the gate rather than reporting a percentage. Compute it from the P4-T3 `OutputPath` report with this self-contained snippet — it defines every variable it uses, because shell state does not persist between tool invocations:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
$coverageXmlPath = Join-Path $root '<FEATURE>\evidence\qa-gates\pester-coverage-final.<TS>.xml'   # the P4-T3 OutputPath
$srcPath = 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
$src     = Get-Content -LiteralPath (Join-Path $root $srcPath)
$first   = ($src | Select-String -SimpleMatch 'function Get-CoberturaClassLineSummary').LineNumber
if (-not $first) { throw 'Get-CoberturaClassLineSummary not found; cannot compute new-code coverage.' }
$last    = <closing-brace line number of that function; determine it by reading the file and record it in the artifact>
$jc      = [xml](Get-Content -LiteralPath $coverageXmlPath -Raw)
$lines   = @($jc.report.package.sourcefile.line | Where-Object { [int]$_.nr -ge $first -and [int]$_.nr -le $last })
if ($lines.Count -eq 0) { throw 'No JaCoCo line records in the helper range; the range or the report path is wrong.' }
$cov = @($lines | Where-Object { [int]$_.ci -gt 0 }).Count
'new-code: {0}/{1} = {2:P2}  (first={3} last={4})' -f $cov, $lines.Count, ($cov / $lines.Count), $first, $last
```

**Remediation path if a figure is below its floor.** If the post-change whole-file line rate is below 88.48% or the new-code rate is below 90%, add further direct unit tests for the uncovered lines of `Get-CoberturaClassLineSummary` inside the `Describe` created by P3-T3, staying within its 80-line budget, then restart the QA loop from P4-T1. Do **not** lower a threshold, do **not** exclude any line from measurement, and do **not** report the shortfall as a pass. Record the number of `It` blocks added as **N** and carry it into the P4-T3 and P4-T4 acceptance counts on the restarted pass; a resulting count of 19 + N is not a gate failure.

- [x] [P4-T7] Write `<FEATURE>/evidence/other/helper-branch-test-map.<TS>.md` mapping every branch of `Get-CoberturaClassLineSummary` to the named test that exercises it (new-key insert, repeat-key `max(hits)`, repeat-key branch promotion, precedence `Total` greater, precedence `Total` equal / `Covered` greater, precedence neither, empty class). Acceptance: every listed branch names at least one `It` block. This artifact is a **scenario-completeness map, not a coverage measurement**; the numeric >= 90% new-code proof is carried by P4-T6. Do not claim that an enumeration of branches satisfies a coverage threshold.
- [x] [P4-T8] Run the scope-lock diff gate and write `<FEATURE>/evidence/qa-gates/scope-lock.<TS>.md`. Acceptance: `git diff --name-only edf3d34c -- scripts tests` lists **exactly** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and nothing else. The gate is scoped to `scripts` and `tests` deliberately: `docs/` and `.claude/agent-memory/` are tracked and legitimately change during this work.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff --name-only edf3d34c -- scripts tests
```

- [x] [P4-T9] Run the no-threshold-change gate and write `<FEATURE>/evidence/qa-gates/threshold-no-change.<TS>.md`. Acceptance: `git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config` returns empty output; **and** every added line in the two changed source files that contains the token `85`, `90` or `75` is enumerated in the artifact with a one-line justification. The gate fails if any added line expresses a coverage threshold rather than a fixture line number, a `hits` value or a count. The judgment is bounded to the enumerated matches, so the check is mechanically reproducible.

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config
git diff HEAD -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 |
    Select-String '^\+' | Select-String -Pattern '\b(85|90|75)\b'
```

- [x] [P4-T10] Re-verify union-builder byte identity **after** formatting and append the result to `<FEATURE>/evidence/qa-gates/union-builder-byte-identity.<TS>.md`. Acceptance: `git diff HEAD -U0 -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` still shows no hunk whose old-side range intersects pre-change lines 217-268. The `HEAD` operand makes the check immune to staging. If the formatter reflowed any line in that range, restore those lines to their pre-change bytes and restart from P4-T1.

### Phase 5 — Post-Change Evidence and Oracle Verification

If any figure in P5-T1 or P5-T2 does not match its required value, return to Phase 2, correct the implementation, and re-execute Phases 3, 4 and 5 in full.

- [x] [P5-T1] Re-run the generator-parity A/B command from P0-T11 against the fixed `Helpers.ps1` and write `<FEATURE>/evidence/qa-gates/postchange-generator-parity.<TS>.md`. Acceptance: the artifact records `LinesValid = 79957`, `LinesCovered = 56124`, `BranchesValid = 23109`, `BranchesCovered = 13472` exactly, reproducing the input document's own root attributes.
- [x] [P5-T2] Re-run the package-filtered A/B command from P0-T12 against the fixed `Helpers.ps1` and write `<FEATURE>/evidence/qa-gates/postchange-package-filtered.<TS>.md`. Acceptance: the artifact records `lines-valid = 62345`, `lines-covered = 53013` and `line-rate = 0.850317`, alongside the pre-change values 110849 / 94937 / 0.856453.
- [x] [P5-T3] Write the consolidated A/B delta artifact `<FEATURE>/evidence/qa-gates/coverage-arithmetic-delta.<TS>.md`. Acceptance: the artifact tabulates pre-change versus post-change for both experiments using the concrete integers captured in P0-T11, P0-T12, P5-T1 and P5-T2, and states that each pre-change figure is strictly greater than its post-change counterpart.
- [x] [P5-T4] Write the threshold handoff record `<FEATURE>/evidence/other/threshold-handoff-494.<TS>.md`. Acceptance: the artifact states as fact that the corrected repository-wide line rate for the #424 committed sample is 85.0317% against the uniform 85% line floor in `.claude/rules/general-unit-test.md` — a margin of 0.03 percentage points — identifies child feature #494 as the owner of threshold reconciliation, and states explicitly that this feature proposes and makes no threshold change.
- [x] [P5-T5] Audit evidence locations and schema for the artifacts that exist at this point. Acceptance: every artifact produced by **Phases 0 through 5** resides under `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,issue-updates,other}/`, each command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, and a repository search confirms no artifact was written under any `artifacts/` path. Record the audit in `<FEATURE>/evidence/other/evidence-location-audit.<TS>.md`. This sweep deliberately excludes the Phase 6 and Phase 7 artifacts, which do not exist yet; they are covered by the final sweep in P7-T20, and AC-16 is certified against that final sweep, not against this one.

### Phase 6 — Follow-Up Issue Filing

Each follow-up is filed through the MCP promotion lifecycle (`mcp__drm-copilot__new_potential_bug_entry` then `mcp__drm-copilot__potential_to_issue`), not left as prose. None of the four is fixed in this change.

**Availability branch (applies to P6-T1 through P6-T4).** If the promotion tool reports that `gh` is unavailable or unauthenticated, **or the promotion MCP tools are not exposed in the executing session at all**, **do not fabricate an issue number.** Record the prepared title and body verbatim in the P6-T5 artifact under a `POSTING BLOCKED` header together with the tool's exact error text, leave AC-20 unchecked, and report the blockage in the completion summary. This is the only sanctioned non-numeric outcome for Phase 6. Under this branch P6-T1..P6-T4 **are** checked off, on the basis of the recorded `POSTING BLOCKED` entry in the P6-T5 artifact — the task's obligation is to attempt the filing and record the outcome truthfully. Only AC-20 remains unchecked, because only AC-20 asserts that issue numbers exist.

- [x] [P6-T1] File follow-up candidate 1: package-level `line-rate` / `branch-rate` are never recomputed after package filtering and class merging in `ConvertTo-KoverageCoberturaXml`, leaving stale values consumed by `scripts/temp-extract-coverage.ps1:47`. Acceptance: a GitHub issue number is returned and recorded.
- [x] [P6-T2] File follow-up candidate 2: a merged Cobertura class retains only the primary class's `<methods>`, so the emitted document's methods do not account for all of its class-level lines; merging carries a duplicate `(name, signature)` hazard on compiler-generated sibling classes. Acceptance: a GitHub issue number is returned and recorded.
- [x] [P6-T3] File follow-up candidate 3: `scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` lacks a `\.claude\` discovery exclusion, so `-SearchRoot .` descends into `.claude\worktrees\agent-*\**` and picks up stale sibling-worktree assemblies. Acceptance: a GitHub issue number is returned and recorded.
- [x] [P6-T4] File follow-up candidate 4: `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36` records an incorrect generalization — root attributes are deduped only in raw `dotnet-coverage` output, not in post-processed `ConvertTo-KoverageCoberturaXml` artifacts. Acceptance: a GitHub issue number is returned and recorded.
- [x] [P6-T5] Write `<FEATURE>/evidence/issue-updates/followups-441.<TS>.md` recording each candidate's title, `PostedAs:`, and either its issue number and GitHub URL or a `POSTING BLOCKED` header. Acceptance: the artifact lists exactly four entries, one per candidate, each carrying **either** an issue number and URL **or** a `POSTING BLOCKED` header with the blocking error and the prepared body verbatim.
- [x] [P6-T6] Confirm none of the four follow-ups was fixed in this change. Acceptance: the P4-T8 scope-lock output still lists exactly two source files, and `git diff --name-only edf3d34c -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/temp-extract-coverage.ps1` returns empty output.

### Phase 7 — Acceptance Criteria Check-Off and Commit

AC source is `<FEATURE>/spec.md` § Acceptance Criteria (work mode `full-bug`). Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, change only `- [ ]` to `- [x]` and never alter criterion text. One AC per task, each citing its own evidence pointer.

- [x] [P7-T1] Check off **AC-1** (generator parity) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/postchange-generator-parity.<TS>.md`. Acceptance: AC-1 is `[x]` and the cited artifact shows 79957 / 56124 / 23109 / 13472.
- [x] [P7-T2] Check off **AC-2** (pre-change figure) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/baseline/prechange-generator-parity.<TS>.md`. Acceptance: AC-2 is `[x]` and the cited artifact shows `LinesValid = 161086` plus three concrete integers each strictly greater than its AC-1 counterpart.
- [x] [P7-T3] Check off **AC-3** (package-filtered A/B) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/postchange-package-filtered.<TS>.md`. Acceptance: AC-3 is `[x]` and the cited artifact shows 62345 / 53013 / 0.850317 against 110849 / 94937 / 0.856453.
- [x] [P7-T4] Check off **AC-4** (per-file merged rate) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-4 is `[x]` and F3 passes with `line-rate` = `'0.6'` and five ascending line children 12, 13, 56, 57, 58.
- [x] [P7-T5] Check off **AC-5** (branch counts deduplicated) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-5 is `[x]`, F2 asserts `branches-valid` = `'2'` and `branches-covered` = `'1'`, and no branch assertion in the suite relies on `branch-rate` alone.
- [x] [P7-T6] Check off **AC-6** (helper contract) in `<FEATURE>/spec.md`, citing `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `<FEATURE>/evidence/other/helper-branch-test-map.<TS>.md`. Acceptance: AC-6 is `[x]` and the helper matches the stated signature, enumeration order, key rule, precedence rule and five returned properties.
- [x] [P7-T7] Check off **AC-7** (defect removed at its one site) in `<FEATURE>/spec.md`, citing the P2-T4 grep results. Acceptance: AC-7 is `[x]` and `.//lines/line` returns 0 matches in the production file.
- [x] [P7-T8] Check off **AC-8** (correct site untouched) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/union-builder-byte-identity.<TS>.md`. Acceptance: AC-8 is `[x]` and the artifact records the post-format re-verification from P4-T10.
- [x] [P7-T9] Check off **AC-9** (delegation replaced) in `<FEATURE>/spec.md`, citing the P2-T4 grep for `$classSummaryXml`. Acceptance: AC-9 is `[x]` and the token returns 0 matches.
- [x] [P7-T10] Check off **AC-10** (structure preserved) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-10 is `[x]` and F6 passes.
- [x] [P7-T11] Check off **AC-11** (six fixtures present and passing) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md`. Acceptance: AC-11 is `[x]`, all six fixtures pass, and none creates a file on disk or mocks an arithmetic path.
- [x] [P7-T12] Check off **AC-12** (fail-before evidence) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/fail-before-f1-f4.<TS>.md`. Acceptance: AC-12 is `[x]` and the artifact records F1 6/4, F2 4/2, F3 `'0.75'`, F4 3/2 against unmodified `Helpers.ps1`.
- [x] [P7-T13] Check off **AC-13** (helper precedence branches covered) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/regression-testing/helper-unit-tests.<TS>.md`. Acceptance: AC-13 is `[x]` and all three precedence-branch tests pass.
- [x] [P7-T14] Check off **AC-14** (zero existing tests broken) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/baseline/pester-baseline.<TS>.md`, `<FEATURE>/evidence/qa-gates/pester-final.<TS>.md` and `<FEATURE>/evidence/regression-testing/pass-after-f1-f6.<TS>.md` (which carries the 0-deletions numstat record written by P3-T2). Acceptance: AC-14 is `[x]`, all eight pre-existing blocks pass in both runs, and the test-file diff shows 0 deletions.
- [x] [P7-T15] Check off **AC-15** (toolchain green) in `<FEATURE>/spec.md`, citing the three P4 artifacts `poshqc-format.<TS>.md`, `poshqc-analyze.<TS>.md` and `pester-final.<TS>.md`. Acceptance: AC-15 is `[x]` and the artifacts record a single clean pass.
- [x] [P7-T16] Check off **AC-17** (no threshold re-tuned) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/threshold-no-change.<TS>.md` and `<FEATURE>/evidence/other/threshold-handoff-494.<TS>.md`. Acceptance: AC-17 is `[x]` and the diff gate returned empty output.
- [x] [P7-T17] Check off **AC-18** (scope boundary held) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/scope-lock.<TS>.md`. Acceptance: AC-18 is `[x]` and the gate lists exactly the two in-scope source files.
- [x] [P7-T18] Check off **AC-19** (file ceiling) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/qa-gates/file-size-audit.<TS>.md`. Acceptance: AC-19 is `[x]` and `Invoke-MSTestWithCoverage.Helpers.ps1` is recorded under 500 lines.
- [x] [P7-T19] Check off **AC-20** (follow-ups filed) in `<FEATURE>/spec.md`, citing `<FEATURE>/evidence/issue-updates/followups-441.<TS>.md`. Acceptance: AC-20 is `[x]` **only if** four issue numbers are recorded, none of them fixed in this change. If any candidate carries a `POSTING BLOCKED` header instead of an issue number, **leave AC-20 unchecked** and record the reason in the P7-T22 summary; do not check off a criterion the evidence does not support.
- [ ] [P7-T20] Re-run the evidence-location and schema audit over the **full** artifact set, including the Phase 6 and Phase 7 artifacts that did not exist at P5-T5, and append the result to `<FEATURE>/evidence/other/evidence-location-audit.<TS>.md` under a `## Final sweep` heading, reusing the P5-T5 timestamp so a single audit artifact carries both sweeps. Acceptance: the appended section enumerates every file under `<FEATURE>/evidence/` by path, confirms each command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, and records `SearchScope:`, `SearchPatterns:` `artifacts/**` and `SearchResult:` `none` for the forbidden-path search. This task runs **before** the AC-16 check-off so that the criterion is certified against evidence that already exists. Note in the appended section that exactly one artifact is written after this sweep — `<FEATURE>/evidence/other/ac-status-summary.<TS>.md` from P7-T22 — and state its intended path so the audit trail is complete; it is not a command-step artifact and carries no `EXIT_CODE:` field.
- [ ] [P7-T21] Check off **AC-16** (canonical evidence locations) in `<FEATURE>/spec.md`, citing the `## Final sweep` section of `<FEATURE>/evidence/other/evidence-location-audit.<TS>.md` written by P7-T20. Acceptance: AC-16 is `[x]` and the final sweep shows no `artifacts/` evidence path and complete schema fields across the full artifact set including the Phase 6 and Phase 7 artifacts.
- [ ] [P7-T22] Write the AC status summary to `<FEATURE>/evidence/other/ac-status-summary.<TS>.md` in the format required by `.claude/skills/acceptance-criteria-tracking/SKILL.md` (Source, Total AC items, Checked off, Remaining, Items remaining). Acceptance: the artifact reports Source = `<FEATURE>/spec.md`, Total = 20, and lists any unchecked item explicitly.
- [ ] [P7-T23] Commit all changes on branch `bug/cobertura-coverage-arithmetic-441` with the message `fix(coverage): dedupe Cobertura line and branch arithmetic (#441, #478)`. Acceptance: `git status --porcelain` returns empty output after the commit, and `git show --stat HEAD` lists the two source files plus the feature documents and evidence artifacts. `.claude/agent-memory/**` is tracked in this repository and is legitimately written by the executing agent during the run; commit any such changes as a **separate commit before** the feature commit, so that `git show --stat HEAD` lists only the two source files, the feature documents and the evidence artifacts. No PR is created and no CI run is triggered from this plan. **Expected residual dirt:** writing the `[x]` for this task into the plan file necessarily modifies the plan after the commit it verifies, so the worktree ends with exactly one modified file — this plan. That is the only permitted residual; `epic-orchestrator` amends the feature commit or follows it with a plan-checklist commit. Any other residual modification is a defect.
