---
name: Remediation Plan: 2026-03-19-utilities-coverage-part-three-87 (2026-03-27T08-20)
status: Planned
work-mode: full-feature
source-spec: docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-inputs.2026-03-27T08-20.md
source-user-story: docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md
last-updated: 2026-03-27T08-20
note: Atomic-planner delegation surface was not available in this tool environment; this file captures an equivalent remediation plan directly so the required artifact exists.
---

# Remediation Plan — utilities-coverage-part-three-87

## Objective

Remediate the review blockers identified in `remediation-inputs.2026-03-27T08-20.md` so the branch becomes isolated to issue `#87` and the refreshed Cobertura report shows `UtilitiesCS >= 80%` line coverage.

## Constraints

- Follow the repository C# toolchain in this exact order: format, analyzer build, nullable build, coverage-enabled MSTest.
- Do not weaken policy requirements.
- Do not introduce scope beyond issue `#87` remediation.
- Do not check off the main coverage acceptance criterion until verification passes.

## Atomic phases

### Phase 1 — Isolate the branch diff
- **Completion criteria:** `git diff --name-status development...HEAD` contains only issue `#87` scope plus current review artifacts.
- [P1-T1] Re-run `git diff --shortstat development...HEAD` and store the updated scope evidence.

### Phase 2 — Close remaining coverage gaps
- **Completion criteria:** The next Cobertura report shows the `UtilitiesCS` package and remaining non-skip production files at or above `80%`.
- [P2-T1] Identify the remaining below-threshold production files from `coverage/coverage.cobertura.xml`.
- [P2-T2] Add or extend MSTest coverage for the remaining Dialogs files (`InputBox`, `MyBox`, `NotImplementedDialog`, `YesNoToAll`).
- [P2-T3] Add or extend MSTest coverage for the remaining EmailIntelligence files led by `SortEmail`, `PeopleScoDictionaryNew`, and `ManagerAsyncLazy`.
- [P2-T4] Add or extend MSTest coverage for remaining helper/extension files led by `AsyncSerialization` and `TipsController`.
- [P2-T5] Confirm new or updated tests are registered in `UtilitiesCS.Test.csproj` when new files are introduced.

### Phase 3 — Re-run the full QA loop
- **Completion criteria:** Format, analyzer build, nullable build, and coverage-enabled MSTest all pass in a single final pass.
- [P3-T1] Run `dotnet tool run csharpier format .`.
- [P3-T2] Run the analyzer build with `-EnableNETAnalyzers -EnforceCodeStyleInBuild`.
- [P3-T3] Run the nullable/type-safety build with `-EnableNullable -TreatWarningsAsErrors`.
- [P3-T4] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`.
- [P3-T5] Record the refreshed coverage headline and verify `UtilitiesCS >= 80%`.

### Phase 4 — Align docs and acceptance tracking
- **Completion criteria:** Feature docs reflect actual completion state and checkboxes are updated only for verified PASS items.
- [P4-T1] Update `v2/spec.md`, `v2/user-story.md`, and `v2/plan.2026-03-22T21-00.md` to reflect the verified post-remediation state.
- [P4-T2] Check off the main coverage acceptance criterion only if the refreshed coverage evidence passes.
- [P4-T3] Refresh review artifacts if the branch is re-reviewed after remediation.

## Validation target

- `PREFLIGHT STATUS: MANUAL-EQUIVALENT ONLY`
- The dedicated `atomic_planner` / `atomic_executor` delegation workflow described by repo prompt files was not available through the exposed tool surface in this session.
- This plan should therefore be treated as the manual-equivalent remediation artifact for follow-on execution.
