# Baseline — MSTest Coverage (Issue #328)

Timestamp: 2026-07-15T18-45
Command: dotnet-coverage collect -f cobertura -o baseline-coverage.2026-07-15T18-45.cobertura.xml "vstest.console.exe UtilitiesCS.Test.dll TaskMaster.Test.dll ToDoModel.Test.dll /Settings:cov.runsettings"
EXIT_CODE: 1

Output Summary:
- Total tests: 4582; Passed: 4563; Failed: 19.
- Whole-process (all modules, incl. vendored) line-rate = 52.66%, branch-rate = 27.26%
  (lines-covered 106318 / 201903; branches-covered 14195 / 52072). The whole-process
  figure is low because it instruments all loaded modules including vendored
  Deedle/FSharp/Swordfish/SVGControl; the repo 80% floor applies to first-party modules.
- The 19 failures are all Deedle/FSharp DataFrame tests (DeedleDoodles, FromArray2D,
  Email2dArray*, GetEmailData*, FromDefaultFolder*, DropFirstN, Exclude*, DfToListEntries,
  FilterToProjectIDs, etc.) that fail only under coverage instrumentation. They pass
  cleanly without instrumentation (verified: ToDoModel.Test 118/118 pass without coverage),
  and the failing set is nondeterministic between coverage runs — a pre-existing
  coverage-instrumentation flakiness pattern, not a functional baseline failure.
- No hang occurred in TaskMaster.Test.

Baseline per-class rates for the four non-exempt target production classes
(authoritative non-zero entry; each class also appears with 0/0 from a non-exercising
assembly due to dotnet-coverage package double-counting):
- UtilitiesCS.OutlookObjects.Store.StoreFilterAttribution: line 100.0%, branch 96.15%
- UtilitiesCS.OutlookObjects.Store.StoresWrapper:           line 98.56%, branch 91.94%
- UtilitiesCS.OutlookObjects.Store.StoreWrapper:            line 94.96%, branch 65.38%
- UtilitiesCS.OutlookObjects.Store.StoreWrapperController:  line 95.21%, branch 88.54%

Methodology note (for consistent P4-T5 delta): coverage is collected via
`dotnet-coverage collect -f cobertura` wrapping vstest over the three test assemblies with
a Workers=4 ClassLevel runsettings. The same method is repeated at P4-T4/P4-T5.
Cobertura saved at baseline-coverage.2026-07-15T18-45.cobertura.xml (same directory).
