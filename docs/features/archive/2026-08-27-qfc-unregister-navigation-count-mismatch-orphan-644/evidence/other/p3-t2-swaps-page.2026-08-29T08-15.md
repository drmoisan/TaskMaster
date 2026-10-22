# Test reconciliation — Swap-page arrangement ([P3-T2])

- Issue: #644
- Task: `[P3-T2]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
- Test amended: `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys`

## Edit made

The two arrangement lines `SeedCollectionKey(kbd, "1");` and `SeedCollectionKey(kbd, "2");` were
replaced with the single line `controller.RegisterNavigation();`, so the two-item outgoing page
registers and ledgers both of its keys through the real production path.

```csharp
            // Arrange: 2-item outgoing page with keys "1" and "2" registered.
            var controller = CreateControllerForSwap(outgoingItemCount: 2, out var kbd);
            controller.RegisterNavigation();
            var oneItemIncomingPage = MakeGroups(1);
```

## Why the edit is required

The test asserts that a page swap removes every outgoing `"Collection"` key. Under the ledger,
the swap's `UnregisterNavigation()` removes exactly the pairs the matching `RegisterNavigation()`
recorded, so out-of-band seeds are no longer keys the controller owns and would survive the swap.
Registering through `RegisterNavigation()` is what makes the outgoing page's keys ledgered and
therefore removable, which is precisely the behaviour the test was written to characterise.

## Acceptance verification — all three assertions unchanged

Read back from the edited method:

```csharp
            // Assert: no stale outgoing key remains; exactly one incoming key "1".
            CountCollectionKey(kbd, "2").Should().Be(0);
            CountCollectionKey(kbd, "1").Should().Be(1);
            kbd.Count(a => a.SourceId == "Collection").Should().Be(1);
```

All three assertions are byte-for-byte what they were before this edit. No `[TestMethod]` was
added or removed.

## Acceptance verification — the file is one line shorter than the baseline

Command: `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs).Count`

```
499
```

`[P0-T7]` baseline: **500**. Measured now: **499**. The file is **exactly one line shorter**, as
the acceptance requires. Two arrangement lines collapsed into one, and `[P3-T1]` and `[P3-T3]` are
each one-for-one replacements, so `-1` is the whole net delta for this file across Phase 3.

This matters because the file sits exactly at the 500-line repository ceiling and may not grow by
even one line. Moving downward keeps that constraint satisfied with margin.

EXIT_CODE: 0

Output Summary: Two `SeedCollectionKey` arrangement lines replaced with a single
`controller.RegisterNavigation();` call in
`LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys`. All three existing
assertions verified unchanged by reading the edited method. The file is now **499** lines, one
below the `[P0-T7]` baseline of 500.
