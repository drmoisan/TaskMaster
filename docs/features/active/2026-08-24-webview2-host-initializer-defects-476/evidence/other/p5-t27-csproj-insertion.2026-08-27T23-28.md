# `QuickFiler.Test.csproj` Insertion Audit ([P5-T27])

Timestamp: 2026-08-27T23-28

Command:

```
git diff origin/epic/quickfiler-bug-family-integration..HEAD -- QuickFiler.Test/QuickFiler.Test.csproj
git diff --numstat origin/epic/quickfiler-bug-family-integration..HEAD -- QuickFiler.Test/QuickFiler.Test.csproj
git diff 4f238289090e4c97ca505511a5a73e8092dce0f9 -- QuickFiler.Test/QuickFiler.Test.csproj
git show 4f238289090e4c97ca505511a5a73e8092dce0f9:QuickFiler.Test/QuickFiler.Test.csproj | grep -n WebView2CoreInitializerTests
grep -n "WebView2CoreInitializerTests|WebView2BreadcrumbHost" QuickFiler.Test/QuickFiler.Test.csproj
```

EXIT_CODE: 0

## Output Summary

This feature's own hunk adds **exactly two lines and removes none**, and moves no line.
`git diff --numstat origin/epic/quickfiler-bug-family-integration..HEAD` reports `2 0` for this file.

```diff
@@ -171,6 +171,8 @@
     <Compile Include="Controllers\MailItemActionsAdapterTests.cs" />
     <Compile Include="Controllers\WpfUiDispatcherTests.cs" />
     <Compile Include="Controllers\WebView2CoreInitializerTests.cs" />
+    <Compile Include="Viewers\WebView2BreadcrumbHostContractTests.cs" />
+    <Compile Include="Viewers\WebView2BreadcrumbHostTests.cs" />
     <Compile Include="Controllers\QfcQueueTests.cs" />
     <Compile Include="TestSupport\WinFormsPumpHost.cs" />
     <Compile Include="TestSupport\WinFormsPumpHostTests.cs" />
```

Both added lines sit immediately after the `Controllers\WebView2CoreInitializerTests.cs` entry, so the
three WebView2 entries are contiguous. The `+` lines are the only change; every other line in the
hunk is unchanged context, which is the mechanical proof that the surrounding ItemGroup was not
re-sorted. A re-sort would have produced paired `-`/`+` lines for every moved entry, and there are
none.

## The `:159` anchor, and why the literal line number no longer resolves

The criterion says the entry is inserted "immediately after `QuickFiler.Test/QuickFiler.Test.csproj:159`".
The spec's Premise correction 2 quotes what `:158-160` held when the spec was written:

```xml
    <Compile Include="Controllers\WpfUiDispatcherTests.cs" />
    <Compile Include="Controllers\WebView2CoreInitializerTests.cs" />
    <Compile Include="Controllers\QfcQueueTests.cs" />
```

so `:159` denotes the `Controllers\WebView2CoreInitializerTests.cs` entry, and the insertion point is
"immediately after that entry, keeping the WebView2 entries contiguous".

That entry has since drifted downward twice, through insertions this feature did not make:

| Tree state | Line number of `Controllers\WebView2CoreInitializerTests.cs` |
| --- | --- |
| Spec authoring time | 159 |
| `BASELINE_SHA` `4f238289` | 170 |
| Current `HEAD` | 173 |

The drift from 159 to 170 predates this feature's first commit. The drift from 170 to 173 was
produced by the merged integration base at `9cb2c4f6`: feature 493 inserted two
`Controllers\QfcItemController.UiThreadDispatcherFixture*` entries and feature 444 inserted one
`Controllers\QfcCollectionControllerNavigationDigitsTests.cs` entry, all above the anchor.

The current file reads:

```
173:    <Compile Include="Controllers\WebView2CoreInitializerTests.cs" />
174:    <Compile Include="Viewers\WebView2BreadcrumbHostContractTests.cs" />
175:    <Compile Include="Viewers\WebView2BreadcrumbHostTests.cs" />
```

The insertion is immediately after the entry the criterion identifies. The literal integer 159 no
longer resolves to that entry, and cannot, because three other features' entries were inserted above
it by branches this feature has no authority over. The criterion is checked off on the anchor the
line number denotes, with this drift recorded rather than glossed.

## Region ownership

Both added entries are inside the alphabetical prefix `Viewers\WebView2*`, which is this feature's
owned project-file region. The merged siblings' entries were not reordered, replaced, or dropped;
that was verified at merge time and recorded in
`evidence/qa-gates/base-merge-reconciliation.2026-08-27T23-09.md`.
