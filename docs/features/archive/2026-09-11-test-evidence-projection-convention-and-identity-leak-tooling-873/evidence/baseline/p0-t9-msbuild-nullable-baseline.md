# P0-T9 — C# Nullable Baseline

Timestamp: 2026-09-13T05-16
Task: [P0-T9]

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
MSBuild resolution: vswhere under the thirty-two-bit Program Files Visual Studio Installer
directory, `-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find
"MSBuild/**/Bin/MSBuild.exe"`, first result. The repository build wrapper was not used.
No solution-wide nullable opt-in property was added: `CLAUDE.md` records that `/p:Nullable=enable`
conscripts every file that never adopted the pragma, and the continuous-integration command omits it
deliberately.
EXIT_CODE: 0
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 0

Build lock: acquired for item 873 before the command and released immediately after it returned.
Outlook was not running and no test host was running; no process was killed.

## Verbatim MSBuild summary lines

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Evidence that compilation actually ran

- `Build succeeded.` is present.
- 19 lines match `Project .+\.csproj.+ on node`, that is every project in the solution was built.
- 36 lines name `csc.exe`, so the C# compiler was invoked and every file carrying a
  `#nullable enable` pragma had its null-flow analysis executed.

The zero warning count is therefore a measured zero. Because `/p:TreatWarningsAsErrors=true` would
have promoted any `CS86xx` diagnostic in a pragma-carrying file to an error, a zero error count over
a completed compile is the operative nullable result.

## SUPERSEDES: an earlier halted-compile measurement

These figures supersede an earlier measurement of this same command recorded at 2026-09-13T04-58,
which read `EXIT_CODE: 1`, `MSBUILD_WARNING_COUNT: 0`, `MSBUILD_ERROR_COUNT: 2`. That earlier run
failed with two occurrences of `error CS0006` for the metadata file
`..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`,
reported against `VBFunctions/VBFunctions.csproj` and `UtilitiesCS/UtilitiesCS.csproj`. The cause was
the same pre-existing analyzer version skew diagnosed in the P0-T8 artifact: fifteen tracked project
files name `3.0.203` in an unconditional `<Analyzer Include>` item while `packages.config`, the props
`Import` and the existence `Error` condition all name `3.0.235`.

Superseded figures, retained so the audit trail is not lost:

```
EXIT_CODE: 1
MSBUILD_WARNING_COUNT: 0
MSBUILD_ERROR_COUNT: 2
```

Why the earlier figures were not carried forward: a CS0006 failure stops compilation before any
file's nullable flow analysis runs, so its `0 Warning(s)` records an absence of measurement rather
than a clean nullable result. Under the plan's baseline-relative C# gate rule the Phase 7 nullable
gate compares against this artifact, and a floor of "exit code no worse than 1, error count no more
than 2" is satisfiable by a build that compiles nothing, which makes the gate unfailable.

Provisioning action that made the re-measurement possible, performed as environment bootstrap by the
run coordinator and not by this delivery:
`nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -DependencyVersion Ignore`,
exit 0. `packages/` is ignored by this repository. After the install,
`git status --porcelain --untracked-files=all` was empty, so zero tracked files were touched and no
project file was edited. The fifteen skewed project files remain unmodified; repairing them is a
separate item.

## Output Summary

EXIT_CODE: 0, MSBUILD_WARNING_COUNT: 0, MSBUILD_ERROR_COUNT: 0. `Build succeeded.` over all 19
projects with 36 compiler invocations, so this is a real nullable measurement rather than a halted
compile. Under the plan's baseline-relative C# gate rule this is the floor the Phase 7 nullable gate
is compared against, and because the floor is now zero exit code and zero errors, that gate can fail.
Supersedes the 2026-09-13T04-58 CS0006 measurement recorded above.
