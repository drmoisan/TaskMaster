# Phase 3 — Swap Register/Unregister Order (pass) (Issue #232)

Timestamp: 2026-07-03T12-05
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys
EXIT_CODE: 0
Output Summary: Total tests 1; Passed 1 (LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys), 307 ms. Confirms AC1: swapping from a 2-item outgoing page (keys "1","2" registered) to a 1-item incoming page leaves zero stale "Collection" keys from the outgoing page and exactly one "Collection" key "1" for the incoming page.
