# P2-T9 — Batch A budget boundary

Timestamp: 2026-09-19T15-58

Command:

```
git show --name-only --format= 48f0c710a9a970587ab8b17956be224513c1f7fd
```

with the enumerated result partitioned by path shape, and

```
Get-ChildItem .claude\state -Filter "powershell-batch-budget.*.json"
```

EXIT_CODE: 0

Batch A commit measured: **`48f0c710a9a970587ab8b17956be224513c1f7fd`**, the head SHA P2-T8
recorded.

## The two counts

| Count | Definition | Measured | Required |
|---|---|---|---|
| Production | paths matching `scripts/**` with extension `.ps1`, `.psm1` or `.psd1` and not under `tests/` | **1** | exactly 1 |
| Test | paths matching `tests/**` with extension `.ps1` or ending `.Tests.ps1` | **1** | exactly 1 |

Enumerated members:

```
PROD: scripts/dependencies/PackageGraph.psm1
TEST: tests/scripts/dependencies/PackageGraph.Tests.ps1
```

Both are the members the task names. They are derived from the phase's own task list rather than
observed and accepted: P1-T4 creates the module and P1-T5 creates its suite, and no other Phase 1
task writes a PowerShell file.

The counts are **exact rather than bounded above**, and the members are asserted rather than
expected, because an at-most-3 bound is satisfied by 0 and 0: a batch that silently dropped a file
would pass it, and the companion commit task cannot close that gap either, since P2-T8 asserts only
that `git show --name-only` lists paths drawn from its pathspec set, which an empty commit also
satisfies. Bounded-above counts catch an overrun and miss an omission; exact counts catch both.

An earlier plan revision counted 2 and 1 on a prediction that the PoshQC formatter would rewrite
`scripts/vscode/Sync-PackageReferences.ps1`. P0-T15 measured 0 of 32 rewritten and P2-T1 measured 0
rewrites again across three passes, so that file is untouched until P3-T4 and belongs to Batch B.
It does not appear in this commit.

## Enumerated paths

