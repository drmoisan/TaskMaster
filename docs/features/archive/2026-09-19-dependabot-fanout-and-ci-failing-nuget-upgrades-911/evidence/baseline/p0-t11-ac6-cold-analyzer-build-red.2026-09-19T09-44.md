# P0-T11 — AC6 Failing Direction on the Merge-Base Tree (expect-fail)

Timestamp: 2026-09-19T22-52

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"
```

EXIT_CODE: 1

ExpectedExitCode: 1

OUTLOOK-CLOSED: true

## CMD-OUTLOOK precondition

Measured immediately before the build, in the same shell invocation that ran it:

```
Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count
```

returned `0`. The invocation carried a guard that exits 99 without building when the count is
non-zero, so the build could not have run against a loaded add-in. No process was terminated; the
user closed Outlook before this task ran.

## Branch selection

`MEZIANTOU-898-STATE: unfixed`. The formal declaration is produced by P0-T19. This task read the
discriminator directly because it runs first:

```
git grep -l "Meziantou.Analyzer.3.0.203" -- "*.csproj" | wc -l
```

returned **15**, so the sibling branch `bug/meziantou-analyzer-hintpath-skew-898` has not merged and
the first acceptance branch of this task applies: a non-zero exit with at least one `CS0006` line
naming `Meziantou.Analyzer.3.0.203`.

## Matching diagnostic lines

Count of lines in `coverage/analyzers.msbuild.log` containing both `CS0006` and
`Meziantou.Analyzer.3.0.203`: **4**.

The four lines are two distinct diagnostics, each emitted twice by the file logger — once with the
`/m` node-id prefix on the live event and once in the error recapitulation at the end of the log.
Absolute worktree prefixes are replaced by `<repo-root>` per the repository's no-absolute-host-paths
rule; every token the acceptance condition reads is preserved unchanged.

```
19>CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [<repo-root>\UtilitiesCS\UtilitiesCS.csproj]
6>CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [<repo-root>\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [<repo-root>\UtilitiesCS\UtilitiesCS.csproj]
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [<repo-root>\VBFunctions\VBFunctions.csproj]
```

The two projects that reached `CoreCompile` before the solution build stopped are `VBFunctions` and
`UtilitiesCS`. Every project depending on either then failed to build for the missing upstream
output rather than for a second `CS0006`: `TaskVisualization.Test`, `TaskTree.Test`, `TaskMaster`,
`QuickFiler.Test`, `UtilitiesCS.Test` and `TaskMaster.Test` are all recorded `-- FAILED` in the same
log. MSBuild's own summary line reads `0 Warning(s)` and `2 Error(s)`.

## Observation — compiler invocation count

Lines in the log containing `/out:obj\Debug\`: **8**. This is recorded as an observation and is not
an acceptance condition of this task. It is well below the 18 that P1-T14 asserts, which is the
expected shape of a red run: the solution aborts once the two analyzer-bearing leaf projects fail,
so most projects never reach `CoreCompile` at all. The figure is recorded here so the P1-T14
non-vacuity guard has a measured red-state counterpart to be read against.

## Acceptance evaluation

- `EXIT_CODE:` is non-zero — measured **1**. PASS.
- The captured log carries at least one line containing both `CS0006` and
  `Meziantou.Analyzer.3.0.203` — measured **4** such lines, enumerated verbatim above. PASS.
- The artifact records the count of such lines as an integer greater than zero — **4**. PASS.
- `OUTLOOK-CLOSED: true` recorded, measured as `0` running processes. PASS.

The failing condition is reachable and was measured independently once before, in
`evidence/regression-testing/898-cold-restore-red-run.2026-09-19T11-40.md`, which recorded the same
`CS0006` diagnostic against `VBFunctions` after a cold `nuget restore`. This run reproduces it
solution-wide.

Output Summary: CMD-MSBUILD-ANALYZERS returned EXIT_CODE 1 against the expected 1, with Outlook
confirmed closed at 0 processes. The log carries 4 lines containing both `CS0006` and
`Meziantou.Analyzer.3.0.203`, covering two distinct projects, `VBFunctions` and `UtilitiesCS`.
MSBuild reported 0 warnings and 2 errors. The `MEZIANTOU-898-STATE: unfixed` branch applies, with
15 `*.csproj` files still naming `Meziantou.Analyzer.3.0.203`. The AC6 failing direction is
captured; the passing direction is verified at P1-T14.
