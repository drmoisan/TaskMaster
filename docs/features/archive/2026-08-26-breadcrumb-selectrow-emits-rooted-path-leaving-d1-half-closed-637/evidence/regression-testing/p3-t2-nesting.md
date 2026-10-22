Timestamp: 2026-08-31T10:42:17-04:00
Command: Scoped `rg` verification of SelectRow diagnostics and guard structure.
EXIT_CODE: 0
Output Summary: Each diagnostic literal occurs once, neither contains `@`, and the first conjunction remains `_boundRoot.Length != 0`.

```csharp
private void SelectRow(BreadcrumbRow row)
{
    if (row.Kind == BreadcrumbRowKind.Banner)
    {
        return; // Banner rows are never selectable.
    }

    string selection =
        row.Kind == BreadcrumbRowKind.TrashPseudoRow
            ? BreadcrumbRowBuilder.TrashRowText
            : row.FilingTarget;
    // #614 D2: normalize eligible rooted targets, preserving no-bound-root pass-through.
    if (
        _boundRoot.Length != 0
        && ArchiveStemContract.IsFullOutlookPath(selection)
    )
    {
        if (!ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out string stem))
        {
            log.Error("Breadcrumb row rejected: target is outside the archive root.");
            return;
        }

        if (stem.Length == 0)
        {
            log.Error("Breadcrumb row rejected: target is the archive root itself.");
            return;
        }

        selection = stem;
    }

    CommitSelection(row, selection);
}
```

The `_boundRoot.Length != 0` condition remains the first conjunct, preserving no-bound-root pass-through mode.
