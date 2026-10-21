# Phase 0 — QuickFiler.Test.csproj Block Tails (P0-T19)

Timestamp: 2026-08-27T23-34
Command: git grep -n on Compile Include entries in QuickFiler.Test/QuickFiler.Test.csproj
EXIT_CODE: 0

`QuickFiler.Test/QuickFiler.Test.csproj` is **493** lines.

## The two block tails, verbatim, with their current line numbers

### Viewers\ block tail — line 100

```
100:    <Compile Include="Viewers\FolderBreadcrumbAssetContractTests.cs" />
```

This is the entry a new `Viewers\ToolStripMenuItemCbTests.cs` is appended after.

### Controllers\QfcItemController.* block tail — line 170

```
170:    <Compile Include="Controllers\QfcItemController.SeamFactoryTests.cs" />
```

This is the entry a new `Controllers\QfcItemController.ThemeMarshallingTests.cs` is appended after.

## Both tails have moved since the plan was authored

| Block | Tail entry | Line in plan | Line now | Shift |
|---|---|---:|---:|---:|
| `Viewers\` | `Viewers\FolderBreadcrumbAssetContractTests.cs` | 96 | **100** | +4 |
| `Controllers\QfcItemController.*` | `Controllers\QfcItemController.SeamFactoryTests.cs` | 157 | **170** | +13 |

The plan's § Fact base anticipates the second shift and instructs that the real number be recorded
rather than "corrected" to the stale value. Both stale numbers come from research section 7.3, which
measured the file on 2026-08-25 before siblings added entries. The recorded numbers above are the
values read on this branch head.

The plan's § Fact base also states that P1-T2's insertion at `:97` moves the
`Controllers\QfcItemController.*` block tail from `:157` to `:158`. That arithmetic is stated against
the stale baseline. Against the measured baseline the same reasoning gives `:170` moving to `:171`.
Neither number is asserted by any acceptance condition: P5-T2 is anchored on the quoted entry
`Controllers\QfcItemController.SeamFactoryTests.cs` and its acceptance is the baseline-plus-2 line
count, which against the measured baseline of **493** is **495**.

## Neither block is alphabetical

Ordering is by area and by insertion history, not alphabetically. Direct evidence from the current
file:

- In the `Viewers\` block, `Viewers\BreadcrumbSelectorOpenRetryTests.cs` (`:74`) precedes
  `Viewers\BreadcrumbSelectorCoordinatorTests.cs` (`:75`), and `O` does not sort before `C`.
- `Viewers\ItemViewerBreadcrumbDropDownContractTests.cs` sits at `:86`, in the middle of a run of
  `Breadcrumb*` entries rather than under `I`.
- The two blocks are not even contiguous by directory. A `Controllers\` entry is interleaved inside
  the `Viewers\` run at `:99`, `Controllers\QfcItemControllerBreadcrumbDropDownTests.cs`, immediately
  before the `Viewers\` tail at `:100`, and the `Controllers\` run resumes at `:101`.
- In the `Controllers\QfcItemController.*` block, `SeamDispatcherTests.cs` (`:168`), then
  `SeamCoreTests.cs` (`:169`), then `SeamFactoryTests.cs` (`:170`) — `Dispatcher`, `Core`, `Factory`
  is not alphabetical.

**No pre-existing entry may be reordered.** Both blocks are shared with sibling epic children, which
append to them independently; reordering would produce a merge conflict against work this feature
does not own. Each new entry is appended at the tail of its block, and nothing else in the file
changes. This matches the ratified precedent recorded in 484's spec (its `:561-567`), which supersedes
the earlier "alphabetically-ordered item group" claim in research section 8.4.

Output Summary: Both block tails are recorded verbatim with their current line numbers. The
`Viewers\` block tail is line **100**, `<Compile Include="Viewers\FolderBreadcrumbAssetContractTests.cs" />`,
and the `Controllers\QfcItemController.*` block tail is line **170**,
`<Compile Include="Controllers\QfcItemController.SeamFactoryTests.cs" />`. Both have moved since the
plan was authored — from `96` and `157` respectively — because sibling children have appended entries
since 2026-08-25; the measured values are recorded rather than the plan's stale ones, as the plan's
§ Fact base directs. Neither block is alphabetical; ordering is by area and insertion history, with a
`Controllers\` entry interleaved at `:99` inside the `Viewers\` run, so **no pre-existing entry may be
reordered** and each new entry is appended at its block tail. The file is **493** lines, so the
baseline-plus-2 count P5-T2 asserts is **495**.
