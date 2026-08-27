Timestamp: 2026-08-27T03-19-23Z
Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
EXIT_CODE: 0
Output Summary: Local baseline passed 6,586 of 6,586 tests with 0 failures. Filtered line coverage was 84.87% (53,979/63,602); filtered branch coverage was 78.8445% (12,746/16,166).

The local host differs from exact-head GitHub CI: the local environment supplied a resolvable OneDrive root, while hosted CI had no OneDrive environment variable and failed 22 tests. This difference is the remediation trigger; it does not alter the exact-head CI baseline.
