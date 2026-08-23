# P11-T12 focused Invoke-MSTestWithCoverage coverage

Timestamp: 2026-08-04T11-15

Command: `Import-Module Pester -RequiredVersion 5.6.1; $configuration = New-PesterConfiguration; $configuration.Run.Path = 'tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1'; $configuration.Run.PassThru = $true; $configuration.CodeCoverage.Enabled = $true; $configuration.CodeCoverage.Path = 'scripts/vscode/Invoke-MSTestWithCoverage.ps1'; $configuration.CodeCoverage.OutputPath = 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/invoke-mstest-wrapper-focused-coverage.2026-08-04T11-14.xml'; $result = Invoke-Pester -Configuration $configuration; exit ([int]($result.FailedCount -gt 0))`

EXIT_CODE: 0

Output Summary: The deterministic in-process Pester v5.6.1 run passed 25/25 tests with no failures, skips, or not-run tests. Pester measured 99/110 covered executable commands, or 90.00% wrapper coverage. The generated JaCoCo report records 87/97 covered executable lines, or 89.69%. The required wrapper command coverage is at least 90%; the former focused result was 86/106 commands (81.13%), so this is not a changed-wrapper regression. This measurement is limited to the changed wrapper and cannot satisfy the unchanged repository-wide >=80% PowerShell coverage policy.

| Measurement | Value |
| --- | ---: |
| Tests discovered | 25 |
| Tests passed | 25 |
| Tests failed | 0 |
| Tests skipped | 0 |
| Covered executable commands | 99 |
| Valid executable commands | 110 |
| Wrapper command coverage | 90.00% |
| Covered executable lines | 87 |
| Valid executable lines | 97 |
| Wrapper line coverage | 89.69% |

Report: `invoke-mstest-wrapper-focused-coverage.2026-08-04T11-14.xml`; SHA-256 `9D0C8208BD62861CCD3D3C2C3300F22629F004FC5C8D6145EDB2B300E1340E55`.

Protected-input SHA-256 values: `coverage.config` `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`; `TaskMaster.runsettings` `199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA`; `scripts/vscode/TaskMaster.cli.runsettings` `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`.
