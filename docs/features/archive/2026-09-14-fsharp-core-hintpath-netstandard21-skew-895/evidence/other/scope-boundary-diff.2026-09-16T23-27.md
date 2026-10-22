# Phase 4 — AC3 Diff Gate, Write-Set Containment, and the AC5 Comment-Only Diff (Issue #895)

Timestamp: 2026-09-17T01-30
Tasks: [P4-T14] and [P4-T15]
WORKTREE-LEAF: agent-a8bc4dc5978785885

Both tasks run after the `[P4-T13]` commit.

ANCHOR: origin/main after fetch, never local main.

`git fetch origin` was re-run at the head of `[P4-T14]` and `git rev-parse --verify origin/main`
exited 0, resolving to `91746d2e4776a59ee1db1856c5c490a009c4958b`. That is unchanged from the
`[P0-T8]` capture, so `origin/main` did not advance during this run.

EXIT_CODE: 0
ExpectedExitCode: 0

## Six-file numstat, two-dot form (worktree against origin/main)

```
1	1	QuickFiler.Test/QuickFiler.Test.csproj
1	1	QuickFiler/QuickFiler.csproj
1	1	ToDoModel/ToDoModel.csproj
```

## Six-file numstat, three-dot form (committed history from the merge base)

```
1	1	QuickFiler.Test/QuickFiler.Test.csproj
1	1	QuickFiler/QuickFiler.csproj
1	1	ToDoModel/ToDoModel.csproj
```

The two outputs are byte-identical. Both are recorded because they answer different questions: the
two-dot form compares the worktree, the three-dot form compares this branch's committed
contribution from the merge base. Their agreement here also establishes that the three-dot form used
at `[P4-T15]` is not silently degenerating to the two-dot diff.

Each of the three edited project files differs from `origin/main` on exactly one line, and no line
appears for `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` or
`ToDoModel.Test/ToDoModel.Test.csproj`, so those three HintPaths are byte-identical before and after
the fix. This is the measured run for AC3; `[P2-T4]` is the confirming pre-commit run.

## CHANGED-PATHS:

```
.claude/agent-memory/atomic-executor/project_plan_line_locators_stale_after_doc_edit.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_895_fsharp_core_hintpath_plan_seams.md
.claude/agent-memory/prd-feature/MEMORY.md
.claude/agent-memory/prd-feature/reference_repo_walking_tests_exclude_claude_worktrees.md
.claude/agent-memory/task-researcher/MEMORY.md
.claude/agent-memory/task-researcher/project_fsharp_core_hintpath_skew_895.md
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/QuickFiler.csproj
TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs
TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs
TaskMaster.Test/TaskMaster.Test.csproj
ToDoModel/ToDoModel.csproj
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/analyzer-baseline.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/channel-probe.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/format-baseline.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/nullable-baseline.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/phase0-instructions-read.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/toolchain-bootstrap.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/tree-baseline.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/other/hintpath-edits.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/other/post-format-sweep.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/other/remarks-correction.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/qa-gates/analyzer-final.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/qa-gates/coverage-delta.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/qa-gates/format-final.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/qa-gates/loop-closure.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/qa-gates/nullable-final.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/qa-gates/test-final.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/expect-fail-build.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/expect-fail-shape-b.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-bootstrap-namespace.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-build.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-shape-a.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/test-authoring.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/issue.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/research/2026-09-16T23-30-fsharp-core-hintpath-research.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md
```

## OUT-OF-WRITE-SET:

NONE

Every changed path falls into exactly one admitted category:

- seven paths under `.claude/agent-memory/`, which are the inherited-path rule's clause B and are
  also every one of them present in the `[P0-T8]` `INHERITED-CLAUSE-A:` capture, so they predate
  this execution run;
- seven source paths, each one of the seven Write Set paths:
  `QuickFiler/QuickFiler.csproj` (1), `QuickFiler.Test/QuickFiler.Test.csproj` (2),
  `ToDoModel/ToDoModel.csproj` (3), `TaskMaster.Test/TaskMaster.Test.csproj` (4),
  `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` (5),
  `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` (6) and
  `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` (7);
