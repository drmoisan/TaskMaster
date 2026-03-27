# Baseline Targeted Test (Remediation: issue-96 2026-03-26T15-25)

Timestamp: 2026-03-26T15:39:00Z

Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController"

EXIT_CODE: 0

## Output Summary

Test Run Successful. Total tests: 6, Passed: 6.

The targeted issue #96 keyboard-registration tests are present and pass in the current branch:
- `RegisterFocusAsyncActions_RightArrowKey_IsRegisteredInKeyActionsAsync` [168 ms] — Passed
- `UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowFromKeyActionsAsync` [2 ms] — Passed

Additional QfcItemController tests also passing:
- `LoadConversationResolverAsync_WhenLoadThrowsOperationCanceled_PropagatesCancellation` — Passed
- `LoadConversationResolverAsync_WhenLoadThrowsNonCancellation_DoesNotThrow` — Passed
- `PopulateConversationAsync_WhenLoadCanceledDuringAsync_ThrowsOperationCanceledNotNullRef` — Passed
- `PopulateConversationAsync_WhenLoadFailsWithNonCancellation_ReturnsWithoutCrash` — Passed

These tests exist in the current mixed branch and must also pass on the clean issue #96 branch after cherry-pick replay.
