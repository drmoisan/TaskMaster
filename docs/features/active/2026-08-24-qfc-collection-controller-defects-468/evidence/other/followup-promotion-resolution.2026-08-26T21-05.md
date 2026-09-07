# Follow-up promotion resolution (AC-29 discharge)

Timestamp: 2026-08-26T21-05

Command:

```
mcp__drm-copilot__potential_to_issue --potential_path <absolute path> --promotion_type <type> --work_mode <mode>
gh issue comment <N> --body-file docs/features/potential/promoted/<entry>.md
gh issue view <N> --json number,state,title
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

The `PROMOTION_DEFERRED` disposition recorded by the executor at `[P14-T6]` is now discharged. The
executor's deferral was correct for its own tool surface: the promotion lifecycle functions are not
exposed to `atomic-executor`, only the four PoshQC functions are. The orchestrator does hold those
functions and ran the lifecycle itself, so no entry was created by hand with a direct issue-creation
command, and every promotion carries a real MCP receipt.

All nine follow-up candidates now map to a real, open GitHub issue. Seven issues were created by this
pass; two already existed.

## Disposition table — nine rows, all resolved

| # | Candidate | Type | Work mode | Issue |
|---|---|---|---|---|
| 1 | `QfcCollectionController.cs` exceeds the 500-line cap | pre-existing | n/a | [#623](https://github.com/drmoisan/TaskMaster/issues/623) |
| 2 | Remove the `stackMovedItems` parameter entirely | refactor | full-feature | [#629](https://github.com/drmoisan/TaskMaster/issues/629) |
| 3 | Relocate the `ReadyForMove` presentation to the caller | refactor | full-feature | [#630](https://github.com/drmoisan/TaskMaster/issues/630) |
| 4 | Consolidate `IFilerFormController` and `IQfcFormController` | refactor | full-feature | [#631](https://github.com/drmoisan/TaskMaster/issues/631) |
| 5 | Remove the orphan `QuickFiler.Interfaces.IQfcFormController` | refactor | full-feature | [#632](https://github.com/drmoisan/TaskMaster/issues/632) |
| 6 | Harden `KbdActions(IEnumerable<UClass>)` with the duplicate check | pre-existing | n/a | [#444](https://github.com/drmoisan/TaskMaster/issues/444) |
| 7 | File the unsynchronized undo handoff | bug | full-bug | [#633](https://github.com/drmoisan/TaskMaster/issues/633) |
| 8 | Revisit the unsynchronized plain read of the re-entrancy counter | bug | full-bug | [#634](https://github.com/drmoisan/TaskMaster/issues/634) |
| 9 | Settle the #468 residual reflective-caller risk repository-wide | bug | full-bug | [#635](https://github.com/drmoisan/TaskMaster/issues/635) |

Exactly nine rows. Zero rows carry `PROMOTION_DEFERRED`.

## Receipts

Each of the seven new operations returned `ok: true` with a `destination_path` under
`docs/features/potential/promoted/`. All seven source entries were moved out of
`docs/features/potential/` by the lifecycle, so no unpromoted residue remains for these candidates.

| Issue | destination_path (repository-relative) |
|---|---|
| #629 | `docs/features/potential/promoted/2026-08-26-qfc-remove-stackmoveditems-parameter.md` |
| #630 | `docs/features/potential/promoted/2026-08-26-qfc-relocate-readyformove-presentation-to-caller.md` |
| #631 | `docs/features/potential/promoted/2026-08-26-consolidate-ifilerformcontroller-and-iqfcformcontroller.md` |
| #632 | `docs/features/potential/promoted/2026-08-26-remove-orphan-quickfiler-interfaces-iqfcformcontroller.md` |
| #633 | `docs/features/potential/promoted/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move.md` |
| #634 | `docs/features/potential/promoted/2026-08-26-qfc-unsynchronized-plain-read-reentrancy-counter.md` |
| #635 | `docs/features/potential/promoted/2026-08-26-issue-468-residual-reflective-caller-risk.md` |

## Known tool defect, and the compensating action taken

The lifecycle tool retains only the potential file's `## Summary` section when it composes the issue
body. Every other section is rendered as the literal `(not provided in potential file)`. This was
verified directly against #629, whose body carries five such placeholder sections while its source
entry is 59 lines of substantive content.

Compensating action: the complete entry was posted as a comment on each of the seven new issues with
`gh issue comment --body-file`, so no analysis is lost. Each issue therefore carries its full problem
statement, proposed behavior, acceptance criteria, constraints and test conditions in its first
comment even though the generated body does not.

## Effect on AC-29

AC-29 reads: "Every entry in `## Follow-up Candidates` is promoted through the potential-to-issue
lifecycle, with the resulting issue numbers recorded in the feature folder."

Both halves are now satisfied: every candidate went through the lifecycle (or already owned an open
issue), and the resulting numbers are recorded in this artifact inside the feature folder. AC-29 is
checked off on this evidence.
