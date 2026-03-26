---
title: "Remediation Plan: final clean issue #87 (2026-03-26T16-10)"
issue: "#87"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-26T16-10"
status: "Planned"
status_color: "blue"
version: "1.0"
work_mode: "full-feature"
requirements_source: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md"
secondary_context: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/user-story.md"
base_ref: "origin/development"
---

# Remediation Plan: final clean issue #87 (2026-03-26T16-10)

## Overview

**Status Badge:** [Planned | blue]

This remediation file defines the final executor-safe recovery pass for issue `#87`. The pass keeps the authoritative plan file and evidence folders on the current `feature/utilities-coverage-part-three-87` checkout, verifies that the clean `#97`, clean `#96`, and residual excluded-work PRs are already merged, creates a sibling Git worktree for the clean issue `#87` branch, rebuilds the issue `#87` branch from `origin/development` using only the direct clean commits plus issue-`#87`-only bootstrap paths from mixed commits, then runs the full C# QA loop before opening the draft feature PR.

## Scope Guardrails

- **CON-1:** Treat `issue.md`, `spec.md`, and `user-story.md` as the authoritative requirements for this final full-feature pass; use `remediation-inputs.2026-03-26T09-40.md`, `artifacts/research/20260326-issue87-unstacking-sequence-research.md`, `.git/branch_analysis_issue87.txt`, `remediation-plan.2026-03-26T09-40.md`, `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/remediation-plan.2026-03-26T15-25.md`, and `remediation-plan.residual-excluded-work.2026-03-26T15-45.md` as supporting context only.
- **CON-2:** Use `origin/development` as the only comparison base for the clean issue `#87` recovery branch.
- **CON-3:** Preserve the current mixed branch as an archive/reference source; do not rewrite or delete it during this pass.
- **CON-4:** Do not replay issue `#97`, issue `#96`, residual excluded-work, or merge commit `c448819` in this pass; issue `#87` must be reconstructed only from the listed clean issue `#87` commits plus issue-`#87`-only bootstrap restores from mixed commits.
- **CON-5:** Do not restore `QuickFiler/**`, `QuickFiler.Test/**`, `TaskMaster/TaskMaster.csproj`, `TaskMaster/Ribbon/RibbonExplorer.xml`, `.codex/**`, `.github/**`, `UtilitiesSwordfish/**`, or `missing-serializable-list.json` in this pass; those paths belong to earlier clean passes.
- **CON-6:** Do not mark any acceptance or completion item satisfied without evidence on disk containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` when a command is involved.
- **CON-7:** Keep the main workspace checkout on `feature/utilities-coverage-part-three-87` for the entire pass so this plan file and the feature evidence folders remain available.
- **CON-8:** Run all clean issue `#87` branch operations in the sibling worktree path `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean`; do not `git switch` the main workspace to the clean branch.
- **CON-9:** Open the clean issue `#87` PR as a draft PR after the full QA loop passes and the focused diff proves the branch contains only issue `#87` scope.

## Requirements Traceability

| REQ | Source | Required outcome | Implementation tasks | Validation tasks |
|---|---|---|---|---|
| REQ-1 | remediation follow-on sequencing + unstacking research | Verify that the clean `#97`, clean `#96`, and residual excluded-work PRs are merged and capture the current mixed-branch + UtilitiesCS baseline before reconstruction begins | P0-T1 through P0-T10 | P0-T10 |
| REQ-2 | unstacking research | Recover issue `#87` onto a dedicated clean branch from `origin/development` inside a sibling worktree using only the identified clean `#87` commits plus issue-`#87`-only bootstrap paths from mixed commits | P1-T1 through P1-T9 | P1-T9 |
| REQ-3 | general-code-change + csharp-code-change + general-unit-test + csharp-unit-test policy | Run the full C# QA loop on the clean issue `#87` branch before PR creation and record that UtilitiesCS meets the required coverage threshold without regression | P2-T1 through P2-T6 | P2-T6 |
| REQ-4 | remediation completion | Push the clean issue `#87` branch, open the draft issue `#87` PR after QA passes, and record the mixed-branch retirement handoff state | P3-T1 through P3-T3 | P3-T3 |

