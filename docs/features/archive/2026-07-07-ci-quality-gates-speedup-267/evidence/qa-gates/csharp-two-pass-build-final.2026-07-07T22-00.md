# Final Two-Pass Retained Build Verification (Issue #267, AC4 local-verification half)

Both passes were run sequentially (pass 1 completed before pass 2 started) against the current working tree, from the current build state (not a from-scratch clean rebuild). Neither pass was consolidated into a single invocation carrying all four properties.

Execution note: invoked in the git-bash shell using dash-style switches (`-t:Build`, `-m`, `-p:...`) rather than leading-slash switches, because git-bash mangles leading-slash MSBuild switches into path arguments (`MSB1008: Only one project can be specified`). Properties and flags are otherwise identical to the plan's stated commands, and identical to what the workflow's `pwsh` shell (`&` call operator with `/`-prefixed switches) will execute on `windows-latest`.

## Pass 1 — Build with analyzers and code style enforcement

- Timestamp: 2026-07-08T01:26:47Z
- Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Output Summary: Build succeeded in 14.75s. 72 Warning(s), 0 Error(s). This pass performed a genuine full-solution compile (the working tree's Debug `bin`/`obj` had been cleaned by a prior execution attempt), so it recompiled every project rather than short-circuiting via MSBuild's incremental up-to-date check. Warnings are predominantly `CS8632` (nullable annotation context) in test projects and `CS0067` (unused event) diagnostics, consistent in kind with the P0-T5 baseline.

## Pass 2 — Build with nullable warnings treated as errors

- Timestamp: 2026-07-08T01:27:16Z (run immediately after pass 1 completed)
- Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 0
- Output Summary: Build succeeded in 1.11s. 0 Warning(s), 0 Error(s). 68 "Skipping target" lines were observed, indicating MSBuild's incremental up-to-date check short-circuited recompilation for all/most projects because pass 1 (immediately preceding, against the same working tree) had just produced up-to-date `obj`/`bin` outputs. This reproduces the same incremental-skip characteristic already documented in the P0-T6 baseline (`csharp-nullable-baseline.2026-07-07T20-45.md`, also 68 "Skipping target" lines, 0/0, ~1.28s).

## Sequencing confirmation

- Pass 1 timestamp (2026-07-08T01:26:47Z) precedes pass 2 timestamp (2026-07-08T01:27:16Z); pass 1's `msbuild.exe` process exited (EXIT_CODE 0) before pass 2 was invoked. Sequential execution confirmed, reproducing the current/target CI step order (pass 1 step, then pass 2 step, each with its own `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard).
- No single invocation carrying all four properties (`EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `Nullable`, `TreatWarningsAsErrors`) was executed; each command above carries only its own two properties, matching the two retained `.github/workflows/ci.yml` steps verbatim (plus `/m`).
- Neither command's enforced properties were weakened to force a pass; both passes exited 0 without modification.

Satisfies the local-verification half of AC4.
