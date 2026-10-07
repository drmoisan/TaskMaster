# Policy Audit — package-manifest-consistency-residuals (Issue #929)

- Artifact timestamp label: 2026-09-30T10-30 (assigned by the calling orchestrator; see Template Resolution Deviation, item 4)
- Reviewer: feature-review agent (reduced audit, work mode `minor-audit`)
- Branch: `bug/package-manifest-consistency-residuals-929`
- Branch head at review: `5ce3c8c3b585df9f8e724d4a740ee94c2df1ad1a` (read from the worktree's git ref)
- Base for the source diff: `231e1c0b55105aeb626bf5a6e8d0266a567cacad` (origin/main at the first merge, 3091b8af9; recorded by P0-T1 as BASE-SHA)
- Item commits: `488492f13` (phase 0 evidence), `b96926588` (implementation), `9e41ffbcf` (final QC evidence and check-off), `d9f50955d` (handoff index); second merge of origin/main `5ce3c8c3b` touched no file in this item's scope (caller statement, not re-derived here)
- Review worktree: `<session-root>/.claude/worktrees/agent-a74dcedbc13b789fd`
- Feature folder: `docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/`
- Total blocking findings: **0**
- Total non-blocking findings: **12**
- Verdict: **PASS** (no remediation required)

## Template Resolution Deviation

1. The MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` are not in this session's tool set. This artifact is hand-authored and preserves the thirteen canonical headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` section 5 verbatim.
2. The caller's binding tool constraint forbids the Bash tool for this review. Every fact below was established with Read, Grep and Glob against the worktree, the executor's committed evidence, and the gitignored tool documents the executor left under `coverage/` and `artifacts/`. No command was executed by the reviewer; no coverage was regenerated (the review contract requires artifact inspection, not regeneration).
3. `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` do not exist in the review worktree, and the copy in the session checkout belongs to a different branch (`bug/stale-analyzer-include-paths`, head `7ef8941e2`, generated 2026-09-29 15:21:41 UTC). Regeneration needs the PR-context MCP tool or `git`, neither of which is available under the tool constraint. Scope was therefore derived from three independent sources that agree: the caller's pasted diff, the executor's anchored name-only diff at P2-T10 (`git diff --name-only 481b33c59 -- .`), and on-disk verification of every named file.
4. No clock is available without Bash. The timestamp label `2026-09-30T10-30` was supplied by the caller; it sorts earlier than several executor evidence labels (the handoff index is labelled `11-22`), which is a property of caller-assigned labels and not a claim about ordering. See Finding NB-1 on the executor labels themselves.

## Rejected Scope Narrowing

None detected. The caller's directive restricts the acceptance-criteria source to the `## Acceptance Criteria` section of `issue.md`, which is the legitimate `minor-audit` rule, and states that Summary item 4 (the maintainer credential follow-up) is not an acceptance criterion, which `issue.md` line 52 itself states. The caller's pasted diff omits the feature folder, the promoted record and the new test file, but names all three as present in the branch diff; all three were audited from disk (Section 9 lists them). The audit scope is the full branch diff against the resolved base.

## Evidence Location Compliance

- Branch diff scanned (P2-T10 anchored name-only diff, 46 paths, plus the untracked porcelain capture): zero paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. Every evidence artifact lies under `docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/{baseline,regression-testing,qa-gates,other}/`. **PASS**
- `validate_evidence_locations.py` is not present in this checkout (Glob `**/validate_evidence_locations.py` returned nothing) and could not have been executed under the tool constraint; the scan above was performed by reading the P2-T10 footprint and the feature-folder Glob listing (69 files).
- Committed test evidence form (CLAUDE.md "Committed Test Evidence Format"): the four coverage copies are two package-level JaCoCo projections (9 `<package>` elements each, no `class`, `sourcefile`, `method` or `line` element) and two trx-derived summaries. No raw Cobertura, raw trx, raw Pester JaCoCo or PoshQC JUnit document is tracked. **PASS**
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none; the caller supplied no non-canonical evidence path.

## Executive Summary

The change removes two `Exists()`-guarded `altcover` `<Import>` elements from `QuickFiler.Test/QuickFiler.Test.csproj`, corrects two stale binding redirects in `SVGControl/app.config` (Fizzler 1.3.1.0, System.Runtime.CompilerServices.Unsafe 6.0.3.0), switches `.github/workflows/dependabot-repair.yml` from the deprecated `app-id` input to `client-id`, aligns the 911 runbook and the workflows README with that input, updates a three-line comment in `scripts/dependencies/ConsistencyVerifier.psm1`, adds two in-memory Import-kind tests to `ConsistencyVerifier.Tests.ps1`, and adds a four-test tree-reading regression file `RepositoryTreeConsistency.Tests.ps1`. No C# source file changes.

Every policy gate evaluated PASS. C# toolchain (CSharpier check, analyzer rebuild, nullable rebuild, MSTest with coverage) and PowerShell toolchain (PoshQC format, analyze, test; coverage read from the CI Pester job) passed on the final iteration with no regression against the Phase 0 baseline. Repo-wide line coverage: C# 85.92% (85.91% baseline), PowerShell 94.51% (94.51% baseline). The only modified PowerShell production file changed comment lines only and holds 158/160 lines covered in both CI documents, with the same two uncovered lines before and after.

Twelve non-blocking findings are recorded in Section 8; none requires remediation before merge. The CI `mstest-coverage` failure on run 36722780748 is assessed as not attributable to this change (Section 6).

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The six new `It` blocks share no mutable state; the tree tests read tracked files by absolute path resolved from `$PSScriptRoot` (`RepositoryTreeConsistency.Tests.ps1` lines 4, 10-11); fixtures in `ConsistencyVerifier.Tests.ps1` are script-scoped here-strings (lines 60-78). |
| Isolation | PASS | Each `It` targets one detector call or one repository invariant; failure messages name the offending element (line 89 lists each `Import` as `<leaf>: line <n> <folder>`). |
| Fast execution | PASS | Local PoshQC run over `tests/scripts/dependencies`: RUN-START 13:46:37Z, JUNIT-WRITTEN 13:47:26Z for 137 tests (P2-T3 iter2). |
| Determinism | PASS | No `Start-Sleep`, retry, wall-clock read or randomness in either test file (Grep for `Start-Sleep` returned nothing). Fail-before (4 of 4 red, P1-T2) and pass-after (4 of 4 green, P1-T11) reproduce deterministically; the intermediate state at P1-T7 (3 green, 1 red) matches the plan's decision D13 exactly. |
| Readability | PASS | Arrange/Act/Assert comments in every new `It`; every `Should` carries `-Because` text. |

### 1.2 Coverage

Coverage languages with changed files on this branch: PowerShell (`scripts/dependencies/ConsistencyVerifier.psm1` comment-only; `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`; `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`) and C# build configuration (`QuickFiler.Test/QuickFiler.Test.csproj`; zero `.cs` files). TypeScript and Python have zero changed files.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/p0-t12-coverage-projection.2026-09-28T20-01.jacoco.xml` (package-level projection; LINE covered sum 56475 of 65736) with `evidence/baseline/p0-t12-test-results.2026-09-28T20-01.summary.txt`
- C# post-change coverage artifact: `evidence/qa-gates/p2-t7-coverage-projection.2026-09-28T20-01.jacoco.xml` (package-level projection; LINE covered sum 56479 of 65736) with `evidence/qa-gates/p2-t7-test-results.2026-09-28T20-01.summary.txt`; the raw post-processed document survives gitignored at `coverage/coverage.cobertura.xml` (root `line-rate="0.859179" branch-rate="0.800809" lines-covered="56479" lines-valid="65736" branches-covered="13657" branches-valid="17054" timestamp="1790776305"`)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: CI Pester job 109731601928 of run 36666302259 (main, head `231e1c0b5`), JaCoCo document downloaded gitignored at `coverage/ci-main-pester-36666302259-1/pester-coverage.xml` (report name `Pester (09/30/2026 03:53:09)`, report-level LINE covered 1721 missed 100), transcribed in `evidence/baseline/p0-t16-pester.2026-09-28T20-01.md`
- PowerShell post-change coverage artifact: CI Pester job 109911885533 of run 36722780748 (branch, head `b96926588`), JaCoCo document downloaded gitignored at `coverage/ci-branch-pester-36722780748-2/pester-coverage.xml` (report name `Pester (09/30/2026 13:37:22)`, report-level LINE covered 1721 missed 100), transcribed in `evidence/qa-gates/p2-t3-pester.iter2.2026-09-28T20-01.md`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 1 (`QuickFiler.Test.csproj`; 0 `.cs`) | 7346 MSTest | 7346 passed, 0 failed (iteration 2) | 85.91% lines / 80.08% branches | 85.92% lines / 80.08% branches | N/A - no C# source line added or changed |
| PowerShell | 3 (1 production comment-only, 2 test files) | 137 local (PoshQC, `tests/scripts/dependencies`) / 379 CI | 137 passed, 0 failed local; 379 passed, 0 failed CI | 94.51% lines | 94.51% lines | 98.75% |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.91% lines (56475/65736) / 80.08% branches (13656/17054) -> Post-change: 85.92% lines (56479/65736) / 80.08% branches (13657/17054). Change: +0.01% lines (+4 covered, all in the untouched UtilitiesCS package) / 0.00% branches (+1 covered). Disposition: PASS. Evidence: the two committed projections reconcile to the printed first-party lines (P0-T12 and P2-T7 iter2); the raw Cobertura root counters match the projection sums; the delta lies entirely in an assembly this change does not touch and is within the known run-to-run band of the C# coverage constants; repo-wide line 85.92% is at or above both the 85% rules floor and the 80% CLAUDE.md floor and branch 80.08% is above the 75% floor; C# coverage verdict: PASS.
- PowerShell: Baseline: 94.51% lines (1721/1821). Post-change: 94.51% lines (1721/1821). Change: 0.00% lines (0 covered, 0 missed). New/changed-code coverage: 98.75%. Disposition: PASS. Evidence: CI Pester jobs 109731601928 (main) and 109911885533 (branch) both print `COVERAGE LinePercent=94.51 Covered=1721 Total=1821`; the only modified production file `dependencies/ConsistencyVerifier.psm1` reads LINE covered 158 missed 2 in both JaCoCo documents (98.75%), the two uncovered source lines are 430 and 475 in both documents (JaCoCo rows 495 and 525 inside the sourcefile block at rows 374-536), and the three changed lines 221-223 are comment-based-help lines that carry no JaCoCo line node, so zero executable production lines changed and no changed-line regression is possible; the 98.75% figure is that file's post-change line coverage, reported because no smaller changed-line population exists; the new files are test files and are outside the denominator by policy; Pester coverage verdict: PASS; Pester reports line and command coverage only, so the branch threshold does not apply to this language and no branch figure is claimed.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Canonical artifact path | State in review worktree | Disposition |
|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` | absent (gitignored path; the executor's runner writes `coverage/coverage.cobertura.xml`, present, and the committed projection stands in per the feature-evidence rule) | PASS on the committed projection and the raw Cobertura root counters |
| PowerShell | `artifacts/pester/powershell-coverage.xml` | present, written by the bundled PoshQC route at local time 09:47:03 on 2026-09-30; report-level LINE `missed="9294" covered="0"`; every `<package>` it instruments lies under `.claude/hooks`, never under `scripts/` | Pester coverage on that document: FAIL (0.00%), a known limitation of the bundled route recorded at issue 928 (P-4) and pre-existing; it is not the measurement of record and is non-blocking |
| PowerShell (measurement of record) | CI `pester-coverage` artifact, run 36722780748 | present gitignored at `coverage/ci-branch-pester-36722780748-2/pester-coverage.xml` | PASS (94.51%) |

Documentation conflict, recorded and not re-raised: CLAUDE.md states an 80% line floor and a 90% new-module target; `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` and `.claude/rules/powershell.md` state 85% line / 75% branch uniformly. Every figure above satisfies both sets. The conflict is tracked as open issue 668 (plan convention 10).

### 1.3 Scenario Completeness, Structure, External Dependencies, Location

| Check | Verdict | Evidence |
|---|---|---|
| Positive and negative flows for the AC4 detector | PASS | `ConsistencyVerifier.Tests.ps1` lines 240-251 (two guarded Imports of an unmanifested package: FindingCount 2, every Kind `Import`, every PackageFolder `altcover.8.6.45`) and lines 253-263 (manifest declares the imported package: Import-kind subset empty, ExaminedCount greater than 0). |
| Regression coverage of the three fixes | PASS | `RepositoryTreeConsistency.Tests.ps1` tests 1-4 red before the fix (P1-T2, messages quote `QuickFiler.Test.csproj` / `altcover.8.6.45`, `1.3.0.0` versus `1.3.1.0`, `client-id`, secret name) and green after (P1-T11). |
| Arrange-Act-Assert | PASS | Every new `It` is sectioned with the three comments. |
| Clear failure messages | PASS | Every `Should` carries `-Because`; test 1 concatenates every offending Import into the message. |
| No external services, no temporary files | PASS | Grep over both test files for `Out-File`, `Set-Content`, `Add-Content`, `New-Item`, `New-TemporaryFile`, `[System.IO.File]::Write*`, `Remove-Item`: no match. Reads are `ReadAllText` / `ReadAllLines` on tracked files. |
| No banned timing APIs | PASS | Grep for `Start-Sleep`: no match. |
| Test file location mirrors production | PASS | `tests/scripts/dependencies/` mirrors `scripts/dependencies/`, the existing convention of the seven sibling `*.Tests.ps1` files. |
| Test names avoid `AC<digit>` | PASS | Both files state the rule in their header (lines 7-9) and no `It` name matches it (P1-T3 count 0; confirmed by reading every `It` line). |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test first, minimal fix, verify | PASS | P1-T1/P1-T2 authored and ran the tree tests red (4 of 4) before P1-T4 to P1-T9 applied the fixes; P1-T11 ran them green; P2 loop ran the toolchain twice (iteration 1 failed at P2-T7 on a pre-existing timing test, iteration 2 passed all seven steps on the unchanged tree). |
| Simplicity, no opportunistic refactor | PASS | Decision D1: the detector already reported Import-kind absences; no production rule was added. The production diff to `ConsistencyVerifier.psm1` is three comment lines for three (499 lines before and after). |
| Reusability | PASS | The new test file reuses the step-block parsing approach of `DependabotConfig.Tests.ps1` (lines 14-21 say so) rather than a new framework. |
| Separation of concerns | PASS | The pure detector `Find-PackageAbsentFromManifest` is exercised with in-memory strings; the tree test isolates file reads to the Arrange step. |
| File size limit (500 lines) | PASS | P2-T9: `QuickFiler.Test.csproj` 568 (XML project file, not code; was 570), `app.config` 23, `dependabot-repair.yml` 173, `ConsistencyVerifier.psm1` 499, `ConsistencyVerifier.Tests.ps1` 337, `RepositoryTreeConsistency.Tests.ps1` 152 (Read shows 153 with the trailing newline). Test files count toward the limit and both are under it. |
| Error handling / logging | PASS | No production logic changed. |
| Naming | PASS | `Get-TokenStepBlock`, `Get-DependentAssemblyBlock` use approved verbs and descriptive nouns; PoshQC analyze reported ok (P2-T2 iter2). |
| Public API compatibility | PASS | No exported function signature changed; `Find-PackageAbsentFromManifest` parameters unchanged (lines 229-234). |
| Dependencies | PASS | No library added. |
| Toolchain loop restart rule | PASS | Iteration 1 P2-T7 failure restarted the loop at P2-T1 (iter2 artifacts for all seven steps). |
| Supporting documents updated | PASS | Runbook (steps 10, 22, YAML sample), workflows README line 116, workflow header comment lines 13-14, verifier comment lines 221-223, fixture comments in the test file. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 PowerShell (`.claude/rules/powershell.md`)

| Requirement | Verdict | Evidence |
|---|---|---|
| Format via PoshQC MCP | PASS | P2-T1 iter2: 14 hashes identical before and after; rewrite count 0. |
| Analyze via PoshQC MCP | PASS | P2-T2 iter2: `PoshQC analyze: pass (0 findings); tool reports no count` (the ruling's required wording). |
| Test via PoshQC MCP | PASS | P2-T3 iter2: JUnit root tests=137 failures=0. |
| PowerShell 7+ compatibility | PASS | No PS5-only construct; `[System.Collections.Generic.List[object]]::new()` and `-LiteralPath` are 7-safe. |
| Advanced-function conventions in production code | PASS | No production function changed. |
| Change budget (2 production files direct-mode) | PASS | One production file, comment-only. |
| Coverage regression on changed lines | PASS | Zero executable lines changed (Section 1.2.1). |

### 3.2 C# build configuration (CLAUDE.md C# Code Change Policy)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier check via `dotnet tool run` | PASS | P2-T4 iter2: `Checked 1625 files`, exit 0, no findings, equal to P0-T9. `.csproj` and `app.config` are excluded by `.csharpierignore` and were not touched by the formatter. |
| Analyzer rebuild (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | PASS | P2-T5 iter2: `Build succeeded.`, 0 warnings, 0 errors, MSBUILD_EXIT=0, CS0006_LINES=0, OUT_LINES 36 equal to P0-T10. |
| Nullable rebuild (`/t:Rebuild`, `TreatWarningsAsErrors`, no `/p:Nullable=enable`) | PASS | P2-T6 iter2: `Build succeeded.`, 0 warnings, 0 errors, MSBUILD_EXIT=0, OUT_LINES 36 equal to P0-T11. Command text matches CLAUDE.md character-for-character. |
| MSTest with coverage via `Invoke-MSTestWithCoverage.ps1` | PASS | P2-T7 iter2: 7346 of 7346 passed; first-party 85.92% lines / 80.08% branches. |
| No `dotnet format`, no `Invoke-VSBuild.ps1` wrapper | PASS | Command records in P2-T4 to P2-T6 name the literal commands (plan convention 7). |

### 3.3 CI workflow authoring (`.claude/rules/ci-workflows.md`)

| Requirement | Verdict | Evidence |
|---|---|---|
| Deliberately-failing nested command pattern | PASS (no `run:` block changed, so the pattern is not engaged) | The change replaces `app-id:` with `client-id:` at line 51 inside a `uses:` step; no `run:` block changed. |
| actionlint | PASS | P1-T7: actionlint 1.7.7, scoped run and repository-wide run both exit 0 with no output; CI `actionlint` job on run 36722780748 succeeded. |
| Modified workflow needs a green run | PARTIAL, non-blocking (Finding NB-3) | `dependabot-repair.yml` is `workflow_run`-triggered and its first step mints an App token from secrets the maintainer has not yet provisioned (issue Summary item 4, deliberately not a merge gate). No run of this workflow can be green on any branch until then; the static lint and the Pester static test cover the changed line. |

## 4. Language-Specific Unit Test Policy Compliance

### 4.1 PowerShell / Pester

| Requirement | Verdict | Evidence |
|---|---|---|
| Pester v5 `Describe`/`Context`/`It` | PASS | `Describe 'Repository tree consistency (issue 929)'` with four `It`; two `It` added under `Context 'Package absent from the manifest'`. |
| One behavior per `It` | PASS with a note (NB-7) | Test 2 iterates two assemblies inside one `It`; a Fizzler failure would mask the Unsafe assertion. Test 1 is a deliberate census and reports every offending Import in one message. |
| `*.Tests.ps1` naming and mirrored layout | PASS | `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`. |
| Mock sparingly, no executable mocks | PASS | No `Mock` in either file. |
| No PATH / cwd / profile reliance | PASS | Paths resolve from `$PSScriptRoot`; `Set-StrictMode -Version Latest` at the top of both files. |
| Line coverage at or above 85% | PASS | 94.51% aggregate; 98.75% for the modified module. |

### 4.2 C# / MSTest

No C# test source changed. The existing suite (MSTest, Moq, FluentAssertions) ran 7346 of 7346 green on the final local iteration (P2-T7 iter2), equal to the Phase 0 baseline (P0-T12). **PASS**

## 5. Test Coverage Detail

| File | Kind | Baseline | Post-change | Threshold | Verdict |
|---|---|---|---|---|---|
| `scripts/dependencies/ConsistencyVerifier.psm1` | modified production (comment lines 221-223 only) | 158/160 lines (98.75%), uncovered source lines 430 and 475 | 158/160 lines (98.75%), uncovered source lines 430 and 475 | 85% line; no changed-line regression | PASS |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | modified test | outside denominator | outside denominator | none | PASS |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | new test | outside denominator | outside denominator | none | PASS |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified build configuration | no executable lines | no executable lines | none | PASS |
| `SVGControl/app.config` | modified runtime configuration | no executable lines | no executable lines | none | PASS |
| `.github/workflows/dependabot-repair.yml` | modified workflow | no coverage tooling | no coverage tooling | actionlint + green run (Section 3.3) | PASS / PARTIAL non-blocking |

The two uncovered lines in `ConsistencyVerifier.psm1` are line 430 (`else { @() }`, the empty-manifest arm of `$package`) and line 475 (the `ResidualVersionDisagreement` failure construction), both pre-existing and unchanged. The `dependencies` package-level LINE counter reads missed 16 covered 777 in both CI documents.

Repo-wide C# by package (post-change projection): QuickFiler 10461/12754 lines, UtilitiesCS 39210/43423, TaskVisualization 1426/1569, SVGControl 877/1854, ToDoModel 1061/1823, Tags 702/758, TaskMaster 2443/3245, TaskTree 295/306, VBFunctions 4/4. SVGControl (the project whose `app.config` changed) is unchanged at 877/1854 lines and 300/638 branches between the two projections, which is consistent with a configuration-only change to that project.

## 6. Test Execution Metrics

| Run | Scope | Result | Source |
|---|---|---|---|
| P0-T12 baseline MSTest | 7346 | 7346 passed, 0 failed | `evidence/baseline/p0-t12-test-results.2026-09-28T20-01.summary.txt` |
| P2-T7 iteration 1 MSTest | 7346 | 7345 passed, 1 failed (`RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` line 189; a five-second `Task.Wait` that did not complete) | `evidence/qa-gates/p2-t7-mstest-coverage.iter1.2026-09-28T20-01.md` |
| P2-T7 iteration 2 MSTest | 7346 | 7346 passed, 0 failed | `evidence/qa-gates/p2-t7-test-results.2026-09-28T20-01.summary.txt` |
| P0-T15 baseline PoshQC test | `tests/scripts/dependencies` | 131 passed, 0 failed | `evidence/baseline/p0-t15-poshqc-test-mcp.2026-09-28T20-01.md` |
| P1-T2 fail-before | same | 135 tests, 4 failed (all four tree tests) | `evidence/regression-testing/p1-t2-tree-test-fail-before.2026-09-28T20-01.md` |
| P1-T3 after adding the two Import tests | same | 137 tests, 4 failed (tree tests; the two new verifier tests green) | `evidence/regression-testing/p1-t3-verifier-import-tests.2026-09-28T20-01.md` |
| P1-T7 intermediate | same | 137 tests, 1 failed (runbook test, before P1-T8) | `evidence/qa-gates/p1-t7-actionlint.2026-09-28T20-01.md` |
| P1-T11 pass-after and P2-T3 iter2 | same | 137 passed, 0 failed | `evidence/regression-testing/p1-t11-tree-test-pass-after.2026-09-28T20-01.md`, `evidence/qa-gates/p2-t3-pester.iter2.2026-09-28T20-01.md` |
| CI run 36666302259 (main, `231e1c0b5`) Pester job | 373 | 373 passed, 0 failed; 94.51% | P0-T16 |
| CI run 36722780748 (branch, `b96926588`) Pester job | 379 | 379 passed, 0 failed; 94.51% (373 + 6 new tests) | P2-T3 iter2 |
| CI run 36722780748 other jobs | build-analyzers, build-nullable, format-check, actionlint, hygiene | success | P2-T3 iter2 |
| CI run 36722780748 mstest-coverage | 7346 | 1 failed: `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` line 206) | P2-T3 iter2 |

Attribution of the two C# test failures: this change edits no `.cs` file. The only edit to the QuickFiler.Test project removes two `<Import>` elements guarded by `Exists()` on a package folder that a restore never creates (P0-T5 `ALTCOVER-RESTORED: False`; no manifest declares altcover, and CI carries no `restore-keys` cache fallback since issue 936), so the imports were inert and the compiled assembly is unchanged. Both tests passed in the local baseline and in local iteration 2 on the same tree; the two failures are on different tests on different hosts. Both are pre-existing timing-dependent tests outside the Write Set; the local one relies on a real five-second wall-clock wait, which the determinism rule bans in test code. The failures are assessed as not attributable to this change. The orchestrator's PR-time CI re-run remains the gate on the branch head, which CI has not yet built (Finding NB-11).

## 7. Code Quality Checks

| Check | Command / method | Result |
|---|---|---|
| Confidentiality masking scan | Grep over the feature folder and `tests/scripts/dependencies` for drive-letter paths, `Users` segments and the developer account name | Feature folder: 1 hit, plan line 209, which is the plan's own hygiene-pattern definition and not a path; tests: only the pre-existing synthetic fixture path literals (a fictitious drive letter and a `fixture` folder, no user or host segment) at lines 82-84 of `Repair-PackageManifestConsistency.Tests.ps1` (outside the diff). Executor CMD-HYGIENE: HITS=0 (P2-T11, P2-T21); CI hygiene guard local run: Findings=0, GUARD_EXIT=0 (P2-T21). PASS |
| Suppression scan (added lines) | Read of every added line in the diff | No `[SuppressMessage]`, `#pragma`, `[Diagnostics.CodeAnalysis.SuppressMessageAttribute]` or analyzer-disable comment added. PASS |
| Workflow change scan | Read of `.github/workflows/dependabot-repair.yml` in full and Grep for `app-id` under `.github/workflows` | One input rename at line 51 plus header comment lines 13-14; `app-id` absent from every workflow. Trigger, permissions, concurrency and `run:` blocks unchanged. PASS |
| actionlint | P1-T7 (local 1.7.7, exit 0 scoped and repository-wide); CI actionlint job success | PASS |
| PoshQC analyze | P2-T2 iter2 | pass (0 findings); tool reports no count. PASS |
| CSharpier check | P2-T4 iter2 | exit 0, 1625 files, no findings. PASS |
| Analyzer and nullable rebuilds | P2-T5 and P2-T6 iter2 | 0 warnings, 0 errors each. PASS |
| Hygiene of committed XML evidence | Read of both projections | root `report`, 9 `package` elements, no raw element. PASS |
| Coverage exclusion policy | Grep of the diff for `exclude` entries | No coverage configuration changed. PASS |

## 8. Gaps and Exceptions

All findings are non-blocking. Total blocking findings: 0.

| ID | Class | Finding | Disposition |
|---|---|---|---|
| NB-1 | Evidence hygiene | Executor `Timestamp:` labels are not clock readings. Each local test artifact embeds the run's UTC clock: P1-T2 label `10-02` versus RUN-START `13:24:02Z` (09:24 local at UTC-4); P1-T11 label `10-17` versus `13:29:57Z`; P2-T3 iter2 label `10-58` versus `13:46:37Z`, corroborated by the bundled PoshQC coverage document's own report name `Pester (09/30/2026 09:47:03)`. The labels lead the clock by 38 to 72 minutes and drift monotonically. The gates demonstrably ran (JUNIT-WRITTEN follows RUN-START in every artifact; the CI run ids and head SHAs are third-party facts). | Non-blocking. The labels cannot be used to establish inter-gate ordering; ordering was taken from the plan's task sequence and the embedded clocks instead. |
| NB-2 | Pre-existing flaky tests | One local iteration-1 failure and one CI failure on two different QuickFiler.Test timing tests (Section 6). Not attributable to this change. | Non-blocking. Follow-up recommended: promote the wall-clock `Task.Wait(TimeSpan.FromSeconds(5))` pattern in `QfcDatamodelLivenessTests.cs` and the dispatcher-fixture concurrency test to an issue under the determinism rule. |
| NB-3 | Workflow green-run gate | `dependabot-repair.yml` cannot produce a green run until the maintainer provisions the App credential (issue Summary item 4, not a merge gate). actionlint and the static Pester test cover the changed line; the runtime token step is unexercised. | Non-blocking, procedural. The maintainer follow-up (P2-T19) already records that `DEPENDABOT_REPAIR_APP_ID` must hold the Client ID once provisioned. |
| NB-4 | Residue outside the AC pathspec | Eight tracked `*.csproj.bak` copies exist (P0-T1 BAK-TRACKED); `QuickFiler.Test/QuickFiler.Test.csproj.bak` and `QuickFiler/QuickFiler.csproj.bak` still carry the altcover token. MSBuild does not read `.bak` files, so AC1's project-file scope is satisfied. | Non-blocking. Follow-up recommended: delete the eight tracked `.bak` copies. |
| NB-5 | Stale citation wording | Runbook line 301 still reads "Private key generation and App ID location (steps 10-12)" although step 10 now records the Client ID. The instruction text (steps 10 and 22) and the YAML sample are correct. | Non-blocking, cosmetic. |
| NB-6 | Comment wrap | `dependabot-repair.yml` line 14 runs to roughly 150 characters; the surrounding header comment wraps near 100. | Non-blocking, cosmetic. |
| NB-7 | Test granularity | `RepositoryTreeConsistency.Tests.ps1` test 2 asserts two assemblies inside one `It`. | Non-blocking, style. |
| NB-8 | Documentation conflict | CLAUDE.md 80%/90% versus rules 85%/75% coverage floors. Both are met by every figure in this audit. Tracked as issue 668. | Recorded, not re-raised. |
| NB-9 | Canonical PowerShell artifact | `artifacts/pester/powershell-coverage.xml` from the bundled PoshQC route instruments only `.claude/hooks` and reads 0 of 9294 lines; it is not a measurement of `scripts/`. The CI JaCoCo document is the measurement of record. `artifacts/csharp/coverage.xml` is absent; the committed projection and `coverage/coverage.cobertura.xml` stand in. | Non-blocking, pre-existing tool limitation (issue 928 P-4). |
| NB-10 | Out-of-scope residual | Eleven other `app.config` files redirect Fizzler to 1.3.0.0 without a Fizzler `<Reference>` to compare against (P0-T18). | Already recorded in `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`; not widened here. |
| NB-11 | CI head lag | CI run 36722780748 built `b96926588` (the implementation commit). The branch head `5ce3c8c3b` adds the evidence commits `9e41ffbcf` and `d9f50955d` and the second origin/main merge. The source diff is unchanged between the two heads (caller statement; P2-T10 footprint shows only feature-folder paths after `b96926588`). | Non-blocking, procedural. The PR-time CI run will build the head. |
| NB-12 | PR context artifacts | Not present in the review worktree and not regenerable under the tool constraint (Template Resolution Deviation item 3). Scope was triangulated from three agreeing sources. | Non-blocking, procedural. |

Exceptions claimed by the executor and accepted: `GATE-SUBSTITUTION` lines (PoshQC ok flag for the analyzer count; JUnit per-file counts for a direct Pester run; CI Pester job for a local coverage run) follow the coordinator's LOCAL POWERSHELL GATES ruling verbatim.

## 9. Summary of Changes

Source diff (`231e1c0b5` to `d9f50955d`), verified on disk:

| Path | Change | Verified by |
|---|---|---|
| `QuickFiler.Test/QuickFiler.Test.csproj` | two `altcover` `<Import>` lines deleted (570 to 568) | Grep `altcover` over `*.csproj,*.config,*.props,*.targets`: 0 matches |
| `SVGControl/app.config` | lines 15 and 19: Fizzler 1.3.0.0 to 1.3.1.0; Unsafe 6.0.2.0 to 6.0.3.0 | Read; `SVGControl.csproj` lines 58 and 82 declare 1.3.1.0 and 6.0.3.0 |
| `.github/workflows/dependabot-repair.yml` | line 51 `app-id` to `client-id`; header comment lines 13-14 | Read in full |
| `.github/workflows/README.md` | line 116 secret description | Grep |
| `docs/features/active/2026-09-19-.../runbooks/github-app-installation-token.runbook.md` | Part B heading, steps 10 and 22, YAML sample line 154 | Read lines 96-159 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | comment lines 221-223 (499 lines unchanged) | Read |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | fixture comments; two `It` blocks (lines 240-263); 337 lines | Read |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | new, 4 tests, 152 lines | Read in full |
| `docs/features/potential/promoted/2026-09-28-package-manifest-consistency-residuals.md` | promoted record (promotion commit) | present on disk |
| `docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/` | issue.md (7 check-offs), plan (56 of 56 checked), 61 evidence `.md`, 2 projections, 2 summaries | Glob (69 files) |

## 10. Compliance Verdict

**PASS.** Zero blocking findings. Twelve non-blocking findings recorded in Section 8; none is a remediation trigger. No `remediation-inputs` artifact is produced. All seven acceptance criteria evaluate PASS in `feature-audit.2026-09-30T10-30.md`; the executor's check-offs stand.

Assumptions made under the tool constraint: the branch head SHA was read from `<session-root>/.git/refs/heads/bug/package-manifest-consistency-residuals-929`; commit contents and the second merge's footprint were not re-derived with git and are taken from the caller's statement and the P2-T10 footprint.

## Appendix A: Test Inventory

New and modified tests on this branch (6):

| File | Test | Purpose |
|---|---|---|
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | `reports an Import whose package the manifest does not declare, with Kind Import` | AC4 positive: two guarded Imports of `altcover.8.6.45` against a manifest declaring only `Contoso.Widgets`; FindingCount 2, all Kind `Import` |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | `reports no Import finding when the manifest declares the imported package` | AC4 negative: agreeing project; Import-kind subset empty; ExaminedCount greater than 0 |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | `reports no Import element whose package the sibling manifest omits, for every project directory that carries a manifest` | AC1 and AC4 tree census (more than 9 project directories; examined count guards the zero) |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | `names in the SVGControl binding redirects the assembly version the SVGControl project reference declares` | AC2 and AC3 |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | `passes client-id and not app-id to the create-github-app-token step of the repair workflow` | AC5 static |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | `instructs the maintainer to store the Client ID in the secret the repair workflow reads by name` | AC6; the secret name is extracted from the workflow and asserted in the runbook |

Existing suites exercised: MSTest 7346 (unchanged); Pester CI population 379 (`tests/scripts/dependencies` 137, `tests/scripts/hygiene` 31, `tests/scripts/vscode` 211).

## Appendix B: Toolchain Commands Reference

Commands as recorded by the executor (not re-run by the reviewer):

1. `dotnet tool run csharpier check .` (P0-T9, P2-T4 iter1/iter2)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with a file logger under `coverage/` (P0-T10, P2-T5)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` with a file logger under `coverage/` (P0-T11, P2-T6)
4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` (P0-T12, P2-T7)
5. `mcp__drm-copilot__run_poshqc_format` / `run_poshqc_analyze` with `scan_folders ["scripts/dependencies","tests/scripts/dependencies"]` (P0-T13/14, P2-T1/2)
6. `mcp__drm-copilot__run_poshqc_test` with `scan_folders ["tests/scripts/dependencies"]`, counts read from `artifacts/pester/pester-junit.xml` (P0-T15, P1-T2, P1-T3, P1-T7, P1-T11, P2-T3)
7. `gh run view <run-id> --log` and `gh run download <run-id> --name pester-coverage` for coverage (P0-T16 run 36666302259; P2-T3 run 36722780748)
8. `scripts/dev-tools/run-actionlint.ps1` and a scoped `actionlint-bin/actionlint.exe` invocation (P0-T19, P1-T7)
9. `scripts/dependencies/Repair-PackageManifestConsistency.ps1 -WhatIf` (P0-T17, P1-T12)
10. `scripts/vscode/Invoke-Restore.ps1` for the cold restore (P0-T5)
11. `scripts/hygiene/Test-RepositoryHygiene.ps1` (P2-T21)

Reviewer's own verification method: Read, Grep and Glob only (caller constraint); no command executed.
