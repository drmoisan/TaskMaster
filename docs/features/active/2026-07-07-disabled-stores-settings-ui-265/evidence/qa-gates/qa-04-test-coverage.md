# Phase 7 — QA Gate 04: Test + Coverage (P7-T4)

Timestamp: 2026-07-08T04-35

Command (plan text): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
Command (as executed, canonical numeric-coverage path): `dotnet-coverage collect --output coverage/utilitiescs-postchange.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:<cov.runsettings Workers=4> /InIsolation`

EXIT_CODE: 0

Output Summary:
- Test result: Total tests: 4230. Passed: 4230. Failed: 0. (Baseline 4223 + 7 new F5 controller tests.)
- Repository first-party testable-denominator line coverage (UtilitiesCS package): 88.01%
  (>= 80% floor). Note: the raw Cobertura overall (72.34%) is polluted by runtime-instrumented
  vendored assemblies and is not the testable denominator.
- Per-new-file line coverage (aggregated across all class + async-state-machine nodes per file):
  - DisabledStoresController.cs: 91.67% (66/72 lines) — >= 90% target. PASS.
    (The only uncovered lines are inside the [ExcludeFromCodeCoverage] Launch() WinForms shell
    and the catch/MyBox branch's non-executed instrumentation; the tested paths — PopulateRows,
    Dgv_CellContentClick guards + happy path, ReenableAsync success and failure — are covered.)
  - DisabledStoreRow.cs: no coverable sequence points (pure auto-property POCO). There are no
    uncovered lines; every property is exercised and asserted by
    PopulateRows_ProjectsServiceEntriesIntoRows. Vacuously satisfies the >= 90% target (0
    uncovered lines). This mirrors the general-unit-test "type-only / trivial" clarification.
  - StoreLaunchReadinessEvaluator.cs (new shared helper): 100% (13/13 lines) — >= 90%. PASS.
- Exempt new files (COM/VSTO/WinForms exemption, excluded from the new-code numerator):
  - IDisabledStoresViewer.cs: interface-only, no coverable lines (legitimately 0% executable).
  - DisabledStoresViewer.cs: 0% (0/14) — WinForms form-derived.
  - DisabledStoresViewer.Designer.cs: 0% (0/82) — Designer-generated.
- Verdict: EXIT 0; new testable-code coverage >= 90%; repository testable-denominator >= 80%. PASS.
