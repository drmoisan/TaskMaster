# P0-T10 — dotnet tool restore

Timestamp: 2026-08-31T18-49
Command: dotnet tool restore
EXIT_CODE: 0

Output Summary: The restore succeeded and reported `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`, followed by `Restore was successful.` The manifest-pinned CSharpier version is therefore 1.2.6, matching the pin CLAUDE.md section C#1 records and the version `.github/workflows/ci.yml` runs. Every format and check invocation in this plan goes through `dotnet tool run csharpier` so this pinned version is the one used.
