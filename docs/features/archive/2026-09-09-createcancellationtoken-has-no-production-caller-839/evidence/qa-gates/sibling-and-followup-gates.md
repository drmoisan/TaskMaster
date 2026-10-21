# Sibling-integrity and follow-up gates (AC6, AC12) — issue #839

Timestamp: 2026-09-13T06-19
Command: git diff --stat 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
Command: git -c grep.patternType=fixed grep -c -e "RibbonController.LoadQuickFiler()" -- docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
Command: git -c grep.patternType=fixed grep -c -e "QfcHomeController.Init()" -- docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
Command: git -c grep.patternType=fixed grep -c -e "IQfcHomeController.Init()" -- docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
Command: git -c grep.patternType=fixed grep -c -e "CreateCancellationToken()" -- docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
Command: git -c grep.patternType=fixed grep -c -e "Init_InitializesCorrectly" -- docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
Command: git status --porcelain --untracked-files=all -- docs/features/potential docs/features/parallel
EXIT_CODE: 0

## Output Summary

Sibling-integrity gate for AC6. The stat diff for QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs against the base commit prints NOTHING, so that file is byte-identical to the base tree. This item did not touch it.

The companion observation AC6 also needs is recorded in evidence/regression-testing/init-token-source-pass-after.md: `PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=1` with a matching `FAILED_` value of 0. The pre-existing cleanup contract therefore still holds on the post-change tree, which matters here because the fix causes `Init()` to create a real `CancellationTokenSource` on paths that previously left the field null, and `Cleanup()` is what disposes and nulls it.

Follow-up gate for AC12. Each of the five symbol searches over spec.md prints at least 1:

    RibbonController.LoadQuickFiler()     4
    QfcHomeController.Init()              7
    IQfcHomeController.Init()             3
    CreateCancellationToken()            13
    Init_InitializesCorrectly            12

The spec text therefore still carries the follow-up record for the dead-path symbols, which is what AC12 verifies. This plan files no follow-up issue from this branch.

Scoped porcelain span over the potential and parallel trees prints NOTHING. No line appears that is absent from the [P0-T8] `INHERITED-PORCELAIN:` set, trivially so, because the span is empty. This plan writes nothing under either tree.

## Candidate follow-up reported to the caller rather than filed

One candidate follow-up was identified during execution and is reported in the executor's return rather than filed from this branch, consistent with AC12 and with the plan filing nothing: scripts/vscode/TaskMaster.cli.runsettings configures MSTest class-level parallelism, and under it three `QfcInitEmailQueueZeroBatchTests` tests fail with a netstandard 2.1 resolution error through Deedle, while the same assembly passes 1394 of 1394 without that file. The repository's CI workflow passes no settings file to vstest, so the local runner diverges from CI. Decision D16 removed the switch from this plan's own commands; whether to remove it at source is out of this item's scope and touches files outside its Write Set.

## Command-transport note

The five `git grep` invocations were run inside one `pwsh -NoProfile -Command` loop that invoked each in turn with the token supplied by variable, so that each invocation's exit code could be captured alongside its output. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. Each retains the plan's pinned fixed-string engine, the `-c` switch, the `-e` token operand and the single spec pathspec exactly as written. The `git diff --stat` span and the scoped `git status` span were addressed to the assigned worktree with a repository-location option; their refs, operands and pathspecs are exactly as the plan writes them.
