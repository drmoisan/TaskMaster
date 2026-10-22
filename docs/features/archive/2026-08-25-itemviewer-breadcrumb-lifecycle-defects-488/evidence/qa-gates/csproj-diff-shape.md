# `QuickFiler.Test.csproj` Diff Shape ([P7-T7])

Timestamp: 2026-08-28T06-17

Command:

```
git diff --numstat 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test/QuickFiler.Test.csproj
```

EXIT_CODE: 0

## Result

```
1	0	QuickFiler.Test/QuickFiler.Test.csproj
```

**Exactly 1 added line and 0 deleted lines.** A zero deletion count is what establishes that no existing
entry was reordered, reworded, or removed: any reordering would appear as matched additions and
deletions, and any rewording as a deletion paired with an addition.

## The delivered line number and its neighbours

The added entry sits at line **88**:

```
    85:    <Compile Include="Viewers\BreadcrumbPendingOpenCloseTests.cs" />
    86:    <Compile Include="Viewers\BreadcrumbDropDownLifecycleTests.cs" />
    87:    <Compile Include="Viewers\ItemViewerBreadcrumbDropDownContractTests.cs" />
    88:    <Compile Include="Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs" />   <-- added
    89:    <Compile Include="Viewers\BreadcrumbDropDownOpenCoordinatorTests.cs" />
    90:    <Compile Include="Viewers\BreadcrumbDropDownOpenCoordinatorTests.Part2.cs" />
```

| Position | Line | Entry |
| --- | --- | --- |
| immediately above | 87 | `Viewers\ItemViewerBreadcrumbDropDownContractTests.cs` |
| **the added entry** | **88** | `Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs` |
| immediately below | 89 | `Viewers\BreadcrumbDropDownOpenCoordinatorTests.cs` |

**Adjacency to the existing `Viewers\ItemViewerBreadcrumbDropDownContractTests.cs` entry is confirmed:**
the new entry is the line immediately after it.

## Note on the anchor's line number

Constraint C4 cites the anchor entry at line **82**, between
`Viewers\BreadcrumbDropDownLifecycleTests.cs` at 81 and `Viewers\BreadcrumbDropDownOpenCoordinatorTests.cs`
at 83. In the current tree the anchor resolves at line **87**, between the same two entries at 86 and 89.
The five-line offset is expected drift against the pre-change citation anchor `0a6aaa31`; this branch's
base `12465043` carries merged sibling features that added entries above this region. The anchor was
resolved **by entry name**, as the plan's standing citation rule requires, and the surrounding structure
C4 describes — the sole `Viewers\ItemViewer*` entry sitting between two `Breadcrumb*` entries — holds
exactly.

## Item-group ordering was not "fixed"

The `Compile Include` item group is **not** alphabetically ordered; it is ordered by area and insertion
history. `Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs` at line 88 sits between two
`Breadcrumb*` entries, which is not its alphabetical position. That is deliberate, and is the delivered
instruction of the spec's Divergence 1 against the research, which had proposed inserting at an
"alphabetical position among the existing `Viewers\ItemViewer*` entries". No entry was reordered to
impose alphabetical order.

## Encoding preserved

The file remains UTF-8 **with BOM** and CRLF line terminators, matching its pre-change state. The insert
was made byte-precisely rather than through a line-oriented tool, because a `sed`-style rewrite strips
CRLF across the whole file and a BOM-unaware writer drops the byte-order mark; either would have turned
a one-line change into a whole-file diff and broken this task's acceptance.

Output Summary: `git diff --numstat <BASE_SHA> -- QuickFiler.Test/QuickFiler.Test.csproj` reports
**exactly 1 added line and 0 deleted lines**. The added entry is at line **88**, immediately below
`Viewers\ItemViewerBreadcrumbDropDownContractTests.cs` at 87 and immediately above
`Viewers\BreadcrumbDropDownOpenCoordinatorTests.cs` at 89, confirming adjacency to the existing entry
with no entry reordered.
