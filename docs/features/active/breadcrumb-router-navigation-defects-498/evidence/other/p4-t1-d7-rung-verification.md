# P4-T1 — Decision D7 Rung Verification (read-only)

Timestamp: 2026-08-26T09-39

Task: `[P4-T1]`. Plan:
`docs/features/active/breadcrumb-router-navigation-defects-498/plan.2026-08-24T09-39.md`.
Mode: READ-ONLY. No production file was written by this task.

Command:

```
sed -n '245,255p' UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs
sed -n '474,478p' UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs
sed -n '14,120p' UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
sed -n '107,110p' UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs
sed -n '84,88p' UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs
cat UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs
grep -rn "[.]Chain\b" UtilitiesCS/OutlookObjects/Folder QuickFiler --include=*.cs
```

EXIT_CODE: 0

## Output Summary

### 1. Citation check

All four cited members resolve at the cited lines in the current worktree; no drift.
`FolderBreadcrumbBridgeRouter.cs` is 485 lines, `BreadcrumbStateModel.cs` 457,
`BreadcrumbSelectionMap.cs` 120, `BreadcrumbRow.cs` 361. `CreateFallbackRow` is declared at
`:245`, `ReplaceRowsPreservingSession` at `:474`, `BreadcrumbStateRow` at `:14`, the D7 read
site at `BreadcrumbSelectionMap.cs:109`, and `BreadcrumbRow.FilingTarget` at
`BreadcrumbRow.cs:88`.

### 2. Constructor signatures of `BreadcrumbStateRow` (`BreadcrumbStateModel.cs:14` onward)

All five constructors, verbatim:

```csharp
internal BreadcrumbStateRow(
    IReadOnlyList<FolderBreadcrumbSegment> chain,
    double? probability
)
    : this(IdentityFromChain(chain), chain, probability) { }

internal BreadcrumbStateRow(
    string identity,
    IReadOnlyList<FolderBreadcrumbSegment> chain,
    double? probability
)

internal BreadcrumbStateRow(string verbatimText)
    : this(
        DefaultPlainIdentity(verbatimText),
        verbatimText,
        !IsBanner(verbatimText),
        null,
        false
    ) { }

internal BreadcrumbStateRow(string identity, string verbatimText, bool isSelectable)
    : this(identity, verbatimText, isSelectable, null, false) { }

internal BreadcrumbStateRow(string identity, string fallbackText, double? probability)
    : this(identity, fallbackText, true, probability, true) { }

private BreadcrumbStateRow(
    string identity,
    string verbatimText,
    bool isSelectable,
    double? probability,
    bool isScoredFallback
)
```

The `Chain` property, verbatim:

```csharp
/// <summary>Root-first ancestor chain for a suggestion row; empty for a plain row.</summary>
public IReadOnlyList<FolderBreadcrumbSegment> Chain { get; }
```

`Chain` is assigned once, in the chain-taking constructor (`Chain = chain.ToArray();`) or to
`EmptySegments` in the verbatim-text constructor. There is no setter and no mutating member for
it. The row exposes no per-row filing-target member of any kind; its only string members are
`Identity`, `VerbatimText` and the derived `FallbackText`, and `VerbatimText` is null on every
row that carries a chain (`IsSuggestion => VerbatimText == null`).

### 3. Body of `CreateFallbackRow` (`FolderBreadcrumbBridgeRouter.cs:245-255`), verbatim

```csharp
private static BreadcrumbStateRow CreateFallbackRow(FolderRow row, int index)
{
    string identity = BreadcrumbRowIdentity.ForFolderRow(row, index);
    return row.Score.HasValue
        ? new BreadcrumbStateRow(
            identity,
            row.Score.Value.FolderPath,
            row.Score.Value.Probability
        )
        : new BreadcrumbStateRow(identity, row.Text, row.Kind != FolderRowKind.Separator);
}
```

`ReplaceRowsPreservingSession` (`:474-478`), verbatim:

```csharp
private void ReplaceRowsPreservingSession(IReadOnlyList<BreadcrumbStateRow> rows)
{
    _model.ReplaceRows(rows);
    _selectionSession.ReconcileRowsReplaced();
}
```

`ReplaceRowsPreservingSession` is a pure swap-and-reconcile: it carries no per-row data of its
own and therefore neither helps nor obstructs preservation. The preservation decision is settled
entirely by what the rows handed to it contain.

### 4. The value read at `BreadcrumbSelectionMap.cs:109`

```csharp
private static string RowValue(BreadcrumbStateRow row)
{
    return row.IsSuggestion ? row.Chain[row.Chain.Count - 1].FolderPath : row.VerbatimText!;
}
```

