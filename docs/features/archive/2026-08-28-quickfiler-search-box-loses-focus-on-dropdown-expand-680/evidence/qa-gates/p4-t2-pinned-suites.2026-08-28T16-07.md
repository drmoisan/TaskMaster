# P4-T2 — Pinned-Suite Green Run (spec AC-5)

Timestamp: 2026-08-28T16-07

Command (DR-1 runner resolution):

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_SearchFocusRegressionTests|FullyQualifiedName~BreadcrumbDropDownSearchIntegrationTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbItemViewerLifecycleCoordinatorTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests" /Logger:"trx;LogFileName=p4-t2.trx" "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t2"
```

EXIT_CODE: 0

Output Summary:

- `Test Run Successful.` Total tests: **75** (> 0); Passed: **75**; Failed: **0**.
  Total time 2.6272 seconds.
- All five spec AC-5 suites ran green, unmodified — established byte-for-byte by the P4-T1 diff gate:
  `QfcItemController_SearchFocusRegressionTests`, `BreadcrumbDropDownSearchIntegrationTests`
  (and `.Part2`), `BreadcrumbDropDownOpenCoordinatorTests` (all three parts),
  `BreadcrumbItemViewerLifecycleCoordinatorTests`, and `ItemViewerBreadcrumbDropDownContractTests`.
- This is the #438 / #400 regression retest required by `issue.md`'s "Integration scenario to
  retest" second clause.
- TRX: the `p4-t2` results subdirectory holds exactly one file, named exactly `p4-t2.trx` (DR-1).

Acceptance: satisfied — `EXIT_CODE: 0`, zero failures, total 75 > 0.
