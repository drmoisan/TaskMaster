# [P0-T5] Manifest-pinned dotnet tool restore

Timestamp: 2026-09-07T06-50

Command: $env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path ; $env:PATH = "$env:DOTNET_ROOT;$env:PATH" ;
dotnet tool restore ; dotnet tool run csharpier --version

EXIT_CODE: 0

## Printed output

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

`dotnet tool run csharpier --version` printed:

```
1.2.6
```

Output Summary: `dotnet tool restore` exited 0 and restored the manifest-pinned CSharpier. The version invocation
also exited 0 and printed `1.2.6`, which contains the required substring 1.2.6, so the formatter used by every
later CSharpier task in this plan is the manifest-pinned version rather than a global install. Both commands ran
with `DOTNET_ROOT` and `PATH` re-bound to the repository-local SDK per R11.
