# P0-T8 — C# Analyzer Baseline

Timestamp: 2026-09-13T05-15
Task: [P0-T8]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
MSBuild resolution: vswhere under the thirty-two-bit Program Files Visual Studio Installer
directory, `-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find
"MSBuild/**/Bin/MSBuild.exe"`, first result. The repository build wrapper was not used.
EXIT_CODE: 0
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 0

Build lock: acquired for item 873 before the command and released immediately after it returned.
Outlook was not running and no test host was running, verified by a process probe before the run;
no process was killed.

## Verbatim MSBuild summary lines

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Evidence that compilation actually ran

The exit code alone does not distinguish a compiling build from a halted one, so the following
observations were taken from the same run's log:

- `Build succeeded.` is present.
- 19 lines match `Project .+\.csproj.+ on node`, that is every project in the solution was built.
- 36 lines name `csc.exe`, so the C# compiler was invoked and the analyzer pass therefore executed.
- 0 lines match ` warning [A-Z]+\d+`, so the zero warning count is a measured zero rather than an
  unmeasured one.
- Time elapsed as reported by MSBuild: `00:00:18.53`.

## Diagnostic set

Empty. No `warning` and no `error` diagnostic line was emitted by any project.

## SUPERSEDES: an earlier halted-compile measurement

These figures supersede an earlier measurement of this same command recorded at 2026-09-13T04-57,
which read `EXIT_CODE: 1`, `MSBUILD_WARNING_COUNT: 0`, `MSBUILD_ERROR_COUNT: 2`. That earlier run
failed with two occurrences of `error CS0006` for the metadata file
`..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`,
reported against `VBFunctions/VBFunctions.csproj` and `UtilitiesCS/UtilitiesCS.csproj`. The cause
was a pre-existing analyzer version skew: fifteen tracked project files carry an unconditional
`<Analyzer Include>` item naming version `3.0.203` while `packages.config`, the props `Import` and
the existence `Error` condition all name `3.0.235`, so a cold restore installs only `3.0.235` and
`csc` is handed a path that does not exist.

Superseded figures, retained so the audit trail is not lost:

```
EXIT_CODE: 1
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 2
```

Why the earlier figures were not carried forward: a CS0006 failure stops compilation before any
analyzer executes, so its `0 Warning(s)` records an absence of measurement rather than a clean
analyzer result. The plan's baseline-relative C# gate rule makes the Phase 7 analyzer gate compare
against this artifact, and a floor of "exit code no worse than 1, error count no more than 2" is
satisfiable by a build that compiles nothing. A gate that cannot fail verifies nothing, so the
baseline had to be re-measured on a tree that actually compiles.

Provisioning action that made the re-measurement possible, performed as environment bootstrap by the
run coordinator and not by this delivery:
`nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -DependencyVersion Ignore`,
exit 0. `packages/` is ignored by this repository. After the install,
`git status --porcelain --untracked-files=all` was empty, so zero tracked files were touched, no
project file was edited, and no entry in this delivery's declared write footprint was altered. The
fifteen skewed project files remain unmodified; repairing them is a separate item.

## Output Summary

EXIT_CODE: 0, MSBUILD_WARNING_COUNT: 0, MSBUILD_ERROR_COUNT: 0. `Build succeeded.` over all 19
projects with 36 compiler invocations, so this is a real analyzer measurement rather than a halted
compile. Under the plan's baseline-relative C# gate rule this is the floor the Phase 7 analyzer gate
is compared against, and because the floor is now zero exit code and zero errors, that gate can
fail. Supersedes the 2026-09-13T04-57 CS0006 measurement recorded above.
