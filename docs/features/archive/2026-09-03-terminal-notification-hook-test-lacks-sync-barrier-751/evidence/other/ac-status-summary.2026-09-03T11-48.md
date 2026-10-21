# P5-T12 — Acceptance Criteria Status Summary (Issue #751)

Timestamp: 2026-09-03T14-52

AC source: `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md`,
`## Acceptance Criteria` section. Work mode is `full-bug` (`issue.md:12`), so `spec.md` is the sole
acceptance-criteria source and no `user-story.md` is produced.

Ten criteria, referred to as AC1 through AC10 by their order in that section.

| AC | Verbatim first line from `spec.md` | Satisfying tasks | Evidence artifact | State |
|---|---|---|---|---|
| AC1 | "The barrier assertion `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);` is" | P2-T1, P5-T1 | `evidence/qa-gates/barrier-assertion-check.2026-09-03T11-48.md` | **checked** |
| AC2 | "The terminal-hook count assertion in that test still expects the value 1 (it is not relaxed, widened" | P2-T2, P5-T2 | `evidence/qa-gates/barrier-assertion-check.2026-09-03T11-48.md` | **checked** |
| AC3 | "Every read and write of `InvokedTerminalHookCount` across" | P2-T3, P2-T4, P4-T10 | `evidence/qa-gates/counter-access-audit.2026-09-03T11-48.md`; `evidence/qa-gates/no-new-member-scan.2026-09-03T11-48.md` | **checked** |
| AC4 | "TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs is byte-identical to its state at branch point" | P4-T8 | `evidence/qa-gates/footprint-scope.2026-09-03T11-48.md` | **checked** |
| AC5 | "The repaired test passes on every run of a repeat-run series executed under the CI-shaped invocation" | P3-T2, P3-T3 | `evidence/qa-gates/repeat-run-comparison.2026-09-03T11-48.md`; `evidence/qa-gates/repeat-run-1.2026-09-03T11-48.md` through `repeat-run-5.2026-09-03T11-48.md` | **checked** |
| AC6 | "Exactly one fail-before route from the Test Strategy is executed and its artifact is committed under" | P1-T1, P1-T2, P4-T10 | `evidence/regression-testing/fail-before-route-selection.2026-09-03T11-48.md`; `evidence/regression-testing/no-fail-before-rationale.2026-09-03T11-48.md` | **checked** |
| AC7 | "After the change, neither `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` nor" | P4-T7, P4-T10 | `evidence/qa-gates/file-size-audit.2026-09-03T11-48.md`; `evidence/qa-gates/no-new-member-scan.2026-09-03T11-48.md` | **checked** |
| AC8 | "No banned determinism API (`Thread.Sleep`, `Task.Delay`, wall-clock wait, polling loop," | P4-T9 | `evidence/qa-gates/determinism-api-scan.2026-09-03T11-48.md` | **checked** |
| AC9 | "A full toolchain pass completes cleanly in a single final pass, in order: `csharpier check` reports no" | P4-T2, P4-T5, P4-T6 | `evidence/qa-gates/toolchain-clean-pass.2026-09-03T11-48.md`; `evidence/qa-gates/csharpier-check.2026-09-03T11-48.md`; `evidence/qa-gates/mstest-full-suite.2026-09-03T11-48.md` | **checked** |
| AC10 | "The issue.md checklist items under \"Proposed Fix / Validation Ideas\" are reconciled: the trace item" | P5-T11 | `evidence/issue-updates/issue-751.2026-09-03T11-48.md` | **checked** |

All artifact paths above are relative to
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/` and exist on disk.

## Summary

```
### Acceptance Criteria Status
- Source: docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md
- Total AC items: 10
- Checked off (delivered): 10
- Remaining (unchecked): 0
- Items remaining: none
```

## Unchecked items and their recorded reasons

None. All ten criteria are checked.

**AC9 note.** P5-T9 recorded **Outcome A**. Both non-A conditions were evaluated and neither held:

- Outcome B requires P4-T2 to have recorded rung 2 (pre-existing repository-wide csharpier drift). P4-T2
  recorded **rung 1**: `dotnet tool run csharpier check .` exited 0 across 1574 files and named no
  unformatted file.
- Outcome C requires the P4-T6 artifact to record a non-zero exit code for the P4-T5 step arising from a
  non-empty failed-name set. P4-T6 records exit code **0** for all five commands, and P4-T5 recorded a
  failed-name set that is **empty**.

## Item outside the AC set — coverage capture

The Coverage Evidence Contract item is **remediation-required**, and it is recorded here for completeness
even though it is not one of the ten acceptance criteria and does not affect any AC state.

P0-T17 and P4-T12 both reached **rung 3** and recorded `COVERAGE_CAPTURE_BLOCKED`, because the prescribed
locate command returned **2** `.coverage` files rather than the required exactly one, on both the baseline
run and the final-QC run. The plan forbids converting an arbitrary member of the set, so no numeric pair was
produced and no figure was fabricated. P4-T12 therefore recorded the **Blocked outcome**.

The no-regression obligation itself is discharged by P4-T11, which observed against the actual post-change
branch diff that the number of changed production lines on this branch is **zero**, so the changed-line
no-regression requirement has an empty subject.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| The summary has exactly ten rows, one per `spec.md` acceptance criterion | 10 rows, AC1 through AC10 | PASS |
| Every row names at least one artifact path that exists on disk | every row names one or more existing artifacts | PASS |
| Any row whose state is unchecked carries the reason recorded by the corresponding P5 check-off task | no row is unchecked | PASS (vacuous) |
