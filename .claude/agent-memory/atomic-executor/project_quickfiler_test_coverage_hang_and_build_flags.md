---
name: quickfiler-test-coverage-hang-and-build-flags
description: Full-assembly QuickFiler.Test under /EnableCodeCoverage can hang the testhost on a loaded box (retry works); project-level msbuild needs /p:Platform=AnyCPU; bare Timer is CS0104-ambiguous in QfcItemController.TestSupport.cs
metadata:
  type: project
---

Three mechanical facts for QuickFiler work, all verified 2026-08-26.

**1. `/EnableCodeCoverage` on the whole `QuickFiler.Test.dll` can hang.**
`vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
/Settings:scripts\vscode\TaskMaster.cli.runsettings` stalled indefinitely on the first attempt (CPU time
flat at 19.5 s across a 20-second sample, working set frozen) and completed in **12.3 seconds** on an
identical second attempt. `TaskMaster.cli.runsettings` requests `Workers=0` = one worker per logical
processor (24 here) at `ClassLevel`, and the box was carrying ~34% unrelated load.

**Why:** the same load-flakiness that hits `WinFormsPumpHost` / STA-pumping tests, amplified by coverage
instrumentation. See [[winformspumphost-tests-load-flaky]] and [[utilitiescs-test-parallelism-flakiness]].

**How to apply:** run it detached (`run_in_background`) rather than in a foreground Bash call, so a
harness timeout does not orphan the chain. If it stalls, sample CPU time twice ~20 s apart to
distinguish "hung" from "slow"; kill only your own `pwsh -> vstest.console -> testhost` chain (never
shared MSBuild/VBCSCompiler workers) and retry the identical command. A retry with no intervening file
change is not a toolchain-loop restart - prove it with a SHA-256 comparison of the owned files and say so
in the evidence.

**2. Project-level msbuild rejects `"/p:Platform=Any CPU"`.**
`MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj ... "/p:Platform=Any CPU"` fails with
"The BaseOutputPath/OutputPath property is not set for project 'QuickFiler.Test.csproj'". The standalone
project defines `AnyCPU`; `Any CPU` is a solution-level alias. Use `/p:Platform=AnyCPU` for a
project-file build and keep `"/p:Platform=Any CPU"` for `TaskMaster.sln`.

**3. Bare `Timer` is CS0104-ambiguous in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`.**
That file imports both `System.Windows.Forms` and `System.Threading`, so write
`System.Threading.Timer` fully qualified. Same shape as the Outlook `Action`/`Exception` ambiguity in
[[outlook-action-ambiguity]], but from the WinForms side.
