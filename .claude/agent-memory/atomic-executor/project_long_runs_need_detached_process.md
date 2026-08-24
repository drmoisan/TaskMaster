---
name: long-runs-need-detached-process
description: Bash-tool background tasks get killed on long runs, taking Start-Job load generators with them; launch multi-hour runners with Start-Process -PassThru instead
metadata:
  type: project
---

A multi-hour runner launched via the Bash tool's `run_in_background` was **killed mid-run** by the
session's background-task lifecycle after roughly an hour, discarding three completed suite runs.
Relaunching the identical script through a detached `Start-Process` survived the full 2.5-hour
window.

**Why:** `run_in_background` tasks are owned by the session and can be stopped. PowerShell
`Start-Job` workers are children of the runner process, so when the runner dies the load generator
dies with it — which is at least fail-safe (no orphan busy loops), but the window is lost and must
be restarted from scratch, because a load window's start/stop utilization samples must bracket a
single continuous run.

**How to apply:** For anything over ~30 minutes, launch it detached and record the PID:

```powershell
$p = Start-Process -FilePath 'pwsh' `
    -ArgumentList @('-NoProfile','-NonInteractive','-File', $script) `
    -RedirectStandardOutput $log -RedirectStandardError $err `
    -WindowStyle Hidden -PassThru
Set-Content -LiteralPath $pidFile -Value $p.Id
```

Then poll the log file. To make real wall-clock time pass, issue a FOREGROUND
`sleep 570` with `timeout: 600000`; it runs the full ~9.5 minutes before being moved to background.
Repeated `run_in_background` sleeps do NOT advance time reliably, because each completion notifies
you immediately and you resume within seconds.

Have the runner append one line per iteration to its log and rewrite a rows JSON after each
iteration, so a kill loses at most the in-flight iteration and the completed ones stay auditable.

Also verify after any kill: `Get-Process pwsh | Select Id,CPU,StartTime` and confirm no worker from
your start time survives. Do not kill processes whose `StartTime` predates your session — see
[[project-sibling-worktree-shared-tooling-hazard]].
