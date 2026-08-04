# P11-T8 focused Invoke-MSTestWithCoverage coverage

Timestamp: 2026-08-04T10-14

Command: `Import-Module Pester -RequiredVersion 5.6.1; $configuration = New-PesterConfiguration; $configuration.Run.Path = 'tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1'; $configuration.CodeCoverage.Enabled = $true; $configuration.CodeCoverage.Path = 'scripts/vscode/Invoke-MSTestWithCoverage.ps1'; $configuration.CodeCoverage.OutputPath = 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/invoke-mstest-wrapper-focused-coverage.2026-08-04T10-14.xml'; Invoke-Pester -Configuration $configuration`

Configuration: one in-process Pester v5.6.1 invocation; test path is only `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`; coverage path is only `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; the non-temporary coverage XML is stored beside this evidence in the canonical feature evidence folder. No external process, source/test/configuration change, coverage filter, exclusion, or threshold modification was used.

EXIT_CODE: 0

Output Summary: The focused run passed 15/15 tests with zero failures, skips, or not-run tests. Pester reported 81.13% command coverage (86/106), below P11-T8’s required 90%. The corresponding line counter is 73/92 (79.35%). This task is therefore not complete. The supplemental result is changed-wrapper evidence only and cannot replace the mandatory MCP PoshQC test gate or establish repository-wide PowerShell coverage.

| Measurement | Value |
| --- | ---: |
| Tests discovered | 15 |
| Tests passed | 15 |
| Tests failed | 0 |
| Tests skipped | 0 |
| Tests not run | 0 |
| Covered executable commands | 86 |
| Valid executable commands | 106 |
| Command coverage | 81.13% |
| Covered executable lines | 73 |
| Valid executable lines | 92 |
| Line coverage | 79.35% |

Generated focused coverage report: `invoke-mstest-wrapper-focused-coverage.2026-08-04T10-14.xml`, SHA-256 `6B051A212A02D7F9B40E788F2CC16B0328B6433C08CFCC3974F582841A3E81BA`.

Integrity checks: `coverage.config` remains SHA-256 `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`; no scoped PowerShell source, test, or configuration path changed.
