# Remediation Plan: QuickFiler High-Confidence Dequeue Streaming (#233)

**Timestamp:** 2026-07-04T11-30
**Planner:** atomic-planner handoff via feature-review workflow
**Primary Requirements Source:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-remediation/remediation-inputs.2026-07-04T11-30.md`
**Target File:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-remediation/remediation-plan.2026-07-04T11-30.md`

## Context Package

- Remediation inputs: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-remediation/remediation-inputs.2026-07-04T11-30.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Policy audit: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-audit/policy-audit.2026-07-04T11-30.md`
- Code review: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-audit/code-review.2026-07-04T11-30.md`
- Feature audit: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-audit/feature-audit.2026-07-04T11-30.md`
- Original plan: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- Acceptance sources: `spec.md`, `user-story.md`

## Evidence Contract

All new baseline, regression, QA, and remediation evidence must be written under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/`. Do not write evidence under forbidden `artifacts/` evidence paths.

### Phase 0 — Remediation Baseline

- [x] [P0-T1] Read `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-remediation/remediation-plan.2026-07-04T11-30.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-remediation/remediation-inputs.2026-07-04T11-30.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/phase0-11-30-instructions-read.md` with `Timestamp:`, `Policy Order:`, and exact files read.
- [x] [P0-T2] Capture the current AC10 baseline by reading `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-ac10-status.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-11-30-ac10-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T3] Run `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-11-30-git-diff-check-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 1 — Resolve AC10 Coverage Disposition

- [x] [P1-T1] Choose the AC10 path from current evidence: either add focused, policy-compliant C# test coverage sufficient to raise repository-wide C# coverage to 80%, or record an approved exception artifact if and only if repository policy has an accepted exception process; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-11-30-ac10-route.md` with the selected route and rationale.
- [x] [P1-T2] If the selected route is implementation, add or adjust only focused C# tests required for the coverage target, preserving issue #233 behavior and keeping changed `.cs` files under 500 lines; verify no live Outlook, temporary files, external services, or source-text assertions are introduced.
- [x] [P1-T3] If the selected route is approved exception, write the exception evidence under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/` and leave policy documents unchanged; the artifact must identify approval basis, scope, and why AC10 can be considered satisfied without changing the 80% policy.

### Phase 2 — Final C# QA Loop

- [x] [P2-T1] Run `dotnet tool run csharpier .`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-11-30-csharpier.md` with `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:`; if it fails or changes files, fix the scoped issue and restart Phase 2 from this task.
- [x] [P2-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-11-30-analyzers.md` with `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE:`, and `Output Summary:`; if it fails, fix the scoped issue and restart Phase 2 from P2-T1.
- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-11-30-nullable.md` with `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`, `EXIT_CODE:`, and `Output Summary:`; if it fails, fix the scoped issue and restart Phase 2 from P2-T1.
- [x] [P2-T4] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-11-30-vstest-results`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-11-30-vstest.md` with test counts, runtime, `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [ ] [P2-T5] Convert the VSTest coverage output to Cobertura and compare repository-wide, changed-file, and new-code thresholds; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-11-30-coverage-comparison.md` with numeric coverage values and explicit PASS/FAIL for AC10.

### Phase 3 — Acceptance Tracking and Review Refresh

- [ ] [P3-T1] Update `spec.md` and `user-story.md` AC10 checkbox only if Phase 2 coverage comparison or the approved exception route makes AC10 PASS; otherwise leave AC10 unchecked and record the reason.
- [ ] [P3-T2] Refresh PR context through the `drm-copilot` MCP `collect_pr_context` tool with base `main`.
- [ ] [P3-T3] Run the full feature-review workflow again and create `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/policy-audit.2026-07-04T12-00.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/code-review.2026-07-04T12-00.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/feature-audit.2026-07-04T12-00.md`.

### Phase 4 — Final Validation

- [ ] [P4-T1] Validate `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/policy-audit.2026-07-04T12-00.md` with `validate_orchestration_artifacts` artifact type `policy-audit`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/code-review.2026-07-04T12-00.md` with artifact type `code-review`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/feature-audit.2026-07-04T12-00.md` with artifact type `feature-audit`.
- [ ] [P4-T2] If remediation remains required, create a new timestamped grouped remediation directory under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/` with a `yyyy-MM-ddTHH-mm-ss-remediation` name and write the next remediation inputs file there; update this same remediation plan file only if it is the active plan target for the same remediation loop, otherwise stop and report the remaining blocker.

## Do Not Do

- Do not modify policy documents.
- Do not mark C# coverage as N/A.
- Do not weaken high-confidence dequeue behavior.
- Do not introduce manual validation requirements.
- Do not write evidence outside the canonical feature evidence folders.

## Handoff Status

The feature-review workflow created this target plan file before invoking the atomic-plan prompt resolution. The plan is ready for executor preflight validation, but it is not an execution result.
