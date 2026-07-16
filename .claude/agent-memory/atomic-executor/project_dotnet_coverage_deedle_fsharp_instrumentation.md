---
name: dotnet-coverage-deedle-fsharp-instrumentation
description: dotnet-coverage instrumenting Deedle/FSharp assemblies fails ~20 DataFrame tests; pass module-exclude settings to get a clean full-suite coverage run
metadata:
  type: project
---

Running the full UtilitiesCS.Test + QuickFiler.Test suite under `dotnet-coverage collect` WITHOUT module excludes fails ~20 Deedle/FSharp DataFrame tests (DeedleDoodles, FromArray2D_*, GetEmailDataInView*, DataFrame Exclude/DropFirstN, InitEmailQueue_*). They pass with zero instrumentation (plain vstest exit 0) and pass under coverage once Deedle/FSharp are excluded from instrumentation.

**Why:** `dotnet-coverage` instruments Deedle/FSharp assemblies by default; the repo's own `TaskMaster.runsettings` already excludes those modules (`.*Deedle.*`, `.*FSharp.*`, plus Castle.Core/FluentAssertions/Moq/Microsoft.Testing/MSTest) from the binary Code Coverage collector, but that runsettings does NOT propagate to dotnet-coverage's own instrumentation. F# interop under instrumentation breaks those tests.

**How to apply:** For a clean numeric Cobertura run, create a dotnet-coverage settings XML (`<Configuration><CodeCoverage><ModulePaths><Exclude>...`) mirroring the TaskMaster.runsettings excludes and pass it via `dotnet-coverage collect --settings <file> --output-format cobertura -- vstest.console.exe <dlls> /InIsolation /Settings:<mstest-runsettings>`. Also lower MSTest Workers to 4 (separate /Settings runsettings) to avoid the [[project_utilitiescs_test_parallelism_flakiness]] timing flakiness. With both, the full suite is 4762/4762 green under coverage. The `OpenRead_ShouldReturnReadableStreamForWrappedFile` FS-adapter test is still occasionally flaky under load (passes isolated) — the known shared-file contention, unrelated to any given feature.
