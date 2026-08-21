# qfc-item-controller-expansion-registry-divergence (Issue #482)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-expansion-registry-divergence/ (Issue #482)
- Discovered during: preparation research for epic #136 child F10 (issue #453)

- Issue: #482
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/482
- Last Updated: 2026-08-08
## Summary

`QfcItemController`'s synchronous and asynchronous expansion paths maintain disjoint keyboard-action
registries for the `'B'` and `'D'` keys while sharing a single `_expanded` state flag. Interleaving
the two paths drives the flag and the registries out of agreement, and the next registration throws
`ArgumentException` because `KbdActions.Add` is not idempotent.

## Affected Code

- `QuickFiler/Controllers/QfcItemController.Navigation.cs` — synchronous `ToggleExpansion()` and
  asynchronous `ToggleExpansionAsync()` register and unregister against separate registries but
  share `_expanded`.
- `QuickFiler/Controllers/KbdActions.cs:90-104` — `Add` throws rather than no-ops on a duplicate
  `(sourceId, key)` pair:

```csharp
public void Add(string sourceId, TKey key, VDelegate @delegate)
{
    if (_list.Any(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key)))
    {
        string message =
            $"Cannot add key because it already exists. Key {key} SourceId {sourceId}";
        logger.Error(message);
        throw new ArgumentException(message);
    }
    ...
}
```

- `QuickFiler/Controllers/QfcCollectionController.cs:1439` —
  `ActivateBySelectionAsync` calls the *synchronous* `ToggleExpansion()` from an asynchronous
  activation path, which is what makes the interleaving reachable in production rather than
  theoretical.

## Reproduction Sequence

1. Expand via the synchronous path (registers `'B'`/`'D'` in the sync registry, sets `_expanded`).
2. Collapse via the asynchronous path (unregisters from the async registry, which does not hold
   those entries, and clears `_expanded`).
3. Expand again via the synchronous path.

The third step re-registers `'B'`/`'D'` while the sync registry still holds them from step 1, and
`KbdActions.Add` throws `ArgumentException`.

## Compounding Factor

`KbdActions.Remove` returns `bool` to signal whether anything was removed, and all 30 call sites in
the codebase discard the return value. A failed unregistration is therefore silent, which is why the
registries can diverge without any diagnostic until the exception surfaces.

## Suspected Fix

Unify the two registries behind a single registration owner keyed on the actual current state rather
than on which code path performed the toggle, and either make `Add` idempotent or make the callers
check `Remove`'s result. Note that making `Add` idempotent is a contract change affecting all
consumers and interacts with #444.

## Severity

Medium-High. Reachable unhandled `ArgumentException` from ordinary user interaction with expansion.

## Related

- #444 — `KbdActions` enumerable constructor bypasses the duplicate guard (same class, different
  entry point).
- #445 — QuickFiler keyboard-action contract defects.

## Scope

Out of scope for epic #136 child F10, whose NFR prohibits behavior change to observable QuickFiler
flows, and whose file assignment excludes `KbdActions.cs` (F3 / #430) and
`QfcCollectionController.cs` (F11 / #454). Filed for independent scheduling with an explicit note
that a fix spans three children's file assignments.
