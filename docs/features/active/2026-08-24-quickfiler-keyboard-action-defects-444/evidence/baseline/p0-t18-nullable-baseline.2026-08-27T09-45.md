# [P0-T18] Baseline nullable / type-check build

Timestamp: 2026-08-27T09-45
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

`/p:Nullable=enable` is **not** present in the command above. That omission is deliberate and matches
`.github/workflows/ci.yml` character for character: no project in this repository carries a
`<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in
that conscripts every file which has never adopted the `#nullable enable` pragma.

`/t:Rebuild` is used rather than `/t:Build` because MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change; a warm `/t:Build` would return exit 0 with `CoreCompile` skipped on every
project and the gate could not fail.

## Summary counts (verbatim)

```
5 Warning(s)
0 Error(s)
```

## Baseline figure

```
BaselineNullableErrors = 0
```

## Proof the gate actually compiled

| Evidence | Value |
| --- | --- |
| Occurrences of `Skipping target "CoreCompile"` in the log | **0** |
| Log lines captured | 11793 |
| Lines matching `: error ` | 0 |

## Warning characterisation

The 5 warnings are the same pre-existing `System.Reactive 7.0.0` `packages.config` diagnostic recorded
under `[P0-T17]`. They are emitted by a NuGet `.targets` file rather than by the compiler, which is why
`/p:TreatWarningsAsErrors=true` did not promote them to errors and the build still returned 0.

Output Summary: exit code 0; `BaselineNullableErrors = 0`; zero `Skipping target "CoreCompile"`
occurrences, so the type-check genuinely ran; `Nullable=enable` absent from the command.
