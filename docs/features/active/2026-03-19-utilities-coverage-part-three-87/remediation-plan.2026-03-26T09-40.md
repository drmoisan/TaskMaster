---
title: "Remediation Plan: 2026-03-19-utilities-coverage-part-three-87 (2026-03-26T09-40)"
issue: "#87"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-26T09-40"
status: "Planned"
status_color: "blue"
version: "1.0"
work_mode: "full-feature"
requirements_source: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-inputs.2026-03-26T09-40.md"
secondary_context: "docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md"
base_ref: "origin/development"
---

# Remediation Plan: 2026-03-19-utilities-coverage-part-three-87 (2026-03-26T09-40)

## Overview

**Status Badge:** [Planned | blue]

This remediation restores PR readiness for issue `#87` by (1) isolating the branch diff relative to `development`, (2) finishing the remaining `UtilitiesCS` coverage work until the documented `>= 80%` threshold is achieved, (3) synchronizing feature docs to the true completion state, and (4) refreshing the review artifacts. This plan was created as a fallback because the repo template path and dedicated `atomic_planner` delegation command were not available through the current tool surface.

## Scope Guardrails

- **CON-1:** Treat `remediation-inputs.2026-03-26T09-40.md` as the authoritative requirements source.
- **CON-2:** Use `origin/development` as the only comparison base for remediation work.
- **CON-3:** Keep remediation limited to issue `#87` PR-readiness blockers; do not widen scope into unrelated QuickFiler, `.codex`, or other feature folders.
- **CON-4:** Do not mark any acceptance or completion item satisfied without evidence on disk containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` when a command is involved.
- **CON-5:** Keep the existing feature goal intact: `UtilitiesCS` must reach `>= 80%` coverage or remaining exceptions must be explicitly documented as skip candidates.

## Requirements Traceability

| REQ | Source | Required outcome | Implementation tasks | Validation tasks |
|---|---|---|---|---|
| REQ-1 | remediation-inputs §1 | Clean branch diff relative to `development` | P1-T1, P1-T2 | P0-T2, P1-T3, P3-T1 |
| REQ-2 | remediation-inputs §2 | Raise `UtilitiesCS` coverage to `>= 80%` and remove remaining non-skip below-threshold files | P2-T1, P2-T2, P2-T3 | P2-T4, P3-T1 |
| REQ-3 | remediation-inputs §3 | Synchronize feature docs to actual completion state | P3-T1 | P3-T2 |
| REQ-4 | remediation-inputs §4 | Refresh review artifacts after cleanup | P3-T2 | P3-T3 |

## Acceptance Criteria

- REQ-1: `artifacts/pr_context.appendix.txt` refreshed against `origin/development` shows only `UtilitiesCS`, `UtilitiesCS.Test`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/` scope.
- REQ-2: coverage evidence records `UtilitiesCS line coverage >= 80%` and `Per-file >=80% result: PASS` for non-skip compiled `UtilitiesCS` files.
- REQ-3: `spec.md`, `user-story.md`, and `plan.2026-03-22T21-00.md` reflect the true completion state with no premature completion claims.
- REQ-4: refreshed `policy-audit`, `code-review`, and `feature-audit` artifacts conclude PASS / ready for PR review.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline capture for remediation

Completion criteria: remediation baseline artifacts document the current oversized diff and the current failed coverage state.

- [ ] [P0-T1] Read `remediation-inputs.2026-03-26T09-40.md`, `spec.md`, `user-story.md`, `plan.2026-03-22T21-00.md`, `policy-audit.2026-03-26T09-40.md`, `code-review.2026-03-26T09-40.md`, and `feature-audit.2026-03-26T09-40.md`, then write a policy-read baseline artifact under `evidence/remediation-baseline/`.
- [ ] [P0-T2] Capture the current diff scope against `origin/development` and save it under `evidence/remediation-baseline/`.
- [ ] [P0-T3] Capture the current failed coverage baseline (`69.81%`) and remaining-below-threshold evidence under `evidence/remediation-baseline/`.

### Phase 1 — Branch isolation cleanup

Completion criteria: the branch diff no longer includes unrelated `.codex`, `.github`, `QuickFiler`, `TaskMaster`, or other feature-folder changes.

- [ ] [P1-T1] Remove or relocate all unrelated paths from the `development...HEAD` diff so only issue `#87` files remain.
- [ ] [P1-T2] Refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` against `origin/development` after cleanup.
- [ ] [P1-T3] Verify that the refreshed appendix contains no unrelated paths and write a focused-diff evidence artifact.

### Phase 2 — Complete the remaining coverage work

Completion criteria: `UtilitiesCS` coverage reaches `>= 80%`, and no remaining non-skip compiled file is below threshold.

- [ ] [P2-T1] Reconcile the remaining non-skip below-threshold files from the latest coverage verification to explicit implementation or skip-justification tasks.
- [ ] [P2-T2] Add or extend deterministic MSTest coverage for the remaining implementation-routed files in `UtilitiesCS.Test`, keeping explicit `Compile Include` registration current.
- [ ] [P2-T3] For any file that truly cannot meet the threshold under repo policy, update skip-candidate evidence with concrete rationale before the final coverage run.
- [ ] [P2-T4] Run the full C# QA loop in order: formatter, analyzer build, nullable build, coverage-enabled MSTest. Save refreshed artifacts under `evidence/qa-gates/`.

### Phase 3 — Docs sync and review refresh

Completion criteria: feature docs accurately reflect completion, and refreshed review artifacts pass.

- [ ] [P3-T1] Synchronize `spec.md`, `user-story.md`, and `plan.2026-03-22T21-00.md` to the successful end state after Phase 2.
- [ ] [P3-T2] Refresh `policy-audit`, `code-review`, and `feature-audit` in the feature folder using the cleaned branch and successful coverage evidence.
- [ ] [P3-T3] Write a remediation end-state artifact summarizing the focused diff, final coverage numbers, and refreshed PASS review set.

## Preflight status

- Dedicated `atomic_planner` / `atomic_executor` delegation was **not available** in the current tool environment.
- This file is a manual fallback plan aligned to the repository’s atomic-plan prompt structure.
- PREFLIGHT status could not be delegated automatically in this session.