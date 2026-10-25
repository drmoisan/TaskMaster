# Acceptance-Criteria Reconciliation (P7-T10)

Timestamp: 2026-10-22T16-38
Command: compare each `- [ ] ACn:` / `- [x] ACn:` line of <FEATURE>/spec.md, with the checkbox marker normalized, against the same line of the SCRATCH copy issue-1005-spec-baseline.md taken at P0-T1; count checked and unchecked AC lines
EXIT_CODE: 0
Output Summary: 8 checked, 0 unchecked. All 8 criterion texts are identical to the P0-T1 copy after normalizing the checkbox marker, and every non-AC line of spec.md is identical to the copy.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/spec.md` (Work Mode full-bug)
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Per-criterion record

| AC | Text identical to P0-T1 copy | Checked by | Evidence |
| --- | --- | --- | --- |
| AC1 | yes | P5-T3 | `regression-testing/dfdeedle-pass-after.2026-10-22T16-26.md`; `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` |
| AC2 | yes | P3-T5 | `regression-testing/idlist-probe-pass-after.2026-10-22T16-18.md`; `ToDoModel/Data Model/ID/IDList.cs` |
| AC3 | yes | P3-T6 | `regression-testing/idlist-probe-pass-after.2026-10-22T16-18.md` (zero probe saves, one mock `Save()`) |
| AC4 | yes | P6-T5 | `regression-testing/ac4-search-before.2026-10-22T16-12.md`, `regression-testing/ac4-search-after.2026-10-22T16-27.md` |
| AC5 | yes | P6-T6 | `regression-testing/settings-probe-fail-before.2026-10-22T16-15.md`, `regression-testing/fail-before-exception.2026-10-22T16-15.md`, `regression-testing/idlist-probe-pass-after.2026-10-22T16-18.md`, `regression-testing/taskmaster-settings-probe-pass-after.2026-10-22T16-25.md` |
| AC6 | yes | P7-T8 | `qa-gates/final-toolchain-tests.2026-10-22T16-35.md`, `qa-gates/parallel-repeat-runs.2026-10-22T16-30.md`, `qa-gates/parallel-constraints-scan.2026-10-22T16-28.md` |
| AC7 | yes | P7-T9 | `qa-gates/final-toolchain-format.2026-10-22T16-32.md`, `qa-gates/final-toolchain-analyzers.2026-10-22T16-32.md`, `qa-gates/final-toolchain-typecheck.2026-10-22T16-33.md`, `qa-gates/final-toolchain-tests.2026-10-22T16-35.md`, `qa-gates/coverage-comparison.2026-10-22T16-36.md`, `qa-gates/file-sizes.2026-10-22T16-37.md` |
| AC8 | yes | P4-T12 | `other/ribbon-static-state-check.2026-10-22T16-23.md` (verdict REMOVE); P4-T5 and P4-T10 removed both class-level attributes |

The comparison base is the P0-T1 `SCRATCH` copy because `spec.md` was untracked at planning time.
