# P5-T9 — Scope Boundary (measured run for AC7's no-production-file clause)

Timestamp: 2026-09-17T02-38

Command (six spans, in one task):

1. `git diff --name-only 66b65a4626095ade5a01643aee4a43c90cc58cbf -- QuickFiler QuickFiler.Test`
2. `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test`
3. `git diff --name-only origin/main..HEAD -- QuickFiler QuickFiler.Test`
4. `git diff --name-only origin/main...HEAD -- QuickFiler QuickFiler.Test`
5. `Get-FileHash -Algorithm SHA256 -LiteralPath scripts/vscode/TaskMaster.cli.runsettings`
6. `git diff --exit-code 66b65a4626095ade5a01643aee4a43c90cc58cbf -- scripts/vscode/TaskMaster.cli.runsettings QuickFiler.Test/QuickFiler.Test.csproj`

EXIT_CODE: 0

CHANNEL: COMMAND

## The four path lists, verbatim

List A, working-tree diff against the merge base:

    LIST-A-WORKTREE-COUNT: 1
    LIST-A QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

List B, porcelain status including untracked:

    LIST-B-PORCELAIN-COUNT: 0

List C, `origin/main..HEAD`:

    LIST-C-TWO-DOT-COUNT: 1
    LIST-C QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

List D, `origin/main...HEAD`:

    LIST-D-THREE-DOT-COUNT: 1
    LIST-D QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

Lists A and B are complementary: the anchored name-listing diff enumerates tracked changes only and
is blind to an untracked file, while porcelain status goes empty once a change is committed. List B
being empty is the expected state, because the source change is committed.

## TWO-DOT-VS-THREE-DOT-AGREE: True

Lists C and D are identical. This agreement is recorded as required by the DIFF BASES block, and its
evidentiary weight is stated honestly rather than overstated: `origin/main` is an ancestor of `HEAD`
on this branch, because the orchestrator merged `origin/main` into the item branch before execution
began and P0-T3 recorded the resulting merge base as equal to `origin/main` itself. While that
ancestry holds, the two-dot and three-dot forms are semantically identical by construction, so their
agreement is a property of the ref topology and is not independent confirmation of anything about
this change.

The check is not vacuous in general: it would report `False` if `origin/main` advanced past the
branch point after P0-T3's fetch, in which case the two-dot list would carry paths this branch never
touched and the disagreement would be reported to the orchestrator as `BASE ADVANCED`. That did not
occur.

## CHANGED-PATHS

The union of lists A and B is the single path
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`. Subtracting
`INHERITED-CLAUSE-A:` from P0-T3 removes nothing, because none of that clause's eleven captured
paths lies under `QuickFiler` or `QuickFiler.Test`. Subtracting the `.claude/agent-memory/` prefix
removes nothing for the same reason.

CHANGED-PATHS: `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`

No path under `QuickFiler/` changed, so no production file was modified. No other path under
`QuickFiler.Test/` changed, so no sibling test file was modified.

## Runsettings and project file

RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57

RUNSETTINGS-CSPROJ-DIFF-EXIT: 0

`git diff --exit-code` over `scripts/vscode/TaskMaster.cli.runsettings` and
`QuickFiler.Test/QuickFiler.Test.csproj`, anchored on the merge base, exits 0: neither file changed.
The runsettings file still carries `Workers=0` and `Scope=ClassLevel`, and the project file needed no
edit because the Write Set file was already registered at line 96.

## Acceptance

All four conditions hold.

- `CHANGED-PATHS:` is exactly `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
  and nothing else.
- The `git diff --exit-code` span for the runsettings and the project file exits 0.
- `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` from P0-T4.
- `TWO-DOT-VS-THREE-DOT-AGREE:` is recorded as `True`, and the three-dot list is exactly the one
  test file path.
