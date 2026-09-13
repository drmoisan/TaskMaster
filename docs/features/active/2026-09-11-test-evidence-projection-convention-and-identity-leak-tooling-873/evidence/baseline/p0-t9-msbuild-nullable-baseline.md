# P0-T9 — C# Nullable Baseline

Timestamp: 2026-09-13T04-58
Task: [P0-T9]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
MSBuild resolution: vswhere under the thirty-two-bit Program Files Visual Studio Installer
directory, `-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find
"MSBuild/**/Bin/MSBuild.exe"`, first result. The repository build wrapper was not used.
No solution-wide nullable opt-in property was added: `CLAUDE.md` records that `/p:Nullable=enable`
conscripts every file that never adopted the pragma, and the continuous-integration command omits it
deliberately.
EXIT_CODE: 1
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 2

Build lock: acquired for item 873 before the command and released immediately after it returned.
Outlook was not running and no test host was running; no process was killed.

## Verbatim MSBuild summary lines

```
    0 Warning(s)
    2 Error(s)
```

## Diagnostic set (absolute host paths reduced to the project file's repository-relative path)

```
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [VBFunctions/VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [UtilitiesCS/UtilitiesCS.csproj]
```

Same cause as P0-T8, diagnosed once there and not re-investigated here: a pre-existing analyzer
HintPath version skew present at the base anchor in 15 project files, where the `<Analyzer Include>`
item names `Meziantou.Analyzer.3.0.203` while `packages.config`, the props `Import` and the restored
package tree all carry `3.0.235`. Because both gates use `/t:Rebuild`, the single cause produces an
identical failure in both.

## Output Summary

EXIT_CODE: 1, MSBUILD_WARNING_COUNT: 0, MSBUILD_ERROR_COUNT: 2. This is the recorded nullable
baseline. Under the plan's baseline-relative C# gate rule, the Phase 7 nullable gate is compared
against this exit code and this error count and must be no worse.

Caveat recorded deliberately: `MSBUILD_WARNING_COUNT: 0` here is the absence of a measurement rather
than a clean nullable result, because compilation stopped before any file's nullable flow analysis
ran.

EXIT_CODE: 1
