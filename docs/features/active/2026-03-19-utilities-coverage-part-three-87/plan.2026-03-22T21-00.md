# 2026-03-19-utilities-coverage-part-three - Plan

- **Issue:** #87
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-22
- **Status:** In Progress
- **Version:** 1.2

## Required References

- General Coding Standards: [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- General Unit Test Policy: [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- C# Code Change Policy: [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- C# Unit Test Policy: [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)
- Spec: [`spec.md`](spec.md)
- User Story: [`user-story.md`](user-story.md)
- Research: [`../../../../artifacts/research/20260319-utilities-coverage-part-three-87-research.md`](../../../../artifacts/research/20260319-utilities-coverage-part-three-87-research.md)

**All work must comply with these policies; do not duplicate their content here.**

## Overview

Raise every production `.cs` file compiled by `UtilitiesCS.csproj` to >= 80% line coverage by adding or extending MSTest unit tests in `UtilitiesCS.Test`, with evidence-backed skip evaluation only where repo policy and deterministic testability constraints make the 80% target unattainable. Work is phased by testability difficulty (Easy → Medium → Hard), now preceded by an explicit reconciliation gate that maps every currently sub-80 non-skip file to either a remaining implementation task or a Phase 4 skip task before further execution resumes. After the latest reconciliation pass, every remaining unchecked implementation task lists only implementation-routed files, and every Phase 4 constrained skip batch mirrors the reconciliation ledger exactly. Approximately 155 files have explicit line-rate below 80% in the Cobertura report, plus ~16 `Designer.cs` files, ~4 commented stubs, and ~40+ pure interface files with no executable code.

## Acceptance Criteria Traceability

| AC | Source (issue.md) | Plan Coverage |
|---|---|---|
| AC1 | Every .cs file compiled by UtilitiesCS.csproj >= 80% line coverage | P0-T5 through P0-T6 reconciliation + P1–P3 implementation tasks + P4-T1 through P4-T39 skip evaluation + P5-T5 verification |
| AC2 | No pre-existing tests broken or removed | P0-T3 baseline + P5-T6 verification |
| AC3 | All new tests follow MSTest + Moq + FluentAssertions conventions | P0-T1 policy read + all P1–P3 implementation tasks |
| AC4 | All new tests deterministic, isolated, no external deps | P0-T1 policy read + all P1–P3 implementation tasks |
| AC5 | All new test files registered in UtilitiesCS.Test.csproj | P1-T13, P2-T24, P3-T68 registration tasks |
| AC6 | C# toolchain loop passes clean | P5-T1 through P5-T4 |
| AC7 | Repo-wide coverage does not regress below baseline | P0-T3 baseline + P5-T5 comparison |

## Implementation Plan (Atomic Tasks)

### Phase 0 — Compliance & Baseline Capture

- [ ] [P0-T1] Read all repo policy files in required order: `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, `csharp-unit-test.instructions.md`
  - Acceptance: Evidence artifact at `evidence/baseline/phase0-instructions-read.md` contains `Timestamp:`, `Policy Order:`, and explicit list of all five files read

- [ ] [P0-T2] Capture baseline build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-build.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P0-T3] Capture baseline test results with coverage by running `vstest.console.exe` with `/EnableCodeCoverage` over all `*.Test.dll` assemblies
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-test-coverage.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total test count, pass count, and repo-wide UtilitiesCS line coverage percentage

- [ ] [P0-T4] Record per-file baseline coverage for all UtilitiesCS production files below 80% line rate from the current `coverage/coverage.cobertura.xml`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-per-file-coverage.md` lists each file with its current line-rate percentage, categorized by difficulty (Easy/Medium/Hard/Skip)

- [ ] [P0-T5] Reconcile every currently sub-80 non-skip UtilitiesCS file from `evidence/qa-gates/final-coverage-verification.md` against the remaining plan and `evidence/other/skip-candidates.md`
  - Acceptance: Evidence artifact at `evidence/baseline/remaining-sub80-reconciliation.md` contains one row for every file listed under "Non-Skip UtilitiesCS Files Below 80%" in `evidence/qa-gates/final-coverage-verification.md`, and each row maps the file to exactly one remaining task path: `Implementation Task` or `Phase 4 Skip Task`

- [ ] [P0-T6] Verify the revised checklist state matches the reconciliation matrix before additional implementation resumes
  - Preconditions: P0-T5 complete
  - Acceptance: Every file mapped to `Implementation Task` in `evidence/baseline/remaining-sub80-reconciliation.md` references an unchecked P1/P2/P3 task ID, every file mapped to `Phase 4 Skip Task` references an unchecked P4 task ID, and no checked task still depends on a file that remains below 80% in `evidence/qa-gates/final-coverage-verification.md`


