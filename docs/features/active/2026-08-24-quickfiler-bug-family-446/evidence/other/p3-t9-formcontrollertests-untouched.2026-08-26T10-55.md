# [P3-T9] `QfcFormControllerTests.cs` Is Absent from the Change Set (D-Plan-2, AC24)

Timestamp: 2026-08-26T10-55

Task: [P3-T9]
Feature: docs/features/active/quickfiler-bug-family-446

Merge base (`<mb>`, from `[P0-T3]`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

Command: `git diff --name-only 61edc19befcf6c4e95b5acd32542f2dcdab41b78...HEAD -- "QuickFiler.Test/Controllers/QfcFormControllerTests.cs"`
EXIT_CODE: 0
Output line count: **0**

Supplementary check, so the claim covers uncommitted state as well as committed state:

Command: `git status --porcelain -- "QuickFiler.Test/Controllers/QfcFormControllerTests.cs"`
EXIT_CODE: 0
Output line count: **0**

## Why this matters

`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 827 lines, already over the 500-line
cap. D-Plan-2 decided not to touch it: `spec.md` marks replacing the tautological
`UndoConsumer_ShouldConsumeUndoQueue` placeholder at `:687-701` as **optional**, an optional outcome
is not an atomic task, and editing an 827-line file would engage AC24's single permitted exception
for no acceptance-criteria gain. Leaving the file unmodified makes that exception vacuously
satisfied.

The four undo-consumer tests this phase added went to
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` instead, which `[P3-T7]` records at 496
lines, within the cap.

## Output Summary

The command produces **zero output lines**: the file is absent from the change set both as committed
diff against the merge base and as working-tree status.
