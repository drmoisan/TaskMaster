# P0-T6 Manifest tool restore (CSharpier)

Timestamp: 2026-09-29T08-53
Command: pwsh -NoProfile -Command 'dotnet tool restore; "RESTORE-EXIT=" + $LASTEXITCODE; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "HELP-EXIT=" + $LASTEXITCODE'
EXIT_CODE: 0
Output Summary:
- "Tool 'csharpier' (version '1.2.6') was restored." / "Restore was successful."
- RESTORE-EXIT=0
- Tool list row: `csharpier       1.2.6        csharpier      <repo-root>\dotnet-tools.json`
- HELP-EXIT=0
