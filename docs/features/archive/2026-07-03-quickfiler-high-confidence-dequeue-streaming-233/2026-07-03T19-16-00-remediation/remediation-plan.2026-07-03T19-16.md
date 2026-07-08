# Remediation Plan: QuickFiler High-Confidence Dequeue Streaming (#233)

- **Issue:** #233
- **Owner:** drmoisan
- **Last Updated:** 2026-07-03T19-16
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** full-feature remediation
- **Primary Requirements Source:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-remediation/remediation-inputs.2026-07-03T19-16.md`
- **Target File:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-remediation/remediation-plan.2026-07-03T19-16.md`

## Context Package

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/issue.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T18-23-00-remediation/remediation-plan.2026-07-03T18-23.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-remediation/remediation-inputs.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-audit/policy-audit.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-audit/code-review.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-audit/feature-audit.2026-07-03T19-16.md`

## Evidence Contract

All new evidence must be written under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/`.

Command-bearing evidence artifacts must include:

- `Timestamp:`
- `Command:`
- `EXIT_CODE:`
- `Output Summary:`

## Implementation Plan

### Phase 0 — Compliance and Baseline

- [x] [P0-T1] Read required policies and remediation inputs
  - Read `AGENTS.md`, `.agents/skills/csharp/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-remediation/remediation-inputs.2026-07-03T19-16.md`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/phase0-r4-remediation-instructions-read.md`.
- [x] [P0-T2] Capture git status baseline
  - Run `git status --short --branch --untracked-files=all`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-git-status-baseline.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T3] Capture base-to-head whitespace baseline
  - Run `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-git-diff-check-baseline.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T4] Capture changed C# file-size baseline
  - Count lines for changed `*.cs` files in `ec4af1f0924b175a725fe50a5d2a61f7d27a3318..HEAD`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-file-size-baseline.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T5] Capture numeric coverage baseline from current QA artifacts
  - Read fully qualified paths `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-remediation-rerun.md` and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison-remediation-final.md`.
  - Write the numeric baseline coverage summary to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-coverage-baseline.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T6] Capture CSharpier check baseline
  - Run `dotnet tool run csharpier -- check .`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-csharpier-check.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T7] Capture analyzer build baseline
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-msbuild-analyzers.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T8] Capture nullable build baseline
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-msbuild-nullable.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T9] Capture MSTest coverage baseline
  - Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\r4-baseline-vstest-results`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-vstest.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric baseline coverage headline values when available from the run output.
- [x] [P0-T10] Convert and compare baseline coverage
  - Resolve the `.coverage` file produced by P0-T9.
  - Run `dotnet-coverage merge <resolved P0-T9 .coverage file> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\r4-baseline-vstest.cobertura.xml -f cobertura`.
  - Record the exact resolved command and numeric baseline coverage, post-change coverage, and new-code coverage fields.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-coverage-comparison.md`.
  - The artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 1 — Repair Evidence Whitespace

- [x] [P1-T1] Remove trailing whitespace from remediation-start evidence
  - Edit only `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-start-state.md`.
  - Remove the trailing spaces on the line reported by `git diff --check`.
- [x] [P1-T2] Verify base-to-head whitespace check
  - Run `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-git-diff-check.md`.

### Phase 2 — Split Oversized Test File

- [x] [P2-T1] Identify issue #233 high-confidence tests to move
  - Inspect `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`.
  - Identify issue #233 high-confidence startup tests and any helpers that can move without changing test behavior.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/r4-test-file-split-map.md`.
- [x] [P2-T2] Move issue #233 high-confidence startup tests into a focused test file
  - Create or update a focused MSTest class under `QuickFiler.Test/Controllers/`.
  - Keep each resulting production/test code file under 500 lines.
  - Preserve MSTest, Moq, and FluentAssertions conventions.
- [x] [P2-T3] Run targeted tests for moved coverage
  - Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:<moved-test-method-names>` using the moved test method names recorded in the P2-T1 split map.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/r4-split-tests.pass.md`.
  - The evidence artifact must record the exact resolved command.
- [x] [P2-T4] Verify changed C# file sizes
  - Count lines for changed `*.cs` files.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-file-size-check.md`.

### Phase 3 — Resolve AC10 Coverage Status

- [x] [P3-T1] Re-run QuickFiler MSTest with coverage
  - Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-vstest-results`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-vstest.md`.
- [x] [P3-T2] Convert and extract numeric coverage
  - Run `dotnet-coverage merge <latest .coverage> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-vstest.cobertura.xml -f cobertura`.
  - Extract repository-path coverage, changed-file coverage, and new-code coverage.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-coverage-comparison.md`.
