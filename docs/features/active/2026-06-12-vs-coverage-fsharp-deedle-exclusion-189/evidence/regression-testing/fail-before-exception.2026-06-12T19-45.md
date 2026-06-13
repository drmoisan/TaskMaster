# Fail-Before Exception Dossier (P0-T5, AC4 fail-before)

Timestamp: 2026-06-12T19-45

Command (attempted reproduction):
```
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:Deedle /collect:"Code Coverage" /Settings:TaskMaster.runsettings /InIsolation
```

EXIT_CODE: 0 (run succeeded; the VerificationException did NOT reproduce from the CLI)

WhyFailingRunImpossible:
The `System.Security.VerificationException: Operation could destabilize the runtime` reported in issue #189 is produced by Visual Studio's "Analyze Code Coverage for All Tests" data collector (`datacollector://microsoft/CodeCoverage/2.0`), which instruments `FSharp.Core`/`Deedle` and rewrites IL that fails CLR verification. The CLI `vstest.console.exe /collect:"Code Coverage"` collector in this environment (VSTest 18.7.0) does not instrument those modules in the same way: the without-exclusion CLI `/collect` run completed with all 42 Deedle tests passing (see `evidence/baseline/verificationexception-baseline.2026-06-12T19-45.md`). Because the CLI does not reproduce the failure even WITHOUT the exclusion, a CLI failing-run artifact cannot be captured here. Visual Studio cannot be driven from this automation environment, so the IDE-native failing run cannot be executed either.

## Alternative Proof — Reported VS Failure Stack (from issue #189)

The defect is documented in issue #189 with the following reported failure stack for the 17 Deedle tests (`DfDeedle_COM_Tests`, `DfDeedle_Tests`, `DeedleTests.DeedleDoodles`) under VS coverage:

```
System.Security.VerificationException: Operation could destabilize the runtime.
  SeqModule.ToArray
   -> ArrayModule.OfSeq
    -> Series.ctor
     -> Frame.FromRows / Frame.FromColumns
      -> DfDeedle.FromArray2D / DfDeedle.FromDefaultFolder
```

Issue #189 confirms (Suspected Cause / Notes):
- The failure is FSharp.Core instrumentation under coverage, not a defect in F#/Deedle (the tests pass uninstrumented).
- `coverage.config` already excludes `.*FSharp.*`, `.*Deedle.*`, and five other module patterns from dotnet-coverage instrumentation, which is why the VS Code Koverage task does not hit the failure.
- Visual Studio never reads `coverage.config`; no `.runsettings` in the repo previously contained a `<DataCollectors>` Code Coverage `ModulePaths` exclusion block (confirmed in `evidence/baseline/runsettings-and-coverage-config.2026-06-12T19-45.md`).

This establishes the pre-fix failing condition that the runsettings exclusion (Phase 1) removes for the VS coverage path. AC6 records the VS-native pass-after confirmation as pending user action.

SearchScope:
- docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/regression-testing/
- docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/baseline/

SearchPatterns:
- fail-before-exception.*.md
- verificationexception-baseline.*.md

SearchResult:
- No prior fail-before-exception dossier existed before this run; this file is the authoritative fail-before record.
- The executed without-exclusion CLI run is recorded at `evidence/baseline/verificationexception-baseline.2026-06-12T19-45.md` (passed, did not reproduce the exception).
