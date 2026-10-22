# Cycle 2 P3-T2: delivered-wait fail-before (expect-fail)

Timestamp: 2026-10-22T13-58
Command: vstest.console.exe QuickFiler.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~EfcFormControllerTests (assembly path excludes the .claude worktree tree) before the CreateIssue1002Controller change
EXIT_CODE: 1
Output Summary:
- total 45, executed 45, passed 44, failed 1 (expected failure; assertion failure, no hang).
- Failing: BindFolderRows_FaultBeforeDelivery_FaultsDeliveredTaskPromptly ("Expected delivered.Task.IsFaulted to be True because a pre-delivery fault must fail the wait instead of leaving it pending, but found False.").
- The 44 pre-existing EfcFormControllerTests tests pass.
