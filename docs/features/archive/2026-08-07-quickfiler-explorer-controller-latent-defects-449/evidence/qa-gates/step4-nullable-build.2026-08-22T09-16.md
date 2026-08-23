# Final QC Step 4 — Nullable / Type-Check Build (Issue #449, [P7-T5])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
  /p:TreatWarningsAsErrors=true /v:n /nologo
```
EXIT_CODE: 0

Log: `.../scratchpad/449/p7t5-nullable.log`, 11,624 lines.

## Result

```
5 Warning(s)
0 Error(s)
```

**EXIT_CODE: 0**, zero errors, with the same 5 pre-existing `System.Reactive` `packages.config`
warnings as baseline. Those are emitted by an imported `.targets` file rather than by the compiler,
which is why `/p:TreatWarningsAsErrors=true` does not promote them to errors.

## `/p:Nullable=enable` was NOT supplied

The command line contains no `/p:Nullable=enable`. This is deliberate and load-bearing:

- No project in this repository carries a `<Nullable>` element and there is no
  `Directory.Build.props`, so the property is a **solution-wide opt-in** that would conscript every
  file which has never adopted the `#nullable enable` pragma.
- Forcing it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero errors without it.
- `.github/workflows/ci.yml` omits it deliberately; the command above is character-for-character the
  CI step "Build with nullable warnings treated as errors".
- Nullable enforcement here is per-file opt-in, and `/p:TreatWarningsAsErrors=true` then promotes the
  opted-in file's `CS86xx` diagnostics to errors. Removing the flag loses no enforcement over any file
  that has opted in.

`QuickFiler/Controllers/QfcExplorerController.cs` carries no `#nullable enable` pragma, so this gate
imposes no nullable obligation on the edited file. Its value is as an independent compiler pass over
the whole change set.

## `/t:Rebuild` was used

Command: `grep -c 'Skipping target "CoreCompile"' p7t5-nullable.log`
EXIT_CODE: 1
Output: `0`

**Count of `Skipping target "CoreCompile"`: zero.** `/t:Build` was not used, because MSBuild's
up-to-date check does not invalidate on a command-line `/p:` change and a warm `/t:Build` would return
exit 0 having skipped `CoreCompile` on every project, making the gate unable to fail.

## Output Summary

Final QC nullable / type-check build PASSED: **EXIT_CODE 0, 0 errors**, 5 pre-existing non-compiler
warnings unchanged from baseline. **`/p:Nullable=enable` was not supplied** — it is a solution-wide
opt-in that CI omits deliberately and that produced 195 errors in `UtilitiesCS.csproj` when forced —
and **`/t:Rebuild` was used**, with a zero count of `Skipping target "CoreCompile"` so the gate is
non-vacuous.
