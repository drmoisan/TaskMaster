# P0-T5 — dotnet local tool restore

Timestamp: 2026-09-01T19-42
Command: `dotnet tool restore` (run from the worktree root)
EXIT_CODE: 0

Output Summary:

    Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

    Restore was successful.

The manifest-pinned CSharpier 1.2.6 is now available to `dotnet tool run`, which is the only invocation form the C# policy permits for the formatter. No globally installed CSharpier is used at any point in this delivery run.

Capture-time sanitisation gate: a case-insensitive fixed-string sweep of this artifact for the drive-qualified user-profile root and for the drive-qualified Program Files root, in each of the two separator spellings, returns zero. The command's success-case output named no absolute path, so no rewrite was required; the gate is nonetheless recorded because P3-T15 commits this artifact in Phase 3 and P4-T28 in Phase 4 is the only later sweep that reaches it.