- [x] [P3-T3] Record provisional AC10 coverage status
  - If coverage policy appears satisfied, write provisional status to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/r4-ac10-provisional-status.md`.
  - Do not check off AC10 in `spec.md` or `user-story.md` in Phase 3; AC10 requires the final C# QA loop to pass first.

### Phase 4 — Final C# QA Loop

Execute P4-T1 through P4-T6 in order. If any command fails or modifies files, perform only the required corrective edits and restart at P4-T1. Any formatting correction requires restarting the loop at P4-T1, and the final pass must use `dotnet tool run csharpier -- check .`. Do not proceed to Phase 5 until P4-T1 through P4-T6 pass in one uninterrupted final pass. Each command-bearing task must write a separate artifact containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P4-T1] Run final CSharpier check
  - Run `dotnet tool run csharpier -- check .`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-csharpier-format.md`.
- [x] [P4-T2] Run final analyzer build
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-msbuild-analyzers.md`.
- [x] [P4-T3] Run final nullable build
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-msbuild-nullable.md`.
- [x] [P4-T4] Run final MSTest coverage
  - Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-final-vstest-results`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-final-vstest.md`.
- [x] [P4-T5] Resolve, merge, and verify final numeric coverage
  - Resolve the `.coverage` file produced by P4-T4.
  - Run `dotnet-coverage merge <resolved P4-T4 .coverage file> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-final-vstest.cobertura.xml -f cobertura`.
  - Record the exact resolved `dotnet-coverage merge ... -o ... -f cobertura` command.
  - Record numeric repository-path coverage, changed-file coverage, new-code coverage, baseline comparison, and threshold results.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-final-coverage-comparison.md`.
- [x] [P4-T6] Reconcile AC10 after final QA evidence
  - If P4-T1 through P4-T5 passed in one final pass and final coverage policy passes, check off AC10 in both `spec.md` and `user-story.md`.
  - If any final QA or coverage policy requirement does not pass, leave AC10 unchecked and write the remaining blocker to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/r4-ac10-blocker.md`.

### Phase 5 — Re-review Readiness

- [x] [P5-T1] Refresh PR context for issue #233
  - Run `mcp__drm_copilot.collect_pr_context` with `base: main`.
  - Confirm `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` reference issue #233 and the current head.
- [x] [P5-T2] Produce post-remediation policy audit
  - Produce a new timestamped policy audit under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/`.
  - The audit must reference issue #233 and the current remediation evidence.
- [x] [P5-T3] Validate post-remediation policy audit
  - Run `mcp__drm_copilot.validate_orchestration_artifacts` with `artifact_type: "policy-audit"` and the P5-T2 policy-audit path.
  - Record the validation result in the policy-audit artifact or adjacent review evidence required by the repository workflow.
- [x] [P5-T4] Produce post-remediation code review
  - Produce a new timestamped code review under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/`.
  - The review must reference issue #233 and the current remediation evidence.
- [x] [P5-T5] Validate post-remediation code review
  - Run `mcp__drm_copilot.validate_orchestration_artifacts` with `artifact_type: "code-review"` and the P5-T4 code-review path.
  - Record the validation result in the code-review artifact or adjacent review evidence required by the repository workflow.
- [x] [P5-T6] Produce post-remediation feature audit
  - Produce a new timestamped feature audit under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/`.
  - The audit must reference issue #233 and the current remediation evidence.
- [x] [P5-T7] Validate post-remediation feature audit
  - Run `mcp__drm_copilot.validate_orchestration_artifacts` with `artifact_type: "feature-audit"` and the P5-T6 feature-audit path.
  - Record the validation result in the feature-audit artifact or adjacent review evidence required by the repository workflow.

## Automated Validation Summary

- Whitespace: Phase 1.
- File-size policy: Phase 2.
- AC10 provisional coverage status: Phase 3.
- C# QA loop and final AC10 reconciliation: Phase 4.
- PR-context refresh and post-remediation review validation: Phase 5.

## Preflight Status

`PREFLIGHT: NOT RUN`

The R4 review environment exposed the MCP validator and atomic-plan prompt resolver but did not expose a separate atomic-executor delegation surface. This plan must receive atomic-executor preflight validation before execution.
