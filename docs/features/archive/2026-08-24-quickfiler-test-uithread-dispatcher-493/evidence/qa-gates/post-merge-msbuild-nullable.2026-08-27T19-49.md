# Post-Merge Toolchain Step 3 — Nullable / TreatWarningsAsErrors

Timestamp: 2026-08-27T19-49
Task: Resume verification — mandatory toolchain re-run after merging the moved epic integration base
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: Exit 0 with "5 Warning(s) / 0 Error(s)" — the same pre-existing packages.config
notices as step 2, unchanged by `TreatWarningsAsErrors`. Zero `error CS` and zero `warning CS`
lines in the log, so no file that has opted into `#nullable enable` produced a promoted CS86xx
diagnostic.

## Command fidelity

The command is character-for-character the one in `.github/workflows/ci.yml` ("Build with nullable
warnings treated as errors"). Two properties are deliberately preserved:

- `/p:Nullable=enable` is NOT added. Nullable enforcement in this repository is per-file opt-in via
  the `#nullable enable` pragma; forcing the property solution-wide conscripts files that never
  adopted it and is not what CI runs.
- `/t:Build` is NOT used, because MSBuild's up-to-date check does not invalidate on a command-line
  `/p:` change, so a warm `/t:Build` would skip `CoreCompile` and the gate could not fail.

## Non-vacuity proof

| Assertion | Measured |
| --- | --- |
| `Skipping target "CoreCompile"` occurrences | 0 |
| `csc.exe` invocations | 36 |
| `error CS` occurrences | 0 |
| `warning CS` occurrences | 0 |
