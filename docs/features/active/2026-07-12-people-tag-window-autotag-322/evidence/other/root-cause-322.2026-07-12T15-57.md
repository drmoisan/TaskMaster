Timestamp: 2026-07-12T15-57

# Root cause — issue #322 (People tag-assignment window auto-tag not invoked)

## (a) Argument divergence between `AssignPeople()` and its siblings

`TaskVisualization/TaskController.Actions.cs:46` — `AssignPeople()` passes
`objItemObject: _active.OlItem.InnerObject` (the raw, unwrapped Outlook COM object) to the shared
`TagPromptRequest`.

By contrast, all three sibling methods pass the `IOutlookItem` wrapper itself, not its
`InnerObject`:
- `TaskVisualization/TaskController.Actions.cs:81` — `AssignContext()`: `objItemObject: _active.OlItem`
- `TaskVisualization/TaskController.Actions.cs:113` — `AssignProject()`: `objItemObject: _active.OlItem`
- `TaskVisualization/TaskController.Actions.cs:151` — `AssignTopic()`: `objItemObject: _active.OlItem`

`_active.OlItem` (`ToDoModel/Data Model/ToDo/ToDoItem.cs:406`, `public IOutlookItem OlItem =>
FlaggableItem;`) is an `OutlookItemFlaggable` instance implementing `IOutlookItem`
(`UtilitiesCS/Interfaces/IReusableTypeClasses/IOutlookItem.cs:6-59`). `IOutlookItem` does not
extend or implement the Outlook interop `MailItem` interface. `.InnerObject`
(`UtilitiesCS/OutlookObjects/Item/OutlookItem.cs:176`, `InnerObject => this._item;`) returns the
raw underlying COM object the wrapper was constructed from.

## (b) Which `AutoAssignPeople.AutoFind` branch each argument reaches

`TaskVisualization/AutoAssignPeople.cs:59-87`:

```
IList<string> AutoFind(object objItem)
{
    if (objItem is null) return [];                                          // 62-65
    else if (objItem is MailItemHelper) helper = objItem as MailItemHelper;   // 66-69
    else if (objItem is IOutlookItem olItem
             && olItem.GetOlItemType() == OlItemType.olMailItem)              // 70-76
        helper = _toHelper(olItem.InnerObject);
    else if (objItem is MailItem olMail) helper = _toHelper(olMail);          // 77-80
    else return [];                                                          // 81-84 (silent, no logging)
    return RunPeopleClassifier(helper);
}
```

- The **current** People argument (`_active.OlItem.InnerObject`, a plain `object` whose declared
  static type carries no `IOutlookItem`/`MailItem` type information beyond its runtime shape) can
  only be routed by the raw `objItem is MailItem` branch (lines 77-80). It never reaches the
  dedicated `IOutlookItem`-wrapped-mail branch (lines 70-76), because `.InnerObject` is unwrapped
  before it ever reaches `AutoFind` — the type information that would satisfy `objItem is
  IOutlookItem` no longer exists on the argument once `.InnerObject` has been read. Any runtime
  shape other than an interop `MailItem` (for example, if the active item's underlying object is
  not exactly a `MailItem` RCW) falls through to the silent `else` (lines 81-84) and returns `[]`
  with no logging — auto-tagging silently does nothing, matching the reported symptom.
- The **corrected** argument (`_active.OlItem`, the `IOutlookItem` wrapper, matching
  Context/Project/Topic) is routed by the dedicated `IOutlookItem` branch (lines 70-76), which
  explicitly calls `olItem.GetOlItemType()` (`UtilitiesCS/OutlookObjects/Item/OutlookItemExtensions.cs:79-99`,
  itself implemented as `item.InnerObject is MailItem`) before invoking the classifier — the
  purpose-built branch for exactly this wrapper shape, restoring parity with the working
  Context/Project/Topic argument pattern.

## (c) `TagController.ResolveMailItem`/`_isMail` gate verdict for the corrected wrapper argument — CONFIRMED BLOCKING

`Tags/TagController.cs:100-108`:

```
public MailItem ResolveMailItem(object objItem)
{
    if ((objItem is not null) && (objItem is MailItem))
        return (MailItem)_objItem;
    else
        return null;
}
```

This is a raw interop-type check: `objItem is MailItem`. `Tags/TagController.cs:50-55` sets
`_olMail = ResolveMailItem(_objItem)` and `_isMail = true` only when `_olMail is not null`.
`Tags/TagController.cs:115-128` (`SetAutoAssignState`) hides/disables the viewer's auto-assign
button unless `autoAssigner is not null & _isMail` (line 118).

If `AssignPeople()` is changed to pass the `IOutlookItem` wrapper (per (a)/(b) above), `_objItem`
in the constructed `TagController` becomes the wrapper instance, not a raw `MailItem`. Because
`OutlookItem`/`OutlookItemFlaggable` implement only `IOutlookItem`
(`UtilitiesCS/OutlookObjects/Item/OutlookItem.cs:14`, `public class OutlookItem : IOutlookItem`) —
never the interop `MailItem` interface — the check `objItem is MailItem` evaluates **false** for
the wrapper. `ResolveMailItem` would then return `null`, `_isMail` would remain `false`, and
`SetAutoAssignState` would hide/disable the auto-assign button entirely for the People flow, which
would be a regression (worse than today, where the button is at least shown) and would not satisfy
AC3/AC4.

**Verdict: CONFIRMED BLOCKING.** The `ResolveMailItem`/`_isMail` gate must also be extended to
recognize an `IOutlookItem`-wrapped mail item (mirroring `AutoAssignPeople.AutoFind`'s own branch
pattern at `AutoAssignPeople.cs:70-76`) for the corrected wrapper argument to actually enable the
auto-assign button. This secondary fix is implemented in Phase 1 task P1-T5.

## (d) Confirmed primary root-cause statement

`AssignPeople()` (`TaskVisualization/TaskController.Actions.cs:46`) is the only one of the four
tag-assignment methods that passes the raw `.InnerObject` instead of the `IOutlookItem` wrapper
(`_active.OlItem`) to the shared `TagPromptRequest`/`TagController` seam. This argument-type
inconsistency causes the People auto-assign flow to bypass `AutoAssignPeople.AutoFind`'s dedicated
`IOutlookItem`-wrapped-mail branch, and (as a distinct, secondary, blocking defect) is
independently masked from ever mattering downstream by `TagController.ResolveMailItem`'s
raw-`MailItem`-only type check, which does not recognize the `IOutlookItem` wrapper and would hide
the auto-assign button entirely if the primary argument were corrected in isolation. Both the
primary argument fix (P1-T4) and the secondary `ResolveMailItem` extension (P1-T5) are required to
restore the People auto-tag path to working parity with Context/Project/Topic.
