# Remediation Inputs: QuickFiler High-Confidence Dequeue Streaming (#233)

**Timestamp:** 2026-07-03T22-18
**Issue:** #233
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Base Branch:** `main`
**Merge Base:** `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`
**Head SHA:** `787bb46198df1a29189077cd450943c23fbb4a1a`
**Primary Requirements Source:** this remediation-inputs artifact

## Remediation Trigger

Remediation is required by the feature-review workflow because:

1. Policy audit FAIL: current `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD` exits 1 on trailing whitespace in issue #233 markdown artifacts.
2. Code review Blocker: the same whitespace failure blocks branch readiness.
3. Feature audit FAIL: AC10 remains failed because repository-path C# coverage is 22.87%, below the 80% floor.

## Enumerated Fix List

### 1. Remove trailing whitespace from issue #233 markdown artifacts

**Affected paths from live review command output:**

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/code-review.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/code-review.2026-07-03T22-10.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-git-diff-check-baseline.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/feature-audit.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/feature-audit.2026-07-03T22-10.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/policy-audit.2026-07-03T19-16.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/policy-audit.2026-07-03T22-10.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T19-16.md`

**Expected behavior:** Base-to-head whitespace validation exits 0.

**Verification command:**

```powershell
git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD
```

### 2. Resolve AC10 coverage policy disposition

**Affected requirement sources:**

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`

**Current evidence:**

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-final-coverage-comparison.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/r4-ac10-blocker.md`

**Expected behavior:** AC10 remains unchecked until the repository-wide coverage policy is satisfied or an approved exception is recorded without weakening policy documents.

**Verification commands:**

```powershell
dotnet tool run csharpier -- check .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-22-18-vstest-results
dotnet-coverage merge <coverage-file> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-22-18-vstest.cobertura.xml -f cobertura
```

### 3. Refresh review readiness evidence after remediation

**Affected paths:**

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- new timestamped policy/code/feature review artifacts under the issue #233 active feature folder

**Expected behavior:** PR context reflects the remediated head, and follow-up review artifacts validate.

**Verification commands/tools:**

```text
mcp__drm_copilot.collect_pr_context with base main
mcp__drm_copilot.validate_orchestration_artifacts for policy-audit, code-review, feature-audit, and plan artifacts
```

## Do Not Do

- Do not modify policy documents to lower coverage or whitespace requirements.
- Do not check off AC10 unless repository-wide coverage satisfies the policy floor or an approved exception is recorded.
- Do not change production C# behavior while correcting markdown whitespace unless a separate reviewed remediation plan authorizes it.
- Do not delete prior review artifacts to hide evidence; preserve audit history and correct formatting in place.
- Do not introduce new non-canonical evidence folders under `artifacts/`.
- Do not leave PR/CI status as assumed if GitHub status can be collected after remediation.

## Required Context Package for Planning

- Remediation inputs: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T22-18.md`
- Policy audit: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/policy-audit.2026-07-03T22-18.md`
- Code review: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/code-review.2026-07-03T22-18.md`
- Feature audit: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/feature-audit.2026-07-03T22-18.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Original feature plan: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- Requirements: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
