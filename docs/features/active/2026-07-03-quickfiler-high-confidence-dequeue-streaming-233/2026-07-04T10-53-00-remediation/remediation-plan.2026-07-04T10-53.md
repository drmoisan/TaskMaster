# Remediation Plan: QuickFiler High-Confidence Dequeue Streaming (#233)

- **Issue:** #233
- **Timestamp:** 2026-07-04T10-53
- **Status:** Draft for executor preflight
- **Primary Requirements Source:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-remediation/remediation-inputs.2026-07-04T10-53.md`
- **Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
- **Base Branch:** `main`
- **Merge Base:** `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`
- **Reviewed Head:** `3752331b5026cc633366739c07c689938d638c72`

## Context Package

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-remediation/remediation-inputs.2026-07-04T10-53.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/policy-audit.2026-07-04T10-53.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/code-review.2026-07-04T10-53.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/feature-audit.2026-07-04T10-53.md`
- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`

## Evidence Contract

All new evidence from executing this plan must be written under:

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/`

Each command-bearing evidence artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 0 — Remediation Baseline

- [x] [P0-T1] Read `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/atomic-executor/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-remediation/remediation-plan.2026-07-04T10-53.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-remediation/remediation-inputs.2026-07-04T10-53.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/policy-audit.2026-07-04T10-53.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/code-review.2026-07-04T10-53.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/feature-audit.2026-07-04T10-53.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/phase0-10-53-instructions-read.md` with `Timestamp:`, `Policy Order:`, and the exact files read.
- [x] [P0-T2] Capture the current branch baseline by running `git status --short --branch --untracked-files=all`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-10-53-git-status-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T3] Reconfirm the AC10 coverage blocker by reading `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md` and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/policy-audit.2026-07-04T10-53.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-10-53-ac10-baseline.md` with repository-path coverage, new-code coverage, no-regression status, and AC10 status.
- [x] [P0-T4] Capture the source-text unit-test baseline by running `Select-String -Path QuickFiler.Test\Controllers\*.cs -Pattern 'File\.ReadAllText|ReadControllerSource|AppDomain\.CurrentDomain\.BaseDirectory'`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-10-53-source-text-test-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 1 — Replace Source-Text Unit Assertions

- [x] [P1-T1] In `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, remove tests and helpers that read production source files or assert implementation strings; replace required coverage with behavior tests that exercise datamodel/gate behavior through existing seams. Preserve issue #233 behavior assertions for dequeue-time scoring, logging observability, and high-confidence gate routing.
- [x] [P1-T2] In `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`, remove the unused `ReadControllerSource` helper and related filesystem imports if no behavior test uses them.
- [x] [P1-T3] Always write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-source-search-evidence.md` using check-only search commands. If AC1 or AC11 still requires repository-wide source-search evidence after P1-T1 and P1-T2, the artifact must record the exact commands, matches, and disposition. If source-search evidence is not required, the artifact must record `SOURCE_SEARCH_EVIDENCE: NOT_REQUIRED`, the AC-specific rationale, the exact commands used to confirm no required source-search evidence remains, and `Output Summary:`. Do not implement the search as an MSTest unit test.
- [x] [P1-T4] Run `Select-String -Path QuickFiler.Test\Controllers\*.cs -Pattern 'File\.ReadAllText|ReadControllerSource|AppDomain\.CurrentDomain\.BaseDirectory'`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-10-53-source-text-test-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. The output summary must state whether source-text unit assertions remain.

### Phase 2 — Resolve AC10 Coverage Disposition

- [x] [P2-T1] Determine the AC10 route by reading `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/policy-audit.2026-07-04T10-53.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T10-53-00-audit/code-review.2026-07-04T10-53.md`, and the current Cobertura coverage data under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/`. Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-ac10-route.md` with exactly one route: `ROUTE: COVERAGE_IMPROVEMENT`, `ROUTE: APPROVED_EXCEPTION`, or `ROUTE: FAIL_CLOSED`. Use these deterministic criteria: `ROUTE: COVERAGE_IMPROVEMENT` is allowed only when the artifact records current covered lines, current total lines, the additional covered lines required for repository-path coverage to reach 80%, the exact issue #233 changed non-COM-bound production file paths with uncovered executable lines, and verifies that those in-scope uncovered lines are sufficient to reach the 80% floor without unrelated production or test work; `ROUTE: APPROVED_EXCEPTION` is allowed only when the artifact records `ApprovedExceptionArtifact: <exact existing path>` and `Test-Path -LiteralPath <exact existing path>` succeeds for an approval artifact that explicitly authorizes issue #233 AC10 repository-path coverage disposition; otherwise write `ROUTE: FAIL_CLOSED`. If no exact existing approved exception artifact exists and `ROUTE: COVERAGE_IMPROVEMENT` criteria are not fully met, the route must fail closed.
- [x] [P2-T2] If `ROUTE: COVERAGE_IMPROVEMENT`, add or update only issue #233-relevant MSTest tests using Moq and FluentAssertions, then write targeted regression evidence under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/`. If the route is not `COVERAGE_IMPROVEMENT`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-coverage-branch-not-selected.md`.
- [x] [P2-T3] Write AC10 disposition evidence for the route selected in P2-T1. If `ROUTE: COVERAGE_IMPROVEMENT`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-ac10-coverage-improvement-disposition.md` with the issue #233 test paths changed, the production paths covered, the expected AC10 threshold effect, and the requirement to prove the final value in P3-T6 before AC10 can be checked. If `ROUTE: APPROVED_EXCEPTION`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-ac10-approved-exception.md` with the exact existing approved exception artifact path from P2-T1 and verification that the path exists. If `ROUTE: FAIL_CLOSED`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-ac10-no-approved-exception.md` and leave AC10 unchecked.

