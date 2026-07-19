# Selection-Output Contract End-to-End Evidence (P6-T3, AC-8, US-7) — STRUCTURAL-IMPOSSIBILITY DOSSIER

Timestamp: 2026-07-18T10-14

WhyRuntimeCaptureImpossible: Filing a mail through `DestinationOlStem`, live search population,
and the attachment-saving dialog gates all require a live Outlook process and real mail items;
none exists in this execution environment. No runtime pass is claimed; each contract leg is
pinned by deterministic unit tests over the exact production types the live path executes.

## Alternative proof — per-leg mapping to deterministic evidence

Path A (suggestions populate, preselect honored, selection files to exactly the shown full path
via `DestinationOlStem` at `QfcItemController.MailActions.cs:103`): NOT RUNTIME-VERIFIED — pinned by:
- `BreadcrumbSelectionMapTests.GetSelectedFolder_SuggestionRow_YieldsTheLeafFullPath`,
  `GetSelectedFolder_ExpandedSubfolderSelection_YieldsTheSubfolderFullPath`,
  `IndexAndItemSelection_RoundTrip` (SetFolderSelectedItem/SetFolderSelectedIndex round-trips).
- `BreadcrumbBridgeCoordinatorTests.InboundSelectionMessage_RaisesSelectionChangedWithMappedPath`
  (the controller's `_selectedFolder = _itemViewer.GetSelectedFolder()` refresh observes the
  mapped full path).
- `BreadcrumbBridgeRouterTests.SetSuggestions_UnresolvablePath_FallsBackToPlainRowPreservingThePath`
  (even unresolvable suggestion paths still yield the exact path string, G10).
- The `AssignFolderComboBox` preselect logic (`QfcItemController.FolderHandling.cs:187-197`) and
  readback (`:198`) are textually unchanged; population goes through the same `IItemViewer`
  members (`SetFolderItems`/`SetFolderSuggestions`/`FolderContains`/`SetFolderSelectedItem`/
  `SetFolderSelectedIndex`/`GetSelectedFolder`).

Path B (typing in search populates plain rows; `SetFolderSelectedIndex(1)` + dropdown-open
behavior intact): NOT RUNTIME-VERIFIED — pinned by:
- `TextBoxSearch_TextChanged` (`QfcItemController.EventHandlers.cs:164-178`) is textually
  unchanged: `ClearFolderItems()` / `SetFolderItems(folders)` / `SetFolderSelectedIndex(1)` /
  `SetFolderDroppedDown(true)` flow over the new members.
- `BreadcrumbBridgeRouterTests.SetItems_PlainRows_RenderVerbatimIncludingTrashToDelete`,
  `BreadcrumbSelectionMapTests.PathBVerbatimStrings_WithWildcards_SurviveExactly`,
  `BreadcrumbRenderProjectionTests.Project_PathBRow_RendersAncestorSplitChainWithEmptyPercentCell`.
- `SetFolderDroppedDown(true)` maps to the breadcrumb focus intent
  (`ItemViewer.FolderSearch.cs` -> `FocusBreadcrumb()`), the documented FR-7 mapping.

"Trash to Delete" literal (attachment gate at `MailActions.cs:90`; dialog-skip at
`QfcCollectionController.cs:170-171`): NOT RUNTIME-VERIFIED — pinned by:
- `BreadcrumbSelectionMapTests.TrashToDelete_IsReturnedByteIdentical` (the selection output is
  the SAME string reference carried in verbatim, so the `!= "Trash to Delete"` comparisons at
  the consuming sites behave exactly as today),
  `CaseDiffersFromLegacyOrdinalContract_IsNotAMatch` (ordinal identity preserved).
- `BreadcrumbBridgeCoordinatorTests.AddItems_AppendsPlainRowsAndContainsFindsThem`.
- The consuming sites (`MailActions.cs:90,103`, `QfcCollectionController.cs:170-171,2308`,
  `QfcItemController.EventHandlers.cs:209-212`) are textually unchanged by this feature.

Per-leg verdict lines: Path A DOSSIER-PINNED; Path B DOSSIER-PINNED; "Trash to Delete"
DOSSIER-PINNED. No leg is recorded as a runtime pass.

MANUAL-VERIFICATION-REQUIRED: yes — the maintainer must verify in the live add-in: filing a
mail from a selected breadcrumb row and from an expanded subfolder lands in exactly the shown
full path; typing in search lists plain rows with index-1 preselect and focus hand-off; and the
"Trash to Delete" attachment gate and dialog-skip behave exactly as before.
