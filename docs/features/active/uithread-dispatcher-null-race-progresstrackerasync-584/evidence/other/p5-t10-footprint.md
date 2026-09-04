# P5-T10 — Committed footprint verification

Timestamp: 2026-09-03T22-30

Command:
```text
env -C <worktree-root> git diff --name-status 87cb4df338322844abfa580abea14df77e738e5c..HEAD
env -C <worktree-root> git status --porcelain -- UtilitiesCS UtilitiesCS.Test "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs" docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
```

EXIT_CODE:
- command 1 — 0
- command 2 — 0

Aggregate EXIT_CODE: 0

## Output Summary

### Command 1 — anchored name-status diff, verbatim

```text
M	QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs
M	UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
M	UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
M	UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
M	UtilitiesCS.Test/Threading/UiThread_Tests.cs
M	UtilitiesCS/Threading/UiThread.cs
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t10-utilitiescs-tests-coverage.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t11-quickfiler-tests.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t12-threshold-reconciliation.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t13-parallel-bucket-census.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t14-reflective-dispatcher-census.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t2-uithread-rederivation.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t3-progresstrackerasync-rederivation.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t4-test-rederivation.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t5-toolchain-resolution.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t6-mcp-probe.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t7-csharpier-check.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t8-analyzer-build.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t9-nullable-build.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/phase0-instructions-read.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/issue-updates/issue-584.2026-09-02T09-02.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p3-t4-progresstrackerasync-unmodified.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p1-t5-donotparallelize.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t2-nullforgiving-removed.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t3-file-size.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t1-analyzer-build.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t5-no-timing-tokens.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t1-format.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t2-format-check.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t3-analyzer-build.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t4-nullable-build.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t5-utilitiescs-tests.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t7-coverage-delta.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t8-loop-closure.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t3-build-before-fix.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t4-expect-fail.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t2-regression-green.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t3-at-risk-tests.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p4-t6-first-pass-failure.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/issue.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/research/defect-scoping.2026-09-02T09-02.md
A	docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md
```

### Command 2 — scoped porcelain status, verbatim

```text
 M docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md
?? docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p5-t10-footprint.md
```

## Acceptance

1. **Source paths.** The anchored name-status diff lists exactly these six source paths and no other
   source path, apart from paths under
   `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/`:

   - `UtilitiesCS/Threading/UiThread.cs` (M)
   - `UtilitiesCS.Test/Threading/UiThread_Tests.cs` (M)
   - `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` (M)
   - `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` (M)
   - `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` (M)
   - `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` (M)

   Every other entry in the diff is under the feature folder. Those entries appear with status `A`
   because the feature folder was untracked at BASE and was committed for the first time by P5-T9.

2. **Baseline format drift set.** The diff lists no path in the `BASELINE_FORMAT_DRIFT_SET` recorded
   in P0-T7. That set is `NONE`, so the clause is satisfied by an empty set, and independently by the
   fact that P4-T1's formatter write scope was restricted to the six owned paths and therefore
   rewrote no unowned path at all.

3. **Gated paths.** The diff lists no path under `.claude/`, `.codex/`, `.agents/`,
   `config/blast-radius.json`, or `config/orchestration-routing.json`. This was checked by filtering
   the diff output for those prefixes; the filter matched nothing and exited 1. Five untracked files
   under `.claude/agent-memory/` exist in this worktree from earlier delegations; they remain
   untracked and uncommitted, and they are outside this command's scope.

4. **Porcelain.** The scoped porcelain output lists
   `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md`,
   modified by this plan's own check-off of P5-T9, and no path outside
   `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/`. P5-T11 commits
   both the plan file and this artifact together.

The two-dot form is correct and not vacuous here: P5-T9 has already committed, so `HEAD` is no longer
identical to BASE. Commit SHA created by P5-T9: `a88a0b0c0e70d8ba59d15e2c03a97324fb8d95e3`.
