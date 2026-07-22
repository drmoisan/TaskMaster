# P5 Line-Limit Split Scope Inventory (supersedes P5-T153 file-count assertion)

Timestamp: 2026-07-22T13:01:48Z

Command: `wc -l <4 production + 8 P5 test sources + collapsed-readiness>; grep -c "Part2.cs" QuickFiler.Test/QuickFiler.Test.csproj; git status --short QuickFiler/Viewers/ItemViewer.Designer.cs; git status --short | grep -Ei "coverage.config|runsettings|packages.config|.editorconfig"`

EXIT_CODE: 0

Output Summary: PASS. This inventory supersedes the pre-split file-count assertion recorded by `p5-numeric-coverage-final-scope.<...>.md` (P5-T153), which validly inventoried the state before the line-limit correction and remains unaltered as historical evidence.

Production files: unchanged at four.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` = 396 lines (<= 460).
- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` = 277 lines (<= 500).
- `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` = 226 lines (<= 500).
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` = 472 lines (<= 480).

Test sources: increased from six to eight by the two new `.Part2.cs` partials.
- `ItemViewerBreadcrumbDropDownContractTests.cs` = 132 (<= 480).
- `BreadcrumbDropDownOpenCoordinatorTests.cs` = 386 (<= 480).
- `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` = 144 (<= 480) [new].
- `BreadcrumbDropDownIntegrationTests.cs` = 500 (exactly 500, unchanged).
- `BreadcrumbPopupBoundaryCoverageTests.cs` = 361 (<= 480).
- `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` = 220 (<= 480) [new].
- `BreadcrumbDropDownLifecycleCoverageTests.cs` = 468 (<= 480).
- `BreadcrumbMessengerHubCoverageTests.cs` = 478 (<= 480).

Other P5 headroom test file: `BreadcrumbCollapsedSurfaceReadinessTests.cs` = 486 lines (the pre-authorized exception above the 480-line headroom cap).

Project includes: `QuickFiler.Test.csproj` includes increased by exactly two (one per new `.Part2.cs` partial); `grep -c "Part2.cs"` = 2.

No package/runsettings/`coverage.config`/filter/threshold/designer/exclusion change: `git status` shows no change to `coverage.config`, any `*.runsettings`, any `packages.config`, `.editorconfig`, or `QuickFiler/Viewers/ItemViewer.Designer.cs`.

Downstream 17-class filter consistency: each `.Part2.cs` partial shares its original `[TestClass]` name (`BreadcrumbDropDownOpenCoordinatorTests`, `BreadcrumbPopupBoundaryCoverageTests`), so the class inventory stays 17 classes. OpenCoordinator remains a single 10-case class (5 cases in the primary partial + 5 in Part2); PopupBoundary remains a single 18-case class (5 + 13). The 160-case total and per-class counts used by the P5-T171 filter are unchanged by the split.
