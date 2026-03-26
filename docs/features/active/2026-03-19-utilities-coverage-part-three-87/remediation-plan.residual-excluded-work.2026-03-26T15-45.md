---
title: "Remediation Plan: residual-excluded-work (2026-03-26T15-45)"
issue: "#87"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-26T15-45"
status: "Planned"
status_color: "blue"
version: "1.0"
work_mode: "full-feature"
requirements_source: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md"
secondary_context: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/user-story.md"
base_ref: "origin/development"
---

# Remediation Plan: residual-excluded-work (2026-03-26T15-45)

## Overview

**Status Badge:** [Planned | blue]

This remediation file defines a worktree-safe recovery pass for the residual excluded non-`#87`, non-`#96`, non-`#97` work currently mixed into `feature/utilities-coverage-part-three-87`. The pass keeps the authoritative plan file and evidence folders on the current workspace checkout, creates a sibling Git worktree for the clean residual branch, replays the verified residual commits plus the selected bootstrap files from mixed commits, runs the applicable QA gates for C# and GitHub Actions inside that sibling worktree, pushes the recovered branch, opens the dedicated PR, and records the handoff to the final clean issue `#87` pass.

## Scope Guardrails

- **CON-1:** Treat `spec.md` and `user-story.md` as the authoritative full-feature requirements for this remediation pass; use `remediation-inputs.2026-03-26T09-40.md`, `artifacts/research/20260326-issue87-unstacking-sequence-research.md`, and `.git/branch_analysis_issue87.txt` as supporting context only.
- **CON-2:** Use `origin/development` as the only comparison base for the clean residual branch.
- **CON-3:** Keep the main workspace checkout on `feature/utilities-coverage-part-three-87` for the entire pass so this plan file and the feature evidence folders remain available.
- **CON-4:** Run all residual branch operations in the sibling worktree path `c:\Users\DanMoisan\repos\TaskMaster-residual-clean`; do not `git switch` the main workspace to the clean branch.
- **CON-5:** Limit commit replay to the verified residual commit set `52742b8`, `4d5f476`, `60408b0`, `16d7d5d`, `0c9a045`, `66220df`, and `ea0206e` plus the selected bootstrap files from `ee92dd6`, `a8d24b2`, and `4634ac5`.
- **CON-6:** Exclude all issue `#87`, issue `#96`, and issue `#97` feature-folder and `UtilitiesCS/**` scope from this pass.
- **CON-7:** Treat `.github/workflows/codex-web-setup-test.yml` as CI-critical and require `scripts/dev-tools/run-actionlint.ps1` to pass in both baseline and final QA evidence.
- **CON-8:** Do not mark any acceptance or completion item satisfied without evidence on disk containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` when a command is involved.

## Requirements Traceability

| REQ | Source | Required outcome | Implementation tasks | Validation tasks |
|---|---|---|---|---|
| REQ-1 | remediation-inputs §1 + unstacking research | Capture the current mixed-branch baseline state and the applicable C# / GitHub Actions QA baseline before residual branch recovery begins | P0-T1 through P0-T9 | P0-T9 |
| REQ-2 | unstacking research | Recover the residual excluded work onto a dedicated clean branch from `origin/development` inside a sibling worktree using only the verified residual commits and bootstrap files | P1-T1 through P1-T12 | P1-T12 |
| REQ-3 | general-code-change + csharp-code-change + github-actions policy | Run the full applicable QA loop on the clean residual branch before PR creation and record baseline-to-final coverage results for the touched C# scope | P2-T1 through P2-T6 | P2-T6 |
| REQ-4 | remediation follow-on sequencing | Push the clean residual branch, create the residual PR after QA passes, and record the handoff to the final clean issue `#87` pass | P3-T1 through P3-T3 | P3-T3 |

## Acceptance Criteria

