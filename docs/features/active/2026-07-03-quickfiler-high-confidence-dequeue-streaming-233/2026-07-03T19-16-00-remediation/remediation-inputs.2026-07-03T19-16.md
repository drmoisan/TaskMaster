# Remediation Inputs: QuickFiler High-Confidence Dequeue Streaming (#233)

**Timestamp:** 2026-07-03T19-16
**Issue:** #233
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Primary Review Artifacts:**
- `policy-audit.2026-07-03T19-16.md`
- `code-review.2026-07-03T19-16.md`
- `feature-audit.2026-07-03T19-16.md`

## Primary Requirements Source

This file is the authoritative remediation requirements source for the next remediation plan. Use the canonical PR context artifacts and issue #233 source files as supporting context only.

## Required Fix List

1. Remove the base-to-head whitespace failure.
   - File: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-start-state.md`
   - Location: line 34
   - Required behavior: `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD` must exit 0.
   - Verification command: `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD`

2. Bring modified test files back under the repository 500-line limit.
   - File: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`
   - Current evidence: base count 395 lines; reviewed head count 552 lines.
   - Required behavior: split issue #233 high-confidence startup tests into focused test files or helpers so each modified test file is under 500 lines.
   - Verification command: count changed `*.cs` files and confirm no production/test code file exceeds 500 lines.

3. Resolve AC10 coverage policy.
   - Files: `spec.md`, `user-story.md`, and coverage evidence under `evidence/qa-gates/`.
   - Current evidence: repository-path coverage is 22.86%; focused gate coverage is 95.00%.
   - Required behavior: AC10 must not be checked off until coverage policy is satisfied or an approved exception is recorded without weakening policy documents.
   - Verification commands:
     - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\<new-results-dir>`
     - `dotnet-coverage merge <latest .coverage> -o <new-cobertura.xml> -f cobertura`
     - coverage extraction/comparison documenting repository-path, changed-file, and new-code coverage.

4. Rerun the final C# QA loop after remediation.
   - Required order:
     1. `dotnet tool run csharpier -- check .`
     2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
     3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
     4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage ...`
   - Required behavior: do not mark remediation complete unless all four steps pass in one final pass and coverage evidence is numeric.

5. Re-run feature-review for issue #233 after remediation.
   - Required artifacts: new timestamped policy audit, code review, and feature audit under the active feature folder.
   - Required behavior: validators must pass for all review artifacts.

## Do Not Do

- Do not modify repository policy files.
- Do not weaken coverage thresholds or acceptance criteria text to force a PASS.
- Do not mark AC10 checked while repository coverage policy remains failed and no approved exception exists.
- Do not leave `QfcHomeControllerRunAsyncTests.cs` or any split replacement test file over 500 lines.
- Do not remove high-confidence behavior coverage while splitting tests.
- Do not change production behavior unless required by the coverage remediation plan and covered by tests.
- Do not create evidence outside `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/`.

## Context Package

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/issue.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T18-23-00-remediation/remediation-plan.2026-07-03T18-23.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-audit/policy-audit.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-audit/code-review.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T19-16-00-audit/feature-audit.2026-07-03T19-16.md`
