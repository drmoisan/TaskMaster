# P0-T13 — Analyzer baseline (baseline)

Timestamp: 2026-09-13T23-06

Command:

```
New-Item -ItemType Directory -Force -Path coverage\logs | Out-Null
$msb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\logs\p0-t13-analyzers.log;Verbosity=detailed"
```

EXIT_CODE: 0

Output Summary:

- Exit code: **0**
- Error count, read by the pattern `(^|[^0-9])[0-9]+ Error\(s\)`: `    0 Error(s)` — **0**
- Warning count, read from the corresponding warnings line: `    0 Warning(s)` — **0**
- `(Select-String -Path coverage\logs\p0-t13-analyzers.log -Pattern 'Task \x22Csc\x22').Count`: **36**

The compile-task count of 36 is greater than zero, which is the non-vacuity observation proving
compilation actually ran rather than being skipped by MSBuild incrementality.

The error count is read by that pattern rather than by searching for a bare zero, because the text
`0 Error(s)` is a substring of `10 Error(s)`.

No `/p:Nullable=enable` was added to this or any other command in this plan.

## Pre-existing worktree defect found and remedied before this baseline

The first attempt at this command returned exit code 1 with `2 Error(s)`. Both were the same
diagnostic, raised by `UtilitiesCS.csproj` and `VBFunctions.csproj`:

```
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found
```

Diagnosis: a NuGet analyzer bump updated `packages.config`, the `<Import>` element and the
`<Error Condition>` guard to `Meziantou.Analyzer.3.0.235` while leaving every `<Analyzer Include>`
item naming `3.0.203`. A probe over the 18 non-package project files in the tree checked all 162
`<Analyzer Include>` items and found 15 missing paths, all of them the same `3.0.203` Meziantou
DLL. Roslynator, AsyncFixer, MSTest.Analyzers, SonarAnalyzer and BannedApiAnalyzers all resolved,
so Meziantou was the only skewed package.

This condition is **pre-existing and not introduced by this delivery**, verified two ways:

- `git grep -c "Meziantou.Analyzer.3.0.203" origin/main -- UtilitiesCS/UtilitiesCS.csproj VBFunctions/VBFunctions.csproj` reports one occurrence in each file on `origin/main`.
- `git diff --name-only origin/main...HEAD -- "*.csproj"` prints nothing, so this branch has never
  touched any project file.

Remedy applied, chosen because it edits nothing tracked: the HintPath-named version was provisioned
into the gitignored packages tree with

```
nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -DependencyVersion Ignore
```

`git check-ignore -v packages/Meziantou.Analyzer.3.0.203` reports the path is ignored at
`.gitignore:191`, and
`git status --porcelain --untracked-files=all -- "*.csproj" "*.config" "*.props" "*.targets"`
printed zero lines after the install, so no tracked build file changed. The probe then reported
`MISSING_COUNT=0` over the same 162 items, and the mandated command above was re-run unmodified and
returned the figures recorded in the Output Summary. This is the same class of action as installing
the repo-local .NET SDK (P0-T8) and running the packages.config restore (P0-T9): a cold-worktree
provisioning step, not a source change.
