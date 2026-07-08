# LiveOutlook Skip-Guard Verification

Timestamp: 2026-06-22T21-15
Command: vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /TestCaseFilter:"TestCategory=LiveOutlook"
(vstest used: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe)
EXIT_CODE: 0

Output Summary:
- Total tests: 1; Passed: 1; Failed: 0. The single LiveOutlook harness test
  `LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold` ran for real and PASSED in 823 ms.
- This development machine HAS Outlook registered, so `new Outlook.Application()` succeeded, `skipReason`
  remained null, and the original real assertion path (captured == null, completed == true,
  maxTickBlockMs within threshold) executed unchanged. This directly verifies the "when Outlook is
  available it runs as before" half of AC-R1.
- Because Outlook IS registered here, the COMException-to-Inconclusive branch cannot be exercised on
  this machine. Per the plan (P2-T5), the skip path is therefore VERIFIED BY INSPECTION of the guard:
  - P1-T1: `IsOutlookUnavailableHResult(int hr)` returns true only for 0x80040154 (REGDB_E_CLASSNOTREG),
    0x80040112 (CLASS_E_NOTLICENSED), 0x80080005 (CO_E_SERVER_EXEC_FAILURE).
  - P1-T2: a `catch (COMException comEx) when (IsOutlookUnavailableHResult(comEx.ErrorCode))` filter
    sets `skipReason` (and does NOT set `captured`) ahead of the general `catch (Exception)`.
  - P1-T3: after `thread.Join()`, `if (skipReason != null) Assert.Inconclusive(...)` runs before the
    `captured`/`completed`/`maxTickBlockMs` assertions, so a no-Outlook environment reports Inconclusive
    (skipped), not Failed. The exact CI HRESULT (0x80040154 REGDB_E_CLASSNOTREG) is the first guarded value.
- The headless-CI negative branch (no Outlook -> Inconclusive) is exercised on CI regardless; AC-R4
  (green CI) is deferred to the orchestrator's post-push CI re-check.
