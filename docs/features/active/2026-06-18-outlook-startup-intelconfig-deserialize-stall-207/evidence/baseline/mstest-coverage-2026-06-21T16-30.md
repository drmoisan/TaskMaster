# Baseline — MSTest with Coverage, LiveOutlook excluded (Issue #207 corrective fix)

Timestamp: 2026-06-22T16-51

Command:
- `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /Settings:TaskMaster.runsettings /TestCaseFilter:"TestCategory!=LiveOutlook"`
  (vstest from VS18 Community TestPlatform; `/InIsolation` required for Moq assemblies; `TaskMaster.runsettings` supplies the Code Coverage DataCollector module excludes for Moq/FluentAssertions/MSTest/FSharp/Deedle/Castle.)
- `dotnet-coverage merge <.coverage> -f cobertura -o evidence/baseline/trx/baseline-2026-06-21T16-30.cobertura.xml`

EXIT_CODE: 0

Output Summary (numeric):
- Total tests: 111. Passed: 111. Failed: 0. (LiveOutlook category excluded; zero LiveOutlook tests exist at baseline, none executed.)
- Aggregate repo-wide line coverage (all instrumented modules): 12.89% (lines-covered=8383 / lines-valid=65052), line-rate=0.12887.
- This aggregate is the all-module raw figure, NOT the CLAUDE.md testable-first-party-denominator measurement (which excludes vendored Swordfish/SVGControl projects, COM/VSTO add-in lifecycle classes, WinForms Designer code, and un-seamed Outlook Interop handlers). Per the established methodology of this feature's prior increments, the aggregate figure is the no-regression comparator; the repo-wide testable-denominator floor is owned by feature/csharp-coverage-uplift.

Cobertura artifact: docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/baseline/trx/baseline-2026-06-21T16-30.cobertura.xml

EVIDENCE_LOCATION_OVERRIDE_REJECTED: an initial dotnet-coverage output to `evidence/baseline/trx/` at the repo root was rejected and moved to the canonical feature-folder path `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/baseline/trx/`.
