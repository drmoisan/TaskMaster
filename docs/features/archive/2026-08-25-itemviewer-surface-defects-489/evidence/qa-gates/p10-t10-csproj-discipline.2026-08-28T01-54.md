# P10-T10 — Project-file discipline

Timestamp: 2026-08-28T01-54
Command: git diff --name-only cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/QuickFiler.csproj ; git diff --numstat cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler.Test/QuickFiler.Test.csproj ; git diff cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler.Test/QuickFiler.Test.csproj
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## `QuickFiler/QuickFiler.csproj` is absent from the P10-T2 diff

`git diff --name-only <BASELINE_SHA> -- QuickFiler/QuickFiler.csproj` produces **zero output lines**,
and the path is not among the 25 in the P10-T2 scope-lock list. This feature adds no new production
file — every production change is an edit to a file that already existed — so the production project
file needs no new `<Compile Include>` entry and was not touched.

## `QuickFiler.Test/QuickFiler.Test.csproj` numstat

```
4	0	QuickFiler.Test/QuickFiler.Test.csproj
```

Exactly **4** added and **0** deleted, as required. Because the deletion count is zero, the diff is
pure insertion: **no pre-existing entry moved**, was reordered, or was rewritten. That is the
strongest available form of the "no pre-existing entry moved" condition — a move would necessarily
show as a paired deletion and addition and would raise the deleted count above zero.

## Verbatim diff

```
diff --git a/QuickFiler.Test/QuickFiler.Test.csproj b/QuickFiler.Test/QuickFiler.Test.csproj
index ee9a0ce1..76e59d50 100644
--- a/QuickFiler.Test/QuickFiler.Test.csproj
+++ b/QuickFiler.Test/QuickFiler.Test.csproj
@@ -98,6 +98,7 @@
     <Compile Include="Viewers\BreadcrumbDropDownCoverageThresholdTests.cs" />
     <Compile Include="Controllers\QfcItemControllerBreadcrumbDropDownTests.cs" />
     <Compile Include="Viewers\FolderBreadcrumbAssetContractTests.cs" />
+    <Compile Include="Viewers\ToolStripMenuItemCbTests.cs" />
     <Compile Include="Controllers\KbdActionsTests.cs" />
     <Compile Include="Controllers\KbdActionsRemainingBranchesTests.cs" />
     <Compile Include="Controllers\KaCharTests.cs" />
@@ -168,6 +169,9 @@
     <Compile Include="Controllers\QfcItemController.SeamDispatcherTests.cs" />
     <Compile Include="Controllers\QfcItemController.SeamCoreTests.cs" />
     <Compile Include="Controllers\QfcItemController.SeamFactoryTests.cs" />
+    <Compile Include="Controllers\QfcItemController.EventWiringTests.Part2.cs" />
+    <Compile Include="Controllers\QfcItemController.ThemeMarshallingTests.cs" />
+    <Compile Include="Controllers\QfcItemController.MailActionsTests.Part2.cs" />
     <Compile Include="Controllers\MailItemActionsAdapterTests.cs" />
     <Compile Include="Controllers\WpfUiDispatcherTests.cs" />
     <Compile Include="Controllers\WebView2CoreInitializerTests.cs" />
```

## Adjacency against the P0-T19-recorded tails

`FEATURE/evidence/baseline/phase0-csproj-block-tails.2026-08-27T23-34.md` records both block tails
verbatim with their current line numbers: the `Viewers\` block tail is
`<Compile Include="Viewers\FolderBreadcrumbAssetContractTests.cs" />` and the
`Controllers\QfcItemController.*` block tail is
`<Compile Include="Controllers\QfcItemController.SeamFactoryTests.cs" />`.

| Condition | Result |
|---|---|
| The `Viewers\` block gains one entry, `Viewers\ToolStripMenuItemCbTests.cs`, immediately after the P0-T19-recorded tail `Viewers\FolderBreadcrumbAssetContractTests.cs` | **Met.** The first hunk shows the new entry on the line directly following that tail, with no intervening line. |
| The `Controllers\QfcItemController.*` block gains three entries in the order `EventWiringTests.Part2.cs`, `ThemeMarshallingTests.cs`, `MailActionsTests.Part2.cs` | **Met.** The second hunk shows them in exactly that order. |
| They are appended immediately after the P0-T19-recorded tail `Controllers\QfcItemController.SeamFactoryTests.cs` | **Met.** The first of the three sits on the line directly following that tail. |
| No pre-existing entry moved | **Met.** `0` deleted lines. |

Every insertion was anchored on the quoted element text of the entry it follows, never on a line
number, which is why the P0-T19-recorded tail movement (`Viewers\` from `:96` to `:100`, and
`Controllers\QfcItemController.*` from `:157` to `:170`, both caused by merged siblings 444 and 493
after this plan was authored) did not affect placement. The plan asserts a baseline-plus-N line count
rather than any printed line number, so no stale number is load-bearing.

## Acceptance

| P10-T10 condition | Result |
|---|---|
| `QuickFiler/QuickFiler.csproj` absent from the P10-T2 diff | Met |
| `git diff --numstat <BASELINE_SHA> -- QuickFiler.Test/QuickFiler.Test.csproj` reports exactly `4` added and `0` deleted | Met |
| `Viewers\` block gains `Viewers\ToolStripMenuItemCbTests.cs` immediately after its recorded tail | Met |
| `Controllers\QfcItemController.*` block gains the three named entries in the stated order, immediately after its recorded tail | Met |
| No pre-existing entry moved | Met |

Output Summary: Project-file discipline **holds**. `QuickFiler/QuickFiler.csproj` is absent from the
P10-T2 diff because this feature adds no new production file.
`QuickFiler.Test/QuickFiler.Test.csproj` reports exactly `4` added and `0` deleted — a pure insertion,
so no pre-existing entry moved. The `Viewers\` block gains one entry,
`Viewers\ToolStripMenuItemCbTests.cs`, immediately after the P0-T19-recorded tail
`Viewers\FolderBreadcrumbAssetContractTests.cs`, and the `Controllers\QfcItemController.*` block
gains three entries in the required order — `EventWiringTests.Part2.cs`, `ThemeMarshallingTests.cs`,
`MailActionsTests.Part2.cs` — immediately after the recorded tail
`Controllers\QfcItemController.SeamFactoryTests.cs`. The verbatim diff is reproduced above.
