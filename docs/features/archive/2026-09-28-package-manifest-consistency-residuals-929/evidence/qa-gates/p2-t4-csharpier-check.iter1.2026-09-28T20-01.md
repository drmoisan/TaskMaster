# P2-T4 — C# QC step 1, CSharpier check (iteration 1)

Timestamp: 2026-09-30T10-47
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool run csharpier check .; "CSHARPIER_EXIT=$LASTEXITCODE"' followed by the CMD-CSHARPIER-CHECK XML-candidate companion command
EXIT_CODE: 0
Output Summary:
- "Checked 1625 files in 8208ms."
- CSHARPIER_EXIT=0 (equal to the P0-T9 exit code 0)
- CSHARPIER-FINDINGS: none (equal to the P0-T9 list)
- XML-CANDIDATES: 6
```
artifacts\pester\pester-junit.xml
artifacts\pester\powershell-coverage.koverage.xml
artifacts\pester\powershell-coverage.xml
coverage\ci-branch-pester-36722780748-1\pester-coverage.xml
coverage\ci-main-pester-36666302259-1\pester-coverage.xml
coverage\coverage.cobertura.jacoco.xml
```
- CHECKED-DELTA: 0 (1625 minus the P0-T9 value 1625)
- XML-DELTA: 3 (6 minus the P0-T9 value 3)
- CHECKED-DELTA equals 0, one of the two admissible values.
- CSHARPIER-COUNTS-IGNORED-XML: false (three additional *.xml files under the ignored trees did not change "Checked N", so CSharpier 1.2.6 did not count them)
- The check subcommand is read-only; no file was rewritten.
