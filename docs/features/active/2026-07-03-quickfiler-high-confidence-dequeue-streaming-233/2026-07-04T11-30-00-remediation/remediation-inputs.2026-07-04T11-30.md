# Remediation Inputs: QuickFiler High-Confidence Dequeue Streaming (#233)

**Timestamp:** 2026-07-04T11-30
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Primary Requirements Source:** This remediation input file.
**Review Artifacts:**
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-audit/policy-audit.2026-07-04T11-30.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-audit/code-review.2026-07-04T11-30.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T11-30-00-audit/feature-audit.2026-07-04T11-30.md`

## Context Package

- Canonical PR context summary: `artifacts/pr_context.summary.txt`
- Canonical PR context appendix: `artifacts/pr_context.appendix.txt`
- Original feature plan: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- Acceptance criteria sources:
  - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
  - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`

## Trigger Summary

Remediation is required because AC10 remains failed. Current evidence records:

- CSharpier check: PASS.
- Analyzer build: PASS.
- Nullable build: PASS.
- VSTest with coverage: PASS execution, 387/387 tests passed.
- New/changed gate coverage: PASS, `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` at 57/60 = 95.00%.
- Repository-path C# coverage: FAIL, 13120/57379 = 22.87% against the required 80% floor.
- Approved exception: none found.

## Required Fix List

1. Resolve AC10 coverage disposition.
   - Files likely involved: coverage evidence and, if implementation is chosen, focused C# test files under `QuickFiler.Test/`.
   - Expected behavior: AC10 may be checked only when repository-wide C# coverage reaches the 80% floor or an approved exception is recorded through the accepted repository process.
   - Verification commands:
     - `dotnet tool run csharpier -- check .`
     - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
     - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
     - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:<feature>\evidence\qa-gates\<run-results>`
     - Coverage conversion and comparison against repository-wide 80%, changed-file no-regression, and new-code 90% thresholds.

2. Refresh acceptance criteria status after AC10 resolution.
   - Files: `spec.md`, `user-story.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/feature-audit.2026-07-04T12-00.md`.
   - Expected behavior: AC10 remains unchecked while coverage is failed; if AC10 passes, check it off in both authoritative source files.
   - Verification commands:
     - Read `issue.md` work mode marker.
     - Verify AC10 status in both authoritative source files.

3. Refresh PR context and re-run review after remediation.
   - Files: `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/policy-audit.2026-07-04T12-00.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/code-review.2026-07-04T12-00.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/feature-audit.2026-07-04T12-00.md`.
   - Expected behavior: the review uses the current branch head and records the full feature-vs-base scope.
   - Verification command:
     - `collect_pr_context` through the `drm-copilot` MCP tool with base `main`.

The previous flat-location wording for the next `feature-audit.<timestamp>.md` is superseded. The next review outputs must use the grouped audit directory `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-04T12-00-00-audit/`.

## Do Not Do

- Do not modify policy documents.
- Do not mark coverage as not applicable for C#.
- Do not narrow the review to plan scope.
- Do not check off AC10 while repository-wide coverage remains below policy without an approved exception.
- Do not weaken or remove issue #233 high-confidence dequeue behavior.
- Do not move live confidence filtering back into UI post-display removal or admission-time filtering.
- Do not add broad, unrelated refactors.

## Completion Criteria

- AC10 is PASS or an approved exception is recorded through accepted repository evidence.
- All C# QA commands pass in order.
- Coverage evidence includes numeric repository-wide, changed-file, and new-code values.
- `spec.md` and `user-story.md` reflect the verified AC10 state.
- New policy-audit, code-review, and feature-audit artifacts pass MCP validation.
