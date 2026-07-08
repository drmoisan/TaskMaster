# Phase 5 — Final-QC Tests + Coverage (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Settings:TaskMaster.runsettings
         vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /Settings:TaskMaster.runsettings
         dotnet-coverage merge <2 .coverage> -o final-cov.cobertura.xml -f cobertura
EXIT_CODE: 0

Output Summary:
Tests (all pass):
- UtilitiesCS.Test: Total 3912, Passed 3912, Failed 0 (baseline 3904; +8 new: 4 DefaultOn + 4 store).
- TaskMaster.Test: Total 107, Passed 107, Failed 0 (baseline 102; +5 load-path/fail-soft).
- Combined: 4019 passed, 0 failed.

Coverage (merged Cobertura, both assemblies):
- Repo line-rate (deduplicated, all first-party + vendored, production + test): 74.11% (92358/124617).
- First-party production-only (deduped, excl vendored Swordfish/SVGControl + all .Test): 62.04%
  (59507/95915). This figure is the pre-existing repo state dominated by VSTO/WinForms/COM-host-bound
  code that CLAUDE.md formally exempts from the 80% floor (testable-denominator exemption); it is not
  a cycle-3 regression (baseline 61.98% -> 62.04%, a slight improvement from the new tests).

New/changed-file coverage (the cycle-3 >= 90% strict target):
- LcppnFolderPredictorStore.cs (new): 32/32 = 100.00%
- AppAutoFileObjects.FolderPredictorLoad.cs (new — LoadFolderPredictorAsync + UseLcppnPredictor): 10/10 = 100.00%
- LcppnFolderPredictorConfig.cs (changed): 100/100 = 100.00%
- OlFolderClassifierGroup.cs (changed): 48/66 = 72.73% (up from 65.38% baseline). The file-level
  figure includes the pre-existing Outlook-COM-bound BuildClassifiersAsync body (uncovered without a
  live Outlook process); the cycle-3 changed lines (the FolderPredictorConfig resolver, the
  ResolveFolderPredictorConfigFromSettings helper, and the GetFolderPredictorAsync seam) are exercised
  by the new AC21/AC22 tests. The serialize block (P3-T2) sits inside the COM-bound build method.
- LcppnFolderPredictor.cs (unchanged): 344/344 = 100.00%
- AppAutoFileObjects.cs: 46/665 = 6.92% (unchanged; VSTO-host-bound; the only cycle-3 edits are the
  partial keyword + two wiring call sites, both inside the COM-bound load orchestration).

No placeholders. vstest /InIsolation required for the Moq assemblies; coverage + module excludes via
TaskMaster.runsettings.
