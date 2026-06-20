# Phase 0 — MSTest + Coverage Baseline (Issue #207, increment 3)

Timestamp: 2026-06-19T21-15

Command:
- vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation
  (VS18 Community vstest.console.exe; /InIsolation required for Moq test assemblies on this toolchain; MSYS_NO_PATHCONV=1 under git-bash)
- dotnet-coverage merge <.coverage> -o TestResults/baseline-cov.cobertura.xml -f cobertura
  (convert the binary .coverage attachment to Cobertura XML to read numeric line-rate)

EXIT_CODE: 0

Output Summary:
- Test run successful. Total tests: 107. Passed: 107. Failed: 0. Skipped: 0. Total time: 3.57 s.
- Aggregate Cobertura line-rate (all instrumented modules, including vendored/untested assemblies in the denominator): 0.12827 => 12.83%. lines-covered=8336, lines-valid=64987.
- This aggregate figure is the raw all-module repo-wide number; it is NOT the CLAUDE.md "testable first-party denominator with COM/VSTO exemptions" figure. It is recorded here as the deterministic, reproducible baseline so the post-change run (P2-T4) can be compared with the identical methodology for no-regression verification.
- The TaskMaster.Test assembly is the in-scope test assembly for increment 3 (it hosts the new RemindersProbeScheduleTests).

Note: increment 3 adds the pure, fully-tested RemindersProbeSchedule type plus a small number of COM/VSTO-exempt lines in AppEvents.Hook() and generated Settings.Designer.cs lines. The new-code coverage obligation (>= 90%) applies only to RemindersProbeSchedule, verified at P2-T4/P2-T5.
