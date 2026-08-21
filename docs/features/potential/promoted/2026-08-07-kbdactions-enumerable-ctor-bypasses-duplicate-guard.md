# kbdactions-enumerable-ctor-bypasses-duplicate-guard (Issue #444)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/kbdactions-enumerable-ctor-bypasses-duplicate-guard/ (Issue #444)
- Discovered during: research for issue #430 (`quickfiler-keyboard-actions-coverage`, child F3 of epic #136)

- Issue: #444
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/444
- Last Updated: 2026-08-08
## Summary

`KbdActions<TKey, UClass, VDelegate>` enforces a duplicate-registration guard in `Add`, but the
`IEnumerable<UClass>` constructor bypasses it entirely. Production code registers a duplicate through
that constructor, which makes a subsequent lookup for that key throw `InvalidOperationException`.

## Evidence

The guard exists in `Add`:

```csharp
// QuickFiler/Controllers/KbdActions.cs:90-92
public void Add(string sourceId, TKey key, VDelegate @delegate)
{
    if (_list.Any(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key)))
```

The constructor has no equivalent check:

```csharp
// QuickFiler/Controllers/KbdActions.cs:25-28
public KbdActions(IEnumerable<UClass> list)
{
    _list = new List<UClass>(list);
}
```

Production registers a duplicate pair through that constructor — two entries sharing
`SourceId = "Collection"` and `Key = Keys.Down`:

```csharp
// QuickFiler/Controllers/QfcCollectionController.cs:1265-1272
_kbdHandler.KeyActions = new KbdActions<Keys, KaKey, Action<Keys>>(
    new List<KaKey>
    {
        new KaKey("Collection", Keys.Up, (k) => SelectPreviousItem()),
        new KaKey("Collection", Keys.Down, (k) => SelectNextItem()),
        new KaKey("Collection", Keys.Down, (k) => _parent.ActionOkAsync()),
    }
);
```

Had these been registered via `Add`, the third entry would have been rejected. Because they were not,
`Find(Keys.Down)` resolves against a two-element match set and throws `InvalidOperationException`
(`KbdActions.cs:67` / `:86`). The consuming call site is `QuickFiler/Controllers/KeyboardHandler.cs:122`.

Both the constructor bypass and the duplicate registration were verified by direct file read at
`origin/epic/quickfiler-per-file-coverage-integration` (base commit `56ca1cea`).

## Impact

`Keys.Down` handling in the QuickFiler collection surface resolves to an exception path rather than to
either registered action. The severity depends on whether `Find(Keys.Down)` is reached in normal
operation, which has not been confirmed by runtime observation — the analysis is static.

## Why this was not fixed in issue #430

Issue #430 (child F3) carries an explicit acceptance criterion of **no behavior change to observable
QuickFiler keyboard flows**, and `QfcCollectionController.cs` is assigned to sibling child F11
(`quickfiler-collection-controller-coverage`), not F3. Adding a guard to the constructor is a breaking
change for the existing call site. F3 characterizes the current behavior in tests rather than changing
it.

## Proposed Fix Direction

Two independent decisions are required, and they should be made together:

1. **Which of the two `Keys.Down` actions is correct?** `SelectNextItem()` and `_parent.ActionOkAsync()`
   are materially different behaviors. This is a product decision, not a mechanical fix.
2. **Should the `IEnumerable` constructor enforce the same duplicate guard as `Add`?** Making the
   constructor consistent with `Add` is the design-correct answer, but it converts the existing latent
   defect into a construction-time throw and therefore must land together with decision 1.

## Acceptance Criteria (early draft)

- [ ] The intended `Keys.Down` behavior for the QuickFiler collection surface is decided and recorded.
- [ ] The duplicate registration in `QfcCollectionController.cs` is resolved to a single entry.
- [ ] `KbdActions(IEnumerable<UClass>)` either enforces the same duplicate guard as `Add` or documents
      in-code why it deliberately does not.
- [ ] A regression test covers the chosen behavior, including the duplicate-input case.
- [ ] Full C# toolchain passes: csharpier, analyzer build, nullable build, coverage-enabled vstest.

## Next Step

- [ ] Promote to GitHub issue (bug template)
- [ ] Coordinate with epic #136 child F11 (`quickfiler-collection-controller-coverage`), which owns
      `QfcCollectionController.cs`
