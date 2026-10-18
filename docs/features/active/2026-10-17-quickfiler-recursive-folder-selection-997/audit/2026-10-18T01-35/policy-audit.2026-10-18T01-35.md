# Policy Compliance Audit: QuickFiler recursive folder selection (#997)

Audit Date: 2026-10-18T01-35 UTC
Scope: feature branch against origin/main; source HEAD 08d02c07f8a981cb34aa626951931fd78af28fd7.
Template: MCP policy_audit.template; usage instructions removed.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 25 source/test files | 7504 repository cases | PASS, 7504 passed, 0 failed | 86.03% lines, 80.21% branches | 86.10% lines, 80.31% branches | 98.70% measured new-file lines |
| JavaScript | 1 extracted/extended script | No execution suite | FAIL R1 | Missing | Missing | Missing |

## Executive Summary

Overall verdict: FAIL. R1 is an autonomous blocking coverage gap: QuickFiler/Resources/FolderBreadcrumb.js is changed executable JavaScript but has no numeric JavaScript baseline, final, changed-line, or new-code coverage evidence. Syntax and C# resource-text tests do not measure script execution. R2 is a nonblocking Markdown whitespace finding. R3 is the outstanding final-head CI gate (awaiting_ci).

Policy Order: supplied AGENTS.md standing instructions, general code policy, general unit-test policy, .agents/skills/csharp/SKILL.md, applicable C# source instructions, repository CLAUDE.md coverage/task corrections, and CI instructions. The explicit current tasks and approved spec select manifest CSharpier, full analyzer Rebuild, warnings-as-errors Rebuild without global nullable opt-in, and the existing MSTest coverage wrapper. No policy documents were modified.

Primary evidence: artifacts/pr_context.summary.txt generated 2026-10-18 01:34:37 UTC at the reviewed HEAD. Secondary evidence: artifacts/pr_context.appendix.txt. Both were fresh at review entry. Subsequent review-only artifacts are not implementation changes and will require collection at the next committed head.

Base: origin/main at 09a4522eccece6aeb0294bb2e4f9888cce24dd08. Merge base is the same SHA, timestamp 2026-10-17T19:20:49-05:00. Supplied base is authoritative; competing-candidate selection is unnecessary. Active feature was selected by changed issue/spec/plan docs and branch suffix 997. Work Mode: full-bug; spec.md is the only AC source.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Independence, isolation, determinism | PASS | Fresh Moq providers/models, TaskCompletionSource barriers; csharp-isolation.2026-10-18T01-14.md records fresh reverse-order fixture processes. No new test filesystem/process/service calls or sleeps found. |
| Positive/negative/edge/error/state/concurrency cases | PASS | Selection, validation, message, ordering, activation and recursive-asset suites cover all named original repros and invalid requests. |
| Readability and diagnostics | PASS | Named MSTest cases, FluentAssertions, focused fixtures, explicit provider-call assertions. |
| C# coverage | PASS | Numeric comparison below; existing COM/VSTO exemptions and denominator retained. |
| JavaScript coverage | FAIL | R1: executable script has no execution coverage artifact. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.03% lines, 80.21% branches (56931/66178 lines; 13792/17195 branches); Post-change: 86.10% lines, 80.31% branches (57268/66515 lines; 13923/17337 branches); Change: +0.07 percentage points lines, +0.10 percentage points branches; Disposition: PASS; New/changed-code coverage: 98.70% new-file lines (379/384), 99.09% changed lines (547/552) versus 97.57% baseline (201/206); all 45 measured new methods >=90%; Evidence: evidence/baseline/csharp-tests-coverage.2026-10-18T01-01.md, evidence/qa-gates/csharp-tests-coverage.2026-10-18T01-27.md, evidence/qa-gates/coverage-comparison.2026-10-18T01-27.md.
- JavaScript: Baseline: missing; Post-change: missing; Change: unknown because measurements are absent; Disposition: FAIL; New/changed-code coverage: missing; Evidence: evidence/qa-gates/coverage-comparison.2026-10-18T01-27.md:15 and self-review.2026-10-18T01-32.md:10. The original implementation is inline in base FolderBreadcrumb.html; baseline must measure that script rather than invent zero coverage for the extracted file.
- HTML, resource XML and project XML are declarative assets, with C# composition/registration checks; they do not create a separate executable-language denominator.

### Coverage Evidence Checklist

