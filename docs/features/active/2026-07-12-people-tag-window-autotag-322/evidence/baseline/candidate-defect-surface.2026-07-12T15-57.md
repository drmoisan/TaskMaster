Timestamp: 2026-07-12T15-57

Capture only — no diagnosis conclusion is drawn here. The conclusion is produced in Phase 1
task P1-T1 (`evidence/other/root-cause-322.<TS>.md`).

Verbatim candidate defect-surface file:line citations from this plan's "Confirmed Facts" section:

- `TaskVisualization/TaskController.Actions.cs:25-56` — `AssignPeople()` constructs a
  `TagPromptRequest` whose `objItemObject` argument (line 46) is `_active.OlItem.InnerObject` (the
  raw, unwrapped Outlook COM object).
- `TaskVisualization/TaskController.Actions.cs:61-91, 93-126, 131-161` — the three sibling methods
  `AssignContext` (line 81), `AssignProject` (line 113), and `AssignTopic` (line 151) all pass
  `objItemObject: _active.OlItem` (the `IOutlookItem` wrapper itself, not `.InnerObject`).
- `TaskVisualization/TaskController.cs:311` — `_autoAssign` is a single shared `IAutoAssign` field
  used as the `autoAssigner` for People (`Actions.cs:42`), Context (`Actions.cs:77`), and Topic
  (`Actions.cs:147`); `AssignProject` (`Actions.cs:109`) uses a distinct `ProjectAssign` method.
- `Tags/TagController.cs:100-108` — `ResolveMailItem(object objItem)` returns a non-null `MailItem`
  only when `objItem is not null && objItem is MailItem` (a raw interop-type check).
  `Tags/TagController.cs:50-55` sets `_olMail = ResolveMailItem(_objItem)` and `_isMail = true` only
  when `_olMail is not null`.
- `Tags/TagController.cs:115-128` — `SetAutoAssignState` hides/disables the viewer's auto-assign
  button unless `autoAssigner is not null & _isMail` (line 118).
- `Tags/TagController.cs:287-296` — `ButtonAutoAssign_Action` (line 287) calls
  `_autoAssigner.AutoFindAsync(_objItem)` (line 291); this only runs when the button is enabled.
- `TaskVisualization/AutoAssignPeople.cs:59-87` — `AutoFind(object objItem)` returns `[]`
  immediately for `null` (lines 62-65), and for any type not matching `MailItemHelper` (lines
  66-69), an `IOutlookItem` whose `GetOlItemType() == OlItemType.olMailItem` (lines 70-76), or a raw
  `MailItem` (lines 77-80); the final `else` branch (lines 81-84) silently returns `[]` with no
  logging.
- `UtilitiesCS/Interfaces/IReusableTypeClasses/IOutlookItem.cs:6-59` — confirms `IOutlookItem` is a
  host-neutral wrapper interface exposing `InnerObject` as the underlying raw COM object; it is not
  itself assignable to the interop `MailItem` type.
- `UtilitiesCS/OutlookObjects/Item/OutlookItem.cs:176` — confirms `InnerObject => this._item`,
  i.e. the raw item the wrapper was constructed from.
- `ToDoModel/Data Model/ToDo/ToDoItem.cs:406` — confirms `Active.OlItem` returns `IOutlookItem` (via
  `FlaggableItem`), so `_active.OlItem.InnerObject` in `AssignPeople()` is the raw object, not the
  wrapper that the other three assign methods pass.
