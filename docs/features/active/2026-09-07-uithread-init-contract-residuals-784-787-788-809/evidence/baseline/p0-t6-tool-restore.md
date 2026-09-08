# [P0-T6] CSharpier manifest tool restore

Timestamp: 2026-09-08T00-21

Command: `dotnet tool restore` (run from the repository root after the SDK preamble)

EXIT_CODE: 0

Output Summary:

`dotnet tool restore` printed:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

The whole output of `dotnet tool run csharpier --version`:

```
1.2.6
```

That output begins with `1.2.6`, which is the version `dotnet-tools.json` pins at repository root. The assertion is on the leading version token rather than on the whole line, because a dotnet tool may append a build-metadata suffix to its informational version; the observed line is recorded in full above and carries no suffix.
