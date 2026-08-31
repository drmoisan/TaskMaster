# Test reconciliation — Reported-repro arrangement ([P3-T1])

- Issue: #644
- Task: `[P3-T1]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
- Test amended: `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix`

## Edit made

The arrangement line `SeedCollectionKey(kbd, "1");` was replaced with
`controller.RegisterNavigation();`. The following `SeedCollectionKey(kbd, "2");` line is left
unchanged.

```csharp
            // Arrange
            var controller = CreateControllerForSwap(outgoingItemCount: 1, out var kbd);
            controller.RegisterNavigation();
            SeedCollectionKey(kbd, "2");
            var cachedTwoItemPage = MakeGroups(2);
```

## Why the edit is required, and why the surviving seed is retained

The one-item outgoing page now registers through the real production path, so key `"1"` is both
present in the registry **and** recorded in the ledger. Under the ledger the outgoing page's
unregistration during the swap drains exactly what it registered, so an out-of-band
`SeedCollectionKey(kbd, "1")` would no longer model a registration the controller owns.

The orphaned `"2"` is a different thing: it models a key left behind by an **earlier** page that
was abandoned through the pre-fix defective swap path, so it is deliberately still modelled out of
band and its seed is retained. That is what keeps the documented collision reachable when the
cached two-item page is swapped in and walks keys `"1"` and `"2"`.

## Acceptance verification — the assertion is unchanged

Read back from the edited method:

```csharp
            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Key 2 SourceId Collection*");
```

The assertion is byte-for-byte what it was before this edit. No `.Should()` chain, exception type,
or message pattern was touched, and no `[TestMethod]` was added or removed.

## Line accounting

Command: `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs).Count`

```
500
```

This edit is a one-for-one line replacement, so the file remains at the `[P0-T7]` baseline of 500
— exactly at the repository ceiling, which it may not exceed. The net `-1` for the file comes from
`[P3-T2]`, which collapses two seed lines into one register call.

EXIT_CODE: 0

Output Summary: `SeedCollectionKey(kbd, "1");` replaced with `controller.RegisterNavigation();` in
`LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix`, with the
`SeedCollectionKey(kbd, "2");` line retained to keep modelling the pre-existing orphan. The
`*Key 2 SourceId Collection*` message assertion is verified unchanged by reading the edited method.
File length remains 500 lines.
