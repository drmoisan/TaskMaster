# Phase 0 — Manifest-pinned dotnet tool restore ([P0-T6])

Timestamp: 2026-09-01T21-55

Command: `dotnet tool restore`

EXIT_CODE: 0

Output, verbatim:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Acceptance reading — the pinned CSharpier version

Command: `dotnet tool run csharpier --version`

EXIT_CODE: 0

Output, verbatim:

```
1.2.6
```

The acceptance condition is that this output **begins with** `1.2.6`, the version `dotnet-tools.json` pins.
It does. Equality over the whole version string is not asserted, because CSharpier 1.x can print an
informational version carrying build metadata after the three-part number; on this run no such metadata
was printed.

Output Summary: `dotnet tool restore` exited 0 and restored csharpier at the manifest-pinned version
1.2.6. `dotnet tool run csharpier --version` prints `1.2.6`, which begins with the pinned version, so both
acceptance clauses of `[P0-T6]` hold. Every CSharpier invocation later in this plan goes through
`dotnet tool run` so that this manifest-pinned build is the one used.
