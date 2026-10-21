Timestamp: 2026-08-31T10:48:27-04:00
Command: Line-count and cached QuickFiler.Test diff inspection against `0eda184ca0009bc79ac9b7146897270c17c095fa`.
EXIT_CODE: 0
Output Summary: The issue #439 test file remains 694 lines. Exactly one removed assertion matches `.Should()`: `router.SelectedFolderPath.Should().Be(fullTarget);`.

This is a deliberate spec correction: the issue #439 criterion that a rooted target survives selection is superseded by issue #614's archive-relative-stem invariant, which #614 enforced on the `SelectHierarchyPath` half and at the filing boundary but not on the `SelectRow` half. This is not a weakened test.

Removed method name: `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`

Added method name: `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`

Replacement comments:

```csharp
// Arrange: the presented target is rooted with casing different from the configured
// root, so the provider must receive the original full path unchanged (#439).
```
