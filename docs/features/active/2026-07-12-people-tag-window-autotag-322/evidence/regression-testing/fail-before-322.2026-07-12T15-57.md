Timestamp: 2026-07-12T15-57
Command: vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignPeople_PassesOutlookItemWrapper_NotInnerObject" /InIsolation
EXIT_CODE: 1
Output Summary: `Total tests: 1`, `Failed: 1`. This is the expected fail-before result for the
`[expect-fail]` regression test, run before any production-code change (P1-T2, prior to P1-T4/P1-T5).

## Failure detail

```
Failed AssignPeople_PassesOutlookItemWrapper_NotInnerObject [1 s]
  Error Message:
   Expected captured.ObjItemObject to refer to UtilitiesCS.OutlookExtensions.OutlookItemFlaggable
   {..., InnerObject = Mock<MailItem:1>.Object, ...}, but found Mock<MailItem:1>.Object.
  Stack Trace:
     at TaskVisualization.Test.TaskControllerActionsTests.AssignPeople_PassesOutlookItemWrapper_NotInnerObject()
```

This confirms the diagnosed defect: `captured.ObjItemObject` (the argument `AssignPeople()`
actually passed) is the raw `Mock<MailItem>.Object` (i.e. `.InnerObject`), not
`controller.Active.OlItem` (the `OutlookItemFlaggable` wrapper), matching the root-cause finding in
`evidence/other/root-cause-322.2026-07-12T15-57.md`.
