# 2026-05-26-actionable-classifier-not-serialized (Plan)

- **Issue:** #164
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-05-26T20-38
- **Status:** Complete
- **Version:** 0.2
- **Work Mode:** minor-audit

> DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED
> Requirements source: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/issue.md` (sole requirements source)

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Phase 0 — Baseline Capture

- [x] [P0-T1] Read and record plan path and requirements source (`issue.md #164`); write `phase0-instructions-read.md` to feature folder.
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/baseline/phase0-instructions-read.md`
- [x] [P0-T2] Record git baseline (branch name, HEAD commit SHA).
  - Expected branch: `bug/actionable-classifier-not-serialized-164`
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/baseline/git-baseline.txt`
- [x] [P0-T3] Run CSharpier in check mode (no files changed) and record output.
  - Command: `dotnet tool run csharpier format --check .`
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/baseline/csharpier-check.txt`
- [x] [P0-T4] Run MSBuild analyzers (baseline) and record output.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/baseline/msbuild-analyzers.txt`
- [x] [P0-T5] Run MSBuild nullable/warnings-as-errors (baseline) and record output.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/baseline/msbuild-nullable.txt`
- [x] [P0-T6] Run VSTest (baseline) and record pass/fail counts.
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/baseline/vstest-baseline.txt`

---

## Phase 1 — Implementation (Small-Scope, Already Applied)

> All production changes are already applied to branch `bug/actionable-classifier-not-serialized-164`. This phase validates scope completeness against AC.

- [x] [P1-T1] Confirm AC1 delivered: `SerializeFolderManagerAsync` in `EmailFiler.cs` calls `(await Globals.AF.Manager["Actionable"]).Serialize()`.
- [x] [P1-T2] Confirm AC2 delivered: `TrainActionableAsync` in `EmailFiler.cs` returns `Task.CompletedTask` when `mailHelper.Actionable == "None"`.
- [x] [P1-T3] Confirm AC3 delivered: `TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier` exists in `EmailFiler_Tests.cs`.

---

## Phase 2 — Final QC Loop

- [x] [P2-T1] Run CSharpier format and confirm no files are modified.
  - Command: `dotnet tool run csharpier format .`
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/qa-gates/csharpier-format.txt`
- [x] [P2-T2] Run MSBuild analyzers; confirm no new errors vs baseline.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/qa-gates/msbuild-analyzers.txt`
- [x] [P2-T3] Run MSBuild nullable/warnings-as-errors; confirm no new warnings vs baseline.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/qa-gates/msbuild-nullable.txt`
- [x] [P2-T4] Run VSTest; confirm AC3 test passes and total failures do not increase beyond the 4 known pre-existing failures.
  - Evidence: `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/evidence/qa-gates/vstest-final.txt`
- [x] [P2-T5] Update plan checklist with evidence-backed completion status; record AC check-off in `issue.md`.