- **REQ-1:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/` contains baseline artifacts for the current mixed-branch diff, the residual commit map, formatter, actionlint, analyzer build, nullable build, and coverage-enabled MSTest, and the baseline coverage artifacts record numeric values for the touched residual C# scope in `Output Summary:`.
- **REQ-2:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-focused-diff.md` records that the clean residual branch diff inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` is limited to `.codex/**`, `.github/**`, `QuickFiler/**`, `QuickFiler.Test/**`, `TaskMaster/**`, `UtilitiesSwordfish/**`, and `missing-serializable-list.json`, with no `UtilitiesCS/**`, issue `#96`, or issue `#97` feature-folder paths.
- **REQ-3:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-qc-test-coverage.md` records numeric post-change coverage values for the touched residual C# scope in `Output Summary:`, `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-coverage-delta.md` records baseline coverage, final coverage, and changed-code coverage for the touched production files, and the formatter, actionlint, analyzer, nullable, and test artifacts all record `EXIT_CODE: 0` from one clean final pass.
- **REQ-4:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-pr.md` records the clean residual PR URL after QA passes, and `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-next-pass-handoff.md` records the next-pass order `clean #87`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Capture residual-pass baseline state

Completion criteria: baseline artifacts capture the authoritative remediation inputs, the residual commit map, and one baseline QA snapshot for the touched C# and workflow scope.

If P0-T4 changes files, rerun P0-T2 through P0-T4 and keep only the final baseline artifacts produced after the formatter reports no additional file changes.

- [ ] [P0-T1] Read `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, `.github/instructions/github-actions.instructions.md`, and `.github/instructions/github-actions-ci-cd-best-practices.instructions.md`, then read `issue.md`, `spec.md`, `user-story.md`, `remediation-inputs.2026-03-26T09-40.md`, `artifacts/research/20260326-issue87-unstacking-sequence-research.md`, and `.git/branch_analysis_issue87.txt`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-phase0-instructions-read.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Policy Order:`, and a `Files Read:` list naming every file in this task.

- [ ] [P0-T2] Capture the current mixed-branch diff scope by running `git diff --name-status $(git merge-base HEAD origin/development) HEAD`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-current-diff-scope.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD`, `EXIT_CODE: 0`, and `Output Summary:` naming residual `.codex`, `.github`, `QuickFiler`, `TaskMaster`, and `UtilitiesSwordfish` scope.

- [ ] [P0-T3] Copy the residual commit map from `.git/branch_analysis_issue87.txt` into `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-commit-map.md`.
  - Acceptance: Artifact contains the exact residual direct-cherry-pick commits `52742b8`, `4d5f476`, `60408b0`, `16d7d5d`, `0c9a045`, `66220df`, `ea0206e` and the bootstrap file sources `ee92dd6`, `a8d24b2`, and `4634ac5`.

- [ ] [P0-T4] Run `dotnet tool run csharpier format .` from the repository root and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-format.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` describing whether the formatter changed files and whether P0-T2 through P0-T4 were rerun.

- [ ] [P0-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/dev-tools/run-actionlint.ps1` from the repository root and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-actionlint.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/dev-tools/run-actionlint.ps1`, `EXIT_CODE: 0`, and `Output Summary:` confirming actionlint passed for the current workflow files.

- [ ] [P0-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [ ] [P0-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [ ] [P0-T8] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` from the repository root and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-test-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`, `EXIT_CODE: 0`, and `Output Summary:` including numeric coverage values for the touched residual C# scope extracted from the full repository coverage run.

- [ ] [P0-T9] Capture the baseline-coverage headline by copying the current residual touched-scope coverage values from `residual-baseline-test-coverage.md` into `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-current-coverage-headline.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Source Artifact: residual-baseline-test-coverage.md`, `Touched Scope: QuickFiler + UtilitiesSwordfish + TaskMaster/AppGlobals`, `Baseline Scope Coverage:`, `Baseline Changed-File Coverage:`, and `Output Summary:`.

### Phase 1 — Recreate residual excluded work in a sibling worktree

Completion criteria: the residual excluded work exists on a clean branch from `origin/development` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean`, and its diff is limited to the residual allowlist while the main workspace remains on `feature/utilities-coverage-part-three-87`.

- [ ] [P1-T1] Verify whether `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and branch `chore/mixed-branch-excluded-work-clean` already exist, then reuse the matching worktree state or remove stale/conflicting state before creating the sibling worktree from `origin/development`, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-worktree-created.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the precheck command(s) and for the final `git worktree add c:\Users\DanMoisan\repos\TaskMaster-residual-clean -b chore/mixed-branch-excluded-work-clean origin/development` command when creation is required, `EXIT_CODE: 0`, `Precheck Result:`, `Worktree Path: c:\Users\DanMoisan\repos\TaskMaster-residual-clean`, `Branch: chore/mixed-branch-excluded-work-clean`, `Base Ref: origin/development`, `Base SHA:`, and `Output Summary:` confirming the final resolved worktree state without changing the main workspace branch.

- [ ] [P1-T2] Cherry-pick commit `52742b8` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-cherry-pick-52742b8.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean cherry-pick 52742b8`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T3] Cherry-pick commit `4d5f476` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-cherry-pick-4d5f476.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean cherry-pick 4d5f476`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T4] Cherry-pick commit `60408b0` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-cherry-pick-60408b0.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean cherry-pick 60408b0`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T5] Cherry-pick commit `16d7d5d` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-cherry-pick-16d7d5d.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean cherry-pick 16d7d5d`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T6] Cherry-pick commit `0c9a045` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-cherry-pick-0c9a045.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean cherry-pick 0c9a045`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T7] Cherry-pick commit `66220df` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-cherry-pick-66220df.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean cherry-pick 66220df`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T8] Cherry-pick commit `ea0206e` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-cherry-pick-ea0206e.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean cherry-pick ea0206e`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T9] Restore only `QuickFiler/Controllers/QfcHomeController.cs` and `missing-serializable-list.json` from commit `ee92dd6` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean`, commit those restored paths, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-bootstrap-ee92dd6.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the restore and commit commands, `EXIT_CODE: 0`, `Committed Paths: QuickFiler/Controllers/QfcHomeController.cs; missing-serializable-list.json`, and `Output Summary:` naming the resulting commit SHA.

- [ ] [P1-T10] Restore only `TaskMaster/TaskMaster.csproj` from commit `a8d24b2` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean`, commit that restored path, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-bootstrap-a8d24b2.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the restore and commit commands, `EXIT_CODE: 0`, `Committed Path: TaskMaster/TaskMaster.csproj`, and `Output Summary:` naming the resulting commit SHA.

- [ ] [P1-T11] Restore only `TaskMaster/AppGlobals/AppAutoFileObjects.cs` from commit `4634ac5` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean`, commit that restored path, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-bootstrap-4634ac5.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the restore and commit commands, `EXIT_CODE: 0`, `Committed Path: TaskMaster/AppGlobals/AppAutoFileObjects.cs`, and `Output Summary:` naming the resulting commit SHA.

- [ ] [P1-T12] Run `git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean diff --name-only origin/development...chore/mixed-branch-excluded-work-clean`, verify the result matches the residual allowlist, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-focused-diff.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean diff --name-only origin/development...chore/mixed-branch-excluded-work-clean`, `EXIT_CODE: 0`, and `Output Summary:` proving every changed path is within `.codex/**`, `.github/**`, `QuickFiler/**`, `QuickFiler.Test/**`, `TaskMaster/**`, `UtilitiesSwordfish/**`, or `missing-serializable-list.json` and that no `UtilitiesCS/**`, `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/**`, or `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**` paths remain.

### Phase 2 — Run the applicable QA loop in the sibling worktree

Completion criteria: one clean final QA pass succeeds on `chore/mixed-branch-excluded-work-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean`, and the test / workflow artifacts record numeric coverage values plus passing actionlint output.

If P2-T1 changes files or if P2-T2, P2-T3, P2-T4, or P2-T5 fails, restart Phase 2 from P2-T1 and only retain artifacts from the final uninterrupted pass with `EXIT_CODE: 0` at every step.

- [ ] [P2-T1] Run `dotnet tool run csharpier format .` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-qc-format.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; dotnet tool run csharpier format ."`, `EXIT_CODE: 0`, and `Output Summary:` stating whether files changed and whether Phase 2 restarted.

- [ ] [P2-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/dev-tools/run-actionlint.ps1` inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-qc-actionlint.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/dev-tools/run-actionlint.ps1"`, `EXIT_CODE: 0`, and `Output Summary:` confirming actionlint passed from the final clean pass.

- [ ] [P2-T3] Run the analyzer build command inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-qc-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [ ] [P2-T4] Run the nullable-as-errors build command inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-qc-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [ ] [P2-T5] Run the repository-standard full MSTest-with-coverage command inside `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-qc-test-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug"`, `EXIT_CODE: 0`, and `Output Summary:` including numeric coverage values for the touched residual C# scope extracted from the full repository coverage run from the final clean pass.

- [ ] [P2-T6] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\residual-coverage-delta.md` by comparing `residual-current-coverage-headline.md` with `residual-qc-test-coverage.md` for the touched residual production files.
  - Acceptance: Artifact contains `Timestamp:`, `Touched Scope: QuickFiler + UtilitiesSwordfish + TaskMaster/AppGlobals`, `Baseline Scope Coverage:`, `Final Scope Coverage:`, `Changed Production Files:`, `Changed-Code Coverage:`, and `Output Summary:` explicitly stating whether the clean residual branch regressed, preserved, or improved coverage for the touched scope using the full coverage runs from `residual-baseline-test-coverage.md` and `residual-qc-test-coverage.md`.

### Phase 3 — Push the residual branch, create the PR, and record the next remediation pass handoff

Completion criteria: the clean residual branch is pushed to `origin`, the residual PR is created after QA passes, and the next-pass order is recorded without introducing external wait-state tasks into this execution plan.

- [ ] [P3-T1] Push `chore/mixed-branch-excluded-work-clean` from `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` to `origin` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-push.md`.
  - Preconditions: P2-T6 complete.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean push -u origin chore/mixed-branch-excluded-work-clean`, `EXIT_CODE: 0`, `Branch: chore/mixed-branch-excluded-work-clean`, `Remote: origin`, `Head SHA:`, and `Output Summary:` confirming the upstream branch was created successfully.

- [ ] [P3-T2] Create a dedicated PR from `chore/mixed-branch-excluded-work-clean` to `development` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-pr.md`.
  - Preconditions: P3-T1 complete.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; gh pr create --repo drmoisan/TaskMaster --base development --head chore/mixed-branch-excluded-work-clean --fill"`, `EXIT_CODE: 0`, `Branch: chore/mixed-branch-excluded-work-clean`, `Base Branch: development`, `Head SHA:`, `PR URL:`, and `Output Summary:` confirming the PR was created successfully from the clean worktree context.

- [ ] [P3-T3] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\residual-next-pass-handoff.md` summarizing the verified next-pass order `clean #87` and the requirement that the final issue `#87` pass be planned and validated separately after the residual PR outcome is known.
  - Acceptance: Artifact contains `Timestamp:`, `Completed Pass: residual excluded work clean branch + PR`, `Next Pass Order: clean #87`, and `Output Summary:` explicitly stating that no final issue `#87` execution was attempted in this plan.

## Branch Sync Protocol

If `origin/development` advances before the residual PR is reviewed, apply this protocol to `chore/mixed-branch-excluded-work-clean` in `c:\Users\DanMoisan\repos\TaskMaster-residual-clean` before continuing with any follow-up work:

1. `git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean fetch origin`
2. `git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean rebase origin/development`
3. Refresh `residual-focused-diff.md` and any affected QA evidence.
4. `git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean push --force-with-lease origin chore/mixed-branch-excluded-work-clean`

## Preflight status

- This file is the canonical remediation plan and must be updated in place for every preflight revision.
- This version intentionally scopes execution to a single locally completable pass and keeps the authoritative plan/evidence paths on the current branch by using a sibling worktree for clean-branch operations.
- Current preflight status: `PREFLIGHT: PENDING`
