# Regression Test — Fail Before Fix

Evidence that both P1-T1 and P1-T2 regression tests fail before the implementation fix is applied.

---

## P1-T1 — RegisterFocusAsyncActions_RightArrowKey_IsRegisteredInKeyActionsAsync

Timestamp: 2026-03-25T10:57:19Z
Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_KeyboardRegistration"
EXIT_CODE: 1

Output Summary:

```
  Failed RegisterFocusAsyncActions_RightArrowKey_IsRegisteredInKeyActionsAsync [313 ms]
  Error Message:
   Expected keyActionsAsync.ContainsKey(Keys.Right) to be True because Keys.Right must be registered
   in KeyActionsAsync so that the keyboard handler intercepts the key press and expands the conversation
   instead of routing it to the mailto: control, but found False.
  Stack Trace:
     at QuickFiler.Controllers.Tests.QfcItemController_KeyboardRegistrationTests
        .RegisterFocusAsyncActions_RightArrowKey_IsRegisteredInKeyActionsAsync()
        in QfcItemControllerTests.cs:line 265
```

---

## P1-T2 — UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowFromKeyActionsAsync

Timestamp: 2026-03-25T10:57:19Z
Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_KeyboardRegistration"
EXIT_CODE: 1

Output Summary:

```
  Failed UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowFromKeyActionsAsync [1 ms]
  Error Message:
   Expected keyActionsAsync.ContainsKey(Keys.Right) to be True because precondition — right key must
   be registered before cleanup, but found False.
  Stack Trace:
     at QuickFiler.Controllers.Tests.QfcItemController_KeyboardRegistrationTests
        .UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowFromKeyActionsAsync()
        in QfcItemControllerTests.cs:line 296
```

---

## Combined Run Summary

Total tests: 2  
Failed: 2  
Passed: 0  
Total time: 1.1581 Seconds  

Both failures confirm that `Keys.Right` is not registered in `KeyActionsAsync` by `RegisterFocusAsyncActions()` at the
pre-fix state. The fix (P1-T3) must add `Keys.Right → ToggleExpansionAsync(On)` to `RegisterFocusAsyncActions()`
and remove it in `UnregisterFocusAsyncActions()`.
