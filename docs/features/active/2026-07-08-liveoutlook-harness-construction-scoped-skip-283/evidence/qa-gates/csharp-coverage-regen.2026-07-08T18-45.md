# C# Coverage Regeneration — Canonical Machine-Verifiable Artifact (Issue #283, R2)

Timestamp: 2026-07-08T18-52
Command: `dotnet-coverage collect --output artifacts/csharp/coverage.xml --output-format cobertura --settings coverage.config -- "<VS18>/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
EXIT_CODE: 0

Output Summary:
- Test result: Total tests 231, Passed 231, Failed 0. Total time ~2.63s.
- LiveOutlook category remains filtered out via `/TestCaseFilter:TestCategory!=LiveOutlook` (no `[TestCategory("LiveOutlook")]`-tagged tests executed; the COM-bound live harness is excluded).
- Canonical machine-readable Cobertura artifact written to `artifacts/csharp/coverage.xml` (permitted non-evidence coverage-output path; `artifacts/coverage/` is forbidden, `artifacts/csharp/` is not).
- Overall root `line-rate=0.16913` = 16.91% (lines-covered 11768 / lines-valid 69579). Baseline (P0-T6 / remediation baseline) was 16.75% (11638 / 69461). No regression; overall coverage increased marginally.
- NEW seam file `TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunner.cs` line coverage: 100.0% (Cobertura per-`class`: `LiveOutlookHarnessRunner` 52/52 line-rate=1; nested `HarnessOutcome` struct 8/8 line-rate=1; combined 60/60 = 100.0%). Exceeds the >= 90% new-code floor.
- Note on line-count: the earlier scratch measurement reported the seam as 30/30; dotnet-coverage's Cobertura per-`<line>` accounting here reports 60/60 across the two nested classes. The rate is 100.0% under both accountings; the floor comparison (>= 90%) and the reported figure (100.0%) are unaffected.

Scope note: `TaskMaster.Test.dll` is the sole run assembly, matching the baseline's measured scope (TaskMaster.Test.dll run vs whole instrumented set), so the 16.75% -> 16.91% comparison is like-for-like.
