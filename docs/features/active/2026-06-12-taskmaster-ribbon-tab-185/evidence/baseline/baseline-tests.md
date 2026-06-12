# Baseline — Tests + Coverage (Issue #185)

Timestamp: 2026-06-12T10-42

Command: vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage

Note: `/InIsolation` is required for this Moq-based test assembly (without it, vstest 17.x
under this VS18 toolchain can fail with a Setup FileNotFound for the STTE adapter). The run
was invoked with `MSYS_NO_PATHCONV=1` to prevent git-bash from rewriting the assembly path.

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 68, Passed: 68, Failed: 0. Total time: 4.54s.
- This is the TaskMaster.Test assembly only (the assembly that contains the in-scope
  RibbonExplorerXmlTests). It is not the full repository suite.

Coverage headline (from this command's .coverage, converted via dotnet-coverage merge -f xml):
- Aggregate line coverage across all instrumented modules: covered=7711, partial=442,
  not-covered=84260, total=92413 lines -> 8.34% (8.82% including partially covered).
- The aggregate is dominated by third-party/other-project DLLs that this single-assembly run
  loads but does not exercise (Deedle 0/10748, log4net, System.Linq.Async, FluentAssertions,
  UtilitiesCS, ToDoModel, TaskVisualization, Swordfish.NET.General, Tags). It is therefore not
  a meaningful repository-wide figure; it is the headline emitted by the plan's specified
  single-assembly command and is recorded here verbatim for an apples-to-apples delta vs P2-T4.

First-party module breakdown (lines covered / partial / not-covered):
- TaskMaster.Test.dll: 2206 / 46 / 118
- TaskMaster.dll: 804 / 63 / 2359
- ToDoModel.dll: 45 / 12 / 3580
- UtilitiesCS.dll: 1774 / 176 / 39134
- TaskVisualization.dll: 13 / 0 / 3600

Changed-code coverage note: The in-scope production change is to a non-compiled XML resource
(RibbonExplorer.xml), which is not line-instrumentable. The in-scope test change adds new
test methods to RibbonExplorerXmlTests (TaskMaster.Test.dll), which are executed by this run.
The repository-wide >=80% gate is evaluated against the full suite, not this targeted run;
this baseline establishes the pre-change figure under the plan's exact command for delta
comparison in P2-T5.
