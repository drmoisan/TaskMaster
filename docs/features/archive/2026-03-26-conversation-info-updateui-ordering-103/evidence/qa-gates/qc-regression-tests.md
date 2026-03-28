# Phase 2 — QC Regression Tests Gate

- Timestamp: 2026-03-26T18:53:00
- Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~ConversationResolver"`
- EXIT_CODE: 0

## Output Summary

```
Passed: 8 / Failed: 0 / Total: 8
```

All 8 ConversationResolver tests passed including the 2 new regression tests:
1. `ConversationInfo_WhenNotSetAndCountIsZero_ThrowsInvalidOperationException` — documents the bug scenario (read before assignment throws)
2. `ConversationInfo_WhenSetBeforeAccessWithCountAtZero_ReturnsCachedValueWithoutThrowing` — validates the fix (cached value returned after setter call)

Pre-existing tests also passed:
- `LoadConversationInfo_WhenCountExpandedIsZero_ThrowsInvalidOperationExceptionNotStackOverflow`
- `ConversationInfoGetter_WhenCountExpandedIsZero_ThrowsInvalidOperationException`
- `Count_WhenZeroCountIsSetViaInternalSetter_SubsequentGetDoesNotInvokeLoadCount`
- `Count_WhenNotYetInitialized_AttemptsToLoadCount`
- `LoadConversationResolverAsync_WhenLoadThrowsOperationCanceled_PropagatesCancellation`
- `LoadConversationResolverAsync_WhenLoadThrowsNonCancellation_DoesNotThrow`

Regression tests gate: **PASSED**.
