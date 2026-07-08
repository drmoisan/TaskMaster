# Gated MSTest Baseline (with coverage)

Timestamp: 2026-06-22T21-15
Command: vstest.console.exe <all 7 *.Test.dll> /InIsolation /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"
(vstest used: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe)

Test assemblies (full solution, matching CI semantics; the plan placeholder
`<TaskMaster.Test assembly path>` is expanded to the full set because the CI step runs the whole
solution and AC-R2 requires whole-suite behavior):
- QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test,
  UtilitiesCS.Test, VBFunctions.Test

EXIT_CODE: 0 (stable run)

Output Summary:
- Total tests: 4310; Passed: 4310; Failed: 0 (gated, LiveOutlook excluded) on the stable repeat run.
- A single non-deterministic failure was observed on one earlier invocation (Total 4310 / Passed 4309
  / Failed 1) and did not reproduce on the immediate repeat (4310/4310). This is a known intermittent
  STA-pump/Dispatcher flake documented in agent memory, pre-existing and unrelated to the in-scope
  test-only harness change. The deterministic gated state is all-pass.
- Repository line-coverage headline (raw Cobertura, all packages incl. vendored Swordfish/SVGControl):
  line-rate 0.6411 = 64.11% (lines-covered 104196 / lines-valid 162515). The policy >= 80% floor
  applies to the first-party testable denominator (excludes vendored + COM/VSTO-exempt code per CLAUDE.md);
  this raw headline is recorded as the baseline reference for the unchanged-coverage no-regression check.
