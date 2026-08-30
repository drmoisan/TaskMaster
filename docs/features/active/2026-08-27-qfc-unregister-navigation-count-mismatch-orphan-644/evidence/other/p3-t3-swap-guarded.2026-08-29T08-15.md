# Test reconciliation — Swap-guarded arrangement and literal count gates ([P3-T3])

- Issue: #644
- Task: `[P3-T3]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
- Test amended: `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey`

## Edit made

The arrangement line `SeedCollectionKey(kbd, "1");` was replaced with
`controller.RegisterNavigation();`, so the one-item outgoing page registers and ledgers its key
through the real production path before the test's `controller.UnregisterNavigation()` Act step.

```csharp
            // Arrange: 1-item outgoing page with key "1" registered.
            var controller = CreateControllerForSwap(outgoingItemCount: 1, out var kbd);
            controller.RegisterNavigation();
            var twoItemCachedPage = MakeGroups(2);
```

No assertion was touched and no `[TestMethod]` was added or removed.

## Acceptance verification — the two literal count gates after `[P3-T1]` through `[P3-T3]`

Both literals are present in this file today, so both counts are measurable before and after the
three edits; neither gate is vacuous.

### `SeedCollectionKey` — exactly 2 matching lines

Command: `git grep -c -F 'SeedCollectionKey' -- QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
EXIT_CODE: 0

```
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:2
```

The two surviving lines, enumerated:

```
386:        private static void SeedCollectionKey(
414:            SeedCollectionKey(kbd, "2");
```

These are exactly the two the acceptance names — the helper's own declaration, and the one
surviving call retained by `[P3-T1]` to model the pre-existing out-of-band orphan. The count fell
from the `[P0-T7]`-era 6 to 2: `[P3-T1]` removed one call, `[P3-T2]` removed two, `[P3-T3]`
removed one.

`SeedCollectionKey` therefore remains used and does not become dead code, which is what the spec
requires.

### `controller.RegisterNavigation();` — exactly 5 matching lines

Command: `git grep -c -F 'controller.RegisterNavigation();' -- QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
EXIT_CODE: 0

```
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:5
```

The five lines, enumerated:

```
413:            controller.RegisterNavigation();                                    <- added by [P3-T1]
434:            controller.RegisterNavigation();                                    <- added by [P3-T2]
457:            controller.RegisterNavigation();                                    <- pre-existing
458:            System.Action secondRegister = () => controller.RegisterNavigation();  <- pre-existing
477:            controller.RegisterNavigation();                                    <- added by [P3-T3]
```

Two were present before Phase 3, in
`RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException`, which needs
no change and must keep passing. The three added by `[P3-T1]`, `[P3-T2]`, and `[P3-T3]` bring the
total to 5.

EXIT_CODE: 0

Output Summary: `SeedCollectionKey(kbd, "1");` replaced with `controller.RegisterNavigation();` in
`SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey`. After
`[P3-T1]` through `[P3-T3]`, `git grep -c -F 'SeedCollectionKey'` reports **exactly 2** matching
lines (the helper declaration on line 386 and the retained call on line 414) and
`git grep -c -F 'controller.RegisterNavigation();'` reports **exactly 5** matching lines. Both
`[P3-T3]` acceptance clauses hold.
