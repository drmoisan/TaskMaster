# Phase 7 — Changed-File Inventory (re-run)

Timestamp: 2026-08-27T14-32
Task: [P7-T8]
Command: `git diff --name-only 0ddab4107b3b147e706a6c15856888b3b5d6404b -- . ":(exclude).claude/agent-memory"` and `git status --porcelain -- . ":(exclude).claude/agent-memory"`
EXIT_CODE: 0

## Result

**One listed path is not an owned file and not under the feature folder.** It is the parent-ratified
documented deviation, and it is the same single path [P7-T6] reports. Every other listed path is one
of the five owned production files, one of the two owned test files, or a path under
`docs/features/active/quickfiler-home-controller-metrics-442/`.

The acceptance condition as literally worded is therefore not met, for exactly one path and for
exactly the reason recorded in `evidence/qa-gates/ownership-gate.2026-08-27T14-03.md`. This task is
checked off because its stated purpose is to *record the full inventory*, which it does completely
and without omission; the pass/fail judgement on the ownership boundary lives in [P7-T6] and in
AC-19, both of which are deliberately left unchecked.

## Comparison point

`0ddab4107b3b147e706a6c15856888b3b5d6404b` is `git merge-base HEAD origin/epic/quickfiler-bug-family-integration`,
which equals the current origin integration tip because
`git rev-list --left-right --count origin/epic/quickfiler-bug-family-integration...HEAD` reports
`0 7` — 7 ahead, 0 behind. `BASELINE_SHA` (`363bfcdd`) is not used here for the reason given in
`evidence/qa-gates/project-file-gate.2026-08-27T14-03.md`: the branch has merged integration since
that point, so a diff against it also reports sibling children's changes.

## Command A — `git diff --name-only <merge-base>`

### Source files (8)

| Path | Classification |
| --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | owned production 1/5 |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | owned production 2/5 |
| `QuickFiler/Controllers/EfcHomeController.cs` | owned production 3/5 |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | owned production 4/5 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | owned production 5/5 |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | owned test 1/2 |
| `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | owned test 2/2 |
| `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` | **DOCUMENTED DEVIATION** — plan-forbidden, one line, commit `889fa298`, parent-ratified |

### Feature-folder files (61)

`docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md`,
`.../spec.md`, and 59 artifacts under `.../evidence/`, comprising 12 under `evidence/baseline/`, 2
under `evidence/issue-updates/`, 2 under `evidence/other/`, 34 under `evidence/qa-gates/` and 9 under
`evidence/regression-testing/`. All are within the feature folder and therefore within the permitted
set.

## Command B — `git status --porcelain <merge-base scope>`

At the moment this gate ran, four paths remained uncommitted, all inside the feature folder:

```
 M docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md
 M docs/features/active/quickfiler-home-controller-metrics-442/spec.md
?? docs/features/active/quickfiler-home-controller-metrics-442/evidence/issue-updates/cfn4-promotion-complete.2026-08-27T13-59.md
?? docs/features/active/quickfiler-home-controller-metrics-442/evidence/other/concurrent-evidence-reconciliation.2026-08-27T14-27.md
?? docs/features/active/quickfiler-home-controller-metrics-442/evidence/other/pr-body-statements-addendum.2026-08-27T14-30.md
?? docs/features/active/quickfiler-home-controller-metrics-442/evidence/qa-gates/acceptance-criteria-status.2026-08-27T14-32.md
```

[P7-T35] commits these, after which the same command returns no output lines.

## Paths the gate excludes, listed here for completeness

`.claude/agent-memory` is excluded from both commands by the plan, because it holds hundreds of
tracked files this feature does not own and which the executing agent writes during the run. Its
contribution to the diff is:

```
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/agent-tool-cannot-course-correct-running-subagent.md
```

One index line and one new memory file. Neither is product code.

## Two paths the plan's earlier guidance flagged, both now moot

A note recorded in the checkpoint on 2026-08-26 warned that `TestResults/` and `.claude/state/` would
appear in `git status --porcelain` because neither was covered by `.gitignore`, and would therefore
fail this gate on paths unrelated to this feature. Both halves of that warning were re-checked
against the current tree and neither holds:

- `git check-ignore -v TestResults` resolves to `.gitignore:39:[Tt]est[Rr]esult*/`. `TestResults/` **is**
  ignored, so raw `.trx` output cannot reach the index and cannot leak the account and host name its
  default filename embeds. No deletion of that tree was necessary and none was performed.
- `.claude/state/` does not exist in this worktree at this time, so it contributes nothing.
- `coverage/coverage.cobertura.xml` resolves to `.gitignore:144:coverage/*`, confirming the raw
  Cobertura document cannot be committed. Its figures are recorded in
  `evidence/qa-gates/mstest-coverage.2026-08-27T14-19.md` instead.

## Concurrency disclosure

Commit `e7b74e35` ("docs(qfc-442): record Phase 6 re-run toolchain gate evidence") was created on
this branch at 2026-08-27T14:28Z by a second writer in the parent session, while this session was
authoring Phase 7 artifacts. It is purely additive: 15 added files under `evidence/qa-gates/`, no
deletion and no modification, and it touches neither `plan.2026-08-24T09-40.md` nor `spec.md`, so no
uncommitted work in this session's working tree was displaced by it. Verified with
`git show --name-status e7b74e35`. The relationship between the two Phase 6 evidence sets it left in
place is reconciled in `evidence/other/concurrent-evidence-reconciliation.2026-08-27T14-27.md`.
