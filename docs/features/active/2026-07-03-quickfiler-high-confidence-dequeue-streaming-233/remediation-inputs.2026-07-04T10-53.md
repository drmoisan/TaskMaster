# Remediation Inputs: QuickFiler High-Confidence Dequeue Streaming (#233)

**Timestamp:** 2026-07-04T10-53
**Issue:** #233
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Primary Requirements Source:** This remediation input file.
**Base Branch:** `main`
**Merge Base:** `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`
**Reviewed Head:** `3752331b5026cc633366739c07c689938d638c72`

## Context Package

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/policy-audit.2026-07-04T10-53.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/code-review.2026-07-04T10-53.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/feature-audit.2026-07-04T10-53.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`

## Trigger Summary

Remediation is required for two reasons:

1. AC10 remains failed. C# toolchain execution passes, focused new-code coverage passes, and no-regression comparison passes, but repository-path C# coverage is 22.87%, below the repository-wide 80% floor.
2. Changed unit tests include source-file reads and source-text assertions. These are brittle implementation checks and should be replaced by behavior tests or moved into non-unit audit evidence.

## Required Fix List

1. **Resolve AC10 coverage disposition.**
   - Files/evidence involved:
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md`
   - Expected behavior: AC10 is checked off only if repository-path C# coverage reaches the required 80% floor or an approved repository exception explicitly authorizes the coverage disposition.
   - Verification commands:
     - `dotnet tool run csharpier -- check .`
     - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
     - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
     - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
     - `dotnet-coverage merge <coverage-file> -o <cobertura-output> -f cobertura`

2. **Replace source-text unit assertions with behavior tests or audit evidence.**
   - Files involved:
     - `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
     - `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`
   - Expected behavior: Unit tests verify observable behavior through seams, mocks, or direct calls. Repository-wide source-search checks, if still required for AC1 or AC11, are recorded as feature-audit evidence under the feature folder rather than as MSTest unit tests.
   - Verification commands:
     - `Select-String -Path QuickFiler.Test\Controllers\*.cs -Pattern 'File\.ReadAllText|ReadControllerSource|AppDomain\.CurrentDomain\.BaseDirectory'`
     - Full C# toolchain commands listed above.

3. **Preserve issue #233 behavior while remediating tests.**
   - Files involved:
     - `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
     - `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
     - `QuickFiler/Controllers/QfcHomeController.cs`
     - `QuickFiler/Controllers/QfcHomeController.Iteration.cs`
   - Expected behavior: High-confidence mode remains enforced at dequeue time, backfills qualifying items, preserves disabled-mode behavior, and avoids post-display removal.
   - Verification commands:
     - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:QfcStreamingDequeueConfidenceGateTests,QfcDatamodelTests,QfcHomeControllerRunAsyncTests,QfcQueuePurePathsTests`
     - Full C# VSTest coverage command.

## Do Not Do

- Do not modify repository policy files.
- Do not weaken AC10, mark coverage as not applicable, or treat the repository-wide coverage floor as informational.
- Do not check off AC10 unless qualifying coverage evidence or approved exception evidence exists.
- Do not broaden production behavior beyond issue #233 while remediating tests.
- Do not delete historical audit, remediation, or evidence artifacts.
- Do not write evidence outside `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/`.
- Do not rely on GitHub CLI or CI status as passing evidence unless the tool is installed and returns current results.

## Completion Criteria

- New remediation plan validates with `mcp__drm_copilot.validate_orchestration_artifacts` as `artifact_type: "plan"`.
- Source-text unit assertions are removed or justified outside the unit-test suite.
- AC10 remains unchecked if coverage disposition is not resolved.
- Policy audit, code review, and feature audit are regenerated and validated after remediation execution.