## Acceptance Criteria

- **REQ-1:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-phase0-instructions-read.md` records the required policy/context reads, `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/issue97-merged-precheck.md`, `issue96-merged-precheck.md`, and `residual-merged-precheck.md` each record a merged PR state from GitHub, and the baseline formatter, analyzer, nullable, and coverage artifacts record the current mixed-branch UtilitiesCS baseline with numeric coverage values in `Output Summary:`.
- **REQ-2:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-focused-diff.md` records that the clean issue `#87` branch diff inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` is limited to `UtilitiesCS/**`, `UtilitiesCS.Test/**`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/**`.
- **REQ-3:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-qc-test-coverage.md` records numeric post-change repository-wide line coverage, numeric UtilitiesCS coverage values, and numeric below-threshold remaining-file count in `Output Summary:`, `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-coverage-delta.md` records baseline repository coverage, final repository coverage, baseline UtilitiesCS coverage, final UtilitiesCS coverage, and changed-code coverage for touched issue `#87` production files, `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-final-coverage-verification.md` records `UtilitiesCS Below-Threshold Files Remaining: 0`, and the formatter, analyzer, nullable, and test artifacts all record `EXIT_CODE: 0` from one clean final pass.
- **REQ-4:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-pr.md` records the clean issue `#87` draft PR URL after QA passes, and `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-closeout-handoff.md` records that all four replacement PRs now exist and that the mixed archive branch must be retained until every replacement PR is merged.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Verify prerequisite merges and capture final UtilitiesCS baselines

Completion criteria: prerequisite PR merges are verified from GitHub, the current mixed-branch scope is captured, and one full baseline C# toolchain pass records numeric UtilitiesCS coverage values.

- [ ] [P0-T1] Read `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, then read `change-plan.md`, `issue.md`, `spec.md`, `user-story.md`, `remediation-inputs.2026-03-26T09-40.md`, `artifacts/research/20260326-issue87-unstacking-sequence-research.md`, `.git/branch_analysis_issue87.txt`, `remediation-plan.2026-03-26T09-40.md`, `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/remediation-plan.2026-03-26T15-25.md`, and `remediation-plan.residual-excluded-work.2026-03-26T15-45.md`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-phase0-instructions-read.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Policy Order:`, and a `Files Read:` list naming every file in this task.

- [ ] [P0-T2] Look up the clean issue `#97` PR by running `gh pr list --repo drmoisan/TaskMaster --state merged --base development --head bug/getmovediagnostics-null-guard-97-clean --json url,state,mergedAt,headRefName`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/issue97-merged-precheck.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: gh pr list --repo drmoisan/TaskMaster --state merged --base development --head bug/getmovediagnostics-null-guard-97-clean --json url,state,mergedAt,headRefName`, `EXIT_CODE: 0`, `Head Branch: bug/getmovediagnostics-null-guard-97-clean`, `PR URL:`, `State: MERGED`, `Merged At:`, and `Output Summary:` confirming the clean issue `#97` PR is merged.

- [ ] [P0-T3] Look up the clean issue `#96` PR by running `gh pr list --repo drmoisan/TaskMaster --state merged --base development --head bug/quickfiler-gui-not-expanding-96-clean --json url,state,mergedAt,headRefName`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/issue96-merged-precheck.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: gh pr list --repo drmoisan/TaskMaster --state merged --base development --head bug/quickfiler-gui-not-expanding-96-clean --json url,state,mergedAt,headRefName`, `EXIT_CODE: 0`, `Head Branch: bug/quickfiler-gui-not-expanding-96-clean`, `PR URL:`, `State: MERGED`, `Merged At:`, and `Output Summary:` confirming the clean issue `#96` PR is merged.

- [ ] [P0-T4] Look up the residual excluded-work PR by running `gh pr list --repo drmoisan/TaskMaster --state merged --base development --head chore/mixed-branch-excluded-work-clean --json url,state,mergedAt,headRefName`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/residual-merged-precheck.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: gh pr list --repo drmoisan/TaskMaster --state merged --base development --head chore/mixed-branch-excluded-work-clean --json url,state,mergedAt,headRefName`, `EXIT_CODE: 0`, `Head Branch: chore/mixed-branch-excluded-work-clean`, `PR URL:`, `State: MERGED`, `Merged At:`, and `Output Summary:` confirming the residual excluded-work PR is merged.

- [ ] [P0-T5] Capture the current mixed-branch diff scope by running `git diff --name-status $(git merge-base HEAD origin/development) HEAD`, then write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-current-diff-scope.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD`, `EXIT_CODE: 0`, and `Output Summary:` naming `UtilitiesCS`, `UtilitiesCS.Test`, the issue `#87` feature folder, and any remaining non-issue-`#87` top-level paths still present on the mixed branch.

- [ ] [P0-T6] Copy the current source-commit split map from `.git/branch_analysis_issue87.txt` into `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-branch-split-source-map.md`.
  - Acceptance: Artifact contains the exact clean issue `#87` direct cherry-pick list and the issue-`#87` bootstrap restore guidance from `.git/branch_analysis_issue87.txt` and the unstacking research.

- [ ] [P0-T7] Run `dotnet tool run csharpier format .` from the repository root and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-format.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` describing whether the formatter changed files.

- [ ] [P0-T8] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [ ] [P0-T9] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline.

- [ ] [P0-T10] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-test-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` including numeric repository-wide line coverage, numeric UtilitiesCS coverage values, numeric changed-file coverage for the issue `#87` scope if reported, and `UtilitiesCS Below-Threshold Files Remaining:`.

### Phase 1 — Recreate issue #87 in a sibling worktree

Completion criteria: issue `#87` exists on a clean branch from `origin/development` inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean`, and its diff is limited to issue `#87` scope while the main workspace remains on `feature/utilities-coverage-part-three-87`.

- [ ] [P1-T1] Resolve archive branch `archive/feature-util-coverage-87-mixed-20260326` for the current mixed branch by first running `git rev-parse --verify archive/feature-util-coverage-87-mixed-20260326`, then creating it from `feature/utilities-coverage-part-three-87` only if the precheck reports it missing, and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/final87-archive-source-branch.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the precheck command and, when creation is required, `git branch archive/feature-util-coverage-87-mixed-20260326 feature/utilities-coverage-part-three-87`, `EXIT_CODE: 0`, `Precheck Result:`, `Source Branch: feature/utilities-coverage-part-three-87`, `Source HEAD SHA:`, `Archive Branch: archive/feature-util-coverage-87-mixed-20260326`, and `Output Summary:` confirming the final resolved archive state.

- [ ] [P1-T2] Resolve sibling worktree `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` with branch `feature/utilities-coverage-part-three-87-clean` by first running precheck commands to determine whether the target branch or worktree already exists, then running `git worktree add c:\Users\DanMoisan\repos\TaskMaster-issue87-clean -b feature/utilities-coverage-part-three-87-clean origin/development` only when creation is required, and write `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-worktree-created.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the precheck command(s) and for the final `git worktree add c:\Users\DanMoisan\repos\TaskMaster-issue87-clean -b feature/utilities-coverage-part-three-87-clean origin/development` command when creation is required, `EXIT_CODE: 0`, `Precheck Result:`, `Worktree Path: c:\Users\DanMoisan\repos\TaskMaster-issue87-clean`, `Branch: feature/utilities-coverage-part-three-87-clean`, `Base Ref: origin/development`, `Base SHA:`, and `Output Summary:` confirming the final resolved worktree state without changing the main workspace branch.

- [ ] [P1-T3] Cherry-pick clean issue `#87` commits `078fd77 3206593 cce7c5a fff20c7` onto `feature/utilities-coverage-part-three-87-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-cherry-pick-batch1.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean cherry-pick 078fd77 3206593 cce7c5a fff20c7`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA after the batch.

- [ ] [P1-T4] Cherry-pick clean issue `#87` commits `d65320b 2326734 5f90762 27639bf` onto `feature/utilities-coverage-part-three-87-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-cherry-pick-batch2.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean cherry-pick d65320b 2326734 5f90762 27639bf`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA after the batch.

- [ ] [P1-T5] Cherry-pick clean issue `#87` commits `5afe10d ee9e4d9 4009d1c 5661a47` onto `feature/utilities-coverage-part-three-87-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-cherry-pick-batch3.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean cherry-pick 5afe10d ee9e4d9 4009d1c 5661a47`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA after the batch.

- [ ] [P1-T6] Cherry-pick clean issue `#87` commits `4830958 6e5d01d` onto `feature/utilities-coverage-part-three-87-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-cherry-pick-batch4.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean cherry-pick 4830958 6e5d01d`, `EXIT_CODE: 0`, and `Output Summary:` naming the resulting head SHA after the batch.

- [ ] [P1-T7] Restore the issue-`#87` side of mixed commits `ee92dd6`, `a8d24b2`, and `5fb07f7` by running `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean restore --source <sha> -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87` for each SHA, then write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-bootstrap-restore-main.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the three restore commands, `EXIT_CODE: 0`, `Restored Paths: UtilitiesCS; UtilitiesCS.Test; docs/features/active/2026-03-19-utilities-coverage-part-three-87`, and `Output Summary:` confirming no out-of-scope paths were restored.

- [ ] [P1-T8] Restore the issue-`#87` side of mixed commit `221e76f` by running `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean restore --source 221e76f -- UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87`, commit the staged bootstrap paths with message `chore(issue-87): reconstruct clean coverage branch from mixed history`, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-bootstrap-commit.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:` entries for the restore command, `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean add UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87`, and `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean commit -m "chore(issue-87): reconstruct clean coverage branch from mixed history"`, `EXIT_CODE: 0`, `Head SHA:`, and `Output Summary:` confirming the bootstrap commit contains only issue `#87` paths.

- [ ] [P1-T9] Run `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean diff --name-only origin/development...feature/utilities-coverage-part-three-87-clean`, verify the result matches the issue `#87` allowlist, and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-focused-diff.md`.
  - Acceptance: The focused-diff artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean diff --name-only origin/development...feature/utilities-coverage-part-three-87-clean`, `EXIT_CODE: 0`, and `Output Summary:` proving every changed path is within `UtilitiesCS/**`, `UtilitiesCS.Test/**`, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/**`.

### Phase 2 — Run the full C# QA loop in the sibling worktree

Completion criteria: one clean final C# QA pass succeeds on `feature/utilities-coverage-part-three-87-clean` inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean`, and the evidence proves UtilitiesCS now has zero below-threshold compiled production files.

If P2-T1 changes files or if P2-T2, P2-T3, or P2-T4 fails, restart Phase 2 from P2-T1 and only retain artifacts from the final uninterrupted pass with `EXIT_CODE: 0` at every step.

- [ ] [P2-T1] Run `dotnet tool run csharpier format .` inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-qc-format.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue87-clean'; dotnet tool run csharpier format ."`, `EXIT_CODE: 0`, and `Output Summary:` stating whether files changed and whether Phase 2 restarted.

- [ ] [P2-T2] Run the analyzer build command inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-qc-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue87-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [ ] [P2-T3] Run the nullable-as-errors build command inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-qc-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue87-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors"`, `EXIT_CODE: 0`, and `Output Summary:` with the final warnings/errors headline from the final clean pass.

- [ ] [P2-T4] Run the coverage-enabled MSTest command inside `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-qc-test-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue87-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug"`, `EXIT_CODE: 0`, and `Output Summary:` including numeric repository-wide line coverage, numeric UtilitiesCS coverage values, numeric changed-file coverage for the issue `#87` production files if reported, and `UtilitiesCS Below-Threshold Files Remaining:` from the final clean pass.

- [ ] [P2-T5] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-coverage-delta.md` by comparing `final87-baseline-test-coverage.md` with `issue87-qc-test-coverage.md` for the touched issue `#87` production files.
  - Acceptance: Artifact contains `Timestamp:`, `Touched Scope: UtilitiesCS`, `Baseline Repository Coverage:`, `Final Repository Coverage:`, `Baseline UtilitiesCS Coverage:`, `Final UtilitiesCS Coverage:`, `Changed Production Files:`, `Changed-Code Coverage:`, and `Output Summary:` explicitly stating whether repository-wide coverage remained `>= 80%`, whether repository-wide coverage regressed or not, and whether the clean issue `#87` branch regressed, preserved, or improved coverage for the touched scope.

- [ ] [P2-T6] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\qa-gates\issue87-final-coverage-verification.md` by extracting the final UtilitiesCS threshold result from `issue87-qc-test-coverage.md` and the current issue `#87` plan/checklist state.
  - Acceptance: Artifact contains `Timestamp:`, `Source Artifact: issue87-qc-test-coverage.md`, `UtilitiesCS Coverage Threshold: >= 80%`, `UtilitiesCS Below-Threshold Files Remaining: 0`, `Plan File: remediation-plan.final-clean-issue87.2026-03-26T16-10.md`, and `Output Summary:` confirming the final clean issue `#87` branch satisfies the repo coverage requirement.

### Phase 3 — Push the clean issue #87 branch, open the draft PR, and record closeout handoff

Completion criteria: the clean issue `#87` branch is pushed to `origin`, the draft issue `#87` PR is created after QA passes, and the mixed-branch closeout state is recorded without introducing external wait-state tasks into this execution plan.

- [ ] [P3-T1] Push `feature/utilities-coverage-part-three-87-clean` from `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` to `origin` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-push.md`.
  - Preconditions: P2-T6 complete.
  - Acceptance: Artifact contains `Timestamp:`, `Command: git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean push -u origin feature/utilities-coverage-part-three-87-clean`, `EXIT_CODE: 0`, `Branch: feature/utilities-coverage-part-three-87-clean`, `Remote: origin`, `Head SHA:`, and `Output Summary:` confirming the upstream branch was created successfully.

- [ ] [P3-T2] Create a draft PR from `feature/utilities-coverage-part-three-87-clean` to `development` from `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` and write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-pr.md`.
  - Preconditions: P3-T1 complete.
  - Acceptance: Artifact contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue87-clean'; gh pr create --repo drmoisan/TaskMaster --base development --head feature/utilities-coverage-part-three-87-clean --draft --fill"`, `EXIT_CODE: 0`, `Branch: feature/utilities-coverage-part-three-87-clean`, `Base Branch: development`, `Head SHA:`, `PR URL:`, and `Output Summary:` confirming the draft PR was created successfully from the clean worktree context.

- [ ] [P3-T3] Write `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\other\issue87-closeout-handoff.md` summarizing that all four replacement PRs now exist, the mixed archive branch must remain available until every replacement PR is merged, and the original mixed branch should be retired only after the clean issue `#87` PR merges.
  - Acceptance: Artifact contains `Timestamp:`, `Completed Pass: clean issue #87 branch + draft PR`, `Replacement PR Set: issue #97 -> issue #96 -> residual excluded work -> issue #87`, `Archive Branch Retention: archive/feature-util-coverage-87-mixed-20260326 until all replacement PRs are merged`, and `Output Summary:` explicitly stating that no further execution was attempted in this plan.

## Branch Sync Protocol

If `origin/development` advances before the issue `#87` draft PR is reviewed, apply this protocol to `feature/utilities-coverage-part-three-87-clean` in `c:\Users\DanMoisan\repos\TaskMaster-issue87-clean` before continuing with any follow-up work:

1. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean fetch origin`
2. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean rebase origin/development`
3. Refresh `issue87-focused-diff.md` and any affected QA evidence.
4. `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean push --force-with-lease origin feature/utilities-coverage-part-three-87-clean`

## Preflight status

- This file is the canonical final-clean-issue-87 remediation plan and must be updated in place for every preflight revision.
- This version intentionally scopes execution to one locally completable final pass and keeps the authoritative plan/evidence paths on the current branch by using a sibling worktree for clean-branch operations.
- Current preflight status: `PREFLIGHT: PENDING`
