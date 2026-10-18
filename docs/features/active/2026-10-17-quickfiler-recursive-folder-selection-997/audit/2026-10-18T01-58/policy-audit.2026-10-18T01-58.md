# Policy Compliance Audit: QuickFiler recursive folder selection (#997)

Audit Date: 2026-10-18T01-58 UTC
Template: MCP policy_audit.template, resolved for this review; instruction block removed.
Source HEAD: 996dab2f5fbeaad7abda2cc0f8903f65803afa47.
Base and merge base: origin/main at 09a4522eccece6aeb0294bb2e4f9888cce24dd08; merge-base timestamp 2026-10-17T19:20:49-05:00.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 25 source/test files | 7504 | PASS, 7504 passed, zero failed | 86.03% lines, 80.21% branches | 86.10% lines, 80.31% branches | 98.70% new-file lines, 99.09% changed lines |
| JavaScript | 1 production script and 4 test modules | 51 baseline, 78 final | PASS, zero failed | 100.00% lines, 99.29% branches | 100.00% lines, 100.00% branches | 100.00% new/changed lines and branches |

## Executive Summary

Local implementation and policy verification: PASS. Prior R1 (JavaScript coverage) and R2 (Markdown whitespace) are resolved by real execution measurements and clean full-branch whitespace. The prior R4 policy-schema rejection is not carried forward as accepted history: this new audit must validate independently with numeric measurements.

REVIEW_STATUS: AWAITING_CI
The sole open blocking delivery finding is R3, classified awaiting_ci: AC8 requires all checks at final PR head. No autonomous blocking finding remains. No new remediation plan or planner delegation is appropriate for this wait-only result.

Policy Order: supplied AGENTS.md standing/general code/general test rules, .agents/skills/csharp/SKILL.md, C# source policies, current CLAUDE.md task/coverage corrections, CI policies, and the canonical .claude/skills/feature-review-workflow/SKILL.md with its shared skills. Current approved TaskMaster tasks govern actual C# commands. No policy was edited.

Primary evidence artifacts/pr_context.summary.txt was read first and matches current committed HEAD, generated 2026-10-18 01:57:56 UTC. Secondary artifacts/pr_context.appendix.txt anchors the complete base-to-head diff. Feature selected by changed scoping docs and branch suffix 997. full-bug in issue.md makes spec.md the only AC source. Review covers all changed implementation languages, all resource/project registrations, test modules and review/remediation documents.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Independence/isolation/determinism | PASS | Fresh Moq/model state, TaskCompletionSource barriers; JavaScript fresh VM per fixture and normal/reversed/shuffled order, seed 997 |
| Positive/negative/edge/error/state/concurrency scenarios | PASS | Selection, validation, ordering, strict messages, provider retry, recursive DOM/event/focus/ARIA fixtures |
| No test temporary files/processes/services | PASS | Browser harness is in memory; source file is loaded once by outer adapter; runner collectors are outside test cases |
| Readability and actionable assertions | PASS | MSTest/FluentAssertions/Moq; node:test and node:assert/strict; named observable outcomes |
| All changed-language coverage | PASS | Numeric baseline/final/new/changed comparisons below; no source region excluded |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.03% lines, 80.21% branches (56931/66178 lines; 13792/17195 branches); Post-change: 86.10% lines, 80.31% branches (57271/66515 lines; 13923/17337 branches); Change: +0.07 percentage points lines, +0.10 points branches; Disposition: PASS; New/changed-code coverage: 98.70% new-file lines (379/384), 99.09% changed lines (547/552) versus 97.57% baseline (201/206); all 45 measured new methods >=90%; Evidence: evidence/baseline/csharp-tests-coverage.2026-10-18T01-01.md, evidence/qa-gates/reviewer-local-qa.2026-10-18T01-42.md, evidence/qa-gates/coverage-comparison.2026-10-18T01-27.md.
- JavaScript: Baseline: 100.00% lines, 99.29% branches (314/314 source lines; 139/140 branches; 31/31 functions); Post-change: 100.00% lines, 100.00% branches (347/347 source lines; 162/162 branches; 35/35 functions); Change: +0.00 points lines, +0.71 points branches; Disposition: PASS; New/changed-code coverage: 100.00% lines (43/43), 100.00% branches (25/25), compared with baseline changed 10/10 lines and 4/4 branches; Evidence: evidence/baseline/issue-997-javascript-execution-coverage.2026-10-18T01-56.md, evidence/qa-gates/issue-997-javascript-execution-coverage.2026-10-18T01-53.md, evidence/qa-gates/issue-997-javascript-coverage-comparison.2026-10-18T01-54.md, evidence/qa-gates/issue-997-javascript-raw-reconciliation.2026-10-18T01-53.md.

