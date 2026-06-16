# Phase 0 — Baseline Tests + Coverage (Cycle 4, #177 / AC25)

Timestamp: 2026-06-16T10-26
Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage`
EXIT_CODE: 0

(Note: `/InIsolation` is required for this Moq-using assembly to avoid the STTE Setup
FileNotFound failure documented in agent memory. Coverage `.coverage` binary was converted to
Cobertura with `dotnet-coverage merge ... -f cobertura` for numeric extraction.)

## Test results
- Filtered FilePathHelper_Tests run: Total 31, Passed 31, Failed 0 (EXIT_CODE 0).
- Full UtilitiesCS.Test assembly run: Total 3912, Passed 3912, Failed 0 (EXIT_CODE 0).

## Coverage headline (baseline, pre-change)
- Assembly aggregate line-rate (all loaded modules incl. vendored/COM-bound): 0.5932 (59.32%).
  This raw aggregate is below 80% because it includes COM/VSTO/WinForms-bound and vendored code
  that is formally exempt per the CLAUDE.md testable-denominator exemption. It is recorded as the
  raw figure only; it is not the policy denominator.
- Target file `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` class line-rate: 0.8462
  (84.62%); per-line parse: 542/638 covered = 84.95%. This is the meaningful pre-change baseline
  for the changed-line coverage delta in P3-T5. `FilePathHelper` is first-party non-exempt code.

Output Summary: 3912/3912 tests pass (31/31 for FilePathHelper_Tests). Baseline FilePathHelper.cs
line coverage = 84.62% (cobertura class line-rate). Assembly raw aggregate line-rate = 59.32%
(includes exempt COM/VSTO/vendored modules; not the policy denominator).
