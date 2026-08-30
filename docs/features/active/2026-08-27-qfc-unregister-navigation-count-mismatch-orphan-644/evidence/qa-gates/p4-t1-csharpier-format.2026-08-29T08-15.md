# QA gate — CSharpier format (write mode) ([P4-T1])

- Issue: #644
- Task: `[P4-T1]`
- Timestamp: 2026-08-29T08-15

## This is the restarted pass

The Phase 4 loop was restarted from `[P4-T1]`, as the phase preamble requires. Reason: `[P4-T8]`
found that `QuickFiler/Controllers/QfcCollectionController.cs` had a `--stat` net addition of **+15**
lines against an acceptance bound of **no greater than 10** (the same bound AC-14 states). The
excess was entirely comment verbosity, so the five-line explanatory comments in
`UnregisterNavigation()` and `RegisterNavigationAsyncAction(...)` were each condensed to two lines,
bringing the net addition to **+9**. That edit changed a tracked source file, so `[P4-T1]` through
`[P4-T7]` were unchecked in the plan and re-run, and this artifact and its siblings record the
restarted pass rather than the first one. The condensation is comment-only: no statement, ordering,
or recorded value changed, and `[P2-T2]` and `[P2-T3]` were updated so their quoted member text
matches the final state.

Command: `dotnet tool run csharpier format .`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

Output:

```
Formatted 1562 files in 1626ms.
```

## Why the exit code and stdout are not the observation

This is a write-mode command: it rewrites tracked source and still exits 0 afterwards, so its exit
code is identical on a clean run and on a repairing run. Its stdout is likewise not decisive —
`Formatted 1562 files` counts files **processed**, not files rewritten, and prints the same shape
either way. The tree was therefore observed instead, with `git status --porcelain` captured
immediately before and immediately after the command.

## Before-listing (verbatim, 42 entries)

```
 M .claude/agent-memory/feature-review/project_review-worktree-differs-from-session-cwd-mirror-artifacts.md
 M .claude/agent-memory/orchestrator/MEMORY.md
 M .claude/agent-memory/orchestrator/bash-tool-collapses-double-backslash-in-sed.md
 M .claude/agent-memory/orchestrator/bootstrapping-orchestrator-state-json-first-write.md
 M .claude/agent-memory/orchestrator/model-routing-feature-review-is-always-fable.md
 M .claude/agent-memory/orchestrator/shared-checkpoint-read-modify-write-corrupts.md
 M .claude/agent-memory/parallel-planner/MEMORY.md
 M .claude/agent-memory/parallel-planner/project_bug_corpus_is_quickfiler_concentrated.md
 M .claude/agent-memory/parallel-planner/project_parallel_surface_partial_port.md
M  QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
M  QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
A  QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
M  QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
M  QuickFiler.Test/QuickFiler.Test.csproj
MM QuickFiler/Controllers/QfcCollectionController.cs
 M docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/plan.2026-08-29T07-42.md
?? .claude/agent-memory/feature-review/project_638-review-residuals.md
?? .claude/agent-memory/orchestrator/conflicting-pr-gets-no-ci-at-all.md
?? .claude/agent-memory/orchestrator/coverage-mode-raw-vs-processed-is-flake-sensitive.md
?? .claude/agent-memory/orchestrator/force-push-guard-blocks-rebase-use-merge.md
?? .claude/agent-memory/orchestrator/merging-main-invalidates-plan-base-anchor.md
?? .claude/agent-memory/orchestrator/run-orchestration-hook-gates-locally.md
?? .claude/agent-memory/parallel-orchestrator/
?? .claude/agent-memory/parallel-planner/feedback_default_to_open_mode_for_parallel_runs.md
?? .claude/agent-memory/parallel-planner/feedback_planner_git_commits_must_be_single_bare_segments.md
?? .claude/agent-memory/parallel-planner/reference_parallel_artifact_authoring_gotchas.md
?? .claude/agent-memory/parallel-planner/reference_worktree_lock_pid_is_the_session_not_the_subagent.md
?? batch1.txt
?? batch2.txt
?? batch3.txt
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/
?? docs/features/potential/2026-08-29-breadcrumb-right-descent-non-goals-follow-up.md
?? docs/features/potential/promoted/2026-08-29-breadcrumb-router-test-cr1-defect-neutral.md
?? docs/features/potential/promoted/2026-08-29-completion-gates-disagree-on-bug-route-potential-entry-tool.md
?? docs/features/potential/promoted/2026-08-29-efc-archive-root-getter-unguarded-against-com-failure.md
?? docs/features/potential/promoted/2026-08-29-efcdatamodel-success-path-test-uses-incidental-crash-as-barrier.md
?? docs/features/potential/promoted/2026-08-29-efcformcontroller-async-void-boundary-sinks-are-log-only.md
?? docs/features/potential/promoted/2026-08-29-efcformcontroller-five-unguarded-archive-root-reads.md
?? docs/features/potential/promoted/2026-08-29-issue-635-spec-ac9-variable-argument-count-stale.md
?? docs/features/potential/promoted/2026-08-29-nonblockingdelaytests-wall-clock-flake.md
?? docs/features/potential/promoted/2026-08-29-parallel-run-merge-gate-misparses-pr-number.md
?? docs/features/potential/promoted/2026-08-29-quickfiler-breadcrumb-bridge-r2-silently-reverts-440.md
```

## After-listing

Both listings were captured to files and compared with `diff`, which reported **no differences**.
The after-listing is byte-identical to the before-listing, 42 entries in the same order, so it is
not re-quoted.

The six footprint paths are staged (`M ` / `A ` in the first column) because `[P4-T8]`'s
`git add QuickFiler QuickFiler.Test` ran before the loop restart. `QfcCollectionController.cs`
shows `MM`: staged at the pre-trim content and modified again in the worktree by the comment
condensation. `[P4-T8]` re-stages before re-measuring.

## Acceptance

> No path appears in the after-listing that is absent from the union of the before-listing, the six
> code paths in the change footprint, the `PRE-EXISTING FORMAT DRIFT SET`, and any path under
> `.claude/agent-memory/`.

**PASS.** The after-listing introduces no path at all relative to the before-listing.

## Files the formatter rewrote in this pass: none

Determined decisively rather than by inference. A last-write scan flagged
`QuickFiler\Controllers\QfcCollectionController.cs` at `14:09:06.679`, a timestamp close enough to
the format run to be ambiguous between the comment-condensation edit and a formatter rewrite. The
ambiguity was resolved by re-running the formatter and comparing the file's timestamp across it:

```
mtime-before=14:09:06.679
Formatted 1562 files in 1892ms.
EXIT_CODE=0
mtime-after=14:09:06.679
```

The timestamp is unchanged across a full `csharpier format .` run, so the formatter does not
rewrite that file; `14:09:06.679` was the hand edit. **No file at all was rewritten by the
formatter in this restarted pass**, which is the expected result given that the first pass had
already converged the tree and the only intervening change was hand-written in already-formatted
style.

Because no tracked file was rewritten, the loop does not restart again and continues to `[P4-T2]`.

Output Summary: `dotnet tool run csharpier format .` exited 0. `git status --porcelain` before and
after is byte-identical at 42 entries, verified by `diff`, so the acceptance holds with no path
added. The formatter rewrote **no file**, established by an unchanged file timestamp across a
second format invocation rather than inferred from stdout. This is the restarted pass, triggered by
the `[P4-T8]` net-line finding described above.
