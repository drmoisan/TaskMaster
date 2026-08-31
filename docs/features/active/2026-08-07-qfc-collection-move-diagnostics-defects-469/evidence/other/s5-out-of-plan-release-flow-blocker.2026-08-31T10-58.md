# CI Recovery Blocker: Unowned Release Flow Process

Timestamp: 2026-08-31T10-58

Command: `Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -match 'Invoke-FullReleaseFlow\\.ps1' } | Select-Object ProcessId,ParentProcessId,Name,CreationDate,CommandLine | ConvertTo-Json -Depth 3`

EXIT_CODE: 0

Output Summary: Two unowned `pwsh.exe` processes were active outside the approved recovery plan. The routed atomic executor confirmed that neither process was part of its command path or authorization. Execution stopped before checkpoint completion, staging, commit, amend, push, PR update, merge, or worktree action.

Observed processes:

```json
[
  {
    "ProcessId": 119248,
    "ParentProcessId": 26328,
    "Name": "pwsh.exe",
    "CreationDate": "2026-08-31T10:55:22.98518-04:00",
    "CommandLine": "\"C:\\Program Files\\PowerShell\\7\\pwsh.exe\" -NoProfile -Command pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File C:\\Users\\DanMoisan\\repos\\drm-copilot\\scripts\\dev-tools\\Invoke-FullReleaseFlow.ps1 -ConfirmToken yes"
  },
  {
    "ProcessId": 131380,
    "ParentProcessId": 119248,
    "Name": "pwsh.exe",
    "CreationDate": "2026-08-31T10:55:23.413529-04:00",
    "CommandLine": "\"C:\\Program Files\\PowerShell\\7\\pwsh.exe\" -NoLogo -NoProfile -ExecutionPolicy Bypass -File C:\\Users\\DanMoisan\\repos\\drm-copilot\\scripts\\dev-tools\\Invoke-FullReleaseFlow.ps1 -ConfirmToken yes"
  }
]
```

Required next action: establish ownership and scope of the release-flow processes before resuming this local-only recovery. Do not terminate the processes from this recovery worktree.
