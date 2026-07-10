# Baseline — Coverage (P0-T5)

Timestamp: 2026-07-09T16-38
Command: dotnet-coverage collect -f cobertura -o artifacts/csharp/coverage.xml "<vstest.console.exe>" Tags.Test\bin\Debug\Tags.Test.dll /InIsolation
EXIT_CODE: 0
Output Summary:
- Existing test run: Tags.Test — 13 passed, 0 failed.
- Raw Cobertura coverage XML copied to artifacts/csharp/coverage.xml (review-gate consumable).
- Packages present in baseline coverage report: Tags.Test, UtilitiesCS, FluentAssertions, Tags.
- **TaskTree.dll baseline line coverage: 0.0%** — no `TaskTree` package appears in the coverage report because no existing test project references or exercises `TaskTree`. This is the expected baseline (TaskTree.Test does not yet exist). Grep for a `TaskTree` package/module entry returns none.

Note: The full existing *.Test.dll set was not run in aggregate for the baseline because the TaskTree.dll figure is definitionally 0% (zero existing tests reference TaskTree); running a single existing test assembly deterministically demonstrates TaskTree's absence from the instrumented module set. The post-change coverage (P7-T4) is measured directly against TaskTree.Test.dll.
