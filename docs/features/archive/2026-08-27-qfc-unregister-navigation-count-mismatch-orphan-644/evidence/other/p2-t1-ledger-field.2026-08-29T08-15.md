# Production fix — Key ledger field added ([P2-T1])

- Issue: #644
- Task: `[P2-T1]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler/Controllers/QfcCollectionController.cs`

## Edit made

Inserted into the private field block immediately after the `_digits` declaration:

```csharp
// Issue #644: the exact (SourceId, Key) pairs the last RegisterNavigation added, so an
// _itemGroups mutation between register and unregister cannot orphan a registration.
private List<(string SourceId, string Key)> _registeredNavigationKeys;

private List<(string SourceId, string Key)> RegisteredNavigationKeys =>
    _registeredNavigationKeys ??= new List<(string SourceId, string Key)>();
```

## Design points this edit satisfies

- **Value tuple, not `record` or `init`.** `QuickFiler/QuickFiler.csproj` targets
  `v4.8.1`, which has no `IsExternalInit` polyfill, so `init` accessors, `record`, and
  `record struct` all fail `CS0518` on this framework. `System.ValueTuple` is referenced by the
  project, so `(string SourceId, string Key)` is available.
- **The lazy `??=` accessor is required, not stylistic.** Every test instance in this repository is
  built with `FormatterServices.GetUninitializedObject`, which bypasses field initialisers, so the
  ledger field is null on every reflection-built controller. Without the lazy accessor, `[P2-T3]`'s
  `foreach` would raise `NullReferenceException` on exactly the instances the new tests use. The
  `??=` idiom is already used twice in this same file, for the `_removeGroupByEntryId` and
  `_notifyNotReady` seams, so this matches the file's existing style. `<LangVersion>preview</LangVersion>`
  makes `??=` available.
- **Private, controller-scoped state.** The field and its accessor are both `private`. No
  interface is edited and no other type reads the ledger, so the public surface is unchanged.

## Acceptance verification

Command: `git grep -F -n '_registeredNavigationKeys' -- QuickFiler/Controllers/QfcCollectionController.cs`
EXIT_CODE: 0

```
QuickFiler/Controllers/QfcCollectionController.cs:122:        private List<(string SourceId, string Key)> _registeredNavigationKeys;
QuickFiler/Controllers/QfcCollectionController.cs:125:            _registeredNavigationKeys ??= new List<(string SourceId, string Key)>();
```

The command exits 0 and prints **exactly two lines**, as the acceptance requires: the field
declaration and its single use inside the lazy accessor.

Output Summary: The `_registeredNavigationKeys` field and its `RegisteredNavigationKeys` lazy
accessor were added to the private field block of `QuickFiler/Controllers/QfcCollectionController.cs`
with a `// Issue #644:` comment stating the invariant. The fixed-string search exits 0 and prints
exactly two lines. `_registeredDigits` is still present at this point; `[P2-T3]` deletes it
together with its assignment and the `format` expression in one indivisible edit.
