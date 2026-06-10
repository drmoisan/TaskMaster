# Baseline — Re-Enabled Regression Tests (Cycle 3, downstream risk)

Timestamp: 2026-06-08T19-44

Command: git diff main..HEAD -- "ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs" "ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs"

EXIT_CODE: 0

Output Summary:
Commit 0883d0f7 commented out the `[TestCategory("ProductionBugSuspected")]` and
`[Ignore("ProductionBugSuspected")]` markers on two regression tests, re-enabling them:

1. ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs
   - Test: Constructor_WithOutlookItem_ShouldInitializeProperties
   - Diff (markers commented out):
       -        [TestCategory("ProductionBugSuspected")]
       -        [Ignore("ProductionBugSuspected")]
       +       //[TestCategory("ProductionBugSuspected")]   <- 7-space indent (formatting defect)
       +        //[Ignore("ProductionBugSuspected")]

2. ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs
   - Test: People_Deserialize_CanDeserializePatternCorrectly
   - Diff (markers commented out, correctly indented at 8 spaces, so not reported by CSharpier):
       -        [TestCategory("ProductionBugSuspected")]
       -        [Ignore("ProductionBugSuspected")]
       +        //[TestCategory("ProductionBugSuspected")]
       +        //[Ignore("ProductionBugSuspected")]

These two test identifiers are the downstream-risk baseline. After the formatting fix
unblocks the build/test CI steps, both must RUN (not Skipped/Ignored) and PASS, verified
locally in P2-T6. If either fails, escalate as a NEW finding per the scope-change rule;
do not re-ignore or weaken the tests.
