# Toolchain Bootstrap — Remediation Cycle 2 Baseline

- Task: `[P0-T1]`
- Timestamp: 2026-08-04T23-21
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Repository root: `c:\Users\DanMoisan\repos\TaskMaster`

## Precondition check

`Install-RepoDotNetSdk.ps1` was **not** run, because its guard condition did not hold:
`.dotnet-sdk/` is already present in this checkout.

```
Command: ls -d .dotnet-sdk
Output:  .dotnet-sdk/
EXIT_CODE: 0
```

`dotnet tool install --global dotnet-coverage` was **not** run, because its guard condition did not
hold: `dotnet-coverage --version` succeeds (see below).

## Commands executed

### 1. Local tool manifest restore

```
Command: dotnet tool restore
EXIT_CODE: 0
Output Summary: Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier.
                Restore was successful.
```

### 2. csharpier version probe

```
Command: dotnet tool run csharpier --version
EXIT_CODE: 0
Output Summary: 1.2.6
```

### 3. dotnet-coverage version probe

```
Command: dotnet-coverage --version
EXIT_CODE: 0
Output Summary: 18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3
```

## Resolved version strings

| Tool | Resolved version | Scope |
|---|---|---|
| csharpier | `1.2.6` | repo-local (repo-root `dotnet-tools.json` manifest; verified present, `.config/dotnet-tools.json` does not exist) |
| dotnet-coverage | `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3` | global (`~/.dotnet/tools`) |

## Output Summary

Bootstrap acceptance satisfied. `dotnet tool run csharpier --version` returns `EXIT_CODE: 0`
(`1.2.6`) and `dotnet-coverage --version` returns `EXIT_CODE: 0`
(`18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`). The environment precondition documented in this
plan's § Environment Precondition is therefore removed: `[P2-T1]`, `[P2-T2]` (csharpier) and
`[P0-T11]`, `[P2-T7]` (coverage) can run. No installer script was required on this host.
