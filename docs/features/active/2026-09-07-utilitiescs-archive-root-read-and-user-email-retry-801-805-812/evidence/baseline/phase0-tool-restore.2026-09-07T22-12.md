# Phase 0 — dotnet Local Tool Restore (P0-T5)

Timestamp: 2026-09-08T06-36

Command: `dotnet tool restore` (run with the current directory set to the worktree root, against `dotnet-tools.json`)

EXIT_CODE: 0

Output Summary: The restore reported `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` followed by `Restore was successful.` A follow-up read-only invocation of `dotnet tool run csharpier --version` printed `1.2.6` and exited 0, so the manifest-pinned version is the one that will run for every formatter gate in this plan.

CSharpier 1.2.6 requires a subcommand; the subcommands are `format`, `check`, `pipe-files`, and `server`, and the bare-path invocation form does not run. Every formatter invocation in this plan therefore goes through `dotnet tool run csharpier format` or `dotnet tool run csharpier check`.
