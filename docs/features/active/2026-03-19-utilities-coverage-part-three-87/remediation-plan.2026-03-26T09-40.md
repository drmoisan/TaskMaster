---
title: "Remediation Plan: 2026-03-19-utilities-coverage-part-three-87 (2026-03-26T09-40)"
issue: "#87"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-26T15-05"
status: "Planned"
status_color: "blue"
version: "1.3"
work_mode: "full-feature"
requirements_source: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md"
secondary_context: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/user-story.md"
base_ref: "origin/development"
---

# Remediation Plan: 2026-03-19-utilities-coverage-part-three-87 (2026-03-26T09-40)

## Overview

**Status Badge:** [Planned | blue]

This remediation file now defines a single executor-safe recovery pass for the first unstacking branch: issue `#97`. The pass keeps the authoritative plan file and evidence folders on the current `feature/utilities-coverage-part-three-87` checkout, creates a sibling Git worktree for the clean issue `#97` branch, runs all clean-branch Git and C# QA commands inside that sibling worktree, opens the dedicated PR only after QA passes, and records the follow-on branch order for later remediation passes.

## Scope Guardrails

- **CON-1:** Treat `spec.md` and `user-story.md` as the authoritative full-feature requirements for this pass; use `remediation-inputs.2026-03-26T09-40.md`, `artifacts/research/20260326-issue87-unstacking-sequence-research.md`, and `.git/branch_analysis_issue87.txt` as supporting context only.
- **CON-2:** Use `origin/development` as the only comparison base for the clean issue `#97` recovery branch.
- **CON-3:** Preserve the current mixed branch as an archive/reference source; do not rewrite or delete it during this pass.
- **CON-4:** Do not cherry-pick merge commit `c448819`; replay only the linear issue `#97` commits `a19ac86` and `ad4ae95`.
- **CON-5:** Do not begin issue `#96`, residual excluded-work, or final clean issue `#87` reconstruction during this pass.
- **CON-6:** Do not mark any acceptance or completion item satisfied without evidence on disk containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` when a command is involved.
- **CON-7:** Keep the main workspace checkout on `feature/utilities-coverage-part-three-87` for the entire pass so `remediation-plan.2026-03-26T09-40.md` and the feature evidence folders remain available.
- **CON-8:** Run all clean issue `#97` branch operations in the sibling worktree path `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean`; do not `git switch` the main workspace to the clean branch.

## Requirements Traceability

| REQ | Source | Required outcome | Implementation tasks | Validation tasks |
|---|---|---|---|---|
| REQ-1 | remediation-inputs §1 | Capture the current mixed-branch baseline state and the applicable C# QA baseline before branch isolation begins | P0-T1 through P0-T8 | P0-T8 |
| REQ-2 | unstacking research | Recover issue `#97` onto a dedicated clean branch from `origin/development` inside a sibling worktree using only the identified issue `#97` commits | P1-T1 through P1-T5 | P1-T5 |
| REQ-3 | general-code-change + csharp-code-change policy | Run the full C# QA loop on the clean issue `#97` branch before PR creation and record baseline-to-final coverage results for the touched QuickFiler scope | P2-T1 through P2-T5 | P2-T5 |
| REQ-4 | remediation follow-on sequencing | Push the clean issue `#97` branch, create the issue `#97` PR after QA passes, and record the next remediation passes in the verified unstacking order after this pass finishes locally | P3-T1 through P3-T3 | P3-T3 |

## Acceptance Criteria