- the remainder, all under the prefix
  `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/`, which is this
  issue's own feature folder and is outside the source write set by convention.

No path was subtracted that is a Write Set path, and no path outside these categories appears.

## Porcelain span

```
git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
```

Output: empty. No modified or untracked source path remains outside the two excluded prefixes after
the commit.

## [P4-T14] Acceptance

- `git rev-parse --verify origin/main` exits 0: yes.
- The two numstat outputs are byte-identical: yes.
- Each is exactly the three `1	1` lines for the three edited project files: yes.
- No line for `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` or
  `ToDoModel.Test/ToDoModel.Test.csproj`: yes.
- `OUT-OF-WRITE-SET: NONE`: yes.
- The porcelain span returns no output: yes.

---

## AC5 Comment-Only Diff:

Task: [P4-T15]
Timestamp: 2026-09-17T01-30

The `[P3-T2]` payload re-run with both of its diff spans re-anchored from `origin/main` to
`origin/main...HEAD`, after the `[P4-T14]` fetch. The two re-anchored spans are exactly:

```
$changed = @(git diff -U0 origin/main...HEAD -- $f).Where({ ($_.StartsWith("+") -or $_.StartsWith("-")) -and -not $_.StartsWith("+++") -and -not $_.StartsWith("---") })
git diff --numstat origin/main...HEAD -- $f
```

Each carries an explicit ref operand. Neither is the worktree-against-index form, which would pass
vacuously here because `[P4-T13]` has already committed the change. The `Select-String` counts in
the same payload read the worktree, which equals `HEAD` post-commit.

EXIT_CODE: 0
ExpectedExitCode: 0

Output:

```
UNSATISFIABLE_COUNT=1
DISPLAY_NAME_TESTS_COUNT=1
DONOTPARALLELIZE_COUNT=2
TESTMETHOD_COUNT=9
BECAUSE_206_COUNT=1
NETSTANDARDBIND_LINES=470
CHANGED_LINES=18
NON_COMMENT_CHANGED_LINES=0
11	7	TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs
```

### [P4-T15] Acceptance

- `UNSATISFIABLE_COUNT=1`: yes. The pre-fix count of 2 is recorded at
  `evidence/baseline/tree-baseline.2026-09-16T23-27.md`, so the transition from 2 to 1 is evidenced
  at both ends. The surviving occurrence is the line-281
  `NegativeControl_WithoutInstall_Netstandard21Throws` summary, which remains true after the fix.
- `DISPLAY_NAME_TESTS_COUNT=1`: yes, up from a pre-fix 0.
- `DONOTPARALLELIZE_COUNT=2`: yes, unchanged.
- `TESTMETHOD_COUNT=9`: yes, unchanged.
- `BECAUSE_206_COUNT=1`: yes, unchanged. The assertion message at lines 206-207 is untouched.
- `NON_COMMENT_CHANGED_LINES=0`: yes. Every changed line in this file is an XML documentation
  comment line.
- The numstat deletions figure is 9: NOT AS WRITTEN. The observed figure is 7, for the reason
  recorded in full at `[P3-T2]`: `[P3-T1]` replaced a nine-line span with a thirteen-line block
  whose first and last lines are identical to the original's, and git reports an unchanged line as
  context rather than as a deletion. The hunk header `@@ -364,7 +364,11 @@` records this directly.
  The net line count `NETSTANDARDBIND_LINES=470` is reached identically under either convention, the
  `<remarks>` element AC5 is worded about survives the edit, and the comment-only property that AC5
  actually rests on is measured directly by `NON_COMMENT_CHANGED_LINES=0`. The plan is not edited;
  the deviation is escalated in the executor's completion report.

This is the measured run for AC5; `[P3-T2]` is the confirming pre-commit run.
