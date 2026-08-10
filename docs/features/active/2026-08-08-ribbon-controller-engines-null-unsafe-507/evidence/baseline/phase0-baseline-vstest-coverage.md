# Phase 0 — Baseline vstest (coverage)

Timestamp: 2026-08-08T16-45

Command: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
Invocation used:
`MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" QuickFiler.Test/bin/Debug/QuickFiler.Test.dll SVGControl.Test/bin/Debug/SVGControl.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskMaster.Test/bin/Debug/TaskMaster.Test.dll TaskTree.Test/bin/Debug/TaskTree.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /EnableCodeCoverage /InIsolation`

MSTest Discovery Caveat applied: assembly list was built via a recursive glob for
`*.Test.dll` under `bin/Debug`, then filtered to exclude any path containing `.claude`
(`find . -iname "*.Test.dll" -path "*bin/Debug*" | grep -v "\.claude"`), yielding exactly the 9
first-party test assemblies (QuickFiler.Test, SVGControl.Test, Tags.Test, TaskMaster.Test,
TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test).
`/InIsolation` was added per prior session precedent (Moq-based assemblies require it to avoid a
`Setup` `FileNotFoundException`).

Precondition note: the solution was rebuilt with default properties (no `/p:Nullable=enable`)
immediately before this run, because the P0-T4 nullable-gate build attempt left
`TaskMaster.Test.csproj`'s prior build output absent from `bin/Debug` (its upstream dependency,
UtilitiesCS.csproj, failed to compile under the forced nullable context). This is a bootstrap
rebuild, not a plan deviation; it uses the same `/p:Configuration=Debug /p:Platform="Any CPU"`
properties as the analyzer baseline (P0-T3), just without the analyzer/nullable flags.

EXIT_CODE: 0

Output Summary: `Total tests: 6294`, `Passed: 6294`, `Failed: 0`, `Skipped: 0` (`Test Run
Successful.`, 1.0151 minutes). Coverage file
`TestResults/278d775f-d952-4eeb-98f6-1f4f00e47f0a/DanMoisan_MEGALODON4_2026-08-08.15_43_02.coverage`
was converted to Cobertura via `dotnet-coverage merge <file> -f cobertura -o
docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-coverage.cobertura.xml`
(exit 0). Repo-wide `line-rate` from the Cobertura root `<coverage>` element:
`0.7443263443535741` = **74.43%**. This raw dotnet-coverage repo-wide line-rate (all instrumented
assemblies, unfiltered) is the headline figure used for the Phase 0 vs Phase 2 no-regression
comparison (P2-T5); it is not the first-party-only denominator figure used elsewhere in this
repository's coverage history, but it is computed identically at baseline and at final QC so the
delta comparison is apples-to-apples.
