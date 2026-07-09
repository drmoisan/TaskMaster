# Baseline — MSTest + Coverage (Issue #208, [P0-T5])

Timestamp: 2026-07-09T09-33

Command: vstest.console.exe "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" /EnableCodeCoverage
(Run via VS18 vstest.console.exe with MSYS_NO_PATHCONV=1 and a Windows-style DLL path under git-bash.
The emitted binary `.coverage` was converted to Cobertura for the numeric headline with
`dotnet-coverage merge -f cobertura` — the merge path validated in prior sessions.)

EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful. Total tests: 224, Passed: 224, Failed: 0. Total time ~5.7s.
- Coverage (whole-process, all loaded modules incl. vendored): line-rate 56.51% (40604/71851 lines
  covered), branch-rate 47.31%.
- Coverage (first-party targeted module, TaskMaster.dll — the module this fix edits): line-rate
  66.53%, branch-rate 60.34%.
- The extracted directory-ensure/path-resolution unit does not yet exist at baseline; its baseline
  coverage is therefore not applicable (added in Phase 1).
- Baseline Cobertura preserved at evidence/baseline/baseline.cobertura.xml for the Phase 2
  no-regression comparison.
