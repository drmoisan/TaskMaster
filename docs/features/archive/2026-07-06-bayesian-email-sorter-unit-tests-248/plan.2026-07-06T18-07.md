# bayesian-email-sorter-unit-tests - Minor-Audit Plan

- **Issue:** #248
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/248
- **Requirements Source:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/issue.md`
- **Plan Path:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/plan.2026-07-06T18-07.md`
- **Feature Folder:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248`
- **Work Mode:** minor-audit
- **Language:** C#
- **Last Updated:** 2026-07-06T18-07
- **Status:** Draft for preflight validation

## Requirements Boundary

This minor-audit plan uses only `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/issue.md` as the requirements source. Acceptance criteria are limited to checkbox items under that file's explicit `## Acceptance Criteria` section.

Implementation is constrained to unit tests for `QuickFiler.Controllers.EmailSorter` and `QuickFiler.Controllers.BayesianPerformanceController`, plus only minimal production testability seams if required. Expected touched files are limited to:

- `QuickFiler/Controllers/EmailSorter.cs`
- `QuickFiler/Controllers/BayesianPerformanceController.cs`
- `QuickFiler.Test/Controllers/EmailSorterTests.cs`
- `QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs`
- `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/issue.md`

All evidence must be written under `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/<kind>/`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Policy and Baseline Evidence

- [x] [P0-T1] Record policy-read evidence for issue #248 before implementation begins.
  - Files: `AGENTS.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/issue.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: Evidence contains `Timestamp:`, `Policy Order:`, and the explicit list of files read.

- [x] [P0-T2] Verify the minor-audit requirements boundary for issue #248.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/issue.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/spec.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/user-story.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/minor-audit-scope.2026-07-06T18-07.md`
  - Acceptance: Evidence confirms `issue.md` contains `- Work Mode: minor-audit`, contains an explicit `## Acceptance Criteria` section, treats only that section as the AC source, and confirms `spec.md` and `user-story.md` are absent.

- [x] [P0-T3] Run the baseline C# formatting command.
  - Files: `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/csharpier-baseline.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:` with the formatter result and whether files changed.

- [x] [P0-T4] Run the baseline C# analyzer build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/csharp-analyzers-baseline.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T5] Run the baseline C# nullable build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/csharp-nullable-baseline.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T6] Run the baseline MSTest coverage command.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with total tests, pass/fail counts, and numeric coverage headline values.

### Phase 1 — Constrained Implementation Handoff

- [x] [P1-T1] Delegate constrained C# implementation to the small-path implementation engineer for issue #248.
  - Files: `QuickFiler.Test/Controllers/EmailSorterTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`
  - Acceptance: The implementation handoff references issue #248, the feature folder, the requirements source, the C# policy skill, and the constraint that production changes are limited to minimal testability seams only if required.

- [x] [P1-T2] Implement deterministic `EmailSorter` unit tests for issue #248.
  - Files: `QuickFiler.Test/Controllers/EmailSorterTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler/Controllers/EmailSorter.cs`
  - Acceptance: Tests cover default construction, options construction, `GetDateKey` formatting, supported triage sort keys, and `KeyNotFoundException` propagation for unsupported triage values; production changes are absent unless a minimal testability seam is required in `QuickFiler/Controllers/EmailSorter.cs`.

- [x] [P1-T3] Implement deterministic `BayesianPerformanceController` unit tests for issue #248.
  - Files: `QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler/Controllers/BayesianPerformanceController.cs`
  - Acceptance: Tests cover direct form value assignment and selection-change behavior without Outlook or external services; production changes are absent unless a minimal internal testability seam is required in `QuickFiler/Controllers/BayesianPerformanceController.cs`.

- [x] [P1-T4] Record constrained implementation scope evidence for issue #248.
  - Files: `QuickFiler.Test/Controllers/EmailSorterTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/regression-testing/implementation-scope.2026-07-06T18-07.md`
  - Acceptance: Evidence lists each changed file, states whether a production seam was introduced, and confirms no production files outside `QuickFiler/Controllers/EmailSorter.cs` and `QuickFiler/Controllers/BayesianPerformanceController.cs` were changed for testability.

- [x] [P1-T5] Run targeted issue #248 unit tests with coverage.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`, `QuickFiler.Test/Controllers/EmailSorterTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~EmailSorterTests|FullyQualifiedName~BayesianPerformanceControllerTests"`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/regression-testing/targeted-vstest-coverage.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with targeted test counts, pass/fail counts, and numeric coverage headline values.

### Phase 2 — Final C# QA Loop

- [x] [P2-T1] Run the final C# formatting command.
  - Files: `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`, `QuickFiler.Test/Controllers/EmailSorterTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs`, `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-final.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:`; if this command changes files, restart Phase 2 from P2-T1 after preserving the evidence.

- [x] [P2-T2] Run the final C# analyzer build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-analyzers-final.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final analyzer result; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T3] Run the final C# nullable build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/EmailSorter.cs`, `QuickFiler/Controllers/BayesianPerformanceController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-nullable-final.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final nullable result; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T4] Run the final MSTest coverage command.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T18-07.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with total tests, pass/fail counts, and numeric coverage headline values; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T5] Record C# coverage comparison evidence for issue #248.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-06T18-07.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/regression-testing/targeted-vstest-coverage.2026-07-06T18-07.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T18-07.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-coverage-comparison.2026-07-06T18-07.md`
  - Acceptance: Evidence records baseline coverage, targeted-test coverage, post-change coverage, changed-code coverage for issue #248, and whether repository-wide coverage remains at least 80% with no changed-line coverage regression.

- [x] [P2-T6] Update issue #248 acceptance-criteria status after verified completion.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/issue.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/issue-updates/ac-status.2026-07-06T18-07.md`
  - Acceptance: Only verified acceptance criteria under `## Acceptance Criteria` in `issue.md` are changed from `[ ]` to `[x]`, unchanged text is preserved, and evidence records total AC items, checked items, remaining items, and the verification evidence used for each checked item.

- [x] [P2-T7] Record final minor-audit readiness evidence for issue #248.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/plan.2026-07-06T18-07.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/issue.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/phase0-instructions-read.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/regression-testing/implementation-scope.2026-07-06T18-07.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-coverage-comparison.2026-07-06T18-07.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/minor-audit-readiness.2026-07-06T18-07.md`
  - Acceptance: Evidence confirms Phase 0 artifacts exist, Phase 1 scope evidence exists, Phase 2 C# QA artifacts exist, every command-bearing task has an executed numeric `EXIT_CODE`, and remaining audit disposition is ready for the reduced minor-audit review.
