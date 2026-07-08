# Final QA Gate — hierarchical-lcppn-folder-prediction (#177)

- Timestamp: 2026-06-12T15-26 (UTC)
- Scope: full change set (Phases 1-7), final end-to-end toolchain pass per `csharp-qa-gate`.

## Toolchain (single final pass, in order)

1. Formatting — `dotnet tool run csharpier format .`
   - Command: `dotnet tool run csharpier format .`
   - EXIT_CODE: 0
   - Output Summary: "Formatted 1076 files"; no changes on the final stable pass (only the
     intended feature files differ from HEAD).

2. Linting — .NET analyzers
   - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - EXIT_CODE: 0
   - Output Summary: 0 Error(s), 20 Warning(s). All warnings are pre-existing CS0618/CS8632 in
     unrelated files; no analyzer diagnostic in any new or changed feature file.

3. Type-checking — nullable / TreatWarningsAsErrors
   - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
   - EXIT_CODE: 0
   - Output Summary: 0 Warning(s), 0 Error(s) (incremental gate, the established prior-phase
     behavior). A targeted `UtilitiesCS.csproj` nullable Rebuild confirmed zero nullable diagnostics
     in the new/changed feature files; the only diagnostics a from-scratch nullable rebuild surfaces
     are pre-existing CS86xx in the vendored `SVGControl` project (one of the 4 vendored projects
     excluded from the analyzer/nullable policy per `csharp.md`).

4. Testing — vstest with code coverage
   - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
   - EXIT_CODE: 0 (test run completed; coverage collected)
   - Output Summary: 3890 total; this feature's 77 tests (Phases 1-7) pass deterministically across
     repeated runs. One pre-existing flaky test
     (`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`, a UI-thread/dispatcher
     test outside this feature) intermittently fails under full-suite parallel load and passes in
     isolation; it is unrelated to this feature (active `ci-flaky-test-isolation-176`) and does not
     affect coverage collection.

## Coverage

- Post-change coverage XML: `coverage.xml` (this folder).
- Coverage comparison: `coverage-comparison.md` (this folder).
- UtilitiesCS.dll line coverage: 85.40% strict / 87.57% inclusive (baseline 85.31% / 87.49%); no
  regression, above the 80% floor.
- Every new module/class reaches >= 90% inclusive line coverage.

## Verdict

All four toolchain steps complete in a single final pass. Coverage thresholds met. The only failing
test is a pre-existing, out-of-scope flake confirmed to pass in isolation.
