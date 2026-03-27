---
title: "Remediation Plan: 2026-03-25-getmovediagnostics-null-guard-97 (2026-03-25T15-12)"
issue: "#97"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-25T15-12"
status: "Planned"
status_color: "blue"
version: "2.0"
work_mode: "minor-audit"
requirements_source: "docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/remediation-inputs.2026-03-25T15-12.md"
secondary_context: "docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md"
base_ref: "origin/feature/utilities-coverage-part-three-87"
---

# Remediation Plan: 2026-03-25-getmovediagnostics-null-guard-97 (2026-03-25T15-12)

## Overview

**Status Badge:** [Planned | blue]

This remediation removes unrelated tooling content from the `#97` diff, repairs the corrected PR-context bundle so the summary matches the appendix, synchronizes `plan.2026-03-25T12-00.md` with canonical QA evidence, and refreshes the `2026-03-25T15-12` audit set. The scope is limited to PR-readiness blockers for issue `#97`; the six issue acceptance criteria in `issue.md` are already satisfied and must remain unchanged.

## Scope Guardrails

- **CON-1:** Treat `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/remediation-inputs.2026-03-25T15-12.md` as the authoritative requirements source; use `issue.md` only as secondary context.
- **CON-2:** Use `origin/feature/utilities-coverage-part-three-87` as the only upstream comparison for remediation work.
- **CON-3:** Keep remediation limited to review blockers for issue `#97`; do not expand into unrelated feature, tooling, or branch-history work.
- **CON-4:** Do not mark any checklist item complete without schema-valid evidence on disk containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` when a command is involved.
- **CON-5:** Baseline-sync and final-sync must both update `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md` in place.
- **CON-6:** The final corrected diff must not include `.codex/*` files or `.github/skills.zip`.

## Requirements Traceability

| REQ | Source | Required outcome | Implementation tasks | Validation tasks |
|---|---|---|---|---|
| REQ-1 | remediation-inputs §1 | Remove or split unrelated `.codex/*` and `.github/skills.zip` changes from the corrected `#97` diff | P1-T1 | P0-T2, P1-T2, P1-T5, P3-T2 |
| REQ-2 | remediation-inputs §2 | Regenerate `artifacts/pr_context.summary.txt` so it matches `artifacts/pr_context.appendix.txt` for the corrected upstream base | P1-T2, P1-T3 | P0-T3, P3-T1 |
| REQ-3 | remediation-inputs §3 | Synchronize `plan.2026-03-25T12-00.md`, active-plan filename references, and canonical Phase 2 QA artifacts | P0-T4, P0-T5, P1-T4, P2-T1, P2-T2, P2-T3, P2-T4, P2-T5, P2-T6 | P2-T6, P3-T2 |
| REQ-4 | remediation-inputs §4 | Re-run the feature review after scope and evidence cleanup so the review artifacts stop reporting the three current blockers | P3-T1 | P1-T5, P2-T6, P3-T2 |

## Acceptance Criteria

- REQ-1: `pwsh -NoProfile -Command "$mergeBase = git merge-base origin/feature/utilities-coverage-part-three-87 HEAD; git diff --name-status $mergeBase HEAD"` exits with code `0`, the output lists no `.codex/agents/atomic-executor.toml`, `.codex/agents/atomic-planner.toml`, `.codex/agents/feature-reviewer.toml`, `.codex/prompts/feature-review-remediate.md`, `.codex/skills/atomic-executor/SKILL.md`, `.codex/skills/atomic-planner/SKILL.md`, `.codex/skills/feature-review/SKILL.md`, or `.github/skills.zip`, and `artifacts/pr_context.appendix.txt` also lists none of those paths.
- REQ-2: `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` both name `origin/feature/utilities-coverage-part-three-87` as the requested base, `artifacts/pr_context.summary.txt` does not contain `Core logic changes: 0 files`, and the summary changed-file overview lists `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, and `QuickFiler.Test/QuickFiler.Test.csproj`.
- REQ-3: `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md` shows `[x]` for `P2-T1` through `P2-T6` only after `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-format.md`, `qc-lint.md`, `qc-nullable.md`, `qc-regression-tests.md`, and `qc-coverage.md` exist with schema-valid command evidence, and no `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.md` file remains.
- REQ-4: `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/policy-audit.2026-03-25T15-12.md`, `code-review.2026-03-25T15-12.md`, and `feature-audit.2026-03-25T15-12.md` no longer contain `.codex/*`, `.github/skills.zip`, summary/appendix mismatch findings, missing QA-artifact findings, `NEEDS REVISION`, or `No-Go`, and the full C# QA evidence set (`qc-format.md`, `qc-lint.md`, `qc-nullable.md`, `qc-regression-tests.md`, `qc-coverage.md`) records `EXIT_CODE: 0`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Context & Remediation Baseline

Completion criteria: remediation-baseline artifacts exist under `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/remediation-baseline/`, and `plan.2026-03-25T12-00.md` is synchronized to the current canonical evidence state before cleanup begins.

- [ ] [P0-T1] Read `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/remediation-inputs.2026-03-25T15-12.md`, `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md`, `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md`, `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/policy-audit.2026-03-25T15-12.md`, `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/code-review.2026-03-25T15-12.md`, `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/feature-audit.2026-03-25T15-12.md`, `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, and `.github/instructions/csharp-unit-test.instructions.md`, then write `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/remediation-baseline/remediation-phase0-instructions-read.2026-03-25T15-12.md`.
	- Acceptance: the artifact exists and contains `Timestamp:`, `Policy Order:`, and the exact path list of all files named in this task.

- [ ] [P0-T2] Capture the current corrected diff baseline by running `pwsh -NoProfile -Command "$mergeBase = git merge-base origin/feature/utilities-coverage-part-three-87 HEAD; git diff --name-status $mergeBase HEAD"` and write `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/remediation-baseline/remediation-diff-scope.2026-03-25T15-12.md`.
	- Acceptance: the artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -Command "$mergeBase = git merge-base origin/feature/utilities-coverage-part-three-87 HEAD; git diff --name-status $mergeBase HEAD"`, `EXIT_CODE: 0`, `Output Summary:`, and the current name-status list showing the unrelated `.codex/*` and `.github/skills.zip` paths.

- [ ] [P0-T3] Capture the current PR-context mismatch baseline by extracting the changed-file overview from `artifacts/pr_context.summary.txt` and the changed-files section from `artifacts/pr_context.appendix.txt` into `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/remediation-baseline/remediation-pr-context-mismatch.2026-03-25T15-12.md`.
	- Acceptance: the artifact exists and its `Output Summary:` includes the exact stale summary line `Core logic changes: 0 files` plus the appendix-listed `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, and `QuickFiler.Test/QuickFiler.Test.csproj` paths.

- [ ] [P0-T4] Baseline-sync `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md` to the current canonical evidence state without widening the issue `#97` scope.
	- Acceptance: `plan.2026-03-25T12-00.md` still lists `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md` as its requirements source, still shows `[x]` for `P2-T1` and `P2-T2` because `evidence/qa-gates/qc-format.md` and `evidence/qa-gates/qc-lint.md` exist, and still shows `[ ]` for `P2-T3` through `P2-T6` until `qc-nullable.md`, `qc-regression-tests.md`, `qc-coverage.md`, and the final synchronization update exist.

- [ ] [P0-T5] Record the active-plan filename baseline in `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/remediation-baseline/remediation-plan-filename.2026-03-25T15-12.md` by auditing `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md`, any sibling `plan.md`, and both PR-context artifacts.
	- Acceptance: the artifact exists and its `Output Summary:` states whether `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.md` exists, whether either PR-context artifact still references `plan.md`, and that `plan.2026-03-25T12-00.md` is the canonical active plan.

### Phase 1 — Diff Scope Cleanup & PR-Context Repair

Completion criteria: the corrected diff excludes unrelated tooling files, `artifacts/pr_context.appendix.txt` and `artifacts/pr_context.summary.txt` both reflect `origin/feature/utilities-coverage-part-three-87`, and the active plan filename is canonicalized.

- [ ] [P1-T1] Remove `.codex/agents/atomic-executor.toml`, `.codex/agents/atomic-planner.toml`, `.codex/agents/feature-reviewer.toml`, `.codex/prompts/feature-review-remediate.md`, `.codex/skills/atomic-executor/SKILL.md`, `.codex/skills/atomic-planner/SKILL.md`, `.codex/skills/feature-review/SKILL.md`, and `.github/skills.zip` from the `pwsh -NoProfile -Command "$mergeBase = git merge-base origin/feature/utilities-coverage-part-three-87 HEAD; git diff --name-status $mergeBase HEAD"` result for branch `getmovediagnostics-null-guard-97`.
	- Acceptance: rerunning `pwsh -NoProfile -Command "$mergeBase = git merge-base origin/feature/utilities-coverage-part-three-87 HEAD; git diff --name-status $mergeBase HEAD"` returns `EXIT_CODE: 0` and the output contains none of those eight paths.

- [ ] [P1-T2] Refresh `artifacts/pr_context.appendix.txt` against base `origin/feature/utilities-coverage-part-three-87` by invoking the repo PR-context collector through the VS Code command `drmCopilotExtension.collectPrContext --base origin/feature/utilities-coverage-part-three-87` after [P1-T1] completes.
	- Dependencies: [P1-T1]
	- Acceptance: `artifacts/pr_context.appendix.txt` contains `Base ref (requested): origin/feature/utilities-coverage-part-three-87`, contains `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, and `QuickFiler.Test/QuickFiler.Test.csproj`, and contains none of the removed `.codex/*` or `.github/skills.zip` paths.

- [ ] [P1-T3] Repair `artifacts/pr_context.summary.txt` so its changed-file overview matches the refreshed appendix from [P1-T2].
	- Dependencies: [P1-T2]
	- Acceptance: `artifacts/pr_context.summary.txt` does not contain `Core logic changes: 0 files`, and the `Changed files overview` section lists `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, and `QuickFiler.Test/QuickFiler.Test.csproj`.

- [ ] [P1-T4] Canonicalize the active-plan filename reference to `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md` across the feature folder and both PR-context artifacts.
	- Dependencies: [P1-T2], [P1-T3]
	- Acceptance: `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` reference `plan.2026-03-25T12-00.md`, neither file references `plan.md`, and no `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.md` file remains in the feature folder.

- [ ] [P1-T5] Write `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/other/remediation-focused-diff.2026-03-25T15-12.md` summarizing the cleaned branch scope after [P1-T1] through [P1-T4].
	- Dependencies: [P1-T1], [P1-T2], [P1-T3], [P1-T4]
	- Acceptance: the artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -Command "$mergeBase = git merge-base origin/feature/utilities-coverage-part-three-87 HEAD; git diff --name-status $mergeBase HEAD"`, `EXIT_CODE: 0`, and `Output Summary:` stating that the diff contains only `QuickFiler/`, `QuickFiler.Test/`, and `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/` paths.

### Phase 2 — Final C# QA Evidence & Original Plan Final Sync

Completion criteria: the canonical QA artifacts under `evidence/qa-gates/` are complete and schema-valid, and `plan.2026-03-25T12-00.md` matches them exactly.

- [ ] [P2-T1] Refresh `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-format.md` by running `dotnet tool run csharpier format .`.
	- Acceptance: `qc-format.md` exists and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:`.

- [ ] [P2-T2] Refresh `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-lint.md` by running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`.
	- Dependencies: [P2-T1]
	- Acceptance: `qc-lint.md` exists and contains the exact command above, `EXIT_CODE: 0`, and `Output Summary:` noting whether any warnings remain pre-existing only.

- [ ] [P2-T3] Create `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-nullable.md` by running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`.
	- Dependencies: [P2-T2]
	- Acceptance: `qc-nullable.md` exists and contains the exact command above, `EXIT_CODE: 0`, and `Output Summary:` describing the nullable/type-check result.

- [ ] [P2-T4] Create `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-regression-tests.md` by rerunning the two targeted issue `#97` regression commands below and recording both results in the same artifact.
	- `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerTests"`
	- `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~GetCalendarReturnsNull"`
	- Dependencies: [P2-T3]
	- Acceptance: `qc-regression-tests.md` exists, records both exact commands, records `EXIT_CODE: 0` for each command, and its `Output Summary:` states that `GetMoveDiagnostics_WhenAppointmentIsNull_DoesNotThrow` and `QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow` passed.

- [ ] [P2-T5] Create `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-coverage.md` by running `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`.
	- Dependencies: [P2-T4]
	- Acceptance: `qc-coverage.md` exists and contains the exact command above, `EXIT_CODE: 0`, and `Output Summary:` with numeric `QuickFiler.Test.dll` and `QuickFiler.dll` line-coverage values plus the saved coverage file path.

- [ ] [P2-T6] Final-sync `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md` and `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md` to the canonical QA evidence set from [P2-T1] through [P2-T5].
	- Dependencies: [P2-T1], [P2-T2], [P2-T3], [P2-T4], [P2-T5]
	- Acceptance: `plan.2026-03-25T12-00.md` shows `[x]` for `P2-T1` through `P2-T6`, `issue.md` still shows all six issue acceptance criteria checked, and each newly checked plan item maps to an existing schema-valid artifact (`qc-format.md`, `qc-lint.md`, `qc-nullable.md`, `qc-regression-tests.md`, `qc-coverage.md`) or the synchronized acceptance-criteria state.

### Phase 3 — Review Refresh & Remediation Closeout

Completion criteria: the refreshed review artifacts no longer report the three current blockers, and the remediation end-state artifact records the cleaned diff, synchronized plan, and complete evidence set.

- [ ] [P3-T1] Refresh `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/policy-audit.2026-03-25T15-12.md`, `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/code-review.2026-03-25T15-12.md`, and `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/feature-audit.2026-03-25T15-12.md` against base `origin/feature/utilities-coverage-part-three-87` after [P1-T5] and [P2-T6] complete.
	- Dependencies: [P1-T5], [P2-T6]
	- Acceptance: the refreshed audit files contain no references to `.codex/*` or `.github/skills.zip` in the `#97` diff, no references to a summary/appendix mismatch, no references to missing `qc-nullable.md`, `qc-regression-tests.md`, or `qc-coverage.md`, and their conclusion sections no longer contain `NEEDS REVISION` or `No-Go`.

- [ ] [P3-T2] Write `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/other/remediation-end-state.2026-03-25T15-12.md` summarizing the final remediation outcome.
	- Dependencies: [P1-T5], [P2-T6], [P3-T1]
	- Acceptance: the artifact exists and contains `Timestamp:`, `Focused Diff Artifact: docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/other/remediation-focused-diff.2026-03-25T15-12.md`, `QA Artifact Set:`, `Refreshed Audit Set:`, and `Output Summary:` stating that issue `#97` is limited to review blockers only and is ready for PR review against `origin/feature/utilities-coverage-part-three-87`.
