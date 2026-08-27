# efc-keyboard-navigation-keeps-highlighted-row-visible (Issue #640)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-keyboard-navigation-keeps-highlighted-row-visible/ (Issue #640)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #640
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/640
- Last Updated: 2026-08-27
## Summary

When the Efc breadcrumb selector contains more rows than fit in its visible area, keyboard
navigation can move the highlighted row beyond the viewport without scrolling the list. The
selection state changes, but the user can no longer see which row is highlighted.

## Environment

- OS/version: Windows desktop TaskMaster Efc with its WebView-backed breadcrumb selector.
- Python version: Not applicable; the affected implementation is C# with embedded JavaScript.
- Command/flags used: Interactive Efc keyboard navigation using the Up and Down arrow keys.
- Data source or fixture: Any folder suggestion list long enough to exceed the selector viewport.

## Steps to Reproduce

1. Open Efc and display a breadcrumb suggestion list that extends below the visible scroll area.
2. Select a visible row, then press Down repeatedly until the highlighted row moves below the
   viewport.
3. Observe that the list does not scroll with the new selection and the highlighted row is off
   screen.
4. Repeat from a lower visible row with Up until the highlighted row moves above the viewport.

## Expected Behavior

The highlighted row must remain completely visible during keyboard navigation.

- If a Down-arrow move places the newly highlighted row below the viewport, scroll it into view and
  position it as high as the scroll area permits while keeping the row completely visible.
- If an Up-arrow move places the newly highlighted row above the viewport, scroll it into view and
  position it as low as the scroll area permits while keeping the row completely visible.
- Do not move the viewport when the newly highlighted row is already completely visible.

## Actual Behavior

Efc intercepts Up and Down in `BreadcrumbDocumentAssets.BridgeJs`, posts an `arrowKey` message, and
the native router updates the selected row and emits a full render. The inbound render handler
replaces the `#rows` markup but does not apply any viewport adjustment. A selected row can therefore
remain outside the visible scroll area after navigation.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: No error is emitted; the defect is visible as an off-screen highlighted row.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Keyboard users lose visual confirmation of the active Efc destination row and must recover by
scrolling manually or changing selection again.

## Suspected Cause / Notes

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`: `BridgeJs` intercepts the arrow
  keys and replaces rendered rows without scrolling the newly selected `.rowwrap.selected` element.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`: Up and Down selection movement already emits
  a render update; the issue is the page-side viewport behavior after that update.
- `QuickFiler/Controllers/EfcFormController.cs` wires Efc to this bridge document.
- This is distinct from open issue #440, which covers Left/Right parent-child navigation, and from
  the Qfc `FolderBreadcrumb.html` asset, which is not the Efc rendering path.

## Proposed Fix / Validation Ideas

- [ ] Preserve the triggering Up/Down direction through the Efc bridge render path and, after the
      selected row is rendered, check whether it lies outside the scroll viewport.
- [ ] For an out-of-view Down move, use start-edge alignment; for an out-of-view Up move, use
      end-edge alignment. Do not scroll an already fully visible selected row.
- [ ] Unit coverage area: `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`
      should assert the generated bridge-script contract for directional, conditional scrolling.
- [ ] Integration scenario to retest: a long Efc list navigated beyond both viewport edges using
      Up and Down, including the first and last rows and a row already visible in the middle.
- [ ] Manual verification notes: confirm the WebView viewport placement interactively because the
      existing renderer tests do not expose browser geometry or scroll offsets.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
