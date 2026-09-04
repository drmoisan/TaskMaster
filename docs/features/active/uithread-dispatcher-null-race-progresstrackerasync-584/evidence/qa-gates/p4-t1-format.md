# P4-T1 — CSharpier format, write scope restricted to the six owned paths (second pass)

Timestamp: 2026-09-03T21-44

Command:
```text
env -C <worktree-root> git status --porcelain
env -C <worktree-root> dotnet tool run csharpier format UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
env -C <worktree-root> git status --porcelain
```

The multi-path invocation was accepted by the pinned CSharpier 1.2.6 CLI, so the per-path fallback
was not used and only three commands were run.

EXIT_CODE:
- command 1 (`git status --porcelain`, before) — 0
- command 2 (`dotnet tool run csharpier format ...`) — 0
- command 3 (`git status --porcelain`, after) — 0

## Output Summary

### Formatter trailing summary line, verbatim

```text
Formatted 6 files in 3827ms.
```

`N` in that line is the count of files PROCESSED, not the count rewritten, so the number alone is not
evidence of a no-op. The before-and-after tree observation below is what establishes what changed.

### Unscoped porcelain, taken BEFORE the formatter ran

```text
 M "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
M  UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
M  UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
M  UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
M  UtilitiesCS.Test/Threading/UiThread_Tests.cs
M  UtilitiesCS/Threading/UiThread.cs
 M docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md
 M docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md
?? .claude/agent-memory/atomic-executor/project_bare_msbuild_not_on_path_in_git_bash.md
?? .claude/agent-memory/atomic-executor/project_msys_slash_switch_conversion_rule.md
?? .claude/agent-memory/atomic-executor/project_vstest_success_run_prints_no_failed_or_skipped_line.md
?? .claude/agent-memory/atomic-executor/project_worktree_isolation_guard_refuses_pwsh_from_bash.md
?? .claude/agent-memory/atomic-planner/project_584_uithread_dispatcher_guard_plan_seams.md
?? docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/
```

### Unscoped porcelain, taken AFTER the formatter ran

```text
 M "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
M  UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
M  UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
M  UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
M  UtilitiesCS.Test/Threading/UiThread_Tests.cs
M  UtilitiesCS/Threading/UiThread.cs
 M docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md
 M docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md
?? .claude/agent-memory/atomic-executor/project_bare_msbuild_not_on_path_in_git_bash.md
?? .claude/agent-memory/atomic-executor/project_msys_slash_switch_conversion_rule.md
?? .claude/agent-memory/atomic-executor/project_vstest_success_run_prints_no_failed_or_skipped_line.md
?? .claude/agent-memory/atomic-executor/project_worktree_isolation_guard_refuses_pwsh_from_bash.md
?? .claude/agent-memory/atomic-planner/project_584_uithread_dispatcher_guard_plan_seams.md
?? docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/
```

The two unscoped porcelain outputs are byte-identical. No entry appeared, disappeared, or changed
status, so no path outside the six owned paths was touched by the formatter, and no owned path
changed status either.

RESTORED_UNOWNED_FORMAT_DRIFT: NOT APPLICABLE (formatter write scope restricted to the six owned paths)

### Post-format line counts of the six owned paths, for reference by P4-T8

```text
  172 UtilitiesCS/Threading/UiThread.cs
  179 UtilitiesCS.Test/Threading/UiThread_Tests.cs
  348 UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
  206 UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
  514 UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  320 QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs
```

`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` is unchanged at 320 lines, the same count
P2-T4 recorded, so the formatter did not re-wrap the retargeted `GetField(` call.

## Acceptance

Satisfied. `EXIT_CODE: 0` for the formatter, and the two unscoped porcelain outputs are identical, so
they differ in no entry at all, which trivially satisfies the clause that they differ only in entries
for the six owned paths.

This task records a before-and-after tree observation in addition to the formatter's exit code,
because a formatter rewrites tracked source and still exits 0 after rewriting.
