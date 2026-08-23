# Baseline Toolchain Step 3 — Analyzer Build (Issue #449, [P0-T10])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
pwsh -NoProfile -Command 'Set-Location "<WORKTREE>";
  & "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
    TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
    /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:n /nologo *> $log;
  "MSBUILD_EXIT=$LASTEXITCODE"'
```
EXIT_CODE: 0

Log captured to the session scratchpad (outside the repository, so no helper artifact is retained
under `evidence/`): `.../scratchpad/449/p0t10-analyzer.log`, 4,800 lines.

MSBuild resolved via `vswhere -latest -prerelease -requires Microsoft.Component.MSBuild` to
`C:\Program Files\Microsoft Visual Studio\18\Community` — the VS **18** full-framework MSBuild, which
is required because the .NET SDK MSBuild fails on this repository's binary `.resx` resources
(MSB3822).

## Warning and error counts

```
5 Warning(s)
0 Error(s)
```

All 5 warnings are the same pre-existing, non-actionable diagnostic — `System.Reactive` v7.0
complaining that the consuming projects use `packages.config` rather than `PackageReference`:

> `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
> The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
> later. Please migrate to PackageReference.`

It is emitted once per consuming project (including `TaskMaster.csproj` and
`UtilitiesCS.Test.csproj`). It is unrelated to this change, is not raised by any of the five wired
analyzers, and is the accepted baseline warning level. **Baseline warning count to compare the final
QC build against: 5.** Zero analyzer diagnostics and zero errors.

## `/t:Rebuild` verification — the `Skipping target "CoreCompile"` gate is ZERO and NON-VACUOUS

Command: `grep -c 'Skipping target "CoreCompile"' p0t10-analyzer.log`
EXIT_CODE: 1 (grep reports 1 for a zero count)
Output: `0`

**Count of occurrences of the string `Skipping target "CoreCompile"` in the captured log: 0 (zero).**
The [P0-T10] acceptance condition is satisfied.

That zero is proven to be a real observation rather than an artifact of the chosen `/v:n` verbosity.
A count of zero would be worthless if the log format never emitted such a line at all, so the log was
additionally searched for the message PREFIX:

Command: `grep -c 'Skipping target' p0t10-analyzer.log`
EXIT_CODE: 0
Output: `9`

Command: `grep -n 'Skipping target' p0t10-analyzer.log`
EXIT_CODE: 0
Output (all nine, verbatim):
```
132:  Skipping target "CopyMSTestV2Resources" because it has no outputs.
711:  Skipping target "CopyMSTestV2Resources" because it has no outputs.
1258: Skipping target "CopyMSTestV2Resources" because it has no outputs.
1566: Skipping target "CopyMSTestV2Resources" because it has no outputs.
1983: Skipping target "CopyMSTestV2Resources" because it has no outputs.
2316: Skipping target "CopyMSTestV2Resources" because it has no outputs.
3412: Skipping target "CopyMSTestV2Resources" because it has no outputs.
3595: Skipping target "CopyMSTestV2Resources" because it has no outputs.
3599: Skipping target "CopyMSTestV2Resources" because it has no outputs.
```

`Skipping target "..."` lines ARE emitted at `/v:n`, nine of them, and every one names
`CopyMSTestV2Resources` (skipped because it has no outputs, which is unrelated to incrementality).
Not one names `CoreCompile`. The gate could have fired and did not, so the zero is discriminating.

Corroborating evidence that compilation genuinely occurred: `grep -c 'Csc\|csc.exe'` returns **36**
and `grep -c 'CoreCompile'` returns **44**. Per repository convention a `csc.exe` count is not used
as the gate — it reads zero even on real compiles under some log shapes — but a non-zero count here is
consistent with a real full rebuild. Elapsed time 00:00:24.83.

`/t:Build` was NOT used. MSBuild's up-to-date check does not invalidate on a command-line `/p:`
change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no
analyzers — the gate cannot fail. `/t:Rebuild` is what makes this a real gate.

## Output Summary

Baseline analyzer build PASSED. **EXIT_CODE 0, 5 warnings, 0 errors.** All 5 warnings are the
pre-existing `System.Reactive` v7.0 `packages.config` advisory, one per consuming project; zero
analyzer diagnostics. The count of `Skipping target "CoreCompile"` in the captured log is **zero**,
and that zero is non-vacuous because the same log carries 9 other `Skipping target "..."` lines (all
`CopyMSTestV2Resources`), proving the message form is visible at this verbosity. 36 `csc` invocations
confirm real compilation. Analyzers ran. The pre-existing `Meziantou.Analyzer` /
`Roslynator.Analyzers` version skew between `packages.config` and the `<Analyzer Include>` HintPaths
produced no `CS0006`, confirming the [P0-T8] finding that both version families are present on disk.
