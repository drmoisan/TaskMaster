# Guardrail Verification (P5-T11, AC-9/AC-10)

Timestamp: 2026-07-18T10-08

Commands run (all against merge base 8e242692):
- `git diff 8e242692 -- QuickFiler/QuickFiler.csproj UtilitiesCS/UtilitiesCS.csproj QuickFiler.Test/QuickFiler.Test.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj QuickFiler/packages.config UtilitiesCS/packages.config QuickFiler.Test/packages.config UtilitiesCS.Test/packages.config | grep -E "^[+-].*(Reference Include|package id|HintPath)"`
- `git diff --stat 8e242692 -- UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs FolderScore.cs FolderScorer.cs FolderPredictor.cs FolderRow.cs`
- `git diff --name-only 8e242692 -- "*.cs" "*.html" | xargs -0 wc -l` (per-file line counts)
- `git diff --stat 8e242692 -- <nine dead-variant Designer files>` (P5-T10 companion check)

Per-guardrail verdicts:

- G1 (no third-party WinForms tree/list control, no WPF/ElementHost): PASS. The only new control is the Designer-declared `Microsoft.Web.WebView2.WinForms.WebView2` breadcrumb; no `BrightIdeasSoftware` additions, no `ElementHost`/WPF types introduced (diff inspection of all touched files). Control technology is WebView2 HTML/CSS/JS (`Resources/FolderBreadcrumb.html`, no third-party JS/CSS).
- G2 (no new NuGet packages; JSON only in UtilitiesCS): PASS. `packages.config` diff count across all four projects: 0 lines. No `<Reference Include>`/`HintPath` additions or removals in any of the four `.csproj` diffs (only `<Compile Include>`/`<Content Include>` entries for new first-party files). All Newtonsoft.Json usage lives in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs`; `QuickFiler` contains no JSON serialization code.
- G3 (scoring/ranking untouched): PASS. `git diff --stat` over `PercentageFormatter.cs`, `FolderScore.cs`, `FolderScorer.cs`, `FolderPredictor.cs`, `FolderRow.cs` is empty (exit 0, no output) — consumed read-only.
- G5 (500-line ceiling): PASS for every NEW file: largest new files are `BreadcrumbBridgeMessages.cs` (443), `BreadcrumbBridgeRouter.cs` (348), `BreadcrumbStateModel.cs` (310), `BreadcrumbBridgeCoordinatorTests.cs` (326), `BreadcrumbStateModelTests.cs` (421), `BreadcrumbBridgeRouterTests.cs` (411), `FolderBreadcrumb.html` (244), `ItemViewer.Breadcrumb.cs` (145), all <= 500. Pre-existing files over the ceiling that received minimal targeted edits (pre-existing size debt, not introduced by this feature): `ItemViewer.Designer.cs` 6218 -> 6224 (generated Designer file), `QfcCollectionController.cs` 2341 -> 2328 (net shrink), `EfcItemController.cs` 1168 -> 1170. `KeyboardHandler.cs` shrank 631 -> 414 (now under the ceiling). No touched file crossed from under to over the ceiling.
- G8 (nine dead viewer variants untouched): PASS. `git diff --stat` over all nine dead-variant Designer files is empty; the only Designer diff in `QuickFiler/Viewers/` is `ItemViewer.Designer.cs`. No dead-variant code-behind file was modified.

AC-9/AC-10 supported: no third-party control, no new packages, scoring/model output unchanged.
