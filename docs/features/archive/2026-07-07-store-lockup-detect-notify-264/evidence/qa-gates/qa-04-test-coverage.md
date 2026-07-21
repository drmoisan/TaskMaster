# QA Gate 04 — Tests + Coverage (P9-T4)

Timestamp: 2026-07-08T08-39

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:<cobertura.runsettings> /TestCaseFilter:"TestCategory!=LiveOutlook"`
(Code Coverage DataCollector enabled via runsettings in Cobertura output mode = the
`/EnableCodeCoverage` mechanism with an offline-parseable result.)

EXIT_CODE: 0

Output Summary:
- Total tests: 4481. Passed: 4481. Failed: 0. Total time ~41 s. (Baseline was 4441; +40 = the new
  F4 tests: 6 CurrentStoreContext, 6 LockupStallDecider, 6 ThreadMonitor, 4 StoreLockupAttribution,
  6 StoreLockupResponder, 3 MyBoxModeless, 3 AppOlObjects attribution-context.)

## New-code coverage (per F4 file, [ExcludeFromCodeCoverage]-honoring methodology)

The runsettings restores the collector's default `<Attributes><Exclude>` set so
`[ExcludeFromCodeCoverage]` host shells and compiler-generated closures are excluded, per the
CLAUDE.md attribute-based exemption. Per-file line coverage (aggregated across all class entries
per file):
- CurrentStoreContext.cs: 48/52 = 92.3%
- LockupStallDecider.cs (incl. LockupAttribution): 24/24 = 100.0%
- StoreLockupAttribution.cs: 20/20 = 100.0%
- StoreLockupResponder.cs: 98/102 = 96.1%
- ThreadMonitor.cs (testable seam; Run/Tick/PingAndAwaitDiagnosticWindow/GetStackTrace exempt): 78/78 = 100.0%
- MyBoxModeless.cs (4-arg host-Show overload exempt): 66/66 = 100.0%
- F4 new-code aggregate: 334/342 = 97.7%

All F4 new files meet the >= 90% new-code threshold.

## Repository / first-party coverage

- [ExcludeFromCodeCoverage]-honoring methodology (testable denominator): UtilitiesCS package line
  coverage = 90.50% (>= 80% floor); overall Cobertura root = 60.82%. TaskMaster package = 63.03%
  (dominated by pre-existing exempt VSTO/WinForms; F4's TaskMaster additions are one tested wrap
  plus exempt startup wiring).

## No-regression (apples-to-apples, identical methodology to P0-T9 baseline)

Measured with the same runsettings as the P0-T9 baseline (no attribute-exclude), so the comparison
is valid:
- Overall line-rate: baseline 56.51% -> post-change 56.69% (UP).
- UtilitiesCS package: baseline 88.25% -> post-change 88.41% (UP).
- TaskMaster package: baseline 66.53% -> post-change 66.57% (UP).
No regression on any first-party package or overall; coverage improved.
