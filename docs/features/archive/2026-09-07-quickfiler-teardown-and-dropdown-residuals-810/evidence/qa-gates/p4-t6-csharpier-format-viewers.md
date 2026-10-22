# [P4-T6] CSharpier Format — The Two Edited Viewer Files

Timestamp: 2026-09-08T10-02
Command: `dotnet tool run csharpier format QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`
EXIT_CODE: 0
Output Summary: The formatter ran over the two files edited by [P4-T4] and [P4-T5] and rewrote neither. Both observation spans are byte-identical before and after the invocation, so the hand-written layout of both edits already matches CSharpier's output at the pinned 1.2.6 version.

Verbatim printed `Formatted` line:

```
Formatted 2 files in 2279ms.
```

PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: True

## Why the exit code is not the observation

`format` is a write-mode command. It rewrites tracked source and still exits 0 after rewriting, and it prints one line of the form `Formatted <count> files in <duration>ms.` whether or not it changed anything, so neither the exit code nor that line distinguishes a clean run from a repairing one. The distinguishing observation is the pair of spans below, captured immediately before and immediately after the invocation.

## `git status --porcelain --untracked-files=all` — before

```
 M QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs
 M QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
 M QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
 M QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
 M QuickFiler/Controllers/QfcFormController.Deactivate.cs
 M QuickFiler/Controllers/QfcFormController.EventHandlers.cs
 M QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
 M QuickFiler/Controllers/QfcHomeController.cs
 M QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
 M QuickFiler/Viewers/BreadcrumbDropDownHost.cs
 M docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/plan.2026-09-07T21-59.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p1-t10-quickfiler-suite.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p1-t2-test-build.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p1-t3-ac1-fail-before.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p1-t7-post-fix-build.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p1-t8-ac1-pass-after.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p1-t9-ac2-fence-after-ac1.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p2-t2-test-build.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p2-t3-ac3-fail-before.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p2-t5-post-fix-build.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p2-t6-ac3-pass-after.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p3-t2-test-build.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p3-t3-ac4-fail-before.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p3-t5-post-fix-build.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p3-t6-ac4-pass-after.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p4-t2-test-build.md
?? docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p4-t3-ac5-fail-before.md
```

## `git status --porcelain --untracked-files=all` — after

Byte-identical to the before span above, line for line and in the same order. `PATH_SETS_IDENTICAL: True` records that comparison.

## `git diff --stat origin/main` — before

```
 .claude/agent-memory/atomic-executor/MEMORY.md     |  101 +-
 ...ol_results_inject_bash_read_edit_instruction.md |   24 +
 .claude/agent-memory/atomic-planner/MEMORY.md      |  214 ++--
 ...t_810_teardown_dropdown_residuals_plan_seams.md |  108 ++
 .claude/agent-memory/orchestrator/MEMORY.md        |    1 +
 ...erincludes-citations-omits-gitignored-writes.md |   48 +
 .../new-active-feature-folder-date-prefix.md       |   12 +
 ...-child-cwd-is-session-root-not-item-worktree.md |   15 +
 ...orktree-isolation-blocks-pwsh-per-agent-type.md |   17 +
 .claude/agent-memory/task-researcher/MEMORY.md     |    1 +
 .../project_qfc810_teardown_dropdown_residuals.md  |   51 +
 .../QfcFormControllerCancelTeardownTests.cs        |   29 +
 .../Controllers/QfcFormControllerCleanupTests.cs   |   56 +
 .../Controllers/QfcHomeControllerCleanupTests.cs   |   43 +-
 .../BreadcrumbDropDownCloseOrderingTests.cs        |   47 +
 .../Controllers/QfcFormController.Deactivate.cs    |   13 +-
 .../Controllers/QfcFormController.EventHandlers.cs |    5 +-
 .../Controllers/QfcFormController.SetupDisposal.cs |   94 +-
 QuickFiler/Controllers/QfcHomeController.cs        |    2 +
 QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs  |   10 +-
 QuickFiler/Viewers/BreadcrumbDropDownHost.cs       |   12 +-
 .../evidence/baseline/p0-t10-msbuild-nullable.md   |   29 +
 .../evidence/baseline/p0-t11-quickfiler-tests.md   |   33 +
 .../evidence/baseline/p0-t12-coverage.md           |   52 +
 .../evidence/baseline/p0-t13-line-counts.md        |   30 +
 .../evidence/baseline/p0-t14-ac2-fence-baseline.md |   41 +
 .../evidence/baseline/p0-t15-baseline-commit.md    |   49 +
 .../evidence/baseline/p0-t2-branch-and-base.md     |   51 +
 .../evidence/baseline/p0-t3-dotnet-sdk.md          |   36 +
 .../evidence/baseline/p0-t4-nuget-restore.md       |   51 +
 .../evidence/baseline/p0-t5-dotnet-tool-restore.md |   38 +
 .../evidence/baseline/p0-t6-dotnet-coverage.md     |   15 +
 .../evidence/baseline/p0-t7-vstest-resolution.md   |   24 +
 .../evidence/baseline/p0-t8-csharpier-check.md     |   20 +
 .../evidence/baseline/p0-t9-msbuild-analyzers.md   |   27 +
 .../evidence/baseline/phase0-instructions-read.md  |   29 +
 .../issue.md                                       |   73 ++
 .../plan.2026-09-07T21-59.md                       |  158 +++
 .../research/research.2026-09-07T22-10.md          | 1162 ++++++++++++++++++++
 .../spec.md                                        |  714 ++++++++++++
 40 files changed, 3291 insertions(+), 244 deletions(-)
```

## `git diff --stat origin/main` — after

Byte-identical to the before span above, line for line and in the same order, including the two viewer rows `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 10 +-` and `QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 12 +-` and the trailing `40 files changed, 3291 insertions(+), 244 deletions(-)` summary. `DIFFSTAT_IDENTICAL: True` records that comparison.

The diffstat span is the discriminating half of the pair. Porcelain status alone could not detect a rewrite of these two files, because both were already reported as `M` before the invocation and would still be reported as `M` after one; the per-file insertion and deletion counts in the diffstat would move if the formatter had changed either file.

## Interpretation

The formatter changed nothing, so no toolchain-loop restart is triggered by this step. The layouts written by hand in [P4-T4] and [P4-T5] are already CSharpier's own output: the four-clause comment block and the fourth `CompleteAll` operation in `BreadcrumbDropDownHost.cs`, and the rewrapped `///` lines in `BreadcrumbDropDownHost.Open.cs`, all sit inside the 100-column print width at their indentation.

The `.claude/agent-memory/` rows in the diffstat are clause-B inherited paths under the rule stated in the plan's Write Set, not changes made by any task of this plan. They are subtracted by [P7-T11].