### Coverage Evidence Checklist

- C# baseline coverage artifact: evidence/baseline/csharp-tests-coverage.2026-10-18T01-01.md.
- C# post-change coverage artifact: evidence/qa-gates/reviewer-local-qa.2026-10-18T01-42.md.
- JavaScript baseline coverage artifact: evidence/baseline/issue-997-javascript-execution-coverage.2026-10-18T01-56.md.
- JavaScript post-change coverage artifact: evidence/qa-gates/issue-997-javascript-execution-coverage.2026-10-18T01-53.md.
- TypeScript baseline coverage artifact: N/A - zero changed TypeScript files.
- TypeScript post-change coverage artifact: N/A - zero changed TypeScript files.
- PowerShell baseline coverage artifact: N/A - zero changed PowerShell files.
- PowerShell post-change coverage artifact: N/A - zero changed PowerShell files.
- Per-language comparison summary: Section 1.2.1; C# PASS; JavaScript PASS.

Existing C# COM/VSTO/generated exemptions remain unchanged. JavaScript measurement executes unchanged full production bytes, retaining every source region and native Node line semantics, including structural/comment lines. LCOV DA counts and branch totals reconcile to raw V8. Extraction-aware mapping pairs 304 unchanged moved lines; they are excluded only from the new/changed delta denominator, never from whole-script coverage. Four new functions have 100% coverage; their union is 13/13 lines and 7/7 branches. Raw collector documents remain in SCRATCH/ignored output, not committed.

## 2. General Code Change Policy Compliance

PASS for objective/plan, regression-first evidence, focused host-owned tree/identity, compatibility, documentation and complete local verification. Original four regressions plus legacy root-boundary regression have deterministic fail-before/pass-after records. All 35 original feature tasks and 17 delegated remediation tasks are complete.

Production C#/script/document/resources/project bytes are unchanged since the independently reviewed source head. The remediation adds only four test modules and evidence/document reconciliation. No C# rerun was needed in this re-review because those measured production inputs are identical; the prior full independent ordered run is retained. No package, exclusion, policy, workflow or EFC production change exists. Full git diff --check origin/main...HEAD now exits 0.

## 3. Language-Specific Code Change Policy Compliance

C#: PASS; manifest CSharpier, full analyzer Rebuild and warnings-as-errors Rebuild produced zero warnings/errors. Optional root binding preserves unbound callers; provider requests await outside lock and apply only to current row/node/population/generation.

JavaScript: PASS; native execution coverage and Node syntax pass. No configured JavaScript formatter/linter/static type checker exists; style review, parser checks and runtime boundary assertions are reported accurately, without invented tool results. No TypeScript implementation changed.

HTML/resource/project XML are declarative assets validated by complete compiled document composition and exact project registration. Runtime script is measured separately. The only changed deployed JavaScript source is FolderBreadcrumb.js; test modules are outside its production denominator. An unchanged SpamBayesTokenizer.js reference file has no project/source runtime reference found and is not a deployed part of this feature.

Quality tiers: N/A; base has no adopted root quality-tiers.yml and CI has no tier-classification gate. Modified-workflow-needs-green-run: N/A; no workflow/action/benchmark changes.

## 4. Language-Specific Unit Test Policy Compliance

C#: PASS; MSTest, Moq and FluentAssertions, preserved existing compatibility assertions. JavaScript: PASS; node:test, strict assertions, fresh in-memory DOM/WebView contexts, actual unchanged script execution, event bubbling and independent selection/toggle verification. Source-loader file I/O occurs in the outer input adapter, not test bodies. Runner-generated collection artifacts are separate from test actions.

## 5. Test Coverage Detail

C# measured new files: coordinator partial 30/30, document composition 4/4, render serializer partial 109/110, descendant model 45/45, selection-session partial 51/51 and router partial 140/144. Complete per-file and 45-method inventory is in coverage-comparison.2026-10-18T01-27.md. Original executor total 57268/66515 and independent reviewer total 57271/66515 are both retained; three existing covered lines vary, with unchanged denominator and passing thresholds.

JavaScript complete source: 347/347 lines, 162/162 branches, 35/35 functions. New/changed behavior: 43/43 lines, 25/25 branches. The script is executed unchanged through vm.runInNewContext with its actual filename and native source coverage; no synthetic LCOV counts or production transforms are used.

## 6. Test Execution Metrics

