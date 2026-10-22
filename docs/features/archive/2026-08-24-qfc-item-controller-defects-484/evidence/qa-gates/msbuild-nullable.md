# Final QC stage 3 — type-check / nullable gate

Timestamp: 2026-08-26T13-43
Task: [P7-T4]

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild, run from the worktree root; the CLAUDE.md
nullable command verbatim with `/t:Rebuild` per decision D1, and without `/p:Nullable=enable` per
decision D3):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

EXIT_CODE: 0

## Result

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:25.04
```

| Metric | Value |
|---|---|
| **Errors** | **0** |
| Warnings | 5 |

The 5 warnings are the same pre-existing `System.Reactive.PackagesConfigCheck.targets(31,5)`
`packages.config` notices as in `[P7-T3]`. They originate in an MSBuild `.targets` file rather than a
compiled source file, so `/p:TreatWarningsAsErrors=true` does not promote them to errors.

`/p:Nullable=enable` is deliberately absent: no project in this repository carries a `<Nullable>`
element and there is no `Directory.Build.props`, so the property would conscript every file that has
never adopted the `#nullable enable` pragma. `.github/workflows/ci.yml` omits it for the same reason.
Nullable enforcement here is per-file opt-in, and no file this feature edited carries the pragma, so
this feature adds no `CS86xx` surface.

## Comparison against the `[P0-T11]` baseline

| | Errors | Warnings | Exit code |
|---|---|---|---|
| `[P0-T11]` baseline | 0 | 5 | 0 |
| `[P7-T4]` post-change | 0 | 5 | 0 |

Unchanged.

Output Summary: EXIT_CODE 0, 0 errors, 5 warnings — identical to the `[P0-T11]` baseline. The
type-check stage passes and the toolchain loop does not restart.
