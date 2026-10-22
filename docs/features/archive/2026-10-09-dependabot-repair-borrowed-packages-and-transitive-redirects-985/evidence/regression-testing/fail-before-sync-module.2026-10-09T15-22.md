# Fail-before: BindingRedirectSync module tests (R1, CR-1 and CR-3, issue #985)

Timestamp: 2026-10-09T15-22
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies/BindingRedirectSync.Tests.ps1
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- Right reason: PESTER Passed=17 Failed=7 Skipped=0 NotRun=0 Total=24.
- Failed set is exactly N1, N2, N3, N4, N5, N6 and `returns the heading and one line per repair`.
- N1 to N4 fail on the absent `Direction` property; N5 fails with `Expected 1, but got 2.` (the case-variant block produced a second repair record); the two report tests fail because the line lacks the direction.

P2-T1 counts (`tests/scripts/dependencies/BindingRedirectSync.Tests.ps1`):
- `^\s+It '`: 24 measured after P2-T1 and P2-T2 were both applied (18 before + 5 from P2-T1 + 1 from P2-T2); the P2-T1 value is therefore 23.
- Each N1 to N5 `It` name: 1 (lines 272, 284, 299, 312, 325).
- `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item`: 0.
- Carriage returns (`\r$`): 0 (LF kept).

P2-T2 counts:
- `^\s+It '`: 24.
- `\(HighestDeployed, Upgrade\)'`: 1; `\(OwnReference, Upgrade\)'`: 1; `\(HighestDeployed, Downgrade\)'`: 1; `\(HighestDeployed\)'`: 0.

Failed tests (verbatim):
```
FAILED-TEST: Invoke-BindingRedirectSync (in-memory fixtures).marks a rewrite to a higher version as an upgrade
FAILED-MESSAGE: The property 'Direction' cannot be found on this object. Verify that the property exists.
FAILED-TEST: Invoke-BindingRedirectSync (in-memory fixtures).marks a rewrite to a lower version as a downgrade
FAILED-MESSAGE: The property 'Direction' cannot be found on this object. Verify that the property exists.
FAILED-TEST: Invoke-BindingRedirectSync (in-memory fixtures).marks the direction unknown when the stale version does not parse
FAILED-MESSAGE: The property 'Direction' cannot be found on this object. Verify that the property exists.
FAILED-TEST: Invoke-BindingRedirectSync (in-memory fixtures).marks the direction unknown when the two versions are numerically equal
FAILED-MESSAGE: The property 'Direction' cannot be found on this object. Verify that the property exists.
FAILED-TEST: Invoke-BindingRedirectSync (in-memory fixtures).processes an assembly name once when two blocks differ only in letter case
FAILED-MESSAGE: Expected 1, but got 2.
FAILED-TEST: Format-BindingRedirectSyncReport.returns the heading and one line per repair
FAILED-MESSAGE: Expected strings to be the same, but they were different. Expected length: 66 Actual length:   57 Strings differ at index 56. Expected: '- Tags.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed, Upgrade)' But was:  '- Tags.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)'
FAILED-TEST: Format-BindingRedirectSyncReport.states a downgrade in the report line
FAILED-MESSAGE: Expected strings to be the same, but they were different. Expected length: 68 Actual length:   57 Strings differ at index 56. Expected: '- Tags.Test: log4net 3.6.0.0 to 3.5.0.0 (HighestDeployed, Downgrade)' But was:  '- Tags.Test: log4net 3.6.0.0 to 3.5.0.0 (HighestDeployed)'
```
