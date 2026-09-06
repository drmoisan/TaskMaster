# Baseline — CSharpier Check (P0-T3)

Timestamp: 2026-09-05T19-20

Command:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

Run from the worktree root. The `DOTNET_ROOT` / `PATH` preamble is required because `global.json`
pins SDK 8.0.205 and the host SDK cannot satisfy it.

EXIT_CODE: 0

Output Summary:

The printed count line, verbatim:

```text
Checked 1580 files in 4053ms.
```

The recorded count is `Checked 1580 files`, which matches the expected baseline of 1580 exactly.
No `BASELINE_CHECKED_FILES:` escape line is required, so P7-T2 derives its expected value from the
tabled 1580 plus two.
