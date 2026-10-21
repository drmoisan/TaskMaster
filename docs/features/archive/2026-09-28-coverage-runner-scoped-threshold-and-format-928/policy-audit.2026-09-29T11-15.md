# Policy Audit: coverage-runner-scoped-threshold-and-format (Issue #928) - Remediation Cycle 1 Exit

- Timestamp label: 2026-09-29T11-15 (assigned without a clock read; no clock command was available under the caller's no-Bash constraint. The label is later than every executor evidence label, the last of which is 2026-09-29T10-59, with a margin as the caller directed.)
- Cycle: remediation cycle 1 exit (re-audit). Prior audit set: `*.2026-09-29T10-00.md` in this folder (1 blocking finding, AC6 PARTIAL). Remediation inputs consumed: `remediation-inputs.2026-09-29T10-00.md` (R-1 blocking, CR-2 folded in). Plan executed: `remediation-plan.2026-09-29T10-00.md` (Grep `^- \[ \]` over the file: 0 matches; every task checked).
- Branch: bug/coverage-runner-scoped-threshold-and-format-928
- Base anchor: 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 (origin/main merge base; evidence/remediation-baseline/r1-p0-t2-identity-and-state.2026-09-29T10-42.md)
- Branch head: 408ac211f4f24376f525af2a477a4d2da50525ec (re-derived by reading `.git/worktrees/agent-aa9f4097fba69c282/HEAD`, which points at `refs/heads/bug/coverage-runner-scoped-threshold-and-format-928`, and that ref file; matches the caller-supplied 408ac211f)
- Work mode: minor-audit (issue.md line 12). AC source: issue.md `## Acceptance Criteria`, AC1 to AC7.
- Reviewer: feature-review (reduced audit, C3)
- Review method: Read, Grep and Glob only. The Bash tool was withheld by the caller, so no git command, no PoshQC MCP tool and no Pester run was executed in this session. Toolchain figures are read from the executor's committed evidence and cross-checked against the gitignored raw documents left in the worktree: the two direct Pester 5.6.1 JaCoCo documents (`coverage/r1-p0-t6-pester-coverage.jacoco.xml`, `coverage/p2-t3-pester-coverage.iter2.jacoco.xml`) were read at the `<sourcefile>` level for the two changed production files, and the bundled JUnit root (`artifacts/pester/pester-junit.xml`) was re-read.

## Template Resolution Deviation

The MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` were not exposed to this session. This artifact is hand-authored and preserves the twelve canonical major headings from `.claude/skills/policy-audit-template-usage/SKILL.md` section 5, in the same shape as the cycle-1 artifact `policy-audit.2026-09-29T10-00.md`. It is not marked BLOCKED, because every required section is evidence-backed.

## Rejected Scope Narrowing

None detected. The caller's prompt names the full branch diff against the merge base (head 408ac211f, base 177b6d78e), supplies the same three-file set the executor's anchored `git diff --name-only` recorded (evidence/qa-gates/p2-t6-scope-lock.2026-09-29T10-57.md), and restricts only the tooling (no Bash), not the audit scope. The minor-audit AC source restriction is the persisted work-mode rule, not a narrowing. Evidence-location instruction: none supplied; nothing to override.

## Evidence Location Compliance

- Every evidence artifact on this branch lives under `docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/<kind>/` with kinds `baseline`, `remediation-baseline`, `regression-testing`, `qa-gates` and `other` (Glob over the feature folder: 48 evidence markdown files, all under those five kinds; `remediation-baseline` is a canonical kind per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` line 20).
- No path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` appears in the branch diff (P2-T6 name-only diff over `scripts tests .github .vscode config` lists exactly the three code files; the `artifacts/` directory is gitignored and Glob over the worktree shows no tracked file under it).
- `validate_evidence_locations.py --root .` could not be executed (no Bash). Verified by inspection instead.
- Verdict: PASS.

## Executive Summary

Remediation cycle 1 relocated the scoped-run gate out of the entry point into a new advanced function `Assert-CoberturaCoverageThresholdForRun` in the path-loaded part file `scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`, behind one unconditional call at entry-point line 406. The function's scoped arm writes exactly one warning and returns; its unscoped arm calls `Assert-CoberturaLineCoverageThreshold` then `Assert-CoberturaBranchCoverageThreshold`, unchanged in text and order (part file lines 102 to 103). CR-2 was folded in: `[ValidateNotNullOrEmpty()]` on both predicate parameters and two `IsPathRooted` fail-fast guards, with four negative tests. Eight `It` blocks were added (22 total); the fourteen existing blocks are untouched.

R-1 is resolved by the agreed measurement route. The identical CMD-PESTER-DIRECT command (Pester 5.6.1, default breakpoint instrumentation, mirroring `.github/workflows/_pester.yml`) now reports the CI population at 94.53% (1626 covered, 94 missed) against the recorded 94.49% baseline (1613/94), the entry point at 113 of 126 (89.68%, equal to its base-anchor figure), and the part file at 13 of 13. This review read the JaCoCo `<sourcefile>` elements directly: the entry point's 13 uncovered lines are exactly the 13 baseline uncovered lines (shifted), the three changed analyzable entry-point lines 313, 406 and 410 are credited, and all 13 part-file lines are credited. The pre-remediation document reproduces the R-1 defect (line 408 at 0 hits), so the before/after pair is consistent. Format, analyzer and test stages pass on the final tree; the RED-first partition for the remediation is recorded (6 failing / 16 passing before, 22/22 after).

- Blocking findings: 0
- Non-blocking findings: 1 (NB-1, canonical hook coverage artifact validity; pre-existing tooling condition)
- Informational findings: 9
- Overall verdict: PASS. Blocking count (FAIL plus blocking PARTIAL): 0.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The direct-function Describe registers its single mock in a `BeforeEach` (lines 158 to 161); the entry-point Describe registers its full mock set in a `BeforeEach` (lines 208 to 236); no `It` depends on another's state; the solo run (r1-p1-t6, 22/22) and the full-population run (p2-t3 iter2, 342/342) both pass. |
| Isolation | PASS | Twelve predicate cases target `Test-CoverageRunIsScoped` alone (eight equivalence cases, four CR-2 negative cases); four cases call `Assert-CoberturaCoverageThresholdForRun` directly with in-memory fixtures and the real threshold assertions; five entry-point cases mock every filesystem and executable seam; one help case reads the parsed AST. |
| Fast execution | PASS | The new suite ran in 0.596 s (gitignored `artifacts/pester/pester-junit.xml`, testsuite element for the file, re-read by this review); whole bundled run 20.074 s. |
| Determinism | PASS | Fixtures are in-memory strings; Grep over the test file for `Start-Sleep`, `TestDrive`, `New-TemporaryFile`, `GetTempPath`, `Invoke-Expression`: 0 matches; the only `$global:` writes are the two `LASTEXITCODE` assignments inside the collector-seam mocks, the established sibling-suite pattern for simulating an executable's exit code. |
| Readability | PASS | Descriptive `It` names; one-line intent comments naming the AC or the CR each case serves (lines 133, 139, 145, 151, 164, 174, 187, 197, 240, 250, 260, 277, 284, 296). |

### 1.2 Coverage Requirements

### Coverage Evidence Checklist

- C# baseline coverage artifact: `N/A - out of scope`
- C# post-change coverage artifact: `N/A - out of scope`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: Route C direct Pester 5.6.1 JaCoCo run over the CI population, figures transcribed in `evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md` (the fixed comparison baseline, decision R7 of the remediation plan); the pre-remediation reproduction is `evidence/remediation-baseline/r1-p0-t6-test-baseline.2026-09-29T10-47.md`, whose gitignored JaCoCo document `coverage/r1-p0-t6-pester-coverage.jacoco.xml` was read by this review
- PowerShell post-change coverage artifact: Route C direct Pester 5.6.1 JaCoCo run, figures transcribed in `evidence/qa-gates/p2-t3-test-coverage.iter2.2026-09-29T10-55.md`; its gitignored JaCoCo document `coverage/p2-t3-pester-coverage.iter2.jacoco.xml` was read by this review (report-level LINE counter missed 94, covered 1626)
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 3 (2 production, 1 test) | 342 (320 base-anchor + 22 new) | 342 passed, 0 failed, 0 skipped | 94.49% lines (1613/1707) | 94.53% lines (1626/1720) | 100.00% lines (part file 13 of 13; changed entry-point lines 3 of 3) |
| C# | 0 | N/A | N/A | N/A | N/A | N/A |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 94.49% lines (1613/1707). Post-change: 94.53% lines (1626/1720). Change: +0.04 points (+13 covered, missed unchanged at 94; the 13 added analyzable lines are the part file's 13, all credited). New/changed-code coverage: 100.00% (13 of 13 new part-file lines; 3 of 3 changed entry-point lines 313, 406, 410). Disposition: PASS. Evidence: evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md and evidence/qa-gates/p2-t3-test-coverage.iter2.2026-09-29T10-55.md, both Route C, identical command; JaCoCo documents read directly by this review.
- C#: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero C# files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

PowerShell coverage verdict (CI-equivalent Route C measurement): PASS (population 94.53%, at or above the 85% rules floor, the 80% CI gate and the 94.49% recorded baseline; Scope part file 100.00%; entry point 89.68% with every changed production line credited).

PowerShell canonical hook artifact `artifacts/pester/powershell-coverage.xml` coverage row: FAIL on artifact validity, disposition non-blocking (NB-1). The bundled PoshQC route regenerated the document at the iteration-2 P2-T3 run (report header dated 09/29/2026 10:54:32), but it instruments only the `.claude` and `.codex` trees: report-level LINE counter covered 0, missed 9294, and a Grep for `Invoke-MSTestWithCoverage` in it returns 0 matches. It is not a measurement of `scripts/` and carries absolute host paths in its package names; it is gitignored and must not be committed. This is the same pre-existing tooling condition recorded at cycle 1 (section 1.2.2 there) and is a promotion candidate (P-4 in section 8), not a property of this change.

- The Route C figures are the CI-equivalent measurement: the command in the plan mirrors `.github/workflows/_pester.yml` lines 36 to 47 (Pester 5.6.1, both test folders, coverage over `scripts/dependencies` and `scripts/vscode`, default breakpoint instrumentation; the token `UseBreakpoints` occurs 0 times in the workflow and in the plan's commands), and the CI gate reads the same report-level LINE counter (workflow line 71: exit 1 below 80).
- Measurement route unchanged from cycle 1 and from the baseline: the remediation plan's decision R7 fixes the comparison baseline at the original P0-T7 figure and permits exactly the two substitutions the original command recorded (`$o` and the pinned `Set-Location` argument); the P2-T3 iteration-2 artifact records both and nothing else.
- Branch coverage for PowerShell: no figure exists, because Pester measures command and line coverage only (.claude/rules/powershell.md); no branch threshold applies and no FAIL is recorded on that account.
- Independent re-execution of the coverage run was not performed in this review (Bash withheld). Cross-checks performed instead: (a) the report-level counters in both JaCoCo documents (1620/95 pre-remediation, 1626/94 post-change) equal the executor's `POPULATION_LINE` rows; (b) the entry-point `<sourcefile>` in the post-change document lists 126 `<line>` nodes with `ci="0"` on exactly 149, 171, 187, 229, 333, 341, 345, 358, 364, 370, 436, 452, 460 (13), matching the executor's `uncovered_lines`; (c) the pre-remediation document lists 129 nodes with `ci="0"` on 14 lines including 408 (`mi="2" ci="0"`), reproducing R-1; (d) the part-file `<sourcefile>` lists 13 nodes (1, 41, 42, 45, 46, 49, 50, 51, 53, 96, 97, 102, 103), all with `ci` at least 1; (e) the population arithmetic (1626 / 1720 = 94.534%; 1613 / 1707 = 94.493%); (f) the analyzable-line accounting for the entry point (base 126; minus the two assertion statements moved into the part file; plus the dot-source at 313 and the call at 406; equals 126); (g) the JUnit root (`tests="342" errors="0" failures="0"`; the new suite `tests="22" errors="0" failures="0" skipped="0"`).

### 1.3 Coverage Exclusion Policy

PASS. No `exclude` entry, no coverage configuration file and no suppression attribute is touched. Both production files remain in the CI coverage denominator (`_pester.yml` line 45 covers `scripts/vscode`). Grep over the two production files for `ExcludeFromCodeCoverage`, `SuppressMessage`: 0.

### 1.4 Scenario Completeness

| Scenario class | Verdict | Cases |
|---|---|---|
| Positive | PASS | It 1 to 5 (unscoped equivalences), It 9 and 10 (scoped entry-point run completes, one warning), It 19 and 20 (direct scoped call: no throw, one warning) |
| Negative | PASS | It 6 to 8 (scoped: subdirectory, name-prefix sibling, parent), It 11 (collector exit 7 still throws), It 15 to 18 (empty and relative inputs rejected) |
| Edge / boundary | PASS | trailing separator (It 4), letter case (It 5), name-prefix sibling (It 7), drive root (It 8), empty-string controls (It 15, 16), branch floor with line at floor (It 13, 22) |
| Error handling | PASS | It 12, 13, 21, 22 (line and branch threshold messages still thrown on unscoped runs), It 11, It 17 and 18 (absolute-path guard messages) |
| Concurrency | PASS | Not relevant: pure predicate, a stateless wrapper function and a single-threaded script |
| State transitions | PASS | Not relevant: no stateful component |

### 1.5 Structure, Documentation, Location, External Dependencies

- Arrange-Act-Assert: PASS (each case arranges via `BeforeEach` or a local fixture expression, acts with one invocation, asserts with `Should`).
- Clear failure messages: PASS (`Should -Throw -ExpectedMessage` with the exact production message, `Should -Invoke ... -Times 1 -Exactly`, `Should -Be`).
- Documentation: PASS (intent comments per case; file-level comments explain the parse-and-dot-source import and the guarded part-file import).
- Test file location: PASS (`tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1` mirrors `scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`).
- External dependencies: PASS (executables reached only through mocked wrapper seams `Invoke-VsWhereExe` and `Invoke-DotnetCoverageExe`; `Get-Command` mocked).
- Temporary files: PASS (Grep over the test file for `TestDrive`, `New-TemporaryFile`, `GetTempPath`: 0 matches; every `Set-Content` and `Remove-Item` the entry point reaches is mocked; the direct-function cases perform no I/O).
- Banned determinism APIs: PASS (no `Start-Sleep`, no wall-clock read).

## 2. General Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Before-changes planning | PASS | remediation-plan.2026-09-29T10-00.md records objective, decisions R1 to R14, Write Set, the Out-of-Scope list and a diagnostic gate (P0-T7) that had to pass before any edit; it did (`PART-FILE-CREDIT-PREMISE: confirmed`). |
| Bugfix Workflow: failing regression test first | PASS | evidence/regression-testing/r1-p1-t3-expect-fail.2026-09-29T10-50.md: 22 tests, 6 failures, 0 errors; It 17 and 18 fail with "no exception was thrown", It 19 to 22 fail with `CommandNotFoundException` on the not-yet-existing function; the 16 others pass as controls. |
| Bugfix Workflow: minimal targeted fix | PASS | Diff footprint against the base anchor is the entry point (+24/-2), one new 104-line part file and one new 306-line test file; the threshold and helpers part files, the sibling suites and the workflows are untouched (P2-T5, P2-T6). |
| Bugfix Workflow: verify before review | PASS | r1-p1-t6 22/22; P2-T1 iter2 format no-op (hashes identical); P2-T2 iter2 analyzer 0 findings on the three changed files; P2-T3 iter2 342/342 with the coverage conditions met; P2-T4 `LOOP-CLOSED: yes`. |
| Simplicity first | PASS | One predicate, one wrapper function, one unconditional call; no new switch, no new abstraction, no opt-out surface. |
| Reusability | PASS | Both functions are standalone with documented contracts; the wrapper is directly unit-testable (It 19 to 22). |
| Extensibility / public API | PASS | No command-line parameter added or changed; no caller can opt out of the unscoped gate (issue.md AC preamble). |
| Separation of concerns | PASS | The part file performs no filesystem I/O (Grep for `Get-ChildItem`, `Test-Path`, `Resolve-Path`, `Get-Content`, `Set-Content`: 0); the entry point keeps the I/O and is thinner than before (the eleven-line conditional became a two-line comment and a four-line call). |
| Error handling / logging | PASS | Scoped run: explicit `Write-Warning` naming both roots. Unscoped run: the two `throw`-ing assertions unchanged. Relative or empty inputs: fail fast with a specific message (`RepoRoot must be an absolute path: ...`). Collector failure still throws before any threshold logic (It 11). |
| Module and file structure (500-line limit) | PASS | Entry point 461 lines, part file 104, test file 306 (Read line counts; P2-T5 arithmetic agrees). Invoke-MSTest.ps1 unchanged at 262. |
| Naming, docs, comments | PASS | Approved verbs `Test-` and `Assert-`; comment-based help on both part-file functions and on `Invoke-MSTestWithCoverageMain`; the entry-point comment at 404 to 405 and the part-file header state why the gate lives in the part file; the change-budget rationale was removed from both comments (cycle-1 CR-3 closed). See I-2 for a wording note. |
| Dependencies | PASS | None added. |
| Match existing style | PASS | Backtick-continued named-argument call mirrors the existing `Invoke-DotnetCoverageCollection` call at 385 to 392; part-file pattern, wrapper-seam mocks and guarded imports copied from sibling files. |
| Toolchain loop (format, lint, type-check, test) | PASS | P2-T1 iter2 (format), P2-T2 iter2 (analyze), P2-T3 iter2 (test with coverage), P2-T4 closure at iteration 2. Type checking does not apply to this language per the rule file. Architecture-boundary, contract and integration stages: no repository-defined stage exists for PowerShell scripts. |
| After-changes documentation | PASS | Script help retained and accurate; plan and remediation-plan check-offs recorded; P2-T13 handoff refreshed with the AC status block; original plan's P2-T3 and P2-T4 checked off (P2-T4 artifact). |

## 3. Language-Specific Code Change Policy Compliance

PowerShell (.claude/rules/powershell.md):

| Item | Verdict | Evidence |
|---|---|---|
| Formatting via PoshQC MCP formatter | PASS | r1-p0-t4 liveness control (an injected indent on line 272 was removed by the formatter, then the file was restored); P2-T1 iter2 shows identical raw-byte hashes before and after a format run for all four Write Set PowerShell files (36f9595e..., cb9b9a74..., 9aec072f..., 2bf36b22...). Invoke-MSTest.ps1 hash 9aec072f... equals its base-anchor value. |
| PSScriptAnalyzer via PoshQC | PASS | P2-T2 iter2 runs (C) entry point, (D) part file, (E) test file, (B) test folder: `ok: true` each. Folder run (A) reports 13, equal to the r1-p0-t5 baseline of 13; Invoke-MSTest.ps1 reports 2, equal to its baseline. No suppression added (Grep for `SuppressMessage` in the three changed files: 0). |
| Type checking | PASS | The rule file defines no type-checking stage for this language; the stage is skipped by rule. |
| PowerShell 7+ compatibility | PASS | `Set-StrictMode -Version Latest`, `[IO.Path]::IsPathRooted`, `[IO.Path]::GetFullPath`, `[StringComparison]::OrdinalIgnoreCase`; no Windows-PowerShell-only construct. |
| Advanced functions, CmdletBinding, Mandatory parameters, validation attributes | PASS | Both part-file functions carry `[CmdletBinding()]`; every parameter is `[Parameter(Mandatory = $true)] [string]`; the predicate's two parameters also carry `[ValidateNotNullOrEmpty()]` (lines 33, 37). |
| ShouldProcess for state-changing actions | PASS | Neither function changes state; the wrapper writes to the warning stream or throws. |
| No global state, no Invoke-Expression, no hard-coded paths | PASS | Grep over both production files for `Invoke-Expression`, `$global:`, `$script:`: 0 in the part file; entry-point `$global:LASTEXITCODE` handling is pre-existing and unchanged. |
| Approved verbs | PASS | `Test-` and `Assert-` are approved; analyzer clean. |
| Under 500 lines | PASS | See section 2. |
| Change budget (at most 3 production files per batch) | PASS | 2 production files changed (entry point, part file); the plan reserved a third (Invoke-MSTest.ps1) that the formatter did not touch. |
| Design seams | PASS | No new executable call; existing wrapper seams reused. |
| Prohibited behaviors (broad refactor, analyzer debt, weakened assertions, sleeps, claiming success without toolchain) | PASS | None observed. The fourteen pre-existing `It` blocks are byte-identical (r1-p1-t2: insertion-only diff, 0 deleted lines); every assertion in the new cases is at least as strict as the entry-point cases it mirrors. |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Pester 5.x, Describe/Context/It | PASS | Four Describe blocks, two Context blocks, 22 It blocks (Read count agrees with r1-p1-t2); Pester 5.6.1 pinned in CI. |
| Mirrors code structure, `*.Tests.ps1` | PASS | tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1. |
| One behavior per It | PASS with note | It 9 carries a second assertion on the `Set-Content` count (cycle-1 I-4, carried as I-3 here); It 20 carries two `Should -Invoke` assertions on the same warning (count, then content), which is one behavior. |
| Mock sparingly, prefer real code paths | PASS | The direct-function cases mock only `Write-Warning`; the threshold assertions and the predicate run for real. The entry-point cases mock the same seam set as the sibling `AssemblyDiscovery` suite requires. |
| Never mock executables directly | PASS | `Invoke-VsWhereExe` and `Invoke-DotnetCoverageExe` (wrapper seams) are mocked; `vswhere` and `dotnet-coverage` are not. |
| Mock signature parity | PASS | `param([string]$VsWherePath, [string[]]$VsWhereArgs)` and `param([string[]]$DotnetCoverageArgs)` match production. |
| Mock registration order / AST import order | PASS | Entry point parsed and dot-sourced, then helpers, then the guarded part file, all in `BeforeAll`; mocks registered in `BeforeEach` before invocation. The sibling suites' `Assert-Cobertura*Threshold` mocks (RunSettings line 415, ResultsDirectory line 198, AssemblyDiscovery lines 138, 143, 157, 162) still intercept the calls now made one scope deeper: all 342 tests pass (plan decision R14). |
| Deterministic test requirements | PASS | No PATH, cwd, profile or network dependence; `$PSScriptRoot`-relative resolution only; the CR-2 guard removes the predicate's only implicit working-directory dependence. |
| Changed-line coverage regression is blocking | PASS | No changed production line is uncovered under the agreed route; entry-point file figure returned to its base-anchor value (113 of 126); see section 5. Cycle-1 finding G-1 is closed. |

## 5. Test Coverage Detail

Route: C (direct Pester 5.6.1 run mirroring the CI workflow; the bundled MCP document does not name the changed files, see 1.2.2). Figures below were read by this review from the JaCoCo `<sourcefile>` elements of the gitignored documents named in the checklist.

| File | Status | Baseline lines (covered/analyzable) | Pre-remediation lines (r1-p0-t6) | Post-change lines (p2-t3 iter2) | Baseline % | Post-change % | Delta (points) | Uncovered changed lines |
|---|---|---|---|---|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | modified | 113/126 | 115/129 | 113/126 | 89.68% | 89.68% | 0.00 | none (313 ci=2, 406 ci=1, 410 ci=2) |
| scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 | new | (absent) | 5/5 | 13/13 | (absent) | 100.00% | new file | none |
| scripts/vscode/Invoke-MSTest.ps1 | unchanged | 49/56 | 49/56 | 49/56 | 87.50% | 87.50% | 0.00 | (no changed lines) |
| CI population (scripts/dependencies + scripts/vscode) | | 1613/1707 | 1620/1715 | 1626/1720 | 94.49% | 94.53% | +0.04 | |

Changed-line derivation for the entry point (from the executor's `git diff -U0` hunk headers in P2-T3 iter2, checked against the file as read): help lines 278 to 291 carry no coverage point; lines 311 to 314 add the reworded comment, the dot-source (313, credited) and a blank; lines 404 to 409 add the two-line comment and the four-line call (406 is the call's only analyzable line, credited; 407 to 409 are backtick continuations and carry no node). Line 410 (the first-party report) is a pre-existing statement that shifted; it is credited. The 13 uncovered entry-point lines are the base anchor's 13 uncovered lines: 149, 171, 187, 229 unchanged; 315, 323, 327, 340, 346, 352 shifted by 18 to 333, 341, 345, 358, 364, 370; 414, 430, 438 shifted by 22 to 436, 452, 460. None of them is a changed line.

Part file: the JaCoCo document lists 13 analyzable lines (1, 41, 42, 45, 46, 49, 50, 51, 53, 96, 97, 102, 103), every one with `ci` at least 1. Lines 42 and 46 (the two guard throws) are credited by It 17 and 18; lines 97, 102 and 103 (the warning and the two assertion statements) are credited by It 19 to 22 and by the entry-point cases It 9, 10, 12, 13. The diagnostic gate r1-p0-t7 established the crediting premise before the edit: in a two-file run ordered AssemblyDiscovery then Scope, the four Threshold.ps1 throw lines reached only by the later-sorting suite were credited (uncovered set 11 in the control run, 7 in the ordered run, with 53, 54, 123, 124 removed), and `CONTAINER_ORDER` confirmed the order Pester used.

Thresholds (uniform tier rule, .claude/rules/quality-tiers.md; CLAUDE.md 80/90 also stated):

- New file line coverage at least 85% (90% per CLAUDE.md): PASS (100.00%).
- Modified file line coverage at least 85%: PASS (89.68%).
- Modified file no regression on changed lines: PASS (all three changed analyzable lines credited; file figure equal to the base anchor).
- Repo-wide (CI population) at least 85%: PASS (94.53%).
- Repo-wide not below recorded baseline (AC6 clause): PASS (94.53% against 94.49%, identical command).

## 6. Test Execution Metrics

| Run | Command (as recorded) | Result |
|---|---|---|
| P0-T7 baseline (Route C, original plan) | CMD-PESTER-DIRECT, Set-Location pinned to the worktree | passed 320, failed 0, skipped 0; LINE covered 1613, missed 94 |
| r1-p0-t6 pre-remediation (bundled) | `mcp__drm-copilot__run_poshqc_test`, four scan folders | JUnit root tests 334, failures 0, errors 0; 27 suites |
| r1-p0-t6 pre-remediation (Route C) | CMD-PESTER-DIRECT | passed 334, failed 0, skipped 0; LINE covered 1620, missed 95; line 408 uncovered (R-1 reproduced) |
| r1-p0-t7 diagnostic control | CMD-DIAG-CONTROL (AssemblyDiscovery suite alone) | passed 5; Threshold.ps1 uncovered 11 including 53, 54, 123, 124 |
| r1-p0-t7 diagnostic ordered | CMD-DIAG-ORDERED (AssemblyDiscovery then Scope) | passed 19; Threshold.ps1 uncovered 7, none of 53, 54, 123, 124; premise confirmed |
| r1-p1-t3 expect-fail (new file only) | `mcp__drm-copilot__run_poshqc_test` | tests 22, failures 6, errors 0 (It 17 to 22 fail; It 1 to 16 pass) |
| r1-p1-t6 pass-after (new file only) | `mcp__drm-copilot__run_poshqc_test` | tests 22, failures 0, errors 0 |
| P2-T3 iter2 final (bundled) | `mcp__drm-copilot__run_poshqc_test`, four scan folders | tests 342, failures 0, errors 0, skipped 0; 27 suites; new suite 22/22 (JUnit root in the worktree re-read by this review: `tests="342" errors="0" failures="0"`) |
| P2-T3 iter2 final (Route C) | CMD-PESTER-DIRECT | passed 342, failed 0, skipped 0; LINE covered 1626, missed 94 |

Reviewer-run tests: none (Bash withheld). Timing: the new suite 0.596 s; whole bundled run 20.074 s (gitignored JUnit root `time` attribute).

## 7. Code Quality Checks

| Check | Result |
|---|---|
| Formatter (PoshQC format, MCP route) | PASS: no rewrite of any Write Set file at iteration 2; formatter liveness proven (r1-p0-t4) |
| Analyzer (PoshQC analyze, MCP route) | PASS: 0 findings on the entry point, the part file and the test file; folder counts equal baseline |
| Type check | No type-checking stage is defined for this language by the rule file; skipped by rule |
| Tests | PASS: 342/342 |
| File size limit | PASS: 461 / 104 / 306 lines |
| Analyzer suppressions added | none |
| Workflow change scan | none (P2-T6 name-only diff over .github lists nothing; `_mstest-coverage.yml` line 95 still invokes `-SearchRoot . -Configuration Debug`; no green-run gate applies) |
| Committed Test Evidence Format | PASS: Glob over the feature folder for `*.xml`, `*.trx`, `*.json`, `*.txt` finds nothing; every figure is a derived projection; raw JaCoCo and JUnit documents remain under gitignored `coverage/` and `artifacts/` |
| Confidentiality masking scan | PASS: Grep over the feature folder and the promoted record for the developer account name, the host name (both read from the gitignored JUnit document and not transcribed here), the drive-letter path pattern `(^\|[^A-Za-z])[A-Za-z]:[\\/]` and the user-profile directory prefix returns 0 matches; the only related tokens are the repository owner's public GitHub handle in the issue URL and the two plan `Owner:` fields (I-1) |
| Tone (agent-authored artifacts) | PASS: measured, factual, no hyperbole or humor in the 21 cycle-1 evidence artifacts read |

## 8. Gaps and Exceptions

- G-1 (cycle 1, Blocking): CLOSED. AC6's baseline clause and changed-line clause are both met by the agreed measurement route (section 5). The plan's final QC loop is recorded closed (P2-T4 `LOOP-CLOSED: yes` at iteration 2); the original plan has no unchecked task.
- NB-1 (Non-blocking, pre-existing tooling): the canonical hook coverage artifact at `artifacts/pester/powershell-coverage.xml`, produced by the bundled PoshQC test route, instruments only the `.claude` and `.codex` trees and therefore reads 0% for the repository scripts; it cannot serve as evidence for any branch that changes `scripts/`. This review records FAIL on that artifact row and PASS on the Route C measurement (section 1.2.2). Not caused by this change; disposition non-blocking. Promotion candidate P-4: make the bundled route's coverage path cover `scripts/dependencies` and `scripts/vscode` (or point the hook at the CI-equivalent document).
- I-1 (Informational): `drmoisan` appears in issue.md line 10 (the GitHub issue URL the template requires) and as the `Owner:` field of the two plan files. It is the public repository owner handle, not the Windows account name, the host name or a path; AC7's clause names the developer account name, and the account-name Grep returns 0.
- I-2 (Informational): the part-file header comment (lines 5 to 6) states as fact that "every test file shares one compiled copy of a path-loaded file". The r1-p0-t7 diagnostic measured the observable consequence (a later-sorting suite's hits on a path-loaded part file are credited), not the interpreter mechanism. Recommend softening to "is credited from any test file (measured at issue #928)" on the next edit of the file; the comment's purpose (why the gate lives here) is served either way.
- I-3 (Informational, carried from cycle 1): It 9 carries two assertions (`Should -Not -Throw` and the `Set-Content` count); acceptable, optional split.
- I-4 (Informational, carried; promotion candidate P-3): script-level comment-based help for the entry point, so `Get-Help .\Invoke-MSTestWithCoverage.ps1` surfaces the scoped-run behavior; AC4 is satisfied by the function help. The `.PARAMETER SearchRoot` text still carries the redundant clause "; they are skipped only on a scoped run".
- I-5 (Informational, pre-existing, outside this diff; promotion candidate P-2): tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 embeds the developer account name in literal fixture paths (Grep for the user-profile prefix in that file: 4 matches at lines 41, 42, 104, 124).
- I-6 (Informational, pre-existing; promotion candidate P-1): the breakpoint under-crediting of entry-point lines reached only by a suite sorting after `Invoke-MSTest.RunSettings.Tests.ps1` remains a property of the four `ParseFile`-importing suites; this change avoided it structurally rather than fixing it.
- I-7 (Informational): CLAUDE.md states 80% line coverage for PowerShell while .claude/rules state 85%; every figure here clears both floors; no literal changed.
- I-8 (Informational): evidence timestamp labels were assigned by the executor and by this review without a clock read (disclosed in each artifact); labels are monotone with the recorded task order and are not used for ordering decisions.
- I-9 (Informational): the `Assert-CoberturaCoverageThresholdForRun` parameters carry `Mandatory` but not `[ValidateNotNullOrEmpty()]`; a mandatory `[string]` already rejects an empty argument at binding, and relative inputs are rejected by the predicate it calls, so behavior is equivalent to the predicate's contract. `[IO.Path]::IsPathRooted` accepts a drive-relative root such as `\repo`; the entry point never produces one (`Resolve-Path` output and a `Join-Path` on it), so no functional exposure exists.
- Not independently verifiable in this session (no git): that the working tree is clean at 408ac211f and that the commit contains no path outside the executor's recorded footprint. The three code files and every feature-folder file were read from the working tree; the P2-T6 porcelain and name-only diff are the executor's evidence.

## 9. Summary of Changes

- `scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1` (new, 104 lines): `Test-CoverageRunIsScoped -RepoRoot -ResolvedSearchRoot` (pure; `[ValidateNotNullOrEmpty()]`; `IsPathRooted` guards; `GetFullPath` normalisation; trailing-separator trim; ordinal case-insensitive equality negated) and `Assert-CoberturaCoverageThresholdForRun -CoberturaXml -RepoRoot -ResolvedSearchRoot` (scoped: one warning naming both roots, return; unscoped: line assertion then branch assertion, unchanged text and order).
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (+24/-2 against the base anchor): `.DESCRIPTION` and `.PARAMETER SearchRoot` help on `Invoke-MSTestWithCoverageMain`; dot-source of the Scope part file after the TrxSummary dot-source with a reworded comment; the two direct `Assert-Cobertura*CoverageThreshold` calls replaced by one unconditional `Assert-CoberturaCoverageThresholdForRun` call preceded by a two-line comment.
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1` (new, 306 lines): 12 predicate cases (8 equivalence, 4 negative), 4 direct wrapper cases, 3 scoped-run entry-point cases, 2 unscoped-run entry-point cases, 1 help case.
- `scripts/vscode/Invoke-MSTest.ps1`: unchanged (formatter rewrote nothing).
- Documentation: issue.md AC6 check-off (7 of 7 checked), original plan P2-T3 and P2-T4 check-offs, remediation plan check-offs, 21 cycle-1 evidence artifacts, refreshed handoff and hygiene scan.

