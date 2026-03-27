---
title: "Remediation Plan: 2026-03-27-quickfiler-navigation-key-collision-111 (2026-03-27T13-11)"
issue: "#111"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-27T13-11"
status: "Planned"
status_color: "blue"
version: "2.0"
work_mode: "minor-audit"
requirements_source: "docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/remediation-inputs.2026-03-27T13-11.md"
secondary_context: "docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md"
base_ref: "main"
---

# Remediation Plan: 2026-03-27-quickfiler-navigation-key-collision-111 (2026-03-27T13-11)

## Overview

**Status Badge:** [Planned | blue]

This remediation restores reviewability for issue `#111` by aligning the branch diff to the intended QuickFiler duplicate-key scope relative to `main`, replacing placeholder-only `minor-audit` requirements in `issue.md` with concrete acceptance criteria, repairing plan items whose checked state is not evidence-backed, and then re-running the repository C# QA loop followed by a fresh feature review.

## Scope Guardrails

- **CON-1:** Treat `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/remediation-inputs.2026-03-27T13-11.md` as the authoritative remediation requirements source.
- **CON-2:** Use `main` as the only comparison base for branch-scope validation.
- **CON-3:** Keep production edits limited to the intended issue `#111` QuickFiler duplicate-key fix scope.
- **CON-4:** Do not add `spec.md` or `user-story.md`; this remains a `minor-audit` feature.
- **CON-5:** Do not mark any plan task complete without schema-valid evidence on disk when a command is involved.
- **CON-6:** Reuse `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/plan.2026-03-27T12-45.md` as the canonical feature plan for baseline-sync and final-sync tasks.

## Requirements Traceability

| REQ | Source | Required outcome | Implementation tasks | Validation tasks |
|---|---|---|---|---|
| REQ-1 | remediation-inputs §1 | Branch diff relative to `main` contains the intended QuickFiler duplicate-key fix and no unrelated dominant payload | P1-T1, P1-T2 | P0-T2, P3-T1 |
| REQ-2 | remediation-inputs §2 | `issue.md` becomes a concrete, authoritative `minor-audit` requirements source with explicit acceptance criteria | P1-T3 | P0-T3, P3-T1 |
| REQ-3 | remediation-inputs §3 | `plan.2026-03-27T12-45.md` checklist is synchronized to real evidence only | P0-T4, P1-T4, P2-T5 | P3-T1 |
| REQ-4 | remediation-inputs §4 | QA and review artifacts are rerun against the corrected branch state | P2-T1, P2-T2, P2-T3, P2-T4, P3-T1 | P3-T1, P3-T2 |

## Acceptance Criteria

- REQ-1: `git diff --name-status main...HEAD` exits with code `0`, includes `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler.Test/Controllers/KbdActionsTests.cs`, and the active feature-folder artifacts for issue `#111`, and no longer shows unrelated `QfcQueue` work as the effective branch payload.
- REQ-2: `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md` contains no placeholder-only bug sections and includes explicit checkbox acceptance criteria for the duplicate-key scenario.
- REQ-3: `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/plan.2026-03-27T12-45.md` shows checked tasks only where corresponding schema-valid evidence exists and the evidence satisfies the literal task acceptance.
- REQ-4: The refreshed `policy-audit`, `code-review`, and `feature-audit` artifacts for issue `#111` do not report branch-scope mismatch, placeholder-only minor-audit requirements, or non-evidence-backed checked plan items, and the full C# QA loop records passing results.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Remediation Baseline and Plan Synchronization

Completion criteria: the remediation baseline is documented and the canonical feature plan is synchronized to the current truth before corrective work begins.

- [ ] [P0-T1] Read `remediation-inputs.2026-03-27T13-11.md`, `issue.md`, `plan.2026-03-27T12-45.md`, `policy-audit.2026-03-27T13-11.md`, `code-review.2026-03-27T13-11.md`, `feature-audit.2026-03-27T13-11.md`, `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, and `.github/instructions/csharp-unit-test.instructions.md`, then write a remediation baseline evidence artifact under `evidence/remediation-baseline/`.
  - Acceptance: the artifact lists the exact files read and includes `Timestamp:` and `Policy Order:` fields.

- [ ] [P0-T2] Capture the current branch-scope baseline by running `git log --date=short --pretty=format:'%h %ad %an %s' main..HEAD` and `git diff --name-status main...HEAD`, then write the result to `evidence/remediation-baseline/remediation-branch-scope.2026-03-27T13-11.md`.
  - Acceptance: the artifact contains both exact commands, `EXIT_CODE: 0`, and an `Output Summary:` showing that unrelated `#106` / archive work currently dominates the range.

- [ ] [P0-T3] Capture the current requirements-source baseline by auditing `issue.md` for placeholder sections and missing acceptance criteria, and write the result to `evidence/remediation-baseline/remediation-issue-source.2026-03-27T13-11.md`.
  - Acceptance: the artifact identifies each remaining placeholder section and explicitly states that `issue.md` is the sole source because `Work Mode: minor-audit` is present.

