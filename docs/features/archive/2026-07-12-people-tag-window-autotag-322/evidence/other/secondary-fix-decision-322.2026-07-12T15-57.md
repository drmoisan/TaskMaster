Timestamp: 2026-07-12T15-57

# Secondary fix decision — `Tags/TagController.cs` `ResolveMailItem`

Per the P1-T1 diagnosis (`evidence/other/root-cause-322.2026-07-12T15-57.md`, section (c)), the
verdict was **CONFIRMED BLOCKING**: `ResolveMailItem`'s raw `objItem is MailItem` check does not
recognize the `IOutlookItem` wrapper that `AssignPeople()` now passes after the P1-T4 fix. Without
extending it, `_isMail` would remain `false` for the People flow and `SetAutoAssignState` would
hide/disable the auto-assign button entirely — a regression, and a failure to satisfy AC3/AC4.

**Outcome applied**: extended `ResolveMailItem` to also accept an `IOutlookItem` whose
`GetOlItemType() == OlItemType.olMailItem`, returning its `InnerObject` cast to `MailItem`,
mirroring `AutoAssignPeople.AutoFind`'s own branch pattern (`AutoAssignPeople.cs:70-76`).

## Diff scope

The `Tags/TagController.cs` diff is limited to:
1. Adding `using UtilitiesCS.OutlookExtensions;` (required for the `GetOlItemType()` extension
   method).
2. Adding one new `else if` branch inside `ResolveMailItem`:

```csharp
public MailItem ResolveMailItem(object objItem) //internal
{
    if ((objItem is not null) && (objItem is MailItem))
    {
        return (MailItem)_objItem;
    }
    else if (
        objItem is IOutlookItem olItem
        && olItem.GetOlItemType() == OlItemType.olMailItem
    )
    {
        return olItem.InnerObject as MailItem;
    }
    else
        return null;
}
```

No other member of `TagController` was changed. The existing first branch (raw `MailItem`) and
final `else` (null) are unchanged, preserving the existing
`ResolveMailItem_ReturnsMailForMailItemAndNullOtherwise` test
(`Tags.Test/TagControllerSeamTests.cs:30-51`) exactly: `mail` still hits the first branch,
`"not a mail"` and `null` still fall through to the final `else` (neither is an `IOutlookItem`).
