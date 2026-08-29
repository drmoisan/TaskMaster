# P0-T9 — Baseline Nullable / Type-Check Build (Issue #680)

Timestamp: 2026-08-28T15-06

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
(run with `/v:m`. This is character-for-character the CI nullable step; `/p:Nullable=enable` is
deliberately absent, and `/t:Rebuild` is required so compiler and nullable-flow diagnostics run.)

EXIT_CODE: 0

Output Summary:

- Build succeeded. 0 error lines in the minimal-verbosity log.
- 5 warning lines, all the same pre-existing non-code `System.Reactive.PackagesConfigCheck.targets`
  `packages.config` advisory recorded in the P0-T8 artifact. Under `/p:TreatWarningsAsErrors=true`
  these are not promoted to errors (they carry no warning code and the build exits 0).
- No `CS86xx` nullable-flow diagnostic was emitted, so every file that has opted into
  `#nullable enable` is clean at baseline.

Acceptance: satisfied — `EXIT_CODE: 0`.
