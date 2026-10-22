# `.child` emitter markup check (P3-T7, #1002 cycle 1)

Timestamp: 2026-10-22T13-28
Command: Grep over UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs and QuickFiler/Resources/FolderBreadcrumb.js for `child`, `name`, `makeSubfolders` and `className = 'child'`
EXIT_CODE: 0
Output Summary:
- BreadcrumbHtmlRenderer.AppendDescendants (keyed descendants, first and deeper levels): emits `<div class="child" ...>` with the name wrapped as `<span class="name">`. The new `.child .name` rule applies.
- BreadcrumbHtmlRenderer.AppendChildren, `row.IsLeafExpanded` branch (keyless leaf children): emits `<div class="child" data-child-index=...>` with bare HTML-encoded text and no `.name` span. Observation: this emitter keeps `display: flex` from `.child`, but its text is an anonymous flex item that no `.child .name` rule truncates. Not edited (BreadcrumbHtmlRenderer.cs is outside this plan's file set).
- BreadcrumbDocumentAssets bridge JS `subfolderResult` handler: creates `div.className = 'child'` with `textContent` set directly, bare text and no `.name` span. Observation only; the JS constant is outside the CSS constants this task edits.
- QuickFiler/Resources/FolderBreadcrumb.js `makeSubfolders` (QFC selector): builds its own subfolder elements with a `seg` span; it is styled by the QFC page stylesheet, not by BaseCss, so CR-1 does not affect it.
- manual-outlook-check step 3 is unchanged; the existing manual-check artifact is not edited.
