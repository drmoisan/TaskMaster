# P11-T7 PoshQC Pester gate

Timestamp: 2026-08-04T10-12

MCP inputs: `workspace_root = C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25`; `scan_folders = ["tests/scripts/vscode"]`.

MCP result: `{"ok":true,"tool":"run_poshqc_test","summary":"Ran bundled PoshQC test against the requested workspace with 1 selected scan folder."}`

EXIT_CODE: 0

Output Summary: The mandatory MCP Pester gate passed: 30 tests, zero failures, zero errors, zero disabled, and zero skipped. Generated coverage reports contain no source entry for `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; this is the known MCP v1.0.20 source-attribution diagnostic. The current aggregate line counter is 0/2315, which cannot satisfy the repository-wide coverage requirement and is recorded only as diagnostic evidence.

## Test results

| Tests | Failures | Errors | Disabled | Skipped |
| ---: | ---: | ---: | ---: | ---: |
| 30 | 0 | 0 | 0 | 0 |

## Generated reports

| Path | SHA-256 |
| --- | --- |
| artifacts/pester/pester-junit.xml | 1DF2D476C51DACD1977DEC653F1819705E44B1733F40D8C403EA63851FE6ABF7 |
| artifacts/pester/powershell-coverage.xml | F3E5182A23B79F8B43F42020D5548AE98D8812248488817D9FEC5DC0413D2F5D |
| artifacts/pester/powershell-coverage.koverage.xml | 0E2B31BA21DB9C3516B7630F0DCF3E0771A0F01C41864F7DAD567C7471C433CE |

## Attribution diagnostic

- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` source-entry count: `0`.
- JaCoCo and Koverage aggregate line counters: `0 covered / 2315 valid executable lines`.
- This missing source attribution is not a passing repository-wide coverage result. P11-T8 supplies changed-wrapper evidence only; P11-T9 records that neither result proves the 74-file repository-wide >=80% policy requirement.
