# P11-T6 — Nullable / type-check build, final QC (loop iteration 1)

Timestamp: 2026-08-28T02-20
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0

Loop iteration: **1**.

## The exact command line as executed

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

It was invoked from the worktree root through `pwsh -NoProfile -Command`, with the spaced platform
spelling `"/p:Platform=Any CPU"` — which is correct for `TaskMaster.sln` and must **not** be replaced
by the unspaced `AnyCPU` form here; the unspaced form is required only for a single-`.csproj`
invocation, where the spaced spelling fails in
`Microsoft.Common.CurrentVersion.targets(843,5)` with an unset `BaseOutputPath`/`OutputPath` and
compiles nothing.

The recorded command line contains `/t:Rebuild`. It contains **neither** `/p:Nullable=enable` **nor**
`/t:Build`:

- **No `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and there
  is no `Directory.Build.props`, so the property is a solution-wide opt-in that would conscript every
  file that has never adopted the `#nullable enable` pragma. `.github/workflows/ci.yml` omits it
  deliberately, and omitting it loses no enforcement over any file that has opted in. The build
  output contains **0** occurrences of the string `Nullable=enable`.
- **No `/t:Build`.** MSBuild's incremental up-to-date check does not invalidate on a command-line
  `/p:` change, so a warm `/t:Build` returns exit `0` with `CoreCompile` skipped on every project and
  the gate cannot fail. `/t:Rebuild` forces the compile.

## What the build reported

- `Build succeeded.` with `5 Warning(s)` and `0 Error(s)`. Time elapsed 00:00:13.34.
- **`CS86xx` diagnostics: 0.** No nullable-flow diagnostic was raised anywhere in the solution, so
  `/p:TreatWarningsAsErrors=true` had nothing to promote to an error.
- The 5 warnings are the same non-Roslyn `System.Reactive` `packages.config` advisory recorded by
  P11-T4 and by the P0-T12 baseline, one per project across `UtilitiesCS`, `UtilitiesCS.Test`,
  `ToDoModel`, `QuickFiler` and `TaskMaster`. They are not errors under
  `/p:TreatWarningsAsErrors=true` because they are raised by a `.targets` file through the MSBuild
  warning channel rather than by the compiler, and the build succeeded with them present.

`BaselineNullableWarningCount:` from `evidence/baseline/phase0-nullable-build.2026-08-28T00-12.md`
is `5` with `0` CS86xx. The final figures are identical: `5` warnings, `0` CS86xx, `0` errors.

## This build is also the operative `net48` guard

The projects in this solution target `net48`/`net481`, which has no `IsExternalInit` type. An `init`
accessor, a `record`, or a `record struct` therefore fails to compile against `net48` with `CS0518`,
and this `/t:Rebuild` is where that failure would surface. The build succeeded with `0 Error(s)`, and
**no such construct is introduced by this feature** — this feature's production edits are deletions,
a renamed interface member, one new event handler and one guard, none of which introduces an `init`
accessor, a `record` or a `record struct`. The guard is therefore both operative and satisfied.

## Acceptance

- **`EXIT_CODE: 0`.** Observed `0`.
- **The recorded command line contains `/t:Rebuild`.** It does.
- **It contains neither `/p:Nullable=enable` nor `/t:Build`.** It contains neither; the only
  occurrence of the substring `Build` in a target switch is inside `/t:Rebuild`.
- **The summary states the `net48` guard.** It does, in the section above.

## Loop consequence

The stage passed and rewrote no source file; the build writes only into gitignored `bin/` and `obj/`.
No restart is triggered; the loop proceeds to P11-T7.

Output Summary: The nullable / type-check gate **passes** at loop iteration 1.
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
/p:TreatWarningsAsErrors=true` exited `0` with `Build succeeded.`, `5 Warning(s)`, `0 Error(s)` in
13.34 seconds, and **0** `CS86xx` diagnostics — identical to the P0-T12 baseline of 5 warnings and 0
CS86xx. The command carries `/t:Rebuild` and carries neither `/p:Nullable=enable` nor `/t:Build`, and
the build output contains zero occurrences of `Nullable=enable`. This build is also the operative
`net48` guard: `init`, `record` and `record struct` fail against `net48` for want of
`IsExternalInit`, none is introduced by this feature, and the build compiled clean.
