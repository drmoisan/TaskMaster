---
title: "Remediation Plan: 2026-03-25-quickfiler-gui-not-expanding-96 (2026-03-26T15-25)"
issue: "#96"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-26T15-25"
status: "Planned"
status_color: "blue"
version: "1.0"
work_mode: "minor-audit"
requirements_source: "docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/issue.md"
secondary_context: "docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/plan.2026-03-25T09-03.md"
base_ref: "origin/development"
---

# Remediation Plan: 2026-03-25-quickfiler-gui-not-expanding-96 (2026-03-26T15-25)

## Overview

**Status Badge:** [Planned | blue]

This remediation file defines a worktree-safe clean-branch recovery pass for issue `#96`. The pass keeps the authoritative plan file and evidence folders on the current workspace checkout, creates a sibling Git worktree for the clean issue `#96` branch from `origin/development`, replays only the issue `#96` commits, reruns the full local C# QA loop in that sibling worktree, pushes the recovered branch, opens the dedicated PR, and records the final handoff state for the remaining issue `#87` remediation sequence.

## Scope Guardrails

- **CON-1:** Treat `issue.md` as the sole authoritative requirements source for this minor-audit pass; use `plan.2026-03-25T09-03.md`, `policy-audit.2026-03-25T14-00.md`, `feature-audit.2026-03-25T14-00.md`, and `artifacts/research/20260326-issue87-unstacking-sequence-research.md` as supporting context only.
- **CON-2:** Use `origin/development` as the only comparison base for the clean issue `#96` recovery branch.
- **CON-3:** Keep the main workspace checkout on `feature/utilities-coverage-part-three-87` for the entire pass so this plan file and the issue `#96` evidence folders remain available.
- **CON-4:** Run all clean issue `#96` branch operations in the sibling worktree path `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean`; do not `git switch` the main workspace to the clean branch.
- **CON-5:** Replay only the issue `#96` commits `bd8fc03` and `3b472b2`; do not include issue `#97`, residual excluded work, or final issue `#87` scope in this pass.
- **CON-6:** Do not mark any acceptance or completion item satisfied without evidence on disk containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` when a command is involved.

## Requirements Traceability

| REQ | Source | Required outcome | Implementation tasks | Validation tasks |
|---|---|---|---|---|
| REQ-1 | issue.md + minor-audit policy | Capture the current baseline state and prior issue `#96` evidence before branch recovery begins | P0-T1 through P0-T8 | P0-T8 |
| REQ-2 | issue.md | Recover issue `#96` onto a dedicated clean branch from `origin/development` inside a sibling worktree using only the identified issue `#96` commits | P1-T1 through P1-T4 | P1-T4 |
| REQ-3 | general-code-change + csharp-code-change policy | Run the full C# QA loop on the clean issue `#96` branch before PR creation and record baseline-to-final coverage results for the touched QuickFiler scope | P2-T1 through P2-T5 | P2-T5 |
| REQ-4 | unstacking follow-on sequencing | Push the clean issue `#96` branch, create the issue `#96` PR after QA passes, and record the handoff to the residual excluded-work pass | P3-T1 through P3-T3 | P3-T3 |

## Acceptance Criteria

