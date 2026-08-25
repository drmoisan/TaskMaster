Timestamp: 2026-08-25T14-41

Files inspected:
- `remediation-inputs.2026-08-25T14-29.md`
- `spec.md`
- `code-review.2026-08-25T14-29.md`
- `feature-audit.2026-08-25T14-29.md`
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`

Permitted production target: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`, limited to `ProjectSuggestionPath`.

Permitted test target: `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`.

Required invariant: match only `ArchiveRootPath + "\\"` with `StringComparison.OrdinalIgnoreCase`, retaining the existing length guard and projecting only the suffix after the root plus separator. Root-only, already-relative, textual-prefix-only, and out-of-root values remain unchanged.

Prohibited targets preserved: `BreadcrumbBridgeRouter`, `EmailFilerConfig`, `EfcDataModel`, `EfcFormController`, persistence, Outlook COM interactions, filesystem APIs, generic source-map normalization, `Store.FilePath`, and mailbox `@` parsing.