- [ ] [P0-T4] Baseline-sync `plan.2026-03-27T12-45.md` so any checked item that is not currently evidence-backed is marked pending until corrected.
  - Acceptance: after the sync, the plan truthfully reflects the current state of `P0-T3`, `P1-T2`, and any other task whose evidence does not satisfy the literal acceptance.

### Phase 1 — Restore Correct Branch Scope and Requirements Source

Completion criteria: the branch diff relative to `main` reflects issue `#111`, and `issue.md` becomes a valid minor-audit requirements source.

- [ ] [P1-T1] Remove or relocate unrelated `QfcQueue` / archive-only payload from the effective `main...HEAD` range for `bug/quickfiler-navigation-key-collision-111`, or recreate the branch from the correct base so the duplicate-key fix is the reviewed payload.
  - Acceptance: `git log --date=short --pretty=format:'%h %ad %an %s' main..HEAD` and `git diff --name-status main...HEAD` no longer present issue `#106` as the dominant branch content.

- [ ] [P1-T2] Ensure the intended QuickFiler duplicate-key implementation is present in the branch relative to `main`, limited to `QuickFiler/Controllers/KbdActions.cs`, optional `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/Controllers/KbdActionsTests.cs`, and matching feature docs.
  - Dependencies: [P1-T1]
  - Acceptance: scoped diff commands for those files are non-empty and correspond to the issue `#111` bug description.

- [ ] [P1-T3] Replace placeholder content in `issue.md` with the actual issue `#111` summary, reproduction, expected/actual behavior, and explicit acceptance-criteria checkbox items for the duplicate-key scenario.
  - Dependencies: [P1-T1]
  - Acceptance: `issue.md` contains no placeholder bug sections and remains the only scoping document in the feature folder.

- [ ] [P1-T4] Rebuild the fail-before and baseline evidence chain for any corrected plan items whose previous evidence was not literal-task compliant, including formatter baseline and focused regression evidence.
  - Dependencies: [P1-T2], [P1-T3]
  - Acceptance: new or corrected evidence artifacts satisfy the exact task acceptance for the synced plan items.

### Phase 2 — Final C# QA Loop and Feature-Plan Final Sync

Completion criteria: the corrected branch state passes the full repository C# QA loop, and the canonical feature plan matches the evidence set exactly.

- [ ] [P2-T1] Run `dotnet tool run csharpier check .` and save the clean-pass result under `evidence/qa-gates/`.
  - Acceptance: the artifact records `EXIT_CODE: 0` and confirms no tracked file changes were introduced.

- [ ] [P2-T2] Run `Invoke-VSBuild.ps1` with `-EnableNETAnalyzers -EnforceCodeStyleInBuild` and save the clean-pass result under `evidence/qa-gates/`.
  - Dependencies: [P2-T1]
  - Acceptance: the artifact records the exact command and `EXIT_CODE: 0`.

- [ ] [P2-T3] Run `Invoke-VSBuild.ps1` with `-EnableNullable -TreatWarningsAsErrors` and save the clean-pass result under `evidence/qa-gates/`.
  - Dependencies: [P2-T2]
  - Acceptance: the artifact records the exact command and `EXIT_CODE: 0`.

- [ ] [P2-T4] Run `Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and save the clean-pass result under `evidence/qa-gates/`.
  - Dependencies: [P2-T3]
  - Acceptance: the artifact records total tests, pass/fail counts, coverage headline values, and `EXIT_CODE: 0`.

- [ ] [P2-T5] Final-sync `plan.2026-03-27T12-45.md` and `issue.md` so checked tasks and acceptance-criteria states match the corrected evidence set exactly.
  - Dependencies: [P1-T4], [P2-T1], [P2-T2], [P2-T3], [P2-T4]
  - Acceptance: no checked plan item lacks backing evidence, and only genuinely delivered issue acceptance criteria are checked in `issue.md`.

### Phase 3 — Refresh Review Artifacts and Close Remediation

Completion criteria: refreshed review artifacts no longer report the current blockers, and remediation closeout evidence is recorded.

- [ ] [P3-T1] Refresh `policy-audit`, `code-review`, and `feature-audit` for issue `#111` against `main` after Phases 1 and 2 complete.
  - Dependencies: [P1-T2], [P1-T3], [P1-T4], [P2-T5]
  - Acceptance: the refreshed artifacts no longer report branch-scope mismatch, placeholder-only requirements, or non-evidence-backed checked plan items.

- [ ] [P3-T2] Write a remediation end-state artifact under `evidence/other/` summarizing the corrected diff, corrected requirements source, QA evidence set, and refreshed review outcome.
  - Dependencies: [P3-T1]
  - Acceptance: the artifact lists the corrected evidence paths and states whether issue `#111` is ready for PR review against `main`.

## Note on plan creation

This remediation plan was created directly in the required path so the active feature folder contains the mandatory artifact. In a fully provisioned review environment, the same file should also be regenerated and validated via the repository's `atomic_planner` handoff workflow.