- **REQ-1:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\` contains baseline artifacts for policy reads, the current diff scope, formatter, analyzer build, nullable build, targeted test baseline, and coverage-enabled QuickFiler.Test execution, and the coverage artifacts record numeric values for the touched `QuickFiler` scope in `Output Summary:`.
- **REQ-2:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-focused-diff.md` records that the clean issue `#96` branch diff inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` is limited to `QuickFiler/**`, `QuickFiler.Test/**`, and `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**`.
- **REQ-3:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\qa-gates\issue96-qc-test-coverage.md` records numeric post-change coverage values for the touched `QuickFiler` scope in `Output Summary:`, `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\qa-gates\issue96-coverage-delta.md` records baseline coverage, final coverage, and changed-code coverage for the touched production files, and the formatter, analyzer, nullable, and test artifacts all record `EXIT_CODE: 0` from one clean final pass.
- **REQ-4:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-pr.md` records the clean issue `#96` PR URL after QA passes, and `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\next-pass-handoff.md` records the next-pass order `residual excluded work -> clean #87`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Capture issue #96 baseline context and QA state

Completion criteria: baseline artifacts capture the authoritative issue `#96` requirements, the current mixed-branch scope, and one baseline QuickFiler-oriented QA snapshot.

If P0-T3 changes files, rerun P0-T2 and P0-T3 and keep only the final baseline artifacts produced after the formatter reports no additional file changes.

- [x] [P0-T1] Read `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, then read `issue.md`, `plan.2026-03-25T09-03.md`, `policy-audit.2026-03-25T14-00.md`, `feature-audit.2026-03-25T14-00.md`, and `artifacts/research/20260326-issue87-unstacking-sequence-research.md`, then write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\phase0-instructions-read.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Policy Order:`, and a `Files Read:` list naming every file in this task.

- [x] [P0-T2] Capture the current issue `#96` mixed-branch diff scope by running `git diff --name-status $(git merge-base HEAD origin/development) HEAD`, then write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\current-diff-scope.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD`, `EXIT_CODE: 0`, and `Output Summary:` naming the presence of issue `#96`, issue `#97`, and issue `#87` scope in the current mixed branch.

- [x] [P0-T3] Run `dotnet tool run csharpier format .` from the repository root and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\baseline-format.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` describing whether the formatter changed files and whether P0-T2/P0-T3 were rerun.

- [x] [P0-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\baseline-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [x] [P0-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\baseline-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [x] [P0-T6] Run `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController"` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\baseline-targeted-test.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` identifying the targeted issue `#96` keyboard-registration tests in the current branch.

- [x] [P0-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` from the repository root and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\baseline-test-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`, `EXIT_CODE: 0`, and `Output Summary:` including numeric coverage values for the touched `QuickFiler` scope extracted from the full repository coverage run.

- [x] [P0-T8] Capture the baseline-coverage headline by copying the current touched-scope coverage values from `baseline-test-coverage.md` into `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\baseline\current-coverage-headline.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Source Artifact: baseline-test-coverage.md`, `Touched Scope: QuickFiler`, `Baseline QuickFiler Coverage:`, `Baseline Changed-File Coverage:`, and `Output Summary:`.

### Phase 1 — Recreate issue #96 in a sibling worktree

Completion criteria: issue `#96` exists on a clean branch from `origin/development` inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean`, and its diff is limited to issue `#96` scope while the main workspace remains on `feature/utilities-coverage-part-three-87`.

- [x] [P1-T1] Verify whether `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` and branch `bug/quickfiler-gui-not-expanding-96-clean` already exist, then reuse the matching worktree state or remove stale/conflicting state before creating the sibling worktree from `origin/development`, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-worktree-created.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the precheck command(s) and for the final `git worktree add c:\Users\DanMoisan\repos\TaskMaster-issue96-clean -b bug/quickfiler-gui-not-expanding-96-clean origin/development` command when creation is required, `EXIT_CODE: 0`, `Precheck Result:`, `Worktree Path: c:\Users\DanMoisan\repos\TaskMaster-issue96-clean`, `Branch: bug/quickfiler-gui-not-expanding-96-clean`, `Base Ref: origin/development`, `Base SHA:`, and `Output Summary:` confirming the final resolved worktree state without changing the main workspace branch.

- [x] [P1-T2] Cherry-pick commit `bd8fc03` onto `bug/quickfiler-gui-not-expanding-96-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-cherry-pick-bd8fc03.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean cherry-pick bd8fc03`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [x] [P1-T3] Cherry-pick commit `3b472b2` onto `bug/quickfiler-gui-not-expanding-96-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-cherry-pick-3b472b2.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean cherry-pick 3b472b2`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [x] [P1-T4] Run `git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean diff --name-only origin/development...bug/quickfiler-gui-not-expanding-96-clean`, verify the result matches the issue `#96` allowlist, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-focused-diff.md`.
  - Acceptance: The focused-diff artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean diff --name-only origin/development...bug/quickfiler-gui-not-expanding-96-clean`, `EXIT_CODE: 0`, and `Output Summary:` proving every changed path is within `QuickFiler/**`, `QuickFiler.Test/**`, or `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**`.

### Phase 2 — Run the full C# QA loop in the sibling worktree

Completion criteria: one clean final C# QA pass succeeds on `bug/quickfiler-gui-not-expanding-96-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean`, and the test artifact records numeric coverage values.

If P2-T1 changes files or if P2-T2, P2-T3, or P2-T4 fails, restart Phase 2 from P2-T1 and only retain artifacts from the final uninterrupted pass with `EXIT_CODE: 0` at every step.

- [x] [P2-T1] Run `dotnet tool run csharpier format .` inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\qa-gates\issue96-qc-format.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue96-clean'; dotnet tool run csharpier format ."`, `EXIT_CODE: 0`, and `Output Summary:` stating whether files changed and whether Phase 2 restarted.

- [x] [P2-T2] Run the analyzer build command inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\qa-gates\issue96-qc-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue96-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [x] [P2-T3] Run the nullable-as-errors build command inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\qa-gates\issue96-qc-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue96-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [x] [P2-T4] Run the repository-standard full MSTest-with-coverage command inside `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\qa-gates\issue96-qc-test-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue96-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug"`, `EXIT_CODE: 0`, and `Output Summary:` including numeric coverage values for the touched `QuickFiler` scope extracted from the full repository coverage run from the final clean pass.

- [x] [P2-T5] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\qa-gates\issue96-coverage-delta.md` by comparing `current-coverage-headline.md` with `issue96-qc-test-coverage.md` for the touched issue `#96` production files.
  - Acceptance: Artifact contains `Timestamp:`, `Touched Scope: QuickFiler`, `Baseline QuickFiler Coverage:`, `Final QuickFiler Coverage:`, `Changed Production Files:`, `Changed-Code Coverage:`, and `Output Summary:` explicitly stating whether the clean issue `#96` branch regressed, preserved, or improved coverage for the touched scope using the full coverage runs from `baseline-test-coverage.md` and `issue96-qc-test-coverage.md`.

### Phase 3 — Push the issue #96 branch, create the PR, and record the next remediation pass handoff

Completion criteria: the clean issue `#96` branch is pushed to `origin`, the issue `#96` PR is created after QA passes, and the next-pass order is recorded without introducing external wait-state tasks into this execution plan.

- [x] [P3-T1] Push `bug/quickfiler-gui-not-expanding-96-clean` from `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` to `origin` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-push.md`.
  - Preconditions: P2-T5 complete.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean push -u origin bug/quickfiler-gui-not-expanding-96-clean`, `EXIT_CODE: 0`, `Branch: bug/quickfiler-gui-not-expanding-96-clean`, `Remote: origin`, `Head SHA:`, and `Output Summary:` confirming the upstream branch was created successfully.

- [x] [P3-T2] Create a dedicated PR from `bug/quickfiler-gui-not-expanding-96-clean` to `development` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\issue96-pr.md`.
  - Preconditions: P3-T1 complete.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue96-clean'; gh pr create --repo drmoisan/TaskMaster --base development --head bug/quickfiler-gui-not-expanding-96-clean --fill"`, `EXIT_CODE: 0`, `Branch: bug/quickfiler-gui-not-expanding-96-clean`, `Base Branch: development`, `Head SHA:`, `PR URL:`, and `Output Summary:` confirming the PR was created successfully from the clean worktree context.

- [x] [P3-T3] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-25-quickfiler-gui-not-expanding-96\evidence\other\next-pass-handoff.md` summarizing the verified follow-on sequence `residual excluded work -> clean #87` and the requirement that each later pass be planned and validated separately after the issue `#96` PR outcome is known.
  - Acceptance: Artifact contains `Timestamp:`, `Completed Pass: issue #96 clean branch + PR`, `Next Pass Order: residual excluded work -> clean #87`, and `Output Summary:` explicitly stating that no later-pass execution was attempted in this plan.

## Branch Sync Protocol

If `origin/development` advances before the issue `#96` PR is reviewed, apply this protocol to `bug/quickfiler-gui-not-expanding-96-clean` in `c:\Users\DanMoisan\repos\TaskMaster-issue96-clean` before continuing with any follow-up work:

1. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean fetch origin`
2. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean rebase origin/development`
3. Refresh `issue96-focused-diff.md` and any affected QA evidence.
4. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue96-clean push --force-with-lease origin bug/quickfiler-gui-not-expanding-96-clean`

## Preflight status

- This file is the canonical remediation plan and must be updated in place for every preflight revision.
- This version intentionally scopes execution to a single locally completable pass and keeps the authoritative plan/evidence paths on the current branch by using a sibling worktree for clean-branch operations.
- Current preflight status: `PREFLIGHT: ALL CLEAR`
