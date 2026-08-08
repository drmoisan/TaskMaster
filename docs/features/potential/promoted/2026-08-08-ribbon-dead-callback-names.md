# ribbon-dead-callback-names (Issue #504)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ribbon-dead-callback-names/ (Issue #504)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #504
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/504
- Last Updated: 2026-08-08
## Summary

Five `onAction` callback names declared in `TaskMaster/Ribbon/RibbonExplorer.xml` have no matching method on `RibbonViewer`. Office binds ribbon callbacks by name at runtime and silently ignores a name it cannot resolve, so the affected controls render normally but do nothing when clicked. Four of the five are a `_Clicked` / `_Click` suffix mismatch on the Quick Filer settings check boxes, which means four user-facing settings toggles are inert.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon
- Data source or fixture: `TaskMaster/Ribbon/RibbonExplorer.xml` embedded resource

## Steps to Reproduce

1. Open Outlook with the TaskMaster add-in loaded.
2. Open the Quick Filer settings menu on the TaskMaster ribbon tab.
3. Toggle "Move Entire Conversation", "Save Attachments", "Save Email Copy", or "Save Pictures".
4. Reopen the menu and observe the toggle state, or inspect the backing setting.

## Expected Behavior

Each check box invokes its handler and flips the corresponding `Globals.InternalQfSettings` value. `RibbonController` already implements the intended toggles (`ToggleMoveEntireConversation`, `ToggleSaveAttachments`, `ToggleSaveEmailCopy`, `ToggleSavePictures` in `TaskMaster/Ribbon/RibbonController.Intelligence.cs`), and `RibbonViewer` already exposes correctly-named wrappers.

## Actual Behavior

The XML declares `onAction="MoveEntireConversation_Clicked"` (and the three sibling `_Clicked` names), but `RibbonViewer` defines `MoveEntireConversation_Click` (no `ed`). Office cannot resolve the declared name, so the click is a no-op. The setting never changes. `getPressed` is wired correctly, so the check box also never appears to change state.

Separately, `onAction="BtnMigrateIDs_Click"` is declared but no such method exists anywhere in the assembly.

Verified by enumerating every `onAction`/`getPressed`/`getEnabled`/`getText`/`onChange` value in the XML (89 unique names) and checking each against `TaskMaster/Ribbon/RibbonViewer.cs`:

```text
MISSING: BtnMigrateIDs_Click
MISSING: MoveEntireConversation_Clicked
MISSING: SaveAttachments_Clicked
MISSING: SaveEmailCopy_Clicked
MISSING: SavePictures_Clicked
```

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: see the verification output above. Office does not surface a user-visible error for an unresolved callback name; the failure is silent unless Office is run with the "Show add-in user interface errors" developer option enabled.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Four user-facing Quick Filer settings cannot be changed from the ribbon. The failure is silent, so a user has no signal that the toggle did not take effect. The fifth name is a control with no implementation at all.

## Suspected Cause / Notes

Naming drift between the ribbon XML and the callback class. `RibbonViewer` consistently uses the `_Click` suffix; these four XML entries use `_Clicked`. `BtnMigrateIDs_Click` appears to be a control whose implementation was never added or was removed without removing the XML entry.

The existing regression suite `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` validates the CustomUI schema structure of the ribbon document but does not validate that every declared callback name resolves to a method with a signature Office can bind. That missing check is why the drift was not caught.

Found while researching issue #503 (ribbon engine readiness guard); out of scope for that fix.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: extend `RibbonExplorerXmlTests` with a reflection-based test that asserts every `onAction`/`getPressed`/`getEnabled`/`getText`/`onChange` name declared in the XML resolves to a public method on `RibbonViewer`. This turns the whole class of drift into a build-time failure.
- [x] Integration scenario to retest: toggle each Quick Filer settings check box in live Outlook and confirm the backing setting changes and the check state persists.
- [x] Manual verification notes: decide whether `BtnMigrateIDs` should be implemented or removed from the XML.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
