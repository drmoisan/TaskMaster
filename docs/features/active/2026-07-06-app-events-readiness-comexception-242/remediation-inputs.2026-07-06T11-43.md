# Remediation Inputs: app-events-readiness-comexception-242 (#242)

Timestamp: 2026-07-06T11-43
Source Review Artifacts:
- `docs/features/active/2026-07-06-app-events-readiness-comexception-242/policy-audit.2026-07-06T11-43.md`
- `docs/features/active/2026-07-06-app-events-readiness-comexception-242/code-review.2026-07-06T11-43.md`
- `docs/features/active/2026-07-06-app-events-readiness-comexception-242/feature-audit.2026-07-06T11-43.md`
Canonical PR Context:
- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
Original Feature Plan:
- `docs/features/active/2026-07-06-app-events-readiness-comexception-242/plan.2026-07-06T10-42.md`
Requirements Source:
- `docs/features/active/2026-07-06-app-events-readiness-comexception-242/issue.md`

## Remediation Triggers

1. `git diff --check origin/main..HEAD` failed because committed issue #242 evidence files contain trailing whitespace.
2. Recorded C# repo-wide line coverage is 13.64%, below the feature-review workflow's explicit 80% coverage floor.
3. A non-approved full VSTest invocation without `/EnableCodeCoverage` failed due missing `System.Threading.Tasks.Extensions, Version=4.2.0.1`, while the repository-approved `/EnableCodeCoverage` command passed.

## Enumerated Fix List

1. Remove trailing whitespace from the exact evidence files reported by `git diff --check origin/main..HEAD`.
   - Files:
     - `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/baseline/baseline-analyzer-build.2026-07-06T10-44.md`
     - `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/baseline/baseline-nullable-build.2026-07-06T10-44.md`
     - `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/baseline/baseline-restore.2026-07-06T10-44.md`
     - `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/regression-testing/fail-before-test-build.2026-07-06T10-50.md`
   - Expected behavior: `git diff --check origin/main..HEAD` exits 0.
   - Verification command: `git diff --check origin/main..HEAD`.

2. Re-run the C# verification sequence after whitespace remediation.
   - Expected behavior: CSharpier check, analyzer build, nullable build, and approved VSTest coverage command pass.
   - Verification commands:
     - `dotnet tool run csharpier check .`
     - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
     - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
     - `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`

3. Resolve the repo-wide C# coverage floor before declaring PR readiness.
   - Expected behavior: Either repo-wide C# line coverage is at least 80%, or an approved policy exception is recorded outside this review without weakening repository policy.
   - Verification command: inspect updated coverage comparison artifact and rerun the policy-audit validator.
   - Current evidence: `final-coverage-comparison.2026-07-06T10-44.md` reports 13.64% repo-wide line coverage and 100.00% changed-code coverage.

4. Document the VSTest invocation dependency behavior if it remains after remediation.
   - Expected behavior: The executor records whether the approved `/EnableCodeCoverage` invocation is the required supported test entry point, or repairs the dependency layout so the same test assembly also passes without the switch.
   - Verification commands:
     - Approved: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`
     - Diagnostic only: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`

## Do Not Do

- Do not modify repository policy documents.
- Do not weaken coverage requirements or mark the policy audit PASS while repo-wide coverage remains below the enforced floor without an approved exception.
- Do not make unrelated implementation changes.
- Do not change the canonical issue number; all artifacts must continue to use issue #242.
- Do not replace the canonical PR-context artifacts with ad hoc reconstruction.
- Do not silently skip failed verification commands.

## Context Package

The remediation planner and executor must use these files as the authoritative context package:
- Remediation inputs: `docs/features/active/2026-07-06-app-events-readiness-comexception-242/remediation-inputs.2026-07-06T11-43.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Policy audit: `docs/features/active/2026-07-06-app-events-readiness-comexception-242/policy-audit.2026-07-06T11-43.md`
- Code review: `docs/features/active/2026-07-06-app-events-readiness-comexception-242/code-review.2026-07-06T11-43.md`
- Feature audit: `docs/features/active/2026-07-06-app-events-readiness-comexception-242/feature-audit.2026-07-06T11-43.md`
- Original feature plan: `docs/features/active/2026-07-06-app-events-readiness-comexception-242/plan.2026-07-06T10-42.md`
- Requirements source: `docs/features/active/2026-07-06-app-events-readiness-comexception-242/issue.md`