- C# baseline: evidence/baseline/csharp-tests-coverage.2026-10-18T01-01.md (feature-relative evidence/baseline path).
- C# final: evidence/qa-gates/csharp-tests-coverage.2026-10-18T01-27.md.
- C# comparison: evidence/qa-gates/coverage-comparison.2026-10-18T01-27.md.
- Per-language comparison summary: Section 1.2.1; C# PASS, JavaScript FAIL.
- JavaScript baseline/final/comparison: missing; R1.
- TypeScript baseline coverage artifact: N/A - zero changed TypeScript files.
- TypeScript post-change coverage artifact: N/A - zero changed TypeScript files.
- PowerShell baseline coverage artifact: N/A - zero changed PowerShell files.
- PowerShell post-change coverage artifact: N/A - zero changed PowerShell files.
- Raw collector/TRX files remain ignored or in SCRATCH; only sanitized numeric evidence is published.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Objective, plan, fail-first bug workflow | PASS | Original plan has 35 completed tasks; five fail-before artifacts record assertion failures, not build/setup failures; original pass-after and legacy-boundary pass-after exist. |
| Separation, API compatibility, error handling | PASS | Host-owned tree/identities; optional root binding; provider await outside lock; explicit error response/retry behavior; no new dependency. |
| File size and registration | PASS | Changed source/test/script inventory is <=474 physical lines; necessary legacy project/resource entries registered once. |
| Full ordered C# toolchain | PASS | Existing clean final pass plus reviewer format/analyzer/nullable checks and repository test rerun, recorded in reviewer-local-qa evidence. |
| Documentation, delivered behavior and next gates | PARTIAL | Scope/usage are documented; AC7 lacks JavaScript coverage and AC8 awaits final-head CI. |
| Whitespace | PARTIAL, nonblocking | R2: full git diff --check origin/main...HEAD returns 2 for Markdown evidence whitespace. Earlier worktree-only command did not check the committed branch diff. |

## 3. Language-Specific Code Change Policy Compliance

C#: PASS. Manifest-pinned CSharpier check passed (1664 files). Analyzer and warnings-as-errors Rebuild each passed with zero warnings/errors. Existing nullable opt-in and public compatibility overloads are preserved. Existing source-level ExcludeFromCodeCoverage on COM/VSTO wiring is unchanged; no new exclusion or package exists.

JavaScript: FAIL for coverage (R1); syntax PASS via node --check QuickFiler/Resources/FolderBreadcrumb.js. Recursive controls use separate stopped click/key events, host-known row/node identity, textContent, and the same composed resource document for both hosts. Resource text/regex tests cannot establish execution coverage.

Python, TypeScript and PowerShell implementation policies: N/A, no changed implementation files. Quality tiers: N/A; base has no root quality-tiers.yml and branch CI does not run tier classification. Modified-workflow-needs-green-run: N/A; no .github/workflows, .github/actions or scripts/benchmarks changes.

## 4. Language-Specific Unit Test Policy Compliance

C#: PASS. MSTest, Moq and FluentAssertions are used. New fixtures reside in the existing test projects and are explicitly compiled. Tests assert destination/event/close state, stale completions and strict tokens. Existing EFC suites are compatibility coverage.

JavaScript: FAIL (R1). No executable JavaScript suite or instrumented baseline/final report was supplied. Do not treat C# string assertions or Node syntax checking as a JavaScript test-coverage pass.

## 5. Test Coverage Detail

| Component | Final covered/valid lines | Status |
| --- | --- | --- |
| BreadcrumbBridgeCoordinator.Descendants.cs | 30/30 | PASS |
| FolderBreadcrumbDocument.cs | 4/4 | PASS |
| BreadcrumbBridgeMessages.Render.cs | 109/110 | PASS |
| BreadcrumbDescendantNode.cs | 45/45 | PASS |
| BreadcrumbSelectionSession.Descendants.cs | 51/51 | PASS |
| FolderBreadcrumbBridgeRouter.Descendants.cs | 140/144 | PASS |
| FolderBreadcrumb.js | no JavaScript coverage report | FAIL |

Full measured modified-file/new-method table: evidence/qa-gates/coverage-comparison.2026-10-18T01-27.md. Baseline and final detailed Cobertura snapshots are retained in ignored coverage/ and SCRATCH. Existing COM/VSTO/generated exclusions are disclosed; no added exclusion is accepted.

## 6. Test Execution Metrics

