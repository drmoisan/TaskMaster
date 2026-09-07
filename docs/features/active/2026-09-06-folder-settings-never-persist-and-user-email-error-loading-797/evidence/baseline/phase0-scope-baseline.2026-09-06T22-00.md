# Phase 0 — Anchored Change-Set Baseline for the Phase 5 Scope Gate (Issue #797)

Timestamp: 2026-09-07T09-21

Commands:

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git diff --name-status $BaseSha HEAD
git status --porcelain --untracked-files=all
```

EXIT_CODE: 0 (for the anchored diff command)

SCOPE-BASELINE-COMMITTED:

```text
A	docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md
A	docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/plan.2026-09-06T22-00.md
A	docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/research/research-folder-settings-persistence.md
A	docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md
A	docs/features/potential/promoted/2026-09-06-folder-settings-never-persist-and-user-email-error-loading.md
```

This listing covers work already committed on this branch before execution began: the five
preparation documents. No source or project file appears in it.

SCOPE-BASELINE-WORKTREE:

```text
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/prd-feature/feedback_backticked_paths_are_the_change_footprint.md
 M .claude/agent-memory/task-researcher/MEMORY.md
 M docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/plan.2026-09-06T22-00.md
?? .claude/agent-memory/atomic-planner/project_797_folder_settings_persistence_plan_seams.md
?? .claude/agent-memory/task-researcher/project_folder_settings_persistence_797.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-analyzer-build.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-bootstrap.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-csharpier-check.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-file-sizes.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-helper-selfcheck.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-instructions-read.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-nullable-build.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-requirements-read.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-vstest.2026-09-06T22-00.md
```

This listing covers uncommitted work. It includes the pre-existing agent-memory modifications left by
the preparation subagents, which are outside this item's Write Set and are neither edited nor
reverted nor committed by this execution, and the Phase 0 evidence artifacts written so far plus this
plan file's task check-offs. The session helper at coverage/plan797-helpers.ps1 and the coverage
outputs do not appear because the coverage directory is git-ignored.

A path may appear in both listings — this plan file appears in both, as an addition in the committed
set and as a modification in the worktree set. The two listings are therefore overlapping rather than
complementary. Phase 5 subtracts the union of the two sets from its own working set, so both are
captured here rather than inferred later.

PREPARATION-TRACKED:

`git ls-files --error-unmatch` over the five preparation documents exited 0 and echoed all five paths,
confirming each is tracked in HEAD:

```text
docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md
docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/plan.2026-09-06T22-00.md
docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/research/research-folder-settings-persistence.md
docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md
docs/features/potential/promoted/2026-09-06-folder-settings-never-persist-and-user-email-error-loading.md
```

No path was reported as untracked, so there is nothing to report to the caller before Phase 1 begins.

Output Summary: The committed baseline holds five preparation documents and no source file. The
worktree baseline holds five pre-existing agent-memory residuals, this plan file, and the Phase 0
evidence artifacts. All five preparation documents are tracked in HEAD.