For a resolved suggestion row the selected-folder value is exactly the LEAF CHAIN SEGMENT's
`FolderPath`. Today the Qfc surface never reaches that branch for a scored row, because
`ResolveLeafKeyAsync` fails on the archive-relative stem and `CreateFallbackRow`'s scored
overload is used instead; that row is a scored fallback whose `VerbatimText` is the presented
stem, so `RowValue` returns the stem. Once `P4-T4` makes resolution succeed, the row becomes a
true suggestion row and `RowValue` switches to the leaf segment's `FolderPath`, which is the
store-qualified full path. That switch is the D7 hazard.

### 5. Who else consumes the chain

```
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs:177: ? row.Chain.Select(segment => segment.DisplayName).ToArray()
UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs:109: return row.IsSuggestion ? row.Chain[row.Chain.Count - 1].FolderPath : row.VerbatimText!;
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:416: var leafKey = row.Chain[row.Chain.Count - 1].Key;
```

These are the only three reads of `BreadcrumbStateRow.Chain` in `UtilitiesCS/OutlookObjects/Folder`
and `QuickFiler`. Rendering reads `DisplayName` only; leaf expansion reads `Key` only; the
selection map is the sole reader of the leaf segment's `FolderPath`. The two other `FolderPath`
reads in the render path (`BreadcrumbRenderProjection.cs:223`,
`BreadcrumbBridgeMessages.cs:344`) are over `row.Subfolders`, not over `Chain`, and are therefore
untouched by any change to a chain segment.

### 6. The precedent and why it does not transfer literally

`BreadcrumbRow.FilingTarget` (`BreadcrumbRow.cs:88`), verbatim:

```csharp
/// <summary>
/// Original presented filing target. This is independent of the full hierarchy paths
/// carried by <see cref="Segments"/> and is used for normal folder selection.
/// </summary>
public string FilingTarget { get; }
```

That is the Efc row type. Its consumer, `SelectRow`, is free to read a dedicated member. The Qfc
consumer is `BreadcrumbSelectionMap.RowValue`, which this plan MUST NOT write, so adding an
equivalent member to `BreadcrumbStateRow` would produce a value that nothing reads. The literal
transfer of the precedent is therefore NOT available. The shape of the precedent — an immutable
per-row presented value held independently of the resolved chain — is available, because
`FolderBreadcrumbSegment` is a public immutable class with a public four-argument constructor
(`FolderBreadcrumbSegment.cs:29-40`) that separates `Key` (identity, used for navigation) from
`FolderPath` (the selection value returned to the host, as its own XML comment at `:24` states).

### 7. Mechanism that satisfies rung 1 within owned files

`SetSuggestionsAsync` (`FolderBreadcrumbBridgeRouter.cs:28-90`, OWNED) already holds both values
at the point of row construction: the presented stem in the local `path`
(`:49`, `row.Score.Value.FolderPath`) and the resolved chain returned by
`GetAncestorChainAsync` (`:57-59`). Rebuilding the chain's LAST segment as
`new FolderBreadcrumbSegment(leaf.Key, leaf.DisplayName, presentedStem, leaf.HasChildren)`
before constructing the `BreadcrumbStateRow` makes `RowValue` return the presented stem while
leaving `Key` (leaf expansion, `:416`), `DisplayName` (rendering, `:177`), the segment count and
the root-to-leaf order untouched. This is confined to `FolderBreadcrumbBridgeRouter.cs` and, if a
helper on the row type is preferred, `BreadcrumbStateModel.cs` — both OWNED — and requires no
edit to `BreadcrumbSelectionMap.cs`, no new member on `IFolderHierarchyProvider`, and no change
to `BreadcrumbSelectionSession`.

D7 RUNG SELECTED: 1

Reasoning: rung 1 requires that the presented archive-relative stem can be preserved into the
value read at `BreadcrumbSelectionMap.cs:109` without writing `BreadcrumbSelectionMap.cs`. That
value is the leaf chain segment's `FolderPath` (section 4). The leaf chain segment is constructed
inside the OWNED `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync`, through a public constructor
on a public immutable type, at a point where the presented stem is in scope (section 7). Every
other reader of the chain reads `DisplayName` or `Key`, never the leaf `FolderPath` (section 5),
so the substitution is observationally confined to the D7 read site itself. Rung 1 is therefore
achievable in owned files, and rungs 2 and 3 — which exist only for the case where it is not — do
not apply. `P5-T3` executes; `P5-T4` and `P5-T5` are recorded NOT APPLICABLE with a pointer to
this artifact.