## 10. Compliance Verdict

- General Unit Test Policy: PASS.
- General Code Change Policy: PASS.
- PowerShell Code Change Policy: PASS.
- PowerShell Unit Test Policy: PASS.
- Committed Test Evidence Format and host-identifier hygiene: PASS.
- Evidence location: PASS.
- Blocking findings: 0. Non-blocking: 1 (NB-1). Informational: 9.
- Overall: PASS. No remediation-inputs artifact is produced for this cycle.

## Appendix A: Test Inventory

tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1 (22 It blocks; numbering per the executor's Test Specification):

| # | Describe / Context | It | AC / finding | Fail-before (P1-T3 original; r1-p1-t3) | Pass-after (r1-p1-t6, P2-T3 iter2) |
|---|---|---|---|---|---|
| 1 | Test-CoverageRunIsScoped | returns false when the resolved search root equals the repository root | preamble | fail; pass | pass |
| 2 | Test-CoverageRunIsScoped | returns false for a dot search root beneath the repository root | preamble | fail; pass | pass |
| 3 | Test-CoverageRunIsScoped | returns false for a dot-backslash search root beneath the repository root | preamble | fail; pass | pass |
| 4 | Test-CoverageRunIsScoped | returns false when only a trailing separator differs | preamble | fail; pass | pass |
| 5 | Test-CoverageRunIsScoped | returns false when only letter case differs | preamble | fail; pass | pass |
| 6 | Test-CoverageRunIsScoped | returns true for a subdirectory search root | preamble | fail; pass | pass |
| 7 | Test-CoverageRunIsScoped | returns true for a sibling directory whose name extends the repository root name | preamble | fail; pass | pass |
| 8 | Test-CoverageRunIsScoped | returns true for the parent directory of the repository root | preamble | fail; pass | pass |
| 9 | threshold gating / scoped run | completes without error on a scoped run whose post-processed document is below both floors | AC1 | fail (threshold message); pass | pass |
| 10 | threshold gating / scoped run | writes exactly one warning naming the skipped assertions and the scoped search root | AC1 | fail (threshold message); pass | pass |
| 11 | threshold gating / scoped run | still terminates with an error when collection returns a non-zero exit code on a scoped run | AC2 | pass (control); pass | pass |
| 12 | threshold gating / unscoped run | throws the line threshold message when the search root is omitted and the line rate is below 80 percent | AC3 | pass (control); pass | pass |
| 13 | threshold gating / unscoped run | throws the branch threshold message for a dot search root when the branch rate is below 75 percent | AC3 | pass (control); pass | pass |
| 14 | comment-based help | documents the scoped-run behavior on the SearchRoot parameter | AC4 | fail (`$key.Count` 0); pass | pass |
| 15 | Test-CoverageRunIsScoped | throws when the repository root is an empty string | CR-2 control | (added at cycle 1) pass | pass |
| 16 | Test-CoverageRunIsScoped | throws when the resolved search root is an empty string | CR-2 control | (added at cycle 1) pass | pass |
| 17 | Test-CoverageRunIsScoped | throws when the repository root is a relative path | CR-2 | (added at cycle 1) fail (no exception) | pass |
| 18 | Test-CoverageRunIsScoped | throws when the resolved search root is a relative path | CR-2 | (added at cycle 1) fail (no exception) | pass |
| 19 | Assert-CoberturaCoverageThresholdForRun | does not throw on a scoped run whose document is below both floors | AC1 / R-1 | (added at cycle 1) fail (command not found) | pass |
| 20 | Assert-CoberturaCoverageThresholdForRun | writes exactly one warning naming the scoped search root | AC1 / R-1 | (added at cycle 1) fail (command not found) | pass |
| 21 | Assert-CoberturaCoverageThresholdForRun | throws the line threshold message on an unscoped run below the line floor | AC3 / R-1 | (added at cycle 1) fail (command not found) | pass |
| 22 | Assert-CoberturaCoverageThresholdForRun | throws the branch threshold message on an unscoped run at the line floor and below the branch floor | AC3 / R-1 | (added at cycle 1) fail (command not found) | pass |

Existing suites: 26 files, 320 tests, all passing before and after (P0-T7, P2-T3 iter2); none edited (P2-T6 name-only diff lists no other test file).

## Appendix B: Toolchain Commands Reference

Recorded by the executor (this review executed none of them):

- Format: `mcp__drm-copilot__run_poshqc_format` with `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`, observed through raw-byte hashes (`git hash-object --no-filters`) and porcelain before and after (r1-p0-t4, p2-t1 iter2).
- Analyze: `mcp__drm-copilot__run_poshqc_analyze` with the six scan sets (A) to (F) in evidence/qa-gates/p2-t2-analyze.iter2.2026-09-29T10-53.md.
- Test (bundled): `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]`; JUnit read from artifacts/pester/pester-junit.xml.
- Test with coverage (Route C): CMD-PESTER-DIRECT as quoted in remediation-plan.2026-09-29T10-00.md (Pester 5.6.1, `Run.Path` over both test folders, `CodeCoverage.Path` over `scripts/dependencies` and `scripts/vscode`, JaCoCo output under the gitignored `coverage/` directory), mirroring `.github/workflows/_pester.yml` lines 36 to 47; exactly two substitutions (`$o` and the pinned `Set-Location` argument).
- Diagnostic: CMD-DIAG-CONTROL and CMD-DIAG-ORDERED as quoted in the remediation plan (single `<worktree-root>` substitution), r1-p0-t7.
- Diff and footprint: `git diff --numstat 177b6d78e -- scripts/vscode tests/scripts/vscode`; `git diff -U0 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1`; `git diff -U0 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`; `git diff --name-only 177b6d78e -- scripts tests .github .vscode config`; `git status --porcelain -uall`.