C#: baseline 7471 passed; final 7504 passed, zero failures, 48.0895 seconds in independent full run. Original focused pass-after 25 UtilitiesCS + 7 QuickFiler; compatibility 332 + 513. JavaScript baseline 51 passed; final 78 passed, zero failures. Re-review replay of both committed modules: 78 passed, zero failed/skipped/cancelled, 51.5486 ms. Normal, reversed and seed-997 shuffled complete fixture orders pass. Every changed source/test/script remains <=474 physical lines; new Node modules are 13,128,239,148 lines.

## 7. Code Quality Checks

| Check | Result | Evidence |
| --- | --- | --- |
| CSharpier / analyzers / compiler | PASS | Current task commands; original final ordered pass and independent reviewer-local-qa |
| C# full tests/coverage | PASS | 7504 passed; 86.10% lines, 80.31% branches; no changed-line regression |
| JavaScript syntax/style | PASS | Separate parser records and issue-997-javascript-style.2026-10-18T01-53.md |
| JavaScript tests/coverage | PASS | Complete source native coverage, raw reconciliation and independent 78-fixture replay |
| File-size/registrations/hygiene | PASS | Existing exact registrations, <=500-line checks, hygiene zero findings |
| Complete branch whitespace | PASS | git diff --check origin/main...HEAD, exit 0 in re-review |
| Final PR-head CI | PARTIAL, awaiting_ci | No PR/current-head CI in fresh context |

## 8. Gaps and Exceptions

R3 is the sole pending delivery gate: final PR-head required CI. Remediability: awaiting_ci. No autonomous finding remains, so no new plan/delegation target is created. Current wait inputs name that gate.

Live WebView2/Outlook was not used; deterministic host and browser-seam behavior is verified, with no unsupported live integration claim. No new policy exception is requested.

## 9. Summary of Changes

Full branch: 125 files, including 25 C# sources/tests, one deployed JavaScript script and four Node test modules, plus project/resources and feature evidence. Host owns lazy descendants, validated filing values and async request validity. QFC renders independent recursive controls and retains committed descendant selection across collapse/Escape. Resource extraction composes one complete script for both viewer hosts. Remediation adds measured execution tests and evidence whitespace repair, preserving production bytes. Canonical appendix contains the exact file/hunk inventory.

## 10. Compliance Verdict

Local policy verification: PASS.
C# coverage verdict: PASS. JavaScript coverage verdict: PASS.
Overall delivery: AWAITING_CI; R3 only, Remediability: awaiting_ci.
No new remediation plan. AC7 can be checked after this independent acceptance; AC8 remains unchecked until exact final PR-head checks succeed. No merge-ready claim is made.

Early source CI run 39087748686 was independently queried: success on 08d02c07f8a981cb34aa626951931fd78af28fd7, all seven required jobs successful. That earlier run is not final PR-head CI for current HEAD 996dab2f5fbeaad7abda2cc0f8903f65803afa47.

## Appendix A: Test Inventory

C# issue-997 suites: descendant selection, validation, ordering, messages, activation and recursive asset composition. Existing session/router/duplicate/score/EFC assertions retain recorded dispositions.
JavaScript issue-997-common.test.mjs: startup, invalid messages/options, render/view order, row identity/cells/percentages/theme/focus, selectors/arrows, direct child and missing WebView cases.
JavaScript issue-997-recursive.test.mjs: three-level rendering, leaf/nonleaf toggles, click/Enter/Space propagation, exact node activation, durable committed child display, invalid optional state and active-descendant ownership.

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -TreatWarningsAsErrors
. ./scripts/vscode/Invoke-MSTestWithCoverage.ps1
Invoke-MSTestWithCoverageMain -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-997-review-20261018013723.cobertura.xml -ResultsDirectory SCRATCH/issue-997-test-results -ScriptRoot ./scripts/vscode
node --check QuickFiler/Resources/FolderBreadcrumb.js
node --test --test-isolation=none --test-concurrency=1 tests/quickfiler-breadcrumb/issue-997-common.test.mjs tests/quickfiler-breadcrumb/issue-997-recursive.test.mjs
git diff --check origin/main...HEAD
gh run view 39087748686 --json headSha,conclusion,jobs
```

Recorded native JavaScript coverage command additionally uses --experimental-test-coverage, exact actual-source --test-coverage-include, spec/LCOV reporters and --test-coverage-lines=85 --test-coverage-branches=75. Baseline selects original inline bytes via the outer source adapter. Exact sanitized commands are in the baseline/final execution artifacts. Raw files remain under SCRATCH.
