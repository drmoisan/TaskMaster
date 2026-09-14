# Baseline MSBuild Analyzer State (issue #742, [P0-T5])

Timestamp: 2026-09-14T02-00

Command: `pwsh -NoProfile -Command '$out = msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1; $exit = $LASTEXITCODE; $out | Select-String -Pattern "^\s*\d+ Error\(s\)\s*$" | ForEach-Object { $_.Line.Trim() }; Write-Output "EXITCODE=$exit"'`

EXIT_CODE: 0

Output Summary: transcribed summary line `0 Error(s)`, exit code `0`. The same command additionally
printed `0 Warning(s)`, recorded here for completeness rather than as a baseline floor.

Acceptance: none stated by the task; this is a baseline capture only.

## Pre-existing cold-worktree blocker encountered and resolved before this measurement

The first run of this exact command in this worktree printed `2 Error(s)` and exited `1`. The two
errors were:

```
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [...\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [...\UtilitiesCS\UtilitiesCS.csproj]
```

Diagnosis: an `<Analyzer Include>` HintPath skew that predates this branch. Fifteen of sixteen
first-party project files name `Meziantou.Analyzer.3.0.203` in their `<Analyzer Include>` item while
`packages.config`, the `<Import>` and the `<Error Condition>` guard in the same files all name
`3.0.235`, which is the version `nuget restore` installs. `TaskMaster.csproj` alone already names
`3.0.235`. Only two errors surface because the two failing projects are upstream of every other
project in the graph, so the rest are skipped rather than compiled.

Verification that this is pre-existing and not introduced by this change:

- `git grep -c -F 'Meziantou.Analyzer.3.0.203' origin/main -- UtilitiesCS/UtilitiesCS.csproj VBFunctions/VBFunctions.csproj QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj` printed `1` for each of the four paths, so `origin/main` carries the identical skew.
- `git diff --name-only origin/main...HEAD -- "*.csproj" "*.config" "*.props" "*.targets"` printed no line, so this branch has modified no project or package file.

Remedy applied (provisioning only, no tracked file edited):

`pwsh -NoProfile -Command 'nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -DependencyVersion Ignore'` — exit code `0`.

This provisions the HintPath-named analyzer version into the repository's `packages/` tree, which
`git check-ignore -v` confirms is ignored at `.gitignore:191` (`**/[Pp]ackages/*`). After the
install, `git status --porcelain --untracked-files=all -- "*.csproj" "*.config" "*.props" "*.targets" "packages"`
printed no line, confirming zero tracked files changed. The mandated msbuild command was then re-run
unmodified and produced the `0 Error(s)` result recorded above.

This is the same class of cold-worktree provisioning as the repo-local .NET SDK install recorded in
`toolchain-bootstrap.2026-09-12T16-09.md`. It edits no source file, no project file, and nothing
belonging to any sibling work item, and it is outside this change's Write Set because it changes no
tracked file at all.