- **REQ-1:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/` contains baseline artifacts for the current mixed-branch diff, the branch split source map, formatter, analyzer build, nullable build, and coverage-enabled MSTest, and the baseline coverage artifacts record numeric coverage values for the touched `QuickFiler` scope in `Output Summary:`.
- **REQ-2:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue97-focused-diff.md` records that the clean issue `#97` branch diff inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` is limited to `QuickFiler/**`, `QuickFiler.Test/**`, and `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/**`.
- **REQ-3:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue97-qc-test-coverage.md` records numeric post-change coverage values for the touched `QuickFiler` scope in `Output Summary:`, `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue97-coverage-delta.md` records baseline coverage, final coverage, and changed-code coverage for the touched production files, and the formatter, analyzer, nullable, and test artifacts all record `EXIT_CODE: 0` from one clean final pass.
- **REQ-4:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue97-pr.md` records the clean issue `#97` PR URL after QA passes, and `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\next-pass-handoff.md` records the next-pass order `#96` -> residual excluded work -> clean `#87`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Capture remediation and C# QA baselines

Completion criteria: baseline artifacts capture the current mixed-branch scope, the source commit map, and one full baseline C# toolchain pass.

- [ ] [P0-T1] Read `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, then read `issue.md`, `spec.md`, `user-story.md`, `remediation-inputs.2026-03-26T09-40.md`, `artifacts/research/20260326-issue87-unstacking-sequence-research.md`, and `.git/branch_analysis_issue87.txt`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/phase0-instructions-read.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Policy Order:`, and a `Files Read:` list naming every file in this task.

- [ ] [P0-T2] Capture the current mixed-branch diff scope by running `git diff --name-status $(git merge-base HEAD origin/development) HEAD`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/current-diff-scope.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD`, `EXIT_CODE: 0`, and `Output Summary:` naming `.codex`, `.github`, `QuickFiler`, `TaskMaster`, issue `#96`, and issue `#97` scope.

- [ ] [P0-T3] Copy the current source-commit split map from `.git/branch_analysis_issue87.txt` into `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/branch-split-source-map.md`.
	- Acceptance: Artifact contains the exact issue `#97`, issue `#96`, residual excluded-work, and clean issue `#87` commit buckets from `.git/branch_analysis_issue87.txt`.

- [ ] [P0-T4] Run `dotnet tool run csharpier format .` from the repository root and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-format.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` describing whether the formatter changed files.

- [ ] [P0-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-analyzers.md`.
	- Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [ ] [P0-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-nullable.md`.
	- Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [ ] [P0-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-test-coverage.md`.
	- Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` including numeric coverage values for the touched `QuickFiler` scope and the numeric remaining below-threshold file count for that scope if reported.

- [ ] [P0-T8] Capture the baseline-coverage headline by copying the current touched-scope coverage values from `baseline-test-coverage.md` into `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/current-coverage-headline.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Source Artifact: baseline-test-coverage.md`, `Touched Scope: QuickFiler`, `Baseline QuickFiler Coverage:`, `Baseline Changed-File Coverage:`, and `Output Summary:`.

### Phase 1 — Recreate issue #97 in a sibling worktree

Completion criteria: issue `#97` exists on a clean branch from `origin/development` inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean`, and its diff is limited to issue `#97` scope while the main workspace remains on `feature/utilities-coverage-part-three-87`.

- [ ] [P1-T1] Create archive branch `archive/feature-util-coverage-87-mixed-20260326` from the current `feature/utilities-coverage-part-three-87` head and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/archive-source-branch.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: git branch archive/feature-util-coverage-87-mixed-20260326 feature/utilities-coverage-part-three-87`, `EXIT_CODE: 0`, `Source Branch: feature/utilities-coverage-part-three-87`, `Source HEAD SHA:`, `Archive Branch: archive/feature-util-coverage-87-mixed-20260326`, and `Output Summary:` confirming identical archive/source SHAs.

- [ ] [P1-T2] Create sibling worktree `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` with branch `bug/getmovediagnostics-null-guard-97-clean` from `origin/development` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue97-worktree-created.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: git worktree add c:\Users\DanMoisan\repos\TaskMaster-issue97-clean -b bug/getmovediagnostics-null-guard-97-clean origin/development`, `EXIT_CODE: 0`, `Worktree Path: c:\Users\DanMoisan\repos\TaskMaster-issue97-clean`, `Branch: bug/getmovediagnostics-null-guard-97-clean`, `Base Ref: origin/development`, `Base SHA:`, and `Output Summary:` confirming the worktree was created without changing the main workspace branch.

- [ ] [P1-T3] Cherry-pick commit `a19ac86` onto `bug/getmovediagnostics-null-guard-97-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue97-cherry-pick-a19ac86.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean cherry-pick a19ac86`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T4] Cherry-pick commit `ad4ae95` onto `bug/getmovediagnostics-null-guard-97-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue97-cherry-pick-ad4ae95.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean cherry-pick ad4ae95`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA.

- [ ] [P1-T5] Run `git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean diff --name-only origin/development...bug/getmovediagnostics-null-guard-97-clean`, verify the result matches the issue `#97` allowlist, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue97-focused-diff.md`.
	- Acceptance: The focused-diff artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean diff --name-only origin/development...bug/getmovediagnostics-null-guard-97-clean`, `EXIT_CODE: 0`, and `Output Summary:` proving every changed path is within `QuickFiler/**`, `QuickFiler.Test/**`, or `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/**`.

### Phase 2 — Run the full C# QA loop in the sibling worktree

Completion criteria: one clean final C# QA pass succeeds on `bug/getmovediagnostics-null-guard-97-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean`, and the test artifact records numeric coverage values.

If P2-T1 changes files or if P2-T2, P2-T3, or P2-T4 fails, restart Phase 2 from P2-T1 and only retain artifacts from the final uninterrupted pass with `EXIT_CODE: 0` at every step.

- [ ] [P2-T1] Run `dotnet tool run csharpier format .` inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue97-qc-format.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue97-clean'; dotnet tool run csharpier format ."`, `EXIT_CODE: 0`, and `Output Summary:` stating whether files changed and whether Phase 2 restarted.

- [ ] [P2-T2] Run the analyzer build command inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue97-qc-analyzers.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue97-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [ ] [P2-T3] Run the nullable-as-errors build command inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue97-qc-nullable.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue97-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [ ] [P2-T4] Run the coverage-enabled MSTest command inside `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue97-qc-test-coverage.md`.
	- Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue97-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug"`, `EXIT_CODE: 0`, and `Output Summary:` including numeric coverage values for the touched `QuickFiler` scope and the numeric remaining below-threshold file count for that scope from the final clean pass.

- [ ] [P2-T5] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue97-coverage-delta.md` by comparing `current-coverage-headline.md` with `issue97-qc-test-coverage.md` for the touched issue `#97` production files.
	- Acceptance: Artifact contains `Timestamp:`, `Touched Scope: QuickFiler`, `Baseline QuickFiler Coverage:`, `Final QuickFiler Coverage:`, `Changed Production Files:`, `Changed-Code Coverage:`, and `Output Summary:` explicitly stating whether the clean issue `#97` branch regressed, preserved, or improved coverage for the touched scope.

### Phase 3 — Push the issue #97 branch, create the PR, and record the next remediation pass handoff

Completion criteria: the clean issue `#97` branch is pushed to `origin`, the issue `#97` PR is created after QA passes, and the next-pass order is recorded without introducing external wait-state tasks into this execution plan.

- [ ] [P3-T1] Push `bug/getmovediagnostics-null-guard-97-clean` from `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` to `origin` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue97-push.md`.
	- Preconditions: P2-T5 complete.
	- Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean push -u origin bug/getmovediagnostics-null-guard-97-clean`, `EXIT_CODE: 0`, `Branch: bug/getmovediagnostics-null-guard-97-clean`, `Remote: origin`, `Head SHA:`, and `Output Summary:` confirming the upstream branch was created successfully.

- [ ] [P3-T2] Create a dedicated PR from `bug/getmovediagnostics-null-guard-97-clean` to `development` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue97-pr.md`.
	- Preconditions: P3-T1 complete.
	- Acceptance: Artifact contains `Timestamp:`, `Command: gh pr create --repo drmoisan/TaskMaster --base development --head bug/getmovediagnostics-null-guard-97-clean --fill`, `EXIT_CODE: 0`, `Branch: bug/getmovediagnostics-null-guard-97-clean`, `Base Branch: development`, `Head SHA:`, `PR URL:`, and `Output Summary:` confirming the PR was created successfully.

- [ ] [P3-T3] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\next-pass-handoff.md` summarizing the verified follow-on sequence `#96` -> residual excluded work -> clean `#87` and the requirement that each later pass be planned and validated separately after the issue `#97` PR outcome is known.
	- Acceptance: Artifact contains `Timestamp:`, `Completed Pass: issue #97 clean branch + PR`, `Next Pass Order: #96 -> residual excluded work -> clean #87`, and `Output Summary:` explicitly stating that no later-pass execution was attempted in this plan.

## Branch Sync Protocol

If `origin/development` advances before the issue `#97` PR is reviewed, apply this protocol to `bug/getmovediagnostics-null-guard-97-clean` in `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean` before continuing with any follow-up work:

1. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean fetch origin`
2. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean rebase origin/development`
3. Refresh `issue97-focused-diff.md` and any affected QA evidence.
4. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue97-clean push --force-with-lease origin bug/getmovediagnostics-null-guard-97-clean`

## Preflight status

- This file is the canonical remediation plan and must be updated in place for every preflight revision.
- This version intentionally scopes execution to a single locally completable pass and keeps the authoritative plan/evidence paths on the current branch by using a sibling worktree for clean-branch operations.
- Current preflight status: `PREFLIGHT: PENDING`