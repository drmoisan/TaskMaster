# Baseline Toolchain Step 4 — Nullable / Type Check Build (Issue #449, [P0-T11])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
pwsh -NoProfile -Command 'Set-Location "<WORKTREE>";
  & "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
    TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
    /p:TreatWarningsAsErrors=true /v:n /nologo *> $log;
  "MSBUILD_EXIT=$LASTEXITCODE"'
```
EXIT_CODE: 0

Log captured to the session scratchpad: `.../scratchpad/449/p0t11-nullable.log`, 11,213 lines.

## Error count

```
5 Warning(s)
0 Error(s)
```

**Baseline error count: 0 (zero).**

The 5 warnings are the same pre-existing `System.Reactive` v7.0 `packages.config` advisory recorded
in `step3-analyzer-build.2026-08-22T09-16.md`. They are emitted by an imported `.targets` file rather
than by the compiler, which is why `/p:TreatWarningsAsErrors=true` does not promote them to errors
and the build still exits 0.

## `/p:Nullable=enable` was NOT supplied

The command line above contains no `/p:Nullable=enable`. This is deliberate and load-bearing:

- No project in this repository carries a `<Nullable>` element and there is no
  `Directory.Build.props`, so the property is a **solution-wide opt-in** that conscripts every file
  which has never adopted the `#nullable enable` pragma.
- Forcing it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero errors without it.
- `.github/workflows/ci.yml` omits it deliberately; the command above is character-for-character the
  CI step "Build with nullable warnings treated as errors".
- Nullable enforcement in this repository is per-file opt-in via `#nullable enable`, and
  `/p:TreatWarningsAsErrors=true` then promotes that file's `CS86xx` diagnostics to errors.

Command: `grep -c 'nullable enable' QuickFiler/Controllers/QfcExplorerController.cs`
EXIT_CODE: 1
Output: `0`

`QuickFiler/Controllers/QfcExplorerController.cs` carries **no** `#nullable enable` pragma, so it does
not participate in nullable flow analysis. This gate therefore imposes no new nullable obligation on
the file this change edits; a failure of this gate in Phase 3, 4, or 5 would indicate a genuine
compiler error introduced by a deletion, not a nullable-annotation debt.

## `/t:Rebuild` was used

Command: `grep -c 'Skipping target "CoreCompile"' p0t11-nullable.log`
EXIT_CODE: 1
Output: `0`

**Count of `Skipping target "CoreCompile"`: 0 (zero).** The same non-vacuity check as [P0-T10]
applies: `grep -c 'Skipping target'` returns **27**, so the message form is emitted at this verbosity
and a `CoreCompile` skip would have been visible. It was not. `/t:Build` was not used, because
MSBuild's up-to-date check does not invalidate on a command-line `/p:` change and a warm `/t:Build`
would return exit 0 having skipped `CoreCompile` on every project, making the gate unable to fail.

## Output Summary

Baseline nullable / type-check build PASSED with **EXIT_CODE 0 and 0 errors** (5 pre-existing
`System.Reactive` `packages.config` warnings, not compiler-emitted, therefore not promoted).
`/p:Nullable=enable` was **not** supplied — it is a solution-wide opt-in that CI omits deliberately
and that produced 195 errors in `UtilitiesCS.csproj` when forced. `/t:Rebuild` **was** used, and the
count of `Skipping target "CoreCompile"` in the captured log is **zero** against 27 other
`Skipping target` lines, so the gate is non-vacuous. `QfcExplorerController.cs` carries no
`#nullable enable` pragma, so this gate adds no nullable obligation to the edited file.