Baseline: 7471 passed, zero failures. Executor final: 7504 passed, zero failures, 55.6659 seconds. Original issue-997 pass-after: 25 UtilitiesCS + 7 QuickFiler tests. Compatibility: 332 UtilitiesCS + 513 QuickFiler tests. Reviewer full run: 7504 passed, zero failures, 48.0895 seconds; coverage postprocessing result is recorded separately in reviewer-local-qa evidence. No numerical JavaScript test/coverage count is available.

## 7. Code Quality Checks

| Check | Status | Evidence |
| --- | --- | --- |
| CSharpier | PASS | dotnet tool run csharpier check . |
| Analyzer Rebuild | PASS | Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild; zero warnings/errors |
| Warnings-as-errors Rebuild | PASS | Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors; zero warnings/errors |
| Full MSTest coverage | PASS | Existing final evidence and reviewer-local-qa record |
| JavaScript syntax | PASS | node --check QuickFiler/Resources/FolderBreadcrumb.js |
| JavaScript numeric coverage | FAIL | R1 |
| Full branch whitespace | PARTIAL, nonblocking | reviewer-diff-whitespace evidence, exit 2 |
| Final PR-head CI | UNVERIFIED | R3; no PR/run available in canonical summary |

## 8. Gaps and Exceptions

R1 (Blocking, autonomous): add deterministic execution tests and numeric JavaScript baseline/final/changed-code coverage for the real embedded script. Apply >=90% new-code line coverage, >=85% changed-language aggregate line coverage, >=75% branch coverage where measured, and no changed-line regression. Preserve no-package/no-policy-weakening constraints.

R2 (Minor, autonomous, nonblocking): normalize trailing whitespace/final blank lines in changed Markdown evidence and run the full branch whitespace comparison.

R3 (Blocking delivery gate, awaiting_ci): all required CI checks must pass at final PR head; AC8 stays unchecked. This alone would route AWAITING_CI without a plan; R1 independently requires remediation planning.

No new policy exception is requested or inferred. No production/test code was fixed during review.

## 9. Summary of Changes

Commit: 08d02c07f fix(quickfiler): enable descendant selection and recursive expansion.
87 branch files changed, including 32 non-documentation files. Production areas: UtilitiesCS folder state/session/map/projection/messages/router and cohesive partials; QuickFiler coordinator/viewer/root binding; extracted compiled script; project/resource registrations. Tests add six focused issue-997 suites and update the existing asset-contract suite. Full file inventory is anchored in artifacts/pr_context.appendix.txt.

## 10. Compliance Verdict

Overall Status: NON-COMPLIANT (R1). REVIEW_STATUS: REMEDIATION_REQUIRED.
C# coverage verdict: PASS. JavaScript coverage verdict: FAIL.
Recommendation: No-go for PR readiness until R1 is remediated and reviewed, then final-head CI satisfies AC8. R2 should be corrected in the same evidence reconciliation.

## Appendix A: Test Inventory

- BreadcrumbDescendantSelectionIssue997Tests: bound root, collapse readback, Escape, intermediate/leaf selection and committed render.
- BreadcrumbDescendantValidationIssue997Tests: unknown/unloaded/cross-row, invalid roots, duplicate names/stores, legacy indexed validation, laziness and provider retry.
- FolderBreadcrumbDescendantOrderingIssue997Tests: collapse/replacement/clear and reversed fetch completion.
- BreadcrumbDescendantMessagesIssue997Tests: strict token/constructor validation and recursive round trip.
- BreadcrumbDescendantActivationIssue997Tests: one notification/close/focus, independent toggle and invalid-message no-ops.
- FolderBreadcrumbRecursiveAssetIssue997Tests: recursive markup contract and complete single compiled-script composition.
- Existing asset/host/session/router/EFC suites: disposition inventory in evidence/qa-gates/asserting-suite-review.2026-10-18T01-30.md.

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -TreatWarningsAsErrors
. ./scripts/vscode/Invoke-MSTestWithCoverage.ps1
Invoke-MSTestWithCoverageMain -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-997-review-20261018013723.cobertura.xml -ResultsDirectory SCRATCH/issue-997-test-results -ScriptRoot ./scripts/vscode
node --check QuickFiler/Resources/FolderBreadcrumb.js
git diff --check origin/main...HEAD
```

SCRATCH is the session-external issue-997 review directory; the wrapper requires its results argument as a workspace-relative path. Initial unexecuted redirection/absolute-results argument attempts were corrected before the successful runs; they are not test failures. No raw machine paths are published.
