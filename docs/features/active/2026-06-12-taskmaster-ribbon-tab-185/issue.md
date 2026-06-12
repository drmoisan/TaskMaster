# taskmaster-ribbon-tab (Issue #185)

- Date captured: 2026-06-12
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/taskmaster-ribbon-tab/ (Issue #185)

- Issue: #185
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/185
- Last Updated: 2026-06-12
- Work Mode: minor-audit

## Problem / Why

The TaskMaster custom ribbon controls for the Explorer window are currently attached to the
built-in Outlook Home (Mail) tab via `<tab idMso="TabMail">` in
`TaskMaster/Ribbon/RibbonExplorer.xml`. This crowds the standard Mail tab and mixes
TaskMaster functionality with Outlook's native commands. The custom controls should live on
their own dedicated tab so they are grouped together and do not clutter the built-in Mail tab.

## Proposed Behavior

Move all custom ribbon groups that currently sit on the `TabMail` tab into a new dedicated
custom tab labeled "Taskmaster". The four affected groups are:

- `SpamBayesGroup` (Spam Bayes)
- `Group2` (Task Master)
- `TriageGroup` (Triage)
- `UtilitiesGroup` (Utilities)

The new tab is a custom tab (declared with `id` + `label="Taskmaster"`, not `idMso`). After the
change, the `TabMail` built-in tab carries none of these custom groups. The pre-existing
`TabFolder` and `TabTasks` tabs are unaffected (they are not on the Mail tab and are out of
scope). All control ids, callbacks, images, labels, and nesting are preserved exactly during
the move so existing callback wiring continues to function unchanged.

## Acceptance Criteria (early draft)

- [x] AC1: A new custom tab declared with an `id` attribute and `label="Taskmaster"` exists in `RibbonExplorer.xml`.
- [x] AC2: The four groups `SpamBayesGroup`, `Group2`, `TriageGroup`, and `UtilitiesGroup` are children of the new Taskmaster tab.
- [x] AC3: The `<tab idMso="TabMail">` element no longer contains any custom group (it is removed or emptied so no custom group remains on the Mail tab).
- [x] AC4: Every control id, `onAction`/`getPressed`/`getText`/`getLabel` callback, `imageMso`, `label`, `keytip`, and menu nesting is preserved unchanged from the original groups.
- [x] AC5: `RibbonExplorer.xml` remains well-formed and schema-valid; existing `RibbonExplorerXmlTests` pass and a new regression test asserts the Taskmaster tab placement.

## Constraints & Risks

- Outlook loads custom ribbons all-or-nothing: any schema violation rejects the entire `customUI`
  document and all TaskMaster buttons silently fail to load.
- Custom tabs require a unique `id` and a `label`; they cannot use `idMso` (which targets built-in tabs).
- Control `id` values must remain unique across the whole `customUI` document.

## Test Conditions to Consider

- [ ] Well-formed XML and menu-legal-children regression tests continue to pass.
- [ ] New assertion: the four groups resolve under the Taskmaster custom tab.
- [ ] New assertion: `TabMail` carries no custom groups after the move.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/taskmaster-ribbon-tab/` folder from the template