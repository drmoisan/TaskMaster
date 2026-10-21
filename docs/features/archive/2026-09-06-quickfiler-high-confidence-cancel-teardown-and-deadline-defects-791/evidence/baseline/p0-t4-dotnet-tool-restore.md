# [P0-T4] dotnet tool restore

Timestamp: 2026-09-06T14-25

Command: `dotnet tool restore` then `dotnet tool run csharpier --version`, both with
`DOTNET_ROOT` bound to the repository-local `.dotnet-sdk` directory and that directory prepended
to `PATH`.

EXIT_CODE: 0

Output Summary:

- `dotnet tool restore` printed `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`
  followed by `Restore was successful.`, exit code 0.
- `dotnet tool run csharpier --version` printed the single line `1.2.6`, exit code 0.

The printed version `1.2.6` is the manifest-pinned version named by `dotnet-tools.json` and by
CLAUDE.md, so the local formatter agrees with `.github/workflows/_format-check.yml`. The
repository-local SDK marker directory `.dotnet-sdk/sdk/8.0.205` exists, so `global.json` resolves
without a machine-wide SDK.

CSHARPIER-VERSION: 1.2.6
