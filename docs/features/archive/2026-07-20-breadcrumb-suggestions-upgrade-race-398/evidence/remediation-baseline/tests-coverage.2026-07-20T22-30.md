# Phase 0 — Full First-Party Suite + Coverage Baseline (P0-T5)

Timestamp: 2026-07-20T22-52

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`

Supplementary coverage command (numeric line/branch headline): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:cobertura.remediation398.runsettings /InIsolation`
(Cobertura-output form of the same Microsoft Code Coverage collector, scoped to the first-party
production denominator UtilitiesCS.dll + QuickFiler.dll with [ExcludeFromCodeCoverage] honored and
vendored/mixed-mode modules excluded. `/EnableCodeCoverage` alone records only block coverage, so the
Cobertura-output variant is used to obtain the true branch %. Test totals are identical between the two
runs. Explicit assembly paths are used; no recursive `*.Test.dll` discovery, so no `\.claude\` path is
loaded. `/InIsolation` is required for the Moq-based test assemblies.)

EXIT_CODE: 0

Output Summary:
- Total tests: 5061. Passed: 5061. Failed: 0. (Both the `/EnableCodeCoverage` run and the Cobertura
  run report identical 5061/5061/0 totals.)
- First-party denominator coverage (UtilitiesCS.dll + QuickFiler.dll, Cobertura aggregate root):
  line 86.54% (lines-covered 43139 / lines-valid 49851), branch 80.26% (branches-covered 9671 /
  branches-valid 12050). Both above the >= 85% line and >= 75% branch floors.
- Raw instrumented-scope block figures from the default `/EnableCodeCoverage` collector (all
  first-party assembly lines, no exemptions applied): UtilitiesCS.dll line 86.80% / block 87.45%,
  QuickFiler.dll line 70.53% / block 72.82%. These raw figures are recorded for completeness; the
  authoritative first-party floor is the exemption-honored denominator above.
- This is the R1/R2 pre-change baseline: no test-file split or coverage artifact has been produced yet.
