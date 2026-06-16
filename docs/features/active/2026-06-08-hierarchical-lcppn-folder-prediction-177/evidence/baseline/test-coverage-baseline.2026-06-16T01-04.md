# Phase 0 — Baseline Test Run + Coverage (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Settings:TaskMaster.runsettings
         vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /Settings:TaskMaster.runsettings
         dotnet-coverage merge <2 .coverage> -o baseline-cov.cobertura.xml -f cobertura
EXIT_CODE: 0

Output Summary:
- UtilitiesCS.Test: Total tests 3904, Passed 3904, Failed 0.
- TaskMaster.Test: Total tests 102, Passed 102, Failed 0.
- Combined: 4006 passed, 0 failed.

Coverage (merged Cobertura, both assemblies):
- Repo line-rate (raw, all modules incl. vendored): 59.03% (101806/172452 — counts duplicated
  multi-assembly modules).
- Repo line-rate (deduplicated by filename, all first-party + vendored, production + test): 74.05%
  (92060/124326).
- First-party production-only (deduped, excluding vendored Swordfish/SVGControl and all .Test
  assemblies): 61.98% (59417/95859).

In-scope file coverage at baseline:
- OlFolderClassifierGroup.cs: 34/52 = 65.38%
- LcppnFolderPredictorConfig.cs: 100/100 = 100.00%
- LcppnFolderPredictor.cs: 344/344 = 100.00%
- AppAutoFileObjects.cs: 46/665 = 6.92% (VSTO-host-bound; the new LoadFolderPredictorAsync logic is
  placed in a separate testable partial file per the plan and will be held to >= 90%).

vstest required /InIsolation for the Moq-based assemblies (per repo environment note); coverage and
module excludes (Moq, FluentAssertions, MSTest, FSharp/Deedle, Castle.Core) supplied via
TaskMaster.runsettings (its Code Coverage DataCollector is default-enabled, so /EnableCodeCoverage
was not additionally passed to avoid double-activation).
