---
name: efc614-store-root-stem-leak
description: "#614: store-root breadcrumb activation leaks full Outlook path to filing boundary; #609 fix lives ONLY in FolderPredictor.ProjectSuggestionPath and deliberately passes out-of-root paths through; FolderConverterTests.cs:329 codifies the always-empty 'Remove illegal characters' bug"
metadata:
  type: project
---

Issue #614 research (2026-08-26). Crash chain verified end-to-end: BreadcrumbBridgeRouter.ToArchiveRelativePath returns non-root inputs VERBATIM (:525); ancestor chains come from FolderTreeRequest.AllStores and include the store-root node (OutlookFolderHierarchyReader.cs:97), with no archive-root floor in BreadcrumbRow.ActivateSegment. EmailFilerConfig.ResolvePaths concatenates OlAncestor+"\"+stem with zero stem validation (:187, :203).

**Why this matters for the fix cycle:**
- The #609/PR #611 fix modified ONLY `FolderPredictor.ProjectSuggestionPath` (:845-858); its remediation plan explicitly prohibited touching Router/EmailFilerConfig/EfcDataModel, and its acceptance criteria REQUIRE out-of-root full paths to pass through byte-for-byte. Do not "restore" pass-through removal as a regression.
- The exception message strips all backslashes before validation, so it cannot distinguish stem `\\mailbox@example.com` from `mailbox@example.com` — only producer-chain analysis identifies the store root.
- `FolderConverter.ToFsFolderpath` has a dead `ask` parameter (never read) — the throw is deterministic, no dialog path, so pure unit tests reproduce the crash exactly.
- Existing test `FolderConverterTests.cs:329` asserts the D5f bug (`Replace(illegalFolderName, "")` always empty) as expected behavior; the fix must update that assertion.
- Two production files declare `UtilitiesCS.FolderConverter`; `UtilitiesCS/EmailIntelligence/FolderConverter.cs` is orphaned (not in csproj) — off-chain promotion candidate.
- ActionOkAsync (:706) accepts empty string; ToArchiveRelativePath returns "" for exact-archive-root selection — guard asymmetry vs IsValidSelection (:1038).

**How to apply:** recommended contract = shared pure `ArchiveStemContract` helper (TryMakeArchiveRelative / RequireArchiveRelativeStem) enforced at router SelectHierarchyPath + both EmailFilerConfig.ResolvePaths overloads + extracted EfcDataModel stem helper; reject (not clamp) out-of-root activation; do NOT null SelectedFolderPath on rejection (collides with open #499 clear-on-rebind design question, see [[qfc-breadcrumb-webview2-351]]).

Full research: docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/research/2026-08-26T10-30-store-root-path-leak-defect-census-research.md
