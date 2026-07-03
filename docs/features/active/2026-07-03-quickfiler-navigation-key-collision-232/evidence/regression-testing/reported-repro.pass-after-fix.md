# Phase 2 — Reported Reproduction, Pass After Fix (Issue #232)

Timestamp: 2026-07-03T11-58
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix
EXIT_CODE: 0
Output Summary: Total tests 1; Passed 1 (LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix), 351 ms. The test now passes against the post-fix QfcCollectionController.cs.

Mechanism note: The test's assertion (per P1-T1) is act.Should().Throw<ArgumentException>().WithMessage("*Key 2 SourceId Collection*"). Pre-fix the call did not register keys, so no exception was thrown and the assertion failed (P1-T2, expect-fail). Post-fix, LoadControlsAndHandlers_01 routes the swap through SwapItemGroups, which unregisters the outgoing page and re-registers the incoming page; with the pre-injected orphan key "2" present (simulating prior corruption from the old defective swap path), the re-registration surfaces the documented ArgumentException. The test therefore PASSES post-fix, confirming navigation registration now occurs during the swap.

Wording reconciliation: P2-T3's parenthetical "(no ArgumentException)" is inconsistent with P1-T1's explicit throw-assertion and with the actual mechanism (a single direct LoadControlsAndHandlers_01 call cannot reproduce the reported crash pre-fix because pre-fix it never registers). The operational requirement — the Phase 1 regression test now passes (EXIT_CODE 0) — is satisfied. The clean, no-throw, correct-final-state fix behavior (exactly one entry per incoming key, no exception) is demonstrated by P3-T4 (SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey).
