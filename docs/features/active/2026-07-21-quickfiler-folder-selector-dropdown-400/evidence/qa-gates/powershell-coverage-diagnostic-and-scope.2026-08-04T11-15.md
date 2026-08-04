# P11-T13 PowerShell coverage diagnostic and scope

Timestamp: 2026-08-04T11-15

Command: `git diff --name-only 050f7cd52a3b13ec2786c9dafbe9f99620ebf9e8 HEAD -- '*.ps1'; Get-FileHash coverage.config, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings -Algorithm SHA256; read P11-T7 and P11-T12 evidence`

EXIT_CODE: 0

Output Summary: Read-only merge-base inspection identifies exactly two changed PowerShell paths: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`. All other source, test, configuration, filter, exclusion, threshold, runsettings, package, and policy inputs remain protected and unchanged by this task.

| Evidence boundary | Result | Meaning |
| --- | --- | --- |
| P11-T7 MCP attribution diagnostic | 0/2315 aggregate executable lines | Diagnostic only; neither changed-wrapper nor repository-wide passing coverage. |
| P11-T12 focused wrapper measurement | 99/110 commands, 90.00%; 87/97 lines, 89.69% | Passing changed-wrapper command coverage; compared with 86/106 commands, 81.13%, it is not a regression. |
| Repository-wide policy | >=80% remains required | Not measured as passing here; pre-existing debt remains below the policy floor. |

Protected-input hashes: `coverage.config` `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`; `TaskMaster.runsettings` `199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA`; `scripts/vscode/TaskMaster.cli.runsettings` `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`.
