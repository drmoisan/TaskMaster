# [P9-T4] Project-file scope

Timestamp: 2026-08-28T01-49
Task: [P9-T4]
Command 1: `git diff --name-only <BASE> -- QuickFiler/QuickFiler.csproj`
Command 2: `git diff --numstat <BASE> -- QuickFiler.Test/QuickFiler.Test.csproj`
EXIT_CODE: 0

Both commands were run against both bases, for the reason `changed-file-set.md` records: the mandated
integration merge `25924673` placed merged siblings #476 and #501 inside the `BASELINE_SHA..HEAD` range.
The acceptance condition is evaluated against `38f097898639b054428188c9c5e266e54972c259`.

## Command 1 — `QuickFiler/QuickFiler.csproj`

| Base | Output lines |
|---|---|
| `38f09789` (**evaluated**) | **0** |
| `002335989830...` (as written) | 1 (`QuickFiler/QuickFiler.csproj`) |

The single as-written hit is merged sibling work: #476 and #501 both added `<Compile Include>` entries
under `Viewers\WebView2*` and `Viewers\Breadcrumb*`. This feature wrote nothing to that file, which the
evaluated zero-line result proves.

## Command 2 — `QuickFiler.Test/QuickFiler.Test.csproj`

| Base | numstat |
|---|---|
| `38f09789` (**evaluated**) | **3 added, 0 deleted** |
| `002335989830...` (as written) | 6 added, 0 deleted |

The evaluated figures are exactly the 3 added and 0 deleted the acceptance condition requires. The three
added lines, verbatim:

```
    <Compile Include="Controllers\EfcItemController.CleanupTests.cs" />
    <Compile Include="Controllers\EfcItemControllerTests.cs" />
    <Compile Include="Controllers\EfcViewerTests.cs" />
```

All three are inside the `Controllers\Efc*` prefix, which is the only region this feature owns. None is
inside a region held by a live sibling: not `Controllers\QfcItemController*` or `Viewers\ToolStrip*`
(#489, LIVE), not `Viewers\Breadcrumb*` (#501), not `Viewers\WebView2*` (#476), not the eight
`QfcCollectionController*` entries of #444, and not the two
`QfcItemController.UiThreadDispatcherFixture*` entries of #493. Zero lines were deleted, so no existing
entry was removed or reordered.

The additional 3 lines visible under the as-written base are the merged siblings' own entries.

Output Summary: PASS. Under the evaluated base `QuickFiler/QuickFiler.csproj` has a zero-line diff and
`QuickFiler.Test/QuickFiler.Test.csproj` has exactly 3 added and 0 deleted lines, all three
`Controllers\Efc*` entries inside the only region this feature owns. The larger as-written figures (1
path and 6 added lines) are attributable entirely to merged siblings #476 and #501 carried in by the
integration merge.
