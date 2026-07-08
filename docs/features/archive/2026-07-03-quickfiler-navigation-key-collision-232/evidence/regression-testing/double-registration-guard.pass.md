# Phase 3 — Double-Registration Guard (pass) (Issue #232)

Timestamp: 2026-07-03T12-05
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException,SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey
EXIT_CODE: 0
Output Summary: Total tests 2; Passed 2.
- RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException (49 ms): proves KbdActions.Add's throw-on-duplicate contract — registering the same 2-item page twice without an intervening unregister throws ArgumentException containing "SourceId Collection". This is the hazard the Phase 2 trailing-register guard prevents.
- SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey (1 ms): confirms AC3 — the guarded zero-item flow (unregister outgoing, drop item, swap in cached 2-item page) throws nothing and leaves exactly one "Collection" entry per incoming key "1" and "2" (no duplicates).
