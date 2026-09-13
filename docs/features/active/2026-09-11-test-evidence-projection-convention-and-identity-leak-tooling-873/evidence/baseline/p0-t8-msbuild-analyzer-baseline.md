# P0-T8 — C# Analyzer Baseline

Timestamp: 2026-09-13T04-57
Task: [P0-T8]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
MSBuild resolution: vswhere under the thirty-two-bit Program Files Visual Studio Installer
directory, `-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find
"MSBuild/**/Bin/MSBuild.exe"`, first result. The repository build wrapper was not used.
EXIT_CODE: 1
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 2

Build lock: acquired for item 873 before the command and released immediately after it returned.
Outlook was not running and no test host was running, verified by a process probe before the run;
no process was killed.

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

Each error was reported twice in the log, once on a node-prefixed line and once on the summary
repetition of the same diagnostic; the MSBuild summary counts two errors, which is the recorded
value.

## Cause, established by observation rather than inference

This is a pre-existing analyzer HintPath version skew, not a condition this delivery created:

- `UtilitiesCS/UtilitiesCS.csproj` line 3 and line 1301 name `Meziantou.Analyzer.3.0.235` in the
  props `Import` and in the existence `Error` condition, and `UtilitiesCS/packages.config` lines 17
  through 22 pin `Meziantou.Analyzer` at version `3.0.235`.
- The same project file's line 1309 `<Analyzer Include>` item still names
  `Meziantou.Analyzer.3.0.203`.
- The restored package tree contains `packages/Meziantou.Analyzer.3.0.235` and
  `packages/Roslynator.Analyzers.5.0.0` only. The `3.0.203` directory the analyzer item names does
  not exist, so `csc` is handed a path that is absent and reports CS0006.
- The stale literal is present at the base anchor: `git grep -c "Meziantou.Analyzer.3.0.203"
  refs/base-anchor-873 -- "*.csproj"` reports one occurrence in each of 15 project files.
- `git diff --name-only refs/base-anchor-873 HEAD` lists six paths, all Markdown documents under
  `docs/`. This branch has modified no project file, no `packages.config` and no source file.

Because `/t:Rebuild` cleans before compiling, this single cause makes the analyzer pass, the
nullable pass and any later test-console run fail identically. It is recorded once here and
cross-referenced from P0-T9 rather than re-investigated there.

## Output Summary

EXIT_CODE: 1, MSBUILD_WARNING_COUNT: 0, MSBUILD_ERROR_COUNT: 2. The solution does not compile in
this worktree for a pre-existing reason that originates on the base commit and that this delivery
neither introduced nor is scoped to repair. Under the plan's baseline-relative C# gate rule, this is
the recorded baseline against which the Phase 7 analyzer gate is compared; the comparison is
"no worse than" this exit code and this error count.

Caveat recorded deliberately, so a later reader does not misread the warning count: the recorded
`MSBUILD_WARNING_COUNT: 0` is the absence of a measurement rather than a clean analyzer result,
because compilation stopped before any analyzer executed. A later gate that compares a warning
count against this floor of zero would be comparing against a value no successful build produced.

EXIT_CODE: 1
