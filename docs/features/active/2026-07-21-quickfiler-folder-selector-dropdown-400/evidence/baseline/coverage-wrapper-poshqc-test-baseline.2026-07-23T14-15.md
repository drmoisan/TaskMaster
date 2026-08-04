# Coverage wrapper PoshQC test baseline

- Timestamp: `2026-07-23T14-15Z`
- Command: `mcp__drm-copilot__run_poshqc_test workspace_root=C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25 scan_folders=[tests/scripts/vscode]`
- EXIT_CODE: `4294967295`
- Output Summary: `The PoshQC wrapper returned -1 without test diagnostics. A supplementary focused Pester 5.6.1 run discovered 11 existing cases and passed 11/11 with zero failures or skips.`

Supplementary command:

`Invoke-Pester -Path tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 -PassThru -Output Detailed`

Supplementary result:

| Measurement | Value |
|---|---:|
| Discovered | 11 |
| Passed | 11 |
| Failed | 0 |
| Skipped | 0 |
| Not run | 0 |
| Exit code | 0 |

The existing test file has SHA-256
`835D3F4890C7D896B09D43330F414A815ACB7670AD0A385CC042F33720EE7F5E`
and 169 physical lines. The final P8-T41 gate must pass through the required PoshQC tool;
the supplementary passing run does not waive that requirement.
