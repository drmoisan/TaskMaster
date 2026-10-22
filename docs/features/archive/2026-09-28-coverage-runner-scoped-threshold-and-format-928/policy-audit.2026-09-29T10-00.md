# Policy Audit: coverage-runner-scoped-threshold-and-format (Issue #928)

- Timestamp label: 2026-09-29T10-00 (assigned without a clock read; no clock command was available under the caller's no-Bash constraint. The label is later than every executor evidence label, the last of which is 2026-09-29T09-29.)
- Branch: bug/coverage-runner-scoped-threshold-and-format-928
- Base anchor: 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 (origin/main merge base, confirmed by evidence/baseline/p0-t3-base-anchor.2026-09-29T08-54.md)
- Branch head: a17ca5bcf (as supplied by the caller; not re-derived, see Deviations)
- Work mode: minor-audit (issue.md line 12). AC source: issue.md `## Acceptance Criteria`, AC1 to AC7.
- Reviewer: feature-review (reduced audit, C3)
- Review method: Read, Grep and Glob only. The Bash tool was withheld by the caller, so no git command, no PoshQC MCP tool and no Pester run was executed in this session. Every toolchain figure below is read from the executor's committed evidence and cross-checked arithmetically and against the gitignored JUnit document in the worktree.

## Template Resolution Deviation

The MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` were not exposed to this session. This artifact is hand-authored and preserves the twelve canonical major headings from `.claude/skills/policy-audit-template-usage/SKILL.md` section 5. It is not marked BLOCKED, because every required section is evidence-backed.

## Rejected Scope Narrowing

None detected. The caller's prompt names the full branch diff against the merge base, supplies the same file set the executor's anchored `git diff --name-only` recorded (evidence/qa-gates/p2-t6-scope-lock.2026-09-29T09-27.md), and restricts only the tooling (no Bash), not the audit scope. The minor-audit AC source restriction is the persisted work-mode rule, not a narrowing. Evidence-location instruction: none supplied; nothing to override.

## Evidence Location Compliance

- Every evidence artifact on this branch lives under `docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/<kind>/` with kinds `baseline`, `regression-testing`, `qa-gates` and `other` (Glob over the feature folder: 27 markdown files, all under those four kinds).
- No path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` appears in the branch diff (P2-T6 name-only diff over `scripts tests .github .vscode config` lists exactly the three code files; the `artifacts/` directory is gitignored at .gitignore line 57 and Glob over the worktree shows no tracked file under it).
- `validate_evidence_locations.py --root .` could not be executed (no Bash). Verified by inspection instead.
- Verdict: PASS.

## Executive Summary

The change adds a pure predicate `Test-CoverageRunIsScoped` in a new part file, gates the two document-level Cobertura threshold assertions in the entry point on it (scoped run: one warning, assertions skipped; unscoped run: the two assertion statements run unchanged, in order), adds comment-based help, and adds a 14-case Pester file. Format, analyzer and test stages pass. The Bugfix Workflow's RED-first requirement is met with a recorded 11-failing/3-passing partition before the fix and 14/14 after it.

One finding is Blocking: AC6 is not met by the plan's agreed measurement route. The changed entry-point line 408 (the scoped-arm `Write-Warning`) reads 0 hits in the CI-equivalent Pester 5.6.1 breakpoint run, the population figure is 94.46% against a 94.49% baseline, and the modified file reads 89.15% against 89.68%. The line is executed by a passing test that asserts the warning text; the shortfall is a crediting defect of breakpoint-based coverage across per-file parsed copies of the entry point. Because the AC text and the changed-line no-regression rule (.claude/rules/powershell.md) are both explicit, the finding is classified Blocking pending either a code-shaped remediation (recommended: relocate the gate into the path-loaded part file) or a maintainer-ratified measurement exception transcribed into issue.md.

- Blocking findings: 1 (AC6, PARTIAL, blocking)
- Non-blocking findings: 1
- Informational findings: 8
- Overall verdict: REMEDIATION REQUIRED

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The new file registers its full mock set in a `BeforeEach` (lines 134 to 162); no `It` depends on another's state; the executor's solo run (P1-T6) and full-population run (P2-T3) both pass 14/14. |
| Isolation | PASS | Eight predicate cases target `Test-CoverageRunIsScoped` alone; five entry-point cases mock every filesystem and executable seam and leave the real threshold assertions and the real collection wrapper in the path; one help case reads the parsed AST. |
| Fast execution | PASS | The new suite ran in 0.521 s (gitignored artifacts/pester/pester-junit.xml, testsuite element for the new file). |
| Determinism | PASS | Fixtures are in-memory strings; no clock, RNG, sleep or wall-clock wait (Grep for `Start-Sleep` returns 0); path semantics are Windows-only like every sibling suite. |
| Readability | PASS | Descriptive `It` names, one-line intent comments naming the AC each case serves (lines 166, 176, 186, 203, 210, 222). |

### 1.2 Coverage Requirements

### Coverage Evidence Checklist

- C# baseline coverage artifact: `N/A - out of scope`
- C# post-change coverage artifact: `N/A - out of scope`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: Route C direct Pester 5.6.1 JaCoCo run over the CI population, figures transcribed in `evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md` (the JaCoCo document itself was written under the gitignored `coverage/` directory and was not committed, per the Committed Test Evidence Format)
- PowerShell post-change coverage artifact: Route C direct Pester 5.6.1 JaCoCo run, figures transcribed in `evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 3 (2 production, 1 test) | 334 (320 baseline + 14 new) | 334 passed, 0 failed, 0 skipped | 94.49% lines (1613/1707) | 94.46% lines (1620/1715) | 87.50% lines (7 of 8 new analyzable lines) |
| C# | 0 | N/A | N/A | N/A | N/A | N/A |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 94.49% lines (1613/1707). Post-change: 94.46% lines (1620/1715). Change: -0.03% lines (+7 covered, +1 missed; the one missed line is entry-point line 408). New/changed-code coverage: 87.50%. Disposition: FAIL. Evidence: evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md and evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md, both Route C, same command.
- C#: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero C# files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

PowerShell coverage verdict: FAIL (blocking; AC6 changed-line clause and baseline clause both unmet by the agreed measurement route; see section 5 for the per-file figures and section 8 for the disposition).

- The canonical hook artifact `artifacts/pester/powershell-coverage.xml` exists in the worktree (gitignored) but instruments only the `.claude` and `.codex` trees: report-level LINE counter missed 9294, covered 0; Grep count of `Invoke-MSTestWithCoverage` in it is 0. It cannot serve as coverage evidence for this branch, it carries absolute host paths in its package names, and it must not be committed. The same document in the session root reads missed 2315, covered 0.
- The Route C figures are the CI-equivalent measurement: the command in the plan mirrors `.github/workflows/_pester.yml` lines 36 to 47 (Pester 5.6.1, both test folders, coverage over `scripts/dependencies` and `scripts/vscode`, default breakpoint instrumentation), and the CI gate reads the same report-level LINE counter (workflow line 59).
- Repo-wide PowerShell line coverage 94.46% is above both the 85% rules floor and the 80% CI gate.
- Branch coverage for PowerShell: no figure exists, because Pester measures command and line coverage only (.claude/rules/powershell.md); no branch threshold applies and no FAIL is recorded on that account.
- Independent re-execution of the coverage run was not performed in this review (Bash withheld). Cross-checks performed: the population arithmetic (1613/1707 = 94.493%, 1620/1715 = 94.461%), the per-file arithmetic (113/126 = 89.68%, 115/129 = 89.15%, 5/5 = 100.00%), the delta accounting (+7 covered +1 missed equals the 8 new analyzable lines: entry point 313, 407, 408 and the five part-file lines), and the gitignored JUnit root attributes in the worktree (`tests="334" errors="0" failures="0"`; the new suite `tests="14" failures="0" errors="0" skipped="0"`).

### 1.3 Coverage Exclusion Policy

PASS. No `exclude` entry, no coverage configuration file and no suppression attribute is touched. Both production files remain in the CI coverage denominator (`_pester.yml` line 45 covers `scripts/vscode`).

### 1.4 Scenario Completeness

| Scenario class | Verdict | Cases |
|---|---|---|
| Positive | PASS | It 1 to 5 (unscoped equivalences), It 9 and 10 (scoped run completes, one warning) |
| Negative | PASS | It 6 to 8 (scoped: subdirectory, name-prefix sibling, parent), It 11 (collector exit 7 still throws) |
| Edge / boundary | PASS | trailing separator (It 4), letter case (It 5), name-prefix sibling that a prefix comparison would misclassify (It 7), drive root (It 8) |
| Error handling | PASS | It 12 and 13 (line and branch threshold messages still thrown on unscoped runs), It 11 |
| Concurrency | PASS | Not relevant: pure predicate and a single-threaded script |
| State transitions | PASS | Not relevant: no stateful component |

### 1.5 Structure, Documentation, Location, External Dependencies

- Arrange-Act-Assert: PASS (each entry-point case arranges via `BeforeEach`, acts with one invocation, asserts with `Should`).
- Clear failure messages: PASS (`Should -Throw -ExpectedMessage`, `Should -Invoke ... -Exactly`, `Should -Be`).
- Documentation: PASS (intent comments per case; file-level comments explain the parse-and-dot-source import and the guarded part-file import).
- Test file location: PASS (`tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1` mirrors `scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`).
- External dependencies: PASS (executables reached only through mocked wrapper seams `Invoke-VsWhereExe` and `Invoke-DotnetCoverageExe`; `Get-Command` mocked).
- Temporary files: PASS (Grep over the new test file for `TestDrive`, `TemporaryFile`, `GetTempPath`, `New-Item`, `Out-File`: 0 matches; every `Set-Content` and `Remove-Item` the entry point reaches is mocked).
- Banned determinism APIs: PASS (no `Start-Sleep`, no wall-clock read).

## 2. General Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Before-changes planning | PASS | plan.2026-09-28T19-45.md (revision round 5) records objective, approach, decisions D1 to D16, Write Set and Out of Scope. |
| Bugfix Workflow: failing regression test first | PASS | evidence/regression-testing/p1-t3-expect-fail.2026-09-29T09-14.md: 14 tests, 11 failures, 0 errors; It 9 and It 10 fail with `Cobertura line coverage 40% is below the required 80% threshold.` (the bug reproduced on a scoped run); the three controls pass. |
| Bugfix Workflow: minimal targeted fix | PASS | Diff footprint is the entry point (+29/-2), one new 49-line part file, one new test file; no opportunistic refactor; the threshold and helpers part files are untouched (P2-T5). |
| Bugfix Workflow: verify before review | PASS with one open condition | P1-T6 14/14; P2-T1 format no-op; P2-T2 analyzer 0 findings on changed files; P2-T3 334/334. The plan's own coverage condition in P2-T3 is unmet, so P2-T4 records `LOOP-CLOSED: no` (see section 8, G-1). |
| Simplicity first | PASS | One predicate, one conditional, no new switch, no new abstraction. |
| Reusability | PASS | The predicate is a standalone pure function with a documented contract. |
| Extensibility / public API | PASS | No command-line parameter added or changed; no caller can opt out of the unscoped gate (issue.md AC preamble). |
| Separation of concerns | PASS | The predicate performs no I/O (Grep for `Get-ChildItem`, `Test-Path`, `Resolve-Path`, `Get-Content`, `Set-Content` in the part file: 0); the entry point keeps the I/O. |
| Error handling / logging | PASS | Scoped run: explicit `Write-Warning` naming both roots (fail loud, not silent). Unscoped run: the two `throw`-ing assertions unchanged. Collector failure still throws before any threshold logic (It 11). |
| Module and file structure (500-line limit) | PASS | Entry point 466 lines, part file 49, test file 232 (Read line counts; P2-T5 arithmetic agrees). Invoke-MSTest.ps1 unchanged at 262. |
| Naming, docs, comments | PASS | Approved verb `Test-`; comment-based help on the predicate and on `Invoke-MSTestWithCoverageMain` (`.DESCRIPTION`, `.PARAMETER SearchRoot`); comments state why (issue #928, part-file placement). See CR-3 for a comment-aging note. |
| Dependencies | PASS | None added. |
| Match existing style | PASS | Part-file pattern, wrapper-seam mocks, parse-and-dot-source import and guarded imports all copied from sibling files. |
| Toolchain loop (format, lint, type-check, test) | PASS on the three PowerShell stages; the plan-level coverage condition is the open item | P2-T1 (format, HB equals HA, empty porcelain), P2-T2 (analyze), P2-T3 (test). Type checking does not apply to PowerShell per the rule file. Architecture-boundary, contract and integration stages: no repository-defined stage exists for PowerShell scripts. |
| After-changes documentation | PASS | Script help updated; plan check-offs recorded; handoff records written. |

## 3. Language-Specific Code Change Policy Compliance

PowerShell (.claude/rules/powershell.md):

| Item | Verdict | Evidence |
|---|---|---|
| Formatting via PoshQC MCP formatter | PASS | P0-T5 liveness control proved the formatter live on this tree (an injected indent was removed); P1-T7 and P2-T1 show identical raw-byte hashes before and after a format run for all four Write Set PowerShell files; Invoke-MSTest.ps1 unchanged (hash 9aec072f... at baseline and after). |
| PSScriptAnalyzer via PoshQC | PASS | P2-T2 runs (C) entry point, (D) part file, (E) test file, (B) test folder: `ok: true` each. Folder run (A) reports 13, equal to the P0-T6 baseline of 13; Invoke-MSTest.ps1 reports 2, equal to its baseline (the two pre-existing `PSAvoidUsingWriteHost` findings that file's own comment records as retained). No suppression added. |
| Type checking | PASS | The rule file defines no type-checking stage for this language; the stage is skipped by rule. |
| PowerShell 7+ compatibility | PASS | `Set-StrictMode -Version Latest`, `[IO.Path]::GetFullPath`, `[StringComparison]::OrdinalIgnoreCase`; no Windows-PowerShell-only construct. |
| Advanced functions, CmdletBinding, Mandatory parameters | PASS | `Test-CoverageRunIsScoped` carries `[CmdletBinding()]`, `[OutputType([bool])]`, two `[Parameter(Mandatory = $true)]` string parameters. |
| ShouldProcess for state-changing actions | PASS | The predicate changes no state. |
| No global state, no Invoke-Expression, no hard-coded paths | PASS | Grep over both production files for `Invoke-Expression`: 0. `$global:LASTEXITCODE` handling is pre-existing and unchanged. |
| Approved verbs | PASS | `Test-` is approved; analyzer clean. |
| Under 500 lines | PASS | See section 2. |
| Change budget (at most 3 production files per batch) | PASS | 2 production files changed (entry point, new part file); the plan reserved a third (Invoke-MSTest.ps1) that the formatter did not touch. |
| Design seams | PASS | No new executable call; existing wrapper seams reused. |
| Prohibited behaviors (broad refactor, analyzer debt, weakened assertions, sleeps, claiming success without toolchain) | PASS | None observed. The executor did not claim AC6; it recorded `AC6-STATUS: PENDING` and escalated. |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Pester 5.x, Describe/Context/It | PASS | Three Describe blocks, two Context blocks, 14 It blocks; Pester 5.6.1 pinned in CI. |
| Mirrors code structure, `*.Tests.ps1` | PASS | tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1. |
| Mock sparingly, prefer real code paths | PASS with note | The threshold assertions and the collection wrapper run for real; 13 seams are mocked, the same set as the sibling `AssemblyDiscovery` suite requires to keep the entry point I/O-free. |
| Never mock executables directly | PASS | `Invoke-VsWhereExe` and `Invoke-DotnetCoverageExe` (wrapper seams) are mocked; `vswhere` and `dotnet-coverage` are not. |
| Mock signature parity | PASS | `param([string]$VsWherePath, [string[]]$VsWhereArgs)` and `param([string[]]$DotnetCoverageArgs)` match production. |
| Mock registration order / AST import order | PASS | Entry point parsed and dot-sourced, then helpers, then the guarded part file, all in `BeforeAll`; mocks registered in `BeforeEach` before invocation. |
| Deterministic test requirements | PASS | No PATH, cwd, profile or network dependence; `$PSScriptRoot`-relative resolution only. |
| Changed-line coverage regression is blocking | FAIL (blocking) | Entry-point changed line 408 reads 0 hits by the agreed route; see section 5 and G-1. The line is executed (It 10 passes on the warning's text), so the defect is in crediting, not in test presence. |

## 5. Test Coverage Detail

Route: C (direct Pester 5.6.1 run mirroring the CI workflow; the bundled MCP document does not name the changed files, see 1.2.2).

| File | Status | Baseline lines (covered/analyzable) | Post-change lines | Baseline % | Post-change % | Delta (points) | Uncovered changed lines |
|---|---|---|---|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | modified | 113/126 | 115/129 | 89.68% | 89.15% | -0.53 | 408 |
| scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 | new | (absent) | 5/5 | (absent) | 100.00% | new file | none |
| scripts/vscode/Invoke-MSTest.ps1 | unchanged | 49/56 | 49/56 | 87.50% | 87.50% | 0.00 | (no changed lines) |
| CI population (scripts/dependencies + scripts/vscode) | | 1613/1707 | 1620/1715 | 94.49% | 94.46% | -0.03 | |

Changed-line derivation for the entry point (from the executor's `git diff -U0` hunk headers, checked against the file as read): help lines 278 to 291 carry no coverage point; lines 311 to 314 add the dot-source (313 covered); lines 404 to 414 add the conditional (407 covered, 408 uncovered, 412 and 413 are the two pre-existing assertion statements moved into the else arm, covered). Every other uncovered entry-point line is a baseline uncovered line shifted by 18 or 27 lines (149, 171, 187, 229 unchanged; 315 to 352 become 333 to 370; 414, 430, 438 become 441, 457, 465).

Thresholds (uniform tier rule, .claude/rules/quality-tiers.md):

- New file line coverage at least 85% (90% per CLAUDE.md): PASS (100.00%).
- Modified file line coverage at least 85%: PASS (89.15%).
- Modified file no regression on changed lines: FAIL (line 408 at 0 hits; file figure 89.68% to 89.15%).
- Repo-wide (CI population) at least 85%: PASS (94.46%).
- Repo-wide not below recorded baseline (AC6 clause): FAIL (94.46% against 94.49%).

Root cause (executor's diagnostics, evaluated independently): each of the four test files that invoke `Invoke-MSTestWithCoverageMain` imports the entry point with `Parser::ParseFile(...).GetScriptBlock()` and dot-sources its own compiled copy (Invoke-MSTest.RunSettings.Tests.ps1 line 17 to 22, Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 lines 13 to 18, Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 lines 13 to 18, the new file lines 14 to 19). Pester 5.6.1 uses breakpoint-based coverage by default; PowerShell binds a pending line breakpoint to the first compiled copy of the file in which that function executes. In the full run `Invoke-MSTest.RunSettings.Tests.ps1` sorts first (a period sorts before `W`), so entry-point breakpoints bind to its copy, and a line reached only by a later file's copy records no hit. Line 408 is reached only by the new file's scoped cases. The executor's three ordered runs (new file alone: line 408 hit; AssemblyDiscovery then new file: 0 hits; reversed: hit) and a fourth run with profiler-based coverage (hit) are consistent with this mechanism. The part files, by contrast, are dot-sourced by path from inside the entry point, which is the likely reason the new part file reads 5/5 from every test file: PowerShell serves a path dot-source from its per-path compiled-script cache, so all test files share one compiled copy. That premise should be confirmed by one measured two-file run before the remediation below relies on it.

## 6. Test Execution Metrics

| Run | Command (as recorded) | Result |
|---|---|---|
| P0-T7 baseline (bundled) | `mcp__drm-copilot__run_poshqc_test`, four scan folders | JUnit root tests 320, failures 0, errors 0; 26 suites |
| P0-T7 baseline (Route C) | CMD-PESTER-DIRECT, Set-Location pinned to the worktree | passed 320, failed 0, skipped 0; LINE covered 1613, missed 94 |
| P1-T3 expect-fail (new file only) | `mcp__drm-copilot__run_poshqc_test` | tests 14, failures 11, errors 0 (It 1 to 10 and 14 fail; 11, 12, 13 pass) |
| P1-T6 pass-after (new file only) | `mcp__drm-copilot__run_poshqc_test` | tests 14, failures 0, errors 0 |
| P2-T3 final (bundled) | `mcp__drm-copilot__run_poshqc_test`, four scan folders | tests 334, failures 0, errors 0, skipped 0; 27 suites; new suite 14/14 (JUnit root in the worktree re-read by this review: `tests="334" errors="0" failures="0"`) |
| P2-T3 final (Route C) | CMD-PESTER-DIRECT | passed 334, failed 0, skipped 0; LINE covered 1620, missed 95 |

Reviewer-run tests: none (Bash withheld). Timing: the new suite 0.521 s; whole bundled run 36.042 s (gitignored JUnit root `time` attribute).

## 7. Code Quality Checks

| Check | Result |
|---|---|
| Formatter (PoshQC format, MCP route) | PASS: no rewrite of any Write Set file; formatter liveness proven (P0-T5) |
| Analyzer (PoshQC analyze, MCP route) | PASS: 0 findings on the entry point, the part file and the test file; folder counts equal baseline |
| Type check | No type-checking stage is defined for this language by the rule file; skipped by rule |
| Tests | PASS: 334/334 |
| File size limit | PASS: 466 / 49 / 232 lines |
| Analyzer suppressions added | none |
| Workflow files modified | none (P2-T6 name-only diff over .github lists nothing; the MSTest coverage workflow still invokes `-SearchRoot .` at line 95; no green-run gate applies) |
| Committed Test Evidence Format | PASS: Glob over the feature folder finds no `.xml` or `.trx`; every figure is a derived projection |
| Host identifiers in committed files | PASS: Grep over the feature folder and the promoted record for the developer account name, the host name (read from the gitignored JUnit document and not transcribed here), the drive-letter path pattern, the user-profile directory prefix pattern and the fixture drive root returns only the GitHub issue URL's scheme separator |
| Tone (agent-authored artifacts) | PASS: measured, factual, no hyperbole or humor in the 27 evidence artifacts read |

## 8. Gaps and Exceptions

- G-1 (Blocking): AC6 unmet by the agreed measurement route. Two clauses fail: "does not fall below its recorded baseline" (94.46% against 94.49%) and "every changed production line covered" (entry-point line 408). The plan's final QC loop is recorded as not closed (P2-T4 `LOOP-CLOSED: no`; P2-T3 and P2-T4 unchecked). The code behavior the line implements is verified by a passing test (It 10, `Should -Invoke Write-Warning -Times 1 -Exactly` with a parameter filter on the message). Disposition: Blocking until remediated or ratified; see the remediation-inputs artifact. Reviewer's evaluation of the four handoff options: (1) measuring with `UseBreakpoints = $false` is rejected as the primary remedy because the evidence route must match the CI workflow's instrumentation and the baseline would have to be re-measured by the new route; changing CI itself is a separate workflow item requiring a green run. (2) Adding a scoped case to the first-binding sibling file is rejected: that file is out of scope and at 499 lines, and the fix would depend on alphabetical file ordering and recur for the next scoped-only line. (3) Relocating the scoped and unscoped arms into the path-loaded Scope part file behind one unconditional entry-point call is recommended: it follows the Coverage Exclusion Policy's own guidance (thin wiring in the entry point, logic in a testable module), keeps the two assertion statements textually identical and ordered, keeps every AC1 to AC5 test valid, adds a directly unit-testable function, and, if the cache premise holds, credits every line from every test file. Its premise (a path-loaded part file is credited from any test file) must be confirmed with one two-file diagnostic run before adoption. (4) A maintainer-ratified measurement exception is acceptable only as a fallback and only if transcribed into issue.md beside AC6 (a waiver recorded solely in gitignored orchestrator state does not reach the PR).
- G-2 (Informational, pre-existing): the same crediting defect affects any entry-point line reached only by a test file that sorts after `Invoke-MSTest.RunSettings.Tests.ps1`. Recommend promotion to a potential-feature entry: convert the four entry-point suites to dot-source the script by path (the `InvocationName` guard at entry-point line 464 makes that safe) or evaluate profiler-based coverage in `_pester.yml`.
- G-3 (Informational, pre-existing, outside this diff): tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 lines 41, 42, 104 and 124 embed the developer account name inside literal fixture paths. Recommend a hygiene issue to replace them with a neutral drive-letter fixture root.
- G-4 (Informational): CLAUDE.md states 80% line coverage for PowerShell and the threshold script enforces 80% / 75% for C#, while .claude/rules state 85% / 75%. This is the known unreconciled documentation conflict; this change does not alter any literal, and every figure here clears both floors.
- G-5 (Informational): evidence timestamp labels for P2-T7 to P2-T12 were assigned without a clock read and later renamed to 09-27; the executor disclosed this in P2-T13. Labels are not used for ordering.
- G-6 (Informational): the PR-context artifacts were absent in the worktree and stale in the session root (they describe the `documentationandmemories` branch at 1c80f6e0e). A hand-authored `artifacts/pr_context.summary.txt` was written in the worktree (gitignored) from the executor's verbatim numstat; no appendix could be generated without git access.
- G-7 (Informational): the branch head SHA a17ca5bcf and the commit state (Phase 0 and 1 at ab3b41890 and 313d0b918; Phase 2 evidence committed afterwards, per the caller) were taken from the caller and the handoff record; not re-derived.

## 9. Summary of Changes

- `scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1` (new, 49 lines): `Test-CoverageRunIsScoped -RepoRoot -ResolvedSearchRoot`, pure, `GetFullPath` normalisation, trailing-separator trim of both separator characters, ordinal case-insensitive equality negated.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (+29/-2): `.DESCRIPTION` and `.PARAMETER SearchRoot` help on `Invoke-MSTestWithCoverageMain`; dot-source of the Scope part file after the TrxSummary dot-source; the two `Assert-Cobertura*CoverageThreshold` calls moved verbatim into the else arm of `if (Test-CoverageRunIsScoped ...) { Write-Warning (...) } else { ... }`.
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1` (new, 232 lines): 8 predicate cases, 3 scoped-run cases, 2 unscoped-run cases, 1 help case.
- `scripts/vscode/Invoke-MSTest.ps1`: unchanged (formatter rewrote nothing; the issue's "would rewrite" claim was measured with a bare `Invoke-Formatter`, not the repository route, per plan D9).
- Documentation: issue.md AC check-offs (AC1 to AC5, AC7), plan check-offs, 27 evidence artifacts, the promoted potential-feature record.

## 10. Compliance Verdict

- General Unit Test Policy: FAIL on the changed-line no-regression rule (G-1); PASS on every other item.
- General Code Change Policy: PASS (the toolchain's three PowerShell stages pass; the plan-level coverage condition is G-1).
- PowerShell Code Change Policy: PASS.
- PowerShell Unit Test Policy: FAIL on the changed-line rule (G-1); PASS otherwise.
- Committed Test Evidence Format and host-identifier hygiene: PASS.
- Evidence location: PASS.
- Blocking findings: 1 (G-1 / AC6 PARTIAL, blocking).
- Overall: REMEDIATION REQUIRED. Remediation inputs: `remediation-inputs.2026-09-29T10-00.md` in this folder.

## Appendix A: Test Inventory

tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1 (14 It blocks):

| # | Describe / Context | It | AC | Fail-before (P1-T3) | Pass-after (P1-T6, P2-T3) |
|---|---|---|---|---|---|
| 1 | Test-CoverageRunIsScoped | returns false when the resolved search root equals the repository root | preamble | fail | pass |
| 2 | Test-CoverageRunIsScoped | returns false for a dot search root beneath the repository root | preamble | fail | pass |
| 3 | Test-CoverageRunIsScoped | returns false for a dot-backslash search root beneath the repository root | preamble | fail | pass |
| 4 | Test-CoverageRunIsScoped | returns false when only a trailing separator differs | preamble | fail | pass |
| 5 | Test-CoverageRunIsScoped | returns false when only letter case differs | preamble | fail | pass |
| 6 | Test-CoverageRunIsScoped | returns true for a subdirectory search root | preamble | fail | pass |
| 7 | Test-CoverageRunIsScoped | returns true for a sibling directory whose name extends the repository root name | preamble | fail | pass |
| 8 | Test-CoverageRunIsScoped | returns true for the parent directory of the repository root | preamble | fail | pass |
| 9 | threshold gating / scoped run | completes without error on a scoped run whose post-processed document is below both floors | AC1 | fail (threshold message) | pass |
| 10 | threshold gating / scoped run | writes exactly one warning naming the skipped assertions and the scoped search root | AC1 | fail (threshold message) | pass |
| 11 | threshold gating / scoped run | still terminates with an error when collection returns a non-zero exit code on a scoped run | AC2 | pass (control) | pass |
| 12 | threshold gating / unscoped run | throws the line threshold message when the search root is omitted and the line rate is below 80 percent | AC3 | pass (control) | pass |
| 13 | threshold gating / unscoped run | throws the branch threshold message for a dot search root when the branch rate is below 75 percent | AC3 | pass (control) | pass |
| 14 | comment-based help | documents the scoped-run behavior on the SearchRoot parameter | AC4 | fail (`$key.Count` 0) | pass |

Existing suites: 26 files, 320 tests, all passing before and after (P0-T7, P2-T3); none edited.

## Appendix B: Toolchain Commands Reference

Recorded by the executor (this review executed none of them):

- Format: `mcp__drm-copilot__run_poshqc_format` with `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`, observed through raw-byte hashes (`git hash-object --no-filters`) and porcelain before and after.
- Analyze: `mcp__drm-copilot__run_poshqc_analyze` with the six scan sets (A) to (F) in evidence/qa-gates/p2-t2-analyze.iter1.2026-09-29T09-18.md.
- Test (bundled): `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]`; JUnit read from artifacts/pester/pester-junit.xml.
- Test with coverage (Route C): CMD-PESTER-DIRECT as quoted in plan.2026-09-28T19-45.md (Pester 5.6.1, `Run.Path` over both test folders, `CodeCoverage.Path` over `scripts/dependencies` and `scripts/vscode`, JaCoCo output under the gitignored `coverage/` directory), mirroring `.github/workflows/_pester.yml` lines 36 to 47.
- Diff and footprint: `git diff --numstat 177b6d78e -- scripts/vscode tests/scripts/vscode`; `git diff -U0 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1`; `git diff --name-only 177b6d78e -- scripts tests .github .vscode config`; `git status --porcelain -uall`.
