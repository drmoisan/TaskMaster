# Phase 0 — Baseline MSTest with Coverage (issue #211)

Timestamp: 2026-06-24T16-30
Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"` then `Microsoft.CodeCoverage.Console.exe merge <.coverage> -f xml -o baseline-coverage.xml`
EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful. Total tests: 4082; Passed: 4082; Failed: 0. (The single known-flaky UI-thread/dispatcher test did not fail this run.)
- Coverage (Cobertura-style merged XML, instruments ALL loaded modules incl. vendored, so the whole-process figure is low and not the policy gate):
  - Whole-process line_coverage: 39.30% (includes vendored SVGControl / UtilitiesSwordfish and all third-party modules).
  - First-party module `UtilitiesCS.dll`: line_coverage 85.46% (35878 covered / 5179 not covered).
  - First-party module `TaskMaster.dll`: line_coverage 49.41% (1128 covered / 1082 not covered). The TaskMaster.dll figure includes VSTO lifecycle / WinForms / Outlook-Interop classes that are `[ExcludeFromCodeCoverage]`-exempt per CLAUDE.md.
- Authoritative baseline references for the post-change delta: `UtilitiesCS.dll` = 85.46%; `TaskMaster.dll` = 49.41%; whole-process = 39.30%.
- New code added by this plan (`StoreWrapperInitClock`, `StoreWrapperInitProbe` in UtilitiesCS.dll; `StartupDiagnosticsProbe.EmitPhaseNet`/`ComputeNetMs` in TaskMaster.dll) does not yet exist at baseline (coverage = N/A pre-change).
- Raw merged XML preserved at `evidence/baseline/baseline-coverage-2026-06-24T16-30.xml`.
