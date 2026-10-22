# Final QA — Step 2, MSBuild Analyzer Gate (P7-T3, AC-30)

Timestamp: 2026-08-27T20-58

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

`$msbuild` is the P0-T4 path:
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:26.17
```

| Metric | Value |
| --- | ---: |
| Analyzer/compiler errors | **0** |
| Warnings | 5 |

## Warning attribution

All 5 warnings are emitted as a bare `warning` with no diagnostic ID — confirmed by extracting every
`: warning <ID>` / `: error <ID>` token from the log, which yields 10 bare `: warning` tokens (each
warning appears twice: once in the per-project output and once in the summary) and **zero** with an ID
attached and **zero** `: error` tokens of any kind.

They are the pre-existing `System.Reactive.PackagesConfigCheck.targets(31,5)` advisory that
`packages.config` is unsupported by System.Reactive v7.0 or later, emitted once per project that
references it. The identical figure of 5 was recorded at baseline (P0-T12), before any change in this
feature, so the change set introduced **no new warning and no analyzer diagnostic**.

## Non-vacuity verification (mandatory)

Count of lines matching `Skipping target "CoreCompile"`: **0**.

A count other than zero would mean the gate compiled nothing and this artifact would have to record FAIL.
Corroborating positive evidence from the same log: **63** `CoreCompile:` target headers, so compilation
and analyzer execution genuinely ran across the solution.

The command uses `/t:Rebuild`, not `/t:Build`. That is load-bearing: MSBuild's up-to-date check does not
invalidate on a command-line `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile`
skipped on every project and this gate could not fail.

The full analyzer set is loaded — Meziantou.Analyzer 3.0.156, the four Roslynator.Analyzers 4.16.0 DLLs,
AsyncFixer, both BannedApiAnalyzers assemblies, and SonarAnalyzer.CSharp — resolvable because P0-T8
back-filled the two version-skewed packages. Without that back-fill each missing `<Analyzer Include>`
path would be `error CS0006` and this gate could never reach exit 0.

Acceptance: `EXIT_CODE: 0` and an analyzer error count of 0. PASS (AC-30).
