# Phase 1 — Reported Reproduction (expect-fail) (Issue #232)

Timestamp: 2026-07-03T11-52
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix
EXIT_CODE: 1 (test failed — expected outcome for this [expect-fail] task)
Output Summary: Total tests 1; Failed 1 (LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix). Failure message: "Expected a <System.ArgumentException> to be thrown, but no exception was thrown." Pre-fix LoadControlsAndHandlers_01 calls ActivateQueuedItemGroups directly and performs no UnregisterNavigation()/RegisterNavigation(), so the injected orphan key "2" is never re-added at this call boundary and no ArgumentException is raised. The failure is traceable to the missing register/unregister pairing (the defect under repair), satisfying the bugfix-workflow fail-before requirement.
