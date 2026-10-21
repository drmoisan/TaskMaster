# Phase 4 — Post-Format Sweep of the Phase 2 and Phase 3 Gates (Issue #895)

Timestamp: 2026-09-17T01-29
Task: [P4-T12]
WORKTREE-LEAF: agent-a8bc4dc5978785885

`[P4-T5]` runs the formatter over the whole tree, so the Phase 2 and Phase 3 measurements are
re-taken here against the post-format state.

EXIT_CODE: 0
ExpectedExitCode: 0

## Output Summary:

```
QuickFiler/QuickFiler.csproj NS21=0 NS20=1
QuickFiler.Test/QuickFiler.Test.csproj NS21=0 NS20=1
ToDoModel/ToDoModel.csproj NS21=0 NS20=1
UtilitiesCS/UtilitiesCS.csproj NS21=0 NS20=1
UtilitiesCS.Test/UtilitiesCS.Test.csproj NS21=0 NS20=1
ToDoModel.Test/ToDoModel.Test.csproj NS21=0 NS20=1
--- SIX-FILE NUMSTAT ---
1	1	QuickFiler.Test/QuickFiler.Test.csproj
1	1	QuickFiler/QuickFiler.csproj
1	1	ToDoModel/ToDoModel.csproj
--- P3-T2 PAYLOAD ---
UNSATISFIABLE_COUNT=1
DISPLAY_NAME_TESTS_COUNT=1
DONOTPARALLELIZE_COUNT=2
TESTMETHOD_COUNT=9
BECAUSE_206_COUNT=1
NETSTANDARDBIND_LINES=470
CHANGED_LINES=18
NON_COMMENT_CHANGED_LINES=0
11	7	TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs
--- FILE SIZE AND PARALLELISM ---
TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs LINES=211 DONOTPARALLELIZE=0
TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs LINES=244 DONOTPARALLELIZE=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs LINES=470 DONOTPARALLELIZE=2
```

## Acceptance

**Every `[P2-T4]` clause holds again.** All six census lines read `NS21=0 NS20=1`; the numstat output
is exactly the three `1	1` lines for the three edited project files, with no line for
`UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` or
`ToDoModel.Test/ToDoModel.Test.csproj`. The formatter did not reach the project files, which is
expected: `.csharpierignore` line 12 keeps `*.csproj` out of its scope.

**Every `[P3-T2]` clause holds again**, with the same values as before the format step:
`UNSATISFIABLE_COUNT=1`, `DISPLAY_NAME_TESTS_COUNT=1`, `DONOTPARALLELIZE_COUNT=2`,
`TESTMETHOD_COUNT=9`, `BECAUSE_206_COUNT=1`, `NETSTANDARDBIND_LINES=470`,
`NON_COMMENT_CHANGED_LINES=0`. `[P4-T5]` recorded `REWRITTEN-COUNT: 0`, so the exemption for
differing post-format figures does not apply and none is needed: the pre-format and post-format
values are identical. `CHANGED_LINES` reads 18 and the numstat reads `11	7`, which is the same
deviation from the plan's literals recorded and explained at `[P3-T2]`, unchanged by the format step.

**File size.** Each of the three files is at most 500 lines: 211, 244 and 470.

**Parallelism.** `DONOTPARALLELIZE` reads 0, 0 and 2 respectively. The two new classes carry no
serialisation attribute, and the pre-existing count of 2 on `NetstandardBindChildDomainTests`, placed
by issue #879, is unchanged. Nothing was serialised by this change.
