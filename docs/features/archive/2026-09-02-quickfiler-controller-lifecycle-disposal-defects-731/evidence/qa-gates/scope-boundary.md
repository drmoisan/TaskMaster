# Scope boundary gate

Timestamp: 2026-09-03T14-42

Task: [P5-T9]
Issue: #731

## Command

```
git add -N QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs
git diff --name-status 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e
git status --porcelain --untracked-files=all
```

The `<DIFF-BASE>` operand is the 40-character SHA recorded on the `Diff base:` line of `EVIDENCE/baseline/tree-invariants.md` by `[P0-T2]`, substituted verbatim: `35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`. It is byte-identical to that recorded value. The literal ref `origin/main` was not used, because `HEAD` is a merge commit whose second parent is that remote ref and a mid-run advance of it would silently change what this gate measures.

The `git add -N` span is required because an anchored name-status diff cannot report the three untracked test files this plan creates. The porcelain span is required because the anchored diff goes empty once the change is committed. The two are complementary and each alone is blind in one state.

EXIT_CODE: 0

## Output Summary

### `git diff --name-status <DIFF-BASE>`, full output

```
M	.claude/agent-memory/atomic-executor/MEMORY.md
M	.claude/agent-memory/atomic-executor/project_agent_memory_tracked_breaks_unscoped_git_gates.md
M	.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md
A	.claude/agent-memory/atomic-executor/project_scope_gate_cannot_list_artifacts_written_after_it.md
M	.claude/agent-memory/atomic-planner/MEMORY.md
M	.claude/agent-memory/atomic-planner/agent-memory-is-tracked-scope-git-gates.md
A	.claude/agent-memory/atomic-planner/porcelain-collapses-untracked-directories.md
A	.claude/agent-memory/atomic-planner/project_731_lifecycle_disposal_plan_seams.md
A	.claude/agent-memory/atomic-planner/repo-wide-cobertura-line-rate-is-nondeterministic.md
A	.claude/agent-memory/atomic-planner/self-referential-evidence-enumeration.md
M	.claude/agent-memory/task-researcher/MEMORY.md
A	.claude/agent-memory/task-researcher/project_qfc_lifecycle_disposal_731.md
A	QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs
M	QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
M	QuickFiler.Test/Controllers/QfcDatamodelTests.cs
A	QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
A	QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs
M	QuickFiler.Test/QuickFiler.Test.csproj
M	QuickFiler/Controllers/QfcCollectionController.cs
M	QuickFiler/Controllers/QfcDatamodel.cs
M	QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
M	QuickFiler/Controllers/QfcQueue.cs
M	QuickFiler/Controllers/QfcRemainingQueueAdmission.cs
M	QuickFiler/Helper Classes/EmailMoveMonitor.cs
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/issue.md
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/research/2026-09-02T13-10-controller-lifecycle-disposal-fix-design-research.md
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/spec.md
```

28 paths in total.

### Direct assertions on the name-status list

- Paths whose filename ends in `Metrics.cs`: **0**. The two QuickFiler Controllers metrics files owned by the sibling parallel work item are untouched.
- Paths under `docs/features/potential/`: **0**.
- `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs`: **not listed at all**. The frozen file is unmodified, as `[P2-T6]` independently confirmed.

### Subtraction arithmetic

**Removed set 1 — AGENT-MEMORY ALLOWANCE.** Every path under `.claude/agent-memory/` is removed, whether or not it was present when `[P0-T2]` ran. 12 paths removed:

```
.claude/agent-memory/atomic-executor/MEMORY.md
.claude/agent-memory/atomic-executor/project_agent_memory_tracked_breaks_unscoped_git_gates.md
.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md
.claude/agent-memory/atomic-executor/project_scope_gate_cannot_list_artifacts_written_after_it.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/agent-memory-is-tracked-scope-git-gates.md
.claude/agent-memory/atomic-planner/porcelain-collapses-untracked-directories.md
.claude/agent-memory/atomic-planner/project_731_lifecycle_disposal_plan_seams.md
.claude/agent-memory/atomic-planner/repo-wide-cobertura-line-rate-is-nondeterministic.md
.claude/agent-memory/atomic-planner/self-referential-evidence-enumeration.md
.claude/agent-memory/task-researcher/MEMORY.md
.claude/agent-memory/task-researcher/project_qfc_lifecycle_disposal_731.md
```

No path under `.claude/agent-memory/` is in scope for issue #731, and no gate in this plan asserts anything about one.

**Removed set 2 — PRE-EXISTING TRACKED DIFF SET.** Every path recorded under the `Pre-existing tracked diff paths` heading by `[P0-T2]` is removed. That recorded set has 16 members; its 12 `.claude/agent-memory/` members were already removed by set 1, so this step removes the remaining 4:

```
docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/issue.md
docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md
docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/research/2026-09-02T13-10-controller-lifecycle-disposal-fix-design-research.md
docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/spec.md
```

Subtracting the recorded set rather than a set quoted in the plan is deliberate: `spec.md` and the plan file are tracked and are modified further by the Phase 6 check-offs, so a hard-coded expectation would go stale the moment the branch gains another commit.

**Arithmetic:** 28 listed − 12 agent-memory − 4 pre-existing feature-folder = **12 residual**.

**Residual, which must be exactly the eleven source paths this plan writes plus the project file:**