### Phase 3 — Final C# QA Loop

- [x] [P3-T1] Run `dotnet tool run csharpier -- check .`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-10-53-csharpier-check.md`. If it fails or formatting changes are required, fix only in-scope files and restart Phase 3 at P3-T1.
- [x] [P3-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-10-53-msbuild-analyzers.md`. If it fails, fix only in-scope diagnostics and restart Phase 3 at P3-T1.
- [x] [P3-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-10-53-msbuild-nullable.md`. If it fails, fix only in-scope diagnostics and restart Phase 3 at P3-T1.
- [x] [P3-T4] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-10-53-vstest-results`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-10-53-vstest.md`. If it fails, fix only in-scope failures and restart Phase 3 at P3-T1.
- [x] [P3-T5] Convert the newest `.coverage` attachment from P3-T4 with `dotnet-coverage merge <coverage-file> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-10-53-vstest.cobertura.xml -f cobertura`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-10-53-coverage-conversion.md`.
- [x] [P3-T6] Compare coverage against the baseline and AC10 threshold; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-10-53-coverage-comparison.md` with repository-path coverage, changed/new-code coverage, no-regression status, and explicit PASS or FAIL for AC10.

### Phase 4 — Acceptance Tracking and Final Review

- [x] [P4-T1] Update only AC10 checkbox state in `spec.md` if Phase 3 evidence or an approved exception satisfies AC10; otherwise leave AC10 unchecked and record the reason in `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-ac10-status.md`.
- [x] [P4-T2] Apply the same AC10 decision to `user-story.md`, preserving criterion text exactly.
- [x] [P4-T3] Refresh PR context by running `mcp__drm_copilot.collect_pr_context` with base `main`; verify `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` reflect the remediated head.
- [x] [P4-T4] Produce fresh timestamped `policy-audit`, `code-review`, and `feature-audit` artifacts for the remediated head under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/`; verify that all three expected artifact paths exist before checking off this task.
- [x] [P4-T5] Validate each fresh audit artifact from P4-T4 with `mcp__drm_copilot.validate_orchestration_artifacts`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-10-53-final-audit-validation.md` with `Timestamp:`, each validator command, each artifact path, each validator result, and `Output Summary:`.
- [x] [P4-T6] Validate this remediation plan with `mcp__drm_copilot.validate_orchestration_artifacts` using `artifact_type: "plan"` and this exact file path.

## Do Not Do

- Do not modify repository policy files.
- Do not broaden production C# behavior while remediating test quality.
- Do not check off AC10 without qualifying evidence or approved exception evidence.
- Do not delete prior audit artifacts to remove failures from history.
- Do not write evidence outside the canonical feature-folder evidence paths.
- Do not treat unavailable GitHub CLI status as passing CI evidence.
