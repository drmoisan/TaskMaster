# Bootstrap: manifest-pinned dotnet tool restore (issue #839)

Timestamp: 2026-09-13T02-37
Command: dotnet tool restore
Command: dotnet tool run csharpier --version
EXIT_CODE: 0

Output Summary:
- dotnet tool restore reported: Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier. Restore was successful.
- The follow-on version probe printed 1.2.6, matching the version dotnet-tools.json pins at the worktree root, so the formatter invoked through dotnet tool run agrees with the version CI runs.
- Both invocations ran while this item held the shared machine build lock, which was released immediately after the version probe returned.
