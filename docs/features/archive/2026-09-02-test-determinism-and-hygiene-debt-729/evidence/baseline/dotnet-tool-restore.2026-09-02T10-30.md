# dotnet tool restore (P0-T4)

Timestamp: 2026-09-03T01-08

Command: `dotnet tool restore`

EXIT_CODE: 0

Output Summary:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

Manifest-pinned formatter version confirmed by `dotnet tool run csharpier --version`:

```
1.2.6
```

The manifest-pinned CSharpier version is 1.2.6, which matches the version
`.github/workflows/ci.yml` runs after `dotnet tool restore`.