| # | Path | Status |
|---|---|---|
| 1 | `QuickFiler/Controllers/QfcCollectionController.cs` | M |
| 2 | `QuickFiler/Controllers/QfcDatamodel.cs` | M |
| 3 | `QuickFiler/Controllers/QfcQueue.cs` | M |
| 4 | `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | M |
| 5 | `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` | M |
| 6 | `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | M |
| 7 | `QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs` | A |
| 8 | `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | A |
| 9 | `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | M |
| 10 | `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | M |
| 11 | `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs` | A |
| 12 | `QuickFiler.Test/QuickFiler.Test.csproj` | M |

The residual is exactly the twelve-member PLAN WRITE SET, with no extra path and none missing. The three `A` entries are the three files created by this plan, visible in the anchored diff only because of the `git add -N` span.

### `git status --porcelain --untracked-files=all`, full output

```
 A QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs
 M QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
 M QuickFiler.Test/Controllers/QfcDatamodelTests.cs
 A QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
 A QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/Controllers/QfcCollectionController.cs
 M QuickFiler/Controllers/QfcDatamodel.cs
 M QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
 M QuickFiler/Controllers/QfcQueue.cs
 M QuickFiler/Controllers/QfcRemainingQueueAdmission.cs
 M "QuickFiler/Helper Classes/EmailMoveMonitor.cs"
 M docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md
?? docs/.../evidence/baseline/csharpier-check.md
?? docs/.../evidence/baseline/dotnet-tool-restore.md
?? docs/.../evidence/baseline/msbuild-analyzers.md
?? docs/.../evidence/baseline/msbuild-nullable.md
?? docs/.../evidence/baseline/mstest-coverage.md
?? docs/.../evidence/baseline/p0-t11-blocked.md
?? docs/.../evidence/baseline/phase0-instructions-read.md
?? docs/.../evidence/baseline/tree-invariants.md
?? docs/.../evidence/qa-gates/coverage-delta.md
?? docs/.../evidence/qa-gates/csharpier-check.md
?? docs/.../evidence/qa-gates/csharpier-format.md
?? docs/.../evidence/qa-gates/msbuild-analyzers.md
?? docs/.../evidence/qa-gates/msbuild-nullable.md
?? docs/.../evidence/qa-gates/mstest-coverage.md
?? docs/.../evidence/qa-gates/setupdisposal-coverage.md
?? docs/.../evidence/regression-testing/fail-before-exception.finding1-topology-pin.md
?? docs/.../evidence/regression-testing/finding1-topology-pin-pass.md
?? docs/.../evidence/regression-testing/finding2-cleanup-fail-before.md
?? docs/.../evidence/regression-testing/finding2-cleanup-pass-after.md
?? docs/.../evidence/regression-testing/finding3-admission-pin-fail-before.md
?? docs/.../evidence/regression-testing/finding3-admission-pin-pass-after.md
?? docs/.../evidence/regression-testing/finding4-volatile-fail-before.md
?? docs/.../evidence/regression-testing/finding4-volatile-pass-after.md
```

The `docs/.../` elision in the untracked rows stands for the single feature-folder prefix `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/`, which is identical on every one of those rows and is written out in full on the tracked plan-file row above them. The elision is presentational only and removes no path information.

### Evidence artifacts asserted against porcelain, not against the diff

The evidence artifacts are untracked, and an anchored name-status diff is structurally blind to untracked files, so they are asserted here against the `--untracked-files=all` porcelain output instead. Every `EVIDENCE/<kind>/` artifact path named by `[P0-T1]` through `[P5-T8]` is present in that output:

- `evidence/baseline/`: `phase0-instructions-read.md`, `tree-invariants.md`, `dotnet-tool-restore.md`, `csharpier-check.md`, `msbuild-analyzers.md`, `msbuild-nullable.md`, `mstest-coverage.md` — all 7 present.
- `evidence/regression-testing/`: `finding1-topology-pin-pass.md`, `fail-before-exception.finding1-topology-pin.md`, `finding2-cleanup-fail-before.md`, `finding2-cleanup-pass-after.md`, `finding3-admission-pin-fail-before.md`, `finding3-admission-pin-pass-after.md`, `finding4-volatile-fail-before.md`, `finding4-volatile-pass-after.md` — all 8 present.
- `evidence/qa-gates/`: `csharpier-format.md`, `csharpier-check.md`, `msbuild-analyzers.md`, `msbuild-nullable.md`, `mstest-coverage.md`, `coverage-delta.md`, `setupdisposal-coverage.md` — all 7 present.

`evidence/baseline/p0-t11-blocked.md` also appears. It is not an artifact this plan names: it is the retained audit record of the block the previous execution attempt correctly raised at `[P0-T11]`, superseded by the completed `[P0-T11]` record inside `evidence/baseline/mstest-coverage.md` and annotated as such in its own text.

### Exclusions, named explicitly so they are auditable rather than implicit

Three artifacts this plan names are outside the porcelain assertion above by construction, because they do not exist at the moment the capture runs:

1. `EVIDENCE/qa-gates/scope-boundary.md` — this file, written by this task after its own capture.
2. `EVIDENCE/qa-gates/file-size-audit.md` — written by `[P5-T10]`.
3. `EVIDENCE/qa-gates/ac-traceability.md` — written by `[P6-T1]`.

### Coverage documents and the helper script

None of the four COVERAGE DOCUMENT PATHS, none of the intermediate extraction files, and not the single permitted helper script appears in either capture, because `coverage/` is gitignored at `.gitignore:144` with only `coverage/.gitkeep` re-included at `:145`. The helper is not a deliverable of issue #731: it is registered in no project file and asserted by no acceptance condition.

## Verdict

PASS. Zero `Metrics.cs` paths, zero `docs/features/potential/` paths, `QfcFormControllerSeamTests.cs` absent, and a residual of exactly the twelve PLAN WRITE SET paths after both recorded subtractions.
