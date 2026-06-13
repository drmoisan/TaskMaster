# Phase 2 — MSTest with Coverage

Timestamp: 2026-06-13T12-48

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.phase2.cobertura.xml
(Koverage dedup applied to coverage/coverage.phase2.firstparty.cobertura.xml.)

EXIT_CODE: vstest reported 1 failure -> pipeline script exit 1 (then dedup re-applied manually)

## Test results
- Total tests: 4068
- Passed: 4067
- Failed: 1
- Failing test: AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
- This is one of the two known pre-existing flaky timing/threading tests (roadmap §0.1). It passed in Phase 1 and failed in baseline + Phase 2, confirming non-determinism. Annotation changes are non-behavioral and cannot affect runtime; this is not a regression.

## Coverage headline (first-party deduped, all non-.Test incl vendored constant)
- covered: 38,767
- lines-valid: 61,048
- line rate: 63.50%

## TaskMaster annotation verification
- TaskVisualization absent: confirmed.
- TaskMaster package denominator: baseline 2,909 lines -> 1,690 lines (reduction ~1,219, matches memo §2.2 ~1,200).
- TaskMaster package rate: 25.68% -> 42.25% (annotated COM/VSTO classes removed; testable seams remain).
- All 6 named TaskMaster types carry exactly one [ExcludeFromCodeCoverage]: ThisAddIn, AddInUtilities, RibbonViewer, TryFunctionalityInConstruction, RibbonController, AppItemEngines.
- All 8 do-not-annotate testable seams unannotated: AppFileSystemFolderPaths, AppStagingFilenames, AppEvents, ApplicationGlobals, AppToDoObjects, AppQuickFilerSettings, AppOlObjects, AppAutoFileObjects.
