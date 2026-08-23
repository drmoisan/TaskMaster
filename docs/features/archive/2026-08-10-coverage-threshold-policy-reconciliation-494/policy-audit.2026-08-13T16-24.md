# Policy Compliance Audit: Issue #494 coverage threshold gate

**Audit Date:** 2026-08-13
**Code Under Test:** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---|---|---|
| PowerShell | 4 | 51 | PASS: 51 passed, 0 failed | 90.16% command coverage | 90.16% command coverage | 92.86% (13/14 evaluator lines); runner wiring 1/1 |

## Executive Summary

The reviewed implementation adds an 80% Cobertura line-rate gate after coverage XML post-processing. It rejects missing, non-numeric, out-of-range, and below-threshold values, and its targeted Pester suite passes. The branch is partially compliant because the issue, spec, and plan prohibit every TaskMaster `.claude/**` change while the reviewed range contains six permitted repository-specific `.claude/agent-memory/**` changes. This stale documentation makes the checked-off acceptance criteria internally inaccurate.

Policy documents evaluated: `AGENTS.md`; `.agents/skills/powershell/SKILL.md`; `issue.md`; and `spec.md`.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - out of scope.
- TypeScript post-change coverage artifact: N/A - out of scope.
- PowerShell baseline coverage artifact: `evidence/baseline/powershell-coverage.2026-08-13T15-46.md`.
- PowerShell post-change coverage artifact: `evidence/qa-gates/powershell-test-coverage.2026-08-13T16-08.md`.
- Per-language comparison summary: `## 5. Test Coverage Detail`.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, determinism | PASS | Inline Cobertura XML strings; no filesystem, process, network, or temporary-file dependency. |
| Positive, negative, edge, and error scenarios | PASS | Tests cover missing, non-numeric, 79.9999%, exact 80%, above 80%, and runner wiring. |
| Coverage | PASS | `evidence/qa-gates/powershell-test-coverage.2026-08-13T16-08.md`: 51/51 passed and 92.86% changed evaluator-line coverage. |
| Readability and diagnostics | PASS | Pester test names state each input condition and expected disposition. |

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 90.16% command coverage -> Post-change: 90.16% command coverage. Change: +0.00% command coverage. New/changed-code coverage: 92.86% evaluator lines and 100.00% runner wiring. Disposition: PASS. Evidence: `evidence/qa-gates/powershell-test-coverage.2026-08-13T16-08.md`.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Planned, scoped implementation | PARTIAL | `issue.md`, `spec.md`, and `plan.2026-08-10T14-10.md` say no `.claude/**` changes, but commit `69493db` contains six repository-specific agent-memory changes permitted by the upstream-prompt boundary. The documents require correction. |
| Focused design and explicit errors | PASS | `Assert-CoberturaLineCoverageThreshold` is a focused helper with explicit failures before artifact write. |
| File cohesion and size | PASS | The two production scripts and two test files remain below 500 lines. |
| Whitespace | PASS | `git diff --check epic/build-ci-coverage-gate-fidelity-integration...HEAD` exited 0. |

## 3. Language-Specific Code Change Policy Compliance

### PowerShell

| Requirement | Status | Evidence |
|---|---|---|
| Advanced function and parameter contract | PASS | `CmdletBinding`, mandatory XML input, invariant numeric parsing, explicit range guard. |
| Error handling | PASS | Missing, malformed, out-of-range, and below-floor values stop the runner before `Set-Content`. |
| Analyzer hygiene | PARTIAL | Targeted analyzer has one pre-existing `PSUseSingularNouns` warning at `Invoke-MSTestWithCoverage.Helpers.ps1:141`; the full analyzer has its documented zero-delta baseline of 225 diagnostics. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Pester framework and isolation | PASS | Targeted MCP Pester check passed for the two modified test files. |
| No temporary files | PASS | Targeted tests use mocks or in-memory XML. |
| Coverage of changed behavior | PASS | Evaluator input states and runner-to-evaluator wiring are tested. |

## 5. Test Coverage Detail

- Baseline evidence: `evidence/baseline/powershell-coverage.2026-08-13T15-46.md`.
- Post-change evidence: `evidence/qa-gates/powershell-test-coverage.2026-08-13T16-08.md`.
- Post-change coverage: 90.16% command coverage; 90.83% Helpers and 89.80% runner JaCoCo line coverage.
- Changed behavior: 13/14 evaluator lines and 1/1 runner wiring line are covered.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---|---|
| Targeted Pester tests | 51 passed, 0 failed, 0 skipped | PASS |
| Fail-before proof | 45 passed, 6 expected failures before implementation | PASS |
| Targeted MCP test rerun | Passed | PASS |
| Full analyzer | 225 diagnostics, zero delta, non-zero exit | PARTIAL |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| Diff whitespace | `git diff --check epic/build-ci-coverage-gate-fidelity-integration...HEAD` | Exit 0 | PASS |
| Targeted analyzer | `mcp__drm-copilot__run_poshqc_analyze` on the two production scripts | Exit 1; one pre-existing warning at Helpers.ps1:141 | PARTIAL |
| Targeted Pester | `mcp__drm-copilot__run_poshqc_test` on the two test files | Passed | PASS |

## 8. Gaps and Exceptions

- **Documentation-scope defect:** the corrected issue, spec, and plan prohibit all `.claude/**` changes but the permitted upstream-prompt boundary allows repository-specific `.claude/agent-memory/**` changes. This causes false scope violations in AC1 and AC6.
- **Pre-existing analyzer debt:** `PSUseSingularNouns` at line 141 predates this change; it is not the remediation trigger.

## 9. Summary of Changes

Commits reviewed: `69493db`, `a6256778`, `018e586b`, and `8f36c21e`. The permitted implementation adds `Assert-CoberturaLineCoverageThreshold`, invokes it after Cobertura processing, and adds deterministic Pester cases plus wiring verification.

## 10. Compliance Verdict

### Overall Status: PARTIALLY COMPLIANT

The coverage evaluator and tests have adequate passing evidence. PR readiness requires remediation of the stale scope/acceptance wording so that the reviewed permitted memory paths are not described as prohibited `.claude/**` changes.

## Appendix A: Test Inventory

- `Assert-CoberturaLineCoverageThreshold`: missing, non-numeric, below 80%, exact 80%, and above 80% inputs.
- `Invoke-MSTestWithCoverageMain`: generated Cobertura output is evaluated before write.

## Appendix B: Toolchain Commands Reference

```powershell
# Targeted analysis
mcp__drm-copilot__run_poshqc_analyze

# Targeted Pester tests
mcp__drm-copilot__run_poshqc_test

# Diff whitespace
git diff --check epic/build-ci-coverage-gate-fidelity-integration...HEAD
```

**Audit Completed By:** Codex feature reviewer
**Policy Version:** Current as of 2026-08-13