The commit carries **82** paths. The 26 under
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/` are the plan
file, `spec.md` and the 24 evidence artifacts; the remaining 56 are listed in full:

```
.csharpierignore
.github/workflows/_build-analyzers.yml
.github/workflows/_build-nullable.yml
.github/workflows/_mstest-coverage.yml
.github/workflows/_pester.yml
QuickFiler.Test/app.config
QuickFiler.Test/packages.config
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/app.config
QuickFiler/packages.config
QuickFiler/QuickFiler.csproj
scripts/dependencies/PackageGraph.psm1
SVGControl.Test/app.config
SVGControl.Test/packages.config
SVGControl/app.config
Tags.Test/app.config
Tags.Test/packages.config
Tags.Test/Tags.Test.csproj
Tags/app.config
Tags/packages.config
Tags/Tags.csproj
TaskMaster.Test/app.config
TaskMaster.Test/packages.config
TaskMaster.Test/TaskMaster.Test.csproj
TaskMaster/app.config
TaskMaster/packages.config
TaskTree.Test/app.config
TaskTree.Test/packages.config
TaskTree.Test/TaskTree.Test.csproj
TaskTree/app.config
TaskTree/packages.config
TaskTree/TaskTree.csproj
TaskVisualization.Test/app.config
TaskVisualization.Test/packages.config
TaskVisualization.Test/TaskVisualization.Test.csproj
TaskVisualization/app.config
TaskVisualization/packages.config
TaskVisualization/TaskVisualization.csproj
tests/scripts/dependencies/PackageGraph.Tests.ps1
ToDoModel.Test/app.config
ToDoModel.Test/packages.config
ToDoModel.Test/ToDoModel.Test.csproj
ToDoModel/app.config
ToDoModel/packages.config
ToDoModel/ToDoModel.csproj
UtilitiesCS.Test/app.config
UtilitiesCS.Test/packages.config
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/app.config
UtilitiesCS/packages.config
UtilitiesCS/UtilitiesCS.csproj
VBFunctions.Test/app.config
VBFunctions.Test/packages.config
VBFunctions.Test/VBFunctions.Test.csproj
VBFunctions/packages.config
VBFunctions/VBFunctions.csproj
```

Exactly two of the 82 are PowerShell files, and they are the two enumerated above.

## Hook state — observation, not assertion

`.claude/state/powershell-batch-budget.default.json` exists. Its contents:

```json
{
  "prodCap": 3,
  "testCap": 3,
  "prodFiles": [
    "<TEMP>/claude/<user-home>-repos-TaskMaster-wt-2026-08-23T22-51/<session>/scratchpad/run-vstest.ps1",
    "<TEMP>/claude/<user-home>-repos-TaskMaster-wt-2026-08-23T22-51/<session>/scratchpad/postrebase_verify.ps1",
    "<TEMP>/claude/<user-home>-repos-TaskMaster-wt-2026-08-23T22-51/<session>/scratchpad/run-toolchain-442.ps1"
  ],
  "testFiles": []
}
```

The three recorded `prodFiles` are scratchpad scripts from a **different worktree and a different
session** (`2026-08-23T22-51`), and `testFiles` is empty. Neither file this batch wrote appears.

That is the state the plan predicts and the reason this task measures the commit rather than the
hook. `.claude/hooks/enforce-powershell-batch-budget.ps1` computes its root as
`Split-Path (Split-Path $PSScriptRoot -Parent) -Parent` and `settings.json:144` registers it by a
relative path resolving against the **session** worktree, so every file this plan writes is
out-of-root and is discarded at lines 277-282 with `permissionDecision = 'allow'`, no slot consumed
and `shouldWriteState = $false`. The arrays therefore stay empty of this batch's work whatever the
batch did, and an assertion over them would read the same on a compliant batch and on one that
wrote thirty PowerShell files.

The commit measurement above asserts the same per-batch budget the hook nominally enforces, fails
when a batch genuinely overruns, and does not depend on a hook that cannot observe this worktree.

`CLAUDE_POWERSHELL_BUDGET_PROD` and `CLAUDE_POWERSHELL_BUDGET_TEST` were **not** raised; no task in
this plan authorises raising either.

## Preconditions recorded as satisfied

| Precondition | Evidence | State |
|---|---|---|
| P2-T3 returned `EXIT_CODE: 0` | `evidence/qa-gates/p2-t3-pester.2026-09-19T09-44.md` — Pester 206 passed, 0 failed | satisfied |
| P2-T4 returned `EXIT_CODE: 0` | `evidence/qa-gates/p2-t4-csharpier-check.2026-09-19T09-44.md` — `Checked 1623 files` | satisfied |
| P2-T5 returned `EXIT_CODE: 0` | `evidence/qa-gates/p2-t5-msbuild-analyzers.2026-09-19T09-44.md` — 0 `CS0006`, 18 assemblies | satisfied |
| P2-T6 returned `EXIT_CODE: 0` | `evidence/qa-gates/p2-t6-msbuild-nullable.2026-09-19T09-44.md` — 18 assemblies, 0 `CS86` | satisfied |
| P2-T7 returned `EXIT_CODE: 0` | `evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md` — 7343 passed | satisfied |
| P2-T2 satisfied its own acceptance as written | `evidence/qa-gates/p2-t2-poshqc-analyze.2026-09-19T09-44.md` — total exactly 16, all members of the P0-T17 baseline, 0 in owned files | satisfied |
| P2-T8 produced a commit | `48f0c710a9a970587ab8b17956be224513c1f7fd`, 82 files | satisfied |

The P2-T2 row is stated as its finding-set condition and **not** as an exit code, because that
task's stated expectation is `ok:false` and a non-zero exit while the 16 pre-existing findings
stand. Recording it as an exit-code precondition would make this boundary unsatisfiable on a
correct run.

## Deviations inside Batch A worth carrying forward

Two things happened inside this batch that a later reader should not have to reconstruct.

1. **P2-T1 ran three times, not once.** Pass 1 rewrote the new Pester suite; pass 3 followed a
   correction to `scripts/dependencies/PackageGraph.psm1` that P2-T2's first run required. Both
   restarts are recorded in the P2-T1 artifact. The final rewrite count is 0.
2. **`pwsh -WorkingDirectory ... -File <relative-path>` resolves the script against the session
   worktree.** It caused one restore to target the wrong checkout before it was caught at P1-T14.
   Every later invocation in this batch uses an absolute script path with an explicit
   `Set-Location`. Batches B, C and D should use the same form.

Output Summary: Batch A's commit `48f0c710a9a970587ab8b17956be224513c1f7fd` carries 82 paths, of
which exactly **1** is a production PowerShell file (`scripts/dependencies/PackageGraph.psm1`) and
exactly **1** is a test PowerShell file (`tests/scripts/dependencies/PackageGraph.Tests.ps1`),
meeting the exact counts this boundary requires. The `.claude/state` budget file exists but records
only three scratchpad paths from an unrelated worktree and session, confirming the hook cannot
observe this worktree and that the commit measurement is the enforceable gate. All seven
preconditions are satisfied. Batch A is closed.
