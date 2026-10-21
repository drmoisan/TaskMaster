# Post-Merge C# Gate Revalidation

Timestamp: 2026-09-13T14-54

Task: none. This artifact is required by the merge that Phase 6 performs, not by a numbered task in the plan. It is the counterpart to `p7-t0-phase6-outstanding-disclosure.md`, which reasoned about Phase 6 running after Phase 7 and identified the one route by which a Phase 7 gate's input could change.

## Why this revalidation was run

The P7-T0 disclosure concluded that Phase 6 as the plan writes it cannot modify any tracked file that a Phase 7 gate measures. That conclusion was correct for Phase 6 as written. It did not cover the merge of `origin/main` that this Phase 6 pass performs, because no task in the plan describes a merge.

The merge changed C# sources and project files:

```
QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/SetupAssemblyInitializer.cs
QuickFiler/Controllers/QfcHomeController.cs
TestSupport/TestAssemblyResolver.cs
UtilitiesCS.Test/TestAssemblyInitializer.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

Those are compilation inputs, and `TestSupport/TestAssemblyResolver.cs` is a new C# file. The C# format, analyzer and nullable gates recorded in P7-T4, P7-T5 and P7-T6 were measured before the merge, so they no longer describe the tree as it now stands. The general code-change policy requires restarting the toolchain loop when files change. The three C# gates were therefore re-run against the merged tree.

The merge changed no PowerShell file. The union of merged paths contains no `.ps1` path, so the PowerShell gates in P7-T1, P7-T2, P7-T3, P7-T7 and P7-T8 retain their inputs unchanged and were not re-run. That is a scoped claim about the merged path set, not an assumption.

## Gate 1 — Format

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

```
Checked 1627 files in 5252ms.
```

Verify mode is used, matching P7-T4's reasoning: this delivery changes no C# source file and a repository-wide write-mode format would rewrite source outside its footprint. The check is read-only, so the recorded exit code is a real observation rather than a post-repair one.

Comparison against the P0-T7 baseline exit code of 0: equal, no worse.

## Gate 2 — Analyzers

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

MSBuild resolution: vswhere under the thirty-two-bit Program Files Visual Studio Installer directory, `-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild/**/Bin/MSBuild.exe"`, first result. Resolved leaf name: `MSBuild.exe`.

EXIT_CODE: 0
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 0

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.80
```

Evidence that compilation actually ran: `/t:Rebuild` was used rather than `/t:Build`, so MSBuild's incremental up-to-date check could not skip `CoreCompile`; the last project in the graph, `UtilitiesCS.Test`, emitted its link line and `Done Building Project ... (Rebuild target(s))`; and the elapsed time of 17.80 seconds matches the 17.89 seconds P7-T5 recorded for a full rebuild, which is inconsistent with a halted compile.

Comparison against the operative P0-T8 baseline of 0 / 0 / 0: equal on all three figures.

## Gate 3 — Nullable

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

No solution-wide nullable opt-in property was added, matching P7-T6 and matching the continuous-integration workflow.

EXIT_CODE: 0
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 0

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.61
```

Comparison against the P0-T9 baseline: equal on all three figures.

## Gate 4 — Tests

The C# test gate is not re-run as a separate command here, because the two Phase 6 end-to-end runs are themselves full-suite executions of every discovered Debug test assembly under the coverage collector. Each executed 7222 tests with 0 failures. Those runs are recorded in `evidence/regression-testing/p6-t2-default-output-run.md` and `evidence/regression-testing/p6-t3-external-output-run.md`.

Repository-wide coverage measured by the P6-T2 run, against the `CLAUDE.md` floors rather than the push-down-owned figures:

| Metric | Observed | Floor | Verdict |
|---|---|---|---|
| First-party line coverage | 85.71% (56066/65416) | 80% | clears |
| First-party branch coverage | 79.87% (13495/16896) | 75% | clears |

## Build Lock

Each of the three commands above was run under the shared build lock, acquired immediately before and released immediately after that single command. No lock was held across more than one command.

## Outlook Precondition

Outlook was confirmed not running before the rebuild gates. The running-process count for the Outlook image name was 0. Outlook was not killed; it was already closed.

## Conclusion

All three re-run C# gates return results identical to both their Phase 0 baselines and their pre-merge Phase 7 recordings. The merge introduced no format drift, no analyzer diagnostic and no nullable diagnostic. The Phase 7 C# gate conclusions stand against the merged tree.
