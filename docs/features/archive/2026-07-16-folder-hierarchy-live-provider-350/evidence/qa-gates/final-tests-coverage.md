# Final QA — Tests and Coverage (Toolchain Step 4)

Timestamp: 2026-07-18T00-34

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage` (executed as `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:cov-utilitiescs.runsettings /InIsolation`; Cobertura collector scoped to `UtilitiesCS.dll`, repo-standard Deedle/FSharp/vendored excludes, MSTest Workers=4)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 4344; Passed: 4344; Failed: 0. (Baseline 4321 + 23 new tests for this feature: 5 FolderBreadcrumbSegment + 8 GetAncestorChain + 10 OutlookFolderHierarchyProvider.)
- Post-change coverage (production assembly UtilitiesCS.dll, first-party):
  - Line coverage: 88.49% (35905 / 40573 lines).
  - Branch coverage: 82.21% (8279 / 10070 branches).
- New/changed production code coverage:
  - FolderBreadcrumbSegment.cs: 100% line (24/24), 100% branch (12/12).
  - OutlookFolderHierarchyProvider.cs (incl. compiler-generated async state machines): 95.12% line (78/82), 83.33% branch (10/12). The 4 uncovered lines are compiler-generated async fault/exception plumbing in the `ResolveLeafKeyAsync` state machine, not authored source lines.
  - FolderTreeSnapshotQueries.GetAncestorChain (new method): 100% line (24/24).
  - Combined new production code: 96.92% line (126/130) — exceeds the 90% new-code threshold.
- IFolderHierarchyProvider.cs is type-only (interface, no executable lines) and legitimately absent from the executable denominator.
- Coverage report: `DanMoisan_MEGALODON4_2026-07-18.08_13_41.cobertura.xml`.

Test gate green; all new tests pass; no coverage regression versus baseline.
