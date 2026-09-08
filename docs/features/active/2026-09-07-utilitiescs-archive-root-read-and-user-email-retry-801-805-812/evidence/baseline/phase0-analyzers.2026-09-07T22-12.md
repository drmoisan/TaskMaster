# Phase 0 — Analyzer Include Path Resolution (P0-T4)

Timestamp: 2026-09-08T06-35

Command: a single `pwsh -NoProfile -File` run, with the current directory set to the worktree root, that enumerates every `*.csproj` outside any `packages` directory, extracts every `<Analyzer Include="...">` value with `Select-String -Pattern '<Analyzer Include="([^"]+)"' -AllMatches`, joins each value to its own declaring project's directory with `Join-Path`, and tests the result with `Test-Path -LiteralPath`.

EXIT_CODE: 0

Output Summary:

- Non-`packages` `*.csproj` files examined: 18
- `<Analyzer Include>` items examined: 162
- Items that failed `Test-Path`: 0

Each `Include` value is resolved against its own declaring project's directory rather than against the repository root, as the task requires. No back-fill was needed, so no `nuget install` pass was run and only this single pass is recorded. A missing analyzer assembly would surface as `error CS0006` and fail the compile rather than warn, so a failed count of 0 is a precondition for the P0-T9 and P0-T10 baseline builds.
