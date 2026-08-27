# Phase 6 — Nullable / Type-Check Gate (final pass)

Timestamp: 2026-08-27T14-18
Task: [P6-T4]
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

Started 2026-08-27T14:17:50Z, ended 2026-08-27T14:18:12Z. `Time Elapsed 00:00:21.88`.

## Output Summary

- **Errors: 0** — the acceptance condition.
- Warnings: 5.
- `CS86xx` diagnostics (nullable-flow): **0**.

### Non-vacuity proof

| Measurement | Value | Meaning |
| --- | --- | --- |
| `Skipping target "CoreCompile"` occurrences | **0** | no project skipped compilation |
| `CoreCompile:` target-execution headers | **54** | compilation ran 54 times |

`/t:Rebuild` is used for the same reason as at [P6-T3]: a warm `/t:Build` would return exit 0 having
skipped `CoreCompile` on every project, so the gate could not fail.

### Command shape

This is character-for-character the command in `.github/workflows/ci.yml` (step "Build with nullable
warnings treated as errors"), with `/t:Build` replaced by `/t:Rebuild` because a local working tree
is warm where a CI runner checkout is always cold. Two properties are deliberately absent and must
not be "restored":

- **No `/p:Nullable=enable`.** Nullable enforcement in this repository is per-file opt-in via the
  `#nullable enable` directive, and no project carries a `<Nullable>` element. Forcing the property
  solution-wide conscripts every file that has never adopted the pragma; it produced 195 errors in
  `UtilitiesCS.csproj` on 2026-08-10 against zero without it. Omitting it loses no enforcement over
  any file that has opted in.
- **No `/t:Build`.** See the non-vacuity note above.

The five warnings are the same pre-existing System.Reactive `packages.config` warning enumerated in
`evidence/qa-gates/msbuild-analyzers.2026-08-27T14-18.md`. With `/p:TreatWarningsAsErrors=true` in
force they did not promote to errors, because they are emitted by an MSBuild target rather than by
the compiler and carry no diagnostic code.

### Bearing on AC-10

AC-10 requires that the solution compile clean under toolchain step 3 with
`double elapsedSeconds` declared at `EfcHomeController.Metrics.cs:35` and `:57`. This run is that
step, and it is clean.
