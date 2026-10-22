# P0-T9 — C# formatter baseline (CMD-CSHARPIER-CHECK)

Timestamp: 2026-09-30T09-19
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool run csharpier check .; "CSHARPIER_EXIT=$LASTEXITCODE"' followed by the CMD-CSHARPIER-CHECK XML-candidate companion command
EXIT_CODE: 0
Output Summary:
- "Checked 1625 files in 7451ms."
- CSHARPIER_EXIT=0
- N = 1625 (greater than 900)

CSHARPIER-FINDINGS: none

XML-CANDIDATES: 3
```
artifacts\pester\pester-junit.xml
artifacts\pester\powershell-coverage.koverage.xml
artifacts\pester\powershell-coverage.xml
```
