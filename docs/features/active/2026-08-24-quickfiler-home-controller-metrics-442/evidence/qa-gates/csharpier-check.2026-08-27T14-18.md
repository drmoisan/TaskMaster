# Phase 6 — CSharpier Check, Repository-Wide (final pass)

Timestamp: 2026-08-27T14-18
Task: [P6-T2]
Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

Started 2026-08-27T14:17:23Z, ended 2026-08-27T14:17:28Z.

## Output Summary

`Checked 1540 files in 5273ms.` Exit code 0, no unformatted file reported. The acceptance condition
(`EXIT_CODE: 0`) holds.

The check is read-only and repository-wide, so it also confirms that no file outside this feature's
owned surface is left in an unformatted state by the scoped mutating pass at [P6-T1].

CSharpier is invoked through `dotnet tool run` so the version pinned by `dotnet-tools.json` (1.2.6)
is used, matching `.github/workflows/ci.yml`. A globally installed CSharpier of a different version
would produce diffs that disagree with CI.
